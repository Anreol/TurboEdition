﻿using RoR2EditorKit;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThunderKit.Core.Attributes;
using ThunderKit.Core.Data;
using ThunderKit.Core.Manifests.Datums;
using ThunderKit.Core.Paths;
using ThunderKit.Core.Pipelines;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;
using UnityEngine.Networking;
using Object = UnityEngine.Object;

namespace Moonstorm.EditorUtils.Pipelines
{
    [PipelineSupport(typeof(Pipeline)), RequiresManifestDatumType(typeof(AssetBundleDefinitions))]
    public class SwapShadersAndStageAssetBundles : PipelineJob
    {
        [EnumFlag]
        public BuildAssetBundleOptions AssetBundleBuildOptions = BuildAssetBundleOptions.UncompressedAssetBundle;
        public BuildTarget buildTarget = BuildTarget.StandaloneWindows;
        public bool recurseDirectories;
        public bool simulate;

        [PathReferenceResolver]
        public string BundleArtifactPath = "<AssetBundleStaging>";
        public override Task Execute(Pipeline pipeline)
        {
            var excludedExtensions = new[] { ".dll", ".cs", ".meta" };

            AssetDatabase.SaveAssets();

            var manifests = pipeline.Manifests;
            var assetBundleDefIndices = new Dictionary<AssetBundleDefinitions, int>();
            var assetBundleDefinitions = new List<AssetBundleDefinitions>();

            for (int i = 0; i < manifests.Length; i++)
            {
                foreach (var abd in manifests[i].Data.OfType<AssetBundleDefinitions>())
                {
                    assetBundleDefinitions.Add(abd);
                    assetBundleDefIndices.Add(abd, i);
                }
            }

            var assetBundleDefs = assetBundleDefinitions.ToArray();
            var hasValidBundles = assetBundleDefs.Any(abd => abd.assetBundles.Any(ab => !string.IsNullOrEmpty(ab.assetBundleName) && ab.assets.Any()));

            if (!hasValidBundles)
            {
                var scriptPath = UnityWebRequest.EscapeURL(AssetDatabase.GetAssetPath(MonoScript.FromScriptableObject(this)));
                pipeline.Log(LogLevel.Warning, $"No valid AssetBundleDefinitions defined, skipping [{nameof(SwapShadersAndStageAssetBundles)}](assetLink://{scriptPath}) Pipeline Job");
                return Task.CompletedTask;
            }

            var bundleArtifactPath = BundleArtifactPath.Resolve(pipeline, this);
            Directory.CreateDirectory(bundleArtifactPath);


            var materials = GetAllMaterialsWithRealShaders();

            if (materials.Length != 0)
            {
                var count = SwapRealShadersForStubbed(materials);
                pipeline.Log(LogLevel.Information, $"Replacing a total of {count} real shaders for stubbed shaders.");
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            var explicitAssets = assetBundleDefs.SelectMany(abd => abd.assetBundles)
                                                .SelectMany(ab => ab.assets)
                                                .ToArray();

            var explicitAssetPaths = new List<string>();
            PopulateWithExplicitAssets(explicitAssets, explicitAssetPaths);

            var defBuildDetails = new List<string>();
            var logBuilder = new StringBuilder();
            var builds = new AssetBundleBuild[assetBundleDefs.Sum(abd => abd.assetBundles.Length)];

            var buildsIndex = 0;
            for (int defIndex = 0; defIndex < assetBundleDefs.Length; defIndex++)
            {
                var assetBundleDef = assetBundleDefs[defIndex];
                var playerAssemblies = CompilationPipeline.GetAssemblies();
                var assemblyFiles = playerAssemblies.Select(pa => pa.outputPath).ToArray();
                var sourceFiles = playerAssemblies.SelectMany(pa => pa.sourceFiles).ToArray();

                defBuildDetails.Clear();

                for (int i = 0; i < assetBundleDef.assetBundles.Length; i++)
                {
                    var def = assetBundleDef.assetBundles[i];

                    var build = builds[buildsIndex];

                    var assets = new List<string>();

                    var firstAsset = def.assets.FirstOrDefault(x => x is SceneAsset);

                    if (firstAsset != null) assets.Add(AssetDatabase.GetAssetPath(firstAsset));
                    else
                    {
                        PopulateWithExplicitAssets(def.assets, assets);

                        var dependencies = assets
                            .SelectMany(assetPath => AssetDatabase.GetDependencies(assetPath))
                            .Where(dap => !explicitAssetPaths.Contains(dap))
                            .ToArray();

                        assets.AddRange(dependencies);
                    }

                    build.assetNames = assets
                        .Select(ap => ap.Replace("\\", "/"))
                        .Where(dap => !ArrayUtility.Contains(excludedExtensions, Path.GetExtension(dap)) &&
                                      !ArrayUtility.Contains(sourceFiles, dap) &&
                                      !ArrayUtility.Contains(assemblyFiles, dap) &&
                                      !AssetDatabase.IsValidFolder(dap))
                        .Distinct()
                        .ToArray();
                    build.assetBundleName = def.assetBundleName;
                    builds[buildsIndex] = build;
                    buildsIndex++;

                    LogBundleDetails(logBuilder, build);

                    defBuildDetails.Add(logBuilder.ToString());
                    logBuilder.Clear();
                }

                var prevInd = pipeline.ManifestIndex;
                pipeline.ManifestIndex = assetBundleDefIndices[assetBundleDef];
                pipeline.Log(LogLevel.Information, $"Creating {assetBundleDef.assetBundles.Length} AssetBundles", defBuildDetails.ToArray());
                pipeline.ManifestIndex = prevInd;
            }

            if (!simulate)
            {
                BuildPipeline.BuildAssetBundles(bundleArtifactPath, builds, AssetBundleBuildOptions, buildTarget);
                for (pipeline.ManifestIndex = 0; pipeline.ManifestIndex < pipeline.Manifests.Length; pipeline.ManifestIndex++)
                {
                    var manifest = pipeline.Manifest;
                    foreach (var assetBundleDef in manifest.Data.OfType<AssetBundleDefinitions>())
                    {
                        var bundleNames = assetBundleDef.assetBundles.Select(ab => ab.assetBundleName).ToArray();
                        foreach (var outputPath in assetBundleDef.StagingPaths.Select(path => path.Resolve(pipeline, this)))
                        {
                            foreach (string dirPath in Directory.GetDirectories(bundleArtifactPath, "*", SearchOption.AllDirectories))
                                Directory.CreateDirectory(dirPath.Replace(bundleArtifactPath, outputPath));

                            foreach (string filePath in Directory.GetFiles(bundleArtifactPath, "*", SearchOption.AllDirectories))
                            {
                                bool found = false;
                                foreach (var bundleName in bundleNames)
                                {
                                    if (filePath.IndexOf(bundleName, System.StringComparison.OrdinalIgnoreCase) >= 0)
                                    {
                                        found = true;
                                        break;
                                    }
                                }
                                if (!found) continue;
                                string destFileName = filePath.Replace(bundleArtifactPath, outputPath);
                                Directory.CreateDirectory(Path.GetDirectoryName(destFileName));
                                FileUtil.ReplaceFile(filePath, destFileName);

                            }

                            var manifestSource = Path.Combine(bundleArtifactPath, $"{Path.GetFileName(bundleArtifactPath)}.manifest");
                            var manifestDestination = Path.Combine(outputPath, $"{manifest.Identity.Name}.manifest");
                            FileUtil.ReplaceFile(manifestSource, manifestDestination);
                        }
                    }
                }
                pipeline.ManifestIndex = -1;
            }

            materials = GetAllMaterialsWithStubbedShaders();

            if (materials.Length != 0)
            {
                var count = RestoreMaterialShaders(materials);
                pipeline.Log(LogLevel.Information, $"Restored a total of {count} stubbed shaders for real shaders.");
            }
            return Task.CompletedTask;
        }

        private static void LogBundleDetails(StringBuilder logBuilder, AssetBundleBuild build)
        {
            logBuilder.AppendLine($"{build.assetBundleName}");
            foreach (var asset in build.assetNames)
            {
                var name = Path.GetFileNameWithoutExtension(asset);
                if (name.Length == 0) continue;
                logBuilder.AppendLine($"[{name}](assetlink://{UnityWebRequest.EscapeURL(asset)})");
                logBuilder.AppendLine();
            }

            logBuilder.AppendLine();
        }

        private Material[] GetAllMaterialsWithRealShaders()
        {
            List<Material> materials = new List<Material>();

            return materials.Union(Util.FindAssetsByType<Material>()
                                       .Where(material => material.shader.name.StartsWith("Hopoo Games")))
                            .Union(Util.FindAssetsByType<Material>()
                                       .Where(material => material.shader.name.StartsWith("CalmWater")))
                            .Union(Util.FindAssetsByType<Material>()
                                       .Where(material => material.shader.name.StartsWith("Decalicious")))
                            .ToArray();
        }

        private Material[] GetAllMaterialsWithStubbedShaders()
        {
            List<Material> materials = new List<Material>();

            return materials.Union(Util.FindAssetsByType<Material>()
                                       .Where(material => material.shader.name.StartsWith("StubbedShader")))
                            .Union(Util.FindAssetsByType<Material>()
                                       .Where(material => material.shader.name.StartsWith("StubbedCalmWater")))
                            .Union(Util.FindAssetsByType<Material>()
                                       .Where(material => material.shader.name.StartsWith("StubbedDecalicious")))
                            .ToArray();
        }
        private int SwapRealShadersForStubbed(Material[] materials)
        {
            var count = 0;
            for (int i = 0; i < materials.Length; i++)
            {
                var current = materials[i];
                if (ShaderSwapDictionary.realToStubbed.TryGetValue(current.shader, out Shader stubbed))
                {
                    count++;
                    current.shader = stubbed;
                }
            }
            AssetDatabase.SaveAssets();
            return count;
        }

        private static void PopulateWithExplicitAssets(IEnumerable<Object> inputAssets, List<string> outputAssets)
        {
            foreach (var asset in inputAssets)
            {
                var assetPath = AssetDatabase.GetAssetPath(asset);

                if (AssetDatabase.IsValidFolder(assetPath))
                {
                    if (asset.name == "Materials")
                        continue;

                    var files = Directory.GetFiles(assetPath, "*", SearchOption.AllDirectories);
                    var assets = files.Select(path => AssetDatabase.LoadAssetAtPath<Object>(path));
                    PopulateWithExplicitAssets(assets, outputAssets);
                }
                else if (asset is UnityPackage up)
                {
                    PopulateWithExplicitAssets(up.AssetFiles, outputAssets);
                }
                else
                {
                    outputAssets.Add(assetPath);
                }
            }
        }

        private static int RestoreMaterialShaders(Material[] materials)
        {
            var count = 0;
            for (int i = 0; i < materials.Length; i++)
            {
                var current = materials[i];
                if (ShaderSwapDictionary.stubbedToReal.TryGetValue(current.shader, out Shader stubbed))
                {
                    count++;
                    current.shader = stubbed;
                }
            }
            AssetDatabase.SaveAssets();
            return count;
        }
    }
}
