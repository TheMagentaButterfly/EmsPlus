using System;
using System.IO;
using Microsoft.Win32;
using Microsoft.Web.WebView2.Core;
using Rage;

namespace EmsPlus.Core
{
    public static class DependencyChecker
    {
        public static bool AreDependenciesValid { get; private set; } = false;

        public static bool RunStartupChecks()
        {
            Game.Console.Print("============[EmsPlus] Running Dependency & Environment Checks============");

            bool filesOk = CheckRequiredFiles();
            bool osOk = CheckCompatibilityMode();
            bool runtimeOk = CheckWebView2Runtime();

            AreDependenciesValid = filesOk && osOk && runtimeOk;

            if (AreDependenciesValid)
            {
                Game.Console.Print("[EmsPlus] [Dependency Check] All dependencies and environment checks PASSED.");
            }
            else
            {
                Game.Console.Print("[EmsPlus] [Dependency Check] ONE OR MORE CHECKS FAILED. See details above.");
            }

            return AreDependenciesValid;
        }

        private static bool CheckRequiredFiles()
        {
            string rootDir = AppDomain.CurrentDomain.BaseDirectory;
            bool allFound = true;

            string[] rootDlls = new[]
            {
                "Microsoft.Web.WebView2.Core.dll",
                "Microsoft.Web.WebView2.WinForms.dll",
                "WebView2Loader.dll",
                "IPT.Common.dll",
                "RAGENativeUI.dll"
            };

            foreach (var dll in rootDlls)
            {
                string path = Path.Combine(rootDir, dll);
                if (!File.Exists(path))
                {
                    Game.Console.Print($"[EmsPlus] [Dependency Check] MISSING FILE: '{dll}' not found in main GTA V directory.");
                    allFound = false;
                }
            }

            string loaderPath = Path.Combine(rootDir, "WebView2Loader.dll");
            if (!File.Exists(loaderPath))
            {
                loaderPath = Path.Combine(rootDir, "runtimes", "win-x64", "native", "WebView2Loader.dll");
            }

            if (!File.Exists(loaderPath))
            {
                Game.Console.Print("[EmsPlus] [Dependency Check] MISSING FILE: 'WebView2Loader.dll' not found in main directory or 'runtimes/win-x64/native/'.");
                allFound = false;
            }
            else
            {
                try
                {
                    using (var stream = new FileStream(loaderPath, FileMode.Open, FileAccess.Read))
                    using (var reader = new BinaryReader(stream))
                    {
                        stream.Seek(0x3C, SeekOrigin.Begin);
                        int peOffset = reader.ReadInt32();

                        stream.Seek(peOffset + 4, SeekOrigin.Begin);
                        ushort machineType = reader.ReadUInt16();

                        if (machineType == 0x014c)
                        {
                            Game.Console.Print("[EmsPlus] [Dependency Check] CRITICAL: 'WebView2Loader.dll' is 32-bit (x86)! GTA V requires the 64-bit (x64) version.");
                            allFound = false;
                        }
                        else if (machineType != 0x8664)
                        {
                            Game.Console.Print($"[EmsPlus] [Dependency Check] WARNING: 'WebView2Loader.dll' machine architecture is 0x{machineType:X4}, expected x64 (0x8664).");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Game.Console.Print($"[EmsPlus] [Dependency Check] Warning: Could not read PE header of WebView2Loader.dll: {ex.Message}");
                }
            }

            string htmlPath = Path.Combine(rootDir, "Plugins", "EmsPlus", "UI", "mdt.html");
            if (!File.Exists(htmlPath))
            {
                Game.Console.Print($"[EmsPlus] [Dependency Check] MISSING FILE: 'mdt.html' not found at '{htmlPath}'.");
                allFound = false;
            }

            if (allFound)
            {
                Game.Console.Print("[EmsPlus] [Dependency Check] Core binaries, architecture (x64), and HTML files: OK");
            }

            return allFound;
        }

        private static bool CheckCompatibilityMode()
        {
            int realMajor = 0;
            int buildNumber = 0;

            try
            {
                using (var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion"))
                {
                    if (key != null)
                    {
                        var majorVal = key.GetValue("CurrentMajorVersionNumber");
                        if (majorVal is int m) realMajor = m;

                        var buildVal = key.GetValue("CurrentBuildNumber")?.ToString() ?? key.GetValue("CurrentBuild")?.ToString();
                        if (int.TryParse(buildVal, out int b)) buildNumber = b;
                    }
                }
            }
            catch { }

            if (realMajor >= 10 || buildNumber >= 10240)
            {
                Game.Console.Print($"[EmsPlus] [Dependency Check] Operating System: Windows 10/11 (Build {buildNumber}): OK");
            }
            else
            {
                if (Environment.OSVersion.Version.Major < 6 ||
                    (Environment.OSVersion.Version.Major == 6 && Environment.OSVersion.Version.Minor <= 1))
                {
                    Game.Console.Print($"[EmsPlus] [Dependency Check] CRITICAL WARNING: Operating System reported as Windows 7 or older ({Environment.OSVersion.VersionString}).");
                    return false;
                }
            }

            bool compatFlagDetected = false;
            try
            {
                string regPath = @"Software\Microsoft\Windows NT\CurrentVersion\AppCompatFlags\Layers";
                using (var key = Registry.CurrentUser.OpenSubKey(regPath))
                {
                    if (key != null)
                    {
                        foreach (var name in key.GetValueNames())
                        {
                            if (name.IndexOf("GTA5.exe", StringComparison.OrdinalIgnoreCase) >= 0 ||
                                name.IndexOf("RAGEPluginHook.exe", StringComparison.OrdinalIgnoreCase) >= 0)
                            {
                                string val = key.GetValue(name)?.ToString() ?? "";
                                if (val.IndexOf("WIN7", StringComparison.OrdinalIgnoreCase) >= 0 ||
                                    val.IndexOf("WIN8", StringComparison.OrdinalIgnoreCase) >= 0 ||
                                    val.IndexOf("VISTASP", StringComparison.OrdinalIgnoreCase) >= 0)
                                {
                                    Game.Console.Print($"[EmsPlus] [Dependency Check] EXPLICIT COMPATIBILITY SHIM DETECTED on '{name}': '{val}'");
                                    compatFlagDetected = true;
                                }
                            }
                        }
                    }
                }
            }
            catch { }

            if (compatFlagDetected)
            {
                return false;
            }

            Game.Console.Print("[EmsPlus] [Dependency Check] OS Environment & Compatibility Flags: OK");
            return true;
        }

        private static bool CheckWebView2Runtime()
        {
            try
            {
                string version = CoreWebView2Environment.GetAvailableBrowserVersionString();

                if (!string.IsNullOrEmpty(version))
                {
                    Game.Console.Print($"[EmsPlus] [Dependency Check] Edge WebView2 Runtime: OK (Version: {version})");
                    return true;
                }
                else
                {
                    Game.Console.Print("[EmsPlus] [Dependency Check] CRITICAL: WebView2 Runtime version returned null or empty.");
                    return false;
                }
            }
            catch (WebView2RuntimeNotFoundException)
            {
                Game.Console.Print("[EmsPlus] [Dependency Check] CRITICAL: Microsoft Edge WebView2 Evergreen Runtime is NOT installed on this PC.");
                Game.Console.Print("[EmsPlus] [Dependency Check] Please install it from: https://developer.microsoft.com/en-us/microsoft-edge/webview2/");
                return false;
            }
            catch (Exception ex)
            {
                Game.Console.Print($"[EmsPlus] [Dependency Check] CRITICAL: Could not query WebView2 Runtime: {ex.Message}");
                return false;
            }
        }
    }
}