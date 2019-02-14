using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CreateAssetMenu(fileName = "AssetImportSetInfo", menuName = "ShanghaiWindy/Tools/AssetImportSetInfo", order = 1)]
public class AssetImportSetInfo : ScriptableObject {
    public void AddVehicleTexture(string newTextureName){
        string[] cache = new string[VehicleTextureNames.Length + 1];

        for (int i = 0; i < VehicleTextureNames.Length;i++){
            cache[i] = VehicleTextureNames[i];

            if (VehicleTextureNames[i] == newTextureName)
                return;
        }
        cache[cache.Length - 1] = newTextureName;

        VehicleTextureNames = cache;

		EditorUtility.SetDirty(this);

		AssetDatabase.SaveAssets();
    }

    public bool IsVehicleTexture(string newTextureName){
		Debug.Log (newTextureName);

        for (int i = 0; i < VehicleTextureNames.Length;i++){
			if (VehicleTextureNames [i] == newTextureName) {
				Debug.Log (VehicleTextureNames [i]);
				return true;
			}
        }

        return false;
    }

    public string[] VehicleTextureNames;
}
