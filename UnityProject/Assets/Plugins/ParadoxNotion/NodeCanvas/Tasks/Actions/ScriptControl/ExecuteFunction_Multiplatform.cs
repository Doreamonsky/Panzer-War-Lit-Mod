using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NodeCanvas.Framework;
using NodeCanvas.Framework.Internal;
using ParadoxNotion;
using ParadoxNotion.Design;
using ParadoxNotion.Serialization;
using UnityEngine;


namespace NodeCanvas.Tasks.Actions
{

    [Name("Execute Function", 10)]
    [Category("✫ Reflected")]
    [Description("Execute a function on a script and save the return if any.\nIf function is an IEnumerator it will execute as a coroutine.")]
    public class ExecuteFunction_Multiplatform : ActionTask, IReflectedWrapper
    {

        [SerializeField]
        protected SerializedMethodInfo method;
        [SerializeField]
        protected List<BBObjectParameter> parameters = new List<BBObjectParameter>();
        [SerializeField, BlackboardOnly]
        protected BBObjectParameter returnValue;

        private object[] args;
        private bool routineRunning;
        private bool[] parameterIsByRef;

        private MethodInfo targetMethod => method;

        public override System.Type agentType {
            get
            {
                if ( targetMethod == null ) { return typeof(Transform); }
                return targetMethod.IsStatic ? null : targetMethod.RTReflectedOrDeclaredType();
            }
        }

        protected override string info {
            get
            {
                if ( method == null ) { return "No Method Selected"; }
                if ( targetMethod == null ) { return method.AsString().FormatError(); }
                var returnInfo = targetMethod.ReturnType == typeof(void) || targetMethod.ReturnType == typeof(IEnumerator) ? "" : returnValue.ToString() + " = ";
                var paramInfo = "";
                for ( var i = 0; i < parameters.Count; i++ ) { paramInfo += ( i != 0 ? ", " : "" ) + parameters[i].ToString(); }
                var mInfo = targetMethod.IsStatic ? targetMethod.RTReflectedOrDeclaredType().FriendlyName() : agentInfo;
                return string.Format("{0}{1}.{2}({3})", returnInfo, mInfo, targetMethod.Name, paramInfo);
            }
        }

        ISerializedReflectedInfo IReflectedWrapper.GetSerializedInfo() { return method; }

        public override void OnValidate(ITaskSystem ownerSystem) {
            if ( method != null && method.HasChanged() ) { SetMethod(method); }
        }

        //store the method info on init
        protected override string OnInit() {
            if ( method == null ) { return "No Method selected"; }
            if ( targetMethod == null ) { return string.Format("Missing Method '{0}'", method.AsString()); }

            if ( args == null ) {
                var methodParameters = targetMethod.GetParameters();
                args = new object[methodParameters.Length];
                parameterIsByRef = new bool[methodParameters.Length];
                for ( var i = 0; i < methodParameters.Length; i++ ) {
                    parameterIsByRef[i] = methodParameters[i].ParameterType.IsByRef;
                }
            }

            return null;
        }


        //do it by calling delegate or invoking method
        protected override void OnExecute() {

            for ( var i = 0; i < parameters.Count; i++ ) {
                args[i] = parameters[i].value;
            }

            var instance = targetMethod.IsStatic ? null : agent;
            if ( targetMethod.ReturnType == typeof(IEnumerator) ) {
                StartCoroutine(InternalCoroutine((IEnumerator)targetMethod.Invoke(instance, args)));
                return;
            }

            returnValue.value = targetMethod.Invoke(instance, args);

            for ( var i = 0; i < parameters.Count; i++ ) {
                if ( parameterIsByRef[i] ) {
                    parameters[i].value = args[i];
                }
            }

            EndAction();
        }

        protected override void OnStop() {
            routineRunning = false;
        }

        IEnumerator InternalCoroutine(IEnumerator routine) {
            routineRunning = true;
            while ( routineRunning && routine.MoveNext() ) {
                if ( routineRunning == false ) {
                    yield break;
                }
                yield return routine.Current;
            }

            if ( routineRunning ) {
                EndAction();
            }
        }

        void SetMethod(MethodInfo method) {
            if ( method == null ) { return; }
            this.method = new SerializedMethodInfo(method);
            this.parameters.Clear();
            var methodParameters = method.GetParameters();
            for ( var i = 0; i < methodParameters.Length; i++ ) {
                var p = methodParameters[i];
                var pType = p.ParameterType;
                var newParam = new BBObjectParameter(pType.IsByRef ? pType.GetElementType() : pType) { bb = blackboard };
                if ( p.IsOptional ) { newParam.value = p.DefaultValue; }
                parameters.Add(newParam);
            }

            if ( method.ReturnType != typeof(void) && targetMethod.ReturnType != typeof(IEnumerator) ) {
                this.returnValue = new BBObjectParameter(method.ReturnType) { bb = blackboard };
            } else { this.returnValue = null; }
        }


        ///----------------------------------------------------------------------------------------------
        ///---------------------------------------UNITY EDITOR-------------------------------------------
#if UNITY_EDITOR

        protected override void OnTaskInspectorGUI() {

            if ( !Application.isPlaying && GUILayout.Button("Select Method") ) {
                var menu = new UnityEditor.GenericMenu();
                if ( agent != null ) {
                    foreach ( var comp in agent.GetComponents(typeof(Component)).Where(c => c.hideFlags != HideFlags.HideInInspector) ) {
                        menu = EditorUtils.GetInstanceMethodSelectionMenu(comp.GetType(), typeof(object), typeof(object), SetMethod, 10, false, false, menu);
                    }
                    menu.AddSeparator("/");
                }
                foreach ( var t in TypePrefs.GetPreferedTypesList(typeof(object)) ) {
                    menu = EditorUtils.GetStaticMethodSelectionMenu(t, typeof(object), typeof(object), SetMethod, 10, false, false, menu);
                    if ( typeof(UnityEngine.Component).IsAssignableFrom(t) ) {
                        menu = EditorUtils.GetInstanceMethodSelectionMenu(t, typeof(object), typeof(object), SetMethod, 10, false, false, menu);
                    }
                }
                menu.ShowAsBrowser("Select Method", this.GetType());
                Event.current.Use();
            }


            if ( targetMethod != null ) {
                GUILayout.BeginVertical("box");
                UnityEditor.EditorGUILayout.LabelField("Type", targetMethod.RTReflectedOrDeclaredType().FriendlyName());
                UnityEditor.EditorGUILayout.LabelField("Method", targetMethod.Name);
                UnityEditor.EditorGUILayout.LabelField("Returns", targetMethod.ReturnType.FriendlyName());
                UnityEditor.EditorGUILayout.HelpBox(DocsByReflection.GetMemberSummary(targetMethod), UnityEditor.MessageType.None);

                if ( targetMethod.ReturnType == typeof(IEnumerator) ) {
                    GUILayout.Label("<b>This will execute as a Coroutine!</b>");
                }

                GUILayout.EndVertical();

                var paramNames = targetMethod.GetParameters().Select(p => p.Name.SplitCamelCase()).ToArray();
                for ( var i = 0; i < paramNames.Length; i++ ) {
                    NodeCanvas.Editor.BBParameterEditor.ParameterField(paramNames[i], parameters[i]);
                }

                if ( targetMethod.ReturnType != typeof(void) && targetMethod.ReturnType != typeof(IEnumerator) ) {
                    NodeCanvas.Editor.BBParameterEditor.ParameterField("Save Return Value", returnValue, true);
                }
            }
        }

#endif
    }
}