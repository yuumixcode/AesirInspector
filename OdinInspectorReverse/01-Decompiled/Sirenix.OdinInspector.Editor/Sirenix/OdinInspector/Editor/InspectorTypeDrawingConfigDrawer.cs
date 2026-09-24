using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Sirenix.OdinInspector.Internal;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor
{
	/// <summary>
	/// <para>Draws an <see cref="T:Sirenix.OdinInspector.Editor.InspectorTypeDrawingConfig" /> instance, and contains methods getting all types that should be drawn by Odin.</para>
	/// <para>Note that this class keeps a lot of static state, and is only intended to draw the instance of <see cref="T:Sirenix.OdinInspector.Editor.InspectorTypeDrawingConfig" /> that exists in the <see cref="T:Sirenix.OdinInspector.Editor.InspectorConfig" /> singleton asset. If used to draw other instances, odd behaviour may occur.</para>
	/// </summary>
	/// <seealso cref="T:Sirenix.OdinInspector.Editor.InspectorTypeDrawingConfig" />.
	/// <seealso cref="!:EditorCompilation" />.
	public class InspectorTypeDrawingConfigDrawer : OdinValueDrawer<InspectorTypeDrawingConfig>
	{
		private enum DisplayType
		{
			AllUnityObjects,
			AllComponents,
			AllScriptableObjects,
			UserScripts
		}

		private class TypeGroup
		{
			public class TypePair
			{
				public readonly Type DrawnType;

				public readonly Type PreExistingEditorType;

				public TypePair(Type drawnType, Type preExistingEditorType)
				{
					DrawnType = drawnType;
					PreExistingEditorType = preExistingEditorType;
				}
			}

			public readonly string Name;

			public readonly List<TypeGroup> SubGroups = new List<TypeGroup>();

			public readonly List<TypePair> SubTypes = new List<TypePair>();

			private readonly List<bool> SubTypesVisible = new List<bool>();

			public bool HasConflict { get; private set; }

			public bool AllSubTypesVisible { get; private set; }

			public bool IsSearchVisible { get; private set; }

			public bool IsExpanded { get; set; }

			public bool HasEligibleTypes { get; private set; }

			public TypeGroup(string name)
			{
				Name = name;
				HasEligibleTypes = false;
			}

			public TypeGroup GetChildGroup(string name)
			{
				for (int i = 0; i < SubGroups.Count; i++)
				{
					if (SubGroups[i].Name == name)
					{
						return SubGroups[i];
					}
				}
				TypeGroup newGroup = new TypeGroup(name);
				SubGroups.Add(newGroup);
				return newGroup;
			}

			public void ExpandAll()
			{
				IsExpanded = true;
				for (int i = 0; i < SubGroups.Count; i++)
				{
					SubGroups[i].ExpandAll();
				}
			}

			public void SetSharedEditorType(Type editorType)
			{
				HasConflict = false;
				for (int i = 0; i < SubTypes.Count; i++)
				{
					if (SubTypes[i].PreExistingEditorType == null)
					{
						GlobalConfig<InspectorConfig>.Instance.DrawingConfig.SetEditorType(SubTypes[i].DrawnType, editorType);
					}
				}
				for (int j = 0; j < SubGroups.Count; j++)
				{
					SubGroups[j].SetSharedEditorType(editorType);
				}
			}

			public void ClearEditorTypes()
			{
				HasConflict = false;
				for (int i = 0; i < SubTypes.Count; i++)
				{
					GlobalConfig<InspectorConfig>.Instance.DrawingConfig.ClearEditorEntryForDrawnType(SubTypes[i].DrawnType);
				}
				for (int j = 0; j < SubGroups.Count; j++)
				{
					SubGroups[j].ClearEditorTypes();
				}
			}

			public Type GetSharedEditorType()
			{
				if (HasConflict)
				{
					return null;
				}
				if (SubTypes.Count > 0)
				{
					for (int i = 0; i < SubTypes.Count; i++)
					{
						if (SubTypes[i].PreExistingEditorType == null)
						{
							return GlobalConfig<InspectorConfig>.Instance.DrawingConfig.GetEditorType(SubTypes[i].DrawnType);
						}
					}
				}
				for (int j = 0; j < SubGroups.Count; j++)
				{
					Type result = SubGroups[j].GetSharedEditorType();
					if (result != null)
					{
						return result;
					}
				}
				return null;
			}

			public void UpdateHasEligibleTypes()
			{
				HasEligibleTypes = false;
				for (int i = 0; i < SubGroups.Count; i++)
				{
					SubGroups[i].UpdateHasEligibleTypes();
					if (SubGroups[i].HasEligibleTypes)
					{
						HasEligibleTypes = true;
					}
				}
				for (int j = 0; j < SubTypes.Count; j++)
				{
					if (SubTypes[j].PreExistingEditorType == null)
					{
						HasEligibleTypes = true;
						break;
					}
				}
			}

			public void UpdateConflicts()
			{
				HasConflict = false;
				for (int i = 0; i < SubGroups.Count; i++)
				{
					SubGroups[i].UpdateConflicts();
					if (SubGroups[i].HasConflict)
					{
						HasConflict = true;
					}
				}
				Type editor = null;
				if (!HasConflict)
				{
					for (int j = 0; j < SubTypes.Count; j++)
					{
						if (SubTypes[j].PreExistingEditorType == null)
						{
							editor = GlobalConfig<InspectorConfig>.Instance.DrawingConfig.GetEditorType(SubTypes[j].DrawnType);
						}
					}
					for (int k = 0; k < SubTypes.Count; k++)
					{
						if (SubTypes[k].PreExistingEditorType == null && GlobalConfig<InspectorConfig>.Instance.DrawingConfig.GetEditorType(SubTypes[k].DrawnType) != editor)
						{
							HasConflict = true;
							break;
						}
					}
				}
				if (HasConflict || SubGroups.Count <= 0)
				{
					return;
				}
				Type firstGroupEditor = null;
				for (int l = 0; l < SubGroups.Count; l++)
				{
					if (SubGroups[l].HasEligibleTypes)
					{
						firstGroupEditor = SubGroups[l].GetSharedEditorType();
						break;
					}
				}
				bool compareEditor = SubTypes.Count > 0;
				if (compareEditor && firstGroupEditor != editor)
				{
					HasConflict = true;
				}
				if (HasConflict)
				{
					return;
				}
				for (int m = 0; m < SubGroups.Count; m++)
				{
					if (SubGroups[m].HasEligibleTypes)
					{
						Type curEditor = SubGroups[m].GetSharedEditorType();
						if ((compareEditor && curEditor != editor) || curEditor != firstGroupEditor)
						{
							HasConflict = true;
							break;
						}
					}
				}
			}

			public void Sort()
			{
				SubGroups.Sort((TypeGroup a, TypeGroup b) => a.Name.CompareTo(b.Name));
				SubTypes.Sort((TypePair a, TypePair b) => a.DrawnType.Name.CompareTo(b.DrawnType.Name));
				foreach (TypeGroup group in SubGroups)
				{
					group.Sort();
				}
			}

			public bool IsTypeVisible(Type type)
			{
				for (int i = 0; i < SubTypes.Count; i++)
				{
					if (SubTypes[i].DrawnType == type)
					{
						if (SubTypesVisible.Count > i)
						{
							return SubTypesVisible[i];
						}
						return false;
					}
				}
				return false;
			}

			public void UpdateSearch(string search, DisplayType displayType)
			{
				IsSearchVisible = false;
				AllSubTypesVisible = true;
				SubTypesVisible.SetLength(SubTypes.Count);
				foreach (TypeGroup group in SubGroups)
				{
					group.UpdateSearch(search, displayType);
					if (group.IsSearchVisible)
					{
						IsSearchVisible = true;
					}
					if (!group.AllSubTypesVisible)
					{
						AllSubTypesVisible = false;
					}
				}
				bool searchIsNullOrWhitespace = search.IsNullOrWhitespace();
				if (searchIsNullOrWhitespace && displayType == DisplayType.AllUnityObjects)
				{
					IsSearchVisible = true;
					AllSubTypesVisible = true;
					for (int i = 0; i < SubTypesVisible.Count; i++)
					{
						SubTypesVisible[i] = true;
					}
					return;
				}
				for (int j = 0; j < SubTypes.Count; j++)
				{
					Type type = SubTypes[j].DrawnType;
					if ((displayType == DisplayType.AllScriptableObjects && !typeof(ScriptableObject).IsAssignableFrom(type)) || (displayType == DisplayType.AllComponents && !typeof(Component).IsAssignableFrom(type)) || (displayType == DisplayType.UserScripts && (AssemblyUtilities.GetAssemblyCategory(type.Assembly) & AssemblyCategory.ProjectSpecific) == 0))
					{
						SubTypesVisible[j] = false;
						AllSubTypesVisible = false;
					}
					else if (searchIsNullOrWhitespace || StringExtensions.Contains(type.FullName, search, StringComparison.InvariantCultureIgnoreCase))
					{
						IsSearchVisible = true;
						SubTypesVisible[j] = true;
					}
					else
					{
						SubTypesVisible[j] = false;
						AllSubTypesVisible = false;
					}
				}
			}
		}

		[StructLayout(LayoutKind.Sequential, Size = 1)]
		private struct ProfileSection : IDisposable
		{
			public static ProfileSection Start(string name)
			{
				return default(ProfileSection);
			}

			public void Dispose()
			{
			}
		}

		private static readonly HashSet<Type> NeverDrawTypes;

		private static bool initializedForDrawing;

		private static readonly TypeGroup ScriptTypesRootGroup;

		private static readonly TypeGroup ImportedAssemblyTypesRootGroup;

		private static readonly TypeGroup UnityTypesRootGroup;

		private static readonly TypeGroup OtherTypesRootGroup;

		private static readonly List<Type> PossibleEditorTypes;

		private static readonly HashSet<Type> PossibleDrawnTypes;

		private static readonly Dictionary<Type, Type> TypeToDrawingEditorMap;

		private static GUIStyle iconStyle;

		private string searchText;

		private DisplayType displayType;

		private Vector2 scrollPos;

		private static GUIStyle IconStyle
		{
			get
			{
				if (iconStyle == null)
				{
					iconStyle = new GUIStyle
					{
						margin = new RectOffset(5, 0, 4, 0)
					};
				}
				return iconStyle;
			}
		}

		static InspectorTypeDrawingConfigDrawer()
		{
			initializedForDrawing = false;
			ScriptTypesRootGroup = new TypeGroup("Script Types");
			ImportedAssemblyTypesRootGroup = new TypeGroup("Imported Assembly Types");
			UnityTypesRootGroup = new TypeGroup("Unity Types");
			OtherTypesRootGroup = new TypeGroup("Other Types");
			PossibleEditorTypes = new List<Type>();
			PossibleDrawnTypes = new HashSet<Type>();
			TypeToDrawingEditorMap = new Dictionary<Type, Type>(FastTypeComparer.Instance);
			using (ProfileSection.Start("InspectorTypeDrawingConfigDrawer static constructor"))
			{
				NeverDrawTypes = new HashSet<Type>(FastTypeComparer.Instance);
				Type networkView = typeof(UnityEngine.Object).Assembly.GetType("UnityEngine.NetworkView");
				if (networkView != null)
				{
					NeverDrawTypes.Add(networkView);
				}
				Type guiText = typeof(UnityEngine.Object).Assembly.GetType("UnityEngine.GUIText");
				if (guiText != null)
				{
					NeverDrawTypes.Add(guiText);
				}
				List<Type> unityObjectTypes;
				using (ProfileSection.Start("Finding all UnityObject types"))
				{
					TypeCache.TypeCollection types = TypeCache.GetTypesDerivedFrom(typeof(UnityEngine.Object));
					unityObjectTypes = new List<Type>(types.Count);
					foreach (Type type in types)
					{
						if (!type.IsDefined(typeof(CompilerGeneratedAttribute), inherit: false) && !type.IsDefined(typeof(ObsoleteAttribute), inherit: false) && !NeverDrawTypes.Contains(type) && !typeof(Joint).IsAssignableFrom(type))
						{
							unityObjectTypes.Add(type);
						}
					}
				}
				Dictionary<Type, Type> haveDrawersAlready = new Dictionary<Type, Type>(FastTypeComparer.Instance);
				Dictionary<Type, Type> derivedClassDrawnTypes = new Dictionary<Type, Type>(FastTypeComparer.Instance);
				using (ProfileSection.Start("Search for editors"))
				{
					foreach (Type type2 in unityObjectTypes)
					{
						if (!typeof(UnityEditor.Editor).IsAssignableFrom(type2))
						{
							continue;
						}
						try
						{
							bool editorForChildClasses;
							Type drawnType = InspectorTypeDrawingConfig.GetEditorDrawnType(type2, out editorForChildClasses);
							if (drawnType != null)
							{
								if (!haveDrawersAlready.ContainsKey(drawnType))
								{
									haveDrawersAlready.Add(drawnType, type2);
								}
								if (editorForChildClasses && !derivedClassDrawnTypes.ContainsKey(drawnType))
								{
									derivedClassDrawnTypes.Add(drawnType, type2);
								}
							}
							if (InspectorTypeDrawingConfig.UnityInspectorEditorIsValidBase(type2, null))
							{
								PossibleEditorTypes.Add(type2);
							}
						}
						catch (TypeLoadException)
						{
						}
						catch (ReflectionTypeLoadException)
						{
						}
					}
				}
				using (ProfileSection.Start("Assign editors to Unity objects"))
				{
					HashSet<Type> stopBaseTypeLookUpTypes = new HashSet<Type>(FastTypeComparer.Instance)
					{
						typeof(object),
						typeof(Component),
						typeof(Behaviour),
						typeof(MonoBehaviour),
						typeof(UnityEngine.Object),
						typeof(ScriptableObject),
						typeof(StateMachineBehaviour)
					};
					if (UnityNetworkingUtility.NetworkBehaviourType != null)
					{
						stopBaseTypeLookUpTypes.Add(UnityNetworkingUtility.NetworkBehaviourType);
					}
					foreach (Type type3 in unityObjectTypes)
					{
						if (type3.IsAbstract || typeof(UnityEditor.Editor).IsAssignableFrom(type3) || typeof(EditorWindow).IsAssignableFrom(type3))
						{
							continue;
						}
						Type preExistingEditorType;
						bool haveDrawerAlready = haveDrawersAlready.TryGetValue(type3, out preExistingEditorType);
						if (!haveDrawerAlready)
						{
							Type baseType = type3.BaseType;
							while (baseType != null && !stopBaseTypeLookUpTypes.Contains(baseType))
							{
								if (derivedClassDrawnTypes.TryGetValue(baseType, out var editor))
								{
									haveDrawerAlready = true;
									preExistingEditorType = editor;
									break;
								}
								baseType = baseType.BaseType;
							}
						}
						if (!haveDrawerAlready)
						{
							PossibleDrawnTypes.Add(type3);
						}
						AddTypeToGroups(type3, preExistingEditorType);
						TypeToDrawingEditorMap[type3] = preExistingEditorType;
					}
				}
				using (ProfileSection.Start("Remove non-eligible editor entries"))
				{
					bool fixedAny = false;
					foreach (Type type4 in GlobalConfig<InspectorConfig>.Instance.DrawingConfig.GetAllDrawnTypesWithEntries())
					{
						if (!PossibleDrawnTypes.Contains(type4))
						{
							GlobalConfig<InspectorConfig>.Instance.DrawingConfig.ClearEditorEntryForDrawnType(type4);
							fixedAny = true;
						}
					}
					if (fixedAny)
					{
						AssetDatabase.SaveAssets();
					}
				}
			}
		}

		public static HashSet<Type> GetAllTypesDrawnByOdin()
		{
			HashSet<Type> result = new HashSet<Type>(FastTypeComparer.Instance);
			foreach (KeyValuePair<Type, Type> entry in TypeToDrawingEditorMap)
			{
				Type editor = entry.Value;
				if ((object)editor != null && ((object)editor == typeof(OdinEditor) || typeof(OdinEditor).IsAssignableFrom(editor)))
				{
					result.Add(entry.Key);
				}
			}
			return result;
		}

		public static bool IsOdinDrawingType(Type drawnType)
		{
			Type editor = GetActualDrawingEditorForType(drawnType);
			if ((object)editor == null)
			{
				return false;
			}
			if ((object)editor != typeof(OdinEditor))
			{
				return typeof(OdinEditor).IsAssignableFrom(editor);
			}
			return true;
		}

		public static Type GetActualDrawingEditorForType(Type drawnType)
		{
			TypeToDrawingEditorMap.TryGetValue(drawnType, out var editorType);
			return editorType;
		}

		internal static void UpdateRootGroupHasEligibletypes()
		{
			ScriptTypesRootGroup.UpdateHasEligibleTypes();
			ImportedAssemblyTypesRootGroup.UpdateHasEligibleTypes();
			UnityTypesRootGroup.UpdateHasEligibleTypes();
			OtherTypesRootGroup.UpdateHasEligibleTypes();
		}

		internal static void UpdateRootGroupConflicts()
		{
			ScriptTypesRootGroup.UpdateConflicts();
			ImportedAssemblyTypesRootGroup.UpdateConflicts();
			UnityTypesRootGroup.UpdateConflicts();
			OtherTypesRootGroup.UpdateConflicts();
		}

		private static void SortRootGroups()
		{
			ScriptTypesRootGroup.Sort();
			ImportedAssemblyTypesRootGroup.Sort();
			UnityTypesRootGroup.Sort();
			OtherTypesRootGroup.Sort();
		}

		private static void UpdateRootGroupsSearch(string search, DisplayType displayType)
		{
			ScriptTypesRootGroup.UpdateSearch(search, displayType);
			ImportedAssemblyTypesRootGroup.UpdateSearch(search, displayType);
			UnityTypesRootGroup.UpdateSearch(search, displayType);
			OtherTypesRootGroup.UpdateSearch(search, displayType);
		}

		private static void AddTypeToGroups(Type type, Type preExistingEditorType)
		{
			TypeGroup group = AssemblyUtilities.GetAssemblyCategory(type.Assembly) switch
			{
				AssemblyCategory.Scripts => ScriptTypesRootGroup, 
				AssemblyCategory.ImportedAssemblies => ImportedAssemblyTypesRootGroup, 
				AssemblyCategory.UnityEngine => UnityTypesRootGroup, 
				_ => OtherTypesRootGroup, 
			};
			if (type.Namespace != null)
			{
				string[] groups = type.Namespace.Split(new char[1] { '.' });
				for (int i = 0; i < groups.Length; i++)
				{
					group = group.GetChildGroup(groups[i]);
				}
			}
			group.SubTypes.Add(new TypeGroup.TypePair(type, preExistingEditorType));
		}

		/// <summary>
		/// Determines whether Odin is capable of creating a custom editor for a given type.
		/// </summary>
		public static bool OdinCanCreateEditorFor(Type type)
		{
			return PossibleDrawnTypes.Contains(type);
		}

		/// <summary>
		/// Gets an array of all assigned editor types, and the types they have to draw.
		/// </summary>
		public static TypeDrawerPair[] GetEditors()
		{
			return GetEditorsForCompilation(ScriptTypesRootGroup).AppendWith(GetEditorsForCompilation(ImportedAssemblyTypesRootGroup)).AppendWith(GetEditorsForCompilation(UnityTypesRootGroup)).AppendWith(GetEditorsForCompilation(OtherTypesRootGroup))
				.ToArray();
		}

		private static IEnumerable<TypeDrawerPair> GetEditorsForCompilation(TypeGroup group)
		{
			foreach (TypeGroup.TypePair type in group.SubTypes)
			{
				Type editor = GlobalConfig<InspectorConfig>.Instance.DrawingConfig.GetEditorType(type.DrawnType);
				if (editor != null && editor != typeof(InspectorTypeDrawingConfig.MissingEditor))
				{
					yield return new TypeDrawerPair(type.DrawnType, editor);
				}
			}
			foreach (TypeGroup subGroup in group.SubGroups)
			{
				foreach (TypeDrawerPair item in GetEditorsForCompilation(subGroup))
				{
					yield return item;
				}
			}
		}

		protected override void Initialize()
		{
			if (!initializedForDrawing)
			{
				UpdateRootGroupHasEligibletypes();
				UpdateRootGroupConflicts();
				SortRootGroups();
				UpdateRootGroupsSearch("", DisplayType.AllUnityObjects);
				initializedForDrawing = true;
			}
			searchText = "";
			displayType = DisplayType.AllUnityObjects;
		}

		/// <summary>
		/// Draws the property.
		/// </summary>
		protected override void DrawPropertyLayout(GUIContent label)
		{
			SirenixEditorGUI.BeginHorizontalToolbar();
			GUILayout.Label("Draw Odin for", GUILayoutOptions.ExpandWidth(expand: false));
			GUILayout.FlexibleSpace();
			SirenixEditorGUI.VerticalLineSeparator();
			GUI.changed = false;
			searchText = SirenixEditorGUI.ToolbarSearchField(searchText);
			if (GUI.changed)
			{
				UpdateRootGroupsSearch(searchText, displayType);
			}
			if (SirenixEditorGUI.ToolbarButton(new GUIContent(" Reset to default ")))
			{
				InspectorConfig asset = GlobalConfig<InspectorConfig>.Instance;
				if (EditorUtility.DisplayDialog("Reset " + asset.name + " to default", "Are you sure you want to reset all settings on " + asset.name + " to default values? This cannot be undone.", "Yes", "No"))
				{
					AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(asset));
					AssetDatabase.Refresh();
					UnityEngine.Object.DestroyImmediate(asset);
					GUIHelper.ExitGUI(removeFocusControl: true);
				}
			}
			SirenixEditorGUI.EndHorizontalToolbar();
			SirenixEditorGUI.BeginVerticalList(drawBorder: true, drawDarkBg: false, GUILayoutOptions.ExpandHeight(expand: false));
			scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayoutOptions.ExpandWidth());
			DrawRootTypeGroup(InspectorDefaultEditors.UserTypes, base.ValueEntry, searchText);
			DrawRootTypeGroup(InspectorDefaultEditors.PluginTypes, base.ValueEntry, searchText);
			DrawRootTypeGroup(InspectorDefaultEditors.UnityTypes, base.ValueEntry, searchText);
			DrawRootTypeGroup(InspectorDefaultEditors.OtherTypes, base.ValueEntry, searchText);
			EditorGUILayout.EndScrollView();
			SirenixEditorGUI.EndVerticalList();
		}

		private void DrawRootTypeGroup(InspectorDefaultEditors editorCategory, IPropertyValueEntry<InspectorTypeDrawingConfig> entry, string searchText)
		{
			TypeGroup typeGroup = editorCategory switch
			{
				InspectorDefaultEditors.UserTypes => ScriptTypesRootGroup, 
				InspectorDefaultEditors.PluginTypes => ImportedAssemblyTypesRootGroup, 
				InspectorDefaultEditors.UnityTypes => UnityTypesRootGroup, 
				_ => OtherTypesRootGroup, 
			};
			if (typeGroup.SubTypes.Count == 0 && typeGroup.SubGroups.Count == 0)
			{
				SirenixEditorGUI.BeginListItem(true, null);
				SirenixEditorGUI.BeginIndentedHorizontal();
				GUIHelper.PushGUIEnabled(enabled: false);
				SirenixEditorGUI.IconButton(EditorIcons.TriangleRight, IconStyle, 16);
				GUILayoutUtility.GetRect(16f, 16f, EditorStyles.toggle, GUILayoutOptions.ExpandWidth(expand: false).Width(16f));
				GUILayout.Label(typeGroup.Name);
				GUIHelper.PopGUIEnabled();
				SirenixEditorGUI.EndIndentedHorizontal();
				SirenixEditorGUI.EndListItem();
				return;
			}
			bool useToggle = true;
			Rect rect = SirenixEditorGUI.BeginListItem(true, null);
			bool toggleExpansion = false;
			SirenixEditorGUI.BeginIndentedHorizontal();
			EditorIcon icon = ((typeGroup.IsExpanded || !searchText.IsNullOrWhitespace()) ? EditorIcons.TriangleDown : EditorIcons.TriangleRight);
			toggleExpansion = SirenixEditorGUI.IconButton(icon, IconStyle, 16);
			if (useToggle)
			{
				EditorGUI.showMixedValue = typeGroup.HasConflict;
				bool isToggled = typeGroup.HasConflict || typeGroup.GetSharedEditorType() == typeof(OdinEditor);
				GUI.changed = false;
				isToggled = EditorGUI.Toggle(GUILayoutUtility.GetRect(16f, 16f, EditorStyles.toggle, GUILayoutOptions.ExpandWidth(expand: false).Width(16f)), isToggled);
				if (GUI.changed)
				{
					typeGroup.ClearEditorTypes();
					if (isToggled)
					{
						GlobalConfig<InspectorConfig>.Instance.DefaultEditorBehaviour |= editorCategory;
					}
					else
					{
						GlobalConfig<InspectorConfig>.Instance.DefaultEditorBehaviour = GlobalConfig<InspectorConfig>.Instance.DefaultEditorBehaviour & ~editorCategory;
					}
					EditorUtility.SetDirty(GlobalConfig<InspectorConfig>.Instance);
				}
				EditorGUI.showMixedValue = false;
			}
			else
			{
				GUILayout.Label("TODO: DROPDOWN!");
			}
			GUILayout.Label(typeGroup.Name);
			SirenixEditorGUI.EndIndentedHorizontal();
			if (toggleExpansion || (Event.current.type == EventType.MouseDown && rect.Contains(Event.current.mousePosition)))
			{
				typeGroup.IsExpanded = !typeGroup.IsExpanded;
				Event.current.Use();
			}
			SirenixEditorGUI.EndListItem();
			if (SirenixEditorGUI.BeginFadeGroup(typeGroup, typeGroup.IsExpanded || !searchText.IsNullOrWhitespace()))
			{
				EditorGUI.indentLevel++;
				foreach (TypeGroup.TypePair subType in typeGroup.SubTypes)
				{
					if (typeGroup.IsTypeVisible(subType.DrawnType))
					{
						DrawType(subType, entry);
					}
				}
				foreach (TypeGroup subGroup in typeGroup.SubGroups)
				{
					DrawTypeGroup(subGroup, entry, searchText);
				}
				EditorGUI.indentLevel--;
			}
			SirenixEditorGUI.EndFadeGroup();
		}

		private void DrawTypeGroup(TypeGroup typeGroup, IPropertyValueEntry<InspectorTypeDrawingConfig> entry, string searchText)
		{
			if (!typeGroup.IsSearchVisible)
			{
				return;
			}
			bool useToggle = true;
			Rect rect = SirenixEditorGUI.BeginListItem(true, null);
			bool toggleExpansion = false;
			SirenixEditorGUI.BeginIndentedHorizontal();
			EditorIcon icon = ((typeGroup.IsExpanded || !searchText.IsNullOrWhitespace()) ? EditorIcons.TriangleDown : EditorIcons.TriangleRight);
			toggleExpansion = SirenixEditorGUI.IconButton(icon, IconStyle, 16);
			if (!typeGroup.HasEligibleTypes)
			{
				toggleExpansion |= SirenixEditorGUI.IconButton(EditorIcons.Transparent, 20);
			}
			else if (useToggle)
			{
				EditorGUI.showMixedValue = typeGroup.HasConflict;
				bool isToggled = typeGroup.HasConflict || typeGroup.GetSharedEditorType() == typeof(OdinEditor);
				GUI.changed = false;
				isToggled = EditorGUI.Toggle(GUILayoutUtility.GetRect(16f, 16f, EditorStyles.toggle, GUILayoutOptions.ExpandWidth(expand: false).Width(16f)), isToggled);
				if (GUI.changed)
				{
					typeGroup.SetSharedEditorType(isToggled ? typeof(OdinEditor) : null);
					UpdateRootGroupConflicts();
				}
				EditorGUI.showMixedValue = false;
			}
			else
			{
				GUILayout.Label("TODO: DROPDOWN!");
			}
			GUILayout.Label(typeGroup.Name);
			SirenixEditorGUI.EndIndentedHorizontal();
			if (toggleExpansion || (Event.current.type == EventType.MouseDown && rect.Contains(Event.current.mousePosition)))
			{
				typeGroup.IsExpanded = !typeGroup.IsExpanded;
				Event.current.Use();
			}
			SirenixEditorGUI.EndListItem();
			if (SirenixEditorGUI.BeginFadeGroup(typeGroup, typeGroup.IsExpanded || !searchText.IsNullOrWhitespace()))
			{
				EditorGUI.indentLevel++;
				foreach (TypeGroup.TypePair subType in typeGroup.SubTypes)
				{
					if (typeGroup.IsTypeVisible(subType.DrawnType))
					{
						DrawType(subType, entry);
					}
				}
				foreach (TypeGroup subGroup in typeGroup.SubGroups)
				{
					DrawTypeGroup(subGroup, entry, searchText);
				}
				EditorGUI.indentLevel--;
			}
			SirenixEditorGUI.EndFadeGroup();
		}

		private void DrawType(TypeGroup.TypePair typeToDraw, IPropertyValueEntry<InspectorTypeDrawingConfig> entry)
		{
			Type currentEditorType = typeToDraw.PreExistingEditorType;
			bool conflict = false;
			if (currentEditorType == null)
			{
				for (int i = 0; i < entry.Values.Count; i++)
				{
					Type type = entry.Values[i].GetEditorType(typeToDraw.DrawnType);
					if (i == 0)
					{
						currentEditorType = type;
					}
					else if (type != currentEditorType)
					{
						currentEditorType = null;
						conflict = true;
						break;
					}
				}
			}
			bool useToggle = true;
			SirenixEditorGUI.BeginListItem(true, null);
			SirenixEditorGUI.BeginIndentedHorizontal();
			SirenixEditorGUI.IconButton(EditorIcons.Transparent, IconStyle, 16);
			if (typeToDraw.PreExistingEditorType != null)
			{
				SirenixEditorGUI.IconButton(EditorIcons.Transparent, IconStyle, 16);
				GUILayout.Label(typeToDraw.DrawnType.GetNiceName());
				GUILayout.Label("Drawn by '" + typeToDraw.PreExistingEditorType?.ToString() + "'", SirenixGUIStyles.RightAlignedGreyMiniLabel);
				for (int j = 0; j < entry.Values.Count; j++)
				{
					if (entry.Values[j].HasEntryForType(typeToDraw.DrawnType))
					{
						entry.Values[j].ClearEditorEntryForDrawnType(typeToDraw.DrawnType);
					}
				}
			}
			else
			{
				EditorGUI.showMixedValue = conflict;
				if (useToggle)
				{
					bool isToggled = currentEditorType == typeof(OdinEditor);
					GUI.changed = false;
					isToggled = EditorGUI.Toggle(GUILayoutUtility.GetRect(16f, 16f, EditorStyles.toggle, GUILayoutOptions.ExpandWidth(expand: false).Width(16f)), isToggled);
					if (GUI.changed)
					{
						for (int k = 0; k < entry.Values.Count; k++)
						{
							entry.Values[k].SetEditorType(typeToDraw.DrawnType, isToggled ? typeof(OdinEditor) : null);
						}
						UpdateRootGroupConflicts();
					}
					GUILayout.Label(typeToDraw.DrawnType.GetNiceName());
				}
				else
				{
					GUILayout.Label("TODO: DROPDOWN!");
				}
			}
			SirenixEditorGUI.EndIndentedHorizontal();
			EditorGUI.showMixedValue = false;
			SirenixEditorGUI.EndListItem();
		}
	}
}
