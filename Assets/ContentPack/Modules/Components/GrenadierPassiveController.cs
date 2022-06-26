using RoR2;
using RoR2.Projectile;
using RoR2.Skills;
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
                return resolvedRollMachine.state.GetType() != resolvedRollMachine.mainStateType.stateType;
            }
        }

        //TODO: run on the client, check for body auth, doesn't have to be networked, bomblets will be projectiles so we let the projectile manager take care of networking.
        private CharacterBody characterBody;

        [HideInInspector]
        [SyncVar]
        public bool netIsOutOfDanger;

        private EntityStateMachine resolvedRollMachine;
        private bool[] authBlastArmorStates;
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
                authBlastArmorStates = new bool[PassiveBlastArmorSkillDef.baseMaxStock];
                for (int i = 0; i < authBlastArmorStates.Length; i++)
                    authBlastArmorStates[i] = true;
            }
            resolvedRollMachine = EntityStateMachine.FindByCustomName(characterBody.gameObject, "Roll");
        }

        private void FixedUpdate()
        {
            if (NetworkServer.active && characterBody.outOfDanger != netIsOutOfDanger)
                netIsOutOfDanger = characterBody.outOfDanger;

            if (hasEffectiveAuthority && passiveSkillSlot)
            {
                if (passiveSkillSlot.skillDef == PassiveBlastArmorSkillDef)
                {
                    float baseIntervals = 1f / (float)PassiveBlastArmorSkillDef.baseMaxStock;
                    if (!netIsOutOfDanger)
                    {
                        //Inside NOT isOutOfDanger because we don't want to trigger if regenerating hp
                        for (int i = authBlastArmorStates.Length - 1; i >= 0 && baseIntervals * i > 0f; i--)
                        {
                            if (authBlastArmorStates[i] && characterBody.healthComponent.combinedHealthFraction < baseIntervals * i)
                            {
                                authBlastArmorStates[i] = false;
                                AuthTriggerBomblets();
                                break;
                            }
                        }
                        blastArmorRechargeTime = PassiveBlastArmorSkillDef.baseRechargeInterval;
                        return;
                    }
                    if (netIsOutOfDanger)
                    {
                        blastArmorRechargeTime -= Time.fixedDeltaTime;
                        if (blastArmorRechargeTime <= 0)
                        {
                            for (int i = 0; i < authBlastArmorStates.Length; i++)
                            {
                                if (!authBlastArmorStates[i] && baseIntervals * i > characterBody.healthComponent.combinedHealthFraction) //Require to be above breaker to recharge it
                                {
                                    authBlastArmorStates[i] = true;
                                    blastArmorRechargeTime = PassiveBlastArmorSkillDef.baseRechargeInterval;
                                    break;
                                }
                            }
                        }
                    }
                }
            }
        }

        private void AuthTriggerBomblets()
        {
            if (PassiveBlastArmorBombletPrefab == null)
                return;
            int rand = UnityEngine.Random.Range(9, 13);
            for (int i = 0; i < rand; i++)
            {
                FireProjectileInfo fireProjectileInfo = new FireProjectileInfo
                {
                    projectilePrefab = PassiveBlastArmorBombletPrefab,
                    position = characterBody.corePosition + (UnityEngine.Random.insideUnitSphere * 5),
                    rotation = Util.QuaternionSafeLookRotation(Vector3.up + UnityEngine.Random.insideUnitSphere * UnityEngine.Random.Range(5f, 90f)), //Random rotation starting from up
                    owner = characterBody.gameObject,
                    damage = characterBody.damage * 1,
                    force = 250,
                    crit = characterBody.RollCrit(),
                    damageColorIndex = DamageColorIndex.Item
                };
                ProjectileManager.instance.FireProjectile(fireProjectileInfo);
            }
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
                        if (isRolling)
                        {
                            damageInfo.force *= 16f;
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