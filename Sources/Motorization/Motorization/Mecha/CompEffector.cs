using RimWorld;
using System.Collections.Generic;
using Verse;
using static UnityEngine.GraphicsBuffer;


namespace Motorization
{
    public class CompEffector : ThingComp
    {
        private bool spawned = false;
        private Effecter _effect;
        public CompProperties_Effector Props => base.props as CompProperties_Effector;
        public override void CompTick()
        {
            if (parent.Spawned && parent.Map != null)
            {
                if (!spawned)
                {
                    _effect = Props.effecter.SpawnAttached(parent, parent.Map);
                }
                else
                {
                    spawned = true;
                    _effect.EffectTick(parent, parent);
                }
            }
        }
        public override void Notify_DefsHotReloaded()
        {
            base.Notify_DefsHotReloaded();
            if (spawned)
            {
                _effect.Cleanup();
                _effect = Props.effecter.SpawnAttached(parent, parent.Map);
            }
        }
        public override void PostDeSpawn(Map map)
        {
            base.PostDeSpawn(map);
            if (spawned)
            {
                spawned = false;
                _effect.Cleanup();
            }
        }
        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref spawned, "spawned");
            Scribe_Deep.Look(ref _effect, "effector", this);
        }
    }

    public class CompProperties_Effector : CompProperties
    {
        public EffecterDef effecter;
        public CompProperties_Effector()
        {
            compClass = typeof(CompDeployMode);
        }
    }
}
