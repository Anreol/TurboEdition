using RoR2;
using RoR2.Achievements;

namespace TurboEdition.Achievements
{
    [RegisterAchievement("GrenadierPostDeathKill", null, null, typeof(GrenadierPostDeathKillAchievement.GrenadierPostDeathKillServerAchievement))]
    public class GrenadierPostDeathKillAchievement : BaseAchievement
    {
        public override BodyIndex LookUpRequiredBodyIndex()
        {
            return BodyCatalog.FindBodyIndex("GrenadierBody");
        }
        
        public override void OnBodyRequirementMet()
        {
            base.OnBodyRequirementMet();
            base.SetServerTracked(true);
        }
        public override void OnBodyRequirementBroken()
        {
            base.SetServerTracked(false);
            base.OnBodyRequirementBroken();
        }

		private class GrenadierPostDeathKillServerAchievement : BaseServerAchievement
		{
			public override void OnInstall()
			{
				base.OnInstall();
                EntityStates.Grenadier.DeathState.onPostDeathExplosionServer += onPostDeathExplosionServer;
			}

            private void onPostDeathExplosionServer(BlastAttack.Result results)
            {
                foreach (var hitPoint in results.hitPoints)
                {
                    if (!hitPoint.hurtBox.healthComponent.alive)
                    {
                        base.Grant();
                        return;
                    }
                }
            }

            public override void OnUninstall()
			{
                EntityStates.Grenadier.DeathState.onPostDeathExplosionServer -= onPostDeathExplosionServer;
                base.OnUninstall();
			}


		}

	}
}