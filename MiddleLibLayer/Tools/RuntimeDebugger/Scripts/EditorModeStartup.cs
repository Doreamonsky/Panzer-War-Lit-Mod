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
        public AssetHotFixConfig[] HotFixConfigs = new AssetHotFixConfig[] { };

        private Camera bgCamera;

        private void Awake()
        {
            bgCamera = gameObject.AddComponent<Camera>();

            gameObject.AddComponent<LuaScriptableModManager>();
            gameObject.AddComponent<ILScriptableModManager>();

            // Initialize Asset Entry
            var entry = gameObject.AddComponent<AssetBundleEntry>();
            entry.OnPackageInitialized += OnPackageInitialized;
            StartCoroutine(entry.AsyncInitialize());

            bgCamera.depth = -99;
            
            GameEventManager.OnNewVehicleSpawned.AddListener(vehicle =>
            {
                if (BaseInitSystem.IsLocalPlayer(vehicle._InstanceNetType))
                {
                    bgCamera.enabled = false;
                }
            });

            GameEventManager.OnNewVehicleDestroyed.AddListener(vehicle =>
            {
                if (BaseInitSystem.IsLocalPlayer(vehicle._InstanceNetType))
                {
                    bgCamera.enabled = true;
                }
            });
        }

        private void OnPackageInitialized()
        {
            var hotFixEntry = gameObject.AddComponent<AssetHotFixEntry>();
            hotFixEntry.Initialize(HotFixConfigs);

            StartCoroutine(AssetBundleManager.HotFix(() =>
                {
                    AssetBundleManager.RunLuaEnvs();

                    GameRoot.GameCoreConfigProvider = new GameCoreConfigCustomProvider(Config);
                    SimpleResourceManager.Instance = new SimpleResourceManager(AssetBundleEntry.Instance.BundleManager);

                    // 正式游戏加载流程
                    RuntimeLogger.Init();

                    var externalDirs = new List<DirectoryInfo>
                    {
                        new("Packages/com.shanghaiwindy.middlelayer/RuntimeRes/BuildPipline-RuntimeSupport/packages/"),
                        new("Packages/com.shanghaiwindy.middlelayer/RuntimeRes/BuildPipline-Official-SoundBank/packages/"),
                        new("Packages/com.shanghaiwindy.middlelayer/RuntimeRes/BuildPipline-Solider/packages/"),
                    };

                    var buildDir = new DirectoryInfo(Application.dataPath + "/../Build/Mod-BuildPipline");
                    foreach (var platformDir in buildDir.GetDirectories())
                    {
                        foreach (var packageDir in platformDir.GetDirectories())
                        {
                            var subPackageDir = new DirectoryInfo($"{packageDir}/packages/");
                            if (subPackageDir.Exists)
                            {
                                externalDirs.Add(subPackageDir);
                            }
                        }
                    }

                    AssetBundleEntry.Instance.PackageManager.AddExternalModFolders(externalDirs.ToArray());

                    foreach (var packageInfo in AssetBundleEntry.Instance.PackageManager.GetPackageInfos(true))
                    {
                        ResourceLog.Log($"{packageInfo.PackageName} is added to package manager.");
                    }

                    SimpleResourceManager.Instance.InstantiateAsync(AssetConst.RUNTIME_SUPPORT, runtimeSupportGo =>
                    {
                        DontDestroyOnLoad(runtimeSupportGo);
                        AssetBundleManager.OnQueryed += () => { StartCoroutine(InitializeAsync()); };
                    });
                }
            ));
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