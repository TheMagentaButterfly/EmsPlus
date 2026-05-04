using Rage;
using System.IO;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Globalization;

namespace EmsPlus.Configuration
{
    public class VehicleConfig
    {
        public string ModelName { get; private set; }
        private string FilePath;

        public bool CanHaveStretcher { get; set; } = true;
        public bool HideStretcherInVehicle { get; set; } = false;
        public bool CanEnterCabin { get; set; } = true;

        public Vector3 StowPos { get; set; } = new Vector3(0f, -1.5f, 0.5f);
        public Rotator StowRot { get; set; } = new Rotator(0f, 0f, 0f);

        public Vector3 SlidePos { get; set; } = new Vector3(0f, -3.5f, 0.5f);
        public Rotator SlideRot { get; set; } = new Rotator(0f, 0f, 0f);

        public Vector3 MedicPos { get; set; } = new Vector3(0.5f, -1.0f, 0.5f);
        public Rotator MedicRot { get; set; } = new Rotator(0f, 0f, 90f);

        public List<int> DoorIndices { get; set; } = new List<int>() { 2, 3 };

        public List<AmbulanceInteractionPoint> InteractionPoints { get; set; } = new List<AmbulanceInteractionPoint>();

        public class AmbulanceInteractionPoint
        {
            public Vector3 Offset { get; set; }
            public float Scale { get; set; }

            public AmbulanceInteractionPoint(Vector3 offset, float scale = 1.0f)
            {
                Offset = offset;
                Scale = scale;
            }
        }

        public VehicleConfig(string modelName)
        {
            ModelName = modelName.ToLower();

            string dir = Path.Combine(Application.StartupPath, "Plugins", "EmsPlus", "Settings", "Vehicles");
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

            FilePath = Path.Combine(dir, $"{ModelName}.ini");
        }

        public void Load()
        {
            if (!File.Exists(FilePath))
            {
                ApplySaneDefaults();
                Save();
                return;
            }

            InitializationFile ini = new InitializationFile(FilePath);

            float sx = ReadFloat(ini, "Stowed", "X", 0f);
            float sy = ReadFloat(ini, "Stowed", "Y", -2.25f);
            float sz = ReadFloat(ini, "Stowed", "Z", 0.45f);
            StowPos = new Vector3(sx, sy, sz);

            float sRx = ReadFloat(ini, "Stowed", "Pitch", 0f);
            float sRy = ReadFloat(ini, "Stowed", "Roll", 0f);
            float sRz = ReadFloat(ini, "Stowed", "Yaw", 0f);
            StowRot = new Rotator(sRx, sRy, sRz);

            float lx = ReadFloat(ini, "Slide", "X", 0f);
            float ly = ReadFloat(ini, "Slide", "Y", -4.5f);
            float lz = ReadFloat(ini, "Slide", "Z", 0.45f);
            SlidePos = new Vector3(lx, ly, lz);

            float lRx = ReadFloat(ini, "Slide", "Pitch", 0f);
            float lRy = ReadFloat(ini, "Slide", "Roll", 0f);
            float lRz = ReadFloat(ini, "Slide", "Yaw", 0f);
            SlideRot = new Rotator(lRx, lRy, lRz);

            float mx = ReadFloat(ini, "MedicSeat", "X", 0.5f);
            float my = ReadFloat(ini, "MedicSeat", "Y", -1.0f);
            float mz = ReadFloat(ini, "MedicSeat", "Z", 0.5f);
            MedicPos = new Vector3(mx, my, mz);

            float mRx = ReadFloat(ini, "MedicSeat", "Pitch", 0f);
            float mRy = ReadFloat(ini, "MedicSeat", "Roll", 0f);
            float mRz = ReadFloat(ini, "MedicSeat", "Yaw", 90f);
            MedicRot = new Rotator(mRx, mRy, mRz);

            CanHaveStretcher = ini.ReadBoolean("Settings", "CanHaveStretcher", true);
            HideStretcherInVehicle = ini.ReadBoolean("Settings", "HideStretcherInVehicle", false);
            CanEnterCabin = ini.ReadBoolean("Settings", "CanEnterCabin", true);
            string doorString = ini.ReadString("Settings", "DoorIds", "2,3");
            DoorIndices.Clear();
            if (!string.IsNullOrWhiteSpace(doorString))
            {
                foreach (var s in doorString.Split(','))
                {
                    if (int.TryParse(s, out int id)) DoorIndices.Add(id);
                }
            }

            InteractionPoints.Clear();
            int i = 0;
            while (true)
            {
                string pointStr = ini.ReadString("InteractionPoints", $"Point_{i}", null);
                if (string.IsNullOrEmpty(pointStr)) break;

                string[] parts = pointStr.Split(',');
                if (parts.Length >= 3 &&
                    float.TryParse(parts[0], NumberStyles.Any, CultureInfo.InvariantCulture, out float x) &&
                    float.TryParse(parts[1], NumberStyles.Any, CultureInfo.InvariantCulture, out float y) &&
                    float.TryParse(parts[2], NumberStyles.Any, CultureInfo.InvariantCulture, out float z))
                {
                    float scale = 1.0f;
                    if (parts.Length == 4) float.TryParse(parts[3], NumberStyles.Any, CultureInfo.InvariantCulture, out scale);

                    InteractionPoints.Add(new AmbulanceInteractionPoint(new Vector3(x, y, z), scale));
                }
                i++;
            }
            if (InteractionPoints.Count == 0) InteractionPoints.Add(new AmbulanceInteractionPoint(new Vector3(0f, -3.5f, 0f), 1.0f));
        }

