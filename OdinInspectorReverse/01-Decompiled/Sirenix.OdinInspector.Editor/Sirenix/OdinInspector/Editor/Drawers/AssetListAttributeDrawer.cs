using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Sirenix.OdinInspector.Editor.Internal;
using Sirenix.OdinInspector.Editor.ValueResolvers;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	/// <summary>
	/// Draws properties marked with <see cref="T:Sirenix.OdinInspector.AssetListAttribute" />.
	/// Displays a configurable list of assets, where each item can be enabled or disabled.
	/// </summary>
	/// <seealso cref="T:Sirenix.OdinInspector.AssetListAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.AssetsOnlyAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.SceneObjectsOnlyAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.RequiredAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.ValidateInputAttribute" />
	[DrawerPriority(DrawerPriorityLevel.AttributePriority)]
	public sealed class AssetListAttributeDrawer<TList, TElement> : OdinAttributeDrawer<AssetListAttribute, TList>, IDefinesGenericMenuItems, IDisposable where TList : IList<TElement> where TElement : UnityEngine.Object
	{
		[Serializable]
		[ShowOdinSerializedPropertiesInInspector]
		private class AssetList
		{
			[HideInInspector]
			public bool AutoPopulate;

			[HideInInspector]
			public string AssetNamePrefix;

			[HideInInspector]
			public string[] LayerNames;

			[HideInInspector]
			public string[] Tags;

			[HideInInspector]
			public IPropertyValueEntry<TList> List;

			[HideInInspector]
			public IOrderedCollectionResolver CollectionResolver;

			[HideInInspector]
			public DirectoryInfo AssetsFolderLocation;

			[HideInInspector]
			public string PrettyPath;

			[HideInInspector]
			public ValueResolver<bool> CustomFilterMethod;

			[HideInInspector]
			public InspectorProperty Property;

			[SerializeField]
			[ListDrawerSettings(IsReadOnly = true, DraggableItems = false, OnTitleBarGUI = "OnListTitlebarGUI", ShowItemCount = false)]
			[DisableContextMenu(true, true)]
			[HideReferenceObjectPicker]
			private List<ToggleableAsset> toggleableAssets = new List<ToggleableAsset>();

			[SerializeField]
			[HideInInspector]
			private HashSet<TElement> toggledAssets = new HashSet<TElement>();

			[SerializeField]
			[HideInInspector]
			private Dictionary<TElement, ToggleableAsset> toggleableAssetLookup = new Dictionary<TElement, ToggleableAsset>();

			[NonSerialized]
			public bool IsPopulated;

			[NonSerialized]
			public double MaxSearchDurationPrFrameInMS = 1.0;

			[NonSerialized]
			public int NumberOfResultsToSearch;

			[NonSerialized]
			public int TotalSearchCount;

			[NonSerialized]
			public int CurrentSearchingIndex;

			[NonSerialized]
			private IEnumerator populateListRoutine;

			public List<ToggleableAsset> ToggleableAssets => toggleableAssets;

			private IEnumerator PopulateListRoutine()
			{
				while (true)
				{
					if (IsPopulated)
					{
						yield return null;
						continue;
					}
					HashSet<UnityEngine.Object> seenObjects = new HashSet<UnityEngine.Object>();
					toggleableAssets.Clear();
					toggleableAssetLookup.Clear();
					IEnumerable<AssetUtilities.AssetSearchResult> allAssets = ((PrettyPath != null) ? AssetUtilities.GetAllAssetsOfTypeWithProgress(typeof(TElement), "Assets/" + PrettyPath.TrimStart(new char[1] { '/' })) : AssetUtilities.GetAllAssetsOfTypeWithProgress(typeof(TElement)));
					int[] layers = ((LayerNames != null) ? LayerNames.Select((string l) => LayerMask.NameToLayer(l)).ToArray() : null);
					Stopwatch sw = new Stopwatch();
					sw.Start();
					foreach (AssetUtilities.AssetSearchResult p in allAssets)
					{
						if (sw.Elapsed.TotalMilliseconds > MaxSearchDurationPrFrameInMS)
						{
							NumberOfResultsToSearch = p.NumberOfResults;
							CurrentSearchingIndex = p.CurrentIndex;
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
						if (AssetNamePrefix != null && !assetName.StartsWith(AssetNamePrefix, StringComparison.InvariantCultureIgnoreCase))
						{
							continue;
						}
						if (AssetsFolderLocation != null)
						{
							DirectoryInfo path = new DirectoryInfo(Path.GetDirectoryName(Application.dataPath + "/" + AssetDatabase.GetAssetPath(asset)));
							if (!AssetsFolderLocation.HasSubDirectory(path))
							{
								continue;
							}
						}
						if ((LayerNames != null && go == null) || (Tags != null && go == null) || (go != null && Tags != null && !Tags.Contains(go.tag)) || (go != null && LayerNames != null && !layers.Contains(go.layer)) || toggleableAssetLookup.ContainsKey(asset as TElement))
						{
							continue;
						}
						if (CustomFilterMethod != null)
						{
							CustomFilterMethod.Context.NamedValues.Set("asset", asset);
							if (!CustomFilterMethod.GetValue())
							{
								continue;
							}
						}
						ToggleableAsset toggleable = new ToggleableAsset(asset as TElement, AutoPopulate);
						toggleableAssets.Add(toggleable);
						toggleableAssetLookup.Add(asset as TElement, toggleable);
					}
					SetToggleValues();
					IsPopulated = true;
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

			public void SetToggleValues(int startIndex = 0)
			{
				if (List.SmartValue == null)
				{
					return;
				}
				for (int i = startIndex; i < toggleableAssets.Count; i++)
				{
					if (toggleableAssets[i] == null || toggleableAssets[i].Object == null)
					{
						Rescan();
						break;
					}
					toggleableAssets[i].Toggled = false;
				}
				for (int i2 = List.SmartValue.Count - 1; i2 >= startIndex; i2--)
				{
					TElement asset = List.SmartValue[i2];
					ToggleableAsset toggleable;
					if (asset == null)
					{
						CollectionResolver.QueueRemoveAt(i2);
					}
					else if (toggleableAssetLookup.TryGetValue(asset, out toggleable))
					{
						toggleable.Toggled = true;
					}
					else if (IsPopulated)
					{
						CollectionResolver.QueueRemoveAt(i2);
					}
				}
			}

			public void Rescan()
			{
				IsPopulated = false;
			}

			private void OnListTitlebarGUI()
			{
				if (PrettyPath != null)
				{
					GUILayout.Label(PrettyPath, SirenixGUIStyles.RightAlignedGreyMiniLabel);
					SirenixEditorGUI.VerticalLineSeparator();
				}
				if (IsPopulated)
				{
					GUILayout.Label(List.SmartValue.Count + " / " + toggleableAssets.Count, SirenixGUIStyles.CenteredGreyMiniLabel);
				}
				else
				{
					GUILayout.Label("Scanning " + CurrentSearchingIndex + " / " + NumberOfResultsToSearch, SirenixGUIStyles.RightAlignedGreyMiniLabel);
				}
				bool disableGUI = !IsPopulated;
				if (disableGUI)
				{
					GUIHelper.PushGUIEnabled(enabled: false);
				}
				if (SirenixEditorGUI.ToolbarButton(EditorIcons.Refresh) && IsPopulated)
				{
					Rescan();
				}
				if (AssetUtilities.CanCreateNewAsset<TElement>() && SirenixEditorGUI.ToolbarButton(SdfIconType.Plus) && IsPopulated)
				{
					string path = PrettyPath;
					if (path == null)
					{
						TElement lastAsset = ((List.SmartValue.Count > 0) ? List.SmartValue[List.SmartValue.Count - 1] : null);
						if (lastAsset == null)
						{
							ToggleableAsset lastToggleable = toggleableAssets.LastOrDefault();
							if (lastToggleable != null)
							{
								lastAsset = lastToggleable.Object;
							}
						}
						if (lastAsset != null)
						{
							path = AssetUtilities.GetAssetLocation(lastAsset);
						}
					}
					AssetUtilities.CreateNewAsset<TElement>(path, null);
					Rescan();
				}
				if (disableGUI)
				{
					GUIHelper.PopGUIEnabled();
				}
			}

			public void UpdateList()
			{
				UpdateList(includeAll: false);
			}

			public void UpdateList(bool includeAll)
			{
				if (List.SmartValue == null)
				{
					return;
				}
				toggledAssets.Clear();
				for (int i = 0; i < toggleableAssets.Count; i++)
				{
					if (includeAll || AutoPopulate || toggleableAssets[i].Toggled)
					{
						toggledAssets.Add(toggleableAssets[i].Object);
					}
				}
				for (int i2 = List.SmartValue.Count - 1; i2 >= 0; i2--)
				{
					if (List.SmartValue[i2] == null)
					{
						CollectionResolver.QueueRemoveAt(i2);
						Rescan();
					}
					else if (!toggledAssets.Contains(List.SmartValue[i2]))
					{
						if (IsPopulated)
						{
							CollectionResolver.QueueRemoveAt(i2);
						}
					}
					else
					{
						toggledAssets.Remove(List.SmartValue[i2]);
					}
				}
				foreach (TElement asset in toggledAssets.GFIterator())
				{
					IOrderedCollectionResolver collectionResolver = CollectionResolver;
					object[] values = Enumerable.Repeat(asset, List.ValueCount).ToArray();
					collectionResolver.QueueAdd(values);
				}
				toggledAssets.Clear();
			}
		}

		[Serializable]
		private class ToggleableAsset
		{
			[HideInInspector]
			public bool AutoToggle;

			public bool Toggled;

			public TElement Object;

			public ToggleableAsset(TElement obj, bool autoToggle)
			{
				AutoToggle = autoToggle;
				Object = obj;
			}
		}

		private sealed class AssetInstanceDrawer : OdinValueDrawer<ToggleableAsset>
		{
			protected override void DrawPropertyLayout(GUIContent label)
			{
				OdinInternalEditorFields.UnityObjectFieldArgs drawArgs = OdinInternalEditorFields.UnityObjectFieldArgs.CreateForProperty(base.Property, Rect.zero, allowSceneObjects: false);
				IPropertyValueEntry<ToggleableAsset> entry = base.ValueEntry;
				drawArgs.BaseType = entry.SmartValue.Object.GetType();
				drawArgs.Value = entry.SmartValue.Object;
				if (entry.SmartValue.AutoToggle)
				{
					drawArgs.Rect = EditorGUILayout.GetControlRect();
					OdinInternalEditorFields.UnityObjectField(in drawArgs);
					return;
				}
				Rect rect = GUILayoutUtility.GetRect(16f, 16f, GUILayoutOptions.ExpandWidth());
				Rect toggleRect = new Rect(rect.x, rect.y, 16f, 16f);
				Rect objectFieldRect = new Rect(rect.x + 20f, rect.y, rect.width - 20f, 16f);
				if (Event.current.type != EventType.Repaint)
				{
					toggleRect.x -= 5f;
					toggleRect.y -= 5f;
					toggleRect.width += 10f;
					toggleRect.height += 10f;
				}
				bool prevChanged = GUI.changed;
				entry.SmartValue.Toggled = GUI.Toggle(toggleRect, entry.SmartValue.Toggled, "");
				if (prevChanged != GUI.changed)
				{
					entry.ApplyChanges();
				}
				GUIHelper.PushGUIEnabled(entry.SmartValue.Toggled);
				drawArgs.Rect = objectFieldRect;
				drawArgs.ReadOnlyDontDisableGUI = true;
				OdinInternalEditorFields.UnityObjectField(in drawArgs);
				GUIHelper.PopGUIEnabled();
			}
		}

		private static readonly NamedValue[] customFilterMethodArgs = new NamedValue[1]
		{
			new NamedValue("asset", typeof(TElement))
		};

		private AssetList assetList;

		private PropertyTree propertyTree;

		private InspectorProperty listProperty;

		protected override void Initialize()
		{
			InspectorProperty property = base.Property;
			IPropertyValueEntry<TList> entry = base.ValueEntry;
			AssetListAttribute attribute = base.Attribute;
			assetList = new AssetList();
			assetList.AutoPopulate = attribute.AutoPopulate;
			assetList.AssetNamePrefix = attribute.AssetNamePrefix;
			assetList.Tags = ((attribute.Tags != null) ? (from i in attribute.Tags.Trim().Split(new char[1] { ',' })
				select i.Trim()).ToArray() : null);
			assetList.LayerNames = ((attribute.LayerNames != null) ? (from i in attribute.LayerNames.Trim().Split(new char[1] { ',' })
				select i.Trim()).ToArray() : null);
			assetList.List = entry;
			assetList.CollectionResolver = property.ChildResolver as IOrderedCollectionResolver;
			assetList.Property = entry.Property;
			if (attribute.Path != null)
			{
				string path = attribute.Path.TrimStart('/', ' ').TrimEnd('/', ' ');
				path = attribute.Path.Trim('/', ' ');
				path = "Assets/" + path + "/";
				path = Application.dataPath + "/" + path;
				assetList.AssetsFolderLocation = new DirectoryInfo(path);
				path = attribute.Path.Trim('/', ' ');
				assetList.PrettyPath = "/" + path.TrimStart(new char[1] { '/' });
			}
			if (attribute.CustomFilterMethod != null)
			{
				assetList.CustomFilterMethod = ValueResolver.Get<bool>(base.Property, attribute.CustomFilterMethod, customFilterMethodArgs);
			}
			if (Event.current != null)
			{
				assetList.MaxSearchDurationPrFrameInMS = 20.0;
				assetList.EnsureListPopulation();
			}
			assetList.MaxSearchDurationPrFrameInMS = 1.0;
			propertyTree = PropertyTree.Create(assetList);
			propertyTree.UpdateTree();
			listProperty = propertyTree.GetPropertyAtPath("toggleableAssets");
		}

		/// <summary>
		/// Draws the property.
		/// </summary>
		protected override void DrawPropertyLayout(GUIContent label)
		{
			InspectorProperty property = base.Property;
			IPropertyValueEntry<TList> entry = base.ValueEntry;
			AssetListAttribute attribute = base.Attribute;
			if (property.ValueEntry.WeakSmartValue != null)
			{
				propertyTree.GetRootProperty(0).Label = label;
				listProperty.State.Enabled = base.Property.State.Enabled;
				listProperty.State.Expanded = base.Property.State.Expanded;
				if (Event.current.type == EventType.Layout)
				{
					assetList.Property = entry.Property;
					assetList.EnsureListPopulation();
					assetList.SetToggleValues();
				}
				if (assetList.CustomFilterMethod != null && assetList.CustomFilterMethod.HasError)
				{
					assetList.CustomFilterMethod.DrawError();
				}
				assetList.Property = entry.Property;
				propertyTree.Draw(applyUndo: false);
				base.Property.State.Enabled = listProperty.State.Enabled;
				base.Property.State.Expanded = listProperty.State.Expanded;
				if (Event.current.type == EventType.Used)
				{
					assetList.UpdateList();
				}
			}
		}

		/// <summary>
		/// Populates the generic menu for the property.
		/// </summary>
		public void PopulateGenericMenu(InspectorProperty property, GenericMenu genericMenu)
		{
			if (assetList == null)
			{
				return;
			}
			if (assetList.List.SmartValue.Count != assetList.ToggleableAssets.Count)
			{
				genericMenu.AddItem(new GUIContent("Include All"), on: false, delegate
				{
					assetList.UpdateList(includeAll: true);
				});
			}
			else
			{
				genericMenu.AddDisabledItem(new GUIContent("Include All"));
			}
		}

		public void Dispose()
		{
			if (propertyTree != null)
			{
				propertyTree.Dispose();
				propertyTree = null;
			}
		}
	}
}
