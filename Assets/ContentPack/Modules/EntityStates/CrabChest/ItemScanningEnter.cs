using RoR2;
using UnityEngine.Events;
using UnityEngine.Networking;

namespace TurboEdition.EntityStates.CrabChest.ItemScanner
{
    internal class ItemScanningEnter : BaseItemStealState
    {
        private bool hasSubscribedToStealFinish;
        private bool hasBegunSteal;

        //public int numMaxStrayDropletsToSteal;
        //public int numMaxItemStackToSteal;

        public int maxHeightTolerance;
        public int maxDistance;
        public int maxConeAngle;
        public string childBoneLocatorString;

        public int delayBeforeBeginningSteal;
        public int maxDuration;
        public float stealInterval;

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (!this.itemStealController)
            {
                return;
            }
            if (!this.hasSubscribedToStealFinish && base.isAuthority)
            {
                this.hasSubscribedToStealFinish = true;
                if (NetworkServer.active)
                {
                    this.itemStealController.onStealFinishServer.AddListener(new UnityAction(this.OnStealEndAuthority));
                }
                else
                {
                    this.itemStealController.onStealFinishClient += this.OnStealEndAuthority;
                }
            }
            if (NetworkServer.active && base.fixedAge > delayBeforeBeginningSteal && !this.hasBegunSteal)
            {
                this.hasBegunSteal = true;
                this.itemStealController.stealInterval = stealInterval;
                TeamIndex teamIndex = base.GetTeam();
                this.itemStealController.StartSteal((CharacterMaster characterMaster) => TeamManager.IsTeamEnemy(characterMaster.teamIndex, teamIndex) && characterMaster.hasBody && TransformBypassesFilter(characterMaster.transform.position, base.transform.position, base.characterBody.inputBank.aimDirection, maxConeAngle, maxDistance, maxHeightTolerance));
            }
            if (base.isAuthority && base.fixedAge > delayBeforeBeginningSteal + maxDuration)
            {
                this.outer.SetNextState(new ItemScanningExit());
            }
            if (childBoneLocatorString.Length > 0)
            {
                this.itemStealController.transform.position = base.GetModelChildLocator().FindChild(childBoneLocatorString).transform.position;
            }
        }

        public override void OnExit()
        {
            if (this.itemStealController && this.hasSubscribedToStealFinish)
            {
                this.itemStealController.onStealFinishServer.RemoveListener(new UnityAction(this.OnStealEndAuthority));
                this.itemStealController.onStealFinishClient -= this.OnStealEndAuthority;
            }
            base.OnExit();
        }

        private void OnStealEndAuthority()
        {
            this.outer.SetNextState(new ItemScanningExit());
        }
    }
}