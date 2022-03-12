#if UNITY_EDITOR

using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using NodeCanvas.Framework;
using ParadoxNotion;
using ParadoxNotion.Serialization;
using ParadoxNotion.Design;

namespace NodeCanvas.Editor
{

    //To let the editor interop with other things
    public static class GraphEditorUtility
    {

        public static bool allowClick = true;
        public static Vector2 realMousePosition { get; set; }
        public static int lastNodeID { get; private set; }
        public static int lastConnectionID { get; private set; }
        public static event System.Action PostGUI;
        public static event System.Action<IGraphElement> onActiveElementChanged;
        private static WeakReference<IGraphElement> _activeElement = new WeakReference<IGraphElement>(null);
        private static WeakReferenceList<IGraphElement> _activeElements = new WeakReferenceList<IGraphElement>();

        ///----------------------------------------------------------------------------------------------

        //Invoke PostGUI
        internal static void InvokePostGUI() {
            if ( PostGUI != null ) {
                PostGUI();
                PostGUI = null;
            }
        }

        ///----------------------------------------------------------------------------------------------

        ///<summary>Selected Node or Connection</summary>
        public static IGraphElement activeElement {
            get
            {
                if ( activeElements.Count > 1 ) { return null; }
                if ( activeElements.Count == 1 ) { return activeElements[0]; }
                _activeElement.TryGetTarget(out IGraphElement reference);
                return reference;
            }
            set
            {
                GUIUtility.keyboardControl = 0;
                if ( !activeElements.Contains(value) || value == null ) {
                    _activeElements.Clear();
                }
                _activeElement.TryGetTarget(out IGraphElement reference);
                if ( reference != value && activeElements.Count == 0 ) {
                    _activeElement.SetTarget(value);
                    lastNodeID = -1;
                    lastConnectionID = -1;
                    if ( value is Node ) {
                        lastNodeID = ( value as Node ).ID;
                    }
                    if ( value is Connection ) {
                        lastNodeID = ( value as Connection ).sourceNode.ID;
                        lastConnectionID = ( value as Connection ).sourceNode.outConnections.IndexOf(value as Connection);
                    }
                    UnityEditor.SceneView.RepaintAll();
                    if ( onActiveElementChanged != null ) {
                        onActiveElementChanged(value);
                    }
                }
            }
        }

        ///<summary>multiple selected Node or Connection</summary>
        public static List<IGraphElement> activeElements {
            get { return _activeElements.ToReferenceList(); }
            set
            {
                GUIUtility.keyboardControl = 0;
                if ( value != null && value.Count == 1 ) {
                    activeElement = value[0];
                    value.Clear();
                }
                _activeElements = value != null ? value : new List<IGraphElement>();
            }
        }

        ///<summary>Selected Node if any</summary>
        public static Node activeNode {
            get { return activeElement as Node; }
        }

        ///<summary>Selected Connection if any</summary>
        public static Connection activeConnection {
            get { return activeElement as Connection; }
        }

        ///----------------------------------------------------------------------------------------------

        ///<summary>Adds an element to active elements</summary>
        public static void AddActiveElement(IGraphElement e) {
            _activeElements.Add(e);
        }

        ///<summary>Removes and element from active elements</summary>
        public static void RemoveActiveElement(IGraphElement e) {
            _activeElements.Remove(e);
        }

        ///----------------------------------------------------------------------------------------------

        //Find nodes of type (T) having the [DropReferenceType] attribute pointing to target unity object type
        public static IEnumerable<System.Type> GetDropedReferenceNodeTypes<T>(UnityEngine.Object obj) where T : IGraphElement {
            var targetType = obj.GetType();
            foreach ( var type in ReflectionTools.GetImplementationsOf(typeof(T)) ) {
                var att = type.RTGetAttribute<DropReferenceType>(true);
                if ( att != null && att.type == targetType ) {
                    yield return type;
                }
            }
        }

        ///----------------------------------------------------------------------------------------------

        ///<summary>Returns the extension at which the graph will be saved with if exported to JSON</summary>
        public static string GetGraphJSONFileExtension(this Graph graph) {
            return graph.GetType().Name.GetCapitals();
        }

        ///<summary>Returns the selected nodes if any or all graph nodes</summary>
        public static IEnumerable<Node> GetSelectedOrAll(this Graph graph) {
            if ( activeNode != null ) { return new Node[] { activeNode }; }
            if ( activeElements.Count > 0 ) { return activeElements.OfType<Node>(); }
            return graph.allNodes;
        }

        ///<summary>Scans the graph for structs and append them in preferred types. Prompts user for confirm.</summary>
        public static void ScanForStructTypesAndAppendThem(Graph graph) {

            var serializedTypes = new List<Type>();
            JSONSerializer.SerializeAndExecuteNoCycles(typeof(NodeCanvas.Framework.Internal.GraphSource), graph.GetGraphSource(), (o, d) =>
            {
                if ( o != null ) { serializedTypes.Add(o.GetType()); }
            });

            serializedTypes = serializedTypes.Concat(serializedTypes.Where(t => t.IsGenericType).Select(t => t.RTGetGenericArguments().First())).ToList();

            var preferredTypes = TypePrefs.GetPreferedTypesList();
            var resultTypes = new List<System.Type>();
            for ( var i = 0; i < serializedTypes.Count; i++ ) {
                var t = serializedTypes[i];
                if ( preferredTypes.Contains(t) ) { continue; }
                if ( resultTypes.Contains(t) ) { continue; }
                if ( t.IsValueType ) { resultTypes.Add(t); }
            }
            if ( resultTypes.Count == 0 ) {
                EditorUtility.DisplayDialog("Scan Results:", "All found struct types serialized in the graph are already in your Preferred Types list.", "OK");
                return;
            }
            var userInfo = string.Join("\n", resultTypes.OrderBy(t => t.Namespace).ThenBy(t => t.Name).Select(t => t.FriendlyName()));
            userInfo = "The following struct types serialized in the graph were found that are not already in your Preferred Types List\n\n\n----\n" + userInfo + "\n----\n\n\nAdd them in your Preferred Types List?";
            if ( EditorUtility.DisplayDialog("Scan Results:", userInfo, "Add Them", "Cancel") ) {
                TypePrefs.SetPreferedTypesList(preferredTypes.Union(resultTypes).ToList());
            }
        }

        ///<summary>Make a deep copy of provided graph asset along with it's sub-graphs.</summary>
        public static Graph DeepCopy(Graph root) {
            if ( root == null ) {
                return null;
            }

            var path = EditorUtility.SaveFilePanelInProject("Copy of " + root.name, root.name + "_duplicate.asset", "asset", string.Empty);
            if ( string.IsNullOrEmpty(path) ) {
                return null;
            }

            var copy = (Graph)ScriptableObject.CreateInstance(root.GetType());
            AssetDatabase.CreateAsset(copy, path);
            EditorUtility.CopySerialized(root, copy);

            //make use of IGraphAssignable interface to find nodes that represent a sub-graph.
            foreach ( var subGraphNode in copy.allNodes.OfType<IGraphAssignable>() ) {
                if ( subGraphNode.subGraph != null ) {
                    //duplicate the existing sub-graph and assign the copy to node.
                    subGraphNode.subGraph = DeepCopy(subGraphNode.subGraph);
                }
            }

            copy.Validate();
            AssetDatabase.SaveAssets();
            return copy;
        }
    }
}

#endif