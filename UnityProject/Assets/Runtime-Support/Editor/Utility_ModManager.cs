using ShanghaiWindy.Core;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace ShanghaiWindy.Editor
{
    public class Utility_ModManager : EditorWindow
    {
        private string modpackName = "DefaultModPack";

        private bool isContainAssetBundle = true;

        [MenuItem("Mod/Mod Manager")]
        static void Init()
        {
            var win = EditorWindow.GetWindow(typeof(Utility_ModManager));
            win.titleContent.text = "Mod Manager";
        }

        private void OnGUI()
        {
            EditorGUILayout.HelpBox("Mod Manager. The utility for easy mod.", MessageType.None, true);

            GUILayout.Space(15);

            EditorGUILayout.HelpBox("Create Vehicle", MessageType.None, true);

            if (GUILayout.Button("Open Create Vehicle"))
            {
                GetWindow<Utility_CreateDefaultVehicleAssets>();
            }

            GUILayout.Space(15);

            EditorGUILayout.HelpBox("Mod Package", MessageType.None, true);

            EditorGUILayout.LabelField("Mod Package Name:");
            modpackName = EditorGUILayout.TextField(modpackName);
            isContainAssetBundle = EditorGUILayout.Toggle("Contain AssetBundle", isContainAssetBundle);

            EditorGUILayout.HelpBox("If you are creating a scripting mod. Then,you shouldn't toggle on the Contain AssetBundle", MessageType.None, true);

            if (GUILayout.Button("Create Mod Package"))
            {
                var dir = new DirectoryInfo("Assets/ModManager");

                if (!dir.Exists)
                {
                    dir.Create();
                }

                if (isContainAssetBundle)
                {
                    var buildPipline = CreateInstance<ModPackageBuildPiplineData>();
                    var modPackage = CreateInstance<ModPackageData>();

                    AssetDatabase.CreateAsset(buildPipline, $"Assets/ModManager/BuildPipline-{modpackName}.asset");
                    AssetDatabase.CreateAsset(modPackage, $"Assets/ModManager/{modpackName}.asset");

                    buildPipline.linkedModPackage = modPackage;

                    EditorGUIUtility.PingObject(buildPipline);
                }
                else
                {
                    var modPackage = CreateInstance<ModPackageData>();
                    AssetDatabase.CreateAsset(modPackage, $"Assets/ModManager/{modpackName}.asset");

                    EditorGUIUtility.PingObject(modPackage);
                }

                EditorUtility.DisplayDialog("Success", "File Created", "OK");
            }

            GUILayout.Space(100);

            if (GUILayout.Button("Build Mod Piplines"))
            {
                AssetDatabase.Refresh();

                var guidList = AssetDatabase.FindAssets($"t:{typeof(ModPackageBuildPiplineData)}");

                foreach (var guid in guidList)
                {
                    var path = AssetDatabase.GUIDToAssetPath(guid);

                    var modPackageBuildPiplineData = AssetDatabase.LoadAssetAtPath<ModPackageBuildPiplineData>(path);

                    if (modPackageBuildPiplineData != null)
                    {
                        ModPackageBuildPiplineDataEditor.BuildPipline(modPackageBuildPiplineData);
                    }
                }

                guidList = AssetDatabase.FindAssets($"t:{typeof(ModPackageData)}");

                foreach (var guid in guidList)
                {
                    var path = AssetDatabase.GUIDToAssetPath(guid);

                    var modPackageData = AssetDatabase.LoadAssetAtPath<ModPackageData>(path);

                    if (modPackageData != null)
                    {
                        ModPackageDataEditor.BuildPipline(modPackageData);
                    }
                }
            }
        }
    }


}
