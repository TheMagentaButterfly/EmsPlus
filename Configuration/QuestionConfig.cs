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
                        if (string.IsNullOrEmpty(qText) || qText.StartsWith("—")) continue;

                        var question = new QuestionDef { Text = qText };

                        foreach (XElement aEl in qEl.Descendants("Answer"))
                        {
                            question.Answers.Add(new AnswerDef
                            {
                                Text = aEl.Value,
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
                        ),
                        new XElement("QuestionGroup", new XAttribute("Name", "Medical — Personal Info"),
                            new XElement("Questions",
                                new XElement("Question", new XAttribute("Question", "Can you tell me your name?"),
                                    new XElement("Answers",
                                        new XElement("Answer", new XAttribute("PedMood", "Neutral"), "My name is {FullName}."),
                                        new XElement("Answer", new XAttribute("PedMood", "Nervous"), "It's... it's {FirstName}. {FullName}."),
                                        new XElement("Answer", new XAttribute("PedMood", "Mad"), "It's {FullName}. Why does that matter right now?")
                                    )
                                ),
                                new XElement("Question", new XAttribute("Question", "How old are you, and what's your date of birth?"),
                                    new XElement("Answers",
                                        new XElement("Answer", new XAttribute("PedMood", "Neutral"), "I'm {Age}. Born on {DOB}.")
                                    )
                                ),
                                new XElement("Question", new XAttribute("Question", "Do you know your approximate height and weight?"),
                                    new XElement("Answers",
                                        new XElement("Answer", new XAttribute("PedMood", "Neutral"), "I'm about {Height} cm and {Weight} kg.")
                                    )
                                )
                            )
                        ),
                        new XElement("QuestionGroup", new XAttribute("Name", "Medical — Pain Assessment (OPQRST)"),
                            new XElement("Questions",
                                new XElement("Question", new XAttribute("Question", "On a scale of 1 to 10, how bad is the pain?"),
                                    new XElement("Answers",
                                        new XElement("Answer", new XAttribute("PedMood", "Neutral"), "It's about a 4. Uncomfortable, but I can manage."),
                                        new XElement("Answer", new XAttribute("PedMood", "Nervous"), "It's an 8! Maybe a 9! Please give me something for it!"),
                                        new XElement("Answer", new XAttribute("PedMood", "Sad"), "I don't know... maybe a 7. It just aches so much."),
                                        new XElement("Answer", new XAttribute("PedMood", "Mad"), "It's a 10! Are you deaf? I'm in agony here!")
                                    )
                                ),
                                new XElement("Question", new XAttribute("Question", "Does the pain radiate or move anywhere else?"),
                                    new XElement("Answers",
                                        new XElement("Answer", new XAttribute("PedMood", "Neutral"), "Yeah, it shoots down my {BodyPart}."),
                                        new XElement("Answer", new XAttribute("PedMood", "Nervous"), "It started in my chest, but now my {BodyPart} is numb!"),
                                        new XElement("Answer", new XAttribute("PedMood", "Confused"), "I can't tell... everywhere just feels wrong.")
                                    )
                                ),
                                new XElement("Question", new XAttribute("Question", "How long has it been hurting like this?"),
                                    new XElement("Answers",
                                        new XElement("Answer", new XAttribute("PedMood", "Neutral"), "It started about {TimeAmount} ago."),
                                        new XElement("Answer", new XAttribute("PedMood", "Mad"), "For {TimeAmount}! And it took you guys this long to show up?!")
                                    )
                                )
                            )
                        ),

                        new XElement("QuestionGroup", new XAttribute("Name", "Medical — Allergies & Intake"),
                            new XElement("Questions",
                                new XElement("Question", new XAttribute("Question", "Are you allergic to any medications?"),
                                    new XElement("Answers",
                                        new XElement("Answer", new XAttribute("PedMood", "Neutral"), "Yes, I have a severe allergy to {Allergy}."),
                                        new XElement("Answer", new XAttribute("PedMood", "Confused"), "I don't think so... wait, maybe {Allergy}? I can't remember."),
                                        new XElement("Answer", new XAttribute("PedMood", "Nervous"), "Yes, {Allergy}! Please don't give me that, it'll kill me!")
                                    )
                                ),
                                new XElement("Question", new XAttribute("Question", "When was the last time you had something to eat or drink?"),
                                    new XElement("Answers",
                                        new XElement("Answer", new XAttribute("PedMood", "Neutral"), "I had a sandwich and some water about {TimeAmount} ago."),
                                        new XElement("Answer", new XAttribute("PedMood", "Sad"), "I haven't eaten in days. I've been too sick."),
                                        new XElement("Answer", new XAttribute("PedMood", "Drunk"), "Just a few beers... like, {TimeAmount} ago. Or maybe 10 beers.")
                                    )
                                )
                            )
                        ),

                        new XElement("QuestionGroup", new XAttribute("Name", "Medical — Neurological (A&O)"),
                            new XElement("Questions",
                                new XElement("Question", new XAttribute("Question", "Can you tell me where we are right now?"),
                                    new XElement("Answers",
                                        new XElement("Answer", new XAttribute("PedMood", "Neutral"), "We are in {CityArea}, right on {StreetName}."),
                                        new XElement("Answer", new XAttribute("PedMood", "Confused"), "I... I don't know. Is this Los Santos? Everything is blurry."),
                                        new XElement("Answer", new XAttribute("PedMood", "Nervous"), "Why? Where did they take me? This isn't {CityArea}!")
                                    )
                                ),
                                new XElement("Question", new XAttribute("Question", "Do you remember what happened to you?"),
                                    new XElement("Answers",
                                        new XElement("Answer", new XAttribute("PedMood", "Neutral"), "I was just walking down {StreetName} and suddenly collapsed."),
                                        new XElement("Answer", new XAttribute("PedMood", "Confused"), "One minute I was driving, the next... nothing. Just sirens."),
                                        new XElement("Answer", new XAttribute("PedMood", "Mad"), "Some maniac bumped into me! You should be calling the cops!")
                                    )
                                )
                            )
                        ),

                        new XElement("QuestionGroup", new XAttribute("Name", "Medical — Trauma & Accidents"),
                            new XElement("Questions",
                                new XElement("Question", new XAttribute("Question", "Did you lose consciousness or black out at any point?"),
                                    new XElement("Answers",
                                        new XElement("Answer", new XAttribute("PedMood", "Neutral"), "No, I've been awake the whole time."),
                                        new XElement("Answer", new XAttribute("PedMood", "Confused"), "I think so. I woke up on the ground about {TimeAmount} ago."),
                                        new XElement("Answer", new XAttribute("PedMood", "Nervous"), "Yes! I saw stars and then everything went dark!")
                                    )
                                ),
                                new XElement("Question", new XAttribute("Question", "Did you hit your head during the fall/impact?"),
                                    new XElement("Answers",
                                        new XElement("Answer", new XAttribute("PedMood", "Neutral"), "No, my head is fine. Just my {BodyPart} took the hit."),
                                        new XElement("Answer", new XAttribute("PedMood", "Sad"), "Yes, my head is pounding. It feels like it's going to split open."),
                                        new XElement("Answer", new XAttribute("PedMood", "Confused"), "I... I can't be sure. There's a lot of blood, isn't there?")
                                    )
                                )
                            )
                        ),

                        new XElement("QuestionGroup", new XAttribute("Name", "Medical — Intoxication"),
                            new XElement("Questions",
                                new XElement("Question", new XAttribute("Question", "Have you had any alcohol or taken any recreational drugs today?"),
                                    new XElement("Answers",
                                        new XElement("Answer", new XAttribute("PedMood", "Neutral"), "No, absolutely not. I don't touch that stuff."),
                                        new XElement("Answer", new XAttribute("PedMood", "Nervous"), "Okay, I took something... but I don't know what it was!"),
                                        new XElement("Answer", new XAttribute("PedMood", "Mad"), "Mind your own business! Just fix me up and let me go!")
                                    )
                                ),
                                new XElement("Question", new XAttribute("Question", "Do you feel nauseous or like you need to throw up?"),
                                    new XElement("Answers",
                                        new XElement("Answer", new XAttribute("PedMood", "Neutral"), "No, my stomach feels fine."),
                                        new XElement("Answer", new XAttribute("PedMood", "Sad"), "Yes... I've been throwing up for the last {TimeAmount}.")
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