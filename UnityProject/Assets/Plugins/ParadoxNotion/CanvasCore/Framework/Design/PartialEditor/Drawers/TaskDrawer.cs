#if UNITY_EDITOR

using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

namespace NodeCanvas.Editor
{

    ///<summary>A drawer for Tasks</summary>
    public class TaskDrawer : ObjectDrawer<Task>
    {
        public override Task OnGUI(GUIContent content, Task instance) {
            //we presume the system is the context unity object (99% will be and always is in default NC)
            var system = contextUnityObject as ITaskSystem;
            if ( system == null ) {
                GUILayout.Label("Can't resolve ITaskSystem for task");
                return instance;
            }

            if ( fieldInfo.FieldType == typeof(ActionList) ) {
                if ( instance == null ) { instance = Task.Create<ActionList>(system); }
                ( instance as ActionList ).ShowListGUI();
                ( instance as ActionList ).ShowNestedActionsGUI();
                return instance;
            }

            if ( fieldInfo.FieldType == typeof(ConditionList) ) {
                if ( instance == null ) { instance = Task.Create<ConditionList>(system); }
                ( instance as ConditionList ).ShowListGUI();
                ( instance as ConditionList ).ShowNestedConditionsGUI();
                return instance;
            }

            //we need capture the objects for the delegate callback
            var _field = fieldInfo;
            var _context = context;
            TaskEditor.TaskFieldMulti(instance, system, _field.FieldType, (t) => _field.SetValue(_context, t));
            EditorUtils.Separator();
            return instance;
        }
    }
}

#endif