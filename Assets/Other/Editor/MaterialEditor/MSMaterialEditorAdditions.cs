﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace Moonstorm.EditorUtils
{
    [InitializeOnLoad]
    public class MSMaterialEditorAdditions
    {
        static MSMaterialEditorAdditions()
        {
            Editor.finishedDefaultHeaderGUI += Draw;
        }

        private static void Draw(Editor obj)
        {
            if (!(obj is MaterialEditor materialEditor))
            {
                return;
            }

            var id = GUIUtility.GetControlID(new GUIContent("Pick shader asset"), FocusType.Passive);

            if ((materialEditor.target as Material).shader.name.StartsWith("StubbedShader"))
            {
                if (GUILayout.Button("Upgrade to Real Shader"))
                {
                    MaterialUpgrader.Upgrade((Material)materialEditor.target);
                }
            }

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
}
