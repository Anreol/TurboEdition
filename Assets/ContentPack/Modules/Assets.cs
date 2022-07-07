using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
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
            //Debug.Log("Remapping materials in assetbundle " + assetBundle.name);
            Material[] assetBundleMaterials = assetBundle.LoadAllAssets<Material>();

            for (int i = 0; i < assetBundleMaterials.Length; i++)
            {
                //Debug.Log("mat: " + assetBundleMaterials[i] + " with shader " + assetBundleMaterials[i].shader + " is stubbed " + (bool)assetBundleMaterials[i].shader.name.Contains("Stubbed"));
                if (assetBundleMaterials[i].shader.name.Contains("Stubbed"))
                {
                    try
                    {
                        SwapShader(assetBundleMaterials[i]);
                        //Debug.Log("Swapped: " + assetBundleMaterials[i].shader);
                    }
                    catch (Exception ex)
                    {
                        TELog.LogE($"Failed to swap shader of material {assetBundleMaterials[i]}: {ex}", true);
                    }
                }
            }
        }

        private static async void SwapShader(Material material)
        {
            var shaderName = material.shader.name.Substring("Stubbed".Length);
            var adressablePath = $"{shaderName}.shader";
            var asyncOp = Addressables.LoadAssetAsync<Shader>(adressablePath);
            var shaderTask = asyncOp.Task;
            var shader = await shaderTask;
            material.shader = shader;
            MaterialsWithSwappedShaders.Add(material);
        }
    }
}