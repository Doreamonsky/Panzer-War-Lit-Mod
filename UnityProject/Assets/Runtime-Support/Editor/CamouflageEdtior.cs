using ShanghaiWindy.Core;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace ShanghaiWindy.Editor
{
    [CustomEditor(typeof(CamouflageData))]
    public class CamouflageEdtior : EditorWindowBase
    {
        private CamouflageData camouflageData;

        private GameObject previewObject;

        public void OnEnable()
        {
            camouflageData = target as CamouflageData;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();


            if (GUILayout.Button("Preview Camo On Cube"))
            {
                var cammoMaterial = camouflageData.EditorInstanceShader();

                var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);

                cube.GetComponent<MeshRenderer>().material = cammoMaterial;
            }

            previewObject = EditorGUILayout.ObjectField(previewObject, typeof(GameObject), allowSceneObjects: true) as GameObject;

            if (GUILayout.Button("Preview Camo On Vehcile"))
            {
                foreach (var meshRenderer in previewObject.GetComponentsInChildren<MeshRenderer>())
                {
                    meshRenderer.material = camouflageData.EditorInstanceShader(meshRenderer.sharedMaterial);
                }
            }

            GUI.DrawTexture(GUILayoutUtility.GetRect(128, 128), camouflageData.mask);

            if (GUILayout.Button("Generate Thumbnail"))
            {
                var thumbnail = new Texture2D(camouflageData.mask.width, camouflageData.mask.height);

                for (var x = 0; x < camouflageData.mask.width; x++)
                {
                    for (var y = 0; y < camouflageData.mask.height; y++)
                    {
                        var pix = camouflageData.mask.GetPixel(x, y);
                        thumbnail.SetPixel(x, y, pix.r * camouflageData.r + pix.g * camouflageData.g + pix.b * camouflageData.b + (1 - pix.r - pix.g - pix.b) * camouflageData.d);
                    }
                }
                var texByte = thumbnail.EncodeToPNG();

                var texPath = $"Assets/Res/Vehicles/Ground/res/Camouflage/Thumbnail/{camouflageData.mask.name}_thumbnail.png";

                var stream = new FileStream(texPath, FileMode.OpenOrCreate);

                stream.Write(texByte, 0, texByte.Length);

                stream.Close();

                AssetDatabase.ImportAsset(texPath, ImportAssetOptions.Default);

                camouflageData.camoThumbnail = AssetDatabase.LoadAssetAtPath<Texture2D>(texPath);
            }


            if (GUI.changed)
            {
                EditorUtility.SetDirty(target);
            }
        }

    }

}
