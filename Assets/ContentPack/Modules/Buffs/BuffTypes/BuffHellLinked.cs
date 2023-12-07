using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Networking;

namespace TurboEdition.Buffs
{
    public class BuffHellLinked : Buff
    {
        public override BuffDef buffDef { get; set; } = TEContent.Buffs.HellLinked;

        public override void Initialize()
        {
        }

        public override void BuffStep(ref CharacterBody body, int stack)
        {
        }

        public override void OnBuffFirstStackGained(ref CharacterBody body)
        {
            HellLinkedManager.AddBody(body);
        }

        public override void OnBuffLastStackLost(ref CharacterBody body)
        {
            HellLinkedManager.RemoveBody(body);
        }

        public override void RecalcStatsStart(ref CharacterBody body)
        {
        }

        public override void RecalcStatsEnd(ref CharacterBody body)
        {
        }

        /// <summary>
        /// Class that handles transmitting damage from one body to another (Server) along with the visuals (Client).
        /// </summary>
        public static class HellLinkedManager
        {
            private static Dictionary<CharacterBody, HellLinkedServerReceiver> hellLinkReceivers = new Dictionary<CharacterBody, HellLinkedServerReceiver>();

            [SystemInitializer(typeof(BuffCatalog))]
            public static void SubscribeToApplicationFixedUpdate()
            {
                //RoR2Application.onFixedUpdate += ProcessList;
                Stage.onServerStageBegin += FlushData;
            }

            public static void AddBody(CharacterBody characterBody)
            {
                HellLinkedServerReceiver hellLinkedServerReceiver = null;
                if (NetworkServer.active) //No need to allocate memory in clients
                {
                    hellLinkedServerReceiver = new HellLinkedServerReceiver(characterBody);
                }
                hellLinkReceivers.Add(characterBody, hellLinkedServerReceiver); //waow
            }

            public static void RemoveBody(CharacterBody characterBody)
            {
                hellLinkReceivers.TryGetValue(characterBody, out HellLinkedServerReceiver hellLinkedReceiver);
                if (hellLinkedReceiver != null) //Server stuff
                {
                    hellLinkedReceiver.Dispose();
                }
                hellLinkReceivers.Remove(characterBody);
            }

            private static void ProcessList()
            {
                if (hellLinkReceivers.Count > 0)
                {
                }
            }

            private static void FlushData(Stage obj)
            {
                foreach (KeyValuePair<CharacterBody, HellLinkedServerReceiver> kvp in hellLinkReceivers)
                {
                    kvp.Value?.Dispose(); //Server stuff
                }
                hellLinkReceivers.Clear();
            }

            /// <summary>
            /// This should ONLY be called IN THE SERVER. If it gets called from the client that's WRONG!
            /// </summary>
            /// <param name="damageInfo"></param>
            /// <param name="teamIndex"></param>
            internal static void RelayDamageToTeam(DamageInfo damageInfo, uint teamIndex)
            {
                foreach (CharacterBody characterBody in hellLinkReceivers.Keys.Where((cb) => { return cb.teamComponent?.teamIndex == (TeamIndex)teamIndex && cb.healthComponent.alive; }))
                {
                    characterBody.healthComponent.TakeDamage(damageInfo);
                    //Bullet attacks do this... but nothing else seems to! wack!
                    TeamComponent teamComponent = damageInfo.attacker.gameObject.GetComponent<TeamComponent>();
                    if (teamComponent && FriendlyFireManager.ShouldDirectHitProceed(characterBody.healthComponent, teamComponent.teamIndex))
                    {
                        GlobalEventManager.instance.OnHitEnemy(damageInfo, characterBody.healthComponent.gameObject);
                    }
                    GlobalEventManager.instance.OnHitAll(damageInfo, characterBody.healthComponent.gameObject);
                }
            }

            /// <summary>
            /// Class that is ONLY meant to exist in the Server.
            /// </summary>
            public class HellLinkedServerReceiver : IOnTakeDamageServerReceiver, IDisposable
            {
                private CharacterBody linkedBody;

                public HellLinkedServerReceiver(CharacterBody characterBody)
                {
                    linkedBody = characterBody;
                    if (linkedBody.healthComponent)
                        HG.ArrayUtils.ArrayAppend(ref linkedBody.healthComponent.onTakeDamageReceivers, this);
                }

                public void OnTakeDamageServer(DamageReport damageReport)
                {
                    if (damageReport.isFallDamage || damageReport.damageInfo.rejected || damageReport.damageInfo.damageType.HasFlag(DamageType.BypassBlock))
                    {
                        return;
                    }
                    DamageInfo damageInfo = new DamageInfo()
                    {
                        attacker = damageReport.damageInfo.attacker,
                        crit = damageReport.damageInfo.crit,
                        canRejectForce = damageReport.damageInfo.canRejectForce,
                        damage = damageReport.damageInfo.damage * 0.5f,
                        damageColorIndex = DamageColorIndex.DeathMark,
                        damageType = damageReport.damageInfo.damageType |= DamageType.BypassBlock,
                        dotIndex = damageReport.damageInfo.dotIndex,
                        force = damageReport.damageInfo.force * 0.5f,
                        inflictor = damageReport.damageInfo.inflictor,
                        position = damageReport.damageInfo.position,
                        procChainMask = damageReport.damageInfo.procChainMask,
                        procCoefficient = damageReport.damageInfo.procCoefficient,
                    };
                    RelayDamageToTeam(damageInfo, (uint)linkedBody.teamComponent.teamIndex);
                }

                public void Dispose()
                {
                    //This SHOULDNT cause any errors because nothing should be fucking with the order of things in this list... I hope.
                    if (linkedBody.healthComponent)
                    {
                        int i = System.Array.IndexOf(linkedBody.healthComponent.onTakeDamageReceivers, this);
                        if (i > -1)
                            HG.ArrayUtils.ArrayRemoveAtAndResize(ref linkedBody.healthComponent.onTakeDamageReceivers, linkedBody.healthComponent.onTakeDamageReceivers.Length, i);
                    }
                }
            }
        }
    }
}