using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;


namespace NodeCanvas.Tasks.Conditions
{

    [Category("GameObject")]
    [Description("A combination of line of sight and view angle check")]
    public class CanSeeTarget2D : ConditionTask<Transform>
    {

        [RequiredField]
        public BBParameter<GameObject> target;
        public BBParameter<float> maxDistance = 50;
        public BBParameter<float> awarnessDistance = 0f;
        [SliderField(1, 180)]
        public BBParameter<float> viewAngle = 70f;
        public Vector2 offset;

        private RaycastHit2D hit;

        protected override string info {
            get { return "Can See " + target; }
        }

        protected override bool OnCheck() {

            var t = target.value.transform;
            if ( Vector2.Distance(agent.position, t.position) > maxDistance.value ) {
                return false;
            }

            var hit = Physics2D.Linecast((Vector2)agent.position + offset, (Vector2)t.position + offset);
            if ( hit.collider != t.GetComponent<Collider2D>() ) {
                return false;
            }

            if ( Vector2.Angle((Vector2)t.position - (Vector2)agent.position, agent.right) < viewAngle.value ) {
                return true;
            }

            if ( Vector2.Distance(agent.position, t.position) < awarnessDistance.value ) {
                return true;
            }

            return false;
        }

        public override void OnDrawGizmosSelected() {
            if ( agent != null ) {
                Gizmos.DrawLine((Vector2)agent.position, (Vector2)agent.position + offset);
                Gizmos.DrawLine((Vector2)agent.position + offset, (Vector2)agent.position + offset + ( (Vector2)agent.right * maxDistance.value ));
                Gizmos.DrawWireSphere((Vector2)agent.position + offset + ( (Vector2)agent.right * maxDistance.value ), 0.1f);
                Gizmos.DrawWireSphere((Vector2)agent.position, awarnessDistance.value);
                Gizmos.matrix = Matrix4x4.TRS((Vector2)agent.position + offset, Quaternion.LookRotation(agent.right), Vector3.one);
                Gizmos.DrawFrustum(Vector3.zero, viewAngle.value, 5, 0, 1f);
            }
        }
    }
}