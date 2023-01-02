using RoR2;
using RoR2.Achievements;

namespace TurboEdition.Achievements
{
    public abstract class BaseHasUnlockableAchievement : BaseAchievement
    {
        protected abstract UnlockableDef[] unlockableDefs { get; }
        private int hasNumber;

        public override void OnInstall()
        {
            base.OnInstall();
            if (unlockableDefs != null)
            {
                foreach (UnlockableDef unlockable in unlockableDefs)
                {
                    if (unlockable != null && base.userProfile.HasUnlockable(unlockable))
                    {
                        hasNumber++;
                    }
                }
                if (hasNumber == unlockableDefs.Length - 1)
                {
                    base.Grant();
                    return;
                }
                hasNumber = 0;
            }
            UserProfile.onUnlockableGranted += this.OnUnlockableGranted;
        }

        public override void OnUninstall()
        {
            UserProfile.onUnlockableGranted -= this.OnUnlockableGranted;
            hasNumber = 0;
            base.OnUninstall();
        }

        private void OnUnlockableGranted(UserProfile userProfile, UnlockableDef unlockableDef)
        {
            if (unlockableDefs != null)
            {
                foreach (UnlockableDef unlockable in unlockableDefs)
                {
                    if (unlockable == unlockableDef)
                    {
                        hasNumber++;
                    }
                }
                if (hasNumber == unlockableDefs.Length - 1)
                {
                    base.Grant();
                }
            }
        }
    }
}