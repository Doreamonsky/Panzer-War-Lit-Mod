#if UNITY_EDITOR

using NodeCanvas.Framework;
using ParadoxNotion;
using ParadoxNotion.Design;
using UnityEditor;
using UnityEngine;


namespace NodeCanvas.Editor
{

    [CustomEditor(typeof(Blackboard))]
    public class BlackboardInspector : UnityEditor.Editor
    {

        private Blackboard bb { get { return (Blackboard)target; } }

        private SerializedProperty parentBlackboardProp;

        void OnEnable() {
            parentBlackboardProp = serializedObject.FindProperty("_parentBlackboard");
        }

        public override void OnInspectorGUI() {
            GUI.color = GUI.color.WithAlpha(parentBlackboardProp.objectReferenceValue ? 1 : 0.6f);
            EditorGUILayout.PropertyField(parentBlackboardProp, EditorUtils.GetTempContent("Parent Asset Blackboard", null, "Optional Parent Asset Blackboard to 'inherit' variables from."));
            serializedObject.ApplyModifiedProperties();
            GUI.color = Color.white;

            BlackboardEditor.ShowVariables(bb);
            EditorUtils.EndOfInspector();
            if ( Event.current.isMouse ) {
                Repaint();
            }
        }
    }
}

#endif