#if UNITY_EDITOR

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NodeCanvas.Framework;
using NodeCanvas.Framework.Internal;
using ParadoxNotion;
using ParadoxNotion.Design;
using UnityEditor;
using UnityEngine;


namespace NodeCanvas.Editor
{

    ///<summary> Editor for IBlackboards</summary>
    public class BlackboardEditor : EditorObjectWrapper<IBlackboard>
    {
        private static readonly GUILayoutOption[] layoutOptions = new GUILayoutOption[] { GUILayout.MaxWidth(100), GUILayout.ExpandWidth(true), GUILayout.MinHeight(18) };

        public static Variable pickedVariable { get; private set; }
        public static IBlackboard pickedVariableBlackboard { get; private set; }

        private List<Variable> tempVariablesList;
        private IEnumerable<string> elementDefinedParameterIDs;

        private UnityEngine.Object contextObject;
        private IBlackboard bb;

        private SerializedObject serializedContext;
        private SerializedProperty variablesProperty;

        //...
        protected override void OnEnable() {
            GraphEditorUtility.onActiveElementChanged -= OnGraphSelectionChange;
            GraphEditorUtility.onActiveElementChanged += OnGraphSelectionChange;
        }

        protected override void OnDisable() {
            GraphEditorUtility.onActiveElementChanged -= OnGraphSelectionChange;
        }

        //...
        void OnGraphSelectionChange(IGraphElement element) {
            elementDefinedParameterIDs = element != null ? Graph.GetParametersInElement(element).Where(p => p != null && p.isDefined).Select(p => p.targetVariableID) : null;
        }

        ///<summary>reset pick info</summary>
        public static void ResetPick() {
            pickedVariable = null;
            pickedVariableBlackboard = null;
        }

        ///<summary>Show variables inspector for target bb. Optionally provide override serialization context object</summary>
        public static void ShowVariables(IBlackboard bb, UnityEngine.Object overrideContextObject = null) {
            EditorWrapperFactory.GetEditor<BlackboardEditor>(bb).InspectorGUI(bb, overrideContextObject);
        }

        ///<summary>Show variables inspector for target bb. Optionally provide override serialization context object</summary>
        void InspectorGUI(IBlackboard bb, UnityEngine.Object overrideContextObject = null) {

            if ( bb.parent != null ) {
                ShowVariables(bb.parent);
                EditorUtils.Separator();
            }

            this.contextObject = overrideContextObject != null ? overrideContextObject : bb.unityContextObject;
            this.bb = bb;

            this.variablesProperty = null;
            if ( contextObject != null && PrefabUtility.IsPartOfPrefabInstance(contextObject) && bb.independantVariablesFieldName != null ) {
                this.serializedContext = new SerializedObject(contextObject);
                this.variablesProperty = serializedContext.FindProperty(bb.independantVariablesFieldName);
            }

            //Add variable button
            GUI.backgroundColor = Colors.lightBlue;
            if ( GUILayout.Button("Add Variable") ) {
                GetAddVariableMenu(bb, contextObject).ShowAsBrowser("Add Variable");
                Event.current.Use();
            }
            GUI.backgroundColor = Color.white;

            //Simple column header info
            EditorGUILayout.HelpBox($"{bb} Blackboard Variables", MessageType.None);
            if ( bb.variables.Keys.Count != 0 ) {
                GUILayout.BeginHorizontal();
                GUI.color = Color.yellow;
                GUILayout.Label("Name", layoutOptions);
                GUILayout.Label("Value", layoutOptions);
                GUI.color = Color.white;
                GUILayout.EndHorizontal();
            }

            //temp list to work with
            if ( tempVariablesList == null || !tempVariablesList.SequenceEqual(bb.variables.Values) ) {
                tempVariablesList = bb.variables.Values.ToList();
            }

            //The actual variables reorderable list
            var options = new EditorUtils.ReorderableListOptions();
            options.blockReorder = contextObject != null ? PrefabUtility.IsPartOfRegularPrefab(contextObject) : false;
            options.unityObjectContext = contextObject;
            options.customItemMenu = (i) => { return GetVariableMenu(tempVariablesList[i], i); };
            EditorUtils.ReorderableList(tempVariablesList, options, (i, isPicked) =>
            {
                var data = tempVariablesList[i] as Variable;
                if ( data == null ) { GUILayout.Label("NULL Variable!"); return; }
                GUILayout.Space(data.varType == typeof(VariableSeperator) ? 5 : 0);
                GUILayout.BeginHorizontal();
                DoVariableGUI(data, i, isPicked);
                GUILayout.EndHorizontal();
                GUI.color = Color.white;
                GUI.backgroundColor = Color.white;
                if ( elementDefinedParameterIDs != null && elementDefinedParameterIDs.Contains(data.ID) ) { EditorUtils.HighlightLastField(); }
            });

            //apply temp list reconstruct the dictionary
            if ( GUI.changed || Event.current.rawType == EventType.MouseUp ) {
                EditorApplication.delayCall += () => { ResetPick(); };
                try { bb.variables = tempVariablesList.ToDictionary(d => d.name, d => d); }
                catch { ParadoxNotion.Services.Logger.LogError("Blackboard has duplicate names!", LogTag.EDITOR, bb); }
            }
        }

