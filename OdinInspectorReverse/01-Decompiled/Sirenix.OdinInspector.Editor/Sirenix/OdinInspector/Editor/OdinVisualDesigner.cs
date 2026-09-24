using System;
using Sirenix.OdinInspector.Editor.Internal;

namespace Sirenix.OdinInspector.Editor
{
	public static class OdinVisualDesigner
	{
		/// <summary>
		/// Opens the Visual Designer for the specified <see cref="T:System.Type" />.
		/// </summary>
		/// <param name="type">The type to display and customize in the Visual Designer.</param>
		/// <returns>
		/// The opened <see cref="T:Sirenix.OdinInspector.Editor.OdinEditorWindow" />, or <c>null</c> if the window could not be opened.
		/// </returns>
		public static OdinEditorWindow OpenForType(Type type)
		{
			if (type == null)
			{
				return null;
			}
			return DesignerEditors.Get(type, null, null)?.OpenWindow();
		}

		/// <summary>
		/// Opens the Visual Designer for the specified object instance.
		/// </summary>
		/// <param name="instance">The object instance to display and customize in the Visual Designer.</param>
		/// <returns>
		/// The opened <see cref="T:Sirenix.OdinInspector.Editor.OdinEditorWindow" />, or <c>null</c> if the window could not be opened or the instance was <c>null</c>.
		/// </returns>
		internal static OdinEditorWindow OpenForInstance(object instance)
		{
			if (instance == null)
			{
				return null;
			}
			if (instance is Type type)
			{
				return OpenForType(type);
			}
			return OpenForType(instance.GetType());
		}

		/// <summary>
		/// Opens the Visual Designer for the specified <see cref="T:Sirenix.OdinInspector.Editor.InspectorProperty" />.
		/// </summary>
		/// <param name="property">The property to display and customize in the Visual Designer.</param>
		/// <returns>
		/// The opened <see cref="T:Sirenix.OdinInspector.Editor.OdinEditorWindow" />, or <c>null</c> if the window could not be opened.
		/// </returns>
		internal static OdinEditorWindow OpenForProperty(InspectorProperty property)
		{
			if (property == null)
			{
				return null;
			}
			Type type = ((property.ValueEntry != null) ? property.ValueEntry.TypeOfValue : property.Info?.TypeOfValue);
			return OpenForType(type);
		}
	}
}
