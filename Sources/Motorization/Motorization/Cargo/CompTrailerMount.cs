using Vehicles;
using Verse;
using System.Collections.Generic;
using SmashTools;
using UnityEngine;
using RimWorld;


namespace Motorization
{
    public class CompProperties_TrailerMount : CompProperties
    {
        public List<Vector3> rotationPivot;
        public bool flipWhenTrailer = false;
        public CompProperties_TrailerMount()
        {
            compClass = typeof(CompTrailerMount);
        }
    }

    public class CompTrailerMount : VehicleComp//掛在母車跟子車上的
    {
        public Rot8 LatestRot => latestRot;
        protected VehiclePawn Pawn => parent as VehiclePawn;
        protected Rot8 latestRot;
        protected int UpdateInterval = 180;
        protected float tempInterval = 0;
        public CompProperties_TrailerMount Props => base.props as CompProperties_TrailerMount;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            Initial(Vehicle.FullRotation);
        }
        public override void Notify_DefsHotReloaded()
        {
            base.Notify_DefsHotReloaded();
        }
        public void DrawTrailer(VehiclePawn_Trailer pawn_Trailer, Vector3 exactPos, Rot8 rot, float angle)
        {
            //旋轉更新
            if (Find.TickManager.CurTimeSpeed != 0 && Pawn.movementStatus == VehicleMovementStatus.Online && Pawn.vehiclePather.Moving)
            {
                if (tempInterval > UpdateInterval)
                {
                    UpdateRotate();
                    tempInterval = 0;
                }
                tempInterval += Vehicle.statHandler.GetStatValue(VehicleStatDefOf.MoveSpeed);
            }
            if (rot.Opposite == latestRot)
            {
                latestRot = rot;
            }
            //母車掛點位置
            Vector3 tractorMount = GetPivot(Vehicle.FullRotation);
            if (pawn_Trailer.TryGetComp<CompTrailerMount>(out CompTrailerMount compTrailerMount))
            {
                //子車旋轉
                Rot8 trailerRot = compTrailerMount.Props.flipWhenTrailer ? LatestRot.Opposite : LatestRot;//顯示的時候如果為逆向則會翻轉。
                //子車掛點位置
                Vector3 trailerMount = compTrailerMount.GetPivot(trailerRot);

                float trailerAngle = GetAngle(trailerRot);
                //if (compTrailerMount.Props.flipWhenTrailer && trailerRot.IsDiagonal)
                //{
                //    trailerAngle += angle > 180 ? 90 : -90;
                //}

                //母車位置+母車(自身轉向時的)旋轉偏移 +子車(自身轉向時的)旋轉偏移
                foreach (var item in pawn_Trailer.CompVehicleTurrets.turrets)
                {
                    item.TurretRotation = item.defaultAngleRotated + trailerRot.AsAngle;
                }
                pawn_Trailer.DrawAt(exactPos + tractorMount - trailerMount, trailerRot, trailerAngle, compDraw: true);
            }
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
        public void Initial(Rot8 rot)
        {
            latestRot = rot;
        }
        protected void UpdateRotate()
        {
            if (latestRot == Vehicle.FullRotation) return;

            //如果大於4代表應該是順時鐘旋轉
            //4 - 3 = 1
            float ra = Vehicle.FullRotation.AsAngle - latestRot.AsAngle;
            if (ra < 0) ra += 360;
            Log.Message(ra);

            RotationDirection direction = ra > 180 ? RotationDirection.Counterclockwise : RotationDirection.Clockwise;
            latestRot.Rotate(direction,true);
            Log.Message(latestRot +" "+ direction);
        }
        public Vector3 GetPivot(Rot8 rot)
        {
            if (Props.rotationPivot != null)
            {
                return Props.rotationPivot[rot.AsInt];
            }
            return Vector3.zero;
        }
        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref latestRot, "latestRot", defaultValue: Rot8.Invalid);
            Scribe_Values.Look(ref tempInterval, "tempInterval", defaultValue: UpdateInterval);
        }
    }
}
