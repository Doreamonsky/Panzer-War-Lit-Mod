using ShanghaiWindy.Core;
using UnityEditor;
using UnityEngine;

namespace ShanghaiWindy.Editor
{
    [CustomEditor(typeof(VehicleInfo))]
    public class VehicleInfoEditor : EditorWindowBase
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if(GUILayout.Button("Set AssetBundle Name"))
            {
                var vehicleInfo = target as VehicleInfo;

                var importer = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(vehicleInfo));

                if (importer.assetBundleName != vehicleInfo.name)
                {
                    importer.SetAssetBundleNameAndVariant(vehicleInfo.name, "vehicleinfo");
                    importer.SaveAndReimport();
                }
            }

            if (GUILayout.Button("Clean AssetBundle Name"))
            {
                var vehicleInfo = target as VehicleInfo;

                var importer = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(vehicleInfo));

                importer.SetAssetBundleNameAndVariant(null, null);
                importer.SaveAndReimport();
            }

        }
    }
}