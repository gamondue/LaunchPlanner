# .NET 10 Upgrade Plan

## Execution Steps

Execute steps below sequentially one by one in the order they are listed.

1. Validate that a .NET 10 SDK required for this upgrade is installed on the machine and if not, help to get it installed.
2. Ensure that the SDK version specified in global.json files is compatible with the .NET 10 upgrade.
3. Upgrade LaunchPlanner.csproj

## Settings

This section contains settings and data used by execution steps.

### Project upgrade details

This section contains details about each project upgrade and modifications that need to be done in the project.

#### LaunchPlanner.csproj modifications

Project conversion:
  - The project needs to be converted from the old .NET Framework project format to the modern SDK-style format

Project properties changes:
  - Target framework should be changed from `net48` to `net10.0-windows`

Other changes:
  - This is a Windows Forms application that will be upgraded to .NET 10
  - The project file structure will be modernized to use SDK-style project format
  - Windows-specific features will be preserved with the `net10.0-windows` target framework
