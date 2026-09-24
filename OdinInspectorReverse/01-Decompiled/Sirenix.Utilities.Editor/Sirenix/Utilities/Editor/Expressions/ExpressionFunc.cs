namespace Sirenix.Utilities.Editor.Expressions
{
	public delegate TResult ExpressionFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TResult>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10);
	public delegate TResult ExpressionFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9);
	public delegate TResult ExpressionFunc<T1, T2, T3, T4, T5, T6, T7, T8, TResult>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8);
	public delegate TResult ExpressionFunc<T1, T2, T3, T4, T5, T6, T7, TResult>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7);
	public delegate TResult ExpressionFunc<T1, T2, T3, T4, T5, T6, TResult>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6);
	public delegate TResult ExpressionFunc<T1, T2, T3, T4, T5, TResult>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5);
	public delegate TResult ExpressionFunc<T1, T2, T3, T4, TResult>(T1 arg1, T2 arg2, T3 arg3, T4 arg4);
	public delegate TResult ExpressionFunc<T1, T2, T3, TResult>(T1 arg1, T2 arg2, T3 arg3);
	public delegate TResult ExpressionFunc<T1, T2, TResult>(T1 arg1, T2 arg2);
	public delegate TResult ExpressionFunc<T1, TResult>(T1 arg1);
	public delegate TResult ExpressionFunc<TResult>();
}
