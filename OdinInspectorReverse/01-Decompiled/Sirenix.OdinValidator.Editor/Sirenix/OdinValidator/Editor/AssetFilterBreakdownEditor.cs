using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace Sirenix.OdinValidator.Editor
{
	public class AssetFilterBreakdownEditor
	{
		private struct Item
		{
			public string Path;

			public int SizeInBytes;
		}

		private Item[] allItems;

		private Item[] items;

		private LargeGuiCollectionHelper listHelper;

		private Vector2 sp;

		private int sizeSortState;

		private int pathSortState;

		private UnityEditor.IMGUI.Controls.SearchField searchField = new UnityEditor.IMGUI.Controls.SearchField();

		private string searchTerm;

		private GUIStyle padding10;

		private GUIStyle padding5;

		public AssetFilterBreakdownEditor(string[] allAssetPaths)
		{
			allItems = new Item[allAssetPaths.Length];
			for (int i = 0; i < allAssetPaths.Length; i++)
			{
				string path = allAssetPaths[i];
				long sizeForFileOrFolder = (File.Exists(path) ? new FileInfo(path).Length : 0);
				allItems[i] = new Item
				{
					Path = path,
					SizeInBytes = (int)sizeForFileOrFolder
				};
			}
			allItems = allItems.OrderByDescending((Item x) => x.SizeInBytes).ToArray();
			items = allItems;
			sizeSortState = 1;
			pathSortState = 0;
		}

		[OnInspectorGUI]
		private void OnGui()
		{
			padding10 = padding10 ?? new GUIStyle
			{
				padding = new RectOffset(10, 10, 10, 10)
			};
			padding5 = padding5 ?? new GUIStyle
			{
				padding = new RectOffset(5, 5, 5, 5)
			};
			EditorGUI.DrawRect(GUIHelper.GetCurrentLayoutRect(), SirenixGUIStyles.DarkEditorBackground);
			Rect rect = EditorGUILayout.BeginVertical(padding10);
			EditorGUI.DrawRect(rect, SirenixGUIStyles.BoxBackgroundColor);
			EditorGUI.DrawRect(rect.AlignBottom(1f), ValidatorGui.BorderColor);
			GUILayout.Label("Asset filter breakdown", SirenixGUIStyles.BoldTitle);
			GUILayout.Label("Below is a consolidated list of all assets set for validation under the current profile. This includes filters from both the 'For everyone' and 'For this machine' categories. Assets can easily be included or excluded by right clicking them in the project window.", SirenixGUIStyles.MultiLineLabel);
			EditorGUILayout.EndVertical();
			Rect rect2 = GUILayoutUtility.GetRect(0f, 30f);
			EditorGUI.DrawRect(rect2.AlignBottom(1f), ValidatorGui.BorderColor);
			string oldSearchTerm = searchTerm;
			string newSearchTerm = searchField.OnGUI(rect2.HorizontalPadding(10f).AlignCenterY(20f), oldSearchTerm, EditorStyles.toolbarSearchField, GUIStyle.none, GUIStyle.none);
			if (oldSearchTerm != newSearchTerm)
			{
				searchTerm = newSearchTerm;
				UpdateFilter();
			}
			Rect tableHeaderRect = GUILayoutUtility.GetRect(0f, 20f);
			EditorGUI.DrawRect(tableHeaderRect.AlignBottom(1f), ValidatorGui.BorderColor);
			int sizeColumnSize = 100;
			float pathColumnSize = tableHeaderRect.width - (float)sizeColumnSize;
			int hPadding = 5;
			EditorGUI.DrawRect(tableHeaderRect, ValidatorGui.EditorWindowBgColor);
			Rect pathColRect = tableHeaderRect.TakeFromLeft(pathColumnSize);
			Rect sizeColRect = tableHeaderRect.TakeFromLeft(sizeColumnSize);
			if (sizeColRect.Contains(Event.current.mousePosition))
			{
				EditorGUI.DrawRect(sizeColRect, ValidatorGui.HighlightedBgColor);
			}
			if (pathColRect.Contains(Event.current.mousePosition))
			{
				EditorGUI.DrawRect(pathColRect, ValidatorGui.HighlightedBgColor);
			}
			sizeColRect = sizeColRect.HorizontalPadding(hPadding);
			pathColRect = pathColRect.HorizontalPadding(hPadding);
			EditorGUI.DrawRect(tableHeaderRect.AlignBottom(1f), ValidatorGui.BorderColor);
			if (GUI.Button(sizeColRect, "Size", SirenixGUIStyles.Label))
			{
				if (sizeSortState == 1)
				{
					allItems = allItems.OrderBy((Item x) => x.SizeInBytes).ToArray();
					sizeSortState = 2;
				}
				else
				{
					allItems = allItems.OrderByDescending((Item x) => x.SizeInBytes).ToArray();
					sizeSortState = 1;
				}
				UpdateFilter();
				pathSortState = 0;
			}
			if (GUI.Button(pathColRect, "Path", SirenixGUIStyles.Label))
			{
				if (pathSortState == 1)
				{
					allItems.Sort((Item a, Item b) => StringUtilities.NumberAwareStringCompare(a.Path, b.Path));
					pathSortState = 2;
				}
				else
				{
					allItems.Sort((Item a, Item b) => StringUtilities.NumberAwareStringCompare(b.Path, a.Path));
					pathSortState = 1;
				}
				UpdateFilter();
				sizeSortState = 0;
			}
			if (sizeSortState > 0)
			{
				SdfIconType icon = ((sizeSortState == 1) ? SdfIconType.CaretDownFill : SdfIconType.CaretUpFill);
				SdfIcons.DrawIcon(sizeColRect.AlignRight(25f).Padding(6f), icon);
			}
			if (pathSortState > 0)
			{
				SdfIconType icon2 = ((pathSortState == 1) ? SdfIconType.CaretDownFill : SdfIconType.CaretUpFill);
				SdfIcons.DrawIcon(pathColRect.AlignRight(25f).Padding(6f), icon2);
			}
			sp = GUILayout.BeginScrollView(sp, GUILayoutOptions.ExpandHeight().ExpandWidth());
			listHelper.AllocateLayout(20, items.Length);
			for (int i = listHelper.StartIndex; i < listHelper.EndIndex; i++)
			{
				Rect rect3 = listHelper.GetRect(i);
				EditorGUI.DrawRect(rect3.AlignBottom(1f), ValidatorGui.BorderColor);
				if (GUI.Button(rect3, GUIContent.none, GUIStyle.none))
				{
					EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(items[i].Path));
				}
				if (rect3.Contains(Event.current.mousePosition))
				{
					EditorGUI.DrawRect(rect3, ValidatorGui.HighlightedBgColor);
				}
				Rect pathColRect2 = rect3.TakeFromLeft(pathColumnSize).HorizontalPadding(hPadding);
				Rect sizeColRect2 = rect3.TakeFromLeft(sizeColumnSize).HorizontalPadding(hPadding);
				if (items[i].SizeInBytes > 0)
				{
					GUI.Label(sizeColRect2, EditorUtility.FormatBytes(items[i].SizeInBytes), SirenixGUIStyles.Label);
				}
				Texture icon3 = AssetDatabase.GetCachedIcon(items[i].Path);
				GUI.DrawTexture(pathColRect2.TakeFromLeft(25f).AlignCenterXY(16f), icon3);
				GUI.Label(pathColRect2, items[i].Path, SirenixGUIStyles.Label);
			}
			GUILayout.EndScrollView();
		}

		private void UpdateFilter()
		{
			if (string.IsNullOrEmpty(searchTerm))
			{
				items = allItems;
				return;
			}
			items = allItems.Where((Item x) => x.Path.IndexOf(searchTerm, StringComparison.InvariantCultureIgnoreCase) != -1).ToArray();
		}

		public static void ShowBreadownWindow(IList<IValidationProfile> dataSources)
		{
			SessionConfig config = new SessionConfig(dataSources.ToArray());
			config.UpdateAll();
			HashSet<string> assets = config.GetAssetsToValidate();
			HashSet<UnityEngine.Object> objects = config.GetObjectsToValidate();
			HashSet<string> scenes = config.GetSceneGuidsToValidate();
			IEnumerable<string> allAssets = (from x in assets.Select((string x) => AssetDatabase.GUIDToAssetPath(x)).Concat(objects.Select((UnityEngine.Object x) => AssetDatabase.GetAssetPath(x))).Concat(scenes.Select((string x) => AssetDatabase.GUIDToAssetPath(x)))
				where x != null
				select x).Distinct();
			AssetFilterBreakdownEditor editor = new AssetFilterBreakdownEditor(allAssets.ToArray());
			OdinEditorWindow wnd = OdinEditorWindow.InspectObjectInDropDown(editor, 600f);
			wnd.WindowPadding = default(Vector4);
		}
	}
}
