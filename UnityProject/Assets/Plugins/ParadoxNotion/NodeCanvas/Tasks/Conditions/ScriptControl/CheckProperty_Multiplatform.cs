using System.Reflection;
using NodeCanvas.Framework;
using NodeCanvas.Framework.Internal;
using ParadoxNotion;
using ParadoxNotion.Design;
using ParadoxNotion.Serialization;
using UnityEngine;
using System.Linq;

namespace NodeCanvas.Tasks.Conditions
{

    [Name("Check Property", 9)]
    [Category("✫ Reflected")]
    [Description("Check a property on a script and return if it's equal or not to the check value")]
    public class CheckProperty_Multiplatform : ConditionTask, IReflectedWrapper
    {

        [SerializeField]
        protected SerializedMethodInfo method;
        [SerializeField]
        protected BBObjectParameter checkValue;
        [SerializeField]
        protected CompareMethod comparison;

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
                if ( method == null ) { return "No Property Selected"; }
                if ( targetMethod == null ) { return method.AsString().FormatError(); }
                var mInfo = targetMethod.IsStatic ? targetMethod.RTReflectedOrDeclaredType().FriendlyName() : agentInfo;
                return string.Format("{0}.{1}{2}", mInfo, targetMethod.Name, OperationTools.GetCompareString(comparison) + checkValue.ToString());
            }
        }

        ISerializedReflectedInfo IReflectedWrapper.GetSerializedInfo() { return method; }

        public override void OnValidate(ITaskSystem ownerSystem) {
            if ( method != null && method.HasChanged() ) { SetMethod(method); }
        }

        //store the method info on agent set for performance
        protected override string OnInit() {
            if ( method == null ) { return "No Property Selected"; }
            if ( targetMethod == null ) { return method.AsString(); }
            return null;
        }

        //do it by invoking method
        protected override bool OnCheck() {
            var instance = targetMethod.IsStatic ? null : agent;
            if ( checkValue.varType == typeof(float) ) {
                return OperationTools.Compare((float)targetMethod.Invoke(instance, null), (float)checkValue.value, comparison, 0.05f);
            }
            if ( checkValue.varType == typeof(int) ) {
                return OperationTools.Compare((int)targetMethod.Invoke(instance, null), (int)checkValue.value, comparison);
            }
            return ObjectUtils.AnyEquals(targetMethod.Invoke(instance, null), checkValue.value);
        }

        void SetMethod(MethodInfo method) {
            if ( method != null ) {
                this.method = new SerializedMethodInfo(method);
                this.checkValue.SetType(method.ReturnType);
                comparison = CompareMethod.EqualTo;
            }
        }


        ///----------------------------------------------------------------------------------------------
        ///---------------------------------------UNITY EDITOR-------------------------------------------
#if UNITY_EDITOR

        protected override void OnTaskInspectorGUI() {

            if ( !Application.isPlaying && GUILayout.Button("Select Property") ) {
                var menu = new UnityEditor.GenericMenu();
                if ( agent != null ) {
                    foreach ( var comp in agent.GetComponents(typeof(Component)).Where(c => c.hideFlags == 0) ) {
                        menu = EditorUtils.GetInstanceMethodSelectionMenu(comp.GetType(), typeof(object), typeof(object), SetMethod, 0, true, true, menu);
                    }
                    menu.AddSeparator("/");
                }
                foreach ( var t in TypePrefs.GetPreferedTypesList(typeof(object)) ) {
                    menu = EditorUtils.GetStaticMethodSelectionMenu(t, typeof(object), typeof(object), SetMethod, 0, true, true, menu);
                    if ( typeof(UnityEngine.Component).IsAssignableFrom(t) ) {
                        menu = EditorUtils.GetInstanceMethodSelectionMenu(t, typeof(object), typeof(object), SetMethod, 0, true, true, menu);
                    }
                }
                menu.ShowAsBrowser("Select Property", this.GetType());
                Event.current.Use();
            }

            if ( targetMethod != null ) {
                GUILayout.BeginVertical("box");
                UnityEditor.EditorGUILayout.LabelField("Type", targetMethod.RTReflectedOrDeclaredType().FriendlyName());
                UnityEditor.EditorGUILayout.LabelField("Property", targetMethod.Name);
                UnityEditor.EditorGUILayout.HelpBox(DocsByReflection.GetMemberSummary(targetMethod), UnityEditor.MessageType.None);
                GUILayout.EndVertical();

                GUI.enabled = checkValue.varType == typeof(float) || checkValue.varType == typeof(int);
                comparison = (CompareMethod)UnityEditor.EditorGUILayout.EnumPopup("Comparison", comparison);
                GUI.enabled = true;
                NodeCanvas.Editor.BBParameterEditor.ParameterField("Value", checkValue);
            }
        }

#endif
    }
}