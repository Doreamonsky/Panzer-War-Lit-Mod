#if UNITY_EDITOR

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityObject = UnityEngine.Object;
using Logger = ParadoxNotion.Services.Logger;

namespace ParadoxNotion.Design
{

    ///<summary> Specific Editor GUIs</summary>
	partial class EditorUtils
    {

        ///<summary>Stores fold states</summary>
		private static readonly Dictionary<Type, bool> registeredEditorFoldouts = new Dictionary<Type, bool>();


        ///<summary>A cool label :-P (for headers)</summary>
        public static void CoolLabel(string text) {
            GUI.skin.label.richText = true;
            GUI.color = Colors.lightOrange;
            GUILayout.Label("<b><size=14>" + text + "</size></b>", Styles.topLeftLabel);
            GUI.color = Color.white;
            GUILayout.Space(2);
        }

        ///<summary>Combines the rest functions for a header style label</summary>
        public static void TitledSeparator(string title) {
            GUILayout.Space(1);
            BoldSeparator();
            CoolLabel(title + " ▼");
            Separator();
        }

        ///<summary>A thin separator</summary>
        public static void Separator() {
            var lastRect = GUILayoutUtility.GetLastRect();
            GUILayout.Space(7);
            GUI.color = new Color(0, 0, 0, 0.3f);
            GUI.DrawTexture(Rect.MinMaxRect(lastRect.xMin, lastRect.yMax + 4, lastRect.xMax, lastRect.yMax + 6), Texture2D.whiteTexture);
            GUI.color = Color.white;
        }

        ///<summary>A thick separator</summary>
        public static void BoldSeparator() {
            var lastRect = GUILayoutUtility.GetLastRect();
            GUILayout.Space(14);
            GUI.color = new Color(0, 0, 0, 0.3f);
            GUI.DrawTexture(new Rect(0, lastRect.yMax + 6, Screen.width, 4), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(0, lastRect.yMax + 6, Screen.width, 1), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(0, lastRect.yMax + 9, Screen.width, 1), Texture2D.whiteTexture);
            GUI.color = Color.white;
        }

        ///<summary>Just a fancy ending for inspectors</summary>
        public static void EndOfInspector() {
            var lastRect = GUILayoutUtility.GetLastRect();
            GUILayout.Space(8);
            GUI.color = new Color(0, 0, 0, 0.4f);
            GUI.DrawTexture(new Rect(0, lastRect.yMax + 6, Screen.width, 4), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(0, lastRect.yMax + 4, Screen.width, 1), Texture2D.whiteTexture);
            GUI.color = Color.white;
        }

        ///<summary>A Search Field</summary>
        public static string SearchField(string search) {
            GUILayout.BeginHorizontal();
            search = EditorGUILayout.TextField(search, Styles.toolbarSearchTextField);
            if ( GUILayout.Button(string.Empty, Styles.toolbarSearchCancelButton) ) {
                search = string.Empty;
                GUIUtility.keyboardControl = 0;
            }
            GUILayout.EndHorizontal();
            return search;
        }

        ///<summary>Used just after a textfield with no prefix to show an italic transparent text inside when empty</summary>
        public static void CommentLastTextField(string check, string comment = "Comments...") {
            if ( string.IsNullOrEmpty(check) ) {
                var lastRect = GUILayoutUtility.GetLastRect();
                GUI.Label(lastRect, " <i>" + comment + "</i>", Styles.topLeftLabel);
            }
        }

        ///<summary>Used just after a field to highlight it</summary>
        public static void HighlightLastField() {
            var lastRect = GUILayoutUtility.GetLastRect();
            lastRect.xMin += 2;
            lastRect.xMax -= 2;
            lastRect.yMax -= 4;
            Styles.Draw(lastRect, Styles.highlightBox);
        }

        ///<summary>Used just after a field to mark it as a prefab override (similar to native unity's one)</summary>
        public static void MarkLastFieldOverride() {
            var rect = GUILayoutUtility.GetLastRect();
            rect.x -= 3; rect.width = 2;
            GUI.color = Colors.prefabOverrideColor;
            GUI.DrawTexture(rect, Texture2D.whiteTexture);
            GUI.color = Color.white;
        }

