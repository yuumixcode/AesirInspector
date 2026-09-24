using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Sirenix.Serialization;
using Sirenix.Utilities;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor
{
	/// <summary>
	/// <para>Contains configuration data for which types Odin should draw in the inspector.</para>
	/// </summary>
	/// <remarks>
	/// <para>Note that this class supports assigning arbitrary editor types to inspect any Unity object type. The Editor Types GUI in preferences simply does not, as of now, support assigning editors of any other type than <see cref="T:Sirenix.OdinInspector.Editor.OdinEditor" />. However, the API is open to further customization.</para>
	/// <para>When an editor is generated for a type, a new editor type is added to the GeneratedOdinEditors assembly, which is derived from the assigned editor type - in most cases, <see cref="T:Sirenix.OdinInspector.Editor.OdinEditor" />.</para>
	/// <para>You can check if an editor is compatible using <see cref="M:Sirenix.OdinInspector.Editor.InspectorTypeDrawingConfig.UnityInspectorEditorIsValidBase(System.Type,System.Type)" />.</para>
	/// </remarks>
	/// <seealso cref="T:Sirenix.OdinInspector.Editor.InspectorTypeDrawingConfigDrawer" />.
	/// <seealso cref="!:EditorCompilation" />.
	[Serializable]
	public sealed class InspectorTypeDrawingConfig
	{
		/// <summary>
		/// A type that indicates that a drawer is missing.
		/// </summary>
		public static class MissingEditor
		{
		}

		private static readonly Dictionary<Type, Type> HardCodedDefaultEditors;

		private static readonly FieldInfo CustomEditorInspectedTypeField;

		private static readonly FieldInfo CustomEditorEditorForChildClassesField;

		private static readonly PropertyInfo DebugLoggerProperty;

		/// <summary>
		/// <para>The type binder that the <see cref="T:Sirenix.OdinInspector.Editor.InspectorTypeDrawingConfig" /> uses to bind types to names, and names to types.</para>
		/// <para>This is usually an instance of <see cref="T:Sirenix.Serialization.DefaultSerializationBinder" />.</para>
		/// </summary>
		public static readonly TwoWaySerializationBinder TypeBinder;

		private static Type odinEditorType;

		[HideInInspector]
		[SerializeField]
		private List<TypeDrawerPair> configs = new List<TypeDrawerPair>();

		private Dictionary<Type, Type> drawerCache = new Dictionary<Type, Type>(FastTypeComparer.Instance);

		private static Type OdinEditorType
		{
			get
			{
				if (odinEditorType == null)
				{
					odinEditorType = AssemblyUtilities.GetTypeByCachedFullName("Sirenix.OdinInspector.Editor.OdinEditor");
				}
				return odinEditorType;
			}
		}

		static InspectorTypeDrawingConfig()
		{
			HardCodedDefaultEditors = new Dictionary<Type, Type>();
			CustomEditorInspectedTypeField = typeof(CustomEditor).GetField("m_InspectedType", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			CustomEditorEditorForChildClassesField = typeof(CustomEditor).GetField("m_EditorForChildClasses", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			DebugLoggerProperty = typeof(Debug).GetProperty("unityLogger") ?? typeof(Debug).GetProperty("logger");
			TypeBinder = new DefaultSerializationBinder();
			if (CustomEditorInspectedTypeField == null || CustomEditorEditorForChildClassesField == null)
			{
				Debug.LogWarning("Could not find internal fields 'm_InspectedType' and/or 'm_EditorForChildClasses' in type UnityEditor.CustomEditor. Automatic inspector editor generation is highly unlikely to work.");
			}
		}

		/// <summary>
		/// Resets the drawing configuration to the default values.
		/// </summary>
		public void ResetToDefault()
		{
			configs.Clear();
			drawerCache.Clear();
			EditorUtility.SetDirty(GlobalConfig<InspectorConfig>.Instance);
			AssetDatabase.SaveAssets();
		}

		/// <summary>
		/// Gets a list of all drawn types that have entries in the drawing config.
		/// </summary>
		public List<Type> GetAllDrawnTypesWithEntries()
		{
			return drawerCache.Keys.ToList();
		}

		/// <summary>
		/// Forces the config's internal drawer type to value type lookup cache to rebuild itself.
		/// </summary>
		public void UpdateCaches()
		{
			drawerCache.Clear();
			for (int i = 0; i < configs.Count; i++)
			{
				TypeDrawerPair config = configs[i];
				Type drawnType = TypeBinder.BindToType(config.DrawnTypeName);
				if (drawnType == null)
				{
					continue;
				}
				Type drawerType;
				if (string.IsNullOrEmpty(config.EditorTypeName))
				{
					drawerType = null;
				}
				else
				{
					drawerType = TypeBinder.BindToType(config.EditorTypeName);
					if (drawerType == null)
					{
						drawerType = typeof(MissingEditor);
					}
				}
				drawerCache[drawnType] = drawerType;
			}
		}

		/// <summary>
		/// Clears the editor type entry for the given drawer, so it will be set to Unity's default.
		/// </summary>
		/// <param name="drawnType">The drawn type to clear the editor for.</param>
		/// <exception cref="T:System.ArgumentNullException">drawnType is null</exception>
		public void ClearEditorEntryForDrawnType(Type drawnType)
		{
			if (drawnType == null)
			{
				throw new ArgumentNullException("drawnType");
			}
			drawerCache.Remove(drawnType);
			string drawnTypeName = TypeBinder.BindToName(drawnType);
			for (int i = 0; i < configs.Count; i++)
			{
				if (configs[i].DrawnTypeName == drawnTypeName)
				{
					configs.RemoveAt(i);
					i--;
				}
			}
			EditorUtility.SetDirty(GlobalConfig<InspectorConfig>.Instance);
		}

		/// <summary>
		/// Assigns a given editor to draw a given type.
		/// </summary>
		/// <param name="drawnType">The drawn type to assign an editor type for.</param>
		/// <param name="editorType">The editor type to assign. When generating editors, a type derived from this editor will be created and set to draw the given drawn type.</param>
		/// <exception cref="T:System.ArgumentNullException">drawnType</exception>
		/// <exception cref="T:System.ArgumentException">The type " + editorType.GetNiceName() + " is not a valid base editor for type " + drawnType.GetNiceName() + ". Check criteria using <see cref="M:Sirenix.OdinInspector.Editor.InspectorTypeDrawingConfig.UnityInspectorEditorIsValidBase(System.Type,System.Type)" />.</exception>
		public void SetEditorType(Type drawnType, Type editorType)
		{
			if (drawnType == null)
			{
				throw new ArgumentNullException("drawnType");
			}
			string drawnTypeName = TypeBinder.BindToName(drawnType);
			string editorTypeName = ((editorType == null) ? "" : TypeBinder.BindToName(editorType));
			if (editorType != null && !UnityInspectorEditorIsValidBase(editorType, drawnType))
			{
				throw new ArgumentException("The type " + editorType.GetNiceName() + " is not a valid base editor for type " + drawnType.GetNiceName() + ".");
			}
			drawerCache[drawnType] = editorType;
			bool added = false;
			for (int i = 0; i < configs.Count; i++)
			{
				TypeDrawerPair pair = configs[i];
				if (pair.DrawnTypeName == drawnTypeName)
				{
					pair.EditorTypeName = editorTypeName;
					configs[i] = pair;
					added = true;
					break;
				}
			}
			if (!added)
			{
				configs.Add(new TypeDrawerPair(drawnType, editorType));
			}
			EditorUtility.SetDirty(GlobalConfig<InspectorConfig>.Instance);
		}

		/// <summary>
		/// Determines whether an editor value has been assigned for a given drawn type.
		/// </summary>
		/// <param name="drawnType">The drawn type to check.</param>
		/// <exception cref="T:System.ArgumentNullException">drawnType is null</exception>
		public bool HasEntryForType(Type drawnType)
		{
			if (drawnType == null)
			{
				throw new ArgumentNullException("drawnType");
			}
			return drawerCache.ContainsKey(drawnType);
		}

		/// <summary>
		/// Gets which editor type would draw the given type. If the type has not been assigned a custom editor type in the config, the default editor type is returned using <see cref="M:Sirenix.OdinInspector.Editor.InspectorTypeDrawingConfig.GetDefaultEditorType(System.Type)" />.
		/// </summary>
		/// <param name="drawnType">The drawn type to get an editor type for.</param>
		/// <returns>The editor that would draw the given type.</returns>
		/// <exception cref="T:System.ArgumentNullException">drawnType is null</exception>
		public Type GetEditorType(Type drawnType)
		{
			if (drawnType == null)
			{
				throw new ArgumentNullException("drawnType");
			}
			if (drawerCache.TryGetValue(drawnType, out var editorType))
			{
				return editorType;
			}
			return GetDefaultEditorType(drawnType);
		}

		/// <summary>
		/// Gets the default editor that this type would have, if no custom editor was set for this type in particular. This is calculated using the value of <see cref="P:Sirenix.OdinInspector.Editor.InspectorConfig.DefaultEditorBehaviour" />.
		/// </summary>
		/// <param name="drawnType">The drawn type to get the default editor for.</param>
		/// <returns>The editor that would draw this type by default, or null, if there is no default Odin-defined editor for the drawn type.</returns>
		/// <exception cref="T:System.ArgumentNullException">drawnType is null</exception>
		public static Type GetDefaultEditorType(Type drawnType)
		{
			if (drawnType == null)
			{
				throw new ArgumentNullException("drawnType");
			}
			if (!InspectorTypeDrawingConfigDrawer.OdinCanCreateEditorFor(drawnType))
			{
				return null;
			}
			if (!HardCodedDefaultEditors.TryGetValue(drawnType, out var editorType))
			{
				if (GlobalConfig<InspectorConfig>.Instance.DefaultEditorBehaviour == InspectorDefaultEditors.None)
				{
					return null;
				}
				if (AssemblyUtilities.GetAssemblyCategory(drawnType.Assembly) switch
				{
					AssemblyCategory.Scripts => (GlobalConfig<InspectorConfig>.Instance.DefaultEditorBehaviour & InspectorDefaultEditors.UserTypes) == InspectorDefaultEditors.UserTypes, 
					AssemblyCategory.ImportedAssemblies => (GlobalConfig<InspectorConfig>.Instance.DefaultEditorBehaviour & InspectorDefaultEditors.PluginTypes) == InspectorDefaultEditors.PluginTypes, 
					AssemblyCategory.UnityEngine => (GlobalConfig<InspectorConfig>.Instance.DefaultEditorBehaviour & InspectorDefaultEditors.UnityTypes) == InspectorDefaultEditors.UnityTypes, 
					_ => (GlobalConfig<InspectorConfig>.Instance.DefaultEditorBehaviour & InspectorDefaultEditors.OtherTypes) == InspectorDefaultEditors.OtherTypes, 
				})
				{
					return OdinEditorType;
				}
			}
			return editorType;
		}

		/// <summary>
		/// Checks whether the given editor can be assigned to draw any type using the <see cref="T:Sirenix.OdinInspector.Editor.InspectorTypeDrawingConfig" /> class.
		/// </summary>
		/// <param name="editorType">Type of the editor to check.</param>
		/// <returns>True if the editor is valid, otherwise false</returns>
		public static bool UnityInspectorEditorIsValidBase(Type editorType)
		{
			return UnityInspectorEditorIsValidBase(editorType, null);
		}

		/// <summary>
		/// <para>Checks whether the given editor can be assigned to draw a given type using the <see cref="T:Sirenix.OdinInspector.Editor.InspectorTypeDrawingConfig" /> class.</para>
		/// <para>This method checks the <see cref="T:UnityEditor.CustomEditor" /> attribute on the type for whether the given type is compatible.</para>
		/// </summary>
		/// <param name="editorType">Type of the editor to check.</param>
		/// <param name="drawnType">Type of the drawn value to check. If this parameter is null, the drawn type is not checked for compatibility with the editor type; only the editor type itself is checked for validity.</param>
		/// <returns>True if the editor is valid, otherwise false</returns>
		/// <exception cref="T:System.ArgumentNullException">editorType</exception>
		public static bool UnityInspectorEditorIsValidBase(Type editorType, Type drawnType)
		{
			if (editorType == null)
			{
				throw new ArgumentNullException("editorType");
			}
			if (editorType.IsAbstract || !typeof(UnityEditor.Editor).IsAssignableFrom(editorType) || editorType.FullName.StartsWith("UnityEditor", StringComparison.InvariantCulture))
			{
				return false;
			}
			if (CustomEditorInspectedTypeField == null)
			{
				return false;
			}
			CustomEditor attribute = editorType.GetAttribute<CustomEditor>(inherit: true);
			if (attribute == null)
			{
				return true;
			}
			if (drawnType != null)
			{
				Type inspectedType = (Type)CustomEditorInspectedTypeField.GetValue(attribute);
				if (inspectedType == drawnType)
				{
					return true;
				}
				if (CustomEditorEditorForChildClassesField != null && inspectedType.IsAssignableFrom(drawnType))
				{
					return (bool)CustomEditorEditorForChildClassesField.GetValue(attribute);
				}
			}
			return false;
		}

		/// <summary>
		/// <para>Gets the type that an editor draws, by extracting it from the editor's <see cref="T:UnityEditor.CustomEditor" /> attribute, if it is declared.</para>
		/// <para>This method returns null for abstract editor types, as those can never draw anything.</para>
		/// </summary>
		/// <param name="editorType">Type of the editor.</param>
		/// <param name="editorForChildClasses">Whether the editor in question is also an editor for types derived from the given type.</param>
		/// <returns></returns>
		/// <exception cref="T:System.ArgumentNullException">editorType</exception>
		public static Type GetEditorDrawnType(Type editorType, out bool editorForChildClasses)
		{
			if (editorType == null)
			{
				throw new ArgumentNullException("editorType");
			}
			editorForChildClasses = false;
			if (editorType.IsAbstract || CustomEditorInspectedTypeField == null || CustomEditorEditorForChildClassesField == null)
			{
				return null;
			}
			UnityEngine.ILogger logger = null;
			if (DebugLoggerProperty != null)
			{
				logger = (UnityEngine.ILogger)DebugLoggerProperty.GetValue(null, null);
			}
			bool previous = true;
			if (logger != null)
			{
				previous = logger.logEnabled;
				logger.logEnabled = false;
			}
			CustomEditor customEditorAttribute = editorType.GetAttribute<CustomEditor>();
			if (logger != null)
			{
				logger.logEnabled = previous;
			}
			if (customEditorAttribute != null)
			{
				editorForChildClasses = (bool)CustomEditorEditorForChildClassesField.GetValue(customEditorAttribute);
				return (Type)CustomEditorInspectedTypeField.GetValue(customEditorAttribute);
			}
			return null;
		}
	}
}
