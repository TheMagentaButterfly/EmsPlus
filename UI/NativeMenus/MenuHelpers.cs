using RAGENativeUI.Elements;
using RAGENativeUI;
using System.Collections.Generic;
using System;

namespace EmsPlus.UI.NativeMenus
{
    public static class MenuHelpers
    {
        public static List<dynamic> FloatValues { get; private set; }
        public static List<dynamic> DegreeValues { get; private set; }

        public static void Initialize()
        {
            FloatValues = new List<dynamic>();
            for (decimal d = -5.00m; d <= 5.00m; d += 0.01m) FloatValues.Add((float)d);

            DegreeValues = new List<dynamic>();
            for (int i = -180; i <= 360; i++) DegreeValues.Add((float)i);
        }

        public static UIMenuListItem AddListControl(UIMenu menu, string title, float currentValue, Action<float> setter, Action refreshAction)
        {
            UIMenuListItem listItem = new UIMenuListItem(title, FloatValues, 0);
            SyncListItem(listItem, currentValue, FloatValues);

            menu.AddItem(listItem);
            menu.OnListChange += (sender, item, index) => {
                if (item == listItem)
                {
                    float newValue = (float)FloatValues[index];
                    setter(newValue);
                    refreshAction?.Invoke();
                }
            };
            return listItem;
        }

        public static UIMenuListItem AddDegreeListControl(UIMenu menu, string title, float currentValue, Action<float> setter, Action refreshAction)
        {
            UIMenuListItem listItem = new UIMenuListItem(title, DegreeValues, 0);
            SyncListItem(listItem, currentValue, DegreeValues);

            menu.AddItem(listItem);
            menu.OnListChange += (sender, item, index) => {
                if (item == listItem)
                {
                    float newValue = (float)DegreeValues[index];
                    setter(newValue);
                    refreshAction?.Invoke();
                }
            };
            return listItem;
        }

        public static void SyncListItem(UIMenuListItem item, float val, List<dynamic> values)
        {
            if (item == null) return;
            int bestIndex = 0;
            float minDiff = float.MaxValue;
            for (int i = 0; i < values.Count; i++)
            {
                float diff = Math.Abs((float)values[i] - val);
                if (diff < minDiff) { minDiff = diff; bestIndex = i; }
            }
            item.Index = bestIndex;
        }
    }
}