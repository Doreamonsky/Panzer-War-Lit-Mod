using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

namespace NodeCanvas.StateMachines
{

    [Name("Any State")]
    [Description("The Transitions of this node will constantly be checked. If any becomes true, the target connected State will Enter regardless of the current State. This node can have no incomming transitions.")]
    [Color("b3ff7f")]
    public class AnyState : FSMNode, IUpdatable
    {

        public bool dontRetriggerStates = false;

        public override string name { //yei for caps
            get { return "FROM ANY STATE"; }
        }

        public override int maxInConnections { get { return 0; } }
        public override int maxOutConnections { get { return -1; } }
        public override bool allowAsPrime { get { return false; } }

        public override void OnGraphStarted() {
            for ( var i = 0; i < outConnections.Count; i++ ) {
                ( outConnections[i] as FSMConnection ).EnableCondition(graphAgent, graphBlackboard);
            }
        }

        public override void OnGraphStoped() {
            for ( var i = 0; i < outConnections.Count; i++ ) {
                ( outConnections[i] as FSMConnection ).DisableCondition();
            }
        }

        void IUpdatable.Update() {

            if ( outConnections.Count == 0 ) {
                return;
            }

            status = Status.Running;

            for ( var i = 0; i < outConnections.Count; i++ ) {

                var connection = (FSMConnection)outConnections[i];
                var condition = connection.condition;

                if ( !connection.isActive || condition == null ) {
                    continue;
                }

                if ( dontRetriggerStates ) {
                    if ( FSM.currentState == (FSMState)connection.targetNode && FSM.currentState.status == Status.Running ) {
                        continue;
                    }
                }

                if ( condition.Check(graphAgent, graphBlackboard) ) {
                    FSM.EnterState((FSMState)connection.targetNode, connection.transitionCallMode);
                    connection.status = Status.Success; //editor vis
                    return;
                }

                connection.status = Status.Failure; //editor vis
            }
        }

        ///----------------------------------------------------------------------------------------------
        ///---------------------------------------UNITY EDITOR-------------------------------------------
#if UNITY_EDITOR

        protected override void OnNodeGUI() {
            base.OnNodeGUI();
            if ( dontRetriggerStates ) {
                UnityEngine.GUILayout.Label("<b>[NO RETRIGGER]</b>");
            }
        }

        public override string GetConnectionInfo(int index) {
            if ( ( outConnections[index] as FSMConnection ).condition == null ) {
                return "* Never Triggered *";
            }
            return null;
        }

        protected override void OnNodeInspectorGUI() {
            EditorUtils.CoolLabel("Transitions");
            if ( outConnections.Count == 0 ) {
                UnityEditor.EditorGUILayout.HelpBox("No Transition", UnityEditor.MessageType.None);
            }

            var anyNullCondition = false;
            EditorUtils.ReorderableList(outConnections, (i, picked) =>
            {
                var connection = (FSMConnection)outConnections[i];
                GUILayout.BeginHorizontal("box");
                if ( connection.condition != null ) {
                    GUILayout.Label(connection.condition.summaryInfo, GUILayout.MinWidth(0), GUILayout.ExpandWidth(true));
                } else {
                    GUILayout.Label("OnFinish (This is never triggered)", GUILayout.MinWidth(0), GUILayout.ExpandWidth(true));
                    anyNullCondition = true;
                }

                GUILayout.FlexibleSpace();
                GUILayout.Label("► '" + connection.targetNode.name + "'");
                GUILayout.EndHorizontal();
            });

            EditorUtils.BoldSeparator();

            if ( anyNullCondition ) {
                UnityEditor.EditorGUILayout.HelpBox("This is not a state and as such it never finish, thus OnFinish transitions are never called.\nPlease add a condition in all transitions of this node.", UnityEditor.MessageType.Warning);
            }

            dontRetriggerStates = UnityEditor.EditorGUILayout.ToggleLeft("Don't Retrigger Running States", dontRetriggerStates);
        }
#endif

    }
}