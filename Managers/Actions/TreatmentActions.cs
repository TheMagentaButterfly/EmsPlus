using EmsPlus.Core;
using EmsPlus.Medical;
using EmsPlus.UI.Tasks;
using EmsPlus.UI.Custom.InspectMenu;
using EmsPlus.UI.Native;
using Rage;

namespace EmsPlus.Managers.Actions
{
    public static class TreatmentActions
    {
        public static void AdministerGeneric(string medName)
        {
            var medDef = EntryPoint.MedicationConfig.GetByName(medName);
            Vector3 patientPos = GameState.CurrentPatient?.Character?.Position ?? Game.LocalPlayer.Character.Position;

            if (medDef != null)
            {
                string reqKit = medDef.RequiredKit;
                if (string.IsNullOrEmpty(reqKit) || reqKit == "NONE")
                {
                    if (medDef.Categories.Contains("IV") || medDef.Categories.Contains("IM")) reqKit = "TRAUMABAG";
                }

                if (reqKit != "NONE" && reqKit != null && !InventoryManager.IsKitAvailable(reqKit, patientPos))
                {
                    return;
                }
            }

            string animDict = EntryPoint.AnimationConfig.MedicTreatDict.Value;
            string animName = EntryPoint.AnimationConfig.MedicTreatName.Value;

            string message = Localization.GetFormat("ACTION_ADMINISTERING_GENERIC", "Administering {0}...", medName);

            if (message == "ACTION_ADMINISTERING_GENERIC") message = $"Administering {medName}...";

            ActionsCore.Run(message, 4000, animDict, animName, () =>
            {
                var p = GameState.CurrentPatient;
                if (p == null) return;

                if (medName == "Epinephrine") p.ApplyTreatment(EmsTreatment.Adrenaline);
                else if (medName == "Naloxone") p.ApplyTreatment(EmsTreatment.Naloxone);
                else if (medName == "Dextrose") p.ApplyTreatment(EmsTreatment.Glucose);
                else Game.DisplayNotification(string.Format(Localization.Get("NOTIF_ADMINISTERED", "~g~Administered {0}."), medName));
            });
        }

        public static void AdministerOxygen()
        {
            Vector3 patientPos = GameState.CurrentPatient?.Character?.Position ?? Game.LocalPlayer.Character.Position;
            bool hasO2 = InventoryManager.IsKitAvailable("OXYGENBAG", patientPos);
            bool hasTrauma = InventoryManager.IsKitAvailable("TRAUMABAG", patientPos);

            if (!hasO2 && !hasTrauma) { return; }

            ActionsCore.Run(Localization.Get("ACTION_ADMINISTERING_OXYGEN", "Applying Oxygen..."), 2000,
                EntryPoint.AnimationConfig.MedicTreatDict.Value,
                EntryPoint.AnimationConfig.MedicTreatName.Value, () =>
                {
                    if (GameState.CurrentPatient != null)
                    {
                        GameState.CurrentPatient.IsReceivingOxygen = true;
                        GameState.CurrentPatient.ApplyTreatment(EmsTreatment.Oxygen);
                    }
                });
        }

        public static void AdministerFluids()
        {
            Vector3 patientPos = GameState.CurrentPatient?.Character?.Position ?? Game.LocalPlayer.Character.Position;
            if (!InventoryManager.IsKitAvailable("TRAUMABAG", patientPos)) return;
            if (GameState.CurrentPatient == null || !GameState.CurrentPatient.IsIVEstablished) return;

            ActionsCore.Run(Localization.Get("ACTION_HANGING_SALINE_BAG", "Hanging Saline Bag..."), 3000,
                EntryPoint.AnimationConfig.MedicTreatDict.Value,
                EntryPoint.AnimationConfig.MedicTreatName.Value,
                () =>
                {
                    GameState.CurrentPatient.IsReceivingFluids = true;
                    GameState.CurrentPatient.ApplyTreatment(EmsTreatment.SalineBag);
                });
        }

        public static void EstablishIV(Rage.PedBoneId bone)
        {
            Vector3 patientPos = GameState.CurrentPatient?.Character?.Position ?? Game.LocalPlayer.Character.Position;
            if (!InventoryManager.IsKitAvailable("TRAUMABAG", patientPos)) return;

            // NATIVE-UI BYPASS
            if (EntryPoint.EmsPlusConfig.UseNativeUIPatientMenu.Value)
            {
                ActionsCore.Run(Localization.Get("ACT_ESTABLISHING_IV", "Establishing IV..."), 5000,
                    EntryPoint.AnimationConfig.MedicTreatDict.Value,
                    EntryPoint.AnimationConfig.MedicTreatName.Value,
                    () => {
                        if (GameState.CurrentPatient != null)
                        {
                            GameState.CurrentPatient.IsIVEstablished = true;
                            GameState.CurrentPatient.ApplyTreatment(EmsTreatment.IVAccess, bone);
                            Rage.Game.DisplayNotification(Localization.Get("NOTIF_IV_ESTABLISHED", "~g~IV Established."));
                        }
                    });
                return;
            }

            // CUSTOM 3D MENU
            GameState.IsPlayerBusy = true;
            MenuCore.CloseAll();
            BodyInspectionManager.StopInspection(false);

            var task = new IvTask();
            task.OnTaskCompleted += (s, e) => {
                if (task.IsActive && GameState.CurrentPatient != null)
                {
                    GameState.CurrentPatient.IsIVEstablished = true;
                    GameState.CurrentPatient.ApplyTreatment(EmsTreatment.IVAccess, bone);
                }
                GameState.IsPlayerBusy = false;
                BodyInspectionManager.StartInspection(GameState.CurrentPatient.Character);
            };

            task.OnTaskAborted += (s, e) => {
                GameState.IsPlayerBusy = false;
                BodyInspectionManager.StartInspection(GameState.CurrentPatient.Character);
            };

            task.Start();
        }

        public static void ApplyCCollar()
        {
            if (GameState.CurrentPatient == null) return;
            ActionsCore.Run(Localization.Get("ACTION_APPLYING_C_COLLAR", "Applying Cervical Collar..."), 4000,
                EntryPoint.AnimationConfig.MedicTreatDict.Value, EntryPoint.AnimationConfig.MedicTreatName.Value, () =>
                {
                    GameState.CurrentPatient.IsCCollarApplied = true;
                    GameState.CurrentPatient.ApplyTreatment(EmsTreatment.CervicalCollar, Rage.PedBoneId.Neck);
                });
        }

        public static void ApplySplint(Rage.PedBoneId bone)
        {
            if (GameState.CurrentPatient == null) return;
            ActionsCore.Run(Localization.Get("ACTION_APPLYING_SPLINT", "Applying Splint..."), 5000,
                EntryPoint.AnimationConfig.MedicTreatDict.Value, EntryPoint.AnimationConfig.MedicTreatName.Value, () =>
                {
                    GameState.CurrentPatient.IsLimbSplinted = true;
                    GameState.CurrentPatient.ApplyTreatment(EmsTreatment.Splint, bone);
                });
        }

        public static void TreatInjury(PhysicalInjury injury)
        {
            if (GameState.CurrentPatient == null || injury == null) return;

            string animDict = EntryPoint.AnimationConfig.MedicTreatDict.Value;
            string animName = EntryPoint.AnimationConfig.MedicTreatName.Value;

            ActionsCore.Run(Localization.Get("NOTIF_TREATING", "Treating injury..."), 3000, animDict, animName, () =>
            {
                GameState.CurrentPatient.ApplyTreatment(EmsTreatment.Bandage, injury.Bone);
            });
        }
    }
}