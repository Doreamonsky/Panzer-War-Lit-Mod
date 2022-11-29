using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.Events;

namespace ParadoxNotion.Serialization
{

    ///<summary>Unity events can either be fields or properties, so we serialize either.</summary>
    ///<summary>This does NOT resolve the actual object, but rather only the memberinfo pointing to that object</summary>
    [Serializable]
    public class SerializedUnityEventInfo : ISerializedReflectedInfo
    {
        [SerializeField]
        private string _baseInfo;

        [NonSerialized]
        private MemberInfo _memberInfo;

        ///<summary>Just a shortcut</summary>
        public bool isStatic {
            get
            {
                if ( _memberInfo is FieldInfo ) { return ( _memberInfo as FieldInfo ).IsStatic; }
                if ( _memberInfo is PropertyInfo ) { return ( _memberInfo as PropertyInfo ).IsStatic(); }
                return false;
            }
        }

        ///<summary>Just a shortcut</summary>
        public Type memberType {
            get
            {
                if ( _memberInfo is FieldInfo ) { return ( _memberInfo as FieldInfo ).FieldType; }
                if ( _memberInfo is PropertyInfo ) { return ( _memberInfo as PropertyInfo ).PropertyType; }
                return null;
            }
        }

        void ISerializationCallbackReceiver.OnBeforeSerialize() {
            if ( _memberInfo != null ) { _baseInfo = string.Format("{0}|{1}", _memberInfo.RTReflectedOrDeclaredType().FullName, _memberInfo.Name); }
        }

        void ISerializationCallbackReceiver.OnAfterDeserialize() {
            if ( _baseInfo == null ) {
                return;
            }

            var split = _baseInfo.Split('|');
            var type = ReflectionTools.GetType(split[0], true);
            if ( type == null ) {
                _memberInfo = null;
                return;
            }

            var name = split[1];
            var result = type.RTGetFieldOrProp(name);
            _memberInfo = null;
            if ( result is FieldInfo && typeof(UnityEventBase).RTIsAssignableFrom(( result as FieldInfo ).FieldType) ) {
                _memberInfo = result;
                return;
            }
            if ( result is PropertyInfo && typeof(UnityEventBase).RTIsAssignableFrom(( result as PropertyInfo ).PropertyType) ) {
                _memberInfo = result;
                return;
            }
        }

        public SerializedUnityEventInfo() { }
        public SerializedUnityEventInfo(FieldInfo info) { _memberInfo = info; }
        public SerializedUnityEventInfo(PropertyInfo info) { _memberInfo = info; }
        public SerializedUnityEventInfo(MemberInfo info) {
            if ( info is FieldInfo || info is PropertyInfo ) {
                _memberInfo = info;
                return;
            }
            throw new System.Exception("MemberInfo is neither Field nor Property");
        }

        public MemberInfo AsMemberInfo() { return _memberInfo; }
        public string AsString() { return _baseInfo != null ? _baseInfo.Replace("|", ".") : "None"; }
        public override string ToString() { return AsString(); }

        //operator
        public static implicit operator FieldInfo(SerializedUnityEventInfo value) {
            return value != null ? value._memberInfo as FieldInfo : null;
        }

        //operator
        public static implicit operator PropertyInfo(SerializedUnityEventInfo value) {
            return value != null ? value._memberInfo as PropertyInfo : null;
        }
    }
}