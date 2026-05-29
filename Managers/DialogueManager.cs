using Rage;
using System.Collections.Generic;
using System.Windows.Forms;
using EmsPlus.Core;

namespace EmsPlus.Managers
{
    public class DialogueLine
    {
        public string SpeakerName { get; set; }
        public string Text { get; set; }

        public DialogueLine(string speaker, string text)
        {
            SpeakerName = speaker;
            Text = text;
        }
    }

    public static class DialogueManager
    {
        private static Queue<DialogueLine> _queue = new Queue<DialogueLine>();
        private static Ped _currentTarget = null;
        private static bool _isActive = false;

        public static bool IsActive => _isActive;

        public static void StartDialogue(Ped target, List<DialogueLine> lines)
        {
            if (_isActive || lines == null || lines.Count == 0) return;

            _queue = new Queue<DialogueLine>(lines);
            _currentTarget = target;
            _isActive = true;

            ShowNextLine();
        }

        private static void ShowNextLine()
        {
            if (_queue.Count > 0)
            {
                var line = _queue.Dequeue();
                Game.DisplaySubtitle($"~b~{line.SpeakerName}:~w~ {line.Text}", 100000);
            }
            else
            {
                EndDialogue();
            }
        }

        private static void EndDialogue()
        {
            if (!_isActive) return;

            if (_currentTarget != null && _currentTarget.Exists())
            {
                if (GameState.CurrentBystander?.Character == _currentTarget)
                    GameState.CurrentBystander.HasBeenSpokenTo = true;
            }

            _isActive = false;
            _currentTarget = null;
            _queue.Clear();

            Game.DisplaySubtitle("", 1);
        }

        public static void Process()
        {
            if (!_isActive) return;

            if (_currentTarget != null && !_currentTarget.Exists())
            {
                EndDialogue();
                return;
            }

            if (Game.IsKeyDown(Keys.Y))
            {
                ShowNextLine();
            }
        }

        public static void Cleanup()
        {
            _isActive = false;
            _currentTarget = null;
            _queue.Clear();
        }
    }
}