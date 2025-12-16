Deploy the UITest package updates to both game projects.

## Project Paths
- **MTD**: C:\Workspaces\game_monster_truck_destruction\UnityProj_MTD
- **TOR**: C:\Workspaces\game_trucks_off_road\UnityProj_TGW
- **Package**: C:\Workspaces\tool_ui_automation

## Steps

1. **Commit and push any pending changes**
   - Check for uncommitted changes in tool_ui_automation
   - If changes exist, commit with appropriate message and push
   - Get the latest commit hash

2. **Update MTD manifest.json**
   - Path: C:\Workspaces\game_monster_truck_destruction\UnityProj_MTD\Packages\manifest.json
   - Update the com.oddgames.uitest entry to use file path for local development:
     ```json
     "com.oddgames.uitest": "file:../../../tool_ui_automation"
     ```
   - Or for git versioning:
     ```json
     "com.oddgames.uitest": "https://github.com/oddgames/ui-automation.git#<commit-hash>"
     ```

3. **Update TOR manifest.json**
   - Path: C:\Workspaces\game_trucks_off_road\UnityProj_TGW\Packages\manifest.json
   - Update the com.oddgames.uitest entry with the latest commit hash:
     ```json
     "com.oddgames.uitest": "https://github.com/oddgames/ui-automation.git#<commit-hash>"
     ```

4. **Report deployment status**
   - Show package version from package.json
   - Show commit hash used
   - List both manifest files updated
   - Remind user to refresh packages in Unity (Assets > Refresh or close/reopen Unity)
