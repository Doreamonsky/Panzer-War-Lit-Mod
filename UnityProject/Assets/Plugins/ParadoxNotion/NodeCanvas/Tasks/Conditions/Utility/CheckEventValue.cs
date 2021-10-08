using NodeCanvas.Framework;
using ParadoxNotion;
using ParadoxNotion.Design;
using Logger = ParadoxNotion.Services.Logger;

namespace NodeCanvas.Tasks.Conditions
{

    [Category("✫ Utility")]
    [Description("Check if an event is received and it's value is equal to specified value, then return true for one frame")]
    public class CheckEventValue<T> : ConditionTask<GraphOwner>
    {

        [RequiredField]
        public BBParameter<string> eventName;
        [Name("Compare Value To")]
        public BBParameter<T> value;

        protected override string info { get { return string.Format("Event [{0}].value == {1}", eventName, value); } }

        protected override void OnEnable() { router.onCustomEvent += OnCustomEvent; }
        protected override void OnDisable() { router.onCustomEvent -= OnCustomEvent; }


        protected override bool OnCheck() { return false; }
        void OnCustomEvent(string eventName, ParadoxNotion.IEventData msg) {
            if ( eventName.Equals(this.eventName.value, System.StringComparison.OrdinalIgnoreCase) ) {
                var receivedValue = msg.valueBoxed;
                if ( ObjectUtils.AnyEquals(receivedValue, value.value) ) {
                    Logger.Log(string.Format("Event Received from ({0}): '{1}'", agent.gameObject.name, eventName), LogTag.EVENT, this);
                    YieldReturn(true);
                }
            }
        }
    }
}