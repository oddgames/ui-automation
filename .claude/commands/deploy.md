Deploy the UITest package updates to both game projects.

## Project Paths
- **MTD**: C:\Workspaces\game_monster_truck_destruction\UnityProj_MTD
- **TOR**: C:\Workspaces\game_trucks_off_road\UnityProj_TGW

## Steps

1. **Verify package is ready**
   - Check package.json exists and is valid
   - Verify all asmdef files exist

2. **Update MTD project**
   - Ensure manifest.json has package reference:
     ```json
     "com.oddgames.uitest": "file:../../../tool_ui_automation"
     ```
   - Ensure HAS_EZ_GUI is in Scripting Define Symbols
   - Report any missing setup

3. **Update TOR project**
   - Ensure manifest.json has package reference:
     ```json
     "com.oddgames.uitest": "file:../../../tool_ui_automation"
     ```
   - No additional defines needed

4. **Report deployment status**
   - List what was updated
   - Note any manual steps needed (like opening Unity to reimport)
