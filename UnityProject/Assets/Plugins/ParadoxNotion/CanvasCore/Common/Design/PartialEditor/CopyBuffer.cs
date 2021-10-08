#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using ParadoxNotion.Serialization;

namespace ParadoxNotion.Design
{

    ///A very simple pool to handle Copy/Pasting
    public static class CopyBuffer
    {

        private static Dictionary<Type, string> cachedCopies = new Dictionary<Type, string>();
        private static Dictionary<Type, object> cachedObjects = new Dictionary<Type, object>();

        public static void FlushMem() {
            cachedCopies = new Dictionary<Type, string>();
            cachedObjects = new Dictionary<Type, object>();
        }

        ///Is copy available?
        public static bool Has<T>() {
            return ( cachedCopies.TryGetValue(typeof(T), out string json) );
        }

        ///Returns true if copy exist and the copy
        public static bool TryGet<T>(out T copy) {
            copy = Get<T>();
            return object.Equals(copy, default(T)) == false;
        }

        ///Returns a copy
        public static T Get<T>() {
            if ( cachedCopies.TryGetValue(typeof(T), out string json) ) {
                return JSONSerializer.Deserialize<T>(json);
            }
            return default(T);
        }

        ///Sets a copy
        public static void Set<T>(T obj) {
            cachedCopies[typeof(T)] = JSONSerializer.Serialize(typeof(T), obj); ;
        }

        ///----------------------------------------------------------------------------------------------

        ///
        public static bool HasCache<T>() {
            return ( cachedObjects.TryGetValue(typeof(T), out object obj) );
        }

        ///
        public static bool TryGetCache<T>(out T copy) {
            copy = GetCache<T>();
            return object.Equals(copy, default(T)) == false;
        }

        ///
        public static T GetCache<T>() {
            if ( cachedObjects.TryGetValue(typeof(T), out object obj) ) {
                return (T)obj;
            }
            return default(T);
        }

        ///
        public static void SetCache<T>(T obj) {
            cachedObjects[typeof(T)] = obj;
        }
    }
}

#endif