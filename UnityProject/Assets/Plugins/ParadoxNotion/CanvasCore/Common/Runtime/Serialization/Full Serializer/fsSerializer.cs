using System;
using System.Collections.Generic;
using System.Linq;
using ParadoxNotion.Serialization.FullSerializer.Internal;
using ParadoxNotion.Serialization.FullSerializer.Internal.DirectConverters;

namespace ParadoxNotion.Serialization.FullSerializer
{

    ///<summary>*Heavily* modified FullSerializer</summary>
    public class fsSerializer
    {
        public const string KEY_OBJECT_REFERENCE = "$ref";
        public const string KEY_OBJECT_DEFINITION = "$id";
        public const string KEY_INSTANCE_TYPE = "$type";
        public const string KEY_VERSION = "$version";
        public const string KEY_CONTENT = "$content";

        ///<summary> Returns true if the given key is a special keyword that full serializer uses to add additional metadata on top of the emitted JSON.</summary>
        public static bool IsReservedKeyword(string key) {
            switch ( key ) {
                case ( KEY_OBJECT_REFERENCE ): return true;
                case ( KEY_OBJECT_DEFINITION ): return true;
                case ( KEY_INSTANCE_TYPE ): return true;
                case ( KEY_VERSION ): return true;
                case ( KEY_CONTENT ): return true;
            }
            return false;
        }

        ///<summary>Irriversibly removes all meta data</summary>
        public static void RemoveMetaData(ref fsData data) {
            if ( data.IsDictionary ) {
                data.AsDictionary.Remove(KEY_OBJECT_REFERENCE);
                data.AsDictionary.Remove(KEY_OBJECT_DEFINITION);
                data.AsDictionary.Remove(KEY_INSTANCE_TYPE);
                data.AsDictionary.Remove(KEY_VERSION);
                data.AsDictionary.Remove(KEY_CONTENT);
            }
        }

        ///<summary> Ensures that the data is a dictionary. If it is not, then it is wrapped inside of one.</summary>
        private static void EnsureDictionary(ref fsData data) {
            if ( data.IsDictionary == false ) {
                var existingData = data.Clone();
                data.BecomeDictionary();
                data.AsDictionary[KEY_CONTENT] = existingData;
            }
        }

        private static bool IsObjectReference(fsData data) {
            if ( data.IsDictionary == false ) return false;
            return data.AsDictionary.ContainsKey(KEY_OBJECT_REFERENCE);
        }
        private static bool IsObjectDefinition(fsData data) {
            if ( data.IsDictionary == false ) return false;
            return data.AsDictionary.ContainsKey(KEY_OBJECT_DEFINITION);
        }
        private static bool IsVersioned(fsData data) {
            if ( data.IsDictionary == false ) return false;
            return data.AsDictionary.ContainsKey(KEY_VERSION);
        }
        private static bool IsTypeSpecified(fsData data) {
            if ( data.IsDictionary == false ) return false;
            return data.AsDictionary.ContainsKey(KEY_INSTANCE_TYPE);
        }
        private static bool IsWrappedData(fsData data) {
            if ( data.IsDictionary == false ) return false;
            return data.AsDictionary.ContainsKey(KEY_CONTENT);
        }

        ///----------------------------------------------------------------------------------------------

        private static void Invoke_OnBeforeSerialize(List<fsObjectProcessor> processors, Type storageType, object instance) {
            for ( int i = 0; i < processors.Count; ++i ) {
                processors[i].OnBeforeSerialize(storageType, instance);
            }

            //!!Call only on non-Unity objects, since they are called back anyways by Unity!!
            if ( instance is UnityEngine.ISerializationCallbackReceiver && !( instance is UnityEngine.Object ) ) {
                ( (UnityEngine.ISerializationCallbackReceiver)instance ).OnBeforeSerialize();
            }
        }
        private static void Invoke_OnAfterSerialize(List<fsObjectProcessor> processors, Type storageType, object instance, ref fsData data) {
            // We run the after calls in reverse order; this significantly reduces the interaction burden between
            // multiple processors - it makes each one much more independent and ignorant of the other ones.
            for ( int i = processors.Count - 1; i >= 0; --i ) {
                processors[i].OnAfterSerialize(storageType, instance, ref data);
            }
        }
        private static void Invoke_OnBeforeDeserialize(List<fsObjectProcessor> processors, Type storageType, ref fsData data) {
            for ( int i = 0; i < processors.Count; ++i ) {
                processors[i].OnBeforeDeserialize(storageType, ref data);
            }
        }
        private static void Invoke_OnBeforeDeserializeAfterInstanceCreation(List<fsObjectProcessor> processors, Type storageType, object instance, ref fsData data) {
            for ( int i = 0; i < processors.Count; ++i ) {
                processors[i].OnBeforeDeserializeAfterInstanceCreation(storageType, instance, ref data);
            }
        }

