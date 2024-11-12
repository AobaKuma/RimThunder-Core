using Vehicles;
using Verse;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using SmashTools;
using RimWorld;

namespace Motorization
{
    public class CompProperties_VehicleCargo : CompProperties
    {
        public bool renderVehicle = true;
        public int lengthLimit = 5;
        public VehicleDrawData drawData = null;

        public bool renderOpposite = false;
        public Vector3 GetOffsetFor(Rot8 rot)
        {
            if (drawData == null) return Vector3.zero;
            switch (rot.AsInt)
            {
                case 0:
                    return drawData.dataNorth;
                case 1:
                    return drawData.dataEast;
                case 2:
                    return drawData.dataSouth;
                case 3:
                    return drawData.dataWest;
                case 4://NorthEastInt
                    return drawData.dataNorthEast;
                case 5://SouthEastInt
                    return drawData.dataSouthEast;
                case 6://SouthWestInt
                    return drawData.dataSouthWest;
                case 7://NorthWestInt
                    return drawData.dataNorthWest;
            }
            return Vector3.zero;
        }
        public CompProperties_VehicleCargo()
        {
            compClass = typeof(CompVehicleCargo);
        }
    }
    public class CompVehicleCargo : VehicleComp
    {
        public override void Initialize(CompProperties props)
        {
            base.Initialize(props);
        }
        public ThingOwner<Thing> Cargo => Vehicle.inventory.innerContainer;
        public CompProperties_VehicleCargo Props => base.props as CompProperties_VehicleCargo;
        public bool TryUnloadThing(VehiclePawn thing)
        {
            if (Cargo.Contains(thing))
            {
                Cargo.Remove(thing);
                GenDrop.TryDropSpawn(thing, this.Vehicle.FullRotation.FacingCell * (int)((parent.def.Size.z + thing.def.Size.z) / 2), parent.Map, ThingPlaceMode.Near, out var _);
                thing.FullRotation = Props.renderOpposite ? Vehicle.FullRotation.Opposite : Vehicle.FullRotation;
                return true;
            }
            return false;
        }
        public bool TryAcceptThing(VehiclePawn thing)
        {
            if (!this.Accepts(thing))
            {
                return false;
            }
            thing.DeSpawn();
            Vehicle.AddOrTransfer(thing);
            return true;
        }
        public float MassCapacity => Vehicle.GetStatValue(VehicleStatDefOf.CargoCapacity);
        public bool Accepts(Thing thing)
        {
            if (thing == null) return false;
            if(thing.def.Size.z> Props.lengthLimit) return false;
            return true;//這邊之後判斷需要額外寫重量那些
        }
        public override void PostDraw()
        {
            if (Vehicle.Spawned && Props.renderVehicle)
            {
                if (DebugSettings.godMode) DebugDraw(Vehicle.DrawPos + GetDrawOffset(Vehicle.FullRotation));

                if (Cargo.Where(v => v is VehiclePawn && !(v is VehiclePawn_Trailer)).FirstOrDefault() != null)
                {
                    VehiclePawn p = (Cargo.Where(v => v is VehiclePawn).FirstOrDefault() as VehiclePawn);

                    //顯示的時候如果為逆向則會翻轉。
                    Rot8 rot = this.Props.renderOpposite ? Vehicle.FullRotation.Opposite : Vehicle.FullRotation;
                    p.Rotation = rot;
                    float trailerAngle = GetAngle(rot);

                    //砲塔
                    if (p.CompVehicleTurrets != null && !p.CompVehicleTurrets.turrets.NullOrEmpty())
                    {
                        foreach (var item in p.CompVehicleTurrets.turrets)
                        {
                            item.TurretRotation = item.defaultAngleRotated + rot.AsAngle;
                        }
                    }
                    p.DrawAt(Vehicle.DrawPos + (CompDrawPos(Vehicle.FullRotation) * Length(p)) + GetDrawOffset(Vehicle.FullRotation), rot, trailerAngle, compDraw: true);
                }
            }
        }
        public void DebugDraw(Vector3 exactPos)
        {
            Mesh mesh;
            mesh = MeshPool.plane10;
            Matrix4x4 matrix = Matrix4x4.TRS(exactPos + Vector3.up, Quaternion.identity, Vector3.one * 2);
            Texture2D texture = ContentFinder<Texture2D>.Get("RT_Dummy");
            Material m = MaterialPool.MatFrom(texture);
            Graphics.DrawMesh(mesh, matrix, m, 0);
        }
        public override void PostDrawUnspawned(Vector3 drawLoc, Rot8 rot8, float rotation)
        {
            base.PostDrawUnspawned(drawLoc, rot8, rotation);
            if(!Props.renderVehicle) return;

            if (DebugSettings.godMode) DebugDraw(drawLoc + GetDrawOffset(rot8));

            if (Cargo.Where(v => v is VehiclePawn && !(v is VehiclePawn_Trailer)).FirstOrDefault() != null)
            {
                VehiclePawn p = (Cargo.Where(v => v is VehiclePawn).FirstOrDefault() as VehiclePawn);

                //顯示的時候如果為逆向則會翻轉。
                Rot8 rot = this.Props.renderOpposite ? rot8.Opposite : rot8;
                float trailerAngle = GetAngle(rot);

                //砲塔
                if (p.CompVehicleTurrets != null && !p.CompVehicleTurrets.turrets.NullOrEmpty())
                {
                    foreach (var item in p.CompVehicleTurrets.turrets)
                    {
                        item.TurretRotation = item.defaultAngleRotated + rot.AsAngle;
                    }
                }
                p.DrawAt(drawLoc + (CompDrawPos(rot8) * Length(p)) + GetDrawOffset(rot8), rot, trailerAngle, compDraw: true);
            }
        }
        private Vector3 CompDrawPos(Rot8 rot)
        {
            return rot.Opposite.AsVector2.ToVector3();
        }
        private float GetAngle(Rot8 rot)
        {
            switch (rot.AsInt)
            {
                case 4:
                    return 90;
                case 5:
                    return -90;
                case 6:
                    return 90;
                case 7:
                    return -90;
                default:
                    return 0;
            }
        }
        private Vector3 GetDrawOffset(Rot8 rot)
        {
            return Props.GetOffsetFor(rot);
        }
        private float Length(VehiclePawn pawn)
        {
            if (this.Props.drawData != null) return 0;
            return GapLength(pawn) + GapLength(Vehicle) + 1.5f;
        }
        private float GapLength(VehiclePawn pawn)
        {
            return pawn.def.Size.z / 2;
        }
    }
}
