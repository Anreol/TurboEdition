using RoR2;
using RoR2.Projectile;
using RoR2.Skills;
using TurboEdition.EntityStates.Grenadier;
using UnityEngine;
using UnityEngine.Networking;

namespace TurboEdition.Components
{
    [RequireComponent(typeof(CharacterBody))]
    internal class GrenadierPassiveController : NetworkBehaviour, IOnIncomingDamageServerReceiver
    {
        private bool hasEffectiveAuthority
        {
            get
            {
                return this.characterBody.hasEffectiveAuthority;
            }
        }

        private bool isRolling
        {
            get
            {
                return resolvedBodyMachine.state.GetType() == typeof(BasePushMoreState);
            }
        }

        //TODO: run on the client, check for body auth, doesn't have to be networked, bomblets will be projectiles so we let the projectile manager take care of networking.
        private CharacterBody characterBody;

        private EntityStateMachine resolvedBodyMachine;
        private bool[] serverBlastArmorStates;
        private float blastArmorRechargeTime;

        [Header("Referenced Components")]
        public GenericSkill passiveSkillSlot;

        [Header("Skill Defs")]
        public SkillDef PassiveBlastArmorSkillDef;

        public GameObject PassiveBlastArmorBombletPrefab;

        private void Awake()
        {
            this.characterBody = base.GetComponent<CharacterBody>();
            if (!PassiveBlastArmorSkillDef || !passiveSkillSlot)
                Debug.LogError("Couldn't find a passive skill def or a passive skill slot for the Grenadier's blast armor, character won't work properly!");
            if (PassiveBlastArmorSkillDef)
            {
                serverBlastArmorStates = new bool[PassiveBlastArmorSkillDef.baseMaxStock];
                for (int i = 0; i < serverBlastArmorStates.Length; i++)
                    serverBlastArmorStates[i] = true;
            }
            resolvedBodyMachine = EntityStateMachine.FindByCustomName(characterBody.gameObject, "Body");
        }

        [Server]
        internal void FixedUpdate()
        {
            if (passiveSkillSlot)
            {
                if (passiveSkillSlot.skillDef == PassiveBlastArmorSkillDef)
                {
                    float baseIntervals = 1f / (float)PassiveBlastArmorSkillDef.baseMaxStock;
                    if (!characterBody.outOfDanger)
                    {
                        //Inside NOT isOutOfDanger because we don't want to trigger if regenerating hp
                        for (int i = serverBlastArmorStates.Length - 1; i >= 0 && baseIntervals * i > 0f; i--)
                        {
                            if (serverBlastArmorStates[i] && characterBody.healthComponent.combinedHealthFraction < baseIntervals * i)
                            {
                                serverBlastArmorStates[i] = false;
                                ServerTriggerBomblets();
                                break;
                            }
                        }
                        blastArmorRechargeTime = PassiveBlastArmorSkillDef.baseRechargeInterval;
                        return;
                    }
                    if (characterBody.outOfDanger)
                    {
                        blastArmorRechargeTime -= Time.fixedDeltaTime;
                        if (blastArmorRechargeTime <= 0)
                        {
                            for (int i = 0; i < serverBlastArmorStates.Length; i++)
                            {
                                if (!serverBlastArmorStates[i] && baseIntervals * i > characterBody.healthComponent.combinedHealthFraction) //Require to be above breaker to recharge it
                                {
                                    serverBlastArmorStates[i] = true;
                                    blastArmorRechargeTime = PassiveBlastArmorSkillDef.baseRechargeInterval;
                                    break;
                                }
                            }
                        }
                    }
                }
            }
        }

        private void ServerTriggerBomblets()
        {
            if (PassiveBlastArmorBombletPrefab == null)
                return;
            int rand = UnityEngine.Random.Range(3, 7);
            for (int i = 0; i < rand; i++)
            {
                //SpawnBomblet(PassiveBlastArmorBombletPrefab, characterBody.corePosition, characterBody.damage, 200f, Vector3.zero, 12f, rand / 9f, 1f);
                SpawnBomblet(PassiveBlastArmorBombletPrefab, characterBody.corePosition, characterBody.transform, characterBody.damage * 3, 50f, Vector3.zero, 12f, rand / 1.5f, 1f);
            }
        }

        //Runs on server, as DelayBlasts must be on the server
        private void SpawnBomblet(GameObject prefab, Vector3 position, Transform parent, float finalDamage, float finalBaseForce, Vector3 bonusForceVectorCoeff, float finalRadius, float timerStagger, float maxTimer)
        {
            GameObject go = UnityEngine.Object.Instantiate<GameObject>(prefab, parent);
            //GameObject go = UnityEngine.Object.Instantiate<GameObject>(prefab, position, UnityEngine.Random.rotation);
            go.transform.localScale = new Vector3(finalRadius, finalRadius, finalRadius); //Set scale same as radius for VFX
            TeamFilter teamFilter = go.GetComponent<TeamFilter>();
            DelayBlast delayBlast = go.GetComponent<DelayBlast>();
            teamFilter.teamIndex = characterBody.teamComponent.teamIndex;
            delayBlast.position = parent.localPosition;// position;
            delayBlast.baseDamage = finalDamage;
            delayBlast.baseForce = finalBaseForce;
            delayBlast.bonusForce = bonusForceVectorCoeff * finalDamage;
            delayBlast.radius = finalRadius;
            delayBlast.attacker = characterBody.gameObject;
            delayBlast.inflictor = null;
            delayBlast.teamFilter = teamFilter;
            delayBlast.crit = characterBody.RollCrit();
            delayBlast.damageColorIndex = DamageColorIndex.Default;
            delayBlast.damageType |= DamageType.AOE;
            delayBlast.falloffModel = BlastAttack.FalloffModel.None;
            delayBlast.procCoefficient = 1;

            delayBlast.timerStagger = timerStagger;
            delayBlast.maxTimer = maxTimer;
        }

        public void OnIncomingDamageServer(DamageInfo damageInfo) //This shouldnt need to be added directly as its built in the BODY prefab, and HC should take it automatically
        {
            if (passiveSkillSlot)
            {
                if (passiveSkillSlot.skillDef == PassiveBlastArmorSkillDef && damageInfo.inflictor && damageInfo.attacker == this.characterBody.gameObject)
                {
                    MarkReducedSelfDamage markReducedSelfDamage = damageInfo.inflictor.GetComponent<MarkReducedSelfDamage>();
                    if (markReducedSelfDamage)
                    {
                        damageInfo.attacker = null;
                        damageInfo.damage /= markReducedSelfDamage.reduceDamageFraction;
                        damageInfo.crit = markReducedSelfDamage.forceNoCrit ? false : damageInfo.crit;
                        damageInfo.damageColorIndex = DamageColorIndex.Default;
                        damageInfo.dotIndex = markReducedSelfDamage.clearDots ? DotController.DotIndex.None : damageInfo.dotIndex;
                        damageInfo.damageType = markReducedSelfDamage.damageTypeOverride;
                        damageInfo.procCoefficient = -255;
                        damageInfo.procChainMask = default(ProcChainMask);
                        if (characterBody.characterMotor && !characterBody.characterMotor.isGrounded)
                        {
                            damageInfo.force *= 4f;
                        }
                        if (isRolling)
                        {
                            damageInfo.force *= 2.5f;
                        }
                        if (damageInfo.rejected)
                        {
                            characterBody.healthComponent.TakeDamageForce(damageInfo, false, false);
                        }
                    }
                }
            }
        }
    }
}