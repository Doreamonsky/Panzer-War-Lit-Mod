using UnityEngine;
using UnityEngine.EventSystems;

namespace ParadoxNotion.Services
{

    ///<summary>Automaticaly added to a gameobject when needed. Handles forwarding Unity event messages to listeners that need them as well as Custom event forwarding. Notice: this is a partial class. Add your own methods/events if you like.</summary>
    public partial class EventRouter : MonoBehaviour
            , IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler,
            IDragHandler, IScrollHandler, IUpdateSelectedHandler, ISelectHandler, IDeselectHandler, IMoveHandler, ISubmitHandler, IDropHandler
    {

        //special router for OnAnimatorMove only
        private EventRouterAnimatorMove _routerAnimatorMove;

        ///----------------------------------------------------------------------------------------------

        public delegate void EventDelegate(EventData msg);
        public delegate void EventDelegate<T>(EventData<T> msg);

        ///----------------------------------------------------------------------------------------------
        public event EventDelegate<PointerEventData> onPointerEnter;
        public event EventDelegate<PointerEventData> onPointerExit;
        public event EventDelegate<PointerEventData> onPointerDown;
        public event EventDelegate<PointerEventData> onPointerUp;
        public event EventDelegate<PointerEventData> onPointerClick;
        public event EventDelegate<PointerEventData> onDrag;
        public event EventDelegate<PointerEventData> onDrop;
        public event EventDelegate<PointerEventData> onScroll;
        public event EventDelegate<BaseEventData> onUpdateSelected;
        public event EventDelegate<BaseEventData> onSelect;
        public event EventDelegate<BaseEventData> onDeselect;
        public event EventDelegate<AxisEventData> onMove;
        public event EventDelegate<BaseEventData> onSubmit;

        void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData) { if ( onPointerEnter != null ) onPointerEnter(new EventData<PointerEventData>(eventData, gameObject, this)); }
        void IPointerExitHandler.OnPointerExit(PointerEventData eventData) { if ( onPointerExit != null ) onPointerExit(new EventData<PointerEventData>(eventData, gameObject, this)); }
        void IPointerDownHandler.OnPointerDown(PointerEventData eventData) { if ( onPointerDown != null ) onPointerDown(new EventData<PointerEventData>(eventData, gameObject, this)); }
        void IPointerUpHandler.OnPointerUp(PointerEventData eventData) { if ( onPointerUp != null ) onPointerUp(new EventData<PointerEventData>(eventData, gameObject, this)); }
        void IPointerClickHandler.OnPointerClick(PointerEventData eventData) { if ( onPointerClick != null ) onPointerClick(new EventData<PointerEventData>(eventData, gameObject, this)); }
        void IDragHandler.OnDrag(PointerEventData eventData) { if ( onDrag != null ) onDrag(new EventData<PointerEventData>(eventData, gameObject, this)); }
        void IDropHandler.OnDrop(PointerEventData eventData) { if ( onDrop != null ) onDrop(new EventData<PointerEventData>(eventData, gameObject, this)); }
        void IScrollHandler.OnScroll(PointerEventData eventData) { if ( onScroll != null ) onScroll(new EventData<PointerEventData>(eventData, gameObject, this)); }
        void IUpdateSelectedHandler.OnUpdateSelected(BaseEventData eventData) { if ( onUpdateSelected != null ) onUpdateSelected(new EventData<BaseEventData>(eventData, gameObject, this)); }
        void ISelectHandler.OnSelect(BaseEventData eventData) { if ( onSelect != null ) onSelect(new EventData<BaseEventData>(eventData, gameObject, this)); }
        void IDeselectHandler.OnDeselect(BaseEventData eventData) { if ( onDeselect != null ) onDeselect(new EventData<BaseEventData>(eventData, gameObject, this)); }
        void IMoveHandler.OnMove(AxisEventData eventData) { if ( onMove != null ) onMove(new EventData<AxisEventData>(eventData, gameObject, this)); }
        void ISubmitHandler.OnSubmit(BaseEventData eventData) { if ( onSubmit != null ) onSubmit(new EventData<BaseEventData>(eventData, gameObject, this)); }

