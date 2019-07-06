using UnityEditor;
using UnityEngine;

namespace ShanghaiWindy.Editor
{
    public class Utility_BuildPipline
    {
        [MenuItem("Tools/Build/Free UnBundled")]
        public static void BuildFreeAndroid()
        {
            PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android, "ClientCode;UNITY_POST_PROCESSING_STACK_V2;ModSupport;Free");

            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, "com.shanghaiwindy.PanzerWarOpenSource");

            SetBuildPass();

            PlayerSettings.Android.useAPKExpansionFiles = false;

            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions()
            {
                scenes = EditorBuildSettingsScene.GetActiveSceneList(EditorBuildSettings.scenes),
                locationPathName = $"Build/Android/Free/PanzerWar-{PlayerSettings.Android.bundleVersionCode}-Free-UnBundled.apk",
                target = BuildTarget.Android,
                targetGroup = BuildTargetGroup.Android,
            });

            Debug.Log("Build to Free Version");
        }

        [MenuItem("Tools/Build/Free Bundled")]
        public static void BuildFreeAndroidBundled()
        {
            PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android, "ClientCode;UNITY_POST_PROCESSING_STACK_V2;ModSupport;Free");

            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, "com.shanghaiwindy.PanzerWarOpenSource");

            SetBuildPass();

            PlayerSettings.Android.useAPKExpansionFiles = true;

            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions()
            {
                scenes = EditorBuildSettingsScene.GetActiveSceneList(EditorBuildSettings.scenes),
                locationPathName = $"Build/Android/Free/PanzerWar-{PlayerSettings.Android.bundleVersionCode}-Free-Bundled.apk",
                target = BuildTarget.Android,
                targetGroup = BuildTargetGroup.Android,
            });

            Debug.Log("Build to Free Version");
        }


        [MenuItem("Tools/Build/Paid")]
        public static void BuildPaidAndroid()
        {
            PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android, "ClientCode;UNITY_POST_PROCESSING_STACK_V2;ModSupport;Paid");

            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, "com.shanghaiwindy.PanzerWarComplete");

            SetBuildPass();

            PlayerSettings.Android.useAPKExpansionFiles = true;

            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions()
            {
                scenes = EditorBuildSettingsScene.GetActiveSceneList(EditorBuildSettings.scenes),
                locationPathName = $"Build/Android/Paid/PanzerWar-{PlayerSettings.Android.bundleVersionCode}-Paid-Bundled.apk",
                target = BuildTarget.Android,
                targetGroup = BuildTargetGroup.Android,
            });

            Debug.Log("Build to Paid Version");
        }

        private static void SetBuildPass()
        {
            PlayerSettings.keyaliasPass = "28858991";
            PlayerSettings.keystorePass = "28858991";
        }


    }

}
