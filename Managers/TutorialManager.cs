using EmsPlus.Core;
using EmsPlus.UI.Native;
using EmsPlus.UI.Native.ConfigMenu;
using IPT.Common.User.Settings;
using System.Collections.Generic;

namespace EmsPlus.Managers
{
    public enum TutorialStage
    {
        NotStarted = 0,
        StartupDone = 1,
        SettingsIntroDone = 2,
        VehicleConfigDone = 3,
        CalloutAcceptedDone = 4,
        OnSceneDone = 5,
        InspectionDone = 6,
        CabinDone = 7,
        Complete = 99
    }

    public static class TutorialManager
    {
        public static void Process()
        {
            if (!EntryPoint.EmsPlusConfig.EnableTutorial.Value ||
                DialogueManager.IsActive ||
                (TutorialStage)EntryPoint.EmsPlusConfig.TutorialProgress.Value >= TutorialStage.Complete)
            {
                return;
            }

            var currentStage = (TutorialStage)EntryPoint.EmsPlusConfig.TutorialProgress.Value;

            switch (currentStage)
            {
                case TutorialStage.NotStarted:
                    if (EmsService.IsOnDuty) ShowStartupTutorial();
                    break;

                case TutorialStage.StartupDone:
                    if (MenuCore.SettingsMenu != null && MenuCore.SettingsMenu.Visible) ShowSettingsIntroTutorial();
                    break;

                case TutorialStage.SettingsIntroDone:
                    if (ConfigMenuBuilder.VehiclePosMenu != null && ConfigMenuBuilder.VehiclePosMenu.Visible) ShowVehicleConfigTutorial();
                    break;
            }
        }

        private static void AdvanceTutorialStage(TutorialStage newStage)
        {
            EntryPoint.EmsPlusConfig.TutorialProgress = new SettingInt("Tutorial", "TutorialProgress", "", (int)newStage, 0, 10, 1);
            EntryPoint.EmsPlusConfig.Save();
        }

        // STAGE 0 -> 1
        private static void ShowStartupTutorial()
        {
            string menuKey = EntryPoint.KeyConfig.OpenMenuKey?.Value?.ToString() ?? "F10";
            string speaker = Localization.Get("TUTORIAL_SPEAKER");
            var lines = new List<DialogueLine>
            {
                new DialogueLine(speaker, Localization.Get("TUTORIAL_STARTUP_1")),
                new DialogueLine(speaker, Localization.Get("TUTORIAL_STARTUP_2")),
                new DialogueLine(speaker, Localization.Get("TUTORIAL_STARTUP_3", menuKey)),
            };
            DialogueManager.StartDialogue(null, lines);
            AdvanceTutorialStage(TutorialStage.StartupDone);
        }

        // STAGE 1 -> 2
        private static void ShowSettingsIntroTutorial()
        {
            string speaker = Localization.Get("TUTORIAL_SPEAKER");
            var lines = new List<DialogueLine>
            {
                new DialogueLine(speaker, Localization.Get("TUTORIAL_SETTINGS_1")),
                new DialogueLine(speaker, Localization.Get("TUTORIAL_SETTINGS_2")),
                new DialogueLine(speaker, Localization.Get("TUTORIAL_SETTINGS_3")),
            };
            DialogueManager.StartDialogue(null, lines);
            AdvanceTutorialStage(TutorialStage.SettingsIntroDone);
        }

        // STAGE 2 -> 3
        private static void ShowVehicleConfigTutorial()
        {
            string speaker = Localization.Get("TUTORIAL_SPEAKER");
            var lines = new List<DialogueLine>
            {
                new DialogueLine(speaker, Localization.Get("TUTORIAL_VEHICLE_1")),
                new DialogueLine(speaker, Localization.Get("TUTORIAL_VEHICLE_2")),
                new DialogueLine(speaker, Localization.Get("TUTORIAL_VEHICLE_3")),
                new DialogueLine(speaker, Localization.Get("TUTORIAL_VEHICLE_4")),
                new DialogueLine(speaker, Localization.Get("TUTORIAL_VEHICLE_5")),
            };
            DialogueManager.StartDialogue(null, lines);
            AdvanceTutorialStage(TutorialStage.VehicleConfigDone);
        }

