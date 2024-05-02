using ShanghaiWindy.Core;
using ShanghaiWindy.Core.Data;
using ShanghaiWindy.Core.Utils;
using ShanghaiWindy.Editor;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEditor.SceneManagement;

[System.Serializable]
public class GameResGmCommand : AbstractGmCommand
{
    public override string GetName()
    {
        return "游戏资产 - Game Res";
    }


    [Button("加载基础资源 - Load Base Asset")]
    public void LoadBase()
    {
        EditorSceneManager.OpenScene(
            "Packages/com.shanghaiwindy.middlelayer/Tools/RuntimeDebugger/BasePlayground.unity");

        if (EditorUtility.DisplayDialog("提示 - Tip", "等待左下角显示游戏加载已完毕的日志后，再执行其他命令哦 - Wait Enter game completed log shown",
                "OK"))
        {
            EditorApplication.EnterPlaymode();
        }
    }

    [ReadOnly] [SHWLabelText("DIY 地图配置 - DIY Map Config")]
    public DIYMapUserDefined currentMap;

    [SHWLabelText("地图分享码 - Map Share Code")]
    public string mapShareCode;

    [Button("加载 DIY 地图 Yaml - Load DIY Map Yaml")]
    public async void LoadDIYMapYaml()
    {
        if (!CheckPlaying())
        {
            return;
        }

        var yamlFile = EditorUtility.OpenFilePanel("提示 - Tip", "请选择 DIY 地图 Yaml - Please pick a diy map yaml", "yaml");
        if (string.IsNullOrEmpty(yamlFile))
        {
            return;
        }

        var userDefine = YamlUtil.DeserializeYaml<DIYMapUserDefined>(yamlFile);
        currentMap = userDefine;

        await DIYMapSerializationUtil.AsyncDeserializeToCurrentScene(currentMap,
            (curCount, maxCount) => { });
    }

    [Button("导入地图分享码 - Import Map ShareCode")]
    public async void ImportMapShareCode()
    {
        var userDefine = await AccountManager.Instance.ImportMapShareCode(mapShareCode, false);
        currentMap = userDefine.MapUserDefined;

        await DIYMapSerializationUtil.AsyncDeserializeToCurrentScene(currentMap,
            (curCount, maxCount) => { });
    }

    private bool CheckPlaying()
    {
        if (!EditorApplication.isPlaying)
        {
            EditorUtility.DisplayDialog("提示 - Tip", "请先加载基础资源 - Please load base asset",
                "OK");
            return false;
        }


        return true;
    }
}