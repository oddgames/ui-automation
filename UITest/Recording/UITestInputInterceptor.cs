using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

namespace ODDGames.UITest
{
    public class UITestInputInterceptor : MonoBehaviour
    {
        public static UITestInputInterceptor Instance { get; private set; }

        const float DRAG_THRESHOLD = 20f;
        const float HOLD_THRESHOLD = 0.5f;
        const float CLICK_MAX_DURATION = 0.3f;

        bool isPointerDown;
        Vector2 pointerDownPosition;
        float pointerDownTime;
        bool isDragging;
        Vector2 lastDragPosition;
        GameObject pointerDownTarget;
        List<RaycastResult> raycastResults = new();

        void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        void Update()
        {
            if (!UITestRecorder.Instance || !UITestRecorder.Instance.IsRecording) return;

            ProcessInput();
        }

        void ProcessInput()
        {
            if (Input.touchCount > 0)
            {
                ProcessTouch(Input.GetTouch(0));
            }
            else
            {
                ProcessMouse();
            }
        }

        void ProcessTouch(Touch touch)
        {
            switch (touch.phase)
            {
                case TouchPhase.Began:
                    OnPointerDown(touch.position);
                    break;
                case TouchPhase.Moved:
                case TouchPhase.Stationary:
                    OnPointerMove(touch.position);
                    break;
                case TouchPhase.Ended:
                case TouchPhase.Canceled:
                    OnPointerUp(touch.position);
                    break;
            }
        }

        void ProcessMouse()
        {
            Vector2 mousePos = Input.mousePosition;

            if (Input.GetMouseButtonDown(0))
            {
                OnPointerDown(mousePos);
            }
            else if (Input.GetMouseButton(0))
            {
                OnPointerMove(mousePos);
            }
            else if (Input.GetMouseButtonUp(0))
            {
                OnPointerUp(mousePos);
            }

            float scroll = Input.mouseScrollDelta.y;
            if (Mathf.Abs(scroll) > 0.1f)
            {
                OnScroll(mousePos, scroll);
            }
        }

        void OnPointerDown(Vector2 position)
        {
            isPointerDown = true;
            pointerDownPosition = position;
            lastDragPosition = position;
            pointerDownTime = Time.realtimeSinceStartup;
            isDragging = false;
            pointerDownTarget = GetUIElementAtPosition(position);
        }

        void OnPointerMove(Vector2 position)
        {
            if (!isPointerDown) return;

            float distance = Vector2.Distance(position, pointerDownPosition);
            if (distance > DRAG_THRESHOLD && !isDragging)
            {
                isDragging = true;
            }

            lastDragPosition = position;
        }

        void OnPointerUp(Vector2 position)
        {
            if (!isPointerDown) return;

            float duration = Time.realtimeSinceStartup - pointerDownTime;
            float distance = Vector2.Distance(position, pointerDownPosition);

            if (isDragging && distance > DRAG_THRESHOLD)
            {
                RecordDrag(pointerDownPosition, position, duration);
            }
            else if (duration >= HOLD_THRESHOLD)
            {
                RecordHold(position, duration);
            }
            else
            {
                RecordClick(position);
            }

            isPointerDown = false;
            isDragging = false;
            pointerDownTarget = null;
        }

        void OnScroll(Vector2 position, float delta)
        {
            var target = GetUIElementAtPosition(position);
            if (target == null) return;

            var (siblingIndex, siblingCount) = GetSiblingInfo(target);

            UITestRecorder.Instance.RecordScroll(
                target.name,
                GetHierarchyPath(target),
                GetComponentTypeName(target),
                GetParentName(target),
                position,
                delta,
                siblingIndex,
                siblingCount,
                GetGrandparentName(target)
            );
        }

        void RecordClick(Vector2 position)
        {
            var target = GetUIElementAtPosition(position);
            if (target == null) return;

            var textContent = GetTextContent(target);
            var (siblingIndex, siblingCount) = GetSiblingInfo(target);

            UITestRecorder.Instance.RecordClick(
                target.name,
                GetHierarchyPath(target),
                GetComponentTypeName(target),
                GetParentName(target),
                position,
                textContent,
                siblingIndex,
                siblingCount,
                GetGrandparentName(target)
            );
        }

        void RecordHold(Vector2 position, float duration)
        {
            var target = pointerDownTarget != null ? pointerDownTarget : GetUIElementAtPosition(position);
            if (target == null) return;

            var (siblingIndex, siblingCount) = GetSiblingInfo(target);

            UITestRecorder.Instance.RecordHold(
                target.name,
                GetHierarchyPath(target),
                GetComponentTypeName(target),
                GetParentName(target),
                position,
                duration,
                siblingIndex,
                siblingCount,
                GetGrandparentName(target)
            );
        }