        ///<summary>Used just after a field to mark warning icon to it</summary>
        public static void MarkLastFieldWarning(string tooltip) {
            Internal_MarkLastField(ParadoxNotion.Design.Icons.warningIcon, tooltip);
        }

        ///<summary>Used just after a field to mark warning icon to it</summary>
        public static void MarkLastFieldError(string tooltip) {
            Internal_MarkLastField(ParadoxNotion.Design.Icons.errorIcon, tooltip);
        }

        //...
        static void Internal_MarkLastField(Texture2D icon, string tooltip) {
            var rect = GUILayoutUtility.GetLastRect();
            rect.x += UnityEditor.EditorGUIUtility.labelWidth;
            rect.x -= 16;
            rect.y += 1;
            rect.width = 16;
            rect.height = 16;
            GUI.Box(rect, EditorUtils.GetTempContent(null, icon, tooltip), GUIStyle.none);
        }

        // public static Rect BeginHighlightArea() {
        //     var rect = GUILayoutUtility.GetLastRect();
        //     GUILayout.BeginVertical();
        //     return rect;
        // }

        // public static void EndHighlightArea(Rect beginRect) {
        //     GUILayout.EndVertical();
        //     var last = GUILayoutUtility.GetLastRect();
        //     var rect = Rect.MinMaxRect(beginRect.xMin, beginRect.yMin, last.xMax, last.yMax);
        //     Styles.Draw(rect, Styles.highlightBox);
        // }

        ///<summary>Editor for LayerMask</summary>
		public static LayerMask LayerMaskField(string prefix, LayerMask layerMask, params GUILayoutOption[] layoutOptions) {
            return LayerMaskField(string.IsNullOrEmpty(prefix) ? GUIContent.none : new GUIContent(prefix), layerMask, layoutOptions);
        }

        ///<summary>Editor for LayerMask</summary>
        public static LayerMask LayerMaskField(GUIContent content, LayerMask layerMask, params GUILayoutOption[] layoutOptions) {
            LayerMask tempMask = EditorGUILayout.MaskField(content, UnityEditorInternal.InternalEditorUtility.LayerMaskToConcatenatedLayersMask(layerMask), UnityEditorInternal.InternalEditorUtility.layers, layoutOptions);
            layerMask = UnityEditorInternal.InternalEditorUtility.ConcatenatedLayersMaskToLayerMask(tempMask);
            return layerMask;
        }

        ///<summary>Do a cached editor Foldout based on provided key object</summary>
        public static bool CachedFoldout(Type key, GUIContent content) {
            var foldout = false;
            registeredEditorFoldouts.TryGetValue(key, out foldout);
            foldout = EditorGUILayout.Foldout(foldout, content);
            return registeredEditorFoldouts[key] = foldout;
        }

        ///<summary>An IList editor (List<T> and Arrays)</summary>
        public static IList ListEditor(GUIContent content, IList list, Type listType, InspectedFieldInfo info) {

            var optionsAtt = info.attributes?.FirstOrDefault(x => x is ListInspectorOptionAttribute) as ListInspectorOptionAttribute;

            var argType = listType.GetEnumerableElementType();
            if ( argType == null ) {
                return list;
            }

            if ( object.Equals(list, null) ) {
                GUILayout.Label("Null List");
                return list;
            }

            if ( optionsAtt == null || optionsAtt.showFoldout ) {
                if ( !CachedFoldout(listType, content) ) {
                    return list;
                }
            } else {
                GUILayout.Label(content.text);
            }

            GUILayout.BeginVertical();
            EditorGUI.indentLevel++;

            var options = new ReorderableListOptions();
            options.allowAdd = optionsAtt == null || optionsAtt.allowAdd;
            options.allowRemove = optionsAtt == null || optionsAtt.allowRemove;
            options.unityObjectContext = info.unityObjectContext;
            list = EditorUtils.ReorderableList(list, options, (i, r) =>
            {
                list[i] = ReflectedFieldInspector("Element " + i, list[i], argType, info);
            });

            EditorGUI.indentLevel--;
            Separator();
            GUILayout.EndVertical();
            return list;
        }

