using System.Reflection;
using System.Linq;
using NodeCanvas.Framework;
using ParadoxNotion;
using ParadoxNotion.Design;
using ParadoxNotion.Serialization;
using ParadoxNotion.Serialization.FullSerializer;
using UnityEngine;


namespace NodeCanvas.Tasks.Conditions
{

    ///----------------------------------------------------------------------------------------------
    ///previous versions
    class CheckCSharpEvent_0
    {
        [SerializeField] public System.Type targetType = null;
        [SerializeField] public string eventName = null;
    }

    class CheckCSharpEvent_0<T>
    {

        [SerializeField] public System.Type targetType = null;
        [SerializeField] public string eventName = null;
    }

    class CheckCSharpEventValue_0<T>
    {
        [SerializeField] public System.Type targetType = null;
        [SerializeField] public string eventName = null;
    }

    [fsMigrateTo(typeof(CheckCSharpEvent))]
    class CheckStaticCSharpEvent
    {
        [SerializeField] public System.Type targetType = null;
        [SerializeField] public string eventName = null;
    }

    [fsMigrateTo(typeof(CheckCSharpEvent<>))]
    class CheckStaticCSharpEvent<T>
    {
        [SerializeField] public System.Type targetType = null;
        [SerializeField] public string eventName = null;
    }
    ///previous versions
    ///----------------------------------------------------------------------------------------------


    [Category("✫ Reflected/Events")]
    [Description("Will subscribe to a public event of Action type and return true when the event is raised.\n(eg public event System.Action [name])")]
    [fsMigrateVersions(typeof(CheckCSharpEvent_0))]
    public class CheckCSharpEvent : ConditionTask, IReflectedWrapper, IMigratable<CheckCSharpEvent_0>, IMigratable<CheckStaticCSharpEvent>
    {

        ///----------------------------------------------------------------------------------------------
        void IMigratable<CheckCSharpEvent_0>.Migrate(CheckCSharpEvent_0 model) {
            this.SetTargetEvent(model.targetType?.RTGetEvent(model.eventName));
        }
        void IMigratable<CheckStaticCSharpEvent>.Migrate(CheckStaticCSharpEvent model) {
            this.SetTargetEvent(model.targetType?.RTGetEvent(model.eventName));
        }
        ///----------------------------------------------------------------------------------------------

        [SerializeField]
        private SerializedEventInfo eventInfo = null;

        private System.Delegate handler;
        private EventInfo targetEvent => eventInfo;

        public override System.Type agentType {
            get
            {
                if ( targetEvent == null ) { return typeof(Transform); }
                return targetEvent.IsStatic() ? null : targetEvent.RTReflectedOrDeclaredType();
            }
        }

        protected override string info {
            get
            {
                if ( eventInfo == null ) { return "No Event Selected"; }
                if ( targetEvent == null ) { return eventInfo.AsString().FormatError(); }
                return string.Format("'{0}' Raised", targetEvent.Name);
            }
        }

        ISerializedReflectedInfo IReflectedWrapper.GetSerializedInfo() { return eventInfo; }

        protected override string OnInit() {
            if ( eventInfo == null ) { return "No Event Selected"; }
            if ( targetEvent == null ) { return eventInfo.AsString().FormatError(); }

            var methodInfo = this.GetType().RTGetMethod("Raised");
            this.handler = methodInfo.RTCreateDelegate(targetEvent.EventHandlerType, this);
            return null;
        }

        protected override void OnEnable() {
            if ( handler != null ) targetEvent.AddEventHandler(targetEvent.IsStatic() ? null : agent, handler);
        }

        protected override void OnDisable() {
            if ( handler != null ) targetEvent.RemoveEventHandler(targetEvent.IsStatic() ? null : agent, handler);
        }

        public void Raised() { YieldReturn(true); }
        protected override bool OnCheck() { return false; }

        void SetTargetEvent(EventInfo info) {
            if ( info != null ) {
                eventInfo = new SerializedEventInfo(info);
            }
        }

        ////////////////////////////////////////
        ///////////GUI AND EDITOR STUFF/////////
        ////////////////////////////////////////
#if UNITY_EDITOR

