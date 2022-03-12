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


        ///<summary>Start the DialogueTree without an Instigator</summary>
        public void StartDialogue() {
            StartDialogue(this, null);
        }

        ///<summary>Start the DialogueTree with a callback for when its finished</summary>
        public void StartDialogue(Action<bool> callback) {
            StartDialogue(this, callback);
        }

        ///<summary>Start the DialogueTree with provided actor as Instigator</summary>
        public void StartDialogue(IDialogueActor instigator) {
            StartDialogue(instigator, null);
        }

        ///<summary>Assign a new DialogueTree and Start it</summary>
        public void StartDialogue(DialogueTree newTree, IDialogueActor instigator, Action<bool> callback) {
            graph = newTree;
            StartDialogue(instigator, callback);
        }

        ///<summary>Start the already assgined DialogueTree with provided actor as instigator and callback</summary>
        public void StartDialogue(IDialogueActor instigator, Action<bool> callback) {
            graph = GetInstance(graph);
            graph.StartGraph(instigator is Component ? (Component)instigator : instigator.transform, blackboard, updateMode, callback);
        }

        ///<summary>Pause the DialogueTree</summary>
        public void PauseDialogue() {
            graph.Pause();
        }

        ///<summary>Stop the DialogueTree</summary>
        public void StopDialogue() {
            graph.Stop();
        }

        ///<summary>Set an actor reference by parameter name</summary>
        public void SetActorReference(string paramName, IDialogueActor actor) {
            if ( behaviour != null ) {
                behaviour.SetActorReference(paramName, actor);
            }
        }

        ///<summary>Set all actor reference parameters at once</summary>
        public void SetActorReferences(Dictionary<string, IDialogueActor> actors) {
            if ( behaviour != null ) {
                behaviour.SetActorReferences(actors);
            }
        }

        ///<summary>Get the actor reference by parameter name</summary>
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