using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[InitializeOnLoad]
public class URPPiplineUpgrader
{
    static URPPiplineUpgrader()
    {
        var urpAsset =
            AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(
                "Packages/com.shanghaiwindy.middlelayer/URPSetting/Common_PipelineAsset.asset");

        QualitySettings.renderPipeline = urpAsset;
        GraphicsSettings.renderPipelineAsset = urpAsset;
        
        Debug.Log("Check if project is urp project.");
    }
}