using UnityEditor;
using UnityEngine;

namespace Moonstorm.EditorUtils
{
    public class SearchAssetByGUID : EditorWindow
    {
        string guid = "";
        string path = "";
        [MenuItem("Tools/ThunderKit/GUIDToAssetPath")]
        static void CreateWindow()
        {
            SearchAssetByGUID window = (SearchAssetByGUID)EditorWindow.GetWindowWithRect(typeof(SearchAssetByGUID), new Rect(0, 0, 800, 120));
        }

        void OnGUI()
        {
            GUILayout.Label("Enter guid");
            guid = GUILayout.TextField(guid);
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Get Asset Path", GUILayout.Width(120)))
                path = GetAssetPath(guid);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Abort", GUILayout.Width(120)))
                Close();
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.Label(path);
            Debug.Log($"GUID Finder: Found asset of guid " + guid + " at address " + path);
        }
        static string GetAssetPath(string guid)
        {
            string p = AssetDatabase.GUIDToAssetPath(guid);
            Debug.Log(p);
            if (p.Length == 0) p = "not found";
            return p;
        }
    }
}
