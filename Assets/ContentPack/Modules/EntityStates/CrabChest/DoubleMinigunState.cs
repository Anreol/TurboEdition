using EntityStates;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace TurboEdition.EntityStates.CrabChest.Weapon
{
    internal class DoubleMinigunState : BaseState
    {
        public static string leftMuzzleName;
        public static string rightMuzzleName;
        protected Transform leftMuzzleTransform;
        protected Transform rightMuzzleTransform;
        private static readonly BuffDef slowBuff = RoR2Content.Buffs.Slow50;

        public override void OnEnter()
        {
            base.OnEnter();
            this.leftMuzzleTransform = base.FindModelChild(DoubleMinigunState.leftMuzzleName);
            this.rightMuzzleTransform = base.FindModelChild(DoubleMinigunState.rightMuzzleName);
            if (NetworkServer.active && base.characterBody)
            {
                base.characterBody.AddBuff(DoubleMinigunState.slowBuff);
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            base.StartAimMode(2f, false);
        }

        public override void OnExit()
        {
            if (NetworkServer.active && base.characterBody)
            {
                base.characterBody.RemoveBuff(DoubleMinigunState.slowBuff);
            }
            base.OnExit();
        }

        protected ref InputBankTest.ButtonState skillButtonState
        {
            get
            {
                return ref base.inputBank.skill1;
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
    }
}