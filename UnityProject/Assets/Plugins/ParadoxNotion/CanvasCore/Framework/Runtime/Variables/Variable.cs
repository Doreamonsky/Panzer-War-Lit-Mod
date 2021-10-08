using System;
using System.Reflection;
using ParadoxNotion;
using ParadoxNotion.Serialization;
using ParadoxNotion.Serialization.FullSerializer;
using NodeCanvas.Framework.Internal;
using UnityEngine;
using Logger = ParadoxNotion.Services.Logger;

namespace NodeCanvas.Framework
{

#if UNITY_EDITOR //handles missing variable types
    [fsObject(Processor = typeof(fsRecoveryProcessor<Variable, MissingVariableType>))]
#endif

    [Serializable, fsUninitialized]
    [ParadoxNotion.Design.SpoofAOT]
    ///Variables are stored in Blackboards and can optionaly be bound to Properties or Fields of a Unity Component
    abstract public class Variable
    {

        [SerializeField] private string _name;
        [SerializeField] private string _id;
        [SerializeField] private bool _isPublic;
        [SerializeField, fsIgnoreInBuild] private bool _debugBoundValue;

        ///Raised when name change
        public event Action<string> onNameChanged;
        ///Raised when value change
        public event Action<object> onValueChanged;
        ///Raised when variable is destroyed/removed from blackboard
        public event Action onDestroy;

        ///The name of the variable
        public string name {
            get { return _name; }
            set
            {
                if ( _name != value ) {
                    _name = value;
                    if ( onNameChanged != null ) {
                        onNameChanged(value);
                    }
                }
            }
        }

        ///A Unique ID
        public string ID { get { return string.IsNullOrEmpty(_id) ? _id = Guid.NewGuid().ToString() : _id; } }
        ///The value as object type when accessing from base class
        public object value { get { return GetValueBoxed(); } set { SetValueBoxed(value); } }
        ///Is the variable exposed public?
        public bool isExposedPublic { get { return _isPublic && !isPropertyBound; } set { _isPublic = value; } }
        ///For debugging data bound value in inspector (editor only)
        public bool debugBoundValue { get { return _debugBoundValue; } set { _debugBoundValue = value; } }
        ///Is the variable bound to a property/field?
        public bool isPropertyBound => !string.IsNullOrEmpty(propertyPath);

        ///Is the variable data bound now?
        abstract public bool isDataBound { get; }
        ///The Type this Variable holds
        abstract public Type varType { get; }
        ///The path to the property this data is binded to. Null if none
        abstract public string propertyPath { get; set; }

        ///----------------------------------------------------------------------------------------------

        ///Used to bind variable to a property
        abstract public void BindProperty(MemberInfo prop, GameObject target = null);
        ///Used to un-bind variable from a property
        abstract public void UnBind();
        ///Called from Blackboard in Awake to Initialize the binding on specified game object
        abstract public void InitializePropertyBinding(GameObject go, bool callSetter = false);
        ///Same as .value. Used for binding.
        abstract public object GetValueBoxed();
        ///Same as .value. Used for binding.
        abstract public void SetValueBoxed(object value);
        ///----------------------------------------------------------------------------------------------

        //required
        public Variable() { _id = Guid.NewGuid().ToString(); }
        public Variable(string name, string ID) { _name = name; _id = ID; }

        //...
        internal void OnDestroy() { if ( onDestroy != null ) { onDestroy(); } }

        ///Duplicate this Variable into target Blackboard
        public Variable Duplicate(IBlackboard targetBB) {
            var finalName = this.name;
            while ( targetBB.variables.ContainsKey(finalName) ) {
                finalName += ".";
            }
            var newVar = targetBB.AddVariable(finalName, varType);
            if ( newVar != null ) {
                newVar.value = this.value;
                newVar.propertyPath = this.propertyPath;
                newVar.isExposedPublic = this.isExposedPublic;
            }
            return newVar;
        }

        //we need this since onValueChanged is an event and we can't check != null outside of this class
        protected bool HasValueChangeEvent() { return onValueChanged != null; }
        //invoke value changed event
        protected void TryInvokeValueChangeEvent(object value) { if ( onValueChanged != null ) { onValueChanged(value); } }

        ///Checks whether a convertion to type is possible
        public bool CanConvertTo(Type toType) { return GetGetConverter(toType) != null; }
        ///Gets a Func<object> that converts the value ToType if possible. Null if not.
        public Func<object> GetGetConverter(Type toType) {

            if ( toType.RTIsAssignableFrom(varType) ) {
                return () => value;
            }

            var converter = TypeConverter.Get(varType, toType);
            if ( converter != null ) {
                return () => converter(value);
            }

            return null;
        }

        ///Checks whether a convertion from type is possible
        public bool CanConvertFrom(Type fromType) { return GetSetConverter(fromType) != null; }
        ///Gets an Action<object> that converts the value fromType if possible. Null if not.
        public Action<object> GetSetConverter(Type fromType) {

            if ( varType.RTIsAssignableFrom(fromType) ) {
                return (x) => value = x;
            }

            var converter = TypeConverter.Get(fromType, varType);
            if ( converter != null ) {
                return (x) => value = converter(x);
            }

            return null;
        }

        //...
        public override string ToString() { return name; }
    }

    ///----------------------------------------------------------------------------------------------

    ///The actual Variable
    public class Variable<T> : Variable
    {

        [SerializeField] private T _value;
        [SerializeField] private string _propertyPath;

        //delegates for binding
        private event Func<T> getter;
        private event Action<T> setter;
        //

