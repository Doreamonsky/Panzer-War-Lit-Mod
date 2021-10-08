#if UNITY_EDITOR

using NodeCanvas.Framework;
using ParadoxNotion;
using ParadoxNotion.Design;
using UnityEditor;
using UnityEngine;

namespace NodeCanvas.Editor
{

    [CustomEditor(typeof(GraphOwner), true)]
    public class GraphOwnerInspector : UnityEditor.Editor
    {

        private GraphOwner owner { get { return (GraphOwner)target; } }

        private SerializedProperty boundGraphSerializationProp;
        private SerializedProperty boundGraphReferencesProp;
        private SerializedProperty graphProp;
        private SerializedProperty blackboardProp;
        private SerializedProperty firstActivationProp;
        private SerializedProperty enableActionProp;
        private SerializedProperty disableActionProp;
        private SerializedProperty sourceProp;
        private SerializedProperty categoryProp;
        private SerializedProperty commentsProp;
        private SerializedProperty lockPrefabProp;
        private SerializedProperty preInitProp;
        private SerializedProperty updateModeProp;
        private SerializedProperty exposeParamsProp;

        private string graphTypeName {
            get { return owner.graphType.Name.SplitCamelCase(); }
        }

        private bool isOwnerPeristant {
            get { return EditorUtility.IsPersistent(owner); }
        }

        public bool isBoundGraphOnPrefabRoot {
            get { return isOwnerPeristant && owner.graphIsBound; }
        }

        public bool isBoundGraphOnPrefabInstance {
            get { return !isOwnerPeristant && owner.graphIsBound && PrefabUtility.IsPartOfAnyPrefab(owner); }
        }

        public bool isBoundGraphPrefabOverridden {
            get { return boundGraphSerializationProp.prefabOverride; }
        }

        ///----------------------------------------------------------------------------------------------

        void OnDestroy() {
            //just a little trick to destroy graph and not left editing a floating one
            if ( owner == null && !ReferenceEquals(owner, null) && owner.graph != null ) {
                if ( owner.graphIsBound ) {
                    Undo.DestroyObjectImmediate(owner.graph);
                }
            }
        }

        void OnEnable() {
            boundGraphSerializationProp = serializedObject.FindProperty("_boundGraphSerialization");
            boundGraphReferencesProp = serializedObject.FindProperty("_boundGraphObjectReferences");
            graphProp = serializedObject.FindProperty("_graph");
            blackboardProp = serializedObject.FindProperty("_blackboard");
            firstActivationProp = serializedObject.FindProperty("_firstActivation");
            enableActionProp = serializedObject.FindProperty("_enableAction");
            disableActionProp = serializedObject.FindProperty("_disableAction");
            sourceProp = serializedObject.FindProperty("_boundGraphSource");
            categoryProp = sourceProp.FindPropertyRelative("_category");
            commentsProp = sourceProp.FindPropertyRelative("_comments");
            lockPrefabProp = serializedObject.FindProperty("_lockBoundGraphPrefabOverrides");
            preInitProp = serializedObject.FindProperty("_preInitializeSubGraphs");
            updateModeProp = serializedObject.FindProperty("_updateMode");
            exposeParamsProp = serializedObject.FindProperty("_serializedExposedParameters");
        }

        //create new graph asset and assign it to owner
        public Graph NewAsAsset() {
            var newGraph = (Graph)EditorUtils.CreateAsset(owner.graphType);
            if ( newGraph != null ) {
                UndoUtility.RecordObject(owner, "New Asset Graph");
                owner.graph = newGraph;
                UndoUtility.SetDirty(owner);
                UndoUtility.SetDirty(newGraph);
                AssetDatabase.SaveAssets();
            }
            return newGraph;
        }

        //create new local graph and assign it to owner
        public Graph NewAsBound() {
            var newGraph = (Graph)ScriptableObject.CreateInstance(owner.graphType);
            UndoUtility.RecordObject(owner, "New Bound Graph");
            owner.SetBoundGraphReference(newGraph);
            UndoUtility.SetDirty(owner);
            return newGraph;
        }

