using System.Linq;
using NodeCanvas.Framework.Internal;
using ParadoxNotion.Design;
using UnityEngine;

namespace NodeCanvas.Framework
{

    public static class IGraphAssignableExtensions
    {
        ///<summary>Checks and possibly makes and returns runtime instance</summary>
        public static Graph CheckInstance(this IGraphAssignable assignable) {
            if ( assignable.subGraph == assignable.currentInstance ) {
                return assignable.currentInstance;
            }

            Graph instance = null;
            if ( assignable.instances == null ) { assignable.instances = new System.Collections.Generic.Dictionary<Graph, Graph>(); }
            if ( !assignable.instances.TryGetValue(assignable.subGraph, out instance) ) {
                instance = Graph.Clone(assignable.subGraph, assignable.graph);
                assignable.instances[assignable.subGraph] = instance;
            }

            assignable.subGraph = instance;
            assignable.currentInstance = instance;
            return instance;
        }

        ///<summary>Utility to start sub graph (makes instance, writes mapping, starts graph and on stop reads mapping)</summary>
        public static bool TryStartSubGraph(this IGraphAssignable assignable, Component agent, System.Action<bool> callback = null) {
            assignable.currentInstance = assignable.CheckInstance();
            if ( assignable.currentInstance != null ) {
                assignable.TryWriteAndBindMappedVariables();
                //we always start with the current graphs blackboard parent bb as the subgraphs parent bb
                assignable.currentInstance.StartGraph(agent, assignable.graph.blackboard.parent, Graph.UpdateMode.Manual, (result) =>
                {
                    if ( assignable.status == Status.Running ) { assignable.TryReadAndUnbindMappedVariables(); }
                    if ( callback != null ) { callback(result); }
                });
                return true;
            }
            return false;
        }

        ///<summary>Stop subgraph if currentInstance exists</summary>
        public static bool TryStopSubGraph(this IGraphAssignable assignable) {
            if ( assignable.currentInstance != null ) {
                assignable.currentInstance.Stop();
                return true;
            }
            return false;
        }

        ///<summary>Pause subgraph if currentInstance exists</summary>
        public static bool TryPauseSubGraph(this IGraphAssignable assignable) {
            if ( assignable.currentInstance != null ) {
                assignable.currentInstance.Pause();
                return true;
            }
            return false;
        }

        ///<summary>Resume subgraph if currentInstance exists</summary>
        public static bool TryResumeSubGraph(this IGraphAssignable assignable) {
            if ( assignable.currentInstance != null ) {
                assignable.currentInstance.Resume();
                return true;
            }
            return false;
        }

        ///<summary>Update subgraph if currentInstance exists</summary>
        public static bool TryUpdateSubGraph(this IGraphAssignable assignable) {
            if ( assignable.currentInstance != null ) {
                if ( assignable.currentInstance.isRunning ) {
                    assignable.currentInstance.UpdateGraph(assignable.graph.deltaTime);
                    return true;
                }
            }
            return false;
        }

        ///<summary>Write mapped variables to subgraph (write in) and bind for read out</summary>
        public static void TryWriteAndBindMappedVariables(this IGraphAssignable assignable) {
            if ( !assignable.currentInstance.allowBlackboardOverrides || assignable.variablesMap == null ) { return; }
            for ( var i = 0; i < assignable.variablesMap.Count; i++ ) {
                var bbParam = assignable.variablesMap[i];
                if ( bbParam.isNone ) { continue; }
                var targetSubVariable = assignable.currentInstance.blackboard.GetVariableByID(bbParam.targetSubGraphVariableID);
                if ( targetSubVariable != null && targetSubVariable.isExposedPublic && !targetSubVariable.isPropertyBound ) {
                    if ( bbParam.canWrite ) { targetSubVariable.value = bbParam.value; }
                    if ( bbParam.canRead ) {
                        targetSubVariable.onValueChanged -= bbParam.SetValue;
                        targetSubVariable.onValueChanged += bbParam.SetValue;
                    }
                }
            }
        }

