#if UNITY_EDITOR

using System;
using System.Collections;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityObject = UnityEngine.Object;


namespace ParadoxNotion.Design
{

    ///<summary>Automatic Inspector functions</summary>
	partial class EditorUtils
    {

        private static GUIContent tempContent;

        ///<summary>A cached temporary content</summary>
        public static GUIContent GetTempContent(string text = "", Texture image = null, string tooltip = null) {
            if ( tempContent == null ) { tempContent = new GUIContent(); }
            tempContent.text = text;
            tempContent.image = image;
            tempContent.tooltip = tooltip;
            return tempContent;
        }

        ///<summary>A cached temporary content</summary>
        public static GUIContent GetTempContent(Texture image = null, string tooltip = null) {
            return GetTempContent(null, image, tooltip);
        }

        ///<summary>Show an automatic editor GUI inspector for target object, taking into account drawer attributes</summary>
        public static void ReflectedObjectInspector(object target, UnityObject unityObjectContext) {

            if ( target == null ) {
                return;
            }

            var fields = target.GetType().RTGetFields();
            for ( var i = 0; i < fields.Length; i++ ) {
                var field = fields[i];

                //no statics
                if ( field.IsStatic ) { continue; }

                //hide type altogether?
                if ( field.FieldType.RTIsDefined(typeof(HideInInspector), true) ) { continue; }

                //inspect only public fields or private fields with the [ExposeField] attribute
                if ( field.IsPublic || field.RTIsDefined(typeof(ExposeFieldAttribute), true) ) {
                    var attributes = field.RTGetAllAttributes();
                    //Hide field?
                    if ( attributes.Any(a => a is HideInInspector) ) { continue; }
                    var serializationInfo = new InspectedFieldInfo(unityObjectContext, field, target, attributes);
                    var currentValue = field.GetValue(target);
                    var newValue = ReflectedFieldInspector(field.Name, currentValue, field.FieldType, serializationInfo);

                    var changed = !object.Equals(newValue, currentValue);
                    if ( changed ) { UndoUtility.RecordObject(unityObjectContext, field.Name); }
                    if ( changed || field.FieldType.IsValueType ) {
                        field.SetValue(target, newValue);
                    }
                    if ( changed ) { UndoUtility.SetDirty(unityObjectContext); }
                }
            }
        }


        ///<summary>Draws an Editor field for object of type directly WITH taking into acount object drawers and drawer attributes</summary>
        public static object ReflectedFieldInspector(string name, object value, Type t, InspectedFieldInfo info) {
            var content = GetTempContent(name.SplitCamelCase());
            if ( info.attributes != null ) {
                //Create proper GUIContent
                var nameAtt = info.attributes.FirstOrDefault(a => a is NameAttribute) as NameAttribute;
                if ( nameAtt != null ) { content.text = nameAtt.name; }

                var tooltipAtt = info.attributes.FirstOrDefault(a => a is TooltipAttribute) as TooltipAttribute;
                if ( tooltipAtt != null ) { content.tooltip = tooltipAtt.tooltip; }
            }

            return ReflectedFieldInspector(content, value, t, info);
        }

        ///<summary>Draws an Editor field for object of type directly WITH taking into acount object drawers and drawer attributes</summary>
        public static object ReflectedFieldInspector(GUIContent content, object value, Type t, InspectedFieldInfo info) {

            if ( t == null ) {
                GUILayout.Label("NO TYPE PROVIDED!");
                return value;
            }

            //Use drawers
            var objectDrawer = PropertyDrawerFactory.GetObjectDrawer(t);
            var newValue = objectDrawer.DrawGUI(content, value, info);
            var changed = !object.Equals(newValue, value);
            if ( changed ) { UndoUtility.RecordObjectComplete(info.unityObjectContext, content.text + "Field Change"); }
            value = newValue;
            if ( changed ) { UndoUtility.SetDirty(info.unityObjectContext); }
            return value;
        }


