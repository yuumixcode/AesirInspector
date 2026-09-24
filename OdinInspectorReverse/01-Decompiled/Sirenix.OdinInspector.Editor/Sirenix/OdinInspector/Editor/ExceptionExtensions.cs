using System;
using System.Reflection;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor
{
	public static class ExceptionExtensions
	{
		public static bool IsExitGUIException(this Exception ex)
		{
			do
			{
				if (ex is ExitGUIException)
				{
					return true;
				}
				ex = ex.InnerException;
			}
			while (ex != null);
			return false;
		}

		public static ExitGUIException AsExitGUIException(this Exception ex)
		{
			do
			{
				if (ex is ExitGUIException)
				{
					return ex as ExitGUIException;
				}
				ex = ex.InnerException;
			}
			while (ex != null);
			return null;
		}

		/// <summary>
		/// Unwraps TargetInvocationException and TypeInitializationException
		/// </summary>
		public static Exception UnwrapException(this Exception ex)
		{
			while (ex != null && (ex is TargetInvocationException || ex is TypeInitializationException))
			{
				ex = ex.InnerException;
			}
			return ex;
		}
	}
}
