using NodeCanvas.Framework;
using ParadoxNotion;
using ParadoxNotion.Design;
using NodeCanvas.StateMachines;

namespace NodeCanvas.Tasks.Conditions
{

    [Category("✫ Utility")]
    [Description("Check the parent state status. This condition is only meant to be used along with an FSM system.")]
    public class CheckStateStatus : ConditionTask
    {
        public CompactStatus status = CompactStatus.Success;

        protected override string info {
            get { return string.Format("State == {0}", status); }
        }

        protected override bool OnCheck() {
            var fsm = ownerSystem as FSM;
            if ( fsm != null ) {
                var state = fsm.currentState;
                return (int)state.status == (int)status;
            }
            return false;
        }
    }
}