        //Bind graph to owner
        public void AssetToBound() {
            UndoUtility.RecordObject(owner, "Bind Asset Graph");
            owner.SetBoundGraphReference(owner.graph);
            UndoUtility.SetDirty(owner);
        }

        //Revert bound graph
        public void PrefabRevertBoundGraph() {
            UndoUtility.RecordObject(owner, "Revert Graph From Prefab");
            var prefabAssetPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(owner);
            PrefabUtility.RevertPropertyOverride(boundGraphSerializationProp, InteractionMode.UserAction);
            PrefabUtility.RevertPropertyOverride(boundGraphReferencesProp, InteractionMode.UserAction);
            GraphEditorUtility.activeElement = null;
            UndoUtility.SetDirty(owner);
            GraphEditor.FullDrawPass();
        }

        //Apply bound graph
        public void PrefabApplyBoundGraph() {
            UndoUtility.RecordObject(owner, "Apply Graph To Prefab");
            var prefabAssetPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(owner);
            PrefabUtility.ApplyPropertyOverride(boundGraphSerializationProp, prefabAssetPath, InteractionMode.UserAction);
            PrefabUtility.ApplyPropertyOverride(boundGraphReferencesProp, prefabAssetPath, InteractionMode.UserAction);
            UndoUtility.SetDirty(owner);
        }

        ///----------------------------------------------------------------------------------------------

        //...
        public override void OnInspectorGUI() {

            DoPrefabRelatedGUI();

            if ( owner.graph == null && !owner.graphIsBound ) {
                DoMissingGraphControls();
                serializedObject.ApplyModifiedProperties();
                return;
            }

            EditorGUI.BeginChangeCheck();
            DoValidGraphControls();
            DoStandardFields();

            GUI.enabled = ( !isBoundGraphOnPrefabInstance || !owner.lockBoundGraphPrefabOverrides ) && !isBoundGraphOnPrefabRoot;
            OnPreExtraGraphOptions();
            GUI.enabled = true;
            if ( EditorGUI.EndChangeCheck() && owner.graph != null ) {
                UndoUtility.RecordObject(owner.graph, "Sub Option Change");
                owner.graph.SelfSerialize();
                UndoUtility.SetDirty(owner.graph);
            }
            EditorUtils.ReflectedObjectInspector(owner, owner);

            DoExposedVariablesMapping();
            DoRuntimeGraphControls();

            OnPostExtraGraphOptions();
            EditorUtils.EndOfInspector();
            serializedObject.ApplyModifiedProperties();
        }

        ///----------------------------------------------------------------------------------------------

        //...
        void DoPrefabRelatedGUI() {

            //show lock bound graph prefab overrides
            if ( owner.graphIsBound ) {
                var case1 = PrefabUtility.IsPartOfPrefabAsset(owner) || UnityEditor.Experimental.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage()?.prefabContentsRoot == owner.gameObject;
                var case2 = PrefabUtility.IsPartOfAnyPrefab(owner) && !isBoundGraphPrefabOverridden;
                if ( case1 || case2 ) { EditorGUILayout.PropertyField(lockPrefabProp, EditorUtils.GetTempContent("Lock Prefab Graph Overrides")); }
            }

            //show bound graph prefab overrides controls
            if ( isBoundGraphPrefabOverridden ) {
                GUILayout.Space(5);
                GUI.color = Colors.prefabOverrideColor;
                GUILayout.BeginHorizontal();
                GUI.color = Color.white;
                var content = EditorUtils.GetTempContent("<b>Bound Graph is prefab overridden.</b>", StyleSheet.canvasIcon);
                GUILayout.Label(content, Styles.topLeftLabel);
                if ( GUILayout.Button("Revert Graph", EditorStyles.miniButtonLeft, GUILayout.Width(100)) ) {
                    PrefabRevertBoundGraph();
                }
                if ( GUILayout.Button("Apply Graph", EditorStyles.miniButtonRight, GUILayout.Width(100)) ) {
                    PrefabApplyBoundGraph();
                }
                GUILayout.EndHorizontal();
                EditorUtils.MarkLastFieldOverride();
                GUILayout.Space(5);
            }
        }

