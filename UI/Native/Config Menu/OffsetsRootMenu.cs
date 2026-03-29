using RAGENativeUI;
using RAGENativeUI.Elements;

namespace EmsPlus.UI.Native.ConfigMenu
{
    public static partial class ConfigMenuBuilder
    {
        #region Offsets Root Menu

        private static void BuildOffsetsRootMenu()
        {
            OffsetsRootMenu = new UIMenu($"{C_HEADER}{Localization.Get("MENU_OFFSETS_ROOT_TITLE")}", Localization.Get("MENU_OFFSETS_ROOT_SUBTITLE"));
            MenuCore.AddMenu(OffsetsRootMenu);

            PropPosMenu = new UIMenu($"{C_HEADER}{Localization.Get("MENU_PROP_OFFSETS_TITLE")}", Localization.Get("MENU_PROP_OFFSETS_SUBTITLE"));
            StretcherPosMenu = new UIMenu($"{C_HEADER}{Localization.Get("MENU_STRETCHER_CARRY_TITLE")}", Localization.Get("MENU_STRETCHER_CARRY_SUBTITLE"));
            PatientPosMenu = new UIMenu($"{C_HEADER}{Localization.Get("MENU_PATIENT_POS_TITLE")}", Localization.Get("MENU_PATIENT_POS_SUBTITLE"));
            VehiclePosMenu = new UIMenu($"{C_HEADER}{Localization.Get("MENU_VEHICLE_CONFIG_TITLE")}", Localization.Get("MENU_VEHICLE_CONFIG_SUBTITLE"));

            MenuCore.AddMenu(PropPosMenu);
            MenuCore.AddMenu(StretcherPosMenu);
            MenuCore.AddMenu(PatientPosMenu);
            MenuCore.AddMenu(VehiclePosMenu);

            var btnProps = new UIMenuItem($"{BULLET} {Localization.Get("ITEM_PROP_CARRY_OFFSETS")}", $"{C_INFO} {Localization.Get("ITEM_PROP_CARRY_OFFSETS_DESC")}");
            var btnCarry = new UIMenuItem($"{BULLET} {Localization.Get("ITEM_STRETCHER_CARRY_POS")}", $"{C_INFO}{Localization.Get("DESC_STRETCHER_CARRY_POS")}");
            var btnPatient = new UIMenuItem($"{BULLET} {Localization.Get("ITEM_PATIENT_ON_STRETCHER")}", $"{C_INFO}{Localization.Get("DESC_PATIENT_ON_STRETCHER")}");
            var btnVehicle = new UIMenuItem($"{BULLET} {Localization.Get("ITEM_VEHICLE_LOADING_POS")}", $"{C_INFO}{Localization.Get("DESC_VEHICLE_LOADING_POS")}");

            OffsetsRootMenu.AddItem(btnProps);
            OffsetsRootMenu.AddItem(btnCarry);
            OffsetsRootMenu.AddItem(btnPatient);
            OffsetsRootMenu.AddItem(btnVehicle);

            OffsetsRootMenu.BindMenuToItem(PropPosMenu, btnProps);
            OffsetsRootMenu.BindMenuToItem(StretcherPosMenu, btnCarry);
            OffsetsRootMenu.BindMenuToItem(PatientPosMenu, btnPatient);
            OffsetsRootMenu.BindMenuToItem(VehiclePosMenu, btnVehicle);
        }

        #endregion
    }
}