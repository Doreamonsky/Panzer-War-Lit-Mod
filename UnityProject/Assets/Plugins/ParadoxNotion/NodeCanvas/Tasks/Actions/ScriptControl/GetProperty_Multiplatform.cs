using System.Reflection;
using NodeCanvas.Framework;
using NodeCanvas.Framework.Internal;
using ParadoxNotion;
using ParadoxNotion.Design;
using ParadoxNotion.Serialization;
using UnityEngine;
using System.Linq;


namespace NodeCanvas.Tasks.Actions
{

    [Name("Get Property", 8)]
    [Category("✫ Reflected")]
    [Description("Get a property of a script and save it to the blackboard")]
    public class GetProperty_Multiplatform : ActionTask, IReflectedWrapper
    {

        [SerializeField]
        protected SerializedMethodInfo method;
        [SerializeField, BlackboardOnly]
        protected BBObjectParameter returnValue;

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
                return string.Format("{0} = {1}.{2}", returnValue.ToString(), mInfo, targetMethod.Name);
            }
        }

        ISerializedReflectedInfo IReflectedWrapper.GetSerializedInfo() { return method; }

        public override void OnValidate(ITaskSystem ownerSystem) {
            if ( method != null && method.HasChanged() ) { SetMethod(method); }
        }

        protected override string OnInit() {
            if ( method == null ) { return "No Property selected"; }
            if ( targetMethod == null ) { return string.Format("Missing Property '{0}'", method.AsString()); }
            return null;
        }

        protected override void OnExecute() {
            returnValue.value = targetMethod.Invoke(targetMethod.IsStatic ? null : agent, null);
            EndAction();
        }

        void SetMethod(MethodInfo method) {
            if ( method != null ) {
                this.method = new SerializedMethodInfo(method);
                this.returnValue.SetType(method.ReturnType);
            }
        }


        ///----------------------------------------------------------------------------------------------
        ///---------------------------------------UNITY EDITOR-------------------------------------------
#if UNITY_EDITOR

        protected override void OnTaskInspectorGUI() {

            if ( !Application.isPlaying && GUILayout.Button("Select Property") ) {
                var menu = new UnityEditor.GenericMenu();
                if ( agent != null ) {
                    foreach ( var comp in agent.GetComponents(typeof(Component)).Where(c => c.hideFlags != HideFlags.HideInInspector) ) {
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
                UnityEditor.EditorGUILayout.LabelField("Property Type", targetMethod.ReturnType.FriendlyName());
                UnityEditor.EditorGUILayout.HelpBox(DocsByReflection.GetMemberSummary(targetMethod), UnityEditor.MessageType.None);
                GUILayout.EndVertical();

                NodeCanvas.Editor.BBParameterEditor.ParameterField("Save As", returnValue, true);
            }
        }
#endif

    }
}
