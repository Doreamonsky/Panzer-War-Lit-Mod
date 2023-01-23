using System.Collections;
using System.Collections.Generic;
using System.IO;
using ShanghaiWindy.Core;
using ShanghaiWindy.Core.Config;
using ShanghaiWindy.Core.Data;
using ShanghaiWindy.Core.ResourceManager;
using UnityEngine;

namespace ShanghaiWindy.Editor.PlayMode
{
    public class EditorModeStartup : MonoBehaviour
    {
        public static System.Action OnInit;
        public GameCoreConfig Config;


        private void Awake()
        {
            // Initialize Asset Entry
            var entry = gameObject.AddComponent<AssetBundleEntry>();
            entry.OnPackageInitialized += Init;
            StartCoroutine(entry.AsyncInitialize());
        }

        private void Init()
        {
            GameRoot.GameCoreConfigProvider = new GameCoreConfigCustomProvider(Config);
            SimpleResourceManager.Instance = new SimpleResourceManager(AssetBundleEntry.Instance.BundleManager);


            // 正式游戏加载流程
            RuntimeLogger.Init();

            var externalDirs = new List<DirectoryInfo>
            {
                new("Packages/com.shanghaiwindy.middlelayer/RuntimeRes/BuildPipline-RuntimeSupport/"),
                new("Packages/com.shanghaiwindy.middlelayer/RuntimeRes/BuildPipline-Official-SoundBank/")
            };

            var buildDir = new DirectoryInfo(Application.dataPath + "/../Build/Mod-BuildPipline");
            foreach (var platformDir in buildDir.GetDirectories())
            {
                foreach (var packageDir in platformDir.GetDirectories())
                {
                    externalDirs.Add(packageDir);
                }
            }

            foreach (var externalDir in externalDirs)
            {
                ResourceLog.Log($"Find dir {externalDir.FullName} as external dir");
            }

            AssetBundleEntry.Instance.PackageManager.AddExternalModFolders(externalDirs.ToArray());

            foreach (var packageInfo in AssetBundleEntry.Instance.PackageManager.GetPackageInfos(true))
            {
                Debug.Log(packageInfo.PackageFolder);
            }

            SimpleResourceManager.Instance.InstantiateAsync(AssetConst.RUNTIME_SUPPORT, runtimeSupportGo =>
            {
                DontDestroyOnLoad(runtimeSupportGo);
                AssetBundleManager.OnQueryed += () => { StartCoroutine(InitializeAsync()); };
            });
        }

        private IEnumerator InitializeAsync()
        {
            yield return new WaitUntil(() => !AssetBundleManager.IsLoadingAB());
            OnInit?.Invoke();

            uGUI_QualitySetting.Initialize();
            PoolManager.Initialize();

            Debug.Log("Enter Game");
        }
    }
}