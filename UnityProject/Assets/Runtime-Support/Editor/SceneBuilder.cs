using ShanghaiWindy.Core;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace ShanghaiWindy.Editor
{
    public class SceneBuilder : EditorWindow
    {
        [MenuItem("Tools/ShanghaiWindy/Build/SceneBuilder")]
        static void Init()
        {
            Rect wr = new Rect(0, 0, 250, 500);
            SceneBuilder window = (SceneBuilder)EditorWindow.GetWindowWithRect(typeof(SceneBuilder), wr, false, "SceneBuilder");
            window.Show();

            LoadSceneData();
        }

        public static List<SceneData> AllSceneData = new List<SceneData>();
        public static string SceneDataNames = null;

        void OnGUI()
        {
            EditorGUILayout.HelpBox("AssetBundle Build Manager ", MessageType.None);

            EditorGUILayout.HelpBox(string.Format("Scene Data Count: {0} Maps:{1}", AllSceneData.Count.ToString(), SceneDataNames), MessageType.None);

            if (GUILayout.Button("Re-Generate Vehicle Prefabs[Modder Do Not Click!]"))
            {
                var assetGUIDs = AssetDatabase.FindAssets("VehicleData");
                foreach (var guid in assetGUIDs)
                {
                    var path = AssetDatabase.GUIDToAssetPath(guid);
                    var vehicleData = AssetDatabase.LoadAssetAtPath<VehicleData>(path);
                }
            }

            if (GUILayout.Button("Label-Cooked-Scene-Assets[Modder Do Not Click!]"))
            {
                LoadSceneData();

                List<GameObject> AllCurrentAssets = new List<GameObject>();

                List<string> AllCurrentAssetGUID = new List<string>();

                foreach (SceneData sceneData in AllSceneData)
                {
                    foreach (GameObject sceneDataObjects in sceneData.SceneObjectReferences)
                    {
                        AllCurrentAssets.Add(sceneDataObjects);
                    }
                }

                foreach (GameObject CurrentAsset in AllCurrentAssets)
                {
                    string Path = AssetDatabase.GetAssetPath(CurrentAsset);

                    AssetImporter assetImporter = AssetImporter.GetAtPath(Path);

                    string AssetPathToGUID = AssetNameCorretor(AssetDatabase.AssetPathToGUID(Path));

                    AllCurrentAssetGUID.Add(AssetPathToGUID);

                    Debug.Log(CurrentAsset.name);

                    if (assetImporter.assetBundleName != AssetPathToGUID || assetImporter.assetBundleVariant != "clientsceneobject")
                    {
                        assetImporter.SetAssetBundleNameAndVariant(AssetPathToGUID, "clientsceneobject");
                    }

                    //PlayerPrefs.SetString(AssetPathToGUID, assetImporter.assetTimeStamp.ToString());


                    //    GameObject Prefab = Instantiate(CurrentAsset);

                    //    #region 服务器打包文件
                    //    GameObject ClientAsset = PrefabUtility.CreatePrefab("Assets/res/Cooked/" + CurrentAsset.name + ".prefab", Prefab);
                    //    assetImporter = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(ClientAsset));
                    //    if (assetImporter.assetBundleName != AssetPathToGUID) {
                    //        assetImporter.SetAssetBundleNameAndVariant(AssetPathToGUID, "clientsceneobject");
                    //    }

                    //    #endregion

                    //    #region 服务器资源处理


                    //    foreach (MeshRenderer meshRenderer in Prefab.GetComponentsInChildren<MeshRenderer>()) {
                    //        DestroyImmediate(meshRenderer);
                    //    }
                    //    foreach (MeshFilter mesh in Prefab.GetComponentsInChildren<MeshFilter>()) {
                    //        DestroyImmediate(mesh);
                    //    }


                    //    #region 服务器打包文件
                    //    GameObject DelicateAsset = PrefabUtility.CreatePrefab("Assets/res/Cooked/DelicatedServer/" + CurrentAsset.name + ".prefab", Prefab);
                    //    assetImporter = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(DelicateAsset));
                    //    if (assetImporter.assetBundleName != AssetPathToGUID) {
                    //        assetImporter.SetAssetBundleNameAndVariant(AssetPathToGUID, "mastersceneobject");
                    //    }
                    //    #endregion

                    //    DestroyImmediate(Prefab);
                    //    #endregion

                }

                //删除没有用的 sceneobject
                string[] AssetBundleNames = AssetDatabase.GetAllAssetBundleNames();

                foreach (string AssetBundleName in AssetBundleNames)
                {
                    string[] AllAssetBundlePaths = AssetDatabase.GetAssetPathsFromAssetBundle(AssetBundleName);
                    foreach (string AssetBundlePath in AllAssetBundlePaths)
                    {
                        GameObject PreviousAsset = AssetDatabase.LoadAssetAtPath(AssetBundlePath, typeof(GameObject)) as GameObject;

                        AssetImporter assetImporter = AssetImporter.GetAtPath(AssetBundlePath);

                        string AssetPathToGUID = assetImporter.assetBundleName;

                        if (!AllCurrentAssetGUID.Contains(AssetPathToGUID))
                        {
                            if (!assetImporter.assetBundleVariant.Contains("sceneobject"))
                            {
                                continue;
                            }
                            Debug.Log(AssetBundlePath);
                            assetImporter.assetBundleName = null;
                            //assetImporter.assetBundleVariant = null;
                            assetImporter.SaveAndReimport();



                        }
                    }
                }

            }

            GUILayout.Space(50);

            EditorGUILayout.HelpBox($"Current Build On:{EditorUserBuildSettings.activeBuildTarget.ToString()}   Tip: Press 'Ctrl + Shift + B' to change the current platform.", MessageType.None);

            if (GUILayout.Button("Build AssetBundle[Modder Click!]"))
            {
                if (!EditorUtility.DisplayDialog("[Important]Comfirm", "Compile Asset on Target Platform:" + EditorUserBuildSettings.activeBuildTarget.ToString(), "Yes", "No"))
                {
                    return;
                }

                DirectoryInfo folder = new DirectoryInfo("Build/packages/");
                if (!folder.Exists)
                {
                    folder.Create();
                }

                AssetBundleManifest assetbundleMainifest = BuildPipeline.BuildAssetBundles("Build/packages/", BuildAssetBundleOptions.ChunkBasedCompression | BuildAssetBundleOptions.DeterministicAssetBundle, EditorUserBuildSettings.activeBuildTarget);
                Hashtable Info = new Hashtable();
                foreach (FileInfo file in folder.GetFiles("*.extramesh"))
                {
                    List<string> Details = new List<string>();
                    Details.Add(GetMD5HashFromFile(file.FullName));
                    Info.Add(file.Name, Details);
                }

                StreamWriter streamWriter = new StreamWriter(folder + "md5.checker");
                streamWriter.Write(EditorJsonUtility.ToJson(Info));
                streamWriter.Close();

                if (!EditorUtility.DisplayDialog("Copy", "Copy cooked assets to Streamassets Folder", "Yes", "No"))
                {
                    return;
                }
                DirectoryInfo dir = new DirectoryInfo("Build/packages/");
                string path = "Build/packages/";
                string Seach_Path = Application.streamingAssetsPath + "/TWRPackages";

                DirectoryInfo TargetDir = new DirectoryInfo(Application.streamingAssetsPath + "/TWRPackages");
                if (!TargetDir.Exists)
                {
                    TargetDir.Create();
                }

                AssetDatabase.RemoveUnusedAssetBundleNames();

                string[] allAssetBundleNames = AssetDatabase.GetAllAssetBundleNames();

                for (int i = 0; i < allAssetBundleNames.Length; i++)
                {
#if ClientCode
                    if (allAssetBundleNames[i].Contains("master"))
                    {
                        continue;
                    }
#else
					if (allAssetBundleNames[i].Contains ("client"))
						continue;
#endif

                    string source = path + "/" + allAssetBundleNames[i];

                    string target = Seach_Path + "/" + allAssetBundleNames[i];

                    if (!File.Exists(target) || (GetMD5HashFromFile(target) != GetMD5HashFromFile(source)))
                    {
                        File.Copy(source, target, true);
                    }
                }

                File.Copy(path + "/packages", Seach_Path + "/packages", true);

            }
        }

        public static void LoadSceneData()
        {
            AllSceneData = new List<SceneData>();

            string[] AllSceneDataAssetsInfo = Directory.GetFiles("Assets/Cooks/Map", "*.asset");
            foreach (string SceneDataAssetPath in AllSceneDataAssetsInfo)
            {
                SceneData sceneData = AssetDatabase.LoadAssetAtPath(SceneDataAssetPath, typeof(SceneData)) as SceneData;
                if (!AllSceneData.Contains(sceneData))
                {
                    SceneDataNames += SceneDataAssetPath + "\n";
                    AllSceneData.Add(sceneData);
                }

            }


            foreach (SceneData sceneData in AllSceneData)
            {
                Debug.Log(sceneData.SceneObjectReferences.Length);
            }
        }

        public static string AssetNameCorretor(string str)
        {
            return str.Replace(" ", "").ToLower();

        }

        public static string GetMD5HashFromFile(string fileName)
        {
            try
            {
                FileStream file = new FileStream(fileName, FileMode.Open);
                System.Security.Cryptography.MD5 md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
                byte[] retVal = md5.ComputeHash(file);
                file.Close();
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < retVal.Length; i++)
                {
                    sb.Append(retVal[i].ToString("x2"));
                }
                return sb.ToString();
            }
            catch (System.Exception ex)
            {
                Debug.Log(ex.Source);
                Debug.Log(ex.Message);
                Debug.Log(ex.StackTrace);

                return null;
            }
        }
    }
}
