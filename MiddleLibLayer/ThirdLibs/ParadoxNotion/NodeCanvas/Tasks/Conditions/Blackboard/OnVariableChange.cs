using NodeCanvas.Framework;
using NodeCanvas.Framework.Internal;
using ParadoxNotion.Design;

namespace NodeCanvas.Tasks.Conditions
{
    [Name("On Variable Changed")]
    [Category("✫ Blackboard")]
    public class BBVariableChanged : ConditionTask
    {

        [BlackboardOnly] public BBObjectParameter targetVariable;

        protected override string info {
            get { return targetVariable + " Changed."; }
        }

        protected override string OnInit() {
            if ( targetVariable.isNone ) {
                return "Blackboard Variable not set.";
            }
            return null;
        }

        protected override void OnEnable() {
            targetVariable.varRef.onValueChanged += OnValueChanged;
        }

        protected override void OnDisable() {
            targetVariable.varRef.onValueChanged -= OnValueChanged;
        }

        protected override bool OnCheck() { return false; }

        private void OnValueChanged(object varValue) {
            YieldReturn(true);
        }
    }
}