        // STAGE 3 -> 4
        public static void TriggerCalloutAcceptedTutorial()
        {
            if (!EntryPoint.EmsPlusConfig.EnableTutorial.Value || (TutorialStage)EntryPoint.EmsPlusConfig.TutorialProgress.Value >= TutorialStage.CalloutAcceptedDone) return;
            string speaker = Localization.Get("TUTORIAL_SPEAKER");
            var lines = new List<DialogueLine> { new DialogueLine(speaker, Localization.Get("TUTORIAL_CALLOUT_ACCEPTED")) };
            DialogueManager.StartDialogue(null, lines);
            AdvanceTutorialStage(TutorialStage.CalloutAcceptedDone);
        }

        // STAGE 4 -> 5
        public static void TriggerOnSceneTutorial()
        {
            if (!EntryPoint.EmsPlusConfig.EnableTutorial.Value || (TutorialStage)EntryPoint.EmsPlusConfig.TutorialProgress.Value >= TutorialStage.OnSceneDone) return;
            string ambMenuKey = EntryPoint.KeyConfig.OpenAmbulanceMenuKey?.Value?.ToString() ?? "T";
            string stretcherKey = EntryPoint.KeyConfig.ToggleStretcherKey?.Value?.ToString() ?? "G";
            string interactKey = EntryPoint.KeyConfig.InteractionKey?.Value?.ToString() ?? "E";
            string speaker = Localization.Get("TUTORIAL_SPEAKER");
            var lines = new List<DialogueLine>
            {
                new DialogueLine(speaker, Localization.Get("TUTORIAL_ONSCENE_1")),
                new DialogueLine(speaker, Localization.Get("TUTORIAL_ONSCENE_2", stretcherKey, ambMenuKey)),
                new DialogueLine(speaker, Localization.Get("TUTORIAL_ONSCENE_3")),
                new DialogueLine(speaker, Localization.Get("TUTORIAL_ONSCENE_4", interactKey))
            };
            DialogueManager.StartDialogue(null, lines);
            AdvanceTutorialStage(TutorialStage.OnSceneDone);
        }

        // STAGE 5 -> 6
        public static void TriggerInspectionTutorial()
        {
            if (!EntryPoint.EmsPlusConfig.EnableTutorial.Value || (TutorialStage)EntryPoint.EmsPlusConfig.TutorialProgress.Value >= TutorialStage.InspectionDone) return;
            string speaker = Localization.Get("TUTORIAL_SPEAKER");
            var lines = new List<DialogueLine>
            {
                new DialogueLine(speaker, Localization.Get("TUTORIAL_INSPECT_1")),
                new DialogueLine(speaker, Localization.Get("TUTORIAL_INSPECT_2")),
                new DialogueLine(speaker, Localization.Get("TUTORIAL_INSPECT_3"))
            };
            DialogueManager.StartDialogue(null, lines);
            AdvanceTutorialStage(TutorialStage.InspectionDone);
        }

        // STAGE 6 -> 7 (Complete)
        public static void TriggerCabinTutorial()
        {
            if (!EntryPoint.EmsPlusConfig.EnableTutorial.Value || (TutorialStage)EntryPoint.EmsPlusConfig.TutorialProgress.Value >= TutorialStage.CabinDone) return;
            string cabinKey = EntryPoint.KeyConfig.ToggleCabinKey?.Value?.ToString() ?? "C";
            string speaker = Localization.Get("TUTORIAL_SPEAKER");
            var lines = new List<DialogueLine>
            {
                new DialogueLine(speaker, Localization.Get("TUTORIAL_CABIN_1")),
                new DialogueLine(speaker, Localization.Get("TUTORIAL_CABIN_2", cabinKey)),
                new DialogueLine(speaker, Localization.Get("TUTORIAL_CABIN_3"))
            };
            DialogueManager.StartDialogue(null, lines);
            AdvanceTutorialStage(TutorialStage.CabinDone);
        }
    }
}