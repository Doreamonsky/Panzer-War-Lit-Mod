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
            DontDestroyOnLoad(gameObject.transform.root);

            GameDataManager.DamageMode = DamageMode;
            GameDataManager.isCustomEditor = true;

            GameRoot.GameCoreConfigProvider = new GameCoreConfigCustomProvider(Config);
            SimpleResourceManager.Instance = new SimpleResourceManager(AssetBundleEntry.Instance.BundleManager);


            var externalDirs = new List<DirectoryInfo>
            {
                new("Packages/com.shanghaiwindy.middlelayer/RuntimeRes/BuildPipline-RuntimeSupport/packages/"),
                new("Packages/com.shanghaiwindy.middlelayer/RuntimeRes/BuildPipline-Official-SoundBank/packages/"),
                new("Packages/com.shanghaiwindy.middlelayer/RuntimeRes/BuildPipline-VFX-AdvancedEdition/packages/"),
                new("Packages/com.shanghaiwindy.middlelayer/RuntimeRes/BuildPipline-Atlas/packages/"),
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
                AssetBundleManager.OnQueryed += InitializeAsync;
            });
        }

        private async void InitializeAsync()
        {
            var atlas = await SimpleResourceManager.Instance.InstantiateAsync(
                "5deb8-30216def39f61bd43b202f002d69b2df.unity3d",
                false, Vector3.zero, Quaternion.identity);
            DontDestroyOnLoad(atlas);

            await AssetBundleManager.AsyncHotFix();
            AssetBundleManager.LoadLuaEnvResources();

            await UniTask.WaitWhile(AssetBundleManager.IsLoadingAB);
            AssetBundleManager.RunLuaEnvs();


            Core.CommonDataManager.Instance.ApplySettings();
            PoolManager.Initialize();

            OnInit?.Invoke();
            Debug.Log("<color=green>Enter game completed. 游戏加载已完毕</color>");
        }
    }
}