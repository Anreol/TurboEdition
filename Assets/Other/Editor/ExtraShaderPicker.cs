using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class ExtraShaderPicker
{
    static ExtraShaderPicker()
    {
        Editor.finishedDefaultHeaderGUI += EditorFinishedDefaultHeaderGUI;
    }

    private static void EditorFinishedDefaultHeaderGUI(Editor obj)
    {
        if (!(obj is MaterialEditor materialEditor))
        {
            return;
        }
        var id = GUIUtility.GetControlID(new GUIContent("Pick shader asset"), FocusType.Passive);
        
        if (GUILayout.Button("Pick shader asset"))
        {
            EditorGUIUtility.ShowObjectPicker<Shader>(null, false, null, id);
        }

        if (Event.current.commandName == "ObjectSelectorUpdated" && EditorGUIUtility.GetObjectPickerControlID() == id)
        {
            materialEditor.SetShader(EditorGUIUtility.GetObjectPickerObject() as Shader, true);
        }
    }
}
