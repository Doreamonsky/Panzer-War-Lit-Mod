using System.IO;
using UnityEditor;
using UnityEngine;

using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace ShanghaiWindy
{
  
    public class SceneBuilder : EditorWindow
    {
        [MenuItem("Tools/ShanghaiWindy/AssetsBuilder")]
        static void Init()
        {
            Rect wr = new Rect(0, 0, 250, 500);
            SceneBuilder window = (SceneBuilder)EditorWindow.GetWindowWithRect(typeof(SceneBuilder), wr, false, "SceneBuilder");
            window.Show();

        }

        void OnGUI()
        {
            if(GUILayout.Button("Build Pipline")){
                if (EditorUtility.DisplayDialog("Warning!", "You are going to build assets as" + EditorUserBuildSettings.activeBuildTarget.ToString(), "OK", "Cancel"))
                {
                    BuildPipline();
                }
            }
     
        }

        private static void BuildPipline(System.Action beforeBuild = null, System.Action afterBuild = null)
        {
            beforeBuild?.Invoke();

            DirectoryInfo folder = new DirectoryInfo("Build/packages");

            if (!folder.Exists)
            {
                folder.Create();
            }

            BuildPipeline.BuildAssetBundles("Build/packages", BuildAssetBundleOptions.ChunkBasedCompression, EditorUserBuildSettings.activeBuildTarget);

            afterBuild?.Invoke();
        }



        public static string AssetNameCorretor(string str)
        {
            return str.Replace(" ", "").ToLower();

        }


    }

    public class ResourcesHelper : EditorWindow
    {
        [MenuItem("Tools/ShanghaiWindy/ResourcesHelper")]
        static void Init()
        {
            Rect wr = new Rect(0, 0, 250, 500);
            ResourcesHelper window = (ResourcesHelper)GetWindowWithRect(typeof(ResourcesHelper), wr, false, "ResourcesHelper");
            window.Show();

        }
        private void OnGUI()
        {
            if (GUILayout.Button("PickAssetBundle"))
            {
                //Fucking Unity We can only use manifest because the assets are case sensitive
                var manifestFile = EditorUtility.OpenFilePanel("Get file", "/", "manifest");

                var manifestName = Path.GetFileName(manifestFile).Split('.');

                var str = File.ReadAllText(manifestFile);

                var pattern = @"(Assets/[\s\S]*?)\n";

                foreach (Match match in Regex.Matches(str, pattern))
                {
                    var filePath = match.Value.Replace("\n", "");

                    Directory.CreateDirectory(Path.GetDirectoryName(filePath));

                    File.WriteAllText(filePath, "Fake Asset Replace with real asset plz!");

                    AssetDatabase.Refresh();

                    var importer = AssetImporter.GetAtPath(filePath);
                    importer.SetAssetBundleNameAndVariant(manifestName[0], manifestName[1]);
                    importer.SaveAndReimport();
                }
            }
        }
    }
}
