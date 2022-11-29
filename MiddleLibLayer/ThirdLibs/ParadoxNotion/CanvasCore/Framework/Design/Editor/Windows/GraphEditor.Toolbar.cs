#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using NodeCanvas.Framework;
using ParadoxNotion.Design;
using System.Linq;

namespace NodeCanvas.Editor
{

    ///<summary>Toolbar</summary>
    public partial class GraphEditor
    {

        //This is called outside Begin/End Windows from GraphEditor.
        public static void ShowToolbar(Graph graph) {

            var owner = graph.agent as GraphOwner;

            GUILayout.BeginHorizontal(EditorStyles.toolbar);

            //----------------------------------------------------------------------------------------------
            //Left side
            //----------------------------------------------------------------------------------------------

            GUILayout.Space(4);

            if ( GUILayout.Button("File", EditorStyles.toolbarDropDown, GUILayout.Width(50)) ) {
                GetToolbarMenu_File(graph, owner).ShowAsContext();
            }

            if ( GUILayout.Button("Edit", EditorStyles.toolbarDropDown, GUILayout.Width(50)) ) {
                GetToolbarMenu_Edit(graph, owner).ShowAsContext();
            }

            if ( GUILayout.Button("Prefs", EditorStyles.toolbarDropDown, GUILayout.Width(50)) ) {
                GetToolbarMenu_Prefs(graph, owner).ShowAsContext();
            }

            GUILayout.Space(10);

            if ( owner != null && GUILayout.Button("Select Owner", EditorStyles.toolbarButton) ) {
                Selection.activeObject = owner;
                EditorGUIUtility.PingObject(owner);
            }

            if ( EditorUtility.IsPersistent(graph) && GUILayout.Button("Select Graph", EditorStyles.toolbarButton) ) {
                Selection.activeObject = graph;
                EditorGUIUtility.PingObject(graph);
            }

            GUILayout.Space(10);

            EditorGUIUtility.SetIconSize(new Vector2(15, 15));
            if ( GUILayout.Button(new GUIContent(StyleSheet.log, "Open Graph Console"), EditorStyles.toolbarButton) ) {
                GraphConsole.ShowWindow();
            }

            if ( GUILayout.Button(new GUIContent(StyleSheet.lens, "Open Graph Explorer"), EditorStyles.toolbarButton) ) {
                GraphExplorer.ShowWindow();
            }

            if ( GUILayout.Button(new GUIContent(StyleSheet.refactor, "Open Graph Refactor"), EditorStyles.toolbarButton) ) {
                GraphRefactor.ShowWindow();
            }
            EditorGUIUtility.SetIconSize(Vector2.zero);

            GUILayout.Space(10);

            ///----------------------------------------------------------------------------------------------

            graph.CallbackOnGraphEditorToolbar();

            //----------------------------------------------------------------------------------------------
            //Mid
            //----------------------------------------------------------------------------------------------

            GUILayout.Space(10);
            GUILayout.FlexibleSpace();

            //...



            //...

            GUILayout.FlexibleSpace();
            GUILayout.Space(10);

            //----------------------------------------------------------------------------------------------
            //Right side
            //----------------------------------------------------------------------------------------------

            GUI.backgroundColor = Color.clear;
            GUI.color = new Color(1, 1, 1, 0.3f);
            GUILayout.Label(string.Format("{0} @NodeCanvas Framework v{1}", graph.GetType().Name, NodeCanvas.Framework.Internal.GraphSource.FRAMEWORK_VERSION.ToString("0.00")), EditorStyles.toolbarButton);
            GUILayout.Space(10);
            GUI.color = Color.white;
            GUI.backgroundColor = Color.white;

            //GRAPHOWNER JUMP SELECTION
            if ( owner != null ) {
                if ( GUILayout.Button(string.Format("[{0}]", owner.gameObject.name), EditorStyles.toolbarDropDown, GUILayout.Width(120)) ) {
                    var menu = new GenericMenu();
                    foreach ( var _o in Object.FindObjectsOfType<GraphOwner>().OrderBy(x => x.gameObject != owner.gameObject) ) {
                        var o = _o;
                        menu.AddItem(new GUIContent(o.gameObject.name + "/" + o.GetType().Name), o == owner, () => { SetReferences(o); });
                    }
                    menu.ShowAsContext();
                }
            }

            Prefs.isEditorLocked = GUILayout.Toggle(Prefs.isEditorLocked, "Lock", EditorStyles.toolbarButton);

            GUILayout.Space(4);

            GUILayout.EndHorizontal();
            GUI.backgroundColor = Color.white;
            GUI.color = Color.white;
        }

