namespace NodeCanvas.Framework
{

    /// Core Execution Status enumeration
    public enum Status
    {
        ///The operation has failed.
        Failure = 0,
        ///The operation has succeeded.
        Success = 1,
        ///The operation is still running.
        Running = 2,
        ///Indicates a "ready" state. No operation is performed yet.
        Resting = 3,
        ///The operation encountered an error. Usually execution error. This status is unhandled and is neither considered Success nor Failure.
        Error = 4,
        ///The operation is considered optional and is neither Success nor Failure.
        Optional = 5,
    }
}