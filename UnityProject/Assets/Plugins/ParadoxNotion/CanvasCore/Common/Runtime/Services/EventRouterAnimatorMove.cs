using UnityEngine;

namespace ParadoxNotion.Services
{

    ///Special EventRouter added automatically when required only when OnAnimatorMove event subscribed.
    ///When OnAnimatorMove method exists, Animator ceases to function normally.
    ///This is why this is moved to a separate component and outside of EventRouter.
    public class EventRouterAnimatorMove : MonoBehaviour
    {
        public event EventRouter.EventDelegate onAnimatorMove;

        void OnAnimatorMove() {
            if ( onAnimatorMove != null ) { onAnimatorMove(new EventData(gameObject, this)); }
        }
    }
}