        ///----------------------------------------------------------------------------------------------

        //FILE MENU
        static GenericMenu GetToolbarMenu_File(Graph graph, GraphOwner owner) {
            var menu = new GenericMenu();
            menu.AddItem(new GUIContent("Clear"), false, () =>
           {
               if ( EditorUtility.DisplayDialog("Clear Canvas", "This will delete all nodes of the currently viewing graph!\nAre you sure?", "YES", "NO!") ) {
                   graph.ClearGraph();
               }
           });

            menu.AddItem(new GUIContent("Import JSON"), false, () =>
          {
              if ( graph.allNodes.Count > 0 && !EditorUtility.DisplayDialog("Import Graph", "All current graph information will be lost. Are you sure?", "YES", "NO") ) {
                  return;
              }

              var path = EditorUtility.OpenFilePanel(string.Format("Import '{0}' Graph", graph.GetType().Name), "Assets", graph.GetGraphJSONFileExtension());
              if ( !string.IsNullOrEmpty(path) ) {
                  if ( graph.Deserialize(System.IO.File.ReadAllText(path), null, true) == false ) { //true: validate, null: graph._objectReferences
                      EditorUtility.DisplayDialog("Import Failure", "Please read the logs for more information", "OK", string.Empty);
                  }
              }
          });

            menu.AddItem(new GUIContent("Export JSON"), false, () =>
          {
              var path = EditorUtility.SaveFilePanelInProject(string.Format("Export '{0}' Graph", graph.GetType().Name), string.Empty, graph.GetGraphJSONFileExtension(), string.Empty);
              if ( !string.IsNullOrEmpty(path) ) {
                  var json = graph.Serialize(null);
                  json = ParadoxNotion.Serialization.JSONSerializer.PrettifyJson(json);
                  System.IO.File.WriteAllText(path, json);
                  AssetDatabase.Refresh();
              }
          });

            menu.AddItem(new GUIContent("Show JSON"), false, () =>
           {
               graph.SelfSerialize();
               ParadoxNotion.Serialization.JSONSerializer.ShowData(graph.GetSerializedJsonData(), graph.name);
           });

            return menu;
        }

        ///----------------------------------------------------------------------------------------------

        //EDIT MENU
        static GenericMenu GetToolbarMenu_Edit(Graph graph, GraphOwner owner) {
            var menu = new GenericMenu();
            //Bind
            if ( !Application.isPlaying && owner != null && !owner.graphIsBound ) {
                menu.AddItem(new GUIContent("Bind To Owner"), false, () =>
                {
                    if ( EditorUtility.DisplayDialog("Bind Graph", "This will make a local copy of the graph, bound to the owner.\n\nThis allows you to make local changes and assign scene object references directly.\n\nNote that you can also use scene object references through the use of Blackboard Variables.\n\nBind Graph?", "YES", "NO") ) {
                        UndoUtility.RecordObject(owner, "New Local Graph");
                        owner.SetBoundGraphReference(owner.graph);
                        UndoUtility.SetDirty(owner);
                    }
                });
            } else menu.AddDisabledItem(new GUIContent("Bind To Owner"));

            //Save to asset
            if ( !EditorUtility.IsPersistent(graph) ) {
                menu.AddItem(new GUIContent("Save To Asset"), false, () =>
                {
                    var newGraph = (Graph)EditorUtils.CreateAsset(graph.GetType());
                    if ( newGraph != null ) {
                        EditorUtility.CopySerialized(graph, newGraph);
                        newGraph.Validate();
                        AssetDatabase.SaveAssets();
                    }
                });
            } else menu.AddDisabledItem(new GUIContent("Save To Asset"));

            //Create defined vars
            if ( graph.blackboard != null ) {
                foreach ( var bb in graph.blackboard.GetAllParents(true).Reverse() ) {
                    var category = "Promote Missing Parameters To Variables/";
                    menu.AddItem(new GUIContent(category + $"In '{bb.identifier}' Blackboard"), false, () =>
                   {
                       if ( EditorUtility.DisplayDialog("Promote Missing Parameters", "This will fill the target Blackboard with a Variable for each defined missing Parameter in the graph.\nContinue?", "YES", "NO") ) {
                           UndoUtility.RecordObject(graph, "Promote Variables");
                           UndoUtility.RecordObject(bb.unityContextObject, "Promote Variables");
                           graph.PromoteMissingParametersToVariables(bb);
                           UndoUtility.SetDirty(graph);
                           UndoUtility.SetDirty(bb.unityContextObject);
                       }
                   });
                }
            } else menu.AddDisabledItem(new GUIContent("Promote Defined Parameters To Variables"));

            menu.AddItem(new GUIContent("Scan Graph for Serialized Struct Types"), false, () =>
            {
                GraphEditorUtility.ScanForStructTypesAndAppendThem(graph);
            });

            if ( !Application.isPlaying ) {
                menu.AddItem(new GUIContent("Re-Validate Graph"), false, () =>
                {
                    graph.SelfDeserialize();
                    graph.Validate();
                    GraphEditorUtility.activeElement = null;
                    FullDrawPass();
                });
            } else menu.AddDisabledItem(new GUIContent("Re-Validate Graph"));

            return menu;
        }