        ///<summary>A IDictionary editor</summary>
        public static IDictionary DictionaryEditor(GUIContent content, IDictionary dict, Type dictType, InspectedFieldInfo info) {

            var keyType = dictType.RTGetGenericArguments()[0];
            var valueType = dictType.RTGetGenericArguments()[1];

            if ( object.Equals(dict, null) ) {
                GUILayout.Label("Null Dictionary");
                return dict;
            }

            if ( !CachedFoldout(dictType, content) ) {
                return dict;
            }

            GUILayout.BeginVertical();

            var keys = dict.Keys.Cast<object>().ToList();
            var values = dict.Values.Cast<object>().ToList();

            if ( GUILayout.Button("Add Element") ) {
                if ( !typeof(UnityObject).IsAssignableFrom(keyType) ) {
                    object newKey = null;
                    if ( keyType == typeof(string) ) {
                        newKey = string.Empty;
                    } else {
                        newKey = Activator.CreateInstance(keyType);
                    }

                    if ( dict.Contains(newKey) ) {
                        Logger.LogWarning(string.Format("Key '{0}' already exists in Dictionary", newKey.ToString()), "Editor");
                        return dict;
                    }

                    keys.Add(newKey);

                } else {
                    Logger.LogWarning("Can't add a 'null' Dictionary Key", "Editor");
                    return dict;
                }

                values.Add(valueType.IsValueType ? Activator.CreateInstance(valueType) : null);
            }

            for ( var i = 0; i < keys.Count; i++ ) {
                GUILayout.BeginHorizontal("box");
                GUILayout.Box("", GUILayout.Width(6), GUILayout.Height(35));

                GUILayout.BeginVertical();
                keys[i] = ReflectedFieldInspector("K:", keys[i], keyType, info);
                values[i] = ReflectedFieldInspector("V:", values[i], valueType, info);
                GUILayout.EndVertical();

                if ( GUILayout.Button("X", GUILayout.Width(18), GUILayout.Height(34)) ) {
                    keys.RemoveAt(i);
                    values.RemoveAt(i);
                }

                GUILayout.EndHorizontal();
            }

            //clear and reconstruct on separate pass after GUI controls
            dict.Clear();
            for ( var i = 0; i < keys.Count; i++ ) {
                try { dict.Add(keys[i], values[i]); }
                catch { Logger.Log("Dictionary Key removed due to duplicate found", "Editor"); }
            }

            Separator();

            GUILayout.EndVertical();
            return dict;
        }


        ///<summary>An editor field where if the component is null simply shows an object field, but if its not, shows a dropdown popup to select the specific component from within the gameobject</summary>
        public static Component ComponentField(GUIContent content, Component comp, Type type, params GUILayoutOption[] GUIOptions) {

            if ( comp == null ) {
                return EditorGUILayout.ObjectField(content, comp, type, true, GUIOptions) as Component;
            }

            var components = comp.GetComponents(type).ToList();
            var componentNames = components.Where(c => c != null).Select(c => c.GetType().FriendlyName() + " (" + c.gameObject.name + ")").ToList();
            componentNames.Insert(0, "[NONE]");

            var index = components.IndexOf(comp);
            index = EditorGUILayout.Popup(content, index, componentNames.Select(n => new GUIContent(n)).ToArray(), GUIOptions);
            return index == 0 ? null : components[index];
        }


        ///<summary>A popup that is based on the string rather than the index</summary>
        public static string StringPopup(GUIContent content, string selected, IEnumerable<string> options, params GUILayoutOption[] GUIOptions) {
            EditorGUILayout.BeginVertical();
            var index = 0;
            var copy = new List<string>(options);
            copy.Insert(0, "[NONE]");
            index = copy.Contains(selected) ? copy.IndexOf(selected) : 0;
            index = EditorGUILayout.Popup(content, index, copy.Select(n => new GUIContent(n)).ToArray(), GUIOptions);
            EditorGUILayout.EndVertical();
            return index == 0 ? string.Empty : copy[index];
        }