        private static void Invoke_OnAfterDeserialize(List<fsObjectProcessor> processors, Type storageType, object instance) {
            for ( int i = processors.Count - 1; i >= 0; --i ) {
                processors[i].OnAfterDeserialize(storageType, instance);
            }

            //!!Call only on non-Unity objects, since they are called back anyways by Unity!!
            if ( instance is UnityEngine.ISerializationCallbackReceiver && !( instance is UnityEngine.Object ) ) {
                ( (UnityEngine.ISerializationCallbackReceiver)instance ).OnAfterDeserialize();
            }
        }

        ///----------------------------------------------------------------------------------------------

        ///<summary> This manages instance writing so that we do not write unnecessary $id fields. We only need to write out an $id field when there is a corresponding $ref field. This is able to write $id references lazily because the fsData instance is not actually written out to text until we have entirely finished serializing it.</summary>
        internal class fsLazyCycleDefinitionWriter
        {
            private Dictionary<int, fsData> _pendingDefinitions = new Dictionary<int, fsData>();
            private HashSet<int> _references = new HashSet<int>();

            public void WriteDefinition(int id, fsData data) {
                if ( _references.Contains(id) ) {
                    EnsureDictionary(ref data);
                    data.AsDictionary[KEY_OBJECT_DEFINITION] = new fsData(id.ToString());
                } else {
                    _pendingDefinitions[id] = data;
                }
            }

            public void WriteReference(int id, Dictionary<string, fsData> dict) {
                fsData data;
                if ( _pendingDefinitions.TryGetValue(id, out data) ) {
                    EnsureDictionary(ref data);
                    data.AsDictionary[KEY_OBJECT_DEFINITION] = new fsData(id.ToString());
                    _pendingDefinitions.Remove(id);
                } else { _references.Add(id); }

                // Write the reference
                dict[KEY_OBJECT_REFERENCE] = new fsData(id.ToString());
            }

            public void Clear() {
                _pendingDefinitions.Clear();
                _references.Clear();
            }
        }

        ///<summary> Converter type to converter instance lookup table. This could likely be stored inside of _cachedConverters, but there is a semantic difference because _cachedConverters goes from serialized type to converter.</summary>
        private Dictionary<Type, fsBaseConverter> _cachedOverrideConverterInstances;
        ///<summary> A cache from type to it's converter.</summary>
        private Dictionary<Type, fsBaseConverter> _cachedConverters;
        ///<summary> Converters that can be used for type registration.</summary>
        private readonly List<fsConverter> _availableConverters;
        ///<summary> Direct converters (optimized _converters). We use these so we don't have to perform a scan through every item in _converters and can instead just do an O(1) lookup. This is potentially important to perf when there are a ton of direct converters.</summary>
        private readonly Dictionary<Type, fsDirectConverter> _availableDirectConverters;

        ///<summary> Processors that are available.</summary>
        private readonly List<fsObjectProcessor> _processors;
        ///<summary> A cache from type to the set of processors that are interested in it.</summary>
        private Dictionary<Type, List<fsObjectProcessor>> _cachedProcessors;

        ///<summary> Reference manager for cycle detection.</summary>
        private fsCyclicReferenceManager _references;
        private fsLazyCycleDefinitionWriter _lazyReferenceWriter;
        ///<summary> Collectors get callbacks on child serialization/deserialization</summary>
        private Stack<ISerializationCollector> _collectors;
        ///<summary> Collector collection child depth (local to each collector)</summary>
        private int _collectableDepth;

