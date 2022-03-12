using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace ParadoxNotion.Serialization.FullSerializer
{
    ///<summary> MetaType contains metadata about a type. This is used by the reflection serializer.</summary>
    public class fsMetaType
    {

        private static Dictionary<Type, fsMetaType> _metaTypes = new Dictionary<Type, fsMetaType>();
        private static Dictionary<Type, object> _defaultInstances = new Dictionary<Type, object>();

        ///<summary> Return MetaType</summary>
        public static fsMetaType Get(Type type) {
            fsMetaType metaType;
            if ( _metaTypes.TryGetValue(type, out metaType) == false ) {
                metaType = new fsMetaType(type);
                _metaTypes[type] = metaType;
            }

            return metaType;
        }

        ///<summary> Clears out the cached type results</summary>
        public static void FlushMem() {
            _metaTypes = new Dictionary<Type, fsMetaType>();
            _defaultInstances = new Dictionary<Type, object>();
        }

        ///----------------------------------------------------------------------------------------------

        private delegate object ObjectGenerator();

        private ObjectGenerator generator;
        public Type reflectedType { get; private set; }
        public fsMetaProperty[] Properties { get; private set; }
        public bool DeserializeOverwriteRequest { get; private set; }

        ///----------------------------------------------------------------------------------------------

        //...
        private fsMetaType(Type reflectedType) {
            this.reflectedType = reflectedType;
            this.generator = GetGenerator(reflectedType);

            var properties = new List<fsMetaProperty>();
            CollectProperties(properties, reflectedType);
            this.Properties = properties.ToArray();

            //TODO: Use it?
            // this.DeserializeOverwriteRequest = reflectedType.RTIsDefined<fsDeserializeOverwrite>(true);
        }

        //...
        static void CollectProperties(List<fsMetaProperty> properties, Type reflectedType) {
            FieldInfo[] fields = reflectedType.RTGetFields();
            for ( var i = 0; i < fields.Length; i++ ) {
                var field = fields[i];

                if ( field.DeclaringType != reflectedType ) {
                    continue;
                }

                if ( CanSerializeField(field) ) {
                    properties.Add(new fsMetaProperty(field));
                }
            }

            if ( reflectedType.BaseType != null ) {
                CollectProperties(properties, reflectedType.BaseType);
            }
        }

        //...
        public static bool CanSerializeField(FieldInfo field) {

            // We don't serialize static fields
            if ( field.IsStatic ) {
                return false;
            }

            // We don't serialize delegates
            if ( typeof(Delegate).IsAssignableFrom(field.FieldType) ) {
                return false;
            }

#if !UNITY_EDITOR
            if ( field.RTIsDefined<fsIgnoreInBuildAttribute>(true) ) {
                return false;
            }
#endif

            // We don't serialize compiler generated fields.
            if ( field.RTIsDefined<CompilerGeneratedAttribute>(true) ) {
                return false;
            }

            // We don't serialize members annotated with any of the ignore serialize attributes
            for ( var i = 0; i < fsGlobalConfig.IgnoreSerializeAttributes.Length; i++ ) {
                if ( field.RTIsDefined(fsGlobalConfig.IgnoreSerializeAttributes[i], true) ) {
                    return false;
                }
            }

            if ( field.IsPublic ) {
                return true;
            }

            for ( var i = 0; i < fsGlobalConfig.SerializeAttributes.Length; i++ ) {
                if ( field.RTIsDefined(fsGlobalConfig.SerializeAttributes[i], true) ) {
                    return true;
                }
            }

            return false;
        }

        ///<summary> Create generator</summary>
        static ObjectGenerator GetGenerator(Type reflectedType) {

            if ( reflectedType.IsInterface || reflectedType.IsAbstract ) {
                return () => { throw new Exception("Cannot create an instance of an interface or abstract type for " + reflectedType); };
            }

            if ( typeof(UnityEngine.ScriptableObject).IsAssignableFrom(reflectedType) ) {
                return () => { return UnityEngine.ScriptableObject.CreateInstance(reflectedType); };
            }

            if ( reflectedType.IsArray ) {
                // we have to start with a size zero array otherwise it will have invalid data inside of it
                return () => { return Array.CreateInstance(reflectedType.GetElementType(), 0); };
            }

            if ( reflectedType == typeof(string) ) {
                return () => { return string.Empty; };
            }

            if ( reflectedType.IsValueType || reflectedType.RTIsDefined<fsUninitialized>(true) || !HasDefaultConstructor(reflectedType) ) {
                return () => { return System.Runtime.Serialization.FormatterServices.GetSafeUninitializedObject(reflectedType); };
            }

            // var exp = Expression.Lambda<Func<object>>(Expression.New(reflectedType)).Compile();
            // return () => { return exp(); };

            return () => { try { return Activator.CreateInstance(reflectedType, /*nonPublic:*/ true); } catch { return null; } };
        }

        //...
        static bool HasDefaultConstructor(Type reflectedType) {
            // arrays are considered to have a default constructor
            if ( reflectedType.IsArray ) {
                return true;
            }

            // value types (ie, structs) always have a default constructor
            if ( reflectedType.IsValueType ) {
                return true;
            }

            // consider private constructors as well
            return reflectedType.RTGetDefaultConstructor() != null;
        }

        ///----------------------------------------------------------------------------------------------

        ///<summary> Returns a *cached* default instance of target type This is mostly used for comparing default serialization properties</summary>
        public object GetDefaultInstance() {
            object instance = null;
            if ( _defaultInstances.TryGetValue(reflectedType, out instance) ) {
                return instance;
            }
            return _defaultInstances[reflectedType] = CreateInstance();
        }

        ///<summary> Creates a new instance of the type that this metadata points back to.</summary>
        public object CreateInstance() {
            if ( generator != null ) { return generator(); }
            throw new Exception("Cant create instance generator for " + reflectedType);
        }

    }
}