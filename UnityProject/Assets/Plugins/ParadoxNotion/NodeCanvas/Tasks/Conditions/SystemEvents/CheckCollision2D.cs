using NodeCanvas.Framework;
using ParadoxNotion;
using ParadoxNotion.Design;
using UnityEngine;


namespace NodeCanvas.Tasks.Conditions
{

    [Category("System Events")]
    [Name("Check Collision 2D")]
    public class CheckCollision2D_Rigidbody : ConditionTask<Rigidbody2D>
    {

        public CollisionTypes checkType = CollisionTypes.CollisionEnter;
        public bool specifiedTagOnly;
        [TagField]
        public string objectTag = "Untagged";

        [BlackboardOnly]
        public BBParameter<GameObject> saveGameObjectAs;
        [BlackboardOnly]
        public BBParameter<Vector3> saveContactPoint;
        [BlackboardOnly]
        public BBParameter<Vector3> saveContactNormal;

        private bool stay;

        protected override string info {
            get { return checkType.ToString() + ( specifiedTagOnly ? ( " '" + objectTag + "' tag" ) : "" ); }
        }

        protected override bool OnCheck() {
            return checkType == CollisionTypes.CollisionStay ? stay : false;
        }

        protected override void OnEnable() {
            router.onCollisionEnter2D += OnCollisionEnter2D;
            router.onCollisionExit2D += OnCollisionExit2D;
        }

        protected override void OnDisable() {
            router.onCollisionEnter2D -= OnCollisionEnter2D;
            router.onCollisionExit2D -= OnCollisionExit2D;
        }

        void OnCollisionEnter2D(ParadoxNotion.EventData<Collision2D> data) {
            if ( !specifiedTagOnly || data.value.gameObject.CompareTag(objectTag) ) {
                stay = true;
                if ( checkType == CollisionTypes.CollisionEnter || checkType == CollisionTypes.CollisionStay ) {
                    saveGameObjectAs.value = data.value.gameObject;
                    saveContactPoint.value = data.value.contacts[0].point;
                    saveContactNormal.value = data.value.contacts[0].normal;
                    YieldReturn(true);
                }
            }
        }

        void OnCollisionExit2D(ParadoxNotion.EventData<Collision2D> data) {
            if ( !specifiedTagOnly || data.value.gameObject.CompareTag(objectTag) ) {
                stay = false;
                if ( checkType == CollisionTypes.CollisionExit ) {
                    saveGameObjectAs.value = data.value.gameObject;
                    YieldReturn(true);
                }
            }
        }
    }

    ///----------------------------------------------------------------------------------------------

    [Category("System Events")]
    [Name("Check Collision 2D")]
    [DoNotList]
    public class CheckCollision2D : ConditionTask<Collider2D>
    {

        public CollisionTypes checkType = CollisionTypes.CollisionEnter;
        public bool specifiedTagOnly;
        [TagField]
        public string objectTag = "Untagged";

        [BlackboardOnly]
        public BBParameter<GameObject> saveGameObjectAs;
        [BlackboardOnly]
        public BBParameter<Vector3> saveContactPoint;
        [BlackboardOnly]
        public BBParameter<Vector3> saveContactNormal;

        private bool stay;

        protected override string info {
            get { return checkType.ToString() + ( specifiedTagOnly ? ( " '" + objectTag + "' tag" ) : "" ); }
        }

        protected override bool OnCheck() {
            return checkType == CollisionTypes.CollisionStay ? stay : false;
        }

        protected override void OnEnable() {
            router.onCollisionEnter2D += OnCollisionEnter2D;
            router.onCollisionExit2D += OnCollisionExit2D;
        }

        protected override void OnDisable() {
            router.onCollisionEnter2D -= OnCollisionEnter2D;
            router.onCollisionExit2D -= OnCollisionExit2D;
        }

        void OnCollisionEnter2D(ParadoxNotion.EventData<Collision2D> data) {
            if ( !specifiedTagOnly || data.value.gameObject.CompareTag(objectTag) ) {
                stay = true;
                if ( checkType == CollisionTypes.CollisionEnter || checkType == CollisionTypes.CollisionStay ) {
                    saveGameObjectAs.value = data.value.gameObject;
                    saveContactPoint.value = data.value.contacts[0].point;
                    saveContactNormal.value = data.value.contacts[0].normal;
                    YieldReturn(true);
                }
            }
        }

        void OnCollisionExit2D(ParadoxNotion.EventData<Collision2D> data) {
            if ( !specifiedTagOnly || data.value.gameObject.CompareTag(objectTag) ) {
                stay = false;
                if ( checkType == CollisionTypes.CollisionExit ) {
                    saveGameObjectAs.value = data.value.gameObject;
                    YieldReturn(true);
                }
            }
        }
    }
}