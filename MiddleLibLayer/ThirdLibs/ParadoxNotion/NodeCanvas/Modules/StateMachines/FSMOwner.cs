using NodeCanvas.Framework;
using UnityEngine;


namespace NodeCanvas.StateMachines
{

    ///<summary> Add this component on a gameobject to behave based on an FSM.</summary>
    [AddComponentMenu("NodeCanvas/FSM Owner")]
    public class FSMOwner : GraphOwner<FSM>
    {

        ///<summary>The current state name of the root fsm.</summary>
        public string currentRootStateName {
            get { return behaviour != null ? behaviour.currentStateName : null; }
        }

        ///<summary>The previous state name of the root fsm.</summary>
        public string previousRootStateName {
            get { return behaviour != null ? behaviour.previousStateName : null; }
        }

        ///<summary>The current deep state name of the fsm including sub fsms if any.</summary>
        public string currentDeepStateName {
            get
            {
                var state = GetCurrentState(true);
                return state != null ? state.name : null;
            }
        }

        ///<summary>The previous deep state name of the fsm including sub fsms if any.</summary>
        public string previousDeepStateName {
            get
            {
                var state = GetPreviousState(true);
                return state != null ? state.name : null;
            }
        }


        ///<summary>Returns the current fsm state optionally recursively by including SubFSMs.</summary>
        public IState GetCurrentState(bool includeSubFSMs = true) {
            if ( behaviour == null ) { return null; }
            var current = behaviour.currentState;
            if ( includeSubFSMs ) {
                while ( current is NestedFSMState ) {
                    var subState = (NestedFSMState)current;
                    current = subState.currentInstance != null ? subState.currentInstance.currentState : null;
                }
            }
            return current;
        }

        ///<summary>Returns the previous fsm state optionally recursively by including SubFSMs.</summary>
        public IState GetPreviousState(bool includeSubFSMs = true) {
            if ( behaviour == null ) { return null; }
            var current = behaviour.currentState;
            var previous = behaviour.previousState;
            if ( includeSubFSMs ) {
                while ( current is NestedFSMState ) {
                    var subState = (NestedFSMState)current;
                    current = subState.currentInstance != null ? subState.currentInstance.currentState : null;
                    previous = subState.currentInstance != null ? subState.currentInstance.previousState : null;
                }
            }
            return previous;
        }


        ///<summary>Enter a state of the root FSM by it's name.</summary>
        public IState TriggerState(string stateName) { return TriggerState(stateName, FSM.TransitionCallMode.Normal); }
        public IState TriggerState(string stateName, FSM.TransitionCallMode callMode) {
            if ( behaviour != null ) {
                return behaviour.TriggerState(stateName, callMode);
            }
            return null;
        }

        ///<summary>Get all root state names, excluding non-named states.</summary>
        public string[] GetStateNames() {
            if ( behaviour != null ) {
                return behaviour.GetStateNames();
            }
            return null;
        }
    }
}