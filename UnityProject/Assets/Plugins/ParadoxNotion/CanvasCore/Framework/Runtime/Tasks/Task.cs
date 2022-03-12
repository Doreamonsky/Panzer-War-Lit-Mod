#define CONVENIENCE_OVER_PERFORMANCE

using System;
using System.Collections;
using ParadoxNotion;
using ParadoxNotion.Design;
using ParadoxNotion.Serialization;
using ParadoxNotion.Serialization.FullSerializer;
using ParadoxNotion.Services;
using NodeCanvas.Framework.Internal;
using UnityEngine;
using Logger = ParadoxNotion.Services.Logger;

namespace NodeCanvas.Framework
{

    //*RECOVERY PROCESSOR IS INSTEAD APPLIED RESPECTIVELY IN ACTIONTASK - CONDITIONTASK*//

    ///<summary>The base class for all Actions and Conditions. You dont actually use or derive this class. Instead derive from ActionTask and ConditionTask</summary>
    [Serializable, fsDeserializeOverwrite, SpoofAOT]
    abstract public partial class Task : ISerializationCollectable, ISerializationCallbackReceiver
    {

        ///----------------------------------------------------------------------------------------------

        //We set the hint type that the agent parameter (if any) is.
        void ISerializationCallbackReceiver.OnBeforeSerialize() {
            if ( agentType == null ) { _agentParameter = null; }
            if ( _agentParameter != null ) { _agentParameter.SetType(agentType); }
        }
        void ISerializationCallbackReceiver.OnAfterDeserialize() { }

        ///----------------------------------------------------------------------------------------------

        ///<summary>If the field type this attribute is used derives Component, then it will be retrieved from the agent. The field is also considered Required for correct initialization.</summary>
        [AttributeUsage(AttributeTargets.Field)]
        protected class GetFromAgentAttribute : Attribute { }

        ///----------------------------------------------------------------------------------------------

        [fsSerializeAs("_isDisabled")]
        private bool _isUserDisabled;
        [fsSerializeAs("overrideAgent")]
        internal protected TaskAgentParameter _agentParameter;

        //
        private ITaskSystem _ownerSystem;
        private Component _currentAgent;
        private string _taskName;
        private string _taskDescription;
        private string _obsoleteInfo;
        private bool _isRuntimeActive;
        private bool _isInitSuccess;
        private EventRouter _eventRouter;
        //

        ///----------------------------------------------------------------------------------------------

        //required
        public Task() { }

        ///<summary>Create a new Task of type assigned to the target ITaskSystem</summary>
        public static T Create<T>(ITaskSystem newOwnerSystem) where T : Task { return (T)Create(typeof(T), newOwnerSystem); }
        public static Task Create(Type type, ITaskSystem newOwnerSystem) {
            if ( type.IsGenericTypeDefinition ) { type = type.MakeGenericType(type.GetFirstGenericParameterConstraintType()); }
            var newTask = (Task)Activator.CreateInstance(type);
            UndoUtility.RecordObject(newOwnerSystem.contextObject, "New Task");
            BBParameter.SetBBFields(newTask, newOwnerSystem.blackboard);
            newTask.Validate(newOwnerSystem);
            newTask.OnCreate(newOwnerSystem);
            return newTask;
        }

        ///<summary>Duplicate the task for the target ITaskSystem</summary>
        virtual public Task Duplicate(ITaskSystem newOwnerSystem) {
            var newTask = JSONSerializer.Clone<Task>(this);
            UndoUtility.RecordObject(newOwnerSystem.contextObject, "Duplicate Task");
            BBParameter.SetBBFields(newTask, newOwnerSystem.blackboard);
            newTask.Validate(newOwnerSystem);
            return newTask;
        }

        ///<summary>Validate the task in respects to the target ITaskSystem</summary>
        public void Validate(ITaskSystem ownerSystem) {
            SetOwnerSystem(ownerSystem);
            OnValidate(ownerSystem);
            var hardError = GetHardError();
            if ( hardError != null ) {
                Logger.LogError(hardError, LogTag.VALIDATION, this);
            }
        }

        ///<summary>Sets the system in which this task lives in</summary>
        public void SetOwnerSystem(ITaskSystem newOwnerSystem) {
            Debug.Assert(newOwnerSystem != null, "Null ITaskSystem set");
            ownerSystem = newOwnerSystem;
        }

        ///----------------------------------------------------------------------------------------------

