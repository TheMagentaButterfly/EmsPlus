using EmsPlus.Core;
using EmsPlus.Managers;
using EmsPlus.Managers.Actions;
using EmsPlus.Medical;
using RAGENativeUI;
using RAGENativeUI.Elements;
using System;
using System.Collections.Generic;

namespace EmsPlus.UI.Native.PatientMenu
{
    public static partial class PatientMenuBuilder
    {
        #region Core & Initialization
        public static UIMenu PatientMenu;
        private static UIMenu DiagnosticsMenu, PatientDataMenu, QuestionRootMenu, TraumaMenu, AirwayMenu, OralMenu, IvMenu, ImMenu, KitMenu;

        private static Dictionary<UIMenuItem, Action> _itemActions = new Dictionary<UIMenuItem, Action>();

        public static void Build()
        {
            PatientMenu = new UIMenu(Localization.Get("MENU_PATIENT_TITLE") ?? "Patient", Localization.Get("MENU_PATIENT_SUBTITLE") ?? "~b~Medical Interaction");

            DiagnosticsMenu = new UIMenu($"~b~{Localization.Get("MENU_DIAGNOSTICS_TITLE") ?? "Diagnostics"}", Localization.Get("SUBTITLE_DIAGNOSTICS") ?? "~b~Assess Patient");
            PatientDataMenu = new UIMenu($"~b~{Localization.Get("MENU_PATIENT_DATA_TITLE") ?? "Patient Data"}", Localization.Get("SUBTITLE_PATIENT_DATA") ?? "~b~Demographics & Info");
            QuestionRootMenu = new UIMenu($"~b~{Localization.Get("MENU_QUESTIONS_TITLE") ?? "Questions"}", Localization.Get("SUBTITLE_QUESTIONS") ?? "~b~Patient Interview");
            TraumaMenu = new UIMenu($"~r~{Localization.Get("MENU_TRAUMA_TITLE") ?? "Trauma"}", Localization.Get("SUBTITLE_TRAUMA") ?? "~b~Treat Injuries");
            AirwayMenu = new UIMenu($"~y~{Localization.Get("MENU_AIRWAY_TITLE") ?? "Airway"}", Localization.Get("SUBTITLE_AIRWAY") ?? "~b~Airway & Breathing");
            OralMenu = new UIMenu($"~g~{Localization.Get("MENU_ORAL_TITLE") ?? "Oral Meds"}", Localization.Get("SUBTITLE_ORAL") ?? "~b~Oral Medications");
            IvMenu = new UIMenu($"~o~{Localization.Get("MENU_IV_TITLE") ?? "IV / Access"}", Localization.Get("SUBTITLE_IV") ?? "~b~Fluids & IV Lines");
            ImMenu = new UIMenu($"~g~{Localization.Get("MENU_IM_TITLE") ?? "IM Meds"}", Localization.Get("SUBTITLE_IM") ?? "~b~Intramuscular Injections");
            KitMenu = new UIMenu($"~p~{Localization.Get("MENU_GROUND_KITS_TITLE") ?? "Ground Kits"}", Localization.Get("SUBTITLE_GROUND_KITS") ?? "~b~Interact with Medical Kits");

            MenuCore.AddMenu(PatientMenu);
            MenuCore.AddMenu(DiagnosticsMenu);
            MenuCore.AddMenu(PatientDataMenu);
            MenuCore.AddMenu(QuestionRootMenu);
            MenuCore.AddMenu(TraumaMenu);
            MenuCore.AddMenu(AirwayMenu);
            MenuCore.AddMenu(OralMenu);
            MenuCore.AddMenu(IvMenu);
            MenuCore.AddMenu(ImMenu);
            MenuCore.AddMenu(KitMenu);

            AttachActionHandler(PatientMenu);
            AttachActionHandler(DiagnosticsMenu);
            AttachActionHandler(TraumaMenu);
            AttachActionHandler(AirwayMenu);
            AttachActionHandler(OralMenu);
            AttachActionHandler(IvMenu);
            AttachActionHandler(ImMenu);
            AttachActionHandler(KitMenu);

            PatientMenu.OnMenuOpen += (s) => RefreshAll();
            DiagnosticsMenu.OnMenuOpen += (s) => BuildDiagnosticsMenu();
            PatientDataMenu.OnMenuOpen += (s) => BuildPatientDataMenu();
            QuestionRootMenu.OnMenuOpen += (s) => BuildQuestionMenu();
            TraumaMenu.OnMenuOpen += (s) => BuildTraumaMenu();
            AirwayMenu.OnMenuOpen += (s) => BuildAirwayMenu();
            OralMenu.OnMenuOpen += (s) => BuildCategoryMenu(OralMenu, "ORAL");
            IvMenu.OnMenuOpen += (s) => BuildIvMenu();
            ImMenu.OnMenuOpen += (s) => BuildCategoryMenu(ImMenu, "IM");
            KitMenu.OnMenuOpen += (s) => BuildKitMenu();
        }

