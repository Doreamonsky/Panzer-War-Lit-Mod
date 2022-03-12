#if UNITY_EDITOR
using UnityEditor;
using NodeCanvas.Editor;
#endif

using System.Collections.Generic;
using ParadoxNotion;
using ParadoxNotion.Design;
using UnityEngine;
using ParadoxNotion.Serialization;

namespace NodeCanvas.Framework
{

    ///<summary>ActionList is an ActionTask itself that holds multiple ActionTasks which can be executed either in parallel or in sequence.</summary>
    [DoNotList]
    public class ActionList : ActionTask
    {

        public enum ActionsExecutionMode
        {
            ActionsRunInSequence,
            ActionsRunInParallel
        }

        public ActionsExecutionMode executionMode;
        public List<ActionTask> actions = new List<ActionTask>();

        private int currentActionIndex;
        private bool[] finishedIndeces;

        protected override string info {
            get
            {
                if ( actions.Count == 0 ) {
                    return "No Actions";
                }

                var finalText = actions.Count > 1 ? ( string.Format("<b>({0})</b>\n", executionMode == ActionsExecutionMode.ActionsRunInSequence ? "In Sequence" : "In Parallel") ) : string.Empty;
                for ( var i = 0; i < actions.Count; i++ ) {

                    var action = actions[i];
                    if ( action == null ) {
                        continue;
                    }

                    if ( action.isUserEnabled ) {
                        var prefix = action.isPaused ? "<b>||</b> " : action.isRunning ? "► " : "▪";
                        finalText += prefix + action.summaryInfo + ( i == actions.Count - 1 ? "" : "\n" );
                    }
                }

                return finalText;
            }
        }

        ///<summary>ActionList overrides to duplicate listed actions correctly</summary>
        public override Task Duplicate(ITaskSystem newOwnerSystem) {
            var newList = (ActionList)base.Duplicate(newOwnerSystem);
            newList.actions.Clear();
            foreach ( var action in actions ) {
                newList.AddAction((ActionTask)action.Duplicate(newOwnerSystem));
            }
            return newList;
        }

        protected override string OnInit() {
            finishedIndeces = new bool[actions.Count];
            return null;
        }

        protected override void OnExecute() {
            currentActionIndex = 0;
            for ( var i = 0; i < actions.Count; i++ ) {
                finishedIndeces[i] = false;
            }
        }

        protected override void OnUpdate() {

            if ( actions.Count == 0 ) {
                EndAction();
                return;
            }

            switch ( executionMode ) {

                //parallel
                case ( ActionsExecutionMode.ActionsRunInParallel ): {
                        for ( var i = 0; i < actions.Count; i++ ) {

                            if ( finishedIndeces[i] ) {
                                continue;
                            }

                            if ( !actions[i].isUserEnabled ) {
                                finishedIndeces[i] = true;
                                continue;
                            }

                            var status = actions[i].Execute(agent, blackboard);
                            if ( status == Status.Failure ) {
                                EndAction(false);
                                return;
                            }

                            if ( status == Status.Success ) {
                                finishedIndeces[i] = true;
                            }
                        }

                        var finished = true;
                        for ( var i = 0; i < actions.Count; i++ ) {
                            finished &= finishedIndeces[i];
                        }

                        if ( finished ) {
                            EndAction(true);
                        }
                    }
                    break;

                //sequence
                case ( ActionsExecutionMode.ActionsRunInSequence ): {
                        for ( var i = currentActionIndex; i < actions.Count; i++ ) {

                            if ( !actions[i].isUserEnabled ) {
                                continue;
                            }

                            var status = actions[i].Execute(agent, blackboard);
                            if ( status == Status.Failure ) {
                                EndAction(false);
                                return;
                            }

                            if ( status == Status.Running ) {
                                currentActionIndex = i;
                                return;
                            }
                        }

                        EndAction(true);
                    }
                    break;
            }
        }

        protected override void OnStop() {
            for ( var i = 0; i < actions.Count; i++ ) {
                if ( actions[i].isUserEnabled ) {
                    actions[i].EndAction(null);
                }
            }
        }

        protected override void OnPause() {
            for ( var i = 0; i < actions.Count; i++ ) {
                if ( actions[i].isUserEnabled ) {
                    actions[i].Pause();
                }
            }
        }

        public override void OnDrawGizmosSelected() {
            for ( var i = 0; i < actions.Count; i++ ) {
                if ( actions[i].isUserEnabled ) {
                    actions[i].OnDrawGizmosSelected();
                }
            }
        }

        public void AddAction(ActionTask action) {

            if ( action is ActionList ) {
                foreach ( var subAction in ( action as ActionList ).actions ) {
                    AddAction(subAction);
                }
                return;
            }

#if UNITY_EDITOR
            UndoUtility.RecordObject(ownerSystem.contextObject, "List Add Task");
            currentViewAction = action;
#endif

            actions.Add(action);
            action.SetOwnerSystem(this.ownerSystem);
        }

