using System;
using System.Collections;
using System.Collections.Generic;

namespace ParadoxNotion.Serialization.FullSerializer.Internal
{
    public class fsReflectedConverter : fsConverter
    {

        public override bool CanProcess(Type type) {
            if ( type.IsArray || typeof(ICollection).IsAssignableFrom(type) ) {
                return false;
            }
            return true;
        }

        public override fsResult TrySerialize(object instance, out fsData serialized, Type storageType) {
            serialized = fsData.CreateDictionary();
            var result = fsResult.Success;

            fsMetaType metaType = fsMetaType.Get(instance.GetType());

            //Dont do this for UnityObject. While there is fsUnityObjectConverter, this converter is also used as override,
            //when serializing a UnityObject directly.
            object defaultInstance = null;
            if ( fsGlobalConfig.SerializeDefaultValues == false && !( instance is UnityEngine.Object ) ) {
                defaultInstance = metaType.GetDefaultInstance();
            }

            for ( int i = 0; i < metaType.Properties.Length; ++i ) {
                fsMetaProperty property = metaType.Properties[i];

                if ( property.WriteOnly ) {
                    continue;
                }

                if ( property.AsReference && Serializer.IgnoreSerializeCycleReferences ) {
                    continue;
                }

                var propertyValue = property.Read(instance);

                // auto instance?
                if ( propertyValue == null && property.AutoInstance ) {
                    propertyValue = fsMetaType.Get(property.StorageType).CreateInstance();
                    property.Write(instance, propertyValue);
                } else if ( fsGlobalConfig.SerializeDefaultValues == false && defaultInstance != null ) {
                    if ( Equals(propertyValue, property.Read(defaultInstance)) ) {
                        continue;
                    }
                }

                fsData serializedData;
                var itemResult = Serializer.TrySerialize(property.StorageType, propertyValue, out serializedData);
                result.AddMessages(itemResult);
                if ( itemResult.Failed ) {
                    continue;
                }

                serialized.AsDictionary[property.JsonName] = serializedData;
            }

            return result;
        }

        public override fsResult TryDeserialize(fsData data, ref object instance, Type storageType) {
            var result = fsResult.Success;

            // Verify that we actually have an Object
            if ( ( result += CheckType(data, fsDataType.Object) ).Failed ) {
                return result;
            }

            if ( data.AsDictionary.Count == 0 ) {
                return fsResult.Success;
            }

            fsMetaType metaType = fsMetaType.Get(storageType);

            for ( int i = 0; i < metaType.Properties.Length; ++i ) {
                fsMetaProperty property = metaType.Properties[i];

                if ( property.ReadOnly ) {
                    continue;
                }

                fsData propertyData;
                if ( data.AsDictionary.TryGetValue(property.JsonName, out propertyData) ) {
                    object deserializedValue = null;

                    //This does not work well with no serializing default values -> Find a workaround.
                    if ( fsGlobalConfig.SerializeDefaultValues ) {
                        if ( metaType.DeserializeOverwriteRequest || typeof(ICollection).IsAssignableFrom(storageType) ) {
                            deserializedValue = property.Read(instance);
                        }
                    }

                    var itemResult = Serializer.TryDeserialize(propertyData, property.StorageType, ref deserializedValue, null);
                    result.AddMessages(itemResult);
                    if ( itemResult.Failed ) continue;

                    property.Write(instance, deserializedValue);
                }
            }

            return result;
        }

        public override object CreateInstance(fsData data, Type storageType) {
            fsMetaType metaType = fsMetaType.Get(storageType);
            return metaType.CreateInstance();
        }
    }
}