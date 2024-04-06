using System.Collections;
using System.Collections.Generic;
using System.IO;
using Cysharp.Threading.Tasks;
using ShanghaiWindy.Core;
using ShanghaiWindy.Core.Config;
using ShanghaiWindy.Core.Data;
using ShanghaiWindy.Core.ResourceManager;
using UnityEngine;

namespace ShanghaiWindy.Editor.PlayMode
{
    public class EditorModeStartup : GamePreLoadRoot
    {
        public static System.Action OnInit;
        public GameCoreConfig Config;
        public EDamageMode DamageMode = EDamageMode.ModuleBased;

        public override async UniTask EnterGame()
        {
            GameDataManager.DamageMode = DamageMode;
            GameDataManager.isCustomEditor = true;

            GameRoot.GameCoreConfigProvider = new GameCoreConfigCustomProvider(Config);
            SimpleResourceManager.Instance = new SimpleResourceManager(AssetBundleEntry.Instance.BundleManager);

            await AssetBundleManager.AsyncHotFix();
            AssetBundleManager.RunLuaEnvs();

            var externalDirs = new List<DirectoryInfo>
            {
                new("Packages/com.shanghaiwindy.middlelayer/RuntimeRes/BuildPipline-RuntimeSupport/packages/"),
                new("Packages/com.shanghaiwindy.middlelayer/RuntimeRes/BuildPipline-Official-SoundBank/packages/"),
                new("Packages/com.shanghaiwindy.middlelayer/RuntimeRes/BuildPipline-VFX-AdvancedEdition/packages/"),
                // new("Packages/com.shanghaiwindy.middlelayer/RuntimeRes/BuildPipline-Solider/packages/"),
            };

            var buildDir = new DirectoryInfo(Application.dataPath + "/../Build/TemporarilyBuild");
            if (buildDir.Exists)
            {
                externalDirs.Add(buildDir);
            }

            AssetBundleEntry.Instance.PackageManager.AddExternalModFolders(externalDirs.ToArray());

            foreach (var packageInfo in AssetBundleEntry.Instance.PackageManager.GetPackageInfos(true))
            {
                ResourceLog.Log($"{packageInfo.PackageName} is added to package manager.");
            }

            SimpleResourceManager.Instance.Instantiate(AssetConst.RUNTIME_SUPPORT, runtimeSupportGo =>
            {
                DontDestroyOnLoad(runtimeSupportGo);
                AssetBundleManager.OnQueryed += () => { StartCoroutine(InitializeAsync()); };
            });
        }

        private IEnumerator InitializeAsync()
        {
            yield return new WaitUntil(() => !AssetBundleManager.IsLoadingAB());
            OnInit?.Invoke();

            Core.CommonDataManager.Instance.ApplySettings();
            PoolManager.Initialize();

            Debug.Log("Enter Game");
        }
    }
}