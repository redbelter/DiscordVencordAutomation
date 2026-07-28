using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.UIA3;

/// <summary>
/// Discord Vencord Plugin Automation Tool
/// 
/// This tool automates the process of:
/// 1. Waiting for Discord to be running (with 60-second timeout)
/// 2. Navigating to Settings → Plugins page
/// 3. Enumerating Vencord plugins and their toggle states
/// 4. Cleaning up Discord processes after completion
/// 
/// Usage: Run as a standalone console application
/// Output: 
///   - flaui_output.txt: Detailed UI automation log
///   - plugins_list.txt: List of detected plugins with status (on/off/unknown)
/// </summary>
class Program
{
    static string logPath;
    
    static void Main(string[] args)
    {
        // Configuration
        logPath = @"C:\Users\red\Desktop\DiscordAutomation\flaui_output.txt";
        
        // Clean up old log file
        if (File.Exists(logPath))
        {
            File.WriteAllText(logPath, "");
        }
        
        Console.WriteLine($"Starting with log at: {logPath}");
        Console.WriteLine($"Current PID: {Environment.ProcessId}");
        Console.WriteLine($"Current directory: {Directory.GetCurrentDirectory()}");
        
        try
        {
            // Initialize log file
            File.WriteAllText(logPath, "=== Discord Vencord Plugin Automation (FlaUI) ===\n");
            Console.WriteLine("File created!");
            
            // Step 1: Kill all existing Discord processes (cleanup from previous runs)
            KillAllDiscordProcesses();
            
            // Step 2: Wait for Discord to be running (with 60-second timeout)
            if (!WaitForDiscordRun())
            {
                Console.WriteLine("Failed to find Discord within 60 seconds");
                return;
            }
            
            // Step 3: Navigate to Plugins page and enumerate plugins
            NavigateToPluginsAndEnumerate();
            
            // Step 4: Clean up - kill Discord after we're done
            Console.WriteLine("Automation complete, cleaning up...");
            File.AppendAllText(logPath, "Automation complete, cleaning up...\n");
            KillAllDiscordProcesses();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"FATAL ERROR: {ex.Message}");
            File.WriteAllText(logPath, $"FATAL ERROR: {ex.Message}\n{ex.StackTrace}");
        }
    }
    
    /// <summary>
    /// Kill all running Discord processes
    /// </summary>
    static void KillAllDiscordProcesses()
    {
        Console.WriteLine("Killing all Discord processes...");
        File.AppendAllText(logPath, "Killing all Discord processes...\n");
        
        foreach (var proc in Process.GetProcessesByName("Discord"))
        {
            Console.WriteLine($"  Killing PID {proc.Id}");
            File.AppendAllText(logPath, $"  Killing PID {proc.Id}\n");
            try 
            { 
                proc.Kill(); 
                proc.WaitForExit(5000); 
            }
            catch (Exception ex) 
            { 
                Console.WriteLine($"    Error: {ex.Message}"); 
                File.AppendAllText(logPath, $"    Error: {ex.Message}\n"); 
            }
        }
        System.Threading.Thread.Sleep(1000); // Wait for processes to fully terminate
    }
    
    /// <summary>
    /// Wait for Discord to be running with timeout (60 seconds)
    /// Does NOT launch Discord - just waits for it to be present
    /// </summary>
    static bool WaitForDiscordRun()
    {
        Console.WriteLine("Waiting for Discord to be running...");
        File.AppendAllText(logPath, "Waiting for Discord to be running...\n");
        
        const int maxWaitCycles = 120; // 120 × 500ms = 60 seconds
        const int waitIntervalMs = 500;
        
        for (int i = 0; i < maxWaitCycles; i++)
        {
            try
            {
                var discordProcs = Process.GetProcessesByName("Discord")
                    .Where(p => p.Id != Environment.ProcessId)
                    .ToArray();
                
                if (discordProcs.Length > 0)
                {
                    Console.WriteLine($"Discord found with {discordProcs.Length} process(es)");
                    File.AppendAllText(logPath, $"Discord found with {discordProcs.Length} process(es)\n");
                    return true;
                }
                
                if (i % 10 == 0)
                {
                    Console.WriteLine($"Waiting for Discord... (attempt {i})");
                    File.AppendAllText(logPath, $"Waiting for Discord... (attempt {i})\n");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                File.AppendAllText(logPath, $"Exception: {ex.Message}\n");
            }
            
            System.Threading.Thread.Sleep(waitIntervalMs);
        }
        
        Console.WriteLine("Timeout waiting for Discord after 60 seconds");
        File.AppendAllText(logPath, "Timeout waiting for Discord after 60 seconds\n");
        return false;
    }
    
    /// <summary>
    /// Navigate to User Settings → Plugins and enumerate all plugins
    /// </summary>
    static void NavigateToPluginsAndEnumerate()
    {
        Console.WriteLine("Testing FlaUI...");
        File.AppendAllText(logPath, "Testing FlaUI...\n");
        
        try
        {
            var automation = new UIA3Automation();
            var window = automation.GetDesktop().FindFirstDescendant(cf => cf.ByName("Discord")).AsWindow();
            
            if (window == null)
            {
                Console.WriteLine("No main window found");
                File.AppendAllText(logPath, "No main window found\n");
                return;
            }
            
            Console.WriteLine($"FOUND WINDOW! Name: {window.Name}");
            File.AppendAllText(logPath, $"FOUND WINDOW! Name: {window.Name}\n");
            
            // Click User Settings
            ClickUserSettings(window);
            
            // Click Plugins button
            ClickPluginsButton(window);
            
            // Enumerate plugins
            EnumeratePlugins(window);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR: {ex.Message}");
            File.AppendAllText(logPath, $"ERROR: {ex.Message}\n{ex.StackTrace}\n");
        }
    }
    
    /// <summary>
    /// Click the User Settings button in Discord's sidebar
    /// </summary>
    static void ClickUserSettings(AutomationElement window)
    {
        Console.WriteLine("Searching for User Settings button...");
        File.AppendAllText(logPath, "Searching for User Settings button...\n");
        System.Threading.Thread.Sleep(5000); // Wait for UI to stabilize
        
        var settingsButton = window.FindFirstDescendant(cf => cf.ByName("User Settings"));
        int retryCount = 0;
        
        while (settingsButton == null && retryCount < 3)
        {
            retryCount++;
            Console.WriteLine($"User Settings button not found (attempt {retryCount}), waiting...");
            File.AppendAllText(logPath, $"User Settings button not found (attempt {retryCount}), waiting...\n");
            System.Threading.Thread.Sleep(3000);
            settingsButton = window.FindFirstDescendant(cf => cf.ByName("User Settings"));
        }
        
        if (settingsButton != null)
        {
            Console.WriteLine("FOUND User Settings button!");
            File.AppendAllText(logPath, "FOUND User Settings button!\n");
            settingsButton.Click();
            File.AppendAllText(logPath, "Clicked User Settings button\n");
            System.Threading.Thread.Sleep(2000);
            
            Console.WriteLine("\n=== UI Tree After User Settings Clicked ===");
            File.AppendAllText(logPath, "\n=== UI Tree After User Settings Clicked ===\n");
            DumpUI(window, 10);
        }
        else
        {
            Console.WriteLine("User Settings button not found after retries");
            File.AppendAllText(logPath, "User Settings button not found after retries\n");
            DumpUI(window, 10);
        }
    }
    
    /// <summary>
    /// Click the Plugins button in Discord's sidebar
    /// </summary>
    static void ClickPluginsButton(AutomationElement window)
    {
        Console.WriteLine("\n=== Looking for Plugins navigation button ===");
        File.AppendAllText(logPath, "\n=== Looking for Plugins navigation button ===\n");
        
        var pluginsButton = window.FindFirstDescendant(cf => cf.ByName("Plugins"));
        
        if (pluginsButton != null)
        {
            Console.WriteLine("FOUND Plugins button!");
            File.AppendAllText(logPath, "FOUND Plugins button!\n");
            pluginsButton.Click();
            Console.WriteLine("Clicked Plugins button, waiting for Vencord plugins page...");
            File.AppendAllText(logPath, "Clicked Plugins button, waiting for Vencord plugins page...\n");
            System.Threading.Thread.Sleep(3000);
            
            Console.WriteLine("\n=== UI Tree After Plugins Clicked ===");
            File.AppendAllText(logPath, "\n=== UI Tree After Plugins Clicked ===\n");
            DumpUI(window, 50);
        }
        else
        {
            Console.WriteLine("Plugins button not found");
            File.AppendAllText(logPath, "Plugins button not found\n");
            DumpUI(window, 50);
        }
    }
    
    /// <summary>
    /// Enumerate all Vencord plugins and their toggle states
    /// </summary>
    static void EnumeratePlugins(AutomationElement window)
    {
        Console.WriteLine("\n=== Enumerating Plugins ===");
        File.AppendAllText(logPath, "\n=== Enumerating Plugins ===\n");
        
        var allElements = window.FindAllDescendants();
        var plugins = new System.Collections.Generic.Dictionary<string, string>();
        
        foreach (var elem in allElements)
        {
            try
            {
                string name = elem.Name ?? "";
                
                // Check if this element looks like a Vencord plugin name
                if (IsPluginName(name))
                {
                    string toggleState = GetToggleState(elem);
                    
                    if (!plugins.ContainsKey(name))
                    {
                        plugins[name] = toggleState;
                        Console.WriteLine($"Plugin: {name} - {toggleState}");
                        File.AppendAllText(logPath, $"Plugin: {name} - {toggleState}\n");
                    }
                }
            }
            catch { /* Skip elements that cause errors */ }
        }
        
        Console.WriteLine($"\n=== Found {plugins.Count} Plugins ===");
        File.AppendAllText(logPath, $"\n=== Found {plugins.Count} Plugins ===\n");
        
        // Save results to file
        SavePluginList(plugins);
    }
    
    /// <summary>
    /// Detect toggle state (ON/OFF) for a plugin checkbox
    /// Uses FlaUI 5.0.0 typed API for pattern access
    /// </summary>
    static string GetToggleState(AutomationElement elem)
    {
        try
        {
            // FlaUI 5.0.0 typed API: Use elem.Patterns.Toggle.PatternOrDefault
            var togglePattern = elem.Patterns.Toggle.PatternOrDefault;
            
            if (togglePattern != null)
            {
                // Get ToggleState from the pattern
                var toggleState = togglePattern.ToggleState;
                var toggleStateValue = toggleState.Value;
                
                return toggleStateValue.ToString();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"    Error: {ex.GetType().Name}: {ex.Message}");
        }
        
        return "Unknown";
    }
    
    /// <summary>
    /// Validate if text looks like a Vencord plugin name
    /// Plugin names are PascalCase with letters, digits, and underscores
    /// </summary>
    static bool IsPluginName(string text)
    {
        if (string.IsNullOrEmpty(text)) return false;
        if (!char.IsUpper(text[0])) return false;
        
        bool hasLetter = false;
        foreach (char c in text)
        {
            if (char.IsLetter(c)) hasLetter = true;
            if (!char.IsLetterOrDigit(c) && c != '_') return false;
        }
        
        if (!hasLetter) return false;
        if (text.Length < 3) return false; // Minimum length for plugin name
        if (text.Length > 50) return false; // Maximum length
        
        return true;
    }
    
    /// <summary>
    /// Save plugin list to plugins_list.txt (sorted alphabetically)
    /// </summary>
    static void SavePluginList(System.Collections.Generic.Dictionary<string, string> plugins)
    {
        var sortedPlugins = plugins.ToList();
        sortedPlugins.Sort((a, b) => a.Key.CompareTo(b.Key));
        
        string pluginListPath = @"C:\Users\red\Desktop\DiscordAutomation\plugins_list.txt";
        var lines = sortedPlugins.Select(p => $"{p.Key} - {p.Value}");
        
        File.WriteAllLines(pluginListPath, lines);
        
        Console.WriteLine($"\n=== Saved {sortedPlugins.Count} plugins to {pluginListPath} ===");
        File.AppendAllText(logPath, $"\n=== Saved {sortedPlugins.Count} plugins to {pluginListPath} ===\n");
    }
    
    /// <summary>
    /// Recursively dump the UI element tree for debugging
    /// </summary>
    static void DumpUI(AutomationElement element, int maxDepth = -1, int currentDepth = 0)
    {
        if (maxDepth >= 0 && currentDepth > maxDepth) return;
        
        string indent = new string(' ', currentDepth * 2);
        string name = element.Name ?? "(empty)";
        string automationId = "(not supported)";
        try { automationId = element.AutomationId ?? "(empty)"; } catch { }
        string controlType = "(not supported)";
        try { controlType = element.ControlType.ToString(); } catch { }
        
        string properties = GetAvailableProperties(element);
        string line = $"{indent}<Element Name=\"{name}\" AutomationId=\"{automationId}\" ControlType=\"{controlType}\" Properties=\"{properties}\">";
        
        Console.WriteLine(line);
        File.AppendAllText(logPath, line + "\n");
        
        foreach (var child in element.FindAllChildren())
        {
            DumpUI(child, maxDepth, currentDepth + 1);
        }
    }
    
    /// <summary>
    /// Get all available properties from an AutomationElement for debugging
    /// </summary>
    static string GetAvailableProperties(AutomationElement elem)
    {
        var props = new System.Collections.Generic.List<string>();
        
        try
        {
            var type = elem.GetType();
            var fields = type.GetFields(
                System.Reflection.BindingFlags.Public | 
                System.Reflection.BindingFlags.NonPublic | 
                System.Reflection.BindingFlags.Instance);
            
            foreach (var field in fields)
            {
                try
                {
                    var val = field.GetValue(elem);
                    if (val != null)
                    {
                        props.Add($"{field.Name}={val}");
                    }
                }
                catch { }
            }
        }
        catch { }
        
        return string.Join(",", props);
    }
}