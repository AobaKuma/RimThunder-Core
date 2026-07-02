using System.Linq;
using RimWorld;
using SmashTools;
using UnityEngine;
using Vehicles;
using Verse;

namespace Motorization
{
    /// <summary>
    /// 「母車攜帶一台子載具(VehiclePawn)並每幀畫出來」的共用骨架。
    /// CompTrailerMount(拖車掛載)與CompVehicleCargo(貨艙裝載)都是這個模式的具體實例，
    /// 差別只在於子載具相對母車的偏移量/角度怎麼算。
    /// </summary>
    public abstract class CompVehicleAttachment : VehicleComp
    {
        /// <summary>
        /// 記錄「上一次已經初始化過渲染狀態」的子載具是誰。
        /// 用來判斷是否需要重新初始化(而不是只用一次性的bool)，
        /// 因為同一個母車在遊戲過程中可能會先後裝載不同的子載具(卸下A、裝上B)，
        /// 如果只用一次性flag，B第一次被畫的時候就不會被正確初始化。
        /// </summary>
        private VehiclePawn lastInitializedChild;

        public ThingOwner<Thing> Cargo => Vehicle.inventory?.innerContainer;

        public bool HasPayload => Cargo.HasData() && Cargo.ContainsAny(v => v is VehiclePawn);

        public VehiclePawn GetPayload => Cargo?.Where(v => v is VehiclePawn).FirstOrDefault() as VehiclePawn;

        /// <summary>
        /// 子載具顯示時是否要以母車的反方向為基準(例如拖車在母車後方，方向自然是相反的)。
        /// </summary>
        protected virtual bool RenderOpposite => false;

        /// <summary>
        /// 子載具實際要顯示的旋轉。預設就是母車方向(或其反向)，
        /// CompTrailerMount會覆寫這個方法加入轉向延遲模擬。
        /// </summary>
        protected virtual Rot8 GetChildRotation(Rot8 parentRot)
        {
            return RenderOpposite ? parentRot.Opposite : parentRot;
        }

        /// <summary>
        /// 子載具相對母車 DrawPos 的位移量。由子類別依各自的定位方式實作
        /// (拖車用掛點pivot表；貨艙用長度間隙+方向資料表)。
        /// </summary>
        protected abstract Vector3 GetChildOffset(Rot8 parentRot, Rot8 childRot, VehiclePawn child);

        /// <summary>
        /// 子載具貼圖的「額外」旋轉角度，疊加在框架已經算好的角度之上。
        /// 注意：對角線方向的貼圖校正(±45度)由框架的Graphic_Rgb.ParallelGetPreRenderResults
        /// 透過 orientation.AsRotationAngle 自動處理，不需要(也不應該)在這裡重複加上±90度，
        /// 否則會疊加成錯誤的角度，造成子載具在對角線朝向時渲染方向明顯偏斜(90度)。
        /// 這裡預設回傳0，子類別如果有額外需求(例如特殊的視覺傾斜效果)才覆寫。
        /// </summary>
        protected virtual float GetChildAngle(Rot8 rot)
        {
            return 0f;
        }

        /// <summary>
        /// 是否要渲染子載具(例如CompProperties_VehicleCargo.renderVehicle=false時可以關閉)。
        /// </summary>
        protected virtual bool ShouldRenderChild => true;

        /// <summary>
        /// 每幀畫圖前的掛勾，子類別可用來畫Debug輔助線(CarrierUtility.DebugDraw)。
        /// </summary>
        protected virtual void OnBeforeDrawChild(Vector3 drawLoc, Rot8 parentRot)
        {
        }

        public override void PostDraw()
        {
            base.PostDraw();
            if (!Vehicle.Spawned) return;
            OnBeforeDrawChild(Vehicle.DrawPos, Vehicle.FullRotation);
            DrawChildAt(Vehicle.DrawPos, Vehicle.FullRotation);
        }

        /// <summary>
        /// 給母車本身也是被別人手動畫出來(未Spawn)時使用，例如拖車被拖曳車拖著跑。
        /// </summary>
        public void DrawUnspawned(Vector3 drawLoc, Rot8 parentRot, float parentRotationAngle)
        {
            OnBeforeDrawChild(drawLoc, parentRot);
            DrawChildAt(drawLoc, parentRot);
        }

        private void DrawChildAt(Vector3 drawLoc, Rot8 parentRot)
        {
            if (!ShouldRenderChild) return;
            if (!(GetPayload is VehiclePawn child)) return;

            if (lastInitializedChild != child)
            {
                lastInitializedChild = child;
                child.ResetRenderStatus(); //媽的你不要動他。

                //子載具被裝進貨艙/掛上拖車時通常已經DeSpawn，之後也不會再被Spawn，
                //代表它自己的CompVehicleTurrets.PostSpawnSetup(炮塔渲染器實際掛上DrawTracker的地方)
                //可能從頭到尾都沒執行過(例如直接生成在貨艙裡)，或是存檔讀取後DrawTracker被重建成空的
                //(DrawTracker本身不隨存檔保存)，導致炮塔渲染器沒有掛回去而完全不渲染。
                //這裡主動呼叫一次RevalidateTurrets()，確保無論子載具的Spawn歷史為何，
                //炮塔渲染器都會正確掛在它自己的DrawTracker上。這個方法本身是先解除註冊再重新註冊，
                //可以安全地重複呼叫，不會造成重複註冊的例外。
                child.CompVehicleTurrets?.RevalidateTurrets();
                return;
            }

            Rot8 childRot = GetChildRotation(parentRot);
            child.Rotation = childRot;
            float childAngle = GetChildAngle(childRot);

            SyncTurretRotation(child, childRot);

            child.DrawAt(drawLoc + GetChildOffset(parentRot, childRot, child), childRot, childAngle);
        }

        protected void SyncTurretRotation(VehiclePawn child, Rot8 rot)
        {
            if (child.CompVehicleTurrets != null && child.CompVehicleTurrets.Turrets != null)
            {
                foreach (var turret in child.CompVehicleTurrets.Turrets)
                {
                    turret.TurretRotation = turret.defaultAngleRotated + rot.AsAngle;
                }
            }
        }

        /// <summary>
        /// 驗證是否可以把thing掛載/裝進來，由子類別實作各自的規則(尺寸限制、類型限制等)。
        /// </summary>
        public abstract AcceptanceReport Accepts(Thing thing);

        /// <summary>
        /// 主動方(母車/拖曳車)把thing收進自己身上。呼叫前必須已經Accepts()過。
        /// </summary>
        public bool TryAttach(VehiclePawn thing)
        {
            if (!Accepts(thing))
            {
                return false;
            }
            thing.DeSpawn();
            Vehicle.AddOrTransfer(thing);
            return true;
        }
    }
}
