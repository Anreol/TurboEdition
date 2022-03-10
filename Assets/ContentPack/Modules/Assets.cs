using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using TurboEdition.Components;
using UnityEngine;
using Path = System.IO.Path;

namespace TurboEdition
{
    public static class Assets
    {
        public static AssetBundle mainAssetBundle => loadedAssetBundles[0];
        internal static string assemblyDir => Path.GetDirectoryName(TurboEdition.pluginInfo.Location);

        private const string assetBundleFolderName = "assetbundles";
        internal static string mainAssetBundleName = "assetTurbo";
        public static ReadOnlyCollection<AssetBundle> loadedAssetBundles;
        public static List<Material> MaterialsWithSwappedShaders { get; private set; } = new List<Material>();

        [RoR2.SystemInitializer] //look at putting it in FinalizeAsync
        public static void Init()
        {
            if (RoR2.RoR2Application.isDedicatedServer || Application.isBatchMode) //We dont need graphics
                return;
            foreach (var assetBundle in loadedAssetBundles)
            {
                MapMaterials(assetBundle);
            }
        }

        public static string[] GetAssetBundlePaths()
        {
            return Directory.GetFiles(Path.Combine(assemblyDir, assetBundleFolderName))
               .Where(filePath => !filePath.EndsWith(".manifest"))
               .OrderByDescending(path => Path.GetFileName(path).Equals(mainAssetBundleName))
               .ToArray();
        }

        internal static void MapMaterials(AssetBundle assetBundle)
        {
            if (assetBundle.isStreamedSceneAssetBundle)
                return;

            var cloudRemapReference = Resources.Load<GameObject>("Prefabs/Effects/OrbEffects/LightningStrikeOrbEffect").transform.Find("Ring").GetComponent<ParticleSystemRenderer>().material;

            Material[] assetBundleMaterials = assetBundle.LoadAllAssets<Material>();
            Material[] assetBundleMaterialObjects = Resources.FindObjectsOfTypeAll<Material>();

            for (int i = 0; i < assetBundleMaterials.Length; i++)
            {
                var material = assetBundleMaterials[i];
                if (material.shader.name.StartsWith("StubbedCalmWater"))
                {
                    material.shader = Shader.Find(material.shader.name.Substring(7));
                    MaterialsWithSwappedShaders.Add(material);
                    continue;
                }
                if (material.shader.name.StartsWith("StubbedDecalicious"))
                {
                    Debug.Log(material.shader.name);
                    Debug.Log(material.shader.name.Substring(7));
                    material.shader = Shader.Find(material.shader.name.Substring(8));
                    MaterialsWithSwappedShaders.Add(material);
                    continue;
                }
                // If it's stubbed, just switch out the shader unless it's fucking cloudremap
                if (material.shader.name.StartsWith("StubbedShader"))
                {
                    material.shader = Resources.Load<Shader>("shaders" + material.shader.name.Substring(13));
                    if (material.shader.name.Contains("Cloud Remap"))
                    {
                        var eatShit = new RuntimeCloudMaterialMapper(material);
                        material.CopyPropertiesFromMaterial(cloudRemapReference);
                        eatShit.SetMaterialValues(ref material);
                    }
                    MaterialsWithSwappedShaders.Add(material);
                    continue;
                }

                //If it's this shader it searches for a material with the same name and copies the properties
                if (material.shader.name.Equals("CopyFromRoR2"))
                {
                    foreach (var gameMaterial in assetBundleMaterialObjects)
                        if (material.name.Equals(gameMaterial.name))
                        {
                            material.shader = gameMaterial.shader;
                            material.CopyPropertiesFromMaterial(gameMaterial);
                            MaterialsWithSwappedShaders.Add(material);
                            break;
                        }
                    continue;
                }
            }
        }
    }
}