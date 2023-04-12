using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Sirenix.OdinInspector.Editor;
using System.IO;
using System.Security.Cryptography;
using Sirenix.OdinInspector;

public class RuntimeLibVersionWindow : OdinEditorWindow
{
    [MenuItem("Mod/查看模组工具版本 - Display Mod Lib Versions")]
    private static void OpenWindow()
    {
        var win = GetWindow<RuntimeLibVersionWindow>();
        win.minSize = new Vector2(800f, 600f);
        win.Show();
    }

    [ReadOnly] [TableList] public List<DllMD5Item> dllMD5List;

    protected override void OnEnable()
    {
        base.OnEnable();
        dllMD5List = new List<DllMD5Item>();
        RefreshDllMD5List();
    }

    [Button("刷新", ButtonSizes.Large)]
    private void RefreshDllMD5List()
    {
        dllMD5List.Clear();

        var dllFolderPath = Application.dataPath + "/../Packages/com.shanghaiwindy.middlelayer/Runtime";
        var dllFiles = Directory.GetFiles(dllFolderPath, "*.dll");

        foreach (var dllFile in dllFiles)
        {
            var fileName = Path.GetFileName(dllFile);

            var md5Hash = CalculateMD5(dllFile);
            dllMD5List.Add(new DllMD5Item { DllName = fileName, Hash = md5Hash });
        }
    }

    private string CalculateMD5(string filePath)
    {
        using (var md5 = MD5.Create())
        {
            using (var stream = File.OpenRead(filePath))
            {
                var hash = md5.ComputeHash(stream);
                return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
            }
        }
    }
}

[Serializable]
public class DllMD5Item
{
    [TableColumnWidth(120)] public string DllName;

    [TableColumnWidth(300)] public string Hash;
}