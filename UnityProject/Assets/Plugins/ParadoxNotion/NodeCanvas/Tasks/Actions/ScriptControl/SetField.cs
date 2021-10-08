using System.Reflection;
using NodeCanvas.Framework;
using NodeCanvas.Framework.Internal;
using ParadoxNotion;
using ParadoxNotion.Design;
using UnityEngine;
using System.Linq;
using ParadoxNotion.Serialization;
using ParadoxNotion.Serialization.FullSerializer;

namespace NodeCanvas.Tasks.Actions
{

    //previous versions
    class SetField_0
    {
        [SerializeField] public System.Type targetType = null;
        [SerializeField] public string fieldName = null;
    }

    ///----------------------------------------------------------------------------------------------

    [Category("✫ Reflected")]
    [Description("Set a variable on a script")]
    [Name("Set Field", 5)]
    [fsMigrateVersions(typeof(SetField_0))]
    public class SetField : ActionTask, IReflectedWrapper, IMigratable<SetField_0>
    {

        ///----------------------------------------------------------------------------------------------
        void IMigratable<SetField_0>.Migrate(SetField_0 model) {
            this.field = new SerializedFieldInfo(model.targetType?.RTGetField(model.fieldName));
        }
        ///----------------------------------------------------------------------------------------------

        [SerializeField]
        protected SerializedFieldInfo field;
        [SerializeField]
        protected BBObjectParameter setValue;

        private FieldInfo targetField => field;

        public override System.Type agentType {
            get
            {
                if ( targetField == null ) { return typeof(Transform); }
                return targetField.IsStatic ? null : targetField.RTReflectedOrDeclaredType();
            }
        }

        protected override string info {
            get
            {
                if ( field == null ) { return "No Field Selected"; }
                if ( targetField == null ) { return field.AsString().FormatError(); }
                var mInfo = targetField.IsStatic ? targetField.RTReflectedOrDeclaredType().FriendlyName() : agentInfo;
                return string.Format("{0}.{1} = {2}", mInfo, targetField.Name, setValue);
            }
        }

        ISerializedReflectedInfo IReflectedWrapper.GetSerializedInfo() { return field; }

        protected override string OnInit() {
            if ( field == null ) { return "No Field Selected"; }
            if ( targetField == null ) { return field.AsString().FormatError(); }
            return null;
        }

        protected override void OnExecute() {
            targetField.SetValue(targetField.IsStatic ? null : agent, setValue.value);
            EndAction();
        }

        void SetTargetField(FieldInfo newField) {
            if ( newField != null ) {
                field = new SerializedFieldInfo(newField);
                setValue.SetType(newField.FieldType);
            }
        }

        ////////////////////////////////////////
        ///////////GUI AND EDITOR STUFF/////////
        ////////////////////////////////////////
#if UNITY_EDITOR

        protected override void OnTaskInspectorGUI() {

            if ( !Application.isPlaying && GUILayout.Button("Select Field") ) {
                var menu = new UnityEditor.GenericMenu();
                if ( agent != null ) {
                    foreach ( var comp in agent.GetComponents(typeof(Component)).Where(c => c.hideFlags != HideFlags.HideInInspector) ) {
                        menu = EditorUtils.GetInstanceFieldSelectionMenu(comp.GetType(), typeof(object), SetTargetField, menu);
                    }
                    menu.AddSeparator("/");
                }
                foreach ( var t in TypePrefs.GetPreferedTypesList(typeof(object)) ) {
                    menu = EditorUtils.GetStaticFieldSelectionMenu(t, typeof(object), SetTargetField, menu);
                    if ( typeof(Component).IsAssignableFrom(t) ) {
                        menu = EditorUtils.GetInstanceFieldSelectionMenu(t, typeof(object), SetTargetField, menu);
                    }
                }
                menu.ShowAsBrowser("Select Field", this.GetType());
                Event.current.Use();
            }

            if ( targetField != null ) {
                GUILayout.BeginVertical("box");
                UnityEditor.EditorGUILayout.LabelField("Type", targetField.RTReflectedOrDeclaredType().FriendlyName());
                UnityEditor.EditorGUILayout.LabelField("Field", targetField.Name);
                UnityEditor.EditorGUILayout.LabelField("Field Type", targetField.FieldType.FriendlyName());
                UnityEditor.EditorGUILayout.HelpBox(DocsByReflection.GetMemberSummary(targetField), UnityEditor.MessageType.None);
                GUILayout.EndVertical();
                NodeCanvas.Editor.BBParameterEditor.ParameterField("Set Value", setValue);
            }
        }
#endif

    }
}