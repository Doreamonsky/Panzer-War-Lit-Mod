using NodeCanvas.Framework;
using UnityEngine;


namespace NodeCanvas.StateMachines
{

    ///<summary>The connection object for FSM nodes. AKA Transitions</summary>
    public class FSMConnection : Connection, ITaskAssignable<ConditionTask>
    {
        [SerializeField]
        private FSM.TransitionCallMode _transitionCallMode;
        [SerializeField]
        private ConditionTask _condition;

        public ConditionTask condition {
            get { return _condition; }
            set { _condition = value; }
        }

        public Task task {
            get { return condition; }
            set { condition = (ConditionTask)value; }
        }

        public FSM.TransitionCallMode transitionCallMode {
            get { return _transitionCallMode; }
            private set { _transitionCallMode = value; }
        }

        //...
        public void EnableCondition(Component agent, IBlackboard blackboard) {
            if ( condition != null ) {
                condition.Enable(agent, blackboard);
            }
        }

        //...
        public void DisableCondition() {
            if ( condition != null ) {
                condition.Disable();
            }
        }

        ///<summary>Perform the transition disregarding whether or not the condition (if any) is valid</summary>
        public void PerformTransition() {
            ( graph as FSM ).EnterState((FSMState)targetNode, transitionCallMode);
        }


        ///----------------------------------------------------------------------------------------------
        ///---------------------------------------UNITY EDITOR-------------------------------------------
#if UNITY_EDITOR

        public override ParadoxNotion.PlanarDirection direction {
            get { return ParadoxNotion.PlanarDirection.Auto; }
        }

        public override TipConnectionStyle tipConnectionStyle {
            get { return TipConnectionStyle.Arrow; }
        }

        public override bool animate {
            get { return status == Status.Failure; }
        }

        protected override string GetConnectionInfo() {
            var result = transitionCallMode == FSM.TransitionCallMode.Normal ? string.Empty : string.Format("<b>[{0}]</b>\n", transitionCallMode.ToString());
            result += condition != null ? condition.summaryInfo : "OnFinish";
            return result;
        }

        protected override void OnConnectionInspectorGUI() {
            if ( Editor.Prefs.showNodeInfo ) {
                UnityEditor.EditorGUILayout.HelpBox("Stacked Call Mode will push the current state to the stack and pop return to it later when any other state without outgoing transitions has been encountered. If you decide to use this feature make sure that you are not cycle stacking states.", UnityEditor.MessageType.None);
            }
            transitionCallMode = (FSM.TransitionCallMode)UnityEditor.EditorGUILayout.EnumPopup("Call Mode (Experimental)", transitionCallMode);
            ParadoxNotion.Design.EditorUtils.Separator();
            NodeCanvas.Editor.TaskEditor.TaskFieldMulti<ConditionTask>(condition, graph, (c) => { condition = c; });
        }

#endif
    }
}