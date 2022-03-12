using System;
using System.Collections;
using ParadoxNotion;
using ParadoxNotion.Serialization.FullSerializer;
using NodeCanvas.Framework.Internal;
using UnityEngine;
using Logger = ParadoxNotion.Services.Logger;
using Threader = ParadoxNotion.Services.Threader;

namespace NodeCanvas.Framework
{

#if UNITY_EDITOR //handles missing parameter types and upgrades of T to BBParameter<T>
    [fsObject(Processor = typeof(fsBBParameterProcessor))]
#endif

    //TODO: Change GlobalBlackboard resolution to use BB.UID

    ///<summary>Class for Parameter Variables that allow binding to a Blackboard variable or specifying a value directly.</summary>
    [ParadoxNotion.Design.SpoofAOT]
    [Serializable, fsAutoInstance, fsUninitialized]
    abstract public class BBParameter : ISerializationCollectable, ISerializationCallbackReceiver
    {

        //reset value to default when using bb
        void ISerializationCallbackReceiver.OnBeforeSerialize() { if ( useBlackboard ) { SetDefaultValue(); } }
        void ISerializationCallbackReceiver.OnAfterDeserialize() { }

        //null means use local _value, empty means [NONE], anything else means use bb variable.
        [SerializeField] private string _name;
        //the target bb variable ID
        [SerializeField] private string _targetVariableID;

        private IBlackboard _bb;
        private Variable _varRef;

        ///<summary>Raised when the BBParameter is linked to a different variable reference.</summary>
        public event Action<Variable> onVariableReferenceChanged;

        ///----------------------------------------------------------------------------------------------

        //required
        public BBParameter() { }

        ///<summary>Create and return an instance of a generic BBParameter<T> with type argument provided and set to read from the specified blackboard</summary>
        public static BBParameter CreateInstance(Type t, IBlackboard bb) {
            if ( t == null ) { return null; }
            var newBBParam = (BBParameter)Activator.CreateInstance(typeof(BBParameter<>).RTMakeGenericType(t));
            newBBParam.bb = bb;
            return newBBParam;
        }

        ///<summary>Set the blackboard reference provided for all BBParameters fields found in target</summary>
        public static void SetBBFields(object target, IBlackboard bb) {
            if ( target == null ) { return; }
            ParadoxNotion.Serialization.JSONSerializer.SerializeAndExecuteNoCycles(target.GetType(), target, (o, d) =>
            {
                if ( o is BBParameter ) { ( o as BBParameter ).bb = bb; }
            });
        }

        ///----------------------------------------------------------------------------------------------

        ///<summary>The target variable ID</summary>
        public string targetVariableID {
            get { return _targetVariableID; }
            protected set { _targetVariableID = value; }
        }

        ///<summary>The Variable object reference if any.One is set when name change as well as when SetBBFields is called. Setting the varRef also binds this parameter with that Variable.</summary>
        public Variable varRef {
            get { return _varRef; }
            protected set
            {
                if ( _varRef != value ) {

#if UNITY_EDITOR
                    //Only required in editor for clearing var reference and updating name
                    if ( !Threader.applicationIsPlaying ) {
                        if ( _varRef != null ) {
                            _varRef.onDestroy -= OnRefDestroyed;
                            _varRef.onNameChanged -= OnRefNameChanged;
                        }
                        if ( value != null ) {
                            value.onDestroy += OnRefDestroyed;
                            value.onNameChanged += OnRefNameChanged;
                            OnRefNameChanged(value.name);
                        }
                        if ( value != null ) {
                            targetVariableID = value.ID;
                        } else if ( string.IsNullOrEmpty(name) ) {
                            targetVariableID = null;
                        }
                    }
#endif

                    _varRef = value;
                    Bind(value);
                    if ( onVariableReferenceChanged != null ) {
                        onVariableReferenceChanged(value);
                    }
                }
            }
        }

#if UNITY_EDITOR
        //clear ref
        void OnRefDestroyed() {
            varRef = null;
            targetVariableID = null;
        }

        //Is the param's variable reference changed name?
        void OnRefNameChanged(string newName) {
            if ( _name.Contains("/") ) { //is global
                var bbName = _name.Split('/')[0];
                newName = bbName + "/" + newName;
            }
            _name = newName;
        }
#endif

        ///<summary>The name of the Variable to read/write from. Null if not, Empty if [NONE].</summary>
        public string name {
            get { return _name; }
            set
            {
                if ( _name != value ) {
                    _name = value;
                    if ( string.IsNullOrEmpty(value) ) {
                        varRef = null;
                        targetVariableID = null;
                    } else {
                        varRef = value != null ? ResolveReference(bb, false) : null;
                    }
                }
            }
        }