        ///----------------------------------------------------------------------------------------------

        //PREFS MENU
        static GenericMenu GetToolbarMenu_Prefs(Graph graph, GraphOwner owner) {
            var menu = new GenericMenu();
            menu.AddItem(new GUIContent("Show Icons"), Prefs.showIcons, () => { Prefs.showIcons = !Prefs.showIcons; });
            menu.AddItem(new GUIContent("Show Node Help"), Prefs.showNodeInfo, () => { Prefs.showNodeInfo = !Prefs.showNodeInfo; });
            menu.AddItem(new GUIContent("Show Comments"), Prefs.showComments, () => { Prefs.showComments = !Prefs.showComments; });
            menu.AddItem(new GUIContent("Show Summary Info"), Prefs.showTaskSummary, () => { Prefs.showTaskSummary = !Prefs.showTaskSummary; });
            menu.AddItem(new GUIContent("Show Node IDs"), Prefs.showNodeIDs, () => { Prefs.showNodeIDs = !Prefs.showNodeIDs; });
            menu.AddItem(new GUIContent("Show Node Running Times"), Prefs.showNodeElapsedTimes, () => { Prefs.showNodeElapsedTimes = !Prefs.showNodeElapsedTimes; });
            menu.AddItem(new GUIContent("Show Grid"), Prefs.showGrid, () => { Prefs.showGrid = !Prefs.showGrid; });
            menu.AddItem(new GUIContent("Grid Snap"), Prefs.snapToGrid, () => { Prefs.snapToGrid = !Prefs.snapToGrid; });
            menu.AddItem(new GUIContent("Log Events Info"), Prefs.logEventsInfo, () => { Prefs.logEventsInfo = !Prefs.logEventsInfo; });
            menu.AddItem(new GUIContent("Log Variables Info"), Prefs.logVariablesInfo, () => { Prefs.logVariablesInfo = !Prefs.logVariablesInfo; });
            menu.AddItem(new GUIContent("Breakpoints Pause Editor"), Prefs.breakpointPauseEditor, () => { Prefs.breakpointPauseEditor = !Prefs.breakpointPauseEditor; });
            menu.AddItem(new GUIContent("Animate Inspector Panel"), Prefs.animatePanels, () => { Prefs.animatePanels = !Prefs.animatePanels; });
            menu.AddItem(new GUIContent("Show Hierarchy Icons"), Prefs.showHierarchyIcons, () => { Prefs.showHierarchyIcons = !Prefs.showHierarchyIcons; });
            menu.AddItem(new GUIContent("Collapse Generics In Browser"), Prefs.collapseGenericTypes, () => { Prefs.collapseGenericTypes = !Prefs.collapseGenericTypes; EditorUtils.FlushScriptInfos(); });
            menu.AddItem(new GUIContent("Connection Style/Hard"), false, () => { Prefs.connectionsMLT = 1f; });
            menu.AddItem(new GUIContent("Connection Style/Soft"), false, () => { Prefs.connectionsMLT = 0.75f; });
            menu.AddItem(new GUIContent("Connection Style/Softer"), false, () => { Prefs.connectionsMLT = 0.5f; });
            menu.AddItem(new GUIContent("Connection Style/Direct"), false, () => { Prefs.connectionsMLT = 0f; });
            if ( graph.isTree ) {
                menu.AddItem(new GUIContent("Automatic Hierarchical Move"), Prefs.hierarchicalMove, () => { Prefs.hierarchicalMove = !Prefs.hierarchicalMove; });
            }
            menu.AddItem(new GUIContent("Open Preferred Types Editor..."), false, () => { TypePrefsEditorWindow.ShowWindow(); });
            return menu;
        }
    }
}

#endif