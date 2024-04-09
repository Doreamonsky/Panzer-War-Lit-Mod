using UnityEditor;
using UnityEngine;
using ICSharpCode.SharpZipLib.Zip;
using ShanghaiWindy.Editor;

public class ProjectWizardWindow : EditorWindow
{
    private const string STORAGE_KEY = "PROJECT_WIZARD_V2";
    private const string CONFIG_VER = "2";

    [InitializeOnLoadMethod]
    private static void OpenWindowOnFirstLaunch()
    {
        if (EditorStorage.Info.GetString(STORAGE_KEY) != CONFIG_VER)
        {
            ReplaceSettings();
        }
    }


    private static void ReplaceSettings()
    {
        string outputPath = Application.dataPath + "/../ProjectSettings";
        string zipPath = "Packages/com.shanghaiwindy.middlelayer/ProjectSettings.zip";

        FastZip fastZip = new FastZip();
        ZipFile zf = new ZipFile(zipPath);

        string fileList = "以下文件将进行变更 - The following files will be replaced:\n\n";

        foreach (ZipEntry entry in zf)
        {
            fileList += entry.Name + "\n";
        }

        zf.Close();

        if (EditorUtility.DisplayDialog(
                "请确认使用新的项目设置选项 (为保障工具正常使用，我们会替换 ProjectSetting 中的部分参数)! - Confirm to use new project setting (We will change some project setting file to ensure project work as expected)",
                fileList,
                "确认 - Confirm",
                "取消 - Cancel"))
        {
            fastZip.ExtractZip(zipPath, outputPath, null);
            EditorStorage.Info.SetString(STORAGE_KEY, CONFIG_VER);
            Debug.Log("Project settings have been replaced.");
        }
    }
}