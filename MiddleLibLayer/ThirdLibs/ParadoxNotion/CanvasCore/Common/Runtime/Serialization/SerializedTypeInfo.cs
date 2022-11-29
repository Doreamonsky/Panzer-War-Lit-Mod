using System;
using System.Reflection;
using UnityEngine;

namespace ParadoxNotion.Serialization
{

    [Serializable]
    public class SerializedTypeInfo : ISerializedReflectedInfo
    {

        [SerializeField]
        private string _baseInfo;

        [NonSerialized]
        private Type _type;

        void ISerializationCallbackReceiver.OnBeforeSerialize() {
            if ( _type != null ) { _baseInfo = _type.FullName; }
        }

        void ISerializationCallbackReceiver.OnAfterDeserialize() {
            if ( _baseInfo == null ) { return; }
            _type = ReflectionTools.GetType(_baseInfo, true);
        }

        public SerializedTypeInfo() { }
        public SerializedTypeInfo(Type info) {
            _baseInfo = null;
            _type = info;
        }

        public MemberInfo AsMemberInfo() { return _type; }
        public string AsString() { return _baseInfo; }
        public override string ToString() { return _baseInfo; }

        //operator
        public static implicit operator Type(SerializedTypeInfo value) {
            return value != null ? value._type : null;
        }
    }
}