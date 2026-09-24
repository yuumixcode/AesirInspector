using System;
using System.Collections.Generic;
using Sirenix.Config;
using Sirenix.OdinInspector.Editor.ValueResolvers;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor
{
	[HideReferenceObjectPicker]
	public class TypeSelectorV2 : OdinSelector<Type>
	{
		internal class TypeSelectorMenuItem : OdinMenuItem
		{
			public bool HasDefaultConstructor;

			public TypeSelectorV2 CurrentSelector;

			public readonly Type Type;

			private bool isInitialized;

			public string GetPath()
			{
				string result = base.Name;
				for (OdinMenuItem nextParent = base.Parent; nextParent != null; nextParent = nextParent.Parent)
				{
					result = nextParent.Name + "/" + result;
				}
				return result;
			}

			public TypeSelectorMenuItem(TypeSelectorV2 selector, OdinMenuTree tree, string name, Type value)
				: base(tree, name, value)
			{
				CurrentSelector = selector;
				base.SearchString = name + ", " + value.FullName;
				Type = value;
			}

			public override void DrawMenuItem(int indentLevel)
			{
				if (!isInitialized)
				{
					Initialize();
					isInitialized = true;
				}
				base.DrawMenuItem(indentLevel);
				bool showNoDefaultCtorTooltip = false;
				bool isIllegal = GlobalConfig<TypeRegistryUserConfig>.Instance.IsIllegal(Type);
				if (!Type.IsInterface && !HasDefaultConstructor)
				{
					Rect defaultCtorPosition = rect.AlignLeft(rect.height);
					SdfIcons.DrawIcon(defaultCtorPosition.Padding(6f, 8f, 6f, 6f), SdfIconType.Exclamation);
					showNoDefaultCtorTooltip = true;
				}
				if (isIllegal)
				{
					EditorGUI.DrawRect(rect, new Color(1f, 1f, 0f, 0.05f));
				}
				if (isIllegal && showNoDefaultCtorTooltip)
				{
					GUI.Label(rect, GUIHelper.TempContent(string.Empty, "This type is illegal, and has no default constructor."));
				}
				else if (isIllegal)
				{
					GUI.Label(rect, GUIHelper.TempContent(string.Empty, "This type is illegal."));
				}
				else if (showNoDefaultCtorTooltip)
				{
					GUI.Label(rect, GUIHelper.TempContent(string.Empty, "This type has no default constructor."));
				}
			}

			private void Initialize()
			{
				if (CurrentSelector.duplicatePaths.Contains(GetPath()))
				{
					if (CurrentSelector.ShowCategories)
					{
						if (TypeRegistry.HasCustomName(Type))
						{
							base.Name = TypeRegistry.GetNiceName(Type) + " (" + Type.Name + ")";
						}
						else
						{
							base.Name = TypeRegistry.GetNiceName(Type) + " (" + Type.Assembly.GetName().Name + ", " + Type.Namespace + ")";
						}
					}
					else if (TypeRegistry.HasCustomName(Type))
					{
						base.Name = TypeRegistry.GetNiceName(Type) + " (" + Type.FullName + ")";
					}
					else
					{
						base.Name = TypeRegistry.GetNiceName(Type) + " (" + Type.Assembly.GetName().Name + ", " + Type.Namespace + ")";
					}
				}
				else
				{
					base.Name = TypeRegistry.GetNiceName(Type);
				}
				if (CurrentSelector.HideNonDefaultCtorInfo)
				{
					HasDefaultConstructor = true;
				}
				else if (Type.IsInterface || Type.IsAbstract)
				{
					HasDefaultConstructor = true;
				}
				else
				{
					HasDefaultConstructor = Type.HasDefaultConstructor();
				}
				TypeRegistryUserConfig userConfig = GlobalConfig<TypeRegistryUserConfig>.Instance;
				if (userConfig.IsIllegal(Type))
				{
					SdfIcon = SdfIconType.ExclamationTriangleFill;
					SdfIconColor = SirenixGUIStyles.YellowWarningColor;
					return;
				}
				bool isUnityType = typeof(UnityEngine.Object).IsAssignableFrom(Type);
				if (TypeRegistry.TryGetIcon(Type, out var icon, out var iconColor))
				{
					SdfIcon = icon;
					SdfIconColor = iconColor;
					return;
				}
				if (!isUnityType)
				{
					SdfIcon = SdfIconType.PuzzleFill;
					return;
				}
				base.Icon = GUIHelper.GetAssetThumbnail(null, Type, preferObjectPreviewOverFileIcon: false);
				if (base.Icon == null)
				{
					base.Icon = EditorIcons.UnityLogo;
				}
			}
		}

		internal abstract class TypeSelectorNoneValue
		{
		}

		internal abstract class TypeSelectorAllUnityTypes
		{
		}

		private const string FIND_UNITY_OBJECT_ITEM_NAME = "Find Unity Object";

		[HideInInspector]
		public bool SupportsMultiSelect;

		[HideInInspector]
		public Type SelectedType;

		[HideInInspector]
		public bool ShowNoneItem;

		[HideInInspector]
		public bool ShowCategories;

		[HideInInspector]
		public bool PreferNamespaces;

		[HideInInspector]
		public bool ShowHiddenTypes;

		internal bool CategorizeUnityObjects;

		internal bool HideNonDefaultCtorInfo = true;

		internal bool useSingleClick;

		private OdinMenuItem noneMenuItem;

		private OdinMenuItem findUnityObjectItem;

		private ValueResolver<bool> filterItemsFunction;

		private IEnumerable<Type> types;

		private readonly Dictionary<string, OdinMenuItem> categories = new Dictionary<string, OdinMenuItem>();

		private readonly List<OdinMenuItem> pathCache = new List<OdinMenuItem>(8);

		private readonly HashSet<string> duplicatePaths = new HashSet<string>();

		private readonly HashSet<string> addedNamesForFlatTree = new HashSet<string>();

		private Rect specialItemsRect = Rect.zero;

		public override string Title => null;

		public TypeSelectorV2(AssemblyCategory assemblyCategory, bool supportsMultiSelect = false, Type selectedType = null, bool? showCategories = null, bool showHidden = false, bool? preferNamespaces = null, bool? showNoneItem = null)
			: this(supportsMultiSelect, selectedType, showCategories, showHidden, preferNamespaces, showNoneItem, null)
		{
			types = TypeRegistry.GetValidTypesInCategory(assemblyCategory);
		}

		public TypeSelectorV2(IEnumerable<Type> types, bool supportsMultiSelect = false, Type selectedType = null, bool? showCategories = null, bool showHidden = false, bool? preferNamespaces = null, bool? showNoneItem = null)
			: this(supportsMultiSelect, selectedType, showCategories, showHidden, preferNamespaces, showNoneItem, null)
		{
			this.types = types;
		}

		internal TypeSelectorV2(AssemblyCategory assemblyCategory, bool supportsMultiSelect, Type selectedType, bool? showCategories, bool showHidden, bool? preferNamespaces, bool? showNoneItem, InspectorProperty property)
			: this(supportsMultiSelect, selectedType, showCategories, showHidden, preferNamespaces, showNoneItem, property)
		{
			types = TypeRegistry.GetValidTypesInCategory(assemblyCategory);
		}

		internal TypeSelectorV2(IEnumerable<Type> types, bool supportsMultiSelect, Type selectedType, bool? showCategories, bool showHidden, bool? preferNamespaces, bool? showNoneItem, InspectorProperty property)
			: this(supportsMultiSelect, selectedType, showCategories, showHidden, preferNamespaces, showNoneItem, property)
		{
			this.types = types;
		}

		protected TypeSelectorV2(bool supportsMultiSelect, Type selectedType, bool? showCategories, bool showHidden, bool? preferNamespaces, bool? showNoneItem, InspectorProperty property)
		{
			SupportsMultiSelect = supportsMultiSelect;
			SelectedType = selectedType;
			ShowNoneItem = showNoneItem ?? GlobalConfig<GeneralDrawerConfig>.Instance.showNoneItem;
			ShowCategories = showCategories ?? GlobalConfig<GeneralDrawerConfig>.Instance.showCategoriesByDefault;
			ShowHiddenTypes = showHidden;
			PreferNamespaces = preferNamespaces ?? GlobalConfig<GeneralDrawerConfig>.Instance.preferNamespacesOverAssemblyCategories;
			TypeSelectorSettingsAttribute settings = property?.GetAttribute<TypeSelectorSettingsAttribute>();
			if (settings == null)
			{
				return;
			}
			if (settings.ShowCategoriesIsSet)
			{
				ShowCategories = settings.ShowCategories;
			}
			if (settings.PreferNamespacesIsSet)
			{
				PreferNamespaces = settings.PreferNamespaces;
			}
			if (settings.ShowNoneItemIsSet)
			{
				ShowNoneItem = settings.ShowNoneItem;
			}
			if (!string.IsNullOrEmpty(settings.FilterTypesFunction))
			{
				NamedValue typeNamedValue = new NamedValue("type", typeof(Type), null);
				filterItemsFunction = ValueResolver.Get<bool>(property, settings.FilterTypesFunction, new NamedValue[1] { typeNamedValue });
				if (filterItemsFunction.HasError)
				{
					Debug.LogWarning(filterItemsFunction.ErrorMessage);
					filterItemsFunction = null;
				}
			}
		}

		public OdinEditorWindow ShowInAux()
		{
			Vector2 windowSize = new Vector2(700f, 500f);
			Rect windowRect = GUIHelper.GetEditorWindowRect();
			OdinEditorWindow window = ShowInPopup();
			window.position = UnityShims.Rect.Ctor(windowRect.center - windowSize * 0.5f, new Vector2(windowSize.x, window.position.height));
			Vector2 minSize = window.minSize;
			Vector2 maxSize = window.maxSize;
			window.minSize = new Vector2(windowSize.x, minSize.y);
			window.maxSize = new Vector2(windowSize.x, maxSize.y);
			return window;
		}

		protected override float DefaultWindowWidth()
		{
			return 450f;
		}

		protected override void BuildSelectionTree(OdinMenuTree tree)
		{
			categories.Clear();
			pathCache.Clear();
			duplicatePaths.Clear();
			addedNamesForFlatTree.Clear();
			tree.Config.SelectMenuItemsOnMouseDown = true;
			tree.Config.EXPERIMENTAL_INTERNAL_SparseFixedLayouting = true;
			tree.Selection.SupportsMultiSelect = SupportsMultiSelect;
			if (CategorizeUnityObjects)
			{
				findUnityObjectItem = new OdinMenuItem(tree, "Find Unity Object", typeof(TypeSelectorAllUnityTypes));
				tree.MenuItems.Add(findUnityObjectItem);
				categories["Find Unity Object"] = findUnityObjectItem;
			}
			else
			{
				findUnityObjectItem = null;
			}
			bool hasFilterFunction = filterItemsFunction != null;
			foreach (Type type in types)
			{
				if (hasFilterFunction)
				{
					filterItemsFunction.Context.NamedValues.Set("type", type);
					if (!filterItemsFunction.GetValue())
					{
						continue;
					}
				}
				AddType(tree, type);
			}
			if (ShowCategories)
			{
				tree.AssignIconToEmptyItems(SdfIconType.FolderFill);
			}
			SortItemsByPriorityAndName();
			if (ShowCategories)
			{
				tree.CollapseEmptyItems();
			}
			if (findUnityObjectItem != null)
			{
				findUnityObjectItem.Icon = EditorIcons.UnityLogo;
				findUnityObjectItem.SdfIcon = SdfIconType.None;
			}
			tree.UpdateMenuTree();
			if (SelectedType != null)
			{
				SetSelection(SelectedType);
			}
			noneMenuItem = new OdinMenuItem(tree, "None", typeof(TypeSelectorNoneValue))
			{
				Icon = EditorIcons.Transparent.Raw
			};
			noneMenuItem.UpdateMenuTreeRecursive();
			if (SupportsMultiSelect || !useSingleClick)
			{
				return;
			}
			OdinMenuItem odinMenuItem = noneMenuItem;
			odinMenuItem.OnDrawItem = (Action<OdinMenuItem>)Delegate.Combine(odinMenuItem.OnDrawItem, (Action<OdinMenuItem>)delegate(OdinMenuItem menuItem)
			{
				if (GUI.Button(menuItem.rect, GUIContent.none, GUIStyle.none))
				{
					menuItem.Select();
					tree.Selection.ConfirmSelection();
				}
			});
			if (!CategorizeUnityObjects)
			{
				return;
			}
			OdinMenuItem odinMenuItem2 = findUnityObjectItem;
			odinMenuItem2.OnDrawItem = (Action<OdinMenuItem>)Delegate.Combine(odinMenuItem2.OnDrawItem, (Action<OdinMenuItem>)delegate(OdinMenuItem menuItem)
			{
				Rect rect = menuItem.rect;
				if (menuItem.Style.AlignTriangleLeft)
				{
					rect.xMin += menuItem.Style.TrianglePadding + menuItem.Style.TriangleSize;
				}
				else
				{
					rect.width -= menuItem.Style.TrianglePadding + menuItem.Style.TriangleSize;
				}
				if (GUI.Button(rect, GUIContent.none, GUIStyle.none))
				{
					menuItem.Select();
					tree.Selection.ConfirmSelection();
				}
			});
		}

		protected override void DrawSelectionTree()
		{
			Rect rect = EditorGUILayout.BeginVertical();
			EditorGUI.DrawRect(rect, SirenixGUIStyles.DarkEditorBackground);
			GUILayout.Space(1f);
			DrawToolbar();
			bool prev = base.SelectionTree.Config.DrawSearchToolbar;
			base.SelectionTree.Config.DrawSearchToolbar = false;
			try
			{
				float desiredSpecialItemHeight = 0f;
				if (ShowNoneItem)
				{
					desiredSpecialItemHeight += (float)base.SelectionTree.DefaultMenuStyle.Height + 4f;
				}
				Rect tmpSpecialItemsRect = GUILayoutUtility.GetRect(0f, desiredSpecialItemHeight);
				if (!tmpSpecialItemsRect.IsPlaceholder())
				{
					specialItemsRect = tmpSpecialItemsRect;
				}
				if (base.SelectionTree.MenuItems.Count == 0)
				{
					GUILayout.BeginVertical(SirenixGUIStyles.ContentPadding);
					SirenixEditorGUI.InfoMessageBox("There are no possible values to select.");
					GUILayout.EndVertical();
				}
				base.SelectionTree.DrawMenuTree();
				GUILayout.BeginArea(specialItemsRect);
				if (ShowNoneItem)
				{
					noneMenuItem.DrawMenuItem(0);
					SirenixEditorGUI.DrawThickHorizontalSeperator(4f, 1f, 1f);
				}
				GUILayout.EndArea();
			}
			finally
			{
				base.SelectionTree.Config.DrawSearchToolbar = prev;
			}
			SirenixEditorGUI.DrawBorders(rect, 1);
			EditorGUILayout.EndVertical();
		}

		protected override void DrawToolbar()
		{
			bool drawTitle = !string.IsNullOrEmpty(Title);
			bool drawSearchToolbar = base.SelectionTree.Config.DrawSearchToolbar;
			bool drawButton = DrawConfirmSelectionButton;
			if (drawTitle || drawSearchToolbar || drawButton)
			{
				SirenixEditorGUI.BeginHorizontalToolbar(base.SelectionTree.Config.SearchToolbarHeight);
				DrawToolbarTitle();
				DrawToolbarSearch();
				EditorGUI.DrawRect(GUILayoutUtility.GetLastRect().AlignLeft(1f), SirenixGUIStyles.BorderColor);
				DrawToolbarConfirmButton();
				DrawToolbarButtons();
				SirenixEditorGUI.EndHorizontalToolbar();
			}
		}

		protected void DrawToolbarButtons()
		{
			if (ToolbarToggle(ShowCategories, SdfIconType.ListNested, GUIHelper.TempContent("", "Toggle Categories")))
			{
				ShowCategories = !ShowCategories;
				RebuildMenuTree();
				base.SelectionTree.FocusSearchField();
			}
			if (ToolbarToggle(ShowHiddenTypes, SdfIconType.EyeFill, GUIHelper.TempContent("", "Customize Visible Types"), ignoreGUIEnabled: false, 0f, -1f))
			{
				ShowHiddenTypes = !ShowHiddenTypes;
				RebuildMenuTree();
				base.SelectionTree.FocusSearchField();
			}
			if (ToolbarToggle(isActive: false, SdfIconType.GearFill, GUIHelper.TempContent("", "Goto 'Type Selector' Tab In Preferences")))
			{
				EditorWindow.GetWindow<SirenixPreferencesWindow>().GotoPreferencesTab("Type Selector");
			}
		}

		private string GetTypePath(Type type)
		{
			bool isUnityObject = typeof(UnityEngine.Object).IsAssignableFrom(type);
			if (ShowCategories)
			{
				string path = TypeRegistry.GetCategoryPath(type, PreferNamespaces);
				if (CategorizeUnityObjects && isUnityObject)
				{
					if (!string.IsNullOrEmpty(path))
					{
						return "Find Unity Object/" + path;
					}
					return "Find Unity Object";
				}
				return path;
			}
			if (!(CategorizeUnityObjects && isUnityObject))
			{
				return string.Empty;
			}
			return "Find Unity Object";
		}

		protected void AddType(OdinMenuTree tree, Type type)
		{
			TypeRegistryUserConfig userConfig = GlobalConfig<TypeRegistryUserConfig>.Instance;
			bool isHidden = !userConfig.IsVisible(type);
			if (!ShowHiddenTypes && isHidden)
			{
				return;
			}
			string name = TypeRegistry.GetName(type);
			TypeSelectorMenuItem typeSelectorMenuItem;
			if (ShowCategories || CategorizeUnityObjects)
			{
				string path = GetTypePath(type);
				if (string.IsNullOrEmpty(path))
				{
					if (addedNamesForFlatTree.Contains(name))
					{
						duplicatePaths.Add(name);
					}
					typeSelectorMenuItem = new TypeSelectorMenuItem(this, tree, name, type);
					tree.MenuItems.Add(typeSelectorMenuItem);
					addedNamesForFlatTree.Add(name);
				}
				else
				{
					path = path.Trim(new char[1] { '/' });
					OdinMenuItem pathItem = GetOrMakePathItem(tree, path);
					OdinMenuItem foundItem;
					int foundItemIndex;
					bool hasItemWithName = TryGetItemWithName(pathItem, name, out foundItem, out foundItemIndex);
					bool isFoundItemValueNull = hasItemWithName && foundItem.Value == null;
					if (PreferNamespaces)
					{
						if (hasItemWithName && !isFoundItemValueNull)
						{
							duplicatePaths.Add(path + "/" + name);
						}
					}
					else
					{
						string fullPath = path + "/" + name;
						if (addedNamesForFlatTree.Contains(fullPath))
						{
							duplicatePaths.Add(fullPath);
						}
						addedNamesForFlatTree.Add(fullPath);
					}
					typeSelectorMenuItem = new TypeSelectorMenuItem(this, tree, name, type);
					if (isFoundItemValueNull)
					{
						typeSelectorMenuItem.ChildMenuItems.AddRange(foundItem.ChildMenuItems);
						pathItem.ChildMenuItems.RemoveAt(foundItemIndex);
						pathItem.ChildMenuItems.Insert(foundItemIndex, typeSelectorMenuItem);
					}
					else
					{
						pathItem.ChildMenuItems.Add(typeSelectorMenuItem);
					}
				}
			}
			else
			{
				if (addedNamesForFlatTree.Contains(name))
				{
					duplicatePaths.Add(name);
				}
				typeSelectorMenuItem = new TypeSelectorMenuItem(this, tree, name, type);
				tree.MenuItems.Add(typeSelectorMenuItem);
				addedNamesForFlatTree.Add(name);
			}
			if (ShowHiddenTypes && TypeRegistry.IsModifiableType(type))
			{
				TypeSelectorMenuItem typeSelectorMenuItem2 = typeSelectorMenuItem;
				typeSelectorMenuItem2.OnDrawItem = (Action<OdinMenuItem>)Delegate.Combine(typeSelectorMenuItem2.OnDrawItem, (Action<OdinMenuItem>)delegate(OdinMenuItem menuItem)
				{
					bool flag = GlobalConfig<TypeRegistryUserConfig>.Instance.IsVisible(type);
					Rect rect = menuItem.Rect.AlignRight(menuItem.Style.IconSize);
					if (typeSelectorMenuItem.ChildMenuItems.Count > 0)
					{
						rect.x -= typeSelectorMenuItem.Style.TriangleSize;
					}
					rect.x -= menuItem.Style.BorderPadding;
					bool flag2 = Event.current.IsMouseOver(rect);
					rect = rect.AlignMiddle(menuItem.Style.IconSize).Padding(1f);
					if (flag2)
					{
						if (EditorGUIUtility.isProSkin)
						{
							SdfIcons.DrawIcon(rect, (!flag) ? SdfIconType.EyeSlashFill : SdfIconType.EyeFill, new Color(1f, 1f, 1f));
						}
						else
						{
							SdfIcons.DrawIcon(rect, (!flag) ? SdfIconType.EyeSlashFill : SdfIconType.EyeFill, new Color(0f, 0f, 0f));
						}
					}
					else if (EditorGUIUtility.isProSkin)
					{
						SdfIcons.DrawIcon(rect, (!flag) ? SdfIconType.EyeSlashFill : SdfIconType.EyeFill, new Color(0.75f, 0.75f, 0.75f));
					}
					else
					{
						SdfIcons.DrawIcon(rect, (!flag) ? SdfIconType.EyeSlashFill : SdfIconType.EyeFill, new Color(0.25f, 0.25f, 0.25f));
					}
					if (Event.current.OnMouseDown(rect, 0))
					{
						GlobalConfig<TypeRegistryUserConfig>.Instance.SetVisibility(type, !flag);
						GUIHelper.RemoveFocusControl();
						Event.current.Use();
					}
					if (!SupportsMultiSelect && useSingleClick)
					{
						Rect rect2 = menuItem.rect;
						rect2.width -= menuItem.Style.IconSize;
						if (menuItem.ChildMenuItems.Count > 0)
						{
							if (menuItem.Style.AlignTriangleLeft)
							{
								rect2.xMin += menuItem.Style.TriangleSize + menuItem.Style.TrianglePadding;
							}
							else
							{
								rect2.width -= menuItem.Style.TriangleSize + menuItem.Style.TrianglePadding;
							}
						}
						if (GUI.Button(rect2, GUIContent.none, GUIStyle.none))
						{
							menuItem.Select();
							base.SelectionTree.Selection.ConfirmSelection();
						}
					}
					if (!flag)
					{
						if (EditorGUIUtility.isProSkin)
						{
							EditorGUI.DrawRect(menuItem.rect, new Color(0f, 0f, 0f, 0.25f));
						}
						else
						{
							EditorGUI.DrawRect(menuItem.rect, new Color(0f, 0f, 0f, 0.075f));
						}
					}
				});
			}
			else
			{
				if (SupportsMultiSelect || !useSingleClick)
				{
					return;
				}
				TypeSelectorMenuItem typeSelectorMenuItem3 = typeSelectorMenuItem;
				typeSelectorMenuItem3.OnDrawItem = (Action<OdinMenuItem>)Delegate.Combine(typeSelectorMenuItem3.OnDrawItem, (Action<OdinMenuItem>)delegate(OdinMenuItem menuItem)
				{
					Rect rect = menuItem.rect;
					if (menuItem.ChildMenuItems.Count > 0)
					{
						if (menuItem.Style.AlignTriangleLeft)
						{
							rect.xMin += menuItem.Style.TriangleSize + menuItem.Style.TrianglePadding;
						}
						else
						{
							rect.width -= menuItem.Style.TriangleSize + menuItem.Style.TrianglePadding;
						}
					}
					if (GUI.Button(rect, GUIContent.none, GUIStyle.none))
					{
						menuItem.Select();
						base.SelectionTree.Selection.ConfirmSelection();
					}
				});
			}
		}

		private bool TryGetItemWithName(OdinMenuItem parent, string name, out OdinMenuItem item, out int index)
		{
			if (parent == null)
			{
				item = null;
				index = -1;
				return false;
			}
			for (int i = 0; i < parent.ChildMenuItems.Count; i++)
			{
				OdinMenuItem child = parent.ChildMenuItems[i];
				if (child.Name == name)
				{
					item = child;
					index = i;
					return true;
				}
			}
			item = null;
			index = -1;
			return false;
		}

		private OdinMenuItem GetOrMakePathItem(OdinMenuTree tree, string path)
		{
			if (categories.TryGetValue(path, out var item))
			{
				return item;
			}
			item = tree.GetMenuItem(path);
			if (item != null)
			{
				return categories[path] = item;
			}
			OdinMenuTreeExtensions.SplitMenuPath(path, out var itemPath, out var itemName);
			item = new OdinMenuItem(tree, itemName, null)
			{
				SdfIcon = SdfIconType.FolderFill
			};
			tree.AddMenuItemAtPath(pathCache, itemPath, item);
			foreach (OdinMenuItem pathItem in pathCache)
			{
				pathItem.SdfIcon = SdfIconType.FolderFill;
			}
			pathCache.Clear();
			return categories[path] = item;
		}

		private void SortItemsByPriorityAndName(OdinMenuItem item = null)
		{
			if (item == null)
			{
				base.SelectionTree.MenuItems.Sort(CompareItemNameAndPriority);
				for (int i = 0; i < base.SelectionTree.MenuItems.Count; i++)
				{
					OdinMenuItem currentItem = base.SelectionTree.MenuItems[i];
					if (currentItem.ChildMenuItems.Count > 0)
					{
						SortItemsByPriorityAndName(currentItem);
					}
				}
				return;
			}
			item.ChildMenuItems.Sort(CompareItemNameAndPriority);
			for (int j = 0; j < item.ChildMenuItems.Count; j++)
			{
				OdinMenuItem currentSubItem = item.ChildMenuItems[j];
				if (currentSubItem.ChildMenuItems.Count > 0)
				{
					SortItemsByPriorityAndName(currentSubItem);
				}
			}
		}

		private int CompareItemNameAndPriority(OdinMenuItem a, OdinMenuItem b)
		{
			bool isANull = a == null;
			bool isBNull = b == null;
			if (isANull && isBNull)
			{
				return 0;
			}
			if (isANull)
			{
				return -1;
			}
			if (isBNull)
			{
				return 1;
			}
			if (CategorizeUnityObjects)
			{
				if (a == findUnityObjectItem)
				{
					return -1;
				}
				if (b == findUnityObjectItem)
				{
					return 1;
				}
			}
			bool isAValueNull = a.Value == null;
			bool isBValueNull = b.Value == null;
			if (isAValueNull && isBValueNull)
			{
				return string.Compare(a.Name, b.Name, StringComparison.Ordinal);
			}
			if (isAValueNull)
			{
				return -1;
			}
			if (isBValueNull)
			{
				return 1;
			}
			int aPriority = TypeRegistry.GetPriority((Type)a.Value);
			int priorityWeight = TypeRegistry.GetPriority((Type)b.Value).CompareTo(aPriority);
			if (priorityWeight == 0)
			{
				return string.Compare(a.Name, b.Name, StringComparison.Ordinal);
			}
			return priorityWeight;
		}

		private static bool ToolbarToggle(bool isActive, SdfIconType icon, GUIContent content, bool ignoreGUIEnabled = false, float iconXOffset = 0f, float iconYOffset = 0f)
		{
			Rect rect = GUILayoutUtility.GetRect(SirenixEditorGUI.currentDrawingToolbarHeight, SirenixEditorGUI.currentDrawingToolbarHeight, GUILayoutOptions.ExpandWidth(expand: false).ExpandHeight(expand: false));
			bool isPressed = GUI.Toggle(rect, isActive, content, SirenixGUIStyles.ToolbarButton) != isActive;
			if (ignoreGUIEnabled && !GUI.enabled && Event.current.rawType == EventType.MouseDown && Event.current.button == 0 && Event.current.IsMouseOver(rect))
			{
				GUIHelper.PushGUIEnabled(enabled: true);
				Event.current.Use();
				GUIHelper.PopGUIEnabled();
				isPressed = true;
			}
			if (isPressed && !isActive)
			{
				GUIHelper.RemoveFocusControl();
				GUIHelper.RequestRepaint();
			}
			if (Event.current.type != EventType.Repaint)
			{
				return isPressed;
			}
			RectOffset stylePadding = SirenixGUIStyles.ToolbarButton.padding;
			Rect iconRect = rect.Padding(stylePadding.left, stylePadding.right, stylePadding.top, stylePadding.bottom);
			if (iconXOffset != 0f)
			{
				iconRect.x += iconXOffset;
			}
			if (iconYOffset != 0f)
			{
				iconRect.y += iconYOffset;
			}
			SdfIcons.DrawIcon(iconRect, icon);
			return isPressed;
		}

		[PropertyOrder(10f)]
		[OnInspectorGUI]
		private void ShowTypeInfo()
		{
			if (base.SelectionTree.Selection.Count < 1)
			{
				return;
			}
			OdinMenuItem selectedItem = base.SelectionTree.Selection[0];
			if (selectedItem is TypeSelectorMenuItem item)
			{
				string fullTypeName = string.Empty;
				string assembly = string.Empty;
				string baseType = string.Empty;
				Rect rect = GUILayoutUtility.GetRect(0f, 56f).Padding(10f, 4f).AlignTop(16f);
				Type type = item.Type;
				if (type != null)
				{
					fullTypeName = type.GetNiceFullName();
					assembly = type.Assembly.GetName().Name;
					baseType = ((type.BaseType == null) ? string.Empty : type.BaseType.GetNiceFullName());
				}
				GUIStyle style = SirenixGUIStyles.LeftAlignedGreyMiniLabel;
				GUI.Label(rect.AlignLeft(75f), "Type Name", style);
				GUI.Label(rect.AlignRight(rect.width - 75f), fullTypeName, style);
				rect.y += 16f;
				GUI.Label(rect.AlignLeft(75f), "Base Type", style);
				GUI.Label(rect.AlignRight(rect.width - 75f), baseType, style);
				rect.y += 16f;
				GUI.Label(rect.AlignLeft(75f), "Assembly", style);
				GUI.Label(rect.AlignRight(rect.width - 75f), assembly, style);
			}
		}
	}
}
