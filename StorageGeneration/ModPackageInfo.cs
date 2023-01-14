namespace ShanghaiWindy.Core
{
    [System.Serializable]
    public struct ModFileManifiest
    {
        public string n;
        public string h;
        public string[] d;
    }

    [System.Serializable]
    public struct ModPackageDependencyData
    {
        public string packageName;
        public string packageGuid;
        public int packageMiniVersion;
    }

    [System.Serializable]
    public class ModPackageInfo
    {
        public string modName;
        public string description;
        public string author;
        public string supportURL;
        public int modVersion;
        public string buildTarget;
        public string buildEngineVersion;
        public string uuid;

        public string guid;
        public ModFileManifiest[] fileManifests = new ModFileManifiest[] { };
        public ModPackageDependencyData[] dependencies = new ModPackageDependencyData[] { };
    }
}