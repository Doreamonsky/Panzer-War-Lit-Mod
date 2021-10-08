using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace ParadoxNotion.Serialization.FullSerializer.Internal
{
    public class fsListConverter : fsConverter
    {

        public override bool CanProcess(Type type) {
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>);
        }

        public override object CreateInstance(fsData data, Type storageType) {
            return fsMetaType.Get(storageType).CreateInstance();
        }

        public override fsResult TrySerialize(object instance_, out fsData serialized, Type storageType) {
            var instance = (IList)instance_;
            var result = fsResult.Success;

            var elementType = storageType.RTGetGenericArguments()[0];
            serialized = fsData.CreateList(instance.Count);
            var serializedList = serialized.AsList;

            for ( var i = 0; i < instance.Count; i++ ) {
                var item = instance[i];
                fsData itemData;

                // auto instance?
                if ( item == null && elementType.RTIsDefined<fsAutoInstance>(true) ) {
                    item = fsMetaType.Get(elementType).CreateInstance();
                    instance[i] = item;
                }

                var itemResult = Serializer.TrySerialize(elementType, item, out itemData);
                result.AddMessages(itemResult);
                if ( itemResult.Failed ) continue;
                serializedList.Add(itemData);
            }
            return result;
        }

        public override fsResult TryDeserialize(fsData data, ref object instance_, Type storageType) {
            var instance = (IList)instance_;
            var result = fsResult.Success;

            if ( ( result += CheckType(data, fsDataType.Array) ).Failed ) {
                return result;
            }

            if ( data.AsList.Count == 0 ) {
                return fsResult.Success;
            }

            var elementType = storageType.RTGetGenericArguments()[0];
            //if we have the exact same count, deserialize overwrite
            if ( instance.Count == data.AsList.Count && fsMetaType.Get(elementType).DeserializeOverwriteRequest ) {
                for ( var i = 0; i < data.AsList.Count; i++ ) {
                    object item = instance[i];
                    var itemResult = Serializer.TryDeserialize(data.AsList[i], elementType, ref item);
                    if ( itemResult.Failed ) continue;
                    instance[i] = item;
                }
                return fsResult.Success;
            }

            //otherwise clear and start anew
            instance.Clear();
            var capacityProperty = instance.GetType().RTGetProperty("Capacity");
            capacityProperty.SetValue(instance, data.AsList.Count);
            for ( var i = 0; i < data.AsList.Count; i++ ) {
                object item = null;
                var itemResult = Serializer.TryDeserialize(data.AsList[i], elementType, ref item);
                if ( itemResult.Failed ) continue;
                instance.Add(item);
            }
            return fsResult.Success;
        }
    }
}