        ///<summary> A UnityObject references database for serialization/deserialization</summary>
        public List<UnityEngine.Object> ReferencesDatabase { get; set; }
        ///<summary> Ignore cycle references? //TODO: Refactor cycle references to avoid doing this.</summary>
        public bool IgnoreSerializeCycleReferences { get; set; }
        ///<summary> An event raised before an object has been serialized given the object</summary>
        public event Action<object> onBeforeObjectSerialized;
        ///<summary> An event raised after an object has been serialized given the object and the serialization data</summary>
        public event Action<object, fsData> onAfterObjectSerialized;


        //...
        public fsSerializer() {
            _cachedOverrideConverterInstances = new Dictionary<Type, fsBaseConverter>();
            _cachedConverters = new Dictionary<Type, fsBaseConverter>();
            _cachedProcessors = new Dictionary<Type, List<fsObjectProcessor>>();

            _references = new fsCyclicReferenceManager();
            _lazyReferenceWriter = new fsLazyCycleDefinitionWriter();
            _collectors = new Stack<ISerializationCollector>();

            // note: The order here is important. Items at the beginning of this
            //       list will be used before converters at the end. Converters
            //       added via AddConverter() are added to the front of the list.
            _availableConverters = new List<fsConverter>
            {
                new fsUnityObjectConverter { Serializer = this },
                new fsTypeConverter { Serializer = this },
                new fsEnumConverter { Serializer = this },
                new fsPrimitiveConverter { Serializer = this },
                new fsArrayConverter { Serializer = this },
                new fsDictionaryConverter { Serializer = this },
                new fsListConverter { Serializer = this },
                new fsReflectedConverter { Serializer = this }
            };
            _availableDirectConverters = new Dictionary<Type, fsDirectConverter>();

            _processors = new List<fsObjectProcessor>();

            //DirectConverters. Add manually for performance
            AddConverter(new AnimationCurve_DirectConverter());
            AddConverter(new Bounds_DirectConverter());
            AddConverter(new GUIStyleState_DirectConverter());
            AddConverter(new GUIStyle_DirectConverter());
            AddConverter(new Gradient_DirectConverter());
            AddConverter(new Keyframe_DirectConverter());
            AddConverter(new LayerMask_DirectConverter());
            AddConverter(new RectOffset_DirectConverter());
            AddConverter(new Rect_DirectConverter());

            AddConverter(new Vector2Int_DirectConverter());
            AddConverter(new Vector3Int_DirectConverter());
        }

        ///----------------------------------------------------------------------------------------------

        //Cleanup cycle references. 
        //This is done to ensure that a problem in one serialization does not transfer to others.
        public void PurgeTemporaryData() {
            _references.Clear();
            _lazyReferenceWriter.Clear();
            _collectors.Clear();
        }

        ///<summary> Fetches all of the processors for the given type.</summary>
        private List<fsObjectProcessor> GetProcessors(Type type) {
            List<fsObjectProcessor> processors;
            if ( _cachedProcessors.TryGetValue(type, out processors) ) {
                return processors;
            }

            // Check to see if the user has defined a custom processor for the type. If they
            // have, then we don't need to scan through all of the processor to check which
            // one can process the type; instead, we directly use the specified processor.
            var attr = type.RTGetAttribute<fsObjectAttribute>(true);
            if ( attr != null && attr.Processor != null ) {
                var processor = (fsObjectProcessor)Activator.CreateInstance(attr.Processor);
                processors = new List<fsObjectProcessor>();
                processors.Add(processor);
                _cachedProcessors[type] = processors;
            } else if ( _cachedProcessors.TryGetValue(type, out processors) == false ) {
                processors = new List<fsObjectProcessor>();

                for ( int i = 0; i < _processors.Count; ++i ) {
                    var processor = _processors[i];
                    if ( processor.CanProcess(type) ) {
                        processors.Add(processor);
                    }
                }

                _cachedProcessors[type] = processors;
            }

            return processors;
        }

        ///<summary> Adds a new converter that can be used to customize how an object is serialized and deserialized.</summary>
        public void AddConverter(fsBaseConverter converter) {
            if ( converter.Serializer != null ) {
                throw new InvalidOperationException("Cannot add a single converter instance to " +
                    "multiple fsConverters -- please construct a new instance for " + converter);
            }

            if ( converter is fsDirectConverter ) {
                var directConverter = (fsDirectConverter)converter;
                _availableDirectConverters[directConverter.ModelType] = directConverter;
            } else if ( converter is fsConverter ) {
                _availableConverters.Insert(0, (fsConverter)converter);
            } else {
                throw new InvalidOperationException("Unable to add converter " + converter +
                    "; the type association strategy is unknown. Please use either " +
                    "fsDirectConverter or fsConverter as your base type.");
            }

            converter.Serializer = this;
            _cachedConverters = new Dictionary<Type, fsBaseConverter>();
        }

