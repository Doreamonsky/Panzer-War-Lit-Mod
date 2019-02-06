using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MapData", menuName = "ShanghaiWindy/MapData")]
public class MapData : ScriptableObject
{
    public enum Mode
    {
        InfiniteMode,
        RTSHistoricalMode,
        Training,
        Utility
    }

    public enum Season
    {
        Spring,
        Summer,
        Autumn,
        Winter
    }
    [System.Serializable]
    public class BuildInfo
    {
        public bool isBuiltIn;
        public bool isCooked;

        public string sceneName;

        public string assetBundleName;
    }

    public string mapName;

    public Season mapSeason;

    public List<Mode> supportModes = new List<Mode>();

    public BuildInfo buildInfo = new BuildInfo();

    public RTSMapData rtsMapData;
}
