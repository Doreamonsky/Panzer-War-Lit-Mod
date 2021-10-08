using System.Reflection;

namespace ParadoxNotion.Serialization
{

    ///Interface between Serialized_X_Info
    public interface ISerializedReflectedInfo : UnityEngine.ISerializationCallbackReceiver
    {
        MemberInfo AsMemberInfo();
        string AsString();
    }
}