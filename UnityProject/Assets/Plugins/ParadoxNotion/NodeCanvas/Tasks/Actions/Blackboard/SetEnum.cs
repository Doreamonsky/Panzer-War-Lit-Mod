using NodeCanvas.Framework;
using NodeCanvas.Framework.Internal;
using ParadoxNotion.Design;


namespace NodeCanvas.Tasks.Actions
{

    [Category("✫ Blackboard")]
    public class SetEnum : ActionTask
    {

        [BlackboardOnly]
        [RequiredField]
        public BBObjectParameter valueA = new BBObjectParameter(typeof(System.Enum));
        public BBObjectParameter valueB = new BBObjectParameter(typeof(System.Enum));

        protected override string info {
            get { return valueA + " = " + valueB; }
        }

        protected override void OnExecute() {
            valueA.value = valueB.value;
            EndAction();
        }


        ///----------------------------------------------------------------------------------------------
        ///---------------------------------------UNITY EDITOR-------------------------------------------
#if UNITY_EDITOR

        protected override void OnTaskInspectorGUI() {
            DrawDefaultInspector();
            if ( valueB.varType != valueA.refType ) { valueB.SetType(valueA.refType); }
        }

#endif
    }
}