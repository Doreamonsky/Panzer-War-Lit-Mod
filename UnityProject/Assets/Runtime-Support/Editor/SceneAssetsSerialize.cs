using ShanghaiWindy.Core;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace ShanghaiWindy.Editor
{
    public class SceneAssetsSerialize : EditorWindow
    {
        [MenuItem("Tools/ShanghaiWindy/Build/SceneAssetsSerialize")]
        static void Init()
        {
            Rect wr = new Rect(0, 0, 250, 500);
            SceneAssetsSerialize window = (SceneAssetsSerialize)EditorWindow.GetWindowWithRect(typeof(SceneAssetsSerialize), wr, false, "SceneAssetsSerialize");
            window.Show();
        }
        void OnGUI()
        {
            EditorGUILayout.HelpBox("The tool will serialize scene  generating a new cooked scene and label scene assets  automatically.Then, you can use SceneBuilder tool to build these assets!", MessageType.Info);

            if (GUILayout.Button("Serialize Scene"))
            {
                string SceneOriginalName = EditorSceneManager.GetActiveScene().name;
                #region Save As New Scene
                EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), "Assets/Res/Cooked/Scene/" + SceneOriginalName + "_Cooked.unity", true);
                EditorSceneManager.OpenScene("Assets/Res/Cooked/Scene/" + SceneOriginalName + "_Cooked.unity");
                #endregion

                GameObject[] SceneObjects = GameObject.FindObjectsOfType<GameObject>();
                List<GameObject> ScenePrefabsGameObjects = new List<GameObject>();

                int SerializedID = 0;

                foreach (GameObject sceneObject in SceneObjects)
                {
                    GameObject PrefabRoot = PrefabUtility.GetOutermostPrefabInstanceRoot(sceneObject); //场景中物体父对象

                    if (PrefabRoot == null)
                    {
                        continue;
                    }

                    if (PrefabUtility.GetPrefabAssetType(PrefabRoot) != PrefabAssetType.NotAPrefab)
                    {
                        if (PrefabRoot.tag == "NotAllowBuild")
                        {
                            continue;
                        }

                        if (ScenePrefabsGameObjects.Contains(PrefabRoot))
                        {
                            Debug.Log(PrefabRoot.name + "Built");
                            continue;
                        }
                        SerializedID += 1;
                        #region Genrerate new ReplaceObject with SceneAssetPrefab.cs
                        GameObject ReplaceObject = new GameObject(PrefabRoot.name + "_CookedResource");

                        ReplaceObject.transform.position = PrefabRoot.transform.position;
                        ReplaceObject.transform.rotation = PrefabRoot.transform.rotation;
                        ReplaceObject.transform.localScale = PrefabRoot.transform.localScale;

                        ReplaceObject.transform.SetParent(PrefabRoot.transform.parent);

                        ReplaceObject.isStatic = PrefabRoot.isStatic;
                        //ReplaceObject.layer = PrefabRoot.layer;
                        //ReplaceObject.transform.tag = PrefabRoot.transform.tag;

                        #region SetUp SceneAssetPrefab
                        SceneAssetPrefab sceneAssetPrefab = ReplaceObject.AddComponent<SceneAssetPrefab>();
                        List<SceneAssetPrefab.MeshParameter> MeshParameters = new List<SceneAssetPrefab.MeshParameter>();
                        #region Save LightMap data
                        if (PrefabRoot.GetComponentsInChildren<ParticleSystem>().Length > 0)
                        {
                            sceneAssetPrefab.HasParticleSystem = true;
                        }

                        if (PrefabRoot.GetComponentsInChildren<MeshRenderer>().Length > 0)
                        {
                            foreach (MeshRenderer meshRender in PrefabRoot.GetComponentsInChildren<MeshRenderer>())
                            {
                                SceneAssetPrefab.MeshParameter MeshParameter = new SceneAssetPrefab.MeshParameter();
                                if (meshRender.transform == PrefabRoot.transform)
                                {
                                    MeshParameter.RendererPathinChild = false;
                                }
                                else
                                {
                                    MeshParameter.RendererPathinChild = true;
                                    Transform Current = meshRender.transform;

                                    string ChildPath = "";
                                    List<string> Relation = new List<string>();
                                    while (Current != PrefabRoot.transform)
                                    {
                                        Relation.Add(Current.name);
                                        Current = Current.parent;
                                    }
                                    for (int i = Relation.Count - 1; i >= 0; i--)
                                    {
                                        ChildPath += Relation[i] + "/";
                                    }
                                    MeshParameter.RendererPath = ChildPath;
                                }
                                MeshParameter.LightingMapIndex = meshRender.lightmapIndex;
                                MeshParameter.LightingMapTilingOffset = meshRender.lightmapScaleOffset;
                                MeshParameter.reflectionusage = meshRender.reflectionProbeUsage;
                                MeshParameters.Add(MeshParameter);
                            }
                        }
                        //if (PrefabUtility.GetCorrespondingObjectFromSource(PrefabRoot) == null)
                        //{
                        //    Debug.Log(PrefabRoot.name);
                        //}
                        sceneAssetPrefab.assetName = PrefabUtility.GetCorrespondingObjectFromSource(PrefabRoot).name;
                        sceneAssetPrefab.meshParameters = MeshParameters.ToArray();
                        sceneAssetPrefab.SerializedID = SerializedID;
                        sceneAssetPrefab.assetBundleName = AssetNameCorretor(AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(PrefabUtility.GetCorrespondingObjectFromSource(PrefabRoot))));
                        sceneAssetPrefab.assetBundleVariant = "sceneobject";
                        #endregion
                        #endregion



                        #endregion

                        if (!ScenePrefabsGameObjects.Contains(PrefabRoot))
                        {
                            ScenePrefabsGameObjects.Add(PrefabRoot);

                        }
                    }
                }
                List<GameObject> GameObjectsInAsset = new List<GameObject>();
                foreach (GameObject scenePrefab in ScenePrefabsGameObjects)
                {
                    GameObject GameObjectInAsset = PrefabUtility.GetCorrespondingObjectFromSource(scenePrefab) as GameObject;
                    if (!GameObjectsInAsset.Contains(GameObjectInAsset))
                    {
                        GameObjectsInAsset.Add(GameObjectInAsset);
                    }
                }

                SceneData sceneData = ScriptableObject.CreateInstance<SceneData>();
                sceneData.SceneObjectReferences = GameObjectsInAsset.ToArray();
                System.IO.DirectoryInfo Dir = System.IO.Directory.CreateDirectory("Assets/Cooks/Map/");

                AssetDatabase.CreateAsset(sceneData, "Assets/Cooks/Map/" + SceneOriginalName + ".asset");
                AssetDatabase.SaveAssets();
                EditorUtility.FocusProjectWindow();
                Selection.activeObject = sceneData;

                foreach (GameObject scenePrefab in ScenePrefabsGameObjects)
                {
                    //if (scenePrefab != null && !Reimported.Contains(PrefabUtility.GetPrefabParent(scenePrefab)))
                    //{
                    //    Reimported.Add(PrefabUtility.GetPrefabParent(scenePrefab));

                    //    Debug.Log(scenePrefab.name + "Asset Reimported!");
                    //    AssetImporter assetImporter = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(PrefabUtility.GetPrefabParent(scenePrefab)));
                    //    string AssetPathToGUID = AssetNameCorretor(AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(PrefabUtility.GetPrefabParent(scenePrefab))));

                    //    assetImporter.assetBundleName = AssetPathToGUID;
                    //    assetImporter.assetBundleVariant = "sceneobject";


                    //}
                    DestroyImmediate(scenePrefab);
                }
                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                EditorSceneManager.SaveOpenScenes();
                string AllScenes = "\n Current Active Scenes:";
                bool AddFlag = true;

                for (int i = 0; i < EditorBuildSettings.scenes.Length; i++)
                {
                    AllScenes += "\n -" + EditorBuildSettings.scenes[i].path;
                    if (EditorBuildSettings.scenes[i].path == EditorSceneManager.GetActiveScene().path)
                    {
                        AddFlag = false;
                    }
                }
                if (AddFlag)
                {
                    if (EditorUtility.DisplayDialog("Add to BuildSetting", "Do you want to add this cooked scene to BuildSetting?" + AllScenes, "Yes", "No,Thanks"))
                    {
                        EditorBuildSettingsScene[] original = EditorBuildSettings.scenes;
                        EditorBuildSettingsScene[] newSettings = new EditorBuildSettingsScene[original.Length + 1];
                        System.Array.Copy(original, newSettings, original.Length);
                        newSettings[newSettings.Length - 1] = new EditorBuildSettingsScene(EditorSceneManager.GetActiveScene().path, true);
                        EditorBuildSettings.scenes = newSettings;
                    }
                }

                EditorUtility.ClearProgressBar();


                //EditorSceneManager.SaveOpenScenes ();

            }
        }
        public static string AssetNameCorretor(string str)
        {
            return str.Replace(" ", "").ToLower();

        }
    }
}
