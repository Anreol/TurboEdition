using EntityStates;
using RoR2;
using RoR2.Stats;
using System;
using UnityEngine;
using UnityEngine.Networking;

namespace TurboEdition.EntityStates.Grenadier
{
    public class DeathState : GenericCharacterDeath
    {
        [Tooltip("Prefab that will be created on start and attached to the child transform.")]
        [SerializeField]
        public GameObject onEnterPrefab;

        [Tooltip("Prefab that will be created when the behavior happens on the child transform.")]
        [SerializeField]
        public GameObject deathPrefab;

        [Tooltip("Where the effects will happen.")]
        [SerializeField]
        public string childLocatorString = "NeckBomb";

        [Tooltip("ID of the achievement to unlock when killing with the explosion.")]
        [SerializeField]
        public string achievementIdentificator = "GrenadierPostDeathKill";

        [Tooltip("Seconds after the behavior happens.")]
        public static int timeUntilBehavior;

        [Tooltip("Blast Attack: Damage of the explosion.")]
        public static float damageCoefficient;

        [Tooltip("Blast Attack: Radius in meters of the explosion.")]
        public static float blastRadius;

        [Tooltip("Blast Attack: Proc coefficient, unsure if it does anything considering that the user is dead.")]
        public static float blastProcCoeff;

        [Tooltip("Blast Attack: Base physics force of the explosion.")]
        public static float blastBaseForce;

        [Tooltip("Blast Attack: Bonus physics force of the explosion.")]
        public static float blastBonusForce;

        [Tooltip("Should the game not end for a little bit, to make sure the player sees the explosion. Set to zero or less to disable.")]
        public static int preventGameOverDuration;

        public static event Action<BlastAttack.Result> onPostDeathExplosionServer;

        private float stopwatch;
        private bool attemptedDeathBehavior;
        private Transform centerTransform;

        public override void OnEnter()
        {
            base.OnEnter();
            if (NetworkServer.active && preventGameOverDuration > 0 && !isBrittle && !isVoidDeath)
            {
                characterBody.master.preventGameOver = true;
            }
            if (base.modelLocator)
            {
                ChildLocator component = base.modelLocator.modelTransform.GetComponent<ChildLocator>();
                if (component)
                {
                    this.centerTransform = component.FindChild(childLocatorString);
                    if (onEnterPrefab)
                    {
                        GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(onEnterPrefab, this.centerTransform.position, this.centerTransform.rotation);
                        ScaleParticleSystemDuration scale = gameObject.GetComponent<ScaleParticleSystemDuration>();
                        if (scale)
                        {
                            scale.newDuration = timeUntilBehavior;
                        }
                        gameObject.transform.parent = this.centerTransform;
                    }
                }
            }
        }

        public override void FixedUpdate()
        {
            this.stopwatch += Time.fixedDeltaTime;
            if (this.stopwatch >= timeUntilBehavior && !isBrittle && !isVoidDeath)
            {
                this.AttemptDeathBehavior();
                if (stopwatch >= preventGameOverDuration + timeUntilBehavior && characterBody && characterBody.master && characterBody.master.preventGameOver)
                {
                    characterBody.master.preventGameOver = false;
                }
            }
        }

        private void AttemptDeathBehavior()
        {
            if (this.attemptedDeathBehavior)
            {
                return;
            }
            this.attemptedDeathBehavior = true;
            if (deathPrefab && NetworkServer.active && this.centerTransform)
            {
                EffectManager.SpawnEffect(deathPrefab, new EffectData
                {
                    origin = this.centerTransform.position,
                    scale = blastRadius,
                    color = new Color(1, 0.5f, 0.7f)
                }, true);

                PlayerStatsComponent playerStatsComponent = PlayerStatsComponent.FindMasterStatsComponent(characterBody.master);
                StatSheet temp = null;
                if (playerStatsComponent)
                {
                    temp = playerStatsComponent.currentStats;
                }

                BlastAttack blastAttack = new BlastAttack();
                blastAttack.baseDamage = damageStat * damageCoefficient;
                blastAttack.crit = RollCrit();
                blastAttack.damageColorIndex = DamageColorIndex.Nearby;
                blastAttack.damageType |= DamageType.Stun1s | DamageType.BypassArmor | DamageType.BypassBlock | DamageType.BypassOneShotProtection;
                blastAttack.position = centerTransform.position;
                blastAttack.radius = blastRadius;
                blastAttack.attacker = (characterBody.gameObject ? characterBody.gameObject : null);
                blastAttack.inflictor = gameObject;
                blastAttack.teamIndex = characterBody.teamComponent.teamIndex;
                blastAttack.procChainMask = default;
                blastAttack.procCoefficient = blastProcCoeff;
                blastAttack.baseForce = blastBaseForce;
                blastAttack.bonusForce = new Vector3(2, 1, 2) * blastBonusForce;
                blastAttack.falloffModel = BlastAttack.FalloffModel.SweetSpot;
                blastAttack.attackerFiltering = AttackerFiltering.NeverHitSelf;
                blastAttack.canRejectForce = false;
                BlastAttack.Result result = blastAttack.Fire();
                onPostDeathExplosionServer?.Invoke(result);

                if (temp != null)
                {
                    playerStatsComponent.currentStats = temp;
                }
            }
        }

        public override void OnExit()
        {
            if (!this.outer.destroying)
            {
                this.AttemptDeathBehavior();
            }
            base.OnExit();
        }
    }
}