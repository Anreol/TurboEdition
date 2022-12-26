using EntityStates;
using RoR2;
using UnityEngine;

namespace TurboEdition.EntityStates.Grenadier.Weapon
{
    public class EnterReload : BaseState
    {
        [SerializeField]
        public float baseDuration;
        [SerializeField]
        public string enterSoundString;

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
            base.PlayCrossfade("Gesture, Override", "ReloadStart", "Reload.playbackRate", this.duration, 0.1f);
            Util.PlaySound(enterSoundString, base.gameObject);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (base.isAuthority && base.fixedAge > this.duration)
            {
                this.outer.SetNextState(GetNextState());
            }
        }
        public virtual Reload GetNextState()
        {
            return new Reload();
        }
        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }
    }
}