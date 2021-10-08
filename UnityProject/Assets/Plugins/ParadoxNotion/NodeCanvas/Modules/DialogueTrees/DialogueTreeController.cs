using System;
using System.Collections.Generic;
using UnityEngine;
using NodeCanvas.Framework;
using ParadoxNotion;

namespace NodeCanvas.DialogueTrees
{

    [AddComponentMenu("NodeCanvas/Dialogue Tree Controller")]
    public class DialogueTreeController : GraphOwner<DialogueTree>, IDialogueActor
    {

        string IDialogueActor.name => name;
        Texture2D IDialogueActor.portrait => null;
        Sprite IDialogueActor.portraitSprite => null;
        Color IDialogueActor.dialogueColor => Color.white;
        Vector3 IDialogueActor.dialoguePosition => Vector3.zero;
        Transform IDialogueActor.transform => transform;


        ///Start the DialogueTree without an Instigator
        public void StartDialogue() {
            StartDialogue(this, null);
        }

        ///Start the DialogueTree with a callback for when its finished
        public void StartDialogue(Action<bool> callback) {
            StartDialogue(this, callback);
        }

        ///Start the DialogueTree with provided actor as Instigator
        public void StartDialogue(IDialogueActor instigator) {
            StartDialogue(instigator, null);
        }

        ///Assign a new DialogueTree and Start it
        public void StartDialogue(DialogueTree newTree, IDialogueActor instigator, Action<bool> callback) {
            graph = newTree;
            StartDialogue(instigator, callback);
        }

        ///Start the already assgined DialogueTree with provided actor as instigator and callback
        public void StartDialogue(IDialogueActor instigator, Action<bool> callback) {
            graph = GetInstance(graph);
            graph.StartGraph(instigator is Component ? (Component)instigator : instigator.transform, blackboard, updateMode, callback);
        }

        ///Pause the DialogueTree
        public void PauseDialogue() {
            graph.Pause();
        }

        ///Stop the DialogueTree
        public void StopDialogue() {
            graph.Stop();
        }

        ///Set an actor reference by parameter name
        public void SetActorReference(string paramName, IDialogueActor actor) {
            if ( behaviour != null ) {
                behaviour.SetActorReference(paramName, actor);
            }
        }

        ///Set all actor reference parameters at once
        public void SetActorReferences(Dictionary<string, IDialogueActor> actors) {
            if ( behaviour != null ) {
                behaviour.SetActorReferences(actors);
            }
        }

        ///Get the actor reference by parameter name
        public IDialogueActor GetActorReferenceByName(string paramName) {
            return behaviour != null ? behaviour.GetActorReferenceByName(paramName) : null;
        }

        ///----------------------------------------------------------------------------------------------
        ///---------------------------------------UNITY EDITOR-------------------------------------------
#if UNITY_EDITOR
        new void Reset() {
            base.enableAction = EnableAction.DoNothing;
            base.disableAction = DisableAction.DoNothing;
            blackboard = gameObject.GetAddComponent<Blackboard>();
            SetBoundGraphReference(ScriptableObject.CreateInstance<DialogueTree>());
        }
#endif

    }
}