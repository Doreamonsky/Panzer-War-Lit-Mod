using ShanghaiWindy.Core;
using UnityEditor;
using UnityEngine;

namespace ShanghaiWindy.Editor
{
    [CustomEditor(typeof(BulletSound))]
    public class BulletSoundDataEditor : EditorWindowBase
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if(GUILayout.Button("Set AssetBundle"))
            {
                var bulletSound = target as BulletSound;

                var importer = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(bulletSound));

                if (importer.assetBundleName != bulletSound.name)
                {
                    importer.SetAssetBundleNameAndVariant(bulletSound.name, "bulletsound");
                    importer.SaveAndReimport();
                }
            }
        }
    }
}