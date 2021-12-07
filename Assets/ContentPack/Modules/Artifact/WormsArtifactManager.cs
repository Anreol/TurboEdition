using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TurboEdition.Artifacts
{
    class WormsArtifactManager
    {
        [SystemInitializer(new Type[]
        {
            typeof(ArtifactCatalog)
        })]
        private static void Init()
        {
            RunArtifactManager.onArtifactEnabledGlobal += RunArtifactManager_onArtifactEnabledGlobal;
            RunArtifactManager.onArtifactDisabledGlobal += RunArtifactManager_onArtifactDisabledGlobal;
        }
        public static ArtifactDef artifact = Assets.mainAssetBundle.LoadAsset<ArtifactDef>("WormsArtifact");
        public static bool honor = false;
        private static void RunArtifactManager_onArtifactDisabledGlobal([JetBrains.Annotations.NotNull] RunArtifactManager runArtifactManager, [JetBrains.Annotations.NotNull] ArtifactDef artifactDef)
        {
            if (artifactDef == RoR2Content.Artifacts.EliteOnly)
            {
                honor = false;
                return;
            }
            if (artifactDef != artifact)
            {
                return;
            }
        }

        private static void RunArtifactManager_onArtifactEnabledGlobal([JetBrains.Annotations.NotNull] RunArtifactManager runArtifactManager, [JetBrains.Annotations.NotNull] ArtifactDef artifactDef)
        {
            throw new NotImplementedException();
        }
    }
}
