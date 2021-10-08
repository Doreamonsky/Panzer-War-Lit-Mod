using NodeCanvas.Framework;
using ParadoxNotion;
using ParadoxNotion.Design;
using UnityEngine;


namespace NodeCanvas.Tasks.Conditions
{

    [Category("System Events")]
    [Name("Check Trigger 2D")]
    [Description("The agent is type of Transform so that Triggers can either work with a Collider or a Rigidbody attached.")]
    public class CheckTrigger2D_Transform : ConditionTask<Transform>
    {

        public TriggerTypes CheckType = TriggerTypes.TriggerEnter;
        public bool specifiedTagOnly;
        [TagField]
        public string objectTag = "Untagged";
        [BlackboardOnly]
        public BBParameter<GameObject> saveGameObjectAs;

        private bool stay;

        protected override string info {
            get { return CheckType.ToString() + ( specifiedTagOnly ? ( " '" + objectTag + "' tag" ) : "" ); }
        }

        protected override bool OnCheck() {
            return CheckType == TriggerTypes.TriggerStay ? stay : false;
        }

        protected override void OnEnable() {
            router.onTriggerEnter2D += OnTriggerEnter2D;
            router.onTriggerExit2D += OnTriggerExit2D;
        }

        protected override void OnDisable() {
            router.onTriggerEnter2D -= OnTriggerEnter2D;
            router.onTriggerExit2D -= OnTriggerExit2D;
        }

        public void OnTriggerEnter2D(ParadoxNotion.EventData<Collider2D> data) {
            if ( !specifiedTagOnly || data.value.gameObject.CompareTag(objectTag) ) {
                stay = true;
                if ( CheckType == TriggerTypes.TriggerEnter || CheckType == TriggerTypes.TriggerStay ) {
                    saveGameObjectAs.value = data.value.gameObject;
                    YieldReturn(true);
                }
            }
        }

        public void OnTriggerExit2D(ParadoxNotion.EventData<Collider2D> data) {
            if ( !specifiedTagOnly || data.value.gameObject.CompareTag(objectTag) ) {
                stay = false;
                if ( CheckType == TriggerTypes.TriggerExit ) {
                    saveGameObjectAs.value = data.value.gameObject;
                    YieldReturn(true);
                }
            }
        }
    }

    ///----------------------------------------------------------------------------------------------

    [Category("System Events")]
    [Name("Check Trigger 2D")]
    [DoNotList]
    public class CheckTrigger2D : ConditionTask<Collider2D>
    {

        public TriggerTypes CheckType = TriggerTypes.TriggerEnter;
        public bool specifiedTagOnly;
        [TagField]
        public string objectTag = "Untagged";
        [BlackboardOnly]
        public BBParameter<GameObject> saveGameObjectAs;

        private bool stay;

        protected override string info {
            get { return CheckType.ToString() + ( specifiedTagOnly ? ( " '" + objectTag + "' tag" ) : "" ); }
        }

        protected override bool OnCheck() {
            return CheckType == TriggerTypes.TriggerStay ? stay : false;
        }

        protected override void OnEnable() {
            router.onTriggerEnter2D += OnTriggerEnter2D;
            router.onTriggerExit2D += OnTriggerExit2D;
        }

        protected override void OnDisable() {
            router.onTriggerEnter2D -= OnTriggerEnter2D;
            router.onTriggerExit2D -= OnTriggerExit2D;
        }

        public void OnTriggerEnter2D(ParadoxNotion.EventData<Collider2D> data) {
            if ( !specifiedTagOnly || data.value.gameObject.CompareTag(objectTag) ) {
                stay = true;
                if ( CheckType == TriggerTypes.TriggerEnter || CheckType == TriggerTypes.TriggerStay ) {
                    saveGameObjectAs.value = data.value.gameObject;
                    YieldReturn(true);
                }
            }
        }

        public void OnTriggerExit2D(ParadoxNotion.EventData<Collider2D> data) {
            if ( !specifiedTagOnly || data.value.gameObject.CompareTag(objectTag) ) {
                stay = false;
                if ( CheckType == TriggerTypes.TriggerExit ) {
                    saveGameObjectAs.value = data.value.gameObject;
                    YieldReturn(true);
                }
            }
        }
    }
}