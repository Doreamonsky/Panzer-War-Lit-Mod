#if UNITY_EDITOR

using System;
using UnityEngine;
using UnityEditor;
using ParadoxNotion;
using ParadoxNotion.Design;
using NodeCanvas.Framework;
using NodeCanvas.Framework.Internal;
using System.Linq;

namespace NodeCanvas.Editor
{

    public class TaskEditor : EditorObjectWrapper<Task>
    {

        private bool isUnfolded = true;
        private EditorPropertyWrapper<TaskAgentParameter> agentParameterProp;
        private EditorMethodWrapper onTaskInspectorGUI;

        private Task task { get { return target; } }

        protected override void OnEnable() {
            agentParameterProp = CreatePropertyWrapper<TaskAgentParameter>("_agentParameter");
            onTaskInspectorGUI = CreateMethodWrapper("OnTaskInspectorGUI");
        }

        ///----------------------------------------------------------------------------------------------

        ///<summary>Show a Task's field without ability to add if null or add multiple tasks to form a list.</summary>
        public static void TaskFieldSingle(Task task, Action<Task> callback, bool showTitlebar = true) {
            if ( task != null ) { ShowTaskInspectorGUI(task, callback, showTitlebar); }
        }

        ///<summary>Show a Task's field. If task null allow add task. Multiple tasks can be added to form a list.</summary>
        public static void TaskFieldMulti<T>(T task, ITaskSystem ownerSystem, Action<T> callback) where T : Task {
            TaskFieldMulti(task, ownerSystem, typeof(T), (Task t) => { callback((T)t); });
        }

        ///<summary>Show a Task's field. If task null allow add task. Multiple tasks can be added to form a list.</summary>
        public static void TaskFieldMulti(Task task, ITaskSystem ownerSystem, Type baseType, Action<Task> callback) {
            //if null simply show an assignment button
            if ( task == null ) {
                ShowCreateTaskSelectionButton(ownerSystem, baseType, callback);
                return;
            }

            //Handle Action/ActionLists so that in GUI level a list is used only when needed
            if ( baseType == typeof(ActionTask) ) {
                if ( !( task is ActionList ) ) {
                    ShowCreateTaskSelectionButton(ownerSystem, baseType, (t) =>
                        {
                            var newList = Task.Create<ActionList>(ownerSystem);
                            UndoUtility.RecordObject(ownerSystem.contextObject, "New Action Task");
                            newList.AddAction((ActionTask)task);
                            newList.AddAction((ActionTask)t);
                            callback(newList);
                        });
                }

                ShowTaskInspectorGUI(task, callback);

                if ( task is ActionList ) {
                    var list = (ActionList)task;
                    if ( list.actions.Count == 1 ) {
                        list.actions[0].isUserEnabled = true;
                        callback(list.actions[0]);
                    }
                }
                return;
            }

            //Handle Condition/ConditionLists so that in GUI level a list is used only when needed
            if ( baseType == typeof(ConditionTask) ) {
                if ( !( task is ConditionList ) ) {
                    ShowCreateTaskSelectionButton(ownerSystem, baseType, (t) =>
                        {
                            var newList = Task.Create<ConditionList>(ownerSystem);
                            UndoUtility.RecordObject(ownerSystem.contextObject, "New Condition Task");
                            newList.AddCondition((ConditionTask)task);
                            newList.AddCondition((ConditionTask)t);
                            callback(newList);
                        });
                }

                ShowTaskInspectorGUI(task, callback);

                if ( task is ConditionList ) {
                    var list = (ConditionList)task;
                    if ( list.conditions.Count == 1 ) {
                        list.conditions[0].isUserEnabled = true;
                        callback(list.conditions[0]);
                    }
                }
                return;
            }

            //in all other cases where the base type is not a base ActionTask or ConditionTask,
            //(thus lists can't be used unless the base type IS a list), simple show the inspector.
            ShowTaskInspectorGUI(task, callback);
        }

        ///<summary>Show the editor inspector of target task</summary>
        static void ShowTaskInspectorGUI(Task task, Action<Task> callback, bool showTitlebar = true) {
            EditorWrapperFactory.GetEditor<TaskEditor>(task).ShowInspector(callback, showTitlebar);
        }

        //Shows a button that when clicked, pops a context menu with a list of tasks deriving the base type specified. When something is selected the callback is called
        public static void ShowCreateTaskSelectionButton<T>(ITaskSystem ownerSystem, Action<T> callback) where T : Task {
            ShowCreateTaskSelectionButton(ownerSystem, typeof(T), (Task t) => { callback((T)t); });
        }

