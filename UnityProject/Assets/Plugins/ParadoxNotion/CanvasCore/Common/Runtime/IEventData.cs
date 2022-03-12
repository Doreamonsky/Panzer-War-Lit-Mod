
using UnityEngine;

namespace ParadoxNotion
{
    ///<summary>Common interface between EventData and EventData<T></summary>
    public interface IEventData
    {
        GameObject receiver { get; }
        object sender { get; }
        object valueBoxed { get; }
    }

    ///<summary>Dispatched within EventRouter and contains data about the event</summary>
    public struct EventData : IEventData
    {
        public GameObject receiver { get; private set; }
        public object sender { get; private set; }
        public object value { get; private set; }
        public object valueBoxed => value;
        public EventData(object value, GameObject receiver, object sender) {
            this.value = value;
            this.receiver = receiver;
            this.sender = sender;
        }
        public EventData(GameObject receiver, object sender) {
            this.value = null;
            this.receiver = receiver;
            this.sender = sender;
        }
    }

    ///<summary>Dispatched within EventRouter and contains data about the event</summary>
    public struct EventData<T> : IEventData
    {
        public GameObject receiver { get; private set; }
        public object sender { get; private set; }
        public T value { get; private set; }
        public object valueBoxed => value;
        public EventData(T value, GameObject receiver, object sender) {
            this.receiver = receiver;
            this.sender = sender;
            this.value = value;
        }
    }
}