using ShanghaiWindy.Core;
using UnityEditor;

namespace ShanghaiWindy.Editor
{
    [CustomEditor(typeof(ModPackageData))]
    public class ModPackageDataEditor : EditorWindowBase
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            var modPackData = target as ModPackageData;

            var importer = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(modPackData));

            if (importer.assetBundleName != modPackData.name)
            {
                importer.SetAssetBundleNameAndVariant(modPackData.name, "modpackdata");
                importer.SaveAndReimport();
            }
        }
    }
}