        ///<summary>The system this task belongs to from which defaults are taken from.</summary>
        public ITaskSystem ownerSystem {
            get { return _ownerSystem; }
            private set { _ownerSystem = value; }
        }

        ///<summary>The owner system's assigned agent</summary>
        public Component ownerSystemAgent => ownerSystem != null ? ownerSystem.agent : null;

        ///<summary>The owner system's assigned blackboard</summary>
        public IBlackboard ownerSystemBlackboard => ownerSystem != null ? ownerSystem.blackboard : null;

        ///<summary>The time in seconds that the owner system is running</summary>
        public float ownerSystemElapsedTime => ownerSystem != null ? ownerSystem.elapsedTime : 0;
        //

        ///<summary>Is the Task user enabled?</summary>
        public bool isUserEnabled {
            get { return !_isUserDisabled; }
            internal set { _isUserDisabled = !value; }
        }

        ///<summary>Is the task obsolete? (marked by [Obsolete]). string.Empty: is not.</summary>
        public string obsolete {
            get
            {
                if ( _obsoleteInfo == null ) {
                    var att = this.GetType().RTGetAttribute<ObsoleteAttribute>(true);
                    _obsoleteInfo = att != null ? att.Message : string.Empty;
                }
                return _obsoleteInfo;
            }
        }

        ///<summary>The friendly task name. This can be overriden with the [Name] attribute</summary>
        public string name {
            get
            {
                if ( _taskName == null ) {
                    var nameAtt = this.GetType().RTGetAttribute<NameAttribute>(false);
                    _taskName = nameAtt != null ? nameAtt.name : GetType().FriendlyName().SplitCamelCase();
                }
                return _taskName;
            }
        }

        ///<summary>The help description of the task if it has any through [Description] attribute</summary>
        public string description {
            get
            {
                if ( _taskDescription == null ) {
                    var descAtt = this.GetType().RTGetAttribute<DescriptionAttribute>(true);
                    _taskDescription = descAtt != null ? descAtt.description : string.Empty;
                }
                return _taskDescription;
            }
        }

        ///<summary>A short summary of what the task will finaly do.</summary>
        public string summaryInfo {
            get
            {
#if UNITY_EDITOR
                if ( !NodeCanvas.Editor.Prefs.showTaskSummary && !( this is ActionList ) && !( this is ConditionList ) ) {
                    return string.Format("<b>{0}</b>", name);
                }
#endif

                if ( this is ActionTask ) { return ( agentIsOverride ? "* " : "" ) + info; }
                if ( this is ConditionTask ) { return ( agentIsOverride ? "* " : "" ) + ( ( this as ConditionTask ).invert ? "If <b>!</b> " : "If " ) + info; }
                return info;
            }
        }

        ///<summary>Override this and return the information of the task summary</summary>
        virtual protected string info => name;

        ///<summary>The type that the agent will be set to by getting component from itself on task initialize. Also defined by using the generic versions of Action and Condition Tasks. You can omit this to keep the agent propagated as is or if there is no need for a specific type anyway.</summary>
        virtual public Type agentType => null;

        ///<summary>Helper summary info to display final agent string within task info if needed</summary>
        public string agentInfo => _agentParameter != null ? _agentParameter.ToString() : "<b>Self</b>";

        ///<summary>The name of the blackboard variable selected if the agent is overriden and set to a blackboard variable or direct assignment.</summary>
        public string agentParameterName => _agentParameter != null ? _agentParameter.name : null;

        ///<summary>Is the agent overriden or the default taken from owner system will be used?</summary>
        public bool agentIsOverride {
            get { return _agentParameter != null; }
            set
            {
                if ( value == false && _agentParameter != null ) {
                    _agentParameter = null;
                }

                if ( value == true && _agentParameter == null ) {
                    _agentParameter = new TaskAgentParameter();
                    _agentParameter.bb = blackboard;
                }
            }
        }

        ///<summary>The current or last executive agent of this task</summary>
        public Component agent {
            get
            {
                if ( _currentAgent != null ) { return _currentAgent; }
                var input = agentIsOverride ? (Component)_agentParameter.value : ownerSystemAgent;
                return input.TransformToType(agentType);
            }
        }

        ///<summary>The current or last blackboard used by this task</summary>
        public IBlackboard blackboard => ownerSystemBlackboard;

        ///<summary>The cached EventRouter of the current agent used to subscribe/unsubscribe events. Use this for custom named events as well -> '.router.onCustomEvent'</summary>
        public EventRouter router => _eventRouter != null ? _eventRouter : _eventRouter = agent?.gameObject.GetAddComponent<EventRouter>();

