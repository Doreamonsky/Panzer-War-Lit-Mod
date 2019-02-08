using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using ShanghaiWindy.Core;

namespace ShanghaiWindy.Editor
{
    public class Utility_Localization : EditorWindow
    {
        static uGUI_Localsize[] data;

        [MenuItem("Tools/Localization")]
        static void Init()
        {
            EditorWindow.GetWindow(typeof(Utility_Localization));

            FindData();
        }

        private static void FindData()
        {
            data = GameObject.FindObjectsOfType<uGUI_Localsize>();
        }

        private void OnGUI()
        {
            if (GUILayout.Button("Refresh"))
            {
                FindData();
            }

            foreach (var d in data)
            {
                EditorGUILayout.TextArea(d.Key);
            }
        }
    }
}