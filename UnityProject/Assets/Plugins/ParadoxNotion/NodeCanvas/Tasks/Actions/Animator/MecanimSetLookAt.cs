using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;


namespace NodeCanvas.Tasks.Actions
{

    [Name("Set Look At")]
    [Category("Animator")]
    public class MecanimSetLookAt : ActionTask<Animator>
    {

        public BBParameter<GameObject> targetPosition;
        public BBParameter<float> targetWeight;

        protected override string info {
            get { return "Mec.SetLookAt " + targetPosition; }
        }

        protected override void OnExecute() {
            router.onAnimatorIK += OnAnimatorIK;
        }

        protected override void OnStop() {
            router.onAnimatorIK -= OnAnimatorIK;
        }

        void OnAnimatorIK(ParadoxNotion.EventData<int> msg) {
            agent.SetLookAtPosition(targetPosition.value.transform.position);
            agent.SetLookAtWeight(targetWeight.value);
            EndAction();
        }
    }
}