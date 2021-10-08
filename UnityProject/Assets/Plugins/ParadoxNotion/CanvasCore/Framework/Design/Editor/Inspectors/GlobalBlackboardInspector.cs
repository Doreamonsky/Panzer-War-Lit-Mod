#if UNITY_EDITOR

using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEditor;
using UnityEngine;

namespace NodeCanvas.Editor
{

    [CustomEditor(typeof(GlobalBlackboard))]
    public class GlobalBlackboardInspector : UnityEditor.Editor
    {

        private GlobalBlackboard bb { get { return (GlobalBlackboard)target; } }

        private SerializedProperty dontDestroyProp;
        private SerializedProperty identifierProp;
        private SerializedProperty destroyDupProp;

        void OnEnable() {
            dontDestroyProp = serializedObject.FindProperty("_dontDestroyOnLoad");
            identifierProp = serializedObject.FindProperty("_identifier");
            destroyDupProp = serializedObject.FindProperty("_singletonMode");
        }

        public override void OnInspectorGUI() {

            EditorGUILayout.PropertyField(identifierProp);
            var existing = GlobalBlackboard.Find(bb.name);
            if ( existing != bb && existing != null && !EditorUtility.IsPersistent(bb) ) {
                EditorUtils.MarkLastFieldError("Another Global Blackboard has the same identifier name. Please rename either.");
            }
            EditorGUILayout.PropertyField(destroyDupProp);
            EditorGUILayout.PropertyField(dontDestroyProp);

            BlackboardEditor.ShowVariables(bb);
            EditorUtils.EndOfInspector();
            serializedObject.ApplyModifiedProperties();
            if ( Event.current.isMouse ) {
                Repaint();
            }
        }
    }
}

#endif