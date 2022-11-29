//common generic delegates

namespace ParadoxNotion
{
    public delegate void ActionCall();
    public delegate void ActionCall<T1>(T1 a);
    public delegate void ActionCall<T1, T2>(T1 a, T2 b);
    public delegate void ActionCall<T1, T2, T3>(T1 a, T2 b, T3 c);
    public delegate void ActionCall<T1, T2, T3, T4>(T1 a, T2 b, T3 c, T4 d);
    public delegate void ActionCall<T1, T2, T3, T4, T5>(T1 a, T2 b, T3 c, T4 d, T5 e);
    public delegate void ActionCall<T1, T2, T3, T4, T5, T6>(T1 a, T2 b, T3 c, T4 d, T5 e, T6 f);
    public delegate void ActionCall<T1, T2, T3, T4, T5, T6, T7>(T1 a, T2 b, T3 c, T4 d, T5 e, T6 f, T7 g);
    public delegate void ActionCall<T1, T2, T3, T4, T5, T6, T7, T8>(T1 a, T2 b, T3 c, T4 d, T5 e, T6 f, T7 g, T8 h);

    public delegate TResult FunctionCall<TResult>();
    public delegate TResult FunctionCall<T1, TResult>(T1 a);
    public delegate TResult FunctionCall<T1, T2, TResult>(T1 a, T2 b);
    public delegate TResult FunctionCall<T1, T2, T3, TResult>(T1 a, T2 b, T3 c);
    public delegate TResult FunctionCall<T1, T2, T3, T4, TResult>(T1 a, T2 b, T3 c, T4 d);
    public delegate TResult FunctionCall<T1, T2, T3, T4, T5, TResult>(T1 a, T2 b, T3 c, T4 d, T5 e);
    public delegate TResult FunctionCall<T1, T2, T3, T4, T5, T6, TResult>(T1 a, T2 b, T3 c, T4 d, T5 e, T6 f);
    public delegate TResult FunctionCall<T1, T2, T3, T4, T5, T6, T7, TResult>(T1 a, T2 b, T3 c, T4 d, T5 e, T6 f, T7 g);
    public delegate TResult FunctionCall<T1, T2, T3, T4, T5, T6, T7, T8, TResult>(T1 a, T2 b, T3 c, T4 d, T5 e, T6 f, T7 g, T8 h);
}