using RoR2;

namespace TurboEdition.EntityStates.CrabChest.Weapon
{
    internal class DoubleMinigunSpindown : DoubleMinigunState
    {
        public static float baseDuration;
        public static string exitPlaySoundEvent;

        private float duration;

        public override void OnEnter()
        {
            base.OnEnter();
            this.duration = DoubleMinigunSpindown.baseDuration / this.attackSpeedStat;
            Util.PlayAttackSpeedSound(DoubleMinigunSpindown.exitPlaySoundEvent, base.gameObject, this.attackSpeedStat);
            base.GetModelAnimator().SetBool("WeaponIsReady", false);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (base.fixedAge >= this.duration && base.isAuthority)
            {
                this.outer.SetNextStateToMain();
            }
        }
    }
}