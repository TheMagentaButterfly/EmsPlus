using RAGENativeUI;
using RAGENativeUI.Elements;

namespace EmsPlus.UI.Native.ConfigMenu
{
    public static partial class ConfigMenuBuilder
    {
        #region Offsets Root Menu

        private static void BuildOffsetsRootMenu()
        {
            OffsetsRootMenu = new UIMenu($"{C_HEADER}{Localization.Get("MENU_OFFSETS_ROOT_TITLE", "Offsets")}", Localization.Get("MENU_OFFSETS_ROOT_SUBTITLE", "Configure various offsets"));
            MenuCore.AddMenu(OffsetsRootMenu);

            PropPosMenu = new UIMenu($"{C_HEADER}{Localization.Get("MENU_PROP_OFFSETS_TITLE", "Prop Offsets")}", Localization.Get("MENU_PROP_OFFSETS_SUBTITLE", "Configure prop offsets"));
            StretcherPosMenu = new UIMenu($"{C_HEADER}{Localization.Get("MENU_STRETCHER_CARRY_TITLE", "Stretcher Carry")}", Localization.Get("MENU_STRETCHER_CARRY_SUBTITLE", "Configure stretcher carry positions"));
            PatientPosMenu = new UIMenu($"{C_HEADER}{Localization.Get("MENU_PATIENT_POS_TITLE", "Patient Position")}", Localization.Get("MENU_PATIENT_POS_SUBTITLE", "Configure patient positions"));
            VehiclePosMenu = new UIMenu($"{C_HEADER}{Localization.Get("MENU_VEHICLE_CONFIG_TITLE", "Vehicle Config")}", Localization.Get("MENU_VEHICLE_CONFIG_SUBTITLE", "Configure vehicle positions"));
            MdtPosMenu = new UIMenu($"{C_HEADER}{Localization.Get("MENU_MDT_POS_TITLE", "MDT Position")}", Localization.Get("MENU_MDT_POS_SUBTITLE", "~b~Adjust MDT UI"));

            MenuCore.AddMenu(PropPosMenu);
            MenuCore.AddMenu(StretcherPosMenu);
            MenuCore.AddMenu(PatientPosMenu);
            MenuCore.AddMenu(VehiclePosMenu);
            MenuCore.AddMenu(MdtPosMenu);

            var btnProps = new UIMenuItem($"{BULLET} {Localization.Get("ITEM_PROP_CARRY_OFFSETS", "Prop Carry Offsets")}", $"{C_INFO} {Localization.Get("ITEM_PROP_CARRY_OFFSETS_DESC", "Configure prop carry offsets")}");
            var btnCarry = new UIMenuItem($"{BULLET} {Localization.Get("ITEM_STRETCHER_CARRY_POS", "Stretcher Carry Positions")}", $"{C_INFO}{Localization.Get("DESC_STRETCHER_CARRY_POS", "Configure stretcher carry positions")}");
            var btnPatient = new UIMenuItem($"{BULLET} {Localization.Get("ITEM_PATIENT_ON_STRETCHER", "Patient on Stretcher")}", $"{C_INFO}{Localization.Get("DESC_PATIENT_ON_STRETCHER", "Configure patient positions on stretcher")}");
            var btnVehicle = new UIMenuItem($"{BULLET} {Localization.Get("ITEM_VEHICLE_LOADING_POS", "Vehicle Loading Positions")}", $"{C_INFO}{Localization.Get("DESC_VEHICLE_LOADING_POS", "Configure vehicle loading positions")}");
            var btnMdt = new UIMenuItem($"{BULLET} {Localization.Get("ITEM_MDT_POS_MENU", "MDT Position & Scale")}", $"{C_INFO}{Localization.Get("DESC_MDT_POS_MENU", "Adjust the size and position of the Mobile Data Terminal on your screen.")}");

            OffsetsRootMenu.AddItem(btnProps);
            OffsetsRootMenu.AddItem(btnCarry);
            OffsetsRootMenu.AddItem(btnPatient);
            OffsetsRootMenu.AddItem(btnVehicle);
            OffsetsRootMenu.AddItem(btnMdt);

            OffsetsRootMenu.BindMenuToItem(PropPosMenu, btnProps);
            OffsetsRootMenu.BindMenuToItem(StretcherPosMenu, btnCarry);
            OffsetsRootMenu.BindMenuToItem(PatientPosMenu, btnPatient);
            OffsetsRootMenu.BindMenuToItem(VehiclePosMenu, btnVehicle);
            OffsetsRootMenu.BindMenuToItem(MdtPosMenu, btnMdt);
        }

        #endregion
    }
}