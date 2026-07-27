using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using FlaUI.Core.Patterns;

class Program
{
    static string logPath;
    static int discordPID = 0;
    
    static void Main()
    {
        logPath = @"C:\Users\red\Desktop\DiscordAutomation\flaui_output.txt";
        
        Console.WriteLine($"Starting with log at: {logPath}");
        Console.WriteLine($"Current PID: {Environment.ProcessId}");
        Console.WriteLine($"Current directory: {Directory.GetCurrentDirectory()}");
        
        try
        {
            File.WriteAllText(logPath, "=== Discord Vencord Plugin Automation (FlaUI) ===\n");
            Console.WriteLine("File created!");
            
            // Kill all Discord processes first
            Console.WriteLine("Killing all Discord processes...");
            File.AppendAllText(logPath, "Killing all Discord processes...\n");
            foreach (var proc in Process.GetProcessesByName("Discord"))
            {
                Console.WriteLine($"  Killing PID {proc.Id}");
                File.AppendAllText(logPath, $"  Killing PID {proc.Id}\n");
                try { proc.Kill(); proc.WaitForExit(5000); }
                catch (Exception ex) { Console.WriteLine($"    Error: {ex.Message}"); File.AppendAllText(logPath, $"    Error: {ex.Message}\n"); }
            }
            System.Threading.Thread.Sleep(1000);
            
            // Launch Discord
            Console.WriteLine("Launching Discord...");
            File.AppendAllText(logPath, "Launching Discord...\n");
            
            string discordPath = @"C:\Users\red\AppData\Local\Discord";
            string discordExe = Path.Combine(discordPath, "Discord.exe");
            
            if (!File.Exists(discordExe))
            {
                var appFolders = Directory.GetDirectories(discordPath).Where(d => d.Contains("app-")).ToArray();
                if (appFolders.Length > 0)
                {
                    discordExe = Path.Combine(appFolders[0], "Discord.exe");
                    Console.WriteLine($"Using app folder path: {discordExe}");
                    File.AppendAllText(logPath, $"Using app folder path: {discordExe}\n");
                }
            }
            
            if (!File.Exists(discordExe))
            {
                Console.WriteLine($"Discord.exe not found at {discordExe}");
                File.AppendAllText(logPath, $"Discord.exe not found at {discordExe}\n");
                return;
            }
            
            var discordProcess = Process.Start(new ProcessStartInfo { FileName = discordExe, UseShellExecute = true });
            if (discordProcess == null) { Console.WriteLine("Failed to start Discord!"); File.AppendAllText(logPath, "Failed to start Discord!\n"); return; }
            
            discordPID = discordProcess.Id;
            Console.WriteLine($"Discord started with PID {discordPID}");
            File.AppendAllText(logPath, $"Discord started with PID {discordPID}\n");
            
            // Wait for Discord to load - wait for main window, not updater
            Console.WriteLine("Waiting for Discord main window...");
            File.AppendAllText(logPath, "Waiting for Discord main window...\n");
            
            bool discordLoaded = false;
            for (int i = 0; i < 60; i++)
            {
                try
                {
                    if (!Process.GetProcessesByName("Discord").Any()) { Console.WriteLine("Discord exited unexpectedly"); File.AppendAllText(logPath, "Discord exited unexpectedly\n"); return; }
                    
                    var attachResult = FlaUI.Core.Application.Attach(discordPID);
                    var automation = new FlaUI.UIA3.UIA3Automation();
                    var window = attachResult.GetMainWindow(automation);
                    
                    if (window != null && window.Name.Contains("Discord") && !window.Name.Contains("Updater"))
                    {
                        discordLoaded = true;
                        Console.WriteLine($"Discord loaded! Window: {window.Name}");
                        File.AppendAllText(logPath, $"Discord loaded! Window: {window.Name}\n");
                        break;
                    }
                    else if (window != null)
                    {
                        Console.WriteLine($"Found window but not main Discord: {window.Name}");
                        File.AppendAllText(logPath, $"Found window but not main Discord: {window.Name}\n");
                    }
                }
                catch { }
                System.Threading.Thread.Sleep(500);
            }
            
            if (!discordLoaded) { Console.WriteLine("Timeout waiting for Discord main window"); File.AppendAllText(logPath, "Timeout waiting for Discord main window\n"); return; }
            
            // Find User Settings and click it
            Console.WriteLine("Testing FlaUI...");
            File.AppendAllText(logPath, "Testing FlaUI...\n");
            
            try
            {
                var app = FlaUI.Core.Application.Attach(discordPID);
                var automation = new FlaUI.UIA3.UIA3Automation();
                var window = app.GetMainWindow(automation);
                
                if (window != null)
                {
                    Console.WriteLine($"FOUND WINDOW! Name: {window.Name}");
                    File.AppendAllText(logPath, $"FOUND WINDOW! Name: {window.Name}\n");
                    
                    Console.WriteLine("Searching for User Settings button...");
                    File.AppendAllText(logPath, "Searching for User Settings button...\n");
                    System.Threading.Thread.Sleep(5000);
                    
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
                        Console.WriteLine($"FOUND User Settings button!");
                        File.AppendAllText(logPath, $"FOUND User Settings button!\n");
                        settingsButton.Click();
                        File.AppendAllText(logPath, "Clicked User Settings button\n");
                        System.Threading.Thread.Sleep(2000);
                        
                        Console.WriteLine("\n=== UI Tree After User Settings Clicked ===");
                        File.AppendAllText(logPath, "\n=== UI Tree After User Settings Clicked ===\n");
                        DumpUI(window, 10);
                        
                        // Look for Plugins button
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
                            
                            // Enumerate plugins with state
                            Console.WriteLine("\n=== Enumerating Plugins ===");
                            File.AppendAllText(logPath, "\n=== Enumerating Plugins ===\n");
                            EnumeratePlugins(window);
                            SavePluginList();
                        }
                        else { Console.WriteLine("Plugins button not found"); File.AppendAllText(logPath, "Plugins button not found\n"); DumpUI(window, 50); }
                    }
                    else { Console.WriteLine("User Settings button not found after retries"); File.AppendAllText(logPath, "User Settings button not found after retries\n"); DumpUI(window, 50); }
                }
                else { Console.WriteLine("No main window found"); File.AppendAllText(logPath, "No main window found\n"); }
            }
            catch (Exception ex) { Console.WriteLine($"ERROR: {ex.Message}"); File.AppendAllText(logPath, $"ERROR: {ex.Message}\n{ex.StackTrace}\n"); }
            
