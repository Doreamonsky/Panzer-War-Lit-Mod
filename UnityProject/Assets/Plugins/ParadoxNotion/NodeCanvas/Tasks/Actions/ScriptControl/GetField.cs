using System.Reflection;
using NodeCanvas.Framework;
using NodeCanvas.Framework.Internal;
using ParadoxNotion;
using ParadoxNotion.Design;
using ParadoxNotion.Serialization;
using ParadoxNotion.Serialization.FullSerializer;
using UnityEngine;
using System.Linq;

namespace NodeCanvas.Tasks.Actions
{

    //previous versions
    class GetField_0
    {
        [SerializeField] public System.Type targetType = null;
        [SerializeField] public string fieldName = null;
    }

    ///----------------------------------------------------------------------------------------------

    [Category("✫ Reflected")]
    [Description("Get a variable of a script and save it to the blackboard")]
    [Name("Get Field", 6)]
    [fsMigrateVersions(typeof(GetField_0))]
    public class GetField : ActionTask, IReflectedWrapper, IMigratable<GetField_0>
    {

        ///----------------------------------------------------------------------------------------------
        void IMigratable<GetField_0>.Migrate(GetField_0 model) {
            this.field = new SerializedFieldInfo(model.targetType?.RTGetField(model.fieldName));
        }
        ///----------------------------------------------------------------------------------------------

        [SerializeField]
        protected SerializedFieldInfo field;
        [SerializeField, BlackboardOnly]
        protected BBObjectParameter saveAs;

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
                return string.Format("{0} = {1}.{2}", saveAs.ToString(), mInfo, targetField.Name);
            }
        }

        ISerializedReflectedInfo IReflectedWrapper.GetSerializedInfo() { return field; }

        protected override string OnInit() {
            if ( field == null ) { return "No Field Selected"; }
            if ( targetField == null ) { return field.AsString().FormatError(); }
            return null;
        }

        protected override void OnExecute() {
            saveAs.value = targetField.GetValue(targetField.IsStatic ? null : agent);
            EndAction();
        }

        void SetTargetField(FieldInfo newField) {
            if ( newField != null ) {
                field = new SerializedFieldInfo(newField);
                saveAs.SetType(newField.FieldType);
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
                NodeCanvas.Editor.BBParameterEditor.ParameterField("Save As", saveAs, true);
            }
        }
#endif

    }
}
