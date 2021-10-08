#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using ParadoxNotion.Design;
using ParadoxNotion;
using NodeCanvas.Framework;

namespace NodeCanvas.Editor
{

    [CustomEditor(typeof(SignalDefinition))]
    public class SignalDefinitionInspector : UnityEditor.Editor
    {

        public override void OnInspectorGUI() {

            base.OnInspectorGUI();

            var def = (SignalDefinition)target;

            if ( GUILayout.Button("Add Parameter") ) {
                EditorUtils.ShowPreferedTypesSelectionMenu(typeof(object), (t) =>
                {
                    UndoUtility.RecordObjectComplete(def, "Add Parameter");
                    def.AddParameter(t.FriendlyName(), t);
                    UndoUtility.SetDirty(def);
                });
            }

            UndoUtility.CheckUndo(def, "Definition");
            var options = new EditorUtils.ReorderableListOptions();
            options.allowRemove = true;
            options.unityObjectContext = def;
            EditorUtils.ReorderableList(def.parameters, options, (i, picked) =>
            {
                var parameter = def.parameters[i];
                GUILayout.BeginHorizontal();
                parameter.name = UnityEditor.EditorGUILayout.DelayedTextField(parameter.name, GUILayout.Width(150), GUILayout.ExpandWidth(true));
                EditorUtils.ButtonTypePopup("", parameter.type, (t) => { parameter.type = t; });
                GUILayout.EndHorizontal();
            });
            UndoUtility.CheckDirty(def);

            EditorUtils.EndOfInspector();
            if ( Event.current.isMouse ) { Repaint(); }
        }
    }
}

#endif