        ///<summary>The blackboard to read/write from. Setting this also sets the variable reference if found</summary>
        public IBlackboard bb {
            get { return _bb; }
            set
            {
                if ( _bb != value ) {
#if UNITY_EDITOR
                    //only nice to have in editor
                    if ( !Threader.applicationIsPlaying ) {
                        if ( _bb != null ) { _bb.onVariableAdded -= OnBBVariableAdded; }
                        if ( value != null ) { value.onVariableAdded += OnBBVariableAdded; }
                    }
#endif
                    _bb = value;
                }
                varRef = value != null ? ResolveReference(_bb, true) : null;
            }
        }

#if UNITY_EDITOR
        //reason: automatically links to varefs if ref is missing when var is added to bb
        void OnBBVariableAdded(Variable variable) {
            // if ( variable.ID == targetVariableID ) { varRef = variable; }
            if ( this.varRef == null && variable.name == this.name && variable.CanConvertTo(varType) ) {
                varRef = variable;
            }
        }
#endif

        ///<summary>Is the parameter -set or should- to read from a blackboard variable?</summary>
        public bool useBlackboard {
            get { return name != null; }
            set
            {
                if ( value == false ) {
                    name = null;
                    targetVariableID = null;
                    varRef = null;
                }
                if ( value == true && name == null ) {
                    name = string.Empty;
                }
            }
        }

        ///<summary>Parameter is presumed dynamic if it starts with an "_" by convention</summary>
        public bool isPresumedDynamic => name != null && name.StartsWith("_");
        ///<summary>Has the user selected [NONE] in the dropdown?</summary>
        public bool isNone => name == string.Empty;
        ///<summary>Is the final value null?</summary>
        public bool isNull => ObjectUtils.AnyEquals(value, null);
        ///<summary>Shortcut to exactly what it says :)</summary>
        public bool isNoneOrNull => isNone || isNull;
        ///<summary>Shortcut to 'useBlackboard AND !isNone'</summary>
        public bool isDefined => !string.IsNullOrEmpty(name);
        ///<summary>The type of the Variable reference or null if there is no Variable referenced. The returned type is for most cases the same as 'VarType'. RefType and VarType can be different when an AutoConvert is taking place.</summary>
        public Type refType => varRef != null ? varRef.varType : null;

        ///<summary>The value as object type when accessing from base class.</summary>
        public object value { get { return GetValueBoxed(); } set { SetValueBoxed(value); } }

        ///<summary>The type of the value that this BBParameter holds</summary>
        abstract public Type varType { get; }
        ///<summary>Set the default value</summary>
        abstract protected void SetDefaultValue();
        ///<summary>Bind the BBParameter to target. Null unbinds.</summary>
        abstract protected void Bind(Variable data);
        ///<summary>Same as .value. Used for binding.</summary>
        abstract public object GetValueBoxed();
        ///<summary>Same as .value. Used for binding.</summary>
        abstract public void SetValueBoxed(object value);

        ///----------------------------------------------------------------------------------------------

        //TODO: seriously...refactor global bbs
        internal void SetTargetVariable(IBlackboard targetBB, Variable targetVariable) {
            if ( targetVariable != null ) {
                _targetVariableID = targetVariable.ID;
                _name = ( targetBB is GlobalBlackboard ) ? string.Format("{0}/{1}", targetBB.identifier, targetVariable.name) : targetVariable.name;
                varRef = ResolveReference(this.bb, true);
            } else {
                targetVariableID = null;
            }
        }

        ///<summary>Resolve the final Variable reference.</summary>
        Variable ResolveReference(IBlackboard targetBlackboard, bool useID) {

            //avoid more work if we dont use a bb variable
            if ( string.IsNullOrEmpty(name) && string.IsNullOrEmpty(targetVariableID) ) {
                return null;
            }

            var targetName = this.name;
            if ( targetName != null && targetName.Contains("/") ) {
                var split = targetName.Split('/');
                targetBlackboard = GlobalBlackboard.Find(split[0]);
                targetName = split[1];
            }

            Variable result = null;
            if ( targetBlackboard == null ) { return null; }
            if ( useID && targetVariableID != null ) { result = targetBlackboard.GetVariableByID(targetVariableID); }
            if ( result == null && !string.IsNullOrEmpty(targetName) ) { result = targetBlackboard.GetVariable(targetName, varType); }
            return result;
        }

        ///<summary>Promotes the parameter to a variable on the target blackboard (overriden if parameter name is a path to a global bb).</summary>
        public Variable PromoteToVariable(IBlackboard targetBB) {

            if ( string.IsNullOrEmpty(name) ) {
                varRef = null;
                return null;
            }

            var varName = name;
            var bbName = string.Empty;
            if ( name.Contains("/") ) {
                var split = name.Split('/');
                bbName = split[0];
                varName = split[1];
                targetBB = GlobalBlackboard.Find(bbName);
            }

            if ( targetBB == null ) {
                varRef = null;
                Logger.LogError(string.Format("Parameter '{0}' failed to promote to a variable, because Blackboard named '{1}' could not be found.", varName, bbName), LogTag.VARIABLE, this);
                return null;
            }

            varRef = targetBB.AddVariable(varName, varType);
            if ( varRef != null ) {
                Logger.Log(string.Format("Parameter '{0}' (of type '{1}') promoted to a Variable in Blackboard '{2}'.", varName, varType.FriendlyName(), targetBB), LogTag.VARIABLE, this);
            } else {
                Logger.LogError(string.Format("Parameter {0} (of type '{1}') failed to promote to a Variable in Blackboard '{2}'.", varName, varType.FriendlyName(), targetBB), LogTag.VARIABLE, this);
            }
            return varRef;
        }

