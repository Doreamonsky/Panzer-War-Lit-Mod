namespace NodeCanvas.StateMachines
{

    ///Implement this interface in any MonoBehaviour attached on FSMOwner gameobject to get relevant state callbacks
	public interface IStateCallbackReceiver
    {
        ///Called when a state enters
		void OnStateEnter(IState state);
        ///Called when a state updates
        void OnStateUpdate(IState state);
        ///Called when a state exists
        void OnStateExit(IState state);
    }
}