using NodeCanvas.Framework;
using UnityEngine;


namespace NodeCanvas.StateMachines
{

    /// Add this component on a gameobject to behave based on an FSM.
    [AddComponentMenu("NodeCanvas/FSM Owner")]
    public class FSMOwner : GraphOwner<FSM>
    {

        ///The current state name of the root fsm.
        public string currentRootStateName {
            get { return behaviour != null ? behaviour.currentStateName : null; }
        }

        ///The previous state name of the root fsm.
        public string previousRootStateName {
            get { return behaviour != null ? behaviour.previousStateName : null; }
        }

        ///The current deep state name of the fsm including sub fsms if any.
        public string currentDeepStateName {
            get
            {
                var state = GetCurrentState(true);
                return state != null ? state.name : null;
            }
        }

        ///The previous deep state name of the fsm including sub fsms if any.
        public string previousDeepStateName {
            get
            {
                var state = GetPreviousState(true);
                return state != null ? state.name : null;
            }
        }


        ///Returns the current fsm state optionally recursively by including SubFSMs.
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

        ///Returns the previous fsm state optionally recursively by including SubFSMs.
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


        ///Enter a state of the root FSM by it's name.
        public IState TriggerState(string stateName) { return TriggerState(stateName, FSM.TransitionCallMode.Normal); }
        public IState TriggerState(string stateName, FSM.TransitionCallMode callMode) {
            if ( behaviour != null ) {
                return behaviour.TriggerState(stateName, callMode);
            }
            return null;
        }

        ///Get all root state names, excluding non-named states.
        public string[] GetStateNames() {
            if ( behaviour != null ) {
                return behaviour.GetStateNames();
            }
            return null;
        }
    }
}