        public override Type varType => typeof(T);
        public override bool isDataBound => getter != null || setter != null;
        public override string propertyPath { get { return _propertyPath; } set { _propertyPath = value; } }

        ///The value as type T when accessing as this type
        new public T value {
            get { return getter != null ? getter() : _value; }
            set
            {
                if ( base.HasValueChangeEvent() ) { //check this first to avoid unescessary value boxing
                    var boxed = (object)value;
                    if ( !ObjectUtils.AnyEquals(_value, boxed) ) {
                        this._value = value;
                        if ( setter != null ) { setter(value); }
                        base.TryInvokeValueChangeEvent(boxed);
                    }
                    return;
                }

                this._value = value;
                if ( setter != null ) { setter(value); }
            }
        }

        ///----------------------------------------------------------------------------------------------

        //required
        public Variable() { }
        public Variable(string name, string ID) : base(name, ID) { }

        ///Same as .value. Used for binding.
        public override object GetValueBoxed() { return value; }
        ///Same as .value. Used for binding.
        public override void SetValueBoxed(object newValue) { this.value = (T)newValue; }
        ///Same as .value. Used for binding.
        public T GetValue() { return value; }
        ///Same as .value. Used for binding.
        public void SetValue(T newValue) { this.value = newValue; }

        ///Set the property binding. Providing target also initializes the property binding
        public override void BindProperty(MemberInfo prop, GameObject target = null) {
            if ( prop is PropertyInfo || prop is FieldInfo ) {
                _propertyPath = string.Format("{0}.{1}", prop.RTReflectedOrDeclaredType().FullName, prop.Name);
                if ( target != null ) { InitializePropertyBinding(target, false); }
            }
        }

        ///Bind getter and setter directly
        public void BindGetSet(Func<T> _get, Action<T> _set) {
            this.getter = _get;
            this.setter = _set;
        }

        ///Removes the property and data binding
        public override void UnBind() {
            _propertyPath = null;
            getter = null;
            setter = null;
        }

        ///Initialize the property binding for target gameobject. The gameobject is only used in case the binding is not static.
        public override void InitializePropertyBinding(GameObject go, bool callSetter = false) {

            if ( !isPropertyBound || !ParadoxNotion.Services.Threader.applicationIsPlaying ) {
                return;
            }

            getter = null;
            setter = null;

            var idx = _propertyPath.LastIndexOf('.');
            var typeString = _propertyPath.Substring(0, idx);
            var memberString = _propertyPath.Substring(idx + 1);
            var type = ReflectionTools.GetType(typeString, /*fallback?*/ true, typeof(Component));

            if ( type == null ) {
                Logger.LogError(string.Format("Type '{0}' not found for Blackboard Variable '{1}' Binding.", typeString, name), LogTag.VARIABLE, go);
                return;
            }

            var member = type.RTGetFieldOrProp(memberString);

            if ( member is FieldInfo ) {
                var field = (FieldInfo)member;
                var instance = field.IsStatic ? null : go.GetComponent(type);
                if ( instance == null && !field.IsStatic ) {
                    Logger.LogError(string.Format("A Blackboard Variable '{0}' is due to bind to a Component type that is missing '{1}'. Binding ignored", name, typeString), LogTag.VARIABLE, go);
                    return;
                }
                if ( field.IsConstant() ) {
                    T value = (T)field.GetValue(instance);
                    getter = () => { return value; };
                } else {
                    getter = () => { return (T)field.GetValue(instance); };
                    setter = (o) => { field.SetValue(instance, o); };
                }

                return;
            }

            if ( member is PropertyInfo ) {
                var prop = (PropertyInfo)member;
                var getMethod = prop.RTGetGetMethod();
                var setMethod = prop.RTGetSetMethod();
                var isStatic = ( getMethod != null && getMethod.IsStatic ) || ( setMethod != null && setMethod.IsStatic );
                var instance = isStatic ? null : go.GetComponent(type);
                if ( instance == null && !isStatic ) {
                    Logger.LogError(string.Format("A Blackboard Variable '{0}' is due to bind to a Component type that is missing '{1}'. Binding ignored.", name, typeString), LogTag.VARIABLE, go);
                    return;
                }

                if ( prop.CanRead ) {
                    try { getter = getMethod.RTCreateDelegate<Func<T>>(instance); } //JIT
                    catch { getter = () => { return (T)getMethod.Invoke(instance, null); }; } //AOT
                } else {
                    getter = () => { Logger.LogError(string.Format("You tried to Get a Property Bound Variable '{0}', but the Bound Property '{1}' is Write Only!", name, _propertyPath), LogTag.VARIABLE, go); return default(T); };
                }

                if ( prop.CanWrite ) {
                    try { setter = setMethod.RTCreateDelegate<Action<T>>(instance); } //JIT
                    catch { setter = (o) => { setMethod.Invoke(instance, ReflectionTools.SingleTempArgsArray(o)); }; } //AOT
                    if ( callSetter ) { setter(_value); }
                } else {
                    setter = (o) => { Logger.LogError(string.Format("You tried to Set a Property Bound Variable '{0}', but the Bound Property '{1}' is Read Only!", name, _propertyPath), LogTag.VARIABLE, go); };
                }

                return;
            }

            Logger.LogError(string.Format("A Blackboard Variable '{0}' is due to bind to a property/field named '{1}' that does not exist on type '{2}'. Binding ignored", name, memberString, type.FullName), LogTag.VARIABLE, go);
        }
    }
}