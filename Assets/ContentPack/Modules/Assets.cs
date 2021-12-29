using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using TurboEdition.ScriptableObjects;
using UnityEngine;
using Path = System.IO.Path;

namespace TurboEdition
{
    public static class Assets
    {
        public static AssetBundle mainAssetBundle
        {
            get
            {
                return assetBundles[0];
            }
        }

        internal static string assemblyDir
        {
            get
            {
                return Path.GetDirectoryName(TurboEdition.pluginInfo.Location);
            }
        }

        private const string assetBundleFolderName = "assetbundles";
        internal static string mainAssetBundleName = "assetTurbo";
        public static ReadOnlyCollection<AssetBundle> assetBundles;

        [RoR2.SystemInitializer] //look at putting it in FinalizeAsync
        public static void Init()
        {
            if (RoR2.RoR2Application.isDedicatedServer || Application.isBatchMode) //We dont need graphics
                return;
            var gameMaterials = Resources.FindObjectsOfTypeAll<Material>();
            foreach (var assetBundle in assetBundles)
                MapMaterials(assetBundle, gameMaterials);
        }

        public static string[] GetAssetBundlePaths()
        {
            return Directory.GetFiles(Path.Combine(assemblyDir, assetBundleFolderName))
               .Where(filePath => !filePath.EndsWith(".manifest"))
               .OrderByDescending(path => Path.GetFileName(path).Equals(mainAssetBundleName))
               .ToArray();
        }

        //This one is the one for actually loading effects into the contentpack
        internal static void LoadEffects()
        {
            var effectHolders = mainAssetBundle.LoadAllAssets<EffectDefHolder>();
            foreach (var effectHolder in effectHolders)
                foreach (var effectPrefab in effectHolder.effectPrefabs)
                    HG.ArrayUtils.ArrayAppend(ref TurboEdition.serializableContentPack.effectDefs, EffectDefHolder.ToEffectDef(effectPrefab));
        }

        internal static void MapMaterials(AssetBundle assetBundle, Material[] gameMaterials)
        {
            if (assetBundle.isStreamedSceneAssetBundle)
                return;
            //The absolute fucking state of having to do shaders in RoR2
            //SHADERS NEVER FUCKING EVER
            var cloudMat = Resources.Load<GameObject>("Prefabs/Effects/OrbEffects/LightningStrikeOrbEffect").transform.Find("Ring").GetComponent<ParticleSystemRenderer>().material;

            Material[] assetBundleMaterials = assetBundle.LoadAllAssets<Material>();

            for (int i = 0; i < assetBundleMaterials.Length; i++)
            {
                var material = assetBundleMaterials[i];
                // If it's stubbed, just switch out the shader unless it's fucking cloudremap
                if (material.shader.name.StartsWith("StubbedShader"))
                {
                    material.shader = Resources.Load<Shader>("shaders" + material.shader.name.Substring(13));
                    if (material.shader.name.Contains("Cloud Remap"))
                    {
                        var cockSucker = new RuntimeCloudMaterialMapper(material);
                        material.CopyPropertiesFromMaterial(cloudMat);
                        cockSucker.SetMaterialValues(ref material);
                    }
                }

                //If it's this shader it searches for a material with the same name and copies the properties
                if (material.shader.name.Equals("CopyFromRoR2"))
                {
                    foreach (var gameMaterial in gameMaterials)
                        if (material.name.Equals(gameMaterial.name))
                        {
                            material.shader = gameMaterial.shader;
                            material.CopyPropertiesFromMaterial(gameMaterial);
                            break;
                        }
                }
                assetBundleMaterials[i] = material;
            }
        }
    }
}