        ///----------------------------------------------------------------------------------------------

        ///<summary>Actions and Conditions call this before execution. Returns if the task was sucessfully initialized as well</summary>
        protected bool Set(Component newAgent, IBlackboard newBB) {

            Debug.Assert(ReferenceEquals(newBB, ownerSystemBlackboard), "Set Blackboard != Owner Blackboard");

            if ( agentIsOverride ) {
                newAgent = (Component)_agentParameter.value;
            }

            if ( _currentAgent != null && newAgent != null && _currentAgent.gameObject == newAgent.gameObject ) {
                return _isInitSuccess;
            }

            return _isInitSuccess = Initialize(newAgent);
        }

        //Initialize whenever agent is set to a new value
        bool Initialize(Component newAgent) {

            //purge cached reference whenever we init new agent
            _eventRouter = null;

            //"Transform" the agent to the agentType and set as current
            _currentAgent = newAgent.TransformToType(agentType);

            //error if it's null but an agentType is required
            if ( _currentAgent == null && agentType != null ) {
                return Error("Failed to resolve Agent to requested type '" + agentType + "', or new Agent is NULL. Does the Agent has the requested Component?");
            }

            //Use the field attributes
            if ( InitializeFieldAttributes(_currentAgent) == false ) {
                return false;
            }

            //let user make further adjustments and inform us if there was an error
            var error = OnInit();
            if ( error != null ) {
                return Error(error);
            }

            return true;
        }

        //...
        bool InitializeFieldAttributes(Component newAgent) {

#if CONVENIENCE_OVER_PERFORMANCE

            //Usage of [RequiredField] and [GetFromAgent] attributes
            var fields = this.GetType().RTGetFields();
            for ( var i = 0; i < fields.Length; i++ ) {
                var field = fields[i];

#if UNITY_EDITOR
                if ( field.RTIsDefined<RequiredFieldAttribute>(true) ) {
                    var value = field.GetValue(this);

                    if ( value == null || value.Equals(null) ) {
                        return Error(string.Format("A required field named '{0}' is not set.", field.Name));
                    }

                    if ( field.FieldType == typeof(string) && string.IsNullOrEmpty((string)value) ) {
                        return Error(string.Format("A required string field named '{0}' is not set.", field.Name));
                    }

                    if ( typeof(BBParameter).RTIsAssignableFrom(field.FieldType) && ( value as BBParameter ).isNull ) {
                        return Error(string.Format("A required BBParameter field value named '{0}' is not set.", field.Name));
                    }
                }
#endif

                if ( newAgent != null && ( typeof(Component).RTIsAssignableFrom(field.FieldType) || field.FieldType.IsInterface ) ) {
                    if ( field.RTIsDefined<GetFromAgentAttribute>(true) ) {
                        var o = newAgent.GetComponent(field.FieldType);
                        field.SetValue(this, o);
                        if ( ReferenceEquals(o, null) ) {
                            return Error(string.Format("GetFromAgent Attribute failed to get the required Component of type '{0}' from '{1}'. Does it exist?", field.FieldType.Name, agent.gameObject.name));
                        }
                    }
                }

            }
#endif

            return true;
        }

        //Utility function to log and return errors
        protected bool Error(string error, string tag = LogTag.EXECUTION) {
            Logger.LogError(error, tag, this);
            return false;
        }

        ///----------------------------------------------------------------------------------------------

        ///<summary>Tasks can start coroutine through MonoManager</summary>
        protected Coroutine StartCoroutine(IEnumerator routine) {
            return MonoManager.current != null ? MonoManager.current.StartCoroutine(routine) : null;
        }

        ///<summary>Tasks can start coroutine through MonoManager</summary>
        protected void StopCoroutine(Coroutine routine) {
            if ( MonoManager.current != null ) { MonoManager.current.StopCoroutine(routine); }
        }

        ///----------------------------------------------------------------------------------------------

        ///<summary>Sends an event through the owner system to handle (same as calling ownerSystem.SendEvent)</summary>
        protected void SendEvent(string name) {
            if ( ownerSystem != null ) { ownerSystem.SendEvent(name, null, this); }
        }
        ///<summary>Sends an event through the owner system to handle (same as calling ownerSystem.SendEvent)</summary>
        protected void SendEvent<T>(string name, T value) {
            if ( ownerSystem != null ) { ownerSystem.SendEvent(name, value, this); }
        }

        ///----------------------------------------------------------------------------------------------

