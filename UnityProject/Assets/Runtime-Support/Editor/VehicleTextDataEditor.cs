using ShanghaiWindy.Core;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace ShanghaiWindy.Editor
{
    [CustomEditor(typeof(VehicleTextData))]
    [CanEditMultipleObjects]
    public class VehicleTextDataEditor : EditorWindowBase
    {
        VehicleTextData vehicleTextData;
        public override void Awake()
        {
            base.Awake();
            EditorHeadline = "ShanghaiWindy Ground Vehicle Text Data Manager";
            vehicleTextData = (VehicleTextData)target;

            UpdateAssetLabel();
        }


        public override void OnInspectorGUI()
        {
            vehicleTextData = (VehicleTextData)target;


            if (GUILayout.Button("Set Asset Label"))
            {
                UpdateAssetLabel();
            }


            base.OnInspectorGUI();




            if (GUI.changed)
            {
                EditorUtility.SetDirty(target);
            }
        }

        private void UpdateAssetLabel()
        {
            if (vehicleTextData.AssetName != "VehicleNameTextData")
            {
                AssetImporter assetImporter = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(vehicleTextData));
                assetImporter.assetBundleName = vehicleTextData.AssetName;
                assetImporter.assetBundleVariant = "data";
            }
        }
    }
}
