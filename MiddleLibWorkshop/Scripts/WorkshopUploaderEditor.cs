using System.Diagnostics;
using System.IO;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using Debug = UnityEngine.Debug;

namespace ShanghaiWindy.Steamworkshop
{
    public class WorkshopUploaderEditor : OdinEditorWindow
    {
        [ReadOnly] [SHWLabelText("上传工具路径 - Uploader Path")]
        public string UploaderPath = "Packages/com.shanghaiwindy.workshop/Binary~/PW-Mod-Uploader.exe";

        [MenuItem("Mod/Steam 创意工坊上传 - Steam Workshop Uploader")]
        private static void Init()
        {
            GetWindow<WorkshopUploaderEditor>();
        }

        [Button("启动 - Lanuch")]
        private void Lanuch()
        {
            var file = new FileInfo(UploaderPath);
            var fullPath = file.FullName;

            Debug.Log($"Try to lanuch from {fullPath}");
            Process.Start(fullPath);
        }
    }
}