        //Shows a button that when clicked, pops a context menu with a list of tasks deriving the base type specified. When something is selected the callback is called
        //On top of that it also shows a search field for Tasks
        public static void ShowCreateTaskSelectionButton(ITaskSystem ownerSystem, Type baseType, Action<Task> callback) {

            GUI.backgroundColor = Colors.lightBlue;
            var label = "Assign " + baseType.Name.SplitCamelCase();
            if ( GUILayout.Button(label) ) {

                Action<Type> TaskTypeSelected = (t) =>
                {
                    var newTask = Task.Create(t, ownerSystem);
                    UndoUtility.RecordObject(ownerSystem.contextObject, "New Task");
                    callback(newTask);
                };

                var menu = EditorUtils.GetTypeSelectionMenu(baseType, TaskTypeSelected);
                if ( CopyBuffer.TryGetCache<Task>(out Task copy) && baseType.IsAssignableFrom(copy.GetType()) ) {
                    menu.AddSeparator("/");
                    menu.AddItem(new GUIContent(string.Format("Paste ({0})", copy.name)), false, () => { callback(copy.Duplicate(ownerSystem)); });
                }
                menu.ShowAsBrowser(label, typeof(Task));
            }

            GUILayout.Space(2);
            GUI.backgroundColor = Color.white;
        }


        ///----------------------------------------------------------------------------------------------


        //Draw the task inspector GUI
        void ShowInspector(Action<Task> callback, bool showTitlebar = true) {
            if ( task.ownerSystem == null ) {
                GUILayout.Label("<b>Owner System is null! This should really not happen but it did!\nPlease report a bug. Thank you :)</b>");
                return;
            }

            //make sure TaskAgent is not null in case task defines an AgentType
            if ( task.agentIsOverride && agentParameterProp.value == null ) {
                agentParameterProp.value = new TaskAgentParameter();
            }

            if ( task.obsolete != string.Empty ) {
                EditorGUILayout.HelpBox(string.Format("This is an obsolete Task:\n\"{0}\"", task.obsolete), MessageType.Warning);
            }

            if ( !showTitlebar || ShowTitlebar(callback) == true ) {

                if ( Prefs.showNodeInfo && !string.IsNullOrEmpty(task.description) ) {
                    EditorGUILayout.HelpBox(task.description, MessageType.None);
                }

                UndoUtility.CheckUndo(task.ownerSystem.contextObject, "Task Inspector");

                SpecialCaseInspector();
                ShowAgentField();
                onTaskInspectorGUI.Invoke();

                UndoUtility.CheckDirty(task.ownerSystem.contextObject);
            }
        }

        //Some special cases for Action & Condition. A bit weird but better than creating a virtual method in this case
        void SpecialCaseInspector() {

            if ( task is ActionTask ) {
                if ( Application.isPlaying ) {
                    if ( ( task as ActionTask ).elapsedTime > 0 ) GUI.color = Color.yellow;
                    EditorGUILayout.LabelField("Elapsed Time", ( task as ActionTask ).elapsedTime.ToString());
                    GUI.color = Color.white;
                }
            }

            if ( task is ConditionTask ) {
                GUI.color = ( task as ConditionTask ).invert ? Color.white : Color.white.WithAlpha(0.5f);
                ( task as ConditionTask ).invert = EditorGUILayout.ToggleLeft("Invert Condition", ( task as ConditionTask ).invert);
                GUI.color = Color.white;
            }
        }