        protected override void OnTaskInspectorGUI() {

            if ( !Application.isPlaying && GUILayout.Button("Select Event") ) {
                var menu = new UnityEditor.GenericMenu();
                if ( agent != null ) {
                    foreach ( var comp in agent.GetComponents(typeof(Component)).Where(c => c.hideFlags == 0) ) {
                        menu = EditorUtils.GetInstanceEventSelectionMenu(comp.GetType(), null, SetTargetEvent, menu);
                    }
                    menu.AddSeparator("/");
                }
                foreach ( var t in TypePrefs.GetPreferedTypesList(typeof(object)) ) {
                    menu = EditorUtils.GetStaticEventSelectionMenu(t, null, SetTargetEvent, menu);
                    if ( typeof(Component).IsAssignableFrom(t) ) {
                        menu = EditorUtils.GetInstanceEventSelectionMenu(t, null, SetTargetEvent, menu);
                    }
                }
                menu.ShowAsBrowser("Select Event", this.GetType());
                Event.current.Use();
            }

            if ( targetEvent != null ) {
                GUILayout.BeginVertical("box");
                UnityEditor.EditorGUILayout.LabelField("Selected Type", targetEvent.DeclaringType.FriendlyName());
                UnityEditor.EditorGUILayout.LabelField("Selected Event", targetEvent.Name);
                GUILayout.EndVertical();
            }
        }
#endif

    }

    ///----------------------------------------------------------------------------------------------

    [Category("✫ Reflected/Events")]
    [Description("Will subscribe to a public event of Action<T> type and return true when the event is raised.\n(eg public event System.Action<T> [name])")]
    [fsMigrateVersions(typeof(CheckCSharpEvent_0<>))]
    public class CheckCSharpEvent<T> : ConditionTask, IReflectedWrapper, IMigratable<CheckCSharpEvent_0<T>>, IMigratable<CheckStaticCSharpEvent<T>>
    {

        ///----------------------------------------------------------------------------------------------
        void IMigratable<CheckCSharpEvent_0<T>>.Migrate(CheckCSharpEvent_0<T> model) {
            this.SetTargetEvent(model.targetType?.RTGetEvent(model.eventName));
        }
        void IMigratable<CheckStaticCSharpEvent<T>>.Migrate(CheckStaticCSharpEvent<T> model) {
            this.SetTargetEvent(model.targetType?.RTGetEvent(model.eventName));
        }
        ///----------------------------------------------------------------------------------------------

        [SerializeField]
        private SerializedEventInfo eventInfo = null;
        [SerializeField, BlackboardOnly]
        private BBParameter<T> saveAs = null;

        private System.Delegate handler;
        private EventInfo targetEvent => eventInfo;

        public override System.Type agentType {
            get
            {
                if ( targetEvent == null ) { return typeof(Transform); }
                return targetEvent.IsStatic() ? null : targetEvent.RTReflectedOrDeclaredType();
            }
        }

        protected override string info {
            get
            {
                if ( eventInfo == null ) { return "No Event Selected"; }
                if ( targetEvent == null ) { return eventInfo.AsString().FormatError(); }
                return string.Format("'{0}' Raised", targetEvent.Name);
            }
        }

        ISerializedReflectedInfo IReflectedWrapper.GetSerializedInfo() { return eventInfo; }

        protected override string OnInit() {
            if ( eventInfo == null ) { return "No Event Selected"; }
            if ( targetEvent == null ) { return eventInfo.AsString().FormatError(); }

            var methodInfo = this.GetType().RTGetMethod("Raised");
            handler = methodInfo.RTCreateDelegate(targetEvent.EventHandlerType, this);
            return null;
        }

        protected override void OnEnable() {
            if ( handler != null ) targetEvent.AddEventHandler(targetEvent.IsStatic() ? null : agent, handler);
        }

        protected override void OnDisable() {
            if ( handler != null ) targetEvent.RemoveEventHandler(targetEvent.IsStatic() ? null : agent, handler);
        }

        public void Raised(T eventValue) {
            saveAs.value = eventValue;
            YieldReturn(true);
        }

        protected override bool OnCheck() { return false; }

        void SetTargetEvent(EventInfo info) {
            if ( info != null ) {
                eventInfo = new SerializedEventInfo(info);
            }
        }

        ////////////////////////////////////////
        ///////////GUI AND EDITOR STUFF/////////
        ////////////////////////////////////////
#if UNITY_EDITOR

        protected override void OnTaskInspectorGUI() {

            if ( !Application.isPlaying && GUILayout.Button("Select Event") ) {
                var menu = new UnityEditor.GenericMenu();
                if ( agent != null ) {
                    foreach ( var comp in agent.GetComponents(typeof(Component)).Where(c => c.hideFlags == 0) ) {
                        menu = EditorUtils.GetInstanceEventSelectionMenu(comp.GetType(), typeof(T), SetTargetEvent, menu);
                    }
                    menu.AddSeparator("/");
                }
                foreach ( var t in TypePrefs.GetPreferedTypesList(typeof(object)) ) {
                    menu = EditorUtils.GetStaticEventSelectionMenu(t, typeof(T), SetTargetEvent, menu);
                    if ( typeof(Component).IsAssignableFrom(t) ) {
                        menu = EditorUtils.GetInstanceEventSelectionMenu(t, typeof(T), SetTargetEvent, menu);
                    }
                }
                menu.ShowAsBrowser("Select Event", this.GetType());
                Event.current.Use();
            }

            if ( targetEvent != null ) {
                GUILayout.BeginVertical("box");
                UnityEditor.EditorGUILayout.LabelField("Selected Type", targetEvent.DeclaringType.FriendlyName());
                UnityEditor.EditorGUILayout.LabelField("Selected Event", targetEvent.Name);
                GUILayout.EndVertical();

                NodeCanvas.Editor.BBParameterEditor.ParameterField("Save Value As", saveAs, true);
            }
        }
#endif

    }

