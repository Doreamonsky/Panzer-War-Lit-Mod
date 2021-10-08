using System.Diagnostics;
using UnityEngine;

namespace ParadoxNotion.Design
{

    /// A simple Undo utility to avoid checking application playing, nulls and if unity editor all the time
    public static class UndoUtility
    {
        public static string lastOperationName { get; private set; }

        ///Same as Undo.RecordObject. Checks null and only takes place in editor time
        [Conditional("UNITY_EDITOR")]
        public static void RecordObject(Object target, string name) {
#if UNITY_EDITOR
            if ( Application.isPlaying || target == null ) { return; }
            lastOperationName = name;
            UnityEditor.Undo.RecordObject(target, name);
#endif
        }

        ///Same as Undo.RegisterCompleteObjectUndo. Checks null and only takes place in editor time
        [Conditional("UNITY_EDITOR")]
        public static void RecordObjectComplete(Object target, string name) {
#if UNITY_EDITOR
            if ( Application.isPlaying || target == null ) { return; }
            lastOperationName = name;
            UnityEditor.Undo.RegisterCompleteObjectUndo(target, name);
#endif
        }

        ///Same as EditorUtility.SetDirty. Checks null and only takes place in editor time
        [Conditional("UNITY_EDITOR")]
        public static void SetDirty(Object target) {
#if UNITY_EDITOR
            if ( Application.isPlaying || target == null ) { return; }
            UnityEditor.EditorUtility.SetDirty(target);
#endif
        }

        [Conditional("UNITY_EDITOR")]
        public static void RecordObject(Object target, string name, System.Action operation) {
            RecordObject(target, name);
            operation();
            SetDirty(target);
        }

        [Conditional("UNITY_EDITOR")]
        public static void RecordObjectComplete(Object target, string name, System.Action operation) {
            RecordObjectComplete(target, name);
            operation();
            SetDirty(target);
        }

        ///Shortcut to return the last undo operation name or the one provided
        public static string GetLastOperationNameOr(string operation) {
            return string.IsNullOrEmpty(lastOperationName) ? operation : lastOperationName;
        }

        ///----------------------------------------------------------------------------------------------

        ///Checks some commong event types and records undo if that is the case
        public static void CheckUndo(Object target, string name) {
            var e = Event.current;
            if (
                ( e.type == EventType.MouseDown ) ||
                ( e.type == EventType.KeyDown ) ||
                ( e.type == EventType.DragPerform ) ||
                ( e.type == EventType.ExecuteCommand )
                ) {
                lastOperationName = name;
                RecordObject(target, name);
            }
        }

        ///Set target dirty if gui changed
        public static void CheckDirty(Object target) {
            if ( GUI.changed ) { SetDirty(target); }
        }
    }
}