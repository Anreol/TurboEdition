using RoR2;
using RoR2.Achievements;

namespace TurboEdition.Achievements
{
    //Achievements MUST be public classes
    [RegisterAchievement("PlayerKillSelf", "Achievement.PlayerKillSelf", "Die5Times", null)]
    public class PlayerKillSelf : BaseAchievement
    {
        public override void OnInstall()
        {
            base.OnInstall();
            GlobalEventManager.onCharacterDeathGlobal += GlobalEventManager_onCharacterDeathGlobal;
        }

        public override void OnUninstall()
        {
            base.OnUninstall();
            GlobalEventManager.onCharacterDeathGlobal -= GlobalEventManager_onCharacterDeathGlobal;
        }

        //This is on character death global... might be finnicky in multiplayer. Check if other players killing themselves unlocks other player's achievements.
        //It shouldn't if we use localUser, and this achievement isnt networked, but still.
        private void GlobalEventManager_onCharacterDeathGlobal(DamageReport obj)
        {
            //There's also base.isUserAlive

            if (obj.victimMaster != null)
            {
                //Death by attack
                if (obj.attackerMaster != null)
                {
                    if (obj.victimMaster == localUser.cachedMaster && obj.victimMaster == localUser.cachedMaster)
                    {
                        Grant();
                        return;
                    }
                }

                //Additional to make the achievement easier to unlock: Death by fall damage
                if (obj.isFallDamage && obj.victimMaster == localUser.cachedMaster)
                {
                    Grant();
                }
            }
            //Would be funny to check umbras. No idea how.
        }
    }
}