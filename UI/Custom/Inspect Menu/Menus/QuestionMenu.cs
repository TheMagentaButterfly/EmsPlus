using System.Drawing;

namespace EmsPlus.UI.Custom.InspectMenu.Menus
{
    public static class QuestionMenu
    {
        public static void BuildGroups()
        {
            foreach (var group in EntryPoint.QuestionConfig.Groups)
            {
                string shortLabel = group.Name.Length > 28 ? group.Name.Substring(0, 25) + "..." : group.Name;

                BodyInspectionManager.CurrentPanelActions.Add(new InspectionAction(
                    shortLabel, Localization.Get("ACT_QUESTION_PATIENT_CATEGORY", "Ask questions from this category"), Color.LightSkyBlue, true, () => {
                        BodyInspectionManager.CurrentMenuCategory = "QUESTIONS_" + group.Name;
                        BodyInspectionManager.RefreshActions();
                    }
                ));
            }
        }

        public static void BuildQuestions(string groupName)
        {
            var group = EntryPoint.QuestionConfig.Groups.Find(g => g.Name == groupName);

            if (group == null)
            {
                BodyInspectionManager.CurrentPanelActions.Add(new InspectionAction("ERROR", "XML Group not found.", Color.Red, false, null));
                return;
            }

            foreach (var q in group.Questions)
            {
                string shortText = q.Text.Length > 28 ? q.Text.Substring(0, 25) + "..." : q.Text;

                BodyInspectionManager.CurrentPanelActions.Add(new InspectionAction(
                    shortText, Localization.Get("DESC_ASK_PATIENT", "Ask the patient"), Color.White, true, () => {
                        Managers.Actions.QuestioningActions.AskQuestion(q);
                        BodyInspectionManager.StopInspection(false);
                    }
                ));
            }
        }
    }
}