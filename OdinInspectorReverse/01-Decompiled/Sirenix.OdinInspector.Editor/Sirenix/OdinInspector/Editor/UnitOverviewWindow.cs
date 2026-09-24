using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sirenix.OdinInspector.Editor.Examples;
using Sirenix.Serialization;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor
{
	public class UnitOverviewWindow : OdinMenuEditorWindow
	{
		private class WindowState
		{
			public float CodeHeight;

			public string SelectedCategory;

			public Dictionary<string, UnitInfo> UnitsToConvertFromByCategory;

			public Dictionary<string, decimal> ValuesToConvertFromByCategory;

			public Dictionary<string, int> SelectedUnitInfoIndexByCategory;
		}

		private const int RowHeight = 25;

		private const float MinNameColWidth = 61f;

		private const float MinSymbolColWidth = 78f;

		private const float DefaultMenuWidth = 250f;

		private const string EditorPrefsKey = "Odin_UnitOverviewWindow_Prefs";

		private static Rect dragRect;

		private static float codeHeight;

		private static float nameColWidth;

		private static float symbolColWidth;

		private static Vector2 scrollPosition;

		private static GUIStyle headerGroupStyle;

		private static string selectedCategory;

		private static readonly Dictionary<string, List<UnitInfo>> unitInfosByCategory = new Dictionary<string, List<UnitInfo>>();

		private static Dictionary<string, UnitInfo> unitsToConvertFromByCategory;

		private static Dictionary<string, decimal> valuesToConvertFromByCategory;

		private static Dictionary<string, int> selectedUnitInfoIndexByCategory;

		private static AttributeExamplePreview examplePreview;

		private static List<UnitInfo> unitInfosInSelectedCategory => unitInfosByCategory[selectedCategory];

		private static UnitInfo unitToConvertFrom
		{
			get
			{
				if (unitsToConvertFromByCategory.TryGetValue(selectedCategory, out var unitInfo) && unitInfo != null)
				{
					return unitInfo;
				}
				unitInfo = unitInfosByCategory[selectedCategory].First();
				return unitsToConvertFromByCategory[selectedCategory] = unitInfo;
			}
			set
			{
				unitsToConvertFromByCategory[selectedCategory] = value;
			}
		}

		private static decimal valueToConvertFrom
		{
			get
			{
				if (valuesToConvertFromByCategory.TryGetValue(selectedCategory, out var value))
				{
					return value;
				}
				return valuesToConvertFromByCategory[selectedCategory] = default(decimal);
			}
			set
			{
				valuesToConvertFromByCategory[selectedCategory] = value;
			}
		}

		private static int selectedUnitInfoIndex
		{
			get
			{
				if (selectedUnitInfoIndexByCategory.TryGetValue(selectedCategory, out var index))
				{
					return index;
				}
				return selectedUnitInfoIndexByCategory[selectedCategory] = 0;
			}
			set
			{
				selectedUnitInfoIndexByCategory[selectedCategory] = value;
			}
		}

		private static Color tableHeaderColor
		{
			get
			{
				if (!EditorGUIUtility.isProSkin)
				{
					return new Color(0.65f, 0.65f, 0.65f);
				}
				return new Color(0.15f, 0.15f, 0.15f);
			}
		}

		public static UnitOverviewWindow ShowWindow()
		{
			return EditorWindow.GetWindow<UnitOverviewWindow>();
		}

		public static void SelectUnitInfo(UnitInfo unitInfo, decimal value)
		{
			UnitOverviewWindow unitOverviewWindow = ShowWindow();
			selectedCategory = unitInfo.UnitCategory;
			unitToConvertFrom = unitInfo;
			valueToConvertFrom = value;
			selectedUnitInfoIndex = unitInfosInSelectedCategory.IndexOf(unitInfo);
			unitOverviewWindow.TrySelectMenuItemWithObject(selectedCategory);
		}

		protected override void OnEnable()
		{
			base.OnEnable();
			WindowPadding = Vector4.zero;
			MenuWidth = 250f;
			headerGroupStyle = new GUIStyle
			{
				padding = new RectOffset(10, 10, 10, 10)
			};
			IEnumerable<UnitInfo> allUnitInfos = UnitNumberUtility.GetAllUnitInfos();
			unitInfosByCategory.Clear();
			foreach (UnitInfo unitInfo in allUnitInfos)
			{
				if (!unitInfosByCategory.TryGetValue(unitInfo.UnitCategory, out var units))
				{
					units = new List<UnitInfo>();
					unitInfosByCategory.Add(unitInfo.UnitCategory, units);
				}
				units.Add(unitInfo);
			}
			WindowState windowState = LoadFromEditorPrefs<WindowState>("Odin_UnitOverviewWindow_Prefs");
			unitsToConvertFromByCategory = windowState?.UnitsToConvertFromByCategory ?? unitInfosByCategory.ToDictionary((KeyValuePair<string, List<UnitInfo>> p) => p.Key, (KeyValuePair<string, List<UnitInfo>> p) => p.Value.First());
			valuesToConvertFromByCategory = windowState?.ValuesToConvertFromByCategory ?? unitInfosByCategory.Keys.ToDictionary((string c) => c, (string _) => 1m);
			selectedUnitInfoIndexByCategory = windowState?.SelectedUnitInfoIndexByCategory ?? unitInfosByCategory.ToDictionary((KeyValuePair<string, List<UnitInfo>> p) => p.Key, (KeyValuePair<string, List<UnitInfo>> p) => 0);
			selectedCategory = windowState?.SelectedCategory ?? unitInfosByCategory.Keys.First();
			codeHeight = windowState?.CodeHeight ?? 200f;
		}

		protected override void OnDisable()
		{
			SaveToEditorPrefs(new WindowState
			{
				CodeHeight = codeHeight,
				SelectedCategory = selectedCategory,
				UnitsToConvertFromByCategory = unitsToConvertFromByCategory,
				ValuesToConvertFromByCategory = valuesToConvertFromByCategory,
				SelectedUnitInfoIndexByCategory = selectedUnitInfoIndexByCategory
			}, "Odin_UnitOverviewWindow_Prefs");
			base.OnDisable();
		}

		protected override OdinMenuTree BuildMenuTree()
		{
			OdinMenuTree tree = new OdinMenuTree();
			tree.Config.DrawSearchToolbar = true;
			tree.DefaultMenuStyle.Height = 25;
			tree.Selection.SelectionChanged += SelectionChanged;
			foreach (string category in unitInfosByCategory.Keys)
			{
				tree.Add(category, category);
			}
			if (!string.IsNullOrEmpty(selectedCategory))
			{
				tree.MenuItems.FirstOrDefault((OdinMenuItem mi) => mi.Name == selectedCategory)?.Select();
			}
			else
			{
				tree.MenuItems.FirstOrDefault()?.Select();
			}
			return tree;
		}

		protected override void DrawEditor(int _)
		{
			DrawHeader();
			DrawTable();
			codeHeight -= SirenixEditorGUI.SlideRect(dragRect.Expand(5f).AddY(2f), MouseCursor.SplitResizeUpDown).y;
			if (Event.current.type == EventType.Repaint)
			{
				dragRect = GUILayoutUtility.GetRect(0f, 4f);
			}
			else
			{
				GUILayoutUtility.GetRect(0f, 4f);
			}
			examplePreview.DrawCode(codeHeight);
			DrawMenuCollapseAndExpandButton();
		}

		private void DrawHeader()
		{
			Rect headerRect = EditorGUILayout.BeginVertical(headerGroupStyle);
			Rect buttonRect = headerRect.AlignRight(100f).AlignCenterY(20f).SubX(10f);
			EditorGUI.DrawRect(headerRect, SirenixGUIStyles.BoxBackgroundColor);
			GUILayout.Label(selectedCategory, SirenixGUIStyles.SectionHeader);
			if (SirenixEditorGUI.SDFIconButton(buttonRect, GUIHelper.TempContent("Reset"), SdfIconType.ArrowCounterclockwise))
			{
				foreach (string category in unitInfosByCategory.Keys)
				{
					valuesToConvertFromByCategory[category] = 1m;
				}
			}
			EditorGUILayout.EndVertical();
			EditorGUI.DrawRect(headerRect.AlignBottom(1f), SirenixGUIStyles.BorderColor);
		}

		private void DrawTable()
		{
			scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
			Rect contentRect = EditorGUILayout.BeginVertical(GUILayout.ExpandHeight(expand: true));
			Rect tableHeaderRect = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.ExpandWidth(expand: true), GUILayout.Height(25f));
			EditorGUI.DrawRect(tableHeaderRect, tableHeaderColor);
			EditorGUI.DrawRect(tableHeaderRect.AlignBottom(1f), SirenixGUIStyles.BorderColor);
			bool drawSymbolCol = tableHeaderRect.width - symbolColWidth - nameColWidth > 200f;
			Rect tableHeaderNameColRect = tableHeaderRect.TakeFromLeft(nameColWidth);
			Rect tableHeaderSymbolColRect = (drawSymbolCol ? tableHeaderRect.TakeFromRight(symbolColWidth) : Rect.zero);
			Rect tableHeaderConvertedColRect = tableHeaderRect;
			EditorGUI.LabelField(tableHeaderNameColRect, "Name", SirenixGUIStyles.BoldLabelCentered);
			EditorGUI.LabelField(tableHeaderConvertedColRect, "Converted", SirenixGUIStyles.BoldLabelCentered);
			if (drawSymbolCol)
			{
				EditorGUI.LabelField(tableHeaderSymbolColRect, "Symbols", SirenixGUIStyles.BoldLabelCentered);
			}
			Rect tableRect = EditorGUILayout.BeginVertical();
			for (int i = 0; i < unitInfosInSelectedCategory.Count; i++)
			{
				UnitInfo unitInfo = unitInfosInSelectedCategory[i];
				Rect rowRect = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayoutOptions.ExpandWidth().Height(25f));
				if (Event.current.OnMouseDown(rowRect, 0, useEvent: false))
				{
					selectedUnitInfoIndex = i;
					UpdateExamplePreview();
				}
				Color cellColor = ((selectedUnitInfoIndex == i) ? SirenixGUIStyles.DefaultSelectedMenuTreeColor : ((!rowRect.Contains(Event.current.mousePosition)) ? ((i % 2 == 0) ? SirenixGUIStyles.ListItemColorEven : SirenixGUIStyles.ListItemColorOdd) : (EditorGUIUtility.isProSkin ? new Color(0.3f, 0.3f, 0.3f) : new Color(0.75f, 0.75f, 0.75f))));
				EditorGUI.DrawRect(rowRect, cellColor);
				Rect nameColRect = rowRect.TakeFromLeft(nameColWidth).Padding(6f, 3f);
				Rect symbolColRect = (drawSymbolCol ? rowRect.TakeFromRight(symbolColWidth).Padding(6f, 3f) : Rect.zero);
				Rect convertedColRect = rowRect.Padding(6f, 3f);
				DrawBadge(nameColRect, unitInfo.Name);
				if (drawSymbolCol)
				{
					int symbolCount = unitInfo.Symbols.Length;
					for (int j = 0; j < symbolCount; j++)
					{
						string symbol = unitInfo.Symbols[j];
						Rect symbolRect = symbolColRect.Split(j, symbolCount);
						if (j != 0)
						{
							symbolRect = symbolRect.AddXMin(3f);
						}
						if (j != symbolCount - 1)
						{
							symbolRect = symbolRect.SubXMax(3f);
						}
						DrawBadge(symbolRect, symbol);
					}
				}
				decimal convertedValue = default(decimal);
				try
				{
					convertedValue = UnitNumberUtility.ConvertUnitFromTo(valueToConvertFrom, unitToConvertFrom, unitInfo);
				}
				catch (OverflowException)
				{
					EditorGUI.LabelField(convertedColRect, "Overflow");
					continue;
				}
				try
				{
					EditorGUI.BeginChangeCheck();
					decimal newValue = SirenixEditorFields.DecimalUnitField(convertedColRect, convertedValue, unitInfo, unitInfo);
					if (EditorGUI.EndChangeCheck())
					{
						valueToConvertFrom = UnitNumberUtility.ConvertUnitFromTo(newValue, unitInfo, unitToConvertFrom);
					}
				}
				catch (OverflowException)
				{
				}
			}
			EditorGUILayout.EndVertical();
			float emptySpace = contentRect.yMax - tableRect.yMax;
			int emptyRowCount = Mathf.CeilToInt(emptySpace / 25f);
			for (int k = 0; k < emptyRowCount; k++)
			{
				int indexRelativeToTable = unitInfosInSelectedCategory.Count + k;
				Color cellColor2 = ((indexRelativeToTable % 2 == 0) ? SirenixGUIStyles.ListItemColorEven : SirenixGUIStyles.ListItemColorOdd);
				EditorGUI.DrawRect(new Rect(tableRect.x, tableRect.yMax + (float)(k * 25), tableRect.width, 25f), cellColor2);
			}
			EditorGUI.DrawRect(tableHeaderConvertedColRect.AlignLeft(1f).SetHeight(contentRect.height), SirenixGUIStyles.BorderColor);
			if (drawSymbolCol)
			{
				EditorGUI.DrawRect(tableHeaderConvertedColRect.AlignRight(1f).SetHeight(contentRect.height), SirenixGUIStyles.BorderColor);
			}
			EditorGUILayout.EndVertical();
			EditorGUILayout.EndScrollView();
		}

		private void DrawBadge(Rect rect, string label)
		{
			Color badgeBackgroundColor = ((!rect.Contains(Event.current.mousePosition)) ? (EditorGUIUtility.isProSkin ? new Color(0.15f, 0.15f, 0.15f) : new Color(0.65f, 0.65f, 0.65f)) : (EditorGUIUtility.isProSkin ? new Color(0.1f, 0.1f, 0.1f) : new Color(0.55f, 0.55f, 0.55f)));
			GUI.DrawTexture(rect, Texture2D.whiteTexture, ScaleMode.StretchToFill, alphaBlend: false, 1f, badgeBackgroundColor, 0f, 3f);
			EditorGUI.LabelField(rect, GUIHelper.TempContent(label, "Copy"), SirenixGUIStyles.LabelCentered);
			EditorGUIUtility.AddCursorRect(rect, MouseCursor.Link);
			if (Event.current.OnMouseDown(rect, 0))
			{
				GUIUtility.systemCopyBuffer = label;
				Debug.Log("Copied <b>" + label + "</b> into the clipboard.");
			}
		}

		private void DrawMenuCollapseAndExpandButton()
		{
			Rect iconRect = new Rect(5f, base.position.height - 30f, 25f, 25f);
			if (iconRect.Contains(Event.current.mousePosition))
			{
				bool menuIsOpen = MenuWidth > 80f;
				Color iconColor = (EditorGUIUtility.isProSkin ? Color.white : Color.black);
				SdfIcons.DrawIcon(iconRect, menuIsOpen ? SdfIconType.ArrowLeftCircleFill : SdfIconType.ArrowRightCircleFill, iconColor);
				if (Event.current.OnMouseUp(iconRect, 0))
				{
					MenuWidth = (menuIsOpen ? (-1f) : 250f);
				}
			}
		}

		private void SelectionChanged(SelectionChangedType _)
		{
			OdinMenuItem selectedMenuItem = base.MenuTree?.Selection.FirstOrDefault();
			if (selectedMenuItem != null)
			{
				selectedCategory = (string)selectedMenuItem.Value;
			}
			nameColWidth = unitInfosInSelectedCategory.Max((UnitInfo ui) => SirenixGUIStyles.Label.CalcSize(GUIHelper.TempContent(ui.Name)).x + 25f);
			symbolColWidth = unitInfosInSelectedCategory.Max((UnitInfo ui) => ui.Symbols.Sum((string s) => SirenixGUIStyles.CenteredTextField.CalcSize(GUIHelper.TempContent(s)).x + 25f));
			nameColWidth = Mathf.Max(nameColWidth, 61f);
			symbolColWidth = Mathf.Max(symbolColWidth, 78f);
			UpdateExamplePreview();
		}

		private static void SaveToEditorPrefs<T>(T valueToSave, string key)
		{
			byte[] bytes = Sirenix.Serialization.SerializationUtility.SerializeValue(valueToSave, DataFormat.JSON);
			string bytesAsString = Encoding.UTF8.GetString(bytes);
			EditorPrefs.SetString(key, bytesAsString);
		}

		private static T LoadFromEditorPrefs<T>(string key)
		{
			string bytesAsString = EditorPrefs.GetString(key);
			byte[] bytes = Encoding.UTF8.GetBytes(bytesAsString);
			return Sirenix.Serialization.SerializationUtility.DeserializeValue<T>(bytes, DataFormat.JSON);
		}

		private static void UpdateExamplePreview()
		{
			string[] defaultUnits = Enum.GetNames(typeof(Units));
			UnitInfo selectedUnit = unitInfosInSelectedCategory[selectedUnitInfoIndex];
			string selectedUnitName = EnumName(selectedUnit.Name);
			UnitInfo nonSelectedUnit = unitInfosInSelectedCategory.FirstOrDefault((UnitInfo ui) => ui != selectedUnit);
			string nonSelectedUnitName = EnumName(nonSelectedUnit.Name);
			bool selectedUnitIsDefaultUnit = defaultUnits.Contains(selectedUnitName);
			bool nonSelectedUnitIsDefaultUnit = defaultUnits.Contains(nonSelectedUnitName);
			string selectedUnitParameter = (selectedUnitIsDefaultUnit ? ("Units." + selectedUnitName) : ("\"" + selectedUnit.Name + "\""));
			string nonSelectedUnitParameter = (nonSelectedUnitIsDefaultUnit ? ("Units." + nonSelectedUnitName) : ("\"" + nonSelectedUnit.Name + "\""));
			string code = "// The unit attribute consists of a display unit and the base unit.\n// The display unit is what you see and enter in the inspector and the base unit is what will actually be saved as the value.\n// For example, let's pretend that we have this code:\n[Unit(base: Units.Meter, display: Units.Centimeter)]\npublic float Example_FromCentimeterToMeter;\n\n// That should create a field in the inspector that displays as the unit 'Centimeter'\n// If we now enter the number 100 into the field it should show '100 cm' and the value that is saved in the float field should be 1 since 100cm == 1m\n\n\n// =====================================================================================================================================\n\n\n// Here are examples of all possible UnitAttribute setups with the currently selected unit (" + selectedUnit.Name + ") as the display value.\n\n";
			foreach (UnitInfo unitInfo in unitInfosInSelectedCategory)
			{
				if (unitInfo != selectedUnit)
				{
					string unitName = EnumName(unitInfo.Name);
					string unitParameter = (defaultUnits.Contains(unitName) ? ("Units." + unitName) : ("\"" + unitInfo.Name + "\""));
					code = code + "[Unit(base: " + unitParameter + ", display: " + selectedUnitParameter + ")]\npublic float From" + selectedUnitName + "To" + unitName + ";\n\n";
				}
			}
			code = code + "\n// =====================================================================================================================================\n\n\n// You can change the display unit by right clicking the field in the inspector and selecting 'Change Unit'.\n// Keep in mind though that this is not persistent and will reset to the display value chosen in code eventually.\n// If you want to force the display unit chosen in code you can set the ForceDisplayUnit parameter to true.\n[Unit(base: " + nonSelectedUnitParameter + ", display: " + selectedUnitParameter + ", ForceDisplayUnit = true)]\npublic float From" + selectedUnitName + "To" + nonSelectedUnitName + "WithForcedDisplayUnit;\n\n\n// If you want to display the value as a non-editable readonly string you can do so by setting the DisplayAsString parameter to true.\n[Unit(base: " + nonSelectedUnitParameter + ", display: " + selectedUnitParameter + ", DisplayAsString = true)]\npublic float From" + selectedUnitName + "To" + nonSelectedUnitName + "DisplayedAsReadonlyString;\n\n\n// You can also use a Unit's name instead of an enum value.\n[Unit(base: \"" + nonSelectedUnit.Name + "\", display: \"" + selectedUnit.Name + "\")]\npublic float From" + selectedUnitName + "To" + nonSelectedUnitName + "WithNameInsteadOfEnum;\n\n\n// Using a Unit's name instead of an enum value is useful when you have added your own custom units.\n#if UNITY_EDITOR\n[InitializeOnLoadMethod]\nprivate static void AddCustomUnits()\n{\n    UnitNumberUtility.AddCustomUnit(\n        name: \"Fenrir\",\n        symbols: new[] { \"Fenrir\", \"fenrir\", \"f\" },\n        unitCategory: UnitCategory.Distance,\n        multiplier: 7); // Since meter is the base unit in the distance category, 1 meter will be 7 fenrir\n}\n#endif\n\n[Unit(base: Units.Meter, display: \"Fenrir\")]\npublic float CustomUnit_FromFenrirToMeter;";
			string codeAsComponent = "using UnityEngine;\nusing Sirenix.OdinInspector;\n\n#if UNITY_EDITOR\nusing UnityEditor;\nusing Sirenix.Utilities.Editor;\n#endif\n\npublic class SomeMonoBehaviour : MonoBehaviour\n{\n    " + code.Replace("\n", "\n    ") + "\n}";
			AttributeExampleInfo attributeExampleInfo = new AttributeExampleInfo();
			attributeExampleInfo.Code = code;
			attributeExampleInfo.CodeAsComponent = codeAsComponent;
			attributeExampleInfo.ExampleType = typeof(Units);
			examplePreview = new AttributeExamplePreview(attributeExampleInfo);
		}

		private static string EnumName(string name)
		{
			string[] words = name.Replace("(", "").Replace(")", "").Replace("/", "")
				.Split(new char[2] { ' ', '-' }, StringSplitOptions.RemoveEmptyEntries);
			for (int i = 0; i < words.Length; i++)
			{
				words[i] = char.ToUpper(words[i][0]) + words[i].Substring(1);
			}
			return string.Join("", words);
		}
	}
}
