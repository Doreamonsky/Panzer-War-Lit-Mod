using System.Reflection;
using NodeCanvas.Framework;
using NodeCanvas.Framework.Internal;
using ParadoxNotion;
using ParadoxNotion.Design;
using ParadoxNotion.Serialization;
using ParadoxNotion.Serialization.FullSerializer;
using UnityEngine;
using System.Linq;

namespace NodeCanvas.Tasks.Conditions
{

    //previous versions
    class CheckField_0
    {
        [SerializeField] public BBParameter checkValue = null;
        [SerializeField] public System.Type targetType = null;
        [SerializeField] public string fieldName = null;
    }

    ///----------------------------------------------------------------------------------------------

    [Name("Check Field", 8)]
    [Category("✫ Reflected")]
    [Description("Check a field on a script and return if it's equal or not to a value")]
    [fsMigrateVersions(typeof(CheckField_0))]
    public class CheckField : ConditionTask, IReflectedWrapper, IMigratable<CheckField_0>
    {
        ///----------------------------------------------------------------------------------------------
        void IMigratable<CheckField_0>.Migrate(CheckField_0 model) {
            try { this.field = new SerializedFieldInfo(model.targetType?.RTGetField(model.fieldName)); }
            finally { this.checkValue = new BBObjectParameter(model.checkValue); }
        }
        ///----------------------------------------------------------------------------------------------

        [SerializeField] protected BBObjectParameter checkValue;
        [SerializeField] protected CompareMethod comparison;
        [SerializeField] protected SerializedFieldInfo field;

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
                return string.Format("{0}.{1}{2}{3}", mInfo, targetField.Name, OperationTools.GetCompareString(comparison), checkValue);
            }
        }

        ISerializedReflectedInfo IReflectedWrapper.GetSerializedInfo() { return field; }

        //store the field info on agent set for performance
        protected override string OnInit() {
            if ( field == null ) { return "No Field Selected"; }
            if ( targetField == null ) { return field.AsString().FormatError(); }
            return null;
        }

        //do it by invoking field
        protected override bool OnCheck() {
            if ( checkValue.varType == typeof(float) ) {
                return OperationTools.Compare((float)targetField.GetValue(agent), (float)checkValue.value, comparison, 0.05f);
            }

            if ( checkValue.varType == typeof(int) ) {
                return OperationTools.Compare((int)targetField.GetValue(agent), (int)checkValue.value, comparison);
            }

            return ObjectUtils.AnyEquals(targetField.GetValue(agent), checkValue.value);
        }

        void SetTargetField(FieldInfo newField) {
            if ( newField != null ) {
                field = new SerializedFieldInfo(newField);
                checkValue.SetType(newField.FieldType);
                comparison = CompareMethod.EqualTo;
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

                GUI.enabled = checkValue.varType == typeof(float) || checkValue.varType == typeof(int);
                comparison = (CompareMethod)UnityEditor.EditorGUILayout.EnumPopup("Comparison", comparison);
                GUI.enabled = true;
                NodeCanvas.Editor.BBParameterEditor.ParameterField("Value", checkValue);
            }
        }
#endif

    }
}