        //...
        void DoMissingGraphControls() {
            EditorGUILayout.HelpBox(owner.GetType().Name + " needs a " + graphTypeName + ".\nAssign or Create a new one...", MessageType.Info);
            if ( !Application.isPlaying && GUILayout.Button("CREATE NEW") ) {
                Graph newGraph = null;
                if ( EditorUtility.DisplayDialog("Create Graph", "Create a Bound or an Asset Graph?\n\n" +
                    "Bound Graph is saved with the GraphOwner and you can use direct scene references within it.\n\n" +
                    "Asset Graph is an asset file and can be reused amongst any number of GraphOwners.\n\n" +
                    "You can convert from one type to the other at any time.",
                    "Bound", "Asset") ) {

                    newGraph = NewAsBound();

                } else {

                    newGraph = NewAsAsset();
                }

                if ( newGraph != null ) {
                    owner.Validate();
                    GraphEditor.OpenWindow(owner);
                }
            }

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(graphProp, new GUIContent(graphTypeName));
            if ( EditorGUI.EndChangeCheck() ) { owner.Validate(); }
        }

        //...
        void DoValidGraphControls() {

            //Graph comments ONLY if Bound graph else readonly
            if ( owner.graph != null ) {
                if ( owner.graphIsBound ) {
                    GUI.contentColor = Color.white.WithAlpha(0.6f);
                    owner.graph.comments = GUILayout.TextArea(owner.graph.comments, GUILayout.Height(45));
                    GUI.contentColor = Color.white;
                    EditorUtils.CommentLastTextField(owner.graph.comments, "Graph comments...");
                } else {
                    GUI.enabled = false;
                    GUILayout.TextArea(owner.graph.comments, GUILayout.Height(45));
                    GUI.enabled = true;
                }
            }

            if ( !isBoundGraphOnPrefabRoot ) {

                //Open behaviour
                GUI.backgroundColor = Colors.lightBlue;
                if ( GUILayout.Button(( "Edit " + owner.graphType.Name.SplitCamelCase() ).ToUpper()) ) {
                    GraphEditor.OpenWindow(owner);
                }
                GUI.backgroundColor = Color.white;

            } else {

                EditorGUILayout.HelpBox("Bound Graphs on prefabs can only be edited by opening the prefab in the prefab editor.", MessageType.Info);

                //Open prefab and behaviour
                GUI.backgroundColor = Colors.lightBlue;
                if ( GUILayout.Button(( "Open Prefab And Edit " + owner.graphType.Name.SplitCamelCase() ).ToUpper()) ) {
                    AssetDatabase.OpenAsset(owner);
                    GraphEditor.OpenWindow(owner);
                }
                GUI.backgroundColor = Color.white;
            }

            //bind asset or delete bound graph
            if ( !Application.isPlaying ) {
                if ( !owner.graphIsBound ) {
                    if ( GUILayout.Button("Bind Graph") ) {
                        if ( EditorUtility.DisplayDialog("Bind Graph", "This will make a local copy of the graph, bound to the owner.\n\nThis allows you to make local changes and assign scene object references directly.\n\nNote that you can also use scene object references through the use of Blackboard Variables.\n\nBind Graph?", "YES", "NO") ) {
                            AssetToBound();
                        }
                    }
                } else {
                    if ( GUILayout.Button("Delete Bound Graph") ) {
                        if ( EditorUtility.DisplayDialog("Delete Bound Graph", "Are you sure?", "YES", "NO") ) {
                            Object.DestroyImmediate(owner.graph, true);
                            UndoUtility.RecordObject(owner, "Delete Bound Graph");
                            owner.SetBoundGraphReference(null);
                            UndoUtility.SetDirty(owner);
                        }
                    }
                }
            }
        }

