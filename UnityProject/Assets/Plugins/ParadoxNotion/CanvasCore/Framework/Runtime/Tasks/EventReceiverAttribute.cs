namespace NodeCanvas.Framework
{

    [System.AttributeUsage(System.AttributeTargets.Class)]
    [System.Obsolete("[EventReceiver] is no longer used. Please use the '.router' property to subscribe/unsubscribe to events (in OnExecute/OnStop for actions and OnEnable/OnDisable for conditions). For custom events, use '.router.onCustomEvent'.")]
    public class EventReceiverAttribute : System.Attribute
    {
        readonly public string[] eventMessages;
        public EventReceiverAttribute(params string[] args) {
            this.eventMessages = args;
        }
    }
}