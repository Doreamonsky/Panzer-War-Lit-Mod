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

            if (GUILayout.Button("Export Data as Json"))
            {
                string path = EditorUtility.SaveFilePanel("Export As Json", "Others/Data/", vehicleTextData.AssetName, "json");

                FileStream fs = new FileStream(path, FileMode.Create);
                byte[] data = System.Text.Encoding.Default.GetBytes(JsonUtility.ToJson(target));
                fs.Write(data, 0, data.Length);
                fs.Flush();
                fs.Close();
            }

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
