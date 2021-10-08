using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;


namespace NodeCanvas.Tasks.Conditions
{

    [Name("Target In View Angle 2D")]
    [Category("GameObject")]
    [Description("Checks whether the target is in the view angle of the agent")]
    public class IsInFront2D : ConditionTask<Transform>
    {

        [RequiredField]
        public BBParameter<GameObject> checkTarget;
        [SliderField(1, 180)]
        public BBParameter<float> viewAngle = 70f;

        protected override string info {
            get { return checkTarget + " in view angle"; }
        }

        protected override bool OnCheck() {
            return Vector2.Angle((Vector2)checkTarget.value.transform.position - (Vector2)agent.position, agent.right) < viewAngle.value;
        }

        public override void OnDrawGizmosSelected() {
            if ( agent != null ) {
                Gizmos.matrix = Matrix4x4.TRS((Vector2)agent.position, agent.rotation, Vector3.one);
                Gizmos.DrawFrustum(Vector3.zero, viewAngle.value, 5, 0, 0f);
            }
        }

    }
}