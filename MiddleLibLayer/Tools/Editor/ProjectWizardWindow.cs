using System;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;
using ICSharpCode.SharpZipLib.Zip;
using System.IO;

public class ProjectWizardWindow : EditorWindow
{
    [MenuItem("Mod/项目初始化指南 - Project Startup Wizard")]
    private static void OpenWindow()
    {
        GetWindow<ProjectWizardWindow>().Show();
    }

    [InitializeOnLoadMethod]
    private static void OpenWindowOnFirstLaunch()
    {
        if (!EditorPrefs.HasKey("ProjectWizardWindowShown"))
        {
            EditorPrefs.SetBool("ProjectWizardWindowShown", true);
            OpenWindow();
        }
    }

    private void OnGUI()
    {
        GUILayout.Label("此工具用于将您的项目设置替换为我们项目提供的设置。" +
                        "替换的目的是确保您的项目设置与我们的要求保持一致，" +
                        "包括正确的图层 (Layer)、标签 (Tag) 和其他设置。" +
                        "单击“应用项目设置”以继续。", EditorStyles.wordWrappedLabel);
        GUILayout.Label(
            "This tool is designed to replace your project settings with our project setting. " +
            "The purpose of this replacement is to ensure your project settings are consistent with the requirements of our project, " +
            "including the correct layers, tags, and other settings. " +
            "Click 'Apply Project Setting' to proceed.", EditorStyles.wordWrappedLabel);

        if (GUILayout.Button("应用项目设置 - Apply Project Setting"))
        {
            ReplaceSettings();
        }
    }

    private void ReplaceSettings()
    {
        string outputPath = Application.dataPath + "/../ProjectSettings";
        string zipPath = "Packages/com.shanghaiwindy.middlelayer/ProjectSettings.zip";

        FastZip fastZip = new FastZip();
        ZipFile zf = new ZipFile(zipPath);

        string fileList = "The following files will be replaced:\n\n";

        foreach (ZipEntry entry in zf)
        {
            fileList += entry.Name + "\n";
        }

        zf.Close();

        if (EditorUtility.DisplayDialog("应用项目设置 - Apply Project Setting", fileList, "确认 - Confirm", "取消 - Cancel"))
        {
            if (Directory.Exists(outputPath))
            {
                Directory.Delete(outputPath, true);
            }

            fastZip.ExtractZip(zipPath, outputPath, null);

            Debug.Log("Project settings have been replaced.");
        }
    }
}