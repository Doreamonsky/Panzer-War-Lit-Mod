using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;


namespace NodeCanvas.StateMachines
{

    [Name("Super Action State", 99)]
    [Description("The Super Action State provides finer control on when to execute actions. This state is never Finished by it's own if there is any Actions in the OnUpdate list and thus OnFinish transitions will never be called in that case. OnExit Actions are only called for 1 frame when the state exits.")]
    public class SuperActionState : FSMState
    {

        [SerializeField]
        private ActionList _onEnterList;
        [SerializeField]
        private ActionList _onUpdateList;
        [SerializeField]
        private ActionList _onExitList;

        private bool enterListFinished = false;

        public override void OnValidate(Graph assignedGraph) {
            if ( _onEnterList == null ) {
                _onEnterList = (ActionList)Task.Create(typeof(ActionList), assignedGraph);
                _onEnterList.executionMode = ActionList.ActionsExecutionMode.ActionsRunInParallel;
            }
            if ( _onUpdateList == null ) {
                _onUpdateList = (ActionList)Task.Create(typeof(ActionList), assignedGraph);
                _onUpdateList.executionMode = ActionList.ActionsExecutionMode.ActionsRunInParallel;
            }
            if ( _onExitList == null ) {
                _onExitList = (ActionList)Task.Create(typeof(ActionList), assignedGraph);
                _onExitList.executionMode = ActionList.ActionsExecutionMode.ActionsRunInParallel;
            }
        }

        protected override void OnEnter() {
            enterListFinished = false;
            OnUpdate();
        }

        protected override void OnUpdate() {
            if ( !enterListFinished ) {
                var enterListStatus = _onEnterList.Execute(graphAgent, graphBlackboard);
                if ( enterListStatus != Status.Running ) {
                    enterListFinished = true;
                    if ( _onUpdateList.actions.Count == 0 ) {
                        Finish(enterListStatus);
                    }
                }
            }
            _onUpdateList.Execute(graphAgent, graphBlackboard);
        }

        protected override void OnExit() {
            _onEnterList.EndAction(null);
            _onUpdateList.EndAction(null);
            _onExitList.Execute(graphAgent, graphBlackboard);
            _onExitList.EndAction(null);
        }

        protected override void OnPause() {
            _onEnterList.Pause();
            _onUpdateList.Pause();
        }

        ///----------------------------------------------------------------------------------------------
        ///---------------------------------------UNITY EDITOR-------------------------------------------
#if UNITY_EDITOR

        [SerializeField, ParadoxNotion.Serialization.FullSerializer.fsIgnoreInBuild]
        private bool foldEnter;
        [SerializeField, ParadoxNotion.Serialization.FullSerializer.fsIgnoreInBuild]
        private bool foldUpdate;
        [SerializeField, ParadoxNotion.Serialization.FullSerializer.fsIgnoreInBuild]
        private bool foldExit;

        protected override void OnNodeGUI() {

            GUILayout.BeginVertical(Styles.roundedBox);
            GUILayout.Label(string.Format("<i>{0} OnEnter Actions</i>", _onEnterList.actions.Count), Styles.leftLabel);
            GUILayout.EndVertical();

            GUILayout.BeginVertical(Styles.roundedBox);
            GUILayout.Label(string.Format("<i>{0} OnUpdate Actions</i>", _onUpdateList.actions.Count), Styles.leftLabel);
            GUILayout.EndVertical();

            GUILayout.BeginVertical(Styles.roundedBox);
            GUILayout.Label(string.Format("<i>{0} OnExit Actions</i>", _onExitList.actions.Count), Styles.leftLabel);
            GUILayout.EndVertical();
        }

        protected override void OnNodeInspectorGUI() {

            ShowTransitionsInspector();

            if ( _onEnterList == null || _onUpdateList == null || _onExitList == null ) {
                return;
            }

            EditorUtils.CoolLabel("Actions");

            foldEnter = UnityEditor.EditorGUILayout.Foldout(foldEnter, string.Format("OnEnter Actions ({0})", _onEnterList.actions.Count));
            if ( foldEnter ) {
                _onEnterList.ShowListGUI();
                _onEnterList.ShowNestedActionsGUI();
            }
            EditorUtils.Separator();

            foldUpdate = UnityEditor.EditorGUILayout.Foldout(foldUpdate, string.Format("OnUpdate Actions ({0})", _onUpdateList.actions.Count));
            if ( foldUpdate ) {
                _onUpdateList.ShowListGUI();
                _onUpdateList.ShowNestedActionsGUI();
            }
            EditorUtils.Separator();

            foldExit = UnityEditor.EditorGUILayout.Foldout(foldExit, string.Format("OnExit Actions ({0})", _onExitList.actions.Count));
            if ( foldExit ) {
                _onExitList.ShowListGUI();
                _onExitList.ShowNestedActionsGUI();
            }
        }

#endif
    }
}