            Console.WriteLine("Done!");
            File.AppendAllText(logPath, "Done!\n");
        }
        catch (Exception ex) { Console.WriteLine($"FATAL ERROR: {ex.Message}"); File.WriteAllText(logPath, $"FATAL ERROR: {ex.Message}\n{ex.StackTrace}"); }
    }
    
    static void EnumeratePlugins(dynamic window)
    {
        var allElements = window.FindAllDescendants();
        var plugins = new System.Collections.Generic.Dictionary<string, string>();
        
        foreach (var elem in allElements)
        {
            try
            {
                string name = elem.Name ?? "";
                if (!string.IsNullOrWhiteSpace(name) && IsPluginName(name))
                {
                    var children = elem.FindAllChildren();
                    string state = CheckToggleState(elem);
                    
                    if (!plugins.ContainsKey(name))
                    {
                        plugins[name] = state;
                        Console.WriteLine($"\nPlugin: {name} - {state}");
                        File.AppendAllText(logPath, $"\nPlugin: {name} - {state}\n");
                        
                        if (plugins.Count <= 5)
                        {
                            Console.WriteLine($"  Children count: {children.Count()}");
                            File.AppendAllText(logPath, $"  Children count: {children.Count()}\n");
                            int i = 0;
                            foreach (var child in children)
                            {
                                i++;
                                string childName = child.Name ?? "(empty)";
                                // Use TryGetPattern API for FlaUI 5
                                string toggleState = GetToggleStateTryGetPattern(child);
                                Console.WriteLine($"    Child {i}: Name='{childName}', ToggleState={toggleState}");
                                File.AppendAllText(logPath, $"    Child {i}: Name='{childName}', ToggleState={toggleState}\n");
                            }
                        }
                    }
                }
            }
            catch { }
        }
        
        Console.WriteLine($"\n=== Found {plugins.Count} Plugins ===");
        File.AppendAllText(logPath, $"\n=== Found {plugins.Count} Plugins ===\n");
    }
    
    static string GetToggleStateTryGetPattern(dynamic elem)
    {
        try
        {
            // Try to get TogglePattern using FlaUI 5 TryGetPattern API
            var elemType = elem.GetType();
            var method = elemType.GetMethod("TryGetPattern");
            if (method != null)
            {
                var patternType = elemType.Assembly.GetType("FlaUI.Core.Patterns.ITogglePattern");
                if (patternType != null)
                {
                    var patternInstance = Activator.CreateInstance(patternType);
                    var parameters = new object[] { patternInstance };
                    var result = method.Invoke(elem, parameters);
                    
                    if (result is bool && (bool)result)
                    {
                        var togglePattern = parameters[0];
                        var currentProp = patternType.GetProperty("Current");
                        if (currentProp != null)
                        {
                            var current = currentProp.GetValue(togglePattern);
                            var toggleStateProp = current.GetType().GetProperty("ToggleState");
                            if (toggleStateProp != null)
                            {
                                return toggleStateProp.GetValue(current).ToString();
                            }
                        }
                    }
                }
            }
        }
        catch { }
        
        return "Unknown";
    }
    
    static string CheckToggleState(dynamic elem)
    {
        return GetToggleStateTryGetPattern(elem);
    }
    
    static string CheckToggleStateRecursive(dynamic elem, int depth)
    {
        if (depth > 20) return "Unknown";
        
        string state = GetToggleStateTryGetPattern(elem);
        if (state != "Unknown")
            return state;
        
        try
        {
            var children = elem.FindAllChildren();
            foreach (var child in children)
            {
                string result = CheckToggleStateRecursive(child, depth + 1);
                if (result != "Unknown")
                    return result;
            }
        }
        catch { }
        
        return "Unknown";
    }
    
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
        if (text.Length < 5) return false;
        if (text.Length > 40) return false;
        
        return true;
    }
    
    static void SavePluginList()
    {
        string[] lines = File.ReadAllLines(logPath);
        var plugins = new System.Collections.Generic.List<string>();
        
        foreach (var line in lines)
        {
            if (line.StartsWith("Plugin: "))
            {
                plugins.Add(line.Substring(8).Trim());
            }
        }
        
        plugins.Sort();
        string pluginListPath = @"C:\Users\red\Desktop\DiscordAutomation\plugins_list.txt";
        File.WriteAllText(pluginListPath, string.Join("\n", plugins));
        
        Console.WriteLine($"\n=== Saved {plugins.Count} plugins to {pluginListPath} ===");
    }
    
    static void DumpUI(dynamic element, int maxDepth = -1, int currentDepth = 0)
    {
        if (maxDepth >= 0 && currentDepth > maxDepth) return;
        
        var indent = new string(' ', currentDepth * 2);
        string name = "(empty)";
        string automationId = "(empty)";
        string controlType = "(empty)";
        
        try { name = element.Name ?? "(empty)"; } catch { }
        try { automationId = element.AutomationId ?? "(empty)"; } catch { }
        try { controlType = element.ControlType?.ProgrammaticName ?? "(empty)"; } catch { }
        
        var properties = GetAvailableProperties(element);
        
        var line = $"{indent}<Element Name=\"{name}\" AutomationId=\"{automationId}\" ControlType=\"{controlType}\" Properties=\"{properties}\">";
        Console.WriteLine(line);
        File.AppendAllText(logPath, line + "\n");
        
        foreach (var child in element.FindAllChildren())
        {
            DumpUI(child, maxDepth, currentDepth + 1);
        }
    }
    
    static string GetAvailableProperties(dynamic elem)
    {
        var props = new System.Collections.Generic.List<string>();
        
        try
        {
            var type = elem.GetType();
            var fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
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