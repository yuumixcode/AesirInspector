using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector.Editor.Internal.UIToolkitIntegration;
using Sirenix.Serialization;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor
{
	/// <summary>
	/// <para>Contains general configuration for all Odin drawers.</para>
	/// <para>
	/// You can modify the configuration in the Odin Preferences window found in 'Tools -&gt; Odin Inspector -&gt; Preferences -&gt; Drawers -&gt; General',
	/// or by locating the configuration file stored as a serialized object in the Sirenix folder under 'Odin Inspector/Config/Editor/GeneralDrawerConfig'.
	/// </para>
	/// </summary>
	[OnStateUpdate("@this.GotoTargetTabName(#(#_DefaultTabGroup))")]
	[Searchable]
	[InitializeOnLoad]
	[SirenixEditorConfig]
	public class GeneralDrawerConfig : GlobalConfig<GeneralDrawerConfig>
	{
		private class ColorPref
		{
			private Color? color;

			private Color defaultColor;

			private string key;

			public Color Value
			{
				get
				{
					if (!color.HasValue)
					{
						string str = EditorPrefs.GetString(key, ColorToString(defaultColor));
						if (TryStringToColor(str, out var parsed))
						{
							color = parsed;
						}
						else
						{
							color = defaultColor;
						}
					}
					return color.Value;
				}
				set
				{
					if (!color.HasValue || color.Value != value)
					{
						EditorPrefs.SetString(key, ColorToString(value));
					}
					color = value;
				}
			}

			public ColorPref(string key, Color defaultValue)
			{
				this.key = key;
				defaultColor = defaultValue;
			}

			public void Reset()
			{
				if (EditorPrefs.HasKey(key))
				{
					EditorPrefs.DeleteKey(key);
					color = null;
				}
			}

			private unsafe static string ColorToString(Color color)
			{
				Color32 col = UnityShims.Color32.op_Implicit(color);
				byte[] bytes = new byte[4];
				fixed (byte* ptr = bytes)
				{
					*(Color32*)ptr = col;
				}
				return ProperBitConverter.BytesToHexString(bytes);
			}

			private unsafe static bool TryStringToColor(string data, out Color color)
			{
				try
				{
					byte[] bytes = ProperBitConverter.HexStringToBytes(data);
					if (bytes.Length != 4)
					{
						color = default(Color);
						return false;
					}
					fixed (byte* ptr = bytes)
					{
						Color32 col = *(Color32*)ptr;
						color = UnityShims.Color32.op_Implicit(col);
						return true;
					}
				}
				catch
				{
					color = default(Color);
					return false;
				}
			}
		}

		[Flags]
		public enum UnityObjectType
		{
			Textures = 2,
			Sprites = 4,
			Materials = 8,
			GameObjects = 0x10,
			Components = 0x20,
			Others = 0x40
		}

		internal string TargetTabName;

		[SerializeField]
		[HideInInspector]
		private bool enableUIToolkitSupport = true;

		private Sirenix.Utilities.Editor.ObjectFieldAlignment? squareUnityObjectAlignment;

		private UnityObjectType? squareUnityObjectEnableFor;

		private QuaternionDrawMode? quaternionDrawMode;

		private float? squareUnityObjectFieldHeight;

		private bool? showPrefabModificationsDisabledMessage;

		private bool? hidePagingWhileOnlyOnePage;

		private bool? useNewImprovedEnumDropdown;

		private bool? hidePagingWhileCollapsed;

		private bool? showMonoScriptInEditor;

		private bool? hideFoldoutWhileEmpty;

		private bool? showPagingInTables;

		private bool? openListsByDefault;

		private bool? drawEnumTypeTitle;

		private bool? showExpandButton;

		private bool? showIndexLabels;

		private bool? showItemCount;

		private int? maxRecursiveDrawDepth;

		private int? numberOfItemsPerPage;

		private bool? precomputeTypeMatching;

		private bool? showPrefabModifiedValueBar;

		private bool? useUnityContextMenuForModifications;

		private bool? enableSmartNumberFields;

		private int? messageBoxFontSize;

		private int? buttonHeight;

		private IconAlignment? buttonIconAlignment;

		private bool? stretchButtons;

		private float? buttonAlignment;

		private static readonly ColorPref listItemColorEvenDarkSkinPref = new ColorPref("GeneralDrawerConfig.ListItemColorEvenDarkSkin", new Color(0.235f, 0.235f, 0.235f, 1f));

		private static readonly ColorPref listItemColorEvenLightSkinPref = new ColorPref("GeneralDrawerConfig.ListItemColorEvenLightSkin", new Color(0.838f, 0.838f, 0.838f, 1f));

		private static readonly ColorPref listItemColorOddDarkSkinPref = new ColorPref("GeneralDrawerConfig.ListItemColorOddDarkSkin", new Color(0.2f, 0.2f, 0.2f, 1f));

		private static readonly ColorPref listItemColorOddLightSkinPref = new ColorPref("GeneralDrawerConfig.ListItemColorOddLightSkin", new Color(0.788f, 0.788f, 0.788f, 1f));

		[NonSerialized]
		[ShowInInspector]
		[TabGroup("Collections", false, 0f)]
		[PropertyOrder(20f)]
		private List<int> exampleList = new List<int>();

		[SerializeField]
		[Title("Backwards Compatibility", null, TitleAlignments.Left, true, true)]
		[PropertyOrder(9f)]
		[TabGroup("Object Fields", false, 0f)]
		public bool useOldUnityObjectField;

		[SerializeField]
		[PropertyOrder(9f)]
		[TabGroup("Object Fields", false, 0f)]
		public bool useOldUnityPreviewField;

		[SerializeField]
		[PropertyOrder(10f)]
		[TitleGroup("_DefaultTabGroup/Type Selector/Backwards Compatibility", null, TitleAlignments.Left, true, true, false, 0f)]
		public bool useOldTypeSelector;

		[TitleGroup("_DefaultTabGroup/Type Selector/Backwards Compatibility", null, TitleAlignments.Left, true, true, false, 0f)]
		[SerializeField]
		[PropertyOrder(10f)]
		public bool useNewObjectSelector = true;

		[SerializeField]
		[TabGroup("Type Selector", false, 0f)]
		[PropertyOrder(10f)]
		[VerticalGroup("_DefaultTabGroup/Type Selector/GeneralGroup", 0f)]
		[LabelText("Show '<none>' Item In The Selector")]
		[Title("General", null, TitleAlignments.Left, true, true)]
		public bool showNoneItem = true;

		[PropertyOrder(10f)]
		[VerticalGroup("_DefaultTabGroup/Type Selector/GeneralGroup", 0f)]
		[SerializeField]
		public bool showCategoriesByDefault;

		[PropertyOrder(10f)]
		[VerticalGroup("_DefaultTabGroup/Type Selector/GeneralGroup", 0f)]
		[SerializeField]
		public bool preferNamespacesOverAssemblyCategories = true;

		[PropertyOrder(10f)]
		[Title("Backwards Compatibility", null, TitleAlignments.Left, true, true)]
		[TabGroup("Polymorphic Fields", false, 0f)]
		public bool useOldPolymorphicField;

		[SerializeField]
		[PropertyOrder(12f)]
		[Title("General", null, TitleAlignments.Left, true, true)]
		[TabGroup("Polymorphic Fields", false, 0f)]
		public bool showBaseType = true;

		[TabGroup("Polymorphic Fields", false, 0f)]
		[EnumToggleButtons]
		[LabelText("How To Handle Non Default Constructors")]
		[PropertyOrder(12f)]
		[SerializeField]
		public NonDefaultConstructorPreference nonDefaultConstructorPreference;

		private static string AllowUIToolkitSupportSuffix
		{
			get
			{
				if (!AllowUIToolkitSupport)
				{
					return "The UIToolkit support feature is only available in Unity 2020.2 and up";
				}
				return "";
			}
		}

		private static bool AllowUIToolkitSupport => ImguiElementUtils.IsSupported;

		/// <summary>
		/// Specify whether or not the script selector above components should be drawn.
		/// </summary>
		[PropertyTooltip("Specify whether or not the UI toolkit support feature should be enabled or not. Note that the DrawWithVisualElements attribute will still work (in Unity 2020.2+) if this is disabled.")]
		[TabGroup("General", false, 0f)]
		[ShowInInspector]
		[EnableIf("AllowUIToolkitSupport")]
		[SuffixLabel("$AllowUIToolkitSupportSuffix", false)]
		public bool EnableUIToolkitSupport
		{
			get
			{
				if (!ImguiElementUtils.IsSupported)
				{
					return false;
				}
				return enableUIToolkitSupport;
			}
			set
			{
				if (ImguiElementUtils.IsSupported)
				{
					enableUIToolkitSupport = value;
				}
			}
		}

		/// <summary>
		/// Specify whether or not the script selector above components should be drawn.
		/// </summary>
		[TabGroup("General", false, 0f)]
		[ShowInInspector]
		[PropertyTooltip("Specify whether or not the script selector above components should be drawn")]
		public bool ShowMonoScriptInEditor
		{
			get
			{
				if (!showMonoScriptInEditor.HasValue)
				{
					showMonoScriptInEditor = EditorPrefs.GetBool("GeneralDrawerConfig.ShowMonoScriptInEditor", defaultValue: true);
				}
				return showMonoScriptInEditor.Value;
			}
			set
			{
				showMonoScriptInEditor = value;
				EditorPrefs.SetBool("GeneralDrawerConfig.ShowMonoScriptInEditor", value);
			}
		}

		/// <summary>
		/// Specify whether or not the warning for properties that do not support prefab modifications should be shown in the inspector.
		/// </summary>
		[TabGroup("General", false, 0f)]
		[ShowInInspector]
		[PropertyTooltip("Specify whether or not the warning for properties that do not support prefab modifications should be shown in the inspector")]
		public bool ShowPrefabModificationsDisabledMessage
		{
			get
			{
				if (!showPrefabModificationsDisabledMessage.HasValue)
				{
					showPrefabModificationsDisabledMessage = EditorPrefs.GetBool("GeneralDrawerConfig.ShowPrefabModificationsDisabledMessage", defaultValue: true);
				}
				return showPrefabModificationsDisabledMessage.Value;
			}
			set
			{
				showPrefabModificationsDisabledMessage = value;
				EditorPrefs.SetBool("GeneralDrawerConfig.ShowPrefabModificationsDisabledMessage", value);
			}
		}

		/// <summary>
		/// Specify whether or not a blue bar should be drawn next to modified prefab values.
		/// </summary>
		[PropertyTooltip("Specify whether or not a blue bar should be drawn next to modified prefab values")]
		[TabGroup("General", false, 0f)]
		[LabelText("Show Blue Prefab Value Modified Bar")]
		[ShowInInspector]
		public bool ShowPrefabModifiedValueBar
		{
			get
			{
				if (!showPrefabModifiedValueBar.HasValue)
				{
					showPrefabModifiedValueBar = EditorPrefs.GetBool("GeneralDrawerConfig.ShowPrefabModifiedValueBar", defaultValue: true);
				}
				return showPrefabModifiedValueBar.Value;
			}
			set
			{
				showPrefabModifiedValueBar = value;
				EditorPrefs.SetBool("GeneralDrawerConfig.ShowPrefabModifiedValueBar", value);
			}
		}

		/// <summary>
		/// Specify whether or not the Unity context menu options should be included when showing the context menu for a Unity-serialized value that has prefab modifications.
		/// </summary>
		[ShowInInspector]
		[TabGroup("General", false, 0f)]
		[PropertyTooltip("Specify whether or not the Unity context menu options should be included when showing the context menu for a Unity-serialized value that has prefab modifications.")]
		public bool UseUnityContextMenuForModifications
		{
			get
			{
				if (!useUnityContextMenuForModifications.HasValue)
				{
					useUnityContextMenuForModifications = EditorPrefs.GetBool("GeneralDrawerConfig.UseUnityContextMenuForModifications", defaultValue: true);
				}
				return useUnityContextMenuForModifications.Value;
			}
			set
			{
				useUnityContextMenuForModifications = value;
				EditorPrefs.SetBool("GeneralDrawerConfig.UseUnityContextMenuForModifications", value);
			}
		}

		/// <summary>
		/// Specifies the maximum depth to which a property can draw itself recursively before the system refuses to draw it any deeper.
		/// </summary>
		[PropertyTooltip("Specifies the maximum depth to which a property can draw itself recursively before the system refuses to draw it any deeper.")]
		[ShowInInspector]
		[MaxValue(100.0)]
		[TabGroup("General", false, 0f)]
		[MinValue(1.0)]
		public int MaxRecursiveDrawDepth
		{
			get
			{
				if (!maxRecursiveDrawDepth.HasValue)
				{
					maxRecursiveDrawDepth = EditorPrefs.GetInt("GeneralDrawerConfig.MaxRecursiveDrawDepth", 10);
				}
				return Mathf.Clamp(maxRecursiveDrawDepth.Value, 1, 100);
			}
			set
			{
				value = Mathf.Clamp(value, 1, 100);
				maxRecursiveDrawDepth = value;
				EditorPrefs.SetInt("GeneralDrawerConfig.MaxRecursiveDrawDepth", value);
			}
		}

		/// <summary>
		/// If set to true, most foldouts throughout the inspector will be expanded by default.
		/// </summary>
		[TabGroup("General", false, 0f)]
		[ShowInInspector]
		[PropertyTooltip("If set to true, most foldouts throughout the inspector will be expanded by default.")]
		public bool ExpandFoldoutByDefault
		{
			get
			{
				return SirenixEditorGUI.ExpandFoldoutByDefault;
			}
			set
			{
				SirenixEditorGUI.ExpandFoldoutByDefault = value;
			}
		}

		/// <summary>
		/// If set to true, buttons will show the result values from invoking them in the inspector by default.
		/// </summary>
		[TabGroup("General", false, 0f)]
		[ShowInInspector]
		[PropertyTooltip("If set to true, buttons will show the result values from invoking them in the inspector by default.")]
		public bool ShowButtonResultsByDefault
		{
			get
			{
				return SirenixEditorGUI.ShowButtonResultsByDefault;
			}
			set
			{
				SirenixEditorGUI.ShowButtonResultsByDefault = value;
			}
		}

		[TabGroup("General", false, 0f)]
		[ShowInInspector]
		[DelayedProperty]
		[PropertyTooltip("Enables Odin smart fields in the inspector.")]
		[MinValue(1.0)]
		[MaxValue(100.0)]
		public bool EnableSmartNumberFields
		{
			get
			{
				if (!enableSmartNumberFields.HasValue)
				{
					enableSmartNumberFields = EditorPrefs.GetBool("GeneralDrawerConfig.EnableSmartNumberFields", defaultValue: true);
				}
				return enableSmartNumberFields.Value;
			}
			set
			{
				enableSmartNumberFields = value;
				EditorPrefs.SetBool("GeneralDrawerConfig.EnableSmartNumberFields", value);
			}
		}

		[MaxValue(100.0)]
		[MinValue(1.0)]
		[PropertyTooltip("Specifies the number of expressions to keep for smart field history.")]
		[TabGroup("General", false, 0f)]
		[ShowInInspector]
		[DelayedProperty]
		public int SmartFieldHistoryLength
		{
			get
			{
				return SirenixEditorFields.expressionHistory.MaxLength;
			}
			set
			{
				SirenixEditorFields.expressionHistory.SetMaxLength(value);
			}
		}

		[PropertyTooltip("Specifies the font size for all message boxes drawn by Odin.")]
		[TabGroup("General", false, 0f)]
		[ShowInInspector]
		[PropertyRange(10.0, 100.0)]
		public int MessageBoxFontSize
		{
			get
			{
				if (!messageBoxFontSize.HasValue)
				{
					messageBoxFontSize = EditorPrefs.GetInt("GeneralDrawerConfig.MessageBoxFontSize", 10);
				}
				return messageBoxFontSize.Value;
			}
			set
			{
				messageBoxFontSize = value;
				EditorPrefs.SetInt("GeneralDrawerConfig.MessageBoxFontSize", messageBoxFontSize.Value);
			}
		}

		/// <summary>
		/// Specify the animation speed for most foldouts throughout the inspector.
		/// </summary>
		[TabGroup("Animations", false, 0f)]
		[PropertyRange(0.0010000000474974513, 4.0)]
		[PropertyTooltip("Specify the animation speed for most foldouts throughout the inspector.")]
		[ShowInInspector]
		public float GUIFoldoutAnimationDuration
		{
			get
			{
				return SirenixEditorGUI.DefaultFadeGroupDuration;
			}
			set
			{
				SirenixEditorGUI.DefaultFadeGroupDuration = value;
			}
		}

		/// <summary>
		/// Specify the shaking duration for most shaking animations throughout the inspector.
		/// </summary>
		[PropertyRange(0.0, 4.0)]
		[TabGroup("Animations", false, 0f)]
		[ShowInInspector]
		[PropertyTooltip("Specify the shaking duration for most shaking animations throughout the inspector.")]
		public float ShakingAnimationDuration
		{
			get
			{
				return SirenixEditorGUI.ShakingAnimationDuration;
			}
			set
			{
				SirenixEditorGUI.ShakingAnimationDuration = value;
			}
		}

		/// <summary>
		/// Specify the animation speed for <see cref="T:Sirenix.OdinInspector.TabGroupAttribute" />
		/// </summary>
		[ShowInInspector]
		[TabGroup("Animations", false, 0f)]
		[PropertyRange(0.0010000000474974513, 4.0)]
		public float TabPageSlideAnimationDuration
		{
			get
			{
				return SirenixEditorGUI.TabPageSlideAnimationDuration;
			}
			set
			{
				SirenixEditorGUI.TabPageSlideAnimationDuration = value;
			}
		}

		/// <summary>
		/// When <c>true</c> the component labels, for vector fields, will be hidden when the field is too narrow.
		/// </summary>
		[TabGroup("Structs", false, 0f)]
		[ShowInInspector]
		[PropertyTooltip("When on the component labels, for vector fields, will be hidden when the field is too narrow.\nThis allows more space for the actual component fields themselves.")]
		public bool ResponsiveVectorComponentFields
		{
			get
			{
				return SirenixEditorFields.ResponsiveVectorComponentFields;
			}
			set
			{
				SirenixEditorFields.ResponsiveVectorComponentFields = value;
			}
		}

		/// <summary>
		/// Specify how the Quaternion struct should be shown in the inspector.
		/// </summary>
		[TabGroup("Structs", false, 0f)]
		[ShowInInspector]
		[EnumToggleButtons]
		[PropertyTooltip("Current mode for how quaternions are edited in the inspector.\n\nEuler: Rotations as yaw, pitch and roll.\n\nAngle axis: Rotations as a axis of rotation, and an angle of rotation around that axis.\n\nRaw: Directly edit the x, y, z and w components of a quaternion.")]
		public QuaternionDrawMode QuaternionDrawMode
		{
			get
			{
				if (!quaternionDrawMode.HasValue)
				{
					quaternionDrawMode = (QuaternionDrawMode)EditorPrefs.GetInt("GeneralDrawerConfig.QuaternionDrawMode", 0);
				}
				return quaternionDrawMode.Value;
			}
			set
			{
				quaternionDrawMode = value;
				EditorPrefs.SetInt("GeneralDrawerConfig.QuaternionDrawMode", (int)value);
			}
		}

		[TabGroup("Structs", false, 0f)]
		[ShowInInspector]
		private Quaternion ExampleQuaternion { get; set; }

		[TabGroup("Structs", false, 0f)]
		[ShowInInspector]
		private Vector3 ExampleVector { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether [use improved enum drop down].
		/// </summary>
		[TabGroup("Enums", false, 0f)]
		[ShowInInspector]
		public bool UseImprovedEnumDropDown
		{
			get
			{
				if (!useNewImprovedEnumDropdown.HasValue)
				{
					useNewImprovedEnumDropdown = EditorPrefs.GetBool("GeneralDrawerConfig.UseImprovedEnumDropDown", defaultValue: true);
				}
				return useNewImprovedEnumDropdown.Value;
			}
			set
			{
				useNewImprovedEnumDropdown = value;
				EditorPrefs.SetBool("GeneralDrawerConfig.UseImprovedEnumDropDown", value);
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether [use improved enum drop down].
		/// </summary>
		[ShowInInspector]
		[TabGroup("Enums", false, 0f)]
		[EnableIf("UseImprovedEnumDropDown")]
		public bool DrawEnumTypeTitle
		{
			get
			{
				if (!drawEnumTypeTitle.HasValue)
				{
					drawEnumTypeTitle = EditorPrefs.GetBool("GeneralDrawerConfig.DrawEnumTypeTitle", defaultValue: false);
				}
				return drawEnumTypeTitle.Value;
			}
			set
			{
				drawEnumTypeTitle = value;
				EditorPrefs.SetBool("GeneralDrawerConfig.DrawEnumTypeTitle", value);
			}
		}

		[TabGroup("Enums", false, 0f)]
		[ShowInInspector]
		private KeyCode ExampleEnum { get; set; }

		[TabGroup("Enums", false, 0f)]
		[ShowInInspector]
		private AssemblyCategory ExampleFlagEnum { get; set; }

		/// <summary>
		/// Specify whether or not a list should hide the foldout triangle when the list is empty.
		/// </summary>
		[ShowInInspector]
		[InfoBox("All collection settings - and more - can be overridden for individual collections using the ListDrawerSettings attribute.", InfoMessageType.Info, null)]
		[PropertyTooltip("Specifies whether all tables should include paging, or if the entirety of the table should be drawn as a list.")]
		[TabGroup("Collections", false, 0f)]
		public bool ShowPagingInTables
		{
			get
			{
				if (!showPagingInTables.HasValue)
				{
					showPagingInTables = EditorPrefs.GetBool("GeneralDrawerConfig.ShowPagingInTables", defaultValue: false);
				}
				return showPagingInTables.Value;
			}
			set
			{
				showPagingInTables = value;
				EditorPrefs.SetBool("GeneralDrawerConfig.ShowPagingInTables", value);
			}
		}

		/// <summary>
		/// Specifies whether a list should hide the foldout triangle when the list is empty.
		/// </summary>
		[Obsolete("This setting no longer has any effect.", false)]
		public bool HideFoldoutWhileEmpty
		{
			get
			{
				if (!hideFoldoutWhileEmpty.HasValue)
				{
					hideFoldoutWhileEmpty = EditorPrefs.GetBool("GeneralDrawerConfig.HideFoldoutWhileEmpty", defaultValue: true);
				}
				return hideFoldoutWhileEmpty.Value;
			}
			set
			{
				hideFoldoutWhileEmpty = value;
				EditorPrefs.SetBool("GeneralDrawerConfig.HideFoldoutWhileEmpty", value);
			}
		}

		/// <summary>
		/// Specify whether or not lists should hide the paging buttons when the list is collapsed.
		/// </summary>
		[PropertyTooltip("Specify whether or not lists should hide the paging buttons when the list is collapsed.")]
		[ShowInInspector]
		[TabGroup("Collections", false, 0f)]
		public bool HidePagingWhileCollapsed
		{
			get
			{
				if (!hidePagingWhileCollapsed.HasValue)
				{
					hidePagingWhileCollapsed = EditorPrefs.GetBool("GeneralDrawerConfig.HidePagingWhileCollapsed", defaultValue: true);
				}
				return hidePagingWhileCollapsed.Value;
			}
			set
			{
				hidePagingWhileCollapsed = value;
				EditorPrefs.SetBool("GeneralDrawerConfig.HidePagingWhileCollapsed", value);
			}
		}

		/// <summary>
		/// Specify whether or not lists should hide the paging buttons when there is only one page.
		/// </summary>
		[ShowInInspector]
		[TabGroup("Collections", false, 0f)]
		public bool HidePagingWhileOnlyOnePage
		{
			get
			{
				if (!hidePagingWhileOnlyOnePage.HasValue)
				{
					hidePagingWhileOnlyOnePage = EditorPrefs.GetBool("GeneralDrawerConfig.HidePagingWhileOnlyOnePage", defaultValue: true);
				}
				return hidePagingWhileOnlyOnePage.Value;
			}
			set
			{
				hidePagingWhileOnlyOnePage = value;
				EditorPrefs.SetBool("GeneralDrawerConfig.HidePagingWhileOnlyOnePage", value);
			}
		}

		/// <summary>
		/// Specify the number of elements drawn per page.
		/// </summary>
		[TabGroup("Collections", false, 0f)]
		[ShowInInspector]
		[OnValueChanged("ResizeExampleList", false)]
		[MaxValue(500.0)]
		[MinValue(2.0)]
		[PropertyTooltip("Specify the number of elements drawn per page.")]
		[LabelText("Number Of Items Per Page")]
		public int NumberOfItemsPrPage
		{
			get
			{
				if (!numberOfItemsPerPage.HasValue)
				{
					numberOfItemsPerPage = EditorPrefs.GetInt("GeneralDrawerConfig.NumberOfItemsPrPage", 15);
				}
				return numberOfItemsPerPage.Value;
			}
			set
			{
				numberOfItemsPerPage = value;
				EditorPrefs.SetInt("GeneralDrawerConfig.NumberOfItemsPrPage", value);
			}
		}

		/// <summary>
		/// Specify whether or not lists should be expanded or collapsed by default.
		/// </summary>
		[TabGroup("Collections", false, 0f)]
		[PropertyTooltip("Specify whether or not lists should be expanded or collapsed by default.")]
		[ShowInInspector]
		public bool OpenListsByDefault
		{
			get
			{
				if (!openListsByDefault.HasValue)
				{
					openListsByDefault = EditorPrefs.GetBool("GeneralDrawerConfig.OpenListsByDefault", defaultValue: false);
				}
				return openListsByDefault.Value;
			}
			set
			{
				openListsByDefault = value;
				EditorPrefs.SetBool("GeneralDrawerConfig.OpenListsByDefault", value);
			}
		}

		/// <summary>
		/// Specify whether or not to include a button which expands the list, showing all pages at once.
		/// </summary>
		[PropertyTooltip("Specify whether or not to include a button which expands the list, showing all pages at once")]
		[ShowInInspector]
		[TabGroup("Collections", false, 0f)]
		public bool ShowExpandButton
		{
			get
			{
				if (!showExpandButton.HasValue)
				{
					showExpandButton = EditorPrefs.GetBool("GeneralDrawerConfig.ShowExpandButton", defaultValue: true);
				}
				return showExpandButton.Value;
			}
			set
			{
				showExpandButton = value;
				EditorPrefs.SetBool("GeneralDrawerConfig.ShowExpandButton", value);
			}
		}

		/// <summary>
		/// Specify whether or not lists should show item count.
		/// </summary>
		[TabGroup("Collections", false, 0f)]
		[ShowInInspector]
		[PropertyTooltip("Specify whether or not lists should show item count.")]
		public bool ShowItemCount
		{
			get
			{
				if (!showItemCount.HasValue)
				{
					showItemCount = EditorPrefs.GetBool("GeneralDrawerConfig.ShowItemCount", defaultValue: true);
				}
				return showItemCount.Value;
			}
			set
			{
				showItemCount = value;
				EditorPrefs.SetBool("GeneralDrawerConfig.ShowItemCount", value);
			}
		}

		/// <summary>
		/// Specify whether or not lists should show item count.
		/// </summary>
		[TabGroup("Collections", false, 0f)]
		[ShowInInspector]
		[PropertyTooltip("Specify whether or not lists should show item count.")]
		public bool ShowIndexLabels
		{
			get
			{
				if (!showIndexLabels.HasValue)
				{
					showIndexLabels = EditorPrefs.GetBool("GeneralDrawerConfig.ShowIndexLabels", defaultValue: false);
				}
				return showIndexLabels.Value;
			}
			set
			{
				showIndexLabels = value;
				EditorPrefs.SetBool("GeneralDrawerConfig.ShowIndexLabels", value);
			}
		}

		/// <summary>
		/// Specify the color of even list elements when in the dark skin.
		/// </summary>
		[PropertyTooltip("Specify the color of even list elements when in the dark skin.")]
		[TabGroup("Collections", false, 0f)]
		[ShowInInspector]
		public Color ListItemColorEvenDarkSkin
		{
			get
			{
				return listItemColorEvenDarkSkinPref.Value;
			}
			set
			{
				if (listItemColorEvenDarkSkinPref.Value != value)
				{
					listItemColorEvenDarkSkinPref.Value = value;
					if (EditorGUIUtility.isProSkin)
					{
						SirenixGUIStyles.ListItemColorEven = value;
					}
				}
			}
		}

		/// <summary>
		/// Specify the color of odd list elements when in the dark skin.
		/// </summary>
		[TabGroup("Collections", false, 0f)]
		[PropertyTooltip("Specify the color of odd list elements when in the dark skin.")]
		[ShowInInspector]
		public Color ListItemColorOddDarkSkin
		{
			get
			{
				return listItemColorOddDarkSkinPref.Value;
			}
			set
			{
				if (listItemColorOddDarkSkinPref.Value != value)
				{
					listItemColorOddDarkSkinPref.Value = value;
					if (EditorGUIUtility.isProSkin)
					{
						SirenixGUIStyles.ListItemColorOdd = value;
					}
				}
			}
		}

		/// <summary>
		/// Specify the color of even list elements when in the light skin.
		/// </summary>
		[TabGroup("Collections", false, 0f)]
		[ShowInInspector]
		[PropertyTooltip("Specify the color of even list elements when in the light skin.")]
		public Color ListItemColorEvenLightSkin
		{
			get
			{
				return listItemColorEvenLightSkinPref.Value;
			}
			set
			{
				if (listItemColorEvenLightSkinPref.Value != value)
				{
					listItemColorEvenLightSkinPref.Value = value;
					if (!EditorGUIUtility.isProSkin)
					{
						SirenixGUIStyles.ListItemColorEven = value;
					}
				}
			}
		}

		/// <summary>
		/// Specify the color of odd list elements when in the light skin.
		/// </summary>
		[ShowInInspector]
		[TabGroup("Collections", false, 0f)]
		[PropertyTooltip("Specify the color of odd list elements when in the light skin.")]
		public Color ListItemColorOddLightSkin
		{
			get
			{
				return listItemColorOddLightSkinPref.Value;
			}
			set
			{
				if (listItemColorOddLightSkinPref.Value != value)
				{
					listItemColorOddLightSkinPref.Value = value;
					if (!EditorGUIUtility.isProSkin)
					{
						SirenixGUIStyles.ListItemColorOdd = value;
					}
				}
			}
		}

		/// <summary>
		/// Gets or sets the default size of the preview object field.
		/// </summary>
		[TabGroup("Object Fields", false, 0f)]
		[ShowInInspector]
		public float SquareUnityObjectFieldHeight
		{
			get
			{
				if (!squareUnityObjectFieldHeight.HasValue)
				{
					squareUnityObjectFieldHeight = EditorPrefs.GetFloat("GeneralDrawerConfig.squareUnityObjectFieldHeight", 50f);
				}
				return squareUnityObjectFieldHeight.Value;
			}
			set
			{
				squareUnityObjectFieldHeight = value;
				EditorPrefs.SetFloat("GeneralDrawerConfig.squareUnityObjectFieldHeight", value);
			}
		}

		/// <summary>
		/// Gets or sets the default alignment of the preview object field.
		/// </summary>
		[TabGroup("Object Fields", false, 0f)]
		[ShowInInspector]
		[EnumToggleButtons]
		public Sirenix.Utilities.Editor.ObjectFieldAlignment SquareUnityObjectAlignment
		{
			get
			{
				if (!squareUnityObjectAlignment.HasValue)
				{
					squareUnityObjectAlignment = (Sirenix.Utilities.Editor.ObjectFieldAlignment)EditorPrefs.GetInt("GeneralDrawerConfig.squareUnityObjectAlignment", 2);
				}
				return squareUnityObjectAlignment.Value;
			}
			set
			{
				squareUnityObjectAlignment = value;
				EditorPrefs.SetInt("GeneralDrawerConfig.squareUnityObjectAlignment", (int)value);
			}
		}

		/// <summary>
		/// Gets or sets which types should be drawn by default by the preview object field.
		/// </summary>
		[LabelText("Enable Globally For")]
		[TabGroup("Object Fields", false, 0f)]
		[ShowInInspector]
		public UnityObjectType SquareUnityObjectEnableFor
		{
			get
			{
				if (!squareUnityObjectEnableFor.HasValue)
				{
					squareUnityObjectEnableFor = (UnityObjectType)EditorPrefs.GetInt("GeneralDrawerConfig.squareUnityObjectEnableFor", 0);
				}
				return squareUnityObjectEnableFor.Value;
			}
			set
			{
				squareUnityObjectEnableFor = value;
				EditorPrefs.SetInt("GeneralDrawerConfig.squareUnityObjectEnableFor", (int)value);
			}
		}

		[ShowInInspector]
		[PreviewField]
		[TabGroup("Object Fields", false, 0f)]
		private UnityEngine.Object ExampleObject { get; set; }

		[OnValueChanged("@$property.Parent.Children[\"#Preview\"].RefreshSetup()", false)]
		[TabGroup("Buttons", false, 0f)]
		[ShowInInspector]
		[CustomValueDrawer("DrawButtonHeight")]
		public int ButtonHeight
		{
			get
			{
				if (!buttonHeight.HasValue)
				{
					buttonHeight = EditorPrefs.GetInt("GeneralDrawerConfig.buttonHeight", 0);
				}
				return buttonHeight.Value;
			}
			set
			{
				buttonHeight = value;
				EditorPrefs.SetInt("GeneralDrawerConfig.buttonHeight", value);
			}
		}

		[EnumToggleButtons]
		[OnValueChanged("@$property.Parent.Children[\"#Preview\"].RefreshSetup()", false)]
		[ShowInInspector]
		[TabGroup("Buttons", false, 0f)]
		public IconAlignment ButtonIconAlignment
		{
			get
			{
				if (!buttonIconAlignment.HasValue)
				{
					buttonIconAlignment = (IconAlignment)EditorPrefs.GetInt("GeneralDrawerConfig.buttonIconAlignment", 0);
				}
				return buttonIconAlignment.Value;
			}
			set
			{
				buttonIconAlignment = value;
				EditorPrefs.SetInt("GeneralDrawerConfig.buttonIconAlignment", (int)value);
			}
		}

		[OnValueChanged("@$property.Parent.Children[\"#Preview\"].RefreshSetup()", false)]
		[ShowInInspector]
		[TabGroup("Buttons", false, 0f)]
		public bool StretchButtons
		{
			get
			{
				if (!stretchButtons.HasValue)
				{
					stretchButtons = EditorPrefs.GetBool("GeneralDrawerConfig.stretchButtons", defaultValue: true);
				}
				return stretchButtons.Value;
			}
			set
			{
				stretchButtons = value;
				EditorPrefs.SetBool("GeneralDrawerConfig.stretchButtons", value);
			}
		}

		[EnableIf("@!StretchButtons")]
		[ShowInInspector]
		[TabGroup("Buttons", false, 0f)]
		[PropertyRange(0.0, 1.0)]
		[OnValueChanged("@$property.Parent.Children[\"#Preview\"].RefreshSetup()", false)]
		public float ButtonAlignment
		{
			get
			{
				if (!buttonAlignment.HasValue)
				{
					buttonAlignment = EditorPrefs.GetFloat("GeneralDrawerConfig.buttonAlignment", 0.5f);
				}
				return buttonAlignment.Value;
			}
			set
			{
				buttonAlignment = value;
				EditorPrefs.SetFloat("GeneralDrawerConfig.buttonAlignment", value);
			}
		}

		private void ResizeExampleList()
		{
			exampleList = Enumerable.Range(0, Math.Max(10, (int)((float)NumberOfItemsPrPage * (float)Math.PI))).ToList();
		}

		[OnInspectorGUI]
		[PropertyOrder(999f)]
		private void DrawFlexibleSpace()
		{
			GUILayout.FlexibleSpace();
		}

		private int DrawButtonHeight(GUIContent label, int height, Func<GUIContent, bool> callNextDrawer)
		{
			EditorGUILayout.BeginHorizontal();
			height = (int)EnumSelector<ButtonSizes>.DrawEnumField(label, (ButtonSizes)height);
			height = SirenixEditorFields.DelayedIntField(height, GUILayout.Width(50f));
			EditorGUILayout.EndHorizontal();
			return height;
		}

		[TitleGroup("_DefaultTabGroup/Buttons/Preview", null, TitleAlignments.Left, true, true, false, 0f)]
		[Button("Example Button With Icon", Icon = SdfIconType.CircleFill)]
		private void ExampleButton_WithIcon()
		{
		}

		[TitleGroup("_DefaultTabGroup/Buttons/Preview", null, TitleAlignments.Left, true, true, false, 0f)]
		[Button("Example Button Without Icon")]
		private void ExampleButton_WithoutIcon()
		{
		}

		[VerticalGroup("_DefaultTabGroup/Type Selector/GeneralGroup", 0f)]
		[Button("Customize", ButtonSizes.Large)]
		[PropertyOrder(10f)]
		public void ShowTypeRegistryUserConfig()
		{
			EditorWindow.GetWindow<TypeRegistryUserConfigWindow>();
		}

		/// <summary>
		/// Resets all settings to default.
		/// </summary>
		[Button(ButtonSizes.Large)]
		[PropertyOrder(1000f)]
		public void ResetToDefault()
		{
			if (EditorPrefs.HasKey("GeneralDrawerConfig.ShowMonoScriptInEditor"))
			{
				EditorPrefs.DeleteKey("GeneralDrawerConfig.ShowMonoScriptInEditor");
			}
			if (EditorPrefs.HasKey("GeneralDrawerConfig.ShowPrefabModificationsDisabledMessage"))
			{
				EditorPrefs.DeleteKey("GeneralDrawerConfig.ShowPrefabModificationsDisabledMessage");
			}
			if (EditorPrefs.HasKey("GeneralDrawerConfig.MaxRecursiveDrawDepth"))
			{
				EditorPrefs.DeleteKey("GeneralDrawerConfig.MaxRecursiveDrawDepth");
			}
			if (EditorPrefs.HasKey("GeneralDrawerConfig.MarkObjectsDirtyOnButtonClick"))
			{
				EditorPrefs.DeleteKey("GeneralDrawerConfig.MarkObjectsDirtyOnButtonClick");
			}
			if (EditorPrefs.HasKey("GeneralDrawerConfig.QuaternionDrawMode"))
			{
				EditorPrefs.DeleteKey("GeneralDrawerConfig.QuaternionDrawMode");
			}
			if (EditorPrefs.HasKey("GeneralDrawerConfig.UseImprovedEnumDropDown"))
			{
				EditorPrefs.DeleteKey("GeneralDrawerConfig.UseImprovedEnumDropDown");
			}
			if (EditorPrefs.HasKey("GeneralDrawerConfig.DrawEnumTypeTitle"))
			{
				EditorPrefs.DeleteKey("GeneralDrawerConfig.DrawEnumTypeTitle");
			}
			if (EditorPrefs.HasKey("GeneralDrawerConfig.HideFoldoutWhileEmpty"))
			{
				EditorPrefs.DeleteKey("GeneralDrawerConfig.HideFoldoutWhileEmpty");
			}
			if (EditorPrefs.HasKey("GeneralDrawerConfig.HidePagingWhileCollapsed"))
			{
				EditorPrefs.DeleteKey("GeneralDrawerConfig.HidePagingWhileCollapsed");
			}
			if (EditorPrefs.HasKey("GeneralDrawerConfig.HidePagingWhileOnlyOnePage"))
			{
				EditorPrefs.DeleteKey("GeneralDrawerConfig.HidePagingWhileOnlyOnePage");
			}
			if (EditorPrefs.HasKey("GeneralDrawerConfig.NumberOfItemsPrPage"))
			{
				EditorPrefs.DeleteKey("GeneralDrawerConfig.NumberOfItemsPrPage");
			}
			if (EditorPrefs.HasKey("GeneralDrawerConfig.OpenListsByDefault"))
			{
				EditorPrefs.DeleteKey("GeneralDrawerConfig.OpenListsByDefault");
			}
			if (EditorPrefs.HasKey("GeneralDrawerConfig.ShowExpandButton"))
			{
				EditorPrefs.DeleteKey("GeneralDrawerConfig.ShowExpandButton");
			}
			if (EditorPrefs.HasKey("GeneralDrawerConfig.ShowItemCount"))
			{
				EditorPrefs.DeleteKey("GeneralDrawerConfig.ShowItemCount");
			}
			if (EditorPrefs.HasKey("GeneralDrawerConfig.ShowIndexLabels"))
			{
				EditorPrefs.DeleteKey("GeneralDrawerConfig.ShowIndexLabels");
			}
			if (EditorPrefs.HasKey("GeneralDrawerConfig.squareUnityObjectFieldHeight"))
			{
				EditorPrefs.DeleteKey("GeneralDrawerConfig.squareUnityObjectFieldHeight");
			}
			if (EditorPrefs.HasKey("GeneralDrawerConfig.squareUnityObjectAlignment"))
			{
				EditorPrefs.DeleteKey("GeneralDrawerConfig.squareUnityObjectAlignment");
			}
			if (EditorPrefs.HasKey("GeneralDrawerConfig.squareUnityObjectEnableFor"))
			{
				EditorPrefs.DeleteKey("GeneralDrawerConfig.squareUnityObjectEnableFor");
			}
			if (EditorPrefs.HasKey("GeneralDrawerConfig.ShowPagingInTables"))
			{
				EditorPrefs.DeleteKey("GeneralDrawerConfig.ShowPagingInTables");
			}
			if (EditorPrefs.HasKey("GeneralDrawerConfig.PrecomputeTypeMatching"))
			{
				EditorPrefs.DeleteKey("GeneralDrawerConfig.PrecomputeTypeMatching");
			}
			if (EditorPrefs.HasKey("GeneralDrawerConfig.ShowPrefabModifiedValueBar"))
			{
				EditorPrefs.DeleteKey("GeneralDrawerConfig.ShowPrefabModifiedValueBar");
			}
			if (EditorPrefs.HasKey("GeneralDrawerConfig.UseUnityContextMenuForModifications"))
			{
				EditorPrefs.DeleteKey("GeneralDrawerConfig.UseUnityContextMenuForModifications");
			}
			if (EditorPrefs.HasKey("GeneralDrawerConfig.buttonHeight"))
			{
				EditorPrefs.DeleteKey("GeneralDrawerConfig.buttonHeight");
			}
			if (EditorPrefs.HasKey("GeneralDrawerConfig.buttonIconAlignment"))
			{
				EditorPrefs.DeleteKey("GeneralDrawerConfig.buttonIconAlignment");
			}
			if (EditorPrefs.HasKey("GeneralDrawerConfig.buttonAlignment"))
			{
				EditorPrefs.DeleteKey("GeneralDrawerConfig.buttonAlignment");
			}
			if (EditorPrefs.HasKey("GeneralDrawerConfig.stretchButtons"))
			{
				EditorPrefs.DeleteKey("GeneralDrawerConfig.stretchButtons");
			}
			if (EditorPrefs.HasKey("GeneralDrawerConfig.MessageBoxFontSize"))
			{
				EditorPrefs.DeleteKey("GeneralDrawerConfig.MessageBoxFontSize");
			}
			useOldUnityObjectField = false;
			useOldUnityPreviewField = false;
			useOldTypeSelector = false;
			useNewObjectSelector = true;
			useOldPolymorphicField = false;
			showNoneItem = true;
			showCategoriesByDefault = false;
			preferNamespacesOverAssemblyCategories = true;
			showBaseType = true;
			nonDefaultConstructorPreference = NonDefaultConstructorPreference.Exclude;
			listItemColorEvenDarkSkinPref.Reset();
			listItemColorEvenLightSkinPref.Reset();
			listItemColorOddDarkSkinPref.Reset();
			listItemColorOddLightSkinPref.Reset();
			if (EditorGUIUtility.isProSkin)
			{
				SirenixGUIStyles.ListItemColorEven = ListItemColorEvenDarkSkin;
				SirenixGUIStyles.ListItemColorOdd = ListItemColorOddDarkSkin;
			}
			else
			{
				SirenixGUIStyles.ListItemColorEven = ListItemColorEvenLightSkin;
				SirenixGUIStyles.ListItemColorOdd = ListItemColorOddLightSkin;
			}
			showMonoScriptInEditor = null;
			hideFoldoutWhileEmpty = null;
			openListsByDefault = null;
			showItemCount = null;
			numberOfItemsPerPage = null;
			hidePagingWhileCollapsed = null;
			hidePagingWhileOnlyOnePage = null;
			showExpandButton = null;
			quaternionDrawMode = null;
			showPrefabModificationsDisabledMessage = null;
			maxRecursiveDrawDepth = null;
			squareUnityObjectFieldHeight = null;
			squareUnityObjectAlignment = null;
			showIndexLabels = null;
			useNewImprovedEnumDropdown = null;
			drawEnumTypeTitle = null;
			showPagingInTables = null;
			precomputeTypeMatching = null;
			showPrefabModifiedValueBar = null;
			useUnityContextMenuForModifications = null;
			buttonHeight = null;
			buttonIconAlignment = null;
			buttonAlignment = null;
			stretchButtons = null;
			messageBoxFontSize = null;
			EditorPrefs.DeleteKey("SirenixEditorGUI.DefaultFadeGroupDuration");
			EditorPrefs.DeleteKey("SirenixEditorGUI.TabPageSlideAnimationDuration");
			EditorPrefs.DeleteKey("SirenixEditorGUI.ShakingAnimationDuration");
			EditorPrefs.DeleteKey("SirenixEditorGUI.ExpandFoldoutByDefault");
			SirenixEditorGUI.DefaultFadeGroupDuration = 0.13f;
			SirenixEditorGUI.TabPageSlideAnimationDuration = 0.13f;
			SirenixEditorGUI.ShakingAnimationDuration = 0.5f;
			SirenixEditorGUI.ExpandFoldoutByDefault = false;
		}

		private void GotoTargetTabName(InspectorProperty property)
		{
			if (!string.IsNullOrEmpty(TargetTabName))
			{
				property.State.Set("CurrentTabName", TargetTabName);
				TargetTabName = null;
			}
		}
	}
}
