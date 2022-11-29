#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using ParadoxNotion.Design;
using ParadoxNotion.Serialization;
using ParadoxNotion.Serialization.FullSerializer;
using System.Collections.Generic;
using System.Linq;
using NodeCanvas.Framework;

namespace NodeCanvas.Editor
{

    ///<summary>Graph Refactoring</summary>
    public class GraphRefactor : EditorWindow
    {

        //...
        public static void ShowWindow() {
            GetWindow<GraphRefactor>().Show();
        }

        private Dictionary<string, List<IMissingRecoverable>> recoverablesMap;
        private Dictionary<string, string> recoverableChangesMap;

        private Dictionary<string, List<ISerializedReflectedInfo>> reflectedMap;
        private Dictionary<string, fsData> reflectedChangesMap;

        private Dictionary<string, List<BBParameter>> missingParametersMap;
        private Dictionary<string, BBParameter> missingParameterChangesMap;

        ///----------------------------------------------------------------------------------------------

        void Flush() {
            recoverablesMap = null;
            reflectedChangesMap = null;
            recoverablesMap = null;
            reflectedChangesMap = null;
            missingParametersMap = null;
            missingParameterChangesMap = null;
        }

        //...
        void Gather() {

            EditorGUIUtility.keyboardControl = 0;
            EditorGUIUtility.hotControl = 0;

            GatherRecoverables();
            GatherReflected();
            GatherBBParameters();
        }

        //...
        void GatherRecoverables() {
            recoverablesMap = new Dictionary<string, List<IMissingRecoverable>>();
            recoverableChangesMap = new Dictionary<string, string>();

            var graph = GraphEditor.currentGraph;
            var metaGraph = graph.GetFlatMetaGraph();
            var recoverables = metaGraph.GetAllChildrenReferencesOfType<IMissingRecoverable>();
            foreach ( var recoverable in recoverables ) {
                List<IMissingRecoverable> collection;
                if ( !recoverablesMap.TryGetValue(recoverable.missingType, out collection) ) {
                    collection = new List<IMissingRecoverable>();
                    recoverablesMap[recoverable.missingType] = collection;
                    recoverableChangesMap[recoverable.missingType] = recoverable.missingType;
                }
                collection.Add(recoverable);
            }
        }

        //...
        void GatherReflected() {
            reflectedMap = new Dictionary<string, List<ISerializedReflectedInfo>>();
            reflectedChangesMap = new Dictionary<string, fsData>();
            var graph = GraphEditor.currentGraph;
            JSONSerializer.SerializeAndExecuteNoCycles(typeof(NodeCanvas.Framework.Internal.GraphSource), graph.GetGraphSource(), DoCollect);
        }

        //...
        void DoCollect(object o, fsData d) {
            if ( o is ISerializedReflectedInfo ) {
                var reflect = (ISerializedReflectedInfo)o;
                if ( reflect.AsMemberInfo() == null ) {
                    List<ISerializedReflectedInfo> collection;
                    if ( !reflectedMap.TryGetValue(reflect.AsString(), out collection) ) {
                        collection = new List<ISerializedReflectedInfo>();
                        reflectedMap[reflect.AsString()] = collection;
                        reflectedChangesMap[reflect.AsString()] = d;
                    }
                    collection.Add(reflect);
                }
            }
        }

        //...
        void GatherBBParameters() {
            missingParametersMap = new Dictionary<string, List<BBParameter>>();
            missingParameterChangesMap = new Dictionary<string, BBParameter>();
            var graph = GraphEditor.currentGraph;
            foreach ( var missingParameter in graph.GetDefinedParameters().Where(p => p.varRef == null && !p.isPresumedDynamic) ) {
                var key = string.Format("{0}({1})", missingParameter.name, missingParameter.varType.Name);
                List<BBParameter> collection;
                if ( !missingParametersMap.TryGetValue(key, out collection) ) {
                    collection = new List<BBParameter>();
                    var fakeParam = new Framework.Internal.BBObjectParameter(missingParameter.varType);
                    fakeParam.name = missingParameter.name;
                    fakeParam.bb = missingParameter.bb;
                    fakeParam.useBlackboard = missingParameter.useBlackboard;
                    missingParametersMap[key] = collection;
                    missingParameterChangesMap[key] = fakeParam;
                }
                collection.Add(missingParameter);
            }
        }

        ///----------------------------------------------------------------------------------------------

        //...
        void Save() {

            if ( recoverableChangesMap.Count > 0 || reflectedChangesMap.Count > 0 || missingParameterChangesMap.Count > 0 ) {

                if ( recoverableChangesMap.Count > 0 ) {
                    SaveRecoverables();
                }
                if ( reflectedChangesMap.Count > 0 ) {
                    SaveReflected();
                }
                if ( missingParametersMap.Count > 0 ) {
                    SaveParameters();
                }

                GraphEditor.currentGraph.SelfSerialize();
                GraphEditor.currentGraph.SelfDeserialize();
                GraphEditor.currentGraph.Validate();
                Gather();
            }
        }