        ///<summary>Generic Popup for selection of any element within a list</summary>
        public static T Popup<T>(T selected, IEnumerable<T> options, params GUILayoutOption[] GUIOptions) {
            return Popup<T>(GUIContent.none, selected, options, GUIOptions);
        }

        ///<summary>Generic Popup for selection of any element within a list</summary>
        public static T Popup<T>(string prefix, T selected, IEnumerable<T> options, params GUILayoutOption[] GUIOptions) {
            return Popup<T>(string.IsNullOrEmpty(prefix) ? GUIContent.none : new GUIContent(prefix), selected, options, GUIOptions);
        }

        ///<summary>Generic Popup for selection of any element within a list</summary>
        public static T Popup<T>(GUIContent content, T selected, IEnumerable<T> options, params GUILayoutOption[] GUIOptions) {
            var listOptions = new List<T>(options);
            listOptions.Insert(0, default(T));
            var stringedOptions = new List<string>(listOptions.Select(o => o != null ? o.ToString() : "[NONE]"));
            stringedOptions[0] = listOptions.Count == 1 ? "[NONE AVAILABLE]" : "[NONE]";

            var index = 0;
            if ( listOptions.Contains(selected) ) {
                index = listOptions.IndexOf(selected);
            }

            var wasEnable = GUI.enabled;
            GUI.enabled = wasEnable && stringedOptions.Count > 1;
            index = EditorGUILayout.Popup(content, index, stringedOptions.Select(s => new GUIContent(s)).ToArray(), GUIOptions);
            GUI.enabled = wasEnable;
            return index == 0 ? default(T) : listOptions[index];
        }


        ///<summary>Generic Button Popup for selection of any element within a list</summary>
        public static void ButtonPopup<T>(string prefix, T selected, List<T> options, Action<T> Callback) {
            ButtonPopup<T>(string.IsNullOrEmpty(prefix) ? GUIContent.none : new GUIContent(prefix), selected, options, Callback);
        }

        ///<summary>Generic Button Popup for selection of any element within a list</summary>
        public static void ButtonPopup<T>(GUIContent content, T selected, List<T> options, Action<T> Callback) {
            var buttonText = selected != null ? selected.ToString() : "[NONE]";
            GUILayout.BeginHorizontal();
            if ( content != null && content != GUIContent.none ) {
                GUILayout.Label(content, GUILayout.Width(0), GUILayout.ExpandWidth(true));
            }
            if ( GUILayout.Button(buttonText, (GUIStyle)"MiniPopup", GUILayout.Width(0), GUILayout.ExpandWidth(true)) ) {
                var menu = new GenericMenu();
                foreach ( var _option in options ) {
                    var option = _option;
                    menu.AddItem(new GUIContent(option != null ? option.ToString() : "[NONE]"), object.Equals(selected, option), () => { Callback(option); });
                }
                menu.ShowAsBrowser("Select Option");
            }
            GUILayout.EndHorizontal();
        }

        ///<summary>Specialized Type button popup</summary>
        public static void ButtonTypePopup(string prefix, Type selected, Action<Type> Callback) {
            ButtonTypePopup(string.IsNullOrEmpty(prefix) ? GUIContent.none : new GUIContent(prefix), selected, Callback);
        }

        ///<summary>Specialized Type button popup</summary>
        public static void ButtonTypePopup(GUIContent content, Type selected, Action<Type> Callback) {
            var buttonText = selected != null ? selected.FriendlyName() : "[NONE]";
            GUILayout.BeginHorizontal();
            if ( content != null && content != GUIContent.none ) {
                GUILayout.Label(content, GUILayout.Width(0), GUILayout.ExpandWidth(true));
            }
            if ( GUILayout.Button(buttonText, (GUIStyle)"MiniPopup", GUILayout.Width(0), GUILayout.ExpandWidth(true)) ) {
                var menu = EditorUtils.GetPreferedTypesSelectionMenu(typeof(object), Callback);
                menu.ShowAsBrowser("Select Type");
            }
            GUILayout.EndHorizontal();
        }
    }
}

#endif