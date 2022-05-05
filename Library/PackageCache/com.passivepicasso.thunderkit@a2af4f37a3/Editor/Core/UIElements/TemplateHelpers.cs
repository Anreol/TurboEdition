﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ThunderKit.Common;
using UnityEditor;
using UnityEngine;
#if UNITY_2019_1_OR_NEWER
using UnityEngine.UIElements;
using UnityEditor.UIElements;
#else
using UnityEngine.Experimental.UIElements;
#endif

namespace ThunderKit.Core.UIElements
{
    public static class TemplateHelpers
    {
        private static bool IsTemplatePath(string path) => path.Contains("Packages/com.passivepicasso.thunderkit");

        private static readonly Dictionary<string, VisualTreeAsset> templateCache = new Dictionary<string, VisualTreeAsset>(StringComparer.Ordinal);

        public static string NicifyPackageName(string name) => ObjectNames.NicifyVariableName(name).Replace("_", " ");

        public static VisualElement GetTemplateInstance(string template, VisualElement target = null, Func<string, bool> isTemplatePath = null)
        {
            var packageTemplate = LoadTemplate(template, isTemplatePath);
            var templatePath = AssetDatabase.GetAssetPath(packageTemplate);
            if(packageTemplate == null)
            {
                Debug.LogError($"Could not find Template: {template}");
                return new Label("Could not find Template: {template}");
            }
            VisualElement instance = target;

#if UNITY_2020_1_OR_NEWER
            if (instance == null) instance = packageTemplate.Instantiate();
            else
                packageTemplate.CloneTree(instance);
#elif UNITY_2019_1_OR_NEWER
            if (instance == null) instance = packageTemplate.CloneTree();
            else
                packageTemplate.CloneTree(instance);
#elif UNITY_2018_1_OR_NEWER
            if (instance == null) instance = packageTemplate.CloneTree(null);
            else
                packageTemplate.CloneTree(instance, null);
#endif

            instance.AddToClassList("grow");

            instance.AddEnvironmentAwareSheets(templatePath);

            return instance;
        }

        const string editorVersion =
#if UNITY_2021_1_OR_NEWER
            "2021";
#elif UNITY_2020_1_OR_NEWER
            "2020";
#elif UNITY_2019_1_OR_NEWER
            "2019";
#elif UNITY_2018_1_OR_NEWER
            "2018";
#endif
        /// <summary>
        /// Selectively adds a set of predetermined style sheets based upon the environment
        /// Variations:
        /// templatePath
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="templatePath">Path to UXML template file</param>
        public static void AddEnvironmentAwareSheets(this VisualElement instance, string templatePath)
        {
            instance.AddSheet(templatePath);
            instance.AddSheet(templatePath, "_style");
            instance.AddSheet(templatePath, $"_{editorVersion}");

            if (EditorGUIUtility.isProSkin)
                instance.AddSheet(templatePath, "_Dark");
            else
                instance.AddSheet(templatePath, "_Light");
        }

        public static void AddSheet(this VisualElement element, string templatePath, string modifier = "")
        {
            string path = string.Empty;
            
            if (templatePath.EndsWith(".uxml"))
                path = templatePath.Replace(".uxml", $"{modifier}.uss");
            else if (templatePath.EndsWith(".uss"))
                path = templatePath.Replace(".uss", $"{modifier}.uss");

            if (!File.Exists(path))
                return;
            MultiVersionLoadStyleSheet(element, path);
        }

        public static void MultiVersionLoadStyleSheet(VisualElement element, string sheetPath)
        {
#if UNITY_2019_1_OR_NEWER
            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(sheetPath);
            if (!element.styleSheets.Contains(styleSheet))
                element.styleSheets.Add(styleSheet);
#elif UNITY_2018_1_OR_NEWER
            if (!element.HasStyleSheetPath(sheetPath))
                element.AddStyleSheetPath(sheetPath);
#endif
        }

        public static VisualTreeAsset LoadTemplate(string name, Func<string, bool> isTemplatePath = null)
        {
            if (templateCache.TryGetValue(name, out var asset) && asset != null)
                return asset;

            return templateCache[name] = CreateTemplate(name, isTemplatePath ?? IsTemplatePath);
        }

        static VisualTreeAsset CreateTemplate(string name, Func<string, bool> isTemplatePath)
        {
            var searchResults = AssetDatabase.FindAssets(name, Constants.FindAllFolders);
            var assetPaths = searchResults.Select(AssetDatabase.GUIDToAssetPath).Select(path => path.Replace("\\", "/"));
            var templatePath = assetPaths
                .Where(path => Path.GetFileNameWithoutExtension(path).Equals(name))
                .Where(path => Path.GetExtension(path).Equals(".uxml", StringComparison.CurrentCultureIgnoreCase))
                .Where(isTemplatePath)
                .FirstOrDefault();
            return AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(templatePath);
        }


        public static string GetAssetDirectory(UnityEngine.Object asset)
        {
            return Path.GetDirectoryName(AssetDatabase.GetAssetPath(asset));
        }

        public static VisualElement LoadTemplateInstance(string templatePath, VisualElement instance = null)
        {
            var visualTreeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(templatePath);
#if UNITY_2020_1_OR_NEWER
            if (instance == null) instance = visualTreeAsset.Instantiate();
            else
                visualTreeAsset.CloneTree(instance);
#elif UNITY_2019_1_OR_NEWER
            if (instance == null) instance = visualTreeAsset.CloneTree();
            else
                visualTreeAsset.CloneTree(instance);
#elif UNITY_2018_1_OR_NEWER
            if (instance == null) instance = visualTreeAsset.CloneTree(null);
            else
                visualTreeAsset.CloneTree(instance, null);
#endif
            return instance;
        }
    }
}