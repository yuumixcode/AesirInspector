using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.Config;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor
{
	public class TypeRegistryUserConfigWindow : OdinMenuEditorWindow
	{
		[Flags]
		internal enum FilterMode
		{
			None = 0,
			Visible = 1,
			Hidden = 2,
			Illegal = 4,
			Valid = 3,
			All = 7
		}

		internal class TypeRegistryMenuItem : OdinMenuItem
		{
			public readonly TypeItemSettingsAccessor Accessor;

			public readonly Type Type;

			private bool isInitialized;

			public TypeRegistryMenuItem(OdinMenuTree tree, string name, Type value)
				: base(tree, name, value)
			{
				Type = value;
				Accessor = new TypeItemSettingsAccessor(value);
				base.Value = Accessor;
			}

			public override void DrawMenuItem(int indentLevel)
			{
				if (!isInitialized)
				{
					Initialize();
					isInitialized = true;
				}
				bool isModified = Accessor.IsModified;
				if (isModified)
				{
					GUIHelper.PushIsBoldLabel(isBold: true);
				}
				Rect visibilityToggleRect = rect.AlignRight(20f).SubX(base.MenuTree.Config.DefaultMenuStyle.BorderPadding).Padding(2f);
				if (Event.current.OnMouseDown(visibilityToggleRect, 0))
				{
					Accessor.Visible = !Accessor.Visible;
				}
				base.DrawMenuItem(indentLevel);
				if (isModified)
				{
					GUIHelper.PopIsBoldLabel();
				}
				if (!Accessor.Visible)
				{
					EditorGUI.DrawRect(rect, ItemHiddenOverlay);
				}
				SdfIconType visibleIcon = (Accessor.Visible ? SdfIconType.EyeFill : SdfIconType.EyeSlashFill);
				if (Event.current.IsMouseOver(visibilityToggleRect))
				{
					SdfIcons.DrawIcon(visibilityToggleRect, visibleIcon, Color.white);
				}
				else
				{
					SdfIcons.DrawIcon(visibilityToggleRect, visibleIcon);
				}
				if (Accessor.IsIllegal)
				{
					EditorGUI.DrawRect(rect.AlignLeft(2f), ItemIllegal);
				}
			}

			private void Initialize()
			{
				if (string.IsNullOrEmpty(Type.Namespace))
				{
					base.Name = Type.GetNiceName() + " (" + Type.Assembly.GetName().Name + ")";
				}
				else
				{
					base.Name = Type.GetNiceName();
				}
				Accessor?.Initialize();
			}
		}

		internal class TypeItemSettingsAccessor
		{
			[HideInInspector]
			public bool IsRemoved;

			internal Type Type;

			internal TypeRegistryItemAttribute ItemAttribute;

			internal string NiceName;

			private bool isInitialized;

			public bool IsModified => GlobalConfig<TypeRegistryUserConfig>.Instance.IsModified(Type);

			[ShowInInspector]
			[DisableIf("@$property.ValueEntry.ValueCount > 1 || this.Type.IsGenericType")]
			public string DisplayName
			{
				get
				{
					if (Settings == null)
					{
						return NiceName;
					}
					if (!string.IsNullOrEmpty(Settings.Name))
					{
						return Settings.Name;
					}
					return NiceName;
				}
				set
				{
					if (Settings == null)
					{
						Settings = new TypeSettings();
					}
					Settings.Name = value;
					GlobalConfig<TypeRegistryUserConfig>.Instance.HandleDefaultSettings(Type, Settings, ItemAttribute);
				}
			}

			[ShowInInspector]
			public string Category
			{
				get
				{
					if (Settings != null && !string.IsNullOrEmpty(Settings.Category))
					{
						return Settings.Category;
					}
					if (ItemAttribute != null && !string.IsNullOrEmpty(ItemAttribute.CategoryPath))
					{
						return ItemAttribute.CategoryPath;
					}
					return string.Empty;
				}
				set
				{
					if (Settings == null)
					{
						Settings = new TypeSettings();
					}
					Settings.Category = value;
					GlobalConfig<TypeRegistryUserConfig>.Instance.HandleDefaultSettings(Type, Settings, ItemAttribute);
				}
			}

			[BoxGroup("IconGroup", true, false, 0f, LabelText = "Icon Settings")]
			[ShowInInspector]
			public SdfIconType Icon
			{
				get
				{
					if (Settings != null && Settings.Icon != SdfIconType.None)
					{
						return Settings.Icon;
					}
					if (ItemAttribute != null && ItemAttribute.Icon != SdfIconType.None)
					{
						return ItemAttribute.Icon;
					}
					return SdfIconType.None;
				}
				set
				{
					if (Settings == null)
					{
						Settings = new TypeSettings();
					}
					Settings.Icon = value;
					GlobalConfig<TypeRegistryUserConfig>.Instance.HandleDefaultSettings(Type, Settings, ItemAttribute);
				}
			}

			[BoxGroup("IconGroup", true, false, 0f)]
			[ShowInInspector]
			public Color LightModeColor
			{
				get
				{
					return Settings?.LightIconColor ?? ItemAttribute?.LightIconColor ?? Color.clear;
				}
				set
				{
					if (Settings == null)
					{
						Settings = new TypeSettings();
					}
					Settings.LightIconColor = value;
					GlobalConfig<TypeRegistryUserConfig>.Instance.HandleDefaultSettings(Type, Settings, ItemAttribute);
				}
			}

			[BoxGroup("IconGroup", true, false, 0f)]
			[ShowInInspector]
			public Color DarkModeColor
			{
				get
				{
					return Settings?.DarkIconColor ?? ItemAttribute?.DarkIconColor ?? Color.clear;
				}
				set
				{
					if (Settings == null)
					{
						Settings = new TypeSettings();
					}
					Settings.DarkIconColor = value;
					GlobalConfig<TypeRegistryUserConfig>.Instance.HandleDefaultSettings(Type, Settings, ItemAttribute);
				}
			}

			[ShowInInspector]
			public bool Visible
			{
				get
				{
					return GlobalConfig<TypeRegistryUserConfig>.Instance.IsVisible(Type);
				}
				set
				{
					GlobalConfig<TypeRegistryUserConfig>.Instance.SetVisibility(Type, value);
				}
			}

			[ShowInInspector]
			public bool IsIllegal
			{
				get
				{
					return GlobalConfig<TypeRegistryUserConfig>.Instance.IsIllegal(Type);
				}
				set
				{
					GlobalConfig<TypeRegistryUserConfig>.Instance.SetIllegal(Type, value);
				}
			}

			[ShowInInspector]
			public int Priority
			{
				get
				{
					int priority = GlobalConfig<TypeRegistryUserConfig>.Instance.GetPriority(Type);
					if (priority != 0)
					{
						return priority;
					}
					if (ItemAttribute != null)
					{
						return ItemAttribute.Priority;
					}
					return 0;
				}
				set
				{
					GlobalConfig<TypeRegistryUserConfig>.Instance.SetPriority(Type, value, ItemAttribute);
				}
			}

			internal TypeSettings Settings
			{
				get
				{
					return GlobalConfig<TypeRegistryUserConfig>.Instance.TryGetSettings(Type);
				}
				set
				{
					GlobalConfig<TypeRegistryUserConfig>.Instance.SetSettings(Type, value);
				}
			}

			[EnableIf("@this.CanReset($property.Parent)")]
			[Button(ButtonSizes.Large)]
			public void Reset()
			{
				GlobalConfig<TypeRegistryUserConfig>.Instance.ResetType(Type);
			}

			internal TypeItemSettingsAccessor(Type type)
			{
				Type = type;
				ItemAttribute = type.GetCustomAttribute<TypeRegistryItemAttribute>();
				NiceName = type.Name;
			}

			internal bool CanReset(InspectorProperty property)
			{
				if (property == null)
				{
					return false;
				}
				for (int i = 0; i < property.ValueEntry.ValueCount; i++)
				{
					if (((TypeItemSettingsAccessor)property.ValueEntry.WeakValues[i]).IsModified)
					{
						return true;
					}
				}
				return false;
			}

			[OnInspectorGUI]
			internal void Initialize()
			{
				if (!isInitialized)
				{
					if (ItemAttribute != null && !string.IsNullOrEmpty(ItemAttribute.Name))
					{
						NiceName = ItemAttribute.Name;
					}
					else
					{
						NiceName = Type.GetNiceName();
					}
					isInitialized = true;
				}
			}
		}

		public const int TOOLBAR_HEIGHT = 28;

		public const int ITEM_HEIGHT = 24;

		public const float ICON_PADDING = 2f;

		private static readonly Color BackgroundLight = new Color(0.7607844f, 0.7607844f, 0.7607844f, 1f);

		private static readonly Color HeaderLight = new Color(0.8235295f, 0.8235295f, 0.8235295f, 1f);

		private static readonly Color ItemHiddenOverlayLight = new Color(0f, 0f, 0f, 0.15f);

		private static readonly Color ItemIllegalLight = new Color(1f, 1f, 0f, 1f);

		private static readonly Color BackgroundDark = new Color(0.171f, 0.171f, 0.171f, 1f);

		private static readonly Color HeaderDark = new Color(0.2431373f, 0.2431373f, 0.2431373f, 1f);

		private static readonly Color ItemHiddenOverlayDark = new Color(0f, 0f, 0f, 0.1803922f);

		private static readonly Color ItemIllegalDark = new Color(1f, 1f, 0f, 1f);

		public Type TypeToScrollTo;

		private OdinMenuItem selectedNamespaceItem;

		private string selectedNamespace;

		private FilterMode filterMode = FilterMode.Visible | FilterMode.Illegal;

		private Rect topRect;

		public static Color Background
		{
			get
			{
				if (!EditorGUIUtility.isProSkin)
				{
					return BackgroundLight;
				}
				return BackgroundDark;
			}
		}

		public static Color Header
		{
			get
			{
				if (!EditorGUIUtility.isProSkin)
				{
					return HeaderLight;
				}
				return HeaderDark;
			}
		}

		public static Color ItemHiddenOverlay
		{
			get
			{
				if (!EditorGUIUtility.isProSkin)
				{
					return ItemHiddenOverlayLight;
				}
				return ItemHiddenOverlayDark;
			}
		}

		public static Color ItemIllegal
		{
			get
			{
				if (!EditorGUIUtility.isProSkin)
				{
					return ItemIllegalLight;
				}
				return ItemIllegalDark;
			}
		}

		protected override void OnEnable()
		{
			base.OnEnable();
			filterMode = FilterMode.Visible | FilterMode.Illegal;
			base.minSize = new Vector2(960f, 600f);
			base.titleContent = new GUIContent("Type Registry Editor");
		}

		private void Awake()
		{
			filterMode = FilterMode.Visible | FilterMode.Illegal;
			base.minSize = new Vector2(960f, 600f);
		}

		protected override IEnumerable<object> GetTargets()
		{
			if (base.MenuTree.Selection.Count > 0)
			{
				if (base.MenuTree.Selection.SelectedValue == null)
				{
					yield return null;
				}
				else
				{
					yield return base.MenuTree.Selection.SelectedValues.ToList();
				}
			}
		}

		protected override void OnImGUI()
		{
			if (TypeToScrollTo != null && base.MenuTree != null)
			{
				foreach (OdinMenuItem odinMenuItem in base.MenuTree.EnumerateTree())
				{
					if (odinMenuItem is TypeRegistryMenuItem item && TypeToScrollTo == item.Type)
					{
						base.MenuTree.Selection.Clear();
						odinMenuItem.Select();
						base.MenuTree.ScrollToMenuItem(odinMenuItem, centerMenuItem: true);
						break;
					}
				}
				TypeToScrollTo = null;
			}
			float windowWidth = base.position.width;
			MenuWidth = Mathf.Clamp(MenuWidth, windowWidth - 500f, windowWidth - 200f);
			Rect topRectPlaceholder = GUILayoutUtility.GetRect(0f, 28f, GUILayoutOptions.ExpandWidth());
			if (!topRectPlaceholder.IsPlaceholder())
			{
				topRect = topRectPlaceholder;
			}
			EditorGUI.DrawRect(base.position.SetPosition(Vector2.zero).AlignLeft(MenuWidth), Background);
			base.OnImGUI();
			EditorGUI.DrawRect(topRect, Header);
			Rect leftTop = topRect.AlignLeft(MenuWidth + 1f);
			Rect namespaceSelectorRect = leftTop.TakeFromRight(240f);
			Rect filterRect = leftTop.TakeFromRight(180f);
			GUILayout.BeginArea(leftTop);
			if (base.MenuTree != null)
			{
				base.MenuTree.DrawSearchToolbar();
			}
			GUILayout.EndArea();
			OdinSelector<OdinMenuItem>.DrawSelectorDropdown(namespaceSelectorRect, selectedNamespaceItem?.Name ?? "All Namespaces", NamespaceCategorySelector, EditorStyles.toolbarDropDown);
			EditorGUI.BeginChangeCheck();
			filterMode = EnumSelector<FilterMode>.DrawEnumField(filterRect, null, GUIHelper.TempContent($"Show: {filterMode}"), filterMode, EditorStyles.toolbarDropDown);
			if (EditorGUI.EndChangeCheck())
			{
				RebuildMenuTree();
			}
			Rect topRight = topRect.AlignRight(topRect.width - MenuWidth);
			topRight = topRight.Padding(4f);
			Rect iconPosition = topRight.TakeFromLeft(30f);
			if (base.MenuTree.Selection.Count > 0 && base.MenuTree.Selection[0] is TypeRegistryMenuItem item2)
			{
				if (typeof(UnityEngine.Object).IsAssignableFrom(item2.Type))
				{
					GUI.DrawTexture(iconPosition, EditorIcons.UnityLogo, ScaleMode.ScaleToFit);
				}
				else
				{
					SdfIcons.DrawIcon(iconPosition, SdfIconType.PuzzleFill);
				}
			}
			else
			{
				SdfIcons.DrawIcon(iconPosition, SdfIconType.PuzzleFill);
			}
			topRight.width -= 30f;
			if (base.MenuTree.Selection.Count == 1)
			{
				GUI.Label(topRight, base.MenuTree.Selection[0].Name, SirenixGUIStyles.TitleCentered);
			}
			else if (base.MenuTree.Selection.Count > 1)
			{
				GUI.Label(topRight, $"{base.MenuTree.Selection[0].Name} (+{base.MenuTree.Selection.Count - 1})", SirenixGUIStyles.TitleCentered);
			}
			else
			{
				GUI.Label(topRight, "None", SirenixGUIStyles.TitleCentered);
			}
		}

		private void RebuildMenuTree()
		{
			base.MenuTree?.Selection?.Clear();
			ForceMenuTreeRebuild();
		}

		protected override OdinMenuTree BuildMenuTree()
		{
			OdinMenuTree odinMenuTree = new OdinMenuTree();
			odinMenuTree.Selection.SupportsMultiSelect = true;
			odinMenuTree.Config = new OdinMenuTreeDrawingConfig
			{
				EXPERIMENTAL_INTERNAL_SparseFixedLayouting = true,
				DrawSearchToolbar = false,
				DefaultMenuStyle = 
				{
					IconPadding = 2f,
					Height = 24
				}
			};
			OdinMenuTree tree = odinMenuTree;
			if (TypeToScrollTo != null && !GlobalConfig<TypeRegistryUserConfig>.Instance.IsVisible(TypeToScrollTo))
			{
				filterMode = FilterMode.All;
			}
			WindowPadding = new Vector4(8f, 8f, 8f, 8f);
			bool validateNamespace = !string.IsNullOrEmpty(selectedNamespace);
			foreach (Type type in AssemblyUtilities.GetTypes(AssemblyCategory.All))
			{
				if (TypeRegistry.IsModifiableType(type) && IsTypeVisible(type) && (!validateNamespace || (!string.IsNullOrEmpty(type.Namespace) && type.Namespace.StartsWith(selectedNamespace))))
				{
					TypeRegistryMenuItem item = AddTypeToTree(tree, type);
					if (TypeToScrollTo != null && type == TypeToScrollTo)
					{
						item?.Select();
					}
				}
			}
			TypeToScrollTo = null;
			tree.CollapseEmptyItems();
			tree.SortMenuItemsByName();
			foreach (OdinMenuItem item2 in tree.EnumerateTree())
			{
				if (item2.Value != null)
				{
					continue;
				}
				item2.SdfIcon = SdfIconType.CollectionFill;
				item2.IsSelectable = false;
				item2.OnDrawItem = (Action<OdinMenuItem>)Delegate.Combine(item2.OnDrawItem, (Action<OdinMenuItem>)delegate(OdinMenuItem menuItem)
				{
					if (Event.current.OnMouseDown(menuItem.rect, 0))
					{
						menuItem.Toggled = !menuItem.Toggled;
					}
				});
			}
			tree.UpdateMenuTree();
			return tree;
		}

		private static string GetFullNameWithoutSlashes(OdinMenuItem item)
		{
			OdinMenuItem parent = item.Parent;
			string result = item.Name.Replace('/', '.');
			while (parent != null)
			{
				result = parent.Name.Replace('/', '.') + "/" + result;
				parent = parent.Parent;
			}
			return result;
		}

		private static string GetFullNameAsNamespace(OdinMenuItem item)
		{
			OdinMenuItem parent = item.Parent;
			string result = item.Name;
			while (parent != null)
			{
				result = parent.Name + "/" + result;
				parent = parent.Parent;
			}
			return result.Replace('/', '.');
		}

		private OdinSelector<OdinMenuItem> NamespaceCategorySelector(Rect rect)
		{
			GenericSelector<OdinMenuItem> result = new GenericSelector<OdinMenuItem>();
			result.SelectionTree.Add("<All>", null);
			foreach (OdinMenuItem item in base.MenuTree.EnumerateTree())
			{
				if (item.ChildMenuItems.Count > 0)
				{
					result.SelectionTree.Add(GetFullNameWithoutSlashes(item), item);
				}
			}
			foreach (OdinMenuItem item2 in result.SelectionTree.EnumerateTree())
			{
				item2.Name = item2.Name.Replace('.', '/');
			}
			result.SelectionConfirmed += delegate(IEnumerable<OdinMenuItem> enumerable)
			{
				OdinMenuItem odinMenuItem = (selectedNamespaceItem = enumerable.FirstOrDefault());
				selectedNamespace = ((odinMenuItem == null) ? string.Empty : GetFullNameAsNamespace(odinMenuItem));
				RebuildMenuTree();
			};
			result.ShowInPopup(rect);
			return result;
		}

		private bool IsTypeVisible(Type type)
		{
			TypeRegistryUserConfig userConfig = GlobalConfig<TypeRegistryUserConfig>.Instance;
			switch (filterMode)
			{
			case FilterMode.None:
				return false;
			case FilterMode.Visible:
				return userConfig.IsVisible(type);
			case FilterMode.Hidden:
				return !userConfig.IsVisible(type);
			case FilterMode.Illegal:
				return userConfig.IsIllegal(type);
			case FilterMode.Visible | FilterMode.Illegal:
				if (!userConfig.IsVisible(type))
				{
					return userConfig.IsIllegal(type);
				}
				return true;
			case FilterMode.Hidden | FilterMode.Illegal:
				if (userConfig.IsVisible(type))
				{
					return userConfig.IsIllegal(type);
				}
				return true;
			case FilterMode.Valid:
				return !userConfig.IsIllegal(type);
			case FilterMode.All:
				return true;
			default:
				throw new ArgumentOutOfRangeException("filterMode", filterMode, null);
			}
		}

		private static TypeRegistryMenuItem AddTypeToTree(OdinMenuTree tree, Type type)
		{
			if (TypeRegistry.IsGeneratedType(type))
			{
				return null;
			}
			string name = type.Name;
			TypeRegistryMenuItem item = new TypeRegistryMenuItem(tree, name, type)
			{
				SearchString = name + ", " + type.FullName
			};
			if (typeof(UnityEngine.Object).IsAssignableFrom(type))
			{
				item.Icon = EditorIcons.UnityLogo;
			}
			else
			{
				item.SdfIcon = SdfIconType.PuzzleFill;
			}
			if (!string.IsNullOrEmpty(type.Namespace))
			{
				string path = TypeRegistry.GetNamespacePath(type);
				tree.AddMenuItemAtPath(path, item);
			}
			else
			{
				item.Name = name + " (" + type.Assembly.GetName().Name + ")";
				tree.MenuItems.Add(item);
			}
			return item;
		}
	}
}
