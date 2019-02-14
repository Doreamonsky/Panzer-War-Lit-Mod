using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using ShanghaiWindy.Core;

namespace ShanghaiWindy.Editor
{
    [CustomEditor(typeof(VehicleGraph))]
    public class VehicleGraphEditor : EditorWindowBase
    {
        private VehicleGraph vehicleGraph;

        public void OnEnable()
        {
            vehicleGraph = target as VehicleGraph;
        }
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (GUILayout.Button("Add Vehicle"))
            {
                var vehicle = CreateInstance<VehicleInfo>();
                AssetDatabase.AddObjectToAsset(vehicle, vehicleGraph);
                vehicleGraph.vehicleList.Add(vehicle);
            }

            for (int i = 0; i < vehicleGraph.vehicleList.Count; i++)
            {
                //Vehicle vehicle = vehicleGraph.vehicleList[i];

                //EditorGUILayout.HelpBox(
                //    string.Format("车辆: {0}   依赖于{5}的研发 \n 等级:{1} 权重:{2} \n 价格:{3} 研发经验:{4}", vehicle.vehicleName, vehicle.rank, vehicle.weight, vehicle.price, vehicle.expToResearch,vehicle.vehicleDepend?.vehicleName),
                //    MessageType.None
                //);

                //if (GUILayout.Button("删除"))
                //{
                //    AssetDatabase.RemoveObjectFromAsset(vehicle);
                //    vehicleGraph.vehicleList.RemoveAt(i);
                //}
                GUILayout.Space(25);
            }
            if (GUI.changed)
            {
                EditorUtility.SetDirty(target);
            }
        }
        public override void DrawPreview(Rect previewArea)
        {
            base.DrawPreview(previewArea);
        }
    }


    //[CustomEditor(typeof(VehicleInfo))]
    //public class VehicleGraphVehicleEditor : EditorWindowBase
    //{
    //    private string[] dirVehicles;

    //    private int selectVehicle;

    //    private VehicleInfo vehicle;

    //    public void OnEnable()
    //    {
    //        vehicle = target as VehicleInfo;

    //        //Get Vehicle List From Directory
    //        var dir = new DirectoryInfo("Assets/Res/Vehicles/Ground/Data/Vehicle/");

    //        var subDirs = dir.GetDirectories();

    //        var list = new List<string>();
    //        foreach (var subDir in subDirs)
    //        {
    //            list.Add(subDir.Name);
    //        }

    //        dirVehicles = list.ToArray();
    //    }
    //    public override void OnInspectorGUI()
    //    {
    //        base.OnInspectorGUI();

    //        EditorGUILayout.Space();

    //        EditorGUILayout.BeginHorizontal();

    //        EditorGUILayout.LabelField("Vehicle Name:");

    //        selectVehicle = EditorGUILayout.Popup(selectVehicle, dirVehicles);

    //        if (GUILayout.Button("Apply Vehicle"))
    //        {
    //            vehicle.name = dirVehicles[selectVehicle];
    //            vehicle.vehicleName = dirVehicles[selectVehicle];
    //        }
    //        EditorGUILayout.EndHorizontal();

    //        //vehicle.rank = EditorGUILayout.IntSlider("Rank:", vehicle.rank, 1, 13);

    //        //vehicle.weight = EditorGUILayout.IntSlider("Weight:", vehicle.weight, vehicle.rank * 10, vehicle.rank * 10 + 10);

    //        //EditorGUILayout.HelpBox(string.Format("Price:{0} \n Exp:{1}", vehicle.price.ToString("N"), vehicle.expToResearch.ToString("N")), MessageType.None);

    //        //if (GUILayout.Button("Auto Exp and Price"))
    //        //{
    //        //    var eco = AssetDatabase.LoadAssetAtPath<EconomicData>("Assets/Res/Database/EconomicData.asset");

    //        //    vehicle.price = (int)eco.VehPrice.Evaluate(vehicle.rank * Random.Range(0.9f, 1.1f));
    //        //    vehicle.expToResearch = (int)eco.VehExp.Evaluate(vehicle.rank * Random.Range(0.9f, 1.1f));
    //        //}

    //        if (GUI.changed)
    //        {
    //            EditorUtility.SetDirty(target);
    //        }
    //    }
    //}
}