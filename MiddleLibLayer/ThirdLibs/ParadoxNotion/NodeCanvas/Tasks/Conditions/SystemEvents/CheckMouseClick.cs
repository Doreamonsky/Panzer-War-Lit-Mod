using NodeCanvas.Framework;
using ParadoxNotion;
using ParadoxNotion.Design;
using UnityEngine;


namespace NodeCanvas.Tasks.Conditions
{

    [Category("System Events")]
    public class CheckMouseClick : ConditionTask<Collider>
    {

        public MouseClickEvent checkType = MouseClickEvent.MouseDown;

        protected override string info {
            get { return checkType.ToString(); }
        }

        protected override bool OnCheck() { return false; }

        protected override void OnEnable() {
            router.onMouseDown += OnMouseDown;
            router.onMouseUp += OnMouseUp;
        }

        protected override void OnDisable() {
            router.onMouseDown -= OnMouseDown;
            router.onMouseUp -= OnMouseUp;
        }

        void OnMouseDown(ParadoxNotion.EventData msg) {
            if ( checkType == MouseClickEvent.MouseDown ) {
                YieldReturn(true);
            }
        }

        void OnMouseUp(ParadoxNotion.EventData msg) {
            if ( checkType == MouseClickEvent.MouseUp ) {
                YieldReturn(true);
            }
        }
    }
}