        ///<summary>Read mapped variables from subgraph (read out) and unbind read out</summary>
        public static void TryReadAndUnbindMappedVariables(this IGraphAssignable assignable) {
            if ( !assignable.currentInstance.allowBlackboardOverrides || assignable.variablesMap == null ) { return; }
            for ( var i = 0; i < assignable.variablesMap.Count; i++ ) {
                var bbParam = assignable.variablesMap[i];
                if ( bbParam.isNone ) { continue; }
                var targetSubVariable = assignable.currentInstance.blackboard.GetVariableByID(bbParam.targetSubGraphVariableID);
                if ( targetSubVariable != null && targetSubVariable.isExposedPublic && !targetSubVariable.isPropertyBound ) {
                    if ( bbParam.canRead ) { bbParam.value = targetSubVariable.value; }
                    targetSubVariable.onValueChanged -= bbParam.SetValue;
                }
            }
        }

        ///----------------------------------------------------------------------------------------------

        ///<summary>Validate the variables mapping</summary>
        public static void ValidateSubGraphAndParameters(this IGraphAssignable assignable) {
            if ( !ParadoxNotion.Services.Threader.applicationIsPlaying ) {
                if ( assignable.subGraph == null || !assignable.subGraph.allowBlackboardOverrides || assignable.subGraph.blackboard.variables.Count == 0 ) {
                    assignable.variablesMap = null;
                }
            }
        }

        ///<summary>Link subgraph variables to parent graph variables matching name and type</summary>
        public static void AutoLinkByName(this IGraphAssignable assignable) {
            if ( assignable.subGraph == null || assignable.variablesMap == null ) { return; }
            foreach ( var bbParam in assignable.variablesMap ) {
                var thatVariable = assignable.subGraph.blackboard.GetVariableByID(bbParam.targetSubGraphVariableID);
                if ( thatVariable != null && thatVariable.isExposedPublic && !thatVariable.isPropertyBound ) {
                    var thisVariable = assignable.graph.blackboard.GetVariable(thatVariable.name, thatVariable.varType);
                    if ( thisVariable != null ) {
                        bbParam.SetType(thatVariable.varType);
                        bbParam.name = thatVariable.name;
                    }
                }
            }
        }

        ///----------------------------------------------------------------------------------------------

#if UNITY_EDITOR

