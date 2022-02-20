using RoR2;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace TurboEdition.Items
{
    public class GracePeriod : Item
    {
        public override ItemDef itemDef { get; set; } = Assets.mainAssetBundle.LoadAsset<ItemDef>("GracePeriod");

        public override void AddBehavior(ref CharacterBody body, int stack)
        {
            if (NetworkServer.active)
            {
                body.AddItemBehavior<GracePeriodBehaviorServer>(stack);

            }
        }

        //Code slightly based on Moffein's solution
        internal class GracePeriodBehaviorServer : CharacterBody.ItemBehavior
        {
            private List<GraceBufferHit> hitList = new List<GraceBufferHit>();
            private List<GraceBufferHit> buffer = new List<GraceBufferHit>();

            private void OnEnable()
            {
                GlobalEventManager.onServerDamageDealt += GlobalEventManager_onServerDamageDealt;
            }

            private void OnDisable()
            {
                GlobalEventManager.onServerDamageDealt -= GlobalEventManager_onServerDamageDealt;
            }

            private void GlobalEventManager_onServerDamageDealt(DamageReport obj)
            {
                bool gotcha = false; //Keep it simple
                foreach (GraceBufferHit item in hitList)
                {
                    if (item.storedBody == obj.victimBody && item.duration > 0f)
                    {
                        gotcha = true;
                        if (ShouldAdd(obj))
                        {
                            item.duration = stack;
                            item.storedReport = obj;
                        }
                        else if (!obj.victim.alive && obj.attackerBody != body)
                        {
                            SimulateDeathMethods(item);
                        }
                    }
                }

                if (!gotcha && ShouldAdd(obj))
                {
                    GraceBufferHit gbh = new GraceBufferHit(body, obj, stack);
                    hitList.Add(gbh);
                }
            }

            public void FixedUpdate()
            {
                buffer.Clear();
                foreach (GraceBufferHit item in hitList)
                {
                    item.duration -= Time.fixedDeltaTime; //TODO: look into this not breaking if game's paused
                    if (item.duration <= 0)
                    {
                        buffer.Add(item);
                    }
                }
                foreach (GraceBufferHit item in buffer)
                {
                    hitList.Remove(item);
                }
                buffer.Clear();
            }

            private bool ShouldAdd(DamageReport obj)
            {
                if (obj.attackerBody != body) return false;
                if (obj.isFallDamage || obj.dotType != DotController.DotIndex.None || obj.damageInfo.rejected || obj.damageInfo.procChainMask.HasProc(ProcType.BounceNearby) || obj.damageInfo.procChainMask.HasProc(ProcType.Missile)) return false;
                if (Util.CheckRoll(100f, body.master.luck, null) && obj.victim.alive) return true;
                return false;
            }

            private void SimulateDeathMethods(GraceBufferHit graceBufferHit)
            {
                if (graceBufferHit.storedReport.victim.alive) return;
                IOnKilledServerReceiver[] components = graceBufferHit.storedReport.victim.GetComponents<IOnKilledServerReceiver>();
                for (int i = 0; i < components.Length; i++)
                {
                    components[i].OnKilledServer(graceBufferHit.storedReport);
                }
                if (graceBufferHit.storedReport.damageInfo.attacker)
                {
                    IOnKilledOtherServerReceiver[] components2 = graceBufferHit.storedReport.damageInfo.attacker.GetComponents<IOnKilledOtherServerReceiver>();
                    for (int i = 0; i < components2.Length; i++)
                    {
                        components2[i].OnKilledOtherServer(graceBufferHit.storedReport);
                    }
                }
                if (Util.CheckRoll(graceBufferHit.storedReport.victim.globalDeathEventChanceCoefficient * 100f, 0f, null))
                {
                    GlobalEventManager.instance.OnCharacterDeath(graceBufferHit.storedReport);
                    return;
                }
            }

            public class GraceBufferHit
            {
                public float duration;
                public DamageReport storedReport;
                public CharacterBody storedBody;

                public GraceBufferHit(CharacterBody body, DamageReport damageDealt, float l)
                {
                    this.storedBody = body;
                    storedReport = damageDealt;
                    duration = l;
                }
            }
        }
    }
}