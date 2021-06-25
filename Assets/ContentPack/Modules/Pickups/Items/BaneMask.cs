using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace TurboEdition.Items
{
    public class BaneMask : Item
    {
        public override ItemDef itemDef { get; set; } = Assets.mainAssetBundle.LoadAsset<ItemDef>("BaneMask");

        public override void AddBehavior(ref CharacterBody body, int stack)
        {
            body.AddItemBehavior<Behavior>(stack);
        }
        internal class Behavior : CharacterBody.ItemBehavior
        {
            public static GameObject pulsePrefab = Assets.mainAssetBundle.LoadAsset<GameObject>("BaneMaskPulse");

            //Prefab info:
            //Final radius: 5, Duration 0.5
            //Destroy On Timer: 3 Seconds
            //Has Shaker Emiter
            private bool alreadyIn = false;

            private void FixedUpdate()
            {
                if (!body.outOfCombat && !alreadyIn)
                {
                    GeneratePulse();
                    alreadyIn = true;
                }
                else if (body.outOfCombat && alreadyIn)
                {
                    alreadyIn = false;
                }
            }

            private void GeneratePulse()
            {
                GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(pulsePrefab, body.transform.position, body.transform.rotation);
                //var sphereSearch = new SphereSearch(); shouldn't be needed at all?
                PulseController component = gameObject.GetComponent<PulseController>();
                component.finalRadius += stack;
                //component.performSearch += CS$<> 8__locals1.< CreatePulseServer > g__PerformPulseSearch | 0;
                component.onPulseHit += Component_onPulseHit;
                component.StartPulseServer();
                NetworkServer.Spawn(gameObject);
            }

            private void Component_onPulseHit(PulseController pulseController, PulseController.PulseHit hitInfo)
            {
                if (hitInfo.hitObject is GameObject gameObject)
                {
                    CharacterBody poopy = gameObject.GetComponent<CharacterBody>();
                    if (poopy.healthComponent)
                    {
                        if (TeamManager.IsTeamEnemy(body.teamComponent.teamIndex, poopy.teamComponent.teamIndex))
                        {
                            if (BuffCatalog.GetBuffDef(BuffCatalog.FindBuffIndex("BuffFear"))) //Lazy to check for SS2 installation, check if catalog has fear in
                            {
                                poopy.AddTimedBuff(BuffCatalog.GetBuffDef(BuffCatalog.FindBuffIndex("BuffFear")), hitInfo.hitSeverity);
                                return;
                            }
                            poopy.AddTimedBuff(RoR2Content.Buffs.Cripple, hitInfo.hitSeverity);
                        }
                    }
                }
            }
        }
    }
}