        ///<summary> Fetches a converter that can serialize/deserialize the given type.</summary>
        private fsBaseConverter GetConverter(Type type, Type overrideConverterType) {

            // Use an override converter type instead if that's what the user has requested.
            if ( overrideConverterType != null ) {
                fsBaseConverter overrideConverter;
                if ( _cachedOverrideConverterInstances.TryGetValue(overrideConverterType, out overrideConverter) == false ) {
                    overrideConverter = (fsBaseConverter)Activator.CreateInstance(overrideConverterType);
                    overrideConverter.Serializer = this;
                    _cachedOverrideConverterInstances[overrideConverterType] = overrideConverter;
                }

                return overrideConverter;
            }

            // Try to lookup an existing converter.
            fsBaseConverter converter;
            if ( _cachedConverters.TryGetValue(type, out converter) ) {
                return converter;
            }

            // Check to see if the user has defined a custom converter for the type. If they
            // have, then we don't need to scan through all of the converters to check which
            // one can process the type; instead, we directly use the specified converter.
            {
                var attr = type.RTGetAttribute<fsObjectAttribute>(true);
                if ( attr != null && attr.Converter != null ) {
                    converter = (fsBaseConverter)Activator.CreateInstance(attr.Converter);
                    converter.Serializer = this;
                    return _cachedConverters[type] = converter;
                }
            }

            // Check for a [fsForward] attribute.
            {
                var attr = type.RTGetAttribute<fsForwardAttribute>(true);
                if ( attr != null ) {
                    converter = new fsForwardConverter(attr);
                    converter.Serializer = this;
                    return _cachedConverters[type] = converter;
                }
            }

            // No converter specified. Find match from general ones.
            {
                fsDirectConverter directConverter;
                if ( _availableDirectConverters.TryGetValue(type, out directConverter) ) {
                    return _cachedConverters[type] = directConverter;
                }

                for ( var i = 0; i < _availableConverters.Count; i++ ) {
                    if ( _availableConverters[i].CanProcess(type) ) {
                        return _cachedConverters[type] = _availableConverters[i];
                    }
                }
            }

            // No converter available
            return _cachedConverters[type] = null;
        }

        ///----------------------------------------------------------------------------------------------

        ///<summary> Serialize the given value.</summary>
        public fsResult TrySerialize(Type storageType, object instance, out fsData data) {
            return TrySerialize(storageType, instance, out data, null);
        }

        ///<summary> Serialize the given value. StorageType: field type. OverideConverter: optional override converter. Instance: the object instance. Data: the serialized state.</summary>
        public fsResult TrySerialize(Type storageType, object instance, out fsData data, Type overrideConverterType) {

            var realType = instance == null ? storageType : instance.GetType();
            var processors = GetProcessors(realType);
            Invoke_OnBeforeSerialize(processors, storageType, instance);

            // We always serialize null directly as null
            if ( ReferenceEquals(instance, null) ) {
                data = new fsData();
                Invoke_OnAfterSerialize(processors, storageType, instance, ref data);
                return fsResult.Success;
            }

            if ( onBeforeObjectSerialized != null ) { onBeforeObjectSerialized(instance); }

            fsResult result;

            try {
                _references.Enter();
                result = Internal_Serialize(storageType, instance, out data, overrideConverterType);
            }

            finally { if ( _references.Exit() ) { _lazyReferenceWriter.Clear(); } }

            //versioning
            TrySerializeVersioning(instance, ref data);

            //invoke processors
            Invoke_OnAfterSerialize(processors, storageType, instance, ref data);

            if ( onAfterObjectSerialized != null ) { onAfterObjectSerialized(instance, data); }

            return result;
        }

