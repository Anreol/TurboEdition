using RoR2;
using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using static RoR2.InfiniteTowerWaveCategory;

namespace TurboEdition.Misc
{
    internal class InfiniteTowerInjector
    {
        [SystemInitializer(new Type[]
         {
            typeof(GameModeCatalog),
        })]
        public static void Init()
        {
            RoR2.InfiniteTowerWaveCategory infiniteTowerWaveCategory = Addressables.LoadAssetAsync<RoR2.InfiniteTowerWaveCategory>("RoR2/DLC1/GameModes/InfiniteTowerRun/InfiniteTowerAssets/InfiniteTowerWaveCategories/CommonWaveCategory.asset").WaitForCompletion();
            WeightedWave spite2Wave = new WeightedWave(){
                prerequisites = Assets.mainAssetBundle.LoadAsset<InfiniteTowerWavePrerequisites>("ArtifactsSpite2DisabledPrerequisite"),
                wavePrefab = Assets.mainAssetBundle.LoadAsset<GameObject>("InfiniteTowerWaveArtifactSpite2"),
                weight = 1
            };

            HG.ArrayUtils.ArrayAppend<WeightedWave>(ref infiniteTowerWaveCategory.wavePrefabs, spite2Wave);
        }
    }
}