        //-------------------------------------------------
        public event EventDelegate onMouseDown;
        public event EventDelegate onMouseDrag;
        public event EventDelegate onMouseEnter;
        public event EventDelegate onMouseExit;
        public event EventDelegate onMouseOver;
        public event EventDelegate onMouseUp;

        void OnMouseDown() { if ( onMouseDown != null ) onMouseDown(new EventData(gameObject, this)); }
        void OnMouseDrag() { if ( onMouseDrag != null ) onMouseDrag(new EventData(gameObject, this)); }
        void OnMouseEnter() { if ( onMouseEnter != null ) onMouseEnter(new EventData(gameObject, this)); }
        void OnMouseExit() { if ( onMouseExit != null ) onMouseExit(new EventData(gameObject, this)); }
        void OnMouseOver() { if ( onMouseOver != null ) onMouseOver(new EventData(gameObject, this)); }
        void OnMouseUp() { if ( onMouseUp != null ) onMouseUp(new EventData(gameObject, this)); }

        //-------------------------------------------------
        public event EventDelegate onEnable;
        public event EventDelegate onDisable;
        public event EventDelegate onDestroy;

        void OnEnable() { if ( onEnable != null ) onEnable(new EventData(gameObject, this)); }
        void OnDisable() { if ( onDisable != null ) onDisable(new EventData(gameObject, this)); }
        void OnDestroy() { if ( onDestroy != null ) onDestroy(new EventData(gameObject, this)); }

        //-------------------------------------------------
        public event EventDelegate onTransformChildrenChanged;
        public event EventDelegate onTransformParentChanged;

        void OnTransformChildrenChanged() { if ( onTransformChildrenChanged != null ) onTransformChildrenChanged(new EventData(gameObject, this)); }
        void OnTransformParentChanged() { if ( onTransformParentChanged != null ) onTransformParentChanged(new EventData(gameObject, this)); }

        //-------------------------------------------------
        public event EventDelegate<int> onAnimatorIK;
        public event EventDelegate onAnimatorMove {
            add { if ( _routerAnimatorMove == null ) { _routerAnimatorMove = gameObject.GetAddComponent<EventRouterAnimatorMove>(); } _routerAnimatorMove.onAnimatorMove += value; }
            remove { if ( _routerAnimatorMove != null ) { _routerAnimatorMove.onAnimatorMove -= value; } }
        }

        void OnAnimatorIK(int layerIndex) { if ( onAnimatorIK != null ) onAnimatorIK(new EventData<int>(layerIndex, gameObject, this)); }

        //-------------------------------------------------
        public event EventDelegate onBecameInvisible;
        public event EventDelegate onBecameVisible;

        void OnBecameInvisible() { if ( onBecameInvisible != null ) onBecameInvisible(new EventData(gameObject, this)); }
        void OnBecameVisible() { if ( onBecameVisible != null ) onBecameVisible(new EventData(gameObject, this)); }

        //-------------------------------------------------
        public event EventDelegate<ControllerColliderHit> onControllerColliderHit;
        public event EventDelegate<GameObject> onParticleCollision;

        void OnControllerColliderHit(ControllerColliderHit hit) { if ( onControllerColliderHit != null ) onControllerColliderHit(new EventData<ControllerColliderHit>(hit, gameObject, this)); }
        void OnParticleCollision(GameObject other) { if ( onParticleCollision != null ) onParticleCollision(new EventData<GameObject>(other, gameObject, this)); }

        //-------------------------------------------------
        public event EventDelegate<Collision> onCollisionEnter;
        public event EventDelegate<Collision> onCollisionExit;
        public event EventDelegate<Collision> onCollisionStay;

