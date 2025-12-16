#if HAS_EZ_GUI
using UnityEngine;

namespace ODDGames.UITest
{
    /// <summary>
    /// Automatically registers EZ GUI (AnB Software) clickable types for UI testing.
    /// This is only compiled when HAS_EZ_GUI is defined in Project Settings.
    /// </summary>
    public static class EzGUIClickableRegistration
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Register()
        {
            // Register EZ GUI clickable types
            UITestBehaviour.RegisterClickable(typeof(AutoSpriteControlBase));
            UITestBehaviour.RegisterClickable(typeof(UIButton3D));
        }
    }
}
#endif
