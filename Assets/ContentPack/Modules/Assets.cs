using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Path = System.IO.Path;

namespace TurboEdition
{
    public static class Assets
    {
        public static AssetBundle mainAssetBundle => loadedAssetBundles[0];
        internal static string assemblyDir => Path.GetDirectoryName(TurboUnityPlugin.pluginInfo.Location);
        internal static string languageRoot => System.IO.Path.Combine(Assets.assemblyDir, "language");

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
            Debug.Log("Remapping materials");
            //var cloudRemapReference = Resources.Load<GameObject>("Prefabs/Effects/OrbEffects/LightningStrikeOrbEffect").transform.Find("Ring").GetComponent<ParticleSystemRenderer>().material;

            Material[] assetBundleMaterials = assetBundle.LoadAllAssets<Material>();
            //Material[] assetBundleMaterialObjects = Resources.FindObjectsOfTypeAll<Material>();

            for (int i = 0; i < assetBundleMaterials.Length; i++)
            {
                if (assetBundleMaterials[i].shader.name.Contains("Stubbed"))
                {
                    try
                    {
                        //Debug.Log("mat: " + assetBundleMaterials[i] + " with shader " + assetBundleMaterials[i].shader);
                        var fuck = SwapShader(assetBundleMaterials[i]);
                        //Debug.Log("Swapped: " + assetBundleMaterials[i].shader);
                    }
                    catch (Exception ex)
                    {
                        TELog.LogE($"Failed to swap shader of material {assetBundleMaterials[i]}: {ex}");
                    }
                }

                /*
                var material = assetBundleMaterials[i];
                if (material.shader.name.StartsWith("StubbedCalmWater"))
                {
                    material.shader = Addressables.LoadAssetAsync<Shader>("Calm Water/" + material.shader.name.Substring(7)).WaitForCompletion();
                    MaterialsWithSwappedShaders.Add(material);
                    continue;
                }
                if (material.shader.name.StartsWith("StubbedDecalicious"))
                {
                    Debug.Log(material.shader.name);
                    Debug.Log(material.shader.name.Substring(7));
                    material.shader = Addressables.LoadAssetAsync<Shader>("Decalicious/" + material.shader.name.Substring(8)).WaitForCompletion();
                    MaterialsWithSwappedShaders.Add(material);
                    continue;
                }
                // If it's stubbed, just switch out the shader unless it's fucking cloudremap
                if (material.shader.name.StartsWith("StubbedShader"))
                {
                    material.shader = Addressables.LoadAssetAsync<Shader>("RoR2/Base/Shaders/HG" + material.shader.name.Substring(13)).WaitForCompletion();
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
                }*/
            }
        }

        private static async Task SwapShader(Material material)
        {
            var shaderName = material.shader.name.Substring(7);
            var adressablePath = $"{shaderName}.shader";
            //Debug.Log("Shader name: " + shaderName + "\nAddressable Path: " + adressablePath);
            var asyncOp = Addressables.LoadAssetAsync<Shader>(adressablePath);
            Task<Shader> shaderTask = asyncOp.Task;
            Shader shader = await shaderTask;
            material.shader = shader;
            //Debug.Log("Finalized swapping, shader is: " + shader.name + " and the material shader is " + material.shader);
            MaterialsWithSwappedShaders.Add(material);
        }
    }
}