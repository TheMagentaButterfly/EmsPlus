using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using EmsPlus.Framework;
using Rage;

namespace EmsPlus.Managers
{
    public static class AddonManager
    {
        public static void LoadAddons()
        {
            string gamePath = Application.StartupPath;
            string addonPath = Path.Combine(gamePath, "Plugins", "EmsPlus", "Plugins");

            if (!Directory.Exists(addonPath))
            {
                Directory.CreateDirectory(addonPath);
                Game.Console.Print($"[EmsPlus] Created addon folder at: {addonPath}");
                return;
            }

            string[] dllFiles = Directory.GetFiles(addonPath, "*.dll");
            Game.Console.Print($"[EmsPlus] Found {dllFiles.Length} DLLs in Plugins folder.");

            foreach (string dll in dllFiles)
            {
                LoadAssembly(dll);
            }
        }

        private static void LoadAssembly(string dllPath)
        {
            try
            {
                Assembly asm = Assembly.LoadFrom(dllPath);
                int count = 0;

                Type[] types;
                try
                {
                    types = asm.GetTypes();
                }
                catch (ReflectionTypeLoadException e)
                {
                    types = e.Types.Where(t => t != null).ToArray();
                    Game.Console.Print($"[EmsPlus] Warning: Some types in {Path.GetFileName(dllPath)} could not be loaded.");
                }

                foreach (Type type in types)
                {
                    if (type.IsClass && !type.IsAbstract && type.IsSubclassOf(typeof(EmsCallout)))
                    {
                        CalloutManager.RegisterCallout(type);
                        count++;
                    }
                }

                if (count > 0)
                {
                    Game.Console.Print($"[EmsPlus] Successfully loaded {count} callouts from {Path.GetFileName(dllPath)}");
                }
                else
                {
                    Game.Console.Print($"[EmsPlus] Addon {Path.GetFileName(dllPath)} loaded, but contained 0 valid EmsCallouts.");
                }
            }
            catch (Exception ex)
            {
                Game.Console.Print($"[EmsPlus] CRITICAL ERROR loading addon {Path.GetFileName(dllPath)}: {ex.Message}");
            }
        }
    }
}