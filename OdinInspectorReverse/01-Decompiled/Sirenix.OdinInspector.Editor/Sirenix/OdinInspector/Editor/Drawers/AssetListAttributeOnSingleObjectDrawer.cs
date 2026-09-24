using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Sirenix.OdinInspector.Editor.ValueResolvers;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	/// <summary>
	/// Not yet documented.
	/// </summary>
	[DrawerPriority(0.0, 0.0, 3001.0)]
	public class AssetListAttributeOnSingleObjectDrawer<TElement> : OdinAttributeDrawer<AssetListAttribute, TElement> where TElement : UnityEngine.Object
	{
		private static readonly NamedValue[] customFilterMethodArgs = new NamedValue[1]
		{
			new NamedValue("asset", typeof(TElement))
		};

		private ValueResolver<bool> customFilterMethod;

		private List<UnityEngine.Object> availableAssets = new List<UnityEngine.Object>();

		private string[] tags;

		private string[] layerNames;

		private DirectoryInfo assetsFolderLocation;

		private string prettyPath;

		private bool isPopulated;

		private double maxSearchDurationPrFrameInMS = 1.0;

		private int numberOfResultsToSearch;

		private int currentSearchingIndex;

		private IEnumerator populateListRoutine;

		protected override void Initialize()
		{
			IPropertyValueEntry<TElement> entry = base.ValueEntry;
			AssetListAttribute attribute = base.Attribute;
			tags = ((attribute.Tags != null) ? (from i in attribute.Tags.Trim().Split(new char[1] { ',' })
				select i.Trim()).ToArray() : null);
			layerNames = ((attribute.LayerNames != null) ? (from i in attribute.LayerNames.Trim().Split(new char[1] { ',' })
				select i.Trim()).ToArray() : null);
			if (attribute.Path != null)
			{
				string path = attribute.Path.Trim('/', ' ');
				path = "Assets/" + path + "/";
				path = Application.dataPath + "/" + path;
				assetsFolderLocation = new DirectoryInfo(path);
				path = attribute.Path.TrimStart(new char[1] { '/' }).TrimEnd(new char[1] { '/' });
				prettyPath = "/" + path.TrimStart(new char[1] { '/' });
			}
			if (attribute.CustomFilterMethod != null)
			{
				customFilterMethod = ValueResolver.Get<bool>(base.Property, attribute.CustomFilterMethod, customFilterMethodArgs);
			}
			if (Event.current != null)
			{
				maxSearchDurationPrFrameInMS = 20.0;
				EnsureListPopulation();
			}
			maxSearchDurationPrFrameInMS = 1.0;
		}

		/// <summary>
		/// Not yet documented.
		/// </summary>
		protected override void DrawPropertyLayout(GUIContent label)
		{
			IPropertyValueEntry<TElement> entry = base.ValueEntry;
			AssetListAttribute attribute = base.Attribute;
			UnityEngine.Object currentValue = (UnityEngine.Object)entry.WeakSmartValue;
			if (customFilterMethod != null && customFilterMethod.HasError)
			{
				customFilterMethod.DrawError();
			}
			else
			{
				EnsureListPopulation();
			}
			SirenixEditorGUI.BeginIndentedVertical(SirenixGUIStyles.PropertyPadding);
			SirenixEditorGUI.BeginHorizontalToolbar();
			if (label != null)
			{
				GUILayout.Label(label);
			}
			GUILayout.FlexibleSpace();
			if (prettyPath != null)
			{
				GUILayout.Label(prettyPath, SirenixGUIStyles.RightAlignedGreyMiniLabel);
				SirenixEditorGUI.VerticalLineSeparator();
			}
			if (isPopulated)
			{
				GUILayout.Label(availableAssets.Count + " items", SirenixGUIStyles.RightAlignedGreyMiniLabel);
				GUIHelper.PushGUIEnabled(GUI.enabled && availableAssets.Count > 0 && (customFilterMethod == null || !customFilterMethod.HasError));
			}
			else
			{
				GUILayout.Label("Scanning " + currentSearchingIndex + " / " + numberOfResultsToSearch, SirenixGUIStyles.RightAlignedGreyMiniLabel);
				GUIHelper.PushGUIEnabled(enabled: false);
			}
			SirenixEditorGUI.VerticalLineSeparator();
			bool drawConflict = entry.Property.ParentValues.Count > 1;
			if (!drawConflict)
			{
				int index = availableAssets.IndexOf(currentValue) + 1;
				if (index > 0)
				{
					GUILayout.Label(index.ToString(), SirenixGUIStyles.RightAlignedGreyMiniLabel);
				}
				else
				{
					drawConflict = true;
				}
			}
			if (drawConflict)
			{
				GUILayout.Label("-", SirenixGUIStyles.RightAlignedGreyMiniLabel);
			}
			if (SirenixEditorGUI.ToolbarButton(EditorIcons.TriangleLeft) && isPopulated)
			{
				int index2 = availableAssets.IndexOf(currentValue) - 1;
				index2 = ((index2 < 0) ? (availableAssets.Count - 1) : index2);
				entry.WeakSmartValue = availableAssets[index2];
			}
			if (SirenixEditorGUI.ToolbarButton(EditorIcons.TriangleDown) && isPopulated)
			{
				GenericMenu m = new GenericMenu();
				UnityEngine.Object selected = currentValue;
				int itemsPrPage = 40;
				bool showPages = availableAssets.Count > 50;
				string page = "";
				int selectedPage = availableAssets.IndexOf(entry.WeakSmartValue as UnityEngine.Object) / itemsPrPage;
				for (int i = 0; i < availableAssets.Count; i++)
				{
					UnityEngine.Object obj = availableAssets[i];
					if (!(obj != null))
					{
						continue;
					}
					string path = AssetDatabase.GetAssetPath(obj);
					string name = (string.IsNullOrEmpty(path) ? obj.name : path.Substring(7).Replace("/", "\\"));
					IPropertyValueEntry<TElement> localEntry = entry;
					if (showPages)
					{
						int p = i / itemsPrPage;
						page = p * itemsPrPage + " - " + Mathf.Min((p + 1) * itemsPrPage, availableAssets.Count - 1);
						if (selectedPage == p)
						{
							page += " (contains selected)";
						}
						page += "/";
					}
					m.AddItem(new GUIContent(page + name), obj == selected, delegate
					{
						localEntry.Property.Tree.DelayActionUntilRepaint(delegate
						{
							localEntry.WeakSmartValue = obj;
						});
					});
				}
				m.ShowAsContext();
			}
			if (SirenixEditorGUI.ToolbarButton(EditorIcons.TriangleRight) && isPopulated)
			{
				int index3 = availableAssets.IndexOf(currentValue) + 1;
				entry.WeakSmartValue = availableAssets[index3 % availableAssets.Count];
			}
			GUIHelper.PopGUIEnabled();
			SirenixEditorGUI.EndHorizontalToolbar();
			SirenixEditorGUI.BeginVerticalList(true, true);
			SirenixEditorGUI.BeginListItem(false, null);
			CallNextDrawer(null);
			SirenixEditorGUI.EndListItem();
			SirenixEditorGUI.EndVerticalList();
			SirenixEditorGUI.EndIndentedVertical();
		}

		private IEnumerator PopulateListRoutine()
		{
			while (true)
			{
				if (isPopulated)
				{
					yield return null;
					continue;
				}
				HashSet<UnityEngine.Object> seenObjects = new HashSet<UnityEngine.Object>();
				int[] layers = ((layerNames != null) ? layerNames.Select((string l) => LayerMask.NameToLayer(l)).ToArray() : null);
				availableAssets.Clear();
				IEnumerable<AssetUtilities.AssetSearchResult> allAssets = ((prettyPath != null) ? AssetUtilities.GetAllAssetsOfTypeWithProgress(base.Property.ValueEntry.BaseValueType, "Assets/" + prettyPath.TrimStart(new char[1] { '/' })) : AssetUtilities.GetAllAssetsOfTypeWithProgress(base.Property.ValueEntry.BaseValueType));
				Stopwatch sw = new Stopwatch();
				sw.Start();
				foreach (AssetUtilities.AssetSearchResult p in allAssets)
				{
					if (sw.Elapsed.TotalMilliseconds > maxSearchDurationPrFrameInMS)
					{
						numberOfResultsToSearch = p.NumberOfResults;
						currentSearchingIndex = p.CurrentIndex;
						GUIHelper.RequestRepaint();
						yield return null;
						sw.Reset();
						sw.Start();
					}
					UnityEngine.Object asset = p.Asset;
					if (!(asset != null) || !seenObjects.Add(asset))
					{
						continue;
					}
					GameObject go = ((asset as Component != null) ? (asset as Component).gameObject : ((asset as GameObject == null) ? null : (asset as GameObject)));
					string assetName = ((go == null) ? asset.name : go.name);
					if (base.Attribute.AssetNamePrefix != null && !assetName.StartsWith(base.Attribute.AssetNamePrefix, StringComparison.InvariantCultureIgnoreCase))
					{
						continue;
					}
					if (assetsFolderLocation != null)
					{
						DirectoryInfo path = new DirectoryInfo(Path.GetDirectoryName(Application.dataPath + "/" + AssetDatabase.GetAssetPath(asset)));
						if (!assetsFolderLocation.HasSubDirectory(path))
						{
							continue;
						}
					}
					if ((layerNames != null && go == null) || (tags != null && go == null) || (go != null && tags != null && !tags.Contains(go.tag)) || (go != null && layerNames != null && !layers.Contains(go.layer)))
					{
						continue;
					}
					if (customFilterMethod != null)
					{
						customFilterMethod.Context.NamedValues.Set("asset", asset);
						if (!customFilterMethod.GetValue())
						{
							continue;
						}
					}
					availableAssets.Add(asset);
				}
				isPopulated = true;
				GUIHelper.RequestRepaint();
				yield return null;
			}
		}

		public void EnsureListPopulation()
		{
			if (Event.current.type == EventType.Layout)
			{
				if (populateListRoutine == null)
				{
					populateListRoutine = PopulateListRoutine();
				}
				populateListRoutine.MoveNext();
			}
		}
	}
}
