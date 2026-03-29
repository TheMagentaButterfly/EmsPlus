using EmsPlus.Core;
using EmsPlus.Managers;
using EmsPlus.Managers.Actions;
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
        private static UIMenu DiagnosticsMenu, TraumaMenu, AirwayMenu, OralMenu, IvMenu, ImMenu, KitMenu;

        // Dictionary to map dynamically created buttons to their specific actions
        private static Dictionary<UIMenuItem, Action> _itemActions = new Dictionary<UIMenuItem, Action>();

        public static void Build()
        {
            // Create Root Menu
            PatientMenu = new UIMenu(Localization.Get("MENU_PATIENT_TITLE"), Localization.Get("MENU_PATIENT_SUBTITLE"));

            // Create Submenus
            DiagnosticsMenu = new UIMenu(Localization.Get("MENU_DIAGNOSTICS_TITLE"), Localization.Get("MENU_DIAGNOSTICS_SUBTITLE"));
            TraumaMenu = new UIMenu(Localization.Get("MENU_TRAUMA_TITLE"), Localization.Get("MENU_TRAUMA_SUBTITLE"));
            AirwayMenu = new UIMenu(Localization.Get("MENU_AIRWAY_TITLE"), Localization.Get("MENU_AIRWAY_SUBTITLE"));
            OralMenu = new UIMenu(Localization.Get("MENU_ORAL_TITLE"), Localization.Get("MENU_ORAL_SUBTITLE"));
            IvMenu = new UIMenu(Localization.Get("MENU_IV_TITLE"), Localization.Get("MENU_IV_SUBTITLE"));
            ImMenu = new UIMenu(Localization.Get("MENU_IM_TITLE"), Localization.Get("MENU_IM_SUBTITLE"));
            KitMenu = new UIMenu(Localization.Get("MENU_GROUND_KITS_TITLE"), Localization.Get("MENU_GROUND_KITS_SUBTITLE"));

            // Add to pool
            MenuCore.AddMenu(PatientMenu);
            MenuCore.AddMenu(DiagnosticsMenu);
            MenuCore.AddMenu(TraumaMenu);
            MenuCore.AddMenu(AirwayMenu);
            MenuCore.AddMenu(OralMenu);
            MenuCore.AddMenu(IvMenu);
            MenuCore.AddMenu(ImMenu);
            MenuCore.AddMenu(KitMenu);

            // Set up click handlers for submenus
            AttachActionHandler(PatientMenu);
            AttachActionHandler(DiagnosticsMenu);
            AttachActionHandler(TraumaMenu);
            AttachActionHandler(AirwayMenu);
            AttachActionHandler(OralMenu);
            AttachActionHandler(IvMenu);
            AttachActionHandler(ImMenu);
            AttachActionHandler(KitMenu);

            // Rebuild submenus dynamically when they open
            PatientMenu.OnMenuOpen += (s) => RefreshAll();
            DiagnosticsMenu.OnMenuOpen += (s) => BuildDiagnosticsMenu();
            TraumaMenu.OnMenuOpen += (s) => BuildTraumaMenu();
            AirwayMenu.OnMenuOpen += (s) => BuildCategoryMenu(AirwayMenu, "AIRWAY");
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

            // 1. Link to Submenus
            var diagItem = new UIMenuItem(Localization.Get("MENU_DIAGNOSTICS_TITLE"), Localization.Get("DESC_ASSESS_VITALS"));
            PatientMenu.AddItem(diagItem);
            PatientMenu.BindMenuToItem(DiagnosticsMenu, diagItem);

            var traumaItem = new UIMenuItem(Localization.Get("MENU_TRAUMA_TITLE"), Localization.Get("DESC_TREAT_INJURIES"));
            PatientMenu.AddItem(traumaItem);
            PatientMenu.BindMenuToItem(TraumaMenu, traumaItem);

            var airwayItem = new UIMenuItem(Localization.Get("MENU_AIRWAY_TITLE"), Localization.Get("DESC_AIRWAY_MASKS"));
            PatientMenu.AddItem(airwayItem);
            PatientMenu.BindMenuToItem(AirwayMenu, airwayItem);

            var oralItem = new UIMenuItem(Localization.Get("MENU_ORAL_TITLE"), Localization.Get("DESC_ORAL_MEDS"));
            PatientMenu.AddItem(oralItem);
            PatientMenu.BindMenuToItem(OralMenu, oralItem);

            var ivItem = new UIMenuItem(Localization.Get("MENU_IV_TITLE"), Localization.Get("DESC_IV_LINES"));
            PatientMenu.AddItem(ivItem);
            PatientMenu.BindMenuToItem(IvMenu, ivItem);

            var imItem = new UIMenuItem(Localization.Get("MENU_IM_TITLE"), Localization.Get("DESC_IM_MEDS"));
            PatientMenu.AddItem(imItem);
            PatientMenu.BindMenuToItem(ImMenu, imItem);

            var kitItem = new UIMenuItem(Localization.Get("MENU_GROUND_KITS_TITLE"), Localization.Get("DESC_GROUND_KITS"));
            PatientMenu.AddItem(kitItem);
            PatientMenu.BindMenuToItem(KitMenu, kitItem);

            // 2. Stretcher Logic (Directly on Root Menu)
            bool isStretcherAvail = StretcherManager.Prop != null && StretcherManager.Prop.Exists() && !StretcherManager.IsAttachedToVehicle;
            bool nearStretcher = isStretcherAvail && p.Character.DistanceTo(StretcherManager.Prop) < 4.0f;

            if (p.IsOnStretcher)
            {
                AddInteractiveItem(PatientMenu, Localization.Get("ACT_UNLOAD_PATIENT"), isStretcherAvail ? Localization.Get("ACT_UNLOAD_PATIENT_ON_GROUND") : Localization.Get("ACT_CANNOT_UNLOAD_INSIDE_VEHICLE"), isStretcherAvail, () => {
                    StretcherActions.UnloadPatient();
                    MenuCore.CloseAll();
                });
            }
            else if (nearStretcher)
            {
                AddInteractiveItem(PatientMenu, Localization.Get("ACT_LOAD_PATIENT"), Localization.Get("ACT_SECURE_PATIENT"), true, () => {
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
                if (_itemActions.ContainsKey(item))
                {
                    _itemActions[item]?.Invoke();
                }
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

        #endregion
    }
}