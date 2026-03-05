using EmsPlus.Managers;
using Rage;
using RAGENativeUI;
using RAGENativeUI.Elements;
using static EmsPlus.Configuration.VehicleConfig;

namespace EmsPlus.UI.NativeMenus.ConfigMenu
{
    public static partial class ConfigMenuBuilder
    {
        #region Interaction Points Menu

        private static void RebuildInteractionPointMenu(UIMenu menu)
        {
            menu.Clear();
            _editingInteractionPointIndex = -1;

            if (AmbulanceManager.CurrentConfig == null)
            {
                menu.AddItem(new UIMenuItem($"{C_DANGER}{Localization.Get("ITEM_NO_VEHICLE")}", Localization.Get("DESC_NO_VEHICLE")));
                return;
            }

            var btnAddNew = new UIMenuItem($"{C_SUCCESS}{Localization.Get("ITEM_ADD_INTERACTION_POINT")}", Localization.Get("DESC_ADD_INTERACTION_POINT"));
            menu.AddItem(btnAddNew);
            menu.OnItemSelect += (s, item, i) =>
            {
                if (item == btnAddNew)
                {
                    AmbulanceManager.CurrentConfig.InteractionPoints.Add(new AmbulanceInteractionPoint(Vector3.Zero, 1.0f));
                    RebuildInteractionPointMenu(menu);
                }
            };

            for (int i = 0; i < AmbulanceManager.CurrentConfig.InteractionPoints.Count; i++)
            {
                int index = i;
                var point = AmbulanceManager.CurrentConfig.InteractionPoints[index];

                var pointMenu = new UIMenu($"{C_HEADER}" + string.Format(Localization.Get("MENU_POINT_TITLE"), index + 1), Localization.Get("MENU_POINT_SUBTITLE"));
                MenuCore.AddMenu(pointMenu);

                var btnEditPoint = new UIMenuItem(string.Format(Localization.Get("ITEM_EDIT_POINT"), index + 1), string.Format(Localization.Get("DESC_EDIT_POINT"), point.Offset.X, point.Offset.Y, point.Offset.Z, point.Scale));
                menu.AddItem(btnEditPoint);
                menu.BindMenuToItem(pointMenu, btnEditPoint);

                pointMenu.OnMenuOpen += (s) => _editingInteractionPointIndex = index;
                pointMenu.OnMenuClose += (s) => _editingInteractionPointIndex = -1;

                BuildEditPointMenu(pointMenu, index, menu);
            }
        }

        private static void BuildEditPointMenu(UIMenu editMenu, int index, UIMenu parentListMenu)
        {
            var point = AmbulanceManager.CurrentConfig.InteractionPoints[index];

            MenuHelpers.AddListControl(editMenu, $"{C_HEADER}X {C_INFO}{Localization.Get("LABEL_LEFT_RIGHT")}", point.Offset.X, v => { var p = AmbulanceManager.CurrentConfig.InteractionPoints[index]; p.Offset = new Vector3(v, p.Offset.Y, p.Offset.Z); }, null);
            MenuHelpers.AddListControl(editMenu, $"{C_HEADER}Y {C_INFO}{Localization.Get("LABEL_FORWARD_BACK")}", point.Offset.Y, v => { var p = AmbulanceManager.CurrentConfig.InteractionPoints[index]; p.Offset = new Vector3(p.Offset.X, v, p.Offset.Z); }, null);
            MenuHelpers.AddListControl(editMenu, $"{C_HEADER}Z {C_INFO}{Localization.Get("LABEL_UP_DOWN")}", point.Offset.Z, v => { var p = AmbulanceManager.CurrentConfig.InteractionPoints[index]; p.Offset = new Vector3(p.Offset.X, p.Offset.Y, v); }, null);
            MenuHelpers.AddListControl(editMenu, $"{C_HEADER}{Localization.Get("LABEL_SCALE_SIZE")}", point.Scale, v => { float clampedScale = v < 0.1f ? 0.1f : v; AmbulanceManager.CurrentConfig.InteractionPoints[index].Scale = clampedScale; }, null);

            var btnDelete = new UIMenuItem($"{C_DANGER}{Localization.Get("ITEM_DELETE_POINT")}", Localization.Get("DESC_DELETE_POINT"));
            editMenu.AddItem(btnDelete);
            editMenu.OnItemSelect += (s, item, i) =>
            {
                if (item == btnDelete)
                {
                    AmbulanceManager.CurrentConfig.InteractionPoints.RemoveAt(index);
                    editMenu.Visible = false;
                    RebuildInteractionPointMenu(parentListMenu);
                }
            };
        }

        #endregion
    }
}