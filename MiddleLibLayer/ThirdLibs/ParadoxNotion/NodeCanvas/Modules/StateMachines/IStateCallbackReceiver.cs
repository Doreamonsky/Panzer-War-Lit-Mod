namespace NodeCanvas.StateMachines
{

    ///<summary>Implement this interface in any MonoBehaviour attached on FSMOwner gameobject to get relevant state callbacks</summary>
	public interface IStateCallbackReceiver
    {
        ///<summary>Called when a state enters</summary>
		void OnStateEnter(IState state);
        ///<summary>Called when a state updates</summary>
        void OnStateUpdate(IState state);
        ///<summary>Called when a state exists</summary>
        void OnStateExit(IState state);
    }
}