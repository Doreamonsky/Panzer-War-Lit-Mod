using System;
using System.Collections;
using ParadoxNotion.Services;
using ParadoxNotion.Serialization;
using ParadoxNotion.Serialization.FullSerializer;
using NodeCanvas.Framework.Internal;
using UnityEngine;


namespace NodeCanvas.Framework
{

    ///<summary>Base class for actions. Extend this to create your own. T is the agentType required by the action. Generic version where T is the AgentType (Component or Interface) required by the Action. For GameObject, use 'Transform'</summary>
    abstract public class ActionTask<T> : ActionTask where T : class
    {
        sealed public override Type agentType { get { return typeof(T); } }
        new public T agent { get { return base.agent as T; } }
    }

    ///----------------------------------------------------------------------------------------------

#if UNITY_EDITOR //handles missing types
    [fsObject(Processor = typeof(fsRecoveryProcessor<ActionTask, MissingAction>))]
#endif

    ///<summary>Base class for all actions. Extend this to create your own.</summary>
    abstract public class ActionTask : Task
    {

        private Status status = Status.Resting;
        private float timeStarted;
        private bool latch;

        ///<summary>The time in seconds this action is running if at all</summary>
        public float elapsedTime => ( isRunning ? ownerSystem.elapsedTime - timeStarted : 0 );

        ///<summary>Is the action currently running?</summary>
        public bool isRunning => status == Status.Running;

        ///<summary>Is the action currently paused?</summary>
        public bool isPaused { get; private set; }

        ///----------------------------------------------------------------------------------------------

        ///<summary>Used to call an action for standalone execution providing a callback. Be careful! *This will make the action execute as a coroutine*</summary>
        public void ExecuteIndependent(Component agent, IBlackboard blackboard, Action<Status> callback) {
            if ( !isRunning ) { MonoManager.current.StartCoroutine(IndependentActionUpdater(agent, blackboard, callback)); }
        }

        //The internal updater for when an action has been called with a callback parameter and only then.
        //This is only used and usefull if user needs to execute an action task completely as standalone.
        IEnumerator IndependentActionUpdater(Component agent, IBlackboard blackboard, Action<Status> callback) {
            while ( Execute(agent, blackboard) == Status.Running ) { yield return null; }
            if ( callback != null ) { callback(status); }
        }

        ///----------------------------------------------------------------------------------------------

        [System.Obsolete("Use 'Execute'")]
        public Status ExecuteAction(Component agent, IBlackboard blackboard) { return Execute(agent, blackboard); }

        ///<summary>Ticks the action for the provided agent and blackboard</summary>
        public Status Execute(Component agent, IBlackboard blackboard) {

            if ( !isUserEnabled ) {
                return Status.Optional;
            }

            if ( isPaused ) {
                OnResume();
            }

            isPaused = false;
            if ( status == Status.Running ) {
                OnUpdate();
                latch = false;
                return status;
            }

            //latch is used to be able to call EndAction anywhere
            if ( latch ) {
                latch = false;
                return status;
            }

            if ( !Set(agent, blackboard) ) {
                latch = false;
                return Status.Failure;
            }

            timeStarted = ownerSystem.elapsedTime;
            status = Status.Running;
            OnExecute();
            if ( status == Status.Running ) {
                OnUpdate();
            }
            latch = false;
            return status;
        }

        ///<summary>Ends the action either in success or failure. Ending with null means that it's a cancel/interrupt. Null is used by the external system. You should use true or false when calling EndAction within it.</summary>
        public void EndAction() { EndAction(true); }
        public void EndAction(bool success) { EndAction((bool?)success); }
        public void EndAction(bool? success) {

            if ( status != Status.Running ) {
                // if ( success == null ) { latch = false; }
                return;
            }

            latch = success != null ? true : false;

            isPaused = false;
            status = success == null ? Status.Resting : ( success == true ? Status.Success : Status.Failure );
            OnStop(success == null);
        }

        ///<summary>Pause the action from updating and calls OnPause</summary>
        public void Pause() {

            if ( status != Status.Running ) {
                return;
            }

            isPaused = true;
            OnPause();
        }

        ///----------------------------------------------------------------------------------------------

        ///<summary>Called once when the actions is executed.</summary>
        virtual protected void OnExecute() { }
        ///<summary>Called every frame, if and while the action is running and until it ends.</summary>
        virtual protected void OnUpdate() { }
        ///<summary>Called whenever the action ends due to any reason with the argument denoting whether the action was interrupted or finished properly.</summary>
        virtual protected void OnStop(bool interrupted) { OnStop(); }
        ///<summary>Called whenever the action ends due to any reason.</summary>
        virtual protected void OnStop() { }
        ///<summary>Called when the action gets paused</summary>
        virtual protected void OnPause() { }
        ///<summary>Called when the action resumes after being paused</summary>
        virtual protected void OnResume() { }
        ///----------------------------------------------------------------------------------------------
    }
}