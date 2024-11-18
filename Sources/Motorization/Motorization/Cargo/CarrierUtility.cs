using Verse;
using Verse.AI;
using Vehicles;
using UnityEngine;


namespace Motorization
{
    public static class CarrierUtility
    {
        public static void DebugDraw(Vector3 exactPos)
        {
            Mesh mesh;
            mesh = MeshPool.plane10;
            Matrix4x4 matrix = Matrix4x4.TRS(exactPos + Vector3.up, Quaternion.identity, Vector3.one * 2);
            Texture2D texture = ContentFinder<Texture2D>.Get("RT_Dummy");
            Material m = MaterialPool.MatFrom(texture);
            Graphics.DrawMesh(mesh, matrix, m, 0);
        }
        public static Toil ToNearestCell(VehiclePawn thisVehicle, VehiclePawn targetVehicle,bool reverse = false)
        {
            Toil toil = ToilMaker.MakeToil("GotoThing");
            toil.initAction = delegate
            {
                thisVehicle.ignition.Drafted = true;
                thisVehicle.vehiclePather.StartPath(RearCell(targetVehicle, reverse), PathEndMode.Touch);
            };
            toil.defaultCompleteMode = ToilCompleteMode.PatherArrival;
            toil.FailOnDespawnedOrNull(TargetIndex.A);
            return toil;
        }
        public static IntVec3 RearCell(VehiclePawn target, bool reverse = false)
        {
            Vector2 v2 = target.FullRotation.AsVector2.normalized;
            if (reverse)
            {
                v2 = target.FullRotation.Opposite.AsVector2.normalized;
            }
            IntVec3 v = new IntVec3((int)v2.x, 0, (int)v2.y) * ((target.def.Size.z + 3) / 2);
            return target.Position + v;
        }
    }
}