        public static void RefreshAll()
        {
            _itemActions.Clear();
            PatientMenu.Clear();

            var p = GameState.CurrentPatient;
            if (p == null) return;

            AddMenuSeparator(PatientMenu, Localization.Get("CAT_SEP_ASSESSMENTS") ?? "~c~=== ~b~ASSESSMENTS ~c~===");

            var diagItem = new UIMenuItem($"~b~{Localization.Get("MENU_DIAGNOSTICS_TITLE")}", Localization.Get("DESC_DIAGNOSTICS"));
            PatientMenu.AddItem(diagItem);
            PatientMenu.BindMenuToItem(DiagnosticsMenu, diagItem);

            var dataItem = new UIMenuItem($"~b~{Localization.Get("MENU_PATIENT_DATA_TITLE")}", Localization.Get("DESC_PATIENT_DATA"));
            PatientMenu.AddItem(dataItem);
            PatientMenu.BindMenuToItem(PatientDataMenu, dataItem);

            if (p.Consciousness != ConsciousnessLevel.Unresponsive)
            {
                var qItem = new UIMenuItem($"~b~{Localization.Get("ACT_QUESTION_PATIENT")}", Localization.Get("DESC_QUESTION_PATIENT"));
                PatientMenu.AddItem(qItem);
                PatientMenu.BindMenuToItem(QuestionRootMenu, qItem);
            }

            AddMenuSeparator(PatientMenu, Localization.Get("CAT_SEP_TREATMENTS") ?? "~c~=== ~r~TREATMENTS ~c~===");

            var traumaItem = new UIMenuItem($"~r~{Localization.Get("MENU_TRAUMA_TITLE")}", Localization.Get("DESC_TRAUMA"));
            PatientMenu.AddItem(traumaItem);
            PatientMenu.BindMenuToItem(TraumaMenu, traumaItem);

            var airwayItem = new UIMenuItem($"~y~{Localization.Get("MENU_AIRWAY_TITLE")}", Localization.Get("DESC_AIRWAY"));
            PatientMenu.AddItem(airwayItem);
            PatientMenu.BindMenuToItem(AirwayMenu, airwayItem);

            var ivItem = new UIMenuItem($"~o~{Localization.Get("MENU_IV_TITLE")}", Localization.Get("DESC_IV"));
            PatientMenu.AddItem(ivItem);
            PatientMenu.BindMenuToItem(IvMenu, ivItem);

            AddMenuSeparator(PatientMenu, Localization.Get("CAT_SEP_MEDICATIONS") ?? "~c~=== ~g~MEDICATIONS ~c~===");

            var oralItem = new UIMenuItem($"~g~{Localization.Get("MENU_ORAL_TITLE")}", Localization.Get("DESC_ORAL"));
            PatientMenu.AddItem(oralItem);
            PatientMenu.BindMenuToItem(OralMenu, oralItem);

            var imItem = new UIMenuItem($"~g~{Localization.Get("MENU_IM_TITLE")}", Localization.Get("DESC_IM"));
            PatientMenu.AddItem(imItem);
            PatientMenu.BindMenuToItem(ImMenu, imItem);

            AddMenuSeparator(PatientMenu, Localization.Get("CAT_SEP_LOGISTICS") ?? "~c~=== ~p~LOGISTICS ~c~===");

            var kitItem = new UIMenuItem($"~p~{Localization.Get("MENU_GROUND_KITS_TITLE")}", Localization.Get("DESC_GROUND_KITS"));
            PatientMenu.AddItem(kitItem);
            PatientMenu.BindMenuToItem(KitMenu, kitItem);

            bool isStretcherAvail = StretcherManager.Prop != null && StretcherManager.Prop.Exists() && !StretcherManager.IsAttachedToVehicle;
            bool nearStretcher = isStretcherAvail && p.Character.DistanceTo(StretcherManager.Prop) < 4.0f;

            // Stretcher items
            if (p.IsOnStretcher)
            {
                AddInteractiveItem(PatientMenu, $"~p~{Localization.Get("ACT_UNLOAD_PATIENT")}", isStretcherAvail ? Localization.Get("ACT_UNLOAD_PATIENT_ON_GROUND") : $"~r~{Localization.Get("ACT_CANNOT_UNLOAD_INSIDE_VEHICLE")}", isStretcherAvail, () => {
                    StretcherActions.UnloadPatient();
                    MenuCore.CloseAll();
                });
            }
            else if (nearStretcher)
            {
                AddInteractiveItem(PatientMenu, $"~p~{Localization.Get("ACT_LOAD_PATIENT")}", Localization.Get("ACT_SECURE_PATIENT"), true, () => {
                    StretcherActions.LoadPatient();
                    MenuCore.CloseAll();
                });
            }
            PatientMenu.RefreshIndex();
        }

        #endregion

        #region Helpers
        private static void AttachActionHandler(UIMenu menu)
        {
            menu.OnItemSelect += (s, item, index) =>
            {
                if (_itemActions.ContainsKey(item)) _itemActions[item]?.Invoke();
            };
        }

        private static UIMenuItem AddInteractiveItem(UIMenu menu, string label, string desc, bool enabled, Action onClick)
        {
            var item = new UIMenuItem(label, desc);
            item.Enabled = enabled;
            if (!enabled) item.SetRightBadge(UIMenuItem.BadgeStyle.Lock);

            menu.AddItem(item);
            _itemActions[item] = onClick;
            return item;
        }

        private static void AddReadonlyItem(UIMenu menu, string leftLabel, string rightLabel)
        {
            var item = new UIMenuItem(leftLabel);
            item.SetRightLabel(rightLabel);
            item.Enabled = false;
            menu.AddItem(item);
        }

        private static void AddMenuSeparator(UIMenu menu, string text)
        {
            var item = new UIMenuItem(text);
            item.Enabled = false;
            menu.AddItem(item);
        }
        #endregion
    }
}