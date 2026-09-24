using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using UnityEditor;
using UnityEngine;

namespace Sirenix.Config
{
	[SirenixEditorConfig]
	public class TypeRegistryUserConfig : GlobalConfig<TypeRegistryUserConfig>
	{
		[SerializeField]
		public SerializedTypeHashSet shownTypes = new SerializedTypeHashSet();

		[SerializeField]
		public SerializedTypeHashSet hiddenTypes = new SerializedTypeHashSet();

		[SerializeField]
		public SerializedTypeHashSet addedIllegalTypes = new SerializedTypeHashSet();

		[SerializeField]
		public SerializedTypeSettingsDictionary typeSettings = new SerializedTypeSettingsDictionary();

		[SerializeField]
		public SerializedTypePriorityDictionary typePriorities = new SerializedTypePriorityDictionary();

		public HashSet<Type> IllegalTypes => addedIllegalTypes.Collection;

		public void OpenEditor()
		{
			EditorWindow.GetWindow<TypeRegistryUserConfigWindow>();
		}

		public void SetVisibility(Type type, bool isVisible)
		{
			if (TypeRegistry.HiddenTypes.Contains(type))
			{
				if (isVisible)
				{
					shownTypes.Collection.Add(type);
				}
				else
				{
					shownTypes.Collection.Remove(type);
				}
			}
			else if (isVisible)
			{
				hiddenTypes.Collection.Remove(type);
			}
			else
			{
				hiddenTypes.Collection.Add(type);
			}
			EditorUtility.SetDirty(this);
		}

		public bool IsVisible(Type type)
		{
			if (shownTypes.Collection.Contains(type))
			{
				return true;
			}
			if (!TypeRegistry.HiddenTypes.Contains(type))
			{
				return !hiddenTypes.Collection.Contains(type);
			}
			return false;
		}

		public bool IsIllegal(Type type)
		{
			return addedIllegalTypes.Collection.Contains(type);
		}

		public void SetIllegal(Type type, bool value)
		{
			if (value)
			{
				addedIllegalTypes.Collection.Add(type);
			}
			else
			{
				addedIllegalTypes.Collection.Remove(type);
			}
			EditorUtility.SetDirty(this);
		}

		public TypeSettings TryGetSettings(Type type)
		{
			if (!typeSettings.Dictionary.ContainsKey(type))
			{
				return null;
			}
			return typeSettings.Dictionary[type];
		}

		public int GetPriority(Type type)
		{
			if (!typePriorities.Dictionary.ContainsKey(type))
			{
				return 0;
			}
			return typePriorities.Dictionary[type];
		}

		public bool IsModified(Type type)
		{
			if (!shownTypes.Collection.Contains(type) && !hiddenTypes.Collection.Contains(type) && !addedIllegalTypes.Collection.Contains(type) && !typePriorities.Dictionary.ContainsKey(type))
			{
				return typeSettings.Dictionary.ContainsKey(type);
			}
			return true;
		}

		public void SetSettings(Type type, TypeSettings value)
		{
			typeSettings.Dictionary[type] = value;
		}

		public void RemoveSettings(Type type)
		{
			typeSettings.Dictionary.Remove(type);
		}

		public void HandleDefaultSettings(Type type, TypeSettings settings, TypeRegistryItemAttribute itemAttribute)
		{
			if (settings.IsDefault())
			{
				if (typeSettings.Dictionary.Remove(type))
				{
					EditorUtility.SetDirty(this);
				}
				return;
			}
			if (itemAttribute != null)
			{
				if (settings.Name == null || (!string.IsNullOrEmpty(itemAttribute.Name) && settings.Name == itemAttribute.Name))
				{
					settings.Name = string.Empty;
				}
				if (settings.Category == null || (!string.IsNullOrEmpty(itemAttribute.CategoryPath) && settings.Category == itemAttribute.CategoryPath))
				{
					settings.Category = string.Empty;
				}
				if (settings.Icon == itemAttribute.Icon)
				{
					settings.Icon = SdfIconType.None;
				}
				if (itemAttribute.DarkIconColor.HasValue && settings.DarkIconColor.HasValue && settings.DarkIconColor.Value == itemAttribute.DarkIconColor.Value)
				{
					settings.DarkIconColor = null;
				}
				if (itemAttribute.LightIconColor.HasValue && settings.LightIconColor.HasValue && settings.LightIconColor.Value == itemAttribute.LightIconColor.Value)
				{
					settings.LightIconColor = null;
				}
			}
			else if (settings.Name == null || settings.Name == type.Name)
			{
				settings.Name = string.Empty;
			}
			if (settings.IsDefault())
			{
				if (typeSettings.Dictionary.Remove(type))
				{
					EditorUtility.SetDirty(this);
				}
			}
			else
			{
				EditorUtility.SetDirty(this);
			}
		}

		public void SetPriority(Type type, int value, TypeRegistryItemAttribute itemAttribute)
		{
			if (value == 0 || (itemAttribute != null && value == itemAttribute.Priority))
			{
				if (typePriorities.Dictionary.Remove(type))
				{
					EditorUtility.SetDirty(this);
				}
			}
			else
			{
				typePriorities.Dictionary[type] = value;
				EditorUtility.SetDirty(this);
			}
		}

		public void ResetType(Type type)
		{
			RemoveSettings(type);
			hiddenTypes.Collection.Remove(type);
			shownTypes.Collection.Remove(type);
			addedIllegalTypes.Collection.Remove(type);
			typePriorities.Dictionary.Remove(type);
			EditorUtility.SetDirty(this);
		}
	}
}