        void OnCollisionEnter(Collision collisionInfo) { if ( onCollisionEnter != null ) onCollisionEnter(new EventData<Collision>(collisionInfo, gameObject, this)); }
        void OnCollisionExit(Collision collisionInfo) { if ( onCollisionExit != null ) onCollisionExit(new EventData<Collision>(collisionInfo, gameObject, this)); }
        void OnCollisionStay(Collision collisionInfo) { if ( onCollisionStay != null ) onCollisionStay(new EventData<Collision>(collisionInfo, gameObject, this)); }

        public event EventDelegate<Collision2D> onCollisionEnter2D;
        public event EventDelegate<Collision2D> onCollisionExit2D;
        public event EventDelegate<Collision2D> onCollisionStay2D;

        void OnCollisionEnter2D(Collision2D collisionInfo) { if ( onCollisionEnter2D != null ) onCollisionEnter2D(new EventData<Collision2D>(collisionInfo, gameObject, this)); }
        void OnCollisionExit2D(Collision2D collisionInfo) { if ( onCollisionExit2D != null ) onCollisionExit2D(new EventData<Collision2D>(collisionInfo, gameObject, this)); }
        void OnCollisionStay2D(Collision2D collisionInfo) { if ( onCollisionStay2D != null ) onCollisionStay2D(new EventData<Collision2D>(collisionInfo, gameObject, this)); }

        //-------------------------------------------------

        public event EventDelegate<Collider> onTriggerEnter;
        public event EventDelegate<Collider> onTriggerExit;
        public event EventDelegate<Collider> onTriggerStay;

        void OnTriggerEnter(Collider other) { if ( onTriggerEnter != null ) onTriggerEnter(new EventData<Collider>(other, gameObject, this)); }
        void OnTriggerExit(Collider other) { if ( onTriggerExit != null ) onTriggerExit(new EventData<Collider>(other, gameObject, this)); }
        void OnTriggerStay(Collider other) { if ( onTriggerStay != null ) onTriggerStay(new EventData<Collider>(other, gameObject, this)); }

        public event EventDelegate<Collider2D> onTriggerEnter2D;
        public event EventDelegate<Collider2D> onTriggerExit2D;
        public event EventDelegate<Collider2D> onTriggerStay2D;

        void OnTriggerEnter2D(Collider2D other) { if ( onTriggerEnter2D != null ) onTriggerEnter2D(new EventData<Collider2D>(other, gameObject, this)); }
        void OnTriggerExit2D(Collider2D other) { if ( onTriggerExit2D != null ) onTriggerExit2D(new EventData<Collider2D>(other, gameObject, this)); }
        void OnTriggerStay2D(Collider2D other) { if ( onTriggerStay2D != null ) onTriggerStay2D(new EventData<Collider2D>(other, gameObject, this)); }

        ///----------------------------------------------------------------------------------------------

        public event System.Action<RenderTexture, RenderTexture> onRenderImage;
        void OnRenderImage(RenderTexture source, RenderTexture destination) { if ( onRenderImage != null ) onRenderImage(source, destination); }
        ///----------------------------------------------------------------------------------------------

        public event EventDelegate onDrawGizmos;
        void OnDrawGizmos() { if ( onDrawGizmos != null ) onDrawGizmos(new EventData(gameObject, this)); }
        ///----------------------------------------------------------------------------------------------


        ///----------------------------------------------------------------------------------------------

        public delegate void CustomEventDelegate(string name, IEventData data);

        ///<summary>Sub/Unsub to a custom named events invoked through this router</summary>
        public event CustomEventDelegate onCustomEvent;

        ///<summary>Invokes a custom named event</summary>
        public void InvokeCustomEvent(string name, object value, object sender) {
            if ( onCustomEvent != null ) { onCustomEvent(name, new EventData(value, gameObject, sender)); }
        }

        ///<summary>Invokes a custom named event</summary>
        public void InvokeCustomEvent<T>(string name, T value, object sender) {
            if ( onCustomEvent != null ) { onCustomEvent(name, new EventData<T>(value, gameObject, sender)); }
        }
    }
}