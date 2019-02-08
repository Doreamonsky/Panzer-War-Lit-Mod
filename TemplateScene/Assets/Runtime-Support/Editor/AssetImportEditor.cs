using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
public class AssetImportEditor : AssetPostprocessor {
    [MenuItem("Assets/Mark as vehicle", false,0)]
    public static void MarkAsVehicleTexture(){
        AssetImportSetInfo assetImportSetInfo = AssetDatabase.LoadAssetAtPath<AssetImportSetInfo>("Assets/TextureSets.asset");
        foreach(Object selected in Selection.objects){
            assetImportSetInfo.AddVehicleTexture(selected.name);
            Debug.Log(string.Format("{0} is marked as vehicle texture", selected.name));
        }
    }
    public void OnPreprocessTexture() {

        bool isMetallicSmoothness = false;
        bool isAlbedoTransparency = false;
        bool isNormal = false;
        bool isVehicleTexture = false;

        AssetImportSetInfo assetImportSetInfo = AssetDatabase.LoadAssetAtPath<AssetImportSetInfo>("Assets/TextureSets.asset");

		Debug.Log (assetImportSetInfo.VehicleTextureNames.Length);
       // bool isVehicleTexture = EditorUtility.DisplayDialog("Selection", "Are you importing vehicle texture", "Yes", "No");

        TextureImporter sourceTexture = this.assetImporter as TextureImporter;

		string[] assetStr = sourceTexture.assetPath.Split (new char[]{ '/', '.' }) ;

		string assetName = assetStr [assetStr.Length - 2];
        
        return;

		Debug.Log (assetName);

		isVehicleTexture = assetImportSetInfo.IsVehicleTexture(assetName);

        if(sourceTexture.assetPath.Contains("MetallicSmoothness")){
            isMetallicSmoothness = true;
        }

        if (sourceTexture.assetPath.Contains("AlbedoTransparency")) {
            isAlbedoTransparency = true;
        }

        if(sourceTexture.assetPath.Contains("Normal")){
            isNormal = true;
        }
        if (!isMetallicSmoothness & !isAlbedoTransparency&!isNormal)
            return;

        TextureImporterPlatformSettings standaloneTextureSettings = new TextureImporterPlatformSettings() {
            overridden = true,
            name = "Standalone",
            format = isMetallicSmoothness ? TextureImporterFormat.Alpha8 : sourceTexture.DoesSourceTextureHaveAlpha()||isNormal ? TextureImporterFormat.DXT5 : TextureImporterFormat.DXT1,
            maxTextureSize = 2048
        };

        sourceTexture.SetPlatformTextureSettings(standaloneTextureSettings);

        TextureImporterPlatformSettings androidTextureSettings = new TextureImporterPlatformSettings() {
            overridden = true,
            name = "Android",
            format = isMetallicSmoothness ? TextureImporterFormat.Alpha8 : sourceTexture.DoesSourceTextureHaveAlpha()||isNormal ? TextureImporterFormat.ETC2_RGBA8 : TextureImporterFormat.ETC2_RGB4,
			maxTextureSize = isAlbedoTransparency ? (isVehicleTexture ? 2048 : 512) : (isVehicleTexture ? 1024 : 256)
        };
        sourceTexture.SetPlatformTextureSettings(androidTextureSettings);

        TextureImporterPlatformSettings iOSTextureSettings = new TextureImporterPlatformSettings() {
            overridden = true,
            name = "iPhone",
            format = isMetallicSmoothness ? TextureImporterFormat.Alpha8 : sourceTexture.DoesSourceTextureHaveAlpha() || isNormal ? TextureImporterFormat.PVRTC_RGBA4 : TextureImporterFormat.PVRTC_RGB4,
			maxTextureSize = isAlbedoTransparency ? (isVehicleTexture ? 2048 : 512) : (isVehicleTexture ? 1024 : 256)
        };
        sourceTexture.SetPlatformTextureSettings(iOSTextureSettings);

		Debug.Log (isVehicleTexture);

        if (isNormal) {
            sourceTexture.normalmap = true;
        }


    }


}