    ///----------------------------------------------------------------------------------------------

    [Category("✫ Reflected/Events")]
    [Description("Will subscribe to a public event of Action<T> type and return true when the event is raised and it's value is equal to provided value as well.\n(eg public event System.Action<T> [name])")]
    [fsMigrateVersions(typeof(CheckCSharpEventValue_0<>))]
    public class CheckCSharpEventValue<T> : ConditionTask, IReflectedWrapper, IMigratable<CheckCSharpEventValue_0<T>>
    {

        ///----------------------------------------------------------------------------------------------
        void IMigratable<CheckCSharpEventValue_0<T>>.Migrate(CheckCSharpEventValue_0<T> model) {
            this.SetTargetEvent(model.targetType?.RTGetEvent(model.eventName));
        }
        ///----------------------------------------------------------------------------------------------

        [SerializeField]
        private SerializedEventInfo eventInfo = null;
        [SerializeField]
        private BBParameter<T> checkValue = null;

        private System.Delegate handler;
        private EventInfo targetEvent => eventInfo;

        public override System.Type agentType {
            get
            {
                if ( targetEvent == null ) { return typeof(Transform); }
                return targetEvent.IsStatic() ? null : targetEvent.RTReflectedOrDeclaredType();
            }
        }

        protected override string info {
            get
            {
                if ( eventInfo == null ) { return "No Event Selected"; }
                if ( targetEvent == null ) { return eventInfo.AsString().FormatError(); }
                return string.Format("'{0}' Raised && Value == {1}", targetEvent.Name, checkValue);
            }
        }

        ISerializedReflectedInfo IReflectedWrapper.GetSerializedInfo() { return eventInfo; }

        protected override string OnInit() {
            if ( eventInfo == null ) { return "No Event Selected"; }
            if ( targetEvent == null ) { return eventInfo.AsString().FormatError(); }

            var methodInfo = this.GetType().RTGetMethod("Raised");
            handler = methodInfo.RTCreateDelegate(targetEvent.EventHandlerType, this);
            return null;
        }

        protected override void OnEnable() {
            if ( handler != null ) targetEvent.AddEventHandler(targetEvent.IsStatic() ? null : agent, handler);
        }

        protected override void OnDisable() {
            if ( handler != null ) targetEvent.RemoveEventHandler(targetEvent.IsStatic() ? null : agent, handler);
        }

        public void Raised(T eventValue) {
            if ( ObjectUtils.AnyEquals(checkValue.value, eventValue) ) {
                YieldReturn(true);
            }
        }

        protected override bool OnCheck() { return false; }

        void SetTargetEvent(EventInfo info) {
            if ( info != null ) {
                eventInfo = new SerializedEventInfo(info);
            }
        }

        ////////////////////////////////////////
        ///////////GUI AND EDITOR STUFF/////////
        ////////////////////////////////////////
#if UNITY_EDITOR

        protected override void OnTaskInspectorGUI() {

            if ( !Application.isPlaying && GUILayout.Button("Select Event") ) {
                var menu = new UnityEditor.GenericMenu();
                if ( agent != null ) {
                    foreach ( var comp in agent.GetComponents(typeof(Component)).Where(c => c.hideFlags == 0) ) {
                        menu = EditorUtils.GetInstanceEventSelectionMenu(comp.GetType(), typeof(T), SetTargetEvent, menu);
                    }
                    menu.AddSeparator("/");
                }
                foreach ( var t in TypePrefs.GetPreferedTypesList(typeof(object)) ) {
                    menu = EditorUtils.GetStaticEventSelectionMenu(t, typeof(T), SetTargetEvent, menu);
                    if ( typeof(Component).IsAssignableFrom(t) ) {
                        menu = EditorUtils.GetInstanceEventSelectionMenu(t, typeof(T), SetTargetEvent, menu);
                    }
                }

                menu.ShowAsBrowser("Select Event", this.GetType());
                Event.current.Use();
            }

            if ( targetEvent != null ) {
                GUILayout.BeginVertical("box");
                UnityEditor.EditorGUILayout.LabelField("Selected Type", targetEvent.DeclaringType.FriendlyName());
                UnityEditor.EditorGUILayout.LabelField("Selected Event", targetEvent.Name);
                GUILayout.EndVertical();

                NodeCanvas.Editor.BBParameterEditor.ParameterField("Check Value", checkValue);
            }
        }
#endif

    }

}