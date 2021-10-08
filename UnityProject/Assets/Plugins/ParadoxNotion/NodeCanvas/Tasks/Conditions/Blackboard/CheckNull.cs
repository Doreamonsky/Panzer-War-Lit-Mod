using NodeCanvas.Framework;
using ParadoxNotion.Design;


namespace NodeCanvas.Tasks.Conditions
{

    [Category("✫ Blackboard")]
    [Description("Check whether or not a variable is null")]
    public class CheckNull : ConditionTask
    {

        [BlackboardOnly]
        public BBParameter<System.Object> variable;

        protected override string info {
            get { return variable + " == null"; }
        }

        protected override bool OnCheck() {
            return variable.value == null;
        }
    }
}