        //...
        void DoVariableGUI(Variable data, int index, bool isPicked) {

            if ( data is MissingVariableType ) {
                var missingVariableType = (MissingVariableType)data;
                GUILayout.Label(data.name, Styles.leftLabel, layoutOptions);
                GUILayout.Label(ReflectionTools.FriendlyTypeName(missingVariableType.missingType).FormatError(), Styles.leftLabel, layoutOptions);
                return;
            }

            //Don't allow name edits in play mode. Instead show just a label
            if ( Application.isPlaying ) {
                if ( data.varType != typeof(VariableSeperator) ) {
                    GUILayout.Label(data.name, Styles.leftLabel, layoutOptions);
                } else {
                    GUI.color = Color.yellow;
                    GUILayout.Label(string.Format("<b>{0}</b>", data.name.ToUpper()), Styles.leftLabel, layoutOptions);
                    GUI.color = Color.white;
                }
                ShowDataFieldGUI(data, index);
                return;
            }

            //store picks
            if ( isPicked && data.varType != typeof(VariableSeperator) ) {
                pickedVariable = data;
                pickedVariableBlackboard = bb;
            }

            //Make name field red if same name exists
            if ( tempVariablesList.Where(v => v != data).Select(v => v.name).Contains(data.name) ) {
                GUI.color = Color.red;
            }

            ShowDataLabelGUI(data, index);
            ShowDataFieldGUI(data, index);
        }

        //Data label (left side)
        void ShowDataLabelGUI(Variable data, int index) {
            var e = Event.current;
            var separator = data.value as VariableSeperator;
            var isVariablePrefabInstanceModified = variablesProperty != null ? variablesProperty.GetArrayElementAtIndex(index).prefabOverride : false;

            //this is a separator
            if ( separator != null ) {
                if ( !separator.isEditingName ) {
                    GUI.color = Color.yellow;
                    GUILayout.Label(string.Format("<b>{0}</b>", data.name.ToUpper()), Styles.leftLabel, layoutOptions);
                    GUI.color = Color.white;
                    if ( e.type == EventType.MouseDown && e.button == 0 && e.clickCount == 2 && GUILayoutUtility.GetLastRect().Contains(e.mousePosition) ) {
                        separator.isEditingName = true;
                        GUIUtility.keyboardControl = 0;
                        e.Use();
                    }
                }

                if ( separator.isEditingName ) {
                    var newName = EditorGUILayout.DelayedTextField(data.name, layoutOptions);
                    if ( data.name != newName ) {
                        UndoUtility.RecordObject(contextObject, "Separator Rename");
                        data.name = newName;
                        UndoUtility.SetDirty(contextObject);
                    }

                    if ( ( e.isKey && e.keyCode == KeyCode.Return ) || ( e.rawType == EventType.MouseUp && !GUILayoutUtility.GetLastRect().Contains(e.mousePosition) ) ) {
                        separator.isEditingName = false;
                        GUIUtility.keyboardControl = 0;
                        e.Use();
                    }
                }

            } else {

                //not a separator
                var wasFontStyle = GUI.skin.textField.fontStyle;
                GUI.skin.textField.fontStyle = isVariablePrefabInstanceModified ? FontStyle.Bold : FontStyle.Normal;
                var newName = EditorGUILayout.DelayedTextField(data.name, layoutOptions);
                if ( data.name != newName ) {
                    UndoUtility.RecordObject(contextObject, "Variable Name Change");
                    data.name = newName;
                    UndoUtility.SetDirty(contextObject);
                }

                GUI.skin.textField.fontStyle = wasFontStyle;
                EditorGUI.indentLevel = 0;
            }

            //show a prefab override marker on the left side
            if ( isVariablePrefabInstanceModified ) {
                EditorUtils.MarkLastFieldOverride();
            }

        }

