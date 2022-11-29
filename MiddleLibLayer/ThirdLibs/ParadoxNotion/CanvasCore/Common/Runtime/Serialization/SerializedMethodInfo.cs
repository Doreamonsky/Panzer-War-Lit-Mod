using System;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace ParadoxNotion.Serialization
{

    ///<summary>Serialized MethodInfo</summary>
    [Serializable]
    public class SerializedMethodInfo : ISerializedMethodBaseInfo
    {

        [SerializeField]
        private string _baseInfo;
        [SerializeField]
        private string _paramsInfo;
        [SerializeField]
        private string _genericArgumentsInfo;

        [NonSerialized]
        private MethodInfo _method;
        [NonSerialized]
        private bool _hasChanged;

        ///<summary>serialize to strings info</summary>
        void ISerializationCallbackReceiver.OnBeforeSerialize() {
            _hasChanged = false;
            if ( _method != null ) {
                _baseInfo = string.Format("{0}|{1}|{2}", _method.RTReflectedOrDeclaredType().FullName, _method.Name, _method.ReturnType.FullName);
                _paramsInfo = string.Join("|", _method.GetParameters().Select(p => p.ParameterType.FullName).ToArray());
                _genericArgumentsInfo = _method.IsGenericMethod ? string.Join("|", _method.RTGetGenericArguments().Select(a => a.FullName).ToArray()) : null;
            }
        }

        //deserialize from strings info
        void ISerializationCallbackReceiver.OnAfterDeserialize() {
            _hasChanged = false;

            if ( _baseInfo == null ) {
                return;
            }

            var split = _baseInfo.Split('|');
            var type = ReflectionTools.GetType(split[0], true);
            if ( type == null ) {
                _method = null;
                return;
            }

            var name = split[1];
            var returnType = split.Length >= 3 ? ReflectionTools.GetType(split[2], true) : null;
            var isSerializedGeneric = !string.IsNullOrEmpty(_genericArgumentsInfo);
            var paramTypeNames = string.IsNullOrEmpty(_paramsInfo) ? null : _paramsInfo.Split('|');
            var parameterTypes = paramTypeNames != null ? new Type[paramTypeNames.Length] : Type.EmptyTypes;
            var paramsFail = false;
            for ( var i = 0; i < parameterTypes.Length; i++ ) {
                var pType = ReflectionTools.GetType(paramTypeNames[i], true);
                if ( pType == null ) {
                    paramsFail = true;
                    break;
                }
                parameterTypes[i] = pType;
            }

            if ( !paramsFail ) {

                if ( isSerializedGeneric ) {

                    var genericArgTypeNames = _genericArgumentsInfo.Split('|');
                    var genericArgTypes = new Type[genericArgTypeNames.Length];
                    var genericArgsFail = false;
                    for ( var i = 0; i < genericArgTypes.Length; i++ ) {
                        var argType = ReflectionTools.GetType(genericArgTypeNames[i], true);
                        if ( argType == null ) {
                            genericArgsFail = true;
                            break;
                        }
                        genericArgTypes[i] = argType;
                    }

                    if ( !genericArgsFail ) {
                        _method = type.RTGetMethod(name, parameterTypes, returnType, genericArgTypes);
                    }

                } else {
                    _method = type.RTGetMethod(name, parameterTypes, returnType);
                }
            }

            //fallback
            if ( _method == null ) {
                _hasChanged = true;
                var methods = type.RTGetMethods();
                _method = methods.FirstOrDefault(m => m.Name == name && m.GetParameters().Length == parameterTypes.Length && isSerializedGeneric == m.IsGenericMethod);
                if ( _method == null ) { _method = methods.FirstOrDefault(m => m.Name == name); }

                if ( _method != null && _method.IsGenericMethod ) {
                    var argType = isSerializedGeneric ? ReflectionTools.GetType(_genericArgumentsInfo.Split('|').First(), true) : _method.GetFirstGenericParameterConstraintType();
                    _method = _method.MakeGenericMethod(argType);
                }
            }
        }

        //required
        public SerializedMethodInfo() { }
        ///<summary>Serialize a new MethodInfo</summary>
        public SerializedMethodInfo(MethodInfo method) {
            _hasChanged = false;
            _method = method;
        }

        public MemberInfo AsMemberInfo() { return _method; }
        public MethodBase GetMethodBase() { return _method; }
        public bool HasChanged() { return _hasChanged; }
        public string AsString() { return string.Format("{0} ({1})", _baseInfo.Replace("|", "."), _paramsInfo.Replace("|", ", ")); }
        public override string ToString() { return AsString(); }

        //operator
        public static implicit operator MethodInfo(SerializedMethodInfo value) {
            return value != null ? value._method : null;
        }
    }
}