using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[InitializeOnLoad]
public class URPPiplineUpgrader
{
    static URPPiplineUpgrader()
    {
        if (QualitySettings.renderPipeline == null || GraphicsSettings.renderPipelineAsset == null)
        {
            var urpAsset =
                AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(
                    "Packages/com.shanghaiwindy.middlelayer/URPSetting/Common_PipelineAsset.asset");

            QualitySettings.renderPipeline = urpAsset;
            GraphicsSettings.renderPipelineAsset = urpAsset;

            EditorUtility.DisplayDialog("Info",
                "你的项目已升级成基于 URP 渲染管线 - Your project has been upgraded to Universal Render Pipline based",
                "OK");
        }

        Debug.Log("Check if project is urp project.");
    }
}