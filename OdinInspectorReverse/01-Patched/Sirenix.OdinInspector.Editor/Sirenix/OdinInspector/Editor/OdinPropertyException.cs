using System;

namespace Sirenix.OdinInspector.Editor
{
	/// <summary>
	/// Odin property system exception.
	/// </summary>
	public class OdinPropertyException : Exception
	{
		/// <summary>
		/// Initializes a new instance of OdinPropertyException.
		/// </summary>
		/// <param name="message">The message for the exception.</param>
		/// <param name="innerException">An inner exception.</param>
		public OdinPropertyException(string message, Exception innerException)
			: base(message, innerException)
		{
		}
	}
}
