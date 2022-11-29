using ParadoxNotion.Design;

namespace NodeCanvas.StateMachines
{

    [Description("This node has no functionality and you can use this for organization.\nOutgoing transitions are immediately evaluated in the same frame that this node is entered, in comparison to an empty Action State which always yields one frame even if empty.")]
    [Color("6ebbff")]
    [Name("Pass")]
    public class EmptyState : FSMState
    {

        public override string name {
            get { return base.name.ToUpper(); }
        }

        protected override void OnEnter() {
            Finish();
            CheckTransitions();
        }
    }
}