        //...
        void DoStandardFields() {
            //basic options
            if ( Application.isPlaying || !owner.graphIsBound ) {
                EditorGUILayout.PropertyField(graphProp, EditorUtils.GetTempContent(graphTypeName));
            }

            var rect = EditorGUILayout.GetControlRect();
            var label = EditorGUI.BeginProperty(rect, EditorUtils.GetTempContent("Blackboard"), blackboardProp);
            owner.blackboard = (IBlackboard)EditorGUI.ObjectField(rect, label, owner.blackboard as Object, typeof(IBlackboard), true);
            EditorGUI.EndProperty();
            if ( owner.blackboard == null ) { EditorUtils.MarkLastFieldWarning("No Blackboard assigned. This is fine if you only want to use Graph Blackboard Variables."); }

            EditorGUILayout.PropertyField(firstActivationProp);
            EditorGUILayout.PropertyField(enableActionProp, EditorUtils.GetTempContent("On Enable"));
            EditorGUILayout.PropertyField(disableActionProp, EditorUtils.GetTempContent("On Disable"));
            EditorGUILayout.PropertyField(updateModeProp);
            EditorGUILayout.PropertyField(preInitProp);
        }

        //...
        void DoExposedVariablesMapping() {

            if ( owner.graph == null ) { return; }

            var separatorDrawn = false;
            var subTreeVariables = owner.graph.blackboard.variables.Values;
            foreach ( var variable in subTreeVariables ) {

                if ( variable is Variable<VariableSeperator> ) { continue; }
                if ( !variable.isExposedPublic || variable.isPropertyBound ) { continue; }

                if ( !separatorDrawn ) {
                    separatorDrawn = true;
                    EditorUtils.Separator();
                    EditorGUILayout.HelpBox("Exposed Graph Variables. Use the arrows button to override/parametrize the variable. Doing this will not change the graph serialization. Prefab overrides are also supported.", MessageType.None);
                }

                if ( owner.exposedParameters == null ) { owner.exposedParameters = new System.Collections.Generic.List<ExposedParameter>(); }
                var exposedParam = owner.exposedParameters.Find(x => x.targetVariableID == variable.ID);
                if ( exposedParam == null ) {
                    GUILayout.BeginHorizontal();
                    GUI.enabled = false;
                    EditorUtils.DrawEditorFieldDirect(new GUIContent(variable.name, "This is an Exposed Public variable of the graph local blackboard. You can use the arrows button on the right side to override/parametrize the default value."), variable.value, variable.varType, default(InspectedFieldInfo));
                    GUI.enabled = true;
                    if ( GUILayout.Button(EditorUtils.GetTempContent("▽△", null, "Override Variable"), Styles.centerLabel, GUILayout.Width(24)) ) {
                        UndoUtility.RecordObject(owner, "Add Override");
                        exposedParam = ExposedParameter.CreateInstance(variable);
                        owner.exposedParameters.Add(exposedParam);
                        // DISABLE: was creating confusion when editing multiple graphowner instances using asset graphs and having different variable overrides
                        // exposedParam.Bind(owner.graph.blackboard);
                        UndoUtility.SetDirty(owner);
                    }
                    EditorGUIUtility.AddCursorRect(GUILayoutUtility.GetLastRect(), MouseCursor.Link);
                    GUILayout.EndHorizontal();
                    continue;
                }

                GUILayout.BeginHorizontal();
                var info = new InspectedFieldInfo();
                info.unityObjectContext = owner;
                exposedParam.valueBoxed = EditorUtils.DrawEditorFieldDirect(new GUIContent(variable.name), exposedParam.valueBoxed, variable.varType, info);
                if ( GUILayout.Button(EditorUtils.GetTempContent("▼▲", null, "Remove Override"), Styles.centerLabel, GUILayout.Width(24)) ) {
                    UndoUtility.RecordObject(owner, "Remove Override");
                    // DISABLE: was creating confusion when editing multiple graphowner instances using asset graphs and having different variable overrides
                    // exposedParam.UnBind(owner.graph.blackboard);
                    owner.exposedParameters.Remove(exposedParam);
                    UndoUtility.SetDirty(owner);
                    continue;
                }
                EditorGUIUtility.AddCursorRect(GUILayoutUtility.GetLastRect(), MouseCursor.Link);
                GUILayout.EndHorizontal();

                var index = owner.exposedParameters.IndexOf(exposedParam);
                var serProp = exposeParamsProp.GetArrayElementAtIndex(index);
                var isPrefabOverride = serProp.prefabOverride;
                if ( isPrefabOverride ) {
                    var rect = GUILayoutUtility.GetLastRect();
                    EditorUtils.MarkLastFieldOverride();
                    if ( rect.Contains(Event.current.mousePosition) && Event.current.type == EventType.ContextClick ) {
                        var prefabAssetPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(owner);
                        var asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(prefabAssetPath);
                        var menu = new GenericMenu();
                        menu.AddItem(new GUIContent($"Apply to Prefab '{asset.name}'"), false, () =>
                        {
                            UndoUtility.RecordObject(owner, "Apply Exposed Parameter");
                            UndoUtility.RecordObject(asset, "Apply Exposed Parameter");
                            PrefabUtility.ApplyPropertyOverride(serProp, prefabAssetPath, InteractionMode.UserAction);
                            UndoUtility.SetDirty(owner);
                            UndoUtility.SetDirty(asset);
                        });
                        menu.AddItem(new GUIContent("Revert"), false, () =>
                        {
                            UndoUtility.RecordObject(owner, "Revert Exposed Parameter");
                            PrefabUtility.RevertPropertyOverride(serProp, InteractionMode.UserAction);
                            UndoUtility.SetDirty(owner);
                        });
                        menu.ShowAsContext();
                    }
                }
            }

            if ( separatorDrawn ) { EditorUtils.Separator(); }

            //cleanup
            if ( owner.exposedParameters != null ) {
                for ( var i = owner.exposedParameters.Count; i-- > 0; ) {
                    var exposedParam = owner.exposedParameters[i];
                    var variable = owner.graph.blackboard.GetVariableByID(exposedParam.targetVariableID);
                    if ( variable == null || !variable.isExposedPublic || variable.isPropertyBound ) {
                        owner.exposedParameters.RemoveAt(i);
                        UndoUtility.SetDirty(owner);
                    }
                }
            }
        }