        //Shows blackboard variables mapping
        public static void ShowVariablesMappingGUI(this IGraphAssignable assignable) {

            if ( assignable.subGraph == null || !assignable.subGraph.allowBlackboardOverrides ) {
                assignable.variablesMap = null;
                return;
            }

            ParadoxNotion.Design.EditorUtils.Separator();
            ParadoxNotion.Design.EditorUtils.CoolLabel("SubGraph Variables Mapping");

            var subTreeVariables = assignable.subGraph.blackboard.variables.Values;
            if ( subTreeVariables.Count == 0 || !subTreeVariables.Any(v => v.isExposedPublic) ) {
                UnityEditor.EditorGUILayout.HelpBox("SubGraph has no exposed public variables. You can make variables exposed public through the 'gear' menu of a variable.", UnityEditor.MessageType.Info);
                assignable.variablesMap = null;
                return;
            }

            UnityEditor.EditorGUILayout.HelpBox("Map SubGraph exposed variables to this graph variables.\nUse the arrow buttons on the right of each parameter to enable WriteIn and/or ReadOut. WriteIn takes place when the SubGraph starts. ReadOut takes place continously while the SubGraph is running.", UnityEditor.MessageType.Info);

            foreach ( var variable in subTreeVariables ) {

                if ( variable is Variable<VariableSeperator> ) { continue; }
                if ( !variable.isExposedPublic || variable.isPropertyBound ) { continue; }

                if ( assignable.variablesMap == null ) {
                    assignable.variablesMap = new System.Collections.Generic.List<BBMappingParameter>();
                }

                var bbParam = assignable.variablesMap.Find(x => x.targetSubGraphVariableID == variable.ID);
                if ( bbParam == null ) {
                    GUILayout.BeginHorizontal();
                    GUI.enabled = false;
                    EditorUtils.DrawEditorFieldDirect(new GUIContent(variable.name), variable.value, variable.varType, default(InspectedFieldInfo));
                    GUI.enabled = true;
                    int tmp = 0;
                    if ( GUILayout.Button(EditorUtils.GetTempContent("▽", null, "Write (In)"), Styles.centerLabel, GUILayout.Width(12)) ) { tmp = 1; }
                    UnityEditor.EditorGUIUtility.AddCursorRect(GUILayoutUtility.GetLastRect(), UnityEditor.MouseCursor.Link);
                    if ( GUILayout.Button(EditorUtils.GetTempContent("△", null, "Read (Out)"), Styles.centerLabel, GUILayout.Width(12)) ) { tmp = -1; }
                    if ( tmp != 0 ) {
                        UndoUtility.RecordObject(assignable.graph, "Override Variable");
                        bbParam = new BBMappingParameter(variable);
                        bbParam.canWrite = tmp == 1;
                        bbParam.canRead = tmp == -1;
                        bbParam.useBlackboard = tmp == -1;
                        bbParam.value = variable.value;
                        bbParam.bb = assignable.graph.blackboard;
                        assignable.variablesMap.Add(bbParam);
                        UndoUtility.SetDirty(assignable.graph);
                    }
                    UnityEditor.EditorGUIUtility.AddCursorRect(GUILayoutUtility.GetLastRect(), UnityEditor.MouseCursor.Link);
                    GUILayout.EndHorizontal();
                    continue;
                }

                if ( bbParam.varType != variable.varType && ( bbParam.canRead || bbParam.canWrite ) ) { bbParam.SetType(variable.varType); }

                GUILayout.BeginHorizontal();

                GUI.enabled = bbParam.canRead || bbParam.canWrite;
                NodeCanvas.Editor.BBParameterEditor.ParameterField(variable.name, bbParam);
                if ( bbParam.canRead && !bbParam.useBlackboard ) { EditorUtils.MarkLastFieldWarning("The parameter is set to Read Out, but is not linked to any Variable."); }
                GUI.enabled = true;

                if ( GUILayout.Button(EditorUtils.GetTempContent(bbParam.canWrite ? "▼" : "▽", null, "Write (In)"), Styles.centerLabel, GUILayout.Width(12)) ) {
                    UndoUtility.RecordObject(assignable.graph, "Set Write In");
                    bbParam.canWrite = !bbParam.canWrite;
                    UndoUtility.SetDirty(assignable.graph);
                }
                UnityEditor.EditorGUIUtility.AddCursorRect(GUILayoutUtility.GetLastRect(), UnityEditor.MouseCursor.Link);
                if ( GUILayout.Button(EditorUtils.GetTempContent(bbParam.canRead ? "▲" : "△", null, "Read (Out)"), Styles.centerLabel, GUILayout.Width(12)) ) {
                    UndoUtility.RecordObject(assignable.graph, "Set Read Out");
                    bbParam.canRead = !bbParam.canRead;
                    UndoUtility.SetDirty(assignable.graph);
                }
                UnityEditor.EditorGUIUtility.AddCursorRect(GUILayoutUtility.GetLastRect(), UnityEditor.MouseCursor.Link);
                if ( !bbParam.canRead && !bbParam.canWrite ) {
                    UndoUtility.RecordObject(assignable.graph, "Remove Override");
                    assignable.variablesMap.Remove(bbParam);
                    UndoUtility.SetDirty(assignable.graph);
                }

                GUILayout.EndHorizontal();
            }

            if ( assignable.variablesMap != null ) {
                for ( var i = assignable.variablesMap.Count; i-- > 0; ) {
                    var bbParam = assignable.variablesMap[i];
                    var variable = assignable.subGraph.blackboard.GetVariableByID(bbParam.targetSubGraphVariableID);
                    if ( variable == null || !variable.isExposedPublic || variable.isPropertyBound ) {
                        assignable.variablesMap.RemoveAt(i);
                        UndoUtility.SetDirty(assignable.graph);
                    }
                }
            }
        }
#endif

        ///----------------------------------------------------------------------------------------------

    }
}