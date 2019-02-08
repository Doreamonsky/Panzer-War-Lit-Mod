using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using ShanghaiWindy.Core;
using ShanghaiWindy.Editor;

public class Utility_AssetManager : EditorWindow
{
    static string[] vehicleList;
    static int index = 0;

    private static string assetName = "", assetVariant = "";

    private static string slnText = "";

    [MenuItem("Tools/General Asset Manager")]
    static void Init()
    {
        var win = EditorWindow.GetWindow(typeof(Utility_AssetManager));
        win.titleContent.text = "General Asset Manager";
    }

    private static void BindingActions()
    {
        EditorApplication.playModeStateChanged += PlayModeChanged;
    }
    private static void CleanBindingActions()
    {
        EditorApplication.playModeStateChanged -= PlayModeChanged;
    }

    private static void PlayModeChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.ExitingEditMode)
        {
            var legacySpawn = GameObject.FindObjectsOfType<SpawnTestVehicleHelper>();

            for (int i = 0; i < legacySpawn.Length; i++)
            {
                SpawnTestVehicleHelper spawn = legacySpawn[i];
                DestroyImmediate(spawn);
            }

            var newSpawn = new GameObject("Spawner", typeof(SpawnTestVehicleHelper));
            newSpawn.GetComponent<SpawnTestVehicleHelper>().currentVehicle = vehicleList[index];
        }
        Debug.Log(state);
    }

    private static void UpdateVehicleList()
    {
        var dir = new DirectoryInfo("Assets/Res/Vehicles/Ground/Data/Vehicle/");
        var subDirs = dir.GetDirectories();

        var list = new List<string>();
        foreach (var subDir in subDirs)
        {
            list.Add(subDir.Name);
        }

        vehicleList = list.ToArray();

        index = PlayerPrefs.GetInt("illustrationEditor/index", 0);
    }

    void OnGUI()
    {
        UpdateVehicleList();

        EditorGUILayout.HelpBox("This is the General Asset Manager. \n You can run vehicles on the current map. And navigate to other tools from here.", MessageType.None, true);

        GUILayout.Space(20);

        EditorGUILayout.HelpBox("Vehicle Debugger! \n Select a vehicle and enter a map with StartPoints. The vehicle will be spawned at StartPoint.", MessageType.None, true);

        index = EditorGUILayout.Popup(index, vehicleList);

        PlayerPrefs.SetInt("illustrationEditor/index", index);

        if (GUILayout.Button("Test Vehicle From Start Point"))
        {
            EditorApplication.isPlaying = true;
        }

        GUILayout.Space(20);

        EditorGUILayout.HelpBox("AssetBundle Manager /n Serialize scene is for steaming loading the scene. Every scene in the gameplay should be serialized. A new blank scene will be created for the serialized scene. You should save it.", MessageType.None, true);

        if (GUILayout.Button("Navigate To Serialize Scene"))
        {
            GetWindow(typeof(SceneAssetsSerialize));
        }

        if (GUILayout.Button("Navigate To Asset Builder"))
        {
            GetWindow(typeof(SceneBuilder));
        }

        GUILayout.Space(20);

        EditorGUILayout.HelpBox("Do Not Click Any Button Below!!", MessageType.None, true);

        if (GUILayout.Button("Binding Actions"))
        {
            BindingActions();
        }
        if (GUILayout.Button("Clean Binding Actions"))
        {
            CleanBindingActions();
        }

        assetName = EditorGUILayout.TextField(assetName);
        assetVariant = EditorGUILayout.TextField(assetVariant);

        if (GUILayout.Button("Assign Selection AB"))
        {
            var selected = Selection.activeObject;
            var importer = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(selected));
            importer.SetAssetBundleNameAndVariant(assetName, assetVariant);
            importer.SaveAndReimport();
        }

        if (GUILayout.Button("Re-Pack All Tanks"))
        {
            var guidList = AssetDatabase.FindAssets("vehicledata");
            foreach (var guid in guidList)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);

                var vehicleData = AssetDatabase.LoadAssetAtPath<VehicleData>(path);

                if (vehicleData != null)
                {
                    VehicleDataEditor.PackAsset(vehicleData);
                }
            }
        }

        if (GUILayout.Button("Sync Vehicle Info"))
        {
            var vehicleDirectory = new DirectoryInfo("Assets/Res/Vehicles/Ground/Data/Vehicle");

            foreach (var subDirectory in vehicleDirectory.GetDirectories())
            {
                var vehicleName = subDirectory.Name;

                var vehicleInfoFilePath = "Assets/Res/Vehicles/Ground/Data/VehicleInfo/" + vehicleName + ".asset";

                var vehicleInfoFile = new FileInfo(vehicleInfoFilePath);

                if (!vehicleInfoFile.Exists)
                {
                    var vehicleInfo = ScriptableObject.CreateInstance<VehicleInfo>();
                    vehicleInfo.name = vehicleName;
                    vehicleInfo.vehicleName = vehicleName;

                    AssetDatabase.CreateAsset(vehicleInfo, vehicleInfoFilePath);
                    AssetDatabase.SaveAssets();
                }
            }
        }

        if (GUILayout.Button("Sync Dll"))
        {
            var fileList = new List<FileInfo>
            {
                 new FileInfo("Temp/bin/Debug/cInput.dll"),
                 new FileInfo("Temp/bin/Debug/cInputFirstPasss.dll"),
                 new FileInfo("Temp/bin/Debug/Core.dll"),
                 new FileInfo("Temp/bin/Debug/EasyTouch.dll"),
                 new FileInfo("Temp/bin/Debug/FogOfWar.dll"),
                 new FileInfo("Temp/bin/Debug/Ionic.Zip.dll"),
                 new FileInfo("Temp/bin/Debug/MaterialUI.dll"),
                 new FileInfo("Temp/bin/Debug/PostProcessing.dll"),
            };

            var utilityEditor = new DirectoryInfo("Assets/ExtraPlugins/Editor").GetFiles("*.cs");

            var componenetEditor = new DirectoryInfo("Assets/Res/Core/Common/Editor").GetFiles("*.cs");

            var editorList = new List<FileInfo>();

            editorList.AddRange(utilityEditor);
            editorList.AddRange(componenetEditor);

            foreach (var file in fileList)
            {
                file.CopyTo($"Build/Runtime-Support/{file.Name}", true);
            }

            foreach (var file in editorList)
            {
                file.CopyTo($"Build/Runtime-Support/Editor/{file.Name}", true);
            }
        }

        slnText = EditorGUILayout.TextField(slnText);

        if (GUILayout.Button("Copy Files"))
        {
            var matches = new Regex("(?<=\").*?(?=\")", RegexOptions.None).Matches(slnText);

            var path = new List<FileInfo>();

            foreach (var match in matches)
            {
                if (match.ToString().Contains("Compile"))
                    continue;

                path.Add(new FileInfo(match.ToString()));
            }

            foreach (var file in path)
            {
                file.CopyTo($"Build/Dll/{file.Name}", true);
            }
        }
    }
}