        //Gather warnings for user convernience. Basicaly used in the editor, but could be used in runtime as well.
        virtual internal string GetWarningOrError() {

            var hardError = GetHardError();
            if ( hardError != null ) { return "* " + hardError; }

            var userError = OnErrorCheck();
            if ( userError != null ) { return userError; }

            if ( obsolete != string.Empty ) {
                return string.Format("Task is obsolete: '{0}'", obsolete);
            }

            if ( agentType != null && agent == null ) {
                if ( _agentParameter == null || ( _agentParameter.isNoneOrNull && !_agentParameter.isDefined ) ) {
                    return string.Format("* '{0}' target agent is null", agentType.Name);
                }
            }

            var fields = this.GetType().RTGetFields();
            for ( var i = 0; i < fields.Length; i++ ) {
                var field = fields[i];
                if ( field.RTIsDefined<RequiredFieldAttribute>(true) ) {
                    var value = field.GetValue(this);
                    if ( value == null || value.Equals(null) ) {
                        return string.Format("* Required field '{0}' is null", field.Name.SplitCamelCase());
                    }
                    if ( field.FieldType == typeof(string) && string.IsNullOrEmpty((string)value) ) {
                        return string.Format("* Required string field '{0}' is null or empty", field.Name.SplitCamelCase());
                    }
                    if ( typeof(BBParameter).RTIsAssignableFrom(field.FieldType) ) {
                        var bbParam = value as BBParameter;
                        if ( bbParam == null ) {
                            return string.Format("* BBParameter '{0}' is null", field.Name.SplitCamelCase());
                        } else if ( !bbParam.isDefined && bbParam.isNoneOrNull ) {
                            return string.Format("* Required parameter '{0}' is null", field.Name.SplitCamelCase());
                        }
                    }
                }
            }
            return null;
        }

        ///<summary> Override and return anything but null to mark the task has an error</summary>
        virtual protected string OnErrorCheck() { return null; }

        ///<summary>A hard error, missing things</summary>
        string GetHardError() {
            if ( this is IMissingRecoverable ) {
                return string.Format("Missing Task '{0}'", ( this as IMissingRecoverable ).missingType);
            }

            if ( this is IReflectedWrapper ) {
                var info = ( this as IReflectedWrapper ).GetSerializedInfo();
                if ( info != null && info.AsMemberInfo() == null ) { return string.Format("Missing Reflected Info '{0}'", info.AsString()); }
            }
            return null;
        }

        ///----------------------------------------------------------------------------------------------

        ///<summary>Override in Tasks. This is called AFTER a NEW agent is set, after initialization and before execution. Return null if everything is ok, or a string with the error if not.</summary>
        virtual protected string OnInit() { return null; }
        ///<summary>Called once the first time task is created</summary>
        virtual public void OnCreate(ITaskSystem ownerSystem) { }
        ///<summary>Called when the task is created, duplicated or otherwise needs validation.</summary>
        virtual public void OnValidate(ITaskSystem ownerSystem) { }
        [System.Obsolete("Use OnDrawGizmosSelected")]
        virtual public void OnDrawGizmos() { OnDrawGizmosSelected(); }
        ///<summary>Draw gizmos when the element containing the task is selected</summary>
        virtual public void OnDrawGizmosSelected() { }

        ///----------------------------------------------------------------------------------------------

        //...
        public override string ToString() {
            return summaryInfo;
        }

        ///----------------------------------------------------------------------------------------------


        ///----------------------------------------------------------------------------------------------
        ///---------------------------------------UNITY EDITOR-------------------------------------------
#if UNITY_EDITOR

        private object _icon;
        //The icon if any of the task
        public Texture2D icon {
            get
            {
                if ( _icon == null ) {
                    var iconAtt = this.GetType().RTGetAttribute<ParadoxNotion.Design.IconAttribute>(true);
                    _icon = iconAtt != null ? TypePrefs.GetTypeIcon(iconAtt, this) : null;
                    if ( _icon == null ) { _icon = new object(); }
                }
                return _icon as Texture2D;
            }
        }

        ///<summary>Draw an automatic editor inspector for this task.</summary>
        protected void DrawDefaultInspector() { EditorUtils.ReflectedObjectInspector(this, ownerSystem.contextObject); }
        ///<summary>Optional override to show custom controls whenever the ShowTaskInspectorGUI is called. By default controls will automaticaly show for most types.</summary>
        virtual protected void OnTaskInspectorGUI() { DrawDefaultInspector(); }

#endif
        ///----------------------------------------------------------------------------------------------
    }
}