        ///<summary>Nicely formated text :)</summary>
        sealed public override string ToString() {
            if ( isNone ) {
                return "<b>NONE</b>";
            }
            if ( useBlackboard ) {
                var text = string.Format("<b>${0}</b>", name);
#if UNITY_EDITOR
                if ( UnityEditor.EditorGUIUtility.isProSkin ) {
                    return varRef != null ? text : string.Format("<color=#FF6C6C>{0}</color>", text);
                } else {
                    return varRef != null ? text : string.Format("<color=#DB2B2B>{0}</color>", text);
                }
#else
                return text;
#endif
            }
            if ( isNull ) {
                return "<b>NULL</b>";
            }
            if ( value is IList || value is IDictionary ) {
                return string.Format("<b>{0}</b>", varType.FriendlyName());
            }
            return string.Format("<b>{0}</b>", value.ToStringAdvanced());
        }
    }

    ///----------------------------------------------------------------------------------------------

    ///<summary>Use BBParameter to create a parameter possible to be linked to a blackboard Variable</summary>
    [Serializable]
    public class BBParameter<T> : BBParameter
    {

        [SerializeField] protected T _value;

        //delegates for Variable binding
        private event Func<T> getter;
        private event Action<T> setter;
        //

        //Value
        new public T value {
            get
            {
                if ( getter != null ) {
                    return getter();
                }

                //Dynamic?
                if ( Threader.applicationIsPlaying ) {
                    if ( varRef == null && bb != null && !string.IsNullOrEmpty(name) ) {
                        //setting the varRef property also binds it.
                        varRef = bb.GetVariable(name, typeof(T));
                        return getter != null ? getter() : default(T);
                    }
                }

                return _value;
            }
            set
            {
                if ( setter != null ) {
                    setter(value);
                    return;
                }

                if ( isNone ) {
                    return;
                }

                //Dynamic?
                if ( varRef == null && bb != null && !string.IsNullOrEmpty(name) ) {
                    if ( isPresumedDynamic ) {
                        Logger.Log(string.Format("Dynamic Parameter Variable '{0}' Encountered...", name), LogTag.VARIABLE, this);
                        //setting the varRef property also binds it
                        varRef = PromoteToVariable(bb);
                        if ( setter != null ) { setter(value); }
                    } else {
                        Logger.LogError(string.Format("A Parameter Variable named '{0}' is missing. If it was meant to be a dynamic variable, please ensure that it starts with an underscore ('_') prefix by convention.", name), LogTag.EXECUTION, this);
                    }
                    return;
                }

                _value = value;
            }
        }

        public override Type varType => typeof(T);

        ///----------------------------------------------------------------------------------------------

        public BBParameter() { }
        public BBParameter(T value) { _value = value; }

        ///<summary>Same as .value. Used for binding.</summary>
        public override object GetValueBoxed() { return value; }
        ///<summary>Same as .value. Used for binding.</summary>
        public override void SetValueBoxed(object newValue) { this.value = (T)newValue; }
        ///<summary>Same as .value. Used for binding.</summary>
        public T GetValue() { return value; }
        ///<summary>Same as .value. Used for binding.</summary>
        public void SetValue(T value) { this.value = value; }

        protected override void SetDefaultValue() { _value = default(T); }

        ///<summary>Binds this BBParameter to a Variable. Null unbinds</summary>
        protected override void Bind(Variable variable) {
            _value = default(T);
            if ( variable == null ) {
                getter = null;
                setter = null;
                return;
            }

            BindGetter(variable);
            BindSetter(variable);
        }

        //Bind the Getter
        bool BindGetter(Variable variable) {
            if ( variable is Variable<T> ) {
                getter = ( variable as Variable<T> ).GetValue;
                return true;
            }

            var convertFunc = variable.GetGetConverter(varType);
            if ( convertFunc != null ) {
                getter = () => { return (T)convertFunc(); };
                return true;
            }

            return false;
        }

        //Bind the Setter
        bool BindSetter(Variable variable) {
            if ( variable is Variable<T> ) {
                setter = ( variable as Variable<T> ).SetValue;
                return true;
            }

            var convertFunc = variable.GetSetConverter(varType);
            if ( convertFunc != null ) {
                setter = (T value) => { convertFunc(value); };
                return true;
            }

            //we still set the setter and let us know that is impossible to do the conversion
            setter = (T value) => { Logger.LogWarning(string.Format("Setting Parameter Type '{0}' back to Variable Type '{1}' is not possible.", typeof(T).FriendlyName(), variable.varType.FriendlyName()), "AutoConvert", this); };
            return false;
        }

        //operator
        public static implicit operator BBParameter<T>(T value) {
            return new BBParameter<T> { value = value };
        }

    }
}