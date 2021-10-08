namespace NodeCanvas.StateMachines
{

    //An interface for states. Works together with IStateCallbackReceiver
    public interface IState
    {
        ///The name of the state
        string name { get; }
        ///The tag of the state
        string tag { get; }
        ///The elapsed time of the state
        float elapsedTime { get; }
        ///The FSM this state belongs to
        FSM FSM { get; }
        ///An array of the state's transition connections
        FSMConnection[] GetTransitions();
        ///Evaluates the state's transitions and returns true if a transition has been performed
        bool CheckTransitions();
        ///Marks the state as Finished
        void Finish(bool success);
    }
}