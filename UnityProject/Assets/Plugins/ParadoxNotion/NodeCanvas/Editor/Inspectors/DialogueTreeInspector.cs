#if UNITY_EDITOR

using System.Linq;
using NodeCanvas.DialogueTrees;
using ParadoxNotion.Design;
using UnityEditor;
using UnityEngine;


namespace NodeCanvas.Editor
{

    [CustomEditor(typeof(DialogueTree))]
    public class DialogueTreeInspector : GraphInspector
    {

        private DialogueTree dialogue {
            get { return target as DialogueTree; }
        }

        public override void OnInspectorGUI() {
            base.OnInspectorGUI();
            ShowActorParameters(dialogue);
            EditorUtils.EndOfInspector();
            if ( GUI.changed ) { UndoUtility.SetDirty(dialogue); }
        }

        //static because it's also used from DialogueTreeController
        public static void ShowActorParameters(DialogueTree dialogue) {
            EditorUtils.CoolLabel("Dialogue Actor Parameters");
            EditorGUILayout.HelpBox("Enter the Key-Value pair for Dialogue Actors involved in the Dialogue.\nThe reference Object must be an IDialogueActor or have an IDialogueActor component. Referencing a Dialogue Actor is optional.", MessageType.Info);

            GUILayout.BeginVertical(GUI.skin.box);

            if ( GUILayout.Button("Add Actor Parameter") ) {
                UndoUtility.RecordObject(dialogue, "New Actor Parameter");
                dialogue.actorParameters.Add(new DialogueTree.ActorParameter("actor parameter name"));
                UndoUtility.SetDirty(dialogue);
            }

            EditorGUILayout.LabelField("INSTIGATOR --> Replaced by the Actor starting the Dialogue");

            var options = new EditorUtils.ReorderableListOptions();
            options.allowAdd = false;
            options.allowRemove = true;
            options.unityObjectContext = dialogue;
            EditorUtils.ReorderableList(dialogue.actorParameters, options, (i, picked) =>
            {
                var reference = dialogue.actorParameters[i];
                GUILayout.BeginHorizontal();
                if ( dialogue.actorParameters.Where(r => r != reference).Select(r => r.name).Contains(reference.name) ) {
                    GUI.backgroundColor = Color.red;
                }
                var newRefName = EditorGUILayout.DelayedTextField(reference.name);
                if ( newRefName != reference.name ) {
                    UndoUtility.RecordObject(dialogue, "Actor Parameter Name Change");
                    reference.name = newRefName;
                    UndoUtility.SetDirty(dialogue);
                }
                GUI.backgroundColor = Color.white;
                var newReference = (Object)EditorGUILayout.ObjectField(reference.actor as Object, typeof(Object), true);
                if ( !ReferenceEquals(newReference, reference.actor) ) {
                    UndoUtility.RecordObject(dialogue, "Actor Assignment");
                    //all this jazz because ObjectField does not work with interfaces...
                    if ( newReference == null ) {
                        reference.actor = null;
                    } else {
                        if ( newReference is Component ) { newReference = ( newReference as Component ).GetComponent(typeof(IDialogueActor)); }
                        if ( newReference is GameObject ) { newReference = ( newReference as GameObject ).GetComponent(typeof(IDialogueActor)); }
                        if ( newReference is IDialogueActor ) {
                            reference.actor = (IDialogueActor)newReference;
                        } else { ParadoxNotion.Services.Logger.LogWarning("Object is not an IDialogueActor and none of the components attached is an IDialogueActor either."); }
                    }
                    UndoUtility.SetDirty(dialogue);
                }
                GUILayout.EndHorizontal();
            });

            GUILayout.EndVertical();
        }
    }
}

#endif