        //...
        fsResult Internal_Serialize(Type storageType, object instance, out fsData data, Type overrideConverterType) {

            var instanceType = instance.GetType();
            var instanceTypeConverter = GetConverter(instanceType, overrideConverterType);
            if ( instanceTypeConverter == null ) {
                data = new fsData();
                // return fsResult.Warn(string.Format("No converter for {0}", instanceType));
                return fsResult.Success;
            }

            var needsCycleSupport = instanceType.RTIsDefined<fsSerializeAsReference>(true);
            if ( needsCycleSupport ) {
                // We've already serialized this object instance (or it is pending higher up on the call stack).
                // Just serialize a reference to it to escape the cycle.
                if ( _references.IsReference(instance) ) {
                    data = fsData.CreateDictionary();
                    _lazyReferenceWriter.WriteReference(_references.GetReferenceId(instance), data.AsDictionary);
                    return fsResult.Success;
                }

                // Mark inside the object graph that we've serialized the instance. We do this *before*
                // serialization so that if we get back into this function recursively, it'll already
                // be marked and we can handle the cycle properly without going into an infinite loop.
                _references.MarkSerialized(instance);
            }

            //push collector
            TryPush(instance);

            // Serialize the instance with it's actual instance type, not storageType.
            var serializeResult = instanceTypeConverter.TrySerialize(instance, out data, instanceType);

            //pop collector
            TryPop(instance);

            if ( serializeResult.Failed ) {
                return serializeResult;
            }

            // Do we need to add type information? If the field type and the instance type are different.
            if ( storageType != instanceType && GetConverter(storageType, overrideConverterType).RequestInheritanceSupport(storageType) ) {
                EnsureDictionary(ref data);
                data.AsDictionary[KEY_INSTANCE_TYPE] = new fsData(instanceType.FullName);
            }

            if ( needsCycleSupport ) {
                _lazyReferenceWriter.WriteDefinition(_references.GetReferenceId(instance), data);
            }

            return serializeResult;
        }


        ///----------------------------------------------------------------------------------------------

        ///<summary> Attempts to deserialize a value from a serialized state.</summary>
        public fsResult TryDeserialize(fsData data, Type storageType, ref object result) {
            return TryDeserialize(data, storageType, ref result, null);
        }

        ///<summary> Attempts to deserialize a value from a serialized state.</summary>
        public fsResult TryDeserialize(fsData data, Type storageType, ref object result, Type overrideConverterType) {

            if ( data.IsNull ) {
                result = null;
                var processors = GetProcessors(storageType);
                Invoke_OnBeforeDeserialize(processors, storageType, ref data);
                Invoke_OnAfterDeserialize(processors, storageType, null);
                return fsResult.Success;
            }

            try {
                _references.Enter();
                return Internal_Deserialize(data, storageType, ref result, overrideConverterType);
            }

            finally { _references.Exit(); }
        }