        void RecordDrag(Vector2 startPosition, Vector2 endPosition, float duration)
        {
            var startTarget = pointerDownTarget != null ? pointerDownTarget : GetUIElementAtPosition(startPosition);

            string targetName = startTarget != null ? startTarget.name : "Screen";
            string targetPath = startTarget != null ? GetHierarchyPath(startTarget) : "";
            string targetType = startTarget != null ? GetComponentTypeName(startTarget) : "";
            string parentName = startTarget != null ? GetParentName(startTarget) : "";
            string grandparentName = startTarget != null ? GetGrandparentName(startTarget) : "";

            var scrollRect = FindScrollRectInParents(startTarget);
            string scrollRectName = scrollRect != null ? scrollRect.name : null;

            var (siblingIndex, siblingCount) = startTarget != null
                ? GetSiblingInfo(startTarget)
                : (0, 0);

            UITestRecorder.Instance.RecordDrag(
                targetName,
                targetPath,
                targetType,
                parentName,
                startPosition,
                endPosition,
                duration,
                scrollRectName,
                siblingIndex,
                siblingCount,
                grandparentName
            );
        }

        GameObject GetUIElementAtPosition(Vector2 screenPosition)
        {
            if (EventSystem.current == null) return null;

            var pointerData = new PointerEventData(EventSystem.current)
            {
                position = screenPosition
            };

            raycastResults.Clear();
            EventSystem.current.RaycastAll(pointerData, raycastResults);

            foreach (var result in raycastResults)
            {
                if (result.gameObject.activeInHierarchy)
                {
                    return result.gameObject;
                }
            }

            return null;
        }

        string GetHierarchyPath(GameObject go)
        {
            if (go == null) return "";

            var path = go.name;
            var parent = go.transform.parent;

            int depth = 0;
            while (parent != null && depth < 5)
            {
                path = parent.name + "/" + path;
                parent = parent.parent;
                depth++;
            }

            return path;
        }

        string GetComponentTypeName(GameObject go)
        {
            if (go == null) return "";

            if (go.GetComponent<Button>()) return "Button";
            if (go.GetComponent<Toggle>()) return "Toggle";
            if (go.GetComponent<Slider>()) return "Slider";
            if (go.GetComponent<InputField>()) return "InputField";
            if (go.GetComponent<TMP_InputField>()) return "TMP_InputField";
            if (go.GetComponent<ScrollRect>()) return "ScrollRect";
            if (go.GetComponent<Dropdown>()) return "Dropdown";
            if (go.GetComponent<TMP_Dropdown>()) return "TMP_Dropdown";
            if (go.GetComponent<Selectable>()) return "Selectable";
            if (go.GetComponent<Image>()) return "Image";
            if (go.GetComponent<RawImage>()) return "RawImage";

            return "GameObject";
        }

        string GetGrandparentName(GameObject go)
        {
            if (go == null) return "";
            var parent = go.transform.parent;
            if (parent == null) return "";
            var grandparent = parent.parent;
            if (grandparent == null) return "";
            return grandparent.name;
        }

        string GetParentName(GameObject go)
        {
            if (go == null || go.transform.parent == null) return "";
            return go.transform.parent.name;
        }

        string GetTextContent(GameObject go)
        {
            if (go == null) return "";

            var tmp = go.GetComponentInChildren<TMP_Text>();
            if (tmp != null && !string.IsNullOrEmpty(tmp.text))
            {
                return tmp.text.Trim();
            }

            var text = go.GetComponentInChildren<Text>();
            if (text != null && !string.IsNullOrEmpty(text.text))
            {
                return text.text.Trim();
            }

            return "";
        }

        (int index, int count) GetSiblingInfo(GameObject go)
        {
            if (go == null || go.transform.parent == null)
                return (0, 1);

            var parent = go.transform.parent;
            var sameName = new List<Transform>();

            for (int i = 0; i < parent.childCount; i++)
            {
                var child = parent.GetChild(i);
                if (!child.gameObject.activeInHierarchy) continue;

                if (child.name == go.name)
                {
                    sameName.Add(child);
                }
            }

            int siblingIndex = 0;
            for (int i = 0; i < sameName.Count; i++)
            {
                if (sameName[i] == go.transform)
                {
                    siblingIndex = i;
                    break;
                }
            }

            return (siblingIndex, sameName.Count);
        }

        ScrollRect FindScrollRectInParents(GameObject go)
        {
            if (go == null) return null;

            var current = go.transform;
            while (current != null)
            {
                var scrollRect = current.GetComponent<ScrollRect>();
                if (scrollRect != null)
                    return scrollRect;
                current = current.parent;
            }

            return null;
        }
    }
}
