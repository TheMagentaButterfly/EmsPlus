using EmsPlus.Managers;
using EmsPlus.Managers.Actions;
using EmsPlus.Medical;
using Rage;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace EmsPlus.UI.Custom.InspectMenu.Menus
{
    public static class MainMenu
    {
        private static readonly List<EmsTreatment> HandsOnTreatments = new List<EmsTreatment>
        {
            EmsTreatment.DirectPressure,
            EmsTreatment.AirwayManagement,
            EmsTreatment.CPR,
            EmsTreatment.RecoveryPosition,
            EmsTreatment.ActiveRewarming,
            EmsTreatment.ActiveCooling,
            EmsTreatment.Monitoring
        };

        public static void Build(BodyPart part, Patient p)
        {
            bool inCabin = AmbulanceManager.IsPlayerInRearCabin;
            bool hasTrauma = InventoryManager.IsKitAvailable("TRAUMABAG", p.Character.Position);
            var injury = p.Conditions.OfType<PhysicalInjury>().FirstOrDefault(i => i.Bone == part.BoneId && !i.IsTreated);
            if (part.LinkedEntity != null) return;

            if (part.LinkedEntity == null)
            {
                if (injury != null)
                {
                    var directActions = injury.RequiredTreatments.Where(t => HandsOnTreatments.Contains(t)).ToList();
                    foreach (var action in directActions)
                    {
                        string label = FormatTreatmentName(action.ToString());
                        BodyInspectionManager.CurrentPanelActions.Add(new InspectionAction(
                            Localization.Get("BTN_PERFORM_ACTION", label), Localization.Get("DESC_HANDS_ON"), Color.Orange, true, () => {
                                BodyInspectionManager.HandleTreatmentLogic(action, injury.Bone);
                            }
                        ));
                    }

                    var toolActions = injury.RequiredTreatments.Where(t => !HandsOnTreatments.Contains(t) && AnatomicalRegistry.IsLocalizedTreatment(t)).ToList();
                    if (toolActions.Count > 0)
                    {
                        if (inCabin)
                        {
                            foreach (var action in toolActions)
                            {
                                string label = FormatTreatmentName(action.ToString());
                                BodyInspectionManager.CurrentPanelActions.Add(new InspectionAction(
                                    Localization.Get("BTN_APPLY_ACTION", label), string.Format(Localization.Get("DESC_FROM_CABINET"), label), Color.LightBlue, true, () => {
                                        BodyInspectionManager.HandleTreatmentLogic(action, injury.Bone);
                                    }
                                ));
                            }
                        }
                        else
                        {
                            BodyInspectionManager.CurrentPanelActions.Add(new InspectionAction(
                                Localization.Get("DIAG_INJURY_DETECTED"), Localization.Get("HINT_OPEN_BAG"), Color.FromArgb(255, 100, 100, 100), false, null
                            ));
                        }
                    }
                }
            }

            if (part.BoneId == PedBoneId.Spine3)
            {
                if (p.Conditions.Any(c => !c.IsTreated && c.RequiredTreatments.Contains(EmsTreatment.CPR)))
                {
                    BodyInspectionManager.CurrentPanelActions.Add(new InspectionAction(
                        Localization.Get("TRT_CPR"), "", Color.Orange, true, () => {
                            BodyInspectionManager.HandleTreatmentLogic(EmsTreatment.CPR, part.BoneId);
                        }
                    ));
                }
            }

            // 2. AMBULANCE MONITOR LOGIC
            if (inCabin && part.BoneId == PedBoneId.Spine3)
            {
                if (!p.IsEcgsConnected)
                {
                    BodyInspectionManager.CurrentPanelActions.Add(new InspectionAction(Localization.Get("ACT_ATTACH_MONITOR"), Localization.Get("ACT_ATTACH_MONITOR"), Color.LightGreen, true, () => {
                        ActionsCore.Run(Localization.Get("ACT_ATTACHING_LEADS"), 2000, EntryPoint.AnimationConfig.MedicTreatDict.Value, EntryPoint.AnimationConfig.MedicTreatName.Value, () => { p.IsEcgsConnected = true; Game.DisplayNotification(Localization.Get("NOTIF_MONITOR_CONNECTED")); });
                        BodyInspectionManager.StopInspection(false);
                    }));
                }
                else
                {
                    BodyInspectionManager.CurrentPanelActions.Add(new InspectionAction(Localization.Get("ACT_REMOVE_MONITOR"), Localization.Get("ACT_REMOVE_MONITOR"), Color.Orange, true, () => {
                        ActionsCore.Run(Localization.Get("ACT_REMOVING_LEADS"), 1500, EntryPoint.AnimationConfig.MedicTreatDict.Value, EntryPoint.AnimationConfig.MedicTreatName.Value, () => { p.IsEcgsConnected = false; Game.DisplayNotification(Localization.Get("NOTIF_MONITOR_DISCONNECTED")); });
                        BodyInspectionManager.StopInspection(false);
                    }));
                }
            }

            if (inCabin && (part.BoneId == PedBoneId.LeftUpperArm || part.BoneId == PedBoneId.RightUpperArm))
            {
                if (!p.IsBpCuffConnected)
                {
                    BodyInspectionManager.CurrentPanelActions.Add(new InspectionAction(Localization.Get("ACT_ATTACH_BP"), Localization.Get("ACT_ATTACH_BP"), Color.LightGreen, true, () => {
                        ActionsCore.Run(Localization.Get("ACT_APPLYING_CUFF"), 2000, EntryPoint.AnimationConfig.MedicTreatDict.Value, EntryPoint.AnimationConfig.MedicTreatName.Value, () => { p.IsBpCuffConnected = true; Game.DisplayNotification(Localization.Get("NOTIF_BP_CONNECTED")); });
                        BodyInspectionManager.StopInspection(false);
                    }));
                }
                else
                {
                    BodyInspectionManager.CurrentPanelActions.Add(new InspectionAction(Localization.Get("ACT_REMOVE_BP"), Localization.Get("ACT_REMOVE_BP"), Color.Orange, true, () => {
                        ActionsCore.Run(Localization.Get("ACT_REMOVING_CUFF"), 1500, EntryPoint.AnimationConfig.MedicTreatDict.Value, EntryPoint.AnimationConfig.MedicTreatName.Value, () => { p.IsBpCuffConnected = false; Game.DisplayNotification(Localization.Get("NOTIF_BP_REMOVED")); });
                        BodyInspectionManager.StopInspection(false);
                    }));
                }
            }

            // 3. STRETCHER LOGIC (Only outside)
            if (!inCabin && (part.BoneId == PedBoneId.Spine3 || part.BoneId == PedBoneId.Spine))
            {
                bool isStretcherAvailable = StretcherManager.Prop != null && StretcherManager.Prop.Exists() && !StretcherManager.IsAttachedToVehicle;
                bool nearStretcher = isStretcherAvailable && p.Character.DistanceTo(StretcherManager.Prop) < 4.0f;

                if (p.IsOnStretcher)
                {
                    BodyInspectionManager.CurrentPanelActions.Add(new InspectionAction(
                        Localization.Get("ACT_UNLOAD_PATIENT"), isStretcherAvailable ? Localization.Get("ACT_UNLOAD_PATIENT_ON_GROUND") : Localization.Get("ACT_CANNOT_UNLOAD_INSIDE_VEHICLE"),
                        Color.FromArgb(255, 255, 150, 0), isStretcherAvailable,
                        () => { StretcherActions.UnloadPatient(); BodyInspectionManager.StopInspection(false); }
                    ));
                }
                else if (nearStretcher)
                {
                    BodyInspectionManager.CurrentPanelActions.Add(new InspectionAction(
                        Localization.Get("ACT_LOAD_PATIENT"), Localization.Get("ACT_SECURE_PATIENT"), Color.FromArgb(255, 0, 255, 100), true,
                        () => { StretcherActions.LoadPatient(); BodyInspectionManager.StopInspection(false); }
                    ));
                }
            }

            // 4. CATEGORY NAVIGATION BUTTONS (IV / Meds / Airway)
            if (part.BoneId == PedBoneId.Head || part.BoneId == PedBoneId.Neck)
            {
                AddBtn(Localization.Get("CAT_AIRWAY"), Localization.Get("CAT_AIRWAY_DESC"), Color.FromArgb(255, 0, 180, 255), "AIRWAY");
            }
            if (hasTrauma || inCabin)
            {
                if (part.BoneId == PedBoneId.Head)
                {
                    AddBtn(Localization.Get("CAT_MEDS"), Localization.Get("CAT_MEDS_DESC"), Color.FromArgb(255, 255, 50, 50), "ORAL");
                }
                if (part.BoneId == PedBoneId.LeftUpperArm || part.BoneId == PedBoneId.RightUpperArm || part.BoneId == PedBoneId.LeftThigh || part.BoneId == PedBoneId.RightThigh)
                {
                    AddBtn(Localization.Get("CAT_IM"), Localization.Get("CAT_IM_DESC"), Color.FromArgb(255, 255, 50, 50), "IM");
                }
                if (part.BoneId == PedBoneId.LeftForeArm || part.BoneId == PedBoneId.RightForearm || part.BoneId == PedBoneId.LeftHand || part.BoneId == PedBoneId.RightHand)
                {
                    AddBtn(Localization.Get("CAT_IV"), Localization.Get("CAT_IV_DESC"), Color.FromArgb(255, 255, 100, 0), "IV");
                }
            }
        }

        private static void AddBtn(string title, string sub, Color col, string cat)
        {
            BodyInspectionManager.CurrentPanelActions.Add(new InspectionAction(title, sub, col, true, () => {
                BodyInspectionManager.CurrentMenuCategory = cat;
                BodyInspectionManager.RefreshActions();
            }));
        }

        private static string FormatTreatmentName(string input)
        {
            if (input == "DirectPressure") return Localization.Get("TRT_DIRECT_PRESSURE");
            if (input == "AirwayManagement") return Localization.Get("TRT_MANAGE_AIRWAY");
            if (input == "ChestSeal") return Localization.Get("TRT_CHESTSEAL");
            if (input == "NeedleDecomp") return Localization.Get("TRT_NEEDLEDECOMP");
            if (input == "CervicalCollar") return Localization.Get("TRT_CERVICALCOLLAR");
            if (input == "PelvicBinder") return Localization.Get("TRT_PELVICBINDER");
            if (input == "TractionSplint") return Localization.Get("TRT_TRACTIONSPLINT");
            if (input == "WoundPacking") return Localization.Get("TRT_WOUNDPACKING");
            if (input == "JunctionalTourniquet") return Localization.Get("TRT_JUNCTIONALTOURNIQUET");
            if (input == "BurnDressing") return Localization.Get("TRT_BURNDRESSING");
            if (input == "WetDressing") return Localization.Get("TRT_WETDRESSING");
            if (input == "EyePatch") return Localization.Get("TRT_EYEPATCH");
            if (input == "EyeShield") return Localization.Get("TRT_EYESHIELD");
            if (input == "IcePack") return Localization.Get("TRT_ICEPACK");
            return input;
        }
    }
}