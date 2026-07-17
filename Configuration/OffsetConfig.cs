using Rage;
using System.Globalization;
using System.IO;

namespace EmsPlus.Configuration
{
    public class OffsetConfig : IPT.Common.User.Configuration
    {
        private const string IniFilePath = "Plugins/EmsPlus/Settings/Offsets.ini";

        // --- STRETCHER OFFSETS ---
        // Stretcher Offsets
        public float StretcherAttachOffsetX = 0.1f;
        public float StretcherAttachOffsetY = 0.45f;
        public float StretcherAttachOffsetZ = 0.0f;
        public float StretcherAttachPitch = 90.0f;
        public float StretcherAttachRoll = 0.0f;
        public float StretcherAttachYaw = 90.0f;

        // Patient Offsets on Raised Stretcher
        public float PatientAttachOffsetX = 0.0f;
        public float PatientAttachOffsetY = -0.1f;
        public float PatientAttachOffsetZ = 1.0f;
        public float PatientAttachPitch = 0.0f;
        public float PatientAttachRoll = 0.0f;
        public float PatientAttachYaw = 180.0f;

        // Patient Offsets on Lowered Stretcher
        public float PatientAttachLoweredOffsetX = 0.0f;
        public float PatientAttachLoweredOffsetY = -0.1f;
        public float PatientAttachLoweredOffsetZ = 1.0f;
        public float PatientAttachLoweredPitch = 0.0f;
        public float PatientAttachLoweredRoll = 0.0f;
        public float PatientAttachLoweredYaw = 180.0f;

        // Patient Offsets on Sitting Stretcher
        public float PatientAttachSittingOffsetX = 0.0f;
        public float PatientAttachSittingOffsetY = -0.2f;
        public float PatientAttachSittingOffsetZ = 0.6f;
        public float PatientAttachSittingPitch = 20.0f;
        public float PatientAttachSittingRoll = 0.0f;
        public float PatientAttachSittingYaw = 180.0f;

        // --- PROP CARRY OFFSETS ---
        // 1. Trauma Bag
        public string TraumaAttachBone = "RightHand";
        public float TraumaAttachX = 0.44f;
        public float TraumaAttachY = 0.01f;
        public float TraumaAttachZ = 0.0f;
        public float TraumaAttachPitch = -45.0f;
        public float TraumaAttachRoll = 270.0f;
        public float TraumaAttachYaw = 0.0f;

        // 2. Oxygen Bag
        public string OxygenAttachBone = "Back";
        public float OxygenAttachX = 0.44f;
        public float OxygenAttachY = 0.01f;
        public float OxygenAttachZ = 0.0f;
        public float OxygenAttachPitch = -45.0f;
        public float OxygenAttachRoll = 270.0f;
        public float OxygenAttachYaw = 0.0f;

        // 3. Defibrilator Kit
        public string DefibAttachBone = "LeftHand";
        public float DefibAttachX = 0.44f;
        public float DefibAttachY = 0.01f;
        public float DefibAttachZ = 0.0f;
        public float DefibAttachPitch = -45.0f;
        public float DefibAttachRoll = 270.0f;
        public float DefibAttachYaw = 0.0f;

        // 4. Lucas Kit
        public string LucasAttachBone = "LeftHand";
        public float LucasAttachX = 0.44f;
        public float LucasAttachY = 0.01f;
        public float LucasAttachZ = 0.0f;
        public float LucasAttachPitch = -45.0f;
        public float LucasAttachRoll = 270.0f;
        public float LucasAttachYaw = 0.0f;

        // --- MDT OFFSETS ---
        public float MdtScale = 1.0f;
        public float MdtOffsetX = 0.0f;
        public float MdtOffsetY = 0.0f;

