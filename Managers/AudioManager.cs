using System;
using System.Collections.Generic;
using System.IO;
using System.Media;
using System.Threading;
using System.Windows.Forms;
using Rage;

namespace EmsPlus.Managers
{
    public static class AudioManager
    {
        private static readonly string AudioPath = Path.Combine(Application.StartupPath, "Plugins", "EmsPlus", "Audio", "Dispatch");
        private static Queue<string> _audioQueue = new Queue<string>();
        private static bool _isThreadRunning = false;
        private static readonly object _queueLock = new object();

        public static void PlayDispatchSequence(List<DispatchAudioItem> sequence)
        {
            lock (_queueLock)
            {
                foreach (var item in sequence)
                {
                    string fullPath = Path.Combine(AudioPath, item.Category, item.FileName + ".wav");
                    if (File.Exists(fullPath))
                    {
                        _audioQueue.Enqueue(fullPath);
                    }
                    else
                    {
                        Game.Console.Print($"[EmsPlus] Audio Missing: {item.Category}/{item.FileName}.wav");
                    }
                }
            }

            if (!_isThreadRunning)
            {
                Thread audioThread = new Thread(ProcessQueue);
                audioThread.IsBackground = true;
                audioThread.Start();
            }
        }

        private static void ProcessQueue()
        {
            _isThreadRunning = true;
            SoundPlayer player = new SoundPlayer();

            while (true)
            {
                string nextFile = null;

                lock (_queueLock)
                {
                    if (_audioQueue.Count > 0)
                    {
                        nextFile = _audioQueue.Dequeue();
                    }
                    else
                    {
                        _isThreadRunning = false;
                        break;
                    }
                }

                if (nextFile != null)
                {
                    try
                    {
                        player.SoundLocation = nextFile;
                        player.Load();
                        player.PlaySync();
                    }
                    catch (Exception)
                    {
                    }
                }

                Thread.Sleep(40);
            }
        }
    }

    public class DispatchAudioItem
    {
        public string Category { get; set; }
        public string FileName { get; set; }

        public DispatchAudioItem(string category, string rawValue)
        {
            Category = category;

            string cleaned = rawValue.ToUpper()
                .Replace(" ", "_")
                .Replace("'", "")
                .Replace("-", "_");

            switch (category.ToLower())
            {
                case "streets":
                    FileName = $"STREET_{cleaned}_01";
                    break;
                case "zones":
                    if (cleaned == "HUMANE_LABS")
                    {
                        FileName = "AREA_HUMANE_LABS";
                    }
                    else
                    {
                        FileName = $"AREA_{cleaned}_01";
                    }
                    break;
                case "callouts":
                    FileName = $"REPORT_{cleaned}";
                    break;
                default:
                    FileName = cleaned;
                    break;
            }
        }
    }
}