        //...
        fsResult Internal_Deserialize(fsData data, Type storageType, ref object result, Type overrideConverterType) {
            //$ref encountered. Do before inheritance.
            if ( IsObjectReference(data) ) {
                int refId = int.Parse(data.AsDictionary[KEY_OBJECT_REFERENCE].AsString);
                result = _references.GetReferenceObject(refId);
                return fsResult.Success;
            }

            var deserializeResult = fsResult.Success;
            var objectType = result != null ? result.GetType() : storageType;
            Type forwardMigrationPreviousType = null;

            // Gather processors and call OnBeforeDeserialize before anything
            var processors = GetProcessors(objectType);
            Invoke_OnBeforeDeserialize(processors, objectType, ref data);

            // If the serialized state contains type information, then we need to make sure to update our
            // objectType and data to the proper values so that when we construct an object instance later
            // and run deserialization we run it on the proper type.
            // $type
            if ( IsTypeSpecified(data) ) {
                var typeNameData = data.AsDictionary[KEY_INSTANCE_TYPE];

                do {
                    if ( !typeNameData.IsString ) {
                        deserializeResult.AddMessage(string.Format("{0} value must be a string", KEY_INSTANCE_TYPE));
                        break;
                    }

                    var typeName = typeNameData.AsString;
                    var type = ReflectionTools.GetType(typeName, storageType);

                    if ( type == null ) {
                        deserializeResult.AddMessage(string.Format("{0} type can not be resolved", typeName));
                        break;
                    }

                    var migrateAtt = type.RTGetAttribute<fsMigrateToAttribute>(true);
                    if ( migrateAtt != null ) {
                        // if migrating from another type, save the original type and mutate the current type
                        if ( !typeof(IMigratable).IsAssignableFrom(migrateAtt.targetType) ) {
                            throw new Exception("TargetType of [fsMigrateToAttribute] must implement IMigratable<T> with T being the target type");
                        }
                        forwardMigrationPreviousType = type;
                        if ( type.IsGenericType && migrateAtt.targetType.IsGenericTypeDefinition ) {
                            type = migrateAtt.targetType.MakeGenericType(type.GetGenericArguments());
                        } else { type = migrateAtt.targetType; }
                    }

                    if ( !storageType.IsAssignableFrom(type) ) {
                        deserializeResult.AddMessage(string.Format("Ignoring type specifier. Field or type {0} can't hold and instance of type {1}", storageType, type));
                        break;
                    }

                    objectType = type;

                } while ( false );
            }

            var converter = GetConverter(objectType, overrideConverterType);
            if ( converter == null ) {
                return fsResult.Warn(string.Format("No Converter available for {0}", objectType));
            }

            // Construct an object instance if we don't have one already using actual objectType
            if ( ReferenceEquals(result, null) || result.GetType() != objectType ) {
                result = converter.CreateInstance(data, objectType);
            }

            // if migrating from another type, do migration now.
            if ( forwardMigrationPreviousType != null ) {
                //we deserialize versioning first on the old model type and then do migration
                var previousInstance = GetConverter(forwardMigrationPreviousType, null).CreateInstance(data, forwardMigrationPreviousType);
                TryDeserializeVersioning(ref previousInstance, ref data);
                TryDeserializeMigration(ref result, ref data, forwardMigrationPreviousType, previousInstance);
            } else {
                // if not a forward migration, try deserialize versioning as normal
                TryDeserializeVersioning(ref result, ref data);
            }

            // invoke callback with objectType
            Invoke_OnBeforeDeserializeAfterInstanceCreation(processors, objectType, result, ref data);

            // $id
            if ( IsObjectDefinition(data) ) {
                var sourceId = int.Parse(data.AsDictionary[KEY_OBJECT_DEFINITION].AsString);
                _references.AddReferenceWithId(sourceId, result);
            }

            // $content
            if ( IsWrappedData(data) ) {
                data = data.AsDictionary[KEY_CONTENT];
            }

            // push collector
            TryPush(result);

            // must pass actual objectType
            deserializeResult += converter.TryDeserialize(data, ref result, objectType);
            if ( deserializeResult.Succeeded ) { Invoke_OnAfterDeserialize(processors, objectType, result); }

            // pop collector
            TryPop(result);

            return deserializeResult;
        }

        ///----------------------------------------------------------------------------------------------

        ///<summary>Push collector to stack</summary>
        void TryPush(object o) {

            if ( o is ISerializationCollectable ) {
                _collectableDepth++;
                if ( _collectors.Count > 0 ) {
                    _collectors.Peek().OnCollect((ISerializationCollectable)o, _collectableDepth);
                }
            }

            //collector is also collectable, so we start at depth 0
            if ( o is ISerializationCollector ) {
                _collectableDepth = -1;
                var parent = _collectors.Count > 0 ? _collectors.Peek() : null;
                _collectors.Push((ISerializationCollector)o);
                ( (ISerializationCollector)o ).OnPush(parent);
            }
        }

        ///<summary>Pop collector in stack and/or collect object</summary>
        void TryPop(object o) {

            //collector is also collectable, so we collect at depth 0
            if ( o is ISerializationCollector ) {
                _collectableDepth = 0;
                _collectors.Pop().OnPop(_collectors.Count > 0 ? _collectors.Peek() : null);
            }

            if ( o is ISerializationCollectable ) {
                _collectableDepth--;
            }
        }

        ///----------------------------------------------------------------------------------------------
        //This is an alternative idea with collection happening AFTER serialization/deserialization
        //There can probably be two callbacks OnBefore and OnAfter?
        // ///<summary>Push collector to stack</summary>
        // void TryPush(object o) {
        //     if ( o is ISerializationCollector ) {
        //         _collectableDepth = -1;
        //         var parent = _collectors.Count > 0 ? _collectors.Peek() : null;
        //         _collectors.Push((ISerializationCollector)o);
        //         ( (ISerializationCollector)o ).OnPush(parent);
        //     }

