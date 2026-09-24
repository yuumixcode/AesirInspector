using System;
using System.ComponentModel;
using System.Diagnostics;

namespace Sirenix.OdinInspector
{
	[AttributeUsage(AttributeTargets.All, AllowMultiple = false, Inherited = true)]
	[Conditional("UNITY_EDITOR")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use [RequiredIn(PrefabKind.PrefabInstance)] instead.", true)]
	public sealed class RequiredInPrefabInstancesAttribute : Attribute
	{
		/// <summary>
		/// The message of the info box.
		/// </summary>
		public string ErrorMessage;

		/// <summary>
		/// The type of the info box.
		/// </summary>
		public InfoMessageType MessageType;

		/// <summary>
		/// Adds an error box to the inspector, if the property is missing.
		/// </summary>
		public RequiredInPrefabInstancesAttribute()
		{
			MessageType = InfoMessageType.Error;
		}

		/// <summary>
		/// Adds an info box to the inspector, if the property is missing.
		/// </summary>
		/// <param name="errorMessage">The message to display in the error box.</param>
		/// <param name="messageType">The type of info box to draw.</param>
		public RequiredInPrefabInstancesAttribute(string errorMessage, InfoMessageType messageType)
		{
			ErrorMessage = errorMessage;
			MessageType = messageType;
		}

		/// <summary>
		/// Adds an error box to the inspector, if the property is missing.
		/// </summary>
		/// <param name="errorMessage">The message to display in the error box.</param>
		public RequiredInPrefabInstancesAttribute(string errorMessage)
		{
			ErrorMessage = errorMessage;
			MessageType = InfoMessageType.Error;
		}

		/// <summary>
		/// Adds an info box to the inspector, if the property is missing.
		/// </summary>
		/// <param name="messageType">The type of info box to draw.</param>
		public RequiredInPrefabInstancesAttribute(InfoMessageType messageType)
		{
			MessageType = messageType;
		}
	}
}
