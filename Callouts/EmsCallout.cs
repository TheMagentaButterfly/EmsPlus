using Rage;
using Rage.Native;
using System.Collections.Generic;
using System.Drawing;
using System;

namespace EmsPlus.Framework
{
    public abstract class EmsCallout
    {
        public Vector3 CalloutPosition { get; set; }
        public string CalloutMessage { get; set; }
        public string CalloutName { get; set; }
        public Blip AreaBlip { get; private set; }

        public bool Accepted { get; private set; } = false;
        public bool Finished { get; private set; } = false;

        /// <summary>
        /// List of Station IDs where this callout can spawn. 
        /// If empty, it can spawn at any station.
        /// </summary>
        public List<string> AllowedStationIDs { get; set; } = new List<string>();

        public virtual bool OnBeforeCalloutDisplayed()
        {
            return true;
        }

        public virtual bool OnCalloutAccepted()
        {
            Accepted = true;
            if (AreaBlip != null && AreaBlip.Exists()) AreaBlip.Delete();
            return true;
        }

        public virtual void Process() { }

        public virtual void End()
        {
            Finished = true;
            if (AreaBlip != null && AreaBlip.Exists()) AreaBlip.Delete();
        }

        protected void ShowCalloutAreaBlipBeforeAccepting(Vector3 position, float radius)
        {
            try
            {
                if (AreaBlip != null && AreaBlip.Exists()) AreaBlip.Delete();

                AreaBlip = new Blip(position, radius);

                if (AreaBlip != null && AreaBlip.Exists())
                {
                    AreaBlip.Color = Color.FromArgb(128, 255, 0, 0);
                    AreaBlip.Alpha = 0.5f;
                }
            }
            catch (Exception ex)
            {
                Game.Console.Print($"[EmsPlus] Warning: Radius Area Blip failed ({ex.Message}). Using fallback blip.");

                try
                {
                    AreaBlip = new Blip(position);
                    if (AreaBlip != null && AreaBlip.Exists())
                    {
                        AreaBlip.Color = Color.Yellow;
                    }
                }
                catch { }
            }
        }

        /// <summary>
        /// A highly robust method to find a safe pedestrian spawn point (sidewalk).
        /// Uses GTA V natives and mathematical offsets compatible with RagePluginHook.
        /// </summary>
        protected Vector3 GetSidewalkPosition(Vector3 roadPosition)
        {
            Vector3 resultPos = Vector3.Zero;
            float roadHeading = 0f;
            Vector3 roadCenter = roadPosition;

            if (NativeFunction.Natives.GET_CLOSEST_VEHICLE_NODE_WITH_HEADING<bool>(
                roadPosition.X, roadPosition.Y, roadPosition.Z,
                out roadCenter, out roadHeading, 1, 3.0f, 0))
            {
                roadPosition = roadCenter;
            }

            bool foundSafe = NativeFunction.Natives.GET_SAFE_COORD_FOR_PED<bool>(
                roadPosition.X, roadPosition.Y, roadPosition.Z,
                true,
                out Vector3 safePos,
                16);

            if (foundSafe && safePos != Vector3.Zero)
            {
                float heightDiff = Math.Abs(safePos.Z - roadPosition.Z);
                if (heightDiff < 2.5f)
                {
                    return safePos;
                }
                else
                {
                    Game.Console.Print($"[EmsPlus] Rejected Ped spawn: Too much elevation difference ({heightDiff}m). Likely subway.");
                }
            }

            float angleRad = (roadHeading - 90f) * (float)(Math.PI / 180.0f);
            Vector3 offsetDir = new Vector3((float)Math.Cos(angleRad), (float)Math.Sin(angleRad), 0f);

            Vector3 targetSide = roadPosition + (offsetDir * 4.5f);

            float? groundZ = World.GetGroundZ(targetSide, true, true);

            if (groundZ.HasValue)
            {
                if (Math.Abs(groundZ.Value - roadPosition.Z) < 3.0f)
                {
                    return new Vector3(targetSide.X, targetSide.Y, groundZ.Value);
                }
            }

            return roadPosition + (offsetDir * 3.5f);
        }
    }
}