        //a Custom titlebar for tasks
        bool ShowTitlebar(Action<Task> callback) {

            GUI.backgroundColor = EditorGUIUtility.isProSkin ? Color.black.WithAlpha(0.3f) : Color.white.WithAlpha(0.5f);
            GUILayout.BeginHorizontal(GUI.skin.box);
            {
                GUI.backgroundColor = Color.white;
                GUILayout.Label("<b>" + ( isUnfolded ? "▼ " : "► " ) + task.name + "</b>" + ( isUnfolded ? "" : "\n<i><size=10>(" + task.summaryInfo + ")</size></i>" ), Styles.leftLabel);

                var taskType = task.GetType();
                if ( taskType.IsGenericType ) {
                    var isBase = taskType.GetFirstGenericParameterConstraintType() == taskType.GetSingleGenericArgument();
                    if ( isBase && GUILayout.Button(StyleSheet.genericType, GUI.skin.label, GUILayout.Width(20), GUILayout.Height(20)) ) {
                        var menu = new GenericMenu();
                        menu = EditorUtils.GetPreferedTypesSelectionMenu(taskType.GetGenericTypeDefinition(), (t) => { callback(Task.Create(t, task.ownerSystem)); }, menu, "Set Generic Type");
                        menu.ShowAsContext();
                    }
                }

                if ( GUILayout.Button(Icons.csIcon, GUI.skin.label, GUILayout.Width(20), GUILayout.Height(20)) ) {
                    EditorUtils.OpenScriptOfType(task.GetType());
                }

                GUI.color = EditorGUIUtility.isProSkin ? Color.white : Color.grey;
                if ( GUILayout.Button(Icons.gearPopupIcon, Styles.centerLabel, GUILayout.Width(20), GUILayout.Height(20)) ) {
                    GetMenu(callback).ShowAsContext();
                }
                GUI.color = Color.white;
            }
            GUILayout.EndHorizontal();

            var titleRect = GUILayoutUtility.GetLastRect();
            EditorGUIUtility.AddCursorRect(titleRect, MouseCursor.Link);
            GUI.color = Color.black.WithAlpha(0.25f);
            GUI.DrawTexture(new Rect(titleRect.x, titleRect.yMax - 1, titleRect.width, 1), Texture2D.whiteTexture);
            GUI.color = Color.white;

            var e = Event.current;
            if ( e.type == EventType.ContextClick && titleRect.Contains(e.mousePosition) ) {
                GetMenu(callback).ShowAsContext();
                e.Use();
            }

            if ( e.button == 0 && e.type == EventType.MouseUp && titleRect.Contains(e.mousePosition) ) {
                isUnfolded = !isUnfolded;
                e.Use();
            }

            return isUnfolded;
        }

        ///<summary>Generate and return task menu</summary>
        GenericMenu GetMenu(Action<Task> callback) {
            var menu = new GenericMenu();
            var taskType = task.GetType();
            menu.AddItem(new GUIContent("Open Script"), false, () => { EditorUtils.OpenScriptOfType(taskType); });
            menu.AddItem(new GUIContent("Copy"), false, () => { CopyBuffer.SetCache<Task>(task); });

            foreach ( var _m in taskType.RTGetMethods() ) {
                var m = _m;
                var att = m.RTGetAttribute<ContextMenu>(true);
                if ( att != null ) {
                    menu.AddItem(new GUIContent(att.menuItem), false, () => { m.Invoke(task, null); });
                }
            }

            if ( taskType.IsGenericType ) {
                menu = EditorUtils.GetPreferedTypesSelectionMenu(taskType.GetGenericTypeDefinition(), (t) => { callback(Task.Create(t, task.ownerSystem)); }, menu, "Change Generic Type");
            }

            menu.AddSeparator("/");

            menu.AddItem(new GUIContent("Delete"), false, () =>
            {
                if ( callback != null ) {
                    UndoUtility.RecordObject(task.ownerSystem.contextObject, "Delete Task");
                    callback(null);
                }
            });

            return menu;
        }

        //Shows the agent field in case an agent type is specified through the use of the generic versions of Action or Condition Task
        void ShowAgentField() {

            if ( task.agentType == null ) {
                return;
            }

            TaskAgentParameter agentParam = agentParameterProp.value;

            if ( Application.isPlaying && task.agentIsOverride && agentParam.value == null ) {
                GUILayout.Label("<b>Missing Agent Reference</b>".FormatError());
                return;
            }

            GUI.color = Color.white.WithAlpha(task.agentIsOverride ? 0.65f : 0.5f);
            GUILayout.BeginVertical(GUI.skin.box);
            GUILayout.BeginHorizontal();

            if ( task.agentIsOverride ) {

                BBParameterEditor.ParameterField(null, agentParam, task.ownerSystem.contextObject);

            } else {

                var compInfo = task.agent == null ? task.agentType.FriendlyName().FormatError() : task.agentType.FriendlyName();
                var icon = TypePrefs.GetTypeIcon(task.agentType);
                var label = string.Format("Use Self ({0})", compInfo);
                var content = EditorUtils.GetTempContent(label, icon);
                GUILayout.Label(content, GUILayout.Height(18), GUILayout.Width(0), GUILayout.ExpandWidth(true));
            }

            GUI.color = Color.white;

            if ( !Application.isPlaying ) {
                var newOverride = EditorGUILayout.Toggle(task.agentIsOverride, GUILayout.Width(18));
                if ( newOverride != task.agentIsOverride ) {
                    UndoUtility.RecordObject(task.ownerSystem.contextObject, "Override Agent");
                    task.agentIsOverride = newOverride;
                }
            }

            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
        }

    }
}

#endif