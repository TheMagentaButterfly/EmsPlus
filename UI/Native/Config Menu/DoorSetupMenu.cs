using EmsPlus.Managers;
using RAGENativeUI;
using RAGENativeUI.Elements;

namespace EmsPlus.UI.Native.ConfigMenu
{
    public static partial class ConfigMenuBuilder
    {
        #region Door Setup Menu

        private static void BuildDoorSubMenu(UIMenu doorsMenu)
        {
            string[] doorNames =
            {
                Localization.Get("DOOR_FRONT_LEFT"),  Localization.Get("DOOR_FRONT_RIGHT"),
                Localization.Get("DOOR_BACK_LEFT"),   Localization.Get("DOOR_BACK_RIGHT"),
                Localization.Get("DOOR_BACK_AUX_1"),  Localization.Get("DOOR_BACK_AUX_2"),
                Localization.Get("DOOR_EXTRA_1"),     Localization.Get("DOOR_EXTRA_2")
            };

            for (int i = 0; i < doorNames.Length; i++)
            {
                int idx = i;
                var chk = new UIMenuCheckboxItem($"{BULLET} {doorNames[i]}", false);
                doorsMenu.AddItem(chk);

                chk.CheckboxEvent += (s, c) =>
                {
                    if (AmbulanceManager.CurrentConfig == null) return;

                    if (c && !AmbulanceManager.CurrentConfig.DoorIndices.Contains(idx)) AmbulanceManager.CurrentConfig.DoorIndices.Add(idx);
                    if (!c && AmbulanceManager.CurrentConfig.DoorIndices.Contains(idx)) AmbulanceManager.CurrentConfig.DoorIndices.Remove(idx);

                    if (AmbulanceManager.CurrentVehicle != null && AmbulanceManager.CurrentVehicle.Exists())
                        try { if (c) AmbulanceManager.CurrentVehicle.Doors[idx].Open(false); else AmbulanceManager.CurrentVehicle.Doors[idx].Close(false); } catch { }
                };
            }

            doorsMenu.OnMenuOpen += (s) =>
            {
                if (AmbulanceManager.CurrentConfig == null) return;
                for (int i = 0; i < 8; i++)
                    ((UIMenuCheckboxItem)doorsMenu.MenuItems[i]).Checked = AmbulanceManager.CurrentConfig.DoorIndices.Contains(i);
            };

            doorsMenu.OnMenuClose += (s) =>
            {
                if (AmbulanceManager.CurrentVehicle == null) return;
                for (int i = 0; i < 8; i++)
                    try { AmbulanceManager.CurrentVehicle.Doors[i].Close(false); } catch { }
            };
        }

        #endregion
    }
}