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

        //TODO: run on the client, check for body auth, doesn't have to be networked, bomblets will be projectiles so we let the projectile manager take care of networking.
        private CharacterBody characterBody;

        [HideInInspector]
        [SyncVar]
        public bool netIsOutOfDanger;

        private bool[] authBlastArmorStates;

        public float baseBlastArmorRechargeTime;
        private float blastArmorRechargeTime;

        [Header("Referenced Components")]
        public GenericSkill passiveSkillSlot;

        [Tooltip("The projectiles that Grenadier will have resistance against.")]
        public GameObject[] grenadierProjectiles;

        [Header("Skill Defs")]
        public SkillDef PassiveBlastArmor;

        public GameObject PassiveBlastArmorBombletPrefab;

        private void Awake()
        {
            this.characterBody = base.GetComponent<CharacterBody>();
            authBlastArmorStates = new bool[PassiveBlastArmor.baseMaxStock];
            for (int i = 0; i < authBlastArmorStates.Length; i++)
            {
                authBlastArmorStates[i] = true;
            }
        }

        private void FixedUpdate()
        {
            if (NetworkServer.active)
            {
                if (characterBody.outOfDanger != netIsOutOfDanger)
                    netIsOutOfDanger = characterBody.outOfDanger;
            }
            if (hasEffectiveAuthority && passiveSkillSlot)
            {
                if (passiveSkillSlot.skillDef == PassiveBlastArmor)
                {
                    float baseIntervals = 1f / (float)PassiveBlastArmor.baseMaxStock;
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
                        blastArmorRechargeTime = baseBlastArmorRechargeTime;
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
                                    blastArmorRechargeTime = baseBlastArmorRechargeTime;
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
            Vector3 sphoore = UnityEngine.Random.insideUnitSphere;
            for (int i = 0; i < rand; i++)
            {
                FireProjectileInfo fireProjectileInfo = new FireProjectileInfo
                {
                    projectilePrefab = PassiveBlastArmorBombletPrefab,
                    position = characterBody.corePosition + (sphoore * 5),
                    rotation = Util.QuaternionSafeLookRotation(characterBody.corePosition),
                    owner = characterBody.gameObject,
                    damage = characterBody.damage * 1,
                    force = 250,
                    crit = characterBody.RollCrit(),
                    damageColorIndex = DamageColorIndex.Item
                };
                ProjectileManager.instance.FireProjectile(fireProjectileInfo);
            }
        }

        public void OnIncomingDamageServer(DamageInfo damageInfo) //This shouldnt need to be added directly as its built in the prefab, and HC should take it automatically
        {
            if (passiveSkillSlot)
            {
                if (passiveSkillSlot.skillDef == PassiveBlastArmor && !damageInfo.rejected)
                {
                    if (damageInfo.inflictor)
                    {
                        Debug.LogWarning("Inflictor: " + damageInfo.inflictor + " n: " + damageInfo.inflictor.name);
                        foreach (GameObject prefab in grenadierProjectiles)
                        {
                            Debug.LogWarning("Prefab thing: " + prefab + " n: " + prefab.name);
                            if (prefab ==  damageInfo.inflictor.gameObject)
                            {
                                Debug.LogError("Passed");
                                damageInfo.damage /= 2;
                                if (characterBody.healthComponent.isHealthLow)
                                    damageInfo.damage /= 2;
                                damageInfo.crit = false;
                                damageInfo.dotIndex = DotController.DotIndex.None;
                                damageInfo.damageType = DamageType.NonLethal;
                                damageInfo.procCoefficient = -255;
                            }
                        }
                    }
                }
            }
        }
    }
}