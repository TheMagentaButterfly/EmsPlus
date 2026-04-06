using Rage;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace EmsPlus
{
    public static class Localization
    {
        private static Dictionary<string, string> _strings = new Dictionary<string, string>();

        public static void Load()
        {
            _strings.Clear();

            string folder = Path.Combine(Application.StartupPath, "Plugins", "EmsPlus", "Settings", "Localization");

            string defaultFile = Path.Combine(folder, "English.ini");
            if (!File.Exists(defaultFile)) CreateDefaultFile(defaultFile);

            string selectedLang = EntryPoint.EmsPlusConfig.Language.Value;
            string targetFile = Path.Combine(folder, $"{selectedLang}.ini");

            if (!File.Exists(targetFile))
            {
                Game.Console.Print($"[EmsPlus] Language file '{selectedLang}.ini' not found. Falling back to English.");
                targetFile = defaultFile;
            }

            ParseIni(targetFile);
            Game.Console.Print($"[EmsPlus] Localization loaded: {Path.GetFileNameWithoutExtension(targetFile)} ({_strings.Count} keys).");
        }

        public static string Get(string key)
        {
            if (_strings.TryGetValue(key, out string value))
                return value;
            return key;
        }

        public static string Get(string key, params object[] args)
        {
            if (_strings.TryGetValue(key, out string value))
            {
                try { return string.Format(value, args); }
                catch { return value; }
            }
            return key;
        }

        private static void ParseIni(string path)
        {
            try
            {
                foreach (string line in File.ReadAllLines(path))
                {
                    string clean = line.Trim();
                    if (string.IsNullOrEmpty(clean) || clean.StartsWith(";") || clean.StartsWith("[")) continue;

                    int splitIndex = clean.IndexOf('=');
                    if (splitIndex > 0)
                    {
                        string key = clean.Substring(0, splitIndex).Trim();
                        string val = clean.Substring(splitIndex + 1).Trim();
                        val = val.Replace("\\n", "\n");

                        if (!_strings.ContainsKey(key))
                            _strings.Add(key, val);
                    }
                }
            }
            catch (System.Exception ex) { Game.Console.Print($"[EmsPlus] Error reading language file: {ex.Message}"); }
        }

        private static void CreateDefaultFile(string path)
        {
            using (StreamWriter w = new StreamWriter(path))
            {
                w.WriteLine("; =========================================================");
                w.WriteLine("; EmsPlus Localization File");
                w.WriteLine("; =========================================================");
                w.WriteLine("");

                w.WriteLine("[Duty Status]");
                w.WriteLine("NOTIF_ON_DUTY=~b~EmsPlus:~w~ You are now ~g~On Duty~w~.");
                w.WriteLine("NOTIF_OFF_DUTY=~b~EmsPlus:~w~ You are now ~r~Off Duty~w~.");
                w.WriteLine("NOTIF_STATUS_UPDATE=~b~Status Update:~w~ {0}");
                w.WriteLine("OFFDUTY=Off Duty");
                w.WriteLine("AVAILABLE=Available");
                w.WriteLine("ENROUTE=En Route");
                w.WriteLine("ONSCENE=On Scene");
                w.WriteLine("TRANSPORTING=Transporting");
                w.WriteLine("BUSY=Busy");
                w.WriteLine("");

                w.WriteLine("[Dispatch]");
                w.WriteLine("DISPATCH_HEADER=DISPATCH");
                w.WriteLine("DISPATCH_SUBTITLE=EMERGENCY CALL");
                w.WriteLine("DISPATCH_CALL_LABEL=~w~Call: ~b~{0}");
                w.WriteLine("DISPATCH_LOC_LABEL=~w~Location: ~y~{0}");
                w.WriteLine("NOTIF_CALLOUT_ACCEPT_PROMPT=~g~Y~w~ to accept ~o~/ ~r~N~w~ to decline.");
                w.WriteLine("NOTIF_CALLOUT_FORCE_ENDED=~b~Dispatch:~w~ Callout ~r~force-ended.");
                w.WriteLine("NOTIF_RESPOND_CODE3=~b~Dispatch:~w~ Respond to the scene, ~r~Code 3~w~.");
                w.WriteLine("NOTIF_SCENE_CODE4=~b~Dispatch:~w~ Scene is clear. ~g~Code 4~w~.");
                w.WriteLine("NOTIF_USER_TERMINATED=~b~Dispatch:~w~ Callout terminated by user.");
                w.WriteLine("");

                w.WriteLine("[Kit Localization]");
                w.WriteLine("TRAUMABAG_NAME=~r~Trauma Bag");
                w.WriteLine("TRAUMABAG_DESC=Contains Drugs, IVs, and advanced diagnostic tools.");
                w.WriteLine("OXYGENBAG_NAME=~b~Oxygen Bag");
                w.WriteLine("OXYGENBAG_DESC=Contains Oxygen tank and masks.");
                w.WriteLine("DEFIBRILLATOR_NAME=~g~Defibrillator");
                w.WriteLine("DEFIBRILLATOR_DESC=ECG, DEFIB, SpO2, NIBP.");
                w.WriteLine("");

                w.WriteLine("[Stretcher Prompts]");
                w.WriteLine("PROMPT_RELEASE_STRETCHER=[G] Release Stretcher");
                w.WriteLine("PROMPT_GRAB_STRETCHER=[G] Grab Stretcher");
                w.WriteLine("PROMPT_LOWER_STRETCHER=[H] Lower");
                w.WriteLine("PROMPT_RAISE_STRETCHER=[H] Raise");
                w.WriteLine("PROMPT_SITTING_STRETCHER=[J] Toggle Sitting");
                w.WriteLine("");

                w.WriteLine("[Menu Titles]");
                w.WriteLine("TITLE_INSPECTION=PATIENT INSPECTION");
                w.WriteLine("SUBTITLE_INSPECTION=Select area to inspect");
                w.WriteLine("TITLE_DIAGNOSTICS=DIAGNOSTICS");
                w.WriteLine("TITLE_ACTIONS=AVAILABLE ACTIONS");
                w.WriteLine("");

                w.WriteLine("[Menu Buttons]");
                w.WriteLine("BTN_DIAGNOSTICS=[TAB] Diagnostics");
                w.WriteLine("BTN_EXIT=[ESC] Exit");
                w.WriteLine("BTN_BACK=◄ BACK");
                w.WriteLine("BTN_BACK_DESC=Return to previous menu");
                w.WriteLine("BTN_PICKUP_KIT=PICK UP KIT");
                w.WriteLine("BTN_PICKUP_KIT_DESC=Equip to hands");
                w.WriteLine("BTN_TAKE_ITEM=TAKE {0}");
                w.WriteLine("BTN_TAKE_ITEM_DESC=Pull item from bag");
                w.WriteLine("BTN_ITEM_EMPTY_DESC=~r~Empty! Restock at ambulance.");
                w.WriteLine("BTN_PERFORM_ACTION=Perform {0}");
                w.WriteLine("BTN_APPLY_ACTION=Apply {0}");
                w.WriteLine("DESC_HANDS_ON=Use your hands");
                w.WriteLine("DESC_FROM_CABINET=From Ambulance Cabinets");
                w.WriteLine("");

                w.WriteLine("[Categories]");
                w.WriteLine("CAT_AIRWAY=Airway / Breathing");
                w.WriteLine("CAT_AIRWAY_DESC=Oxygen & Masks");
                w.WriteLine("CAT_MEDS=Medications");
                w.WriteLine("CAT_MEDS_DESC=Administer Drugs");
                w.WriteLine("CAT_IV=Circulation / IV");
                w.WriteLine("CAT_IV_DESC=Access & Fluids");
                w.WriteLine("CAT_IM=Intramuscular Injection");
                w.WriteLine("CAT_IM_DESC=Administer IM Drugs");
                w.WriteLine("CAT_TRAUMA=Trauma Supplies");
                w.WriteLine("CAT_TRAUMA_DESC=Bandages, Tourniquets");
                w.WriteLine("CAT_IMMOBILIZE=Immobilization");
                w.WriteLine("CAT_IMMOBILIZE_DESC=Splints, Collars");
                w.WriteLine("CAT_WOUNDCARE=Wound Care");
                w.WriteLine("CAT_WOUNDCARE_DESC=Dressings, Burns, Eyes");
                w.WriteLine("");

                w.WriteLine("[BodyParts]");
                w.WriteLine("BP_HEAD=Head");
                w.WriteLine("BP_NECK=Neck");
                w.WriteLine("BP_CHEST=Chest");
                w.WriteLine("BP_STOMACH=Stomach");
                w.WriteLine("BP_L_UPPER_ARM=Left Upper Arm");
                w.WriteLine("BP_L_FOREARM=Left Forearm");
                w.WriteLine("BP_L_HAND=Left Hand");
                w.WriteLine("BP_R_UPPER_ARM=Right Upper Arm");
                w.WriteLine("BP_R_FOREARM=Right Forearm");
                w.WriteLine("BP_R_HAND=Right Hand");
                w.WriteLine("BP_L_THIGH=Left Thigh");
                w.WriteLine("BP_L_CALF=Left Calf");
                w.WriteLine("BP_L_FOOT=Left Foot");
                w.WriteLine("BP_R_THIGH=Right Thigh");
                w.WriteLine("BP_R_CALF=Right Calf");
                w.WriteLine("BP_R_FOOT=Right Foot");
                w.WriteLine("");

                w.WriteLine("[Diagnostics]");
                w.WriteLine("DIAG_DISPATCH_DIAGNOSIS=DISPATCH DIAGNOSIS");
                w.WriteLine("DIAG_WITNESS_INFO=Witness Info");
                w.WriteLine("DIAG_WITNESS_PRESENT=Person Nearby");
                w.WriteLine("DIAG_WITNESS_ACTION=Question Witness");
                w.WriteLine("DIAG_MED_TAGS=Medical Tags");
                w.WriteLine("DIAG_MED_TAGS_DESC=Check Wallet/Tags");
                w.WriteLine("DIAG_MED_TAGS_ACTION=Read History");
                w.WriteLine("DIAG_BGL=Blood Glucose");
                w.WriteLine("DIAG_BGL_VALUE={0} mg/dL");
                w.WriteLine("DIAG_BP=Blood Pressure");
                w.WriteLine("DIAG_HR=Heart Rate");
                w.WriteLine("DIAG_SPO2=O2 Saturation");
                w.WriteLine("DIAG_MONITOR=Monitor");
                w.WriteLine("DIAG_MONITOR_NOT_CONNECTED=Not Connected");
                w.WriteLine("DIAG_CONSCIOUSNESS=Consciousness");
                w.WriteLine("DIAG_STATUS_REQUIRED=REQUIRED");
                w.WriteLine("DIAG_STATUS_TREATED=TREATED");
                w.WriteLine("DIAG_STATUS_STABLE=STABLE");
                w.WriteLine("DIAG_INJURY_DETECTED=INJURY DETECTED");
                w.WriteLine("DIAG_MINOR_BLEEDING=MINOR BLEEDING");
                w.WriteLine("DIAG_MODERATE_BLEEDING=MODERATE BLEEDING");
                w.WriteLine("DIAG_SEVERE_HEMORRHAGE=SEVERE HEMORRHAGE");
                w.WriteLine("DIAG_STATUS_UNKNOWN=UNKNOWN STATUS");
                w.WriteLine("DIAG_STATUS_CLEAR=CLEAR");
                w.WriteLine("DIAG_STATUS_EQUIPMENT=MEDICAL EQUIPMENT");
                w.WriteLine("");

                w.WriteLine("[Actions]");
                w.WriteLine("ACT_TREAT_INJURY=Treat Injury");
                w.WriteLine("ACT_APPLY_BANDAGE=Apply Bandage");
                w.WriteLine("ACT_ESTABLISH_IV=ESTABLISH IV");
                w.WriteLine("ACT_START_LINE=Start Intravenous Line");
                w.WriteLine("ACT_CHECK_BGL=Check BGL");
                w.WriteLine("ACT_GLUCOMETER=Use Glucometer");
                w.WriteLine("ACT_LOAD_PATIENT=LOAD PATIENT");
                w.WriteLine("ACT_SECURE_PATIENT=Secure to stretcher");
                w.WriteLine("ACT_UNLOAD_PATIENT=UNLOAD PATIENT");
                w.WriteLine("ACT_UNLOAD_PATIENT_ON_GROUND=Place on ground");
                w.WriteLine("ACT_CANNOT_UNLOAD_INSIDE_VEHICLE=Cannot unload inside vehicle");
                w.WriteLine("ACT_APPLIED_PREFIX=Applied: {0}");
                w.WriteLine("ACT_ATTACH_MONITOR=Attach ECG/SpO2 Monitor");
                w.WriteLine("ACT_REMOVE_MONITOR=Remove ECG/SpO2 Monitor");
                w.WriteLine("ACT_ATTACH_BP=Attach BP Cuff");
                w.WriteLine("ACT_REMOVE_BP=Remove BP Cuff");
                w.WriteLine("ACT_HANG_FLUIDS=Hang IV Fluids");
                w.WriteLine("ACT_STOP_FLUIDS=STOP FLUIDS");
                w.WriteLine("");

                w.WriteLine("[Treatments]");
                w.WriteLine("TRT_BANDAGE=Bandage");
                w.WriteLine("TRT_TOURNIQUET=Tourniquet");
                w.WriteLine("TRT_JUNCTIONALTOURNIQUET=Junctional TQ");
                w.WriteLine("TRT_WOUNDPACKING=Wound Packing");
                w.WriteLine("TRT_DIRECT_PRESSURE=Direct Pressure");
                w.WriteLine("TRT_SUTURE=Suture Kit");
                w.WriteLine("TRT_SPLINT=SAM Splint");
                w.WriteLine("TRT_TRACTIONSPLINT=Traction Splint");
                w.WriteLine("TRT_PELVICBINDER=Pelvic Binder");
                w.WriteLine("TRT_CERVICALCOLLAR=C-Collar");
                w.WriteLine("TRT_SPINALIMMOBILISATION=Backboard");
                w.WriteLine("TRT_CHESTSEAL=Chest Seal");
                w.WriteLine("TRT_NEEDLEDECOMP=Needle Decompression");
                w.WriteLine("TRT_MANAGE_AIRWAY=Manage Airway");
                w.WriteLine("TRT_OXYGEN=O2 Mask");
                w.WriteLine("TRT_HIGHFLOWOXYGEN=NRB Mask (High Flow)");
                w.WriteLine("TRT_BAGVALVEMASK=BVM (Bag Valve)");
                w.WriteLine("TRT_CPR=CPR");
                w.WriteLine("TRT_IVACCESS=IV Start Kit");
                w.WriteLine("TRT_SALINEBAG=Saline Bag");
                w.WriteLine("TRT_BURNDRESSING=Burn Dressing");
                w.WriteLine("TRT_WETDRESSING=Wet Dressing");
                w.WriteLine("TRT_ICEPACK=Ice Pack");
                w.WriteLine("TRT_IRRIGATION=Irrigation Fluid");
                w.WriteLine("TRT_EYEPATCH=Eye Patch");
                w.WriteLine("TRT_EYESHIELD=Eye Shield");
                w.WriteLine("TRT_STABILISEOBJECT=Object Stabilizer");
                w.WriteLine("TRT_IVACCESS=IV Start Kit");
                w.WriteLine("TRT_NALOXONE=Naloxone");
                w.WriteLine("TRT_GLUCOSE=Glucose");
                w.WriteLine("TRT_ADRENALINE=Adrenaline");
                w.WriteLine("");

                w.WriteLine("[Injuries]");
                w.WriteLine("INJURY_BRUISING=Bruising");
                w.WriteLine("INJURY_LACERATION=Laceration");
                w.WriteLine("INJURY_FRACTURE=Simple Fracture");
                w.WriteLine("INJURY_COMPOUNDFRACTURE=Compound Fracture");
                w.WriteLine("INJURY_ARTERIALBLEED=Arterial Bleed");
                w.WriteLine("INJURY_GUNSHOTWOUND=Gunshot Wound");
                w.WriteLine("INJURY_STABWOUND=Stab Wound");
                w.WriteLine("INJURY_EVISCERATION=Evisceration");
                w.WriteLine("INJURY_AMPUTATION=Traumatic Amputation");
                w.WriteLine("INJURY_BURN1=1st Degree Burn");
                w.WriteLine("INJURY_BURN2=2nd Degree Burn");
                w.WriteLine("INJURY_BURN3=3rd Degree Burn");
                w.WriteLine("INJURY_TENSIONPNEUMOTHORAX=Tension Pneumothorax");
                w.WriteLine("INJURY_SUCKINGCHESTWOUND=Sucking Chest Wound");
                w.WriteLine("INJURY_BLASTINJURY=Blast Injury");
                w.WriteLine("INJURY_SHRAPNEL=Shrapnel Wounds");
                w.WriteLine("");

                w.WriteLine("[Requirements]");
                w.WriteLine("REQ_TRAUMA_BAG=Requires Trauma Bag");
                w.WriteLine("REQ_OXYGEN_BAG=Requires Oxygen Bag");
                w.WriteLine("REQ_IV=Requires IV Access");
                w.WriteLine("REQ_GENERIC=Requires {0}");
                w.WriteLine("ERR_INVALID_TARGET=~r~Invalid Target:~w~ That body part doesn't need this tool.");
                w.WriteLine("ERR_ANATOMY_FAIL=~r~Cannot apply this treatment to this location.");
                w.WriteLine("ERR_TOOL_MISMATCH=~r~Wrong Tool:~w~ Select a different item.");
                w.WriteLine("ERR_NO_CABIN=~r~This vehicle does not have an accessible cabin.");
                w.WriteLine("HINT_OPEN_BAG=Open your medical bag to get tools for this.");
                w.WriteLine("");

                w.WriteLine("[Notifications]");
                w.WriteLine("NOTIF_CONFIGSRELOADED=~b~EmsPlus~w~: All configurations reloaded!");
                w.WriteLine("NOTIF_INSPECTING=Inspecting...");
                w.WriteLine("NOTIF_TREATING=Treating injury...");
                w.WriteLine("NOTIF_INJURY_TREATED=~g~Injury Treated.");
                w.WriteLine("NOTIF_ADMINISTERED=~g~Administered:~w~ {0}");
                w.WriteLine("NOTIF_PATIENT_STABILIZED=~g~The patient has been stabilized!");
                w.WriteLine("NOTIF_POSITION_COPIED=Position copied to clipboard!");
                w.WriteLine("NOTIF_TOOL_PREPARED=~b~Prepared:~w~ {0}. Select injured body part.");
                w.WriteLine("NOTIF_MONITOR_CONNECTED=~g~Monitor connected.");
                w.WriteLine("NOTIF_MONITOR_DISCONNECTED=~y~Monitor disconnected.");
                w.WriteLine("NOTIF_BP_CONNECTED=~g~BP Cuff attached.");
                w.WriteLine("NOTIF_BP_REMOVED=~y~BP Cuff removed.");
                w.WriteLine("ACT_ATTACHING_LEADS=Attaching leads...");
                w.WriteLine("ACT_REMOVING_LEADS=Removing leads...");
                w.WriteLine("ACT_APPLYING_CUFF=Applying cuff...");
                w.WriteLine("ACT_REMOVING_CUFF=Removing cuff...");
                w.WriteLine("NOTIF_FLUIDS_STOPPED=~y~Fluids stopped.");
                w.WriteLine("NOTIF_ALREADYPLACED_KIT=~r~You already have this kit deployed.");
                w.WriteLine("NOTIF_MUST_HOLD_STRETCHER=~r~You must be holding the stretcher.");
                w.WriteLine("SUBTITLE_TRANSPORT_PATIENT=~y~Transport patient to the nearest hospital.");
                w.WriteLine("NOTIF_HOSPITAL_WAYPOINT_SET=~b~Dispatch:~w~ Route to nearest hospital set.");
                w.WriteLine("NOTIF_PATIENT_HANDED_OVER=~g~Transport Complete:~w~ Patient handed over to hospital staff.");
                w.WriteLine("NOTIF_VEHICLE_DETECTED=~g~Vehicle Detected: {0}");
                w.WriteLine("NOTIF_NO_AMBULANCE_NEARBY=~r~No Valid Vehicle Nearby");
                w.WriteLine("HELP_HANDOVER_PATIENT=Press ~INPUT_CONTEXT~ to handover patient.");
                w.WriteLine("NOTIF_MEDICALBAGS_RESTOCKED=~b~Dispatch:~w~ Medical bags ~g~restocked~w~.");
                w.WriteLine("");

                w.WriteLine("[Patient States]");
                w.WriteLine("CONSC_ALERT=Alert");
                w.WriteLine("CONSC_VERBAL=Verbal");
                w.WriteLine("CONSC_PAIN=Pain");
                w.WriteLine("CONSC_UNRESPONSIVE=Unresponsive");
                w.WriteLine("VITAL_NONE=NONE / FLATLINE");
                w.WriteLine("VITAL_CRITICAL_LOW=CRITICAL LOW");
                w.WriteLine("VITAL_LOW=LOW");
                w.WriteLine("VITAL_NORMAL=NORMAL");
                w.WriteLine("VITAL_ELEVATED=ELEVATED");
                w.WriteLine("VITAL_CRITICAL_HIGH=CRITICAL HIGH");
                w.WriteLine("");

                w.WriteLine("[Ambulance Menu]");
                w.WriteLine("MENU_AMBULANCE_TITLE=Ambulance");
                w.WriteLine("MENU_AMBULANCE_SUBTITLE=~b~Interaction");
                w.WriteLine("HELP_OPEN_AMBULANCE_MENU=Press {0} to open the ambulance menu.");
                w.WriteLine("ITEM_TOGGLE_DOORS=Toggle Rear Doors");
                w.WriteLine("ITEM_OPEN_REAR_DOORS=Open Rear Doors");
                w.WriteLine("ITEM_CLOSE_REAR_DOORS=Close Rear Doors");
                w.WriteLine("ITEM_LOAD_STRETCHER=Load Stretcher");
                w.WriteLine("ITEM_UNLOAD_STRETCHER=Unload Stretcher");
                w.WriteLine("ITEM_ENTER_CABIN=Enter Patient Cabin");
                w.WriteLine("ITEM_EXIT_CABIN=Exit Patient Cabin");
                w.WriteLine("ITEM_MEDICAL_EQUIPMENT=Medical Equipment");
                w.WriteLine("ITEM_STORE_CURRENT=Store Current Item");
                w.WriteLine("DESC_LOAD_STRETCHER=Load the Stretcher into the ambulance.");
                w.WriteLine("DESC_UNLOAD_STRETCHER=Unload the Stretcher from the ambulance.");
                w.WriteLine("DESC_MUST_HOLD_STRETCHER=You must be holding the stretcher to load it into the vehicle.");
                w.WriteLine("DESC_ACCESS_RESTRICTED=~r~Cannot use while inside a vehicle.");
                w.WriteLine("DESC_TOGGLE_DOORS=Open or Close the rear doors.");
                w.WriteLine("DESC_ENTER_CABIN=Sit in the back to treat the patient en-route.");
                w.WriteLine("DESC_EXIT_CABIN=Step out of the ambulance.");
                w.WriteLine("DESC_COLLECT_KITS=Put away all current items and collect equipment from the ground.");
                w.WriteLine("DESC_STORE_CURRENT=Put away current item.");
                w.WriteLine("LABEL_ACCESS_RESTRICTED=~c~Access Restricted");
                w.WriteLine("ITEM_NONE_FOUND=None Found");
                w.WriteLine("DESC_NO_EQUIPMENT=No equipment found.");
                w.WriteLine("DESC_EXIT_VEHICLE=~r~Exit vehicle to use this menu.");
                w.WriteLine("");

                w.WriteLine("[Config Menus]");
                w.WriteLine("MENU_CONFIG_TITLE=EmsPlus");
                w.WriteLine("MENU_CONFIG_SUBTITLE=~b~Configuration");
                w.WriteLine("MENU_FORCE_CALLOUT_TITLE=Force Callout");
                w.WriteLine("MENU_FORCE_CALLOUT_SUBTITLE=~b~Debug Menu");
                w.WriteLine("ITEM_FORCE_CALLOUT=Force Callout");
                w.WriteLine("ITEM_FORCE_CALLOUT_DESC=Manually start a specific callout for testing or roleplay.");
                w.WriteLine("MENU_OFFSETS_ROOT_TITLE=Offsets Configuration");
                w.WriteLine("MENU_OFFSETS_ROOT_SUBTITLE=~b~Adjust Positions");
                w.WriteLine("ITEM_OFFSETS_POSITIONS=Offsets & Positions");
                w.WriteLine("ITEM_OFFSETS_POSITIONS_DESC=Adjust stretcher, patient, and vehicle offsets.");
                w.WriteLine("ITEM_SAVE_SETTINGS=~g~Save All Settings");
                w.WriteLine("ITEM_SAVE_SETTINGS_DESC=Writes changes to the .ini files.");
                w.WriteLine("MENU_PROP_OFFSETS_TITLE=Prop Offsets");
                w.WriteLine("MENU_PROP_OFFSETS_SUBTITLE=~b~Hand Position");
                w.WriteLine("ITEM_PROP_CARRY_OFFSETS=Prop Carry Offsets");
                w.WriteLine("ITEM_PROP_CARRY_OFFSETS_DESC=Adjust position of medical bags in hand.");
                w.WriteLine("MENU_STRETCHER_CARRY_TITLE=Carry Position");
                w.WriteLine("MENU_STRETCHER_CARRY_SUBTITLE=~b~Player Attachment");
                w.WriteLine("ITEM_STRETCHER_CARRY_POS=Stretcher Carry Position");
                w.WriteLine("MENU_PATIENT_POS_TITLE=Patient Position");
                w.WriteLine("MENU_PATIENT_POS_SUBTITLE=~b~Stretcher Attachment");
                w.WriteLine("ITEM_PATIENT_ON_STRETCHER=Patient on Stretcher");
                w.WriteLine("MENU_VEHICLE_CONFIG_TITLE=Vehicle Configuration");
                w.WriteLine("MENU_VEHICLE_CONFIG_SUBTITLE=~b~Ambulance Setup");
                w.WriteLine("ITEM_VEHICLE_LOADING_POS=Vehicle Settings");
                w.WriteLine("LABEL_OFFSET_X=Offset X");
                w.WriteLine("LABEL_OFFSET_Y=Offset Y");
                w.WriteLine("LABEL_OFFSET_Z=Offset Z");
                w.WriteLine("LABEL_PITCH=Pitch");
                w.WriteLine("LABEL_ROLL=Roll");
                w.WriteLine("LABEL_YAW=Yaw");
                w.WriteLine("MODE_NORMAL_STRETCHER=Normal Stretcher");
                w.WriteLine("MODE_LOWERED_STRETCHER=Lowered Stretcher");
                w.WriteLine("MODE_SITTING_STRETCHER=Sitting Stretcher");
                w.WriteLine("MODE_STOWED_POS=Stowed Position");
                w.WriteLine("MODE_SLIDE_POS=Slide Position");
                w.WriteLine("ITEM_EDITING_MODE=Editing Mode");
                w.WriteLine("ITEM_EDITING_MODE_DESC_PATIENT=Select which stretcher state to adjust.");
                w.WriteLine("ITEM_EDITING_MODE_DESC_VEHICLE=Select which state to adjust.");
                w.WriteLine("ITEM_RELOAD_VEHICLE=Reload/Detect Vehicle");
                w.WriteLine("ITEM_RELOAD_VEHICLE_DESC=Find closest vehicle.");
                w.WriteLine("MENU_DOOR_SETUP_TITLE=Door Setup");
                w.WriteLine("MENU_DOOR_SETUP_SUBTITLE=~b~Select Doors");
                w.WriteLine("ITEM_CONFIGURE_DOORS=Configure Doors");
                w.WriteLine("ITEM_CONFIGURE_DOORS_DESC=Select which doors open.");
                w.WriteLine("DOOR_FRONT_LEFT=Front Left");
                w.WriteLine("DOOR_FRONT_RIGHT=Front Right");
                w.WriteLine("DOOR_BACK_LEFT=Back Left");
                w.WriteLine("DOOR_BACK_RIGHT=Back Right");
                w.WriteLine("DOOR_BACK_AUX_1=Hood/Back Aux");
                w.WriteLine("DOOR_BACK_AUX_2=Trunk/Back Aux");
                w.WriteLine("DOOR_EXTRA_1=Extra 1");
                w.WriteLine("DOOR_EXTRA_2=Extra 2");
                w.WriteLine("ITEM_SELECT_KIT=Select Kit");
                w.WriteLine("ITEM_SELECT_KIT_DESC_PROP=Select which item to adjust.");
                w.WriteLine("ITEM_ADD_TO_ALLOWED=Add to Allowed Vehicles");
                w.WriteLine("ITEM_ADD_TO_ALLOWED_DESC=Enable interaction menu and settings saving for this vehicle model.");
                w.WriteLine("ITEM_CAN_HAVE_STRETCHER=Can Have Stretcher");
                w.WriteLine("ITEM_CAN_HAVE_STRETCHER_DESC=If disabled, this vehicle will not have a stretcher.");
                w.WriteLine("ITEM_CAN_ENTER_CABIN=Has Patient Cabin");
                w.WriteLine("ITEM_CAN_ENTER_CABIN_DESC=If disabled, you cannot enter the rear of the ambulance.");
                w.WriteLine("MENU_INTERACTION_POINTS_TITLE=Interaction Points");
                w.WriteLine("MENU_INTERACTION_POINTS_SUBTITLE=~b~Configure Menu Hotspots");
                w.WriteLine("DESC_INTERACTION_POINTS=Add or remove menu hotspot locations.");
                w.WriteLine("MODE_MEDIC_SEAT_POS=Medic Seat Pos");
                w.WriteLine("LABEL_LEFT_RIGHT=(Left/Right)");
                w.WriteLine("LABEL_FORWARD_BACK=(Forward/Back)");
                w.WriteLine("LABEL_UP_DOWN=(Up/Down)");
                w.WriteLine("LABEL_TILT=(Tilt)");
                w.WriteLine("LABEL_LEAN=(Lean)");
                w.WriteLine("LABEL_ROTATE=(Rotate)");
                w.WriteLine("ITEM_NO_VEHICLE=~r~No Vehicle Detected");
                w.WriteLine("DESC_NO_VEHICLE=Get near an ambulance and use 'Reload/Detect Vehicle' in the previous menu.");
                w.WriteLine("ITEM_ADD_INTERACTION_POINT=~g~Add New Interaction Point");
                w.WriteLine("DESC_ADD_INTERACTION_POINT=Adds a new point at the vehicle's origin (0,0,0) for you to adjust.");
                w.WriteLine("MENU_POINT_TITLE=Point #{0}");
                w.WriteLine("MENU_POINT_SUBTITLE=~b~Edit Offset");
                w.WriteLine("ITEM_EDIT_POINT=~b~•~s~ Edit Point #{0}");
                w.WriteLine("DESC_EDIT_POINT=Offset: ({0:F2}, {1:F2}, {2:F2}) | Scale: {3:F1}");
                w.WriteLine("ITEM_DELETE_POINT=~r~Delete This Point");
                w.WriteLine("DESC_DELETE_POINT=Removes this interaction point permanently.");
                w.WriteLine("LABEL_SCALE_SIZE=Scale / Size");
                w.WriteLine("DESC_STRETCHER_CARRY_POS=Adjust stretcher carry position");
                w.WriteLine("DESC_PATIENT_ON_STRETCHER=Configure patient positioning");
                w.WriteLine("DESC_VEHICLE_LOADING_POS=Vehicle-specific settings");
                w.WriteLine("ITEM_NO_KITS_LOADED=No Kits Loaded");
                w.WriteLine("NOTIF_SETTINGS_SAVED_GENERIC=Settings Saved.");
                w.WriteLine("NOTIF_SETTINGS_SAVED_VEHICLE=Settings saved for {0}.");
                w.WriteLine("NOTIF_CONFIG_NOT_SAVED=Not an allowed vehicle. Add it to allowed list first.");
                w.WriteLine("NOTIF_VEHICLE_ADDED=Added {0} to allowed vehicles.");
                w.WriteLine("NOTIF_VEHICLE_REMOVED=Removed {0} from allowed vehicles.");
                w.WriteLine("");

                w.WriteLine("[Patient Menu]");
                w.WriteLine("MENU_PATIENT_TITLE=Patient");
                w.WriteLine("MENU_PATIENT_SUBTITLE=~b~Medical Interaction");
                w.WriteLine("MENU_DIAGNOSTICS_TITLE=Diagnostics");
                w.WriteLine("MENU_DIAGNOSTICS_SUBTITLE=~b~Assess Patient");
                w.WriteLine("MENU_TRAUMA_TITLE=Trauma");
                w.WriteLine("MENU_TRAUMA_SUBTITLE=~b~Treat Injuries");
                w.WriteLine("MENU_AIRWAY_TITLE=Airway");
                w.WriteLine("MENU_AIRWAY_SUBTITLE=~b~Airway & Breathing");
                w.WriteLine("MENU_ORAL_TITLE=Oral");
                w.WriteLine("MENU_ORAL_SUBTITLE=~b~Oral Medications");
                w.WriteLine("MENU_IV_TITLE=IV / Access");
                w.WriteLine("MENU_IV_SUBTITLE=~b~Fluids & IV Lines");
                w.WriteLine("MENU_IM_TITLE=IM");
                w.WriteLine("MENU_IM_SUBTITLE=~b~Intramuscular Injections");
                w.WriteLine("MENU_GROUND_KITS_TITLE=Ground Kits");
                w.WriteLine("MENU_GROUND_KITS_SUBTITLE=~b~Interact with Medical Kits");
                w.WriteLine("DESC_ASSESS_VITALS=Assess patient vitals and history.");
                w.WriteLine("DESC_TREAT_INJURIES=Bandage visible injuries.");
                w.WriteLine("DESC_AIRWAY_MASKS=Oxygen & Masks");
                w.WriteLine("DESC_ORAL_MEDS=Administer oral medications.");
                w.WriteLine("DESC_IV_LINES=Lines, Fluids, IV Meds.");
                w.WriteLine("DESC_IM_MEDS=Intramuscular medications.");
                w.WriteLine("DESC_GROUND_KITS=Interact with kits you placed nearby.");
                w.WriteLine("ITEM_STATUS_CONSCIOUSNESS=Status: {0}");
                w.WriteLine("ITEM_CHECK_MED_TAGS=Check Medical Tags");
                w.WriteLine("DESC_CHECK_MED_TAGS=Check wallet or bracelet");
                w.WriteLine("ITEM_CHECK_BGL=Check BGL");
                w.WriteLine("DESC_CHECK_BGL=Use Glucometer");
                w.WriteLine("ITEM_IV_ESTABLISHED=IV ESTABLISHED");
                w.WriteLine("DESC_IV_ESTABLISHED=Access available");
                w.WriteLine("ITEM_HANG_FLUIDS=Hang Fluids");
                w.WriteLine("DESC_HANG_FLUIDS=0.9% Normal Saline");
                w.WriteLine("ITEM_STOP_FLUIDS=Stop Fluids");
                w.WriteLine("DESC_STOP_FLUIDS=Disconnect Line");
                w.WriteLine("ITEM_NO_KITS_NEARBY=No Kits Nearby");
                w.WriteLine("DESC_NO_KITS_NEARBY=Drop a medical bag near the patient to interact with it.");
                w.WriteLine("ITEM_PICK_UP_KIT_FORMAT=Pick Up {0}");
                w.WriteLine("DESC_PICK_UP_KIT=Equip this item to your hands.");
                w.WriteLine("ITEM_ATTACH_MONITOR=Attach Leads/SpO2");
                w.WriteLine("DESC_ATTACH_MONITOR=Connect Monitor");
                w.WriteLine("ITEM_REMOVE_MONITOR=Remove Leads/SpO2");
                w.WriteLine("DESC_REMOVE_MONITOR=Disconnect Monitor");
                w.WriteLine("ITEM_ATTACH_BP_CUFF=Attach BP Cuff");
                w.WriteLine("DESC_ATTACH_BP_CUFF=Auto-Cycle BP");
                w.WriteLine("ITEM_REMOVE_BP_CUFF=Remove BP Cuff");
                w.WriteLine("DESC_REMOVE_BP_CUFF=Remove Cuff");
                w.WriteLine("ITEM_APPLY_TREATMENT_FORMAT=Apply {0}");
                w.WriteLine("DESC_TREATMENT_LOCATION_FORMAT=Location: ~y~{0}");
                w.WriteLine("ITEM_NO_INJURIES=~g~No Injuries Found");
                w.WriteLine("DESC_NO_INJURIES=Patient has no visible untreated trauma.");
                w.WriteLine("ACT_APPLY_OXYGEN_MASK=Apply Oxygen Mask");
                w.WriteLine("DESC_STANDARD_O2=Standard O2 Therapy");
                w.WriteLine("ACT_APPLY_NRB_MASK=Apply NRB Mask");
                w.WriteLine("DESC_HIGH_FLOW_O2=High Flow Oxygen");
                w.WriteLine("ACT_USE_BVM=Use Bag Valve Mask");
                w.WriteLine("DESC_ASSIST_VENTILATIONS=Assist Ventilations");
                w.WriteLine("ACT_MANUAL_AIRWAY=Manual Airway Maneuver");
                w.WriteLine("DESC_HEAD_TILT=Head-tilt/Chin-lift");
                w.WriteLine("ACT_GIVE_ORAL_GLUCOSE=Give Oral Glucose");
                w.WriteLine("DESC_TREAT_HYPOGLYCEMIA=Treat Hypoglycemia");
                w.WriteLine("ACT_GIVE_ORAL_ANALGESIA=Give Oral Analgesia");
                w.WriteLine("DESC_MILD_PAIN_RELIEF=Mild Pain Relief");
                w.WriteLine("ACT_PUSH_EPI=Push Adrenaline (Epi)");
                w.WriteLine("DESC_EPI_DOSE=1mg 1:10,000 IV");
                w.WriteLine("ACT_PUSH_ANALGESIA=Push Analgesia");
                w.WriteLine("DESC_STRONG_IV_PAIN_RELIEF=Strong IV Pain Relief");
                w.WriteLine("ACT_PUSH_NALOXONE=Push Naloxone");
                w.WriteLine("DESC_OPIOID_ANTAG_IV=Opioid Antagonist IV");
                w.WriteLine("ACT_EPI_AUTO_INJECTOR=Epinephrine Auto-Injector");
                w.WriteLine("DESC_IM_INJECTION=IM Injection");
                w.WriteLine("ACT_NALOXONE_IM=Naloxone (IM/IN)");
                w.WriteLine("DESC_OPIOID_ANTAG=Opioid Antagonist");
                w.WriteLine("ACT_ANALGESIA_IM=Analgesia (IM)");
                w.WriteLine("DESC_IM_PAIN_RELIEF=IM Pain Relief");
                w.WriteLine("");

                w.WriteLine("[Station Manager]");
                w.WriteLine("BLIP_STATION_NAME=Fire/EMS Station");
                w.WriteLine("TEXT_ON_DUTY=~g~On Duty");
                w.WriteLine("TEXT_OFF_DUTY=~r~Off Duty");
                w.WriteLine("HELP_TOGGLE_DUTY=Press ~INPUT_CONTEXT~ to go {0}.");
                w.WriteLine("");

                w.WriteLine("[Interactive Tasks]");
                w.WriteLine("TASK_HELP_TEXT=Drag items to complete the procedure");
                w.WriteLine("TASK_BGL_TITLE=BLOOD GLUCOSE TEST");
                w.WriteLine("TASK_BGL_STEP_PRICK=Prick the finger");
                w.WriteLine("TASK_BGL_STEP_SAMPLE=Collect sample");
                w.WriteLine("TASK_BGL_STEP_INSERT=Insert into glucometer");
                w.WriteLine("TASK_BGL_STEP_COMPLETE=Complete!");
                w.WriteLine("TASK_IV_TITLE=IV PLACEMENT");
                w.WriteLine("TASK_IV_STEP_SPIKE=Spike bag");
                w.WriteLine("TASK_IV_STEP_CATH=Place catheter");
                w.WriteLine("TASK_IV_STEP_VEIN=Locate vein");
                w.WriteLine("TASK_IV_STEP_CONNECT=Connect line");
                w.WriteLine("TASK_IV_STEP_FIX=Secure tape");
                w.WriteLine("TASK_IV_STEP_COMPLETE=Complete!");
                w.WriteLine("TASK_SKILL_CHECK_PASS=~g~Pass!");
                w.WriteLine("TASK_SKILL_CHECK_FAIL=~r~Fail!");
            }
        }
    }
}