        ///<summary>Draws an Editor field for object of type directly WITHOUT taking into acount object drawers and drawer attributes unless provided</summary>
        public static object DrawEditorFieldDirect(GUIContent content, object value, Type t, InspectedFieldInfo info) {

            ///----------------------------------------------------------------------------------------------
            bool handled;
            var newValue = DirectFieldControl(content, value, t, info.unityObjectContext, info.attributes, out handled);
            var changed = !object.Equals(newValue, value);
            if ( changed ) { UndoUtility.RecordObjectComplete(info.unityObjectContext, content.text + "Field Change"); }
            value = newValue;
            if ( changed ) { UndoUtility.SetDirty(info.unityObjectContext); }
            if ( handled ) { return value; }
            ///----------------------------------------------------------------------------------------------


            if ( typeof(IList).IsAssignableFrom(t) ) {
                return ListEditor(content, (IList)value, t, info);
            }

            if ( typeof(IDictionary).IsAssignableFrom(t) ) {
                return DictionaryEditor(content, (IDictionary)value, t, info);
            }

            //show nested class members recursively, avoid all collections not handles above manually
            if ( value != null && ( t.IsClass || t.IsValueType ) && !typeof(ICollection).IsAssignableFrom(t) ) {

                if ( EditorGUI.indentLevel <= 8 ) {

                    if ( !CachedFoldout(t, content) ) {
                        return value;
                    }

                    EditorGUI.indentLevel++;
                    ReflectedObjectInspector(value, info.unityObjectContext);
                    EditorGUI.indentLevel--;
                }

            } else {

                EditorGUILayout.LabelField(content, GetTempContent(string.Format("NonInspectable ({0})", t.FriendlyName())));
            }

            return value;
        }

        //...
        public static object DirectFieldControl(GUIContent content, object value, Type t, UnityEngine.Object unityObjectContext, object[] attributes, out bool handled, params GUILayoutOption[] options) {

            handled = true;

            //Check scene object type for UnityObjects. Consider Interfaces as scene object type. Assume that user uses interfaces with UnityObjects
            if ( typeof(UnityObject).IsAssignableFrom(t) || t.IsInterface ) {
                if ( value == null || value is UnityObject ) { //check this to avoid case of interface but no unityobject
                    var isSceneObjectType = ( typeof(Component).IsAssignableFrom(t) || t == typeof(GameObject) || t.IsInterface );
                    var newValue = EditorGUILayout.ObjectField(content, (UnityObject)value, t, isSceneObjectType, options);
                    if ( unityObjectContext != null && newValue != null ) {
                        if ( !Application.isPlaying && EditorUtility.IsPersistent(unityObjectContext) && !EditorUtility.IsPersistent(newValue as UnityEngine.Object) ) {
                            ParadoxNotion.Services.Logger.LogWarning("Assets can not have scene object references", "Editor", unityObjectContext);
                            newValue = value as UnityObject;
                        }
                    }
                    return newValue;
                }
            }

            //Check Type second
            if ( t == typeof(Type) ) {
                return Popup<Type>(content, (Type)value, TypePrefs.GetPreferedTypesList(true), options);
            }

            //get real current type
            t = value != null ? value.GetType() : t;

            //for these just show type information
            if ( t.IsAbstract || t == typeof(object) || typeof(Delegate).IsAssignableFrom(t) || typeof(UnityEngine.Events.UnityEventBase).IsAssignableFrom(t) ) {
                EditorGUILayout.LabelField(content, new GUIContent(string.Format("({0})", t.FriendlyName())), options);
                return value;
            }

            //create instance for value types
            if ( value == null && t.RTIsValueType() ) {
                value = System.Activator.CreateInstance(t);
            }

