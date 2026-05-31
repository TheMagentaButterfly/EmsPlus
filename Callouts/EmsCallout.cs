using EmsPlus.Managers;
using Rage;
using Rage.Native;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace EmsPlus.Callouts
{
    public abstract class EmsCallout
    {
        public Vector3 CalloutPosition { get; set; }
        public string CalloutMessage { get; set; }
        public string CalloutName { get; set; }
        public Blip AreaBlip { get; private set; }
        public bool Accepted { get; private set; } = false;
        public bool PlayerArrived { get; private set; } = false;
        public bool Finished { get; private set; } = false;

        /// <summary>
        /// List of Station IDs where this callout can spawn. 
        /// If empty, it can spawn at any station.
        /// </summary>
        public List<string> AllowedStationIDs { get; set; } = new List<string>();
        protected List<Entity> SceneEntities = new List<Entity>();

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


        public virtual void OnPlayerArrivedOnScene()
        {
            Managers.TutorialManager.TriggerOnSceneTutorial();
        }

        public virtual void Process()
        {
            if (!PlayerArrived && Game.LocalPlayer.Character.DistanceTo(CalloutPosition) < 40f)
            {
                PlayerArrived = true;
                OnPlayerArrivedOnScene();
            }
        }

        public virtual void End()
        {
            Finished = true;
            if (AreaBlip != null && AreaBlip.Exists()) AreaBlip.Delete();
            foreach (var ent in SceneEntities)
            {
                if (ent != null && ent.Exists()) ent.Dismiss();
            }
            SceneEntities.Clear();
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
        /// A robust fallback method to find a safe pedestrian spawn point (sidewalk).
        /// </summary>
        protected Vector3 GetSidewalkPosition(Vector3 roadPosition)
        {
            if (NativeFunction.Natives.GET_SAFE_COORD_FOR_PED<bool>(roadPosition.X, roadPosition.Y, roadPosition.Z, true, out Vector3 safePos, 16))
            {
                if (safePos != Vector3.Zero && Math.Abs(safePos.Z - roadPosition.Z) < 5.0f)
                {
                    return safePos;
                }
            }

            Vector3 streetPos = World.GetNextPositionOnStreet(roadPosition);
            if (streetPos == Vector3.Zero) streetPos = roadPosition;

            NativeFunction.Natives.GET_CLOSEST_VEHICLE_NODE_WITH_HEADING<bool>(
                streetPos.X, streetPos.Y, streetPos.Z,
                out Vector3 roadCenter, out float roadHeading, 1, 3.0f, 0);

            if (roadCenter == Vector3.Zero) roadCenter = streetPos;

            float angleRad = (roadHeading - 90f) * (float)(Math.PI / 180.0f);
            Vector3 offsetDir = new Vector3((float)Math.Cos(angleRad), (float)Math.Sin(angleRad), 0f);
            Vector3 targetSide = roadCenter + (offsetDir * 4.5f);

            float? groundZ = World.GetGroundZ(targetSide, true, true);
            if (groundZ.HasValue && Math.Abs(groundZ.Value - roadCenter.Z) < 5.0f)
            {
                return new Vector3(targetSide.X, targetSide.Y, groundZ.Value);
            }

            return roadCenter;
        }

        protected void SpawnEmergencyUnit(string vehModelStr, string pedModelStr, Vector3 centerPos, bool sirensOn = true)
        {
            Vector3 spawnPos = World.GetNextPositionOnStreet(centerPos.Around(10f, 30f));
            if (spawnPos == Vector3.Zero) spawnPos = centerPos.Around(15f);

            Model vehModel = new Model(vehModelStr);
            if (!vehModel.IsValid)
            {
                Game.Console.Print($"[EmsPlus] Error: Invalid vehicle model '{vehModelStr}' requested.");
                return;
            }

            vehModel.LoadAndWait();
            Vehicle veh = new Vehicle(vehModel, spawnPos);
            vehModel.Dismiss();

            if (veh.Exists())
            {
                veh.IsPersistent = true;
                if (sirensOn) veh.IsSirenOn = true;
                SceneEntities.Add(veh);

                Model pedModel = new Model(pedModelStr);
                if (!pedModel.IsValid)
                {
                    Game.Console.Print($"[EmsPlus] Error: Invalid ped model '{pedModelStr}' requested.");
                    return;
                }

                pedModel.LoadAndWait();
                Ped responder = new Ped(pedModel, veh.GetOffsetPosition(new Vector3(2.5f, 0, 0)), veh.Heading);
                pedModel.Dismiss();

                if (responder.Exists())
                {
                    responder.IsPersistent = true;
                    responder.BlockPermanentEvents = true;
                    SceneEntities.Add(responder);
                    NativeFunction.Natives.TASK_TURN_PED_TO_FACE_COORD(responder, centerPos.X, centerPos.Y, centerPos.Z, -1);
                }
            }
        }

        protected Ped SpawnBystander(string pedModelStr, Vector3 centerPos)
        {
            Model pedModel = new Model(pedModelStr);
            if (!pedModel.IsValid)
            {
                Game.Console.Print($"[EmsPlus] Error: Invalid bystander ped model '{pedModelStr}' requested.");
                return null;
            }

            pedModel.LoadAndWait();
            Vector3 spawnPos = GetSidewalkPosition(centerPos.Around(2f, 6f));
            Ped ped = new Ped(pedModel, spawnPos, 0f);
            pedModel.Dismiss();

            if (ped.Exists())
            {
                ped.IsPersistent = true;
                ped.BlockPermanentEvents = true;
                NativeFunction.Natives.TASK_TURN_PED_TO_FACE_COORD(ped, centerPos.X, centerPos.Y, centerPos.Z, -1);
                SceneEntities.Add(ped);
                return ped;
            }
            return null;
        }
    }
}