        //     //collector is also collectable, so we start at depth 0
        //     if ( o is ISerializationCollectable ) {
        //         _collectableDepth++;

        //     }
        // }

        // ///<summary>Pop collector in stack and/or collect object</summary>
        // void TryPop(object o) {
        //     if ( o is ISerializationCollector ) {
        //         _collectableDepth = 1;
        //         _collectors.Pop().OnPop(_collectors.Count > 0 ? _collectors.Peek() : null);
        //     }

        //     //collector is also collectable, so we collect at depth 0
        //     if ( o is ISerializationCollectable ) {
        //         _collectableDepth--;
        //         if ( _collectors.Count > 0 ) {
        //             _collectors.Peek().OnCollect((ISerializationCollectable)o, _collectableDepth);
        //         }
        //     }
        // }
        ///----------------------------------------------------------------------------------------------

        ///<summary>Version migration serialize</summary>
        void TrySerializeVersioning(object currentInstance, ref fsData data) {
            if ( currentInstance is IMigratable && data.IsDictionary ) {
                var att = currentInstance.GetType().RTGetAttribute<fsMigrateVersionsAttribute>(true);
                if ( att != null && att.previousTypes.Length > 0 ) {
                    data.AsDictionary[KEY_VERSION] = new fsData(att.previousTypes.Length);
                }
            }
        }

        ///<summary>Version migration deserialize</summary>
        void TryDeserializeVersioning(ref object currentInstance, ref fsData currentData) {
            if ( currentInstance is IMigratable && currentData.IsDictionary ) {

                var instanceType = currentInstance.GetType();
                fsData serializedVersionData;
                int serializedVersion = 0;
                if ( currentData.AsDictionary.TryGetValue(KEY_VERSION, out serializedVersionData) ) {
                    serializedVersion = (int)serializedVersionData.AsInt64;
                }

                var att = instanceType.RTGetAttribute<fsMigrateVersionsAttribute>(true);
                if ( att != null ) {
                    var previousTypes = att.previousTypes;
                    var currentVersion = previousTypes.Length;

                    if ( currentVersion > serializedVersion ) {
                        var previousType = previousTypes[serializedVersion];
                        TryDeserializeMigration(ref currentInstance, ref currentData, previousType, null);
                    }
                }
            }
        }

        ///<summary>Create instance of previous type, deserialize it with previous data and call Migrate to currentInstance</summary>
        void TryDeserializeMigration(ref object currentInstance, ref fsData currentData, Type previousType, object previousInstance) {
            if ( currentInstance is IMigratable && currentData.IsDictionary ) {

                var instanceType = currentInstance.GetType();
                if ( instanceType.IsGenericType && previousType.IsGenericTypeDefinition ) {
                    previousType = previousType.MakeGenericType(instanceType.GetGenericArguments());
                }

                System.Reflection.InterfaceMapping interfaceMap;
                try { interfaceMap = instanceType.GetInterfaceMap(typeof(IMigratable<>).MakeGenericType(previousType)); }
                catch ( Exception e ) { throw new Exception("Type must implement IMigratable<T> for each one of the types specified in the [fsMigrateVersionsAttribute] or [fsMigrateToAttribute]\n" + e.Message); }
                var migrateMethod = interfaceMap.InterfaceMethods.First(m => m.Name == nameof(IMigratable<object>.Migrate));

                //create previous instance and deserialize through converter only
                var converter = GetConverter(previousType, null);
                if ( previousInstance == null ) { previousInstance = converter.CreateInstance(currentData, previousType); }
                converter.TryDeserialize(currentData, ref previousInstance, previousType).AssertSuccess();
                migrateMethod.Invoke(currentInstance, ReflectionTools.SingleTempArgsArray(previousInstance));

                fsData serializedData;
                //we serialize the previous instance then remove all serialization keys from the original data that will
                //be used for deserialization on the current instance down later. This way we dont overwrite the migration.
                converter.TrySerialize(previousInstance, out serializedData, previousType).AssertSuccess();
                foreach ( var pair in serializedData.AsDictionary ) { currentData.AsDictionary.Remove(pair.Key); }
            }
        }
    }
}