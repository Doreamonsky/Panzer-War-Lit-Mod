using System.Reflection;

namespace ParadoxNotion.Serialization
{

    public interface ISerializedMethodBaseInfo : ISerializedReflectedInfo
    {
        MethodBase GetMethodBase();
        bool HasChanged();
    }
}