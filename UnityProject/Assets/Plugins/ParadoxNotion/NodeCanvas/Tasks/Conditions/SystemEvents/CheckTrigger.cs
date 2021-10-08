using NodeCanvas.Framework;
using ParadoxNotion;
using ParadoxNotion.Design;
using UnityEngine;


namespace NodeCanvas.Tasks.Conditions
{

    [Category("System Events")]
    [Description("The agent is type of Transform so that Triggers can either work with a Collider or a Rigidbody attached.")]
    public class CheckTrigger_Transform : ConditionTask<Transform>
    {

        public TriggerTypes checkType = TriggerTypes.TriggerEnter;
        public bool specifiedTagOnly;
        [TagField, ShowIf("specifiedTagOnly", 1)]
        public string objectTag = "Untagged";
        [BlackboardOnly]
        public BBParameter<GameObject> saveGameObjectAs;

        private bool stay;

        protected override string info {
            get { return checkType.ToString() + ( specifiedTagOnly ? ( " '" + objectTag + "' tag" ) : "" ); }
        }

        protected override bool OnCheck() {
            if ( checkType == TriggerTypes.TriggerStay ) { return stay; }
            return false;
        }

        protected override void OnEnable() {
            router.onTriggerEnter += OnTriggerEnter;
            router.onTriggerExit += OnTriggerExit;
        }

        protected override void OnDisable() {
            router.onTriggerEnter -= OnTriggerEnter;
            router.onTriggerExit -= OnTriggerExit;
        }

        public void OnTriggerEnter(ParadoxNotion.EventData<Collider> data) {
            if ( !specifiedTagOnly || data.value.gameObject.CompareTag(objectTag) ) {
                stay = true;
                if ( checkType == TriggerTypes.TriggerEnter || checkType == TriggerTypes.TriggerStay ) {
                    saveGameObjectAs.value = data.value.gameObject;
                    YieldReturn(true);
                }
            }
        }

        public void OnTriggerExit(ParadoxNotion.EventData<Collider> data) {
            if ( !specifiedTagOnly || data.value.gameObject.CompareTag(objectTag) ) {
                stay = false;
                if ( checkType == TriggerTypes.TriggerExit ) {
                    saveGameObjectAs.value = data.value.gameObject;
                    YieldReturn(true);
                }
            }
        }
    }

    ///----------------------------------------------------------------------------------------------

    [Category("System Events")]
    [DoNotList]
    public class CheckTrigger : ConditionTask<Collider>
    {

        public TriggerTypes checkType = TriggerTypes.TriggerEnter;
        public bool specifiedTagOnly;
        [TagField, ShowIf("specifiedTagOnly", 1)]
        public string objectTag = "Untagged";
        [BlackboardOnly]
        public BBParameter<GameObject> saveGameObjectAs;

        private bool stay;

        protected override string info {
            get { return checkType.ToString() + ( specifiedTagOnly ? ( " '" + objectTag + "' tag" ) : "" ); }
        }

        protected override bool OnCheck() {
            if ( checkType == TriggerTypes.TriggerStay ) { return stay; }
            return false;
        }

        protected override void OnEnable() {
            router.onTriggerEnter += OnTriggerEnter;
            router.onTriggerExit += OnTriggerExit;
        }

        protected override void OnDisable() {
            router.onTriggerEnter -= OnTriggerEnter;
            router.onTriggerExit -= OnTriggerExit;
        }

        public void OnTriggerEnter(ParadoxNotion.EventData<Collider> data) {
            if ( !specifiedTagOnly || data.value.gameObject.CompareTag(objectTag) ) {
                stay = true;
                if ( checkType == TriggerTypes.TriggerEnter || checkType == TriggerTypes.TriggerStay ) {
                    saveGameObjectAs.value = data.value.gameObject;
                    YieldReturn(true);
                }
            }
        }

        public void OnTriggerExit(ParadoxNotion.EventData<Collider> data) {
            if ( !specifiedTagOnly || data.value.gameObject.CompareTag(objectTag) ) {
                stay = false;
                if ( checkType == TriggerTypes.TriggerExit ) {
                    saveGameObjectAs.value = data.value.gameObject;
                    YieldReturn(true);
                }
            }
        }
    }
}