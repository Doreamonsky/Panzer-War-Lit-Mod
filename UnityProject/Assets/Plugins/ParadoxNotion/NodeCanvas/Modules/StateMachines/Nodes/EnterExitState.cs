using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;


namespace NodeCanvas.StateMachines
{

    [Name("Enter | Exit")]
    [Description("Execute a number of Actions when the FSM enters/starts and when it stops/exits. Note that these actions can not be latent (multiple frames).")]
    [Color("ff64cb")]
    public class EnterExitState : FSMNode
    {

        [SerializeField] private ActionList _actionListEnter;
        [SerializeField] private ActionList _actionListExit;

        public ActionList actionListEnter {
            get { return _actionListEnter; }
            set { _actionListEnter = value; }
        }

        public ActionList actionListExit {
            get { return _actionListExit; }
            set { _actionListExit = value; }
        }

        public override string name => base.name.ToUpper();
        public override int maxInConnections => 0;
        public override int maxOutConnections => 0;
        public override bool allowAsPrime => false;

        ///----------------------------------------------------------------------------------------------

        public override void OnValidate(Graph assignedGraph) {
            if ( actionListEnter == null ) {
                actionListEnter = (ActionList)Task.Create(typeof(ActionList), assignedGraph);
                actionListEnter.executionMode = ActionList.ActionsExecutionMode.ActionsRunInParallel;
            }
            if ( actionListExit == null ) {
                actionListExit = (ActionList)Task.Create(typeof(ActionList), assignedGraph);
                actionListExit.executionMode = ActionList.ActionsExecutionMode.ActionsRunInParallel;
            }
        }

        public override void OnGraphStarted() {
            actionListEnter.Execute(graphAgent, graphBlackboard);
            actionListEnter.EndAction(null);
        }

        public override void OnGraphStoped() {
            actionListExit.Execute(graphAgent, graphBlackboard);
            actionListExit.EndAction(null);
        }

        ///----------------------------------------------------------------------------------------------
        ///---------------------------------------UNITY EDITOR-------------------------------------------
#if UNITY_EDITOR

        protected override void OnNodeGUI() {
            GUILayout.BeginVertical(Styles.roundedBox);
            GUILayout.Label(actionListEnter.summaryInfo);
            GUILayout.EndVertical();
            GUILayout.BeginVertical(Styles.roundedBox);
            GUILayout.Label(actionListExit.summaryInfo);
            GUILayout.EndVertical();
            base.OnNodeGUI();
        }

        protected override void OnNodeInspectorGUI() {
            EditorUtils.CoolLabel("Enter Actions");
            actionListEnter.ShowListGUI();
            actionListEnter.ShowNestedActionsGUI();
            EditorUtils.Separator();
            EditorUtils.CoolLabel("Exit Actions");
            actionListExit.ShowListGUI();
            actionListExit.ShowNestedActionsGUI();
        }

#endif
    }
}