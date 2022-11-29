using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;


namespace NodeCanvas.BehaviourTrees
{

    [Name("Step Sequencer")]
    [Category("Composites")]
    [Description("In comparison to a normal Sequencer which executes all its children until one fails, Step Sequencer executes its children one-by-one per Step Sequencer execution. The executed child status is returned regardless of Success or Failure.")]
    [ParadoxNotion.Design.Icon("StepIterator")]
    [Color("bf7fff")]
    public class StepIterator : BTComposite
    {

        private int current;

        public override void OnGraphStarted() {
            current = 0;
        }

        protected override Status OnExecute(Component agent, IBlackboard blackboard) {
            current = current % outConnections.Count;
            return outConnections[current].Execute(agent, blackboard);
        }

        protected override void OnReset() {
            current++;
        }
    }
}