using EmsPlus.Core;
using RAGENativeUI;
using RAGENativeUI.Elements;
using System.Collections.Generic;

namespace EmsPlus.UI.Native.PatientMenu
{
    public static partial class PatientMenuBuilder
    {
        private static Dictionary<string, UIMenu> _questionSubmenus = new Dictionary<string, UIMenu>();

        private static void BuildQuestionMenu()
        {
            QuestionRootMenu.Clear();
            var p = GameState.CurrentPatient;
            if (p == null) return;

            if (_questionSubmenus.Count == 0)
            {
                foreach (var group in EntryPoint.QuestionConfig.Groups)
                {
                    string cleanName = group.Name.Length > 25 ? group.Name.Substring(0, 22) + "..." : group.Name;

                    var groupMenu = new UIMenu($"~b~{cleanName}", Localization.Get("DESC_SELECT_QUESTION"));
                    MenuCore.AddMenu(groupMenu);
                    AttachActionHandler(groupMenu);
                    _questionSubmenus.Add(group.Name, groupMenu);

                    foreach (var q in group.Questions)
                    {
                        string shortQ = q.Text.Length > 40 ? q.Text.Substring(0, 37) + "..." : q.Text;

                        AddInteractiveItem(groupMenu, $"~w~{shortQ}", q.Text, true, () => {
                            MenuCore.CloseAll();
                            Managers.Actions.QuestioningActions.AskQuestion(q);
                        });
                    }
                }
            }

            foreach (var group in EntryPoint.QuestionConfig.Groups)
            {
                var groupItem = new UIMenuItem($"~b~{group.Name}", Localization.Get("DESC_QUESTION_CATEGORY"));
                QuestionRootMenu.AddItem(groupItem);
                QuestionRootMenu.BindMenuToItem(_questionSubmenus[group.Name], groupItem);
            }

            QuestionRootMenu.RefreshIndex();
        }
    }
}