        //...
        void SaveRecoverables() {
            foreach ( var pair in recoverablesMap ) {
                foreach ( var recoverable in pair.Value ) {
                    recoverable.missingType = recoverableChangesMap[pair.Key];
                }
            }
        }

        //...
        void SaveReflected() {
            foreach ( var pair in reflectedMap ) {
                foreach ( var reflect in pair.Value ) {
                    var data = reflectedChangesMap[pair.Key];
                    JSONSerializer.TryDeserializeOverwrite(reflect, data.ToString(), null);
                }
            }
        }

        void SaveParameters() {
            foreach ( var pair in missingParametersMap ) {
                var change = missingParameterChangesMap[pair.Key];
                foreach ( var bbParam in pair.Value ) {
                    if ( change.useBlackboard ) {
                        bbParam.name = change.name;
                        bbParam.SetTargetVariable(change.bb, change.varRef);
                    } else {
                        bbParam.name = null;
                        bbParam.value = change.value;
                    }
                }
            }
        }

        ///----------------------------------------------------------------------------------------------

        //...
        void OnEnable() {
            titleContent = new GUIContent("Refactor", StyleSheet.canvasIcon);
            GraphEditor.onCurrentGraphChanged -= OnGraphChanged;
            GraphEditor.onCurrentGraphChanged += OnGraphChanged;
        }

        //...
        void OnDisable() {
            GraphEditor.onCurrentGraphChanged -= OnGraphChanged;
            Flush();
        }

        void OnGraphChanged(Graph graph) { Flush(); Repaint(); }

        //...
        void OnGUI() {

            if ( Application.isPlaying ) {
                ShowNotification(new GUIContent("Refactor only works in editor mode. Please exit play mode."));
                return;
            }

            if ( GraphEditor.current == null || GraphEditor.currentGraph == null ) {
                ShowNotification(new GUIContent("No Graph is currently open in the Graph Editor."));
                return;
            }

            RemoveNotification();

            EditorGUILayout.HelpBox("Batch refactor missing nodes, tasks, parameters, types as well as missing reflection based methods, properties, fields and so on references. Note that changes made here are irreversible. Please proceed with caution.\n\n1) Hit Gather to fetch missing elements from the currently viewing graph in the editor.\n2) Rename elements serialization data to their new name (keep the same format).\n3) Hit Save to commit your changes.", MessageType.Info);

            if ( GUILayout.Button("Gather", GUILayout.Height(30)) ) { Gather(); }
            EditorUtils.Separator();

            if ( recoverablesMap == null || reflectedMap == null || missingParametersMap == null ) { return; }

            EditorUtils.CoolLabel("Recoverables");
            EditorGUI.indentLevel = 1;
            DoRecoverables();
            GUILayout.Space(5);

            EditorUtils.CoolLabel("Reflection");
            EditorGUI.indentLevel = 1;
            DoReflected();
            GUILayout.Space(5);

            EditorUtils.CoolLabel("Parameters");
            EditorGUI.indentLevel = 1;
            DoParameters();

            if ( recoverableChangesMap.Count > 0 || reflectedChangesMap.Count > 0 || missingParametersMap.Count > 0 ) {
                EditorUtils.Separator();
                if ( GUILayout.Button("Save", GUILayout.Height(30)) ) { Save(); }
            } else {
                EditorUtils.Separator();
            }
        }

        //...
        void DoRecoverables() {

            if ( recoverablesMap.Count == 0 ) {
                GUILayout.Label("No missing recoverable elements found.");
                return;
            }

            foreach ( var pair in recoverablesMap ) {
                var originalName = pair.Key;
                GUILayout.Label(string.Format("<b>{0} occurencies: Type '{1}'</b>", pair.Value.Count, originalName));
                GUILayout.Space(5);
                var typeName = recoverableChangesMap[originalName];
                typeName = EditorGUILayout.TextField("Type Name", typeName);
                recoverableChangesMap[originalName] = typeName;
                EditorUtils.Separator();
            }
        }

        //...
        void DoReflected() {

            if ( reflectedMap.Count == 0 ) {
                GUILayout.Label("No missing reflected references found.");
                return;
            }

            foreach ( var pair in reflectedMap ) {
                var information = pair.Key;
                GUILayout.Label(string.Format("<b>{0} occurencies: '{1}'</b>", pair.Value.Count, information));
                GUILayout.Space(5);
                fsData data = reflectedChangesMap[information];
                var dict = new Dictionary<string, fsData>(data.AsDictionary);
                foreach ( var dataPair in dict ) {
                    var value = dataPair.Value.AsString;
                    var newValue = EditorGUILayout.TextField(dataPair.Key, value);
                    if ( newValue != value ) {
                        data.AsDictionary[dataPair.Key] = new fsData(newValue);
                    }
                }
                reflectedChangesMap[information] = data;
                EditorUtils.Separator();
            }
        }

        //...
        void DoParameters() {

            if ( missingParametersMap.Count == 0 ) {
                GUILayout.Label("No missing BB Parameters found.");
                return;
            }

            foreach ( var pair in missingParameterChangesMap ) {
                BBParameterEditor.ParameterField(pair.Key, pair.Value, GraphEditor.currentGraph);
            }
        }

    }
}
#endif

