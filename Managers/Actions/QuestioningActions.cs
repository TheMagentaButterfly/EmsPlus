using EmsPlus.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EmsPlus.Managers.Actions
{
    public static class QuestioningActions
    {
        private static Random _rnd = new Random();

        public static void AskQuestion(Configuration.QuestionDef question)
        {
            var p = GameState.CurrentPatient;
            if (p == null || !p.Character.Exists()) return;

            if (p.Consciousness == Medical.ConsciousnessLevel.Unresponsive)
            {
                Rage.Game.DisplayNotification("~r~Patient is unresponsive and cannot answer.");
                return;
            }

            string pMood = p.Mood;

            // Try to find answers matching the patient's current Mood
            var validAnswers = question.Answers.Where(a => a.Mood.Equals(pMood, StringComparison.OrdinalIgnoreCase)).ToList();

            // If no exact match is found, fallback to Neutral, or just pick any available answer
            if (validAnswers.Count == 0) validAnswers = question.Answers.Where(a => a.Mood.Equals("Neutral", StringComparison.OrdinalIgnoreCase)).ToList();
            if (validAnswers.Count == 0) validAnswers = question.Answers.ToList();

            string answerText = validAnswers.Count > 0 ? validAnswers[_rnd.Next(validAnswers.Count)].Text : "...";

            var lines = new List<DialogueLine>
            {
                new DialogueLine("Paramedic", question.Text),
                new DialogueLine("Patient", answerText)
            };

            DialogueManager.StartDialogue(p.Character, lines);
            p.HasBeenSpokenTo = true;
        }
    }
}