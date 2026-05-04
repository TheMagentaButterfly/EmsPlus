using Rage;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;

namespace EmsPlus.Configuration
{
    public class AnswerDef
    {
        public string Text { get; set; }
        public string Mood { get; set; }
    }

    public class QuestionDef
    {
        public string Text { get; set; }
        public List<AnswerDef> Answers { get; set; } = new List<AnswerDef>();
    }

    public class QuestionGroup
    {
        public string Name { get; set; }
        public List<QuestionDef> Questions { get; set; } = new List<QuestionDef>();
    }

    public class QuestionConfig
    {
        private const string FilePath = "Plugins/EmsPlus/Settings/Data/Questions.xml";
        public List<QuestionGroup> Groups { get; private set; } = new List<QuestionGroup>();

        public void Load()
        {
            Groups.Clear();
            if (!File.Exists(FilePath)) CreateDefaultFile();

            try
            {
                XDocument doc = XDocument.Load(FilePath);
                foreach (XElement groupEl in doc.Descendants("QuestionGroup"))
                {
                    var group = new QuestionGroup { Name = groupEl.Attribute("Name")?.Value ?? "General" };

                    foreach (XElement qEl in groupEl.Descendants("Question"))
                    {
                        var qText = qEl.Attribute("Question")?.Value;
                        if (string.IsNullOrEmpty(qText) || qText.StartsWith("—")) continue; // Skip separators

                        var question = new QuestionDef { Text = qText };

                        foreach (XElement aEl in qEl.Descendants("Answer"))
                        {
                            question.Answers.Add(new AnswerDef
                            {
                                Text = aEl.Value,
                                // Support both "Mood" and "PedMood" attributes
                                Mood = aEl.Attribute("PedMood")?.Value ?? aEl.Attribute("Mood")?.Value ?? "Neutral"
                            });
                        }

                        if (question.Answers.Count > 0)
                        {
                            group.Questions.Add(question);
                        }
                    }

                    if (group.Questions.Count > 0) Groups.Add(group);
                }
                Game.Console.Print($"[EmsPlus] Loaded {Groups.Count} question groups from Questions.xml.");
            }
            catch (Exception ex)
            {
                Game.Console.Print($"[EmsPlus] Error loading Questions.xml: {ex.Message}");
            }
        }

        private void CreateDefaultFile()
        {
            try
            {
                // Generates a default medical-focused XML if the user hasn't provided one yet
                var doc = new XDocument(new XElement("CustomQuestions",
                    new XElement("QuestionGroups",
                        new XElement("QuestionGroup", new XAttribute("Name", "Medical — Intro"),
                            new XElement("Questions",
                                new XElement("Question", new XAttribute("Question", "Hello, I'm a paramedic. What seems to be the problem today?"),
                                    new XElement("Answers",
                                        new XElement("Answer", new XAttribute("PedMood", "Neutral"), "I'm not feeling well, I need to go to the hospital."),
                                        new XElement("Answer", new XAttribute("PedMood", "Nervous"), "I don't know what's happening to me... please help!"),
                                        new XElement("Answer", new XAttribute("PedMood", "Sad"), "Everything hurts... I just want it to stop."),
                                        new XElement("Answer", new XAttribute("PedMood", "Mad"), "About time you got here! I've been waiting forever.")
                                    )
                                ),
                                new XElement("Question", new XAttribute("Question", "Can you tell me where it hurts the most?"),
                                    new XElement("Answers",
                                        new XElement("Answer", new XAttribute("PedMood", "Neutral"), "Mostly in my chest and arms."),
                                        new XElement("Answer", new XAttribute("PedMood", "Nervous"), "It's a sharp pain... it's everywhere!"),
                                        new XElement("Answer", new XAttribute("PedMood", "Mad"), "My chest! Are you blind? Do something!")
                                    )
                                )
                            )
                        ),
                        new XElement("QuestionGroup", new XAttribute("Name", "Medical — History"),
                            new XElement("Questions",
                                new XElement("Question", new XAttribute("Question", "Do you have any known medical conditions?"),
                                    new XElement("Answers",
                                        new XElement("Answer", new XAttribute("PedMood", "Neutral"), "I have high blood pressure."),
                                        new XElement("Answer", new XAttribute("PedMood", "Nervous"), "I had a heart attack a few years ago... is it happening again?"),
                                        new XElement("Answer", new XAttribute("PedMood", "Sad"), "I have asthma... I can't catch my breath.")
                                    )
                                ),
                                new XElement("Question", new XAttribute("Question", "Are you taking any prescribed medications?"),
                                    new XElement("Answers",
                                        new XElement("Answer", new XAttribute("PedMood", "Neutral"), "Just some standard blood thinners."),
                                        new XElement("Answer", new XAttribute("PedMood", "Nervous"), "I take nitroglycerin... but I lost my bottle!"),
                                        new XElement("Answer", new XAttribute("PedMood", "Sad"), "I haven't been able to afford my meds lately.")
                                    )
                                )
                            )
                        )
                    )
                ));
                doc.Save(FilePath);
            }
            catch { }
        }
    }
}