            //create new instance with button for non value types
            if ( value == null && !t.IsAbstract && !t.IsInterface && ( t.IsArray || t.GetConstructor(Type.EmptyTypes) != null ) ) {
                if ( content != GUIContent.none ) {
                    GUILayout.BeginHorizontal();
                    EditorGUILayout.PrefixLabel(content, GUI.skin.button);
                }
                if ( GUILayout.Button("(null) Create", options) ) {
                    value = t.IsArray ? Array.CreateInstance(t.GetElementType(), 0) : Activator.CreateInstance(t);
                }
                if ( content != GUIContent.none ) { GUILayout.EndHorizontal(); }
                return value;
            }


            ///----------------------------------------------------------------------------------------------


            if ( t == typeof(string) ) {
                return EditorGUILayout.TextField(content, (string)value, options);
            }

            if ( t == typeof(char) ) {
                var c = (char)value;
                var s = c.ToString();
                s = EditorGUILayout.TextField(content, s, options);
                return string.IsNullOrEmpty(s) ? (char)c : (char)s[0];
            }

            if ( t == typeof(bool) ) {
                return EditorGUILayout.Toggle(content, (bool)value, options);
            }

            if ( t == typeof(int) ) {
                return EditorGUILayout.IntField(content, (int)value, options);
            }

            if ( t == typeof(float) ) {
                return EditorGUILayout.FloatField(content, (float)value, options);
            }

            if ( t == typeof(byte) ) {
                return Convert.ToByte(Mathf.Clamp(EditorGUILayout.IntField(content, (byte)value, options), 0, 255));
            }

            if ( t == typeof(long) ) {
                return EditorGUILayout.LongField(content, (long)value, options);
            }

            if ( t == typeof(double) ) {
                return EditorGUILayout.DoubleField(content, (double)value, options);
            }

            if ( t == typeof(Vector2) ) {
                return EditorGUILayout.Vector2Field(content, (Vector2)value, options);
            }

            if ( t == typeof(Vector2Int) ) {
                return EditorGUILayout.Vector2IntField(content, (Vector2Int)value, options);
            }

            if ( t == typeof(Vector3) ) {
                return EditorGUILayout.Vector3Field(content, (Vector3)value, options);
            }

            if ( t == typeof(Vector3Int) ) {
                return EditorGUILayout.Vector3IntField(content, (Vector3Int)value, options);
            }

            if ( t == typeof(Vector4) ) {
                return EditorGUILayout.Vector4Field(content, (Vector4)value, options);
            }

            if ( t == typeof(Quaternion) ) {
                var quat = (Quaternion)value;
                var vec4 = new Vector4(quat.x, quat.y, quat.z, quat.w);
                vec4 = EditorGUILayout.Vector4Field(content, vec4, options);
                return new Quaternion(vec4.x, vec4.y, vec4.z, vec4.w);
            }

            if ( t == typeof(Color) ) {
                var att = attributes?.FirstOrDefault(a => a is ColorUsageAttribute) as ColorUsageAttribute;
                var hdr = att != null ? att.hdr : false;
                var showAlpha = att != null ? att.showAlpha : true;
                return EditorGUILayout.ColorField(content, (Color)value, true, showAlpha, hdr, options);
            }

            if ( t == typeof(Gradient) ) {
                return EditorGUILayout.GradientField(content, (Gradient)value, options);
            }

            if ( t == typeof(Rect) ) {
                return EditorGUILayout.RectField(content, (Rect)value, options);
            }

            if ( t == typeof(AnimationCurve) ) {
                return EditorGUILayout.CurveField(content, (AnimationCurve)value, options);
            }

            if ( t == typeof(Bounds) ) {
                return EditorGUILayout.BoundsField(content, (Bounds)value, options);
            }

            if ( t == typeof(LayerMask) ) {
                return LayerMaskField(content, (LayerMask)value, options);
            }

            if ( t.IsSubclassOf(typeof(System.Enum)) ) {
                if ( t.RTIsDefined(typeof(FlagsAttribute), true) ) {
                    return EditorGUILayout.EnumFlagsField(content, (System.Enum)value, options);
                }
                return EditorGUILayout.EnumPopup(content, (System.Enum)value, options);
            }

            handled = false;
            return value;
        }

    }
}

#endif
