using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NodeCanvas.Framework;
using NodeCanvas.Framework.Internal;
using ParadoxNotion;
using ParadoxNotion.Design;
using ParadoxNotion.Serialization;
using UnityEngine;


namespace NodeCanvas.Tasks.Conditions
{

    [Name("Check Function", 10)]
    [Category("✫ Reflected")]
    [Description("Call a function on a component and return whether or not the return value is equal to the check value")]
    public class CheckFunction_Multiplatform : ConditionTask, IReflectedWrapper
    {
        [SerializeField]
        protected SerializedMethodInfo method;
        [SerializeField]
        protected List<BBObjectParameter> parameters = new List<BBObjectParameter>();
        [SerializeField]
        protected CompareMethod comparison;
        [SerializeField, BlackboardOnly]
        protected BBObjectParameter checkValue;

        private object[] args;
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
                var paramInfo = "";
                for ( var i = 0; i < parameters.Count; i++ ) {
                    paramInfo += ( i != 0 ? ", " : "" ) + parameters[i].ToString();
                }
                var mInfo = targetMethod.IsStatic ? targetMethod.RTReflectedOrDeclaredType().FriendlyName() : agentInfo;
                return string.Format("{0}.{1}({2}){3}", mInfo, targetMethod.Name, paramInfo, OperationTools.GetCompareString(comparison) + checkValue);
            }
        }

        ISerializedReflectedInfo IReflectedWrapper.GetSerializedInfo() { return method; }

        public override void OnValidate(ITaskSystem ownerSystem) {
            if ( method != null && method.HasChanged() ) { SetMethod(method); }
        }

        //store the method info on agent set for performance
        protected override string OnInit() {
            if ( method == null ) { return "No Method Selected"; }
            if ( targetMethod == null ) { return method.AsString(); }

            if ( args == null ) {
                var methodParameters = targetMethod.GetParameters();
                args = new object[methodParameters.Length];
                parameterIsByRef = new bool[methodParameters.Length];
                for ( var i = 0; i < parameters.Count; i++ ) {
                    parameterIsByRef[i] = methodParameters[i].ParameterType.IsByRef;
                }
            }

            return null;
        }

        //do it by invoking method
        protected override bool OnCheck() {

            for ( var i = 0; i < parameters.Count; i++ ) {
                args[i] = parameters[i].value;
            }

            var instance = targetMethod.IsStatic ? null : agent;
            bool result;
            if ( checkValue.varType == typeof(float) ) {
                result = OperationTools.Compare((float)targetMethod.Invoke(instance, args), (float)checkValue.value, comparison, 0.05f);
            } else if ( checkValue.varType == typeof(int) ) {
                result = OperationTools.Compare((int)targetMethod.Invoke(instance, args), (int)checkValue.value, comparison);
            } else {
                result = ObjectUtils.AnyEquals(targetMethod.Invoke(instance, args), checkValue.value);
            }

            for ( var i = 0; i < parameters.Count; i++ ) {
                if ( parameterIsByRef[i] ) {
                    parameters[i].value = args[i];
                }
            }

            return result;
        }


        void SetMethod(MethodInfo method) {
            if ( method == null ) {
                return;
            }
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

            this.checkValue = new BBObjectParameter(method.ReturnType) { bb = blackboard };
            comparison = CompareMethod.EqualTo;
        }


        ///----------------------------------------------------------------------------------------------
        ///---------------------------------------UNITY EDITOR-------------------------------------------
#if UNITY_EDITOR

        protected override void OnTaskInspectorGUI() {

            if ( !Application.isPlaying && GUILayout.Button("Select Method") ) {
                var menu = new UnityEditor.GenericMenu();
                if ( agent != null ) {
                    foreach ( var comp in agent.GetComponents(typeof(Component)).Where(c => c.hideFlags == 0) ) {
                        menu = EditorUtils.GetInstanceMethodSelectionMenu(comp.GetType(), typeof(object), typeof(object), SetMethod, 10, false, true, menu);
                    }
                    menu.AddSeparator("/");
                }
                foreach ( var t in TypePrefs.GetPreferedTypesList(typeof(object)) ) {
                    menu = EditorUtils.GetStaticMethodSelectionMenu(t, typeof(object), typeof(object), SetMethod, 10, false, true, menu);
                    if ( typeof(UnityEngine.Component).IsAssignableFrom(t) ) {
                        menu = EditorUtils.GetInstanceMethodSelectionMenu(t, typeof(object), typeof(object), SetMethod, 10, false, true, menu);
                    }
                }
                menu.ShowAsBrowser("Select Method", this.GetType());
                Event.current.Use();
            }

            if ( targetMethod != null ) {
                GUILayout.BeginVertical("box");
                UnityEditor.EditorGUILayout.LabelField("Type", targetMethod.RTReflectedOrDeclaredType().FriendlyName());
                UnityEditor.EditorGUILayout.LabelField("Method", targetMethod.Name);
                UnityEditor.EditorGUILayout.HelpBox(DocsByReflection.GetMemberSummary(targetMethod), UnityEditor.MessageType.None);
                GUILayout.EndVertical();

                var paramNames = targetMethod.GetParameters().Select(p => p.Name.SplitCamelCase()).ToArray();
                for ( var i = 0; i < paramNames.Length; i++ ) {
                    NodeCanvas.Editor.BBParameterEditor.ParameterField(paramNames[i], parameters[i]);
                }

                GUI.enabled = checkValue.varType == typeof(float) || checkValue.varType == typeof(int);
                comparison = (CompareMethod)UnityEditor.EditorGUILayout.EnumPopup("Comparison", comparison);
                GUI.enabled = true;
                NodeCanvas.Editor.BBParameterEditor.ParameterField("Check Value", checkValue);
            }
        }

#endif
    }
}