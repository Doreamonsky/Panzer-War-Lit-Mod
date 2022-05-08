projectDir="D:\Projects\PanzerWar-Mods\UnityProject"
manifestDir="D:\Projects\PanzerWar-Mods\UnityProject\BuildPiplineCI\TestBuildPipline.json"
editorDir="H:\UnityEditor\2021.3.0f1\Editor\Unity.exe"
logPath="D:\Projects\PanzerWar-Mods\UnityProject\BuildPiplineCI\log.txt"
$editorDir -projectPath $projectDir -batchmode -quit -executeMethod ShanghaiWindy.Editor.Utility_BuildPipline.DoBuildPiplineManifest -config $manifestDir -logFile $logPath