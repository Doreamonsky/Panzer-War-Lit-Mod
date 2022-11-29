#if UNITY_EDITOR

using ParadoxNotion.Design;
using NodeCanvas.Framework;
using UnityEditor;
using UnityEngine;

namespace NodeCanvas.Editor
{

    public class ExternalInspectorWindow : EditorWindow
    {

        private Vector2 scrollPos;
        private bool willRepaint;

        public static void ShowWindow() {
            var window = GetWindow<ExternalInspectorWindow>();
            window.Show();
        }

        void OnEnable() {
            Prefs.useExternalInspector = true;
            titleContent = new GUIContent("Inspector", StyleSheet.canvasIcon);
            GraphEditorUtility.onActiveElementChanged -= OnActiveElementChange;
            GraphEditorUtility.onActiveElementChanged += OnActiveElementChange;
        }

        void OnDisable() {
            Prefs.useExternalInspector = false;
            GraphEditorUtility.onActiveElementChanged -= OnActiveElementChange;
        }

        void OnActiveElementChange(IGraphElement element) {
            willRepaint = true;
        }

        void Update() {
            if ( willRepaint ) {
                willRepaint = false;
                Repaint();
            }
        }

        void OnGUI() {

            if ( GraphEditor.current == null || GraphEditor.currentGraph == null ) {
                GUILayout.Label("No graph is open in the Graph Editor");
                return;
            }

            if ( EditorApplication.isCompiling && !Application.isPlaying) {
                ShowNotification(new GUIContent("...Compiling Please Wait..."));
                return;
            }

            var currentSelection = GraphEditorUtility.activeElement;
            if ( currentSelection == null ) {
                GUILayout.Label("No selection in Graph Editor");
                return;
            }

            UndoUtility.CheckUndo(currentSelection.graph, "Inspector Change");
            scrollPos = GUILayout.BeginScrollView(scrollPos);
            {
                if ( currentSelection is Node ) {
                    var node = (Node)currentSelection;
                    Title(node.name);
                    Node.ShowNodeInspectorGUI(node);
                }

                if ( currentSelection is Connection ) {
                    Title("Connection");
                    Connection.ShowConnectionInspectorGUI(currentSelection as Connection);
                }
            }
            EditorUtils.EndOfInspector();
            GUILayout.EndScrollView();

            UndoUtility.CheckDirty(currentSelection.graph);

            if ( GUI.changed ) {
                GraphEditor.current.Repaint();
            }
        }

        void Title(string text) {
            GUILayout.Space(5);
            GUILayout.BeginHorizontal("box", GUILayout.Height(28));
            GUILayout.FlexibleSpace();
            GUILayout.Label("<b><size=16>" + text + "</size></b>");
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            EditorUtils.BoldSeparator();
        }
    }
}

#endif