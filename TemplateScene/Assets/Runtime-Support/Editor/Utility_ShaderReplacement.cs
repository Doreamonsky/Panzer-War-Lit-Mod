using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class Utility_ShaderReplacement : MonoBehaviour
{
   
    [MenuItem("Tools/ReplaceShaders")]
    public static void ReplaceShader() {
        foreach (var building in Selection.gameObjects)
        {
            foreach(var meshRenderer in building.GetComponentsInChildren<MeshRenderer>())
            {
                meshRenderer.sharedMaterial.shader = Shader.Find("Mobile/Bumped Diffuse");
            }
        }
    }

    [MenuItem("Tools/ReplaceVehicleShaders")]
    public static void ReplaceVehicleShader()
    {
        foreach (var building in Selection.gameObjects)
        {
            foreach (var meshRenderer in building.GetComponentsInChildren<MeshRenderer>())
            {
                meshRenderer.sharedMaterial.shader = Shader.Find("Mobile/Bumped Specular");
                meshRenderer.sharedMaterial.SetFloat(Shader.PropertyToID("_Shininess"), 0.5f);
            }
        }
    }
}
