using System;
using System.Collections;
using ParadoxNotion.Serialization;
using ParadoxNotion.Serialization.FullSerializer;
using NodeCanvas.Framework.Internal;
using UnityEngine;


namespace NodeCanvas.Framework
{

    ///Base class for Conditions. Conditions dont span multiple frames like actions and return true or false immediately on execution. Derive this to create your own.
    ///Generic version to define the AgentType where T is the agentType (Component or Interface) required by the Condition.
    ///For GameObject, use 'Transform'
	abstract public class ConditionTask<T> : ConditionTask where T : class
    {
        sealed public override Type agentType { get { return typeof(T); } }
        new public T agent { get { return base.agent as T; } }
    }

    ///----------------------------------------------------------------------------------------------

#if UNITY_EDITOR //handles missing types
    [fsObject(Processor = typeof(fsRecoveryProcessor<ConditionTask, MissingCondition>))]
#endif

    ///Base class for all Conditions. Conditions dont span multiple frames like actions and return true or false immediately on execution. Derive this to create your own
    abstract public class ConditionTask : Task
    {

        [SerializeField]
        private bool _invert;

        private int yieldReturn = -1;
        private int yields;
        private bool isRuntimeEnabled;

        public bool invert {
            get { return _invert; }
            set { _invert = value; }
        }

        ///...
        public void Enable(Component agent, IBlackboard bb) {
            if ( !isRuntimeEnabled && isUserEnabled ) {
                if ( Set(agent, bb) ) {
                    isRuntimeEnabled = true;
                    OnEnable();
                }
            }
        }

        ///...
        public void Disable() {
            if ( isRuntimeEnabled && isUserEnabled ) {
                isRuntimeEnabled = false;
                OnDisable();
            }
        }

        [System.Obsolete("Use 'Check'")]
        public bool CheckCondition(Component agent, IBlackboard blackboard) { return Check(agent, blackboard); }

        ///Check the condition for the provided agent and with the provided blackboard
        public bool Check(Component agent, IBlackboard blackboard) {

            if ( !isUserEnabled ) {
                return false;
            }

            if ( !Set(agent, blackboard) ) {
                return false;
            }

            Debug.Assert(isRuntimeEnabled, "Condition.Check when enabled = false");

            if ( yieldReturn != -1 ) {
                var b = invert ? !( yieldReturn == 1 ) : ( yieldReturn == 1 );
                yieldReturn = -1;
                return b;
            }

            return invert ? !OnCheck() : OnCheck();
        }

        ///Enables, Checks then Disables the condition. Useful for one-off checks only
        public bool CheckOnce(Component agent, IBlackboard blackboard) {
            Enable(agent, blackboard);
            var result = Check(agent, blackboard);
            Disable();
            return result;
        }

        ///Helper method that holds the return value provided for one frame, for the condition to return.
        protected void YieldReturn(bool value) {
            if ( isRuntimeEnabled ) {
                yieldReturn = value ? 1 : 0;
                StartCoroutine(Flip());
            }
        }

        //...
        IEnumerator Flip() {
            yields++;
            yield return null;
            yields--;
            if ( yields == 0 ) {
                yieldReturn = -1;
            }
        }

        ///----------------------------------------------------------------------------------------------

        ///Override to do things when condition is enabled
        virtual protected void OnEnable() { }
        ///Override to do things when condition is disabled
        virtual protected void OnDisable() { }
        ///Override to return whether the condition is true or false. The result will be inverted if Invert is checked.
        virtual protected bool OnCheck() { return true; }

        ///----------------------------------------------------------------------------------------------
    }
}