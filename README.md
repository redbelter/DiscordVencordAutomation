# Discord Vencord Plugin Automation Tool

This tool automates the detection of Vencord plugin names and their ON/OFF status in Discord's Settings → Plugins page using UI Automation (FlaUI).

## Quick Start

```bash
cd C:\Users\red\Desktop\DiscordAutomation
dotnet run
```

## What It Does

1. **Launches Discord** - Finds the latest Discord app folder and launches it
2. **Retry logic** - If Discord doesn't load within 60 seconds, it kills and restarts (up to 3 times)
3. **Navigates to Plugins** - Opens Settings → Plugins page
4. **Enumerates plugins** - Lists all Vencord plugins and detects toggle states
5. **Saves results** - Outputs to `plugins_list.txt`

## Output Files

### `plugins_list.txt`
Contains all detected plugins in the format:
```
PluginName - Status
```

Example:
```
Deafen - Off
Online - Unknown
AlwaysAnimate - Off
```

**Status values:**
- `On` - Plugin is enabled
- `Off` - Plugin is disabled  
- `Unknown` - Toggle state could not be determined (common for Discord's own UI elements)

### `flaui_output.txt`
Detailed UI automation log for debugging (contains full element tree dump)

## Requirements

- Windows 10/11
- .NET 8.0+
- Discord installed at `C:\Users\red\AppData\Local\Discord`

## Technical Details

**Automation Library:** FlaUI 5.0.0 (UIA3 backend)

**Toggle State Detection:**
```csharp
var togglePattern = elem.Patterns.Toggle.PatternOrDefault;
if (togglePattern != null)
{
    var toggleState = togglePattern.ToggleState;
    var toggleStateValue = toggleState.Value;
    return toggleStateValue.ToString();
}
```

**Key Classes:**
- `Program.Main()` - Entry point, orchestrates the automation flow
- `WaitForDiscordLoad()` - Handles 3-retry logic with 60-second timeout
- `NavigateToPluginsAndEnumerate()` - Navigates UI and triggers enumeration
- `EnumeratePlugins()` - Finds all plugin elements and checks toggle states
- `IsPluginName()` - Validates if an element looks like a Vencord plugin

## For AI Assistants

When running this tool:
1. It will launch Discord automatically - no manual intervention needed
2. Wait ~10-15 seconds for the full process to complete
3. Check `plugins_list.txt` for the results
4. If it fails, check `flaui_output.txt` for error details

## Known Limitations

- Toggle detection may return "Unknown" for Discord's own UI elements (not Vencord plugins)
- Requires Discord to be running in interactive session (not background/daemon mode)
- WebView2/WebView elements may not fully expose UIA toggle patterns

## Running as a Scheduled Task

The tool is set up as a scheduled task on the target machine:
- **Task name:** `\DiscordAutomation`
- **Triggers:** Scheduled to run at configured intervals
- **Run with highest privileges:** Yes (requires admin for UI automation)

### For AI Assistants - Running the Scheduled Task:

```powershell
# Run the task manually
schtasks /run /tn "\DiscordAutomation"

# Check task status
schtasks /query /tn "\DiscordAutomation" /fo LIST

# View task XML for reference
schtasks /query /tn "\DiscordAutomation" /xml
```

### Triggering via GitHub Actions:

```yaml
- name: Trigger Discord Automation
  run: schtasks /run /tn "\DiscordAutomation"
```

### Expected Output Files:
- `plugins_list.txt` - Plugin list with status
- `flaui_output.txt` - Detailed automation log

## Running the Compiled Release

Download the latest release from GitHub and extract to `C:\Users\red\Desktop\DiscordAutomation\`:

```bash
cd C:\Users\red\Desktop\DiscordAutomation
DiscordAutomation.exe
```