        internal override string GetWarningOrError() {
            for ( var i = 0; i < actions.Count; i++ ) {
                var result = actions[i].GetWarningOrError();
                if ( result != null ) { return result; }
            }
            return null;
        }

        ///----------------------------------------------------------------------------------------------
        ///---------------------------------------UNITY EDITOR-------------------------------------------
#if UNITY_EDITOR

        private ActionTask currentViewAction;

        //...
        protected override void OnTaskInspectorGUI() {
            ShowListGUI();
            ShowNestedActionsGUI();
        }

        ///<summary>Show the sub-tasks list</summary>
        public void ShowListGUI() {

            if ( ownerSystem == null ) {
                GUILayout.Label("Owner System is null!");
                return;
            }

            TaskEditor.ShowCreateTaskSelectionButton<ActionTask>(ownerSystem, AddAction);

            ValidateList();

            if ( actions.Count == 0 ) {
                EditorGUILayout.HelpBox("No Actions", MessageType.None);
                return;
            }

            if ( actions.Count == 1 ) { return; }

            //show the actions
            EditorUtils.ReorderableList(actions, (i, picked) =>
            {
                var action = actions[i];
                GUI.color = Color.white.WithAlpha(action == currentViewAction ? 0.75f : 0.25f);
                EditorGUILayout.BeginHorizontal("box");

                GUI.color = Color.white.WithAlpha(action.isUserEnabled ? 0.8f : 0.25f);
                GUI.enabled = !Application.isPlaying;
                action.isUserEnabled = EditorGUILayout.Toggle(action.isUserEnabled, GUILayout.Width(18));
                GUI.enabled = true;

                GUILayout.Label(( action.isPaused ? "<b>||</b> " : action.isRunning ? "► " : "" ) + action.summaryInfo, GUILayout.MinWidth(0), GUILayout.ExpandWidth(true));

                if ( !Application.isPlaying && GUILayout.Button("X", GUILayout.Width(20)) ) {
                    UndoUtility.RecordObject(ownerSystem.contextObject, "List Remove Task");
                    actions.RemoveAt(i);
                    if ( actions.Count == 1 ) { actions[0].isUserEnabled = true; }
                }

                EditorGUILayout.EndHorizontal();

                var lastRect = GUILayoutUtility.GetLastRect();
                EditorGUIUtility.AddCursorRect(lastRect, MouseCursor.Link);
                if ( Event.current.type == EventType.MouseDown && lastRect.Contains(Event.current.mousePosition) ) {
                    currentViewAction = action == currentViewAction ? null : action;
                    Event.current.Use();
                }

                GUI.color = Color.white;
            });

            executionMode = (ActionsExecutionMode)EditorGUILayout.EnumPopup(executionMode);
        }

        ///<summary>Show currently selected task inspector</summary>
        public void ShowNestedActionsGUI() {

            if ( actions.Count == 1 ) {
                currentViewAction = actions[0];
            }

            if ( currentViewAction != null ) {
                EditorUtils.Separator();
                TaskEditor.TaskFieldSingle(currentViewAction, (a) =>
                {
                    if ( a == null ) {
                        var i = actions.IndexOf(currentViewAction);
                        actions.RemoveAt(i);
                    }
                    currentViewAction = (ActionTask)a;
                });
            }
        }

        //Validate possible null tasks
        void ValidateList() {
            for ( var i = actions.Count; i-- > 0; ) {
                if ( actions[i] == null ) {
                    actions.RemoveAt(i);
                }
            }
        }

        [ContextMenu("Save List Preset")]
        void DoSavePreset() {
            var path = EditorUtility.SaveFilePanelInProject("Save Preset", "", "actionList", "");
            if ( !string.IsNullOrEmpty(path) ) {
                System.IO.File.WriteAllText(path, JSONSerializer.Serialize(typeof(ActionList), this, null, true)); //true for pretyJson
                AssetDatabase.Refresh();
            }
        }

        [ContextMenu("Load List Preset")]
        void DoLoadPreset() {
            var path = EditorUtility.OpenFilePanel("Load Preset", "Assets", "actionList");
            if ( !string.IsNullOrEmpty(path) ) {
                var json = System.IO.File.ReadAllText(path);
                var list = JSONSerializer.TryDeserializeOverwrite<ActionList>(this, json);
                this.actions = list.actions;
                this.executionMode = list.executionMode;
                this.currentViewAction = null;
                foreach ( var a in actions ) {
                    a.SetOwnerSystem(this.ownerSystem);
                }
            }
        }

#endif
    }
}