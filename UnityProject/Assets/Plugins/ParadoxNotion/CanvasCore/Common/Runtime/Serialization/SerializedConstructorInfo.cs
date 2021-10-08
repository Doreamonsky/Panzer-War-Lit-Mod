using System;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace ParadoxNotion.Serialization
{
    [Serializable]
    public class SerializedConstructorInfo : ISerializedMethodBaseInfo
    {

        [SerializeField]
        private string _baseInfo;
        [SerializeField]
        private string _paramsInfo;

        [NonSerialized]
        private ConstructorInfo _constructor;
        [NonSerialized]
        private bool _hasChanged;

        void ISerializationCallbackReceiver.OnBeforeSerialize() {
            _hasChanged = false;
            if ( _constructor != null ) {
                _baseInfo = _constructor.RTReflectedOrDeclaredType().FullName + "|" + "$Constructor";
                _paramsInfo = string.Join("|", _constructor.GetParameters().Select(p => p.ParameterType.FullName).ToArray());
            }
        }

        void ISerializationCallbackReceiver.OnAfterDeserialize() {
            _hasChanged = false;

            if ( _baseInfo == null ) {
                return;
            }

            var split = _baseInfo.Split('|');
            var type = ReflectionTools.GetType(split[0], true);
            if ( type == null ) {
                _constructor = null;
                return;
            }

            var paramTypeNames = string.IsNullOrEmpty(_paramsInfo) ? null : _paramsInfo.Split('|');
            var parameterTypes = paramTypeNames != null ? new Type[paramTypeNames.Length] : Type.EmptyTypes;
            var paramsFail = false;
            if ( paramTypeNames != null ) {
                for ( var i = 0; i < paramTypeNames.Length; i++ ) {
                    var pType = ReflectionTools.GetType(paramTypeNames[i], true);
                    if ( pType == null ) {
                        paramsFail = true;
                        break;
                    }
                    parameterTypes[i] = pType;
                }
            }

            if ( !paramsFail ) {
                _constructor = type.RTGetConstructor(parameterTypes);
            }

            //fallback
            if ( _constructor == null ) {
                _hasChanged = true;
                var constructors = type.RTGetConstructors();
                _constructor = constructors.FirstOrDefault(c => c.GetParameters().Length == parameterTypes.Length);
                if ( _constructor == null ) { _constructor = constructors.FirstOrDefault(); }
            }
        }

        public SerializedConstructorInfo() { }
        public SerializedConstructorInfo(ConstructorInfo constructor) {
            _hasChanged = false;
            _constructor = constructor;
        }

        public MemberInfo AsMemberInfo() { return _constructor; }
        public MethodBase GetMethodBase() { return _constructor; }
        public bool HasChanged() { return _hasChanged; }
        public string AsString() { return string.Format("{0} ({1})", _baseInfo.Replace("|", "."), _paramsInfo.Replace("|", ", ")); }
        public override string ToString() { return AsString(); }

        //operator
        public static implicit operator ConstructorInfo(SerializedConstructorInfo value) {
            return value != null ? value._constructor : null;
        }

    }
}