        //show variable data
        void ShowDataFieldGUI(Variable data, int index) {
            //Prop Bind info
            if ( data.isPropertyBound ) {
                var idx = data.propertyPath.LastIndexOf('.');
                var typeName = data.propertyPath.Substring(0, idx);
                var memberName = data.propertyPath.Substring(idx + 1);
                GUI.color = new Color(0.8f, 0.8f, 1);
                var suf = data.debugBoundValue && Application.isPlaying ? data.value.ToStringAdvanced() : typeName.Split('.').Last();
                GUILayout.Label(string.Format(".{0} ({1}) {2}", memberName, suf, data.debugBoundValue ? "*" : string.Empty), Styles.leftLabel, layoutOptions);
                GUI.color = Color.white;
                return;
            }

            GUI.color = data.isExposedPublic ? GUI.color.WithAlpha(0.5f) : GUI.color;
            EditorGUIUtility.labelWidth = 10;
            var newVal = VariableField(data, contextObject, layoutOptions);
            EditorGUIUtility.labelWidth = 0;
            if ( !Equals(data.value, newVal) ) {
                UndoUtility.RecordObject(contextObject, "Variable Value Change");
                data.value = newVal;
                UndoUtility.SetDirty(contextObject);
            }
            GUI.color = Color.white;
        }

        ///<summary>Return get add variable menu</summary>
        GenericMenu GetAddVariableMenu(IBlackboard bb, UnityEngine.Object contextParent) {
            System.Action<System.Type> AddNewVariable = (t) =>
            {
                UndoUtility.RecordObject(contextParent, "Variable Added");
                var name = "my" + t.FriendlyName();
                while ( bb.GetVariable(name) != null ) {
                    name += ".";
                }
                bb.AddVariable(name, t);
                UndoUtility.SetDirty(contextParent);
            };

            System.Action<PropertyInfo> AddBoundProp = (p) =>
            {
                UndoUtility.RecordObject(contextParent, "Variable Added");
                var newVar = bb.AddVariable(p.Name, p.PropertyType);
                newVar.BindProperty(p);
                UndoUtility.SetDirty(contextParent);
            };

            System.Action<FieldInfo> AddBoundField = (f) =>
            {
                UndoUtility.RecordObject(contextParent, "Variable Added");
                var newVar = bb.AddVariable(f.Name, f.FieldType);
                newVar.BindProperty(f);
                UndoUtility.SetDirty(contextParent);
            };

            System.Action AddSeparator = () =>
            {
                UndoUtility.RecordObject(contextParent, "Separator Added");
                bb.AddVariable("Separator (Double Click To Rename)", new VariableSeperator());
                UndoUtility.SetDirty(contextParent);
            };

            var menu = new GenericMenu();
            menu = EditorUtils.GetPreferedTypesSelectionMenu(typeof(object), AddNewVariable, menu, "New", true);

            if ( bb.propertiesBindTarget != null ) {
                foreach ( var comp in bb.propertiesBindTarget.GetComponents(typeof(Component)).Where(c => c.hideFlags == 0) ) {
                    menu = EditorUtils.GetInstanceFieldSelectionMenu(comp.GetType(), typeof(object), AddBoundField, menu, "Bound (Self)");
                    menu = EditorUtils.GetInstancePropertySelectionMenu(comp.GetType(), typeof(object), AddBoundProp, false, false, menu, "Bound (Self)");
                }
                menu.AddSeparator("Bound (Self)/");
            }
            foreach ( var type in TypePrefs.GetPreferedTypesList() ) {
                if ( bb.propertiesBindTarget != null && typeof(UnityEngine.Component).RTIsAssignableFrom(type) ) {
                    menu = EditorUtils.GetInstanceFieldSelectionMenu(type, typeof(object), AddBoundField, menu, "Bound (Self)");
                    menu = EditorUtils.GetInstancePropertySelectionMenu(type, typeof(object), AddBoundProp, false, false, menu, "Bound (Self)");
                }
                menu = EditorUtils.GetStaticFieldSelectionMenu(type, typeof(object), AddBoundField, menu, "Bound (Static)");
                menu = EditorUtils.GetStaticPropertySelectionMenu(type, typeof(object), AddBoundProp, false, false, menu, "Bound (Static)");
            }

            menu.AddSeparator("/");
            menu.AddItem(new GUIContent("Add Header Separator"), false, () => { AddSeparator(); });
            return menu;
        }

