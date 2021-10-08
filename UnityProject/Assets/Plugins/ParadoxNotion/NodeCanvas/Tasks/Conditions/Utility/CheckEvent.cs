using NodeCanvas.Framework;
using ParadoxNotion.Design;
using ParadoxNotion;
using Logger = ParadoxNotion.Services.Logger;

namespace NodeCanvas.Tasks.Conditions
{

    [Category("✫ Utility")]
    [Description("Check if an event is received and return true for one frame")]
    public class CheckEvent : ConditionTask<GraphOwner>
    {

        [RequiredField]
        public BBParameter<string> eventName;

        protected override string info { get { return "[" + eventName.ToString() + "]"; } }

        protected override void OnEnable() { router.onCustomEvent += OnCustomEvent; }
        protected override void OnDisable() { router.onCustomEvent -= OnCustomEvent; }

        protected override bool OnCheck() { return false; }

        void OnCustomEvent(string eventName, IEventData data) {
            if ( eventName.Equals(this.eventName.value, System.StringComparison.OrdinalIgnoreCase) ) {
                Logger.Log(string.Format("Event Received from ({0}): '{1}'", agent.gameObject.name, name), LogTag.EVENT, this);
                YieldReturn(true);
            }
        }
    }

    ///----------------------------------------------------------------------------------------------

    [Category("✫ Utility")]
    [Description("Check if an event is received and return true for one frame. Optionaly save the received event's value")]
    public class CheckEvent<T> : ConditionTask<GraphOwner>
    {

        [RequiredField]
        public BBParameter<string> eventName;
        [BlackboardOnly]
        public BBParameter<T> saveEventValue;

        protected override string info { get { return string.Format("Event [{0}]\n{1} = EventValue", eventName, saveEventValue); } }

        protected override void OnEnable() { router.onCustomEvent += OnCustomEvent; }
        protected override void OnDisable() { router.onCustomEvent -= OnCustomEvent; }

        protected override bool OnCheck() { return false; }

        void OnCustomEvent(string eventName, IEventData data) {
            if ( eventName.Equals(this.eventName.value, System.StringComparison.OrdinalIgnoreCase) ) {
                if ( data is EventData<T> ) { //avoid boxing if able
                    saveEventValue.value = ( (EventData<T>)data ).value;
                } else if ( data.valueBoxed is T ) { saveEventValue.value = (T)data.valueBoxed; }

                Logger.Log(string.Format("Event Received from ({0}): '{1}'", agent.gameObject.name, eventName), LogTag.EVENT, this);
                YieldReturn(true);
            }
        }
    }
}