        //...
        void DoRuntimeGraphControls() {
            //execution debug controls
            if ( Application.isPlaying && owner.graph != null && !isOwnerPeristant ) {
                EditorUtils.Separator();
                GUILayout.BeginHorizontal("box");
                GUILayout.FlexibleSpace();

                GUI.contentColor = EditorGUIUtility.isProSkin ? Color.white : Colors.Grey(0.3f);

                GUI.color = Colors.Grey(owner.isRunning ? 1f : 0.7f);
                if ( GUILayout.Button(Icons.playIcon, Styles.buttonLeft) ) {
                    if ( owner.isRunning ) owner.StopBehaviour();
                    else owner.StartBehaviour();
                }

                GUI.color = Colors.Grey(owner.isPaused ? 1f : 0.7f);
                if ( GUILayout.Button(Icons.pauseIcon, Styles.buttonMid) ) {
                    if ( owner.isPaused ) owner.StartBehaviour();
                    else owner.PauseBehaviour();
                }

                GUI.color = Colors.Grey(0.7f);
                if ( GUILayout.Button(Icons.stepIcon, Styles.buttonRight) ) {
                    owner.PauseBehaviour();
                    owner.UpdateBehaviour();
                }
                GUI.color = Color.white;

                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
            }
        }

        ///----------------------------------------------------------------------------------------------

        virtual protected void OnPreExtraGraphOptions() { }
        virtual protected void OnPostExtraGraphOptions() { }
    }
}

#endif