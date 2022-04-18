using EntityStates;
using EntityStates.TitanMonster;
using RoR2;
using System;
using System.Linq;
using UnityEngine;

namespace TurboEdition.EntityStates.CrabChest.ItemScanner
{
    internal class LeapAtEnemy : BaseState
    {
        private FireFist.Predictor fistPredictor;
        private Transform targetTransform;
        private OverlapAttack attack;

        private Vector3 predictedTargetPosition;
        private bool hasLeapedAtPrediction;

        public float velocityInitialSpeed;
        public float moveSpeedStatCoefficient;
        public float leapDamageCoefficient;

        public string enterSoundString;
        public GameObject enterEffectPrefab;

        public float searchMaxDistance;
        public float searchMaxAngle;
        private float durationToSpendTracking;

        public override void OnEnter()
        {
            base.OnEnter();
            this.PlayAnimation("Base", "Leap");
            Util.PlaySound(enterSoundString, base.gameObject);
            if (enterEffectPrefab)
            {
                EffectManager.SimpleImpactEffect(enterEffectPrefab, base.characterBody.corePosition, Vector3.up, false);
            }

            BullseyeSearch bullseyeSearch = new BullseyeSearch();
            bullseyeSearch.viewer = base.characterBody;
            bullseyeSearch.teamMaskFilter = TeamMask.allButNeutral;
            bullseyeSearch.teamMaskFilter.RemoveTeam(base.characterBody.teamComponent.teamIndex);
            bullseyeSearch.sortMode = BullseyeSearch.SortMode.Distance;
            bullseyeSearch.minDistanceFilter = 0f;
            bullseyeSearch.maxDistanceFilter = searchMaxDistance;
            bullseyeSearch.searchOrigin = base.inputBank.aimOrigin;
            bullseyeSearch.searchDirection = base.inputBank.aimDirection;
            bullseyeSearch.maxAngleFilter = searchMaxAngle;
            bullseyeSearch.filterByLoS = true;
            bullseyeSearch.RefreshCandidates();
            HurtBox hurtBox = bullseyeSearch.GetResults().FirstOrDefault<HurtBox>();
            if (hurtBox)
            {
                fistPredictor = new FireFist.Predictor(base.transform);
                fistPredictor.SetTargetTransform(hurtBox.transform);
                this.targetTransform = hurtBox.transform;
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (this.fistPredictor != null)
                this.fistPredictor.Update();
            if (this.fixedAge <= durationToSpendTracking) //Tracking state....
            {
                if (this.fistPredictor != null)
                {
                    this.fistPredictor.GetPredictedTargetPosition(durationToSpendTracking, out this.predictedTargetPosition); //Update the target position
                }
            }
            else if (!this.hasLeapedAtPrediction)
            {
                this.hasLeapedAtPrediction = true;
                this.LeapAtTransform();
            }
            return;
        }

        public override void OnExit()
        {
            this.fistPredictor = null;
            base.PlayCrossfade("Body", "ExitFist", "ExitFist.playbackRate", FireFist.exitDuration, 0.3f);
            base.OnExit();
        }

        public void LeapAtTransform()
        {
            if (predictedTargetPosition != null || predictedTargetPosition != Vector3.zero)
                targetTransform.position = predictedTargetPosition;
            if (targetTransform == null || targetTransform.position == Vector3.zero)
                return;

            HitBoxGroup hitBoxGroup = null;
            Transform modelTransform = base.GetModelTransform();
            if (!modelTransform)
                return;
            hitBoxGroup = Array.Find<HitBoxGroup>(modelTransform.GetComponents<HitBoxGroup>(), (HitBoxGroup element) => element.groupName == "LeapBox");

            this.attack = new OverlapAttack();
            this.attack.attacker = base.gameObject;
            this.attack.inflictor = base.gameObject;
            this.attack.teamIndex = base.GetTeam();
            this.attack.damage = leapDamageCoefficient * this.damageStat;
            this.attack.hitEffectPrefab = null;
            this.attack.hitBoxGroup = hitBoxGroup;
            this.attack.isCrit = base.RollCrit();
            this.attack.damageType = DamageType.Stun1s;
            this.attack.damageColorIndex = DamageColorIndex.Default;

            if (base.characterMotor)
            {
                Vector3 position = this.targetTransform.position;
                float velocity = velocityInitialSpeed * (base.moveSpeedStat * moveSpeedStatCoefficient);
                Vector3 trajectoryVector = position - base.transform.position;
                Vector2 vector2 = new Vector2(trajectoryVector.x, trajectoryVector.z);
                float magnitude = vector2.magnitude;
                float y = Trajectory.CalculateInitialYSpeed(magnitude / velocity, trajectoryVector.y);
                Vector3 properVelocity = new Vector3(vector2.x / magnitude * velocity, y, vector2.y / magnitude * velocity);
                base.characterMotor.velocity = properVelocity;
                base.characterMotor.disableAirControlUntilCollision = true;
                base.characterMotor.Motor.ForceUnground();
                base.characterDirection.forward = properVelocity;
            }
        }
    }
}