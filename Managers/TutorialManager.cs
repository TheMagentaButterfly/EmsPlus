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
            string speaker = Localization.Get("TUTORIAL_SPEAKER", "Tutorial");
            var lines = new List<DialogueLine>
            {
                new DialogueLine(speaker, Localization.GetFormat("TUTORIAL_STARTUP_1", "Welcome to EmsPlus! This brief tutorial will guide you through the main features. Press ~y~Y~w~ to advance dialogue boxes like this one.")),
                new DialogueLine(speaker, Localization.Get("TUTORIAL_STARTUP_2", "Your first step is to go on duty. To do so find a ~r~Fire Station~w~ on your map and press ~y~E~w~ inside the red marker.")),
                new DialogueLine(speaker, Localization.GetFormat("TUTORIAL_STARTUP_3", "Once on duty, you can open the main Settings Menu at any time by pressing ~y~{0}~w~. You can also disable this tutorial there. For now, let's wait for a callout.", menuKey)),
            };
            DialogueManager.StartDialogue(null, lines);
            AdvanceTutorialStage(TutorialStage.StartupDone);
        }

        // STAGE 1 -> 2
        private static void ShowSettingsIntroTutorial()
        {
            string speaker = Localization.Get("TUTORIAL_SPEAKER", "Tutorial");
            var lines = new List<DialogueLine>
            {
                new DialogueLine(speaker, Localization.Get("TUTORIAL_SETTINGS_1", "This is the EmsPlus Configuration Menu. From here, you can customize almost every part of the mod.")),
                new DialogueLine(speaker, Localization.Get("TUTORIAL_SETTINGS_2", "The most important section is 'Offsets & Positions'. This lets you adjust how props are attached to your character and vehicles.")),
                new DialogueLine(speaker, Localization.Get("TUTORIAL_SETTINGS_3", "Let's try configuring a vehicle. Find an ambulance you want to use, get near it, then open 'Vehicle Settings' from the previous menu.")),
            };
            DialogueManager.StartDialogue(null, lines);
            AdvanceTutorialStage(TutorialStage.SettingsIntroDone);
        }

        // STAGE 2 -> 3
        private static void ShowVehicleConfigTutorial()
        {
            string speaker = Localization.Get("TUTORIAL_SPEAKER", "Tutorial");
            var lines = new List<DialogueLine>
            {
                new DialogueLine(speaker, Localization.Get("TUTORIAL_VEHICLE_1", "This is the Vehicle Configuration menu. First, click 'Reload/Detect Vehicle' to make sure the menu is editing the correct ambulance.")),
                new DialogueLine(speaker, Localization.Get("TUTORIAL_VEHICLE_2", "Check 'Add to Allowed Vehicles'. This tells EmsPlus you want to use this model and enables saving its settings.")),
                new DialogueLine(speaker, Localization.Get("TUTORIAL_VEHICLE_3", "Use the 'Editing Mode' list to select what you want to adjust. 'Stowed Position' is where the stretcher sits inside the ambulance.")),
                new DialogueLine(speaker, Localization.Get("TUTORIAL_VEHICLE_4", "'Slide Position' is where it ends up after you unload it. Adjust the X, Y, Z, and rotation values until the ghost stretcher looks correct, then click 'Save All Settings' in the main menu.")),
                new DialogueLine(speaker, Localization.Get("TUTORIAL_VEHICLE_5", "You can also configure which doors open and set up custom interaction points for the ambulance menu. When you're done, go back on duty to get a call.")),
            };
            DialogueManager.StartDialogue(null, lines);
            AdvanceTutorialStage(TutorialStage.VehicleConfigDone);
        }

        // STAGE 3 -> 4
        public static void TriggerCalloutAcceptedTutorial()
        {
            if (!EntryPoint.EmsPlusConfig.EnableTutorial.Value || (TutorialStage)EntryPoint.EmsPlusConfig.TutorialProgress.Value >= TutorialStage.CalloutAcceptedDone) return;
            string speaker = Localization.Get("TUTORIAL_SPEAKER", "Tutorial");
            var lines = new List<DialogueLine> { new DialogueLine(speaker, Localization.Get("TUTORIAL_CALLOUT_ACCEPTED", "You've received a callout! Respond ~r~Code 3~w~ to the scene marked on your GPS.")) };
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
            string speaker = Localization.Get("TUTORIAL_SPEAKER", "Tutorial");
            var lines = new List<DialogueLine>
            {
                new DialogueLine(speaker, Localization.Get("TUTORIAL_ONSCENE_1", "You've arrived on scene. Before approaching the patient, it's critical to get your equipment ready.")),
                new DialogueLine(speaker, Localization.GetFormat("TUTORIAL_ONSCENE_2", "Go to the rear of your ambulance. To unload the stretcher, you can use the quick-key ~y~{0}~w~, or open the ambulance menu with ~y~{1}~w~.", stretcherKey, ambMenuKey)),
                new DialogueLine(speaker, Localization.Get("TUTORIAL_ONSCENE_3", "From the ambulance menu, you can also equip your primary medical bags, like the Trauma Bag and Oxygen Bag.")),
                new DialogueLine(speaker, Localization.GetFormat("TUTORIAL_ONSCENE_4", "With your gear equipped, you are ready. Approach the patient and press ~y~{0}~w~ to begin the inspection.", interactKey))
            };
            DialogueManager.StartDialogue(null, lines);
            AdvanceTutorialStage(TutorialStage.OnSceneDone);
        }

        // STAGE 5 -> 6
        public static void TriggerInspectionTutorial()
        {
            if (!EntryPoint.EmsPlusConfig.EnableTutorial.Value || (TutorialStage)EntryPoint.EmsPlusConfig.TutorialProgress.Value >= TutorialStage.InspectionDone) return;
            string speaker = Localization.Get("TUTORIAL_SPEAKER", "Tutorial");
            var lines = new List<DialogueLine>
            {
                new DialogueLine(speaker, Localization.Get("TUTORIAL_INSPECT_1", "This is the Patient Inspection Menu. Use your mouse to select different body parts to assess them.")),
                new DialogueLine(speaker, Localization.Get("TUTORIAL_INSPECT_2", "Press ~y~TAB~w~ to cycle between the Diagnostics and Patient Data panels. You can also click the buttons at the top right.")),
                new DialogueLine(speaker, Localization.Get("TUTORIAL_INSPECT_3", "When you find an injury, open a medical bag you placed on the ground, select the correct tool (like a bandage), then click on the injured body part to apply it."))
            };
            DialogueManager.StartDialogue(null, lines);
            AdvanceTutorialStage(TutorialStage.InspectionDone);
        }

        // STAGE 6 -> 7 (Complete)
        public static void TriggerCabinTutorial()
        {
            if (!EntryPoint.EmsPlusConfig.EnableTutorial.Value || (TutorialStage)EntryPoint.EmsPlusConfig.TutorialProgress.Value >= TutorialStage.CabinDone) return;
            string cabinKey = EntryPoint.KeyConfig.ToggleCabinKey?.Value?.ToString() ?? "C";
            string speaker = Localization.Get("TUTORIAL_SPEAKER", "Tutorial");
            var lines = new List<DialogueLine>
            {
                new DialogueLine(speaker, Localization.Get("TUTORIAL_CABIN_1", "The patient is now loaded. You can enter the patient cabin to continue treatment while transporting to the hospital.")),
                new DialogueLine(speaker, Localization.GetFormat("TUTORIAL_CABIN_2", "Press the quick-key ~y~{0}~w~ to enter or exit the cabin. You can also use the ambulance menu. Inside, you have access to all your medical supplies.", cabinKey)),
                new DialogueLine(speaker, Localization.Get("TUTORIAL_CABIN_3", "A waypoint to the nearest hospital has been set. Transport the patient to complete the call. This concludes the tutorial!"))
            };
            DialogueManager.StartDialogue(null, lines);
            AdvanceTutorialStage(TutorialStage.CabinDone);
        }
    }
}