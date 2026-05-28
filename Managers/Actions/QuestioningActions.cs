using EmsPlus.Core;
using EmsPlus.Medical;
using Rage;
using Rage.Native;
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
                Game.DisplayNotification(Localization.Get("NOTIF_PATIENT_UNABLE_TO_ANSWER", "~r~Patient is unresponsive and cannot answer."));
                return;
            }

            string pMood = p.Mood;
            var validAnswers = question.Answers.Where(a => a.Mood.Equals(pMood, StringComparison.OrdinalIgnoreCase)).ToList();
            if (validAnswers.Count == 0) validAnswers = question.Answers.Where(a => a.Mood.Equals("Neutral", StringComparison.OrdinalIgnoreCase)).ToList();
            if (validAnswers.Count == 0) validAnswers = question.Answers.ToList();

            string answerText = validAnswers.Count > 0 ? validAnswers[_rnd.Next(validAnswers.Count)].Text : "...";

            string streetName = "this street";
            string cityArea = "this area";

            NativeFunction.Natives.GET_STREET_NAME_AT_COORD(p.Character.Position.X, p.Character.Position.Y, p.Character.Position.Z, out uint sHash, out uint cHash);
            string sName = NativeFunction.Natives.GET_STREET_NAME_FROM_HASH_KEY<string>(sHash);
            if (!string.IsNullOrEmpty(sName)) streetName = sName;

            string zone = NativeFunction.Natives.GET_NAME_OF_ZONE<string>(p.Character.Position.X, p.Character.Position.Y, p.Character.Position.Z);
            string zName = Game.GetLocalizedString(zone);
            if (!string.IsNullOrEmpty(zName)) cityArea = zName;

            string bodyPart = "chest";
            var injury = p.Conditions.OfType<PhysicalInjury>().FirstOrDefault(i => !i.IsTreated);
            if (injury != null) bodyPart = FormatBoneName(injury.Bone);

            var times = new[] { "10 minutes", "half an hour", "an hour", "a few minutes", "20 minutes" };
            string timeAmount = times[_rnd.Next(times.Length)];

            answerText = answerText.Replace("{FirstName}", p.Details.FirstName)
                                   .Replace("{LastName}", p.Details.LastName)
                                   .Replace("{FullName}", p.Details.FullName)
                                   .Replace("{Age}", p.Details.Age.ToString())
                                   .Replace("{DOB}", p.Details.DateOfBirth)
                                   .Replace("{Weight}", p.Details.Weight.ToString())
                                   .Replace("{Height}", p.Details.Height.ToString())
                                   .Replace("{StreetName}", streetName)
                                   .Replace("{CityArea}", cityArea)
                                   .Replace("{BodyPart}", bodyPart)
                                   .Replace("{Allergy}", p.Details.PrimaryAllergy)
                                   .Replace("{TimeAmount}", timeAmount);

            var lines = new List<DialogueLine>
            {
                new DialogueLine("Paramedic", question.Text),
                new DialogueLine("Patient", answerText)
            };

            DialogueManager.StartDialogue(p.Character, lines);
            p.HasBeenSpokenTo = true;
        }

        private static string FormatBoneName(PedBoneId bone)
        {
            switch (bone)
            {
                case PedBoneId.Head: return "head";
                case PedBoneId.Neck: return "neck";
                case PedBoneId.Spine3: return "chest";
                case PedBoneId.Spine2:
                case PedBoneId.Spine1:
                case PedBoneId.Spine: return "back";
                case PedBoneId.LeftUpperArm:
                case PedBoneId.LeftForeArm: return "left arm";
                case PedBoneId.RightUpperArm:
                case PedBoneId.RightForearm: return "right arm";
                case PedBoneId.LeftHand: return "left hand";
                case PedBoneId.RightHand: return "right hand";
                case PedBoneId.LeftThigh:
                case PedBoneId.LeftCalf: return "left leg";
                case PedBoneId.RightThigh:
                case PedBoneId.RightCalf: return "right leg";
                case PedBoneId.LeftFoot: return "left foot";
                case PedBoneId.RightFoot: return "right foot";
                case PedBoneId.Pelvis: return "pelvis";
                default: return "body";
            }
        }
    }
}