using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;
using Logger = ParadoxNotion.Services.Logger;

namespace NodeCanvas.StateMachines
{

    ///<summary> Base class for fsm nodes that are actually states</summary>
    abstract public class FSMState : FSMNode, IState
    {

        public enum TransitionEvaluationMode
        {
            CheckContinuously,
            CheckAfterStateFinished,
            CheckManually
        }

        [SerializeField]
        private TransitionEvaluationMode _transitionEvaluation;

        private bool _hasInit;

        public override bool allowAsPrime => true;
        public override bool canSelfConnect => true;
        public override int maxInConnections => -1;
        public override int maxOutConnections => -1;

        public TransitionEvaluationMode transitionEvaluation {
            get { return _transitionEvaluation; }
            set { _transitionEvaluation = value; }
        }

        ///<summary>Returns all transitions of the state</summary>
        public FSMConnection[] GetTransitions() {
            var result = new FSMConnection[outConnections.Count];
            for ( var i = 0; i < outConnections.Count; i++ ) {
                result[i] = (FSMConnection)outConnections[i];
            }
            return result;
        }

        ///<summary>Declares that the state has finished</summary>
        public void Finish() { Finish(Status.Success); }
        public void Finish(bool inSuccess) { Finish(inSuccess ? Status.Success : Status.Failure); }
        public void Finish(Status status) { this.status = status; }

        ///----------------------------------------------------------------------------------------------

        public override void OnGraphPaused() { if ( status == Status.Running ) { OnPause(); } }

        ///----------------------------------------------------------------------------------------------

        //avoid connecting from same source
        protected override bool CanConnectFromSource(Node sourceNode) {
            if ( this.IsChildOf(sourceNode) ) {
                Logger.LogWarning("States are already connected together. Consider using multiple conditions on an existing transition instead", LogTag.EDITOR, this);
                return false;
            }
            return true;
        }

        //avoid connecting to same target
        protected override bool CanConnectToTarget(Node targetNode) {
            if ( this.IsParentOf(targetNode) ) {
                Logger.LogWarning("States are already connected together. Consider using multiple conditions on an existing transition instead", LogTag.EDITOR, this);
                return false;
            }
            return true;
        }

        //OnEnter...
        sealed protected override Status OnExecute(Component agent, IBlackboard bb) {

            if ( !_hasInit ) {
                _hasInit = true;
                OnInit();
            }

            if ( status == Status.Resting ) {
                status = Status.Running;

                for ( int i = 0; i < outConnections.Count; i++ ) {
                    ( (FSMConnection)outConnections[i] ).EnableCondition(agent, bb);
                }

                OnEnter();
            }

            return status;
        }

        //OnUpdate...
        public void Update() {
            var case1 = transitionEvaluation == TransitionEvaluationMode.CheckContinuously;
            var case2 = transitionEvaluation == TransitionEvaluationMode.CheckAfterStateFinished && status != Status.Running;
            if ( case1 || case2 ) {
                CheckTransitions();
            }

            if ( status == Status.Running ) {
                OnUpdate();
            }
        }

        ///<summary>Returns true if a transitions was valid and thus made</summary>
        public bool CheckTransitions() {

            for ( var i = 0; i < outConnections.Count; i++ ) {

                var connection = (FSMConnection)outConnections[i];
                var condition = connection.condition;

                if ( !connection.isActive ) {
                    continue;
                }

                if ( ( condition != null && condition.Check(graphAgent, graphBlackboard) ) || ( condition == null && status != Status.Running ) ) {
                    FSM.EnterState((FSMState)connection.targetNode, connection.transitionCallMode);
                    connection.status = Status.Success; //editor vis
                    return true;
                }

                connection.status = Status.Failure; //editor vis
            }

            return false;
        }

        //OnExit...
        sealed protected override void OnReset() {
            for ( int i = 0; i < outConnections.Count; i++ ) {
                ( (FSMConnection)outConnections[i] ).DisableCondition();
            }

#if UNITY_EDITOR
            //Done for visualizing in editor
            for ( var i = 0; i < inConnections.Count; i++ ) {
                inConnections[i].status = Status.Resting;
            }
#endif

            OnExit();
        }


        //Converted
        virtual protected void OnInit() { }
        virtual protected void OnEnter() { }
        virtual protected void OnUpdate() { }
        virtual protected void OnExit() { }
        virtual protected void OnPause() { }
        //


        ///----------------------------------------------------------------------------------------------
        ///---------------------------------------UNITY EDITOR-------------------------------------------
#if UNITY_EDITOR

        //...
        protected override void OnNodeInspectorGUI() {
            ShowTransitionsInspector();
            DrawDefaultInspector();
        }

        protected override void OnNodeExternalGUI() {
            var peek = FSM.PeekStack();
            if ( peek != null && FSM.currentState == this ) {
                UnityEditor.Handles.color = Color.grey;
                UnityEditor.Handles.DrawAAPolyLine(rect.center, peek.rect.center);
                UnityEditor.Handles.color = Color.white;
            }
        }

        //...
        protected override UnityEditor.GenericMenu OnContextMenu(UnityEditor.GenericMenu menu) {
            if ( Application.isPlaying ) {
                menu.AddItem(new GUIContent("Enter State"), false, () => { FSM.EnterState(this, FSM.TransitionCallMode.Normal); });
            } else { menu.AddDisabledItem(new GUIContent("Enter State")); }
            menu.AddItem(new GUIContent("Breakpoint"), isBreakpoint, () => { isBreakpoint = !isBreakpoint; });
            return menu;
        }

        //...
        protected void ShowTransitionsInspector() {

            EditorUtils.CoolLabel("Transitions");

            if ( outConnections.Count == 0 ) {
                UnityEditor.EditorGUILayout.HelpBox("No Transition", UnityEditor.MessageType.None);
            }

            var onFinishExists = false;
            EditorUtils.ReorderableList(outConnections, (i, picked) =>
            {
                var connection = (FSMConnection)outConnections[i];
                GUILayout.BeginHorizontal("box");
                if ( connection.condition != null ) {
                    GUILayout.Label(connection.condition.summaryInfo, GUILayout.MinWidth(0), GUILayout.ExpandWidth(true));
                } else {
                    GUILayout.Label("OnFinish" + ( onFinishExists ? " (exists)" : string.Empty ), GUILayout.MinWidth(0), GUILayout.ExpandWidth(true));
                    onFinishExists = true;
                }

                GUILayout.FlexibleSpace();
                GUILayout.Label("► '" + connection.targetNode.name + "'");
                GUILayout.EndHorizontal();
            });

            transitionEvaluation = (TransitionEvaluationMode)UnityEditor.EditorGUILayout.EnumPopup(transitionEvaluation);
            EditorUtils.BoldSeparator();
        }

#endif

        ///----------------------------------------------------------------------------------------------



    }
}