        public void Save()
        {
            InitializationFile ini = new InitializationFile(FilePath);

            ini.Write("Settings", "CanHaveStretcher", CanHaveStretcher);
            ini.Write("Settings", "HideStretcherInVehicle", HideStretcherInVehicle);
            ini.Write("Settings", "CanEnterCabin", CanEnterCabin);
            string doorString = string.Join(",", DoorIndices);
            ini.Write("Settings", "DoorIds", doorString);

            ini.Write("InteractionPoints", null, (string)null);

            ini.Write("Stowed", "X", StowPos.X);
            ini.Write("Stowed", "Y", StowPos.Y);
            ini.Write("Stowed", "Z", StowPos.Z);
            ini.Write("Stowed", "Pitch", StowRot.Pitch);
            ini.Write("Stowed", "Roll", StowRot.Roll);
            ini.Write("Stowed", "Yaw", StowRot.Yaw);

            ini.Write("Slide", "X", SlidePos.X);
            ini.Write("Slide", "Y", SlidePos.Y);
            ini.Write("Slide", "Z", SlidePos.Z);
            ini.Write("Slide", "Pitch", SlideRot.Pitch);
            ini.Write("Slide", "Roll", SlideRot.Roll);
            ini.Write("Slide", "Yaw", SlideRot.Yaw);

            ini.Write("MedicSeat", "X", MedicPos.X);
            ini.Write("MedicSeat", "Y", MedicPos.Y);
            ini.Write("MedicSeat", "Z", MedicPos.Z);
            ini.Write("MedicSeat", "Pitch", MedicRot.Pitch);
            ini.Write("MedicSeat", "Roll", MedicRot.Roll);
            ini.Write("MedicSeat", "Yaw", MedicRot.Yaw);

            for (int i = 0; i < InteractionPoints.Count; i++)
            {
                var point = InteractionPoints[i];
                string pointStr = string.Format(CultureInfo.InvariantCulture, "{0},{1},{2},{3}",
                    point.Offset.X, point.Offset.Y, point.Offset.Z, point.Scale);
                ini.Write("InteractionPoints", $"Point_{i}", pointStr);
            }
        }

        private void ApplySaneDefaults()
        {
            if (ModelName == "ambulance")
            {
                StowPos = new Vector3(0f, -2.25f, 0.45f);
                StowRot = new Rotator(0f, 0f, 0f);
                SlidePos = new Vector3(0f, -4.5f, 0.45f);
                SlideRot = new Rotator(0f, 0f, 0f);
                MedicPos = new Vector3(0.5f, -0.2f, 0.45f);
                MedicRot = new Rotator(0f, 0f, 90f);
            }
            
            DoorIndices = new List<int>() { 2, 3 };
            InteractionPoints = new List<AmbulanceInteractionPoint>() { new AmbulanceInteractionPoint(new Vector3(0f, -3.5f, 0f), 1.0f) };
            CanHaveStretcher = true;
            HideStretcherInVehicle = false;
            CanEnterCabin = true;
        }

        private float ReadFloat(InitializationFile ini, string section, string key, float defaultVal)
        {
            try { return (float)ini.ReadDouble(section, key, defaultVal); }
            catch { return defaultVal; }
        }
    }
}