using ShanghaiWindy.Core;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace ShanghaiWindy.Editor
{
    [CustomEditor(typeof(ModPackageBuildPiplineData))]
    [CanEditMultipleObjects]
    public class ModPackageBuildPiplineDataEditor : EditorWindowBase
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            var buildData = target as ModPackageBuildPiplineData;

            EditorGUILayout.HelpBox($"Current Build On:{EditorUserBuildSettings.activeBuildTarget.ToString()}   Tip: Press 'Ctrl + Shift + B' to change the current platform.", MessageType.None);

            if (GUILayout.Button("Build Linked Objects to AssetBundle"))
            {
                List<AssetBundleBuild> buildMap = new List<AssetBundleBuild>();

                foreach (var o in buildData.linkedObjects)
                {
                    var assetPath = AssetDatabase.GetAssetPath(o);
                    var importer = AssetImporter.GetAtPath(assetPath);

                    if (importer == null)
                    {
                        continue;
                    }

                    var assetBundleName = importer.assetBundleName;
                    var assetBundleVariant = importer.assetBundleVariant;
                    var assetBundleFullName = string.IsNullOrEmpty(assetBundleVariant) ? assetBundleName : assetBundleName + "." + assetBundleVariant;

                    if (string.IsNullOrEmpty(assetBundleName) || string.IsNullOrEmpty(assetBundleVariant))
                    {
                        EditorUtility.DisplayDialog("Fetal Error", $"Asset:{o.name} has not set the assetbundle name and variant. Index in List: {buildData.linkedObjects.IndexOf(o)}", "OK");
                        return;
                    }

                    AssetBundleBuild build = new AssetBundleBuild
                    {
                        assetBundleName = assetBundleName,
                        assetBundleVariant = assetBundleVariant,
                        assetNames = AssetDatabase.GetAssetPathsFromAssetBundle(assetBundleFullName)
                    };

                    buildMap.Add(build);
                }

                var buildDir = $"Build/Mod-BuildPipline/{EditorUserBuildSettings.activeBuildTarget}/{buildData.name}/packages/";

                var dir = new DirectoryInfo(buildDir);

                if (!dir.Exists)
                {
                    dir.Create();
                }

                var manifest = BuildPipeline.BuildAssetBundles(buildDir, buildMap.ToArray(), BuildAssetBundleOptions.ChunkBasedCompression, EditorUserBuildSettings.activeBuildTarget);

                if (buildData.linkedModPackage != null)
                {
                    buildData.linkedModPackage.relatedAssets = new List<string>();
                }

                foreach (var file in dir.GetFiles("*"))
                {
                    if (buildData.linkedModPackage != null)
                    {
                        buildData.linkedModPackage.relatedAssets.Add(MakeRelative(file.FullName, Application.dataPath));
                    }
                }

                EditorUtility.RevealInFinder(buildDir);
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