        ///<summary>Get a menu for variable</summary>
        GenericMenu GetVariableMenu(Variable data, int index) {
            var menu = new GenericMenu();
            if ( data.varType == typeof(VariableSeperator) ) {
                menu.AddItem(new GUIContent("Rename"), false, () => { ( data.value as VariableSeperator ).isEditingName = true; });
                menu.AddItem(new GUIContent("Remove"), false, () =>
                {
                    UndoUtility.RecordObject(contextObject, "Remove Variable");
                    bb.RemoveVariable(data.name);
                    UndoUtility.SetDirty(contextObject);
                });
                return menu;
            }

            System.Action<PropertyInfo> BindProp = (p) =>
            {
                UndoUtility.RecordObject(contextObject, "Bind Variable");
                data.BindProperty(p);
                UndoUtility.SetDirty(contextObject);
            };
            System.Action<FieldInfo> BindField = (f) =>
            {
                UndoUtility.RecordObject(contextObject, "Bind Variable");
                data.BindProperty(f);
                UndoUtility.SetDirty(contextObject);
            };

            menu.AddDisabledItem(new GUIContent(string.Format("Type: {0}", data.varType.FriendlyName())));

            if ( bb.propertiesBindTarget != null ) {
                foreach ( var comp in bb.propertiesBindTarget.GetComponents(typeof(Component)).Where(c => c != null) ) {
                    menu = EditorUtils.GetInstanceFieldSelectionMenu(comp.GetType(), data.varType, BindField, menu, "Bind (Self)");
                    menu = EditorUtils.GetInstancePropertySelectionMenu(comp.GetType(), data.varType, BindProp, false, false, menu, "Bind (Self)");
                }
                menu.AddSeparator("Bind (Self)/");
            }
            foreach ( var type in TypePrefs.GetPreferedTypesList() ) {
                if ( bb.propertiesBindTarget != null && typeof(UnityEngine.Component).RTIsAssignableFrom(type) ) {
                    menu = EditorUtils.GetInstanceFieldSelectionMenu(type, typeof(object), BindField, menu, "Bind (Self)");
                    menu = EditorUtils.GetInstancePropertySelectionMenu(type, typeof(object), BindProp, false, false, menu, "Bind (Self)");
                }
                menu = EditorUtils.GetStaticFieldSelectionMenu(type, data.varType, BindField, menu, "Bind (Static)");
                menu = EditorUtils.GetStaticPropertySelectionMenu(type, data.varType, BindProp, false, false, menu, "Bind (Static)");
            }

            menu.AddItem(new GUIContent("Duplicate"), false, () =>
            {
                UndoUtility.RecordObject(contextObject, "Duplicate Variable");
                data.Duplicate(bb);
                UndoUtility.SetDirty(contextObject);
            });

            if ( bb is BlackboardSource ) { //TODO: avoid this check (but Im too tired now)
                if ( !data.isPropertyBound ) {
                    menu.AddItem(new GUIContent("Exposed Public"), data.isExposedPublic, () =>
                    {
                        UndoUtility.RecordObject(contextObject, "Modify Variable");
                        data.isExposedPublic = !data.isExposedPublic;
                        UndoUtility.SetDirty(contextObject);
                    });
                } else { menu.AddDisabledItem(new GUIContent("Bound Variables Can't Be Exposed")); }
            }

            menu.AddSeparator("/");
            if ( data.isPropertyBound ) {
                menu.AddItem(new GUIContent("Runtime Debug Bound Value"), data.debugBoundValue, () =>
                {
                    UndoUtility.RecordObject(contextObject, "Debug Bound Variable Value");
                    data.debugBoundValue = !data.debugBoundValue;
                    UndoUtility.SetDirty(contextObject);
                });

                menu.AddItem(new GUIContent("UnBind"), false, () =>
                {
                    UndoUtility.RecordObject(contextObject, "UnBind Variable");
                    data.UnBind();
                    UndoUtility.SetDirty(contextObject);
                });
            } else {
                menu.AddDisabledItem(new GUIContent("UnBind"));
            }


            var serProp = variablesProperty?.GetArrayElementAtIndex(index);
            if ( serProp != null && serProp.prefabOverride ) {
                var prefabAssetPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(contextObject);
                var asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(prefabAssetPath);
                menu.AddItem(new GUIContent("Apply Prefab Modification To '" + asset.name + "'"), false, () =>
                {
                    UndoUtility.RecordObject(asset, "Apply Variable To Prefab");
                    UndoUtility.RecordObject(contextObject, "Apply Variable To Prefab");
                    PrefabUtility.ApplyPropertyOverride(serProp, prefabAssetPath, InteractionMode.UserAction);
                    UndoUtility.SetDirty(contextObject);
                    UndoUtility.SetDirty(asset);
                });

                menu.AddItem(new GUIContent("Revert Prefab Modification"), false, () =>
                {
                    UndoUtility.RecordObject(contextObject, "Revert Variable From Prefab");
                    PrefabUtility.RevertPropertyOverride(serProp, InteractionMode.UserAction);
                    UndoUtility.SetDirty(contextObject);
                });
            }

            System.Action<System.Type> ChangeType = (t) =>
            {
                UndoUtility.RecordObject(contextObject, "Change Variable Type");
                bb.ChangeVariableType(data, t);
                UndoUtility.SetDirty(contextObject);
            };

            menu = EditorUtils.GetPreferedTypesSelectionMenu(typeof(object), ChangeType, menu, "Change Type");

            menu.AddItem(new GUIContent("Delete Variable"), false, () =>
            {
                if ( EditorUtility.DisplayDialog("Delete Variable '" + data.name + "'", "Are you sure?", "Yes", "No") ) {
                    UndoUtility.RecordObject(contextObject, "Delete Variable");
                    bb.RemoveVariable(data.name);
                    GUIUtility.hotControl = 0;
                    GUIUtility.keyboardControl = 0;
                    UndoUtility.SetDirty(contextObject);
                }
            });

            return menu;
        }