        public override void Load()
        {
            if (!File.Exists(IniFilePath)) Save();

            InitializationFile ini = new InitializationFile(IniFilePath);

            StretcherAttachOffsetX = ReadFloat(ini, "StretcherAttachment", "X", 0);
            StretcherAttachOffsetY = ReadFloat(ini, "StretcherAttachment", "Y", 0);
            StretcherAttachOffsetZ = ReadFloat(ini, "StretcherAttachment", "Z", 0);
            StretcherAttachPitch = ReadFloat(ini, "StretcherAttachment", "Pitch", 0);
            StretcherAttachRoll = ReadFloat(ini, "StretcherAttachment", "Roll", 0);
            StretcherAttachYaw = ReadFloat(ini, "StretcherAttachment", "Yaw", 0);

            PatientAttachOffsetX = ReadFloat(ini, "PatientAttachment", "X", 0);
            PatientAttachOffsetY = ReadFloat(ini, "PatientAttachment", "Y", 0);
            PatientAttachOffsetZ = ReadFloat(ini, "PatientAttachment", "Z", 0);
            PatientAttachPitch = ReadFloat(ini, "PatientAttachment", "Pitch", 0);
            PatientAttachRoll = ReadFloat(ini, "PatientAttachment", "Roll", 0);
            PatientAttachYaw = ReadFloat(ini, "PatientAttachment", "Yaw", 0);

            PatientAttachLoweredOffsetX = ReadFloat(ini, "PatientAttachmentLowered", "X", 0);
            PatientAttachLoweredOffsetY = ReadFloat(ini, "PatientAttachmentLowered", "Y", 0);
            PatientAttachLoweredOffsetZ = ReadFloat(ini, "PatientAttachmentLowered", "Z", 0);
            PatientAttachLoweredPitch = ReadFloat(ini, "PatientAttachmentLowered", "Pitch", 0);
            PatientAttachLoweredRoll = ReadFloat(ini, "PatientAttachmentLowered", "Roll", 0);
            PatientAttachLoweredYaw = ReadFloat(ini, "PatientAttachmentLowered", "Yaw", 0);

            PatientAttachSittingOffsetX = ReadFloat(ini, "PatientAttachmentSitting", "X", 0);
            PatientAttachSittingOffsetY = ReadFloat(ini, "PatientAttachmentSitting", "Y", 0);
            PatientAttachSittingOffsetZ = ReadFloat(ini, "PatientAttachmentSitting", "Z", 0);
            PatientAttachSittingPitch = ReadFloat(ini, "PatientAttachmentSitting", "Pitch", 0);
            PatientAttachSittingRoll = ReadFloat(ini, "PatientAttachmentSitting", "Roll", 0);
            PatientAttachSittingYaw = ReadFloat(ini, "PatientAttachmentSitting", "Yaw", 0);


            TraumaAttachBone = ini.ReadString("TraumaBag", "Bone", "RightHand");
            TraumaAttachX = ReadFloat(ini, "TraumaBag", "X", 0.44f);
            TraumaAttachY = ReadFloat(ini, "TraumaBag", "Y", 0.01f);
            TraumaAttachZ = ReadFloat(ini, "TraumaBag", "Z", 0.0f);
            TraumaAttachPitch = ReadFloat(ini, "TraumaBag", "Pitch", -45.0f);
            TraumaAttachRoll = ReadFloat(ini, "TraumaBag", "Roll", 270.0f);
            TraumaAttachYaw = ReadFloat(ini, "TraumaBag", "Yaw", 0.0f);

            OxygenAttachBone = ini.ReadString("OxygenBag", "Bone", "Back");
            OxygenAttachX = ReadFloat(ini, "OxygenBag", "X", 0.44f);
            OxygenAttachY = ReadFloat(ini, "OxygenBag", "Y", 0.01f);
            OxygenAttachZ = ReadFloat(ini, "OxygenBag", "Z", 0.0f);
            OxygenAttachPitch = ReadFloat(ini, "OxygenBag", "Pitch", -45.0f);
            OxygenAttachRoll = ReadFloat(ini, "OxygenBag", "Roll", 270.0f);
            OxygenAttachYaw = ReadFloat(ini, "OxygenBag", "Yaw", 0.0f);

            DefibAttachBone = ini.ReadString("DefibKit", "Bone", "LeftHand");
            DefibAttachX = ReadFloat(ini, "DefibKit", "X", 0.44f);
            DefibAttachY = ReadFloat(ini, "DefibKit", "Y", 0.01f);
            DefibAttachZ = ReadFloat(ini, "DefibKit", "Z", 0.0f);
            DefibAttachPitch = ReadFloat(ini, "DefibKit", "Pitch", -45.0f);
            DefibAttachRoll = ReadFloat(ini, "DefibKit", "Roll", 270.0f);
            DefibAttachYaw = ReadFloat(ini, "DefibKit", "Yaw", 0.0f);


            MdtScale = ReadFloat(ini, "MDT", "Scale", 1.0f);
            MdtOffsetX = ReadFloat(ini, "MDT", "OffsetX", 0.0f);
            MdtOffsetY = ReadFloat(ini, "MDT", "OffsetY", 0.0f);
        }

