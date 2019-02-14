using ICSharpCode.SharpZipLib.Zip;
using ShanghaiWindy.Core;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace ShanghaiWindy.Editor
{
    [CustomEditor(typeof(ModPackageData))]
    [CanEditMultipleObjects]
    public class ModPackageDataEditor : EditorWindowBase
    {
        public override void OnInspectorGUI()
        {

            base.OnInspectorGUI();

            var modPackData = target as ModPackageData;

            modPackData.buildTarget = EditorUserBuildSettings.activeBuildTarget.ToString();

            if (GUILayout.Button("Add Related Assets"))
            {
                var filePath = Application.dataPath;

                if (modPackData.relatedAssets.Count != 0)
                {
                    filePath = modPackData.relatedAssets[modPackData.relatedAssets.Count - 1];
                }

                modPackData.relatedAssets.Add(MakeRelative(EditorUtility.OpenFilePanel("Select Related File", filePath, null), Application.dataPath));
            }

            EditorGUILayout.HelpBox($"Current Build On:{EditorUserBuildSettings.activeBuildTarget.ToString()}   Tip: Press 'Ctrl + Shift + B' to change the current platform.", MessageType.None);

            if (GUILayout.Button("Package Mod Now"))
            {
                var buildMap = new AssetBundleBuild[1];
                buildMap[0].addressableNames = new string[]
                {
                    AssetDatabase.GetAssetPath(target)
                };
                buildMap[0].assetBundleName = modPackData.name;
                buildMap[0].assetBundleVariant = "modpackdata";
                buildMap[0].assetNames = new string[]
                {
                    AssetDatabase.GetAssetPath(target)
                };

                var buildDir = $"Build/Mod-Package/{modPackData.name}/Temp/";

                var modPackDir = $"Build/Mod-Package/{modPackData.name}/";

                var dir = new DirectoryInfo(buildDir);

                if (!dir.Exists)
                {
                    dir.Create();
                }

                BuildPipeline.BuildAssetBundles(buildDir, buildMap, BuildAssetBundleOptions.None, EditorUserBuildSettings.activeBuildTarget);

                foreach (var uselessAsset in dir.GetFiles("packages"))
                {
                    uselessAsset.Delete();
                }

                foreach (var relatedAsset in modPackData.relatedAssets)
                {
                    var file = new FileInfo(relatedAsset);

                    if (file.Exists)
                    {
                        File.Copy(file.FullName, buildDir + file.Name, true);
                    }
                }

                var zip = new FastZip();
                zip.CreateZip(modPackDir + modPackData.name + ".modpack", buildDir, false, null);

                EditorUtility.RevealInFinder(modPackDir + modPackData.modName + ".modpack");
            }



            if (GUI.changed)
            {
                EditorUtility.SetDirty(target);
            }
        }

        private string MakeRelative(string filePath, string referencePath)
        {
            var fileUri = new System.Uri(filePath);
            var referenceUri = new System.Uri(referencePath);
            return referenceUri.MakeRelativeUri(fileUri).ToString();
        }
    }
}