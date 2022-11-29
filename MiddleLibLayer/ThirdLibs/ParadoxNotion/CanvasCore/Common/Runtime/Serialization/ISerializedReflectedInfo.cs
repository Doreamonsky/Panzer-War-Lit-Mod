using System.Reflection;

namespace ParadoxNotion.Serialization
{

    ///<summary>Interface between Serialized_X_Info</summary>
    public interface ISerializedReflectedInfo : UnityEngine.ISerializationCallbackReceiver
    {
        MemberInfo AsMemberInfo();
        string AsString();
    }
}