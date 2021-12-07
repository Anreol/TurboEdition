using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Moonstorm.EditorUtils
{
    [InitializeOnLoad]
    public static class HopooShaderDictionary
    {
        public static Dictionary<Shader, Shader> realToStubbed = new Dictionary<Shader, Shader>();
        public static Dictionary<Shader, Shader> stubbedToReal = new Dictionary<Shader, Shader>();

        static HopooShaderDictionary()
        {
            PopulateDictionary(realToStubbed, true);
            PopulateDictionary(stubbedToReal, false);
            if (realToStubbed.Count == 0 || stubbedToReal.Count == 0)
                Debug.Log($"The Hopoo Shaders dictionary got populated with zero entries, there might be an issue.");
            else
                Debug.Log("Populated Hopoo Shader dictionary.");
        }

        public static void PopulateDictionary(Dictionary<Shader, Shader> shaderDictionary, bool realToStubbed)
        {
            var allShadersInAssets = (List<Shader>)Util.FindAssetsByType<Shader>("hg");

            for (int i = 0; i < allShadersInAssets.Count; i++)
            {
                var current = allShadersInAssets[i];
                Shader real;
                string realFileName;

                Shader stubbed;
                if (current.name.StartsWith("Hopoo Games"))
                {
                    real = current;
                    realFileName = Path.GetFileName(AssetDatabase.GetAssetPath(real)).Replace(".asset", string.Empty);

                    stubbed = allShadersInAssets.Where(shader => shader.name != real.name)
                                                       .Select(shader => AssetDatabase.GetAssetPath(shader))
                                                       .Where(path => path.Contains(".shader"))
                                                       .Where(path => path.Contains(realFileName))
                                                       .Select(path => AssetDatabase.LoadAssetAtPath<Shader>(path))
                                                       .First();

                    if (real && stubbed)
                    {
                        if (realToStubbed)
                            shaderDictionary.Add(real, stubbed);
                        else
                            shaderDictionary.Add(stubbed, real);
                    }
                }
            }
        }

        /*private static List<Shader> FindAllShaders()
        {
            List<Shader> shaders = new List<Shader>();
            string[] guids = AssetDatabase.FindAssets("hg t:Shader", null);
            for (int i = 0; i < guids.Length; i++)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guids[i]);
                Shader asset = AssetDatabase.LoadAssetAtPath<Shader>(assetPath);
                if (asset != null)
                {
                    shaders.Add(asset);
                }
            }
            return shaders;
        }*/
    }
}