        //the variable data field
        object VariableField(Variable data, UnityEngine.Object contextParent, GUILayoutOption[] layoutOptions) {

            var o = data.value;
            var t = data.varType;

            if ( t == typeof(VariableSeperator) ) {
                GUILayout.Space(0);
                return o;
            }

            //allow creation of derived classes for abstract classes via button
            if ( o == null && t.IsAbstract && !typeof(UnityEngine.Object).IsAssignableFrom(t) && !t.IsInterface && t != typeof(System.Type) ) {
                if ( GUILayout.Button("(null) Create", layoutOptions) ) {
                    EditorUtils.GetTypeSelectionMenu(t, (derived) => { data.value = System.Activator.CreateInstance(derived); }).ShowAsContext();
                }
                return o;
            }

            ///----------------------------------------------------------------------------------------------
            bool handled;
            o = EditorUtils.DirectFieldControl(GUIContent.none, o, t, contextParent, null, out handled, layoutOptions);
            if ( handled ) { return o; }
            ///----------------------------------------------------------------------------------------------

            //If some other type, show it in the generic object editor window with its true value type
            t = o != null ? o.GetType() : t;
            if ( GUILayout.Button(string.Format("{0} {1}", t.FriendlyName(), ( o is IList ) ? ( (IList)o ).Count.ToString() : string.Empty), layoutOptions) ) {
                //we use bb.GetVariableByID to avoid undo creating new instance of variable and thus generic inspector, left inspecting something else
                GenericInspectorWindow.Show(data.name, t, contextParent, () => { return bb.GetVariableByID(data.ID).value; }, (newValue) => { bb.GetVariableByID(data.ID).value = newValue; });
            }

            return o;
        }
    }
}

#endif