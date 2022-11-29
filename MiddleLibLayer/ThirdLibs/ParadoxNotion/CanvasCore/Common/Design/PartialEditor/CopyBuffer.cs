#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using ParadoxNotion.Serialization;

namespace ParadoxNotion.Design
{

    ///<summary>A very simple pool to handle Copy/Pasting</summary>
    public static class CopyBuffer
    {

        private static Dictionary<Type, string> cachedCopies = new Dictionary<Type, string>();
        private static Dictionary<Type, object> cachedObjects = new Dictionary<Type, object>();

        public static void FlushMem() {
            cachedCopies = new Dictionary<Type, string>();
            cachedObjects = new Dictionary<Type, object>();
        }

        ///<summary>Is copy available?</summary>
        public static bool Has<T>() {
            return ( cachedCopies.TryGetValue(typeof(T), out string json) );
        }

        ///<summary>Returns true if copy exist and the copy</summary>
        public static bool TryGet<T>(out T copy) {
            copy = Get<T>();
            return object.Equals(copy, default(T)) == false;
        }

        ///<summary>Returns a copy</summary>
        public static T Get<T>() {
            if ( cachedCopies.TryGetValue(typeof(T), out string json) ) {
                return JSONSerializer.Deserialize<T>(json);
            }
            return default(T);
        }

        ///<summary>Sets a copy</summary>
        public static void Set<T>(T obj) {
            cachedCopies[typeof(T)] = JSONSerializer.Serialize(typeof(T), obj); ;
        }

        ///----------------------------------------------------------------------------------------------

        ///<summary></summary>
        public static bool HasCache<T>() {
            return ( cachedObjects.TryGetValue(typeof(T), out object obj) );
        }

        ///<summary></summary>
        public static bool TryGetCache<T>(out T copy) {
            copy = GetCache<T>();
            return object.Equals(copy, default(T)) == false;
        }

        ///<summary></summary>
        public static T GetCache<T>() {
            if ( cachedObjects.TryGetValue(typeof(T), out object obj) ) {
                return (T)obj;
            }
            return default(T);
        }

        ///<summary></summary>
        public static void SetCache<T>(T obj) {
            cachedObjects[typeof(T)] = obj;
        }
    }
}

#endif