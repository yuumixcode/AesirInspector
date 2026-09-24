using System;
using System.ComponentModel;
using System.Diagnostics;

namespace Sirenix.OdinInspector
{
	[Obsolete("Use [RequiredIn(PrefabKind.PrefabAsset)] instead.", true)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	[AttributeUsage(AttributeTargets.All, AllowMultiple = false, Inherited = true)]
	[Conditional("UNITY_EDITOR")]
	public sealed class RequiredInPrefabAssetsAttribute : Attribute
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
		public RequiredInPrefabAssetsAttribute()
		{
			MessageType = InfoMessageType.Error;
		}

		/// <summary>
		/// Adds an info box to the inspector, if the property is missing.
		/// </summary>
		/// <param name="errorMessage">The message to display in the error box.</param>
		/// <param name="messageType">The type of info box to draw.</param>
		public RequiredInPrefabAssetsAttribute(string errorMessage, InfoMessageType messageType)
		{
			ErrorMessage = errorMessage;
			MessageType = messageType;
		}

		/// <summary>
		/// Adds an error box to the inspector, if the property is missing.
		/// </summary>
		/// <param name="errorMessage">The message to display in the error box.</param>
		public RequiredInPrefabAssetsAttribute(string errorMessage)
		{
			ErrorMessage = errorMessage;
			MessageType = InfoMessageType.Error;
		}

		/// <summary>
		/// Adds an info box to the inspector, if the property is missing.
		/// </summary>
		/// <param name="messageType">The type of info box to draw.</param>
		public RequiredInPrefabAssetsAttribute(InfoMessageType messageType)
		{
			MessageType = messageType;
		}
	}
}
