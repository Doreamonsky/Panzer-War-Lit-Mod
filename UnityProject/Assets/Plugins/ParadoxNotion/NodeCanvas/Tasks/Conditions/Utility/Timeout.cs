using UnityEngine;
using NodeCanvas.Framework;
using ParadoxNotion.Design;
using ParadoxNotion.Services;

namespace NodeCanvas.Tasks.Conditions
{

    [Category("✫ Utility")]
    [Description("Will return true after a specific amount of time has passed and false while still counting down")]
    public class Timeout : ConditionTask
    {

        public BBParameter<float> timeout = 1f;
        private float currentTime;

        protected override string info {
            get { return string.Format("Timeout {0}/{1}", currentTime.ToString("0.00"), timeout.ToString()); }
        }

        protected override void OnEnable() {
            currentTime = 0;
            MonoManager.current.onLateUpdate += MoveNext;
        }

        protected override void OnDisable() {
            MonoManager.current.onLateUpdate -= MoveNext;
        }

        void MoveNext() {
            currentTime += Time.deltaTime;
            currentTime = Mathf.Min(currentTime, timeout.value);
        }

        protected override bool OnCheck() {
            return currentTime >= timeout.value;
        }
    }
}
