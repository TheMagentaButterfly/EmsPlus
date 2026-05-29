using Rage;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace EmsPlus
{
    public static class Localization
    {
        private static Dictionary<string, string> _strings = new Dictionary<string, string>();
        private static string _currentLangFile = "";

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

            _currentLangFile = targetFile;
            ParseIni(targetFile);
            Game.Console.Print($"[EmsPlus] Localization loaded: {Path.GetFileNameWithoutExtension(targetFile)} ({_strings.Count} keys).");
        }

        /// <summary>
        /// Gets a localized string. If the key does not exist and a defaultText is provided, 
        /// it automatically adds the key and text to the .ini file so translators can find it later!
        /// </summary>
        public static string Get(string key, string defaultText = null)
        {
            if (_strings.TryGetValue(key, out string value))
                return value;

            if (!string.IsNullOrEmpty(defaultText))
            {
                _strings[key] = defaultText;
                AppendMissingKey(key, defaultText);
                return defaultText;
            }

            return key;
        }

        /// <summary>
        /// A helper method for formatted strings that also supports auto-appending.
        /// </summary>
        public static string GetFormat(string key, string defaultText, params object[] args)
        {
            string text = Get(key, defaultText);
            try { return string.Format(text, args); }
            catch { return text; }
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

        private static void AppendMissingKey(string key, string value)
        {
            try
            {
                if (string.IsNullOrEmpty(_currentLangFile) || !File.Exists(_currentLangFile)) return;

                string safeValue = value.Replace("\n", "\\n");

                File.AppendAllText(_currentLangFile, $"{key}={safeValue}\n");

                Game.Console.Print($"[EmsPlus] Localization Auto-Added: '{key}'");
            }
            catch { }
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
                w.WriteLine("; English");
                w.WriteLine("; =========================================================");
                w.WriteLine("");

                w.WriteLine("STATUS_AVAILABLE=~g~Available");
                w.WriteLine("STATUS_AVAILABLEATSTATION=~g~Available at Station");
                w.WriteLine("STATUS_ENROUTE=~r~En Route");
                w.WriteLine("STATUS_ONSCENE=~r~On Scene");
                w.WriteLine("STATUS_REQUESTTOSPEAK=~r~Request to speak");
                w.WriteLine("STATUS_OFFDUTY=~u~Off Duty");
                w.WriteLine("STATUS_TRANSPORTING=~r~Transporting");
                w.WriteLine("STATUS_ATDESTINATION=~y~At Destination");
                w.WriteLine("STATUS_BUSY=~m~Busy");
                w.WriteLine("STATUS_URGENTREQUESTTOSPEAK=~h~~p~Urgent request to speak");
                w.WriteLine("STATUS_EMERGENCY=~h~~p~Emergency");

                w.WriteLine("ACT_APPLY_NRB_MASK=Apply NRB Mask");
                w.WriteLine("ACT_APPLY_OXYGEN_MASK=Apply Oxygen Mask");
                w.WriteLine("ACT_APPLY_TREATMENT_COLORED=Apply {0}");
                w.WriteLine("ACT_APPLYING_CUFF=Applying BP Cuff...");
                w.WriteLine("ACT_APPLYING_NRB_MASK=Applying NRB Mask...");
                w.WriteLine("ACT_APPLYING_O2_MASK=Applying O2 Mask...");
                w.WriteLine("ACT_ASSISTING_VENTILATIONS=Assisting Ventilations...");
                w.WriteLine("ACT_ATTACH_BP=Attach BP Cuff");
                w.WriteLine("ACT_ATTACH_BP_DESC=Attach BP Cuff");
                w.WriteLine("ACT_ATTACH_MONITOR=Attach ECG/SpO2 Monitor");
                w.WriteLine("ACT_ATTACH_MONITOR_DESC=Attach ECG/SpO2 Monitor");
                w.WriteLine("ACT_ATTACHING_LEADS=Attaching leads...");
                w.WriteLine("ACT_CANNOT_UNLOAD_INSIDE_VEHICLE=Cannot unload inside vehicle");
                w.WriteLine("ACT_CHECK_BGL=Check BGL");
                w.WriteLine("ACT_CHECKING_BGL=Checking Blood Glucose...");
                w.WriteLine("ACT_ESTABLISH_IV=Establish IV");
                w.WriteLine("ACT_ESTABLISHING_IV=Establishing IV...");
                w.WriteLine("ACT_GLUCOMETER=Use Glucometer");
                w.WriteLine("ACT_HANG_FLUIDS=Hang Fluids");
                w.WriteLine("ACT_LOAD_PATIENT=Load Patient");
                w.WriteLine("ACT_MANAGING_AIRWAY=Managing Airway...");
                w.WriteLine("ACT_MANUAL_AIRWAY=Manual Airway Management");
                w.WriteLine("ACT_PICK_UP_KIT_FORMAT=Pick Up {0}");
                w.WriteLine("ACT_QUESTION_PATIENT=QUESTION PATIENT");
                w.WriteLine("ACT_REMOVE_BP=Remove BP Cuff");
                w.WriteLine("ACT_REMOVE_BP_DESC=Remove BP Cuff");
                w.WriteLine("ACT_REMOVE_MONITOR=Remove ECG/SpO2 Monitor");
                w.WriteLine("ACT_REMOVE_MONITOR_DESC=Remove ECG/SpO2 Monitor");
                w.WriteLine("ACT_REMOVING_CUFF=Removing BP Cuff...");
                w.WriteLine("ACT_REMOVING_LEADS=Removing leads...");
                w.WriteLine("ACT_SECURE_PATIENT=Secure Patient");
                w.WriteLine("ACT_START_LINE=Start IV Line");
                w.WriteLine("ACT_STOP_FLUIDS=Stop Fluids");
                w.WriteLine("ACT_TRAUMA_INSPECT=Inspecting...");
                w.WriteLine("ACT_TRAUMA_SWEEP=Trauma Sweep");
                w.WriteLine("ACT_UNLOAD_PATIENT=Unload Patient");
                w.WriteLine("ACT_UNLOAD_PATIENT_ON_GROUND=Unload patient on ground");
                w.WriteLine("ACT_USE_BVM=Use Bag Valve Mask");
                w.WriteLine("ACTION_ADMINISTERING_GENERIC=Administering {0}...");
                w.WriteLine("ACTION_ADMINISTERING_OXYGEN=Applying Oxygen...");
                w.WriteLine("ACTION_APPLYING_C_COLLAR=Applying Cervical Collar...");
                w.WriteLine("ACTION_APPLYING_SPLINT=Applying Splint...");
                w.WriteLine("ACTION_HANGING_SALINE_BAG=Hanging Saline Bag...");
                w.WriteLine("ACTION_LAY=Lay Patient Down");
                w.WriteLine("ACTION_LOWER=Lower");
                w.WriteLine("ACTION_RAISE=Raise");
                w.WriteLine("ACTION_SIT=Sit Patient Up");
                w.WriteLine("AVAILABLE_ACTIONS=AVAILABLE ACTIONS");
                w.WriteLine("BP_CHEST=Chest");
                w.WriteLine("BP_HEAD=Head");
                w.WriteLine("BP_L_CALF=Left Calf");
                w.WriteLine("BP_L_FOOT=Left Foot");
                w.WriteLine("BP_L_FOREARM=Left Forearm");
                w.WriteLine("BP_L_HAND=Left Hand");
                w.WriteLine("BP_L_THIGH=Left Thigh");
                w.WriteLine("BP_L_UPPER_ARM=Left Upper Arm");
                w.WriteLine("BP_NECK=Neck");
                w.WriteLine("BP_R_CALF=Right Calf");
                w.WriteLine("BP_R_FOOT=Right Foot");
                w.WriteLine("BP_R_FOREARM=Right Forearm");
                w.WriteLine("BP_R_HAND=Right Hand");
                w.WriteLine("BP_R_THIGH=Right Thigh");
                w.WriteLine("BP_R_UPPER_ARM=Right Upper Arm");
                w.WriteLine("BP_STOMACH=Stomach");
                w.WriteLine("BTN_APPLY_ACTION=Apply {0}");
                w.WriteLine("BTN_BACK=◄ BACK");
                w.WriteLine("BTN_BACK_DESC=Return");
                w.WriteLine("BTN_DIAGNOSTICS=[TAB] Diagnostics");
                w.WriteLine("BTN_EXIT=[ESC] Exit");
                w.WriteLine("BTN_ITEM_EMPTY_DESC=~r~Empty! Restock at ambulance.");
                w.WriteLine("BTN_PATIENT_DATA=[TAB] Patient Data");
                w.WriteLine("BTN_PERFORM_ACTION=Perform {0}");
                w.WriteLine("BTN_PICKUP_KIT=PICK UP KIT");
                w.WriteLine("BTN_PICKUP_KIT_DESC=Equip to hands");
                w.WriteLine("BTN_TAKE_ITEM_DESC=Take item from bag");
                w.WriteLine("CAT_AIRWAY=Airway / Breathing");
                w.WriteLine("CAT_AIRWAY_DESC=Oxygen & Masks");
                w.WriteLine("CAT_CIRCULATION=");
                w.WriteLine("CAT_CIRCULATION_DESC=");
                w.WriteLine("CAT_IM=Intramuscular Injection");
                w.WriteLine("CAT_IM_DESC=Administer IM Drugs");
                w.WriteLine("CAT_IMMOBILIZE=Immobilization");
                w.WriteLine("CAT_IMMOBILIZE_DESC=Splints, Collars");
                w.WriteLine("CAT_IV=Circulation / IV");
                w.WriteLine("CAT_IV_DESC=IV Access & Fluids");
                w.WriteLine("CAT_MEDS=Medications");
                w.WriteLine("CAT_MEDS_DESC=Administer Drugs");
                w.WriteLine("CAT_SEP_ASSESSMENTS=~c~=== ~b~ASSESSMENTS ~c~===");
                w.WriteLine("CAT_SEP_CONDITIONS=~c~=== ~r~ACTIVE CONDITIONS ~c~===");
                w.WriteLine("CAT_SEP_DEFIB=~c~=== DEFIBRILLATOR ===");
                w.WriteLine("CAT_SEP_IV_MEDS=~c~=== IV MEDS ===");
                w.WriteLine("CAT_SEP_LOGISTICS=~c~=== ~p~LOGISTICS ~c~===");
                w.WriteLine("CAT_SEP_MEDICAL_INFO=~c~=== ~r~MEDICAL INFO ~c~===");
                w.WriteLine("CAT_SEP_MEDICATIONS=~c~=== ~g~MEDICATIONS ~c~===");
                w.WriteLine("CAT_SEP_OXYGEN=~c~=== OXYGEN & MASKS ===");
                w.WriteLine("CAT_SEP_TREATMENTS=~c~=== ~r~TREATMENTS ~c~===");
                w.WriteLine("CAT_TRAUMA=Trauma Supplies");
                w.WriteLine("CAT_TRAUMA_DESC=Bandages, Tourniquets");
                w.WriteLine("CAT_WOUNDCARE=Wound Care");
                w.WriteLine("CAT_WOUNDCARE_DESC=Dressings, Burns, Eyes");
                w.WriteLine("DATA_ALLERGY=Known Allergies");
                w.WriteLine("DATA_BLOOD=Blood Type");
                w.WriteLine("DATA_DOB=DOB & Age");
                w.WriteLine("DATA_GENDER=Gender");
                w.WriteLine("DATA_HEIGHT=Height");
                w.WriteLine("DATA_NAME=Name");
                w.WriteLine("DATA_WEIGHT=Weight");
                w.WriteLine("DEFIBRILLATOR_DESC=A device used to deliver an electric shock to the heart.");
                w.WriteLine("DEFIBRILLATOR_NAME=Defibrillator");
                w.WriteLine("DESC_ADD_INTERACTION_POINT=Add a new interaction point.");
                w.WriteLine("DESC_AIRWAY=Manage airway");
                w.WriteLine("DESC_ASSIST_VENTILATIONS=Assist Ventilations");
                w.WriteLine("DESC_ATTACH_BP_CUFF=Auto-Cycle BP");
                w.WriteLine("DESC_ATTACH_MONITOR=Connect ECG/SpO2 Leads");
                w.WriteLine("DESC_COLLECT_KITS=Collect the kits.");
                w.WriteLine("DESC_DELETE_POINT=Delete this interaction point.");
                w.WriteLine("DESC_DIAGNOSTICS=Assess patient");
                w.WriteLine("DESC_EDIT_POINT=Edit the interaction point at X: {0}, Y: {1}, Z: {2}, Scale: {3}");
                w.WriteLine("DESC_ENABLE_TUTORIAL=Enable or disable the tutorial");
                w.WriteLine("DESC_ENTER_CABIN=Enter or exit the ambulance cabin");
                w.WriteLine("DESC_EXIT_CABIN=Exit the ambulance cabin.");
                w.WriteLine("DESC_EXIT_VEHICLE=You must exit the vehicle to access this menu.");
                w.WriteLine("DESC_FROM_CABINET=From Ambulance Cabinets");
                w.WriteLine("DESC_GROUND_KITS=Manage ground kits");
                w.WriteLine("DESC_HANDS_ON=Use your hands");
                w.WriteLine("DESC_HANG_FLUIDS=Hang IV Fluids");
                w.WriteLine("DESC_HEAD_TILT=Head-tilt/Chin-lift");
                w.WriteLine("DESC_HIGH_FLOW_O2=High Flow Oxygen");
                w.WriteLine("DESC_IM=Intramuscular");
                w.WriteLine("DESC_INJURY_LOCATION=Location: ~y~{0}~w~ ({1})");
                w.WriteLine("DESC_INTERACTION_POINTS=Configure interaction points");
                w.WriteLine("DESC_IV=Manage IV");
                w.WriteLine("DESC_IV_ESTABLISHED=Access available");
                w.WriteLine("DESC_LOAD_STRETCHER=Load a stretcher into the ambulance");
                w.WriteLine("DESC_MUST_HOLD_STRETCHER=You must hold the stretcher to load it.");
                w.WriteLine("DESC_NO_INJURIES_NATIVEUI=Patient has no visible untreated trauma. Make sure to perform a Trauma Sweep from the Diagnostics menu first.");
                w.WriteLine("DESC_NO_KITS_NEARBY=There are no kits nearby.");
                w.WriteLine("DESC_NO_VEHICLE=No vehicle is currently selected.");
                w.WriteLine("DESC_PATIENT_DATA=View patient data");
                w.WriteLine("DESC_PATIENT_ON_STRETCHER=Configure patient positions on stretcher");
                w.WriteLine("DESC_PICK_UP_KIT=Equip this item to your hands.");
                w.WriteLine("DESC_QUESTION_CATEGORY= Select a category");
                w.WriteLine("DESC_QUESTION_PATIENT=Question the patient");
                w.WriteLine("DESC_REMOVE_BP_CUFF=Remove Cuff");
                w.WriteLine("DESC_REMOVE_MONITOR=Disconnect ECG/SpO2 Leads");
                w.WriteLine("DESC_SELECT_QUESTION=Select a question");
                w.WriteLine("DESC_STANDARD_O2=Standard O2 Therapy");
                w.WriteLine("DESC_STOP_FLUIDS=Stop IV Fluids");
                w.WriteLine("DESC_STORE_CURRENT=Store the current item");
                w.WriteLine("DESC_STRETCHER_CARRY_POS=Configure stretcher carry positions");
                w.WriteLine("DESC_TIP_FORMAT=Status: {0}\nRequired: {1}");
                w.WriteLine("DESC_TOGGLE_DOORS=Open or close the ambulance doors");
                w.WriteLine("DESC_TRAUMA=Manage trauma");
                w.WriteLine("DESC_TRAUMA_SWEEP=Perform a trauma sweep");
                w.WriteLine("DESC_UNLOAD_STRETCHER=Unload a stretcher from the ambulance");
                w.WriteLine("DESC_VEHICLE_LOADING_POS=Configure vehicle loading positions");
                w.WriteLine("DIAG_BGL=Blood Glucose");
                w.WriteLine("DIAG_BP=Blood Pressure");
                w.WriteLine("DIAG_CONSCIOUSNESS=Consciousness");
                w.WriteLine("CONSC_PAIN=Pain");
                w.WriteLine("CONSC_ALERT=Alert");
                w.WriteLine("CONSC_VERBAL=Verbal");
                w.WriteLine("CONSC_UNRESPONSIVE=Unresponsive");
                w.WriteLine("DIAG_DISPATCH_DIAGNOSIS=DISPATCH DIAGNOSIS");
                w.WriteLine("DIAG_HR=Heart Rate");
                w.WriteLine("DIAG_INJURY_DETECTED=INJURY DETECTED");
                w.WriteLine("DIAG_MONITOR=Monitor");
                w.WriteLine("DIAG_MONITOR_NOT_CONNECTED=Not Connected");
                w.WriteLine("DIAG_SPO2=O2 Saturation");
                w.WriteLine("DIAG_STATUS_CLEAR=CLEAR");
                w.WriteLine("DIAG_STATUS_EQUIPMENT=MEDICAL EQUIPMENT");
                w.WriteLine("DIAG_STATUS_INTERVIEW=PATIENT INTERVIEW");
                w.WriteLine("DIAG_STATUS_NEEDS_TREATMENT=~r~Needs Treatment");
                w.WriteLine("DIAG_STATUS_NONE=~c~None");
                w.WriteLine("DIAG_STATUS_REQUIRED=REQUIRED");
                w.WriteLine("DIAG_STATUS_TREATED=TREATED");
                w.WriteLine("DIAG_STATUS_UNKNOWN=UNKNOWN STATUS");
                w.WriteLine("DOOR_BACK_AUX_1=Hood/Back Aux");
                w.WriteLine("DOOR_BACK_AUX_2=Trunk/Back Aux");
                w.WriteLine("DOOR_BACK_LEFT=Back Left");
                w.WriteLine("DOOR_BACK_RIGHT=Back Right");
                w.WriteLine("DOOR_EXTRA_1=Extra 1");
                w.WriteLine("DOOR_EXTRA_2=Extra 2");
                w.WriteLine("DOOR_FRONT_LEFT=Front Left");
                w.WriteLine("DOOR_FRONT_RIGHT=Front Right");
                w.WriteLine("ERR_NO_CABIN=~r~This vehicle does not have an accessible cabin.");
                w.WriteLine("ERR_NO_STRETCHER_FOR_CABIN=~r~Cannot enter cabin.~w~ The stretcher must be loaded first.");
                w.WriteLine("ERR_STRETCHER_IN_AMBULANCE=~r~Cannot load patient:~w~ stretcher is already in ambulance.");
                w.WriteLine("ERR_MUST_ENTER_CABIN=~r~You must enter the patient cabin to inspect the patient.");
                w.WriteLine("HELP_AMBULANCE_MENU=Press ~y~{0}~w~ to open the equipment menu.");
                w.WriteLine("HELP_ENTER_INTERIOR=Press ~INPUT_CONTEXT~ to enter {0}.");
                w.WriteLine("HELP_EXIT_INTERIOR=Press ~INPUT_CONTEXT~ to exit.");
                w.WriteLine("HELP_HANDOVER_PATIENT=Press ~INPUT_CONTEXT~ to handover patient.");
                w.WriteLine("HELP_INSPECT_PATIENT=Press ~y~{0}~w~ to inspect the patient.");
                w.WriteLine("HELP_OPEN_AMBULANCE_MENU=Press ~y~{0}~w~ to open the ambulance menu.");
                w.WriteLine("HELP_STRETCHER_CONTROL_GRAB=~y~{0}~w~: {1} Stretcher");
                w.WriteLine("HELP_STRETCHER_CONTROL_HEIGHT=~y~{0}~w~: {1}");
                w.WriteLine("HELP_STRETCHER_CONTROL_SIT=~y~{0}~w~: {1}");
                w.WriteLine("HELP_TALK_BYSTANDER=Press ~y~Y~w~ to talk to the witness.");
                w.WriteLine("HELP_TALK_TO_PATIENT=Press ~y~Y~w~ to talk to the patient.");
                w.WriteLine("HELP_TALK_TO_WITNESS=Press ~y~Y~w~ to talk to the witness.");
                w.WriteLine("HELP_TOGGLE_DUTY=Press ~INPUT_CONTEXT~ to go {0}.");
                w.WriteLine("HINT_OPEN_BAG=Open your medical bag to get tools for this.");
                w.WriteLine("ITEM_ADD_INTERACTION_POINT=Add Interaction Point");
                w.WriteLine("ITEM_ADD_TO_ALLOWED=Add to Allowed Vehicles");
                w.WriteLine("ITEM_ADD_TO_ALLOWED_DESC=Enable interaction menu for this model.");
                w.WriteLine("ITEM_CAN_ENTER_CABIN=Has Patient Cabin");
                w.WriteLine("ITEM_CAN_ENTER_CABIN_DESC=If disabled, you cannot enter the rear of the ambulance.");
                w.WriteLine("ITEM_CAN_HAVE_STRETCHER=Can Have Stretcher");
                w.WriteLine("ITEM_CAN_HAVE_STRETCHER_DESC=If disabled, acts as rapid response unit.");
                w.WriteLine("ITEM_CLOSE_REAR_DOORS=Close Rear Doors");
                w.WriteLine("ITEM_CONFIGURE_DOORS=Configure Doors");
                w.WriteLine("ITEM_CONFIGURE_DOORS_DESC=Configure the doors of the vehicle");
                w.WriteLine("ITEM_DELETE_POINT=Delete Point");
                w.WriteLine("ITEM_EDIT_POINT=Edit Point {0}");
                w.WriteLine("ITEM_EDITING_MODE=Editing Mode");
                w.WriteLine("ITEM_EDITING_MODE_DESC_PATIENT=Select the mode for editing patient positions");
                w.WriteLine("ITEM_EDITING_MODE_DESC_VEHICLE=Select the mode to edit");
                w.WriteLine("ITEM_ENABLE_TUTORIAL=Enable Tutorial");
                w.WriteLine("ITEM_ENTER_CABIN=Enter Cabin");
                w.WriteLine("ITEM_EXIT_CABIN=Exit Cabin");
                w.WriteLine("ITEM_FORCE_CALLOUT=Force Callout");
                w.WriteLine("ITEM_FORCE_CALLOUT_DESC=Force a callout to occur");
                w.WriteLine("ITEM_HANG_FLUIDS=Hang Fluids");
                w.WriteLine("ITEM_HIDE_STRETCHER=Hide Stretcher in Vehicle");
                w.WriteLine("ITEM_HIDE_STRETCHER_DESC=Hides the stretcher when inside.");
                w.WriteLine("ITEM_IV_ESTABLISHED_COLORED=~g~IV ESTABLISHED");
                w.WriteLine("ITEM_LOAD_STRETCHER=Load Stretcher");
                w.WriteLine("ITEM_MEDICAL_EQUIPMENT=Medical Equipment");
                w.WriteLine("ITEM_NO_INJURIES_COLORED=~g~No Injuries Found");
                w.WriteLine("ITEM_NO_KITS_NEARBY=No Kits Nearby");
                w.WriteLine("ITEM_NO_VEHICLE=No Vehicle");
                w.WriteLine("ITEM_OFFSETS_POSITIONS=Offsets & Positions");
                w.WriteLine("ITEM_OFFSETS_POSITIONS_DESC=Configure offsets and positions");
                w.WriteLine("ITEM_OPEN_REAR_DOORS=Open Rear Doors");
                w.WriteLine("ITEM_PATIENT_ON_STRETCHER=Patient on Stretcher");
                w.WriteLine("ITEM_PROP_CARRY_OFFSETS=Prop Carry Offsets");
                w.WriteLine("ITEM_PROP_CARRY_OFFSETS_DESC=Configure prop carry offsets");
                w.WriteLine("ITEM_RELOAD_VEHICLE=Reload Vehicle");
                w.WriteLine("ITEM_RELOAD_VEHICLE_DESC=Reload the current vehicle configuration");
                w.WriteLine("ITEM_SAVE_SETTINGS=Save Settings");
                w.WriteLine("ITEM_SAVE_SETTINGS_DESC=Save the current settings");
                w.WriteLine("ITEM_SELECT_KIT=Select Kit");
                w.WriteLine("ITEM_SELECT_KIT_DESC_PROP=Select the kit to edit");
                w.WriteLine("ITEM_STORE_CURRENT=Store Current");
                w.WriteLine("ITEM_STRETCHER_CARRY_POS=Stretcher Carry Positions");
                w.WriteLine("ITEM_TOGGLE_DOORS=Toggle Doors");
                w.WriteLine("ITEM_UNLOAD_STRETCHER=Unload Stretcher");
                w.WriteLine("ITEM_VEHICLE_LOADING_POS=Vehicle Loading Positions");
                w.WriteLine("LABEL_FORWARD_BACK=Forward/Back");
                w.WriteLine("LABEL_LEAN=Lean");
                w.WriteLine("LABEL_LEFT_RIGHT=Left/Right");
                w.WriteLine("LABEL_PITCH=Pitch");
                w.WriteLine("LABEL_ROLL=Roll");
                w.WriteLine("LABEL_ROTATE=Rotate");
                w.WriteLine("LABEL_SCALE_SIZE=Scale/Size");
                w.WriteLine("LABEL_TILT=Tilt");
                w.WriteLine("LABEL_UP_DOWN=Up/Down");
                w.WriteLine("LABEL_YAW=Yaw");
                w.WriteLine("LBL_SYSTEMIC=Systemic");
                w.WriteLine("MENU_AIRWAY_TITLE=Airway");
                w.WriteLine("MENU_AMBULANCE_SUBTITLE=Manage your ambulance");
                w.WriteLine("MENU_AMBULANCE_TITLE=Ambulance");
                w.WriteLine("MENU_CONFIG_SUBTITLE=Settings");
                w.WriteLine("MENU_CONFIG_TITLE=Configuration");
                w.WriteLine("MENU_DIAGNOSTICS_TITLE=Diagnostics");
                w.WriteLine("MENU_DOOR_SETUP_SUBTITLE=Configure door settings");
                w.WriteLine("MENU_DOOR_SETUP_TITLE=Door Setup");
                w.WriteLine("MENU_FORCE_CALLOUT_SUBTITLE=Manually start a callout");
                w.WriteLine("MENU_FORCE_CALLOUT_TITLE=Force Callout");
                w.WriteLine("MENU_GROUND_KITS_TITLE=Ground Kits");
                w.WriteLine("MENU_IM_TITLE=IM Meds");
                w.WriteLine("MENU_INTERACTION_POINTS_SUBTITLE=Configure interaction points");
                w.WriteLine("MENU_INTERACTION_POINTS_TITLE=Interaction Points");
                w.WriteLine("MENU_IV_TITLE=IV / Access");
                w.WriteLine("MENU_OFFSETS_ROOT_SUBTITLE=Configure various offsets");
                w.WriteLine("MENU_OFFSETS_ROOT_TITLE=Offsets");
                w.WriteLine("MENU_ORAL_TITLE=Oral Meds");
                w.WriteLine("MENU_PATIENT_DATA_COLORED=Patient Data");
                w.WriteLine("MENU_PATIENT_DATA_TITLE=Patient Data");
                w.WriteLine("MENU_PATIENT_POS_SUBTITLE=Configure patient positions");
                w.WriteLine("MENU_PATIENT_POS_TITLE=Patient Position");
                w.WriteLine("MENU_PATIENT_SUBTITLE=~b~Medical Interaction");
                w.WriteLine("MENU_PATIENT_TITLE=Patient");
                w.WriteLine("MENU_POINT_SUBTITLE=Edit the interaction point");
                w.WriteLine("MENU_POINT_TITLE=Point {0}");
                w.WriteLine("MENU_PROP_OFFSETS_SUBTITLE=Configure prop offsets");
                w.WriteLine("MENU_PROP_OFFSETS_TITLE=Prop Offsets");
                w.WriteLine("MENU_QUESTIONS_TITLE=QUESTIONS");
                w.WriteLine("MENU_STRETCHER_CARRY_SUBTITLE=Configure stretcher carry positions");
                w.WriteLine("MENU_STRETCHER_CARRY_TITLE=Stretcher Carry");
                w.WriteLine("MENU_TRAUMA_TITLE=Trauma");
                w.WriteLine("MENU_VEHICLE_CONFIG_SUBTITLE=Configure vehicle positions");
                w.WriteLine("MENU_VEHICLE_CONFIG_TITLE=Vehicle Config");
                w.WriteLine("MODE_LOWERED_STRETCHER=Lowered Stretcher");
                w.WriteLine("MODE_MEDIC_SEAT_POS=Medic Seat Position");
                w.WriteLine("MODE_NORMAL_STRETCHER=Normal Stretcher");
                w.WriteLine("MODE_SITTING_STRETCHER=Sitting Stretcher");
                w.WriteLine("MODE_SLIDE_POS=Slide Position");
                w.WriteLine("MODE_STOWED_POS=Stowed Position");
                w.WriteLine("NOTIF_ADMINISTERED=~g~Administered {0}.");
                w.WriteLine("NOTIF_AIRWAY_OPENED=~g~Airway manually opened.");
                w.WriteLine("NOTIF_BGL_HIGH=~r~BGL Result: HIGH.");
                w.WriteLine("NOTIF_BGL_LOW=~y~BGL Result: LOW.");
                w.WriteLine("NOTIF_BGL_NORMAL=~g~BGL Result: NORMAL.");
                w.WriteLine("NOTIF_BP_CONNECTED=~g~BP Cuff connected.");
                w.WriteLine("NOTIF_BP_REMOVED=~r~BP Cuff removed.");
                w.WriteLine("NOTIF_CALLOUT_ACCEPT_PROMPT=~g~Y~w~ to accept ~o~/ ~r~N~w~ to decline.");
                w.WriteLine("NOTIF_CANNOT_TREAT_DECEASED=~r~Cannot treat a deceased patient.");
                w.WriteLine("NOTIF_CONFIG_NOT_SAVED=Configuration not saved.");
                w.WriteLine("NOTIF_CONFIGSRELOADED=~b~EmsPlus~w~: All configurations reloaded!");
                w.WriteLine("NOTIF_FLUIDS_STOPPED=IV Fluids stopped.");
                w.WriteLine("NOTIF_HOSPITAL_WAYPOINT_SET=~b~Dispatch:~w~ Route to nearest hospital set.");
                w.WriteLine("NOTIF_IV_ESTABLISHED=~g~IV Established.");
                w.WriteLine("NOTIF_MEDICALBAGS_RESTOCKED=~b~Dispatch:~w~ Medical bags ~g~restocked~w~.");
                w.WriteLine("NOTIF_MONITOR_CONNECTED=~g~Monitor connected.");
                w.WriteLine("NOTIF_MONITOR_DISCONNECTED=~r~Monitor disconnected.");
                w.WriteLine("NOTIF_MUST_HOLD_STRETCHER=~r~You must be holding the stretcher.");
                w.WriteLine("NOTIF_NO_VEHICLE_NEARBY=No vehicle nearby.");
                w.WriteLine("NOTIF_ON_DUTY=~b~EmsPlus:~w~ You are now ~g~On Duty~w~.");
                w.WriteLine("NOTIF_PATIENT_FLATLINED=~b~Dispatch:~r~ Patient has flatlined. Time of death recorded.");
                w.WriteLine("NOTIF_PATIENT_HANDED_OVER=~g~Transport Complete:~w~ Patient handed over to hospital staff.");
                w.WriteLine("NOTIF_PATIENT_REGAINED_CONSCIOUSNESS=~g~Patient has regained consciousness.");
                w.WriteLine("NOTIF_PATIENT_UNABLE_TO_ANSWER=~r~Patient is unresponsive and cannot answer.");
                w.WriteLine("NOTIF_POSITION_COPIED=~g~Position copied to clipboard!");
                w.WriteLine("NOTIF_PULSE_RESTORED=~g~PULSE RESTORED!~w~ Keep monitoring.");
                w.WriteLine("NOTIF_SCENE_SECURED=~r~Scene Secured:~w~ Traffic stopped and pedestrians blocked.");
                w.WriteLine("NOTIF_SCENE_UNSECURED=~g~Scene Unsecured:~w~ Traffic and pedestrians returning to normal.");
                w.WriteLine("NOTIF_SETTINGS_SAVED_GENERIC=Settings saved successfully.");
                w.WriteLine("NOTIF_SETTINGS_SAVED_VEHICLE=Settings saved successfully for {0}.");
                w.WriteLine("NOTIF_STATUS_UPDATE=~b~Status Update:~w~ {0}");
                w.WriteLine("NOTIF_STRETCHER_NOT_IN_VEHICLE=~r~Stretcher is not in the vehicle and you are not holding it.");
                w.WriteLine("NOTIF_TRAUMA_SWEEP_COMPLETE=~g~Trauma sweep complete. Check Diagnostics for tips.");
                w.WriteLine("NOTIF_TREATING=Treating injury...");
                w.WriteLine("NOTIF_VEHICLE_ADDED=Added {0} to allowed vehicles.");
                w.WriteLine("NOTIF_VEHICLE_DETECTED=Vehicle detected: {0}");
                w.WriteLine("NOTIF_VEHICLE_REMOVED=Removed {0} from allowed vehicles.");
                w.WriteLine("OXYGENBAG_DESC=A bag containing oxygen supplies.");
                w.WriteLine("OXYGENBAG_NAME=Oxygen Bag");
                w.WriteLine("PROMPT_GRAB_STRETCHER=[G] Grab Stretcher");
                w.WriteLine("PROMPT_LOWER_STRETCHER=[H] Lower");
                w.WriteLine("PROMPT_RAISE_STRETCHER=[H] Raise");
                w.WriteLine("PROMPT_RELEASE_STRETCHER=[G] Release Stretcher");
                w.WriteLine("PROMPT_SITTING_STRETCHER=[J] Toggle Sitting");
                w.WriteLine("REQ_GENERIC=Requires {0}");
                w.WriteLine("REQ_GENERIC_COLORED=~r~Requires {0}");
                w.WriteLine("REQ_IV=Requires IV");
                w.WriteLine("REQ_OXYGEN_BAG=Requires Oxygen Bag");
                w.WriteLine("REQ_TRAUMA_BAG=Requires Trauma Bag");
                w.WriteLine("SUBTITLE_AIRWAY=~b~Airway & Breathing");
                w.WriteLine("SUBTITLE_DIAGNOSTICS=~b~Assess Patient");
                w.WriteLine("SUBTITLE_GROUND_KITS=~b~Interact with Medical Kits");
                w.WriteLine("SUBTITLE_IM=~b~Intramuscular Injections");
                w.WriteLine("SUBTITLE_INSPECTION=Select area to inspect");
                w.WriteLine("SUBTITLE_IV=~b~Fluids & IV Lines");
                w.WriteLine("SUBTITLE_ORAL=~b~Oral Medications");
                w.WriteLine("SUBTITLE_PATIENT_DATA=~b~Demographics & Info");
                w.WriteLine("SUBTITLE_QUESTIONS=~b~Patient Interview");
                w.WriteLine("SUBTITLE_TRANSPORT_PATIENT=~y~Transport patient to the nearest hospital.");
                w.WriteLine("SUBTITLE_TRAUMA=~b~Treat Injuries");
                w.WriteLine("TASK_BGL_STEP_COMPLETE=Test complete!");
                w.WriteLine("TASK_BGL_STEP_INSERT=Insert the strip into the glucometer.");
                w.WriteLine("TASK_BGL_STEP_PRICK=Prick the finger.");
                w.WriteLine("TASK_BGL_STEP_SAMPLE=Collect the blood sample.");
                w.WriteLine("TASK_BGL_TITLE=BLOOD GLUCOSE TEST");
                w.WriteLine("TASK_HELP_TEXT=Drag items to complete the procedure");
                w.WriteLine("TASK_IV_STEP_CATH=Insert the catheter.");
                w.WriteLine("TASK_IV_STEP_COMPLETE=IV placement complete.");
                w.WriteLine("TASK_IV_STEP_FIX=Secure the IV with tape.");
                w.WriteLine("TASK_IV_STEP_VEIN=Locate the vein.");
                w.WriteLine("TASK_IV_TITLE=IV PLACEMENT");
                w.WriteLine("TEXT_OFF_DUTY=~r~Off Duty");
                w.WriteLine("TEXT_ON_DUTY=~g~On Duty");
                w.WriteLine("TITLE_DIAGNOSTICS=DIAGNOSTICS");
                w.WriteLine("TITLE_INSPECTION=PATIENT INSPECTION");
                w.WriteLine("TITLE_PATIENT_DATA=PATIENT DATA");
                w.WriteLine("TRAUMABAG_DESC=A bag containing trauma supplies.");
                w.WriteLine("TRAUMABAG_NAME=Trauma Bag");
                w.WriteLine("TRT_BAGVALVEMASK=BVM (Bag Valve)");
                w.WriteLine("TRT_BANDAGE=Bandage");
                w.WriteLine("TRT_BURNDRESSING=Burn Dressing");
                w.WriteLine("TRT_CERVICALCOLLAR=Cervical Collar");
                w.WriteLine("TRT_CHESTSEAL=Chest Seal");
                w.WriteLine("TRT_CPR=CPR");
                w.WriteLine("TRT_CPR_DESC=Perform CPR");
                w.WriteLine("TRT_DIRECT_PRESSURE=Direct Pressure");
                w.WriteLine("TRT_EYEPATCH=Eye Patch");
                w.WriteLine("TRT_EYESHIELD=Eye Shield");
                w.WriteLine("TRT_HIGHFLOWOXYGEN=NRB Mask (High Flow)");
                w.WriteLine("TRT_ICEPACK=Ice Pack");
                w.WriteLine("TRT_IRRIGATION=Irrigation Fluid");
                w.WriteLine("TRT_IVACCESS=IV Start Kit");
                w.WriteLine("TRT_JUNCTIONALTOURNIQUET=Junctional Tourniquet");
                w.WriteLine("TRT_MANAGE_AIRWAY=Airway Management");
                w.WriteLine("TRT_NEEDLEDECOMP=Needle Decompression");
                w.WriteLine("TRT_OXYGEN=O2 Mask");
                w.WriteLine("TRT_PELVICBINDER=Pelvic Binder");
                w.WriteLine("TRT_SALINEBAG=Saline Bag");
                w.WriteLine("TRT_SPINALIMMOBILISATION=Backboard");
                w.WriteLine("TRT_SPLINT=SAM Splint");
                w.WriteLine("TRT_STABILISEOBJECT=Object Stabilization");
                w.WriteLine("TRT_SUTURE=Suture Kit");
                w.WriteLine("TRT_TOURNIQUET=Tourniquet");
                w.WriteLine("TRT_TRACTIONSPLINT=Traction Splint");
                w.WriteLine("TRT_WETDRESSING=Wet Dressing");
                w.WriteLine("TRT_WOUNDPACKING=Wound Packing");
                w.WriteLine("TUTORIAL_CABIN_1=The patient is now loaded. You can enter the patient cabin to continue treatment while transporting to the hospital.");
                w.WriteLine("TUTORIAL_CABIN_2=Press the quick-key ~y~{0}~w~ to enter or exit the cabin. You can also use the ambulance menu. Inside, you have access to all your medical supplies.");
                w.WriteLine("TUTORIAL_CABIN_3=A waypoint to the nearest hospital has been set. Transport the patient to complete the call. This concludes the tutorial!");
                w.WriteLine("TUTORIAL_CALLOUT_ACCEPTED=You've received a callout! Respond ~r~Code 3~w~ to the scene marked on your GPS.");
                w.WriteLine("TUTORIAL_INSPECT_1=This is the Patient Inspection Menu. Use your mouse to select different body parts to assess them.");
                w.WriteLine("TUTORIAL_INSPECT_2=Press ~y~TAB~w~ to cycle between the Diagnostics and Patient Data panels. You can also click the buttons at the top right.");
                w.WriteLine("TUTORIAL_INSPECT_3=When you find an injury, open a medical bag you placed on the ground, select the correct tool (like a bandage), then click on the injured body part to apply it.");
                w.WriteLine("TUTORIAL_ONSCENE_1=You've arrived on scene. Before approaching the patient, it's critical to get your equipment ready.");
                w.WriteLine("TUTORIAL_ONSCENE_2=Go to the rear of your ambulance. To unload the stretcher, you can use the quick-key ~y~{0}~w~, or open the ambulance menu with ~y~{1}~w~.");
                w.WriteLine("TUTORIAL_ONSCENE_3=From the ambulance menu, you can also equip your primary medical bags, like the Trauma Bag and Oxygen Bag.");
                w.WriteLine("TUTORIAL_ONSCENE_4=With your gear equipped, you are ready. Approach the patient and press ~y~{0}~w~ to begin the inspection.");
                w.WriteLine("TUTORIAL_SETTINGS_1=This is the EmsPlus Configuration Menu. From here, you can customize almost every part of the mod.");
                w.WriteLine("TUTORIAL_SETTINGS_2=The most important section is 'Offsets & Positions'. This lets you adjust how props are attached to your character and vehicles.");
                w.WriteLine("TUTORIAL_SETTINGS_3=Let's try configuring a vehicle. Find an ambulance you want to use, get near it, then open 'Vehicle Settings' from the previous menu.");
                w.WriteLine("TUTORIAL_SPEAKER=Tutorial");
                w.WriteLine("TUTORIAL_STARTUP_1=Welcome to EmsPlus! This brief tutorial will guide you through the main features. Press ~y~Y~w~ to advance dialogue boxes like this one.");
                w.WriteLine("TUTORIAL_STARTUP_2=Your first step is to go on duty. To do so find a ~r~Fire Station~w~ on your map and press ~y~E~w~ inside the red marker.");
                w.WriteLine("TUTORIAL_STARTUP_3=Once on duty, you can open the main Settings Menu at any time by pressing ~y~{0}~w~. You can also disable this tutorial there. For now, let's wait for a callout.");
                w.WriteLine("TUTORIAL_VEHICLE_1=This is the Vehicle Configuration menu. First, click 'Reload/Detect Vehicle' to make sure the menu is editing the correct ambulance.");
                w.WriteLine("TUTORIAL_VEHICLE_2=Check 'Add to Allowed Vehicles'. This tells EmsPlus you want to use this model and enables saving its settings.");
                w.WriteLine("TUTORIAL_VEHICLE_3=Use the 'Editing Mode' list to select what you want to adjust. 'Stowed Position' is where the stretcher sits inside the ambulance.");
                w.WriteLine("TUTORIAL_VEHICLE_4='Slide Position' is where it ends up after you unload it. Adjust the X, Y, Z, and rotation values until the ghost stretcher looks correct, then click 'Save All Settings' in the main menu.");
                w.WriteLine("TUTORIAL_VEHICLE_5=You can also configure which doors open and set up custom interaction points for the ambulance menu. When you're done, go back on duty to get a call.");
            }
        }
    }
}