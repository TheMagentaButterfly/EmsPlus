using EmsPlus.Core;
using EmsPlus.Managers;
using Rage;
using RAGENativeUI;
using RAGENativeUI.Elements;

namespace EmsPlus.UI.Native.ConfigMenu
{
    public static partial class ConfigMenuBuilder
    {
        #region Force Callout Menu

        private static void BuildForceCalloutMenu()
        {
            ForceCalloutMenu = new UIMenu($"{C_HEADER}{Localization.Get("MENU_FORCE_CALLOUT_TITLE")}", Localization.Get("MENU_FORCE_CALLOUT_SUBTITLE"));
            MenuCore.AddMenu(ForceCalloutMenu);

            ForceCalloutMenu.OnMenuOpen += (menu) =>
            {
                menu.Clear();

                var calloutTypes = CalloutManager.GetRegisteredCalloutTypes();

                if (calloutTypes.Count == 0)
                {
                    menu.AddItem(new UIMenuItem("~r~No callouts registered.", "Ensure callout packs are loaded correctly."));
                    return;
                }

                foreach (var calloutType in calloutTypes)
                {
                    string displayName = calloutType.Name.Replace("Callout", "");
                    var calloutItem = new UIMenuItem(displayName, $"Manually start the {displayName} callout.");
                    menu.AddItem(calloutItem);
                }
            };

            ForceCalloutMenu.OnItemSelect += (sender, selectedItem, index) =>
            {
                if (!EmsService.IsOnDuty)
                {
                    Game.DisplayNotification("~r~Error:~w~ You must be on duty to start a callout.");
                    return;
                }

                var calloutTypes = CalloutManager.GetRegisteredCalloutTypes();

                if (index >= calloutTypes.Count) return;

                var selectedCalloutType = calloutTypes[index];

                MenuCore.CloseAll();

                GameFiber.StartNew(delegate
                {
                    GameFiber.Sleep(2000);
                    CalloutManager.ForceCallout(selectedCalloutType.Name);
                });
            };
        }

        #endregion
    }
}