using System.Collections.Generic;
using NodeCanvas.Framework;
using ParadoxNotion;
using ParadoxNotion.Design;
using UnityEngine;


namespace NodeCanvas.BehaviourTrees
{

    [Name("Parallel", 8)]
    [Category("Composites")]
    [Description("Execute all child nodes once but simultaneously and return Success or Failure depending on the selected ParallelPolicy.")]
    [ParadoxNotion.Design.Icon("Parallel")]
    [Color("ff64cb")]
    public class Parallel : BTComposite
    {

        public enum ParallelPolicy
        {
            FirstFailure,
            FirstSuccess,
            FirstSuccessOrFailure
        }

        [Tooltip("The policy determines when the Parallel node will end and return its Status.")]
        public ParallelPolicy policy = ParallelPolicy.FirstFailure;
        [Name("Repeat"), Tooltip("If true, child nodes are repeated until the Policy set is met, or until all children have had a chance to complete at least once.")]
        public bool dynamic;

        private bool[] finishedConnections;
        private int finishedConnectionsCount;

        public override void OnGraphStarted() {
            finishedConnections = new bool[outConnections.Count];
            finishedConnectionsCount = 0;
        }

        protected override Status OnExecute(Component agent, IBlackboard blackboard) {

            var defferedStatus = Status.Resting;
            for ( var i = 0; i < outConnections.Count; i++ ) {
                var connection = outConnections[i];
                var isConnectionFinished = finishedConnections[i] == true;

                if ( !dynamic && isConnectionFinished ) {
                    continue;
                }

                if ( connection.status != Status.Running && isConnectionFinished ) {
                    connection.Reset();
                }

                status = connection.Execute(agent, blackboard);

                if ( defferedStatus == Status.Resting ) {
                    if ( status == Status.Failure && ( policy == ParallelPolicy.FirstFailure || policy == ParallelPolicy.FirstSuccessOrFailure ) ) {
                        defferedStatus = Status.Failure;
                    }

                    if ( status == Status.Success && ( policy == ParallelPolicy.FirstSuccess || policy == ParallelPolicy.FirstSuccessOrFailure ) ) {
                        defferedStatus = Status.Success;
                    }
                }

                if ( status != Status.Running && !isConnectionFinished ) {
                    finishedConnections[i] = true;
                    finishedConnectionsCount++;
                }
            }

            if ( defferedStatus != Status.Resting ) {
                ResetRunning();
                return defferedStatus;
            }

            if ( finishedConnectionsCount == outConnections.Count ) {
                ResetRunning();
                switch ( policy ) {
                    case ParallelPolicy.FirstFailure:
                        return Status.Success;
                    case ParallelPolicy.FirstSuccess:
                        return Status.Failure;
                }
            }

            return Status.Running;
        }

        protected override void OnReset() {
            for ( var i = 0; i < finishedConnections.Length; i++ ) { finishedConnections[i] = false; }
            finishedConnectionsCount = 0;
        }

        void ResetRunning() {
            for ( var i = 0; i < outConnections.Count; i++ ) {
                if ( outConnections[i].status == Status.Running ) {
                    outConnections[i].Reset();
                }
            }
        }

        ///----------------------------------------------------------------------------------------------
        ///---------------------------------------UNITY EDITOR-------------------------------------------

#if UNITY_EDITOR

        protected override void OnNodeGUI() {
            GUILayout.Label(( dynamic ? "<b>REPEAT</b>\n" : "" ) + policy.ToString().SplitCamelCase());
        }

#endif
    }
}