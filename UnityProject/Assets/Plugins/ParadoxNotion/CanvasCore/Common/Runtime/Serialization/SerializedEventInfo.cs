using System;
using System.Reflection;
using UnityEngine;

namespace ParadoxNotion.Serialization
{

    [Serializable]
    public class SerializedEventInfo : ISerializedReflectedInfo
    {

        [SerializeField]
        private string _baseInfo;

        [NonSerialized]
        private EventInfo _event;

        void ISerializationCallbackReceiver.OnBeforeSerialize() {
            if ( _event != null ) {
                _baseInfo = string.Format("{0}|{1}", _event.RTReflectedOrDeclaredType().FullName, _event.Name);
            }
        }

        void ISerializationCallbackReceiver.OnAfterDeserialize() {
            if ( _baseInfo == null ) {
                return;
            }
            var split = _baseInfo.Split('|');
            var type = ReflectionTools.GetType(split[0], true);
            if ( type == null ) {
                _event = null;
                return;
            }
            var name = split[1];
            _event = type.RTGetEvent(name);
        }

        public SerializedEventInfo() { }
        public SerializedEventInfo(EventInfo info) {
            _event = info;
        }

        public MemberInfo AsMemberInfo() { return _event; }
        public string AsString() { return _baseInfo != null ? _baseInfo.Replace("|", ".") : null; }
        public override string ToString() { return AsString(); }

        //operator
        public static implicit operator EventInfo(SerializedEventInfo value) {
            return value != null ? value._event : null;
        }
    }
}