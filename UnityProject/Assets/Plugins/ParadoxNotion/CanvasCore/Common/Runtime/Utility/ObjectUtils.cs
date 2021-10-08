using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ParadoxNotion
{
    public static class ObjectUtils
    {
        ///Equals and ReferenceEquals check with added special treat for Unity Objects
		public static bool AnyEquals(object a, object b) {

            //regardless calling ReferenceEquals, unity is still doing magic and this is the only true solution (I've found)
            if ( ( a is UnityEngine.Object || a == null ) && ( b is UnityEngine.Object || b == null ) ) {
                return a as UnityEngine.Object == b as UnityEngine.Object;
            }

            //while '==' is reference equals, we still use '==' for when one is unity object and the other is not
            return a == b || object.Equals(a, b) || object.ReferenceEquals(a, b);
        }

        ///Fisher-Yates shuffle algorithm to shuffle lists
        public static List<T> Shuffle<T>(this List<T> list) {
            for ( var i = list.Count - 1; i > 0; i-- ) {
                var j = (int)Mathf.Floor(Random.value * ( i + 1 ));
                var temp = list[i];
                list[i] = list[j];
                list[j] = temp;
            }
            return list;
        }

        ///Quick way to check "is" and get a casted result
        public static bool Is<T>(this object o, out T result) {
            if ( o is T ) {
                result = (T)o;
                return true;
            }
            result = default(T);
            return false;
        }

        ///Gets component or adds it of it doesnt exist
        public static T GetAddComponent<T>(this GameObject gameObject) where T : Component {
            if ( gameObject == null ) { return null; }
            var result = gameObject.GetComponent<T>();
            if ( result == null ) {
                result = gameObject.AddComponent<T>();
            }
            return result;
        }

        ///"Transform" the component to target type from the same gameobject
        public static Component TransformToType(this Component current, System.Type type) {
            if ( current != null && type != null && !type.RTIsAssignableFrom(current.GetType()) ) {
                if ( type.RTIsSubclassOf(typeof(Component)) || type.RTIsInterface() ) {
                    current = current.GetComponent(type);
                }
            }
            return current;
        }

        ///Return all GameObjects within specified LayerMask, optionaly excluding specified GameObject
        public static IEnumerable<GameObject> FindGameObjectsWithinLayerMask(LayerMask mask, GameObject exclude = null) {
            return UnityEngine.Object.FindObjectsOfType<GameObject>().Where(x => x != exclude && x.IsInLayerMask(mask));
        }

        ///Return if GameObject is within specified LayerMask
        public static bool IsInLayerMask(this GameObject gameObject, LayerMask mask) {
            return mask == ( mask | ( 1 << gameObject.layer ) );
        }
    }
}