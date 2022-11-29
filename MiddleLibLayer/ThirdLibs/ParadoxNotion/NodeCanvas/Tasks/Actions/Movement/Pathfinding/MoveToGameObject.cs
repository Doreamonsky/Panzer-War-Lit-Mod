using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;
using NavMeshAgent = UnityEngine.AI.NavMeshAgent;

namespace NodeCanvas.Tasks.Actions
{

    [Name("Seek (GameObject)")]
    [Category("Movement/Pathfinding")]
    public class MoveToGameObject : ActionTask<NavMeshAgent>
    {

        [RequiredField]
        public BBParameter<GameObject> target;
        public BBParameter<float> speed = 4;
        public BBParameter<float> keepDistance = 0.1f;

        private Vector3? lastRequest;

        protected override string info {
            get { return "Seek " + target; }
        }

        protected override void OnExecute() {
            if ( target.value == null ) { EndAction(false); return; }
            agent.speed = speed.value;
            if ( Vector3.Distance(agent.transform.position, target.value.transform.position) < agent.stoppingDistance + keepDistance.value ) {
                EndAction(true);
                return;
            }
        }

        protected override void OnUpdate() {
            if ( target.value == null ) { EndAction(false); return; }
            var pos = target.value.transform.position;
            if ( lastRequest != pos ) {
                if ( !agent.SetDestination(pos) ) {
                    EndAction(false);
                    return;
                }
            }

            lastRequest = pos;

            if ( !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance + keepDistance.value ) {
                EndAction(true);
            }
        }


        protected override void OnPause() { OnStop(); }
        protected override void OnStop() {
            if ( lastRequest != null && agent.gameObject.activeSelf ) {
                agent.Warp(agent.transform.position);
                agent.ResetPath();
            }
            lastRequest = null;
        }

        public override void OnDrawGizmosSelected() {
            if ( target.value != null ) {
                Gizmos.DrawWireSphere(target.value.transform.position, keepDistance.value);
            }
        }
    }
}