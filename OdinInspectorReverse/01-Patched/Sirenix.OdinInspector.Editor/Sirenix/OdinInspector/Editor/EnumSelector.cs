using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor
{
	/// <summary>
	/// A feature-rich enum selector with support for flag enums.
	/// </summary>
	/// <example>
	/// <code>
	/// KeyCode someEnumValue;
	///
	/// [OnInspectorGUI]
	/// void OnInspectorGUI()
	/// {
	///     // Use the selector manually. See the documentation for OdinSelector for more information.
	///     if (GUILayout.Button("Open Enum Selector"))
	///     {
	///         EnumSelector&lt;KeyCode&gt; selector = new EnumSelector&lt;KeyCode&gt;();
	///         selector.SetSelection(this.someEnumValue);
	///         selector.SelectionConfirmed += selection =&gt; this.someEnumValue = selection.FirstOrDefault();
	///         selector.ShowInPopup(); // Returns the Odin Editor Window instance, in case you want to mess around with that as well.
	///     }
	///
	///     // Draw an enum dropdown field which uses the EnumSelector popup:
	///     this.someEnumValue = EnumSelector&lt;KeyCode&gt;.DrawEnumField(new GUIContent("My Label"), this.someEnumValue);
	/// }
	///
	/// // All Odin Selectors can be rendered anywhere with Odin. This includes the EnumSelector.
	/// EnumSelector&lt;KeyCode&gt; inlineSelector;
	///
	/// [ShowInInspector]
	/// EnumSelector&lt;KeyCode&gt; InlineSelector
	/// {
	///     get { return this.inlineSelector ?? (this.inlineSelector = new EnumSelector&lt;KeyCode&gt;()); }
	///     set { }
	/// }
	/// </code>
	/// </example>
	/// <seealso cref="T:Sirenix.OdinInspector.Editor.OdinSelector`1" />
	/// <seealso cref="T:Sirenix.OdinInspector.Editor.TypeSelector" />
	/// <seealso cref="T:Sirenix.OdinInspector.Editor.TypeSelectorV2" />
	/// <seealso cref="T:Sirenix.OdinInspector.Editor.GenericSelector`1" />
	/// <seealso cref="T:Sirenix.OdinInspector.Editor.OdinMenuTree" />
	/// <seealso cref="T:Sirenix.OdinInspector.Editor.OdinEditorWindow" />
	public class EnumSelector<T> : OdinSelector<T>
	{
		private static readonly StringBuilder SB;

		private static readonly StringBuilder tooltipSB;

		private static readonly Func<T, T, bool> EqualityComparer;

		private static Color highlightLineColor;

		private static Color selectedMaskBgColor;

		private static readonly string title;

		private float maxEnumLabelWidth;

		private ulong curentValue;

		private ulong curentMouseOverValue;

		private static readonly ulong definedIndividualFlagsMask;

		public static bool DrawSearchToolbar;

		private bool wasMouseDown;

		/// <summary>
		/// By default, the enum type will be drawn as the title for the selector. No title will be drawn if the string is null or empty.
		/// </summary>
		public override string Title
		{
			get
			{
				if (GlobalConfig<GeneralDrawerConfig>.Instance.DrawEnumTypeTitle)
				{
					return title;
				}
				return null;
			}
		}

		/// <summary>
		/// Gets a value indicating whether this instance is flag enum.
		/// </summary>
		public bool IsFlagEnum => EnumTypeUtilities<T>.IsFlagEnum;

		static EnumSelector()
		{
			SB = new StringBuilder();
			tooltipSB = new StringBuilder();
			EqualityComparer = PropertyValueEntry<T>.EqualityComparer;
			highlightLineColor = (EditorGUIUtility.isProSkin ? new Color(0.5f, 1f, 0f, 1f) : new Color(0.015f, 0.68f, 0.015f, 1f));
			selectedMaskBgColor = (EditorGUIUtility.isProSkin ? new Color(0.5f, 1f, 0f, 0.1f) : new Color(0.02f, 0.537f, 0f, 0.31f));
			title = typeof(T).Name.SplitPascalCase();
			DrawSearchToolbar = true;
			if (!typeof(T).IsEnum || !EnumTypeUtilities<T>.IsFlagEnum)
			{
				return;
			}
			ulong mask = 0uL;
			EnumTypeUtilities<T>.EnumMember[] members = EnumTypeUtilities<T>.AllEnumMemberInfos;
			for (int i = 0; i < members.Length; i++)
			{
				ulong v = (ulong)Convert.ToInt64(members[i].Value);
				if (v != 0L && (v & (v - 1)) == 0L)
				{
					mask |= v;
				}
			}
			definedIndividualFlagsMask = mask;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:Sirenix.OdinInspector.Editor.EnumSelector`1" /> class.
		/// </summary>
		public EnumSelector()
		{
			if (!typeof(T).IsEnum)
			{
				throw new NotSupportedException(typeof(T).GetNiceFullName() + " is not an enum type.");
			}
			if (Event.current != null)
			{
				string[] names = Enum.GetNames(typeof(T));
				foreach (string item in names)
				{
					maxEnumLabelWidth = Mathf.Max(maxEnumLabelWidth, SirenixGUIStyles.Label.CalcSize(new GUIContent(item)).x);
				}
				if (Title != null)
				{
					string titleAndSearch = Title + "                      ";
					maxEnumLabelWidth = Mathf.Max(maxEnumLabelWidth, SirenixGUIStyles.Label.CalcSize(new GUIContent(titleAndSearch)).x);
				}
			}
		}

		/// <summary>
		/// Populates the tree with all enum values.
		/// </summary>
		protected override void BuildSelectionTree(OdinMenuTree tree)
		{
			tree.Selection.SupportsMultiSelect = IsFlagEnum;
			tree.Config.DrawSearchToolbar = DrawSearchToolbar;
			tree.Config.SelectMenuItemsOnMouseDown = true;
			tree.Config.ConfirmSelectionOnDoubleClick = false;
			EnumTypeUtilities<T>.EnumMember[] enumVals = EnumTypeUtilities<T>.AllEnumMemberInfos;
			EnumTypeUtilities<T>.EnumMember[] array = enumVals;
			for (int i = 0; i < array.Length; i++)
			{
				EnumTypeUtilities<T>.EnumMember item = array[i];
				if (!item.Hide)
				{
					tree.Add(item.NiceName, item, item.Icon);
				}
			}
			if (IsFlagEnum)
			{
				tree.DefaultMenuStyle.Offset += 15f;
				if (!(from x in enumVals
					where x.Value != null
					select Convert.ToInt64(x.Value)).Contains(0L))
				{
					tree.MenuItems.Insert(0, new OdinMenuItem(tree, GetNoneValueString(), new EnumTypeUtilities<T>.EnumMember
					{
						Value = GetZeroValue(),
						Name = "None",
						NiceName = "None",
						IsObsolete = false,
						Message = ""
					}));
				}
				tree.EnumerateTree().ForEach(delegate(OdinMenuItem x)
				{
					x.OnDrawItem = (Action<OdinMenuItem>)Delegate.Combine(x.OnDrawItem, new Action<OdinMenuItem>(DrawEnumFlagItem));
				});
				DrawConfirmSelectionButton = false;
			}
			else
			{
				tree.EnumerateTree().ForEach(delegate(OdinMenuItem x)
				{
					x.OnDrawItem = (Action<OdinMenuItem>)Delegate.Combine(x.OnDrawItem, new Action<OdinMenuItem>(DrawEnumItem));
				});
			}
			tree.EnumerateTree().ForEach(delegate(OdinMenuItem x)
			{
				x.OnDrawItem = (Action<OdinMenuItem>)Delegate.Combine(x.OnDrawItem, new Action<OdinMenuItem>(DrawEnumInfo));
			});
		}

		private static T GetZeroValue()
		{
			Type backingType = Enum.GetUnderlyingType(typeof(T));
			object backingZero = Convert.ChangeType(0, backingType);
			return (T)backingZero;
		}

		private void DrawEnumInfo(OdinMenuItem obj)
		{
			if (obj.Value is EnumTypeUtilities<T>.EnumMember)
			{
				EnumTypeUtilities<T>.EnumMember member = (EnumTypeUtilities<T>.EnumMember)obj.Value;
				bool hasMessage = !string.IsNullOrEmpty(member.Message);
				if (member.IsObsolete)
				{
					Rect rect = obj.Rect.Padding(5f, 3f).AlignRight(16f).AlignCenterY(16f);
					GUI.DrawTexture(rect, EditorIcons.TestInconclusive);
				}
				else if (hasMessage)
				{
					Rect rect2 = obj.Rect.Padding(5f, 3f).AlignRight(16f).AlignCenterY(16f);
					GUI.DrawTexture(rect2, EditorIcons.ConsoleInfoIcon);
				}
				if (hasMessage)
				{
					GUI.Label(obj.Rect, new GUIContent("", member.Message));
				}
			}
		}

		private void DrawEnumItem(OdinMenuItem obj)
		{
			Rect clickableRect = obj.Rect;
			if (obj.ChildMenuItems.Count > 0)
			{
				if (obj.Style.AlignTriangleLeft)
				{
					clickableRect.xMin += obj.Style.TrianglePadding + obj.Style.TriangleSize;
				}
				else
				{
					clickableRect.xMax -= obj.Style.TrianglePadding + obj.Style.TriangleSize;
				}
			}
			if (Event.current.type == EventType.MouseDown && Event.current.IsMouseOver(clickableRect))
			{
				obj.Select();
				if (obj.ChildMenuItems.Count == 0)
				{
					Event.current.Use();
				}
				wasMouseDown = true;
			}
			if (wasMouseDown)
			{
				GUIHelper.RequestRepaint();
			}
			if (wasMouseDown && Event.current.type == EventType.MouseDrag && Event.current.IsMouseOver(clickableRect) && obj.Value != null)
			{
				obj.Select();
			}
			if (Event.current.type == EventType.MouseUp)
			{
				wasMouseDown = false;
				if (obj.IsSelected && Event.current.IsMouseOver(clickableRect) && obj.Value != null)
				{
					obj.MenuTree.Selection.ConfirmSelection();
				}
			}
		}

		[OnInspectorGUI]
		[PropertyOrder(-1000f)]
		private void SpaceToggleEnumFlag()
		{
			if (base.SelectionTree != OdinMenuTree.ActiveMenuTree || !IsFlagEnum || Event.current.keyCode != KeyCode.Space || Event.current.type != EventType.KeyDown || base.SelectionTree == null)
			{
				return;
			}
			foreach (OdinMenuItem item in base.SelectionTree.Selection)
			{
				ToggleEnumFlag(item);
			}
			TriggerSelectionChanged();
			Event.current.Use();
		}

		/// <summary>
		/// When ShowInPopup is called, without a specified window width, this method gets called.
		/// Here you can calculate and give a good default width for the popup.
		/// The default implementation returns 0, which will let the popup window determine the width itself. This is usually a fixed value.
		/// </summary>
		protected override float DefaultWindowWidth()
		{
			return Mathf.Clamp(maxEnumLabelWidth + 50f, 160f, 400f);
		}

		private void DrawEnumFlagItem(OdinMenuItem obj)
		{
			Rect clickableRect = obj.Rect;
			if (obj.ChildMenuItems.Count > 0)
			{
				if (obj.Style.AlignTriangleLeft)
				{
					clickableRect.xMin += obj.Style.TrianglePadding + obj.Style.TriangleSize;
				}
				else
				{
					clickableRect.xMax -= obj.Style.TrianglePadding + obj.Style.TriangleSize;
				}
			}
			if ((Event.current.type == EventType.MouseDown || Event.current.type == EventType.MouseUp) && Event.current.IsMouseOver(clickableRect) && obj.Value != null)
			{
				if (Event.current.type == EventType.MouseDown)
				{
					ToggleEnumFlag(obj);
					TriggerSelectionChanged();
				}
				Event.current.Use();
			}
			if (Event.current.type != EventType.Repaint || obj.Value == null)
			{
				return;
			}
			ulong val = (ulong)Convert.ToInt64(GetMenuItemEnumValue(obj));
			bool isPowerOfTwo = (val & (val - 1)) == 0;
			if (val != 0L && !isPowerOfTwo)
			{
				if (obj.Rect.Contains(Event.current.mousePosition))
				{
					curentMouseOverValue = val;
				}
				else if (val == curentMouseOverValue)
				{
					curentMouseOverValue = 0uL;
				}
			}
			bool chked = (val & curentValue) == val && (val != 0L || curentValue == 0);
			if (val != 0 && isPowerOfTwo && (val & curentMouseOverValue) == val && (val != 0L || curentMouseOverValue == 0))
			{
				EditorGUI.DrawRect(obj.Rect.AlignLeft(6f).Padding(2f), highlightLineColor);
			}
			if (!(chked || isPowerOfTwo))
			{
				return;
			}
			Rect rect = obj.Rect.AlignLeft(30f).AlignCenter(EditorIcons.TestPassed.width, EditorIcons.TestPassed.height);
			if (chked)
			{
				if (isPowerOfTwo)
				{
					if (!EditorGUIUtility.isProSkin)
					{
						Color tmp = GUI.color;
						GUI.color = new Color(1f, 0.7f, 1f, 1f);
						GUI.DrawTexture(rect, EditorIcons.TestPassed);
						GUI.color = tmp;
					}
					else
					{
						GUI.DrawTexture(rect, EditorIcons.TestPassed);
					}
				}
				else
				{
					EditorGUI.DrawRect(obj.Rect.AlignTop(obj.Rect.height - (float)(EditorGUIUtility.isProSkin ? 1 : 0)), selectedMaskBgColor);
				}
			}
			else
			{
				GUI.DrawTexture(rect, EditorIcons.TestNormal);
			}
		}

		private void ToggleEnumFlag(OdinMenuItem obj)
		{
			ulong val = (ulong)Convert.ToInt64(GetMenuItemEnumValue(obj));
			if ((val & curentValue) == val)
			{
				curentValue = ((val == 0L) ? 0 : (curentValue & ~val));
				if (definedIndividualFlagsMask != 0L && (curentValue & definedIndividualFlagsMask) == 0L)
				{
					curentValue = 0uL;
				}
			}
			else
			{
				curentValue |= val;
			}
			if (Event.current.clickCount >= 2)
			{
				Event.current.Use();
			}
		}

		/// <summary>
		/// Gets the currently selected enum value.
		/// </summary>
		public override IEnumerable<T> GetCurrentSelection()
		{
			if (IsFlagEnum)
			{
				yield return (T)Enum.ToObject(typeof(T), curentValue);
			}
			else if (base.SelectionTree.Selection.Count > 0)
			{
				yield return (T)Enum.ToObject(typeof(T), GetMenuItemEnumValue(base.SelectionTree.Selection.Last()));
			}
		}

		/// <summary>
		/// Selects an enum.
		/// </summary>
		public override void SetSelection(T selected)
		{
			if (IsFlagEnum)
			{
				curentValue = (ulong)Convert.ToInt64(selected);
				return;
			}
			IEnumerable<OdinMenuItem> selection = from x in base.SelectionTree.EnumerateTree()
				where Convert.ToInt64(GetMenuItemEnumValue(x)) == Convert.ToInt64(selected)
				select x;
			base.SelectionTree.Selection.AddRange(selection);
		}

		private static object GetMenuItemEnumValue(OdinMenuItem item)
		{
			if (item.Value is EnumTypeUtilities<T>.EnumMember)
			{
				return ((EnumTypeUtilities<T>.EnumMember)item.Value).Value;
			}
			return default(T);
		}

		/// <summary>
		/// Draws an enum selector field using the enum selector.
		/// </summary>
		public static T DrawEnumField(GUIContent label, GUIContent contentLabel, T value, GUIStyle style = null, SdfIconType valueIcon = SdfIconType.None)
		{
			SirenixEditorGUI.GetFeatureRichControlRect(label, out var id, out var _, out var rect);
			if (OdinSelector<T>.DrawSelectorButton(rect, contentLabel, valueIcon, style ?? EditorStyles.popup, id, returnValuesOnSelectionChange: true, out Action<EnumSelector<T>> bindSelector, out Func<IEnumerable<T>> getResult))
			{
				EnumSelector<T> selector = new EnumSelector<T>();
				if (!EditorGUI.showMixedValue)
				{
					selector.SetSelection(value);
				}
				OdinEditorWindow window = selector.ShowInPopup(rect);
				if (EnumTypeUtilities<T>.IsFlagEnum)
				{
					window.OnClose += selector.SelectionTree.Selection.ConfirmSelection;
				}
				bindSelector(selector);
				if (Application.platform == RuntimePlatform.LinuxEditor)
				{
					GUIHelper.ExitGUI(removeFocusControl: true);
				}
			}
			if (getResult != null)
			{
				value = getResult().FirstOrDefault();
			}
			return value;
		}

		/// <summary>
		/// Draws an enum selector field using the enum selector.
		/// </summary>
		public static T DrawEnumField(GUIContent label, T value, GUIStyle style = null)
		{
			SdfIconType icon = SdfIconType.None;
			string tooltip = "";
			string display = ((!EditorGUI.showMixedValue) ? GetValueString(value, out icon, out tooltip) : "—");
			return DrawEnumField(label, new GUIContent(display, tooltip), value, style, icon);
		}

		/// <summary>
		/// Draws an enum selector field using the enum selector.
		/// </summary>
		public static T DrawEnumField(Rect rect, GUIContent label, GUIContent contentLabel, T value, GUIStyle style = null, SdfIconType valueIcon = SdfIconType.None)
		{
			rect = SirenixEditorGUI.GetFeatureRichControl(rect, label, out var id, out var _);
			if (OdinSelector<T>.DrawSelectorButton(rect, contentLabel, valueIcon, style ?? EditorStyles.popup, id, returnValuesOnSelectionChange: true, out Action<EnumSelector<T>> bindSelector, out Func<IEnumerable<T>> getResult))
			{
				EnumSelector<T> selector = new EnumSelector<T>();
				if (!EditorGUI.showMixedValue)
				{
					selector.SetSelection(value);
				}
				OdinEditorWindow window = selector.ShowInPopup(rect);
				if (EnumTypeUtilities<T>.IsFlagEnum)
				{
					window.OnClose += selector.SelectionTree.Selection.ConfirmSelection;
				}
				bindSelector(selector);
				if (Application.platform == RuntimePlatform.LinuxEditor)
				{
					GUIHelper.ExitGUI(removeFocusControl: true);
				}
			}
			if (getResult != null)
			{
				value = getResult().FirstOrDefault();
			}
			return value;
		}

		/// <summary>
		/// Draws an enum selector field using the enum selector.
		/// </summary>
		public static T DrawEnumField(Rect rect, GUIContent label, T value, GUIStyle style = null)
		{
			string display = ((EnumTypeUtilities<T>.IsFlagEnum && Convert.ToInt64(value) == 0L) ? GetNoneValueString() : (EditorGUI.showMixedValue ? "—" : value.ToString().SplitPascalCase()));
			return DrawEnumField(rect, label, new GUIContent(display), value, style);
		}

		private static string GetNoneValueString()
		{
			string name = Enum.GetName(typeof(T), GetZeroValue());
			if (name != null)
			{
				return name.SplitPascalCase();
			}
			return "None";
		}

		private static string GetValueString(T value, out SdfIconType sdfIcon, out string tooltip)
		{
			EnumTypeUtilities<T>.EnumMember[] enumVals = EnumTypeUtilities<T>.AllEnumMemberInfos;
			sdfIcon = SdfIconType.None;
			tooltip = "";
			for (int i = 0; i < enumVals.Length; i++)
			{
				EnumTypeUtilities<T>.EnumMember val = enumVals[i];
				if (EqualityComparer(val.Value, value))
				{
					sdfIcon = val.Icon;
					tooltip = val.Tooltip;
					return val.NiceName;
				}
			}
			if (EnumTypeUtilities<T>.IsFlagEnum)
			{
				long val64 = Convert.ToInt64(value);
				if (val64 == 0L)
				{
					return GetNoneValueString();
				}
				SB.Length = 0;
				tooltipSB.Length = 0;
				for (int j = 0; j < enumVals.Length; j++)
				{
					EnumTypeUtilities<T>.EnumMember val65 = enumVals[j];
					long flags = Convert.ToInt64(val65.Value);
					if (flags != 0L && (val64 & flags) == flags)
					{
						tooltipSB.Append(val65.NiceName + ": " + val65.Tooltip + "\n");
						if (SB.Length > 0)
						{
							SB.Append(", ");
						}
						SB.Append(val65.NiceName);
					}
				}
				tooltip = tooltipSB.ToString();
				return SB.ToString();
			}
			return value.ToString().SplitPascalCase();
		}
	}
}