        public void Save()
        {
            InitializationFile ini = new InitializationFile(IniFilePath);

            ini.Write("StretcherAttachment", "X", StretcherAttachOffsetX);
            ini.Write("StretcherAttachment", "Y", StretcherAttachOffsetY);
            ini.Write("StretcherAttachment", "Z", StretcherAttachOffsetZ);
            ini.Write("StretcherAttachment", "Pitch", StretcherAttachPitch);
            ini.Write("StretcherAttachment", "Roll", StretcherAttachRoll);
            ini.Write("StretcherAttachment", "Yaw", StretcherAttachYaw);

            ini.Write("PatientAttachment", "X", PatientAttachOffsetX);
            ini.Write("PatientAttachment", "Y", PatientAttachOffsetY);
            ini.Write("PatientAttachment", "Z", PatientAttachOffsetZ);
            ini.Write("PatientAttachment", "Pitch", PatientAttachPitch);
            ini.Write("PatientAttachment", "Roll", PatientAttachRoll);
            ini.Write("PatientAttachment", "Yaw", PatientAttachYaw);

            ini.Write("PatientAttachmentLowered", "X", PatientAttachLoweredOffsetX);
            ini.Write("PatientAttachmentLowered", "Y", PatientAttachLoweredOffsetY);
            ini.Write("PatientAttachmentLowered", "Z", PatientAttachLoweredOffsetZ);
            ini.Write("PatientAttachmentLowered", "Pitch", PatientAttachLoweredPitch);
            ini.Write("PatientAttachmentLowered", "Roll", PatientAttachLoweredRoll);
            ini.Write("PatientAttachmentLowered", "Yaw", PatientAttachLoweredYaw);

            ini.Write("PatientAttachmentSitting", "X", PatientAttachSittingOffsetX);
            ini.Write("PatientAttachmentSitting", "Y", PatientAttachSittingOffsetY);
            ini.Write("PatientAttachmentSitting", "Z", PatientAttachSittingOffsetZ);
            ini.Write("PatientAttachmentSitting", "Pitch", PatientAttachSittingPitch);
            ini.Write("PatientAttachmentSitting", "Roll", PatientAttachSittingRoll);
            ini.Write("PatientAttachmentSitting", "Yaw", PatientAttachSittingYaw);


            ini.Write("TraumaBag", "Bone", TraumaAttachBone);
            ini.Write("TraumaBag", "X", TraumaAttachX);
            ini.Write("TraumaBag", "Y", TraumaAttachY);
            ini.Write("TraumaBag", "Z", TraumaAttachZ);
            ini.Write("TraumaBag", "Pitch", TraumaAttachPitch);
            ini.Write("TraumaBag", "Roll", TraumaAttachRoll);
            ini.Write("TraumaBag", "Yaw", TraumaAttachYaw);

            ini.Write("OxygenBag", "Bone", OxygenAttachBone);
            ini.Write("OxygenBag", "X", OxygenAttachX);
            ini.Write("OxygenBag", "Y", OxygenAttachY);
            ini.Write("OxygenBag", "Z", OxygenAttachZ);
            ini.Write("OxygenBag", "Pitch", OxygenAttachPitch);
            ini.Write("OxygenBag", "Roll", OxygenAttachRoll);
            ini.Write("OxygenBag", "Yaw", OxygenAttachYaw);

            ini.Write("DefibKit", "Bone", DefibAttachBone);
            ini.Write("DefibKit", "X", DefibAttachX);
            ini.Write("DefibKit", "Y", DefibAttachY);
            ini.Write("DefibKit", "Z", DefibAttachZ);
            ini.Write("DefibKit", "Pitch", DefibAttachPitch);
            ini.Write("DefibKit", "Roll", DefibAttachRoll);
            ini.Write("DefibKit", "Yaw", DefibAttachYaw);

            ini.Write("LucasKit", "X", LucasAttachX);
            ini.Write("LucasKit", "Y", LucasAttachY);
            ini.Write("LucasKit", "Z", LucasAttachZ);
            ini.Write("LucasKit", "Pitch", LucasAttachPitch);
            ini.Write("LucasKit", "Roll", LucasAttachRoll);
            ini.Write("LucasKit", "Yaw", LucasAttachYaw);

            ini.Write("MDT", "Scale", MdtScale);
            ini.Write("MDT", "OffsetX", MdtOffsetX);
            ini.Write("MDT", "OffsetY", MdtOffsetY);
        }

        private float ReadFloat(InitializationFile ini, string section, string key, float defaultVal)
        {
            try
            {
                string raw = ini.ReadString(section, key, defaultVal.ToString(CultureInfo.InvariantCulture));

                raw = raw.Replace(',', '.');

                if (float.TryParse(raw, NumberStyles.Any, CultureInfo.InvariantCulture, out float result))
                {
                    return result;
                }
                return defaultVal;
            }
            catch { return defaultVal; }
        }
    }
}