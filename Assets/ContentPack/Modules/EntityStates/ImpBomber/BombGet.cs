using EntityStates;
using RoR2;
using System;
using UnityEngine;

namespace TurboEdition.EntityStates.ImpBomber.Weapon
{
    internal class BombGet : BaseState
    {
        public static float baseDuration;
        public static string enterSoundString;
        public static string bombBoneChildName;

        private ChildLocator childLocator;
        private GameObject bombInstance;
        [SerializeField]
        public GameObject bombPrefabDefault;

        private float duration
        {
            get
            {
                return baseDuration / this.attackSpeedStat;
            }
        }

        public override void OnEnter()
        {
            base.OnEnter();
            if (skillLocator)
            {
                skillLocator.primary.DeductStock(skillLocator.primary.stock);
            }
            base.PlayCrossfade("Gesture, Additive", "GetBomb", "BombGet.playbackRate", this.duration, 0.1f);
            Util.PlaySound(enterSoundString, base.gameObject);
            this.childLocator = base.GetModelChildLocator();
            if (this.childLocator)
            {
                Transform transform = this.childLocator.FindChild(bombBoneChildName) ?? base.characterBody.coreTransform;
                if (transform && this.bombPrefabDefault)
                {
                    this.bombInstance = UnityEngine.Object.Instantiate<GameObject>(this.bombPrefabDefault, transform.position, transform.rotation, transform);
                }
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (base.isAuthority && base.fixedAge > this.duration)
            {
                BombHolding jc = new BombHolding();
                jc.boneBombInstance = bombInstance;
                this.outer.SetNextState(jc);
            }
        }
        public override void OnExit()
        {
            if (base.skillLocator)
            {
                base.GetModelAnimator().SetBool("BombHolding.active", true);
                base.PlayCrossfade("Gesture, Override", "HoldingBomb", 0.5f);
                skillLocator.primary.AddOneStock();
            }
            base.OnExit();
        }
        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }
    }
}