using Rage;
using System;
using System.Net;
using System.Reflection;

namespace EmsPlus.Managers
{
    public static class UpdateManager
    {
        // ------------------------------
        // REPLACE THIS WITH YOUR FILE ID
        // ------------------------------
        private const string LCPDFR_FILE_ID = "52105"; //52105 is the file ID for EmsPlus on LCPDFR.com

        public static void CheckForUpdates()
        {
            System.Threading.Tasks.Task.Run(() =>
            {
                try
                {
                    using (WebClient client = new WebClient())
                    {
                        // LCPDFR Version API Endpoint
                        string url = $"http://www.lcpdfr.com/applications/downloadsng/interface/api.php?do=checkForUpdates&fileId={LCPDFR_FILE_ID}&textOnly=1";

                        string remoteVersionString = client.DownloadString(url).Trim();

                        Version localVersion = Assembly.GetExecutingAssembly().GetName().Version;

                        if (Version.TryParse(remoteVersionString, out Version remoteVersion))
                        {
                            // Update Available Logic
                            if (remoteVersion > localVersion)
                            {
                                Game.Console.Print($"[EmsPlus] UPDATE AVAILABLE: v{remoteVersion} is out! (Current: v{localVersion})");

                                GameFiber.StartNew(delegate
                                {
                                    GameFiber.Sleep(5000);
                                    Game.DisplayNotification("commonmenu", "mp_alerttriangle", "~r~EmsPlus Update", "~g~New Version Available", $"Version ~b~{remoteVersion}~w~ is available on LCPDFR.com.");
                                });
                            }

                            // Developer Version Logic
                            else if (remoteVersion < localVersion)
                            {
                                Game.Console.Print($"[EmsPlus] You are using the developer version (v{localVersion}).");

                                GameFiber.StartNew(delegate
                                {
                                    GameFiber.Sleep(5000);
                                    Game.DisplayNotification("commonmenu", "mp_alerttriangle", "~r~EmsPlus Developer Version", "~y~This is a Developer Version", $"~b~Version {localVersion}~y~ Please report any bugs you may find!");
                                });
                            }

                            // Up to Date Logic
                            else if (remoteVersion == localVersion)
                            {
                                Game.Console.Print($"[EmsPlus] Plugin is up to date (v{localVersion}).");
                            }
                        }
                        else
                        {
                            Game.Console.Print($"[EmsPlus] Could not parse remote version: {remoteVersionString}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Game.Console.Print($"[EmsPlus] Update check failed: {ex.Message}");
                }
            });
        }
    }
}