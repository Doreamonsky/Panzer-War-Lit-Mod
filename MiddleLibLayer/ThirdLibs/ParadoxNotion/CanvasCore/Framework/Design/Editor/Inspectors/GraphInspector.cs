#if UNITY_EDITOR

using NodeCanvas.Framework;
using ParadoxNotion;
using ParadoxNotion.Design;
using UnityEditor;
using UnityEngine;


namespace NodeCanvas.Editor
{

    [CustomEditor(typeof(Graph), true)]
    public class GraphInspector : UnityEditor.Editor
    {

        private Graph graph => (Graph)target;

        public override void OnInspectorGUI() {
            UndoUtility.CheckUndo(this, "Graph Inspector");
            ShowBasicGUI();
            EditorUtils.Separator();
            ShowBlackboardGUI();
            EditorUtils.EndOfInspector();
            UndoUtility.CheckDirty(this);
        }

        //name, description, edit button
        public void ShowBasicGUI() {
            GUILayout.Space(10);
            graph.category = GUILayout.TextField(graph.category);
            EditorUtils.CommentLastTextField(graph.category, "Category...");

            graph.comments = GUILayout.TextArea(graph.comments, GUILayout.Height(45));
            EditorUtils.CommentLastTextField(graph.comments, "Comments...");

            GUI.backgroundColor = Colors.lightBlue;
            if ( GUILayout.Button(string.Format("EDIT {0}", graph.GetType().Name.SplitCamelCase().ToUpper())) ) {
                GraphEditor.OpenWindow(graph);
            }
            GUI.backgroundColor = Color.white;
        }

        public void ShowBlackboardGUI() {
            BlackboardEditor.ShowVariables(graph.blackboard, graph);
        }
    }
}

#endif