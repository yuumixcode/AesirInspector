using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.OdinInspector.Editor.Internal;
using Sirenix.OdinInspector.Editor.Validation;
using Sirenix.OdinInspector.Editor.Validation.Internal;
using Sirenix.OdinInspector.Editor.Windows;
using Sirenix.Reflection.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using Sirenix.Utilities.Editor.Expressions;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Sirenix.OdinValidator.Editor
{
	public class ValidationSessionEditor : IDisposable
	{
		[HideReferenceObjectPicker]
		[HideLabel]
		internal class ValidationProfileEditor
		{
			[HideInInspector]
			public Action OnSaveChanges;

			[HideInInspector]
			public IList<IValidationProfile> DataSources;

			[HideInInspector]
			public EditorPrefInt SelectedConfigSource;

			private int selectedConfigSourceFallback;

			private bool isReadyToDropDraggedItems;

			private bool isDragging;

			private bool hasDragContent;

			private ValidationItem[] toInclude;

			private HashSet<ValidationItem> alreadyIncluded;

			private HashSet<ValidationItem> excludesToRemove;

			private Rect dropRect;

			private Dictionary<string, bool> assetPathExistLookup = new Dictionary<string, bool>();

			private int SelectedConfig
			{
				get
				{
					if (SelectedConfigSource == null)
					{
						return selectedConfigSourceFallback;
					}
					return SelectedConfigSource.Value;
				}
				set
				{
					if (SelectedConfigSource == null)
					{
						selectedConfigSourceFallback = value;
					}
					else
					{
						SelectedConfigSource.Value = value;
					}
				}
			}

			[PropertyOrder(-2f)]
			[OnInspectorGUI]
			public void Draw()
			{
				if (DataSources.Count == 0)
				{
					GUILayout.Label("No config is specified for this session.");
					return;
				}
				if (DataSources.Count > 1)
				{
					Rect toolbarRect = GUILayoutUtility.GetRect(0f, 21f);
					Rect generateReportButtonRect = toolbarRect.TakeFromRight(toolbarRect.height);
					toolbarRect.TakeFromRight(1f);
					if (DrawToolbarButton(generateReportButtonRect, SdfIconType.Receipt, "Asset filter breakdown. Generate and view a list of all assets that will be validated given the current profile."))
					{
						AssetFilterBreakdownEditor.ShowBreadownWindow(DataSources);
					}
					float width = toolbarRect.width / (float)DataSources.Count;
					for (int i = 0; i < DataSources.Count; i++)
					{
						Rect btnRect = toolbarRect.TakeFromLeft(width);
						if (i != DataSources.Count - 1)
						{
							EditorGUI.DrawRect(btnRect.TakeFromRight(1f), ValidatorGui.BorderColor);
						}
						IValidationProfile s = DataSources[i];
						int count = s.Include.Count + s.Exclude.Count;
						SdfIconType icon;
						string name;
						switch (s.Type)
						{
						case SessionConfigDataType.Persistent:
							icon = SdfIconType.PeopleFill;
							name = $"For everyone ({count})";
							break;
						case SessionConfigDataType.NonPersistent:
							icon = SdfIconType.DisplayFill;
							name = $"For this machine ({count})";
							break;
						case SessionConfigDataType.Custom:
							name = $"Config ({count})";
							icon = SdfIconType.Folder2;
							break;
						default:
							throw new NotImplementedException();
						}
						if (ValidatorGui.ToolbarBtn(btnRect, SelectedConfig == i, icon, name, null))
						{
							SelectedConfig = i;
						}
					}
				}
				else
				{
					SelectedConfig = 0;
				}
				if (SelectedConfig >= DataSources.Count)
				{
					SelectedConfig = 0;
				}
				IValidationProfile source = DataSources[SelectedConfig];
				EditorGUI.BeginChangeCheck();
				BeginDragAndDrop(source);
				DrawFilterAssetsGroup(source, "Scenes", EditorIcons.UnityLogo, ValidationItem.ValidationItemType.Scene, delegate(IList<ValidationItem> dst)
				{
					SelectScenePopup(dst.Add, null, includeAssetDeps: true);
				});
				DrawFilterAssetsGroup(source, "Assets", EditorIcons.UnityFolderIcon, ValidationItem.ValidationItemType.Asset, delegate(IList<ValidationItem> dst)
				{
					EditorWindow wnd = null;
					AssetSelector obj = new AssetSelector(delegate(AssetSelector x)
					{
						dst.Add(ValidationItem.FromAssetPath(x.Path, x.Filter));
						wnd.Close();
						source.SaveChanges();
						OnSaveChanges?.Invoke();
					});
					wnd = OdinEditorWindow.InspectObjectInDropDown(obj, 400f);
				});
				DrawFilterAssetsGroup(source, "Asset Bundles", EditorIcons.UnityLogo, ValidationItem.ValidationItemType.AssetBundle, delegate(IList<ValidationItem> dst)
				{
					string[] allAssetBundleNames = AssetDatabase.GetAllAssetBundleNames();
					List<string> list = (from x in dst
						where x.Type == ValidationItem.ValidationItemType.AssetBundle
						select x.AssetBundle).ToList();
					GenericSelector<string> genericSelector = new GenericSelector<string>(list.Concat(allAssetBundleNames).Distinct());
					genericSelector.SetSelection(list);
					genericSelector.SelectionConfirmed += delegate(IEnumerable<string> newSelection)
					{
						List<ValidationItem> collection = newSelection.Select(delegate(string bundle)
						{
							foreach (ValidationItem current in dst)
							{
								if (current.Type == ValidationItem.ValidationItemType.AssetBundle && current.AssetBundle == bundle)
								{
									return current;
								}
							}
							return ValidationItem.FromAssetBundle(bundle);
						}).ToList();
						for (int num = dst.Count - 1; num >= 0; num--)
						{
							if (dst[num].Type == ValidationItem.ValidationItemType.AssetBundle)
							{
								dst.RemoveAt(num);
							}
						}
						dst.AddRange(collection);
						source.SaveChanges();
						OnSaveChanges?.Invoke();
					};
					genericSelector.ShowInPopup(400f);
				});
				GUIHelper.PushGUIEnabled(AddressablesUtility.AddressablesAvailable && GUI.enabled);
				DrawFilterAssetsGroup(source, "Addressable Groups", EditorIcons.UnityLogo, ValidationItem.ValidationItemType.AddressableGroup, delegate(IList<ValidationItem> dst)
				{
					List<string> addressableGroupNames = AddressablesUtility.GetAddressableGroupNames();
					List<string> list = (from x in dst
						where x.Type == ValidationItem.ValidationItemType.AddressableGroup
						select x.AddressableGroup).ToList();
					GenericSelector<string> genericSelector = new GenericSelector<string>(list.Concat(addressableGroupNames).Distinct());
					genericSelector.SetSelection(list);
					genericSelector.SelectionConfirmed += delegate(IEnumerable<string> newSelection)
					{
						List<ValidationItem> collection = newSelection.Select(delegate(string group)
						{
							foreach (ValidationItem current in dst)
							{
								if (current.Type == ValidationItem.ValidationItemType.AddressableGroup && current.AddressableGroup == group)
								{
									return current;
								}
							}
							return ValidationItem.FromAddressableGroup(group);
						}).ToList();
						for (int num = dst.Count - 1; num >= 0; num--)
						{
							if (dst[num].Type == ValidationItem.ValidationItemType.AddressableGroup)
							{
								dst.RemoveAt(num);
							}
						}
						dst.AddRange(collection);
						source.SaveChanges();
						OnSaveChanges?.Invoke();
					};
					genericSelector.ShowInPopup(400f);
				});
				GUIHelper.PopGUIEnabled();
				if (EditorGUI.EndChangeCheck())
				{
					source.SaveChanges();
					OnSaveChanges?.Invoke();
				}
			}

			private void BeginDragAndDrop(IValidationProfile source)
			{
				if (DragAndDropUtilities.IsDragging && !isDragging)
				{
					IEnumerable<UnityEngine.Object> dragging = DragAndDrop.objectReferences.Where((UnityEngine.Object x) => (bool)x && AssetDatabase.Contains(x));
					ValidationItem[] items = ValidationItem.FromSelection(dragging, includeSceneDependencies: true);
					excludesToRemove = new HashSet<ValidationItem>(items.Where((ValidationItem x) => source.Exclude.Contains(x)).ToArray());
					toInclude = items.Except(source.Include).ToArray();
					alreadyIncluded = new HashSet<ValidationItem>(items.Where((ValidationItem x) => source.Include.Contains(x)).ToArray());
					hasDragContent = toInclude.Length != 0 || excludesToRemove.Count > 0 || alreadyIncluded.Count > 0;
				}
				else if (!DragAndDropUtilities.IsDragging && hasDragContent)
				{
					hasDragContent = false;
					excludesToRemove = null;
					toInclude = null;
					alreadyIncluded = null;
				}
				isDragging = DragAndDropUtilities.IsDragging;
				dropRect = GUIHelper.GetCurrentLayoutRect();
				isReadyToDropDraggedItems = false;
				if (!hasDragContent)
				{
					return;
				}
				GUIHelper.RequestRepaint();
				int dragId = DragAndDropUtilities.GetDragAndDropId(dropRect);
				object result = DragAndDropUtilities.DropZone(dropRect, null, typeof(UnityEngine.Object), dragId);
				if (result != null)
				{
					ValidationItem[] array = toInclude;
					foreach (ValidationItem item in array)
					{
						source.Include.Add(item);
					}
					foreach (ValidationItem item2 in excludesToRemove)
					{
						source.Exclude.Remove(item2);
					}
					hasDragContent = false;
					excludesToRemove = null;
					toInclude = null;
					alreadyIncluded = null;
					source.SaveChanges();
					OnSaveChanges?.Invoke();
				}
				ValidationItem[] array2 = toInclude;
				isReadyToDropDraggedItems = array2 != null && array2.Length != 0 && dragId == DragAndDropUtilities.HoveringAcceptedDropZone;
			}

			private void DrawFilterAssetsGroup(IValidationProfile source, string headerText, Texture2D headerIcon, ValidationItem.ValidationItemType vType, Action<IList<ValidationItem>> select)
			{
				int includeCount = 0;
				int excludeCount = 0;
				foreach (ValidationItem item2 in source.Include)
				{
					if (item2.Type == vType)
					{
						includeCount++;
					}
				}
				foreach (ValidationItem item3 in source.Exclude)
				{
					if (item3.Type == vType)
					{
						excludeCount++;
					}
				}
				Rect rect = GUILayoutUtility.GetRect(0f, 21f);
				Rect addRect = rect.TakeFromRight(rect.height);
				rect.TakeFromRight(1f);
				Rect exRect = rect.TakeFromRight(rect.height);
				rect.TakeFromRight(1f);
				ValidatorGui.Header(rect, headerText, headerIcon);
				if (DrawToolbarButton(addRect, SdfIconType.Plus, "Add include"))
				{
					select(source.Include);
				}
				if (DrawToolbarButton(exRect, SdfIconType.SlashCircle, "Add exclude"))
				{
					select(source.Exclude);
				}
				if (includeCount > 0 && excludeCount > 0)
				{
					GUI.Label(GUILayoutUtility.GetRect(0f, 21f).HorizontalPadding(3f), "Included", ValidatorGui.LabelLowerCenterBold);
				}
				DrawValidationItems(source.Include, vType);
				if (isReadyToDropDraggedItems)
				{
					GUIHelper.PushGUIEnabled(enabled: false);
					DrawValidationItems(toInclude, vType);
					GUIHelper.PopGUIEnabled();
				}
				if (excludeCount > 0)
				{
					GUI.Label(GUILayoutUtility.GetRect(0f, 21f).HorizontalPadding(3f), "Excluded", ValidatorGui.LabelLowerCenterBold);
					DrawValidationItems(source.Exclude, vType);
				}
			}

			private static Texture GetValidationItemIcon(ref ValidationItem item)
			{
				if (item.Type == ValidationItem.ValidationItemType.Asset)
				{
					if (item.Asset.IsFile)
					{
						UnityEngine.Object obj = AssetDatabase.LoadMainAssetAtPath(item.Asset.Path);
						return GUIHelper.GetAssetThumbnail(obj, null, preferObjectPreviewOverFileIcon: false);
					}
					return EditorIcons.UnityFolderIcon;
				}
				if (item.Type == ValidationItem.ValidationItemType.AddressableGroup)
				{
					return GUIHelper.GetAssetThumbnail(null, typeof(MonoBehaviour), preferObjectPreviewOverFileIcon: false);
				}
				if (item.Type == ValidationItem.ValidationItemType.AssetBundle)
				{
					return GUIHelper.GetAssetThumbnail(null, typeof(UnityEngine.Object), preferObjectPreviewOverFileIcon: false);
				}
				if (item.Type == ValidationItem.ValidationItemType.Scene)
				{
					return GUIHelper.GetAssetThumbnail(null, typeof(SceneAsset), preferObjectPreviewOverFileIcon: false);
				}
				if (item.Type == ValidationItem.ValidationItemType.Object)
				{
					return GUIHelper.GetAssetThumbnail(item.Object, null, preferObjectPreviewOverFileIcon: false);
				}
				throw new NotImplementedException(item.Type.ToString());
			}

			private bool ValidateItemIsValid(in ValidationItem item, out string errorMessage)
			{
				errorMessage = null;
				if (item.Type == ValidationItem.ValidationItemType.Asset)
				{
					if (item.Asset.Path == null)
					{
						errorMessage = "Path is null.";
						return false;
					}
					if (!assetPathExistLookup.TryGetValue(item.Asset.Path, out var pathExists))
					{
						pathExists = Directory.Exists(item.Asset.Path) || File.Exists(item.Asset.Path);
						assetPathExistLookup[item.Asset.Path] = pathExists;
					}
					if (!pathExists)
					{
						errorMessage = "Path does not exist.";
						return false;
					}
					return true;
				}
				if (item.Type == ValidationItem.ValidationItemType.Scene && item.Scene.Type == ValidationItem.SceneIncludeType.OpenScenes && !Application.isPlaying)
				{
					int loadedSceneCount = SceneUtilities.GetLoadedSceneCount();
					int loadedSubSceneCount = 0;
					SceneSetup[] sceneManagerSetup = EditorSceneManager.GetSceneManagerSetup();
					foreach (SceneSetup sceneSetup in sceneManagerSetup)
					{
						if (sceneSetup.isSubScene)
						{
							loadedSubSceneCount++;
						}
					}
					int loadedRootSceneCount = loadedSceneCount - loadedSubSceneCount;
					if (loadedRootSceneCount > EditorSceneManager.GetSceneManagerSetup().Length)
					{
						errorMessage = "Unsaved open scenes are not validated.";
						return false;
					}
				}
				return true;
			}

			private void DrawValidationItems(IList<ValidationItem> items, ValidationItem.ValidationItemType type)
			{
				for (int i = 0; i < items.Count; i++)
				{
					ValidationItem item = items[i];
					if (item.Type != type)
					{
						continue;
					}
					string errorMessage;
					bool isValid = ValidateItemIsValid(in item, out errorMessage);
					bool isAboutToBeRemoved = isReadyToDropDraggedItems && excludesToRemove.Contains(item) && items != toInclude;
					Rect rect = GUILayoutUtility.GetRect(0f, 21f);
					Rect contentRect = rect.HorizontalPadding(ValidatorGui.ContentPadding, 0f).AlignCenterY(EditorGUIUtility.singleLineHeight);
					Rect toggleRect = contentRect.TakeFromLeft(contentRect.height);
					Rect iconRect = contentRect.TakeFromLeft(contentRect.height).AlignCenter(16f, 16f);
					string label = null;
					string suffix = null;
					GUIStyle suffixLabelStyle = SirenixGUIStyles.RightAlignedGreyMiniLabel;
					if (item.Type != ValidationItem.ValidationItemType.Scene || (item.Scene.Type != ValidationItem.SceneIncludeType.OpenScenes && item.Scene.Type != ValidationItem.SceneIncludeType.ScenesInBuildOptions))
					{
						Rect deleteRect = contentRect.TakeFromRight(rect.height).AlignCenterY(rect.height - ValidatorGui.ContentPadding);
						contentRect.TakeFromRight(1f);
						Rect editRect = contentRect.TakeFromRight(rect.height).AlignCenterY(rect.height - ValidatorGui.ContentPadding);
						if (ValidatorGui.IconButton(deleteRect, SdfIconType.X, null))
						{
							items.RemoveAt(i);
							i--;
							break;
						}
						if (ValidatorGui.IconButton(editRect, SdfIconType.PencilFill, null))
						{
							if (item.Type == ValidationItem.ValidationItemType.Asset)
							{
								EditorWindow wnd = null;
								int toReplace = i;
								AssetSelector selector = new AssetSelector(delegate(AssetSelector x)
								{
									items[toReplace] = ValidationItem.FromAssetPath(x.Path, x.Filter);
									wnd.Close();
								});
								selector.Path = item.Asset.Path;
								selector.Filter = item.Asset.Filter;
								wnd = OdinEditorWindow.InspectObjectInDropDown(selector, editRect, 400f);
							}
							else if (item.Type == ValidationItem.ValidationItemType.Scene)
							{
								int toReplace2 = i;
								SelectScenePopup(delegate(ValidationItem x)
								{
									items[toReplace2] = x;
								}, items[toReplace2].Scene.Value, items[toReplace2].Scene.IncludeAssetDependencies);
							}
						}
					}
					if (item.Type == ValidationItem.ValidationItemType.Asset)
					{
						label = item.Asset.Path;
						suffix = item.Asset.Filter;
						if (item.Asset.IsDirectory)
						{
							label += "/*";
						}
					}
					else if (item.Type == ValidationItem.ValidationItemType.AddressableGroup)
					{
						label = item.AddressableGroup;
					}
					else if (item.Type == ValidationItem.ValidationItemType.AssetBundle)
					{
						label = item.AssetBundle;
					}
					else if (item.Type == ValidationItem.ValidationItemType.Scene)
					{
						if (item.Scene.Type == ValidationItem.SceneIncludeType.OpenScenes)
						{
							label = "Open Scenes";
						}
						else if (item.Scene.Type == ValidationItem.SceneIncludeType.SceneGuid)
						{
							label = new SceneReference(item.Scene.Value).Name + ".unity";
						}
						else if (item.Scene.Type == ValidationItem.SceneIncludeType.ScenesInBuildOptions)
						{
							label = "Scenes in build options";
						}
						else
						{
							if (item.Scene.Type != ValidationItem.SceneIncludeType.ScenesInFolder)
							{
								throw new NotImplementedException(item.Scene.Type.ToString());
							}
							label = item.Scene.Value + "/*.unity";
						}
						contentRect.TakeFromRight(1f);
						Rect btnRect = contentRect.TakeFromRight(rect.height).AlignCenterY(rect.height);
						string tooltip = (item.Scene.IncludeAssetDependencies ? "Exclude asset dependencies" : "Include asset dependencies");
						bool isMouseOver = btnRect.Contains(Event.current.mousePosition);
						Texture dIcon = (item.Scene.IncludeAssetDependencies ? folderOn : folderOff);
						SdfIconType sdfIcon = (item.Scene.IncludeAssetDependencies ? SdfIconType.Diagram3Fill : SdfIconType.Diagram3);
						Color col = (item.Scene.IncludeAssetDependencies ? new Color(1f, 1f, 1f, isMouseOver ? 1f : 0.7f) : new Color(1f, 1f, 1f, isMouseOver ? 1f : 0.3f));
						SdfIcons.DrawIcon(btnRect.AlignCenterXY(16f, 16f), sdfIcon, col, ValidatorGui.EditorWindowBgColor);
						if (GUI.Button(btnRect, GUIHelper.TempContent("", tooltip), GUIStyle.none))
						{
							item.Scene.IncludeAssetDependencies = !item.Scene.IncludeAssetDependencies;
							GUI.changed = true;
						}
					}
					else
					{
						if (item.Type != ValidationItem.ValidationItemType.Object)
						{
							throw new NotImplementedException(item.Type.ToString());
						}
						label = item.Object.ToString();
					}
					bool changeColor = isAboutToBeRemoved;
					if (changeColor)
					{
						GUI.color = Color.red;
					}
					if (suffix != null)
					{
						float width = suffixLabelStyle.CalcSize(new GUIContent(suffix)).x;
						contentRect.TakeFromRight(5f);
						Rect suffixRect = contentRect.TakeFromRight(width + 5f);
						GUI.Label(suffixRect, suffix, suffixLabelStyle);
						contentRect.xMax = suffixRect.xMin;
					}
					if (isValid)
					{
						GUI.Label(contentRect, label);
					}
					else
					{
						GUI.DrawTexture(contentRect.TakeFromLeft(contentRect.height).AlignCenterXY(16f), EditorIcons.UnityErrorIcon);
						GUI.Label(contentRect, GUIHelper.TempContent(label, null, errorMessage));
					}
					if (changeColor)
					{
						GUI.color = Color.white;
					}
					GUI.DrawTexture(iconRect, GetValidationItemIcon(ref item));
					item.Enabled = EditorGUI.Toggle(toggleRect.SetXMax(contentRect.xMax), item.Enabled);
					items[i] = item;
				}
			}
		}

		public class RuleDataDrawer
		{
			[HideLabel]
			[ShowInInspector]
			[HideReferenceObjectPicker]
			public IValidator Data;

			[HideInInspector]
			public CombinedRuleInstance Rule;

			private GUIStyle ContentPadding = new GUIStyle
			{
				padding = new RectOffset(10, 10, 10, 10)
			};

			private ConfigSourceType configSourceType;

			private Vector2 scrollPosition = Vector2.zero;

			public EditorPrefEnum<ConfigSourceType> SelectedRuleSrc => new EditorPrefEnum<ConfigSourceType>("Odin_Validator_Editor_selectedRuleSrc", ConfigSourceType.Project);

			public RuleDataDrawer(CombinedRuleInstance rule, ConfigSourceType configSourceType)
			{
				Rule = rule;
				this.configSourceType = configSourceType;
				Data = rule.GetOverrideDataOrDefaultCopy(configSourceType);
			}

			public void SaveChanges()
			{
				Rule.SetOverrideData(configSourceType, Data);
				GlobalConfig<RuleConfig>.Instance.GetRuleDataWrapper().SaveChanges();
			}

			[PropertyOrder(-9f)]
			[OnInspectorGUI]
			public void BeginScrollView()
			{
				scrollPosition = GUILayout.BeginScrollView(scrollPosition);
			}

			[PropertyOrder(1000000f)]
			[OnInspectorGUI]
			public void EndScrollView()
			{
				GUILayout.EndScrollView();
			}

			[PropertyOrder(-10f)]
			[OnInspectorGUI]
			private void DrawHeader(InspectorProperty prop)
			{
				bool hasProperties = prop.Tree.RootProperty.Children["Data"].Children.Count > 0;
				ConfigSourceType selectedRuleSrc = SelectedRuleSrc.Value;
				bool hasOverridenData = Rule.IsDataOverriddenIn(selectedRuleSrc);
				bool hasDescription = Rule.Description != null;
				Rect headerRect = GUILayoutUtility.GetRect(0f, 21f);
				EditorGUI.DrawRect(headerRect, ValidatorGui.ToolbarBgColor);
				if (!hasDescription)
				{
					EditorGUI.DrawRect(headerRect.AlignBottom(1f), ValidatorGui.BorderColor);
				}
				EditorGUI.DrawRect(headerRect.AlignTop(1f).AddY(-1f), ValidatorGui.BorderColor);
				GUI.Label(headerRect.AlignBottom(21f).AlignCenterY(EditorGUIUtility.singleLineHeight).HorizontalPadding(ValidatorGui.ContentPadding), Rule.Name, SirenixGUIStyles.WhiteLabelCentered);
				if (Rule.Description != null)
				{
					Rect rect = EditorGUILayout.BeginVertical(ContentPadding);
					EditorGUI.DrawRect(rect, ValidatorGui.ToolbarBgColor);
					EditorGUI.DrawRect(rect.AlignBottom(1f), ValidatorGui.BorderColor);
					GUILayout.Label(Rule.Description, SirenixGUIStyles.MultiLineLabel);
					EditorGUILayout.EndVertical();
				}
				if (hasProperties)
				{
					Rect buttonsRect = GUILayoutUtility.GetRect(0f, 21f);
					Rect resetButtonRect = buttonsRect.Split(0, 2);
					Rect saveButtonRect = buttonsRect.Split(1, 2);
					saveButtonRect.xMax++;
					Color tmp1 = ValidatorGui.BtnContentColor;
					Color tmp2 = ValidatorGui.BtnMouseOverContentColor;
					Color tmp3 = ValidatorGui.LabelVerticalCentered.normal.textColor;
					string saveTooltip = ((selectedRuleSrc == ConfigSourceType.Local) ? "Save changes for this machine" : "Save changes for everyone");
					string resetTooltip = ((selectedRuleSrc == ConfigSourceType.Local) ? "Reset config for this machine" : "Reset config for everyone");
					resetTooltip += (hasOverridenData ? "" : "\n\nNo changes to reset");
					ValidatorGui.BtnContentColor = Color.green;
					ValidatorGui.BtnMouseOverContentColor = Color.green;
					ValidatorGui.LabelVerticalCentered.normal.textColor = new Color(1f, 1f, 1f, 1f);
					bool clickedSave = ValidatorGui.ToolbarBtn(saveButtonRect, on: false, SdfIconType.Check, "Save changes", saveTooltip);
					EditorGUI.DrawRect(saveButtonRect.AlignLeft(1f), ValidatorGui.BorderColor);
					ValidatorGui.BtnContentColor = new Color(1f, 0.3f, 0.3f);
					ValidatorGui.BtnMouseOverContentColor = new Color(1f, 0.3f, 0.3f);
					GUIHelper.PushGUIEnabled(hasOverridenData);
					bool clickedReset = ValidatorGui.ToolbarBtn(resetButtonRect, on: false, SdfIconType.ArrowCounterclockwise, "Reset", resetTooltip);
					GUIHelper.PopGUIEnabled();
					ValidatorGui.BtnContentColor = tmp1;
					ValidatorGui.BtnMouseOverContentColor = tmp2;
					ValidatorGui.LabelVerticalCentered.normal.textColor = tmp3;
					if (clickedSave)
					{
						SaveChanges();
					}
					if (clickedReset)
					{
						SerializedRule serializedRule = Rule.GetSerializedRule(SelectedRuleSrc.Value);
						if (serializedRule != null)
						{
							serializedRule.DataOverride = null;
						}
						Data = Rule.GetOverrideDataOrDefaultCopy(configSourceType);
					}
					EditorGUI.DrawRect(buttonsRect.AlignBottom(1f), ValidatorGui.BorderColor);
				}
				GUILayout.Space(2f);
				if (!hasProperties)
				{
					Rect r = EditorGUILayout.BeginVertical();
					GUILayout.FlexibleSpace();
					EditorGUILayout.EndVertical();
					GUIHelper.PushGUIEnabled(enabled: false);
					GUI.Label(r, "This rule has no configurable properties.", SirenixGUIStyles.MultiLineCenteredLabel);
					GUIHelper.PopGUIEnabled();
				}
			}
		}

		private class AssetSelector
		{
			private Action<AssetSelector> confirm;

			[HorizontalGroup(0f, 0, 0, 0f)]
			[ValidateInput("IsValidPath", null, InfoMessageType.Error)]
			[PlaceholderLabel("* Path (file or folder)", SdfIconType.FolderFill)]
			public string Path;

			[PlaceholderLabel("Filter (t: ExampleType)", SdfIconType.Search)]
			[HorizontalGroup(150f, 0, 0, 0f)]
			[DisableIf("PathIsFile")]
			public string Filter;

			[PropertyOrder(-10f)]
			[OnInspectorGUI]
			private void DrawHeader()
			{
				Rect rect = GUILayoutUtility.GetRect(0f, 19f);
				if (Event.current.type == EventType.Repaint)
				{
					Rect outerRect = GUIHelper.GetCurrentLayoutRect();
					rect.yMin = outerRect.y;
					rect.xMin = outerRect.x;
					rect.width = outerRect.width;
					EditorStyles.toolbar.Draw(rect, GUIContent.none, 0);
					SirenixGUIStyles.BoldLabelCentered.Draw(rect, new GUIContent("Select a folder or a file path"), 0);
				}
			}

			public AssetSelector(Action<AssetSelector> confirm)
			{
				this.confirm = confirm;
			}

			private bool IsValidPath()
			{
				if (string.IsNullOrEmpty(Path))
				{
					return true;
				}
				if (Directory.Exists(Path))
				{
					return true;
				}
				if (File.Exists(Path))
				{
					return true;
				}
				return false;
			}

			private string[] GetAllAssets()
			{
				return AssetDatabase.GetAllAssetPaths();
			}

			private bool PathIsFile()
			{
				if (string.IsNullOrEmpty(Path))
				{
					return false;
				}
				return File.Exists(Path);
			}

			[ButtonGroup("_DefaultGroup", 0f)]
			[Button(ButtonSizes.Medium)]
			[DisableIf("@string.IsNullOrEmpty(this.Path) || !IsValidPath()")]
			public void Ok()
			{
				confirm(this);
			}
		}

		private class Timings
		{
			private Dictionary<Type, AverageSampler> sampleBuckets = new Dictionary<Type, AverageSampler>();

			public List<(Type type, AverageSampler sampler)> Samples = new List<(Type, AverageSampler)>();

			public void Reset()
			{
				sampleBuckets.Clear();
				Samples.Clear();
			}

			public void SampleResult(PersistentValidationResultBatch result)
			{
				if (result == null)
				{
					return;
				}
				Type validatorType = result.ValidatorType;
				if (validatorType == null)
				{
					validatorType = typeof(NoValidator);
				}
				if (!sampleBuckets.TryGetValue(validatorType, out var sampler))
				{
					sampler = (sampleBuckets[validatorType] = new AverageSampler());
					Samples.Add((validatorType, sampler));
					if (!GlobalConfig<GlobalValidationConfig>.Instance.SkipFirstSample)
					{
						sampler.Add(result.ValidationTimeMS);
					}
				}
				else
				{
					sampler.Add(result.ValidationTimeMS);
				}
			}
		}

		public enum MenuOptions
		{
			FilterResults,
			FilterAssets,
			Rules,
			Events,
			Config,
			Profiler,
			BulkFixing,
			About,
			None
		}

		private class SceneSelector : GenericSelector<string>
		{
			[HideInInspector]
			public bool IncludeAssetDependencies;

			public SceneSelector(IEnumerable<string> collection, bool includeAssetDependencies)
				: base((string)null, collection, supportsMultiSelect: false, (Func<string, string>)null)
			{
				IncludeAssetDependencies = includeAssetDependencies;
			}

			protected override void DrawSelectionTree()
			{
				base.SelectionTree.DrawSearchToolbar();
				IncludeAssetDependencies = EditorGUILayout.ToggleLeft("Include asset dependencies", IncludeAssetDependencies);
				base.DrawSelectionTree();
			}
		}

		[Serializable]
		private class OnGUIDropDown
		{
			[HideInInspector]
			public Action OnGUI;

			[HideInInspector]
			public string Title;

			public OnGUIDropDown(string title, Action action)
			{
				OnGUI = action;
				Title = title;
			}

			[PropertyOrder(-10f)]
			[OnInspectorGUI]
			private void DrawHeader()
			{
				Rect rect = GUILayoutUtility.GetRect(0f, 19f);
				if (Event.current.type == EventType.Repaint)
				{
					Rect outerRect = GUIHelper.GetCurrentLayoutRect();
					rect.yMin = outerRect.y;
					rect.xMin = outerRect.x;
					rect.width = outerRect.width;
					EditorStyles.toolbar.Draw(rect, GUIContent.none, 0);
					SirenixGUIStyles.BoldLabelCentered.Draw(rect, new GUIContent("Select a folder or a file path"), 0);
				}
			}

			[OnInspectorGUI]
			private void OnInspectorGUI()
			{
				OnGUI();
			}

			[Button(ButtonSizes.Medium)]
			[PropertyOrder(10f)]
			public void Ok()
			{
				EditorWindow window = GUIHelper.CurrentWindow;
				EditorApplication.delayCall = (EditorApplication.CallbackFunction)Delegate.Combine(EditorApplication.delayCall, (EditorApplication.CallbackFunction)delegate
				{
					if ((bool)window)
					{
						window.Close();
					}
				});
			}
		}

		private static GUIStyle selectableLabelStyle;

		private static GUIStyle padding;

		private static GUIStyle labelStyle;

		private static ExpressionFunc<EditorWindow, bool> windowIsVisible;

		private static Texture2D validatorLogo;

		private static Texture folderOff;

		private static Texture2D folderOn;

		public static List<ValidationSessionEditor> ActiveEditors = new List<ValidationSessionEditor>();

		private static double prevRepaintTime;

		private bool normalize;

		private Vector2 bulkFixMetaDataScrollPos;

		private Vector2 metaDataScrollPos;

		private Vector2 resultsScrollPos;

		private Vector2 infoScrollPos;

		private Vector2 bulkFixScrollPos;

		private RuleDataWrapper rules;

		private string showTip;

		private PersistentValidationResult prevSelectedResult;

		private PropertyTree currentResultTree;

		private ResultItem? currentSelectedRestoredHighestSeverityResultItem;

		private PropertyTree metaDataTree;

		private PropertyTree issueFixerTree;

		private Fix currentFix;

		private Timings timings = new Timings();

		private Action delayedAction;

		private int offset;

		private SearchField searchField = new SearchField();

		private OdinValidatorWindow window;

		private LargeGuiCollectionHelper largeGuiCollectionDrawing;

		private readonly FlagEnumColumnDrawer<ResultListColumnFilter> columnDrawer;

		private static EditorPrefBool expandEventsOnPlay = new EditorPrefBool("Odin_Validator_Editor_expandEventsOnPlay", defaultValue: false);

		private static EditorPrefBool expandEventsOnBuild = new EditorPrefBool("Odin_Validator_Editor_expandEventsOnBuild", defaultValue: false);

		private static EditorPrefBool expandEventsOnProjectStartup = new EditorPrefBool("Odin_Validator_Editor_expandEventsOnProjectStartup", defaultValue: false);

		private EditorPrefBool hideFilterTips = new EditorPrefBool("Odin_Validator_Editor_hideFilterTips", defaultValue: false);

		private EditorPrefFloat leftMenuWidth = new EditorPrefFloat("Odin_Validator_Editor_leftMenuWidth", 190f);

		private EditorPrefFloat rightMenuWidth = new EditorPrefFloat("Odin_Validator_Editor_rightMenuWidth", 190f);

		private EditorPrefFloat resultInfoHeight = new EditorPrefFloat("Odin_Validator_Editor_resultInfoHeight", 90f);

		private EditorPrefBool filterToggleFixTypes = new EditorPrefBool("Odin_Validator_Editor_filterToggleFixTypes", defaultValue: false);

		private EditorPrefBool filterToggleScenes = new EditorPrefBool("Odin_Validator_Editor_filterToggleScenes", defaultValue: false);

		private EditorPrefBool filterToggleObjectTypes = new EditorPrefBool("Odin_Validator_Editor_filterToggleObjectTypes", defaultValue: false);

		private EditorPrefBool filterToggleValidatorTypes = new EditorPrefBool("Odin_Validator_Editor_filterToggleValidatorTypes", defaultValue: false);

		private static EditorPrefBool expandGeneralSettings = new EditorPrefBool("Odin_Validator_Editor_expandGeneralSettings", defaultValue: true);

		private static EditorPrefBool expandAdvancedSettings = new EditorPrefBool("Odin_Validator_Editor_expandAdvancedSettings", defaultValue: false);

		private static EditorPrefBool expandWidgetSettings = new EditorPrefBool("Odin_Validator_Editor_expandWidgetSettings", defaultValue: false);

		private static EditorPrefBool expandWindowSettings = new EditorPrefBool("Odin_Validator_Editor_expandWindowSettings", defaultValue: false);

		private EditorPrefBool expandProfilerTickTime = new EditorPrefBool("Odin_Validator_Editor_expandProfilerTickTime", defaultValue: false);

		private EditorPrefBool expandProfilerBreakdown = new EditorPrefBool("Odin_Validator_Editor_expandProfilerBreakdown", defaultValue: false);

		private EditorPrefBool expandProfilerCurrentSession = new EditorPrefBool("Odin_Validator_Editor_expandProfilerCurrentSession", defaultValue: false);

		private EditorPrefBool expandProfilerTimings = new EditorPrefBool("Odin_Validator_Editor_expandProfilerTimings", defaultValue: false);

		private EditorPrefBool expandProfilerAssetLoadTimings = new EditorPrefBool("Odin_Validator_Editor_expandProfilerAssetLoadTimings", defaultValue: false);

		private EditorPrefBool expandProfilerEvents = new EditorPrefBool("Odin_Validator_Editor_expandProfilerEvents", defaultValue: false);

		private EditorPrefEnum<MenuOptions> selectedOption = new EditorPrefEnum<MenuOptions>("Odin_Validator_Editor_selectedOption", MenuOptions.FilterResults);

		private EditorPrefBool selectedOptionIsVisisble = new EditorPrefBool("Odin_Validator_Editor_selectedOptionIsVisisble", defaultValue: false);

		private static EditorPrefBool showConfigLocalOverride = new EditorPrefBool("Odin_Validator_Editor_showConfigLocalOverride", defaultValue: false);

		private EditorPrefFloat[] scrollPositions = EnumTypeUtilities<MenuOptions>.VisibleEnumMemberInfos.Select((EnumTypeUtilities<MenuOptions>.EnumMember x) => new EditorPrefFloat("Odin_Validator_Editor_scrollPositions" + x.Value, 0f)).ToArray();

		private EditorPrefEnum<ConfigSourceType> selectedRuleSrc = new EditorPrefEnum<ConfigSourceType>("Odin_Validator_Editor_selectedRuleSrc", ConfigSourceType.Project);

		private EditorPrefEnum<ConfigSourceType> selectedExcludeSrc = new EditorPrefEnum<ConfigSourceType>("Odin_Validator_Editor_selectedExcludeSrc", ConfigSourceType.Project);

		private PersistentValidationResult selectionToRecover;

		private ValidationProfileEditor sessionConfigDataDrawer;

		private int nextScrollTo = -1;

		public int SelectedIndex;

		private int selectedRuleIndex = -1;

		internal PropertyTree selectedRuleDataDrawer;

		private int profilerAssetLoadTimingsCount;

		[Obsolete("", false)]
		public bool IsMainWindow;

		[Obsolete("", false)]
		public readonly bool IsControllingMainSession;

		private Vector2 scrollPoisition = Vector2.zero;

		private static GUIStyle Padding
		{
			get
			{
				padding = padding ?? new GUIStyle
				{
					padding = new RectOffset(10, 10, 10, 10)
				};
				return padding;
			}
		}

		public ValidationSession ValidationSession { get; private set; }

		public bool MenuVisibility
		{
			get
			{
				return selectedOptionIsVisisble.Value;
			}
			set
			{
				if (value != selectedOptionIsVisisble.Value)
				{
					selectionToRecover = selectionToRecover ?? SelectedResult;
					selectedOptionIsVisisble.Value = value;
				}
			}
		}

		public MenuOptions SelectedMenu
		{
			get
			{
				return selectedOption.Value;
			}
			set
			{
				if (value != selectedOption.Value)
				{
					selectionToRecover = selectionToRecover ?? SelectedResult;
					selectedOption.Value = value;
				}
			}
		}

		public OdinValidatorWindow Window => window;

		public PersistentValidationResult SelectedResult => SelectedItem.Result;

		private ValidationSessionResultCollector.ResultItemSingle SelectedItem
		{
			get
			{
				if (SelectedIndex < 0)
				{
					SelectIndexAndRemoveFocusControlAndExitGUI(0);
				}
				else if (SelectedIndex >= ValidationSession.Results.Length)
				{
					SelectIndexAndRemoveFocusControlAndExitGUI(ValidationSession.Results.Length - 1);
				}
				if (ValidationSession.Results.Length > 0)
				{
					return ValidationSession.Results[SelectedIndex];
				}
				return default(ValidationSessionResultCollector.ResultItemSingle);
			}
		}

		[InitializeOnLoadMethod]
		private static void Init()
		{
			new Thread((ThreadStart)delegate
			{
				string errorMessage;
				Delegate obj = ExpressionUtility.ParseExpression("this.m_Parent && this.m_Parent.actualView == this", new EmitContext
				{
					IsStatic = false,
					Type = typeof(EditorWindow)
				}, out errorMessage);
				if (errorMessage != null)
				{
					throw new Exception(errorMessage);
				}
				windowIsVisible = (ExpressionFunc<EditorWindow, bool>)obj;
			}).Start();
			EditorApplication.update = (EditorApplication.CallbackFunction)Delegate.Combine(EditorApplication.update, new EditorApplication.CallbackFunction(ProcessVisibleEditors));
			UnityEditorEventUtility.DuringSceneGUI += UnityEditorEventUtility_DuringSceneGUI;
		}

		private static void ProcessVisibleEditors()
		{
			if (windowIsVisible == null)
			{
				return;
			}
			bool repaintValidatingWindows = prevRepaintTime + 0.2 < EditorApplication.timeSinceStartup;
			if (repaintValidatingWindows)
			{
				prevRepaintTime = EditorApplication.timeSinceStartup;
			}
			foreach (ValidationSessionEditor editor in ActiveEditors)
			{
				if (!editor.ValidationSession.IsValidatingInBackground || !windowIsVisible(editor.Window))
				{
					continue;
				}
				if (repaintValidatingWindows && editor.ValidationSession.ShouldDisplayProgressBar)
				{
					editor.window.Repaint();
				}
				if (editor.ValidationSession.WorkQueue.Count != 0 || editor.ValidationSession.currentlyProcessingWorkItem.HasValue || !GlobalConfig<GlobalValidationConfig>.Instance.ContinuouslyValidateVisibleIssues.Value)
				{
					continue;
				}
				int idx = editor.largeGuiCollectionDrawing.StartIndex + editor.offset;
				if (idx >= 0 && idx < editor.ValidationSession.Results.Length && idx < editor.largeGuiCollectionDrawing.EndIndex)
				{
					ValidationSessionResultCollector.ResultItemSingle r = editor.ValidationSession.Results[idx];
					DynamicObjectAddress address = r.Result.DynamicObjectAddress;
					UnityEngine.Object obj;
					string err;
					if (r.WorkItem.GlobalValidator != null)
					{
						ValidationWorkItem wi = r.WorkItem;
						wi.GlobalValidator = DefaultValidatorLocator.Instance.GetGlobalValidator(wi.GlobalValidator.GetType()) ?? wi.GlobalValidator;
						editor.ValidationSession.Enqueue(wi, insert: false);
					}
					else if (address != null && address.TryGetObjectReference(openSceneIfNeeded: false, autoSaveIfOpenScene: false, out obj, out err))
					{
						if (r.WorkItem.SceneValidators.HasValue && r.WorkItem.SceneValidators.Value.IsLoaded)
						{
							editor.ValidationSession.Enqueue(r.WorkItem, insert: true);
						}
						else if ((bool)obj)
						{
							OdinEntityId entityId = OdinEntityId.FromObject(obj);
							editor.ValidationSession.Enqueue(new ProjectEvent
							{
								AssetGuid = address.LatestAddress.AssetGUID,
								Path = address.LatestAddress.AssetPath,
								Type = ProjectEventType.Revalidation,
								InstanceID = OdinEntityId.Internal.ToInt32(entityId),
								EntityId = entityId,
								Source = r.WorkItem.Source
							}, insert: false);
						}
					}
					editor.offset++;
				}
				else
				{
					editor.offset = 0;
				}
			}
		}

		private static void UnityEditorEventUtility_DuringSceneGUI(SceneView sceneView)
		{
			foreach (ValidationSessionEditor item in ActiveEditors)
			{
				item.OnSceneGui(sceneView);
			}
		}

		private static IValidator GetValidatorForResultOrNull(PropertyTree tree, PersistentValidationResult result)
		{
			if (typeof(GlobalValidator).IsAssignableFrom(result.ValidatorType))
			{
				Type type = result.ValidatorType;
				return DefaultValidatorLocator.Instance.GetGlobalValidator(type);
			}
			if (result.Path == null)
			{
				return null;
			}
			if (typeof(SceneValidator).IsAssignableFrom(result.ValidatorType))
			{
				SceneReference scene = result.GetSceneReference();
				if (scene.IsLoaded)
				{
					Type type2 = result.ValidatorType;
					if (DefaultValidatorLocator.Instance.TryGetSceneValidator(scene, type2, out var validator))
					{
						return validator;
					}
				}
				return null;
			}
			if (tree == null)
			{
				return null;
			}
			InspectorProperty prop = tree.GetPropertyAtPath(result.Path);
			if (prop != null)
			{
				ValidationComponent validator2 = prop.GetComponent<ValidationComponent>();
				IList<Validator> validators = validator2.GetValidators();
				for (int i = 0; i < validators.Count; i++)
				{
					Validator val = validators[i];
					if (val.GetType() == result.ValidatorType)
					{
						if (!(val is IAttributeValidator iAttr))
						{
							return val;
						}
						if (result.ValidatorIndex == iAttr.AttributeNumber)
						{
							return val;
						}
					}
				}
			}
			return null;
		}

		public static ValidationSessionEditor OpenOrFocusWindowForSession(ValidationSession session)
		{
			ValidationSessionEditor editorForSession = ActiveEditors.FirstOrDefault((ValidationSessionEditor n) => n.ValidationSession == session);
			if (editorForSession != null)
			{
				editorForSession.Window.Show();
				editorForSession.Window.Focus();
			}
			else
			{
				editorForSession = OdinValidatorWindow.OpenWindow(session, disposeSessionOnWindowDestroy: false);
			}
			return editorForSession;
		}

		private void SelectIndexAndRemoveFocusControlAndExitGUI(int index)
		{
			if (index != SelectedIndex && index >= 0)
			{
				SelectedIndex = index;
				if (Event.current != null)
				{
					GUIHelper.RemoveFocusControl();
					GUIHelper.ExitGUI(removeFocusControl: true);
				}
			}
		}

		public ValidationSessionEditor(OdinValidatorWindow window, ValidationSession session)
		{
			this.window = window;
			columnDrawer = new FlagEnumColumnDrawer<ResultListColumnFilter>("VALIDATION_SESSION2", ResultListColumnFilter.Message | ResultListColumnFilter.Location, ResultListColumnFilter.Message, null);
			rules = GlobalConfig<RuleConfig>.Instance.GetRuleDataWrapper();
			if (ValidationSession != null)
			{
				ValidationSession.Results.OnResultsChanged -= RepaintWindow;
			}
			ValidationSession = session;
			ValidationSession.Results.OnResultsChanged += RepaintWindow;
			sessionConfigDataDrawer = new ValidationProfileEditor
			{
				OnSaveChanges = ValidationSession.Config.SaveChanges,
				DataSources = ValidationSession.Config.SessionData,
				SelectedConfigSource = new EditorPrefInt("Odin_Validator_Editor_SelectedConfigSource", 0)
			};
			ActiveEditors.Add(this);
		}

		public void OnGUI(Rect area)
		{
			if (Event.current.type == EventType.Layout)
			{
				HandleIssueSelectionChanges();
			}
			Rect originalAreaValue = area;
			folderOff = (folderOff ? folderOff : EditorGUIUtility.IconContent("d_FolderEmpty On Icon").image);
			folderOn = (folderOn ? folderOn : EditorIcons.UnityFolderIcon);
			labelStyle = labelStyle ?? new GUIStyle(SirenixGUIStyles.Label);
			labelStyle.alignment = TextAnchor.MiddleLeft;
			Rect toolbarRect = area.TakeFromTop(21f);
			Rect leftMenuSelectorRect = area.TakeFromLeft(ValidatorGui.IconButtonWidth);
			float leftMenuWidth = this.leftMenuWidth.Value * (float)(MenuVisibility ? 1 : 0);
			Rect leftMenuRect = area.TakeFromLeft(leftMenuWidth);
			if (MenuVisibility && SelectedMenu == MenuOptions.Rules && selectedRuleIndex > -1 && selectedRuleDataDrawer != null)
			{
				Rect ruleConfigRect = area.TakeFromLeft(400f);
				SirenixEditorGUI.DrawSolidRect(ruleConfigRect, SirenixGUIStyles.DarkEditorBackground);
				EditorGUI.DrawRect(ruleConfigRect.TakeFromRight(1f), ValidatorGui.BorderColor);
				GUILayout.BeginArea(ruleConfigRect);
				selectedRuleDataDrawer.Draw(applyUndo: false);
				GUILayout.EndArea();
			}
			this.leftMenuWidth.Value = ValidatorGui.VerticalMenuSlider(leftMenuRect.TakeFromRight(1f), this.leftMenuWidth, 100f, originalAreaValue.width - 40f);
			if (SelectedMenu == MenuOptions.BulkFixing && MenuVisibility)
			{
				Rect rightMenuRect = area.TakeFromRight(rightMenuWidth.Value);
				rightMenuWidth.Value = ValidatorGui.VerticalMenuSlider(area.TakeFromRight(1f), rightMenuWidth, 100f, originalAreaValue.width - 40f, -1);
				DrawBulkFixingFixer(rightMenuRect);
				DrawResults(area);
			}
			else
			{
				Rect issueInfoRect = area.TakeFromBottom(resultInfoHeight);
				Rect issueInfoSlideRect = issueInfoRect.TakeFromTop(1f);
				resultInfoHeight.Value = ValidatorGui.HorizontalMenuSlider(issueInfoSlideRect, resultInfoHeight, 50f, 600f);
				DrawResults(area);
				DrawBottomResultInfo(issueInfoRect);
			}
			DrawLeftMenu(leftMenuRect);
			DrawLeftMenuSelector(leftMenuSelectorRect);
			DrawToolbar(toolbarRect);
			if (Event.current.type != EventType.Repaint || delayedAction == null)
			{
				return;
			}
			try
			{
				delayedAction();
			}
			catch (Exception ex)
			{
				if (ex.IsExitGUIException())
				{
					throw ex.AsExitGUIException();
				}
				Debug.LogException(ex);
			}
			finally
			{
				delayedAction = null;
			}
		}

		public void OnSceneGui(SceneView sceneView)
		{
			HandleIssueSelectionChanges();
			PersistentValidationResult result = SelectedResult;
			if (!(result != null) || !currentSelectedRestoredHighestSeverityResultItem.HasValue)
			{
				return;
			}
			ResultItem resultItem = currentSelectedRestoredHighestSeverityResultItem.Value;
			if (resultItem.OnSceneGUI != null)
			{
				try
				{
					resultItem.OnSceneGUI();
				}
				catch (Exception exception)
				{
					Debug.LogException(exception);
				}
			}
		}

		private IValidator GetValidatorForCurrentSelectedResultOrNull()
		{
			if (SelectedResult == null)
			{
				return null;
			}
			return GetValidatorForResultOrNull(currentResultTree, SelectedResult);
		}

		private void RepaintWindow()
		{
			if ((bool)window)
			{
				window.Repaint();
			}
		}

		private void HandleIssueSelectionChanges()
		{
			ValidationSession.Results.ApplyFilters = (SelectedMenu == MenuOptions.FilterResults || SelectedMenu == MenuOptions.BulkFixing) && MenuVisibility;
			bool enableBulkFixing = SelectedMenu == MenuOptions.BulkFixing && MenuVisibility;
			if (enableBulkFixing != ValidationSession.Results.FixTypeFilters.Enabled)
			{
				ValidationSession.Results.FixTypeFilters.Enabled = enableBulkFixing;
				ValidationSession.Results.MarkFiltersDirty();
			}
			if (Event.current.type == EventType.Layout)
			{
				if (SelectedMenu == MenuOptions.FilterResults || !MenuVisibility)
				{
					if (ValidationSession.Results.FixTypeFilters.Enabled)
					{
						if (selectionToRecover == null)
						{
							selectionToRecover = SelectedResult;
						}
						ValidationSession.Results.MarkFiltersDirty();
					}
				}
				else if (SelectedMenu == MenuOptions.BulkFixing)
				{
					ValidationSessionResultCollector.Filter<FixIdentifier> filters = ValidationSession.Results.FixTypeFilters;
					if (!filters.Enabled)
					{
						if (selectionToRecover == null)
						{
							selectionToRecover = SelectedResult;
						}
						filters.Enabled = true;
						ValidationSession.Results.MarkFiltersDirty();
					}
					bool turnOthersOff = false;
					if (Event.current.type == EventType.Layout)
					{
						foreach (ValidationSessionResultCollector.Filter<FixIdentifier>.FilteredItem item in filters.OrderedItems)
						{
							if (turnOthersOff)
							{
								if (item.Enabled)
								{
									if (selectionToRecover == null)
									{
										selectionToRecover = SelectedResult;
									}
									item.Enabled = false;
									ValidationSession.Results.MarkFiltersDirty();
								}
							}
							else if (item.Enabled)
							{
								turnOthersOff = true;
							}
						}
					}
				}
			}
			PersistentValidationResult selectedResult = SelectedResult;
			if (selectionToRecover != null)
			{
				PersistentValidationResult toRecover = selectionToRecover;
				InvokeEndOfRepaint(delegate
				{
					if (prevSelectedResult != selectedResult)
					{
						int num = 0;
						ValidationSessionResultCollector.ResultItemSingle[] filteredItems = ValidationSession.Results.GetFilteredItems();
						for (int i = 0; i < filteredItems.Length; i++)
						{
							if (PersistentValidationResult.Comparer.Equals(toRecover, filteredItems[i].Result))
							{
								num = i;
								break;
							}
						}
						selectedResult = SelectedResult;
						nextScrollTo = SelectedIndex;
						if (num != SelectedIndex && num >= 0)
						{
							SelectedIndex = num;
							if (Event.current != null)
							{
								GUIHelper.RemoveFocusControl();
								GUIHelper.ExitGUI(removeFocusControl: true);
							}
						}
					}
				});
				selectionToRecover = null;
			}
			if (prevSelectedResult != selectedResult)
			{
				SceneView.RepaintAll();
				prevSelectedResult = selectedResult;
				currentResultTree?.Dispose();
				currentResultTree = null;
				metaDataTree?.Dispose();
				metaDataTree = null;
				currentSelectedRestoredHighestSeverityResultItem = null;
				if (selectedResult?.DynamicObjectAddress != null && selectedResult.ValidatorType != null)
				{
					selectedResult.DynamicObjectAddress.TryGetObjectReference(openSceneIfNeeded: false, autoSaveIfOpenScene: false, out var uObj, out var _);
					if ((bool)uObj)
					{
						currentResultTree = PropertyTree.Create(uObj);
					}
					IValidator validator = GetValidatorForCurrentSelectedResultOrNull();
					ResultItemMetaData[] metadata = null;
					ResultItemPersistor.PersistenceContext rebuildContext = default(ResultItemPersistor.PersistenceContext);
					bool hasRebuildContext = false;
					if (validator != null)
					{
						hasRebuildContext = true;
						rebuildContext = ResultItemPersistor.CreateContextFromValidator(validator);
					}
					else if (currentResultTree != null)
					{
						hasRebuildContext = true;
						InspectorProperty prop = currentResultTree.GetPropertyAtPath(selectedResult.Path);
						rebuildContext = new ResultItemPersistor.PersistenceContext
						{
							Tree = currentResultTree,
							Property = prop,
							Root = uObj,
							ValueEntry = prop?.ValueEntry,
							BaseValueEntry = prop?.BaseValueEntry
						};
					}
					if (hasRebuildContext)
					{
						ResultItem rebuiltResult2;
						if (typeof(SceneValidator).IsAssignableFrom(selectedResult.ValidatorType))
						{
							SceneReference scene = selectedResult.GetSceneReference();
							if (scene.IsValid && scene.IsLoaded && ResultItemPersistor.TryRebuildResultItem(in selectedResult.Result, ref rebuildContext, openSceneIfNeeded: false, out var rebuiltResult))
							{
								currentSelectedRestoredHighestSeverityResultItem = rebuiltResult;
								metadata = currentSelectedRestoredHighestSeverityResultItem.Value.MetaData;
							}
						}
						else if (ResultItemPersistor.TryRebuildResultItem(in selectedResult.Result, ref rebuildContext, openSceneIfNeeded: false, out rebuiltResult2))
						{
							currentSelectedRestoredHighestSeverityResultItem = rebuiltResult2;
							metadata = currentSelectedRestoredHighestSeverityResultItem.Value.MetaData;
						}
						if (metadata != null && metadata.Length != 0)
						{
							metaDataTree = PropertyTree.Create(new ResultItemMetaDataDrawer(metadata, excludeFirstButton: false));
						}
					}
				}
			}
			Fix fix = selectedResult?.Data.Fix;
			if (SelectedResult == null)
			{
				issueFixerTree?.Dispose();
				issueFixerTree = null;
				currentFix = null;
			}
			else if (currentFix != null)
			{
				if (fix != null)
				{
					MethodInfo currFixerType = currentFix.Action.Method;
					MethodInfo fixerType = fix.Action.Method;
					if (fixerType != currFixerType)
					{
						issueFixerTree?.Dispose();
						issueFixerTree = null;
						currentFix = null;
					}
				}
				else
				{
					issueFixerTree?.Dispose();
					issueFixerTree = null;
					currentFix = null;
				}
			}
			if (currentFix == null && fix != null)
			{
				object obj = fix.CreateEditorObject();
				issueFixerTree?.Dispose();
				if (obj != null)
				{
					issueFixerTree = PropertyTree.Create(obj);
				}
				currentFix = fix;
			}
		}

		private void DrawBulkFixingFixer(Rect rect)
		{
			if (currentFix != null)
			{
				Rect toolbarRect = rect.TakeFromTop(21f);
				Rect btnRect = toolbarRect.AlignRight(110f).VerticalPadding(0f, 1f);
				ValidatorGui.Header(toolbarRect, currentFix.Title, null);
				EditorGUI.DrawRect(btnRect.AddX(-1f).SetWidth(1f), ValidatorGui.BorderColor);
				if (ValidatorGui.ToolbarBtn(btnRect, on: false, SdfIconType.Tools, "Fix now", ""))
				{
					List<PersistentValidationResult> toFix = (from x in ValidationSession.Results.GetFilteredItems()
						where x.Result != null && x.Result.ResultType != ValidationResultType.Valid
						select x.Result).ToList();
					FixResults(toFix);
				}
				GUILayout.BeginArea(rect.TakeFromTop(rect.height * 0.5f));
				bulkFixScrollPos = EditorGUILayout.BeginScrollView(bulkFixScrollPos, Padding);
				DrawFixerTree(rect.width);
				EditorGUILayout.EndScrollView();
				GUILayout.EndArea();
			}
			PersistentValidationResult selectedResult = SelectedResult;
			if (selectedResult != null)
			{
				GUILayout.BeginArea(rect);
				GUILayout.Space(1f);
				ValidatorGui.Header("Issue info");
				bulkFixMetaDataScrollPos = EditorGUILayout.BeginScrollView(bulkFixMetaDataScrollPos);
				GUILayout.BeginVertical(Padding);
				DrawIssueMessage(rect.width - (float)Padding.padding.horizontal);
				GUILayout.Space(10f);
				DrawIssueInfo();
				GUILayout.EndVertical();
				if (metaDataTree != null)
				{
					ValidatorGui.Header("Metadata");
					DrawMetaData();
				}
				EditorGUILayout.EndScrollView();
				GUILayout.EndArea();
			}
		}

		private void DrawFixerTree(float width)
		{
			if (issueFixerTree != null)
			{
				GUIHelper.PushLabelWidth(width * 0.25f);
				issueFixerTree.Draw(applyUndo: false);
				GUIHelper.PopLabelWidth();
			}
			else
			{
				SirenixEditorGUI.MessageBox("Click the execute button above to apply fix", MessageType.Info, GlobalConfig<GeneralDrawerConfig>.Instance.MessageBoxFontSize);
			}
		}

		private void FixResults(List<PersistentValidationResult> toFix)
		{
			Delegate action = currentFix.Action;
			object arg = issueFixerTree?.WeakTargets[0];
			MethodInfo fixType = currentFix.Action.Method;
			InvokeEndOfRepaint(delegate
			{
				PropertyTree propertyTree = null;
				SceneSetup[] sceneManagerSetup = EditorSceneManager.GetSceneManagerSetup();
				if (toFix.Count == 0)
				{
					return;
				}
				HashSet<SceneReference> hashSet = new HashSet<SceneReference>();
				foreach (PersistentValidationResult current in toFix)
				{
					SceneReference sceneReference = current.GetSceneReference();
					if (sceneReference.IsValid)
					{
						hashSet.Add(sceneReference);
					}
				}
				if (hashSet.Any((SceneReference x) => !x.IsLoaded))
				{
					EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
				}
				SceneReference sceneReference2 = default(SceneReference);
				try
				{
					for (int num = 0; num < toFix.Count; num++)
					{
						PersistentValidationResult persistentValidationResult = toFix[num];
						if (typeof(GlobalValidator).IsAssignableFrom(persistentValidationResult.ValidatorType))
						{
							IValidator validatorForResultOrNull = GetValidatorForResultOrNull(null, persistentValidationResult);
							ResultItemPersistor.PersistenceContext context = ResultItemPersistor.CreateContextFromValidator(validatorForResultOrNull);
							if (ResultItemPersistor.TryRebuildResultItem(in persistentValidationResult.Result, ref context, openSceneIfNeeded: false, out var result))
							{
								if (arg == null)
								{
									result.Fix.Action.DynamicInvoke();
								}
								else
								{
									result.Fix.Action.DynamicInvoke(arg);
								}
							}
						}
						else if (persistentValidationResult.DynamicObjectAddress == null)
						{
							if ((bool)GlobalConfig<GlobalValidationConfig>.Instance.DebugMode)
							{
								Debug.LogError("Currently unsupported/unimplented case: fixing issue for non-UnityEngine.Object root value.");
							}
						}
						else
						{
							SceneReference sceneReference3 = persistentValidationResult.GetSceneReference();
							if (sceneReference3.IsValid && sceneReference2 != sceneReference3)
							{
								ValidationSession.Results.ProcessQueue();
								sceneReference2 = sceneReference3;
							}
							if (!persistentValidationResult.DynamicObjectAddress.TryGetObjectReference(openSceneIfNeeded: true, autoSaveIfOpenScene: true, out var result2, out var errorMessage))
							{
								if ((bool)GlobalConfig<GlobalValidationConfig>.Instance.DebugMode)
								{
									Debug.LogError("Could not load object to fix with; resolving address failed with error '" + errorMessage + "'. The address was: " + persistentValidationResult.DynamicObjectAddress.LatestAddress.ToString(prettyPrint: true));
								}
							}
							else if (!(persistentValidationResult.ValidatorType == null))
							{
								if (GUIHelper.ShouldDisplaySmartCancellableProgressBar() && GUIHelper.DisplaySmartUpdatingCancellableProgressBar($"Fixing {num + 1} / {toFix.Count} results", result2?.ToString() ?? "", (float)num / (float)toFix.Count))
								{
									break;
								}
								if (typeof(SceneValidator).IsAssignableFrom(persistentValidationResult.ValidatorType))
								{
									Scene scene;
									Scene scene2;
									if (!sceneReference3.IsValid)
									{
										Debug.LogError("Invalid scene, unable to apply fix for " + persistentValidationResult.DynamicObjectAddress.LatestAddress.AssetPath);
									}
									else if ((sceneReference3.IsLoaded || sceneReference3.TryOpenScene(OpenSceneMode.Single, out scene)) && sceneReference3.TryGetScene(out scene2))
									{
										IValidator validatorForResultOrNull2 = GetValidatorForResultOrNull(null, persistentValidationResult);
										InvokeFix(result2, arg, fixType, persistentValidationResult, validatorForResultOrNull2);
										EditorSceneManager.SaveScene(scene2);
									}
								}
								else
								{
									if (propertyTree == null || propertyTree.TargetType != result2.GetType())
									{
										propertyTree?.Dispose();
										propertyTree = PropertyTree.Create(result2);
									}
									else
									{
										propertyTree.SetTargets(result2);
									}
									IValidator validatorForResultOrNull3 = GetValidatorForResultOrNull(propertyTree, persistentValidationResult);
									InvokeFix(result2, arg, fixType, persistentValidationResult, validatorForResultOrNull3);
								}
							}
						}
					}
				}
				finally
				{
					Undo.FlushUndoRecordObjects();
					propertyTree?.Dispose();
					propertyTree = null;
					ValidationSession.Results.ProcessQueue();
					EditorUtility.ClearProgressBar();
					SceneSetup[] sceneManagerSetup2 = EditorSceneManager.GetSceneManagerSetup();
					if (sceneManagerSetup2.Length != sceneManagerSetup.Length)
					{
						EditorSceneManager.SaveOpenScenes();
						EditorSceneManager.RestoreSceneManagerSetup(sceneManagerSetup);
					}
					else
					{
						for (int num2 = 0; num2 < sceneManagerSetup2.Length; num2++)
						{
							if (sceneManagerSetup[num2].path != sceneManagerSetup2[num2].path)
							{
								EditorSceneManager.SaveOpenScenes();
								EditorSceneManager.RestoreSceneManagerSetup(sceneManagerSetup);
								break;
							}
						}
					}
				}
			});
		}

		private void InvokeFix(UnityEngine.Object obj, object arg, MethodInfo fixType, PersistentValidationResult item, IValidator validator)
		{
			ResultItemPersistor.PersistenceContext restoreContext = ResultItemPersistor.CreateContextFromValidator(validator);
			if (!ResultItemPersistor.TryRebuildResultItem(in item.Result, ref restoreContext, openSceneIfNeeded: false, out var restoredResultItem) || !(restoredResultItem.Fix.Action.Method == fixType))
			{
				return;
			}
			try
			{
				Undo.RegisterCompleteObjectUndo(obj, "Bulk Fix (" + currentFix.GetType().GetNiceName() + ")");
				if (arg == null)
				{
					restoredResultItem.Fix.Action.DynamicInvoke();
				}
				else
				{
					restoredResultItem.Fix.Action.DynamicInvoke(arg);
				}
				if (restoreContext.Tree != null)
				{
					restoreContext.Tree.InvokeDelayedActions();
					restoreContext.Tree.ApplyChanges();
					restoreContext.Tree.InvokeDelayedActions();
				}
				if (item.DynamicObjectAddress.TryGetObjectReference(openSceneIfNeeded: true, autoSaveIfOpenScene: true, out var fixedObj, out var _))
				{
					ValidationSession.Enqueue(ValidationWorkItem.CreateForEntityId(OdinEntityId.FromObject(fixedObj), ProjectEventSource.RevalidationDuringFixes), insert: false);
					ValidationSession.ValidateQueuedUpWorkNow(showProgressBar: false, processResultQueue: false);
				}
				if (validator is SceneValidator)
				{
					SceneReference scene = item.GetSceneReference();
					if (scene.IsValid)
					{
						ValidationSession.Enqueue(ValidationWorkItem.CreateForSceneValidators(scene, ProjectEventSource.RevalidationDuringFixes), insert: false);
						ValidationSession.ValidateQueuedUpWorkNow(showProgressBar: false, processResultQueue: false);
					}
				}
			}
			catch (Exception exception)
			{
				if ((bool)GlobalConfig<GlobalValidationConfig>.Instance.DebugMode)
				{
					Debug.LogException(exception);
				}
			}
		}

		private void DrawBulkFixing()
		{
			DrawFilteredList(ValidationSession.Results.FixTypeFilters, filterToggleFixTypes, "Fixables", isRadio: true);
			DrawFilteredList(ValidationSession.Results.SceneFilters, filterToggleScenes, "Locations");
			DrawFilteredList(ValidationSession.Results.ObjectTypeFilters, filterToggleObjectTypes, "Types");
			DrawFilteredList(ValidationSession.Results.ValidatorTypeFilters, filterToggleValidatorTypes, "Validators");
		}

		private void DrawBottomResultInfo(Rect issueInfoRect)
		{
			if (!(SelectedResult == null))
			{
				if (issueInfoRect.width > 500f)
				{
					Rect rightMenuRect = issueInfoRect.TakeFromRight(rightMenuWidth.Value);
					rightMenuWidth.Value = ValidatorGui.VerticalMenuSlider(issueInfoRect.TakeFromRight(1f), rightMenuWidth, 100f, 900f, -1);
					GUILayout.BeginArea(rightMenuRect);
					metaDataScrollPos = EditorGUILayout.BeginScrollView(metaDataScrollPos);
					DrawMetaFixer(rightMenuRect.width);
					EditorGUILayout.EndScrollView();
					GUILayout.EndArea();
					GUILayout.BeginArea(issueInfoRect);
					infoScrollPos = EditorGUILayout.BeginScrollView(infoScrollPos);
					GUILayout.BeginVertical(Padding);
					DrawIssueMessage(issueInfoRect.width - (float)Padding.padding.horizontal);
					GUILayout.EndVertical();
					EditorGUILayout.EndScrollView();
					GUILayout.EndArea();
				}
				else
				{
					GUILayout.BeginArea(issueInfoRect);
					metaDataScrollPos = EditorGUILayout.BeginScrollView(metaDataScrollPos);
					GUILayout.BeginVertical(Padding);
					DrawIssueMessage(issueInfoRect.width - (float)Padding.padding.horizontal);
					GUILayout.EndVertical();
					DrawMetaFixer(issueInfoRect.width);
					EditorGUILayout.EndScrollView();
					GUILayout.EndArea();
				}
			}
			void DrawMetaFixer(float width)
			{
				bool drawInfoHeader = false;
				if (currentFix != null)
				{
					Rect toolbarRect = ValidatorGui.Header(currentFix.Title);
					GUILayout.BeginVertical(Padding);
					DrawFixerTree(width);
					drawInfoHeader = true;
					Rect btnRect = toolbarRect.AlignRight(110f).VerticalPadding(0f, 1f);
					EditorGUI.DrawRect(btnRect.AddX(-1f).SetWidth(1f), ValidatorGui.BorderColor);
					if (ValidatorGui.ToolbarBtn(btnRect, on: false, SdfIconType.Tools, "Fix now", ""))
					{
						FixResults(new List<PersistentValidationResult> { SelectedResult });
						if ((bool)GlobalConfig<GlobalValidationConfig>.Instance.SelectNextIssueOnFix)
						{
							SelectAndScrollToIssueAtIndex(SelectedIndex + 1);
						}
					}
					GUILayout.EndVertical();
				}
				if (metaDataTree != null)
				{
					drawInfoHeader = true;
					ValidatorGui.Header("Metadata");
					DrawMetaData();
					GUILayout.Space(10f);
				}
				if (drawInfoHeader)
				{
					ValidatorGui.Header("Info");
				}
				GUILayout.BeginVertical(Padding);
				DrawIssueInfo();
				GUILayout.EndVertical();
			}
		}

		private void SelectAndScrollToIssueAtIndex(int newIndex)
		{
			InvokeEndOfRepaint(delegate
			{
				SelectedIndex = newIndex;
				GUIHelper.RequestRepaint();
				largeGuiCollectionDrawing.ScrollTo(SelectedIndex, ref resultsScrollPos);
				GUIHelper.RemoveFocusControl();
			});
		}

		private void DrawMetaData()
		{
			if (metaDataTree != null)
			{
				GUILayout.BeginVertical(Padding);
				metaDataTree.Draw(applyUndo: false);
				GUILayout.EndVertical();
			}
		}

		private void DrawIssueMessage(float width)
		{
			selectableLabelStyle = selectableLabelStyle ?? new GUIStyle(SirenixGUIStyles.MultiLineLabel)
			{
				onActive = SirenixGUIStyles.MultiLineLabel.normal,
				onFocused = SirenixGUIStyles.MultiLineLabel.normal,
				onHover = SirenixGUIStyles.MultiLineLabel.normal,
				onNormal = SirenixGUIStyles.MultiLineLabel.normal,
				normal = SirenixGUIStyles.MultiLineLabel.normal,
				active = SirenixGUIStyles.MultiLineLabel.normal,
				hover = SirenixGUIStyles.MultiLineLabel.normal,
				focused = SirenixGUIStyles.MultiLineLabel.normal,
				alignment = TextAnchor.UpperLeft,
				fixedHeight = 0f,
				stretchHeight = true,
				richText = true
			};
			PersistentValidationResult selectedResult = SelectedResult;
			if (!(selectedResult == null))
			{
				selectableLabelStyle.richText = true;
				string message = selectedResult.Message;
				float height = selectableLabelStyle.CalcHeight(GUIHelper.TempContent(message), width);
				Rect rect = GUILayoutUtility.GetRect(0f, height);
				EditorGUI.SelectableLabel(rect, message, selectableLabelStyle);
			}
		}

		private void DrawIssueInfo()
		{
			PersistentValidationResult selectedResult = SelectedResult;
			if (!(selectedResult == null))
			{
				ObjectAddress address = selectedResult.DynamicObjectAddress?.LatestAddress;
				string propertyPath = selectedResult.Path;
				SceneReference scene = selectedResult.GetSceneReference();
				string assetPath = address?.AssetPath;
				string hierarchyPath = address?.Hierarchy.ToString();
				string validatorType = selectedResult.GetGenericValidatorType().GetNiceName();
				bool showAdvanced = GlobalConfig<GlobalValidationConfig>.Instance.DebugMode.Value;
				DrawItem("Scene", scene.Name);
				DrawItem("Asset Path", assetPath);
				DrawItem("Hierachy Path", hierarchyPath);
				DrawItem("Property Path", propertyPath);
				DrawItem("Validator Type", validatorType);
				if (showAdvanced)
				{
					string isBroken = address?.IsBroken.ToString();
					ValidationWorkItem workItem = SelectedItem.WorkItem;
					string latestEntityId = selectedResult.DynamicObjectAddress?.LatestEntityId.ToString();
					string objectType = selectedResult.DynamicObjectAddress?.LatestAddress?.ObjectType.Type?.GetNiceName();
					DrawItem("Is Broken", isBroken);
					DrawItem("Object Type", objectType);
					DrawItem("Selection Object", selectedResult.Data.SelectionObjectAddress?.ToString());
					DrawItem("Latest Entity Id", latestEntityId);
					int h1 = RuntimeHelpers.GetHashCode(selectedResult.DynamicObjectAddress);
					int h2 = RuntimeHelpers.GetHashCode(selectedResult.DynamicObjectAddress.LatestAddress);
					DrawItem("DO Address ID", h1.GetHashCode().ToString());
					DrawItem("Latest Address ID", h2.GetHashCode().ToString());
					DrawItem("Latest Address Type", selectedResult.DynamicObjectAddress.LatestAddress.Type.ToString());
					DrawWorkItem(workItem, "Work Item");
				}
			}
			static void DrawItem(string label, string value)
			{
				if (!string.IsNullOrEmpty(value) || GlobalConfig<GlobalValidationConfig>.Instance.DebugMode.Value)
				{
					Rect rect = GUILayoutUtility.GetRect(0f, EditorGUIUtility.singleLineHeight);
					bool isMouseOver = rect.Contains(Event.current.mousePosition);
					Rect labelRect = rect.TakeFromLeft(100f);
					GUIStyle style = (isMouseOver ? SirenixGUIStyles.LeftAlignedWhiteMiniLabel : SirenixGUIStyles.LeftAlignedGreyMiniLabel);
					GUI.Label(labelRect, label, style);
					GUI.Label(rect, value, style);
				}
			}
		}

		private void DrawResults(Rect resultsRect)
		{
			GUILayout.BeginArea(resultsRect);
			Rect columnHeaderRect = GUILayoutUtility.GetRect(0f, 21f);
			float time = (float)EditorApplication.timeSinceStartup;
			columnDrawer.ColumnLabelStyle = SirenixGUIStyles.Label;
			columnDrawer.BeginDrawColumns(columnHeaderRect);
			resultsScrollPos = EditorGUILayout.BeginScrollView(resultsScrollPos);
			largeGuiCollectionDrawing.AllocateLayout(21, ValidationSession.Results.Length);
			if (nextScrollTo != -1)
			{
				largeGuiCollectionDrawing.ScrollTo(nextScrollTo, ref resultsScrollPos);
				nextScrollTo = -1;
			}
			if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.A && Sirenix.Utilities.Editor.UnityShims.Misc.GetEventModifiers(Event.current) == 2)
			{
				List<DynamicObjectAddress> toSelect = new List<DynamicObjectAddress>();
				for (int j = 0; j < ValidationSession.Results.Length; j++)
				{
					toSelect.Add(ValidationSession.Results[j].Result?.SelectionObjectAddress);
				}
				SelectionUtils.AddToSelection(toSelect);
			}
			if (Event.current.type != EventType.Layout)
			{
				ResultListColumnFilter[] columns = EnumTypeUtilities<ResultListColumnFilter>.DecomposeEnumFlagValues(columnDrawer.Columns.Value);
				for (int i = largeGuiCollectionDrawing.StartIndex; i < largeGuiCollectionDrawing.EndIndex; i++)
				{
					ref ValidationSessionResultCollector.ResultItemSingle item = ref ValidationSession.Results[i];
					item.Update();
					DynamicObjectAddress objAddress = item.Result.SelectionObjectAddress;
					bool isSelectedInHiearchy = objAddress != null && SelectionUtils.Selection.Contains(objAddress.LatestEntityId);
					Rect rect = largeGuiCollectionDrawing.GetRect(i);
					bool isUnloaded = item.Result.SelectionObjectAddress?.IsUnloaded ?? false;
					bool isSelected = i == SelectedIndex;
					if (isUnloaded)
					{
						GUIHelper.PushColor(new Color(1f, 1f, 1f, 0.2f));
					}
					Color bg = ValidatorGui.EditorWindowBgColor;
					if (isSelected)
					{
						bg = ValidatorGui.BtnActiveBgColor;
					}
					if (isSelectedInHiearchy && i != SelectedIndex)
					{
						bg = ValidatorGui.BtnActiveBgColor;
						bg.a *= 0.3f;
					}
					float speed = 0.2f;
					float stateChangeT = time - item.LastStateChangeTime;
					if (stateChangeT < speed)
					{
						GUIHelper.RequestRepaint();
						stateChangeT = (speed - stateChangeT) / speed;
					}
					else
					{
						stateChangeT = 0f;
					}
					if (bg != ValidatorGui.EditorWindowBgColor)
					{
						EditorGUI.DrawRect(rect, bg);
					}
					GUIStyle labelStyle = ((i == SelectedIndex) ? ValidatorGui.WhiteLabelVertical : ValidatorGui.LabelVertical);
					ObjectAddress address = item.Result.SelectionObjectAddress?.LatestAddress;
					ResultListColumnFilter[] array = columns;
					foreach (ResultListColumnFilter column in array)
					{
						Rect columnRect = columnDrawer.GetColumnRect(rect, column).AlignCenterY(EditorGUIUtility.singleLineHeight);
						columnRect.x += 7f;
						columnRect.width -= 14f;
						switch (column)
						{
						case ResultListColumnFilter.Message:
						{
							if (item.Result.Batch.Count > 1 && Event.current.type == EventType.Repaint)
							{
								Color col = ValidatorGui.BtnContentColor;
								float one = columnRect.height;
								float halfY = columnRect.height * 0.5f;
								int halfX = 4;
								PersistentValidationResultBatch currBatch = item.Result.Batch;
								bool isFirst = i == 0 || currBatch != ValidationSession.Results[i - 1].Result.Batch;
								bool isLast = i == ValidationSession.Results.Length - 1 || currBatch != ValidationSession.Results[i + 1].Result.Batch;
								if (isFirst && !isLast)
								{
									EditorGUI.DrawRect(new Rect(columnRect.x - (float)halfX, columnRect.y + halfY, 1f, halfY + 1f), col);
									EditorGUI.DrawRect(new Rect(columnRect.x - (float)halfX, columnRect.y + halfY, halfX, 1f), col);
								}
								else if (isLast && !isFirst)
								{
									EditorGUI.DrawRect(new Rect(columnRect.x - (float)halfX, columnRect.y - 2f, 1f, halfY + 3f), col);
									EditorGUI.DrawRect(new Rect(columnRect.x - (float)halfX, columnRect.y + halfY, halfX, 1f), col);
								}
								else if (!isLast && !isFirst && i > 0 && i < ValidationSession.Results.Length)
								{
									EditorGUI.DrawRect(new Rect(columnRect.x - (float)halfX, columnRect.y - 2f, 1f, one + 3f), col);
									EditorGUI.DrawRect(new Rect(columnRect.x - (float)halfX, columnRect.y + halfY, halfX, 1f), col);
								}
							}
							float h = 16f + stateChangeT * 10f;
							Rect iconRect3 = columnRect.TakeFromLeft(columnRect.height).AlignCenter(h, h);
							if (item.Result.ResultType == ValidationResultType.Error)
							{
								ValidatorGui.DrawErrorIcon(iconRect3, bg, hasErrors: true, isSelected);
							}
							else if (item.Result.ResultType == ValidationResultType.Warning)
							{
								ValidatorGui.DrawWarningIcon(iconRect3, bg, hasWarnings: true, isSelected);
							}
							else
							{
								ValidatorGui.DrawValidIcon(iconRect3, bg, hasValids: true, isSelected);
							}
							if (item.Result.Data.Fix != null)
							{
								Rect fixIconRect = columnRect.TakeFromRight(columnRect.height).AlignCenter(h, h);
								if (isSelected)
								{
									SdfIcons.DrawIcon(fixIconRect, SdfIconType.Tools, ValidatorGui.BtnMouseOverContentColor);
								}
								else
								{
									SdfIcons.DrawIcon(fixIconRect, SdfIconType.Tools, ValidatorGui.BtnContentColor);
								}
							}
							bool prev = labelStyle.richText;
							labelStyle.richText = true;
							GUI.Label(columnRect, item.Result.Message, labelStyle);
							break;
						}
						case ResultListColumnFilter.Object:
						{
							bool isUnknown = address == null || address == ObjectAddress.Unknown;
							if (!isUnknown)
							{
								Texture2D icon3 = GUIHelper.GetAssetThumbnail(null, address.ObjectType.Type, preferObjectPreviewOverFileIcon: false);
								if (icon3 != null)
								{
									Rect iconRect4 = columnRect.TakeFromLeft(columnRect.height).AlignCenter(16f, 16f);
									GUI.DrawTexture(iconRect4, icon3);
								}
							}
							if (isUnknown)
							{
								GUI.Label(columnRect, "-", labelStyle);
								break;
							}
							string objTypeName = address.ObjectType.GetNiceName();
							if (objTypeName != null)
							{
								GUI.Label(columnRect, address.Name + " / " + objTypeName, labelStyle);
							}
							else
							{
								GUI.Label(columnRect, address.Name, labelStyle);
							}
							break;
						}
						case ResultListColumnFilter.Location:
						{
							if (address == null || string.IsNullOrEmpty(address.AssetPath))
							{
								GUI.Label(columnRect, "-", labelStyle);
								break;
							}
							string location = Path.GetFileNameWithoutExtension(address.AssetPath);
							Texture icon2 = ((address.Type != ObjectAddress.AddressType.SceneComponent && address.Type != ObjectAddress.AddressType.SceneGameObject) ? ((address.Type == ObjectAddress.AddressType.PrefabComponent || address.Type == ObjectAddress.AddressType.PrefabGameObject) ? ValidatorGui.PrefabIcon : ((address.Type != ObjectAddress.AddressType.Asset) ? GUIHelper.GetAssetThumbnail(null, typeof(UnityEngine.Object), preferObjectPreviewOverFileIcon: false) : GUIHelper.GetAssetThumbnail(null, address.ObjectType.Type, preferObjectPreviewOverFileIcon: false))) : ValidatorGui.SceneAssetIcon);
							Rect iconRect2 = columnRect.TakeFromLeft(columnRect.height).AlignCenter(16f, 16f);
							if (icon2 != null)
							{
								GUI.DrawTexture(iconRect2, icon2);
							}
							GUI.Label(columnRect, location, labelStyle);
							break;
						}
						case ResultListColumnFilter.Validator:
						{
							Type t2 = item.Result.ValidatorType;
							if (t2 != null)
							{
								GUI.Label(columnRect, t2.GetNiceName(), labelStyle);
							}
							else
							{
								GUI.Label(columnRect, "-", labelStyle);
							}
							break;
						}
						case ResultListColumnFilter.Path:
						{
							ObjectAddress.AddressType t = item.Result.SelectionObjectAddress?.LatestAddress.Type ?? ObjectAddress.AddressType.Unknown;
							Texture icon = (((object)address != null && address.AssetPath != null) ? ((t != ObjectAddress.AddressType.PrefabGameObject && t != ObjectAddress.AddressType.PrefabComponent && !address.AssetPath.FastEndsWith(".prefab")) ? ((t != ObjectAddress.AddressType.SceneComponent && t != ObjectAddress.AddressType.SceneGameObject) ? GUIHelper.GetAssetThumbnail(null, address.ObjectType, preferObjectPreviewOverFileIcon: false) : ValidatorGui.SceneAssetIcon) : ValidatorGui.PrefabIcon) : null);
							if (icon != null)
							{
								Rect iconRect = columnRect.TakeFromLeft(columnRect.height).AlignCenter(16f, 16f);
								GUI.DrawTexture(iconRect, icon);
							}
							GUI.Label(columnRect, address?.AssetPath ?? "-", labelStyle);
							break;
						}
						}
					}
					if (isUnloaded)
					{
						GUIHelper.PopColor();
					}
					if (rect.Contains(Event.current.mousePosition) && Event.current.isMouse && Event.current.type == EventType.MouseDown)
					{
						if (Sirenix.Utilities.Editor.UnityShims.Misc.GetEventModifiers(Event.current) == 2 || Sirenix.Utilities.Editor.UnityShims.Misc.GetEventModifiers(Event.current) == 1)
						{
							if (Event.current.button == 1)
							{
								if (!item.Result.SelectionObjectAddress.IsUnloaded && !item.Result.SelectionObjectAddress.IsBroken && item.Result.SelectionObjectAddress.TryGetObjectReference(openSceneIfNeeded: false, autoSaveIfOpenScene: false, out var obj, out var _))
								{
									if (obj.GetType() == typeof(GameObject))
									{
										GUIHelper.OpenInspectorWindow(obj);
									}
									else
									{
										OdinEditorWindow.InspectObjectInDropDown(obj, 400f);
									}
								}
								Event.current.Use();
							}
							else if (Sirenix.Utilities.Editor.UnityShims.Misc.GetEventModifiers(Event.current) == 2)
							{
								SelectionUtils.AddToSelection(Enumerable.Repeat(item.Result.SelectionObjectAddress, 1));
							}
							else if (Sirenix.Utilities.Editor.UnityShims.Misc.GetEventModifiers(Event.current) == 1)
							{
								int from = SelectedIndex;
								int to = i;
								if (to < from)
								{
									int tmp = from;
									from = to;
									to = tmp;
								}
								List<DynamicObjectAddress> toSelect2 = new List<DynamicObjectAddress>();
								for (int l = from; l <= to; l++)
								{
									toSelect2.Add(ValidationSession.Results[l].Result?.SelectionObjectAddress);
								}
								SelectionUtils.AddToSelection(toSelect2);
							}
						}
						else
						{
							DynamicObjectAddress address2 = item.Result?.SelectionObjectAddress;
							if (Event.current.clickCount == 2 && SelectedIndex == i)
							{
								InvokeEndOfRepaint(delegate
								{
									SelectionUtils.SelectInInspector(address2, allowOpenScene: true, ping: true);
								});
							}
							else
							{
								InvokeEndOfRepaint(delegate
								{
									SelectionUtils.SelectInInspector(address2, allowOpenScene: false, ping: false);
								});
							}
						}
						GUIHelper.RequestRepaint();
						largeGuiCollectionDrawing.ScrollTo(i, ref resultsScrollPos);
						Event.current.Use();
						SelectIndexAndRemoveFocusControlAndExitGUI(i);
					}
					if (Event.current.OnContextClick(rect) && Sirenix.Utilities.Editor.UnityShims.Misc.GetEventModifiers(Event.current) != 2 && Sirenix.Utilities.Editor.UnityShims.Misc.GetEventModifiers(Event.current) != 1)
					{
						CreateAndShowGenericMenu(item);
					}
				}
			}
			EditorGUILayout.EndScrollView();
			columnDrawer.EndDrawColumns(GUIHelper.GetCurrentLayoutRect().ResetPosition());
			GUILayout.EndArea();
			int index = SelectedIndex;
			bool shiftSelect = (Sirenix.Utilities.Editor.UnityShims.Misc.GetEventModifiers(Event.current) & 1) != 0;
			ValidatorGui.HandleKeyboardNavigation(ref index);
			index = Mathf.Clamp(index, 0, ValidationSession.Results.Length - 1);
			if (SelectedIndex == index)
			{
				return;
			}
			if (index >= 0 && index < ValidationSession.Results.Length)
			{
				if (shiftSelect)
				{
					int from2 = SelectedIndex;
					int to2 = index;
					if (to2 < from2)
					{
						int tmp2 = from2;
						from2 = to2;
						to2 = Mathf.Clamp(tmp2, 0, ValidationSession.Results.Length - 1);
					}
					List<DynamicObjectAddress> toSelect3 = new List<DynamicObjectAddress>();
					for (int j2 = from2; j2 < to2; j2++)
					{
						toSelect3.Add(ValidationSession.Results[j2].Result?.SelectionObjectAddress);
					}
					SelectionUtils.AddToSelection(toSelect3);
				}
				else
				{
					SelectionUtils.SelectInInspector(ValidationSession.Results[index].Result?.SelectionObjectAddress, allowOpenScene: false, ping: false);
				}
			}
			largeGuiCollectionDrawing.ScrollTo(SelectedIndex, ref resultsScrollPos);
			SelectIndexAndRemoveFocusControlAndExitGUI(index);
		}

		private void CollectTimings(ValidationSession.ValidationSessionResult e)
		{
			if (e.Result == null && e.Type == ValidationSession.ValidationSessionResult.ValidationSessionResultType.ResultAddedOrChanged)
			{
				throw new NullReferenceException();
			}
			timings.SampleResult(e.Result);
		}

		private void DrawLeftMenuSelector(Rect rect)
		{
			int height = ValidatorGui.LeftMenuLineHeight - 1;
			Rect sideMenuRect = rect;
			EditorGUI.DrawRect(sideMenuRect.TakeFromRight(1f), ValidatorGui.BorderColor);
			if (SideMenuButton(sideMenuRect.TakeFromBottom(height), MenuVisibility ? SdfIconType.ArrowBarLeft : SdfIconType.ArrowBarRight, selected: false, new GUIContent(MenuVisibility ? "Hide" : "Show")))
			{
				MenuVisibility = !MenuVisibility;
			}
			EditorGUI.DrawRect(sideMenuRect.TakeFromBottom(1f), ValidatorGui.BorderColor);
			foreach (MenuOptions option in Enum.GetValues(typeof(MenuOptions)).Cast<MenuOptions>())
			{
				if (option == MenuOptions.None)
				{
					continue;
				}
				SdfIconType icon = option switch
				{
					MenuOptions.FilterAssets => SdfIconType.Folder, 
					MenuOptions.FilterResults => SdfIconType.Search, 
					MenuOptions.Rules => SdfIconType.CardChecklist, 
					MenuOptions.Events => SdfIconType.CalendarCheck, 
					MenuOptions.Config => SdfIconType.Gear, 
					MenuOptions.Profiler => SdfIconType.MenuButtonWide, 
					MenuOptions.BulkFixing => SdfIconType.UiChecks, 
					MenuOptions.About => SdfIconType.QuestionCircle, 
					_ => throw new NotImplementedException(), 
				};
				GUIContent label = new GUIContent(option.ToString().SplitPascalCase());
				bool selected = SelectedMenu == option;
				if (SideMenuButton(sideMenuRect.TakeFromTop(height), icon, selected && MenuVisibility, label))
				{
					MenuOptions localOptions = option;
					InvokeEndOfRepaint(delegate
					{
						if (SelectedMenu == localOptions)
						{
							MenuVisibility = !MenuVisibility;
						}
						else
						{
							SelectedMenu = localOptions;
							MenuVisibility = true;
						}
					});
				}
				EditorGUI.DrawRect(sideMenuRect.TakeFromTop(1f), ValidatorGui.BorderColor);
			}
			if (SideMenuButton(sideMenuRect.TakeFromTop(height), SdfIconType.EnvelopePlusFill, selected: false, GUIHelper.TempContent("Feedback")))
			{
				OdinFeedbackWindow.Open("Odin Validator");
			}
			static bool SideMenuButton(Rect btnRect, SdfIconType icon2, bool flag, GUIContent content)
			{
				bool isMouseOver = btnRect.Contains(Event.current.mousePosition);
				float nameWidth = labelStyle.CalcSize(content).x;
				float prevWidth = btnRect.width;
				if (isMouseOver)
				{
					btnRect.width += nameWidth + 10f;
				}
				Rect iconRect = btnRect.TakeFromLeft(prevWidth);
				if (flag)
				{
					EditorGUI.DrawRect(btnRect, ValidatorGui.BtnActiveBgColor);
					EditorGUI.DrawRect(iconRect, ValidatorGui.BtnActiveBgColor);
					SdfIcons.DrawIcon(iconRect.Padding(0f, 2f), icon2, Color.white, ValidatorGui.BtnActiveBgColor);
					if (isMouseOver)
					{
						labelStyle.normal.textColor = Color.white;
						GUI.Label(btnRect.AlignRight(nameWidth + 5f), content, labelStyle);
					}
				}
				else if (isMouseOver)
				{
					EditorGUI.DrawRect(iconRect, ValidatorGui.BtnMouseOverBgColor);
					EditorGUI.DrawRect(btnRect, ValidatorGui.BtnMouseOverBgColor);
					labelStyle.normal.textColor = ValidatorGui.BtnMouseOverContentColor;
					GUI.Label(btnRect.AlignRight(nameWidth + 5f), content, labelStyle);
					SdfIcons.DrawIcon(iconRect.Padding(0f, 2f), icon2, ValidatorGui.BtnMouseOverContentColor, ValidatorGui.BtnMouseOverBgColor);
					SirenixEditorGUI.DrawBorders(btnRect.Expand(1f), 0, 1, 1, 1, ValidatorGui.BorderColor);
				}
				else
				{
					EditorGUI.DrawRect(iconRect, ValidatorGui.EditorWindowBgColor);
					SdfIcons.DrawIcon(iconRect.Padding(0f, 2f), icon2, ValidatorGui.BtnContentColor, ValidatorGui.EditorWindowBgColor);
				}
				return GUI.Button(iconRect, GUIContent.none, GUIStyle.none);
			}
		}

		private void DrawLeftMenu(Rect rect)
		{
			if (SelectedMenu != MenuOptions.None && MenuVisibility)
			{
				GUILayout.BeginArea(rect);
				GUIHelper.PushLabelWidth(rect.width * 0.25f);
				scrollPositions[(int)SelectedMenu].Value = EditorGUILayout.BeginScrollView(new Vector2(0f, scrollPositions[(int)SelectedMenu].Value)).y;
				showTip = null;
				switch (SelectedMenu)
				{
				case MenuOptions.FilterResults:
					DrawFilterResults();
					break;
				case MenuOptions.FilterAssets:
					sessionConfigDataDrawer.Draw();
					break;
				case MenuOptions.Rules:
					DrawRules(selectedRuleSrc, rules, ref selectedRuleIndex, ref selectedRuleDataDrawer);
					break;
				case MenuOptions.Events:
					DrawEvents();
					break;
				case MenuOptions.Config:
					DrawConfig();
					break;
				case MenuOptions.Profiler:
					DrawProfiler();
					break;
				case MenuOptions.BulkFixing:
					DrawBulkFixing();
					break;
				case MenuOptions.About:
					DrawAbout();
					break;
				default:
					throw new NotImplementedException();
				case MenuOptions.None:
					break;
				}
				EditorGUILayout.EndScrollView();
				if (showTip != null)
				{
					SirenixEditorGUI.MessageBox(showTip, MessageType.Info, GlobalConfig<GeneralDrawerConfig>.Instance.MessageBoxFontSize);
					GUILayout.Space(5f);
					showTip = null;
				}
				GUIHelper.PopLabelWidth();
				GUILayout.EndArea();
			}
		}

		public static void DrawEvents()
		{
			AutomationConfig events = GlobalConfig<AutomationConfig>.Instance;
			EditorGUI.BeginChangeCheck();
			expandEventsOnPlay.Value = ValidatorGui.FoldoutToggle(expandEventsOnPlay.Value, ref events.OnPlayMode, "On Play");
			if (SirenixEditorGUI.BeginFadeGroup(expandEventsOnPlay, expandEventsOnPlay.Value))
			{
				GUIHelper.PushGUIEnabled(events.OnPlayMode);
				DrawValidationSesssionSelector(GetRect(), events.OnPlayModeSetup.ProfileAsset, delegate(ValidationProfile x)
				{
					events.OnPlayModeSetup.ProfileAsset = x;
					EditorUtility.SetDirty(events);
				});
				events.OnPlayModeIfWarnings = EnumSelector<AutomationConfig.OnPlayModeActions>.DrawEnumField(GetRect(), GUIHelper.TempContent("If Warnings"), events.OnPlayModeIfWarnings);
				events.OnPlayModeIfErrors = EnumSelector<AutomationConfig.OnPlayModeActions>.DrawEnumField(GetRect(), GUIHelper.TempContent("If Errors"), events.OnPlayModeIfErrors);
				events.OnPlayModeAlwaysCompleteValidationFully = EditorGUI.ToggleLeft(GetRect(), "Always Complete Validation", events.OnPlayModeAlwaysCompleteValidationFully);
				events.OnPlayModeFlashScreen = EditorGUI.ToggleLeft(GetRect(), "Flash Screen", events.OnPlayModeFlashScreen);
				GUIHelper.PopGUIEnabled();
				GUILayout.Space(10f);
			}
			SirenixEditorGUI.EndFadeGroup();
			expandEventsOnBuild.Value = ValidatorGui.FoldoutToggle(expandEventsOnBuild.Value, ref events.OnBuild, "On Build");
			if (SirenixEditorGUI.BeginFadeGroup(expandEventsOnBuild, expandEventsOnBuild.Value))
			{
				GUIHelper.PushGUIEnabled(events.OnBuild);
				DrawValidationSesssionSelector(GetRect(), events.OnBuildSetup.ProfileAsset, delegate(ValidationProfile x)
				{
					events.OnBuildSetup.ProfileAsset = x;
					EditorUtility.SetDirty(events);
				});
				events.OnBuildIfWarnings = EnumSelector<AutomationConfig.OnBuildActions>.DrawEnumField(GetRect(), GUIHelper.TempContent("If Warnings"), events.OnBuildIfWarnings);
				events.OnBuildIfErrors = EnumSelector<AutomationConfig.OnBuildActions>.DrawEnumField(GetRect(), GUIHelper.TempContent("If Errors"), events.OnBuildIfErrors);
				events.OnBuildAlwaysCompleteValidationFully = EditorGUI.ToggleLeft(GetRect(), "Always Complete Validation", events.OnBuildAlwaysCompleteValidationFully);
				events.OnBuildFlashScreen = EditorGUI.ToggleLeft(GetRect(), "Flash Screen", events.OnBuildFlashScreen);
				GUIHelper.PopGUIEnabled();
				GUILayout.Space(10f);
			}
			SirenixEditorGUI.EndFadeGroup();
			expandEventsOnProjectStartup.Value = ValidatorGui.FoldoutToggle(expandEventsOnProjectStartup.Value, ref events.OnProjectStartup, "On Project Startup");
			if (SirenixEditorGUI.BeginFadeGroup(expandEventsOnProjectStartup, expandEventsOnProjectStartup.Value))
			{
				GUIHelper.PushGUIEnabled(events.OnProjectStartup);
				DrawValidationSesssionSelector(GetRect(), events.OnProjectStartupSetup.ProfileAsset, delegate(ValidationProfile x)
				{
					events.OnProjectStartupSetup.ProfileAsset = x;
					EditorUtility.SetDirty(events);
				});
				events.OnProjectStartupIfWarnings = EnumSelector<AutomationConfig.OnProjectStartupActions>.DrawEnumField(GetRect(), new GUIContent("If Warnings"), events.OnProjectStartupIfWarnings);
				events.OnProjectStartupIfErrors = EnumSelector<AutomationConfig.OnProjectStartupActions>.DrawEnumField(GetRect(), new GUIContent("If Errors"), events.OnProjectStartupIfErrors);
				events.OnProjectStartupAlwaysCompleteValidationFully = EditorGUI.ToggleLeft(GetRect(), "Always Complete Validation", events.OnProjectStartupAlwaysCompleteValidationFully);
				events.OnProjectStartupFlashScreen = EditorGUI.ToggleLeft(GetRect(), "Flash Screen", events.OnProjectStartupFlashScreen);
				GUIHelper.PopGUIEnabled();
				GUILayout.Space(10f);
			}
			SirenixEditorGUI.EndFadeGroup();
			if (EditorGUI.EndChangeCheck())
			{
				EditorUtility.SetDirty(events);
			}
		}

		private void DrawAbout()
		{
			if (validatorLogo == null)
			{
				if (EditorGUIUtility.isProSkin)
				{
					validatorLogo = OdinEditorResources.OdinValidatorLogo;
				}
				else
				{
					validatorLogo = OdinEditorResources.OdinValidatorLogoBlack;
				}
			}
			GUITextureDrawingUtil.DrawTexture(GUILayoutUtility.GetRect(0f, 100f).Padding(20f, 20f, 20f, 0f), validatorLogo, ScaleMode.ScaleToFit, Color.white, default(Color), 0f);
			GUILayout.BeginVertical(Padding);
			GUILayout.Label("Developed and published by Sirenix", SirenixGUIStyles.CenteredGreyMiniLabel);
			GUILayout.Label("Version " + OdinInspectorVersion.Version, SirenixGUIStyles.CenteredGreyMiniLabel);
			GUILayout.BeginHorizontal();
			if (GUILayout.Button("Tutorials"))
			{
				Application.OpenURL("https://odininspector.com/tutorials#odin-validator");
			}
			if (GUILayout.Button("Issue Tracker"))
			{
				Application.OpenURL("https://bitbucket.org/sirenix/odin-inspector/issues");
			}
			if (GUILayout.Button("Support"))
			{
				Application.OpenURL("https://discord.gg/WTYJEra");
			}
			GUILayout.EndHorizontal();
			GUILayout.EndVertical();
		}

		private void DrawFilterResults()
		{
			DrawFilteredList(ValidationSession.Results.SceneFilters, filterToggleScenes, "Locations");
			DrawFilteredList(ValidationSession.Results.ObjectTypeFilters, filterToggleObjectTypes, "Types");
			DrawFilteredList(ValidationSession.Results.ValidatorTypeFilters, filterToggleValidatorTypes, "Validators");
			GUILayout.FlexibleSpace();
		}

		private void DrawFilteredList<T>(ValidationSessionResultCollector.Filter<T> collection, EditorPrefBool foldoutToggle, string name, bool isRadio = false)
		{
			if (collection.OrderedItems.Count == 0)
			{
				return;
			}
			if (isRadio)
			{
				ValidatorGui.Header(name);
			}
			else
			{
				foldoutToggle.Value = ValidatorGui.Foldout(foldoutToggle.Value, name, (!isRadio) ? 21 : 0);
			}
			Rect toggleRect = GUILayoutUtility.GetLastRect().AlignLeft(21f);
			toggleRect.x += ValidatorGui.ContentPadding;
			List<ValidationSessionResultCollector.Filter<T>.FilteredItem> items = collection.OrderedItems;
			if (!isRadio)
			{
				bool isMixed = false;
				bool isAllToggled = true;
				if (Event.current.type != EventType.Layout && collection.OrderedItems.Count > 0)
				{
					bool firstIsToggled = collection.OrderedItems[0].Enabled;
					for (int i = 0; i < items.Count; i++)
					{
						bool isToggled = items[i].Enabled;
						if (isToggled != firstIsToggled)
						{
							isMixed = true;
						}
						if (!isToggled)
						{
							isAllToggled = false;
						}
					}
				}
				EditorGUI.BeginChangeCheck();
				EditorGUI.showMixedValue = isMixed;
				bool newToggleAllState = EditorGUI.ToggleLeft(toggleRect, GUIContent.none, isAllToggled);
				EditorGUI.showMixedValue = false;
				if (EditorGUI.EndChangeCheck())
				{
					InvokeEndOfRepaint(delegate
					{
						selectionToRecover = SelectedResult;
						foreach (ValidationSessionResultCollector.Filter<T>.FilteredItem current in items)
						{
							current.Enabled = newToggleAllState;
						}
						ValidationSession.Results.MarkFiltersDirty();
					});
				}
			}
			if (SirenixEditorGUI.BeginFadeGroup(foldoutToggle, foldoutToggle.Value))
			{
				GUIContent countContent = GUIHelper.TempContent("");
				GUIStyle labelStyle = SirenixGUIStyles.Label;
				GUIStyle toggleStyle = (isRadio ? EditorStyles.radioButton : EditorStyles.toggle);
				for (int i2 = 0; i2 < items.Count; i2++)
				{
					ValidationSessionResultCollector.Filter<T>.FilteredItem item = items[i2];
					Rect rect = GUILayoutUtility.GetRect(0f, 21f).AlignCenterY(EditorGUIUtility.singleLineHeight);
					if (Event.current.type != EventType.Layout && Event.current.type != EventType.Repaint && Event.current.type == EventType.MouseDown && rect.Contains(Event.current.mousePosition))
					{
						InvokeEndOfRepaint(delegate
						{
							selectionToRecover = SelectedResult;
							if (isRadio)
							{
								foreach (ValidationSessionResultCollector.Filter<T>.FilteredItem current in items)
								{
									current.Enabled = false;
								}
								item.Enabled = true;
							}
							else
							{
								item.Enabled = !item.Enabled;
							}
							ValidationSession.Results.MarkFiltersDirty();
						});
						Event.current.Use();
					}
					if (Event.current.type != EventType.Repaint)
					{
						continue;
					}
					bool highlightIcon;
					(Texture, SdfIconType) icon = GetFilterItemIcon(item, out highlightIcon);
					bool hasIcon = icon.Item1 != null || icon.Item2 != SdfIconType.None;
					rect.TakeFromLeft(ValidatorGui.ContentPadding);
					Rect contentRect = rect;
					toggleRect = contentRect.TakeFromLeft(contentRect.height);
					countContent.text = item.Count.ToString();
					float countRectWidth = labelStyle.CalcSize(countContent).x;
					Rect countRect = contentRect.TakeFromRight(countRectWidth + ValidatorGui.ContentPadding).AlignLeft(countRectWidth);
					Rect iconRect = contentRect.TakeFromLeft(hasIcon ? contentRect.height : 0f).AlignCenter(16f, 16f);
					Rect labelRect = contentRect;
					toggleStyle.Draw(toggleRect, GUIContent.none, rect.Contains(Event.current.mousePosition), isActive: false, item.Enabled, hasKeyboardFocus: false);
					if (!item.Enabled)
					{
						GUIHelper.PushColor(new Color(1f, 1f, 1f, 0.4f));
					}
					GUI.Label(countRect, countContent, labelStyle);
					GUI.Label(labelRect, item.Name, labelStyle);
					if (hasIcon)
					{
						if ((bool)icon.Item1)
						{
							Color prev = GUI.color;
							GUI.color = (highlightIcon ? new Color(1f, 1f, 1f, 1f) : new Color(1f, 1f, 1f, 0.4f));
							GUI.DrawTexture(iconRect, icon.Item1);
							GUI.color = prev;
						}
						else
						{
							SdfIcons.DrawIcon(iconRect, icon.Item2);
						}
					}
					if (!item.Enabled)
					{
						GUIHelper.PopColor();
					}
				}
				GUILayout.Space(10f);
			}
			SirenixEditorGUI.EndFadeGroup();
		}

		internal static void DrawConfig()
		{
			Rect toolbarRect = GUILayoutUtility.GetRect(0f, 21f);
			EditorGUI.DrawRect(toolbarRect.TakeFromBottom(1f), ValidatorGui.BorderColor);
			Rect projectRect = toolbarRect.TakeFromLeft(toolbarRect.width * 0.5f);
			EditorGUI.DrawRect(toolbarRect.TakeFromLeft(1f), ValidatorGui.BorderColor);
			Rect localRect = toolbarRect;
			if (ValidatorGui.ToolbarBtn(projectRect, !showConfigLocalOverride, SdfIconType.PeopleFill, "For everyone", "Settings shared with your team"))
			{
				showConfigLocalOverride.Value = false;
			}
			if (ValidatorGui.ToolbarBtn(localRect, showConfigLocalOverride, SdfIconType.DisplayFill, "For this machine", "Settings local only to this machine"))
			{
				showConfigLocalOverride.Value = true;
			}
			bool localOverride = showConfigLocalOverride.Value;
			expandGeneralSettings.Value = ValidatorGui.Foldout(expandGeneralSettings.Value, "Background validation  ");
			if (SirenixEditorGUI.BeginFadeGroup(expandGeneralSettings, expandGeneralSettings.Value))
			{
				DrawBackgroundValidationConfig(localOverride, drawProfileSelector: true, allowBold: true);
			}
			SirenixEditorGUI.EndFadeGroup();
			expandWidgetSettings.Value = ValidatorGui.Foldout(expandWidgetSettings.Value, "Scene Widget");
			if (SirenixEditorGUI.BeginFadeGroup(expandWidgetSettings, expandWidgetSettings.Value))
			{
				EditorGUI.BeginChangeCheck();
				GlobalConfig<GlobalValidationConfig>.Instance.ShowWidget.Draw(GetRect(), "Shown", null, localOverride);
				GUIHelper.PushGUIEnabled(GlobalConfig<GlobalValidationConfig>.Instance.ShowWidget.Value);
				GlobalConfig<GlobalValidationConfig>.Instance.ShowWidgetOnlyWhenErrorOrWarnings.Draw(GetRect(), "Only show widget when results are found", null, localOverride);
				GlobalValidationConfig.WidgetAnchor.Value = EnumSelector<SceneValidationWidget.WidgetAnchor>.DrawEnumField(GetRect(), new GUIContent("Anchor"), GlobalValidationConfig.WidgetAnchor.Value);
				GlobalValidationConfig.WidgetOffsetX.Value = EditorGUI.FloatField(GetRect(), new GUIContent("Offset X"), GlobalValidationConfig.WidgetOffsetX.Value);
				GlobalValidationConfig.WidgetOffsetY.Value = EditorGUI.FloatField(GetRect(), new GUIContent("Offset Y"), GlobalValidationConfig.WidgetOffsetY.Value);
				GUIHelper.PopGUIEnabled();
				if (EditorGUI.EndChangeCheck())
				{
					SceneView.RepaintAll();
				}
			}
			SirenixEditorGUI.EndFadeGroup();
			expandWindowSettings.Value = ValidatorGui.Foldout(expandWindowSettings.Value, "Validator Window");
			if (SirenixEditorGUI.BeginFadeGroup(expandWindowSettings, expandWindowSettings.Value))
			{
				GlobalConfig<GlobalValidationConfig>.Instance.ContinuouslyValidateVisibleIssues.Draw(GetRect(), "Continuously validate visible issues", "Issues visible in the issue list will be continuously revalidated all the time.", localOverride);
				GlobalConfig<GlobalValidationConfig>.Instance.PingOnDoubleClick.Draw(GetRect(), "Ping Object On Double Click", "Ping objects when they are double-clicked in the result view", localOverride);
				GlobalConfig<GlobalValidationConfig>.Instance.FocusObjectOnDoubleClick.Draw(GetRect(), "Frame Object On Double Click", "Move to the camera to scene objects when they are double-clicked in the result view", localOverride);
				GlobalConfig<GlobalValidationConfig>.Instance.FrameSelection.Draw(GetRect(), "Frame Selection", "Move the camera to view all scene objects that are selected in the result view", localOverride);
				GlobalConfig<GlobalValidationConfig>.Instance.SelectNextIssueOnFix.Draw(GetRect(), "Select Next Issue On Fix", "Select the next issue in the result view when an issue is fixed", localOverride);
				GlobalConfig<GlobalValidationConfig>.Instance.OpenComponentInInspectorAndCloseOthers.Draw(GetRect(), "Open Component In Inspector And Close Others", "Open the component in the inspector containing the error selected in the result view, and close all other components. NOTE: this setting may cause lag when selecting results in some versions of Unity", localOverride);
				GlobalConfig<GlobalValidationConfig>.Instance.DebugMode.Draw(GetRect(), "Debug Mode", "Shows advanced info data and enables error reporting during fixes.", localOverride);
			}
			SirenixEditorGUI.EndFadeGroup();
			expandAdvancedSettings.Value = ValidatorGui.Foldout(expandAdvancedSettings.Value, "Advanced");
			if (SirenixEditorGUI.BeginFadeGroup(expandAdvancedSettings, expandAdvancedSettings.Value))
			{
				EditorGUI.BeginChangeCheck();
				GlobalConfig<GlobalValidationConfig>.Instance.DeepValidation.Draw(GetRect(), "Deep validations", "Validate all components and assets deeply. When this is off, only components and assets drawn by Odin will be validated deeply, such that their members are scanned, and all other components and assets are only validated directly on the root objects, without their members being scanned. Enabling this will *significantly* increase validation times and is very unlikely to catch more issues.", localOverride);
				if (EditorGUI.EndChangeCheck())
				{
					ValidationRunnerConfig.Default.DeepValidation = GlobalConfig<GlobalValidationConfig>.Instance.DeepValidation.Value;
				}
				GlobalConfig<GlobalValidationConfig>.Instance.PauseValidationWhileWorkingInSceneView.Draw(GetRect(), "Pause Validation While Working In Scene View", "Validation will be paused while you are actively working in the scene view, to prevent perceptible stutter during user input.", localOverride);
				bool watchForChanges = GlobalConfig<GlobalValidationConfig>.Instance.WatchForChanges.Value;
				bool validatingBackground = GlobalConfig<GlobalValidationConfig>.Instance.ValidateInBackground.Value;
				string msg = "Instead of turning off either of the settings below, a better way to disable Odin Validator is to toggle the setting for \"Background Validation > Keep main validation session alive\" off. This will ensure that the watching for changes, and performing validation in the background only runs when a validation window is open. \n\nWithout the two settings below enabled, the validator will not be able to detect when issues have been resolved.";
				if (!watchForChanges || !validatingBackground)
				{
					SirenixEditorGUI.MessageBox(msg, MessageType.Warning, GlobalConfig<GeneralDrawerConfig>.Instance.MessageBoxFontSize);
				}
				GlobalConfig<GlobalValidationConfig>.Instance.WatchForChanges.Draw(GetRect(), "Watch for changes", msg, localOverride);
				GlobalConfig<GlobalValidationConfig>.Instance.ValidateInBackground.Draw(GetRect(), "Validate in background", msg, localOverride);
				if (watchForChanges != GlobalConfig<GlobalValidationConfig>.Instance.WatchForChanges.Value || validatingBackground != GlobalConfig<GlobalValidationConfig>.Instance.ValidateInBackground.Value)
				{
					watchForChanges = GlobalConfig<GlobalValidationConfig>.Instance.WatchForChanges.Value;
					validatingBackground = GlobalConfig<GlobalValidationConfig>.Instance.ValidateInBackground.Value;
					foreach (ValidationSession item in ValidationSession.ActiveValidationSessions)
					{
						if (item.IsWatching != watchForChanges || item.IsValidatingInBackground != validatingBackground)
						{
							item.StartSession(watchForChanges, validatingBackground);
							item.StopSession(!watchForChanges, !validatingBackground);
						}
					}
				}
			}
			SirenixEditorGUI.EndFadeGroup();
		}

		public static void DrawBackgroundValidationConfig(bool localOverride, bool drawProfileSelector, bool allowBold)
		{
			GlobalConfig<GlobalValidationConfig>.Instance.KeepMainValidationSessionAliveInBackground.Draw(GetRect(), "Keep main profile alive in background", "Disable this if you want the validator not to validate any changes while the validator window is closed.", localOverride, allowBold);
			if (drawProfileSelector)
			{
				GUIHelper.PushGUIEnabled(GlobalConfig<GlobalValidationConfig>.Instance.KeepMainValidationSessionAliveInBackground.Value && !localOverride && GUI.enabled);
				Rect r = GetRect();
				r.xMin += EditorStyles.toggle.padding.left;
				GUI.Label(r.TakeFromLeft(EditorGUIUtility.labelWidth), "Main Profile");
				DrawValidationSesssionSelector(r, ValidationProfile.MainValidationProfile, delegate(ValidationProfile x)
				{
					ValidationProfile.MainValidationProfile = x;
				});
				GUIHelper.PopGUIEnabled();
			}
			GUIHelper.PushGUIEnabled(GlobalConfig<GlobalValidationConfig>.Instance.KeepMainValidationSessionAliveInBackground);
			GlobalConfig<GlobalValidationConfig>.Instance.ValidateScenesOnSceneLoad.Draw(GetRect(), "Validate scenes from active profile on scene load", null, localOverride, allowBold);
			GlobalConfig<GlobalValidationConfig>.Instance.PopulateQueueOnGameObjectDeleted.Draw(GetRect(), "Revalidate scene on gameobject destroyed", null, localOverride, allowBold);
			GlobalConfig<GlobalValidationConfig>.Instance.QueueAssetsOnLoad.Draw(GetRect(), "Validate assets from active profile on load", "Warning: In order to validate an asset, it has to be loaded first. Some assets might take a while to load, and asset loading cannot be asynchronous, so depending on your project, this option may cause Unity to stutter at times as individual large assets are loaded, especially if all assets in your project are not fully imported yet.\r\n\r\nOdin Validator does its best to pause background validation while the camera is being moved in the scene view and other user actions requiring smooth framerates are being performed, but it can't detect everything, so your user experience may be impacted while assets are validated in the background.", localOverride, allowBold);
			GlobalConfig<GlobalValidationConfig>.Instance.PopulateQueueOnAssetDeleted.Draw(GetRect(), "Revalidate active profile on asset deleted", null, localOverride, allowBold);
			GUIHelper.PopGUIEnabled();
			GlobalConfig<GlobalValidationConfig>.Instance.ValidateMainProfileOnLoad.LocalOverride = GlobalConfig<GlobalValidationConfig>.Instance.QueueAssetsOnLoad;
			GlobalConfig<GlobalValidationConfig>.Instance.ValidateMainProfileOnLoad.Value = GlobalConfig<GlobalValidationConfig>.Instance.QueueAssetsOnLoad.Value;
		}

		private static Rect GetRect()
		{
			return GUILayoutUtility.GetRect(0f, 21f).AlignCenterY(EditorGUIUtility.singleLineHeight).HorizontalPadding(ValidatorGui.ContentPadding);
		}

		private void DrawProfiler()
		{
			GUIHelper.RequestRepaint();
			List<BackgroundTaskHandle> tasks = BackgroundTaskRunner.AllTasks;
			float total = BackgroundTaskRunner.TotalWorkLastTickMs;
			int max = 6;
			float tMax = Mathf.Clamp01(total / (float)max);
			float tUser = total / (float)BackgroundTaskRunner.MaxBackgroundTaskMSPerFrame;
			expandProfilerTickTime.Value = ValidatorGui.Foldout(expandProfilerTickTime.Value, "Max tick time");
			if (SirenixEditorGUI.BeginFadeGroup(expandProfilerTickTime, expandProfilerTickTime.Value))
			{
				GUILayout.BeginVertical(Padding);
				SirenixEditorGUI.MessageBox("Odin Validator is using a slice of CPU time every frame when validation is toggled on. Here you can monitor how much time it is spending every frame, and adjust how much time it is allowed to spend.");
				GUILayout.Space(10f);
				Rect slideRect = GUILayoutUtility.GetRect(0f, 21f);
				EditorGUI.DrawRect(slideRect, ValidatorGui.BorderColor);
				EditorGUI.DrawRect(slideRect.AlignLeft(slideRect.width * tMax).Padding(1f), ValidatorGui.Green);
				tMax = BackgroundTaskRunner.MaxBackgroundTaskMSPerFrame.Value / (float)max;
				Rect r = slideRect.AlignLeft(slideRect.width * tMax).AlignRight(2f);
				EditorGUI.DrawRect(r, Color.white);
				EditorGUI.DrawRect(r.AlignTop(1f).AlignRight(5f), Color.white);
				EditorGUI.DrawRect(r.AlignBottom(1f).AlignRight(5f), Color.white);
				GUI.Label(r.AddX(7f).AlignLeft(200f), "Max " + BackgroundTaskRunner.MaxBackgroundTaskMSPerFrame.Value.ToString("0.00") + " ms", SirenixGUIStyles.LeftAlignedWhiteMiniLabel);
				GUIHelper.PushColor(new Color(1f, 1f, 1f, 0.8f));
				GUI.Label(slideRect.HorizontalPadding(0f, 10f), total.ToString("0.00") + " ms", SirenixGUIStyles.LeftAlignedWhiteMiniLabel);
				GUIHelper.PopColor();
				BackgroundTaskRunner.MaxBackgroundTaskMSPerFrame.Value = ValidatorGui.SlideRect(slideRect, BackgroundTaskRunner.MaxBackgroundTaskMSPerFrame.Value, 0.2f, max);
				GUILayout.EndVertical();
			}
			SirenixEditorGUI.EndFadeGroup();
			expandProfilerBreakdown.Value = ValidatorGui.Foldout(expandProfilerBreakdown.Value, "Breakdown");
			if (SirenixEditorGUI.BeginFadeGroup(expandProfilerBreakdown, expandProfilerBreakdown.Value))
			{
				float mul = Mathf.Clamp(tUser, 0f, 1f);
				normalize = EditorGUI.ToggleLeft(GetRect(), "Normalize", normalize);
				if (normalize)
				{
					mul = 1f;
				}
				GUILayout.BeginVertical(Padding);
				for (int i = 0; i < tasks.Count; i++)
				{
					if (i != 0)
					{
						GUILayout.Space(10f);
					}
					BackgroundTaskHandle item = tasks[i];
					BackgroundTaskHandle.State state = item.CurrentState;
					Color backColor = ValidatorGui.BorderColor;
					Color forColor = state switch
					{
						BackgroundTaskHandle.State.Paused => Color.grey, 
						BackgroundTaskHandle.State.Running => ValidatorGui.DarkRed, 
						BackgroundTaskHandle.State.Killed => Color.red, 
						BackgroundTaskHandle.State.Relaxed => ValidatorGui.BtnActiveBgColor, 
						_ => Color.white, 
					};
					Rect rect = GUILayoutUtility.GetRect(0f, 21f);
					Rect labelRect = rect;
					Rect progressBarRect = rect;
					if (state == BackgroundTaskHandle.State.Paused || state == BackgroundTaskHandle.State.Killed)
					{
						backColor.a *= 0.5f;
						EditorGUI.DrawRect(progressBarRect, backColor);
					}
					else
					{
						EditorGUI.DrawRect(progressBarRect, backColor);
						EditorGUI.DrawRect(progressBarRect.Padding(1f).AlignLeft(progressBarRect.width * (float)item.WorkWeight * mul), forColor);
					}
					GUI.Label(labelRect, item.Name, SirenixGUIStyles.MiniLabelCentered);
				}
				GUILayout.EndVertical();
			}
			SirenixEditorGUI.EndFadeGroup();
			expandProfilerCurrentSession.Value = ValidatorGui.Foldout(expandProfilerCurrentSession.Value, "Current session");
			if (SirenixEditorGUI.BeginFadeGroup(expandProfilerCurrentSession, expandProfilerCurrentSession.Value))
			{
				GUILayout.BeginVertical(Padding);
				EditorGUILayout.LabelField("Name", ValidationSession.Name);
				EditorGUILayout.LabelField("Is Watching", ValidationSession.IsWatching.ToString() ?? "");
				EditorGUILayout.LabelField("Is Validating", ValidationSession.IsValidatingInBackground.ToString() ?? "");
				EditorGUILayout.LabelField("Work queue", ValidationSession.WorkQueue.Count.ToString() ?? "");
				EditorGUILayout.LabelField("Work done", ValidationSession.WorkDone.Count.ToString() ?? "");
				EditorGUILayout.LabelField("Remaining work", ValidationSession.remainingWorkCountSample.ToString() ?? "");
				EditorGUILayout.LabelField("Work done", ValidationSession.remainingWorkCountSample.ToString() ?? "");
				ValidationWorkItem prevWorkItem = ValidationSession.prevProcessedWorkItem;
				ValidationWorkItem? currWorkItem = ValidationSession.currentlyProcessingWorkItem;
				ValidationWorkItem nextWorkItem = ((ValidationSession.WorkQueue.Count > 0) ? ValidationSession.WorkQueue.PeekMaybeInvalid() : default(ValidationWorkItem));
				DrawWorkItem(prevWorkItem, "Previous Work Item");
				DrawWorkItem(currWorkItem.HasValue ? currWorkItem.Value : default(ValidationWorkItem), "Current Work Item");
				DrawWorkItem(nextWorkItem, "Next Work Item");
				GUILayout.EndVertical();
			}
			SirenixEditorGUI.EndFadeGroup();
			expandProfilerAssetLoadTimings.Value = ValidatorGui.Foldout(expandProfilerAssetLoadTimings.Value, "Asset Load Timings");
			if (SirenixEditorGUI.BeginFadeGroup(expandProfilerAssetLoadTimings, expandProfilerAssetLoadTimings.Value))
			{
				GUILayout.BeginVertical(Padding);
				GUILayout.Label("Every time Odin Validator asks Unity to load an asset, it measures how long the asset loading operation takes. The most expensive asset load operations are recorded and displayed here. If you find that the Validator is causing the editor to stutter, asset loading is a likely culprit and you can find the offending assets here and then exclude them from the active profile if suitable.", SirenixGUIStyles.MultiLineLabel);
				GUILayout.BeginHorizontal();
				if (GUILayout.Button("Clear"))
				{
					AssetLoadTimings.Timings.Clear();
					GUIHelper.ExitGUI(removeFocusControl: false);
				}
				if (GUILayout.Button("Open Asset Filter Breakdown"))
				{
					AssetFilterBreakdownEditor.ShowBreadownWindow(ValidationSession.Config.SessionData);
				}
				GUILayout.EndHorizontal();
				if (Event.current.type == EventType.Layout)
				{
					profilerAssetLoadTimingsCount = AssetLoadTimings.Timings.Count;
				}
				Rect timingsRect = GUILayoutUtility.GetRect(0f, 21 * (profilerAssetLoadTimingsCount + 1));
				GUILayout.EndVertical();
				Rect rect2 = timingsRect.AlignTop(21f);
				Rect labelRect2 = rect2.SubXMax(150f).HorizontalPadding(5f);
				Rect timeRect = rect2.SubXMax(70f).AlignRight(80f);
				Rect dateRect = rect2.AlignRight(70f);
				EditorGUI.DrawRect(rect2, ValidatorGui.ToolbarBgColor);
				SirenixEditorGUI.DrawBorders(rect2, 0, 0, 0, 1);
				GUI.Label(labelRect2, "Asset Path", SirenixGUIStyles.Label);
				GUI.Label(timeRect, "Time Taken", SirenixGUIStyles.Label);
				GUI.Label(dateRect, "Load Date", SirenixGUIStyles.Label);
				rect2 = timingsRect.AlignTop(21f);
				labelRect2 = rect2.SubXMax(170f).HorizontalPadding(5f);
				timeRect = rect2.SubXMax(70f).AlignRight(80f);
				dateRect = rect2.AlignRight(70f);
				for (int j = 0; j < profilerAssetLoadTimingsCount && AssetLoadTimings.Timings.Count > j; j++)
				{
					AssetLoadTimings.LoadTimeData timing = AssetLoadTimings.Timings[j];
					rect2.y += 21f;
					labelRect2.y += 21f;
					timeRect.y += 21f;
					dateRect.y += 21f;
					Rect copyIconRect = labelRect2.AlignRight(25f).AddX(25f);
					if (SirenixEditorGUI.SDFIconButton(copyIconRect.Padding(3f), GUIHelper.TempContent("Copy to clipboard", "Copy to clipboard"), SdfIconType.Files, IconAlignment.RightEdge, SirenixGUIStyles.IconButton))
					{
						EditorGUIUtility.systemCopyBuffer = timing.AssetPath;
					}
					if (GUI.Button(labelRect2.AddXMax(5f), GUIHelper.TempContent(timing.AssetPath, timing.AssetPath), labelRect2.Contains(Event.current.mousePosition) ? SirenixGUIStyles.HighlightedLabel : SirenixGUIStyles.Label))
					{
						UnityEngine.Object obj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(timing.AssetPath);
						EditorGUIUtility.PingObject(obj);
					}
					GUI.Label(timeRect, timing.LoadMs.ToString("0.000") + " ms", SirenixGUIStyles.Label);
					GUI.Label(dateRect, timing.LoadDate.ToString("HH:mm:ss"), SirenixGUIStyles.Label);
				}
				SirenixEditorGUI.DrawBorders(timingsRect.SetX(timeRect.x - 3f).SetWidth(timeRect.width), 1, 1, 0, 0);
				SirenixEditorGUI.DrawBorders(timingsRect, 1);
			}
			SirenixEditorGUI.EndFadeGroup();
			expandProfilerEvents.Value = ValidatorGui.Foldout(expandProfilerEvents.Value, "Project Watcher");
			if (SirenixEditorGUI.BeginFadeGroup(expandProfilerEvents, expandProfilerEvents.Value))
			{
				GUILayout.BeginVertical(Padding);
				for (int k = 0; k < ProjectWatcher.latestEvents.Length; k++)
				{
					ProjectEvent e = ProjectWatcher.latestEvents[k];
					Rect titlesRect = GUILayoutUtility.GetRect(0f, 21f);
					string[] obj2 = new string[5]
					{
						(ProjectWatcher.latestEvents.Position - k).ToString(),
						" : ",
						e.Type.ToString(),
						" : ",
						null
					};
					ProjectEvent projectEvent = e;
					obj2[4] = projectEvent.ToString();
					GUI.Label(titlesRect, string.Concat(obj2));
				}
				GUILayout.EndVertical();
			}
			SirenixEditorGUI.EndFadeGroup();
		}

		private static void DrawWorkItem(ValidationWorkItem workItem, string label)
		{
			bool isValid = workItem.IsValid();
			EditorGUILayout.LabelField(label, isValid ? workItem.ToNiceString() : "-");
			if (!isValid)
			{
				GUIHelper.PushGUIEnabled(enabled: false);
			}
			EditorGUILayout.LabelField("   Asset Guid", string.IsNullOrEmpty(workItem.AssetGuid) ? "-" : workItem.AssetGuid);
			EditorGUILayout.LabelField("   Asset Path", string.IsNullOrEmpty(workItem.AssetGuid) ? "-" : AssetDatabase.GUIDToAssetPath(workItem.AssetGuid));
			EditorGUILayout.LabelField("   Entity Id", (!workItem.EntityId.IsValid) ? "-" : workItem.EntityId.ToString());
			EditorGUILayout.LabelField("   Non Unity Object:", (workItem.NonUnityObjectValue == null) ? "-" : "Provided");
			EditorGUILayout.LabelField("   Source:", workItem.Source.ToString());
			EditorGUILayout.LabelField("   Scene Content", workItem.SceneContent.HasValue ? workItem.SceneContent.Value.Name : "-");
			EditorGUILayout.LabelField("   Scene Validators", workItem.SceneValidators.HasValue ? workItem.SceneValidators.Value.Name : "-");
			if (!isValid)
			{
				GUIHelper.PopGUIEnabled();
			}
		}

		internal static void DrawRules(EditorPrefEnum<ConfigSourceType> selectedRuleSrc, RuleDataWrapper rules, ref int selectedRuleIndex, ref PropertyTree ruleDataDrawerTree)
		{
			Rect toolbarRect = GUILayoutUtility.GetRect(0f, 21f);
			EditorGUI.DrawRect(toolbarRect.TakeFromBottom(1f), ValidatorGui.BorderColor);
			Rect projectRect = toolbarRect.TakeFromLeft(toolbarRect.width * 0.5f);
			EditorGUI.DrawRect(toolbarRect.TakeFromLeft(1f), ValidatorGui.BorderColor);
			Rect localRect = toolbarRect;
			if (ValidatorGui.ToolbarBtn(projectRect, selectedRuleSrc.Value == ConfigSourceType.Project, SdfIconType.PeopleFill, "For everyone", "Rules shared with your team"))
			{
				selectedRuleSrc.Value = ConfigSourceType.Project;
			}
			if (ValidatorGui.ToolbarBtn(localRect, selectedRuleSrc.Value == ConfigSourceType.Local, SdfIconType.DisplayFill, "For this machine", "Rules local only for you"))
			{
				selectedRuleSrc.Value = ConfigSourceType.Local;
			}
			Rect toolbarRect2 = GUILayoutUtility.GetRect(0f, 21f);
			Rect addRect = toolbarRect2.TakeFromRight(toolbarRect2.height);
			toolbarRect2.TakeFromRight(1f);
			Rect exRect = toolbarRect2.TakeFromRight(toolbarRect2.height);
			toolbarRect2.TakeFromRight(1f);
			ValidatorGui.Header(toolbarRect2, "All rules", null);
			if (DrawToolbarButton(addRect, SdfIconType.Plus, "Create new custom rule"))
			{
				GenericMenu menu = new GenericMenu();
				ValidatorScriptTemplates.AddRuleTemplateScriptCreationToGenericMenu(menu);
				menu.ShowAsContext();
			}
			if (DrawToolbarButton(exRect, SdfIconType.ArrowCounterclockwise, "Reset to default"))
			{
				if (EditorUtility.DisplayDialog("Reset rules?", "This will reset all your rules to their default settings. All current rule data config will be lost.", "Ok", "Cancel"))
				{
					CombinedRuleInstance[] array = rules.Rules;
					foreach (CombinedRuleInstance item in array)
					{
						if (selectedRuleSrc.Value == ConfigSourceType.Project)
						{
							item.Project = null;
						}
						if (selectedRuleSrc.Value == ConfigSourceType.Local)
						{
							item.Local = null;
						}
					}
					rules.SaveChanges();
				}
				GUIHelper.ExitGUI(removeFocusControl: true);
			}
			CombinedRuleInstance[] allRules = rules.Rules;
			EditorGUI.BeginChangeCheck();
			for (int j = 0; j < allRules.Length; j++)
			{
				CombinedRuleInstance rule = allRules[j];
				Rect itemRect = GUILayoutUtility.GetRect(0f, 21f).AlignCenterY(EditorGUIUtility.singleLineHeight);
				bool isMouseOver = itemRect.Contains(Event.current.mousePosition);
				bool isEnabledOverriddenInCurrent = rule.IsEnabledOverriddenIn(selectedRuleSrc.Value);
				bool overriddenInCurrent = rule.EnabledOverrideState == selectedRuleSrc.Value;
				bool isCurrentProjectAndEnabledOverridenInLocal = selectedRuleSrc.Value == ConfigSourceType.Project && rule.EnabledOverrideState == ConfigSourceType.Local;
				SerializedRule serializedRule = rule.GetSerializedRule(selectedRuleSrc.Value);
				bool hasDataOverride = rule.IsDataOverriddenIn(selectedRuleSrc.Value) || isEnabledOverriddenInCurrent;
				bool isSelected = selectedRuleIndex == j;
				Color mouseOverColor = ValidatorGui.BtnMouseOverBgColor;
				Color selectedColor = ValidatorGui.BtnActiveBgColor;
				if (isSelected || isMouseOver)
				{
					EditorGUI.DrawRect(itemRect, isSelected ? selectedColor : mouseOverColor);
				}
				itemRect.TakeFromLeft(ValidatorGui.ContentPadding);
				itemRect.TakeFromRight(ValidatorGui.ContentPadding);
				GUIHelper.PushColor(new Color(1f, 1f, 1f, isCurrentProjectAndEnabledOverridenInLocal ? 0.5f : 1f));
				Rect toggleRect = itemRect.TakeFromLeft(itemRect.height);
				Rect labelRect = itemRect;
				GUIStyle labelStyle = (isSelected ? SirenixGUIStyles.WhiteLabel : SirenixGUIStyles.Label);
				FontStyle prevFontStyle = labelStyle.fontStyle;
				labelStyle.fontStyle = (hasDataOverride ? FontStyle.Bold : FontStyle.Normal);
				bool clicked = GUI.Button(labelRect, GUIHelper.TempContent(rule.Name), labelStyle);
				labelStyle.fontStyle = prevFontStyle;
				if (clicked)
				{
					if (Event.current.button == 1)
					{
						CombinedRuleInstance localRule = rule;
						GenericMenu menu2 = new GenericMenu();
						if (isEnabledOverriddenInCurrent)
						{
							menu2.AddItem(new GUIContent("Reset enabled"), on: false, delegate
							{
								serializedRule.DataOverride = null;
								serializedRule.EnabledOverridden = false;
							});
						}
						else
						{
							menu2.AddDisabledItem(new GUIContent("Reset enabled"));
						}
						if (serializedRule?.DataOverride != null)
						{
							menu2.AddItem(new GUIContent("Reset config"), on: false, delegate
							{
								serializedRule.DataOverride = null;
							});
						}
						else
						{
							menu2.AddDisabledItem(new GUIContent("Reset config"));
						}
						menu2.ShowAsContext();
					}
					else if (selectedRuleIndex == j)
					{
						selectedRuleIndex = -1;
					}
					else
					{
						selectedRuleIndex = j;
						ruleDataDrawerTree?.Dispose();
						ruleDataDrawerTree = PropertyTree.Create(new RuleDataDrawer(rule, selectedRuleSrc.Value));
					}
				}
				if (hasDataOverride)
				{
					GUIHelper.PushIsBoldLabel(hasDataOverride);
					serializedRule.Enabled = EditorGUI.ToggleLeft(toggleRect, new GUIContent(rule.Name, isCurrentProjectAndEnabledOverridenInLocal ? "Rule is overriden by local changes on this machine." : rule.Description), serializedRule.Enabled);
					GUIHelper.PopIsBoldLabel();
				}
				else
				{
					bool enabled = rule.IsEnabledIn(selectedRuleSrc.Value | ConfigSourceType.Default);
					if (enabled != EditorGUI.ToggleLeft(toggleRect, new GUIContent(rule.Name, rule.Description), enabled))
					{
						enabled = !enabled;
						rule.SetEnabledOverrideState(selectedRuleSrc, enabled);
					}
				}
				GUIHelper.PopColor();
			}
			GUILayout.FlexibleSpace();
			if (EditorGUI.EndChangeCheck())
			{
				rules.SaveChanges();
			}
		}

		private static void SelectScenePopup(Action<ValidationItem> onSelect, string selected, bool includeAssetDeps)
		{
			IEnumerable<string> scenes = (from x in AssetDatabase.FindAssets("t:scene")
				select AssetDatabase.GUIDToAssetPath(x)).Distinct();
			SceneSelector selector = new SceneSelector(scenes, includeAssetDeps);
			bool isValidGuid = selected != null && selected.Length == 32 && !Directory.Exists(selected);
			string path = (isValidGuid ? AssetDatabase.GUIDToAssetPath(selected) : selected);
			if (path != null)
			{
				selector.SelectionTree.EnumerateTree().FirstOrDefault((OdinMenuItem x) => x.GetFullPath() == path)?.Select();
			}
			selector.SelectionTree.Config.DrawSearchToolbar = false;
			selector.SelectionTree.SortMenuItemsByName();
			foreach (OdinMenuItem item in selector.SelectionTree.EnumerateTree())
			{
				item.Icon = ((item.ChildMenuItems.Count == 0) ? EditorIcons.UnityLogo : EditorIcons.UnityFolderIcon);
			}
			selector.SelectionConfirmed += delegate
			{
				string text = selector.SelectionTree.Selection.Select((OdinMenuItem x) => x.GetFullPath()).FirstOrDefault();
				if (text != null)
				{
					onSelect(File.Exists(text) ? ValidationItem.FromScenePath(text, selector.IncludeAssetDependencies) : ValidationItem.FromSceneFolderPath(text, selector.IncludeAssetDependencies));
				}
			};
			OdinEditorWindow window = selector.ShowInPopup();
		}

		private static bool DrawToolbarButton(Rect rect, SdfIconType icon, string tooltip = "")
		{
			EditorGUI.DrawRect(rect.TakeFromBottom(1f), ValidatorGui.BorderColor);
			EditorGUI.DrawRect(rect.AlignTop(1f).AddY(-1f), ValidatorGui.BorderColor);
			bool clicked = ValidatorGui.ToolbarBtn(rect, on: false, icon, "", tooltip);
			EditorGUI.DrawRect(rect.AlignLeft(1f).AddX(-1f), ValidatorGui.BorderColor);
			return clicked;
		}

		private void CreateAndShowGenericMenu(ValidationSessionResultCollector.ResultItemSingle item)
		{
			GenericMenu menu = new GenericMenu();
			Type objType = item.Result.GetObjectType();
			Type valType = item.Result.GetGenericValidatorType();
			SceneReference scene = item.Result.GetSceneReference();
			string message = item.Result.Message;
			if (scene.GUID != null)
			{
				menu.AddItem(new GUIContent("Only show results from this scene"), on: false, delegate
				{
					SelectedMenu = MenuOptions.FilterResults;
					MenuVisibility = true;
					foreach (ValidationSessionResultCollector.Filter<SceneReference>.FilteredItem current in ValidationSession.Results.SceneFilters.OrderedItems)
					{
						current.Enabled = current.Value.GUID == scene.GUID;
					}
					ValidationSession.Results.MarkFiltersDirty();
					GUIHelper.RemoveFocusControl();
				});
			}
			else
			{
				menu.AddItem(new GUIContent("Only show results from assets"), on: false, delegate
				{
					SelectedMenu = MenuOptions.FilterResults;
					MenuVisibility = true;
					foreach (ValidationSessionResultCollector.Filter<SceneReference>.FilteredItem current in ValidationSession.Results.SceneFilters.OrderedItems)
					{
						current.Enabled = current.Value.GUID == null;
					}
					ValidationSession.Results.MarkFiltersDirty();
					GUIHelper.RemoveFocusControl();
				});
			}
			menu.AddItem(new GUIContent("Only show results from this object type"), on: false, delegate
			{
				SelectedMenu = MenuOptions.FilterResults;
				MenuVisibility = true;
				foreach (ValidationSessionResultCollector.Filter<Type>.FilteredItem current in ValidationSession.Results.ObjectTypeFilters.OrderedItems)
				{
					current.Enabled = current.Value == objType;
				}
				ValidationSession.Results.MarkFiltersDirty();
				GUIHelper.RemoveFocusControl();
			});
			menu.AddItem(new GUIContent("Only show results from this validator"), on: false, delegate
			{
				SelectedMenu = MenuOptions.FilterResults;
				MenuVisibility = true;
				foreach (ValidationSessionResultCollector.Filter<Type>.FilteredItem current in ValidationSession.Results.ValidatorTypeFilters.OrderedItems)
				{
					current.Enabled = current.Value == valType;
				}
				ValidationSession.Results.MarkFiltersDirty();
				GUIHelper.RemoveFocusControl();
			});
			menu.AddItem(new GUIContent("Only show results with this error message"), on: false, delegate
			{
				selectionToRecover = SelectedResult;
				ValidationSession.Results.SearchTerm = message;
				ValidationSession.Results.MarkFiltersDirty();
				GUIHelper.RemoveFocusControl();
			});
			menu.AddSeparator("");
			string msg = item.Result.Message;
			string assetPath = item.Result.SelectionObjectAddress.LatestAddress.AssetPath;
			menu.AddItem(new GUIContent("Copy message"), on: false, delegate
			{
				Clipboard.Copy(msg);
			});
			menu.AddItem(new GUIContent("Copy asset path"), on: false, delegate
			{
				Clipboard.Copy(assetPath);
			});
			if (scene.GUID != null)
			{
				menu.AddSeparator("");
				menu.AddItem(new GUIContent("Ping scene asset"), on: false, delegate
				{
					UnityEngine.Object obj = AssetDatabase.LoadAssetAtPath(scene.Path, typeof(UnityEngine.Object));
					EditorGUIUtility.PingObject(obj);
				});
			}
			PropertyTree treeToDispose = null;
			try
			{
				Type validatorType = item.Result.ValidatorType;
				bool definesGenericMenuItems = validatorType != null && typeof(IDefinesGenericMenuItems).IsAssignableFrom(validatorType);
				bool hasContextClickDelegate = item.Result.Data.OnContextClick != null;
				if (definesGenericMenuItems || hasContextClickDelegate)
				{
					item.Result.DynamicObjectAddress.TryGetObjectReference(openSceneIfNeeded: false, autoSaveIfOpenScene: false, out var uObj, out var _);
					if ((bool)uObj)
					{
						treeToDispose = PropertyTree.Create(uObj);
					}
					IValidator iValidator = GetValidatorForResultOrNull(treeToDispose, item.Result);
					if (iValidator != null)
					{
						menu.AddSeparator(string.Empty);
						if (iValidator is IDefinesGenericMenuItems definer && iValidator is Validator validator1)
						{
							definer.PopulateGenericMenu(validator1.Property, menu);
						}
						if (hasContextClickDelegate)
						{
							ResultItemPersistor.PersistenceContext rebuildContext = ResultItemPersistor.CreateContextFromValidator(iValidator);
							if (ResultItemPersistor.TryRebuildResultItem(in item.Result.Result, ref rebuildContext, openSceneIfNeeded: false, out var restoredResultItem) && restoredResultItem.OnContextClick != null)
							{
								restoredResultItem.OnContextClick(menu);
							}
						}
					}
				}
				menu.ShowAsContext();
			}
			finally
			{
				treeToDispose?.Dispose();
			}
		}

		public static void DrawValidationSesssionSelector(Rect rect, ValidationProfile selected, Action<ValidationProfile> onSelected)
		{
			string name = (selected ? selected.name : "Select Profile");
			GUIContent lbl = GUIHelper.TempContent(name);
			GUIStyle style = EditorStyles.popup;
			float size = style.CalcSize(lbl).x;
			lbl.tooltip = "Select profile";
			if (!GUI.Button(rect, lbl, style))
			{
				return;
			}
			ValidationProfileSelector selector = new ValidationProfileSelector(selected);
			OdinEditorWindow wnd = OdinEditorWindow.InspectObjectInDropDown(selector);
			wnd.WindowPadding = Vector4.zero;
			selector.Selector.SelectionConfirmed += delegate(IEnumerable<ValidationProfile> x)
			{
				EditorApplication.delayCall = (EditorApplication.CallbackFunction)Delegate.Combine(EditorApplication.delayCall, (EditorApplication.CallbackFunction)delegate
				{
					wnd.Close();
					onSelected(x.FirstOrDefault());
				});
			};
		}

		private void DrawToolbar(Rect toolbarRect)
		{
			Rect totalRect = toolbarRect;
			bool isValidating = ValidationSession.ShouldDisplayProgressBar;
			EditorGUI.DrawRect(toolbarRect.TakeFromBottom(1f), ValidatorGui.BorderColor);
			Color col = ((ValidationSession.Results.TotalErrorCount == 0) ? ValidatorGui.GrayIconColor : ValidatorGui.RedErrorColor);
			if (Button(ref toolbarRect, fromLeft: false, SdfIconType.ExclamationOctagonFill, ValidationSession.Results.ShowErrors, ValidationSession.Results.TotalErrorCount.ToString(), "Show errors", col))
			{
				InvokeEndOfRepaint(delegate
				{
					ValidationSession.Results.ShowErrors = !ValidationSession.Results.ShowErrors;
				});
			}
			col = ((ValidationSession.Results.TotalWarningCount == 0) ? ValidatorGui.GrayIconColor : ValidatorGui.YellowWarningColor);
			if (Button(ref toolbarRect, fromLeft: false, SdfIconType.ExclamationTriangleFill, ValidationSession.Results.ShowWarnings, ValidationSession.Results.TotalWarningCount.ToString(), "Show warnings", col))
			{
				InvokeEndOfRepaint(delegate
				{
					ValidationSession.Results.ShowWarnings = !ValidationSession.Results.ShowWarnings;
				});
			}
			col = ((ValidationSession.Results.TotalValidCount == 0) ? ValidatorGui.GrayIconColor : ValidatorGui.GreenValidColor);
			if (Button(ref toolbarRect, fromLeft: false, SdfIconType.CheckCircleFill, ValidationSession.Results.ShowValid, ValidationSession.Results.TotalValidCount.ToString(), "Show fixed results", col))
			{
				InvokeEndOfRepaint(delegate
				{
					ValidationSession.Results.ShowValid = !ValidationSession.Results.ShowValid;
				});
			}
			bool isBulkFixing = SelectedMenu == MenuOptions.BulkFixing && MenuVisibility;
			bool enable = isBulkFixing || ValidationSession.Results.FixTypeFilters.OrderedItems.Count > 0;
			GUIHelper.PushGUIEnabled(enable);
			if (Button(ref toolbarRect, fromLeft: false, SdfIconType.UiChecks, isBulkFixing, "Bulk fix issues", "Fix multiple issues."))
			{
				InvokeEndOfRepaint(delegate
				{
					if (isBulkFixing)
					{
						MenuVisibility = false;
					}
					else
					{
						if (SelectedResult != null)
						{
							Fix fix = SelectedResult.Data.Fix;
							if (fix != null)
							{
								FixIdentifier fixIdentifier = fix.CreateIdentifier((SelectedResult.ValidatorType == null) ? "Fix" : SelectedResult.ValidatorType.GetNiceValidatorTypeName());
								foreach (ValidationSessionResultCollector.Filter<FixIdentifier>.FilteredItem current in ValidationSession.Results.FixTypeFilters.OrderedItems)
								{
									current.Enabled = fixIdentifier == current.Value;
								}
							}
						}
						SelectedMenu = MenuOptions.BulkFixing;
						MenuVisibility = true;
					}
				});
			}
			GUIHelper.PopGUIEnabled();
			GUIHelper.PushGUIEnabled(!Application.isPlaying);
			GUIContent name = GUIHelper.TempContent("  " + ValidationSession.Name + "  ");
			Rect profileNameRect;
			if ((bool)window.profile)
			{
				float size = EditorStyles.toolbarDropDown.CalcSize(name).x;
				name.tooltip = "Select profile";
				profileNameRect = toolbarRect.TakeFromLeft(size);
				if (GUI.Button(profileNameRect, name, EditorStyles.toolbarDropDown))
				{
					ValidationProfileSelector selector = new ValidationProfileSelector(window.profile);
					OdinEditorWindow wnd = OdinEditorWindow.InspectObjectInDropDown(selector);
					wnd.WindowPadding = Vector4.zero;
					selector.Selector.SelectionConfirmed += delegate(IEnumerable<ValidationProfile> x)
					{
						EditorApplication.delayCall = (EditorApplication.CallbackFunction)Delegate.Combine(EditorApplication.delayCall, (EditorApplication.CallbackFunction)delegate
						{
							wnd.Close();
						});
						Window.SwitchProfile(x.FirstOrDefault());
					};
				}
			}
			else
			{
				float size2 = EditorStyles.toolbarDropDown.CalcSize(name).x;
				profileNameRect = toolbarRect.TakeFromLeft(size2);
				GUI.Label(profileNameRect, name);
			}
			if (Event.current.type == EventType.Repaint && isValidating)
			{
				float t = ValidationSession.CalculateCurrentValidationProgress();
				Color bg = ValidatorGui.EditorWindowBgColor;
				EditorGUI.DrawRect(profileNameRect, bg);
				ValidatorGui.ProgressBar(profileNameRect.Padding(2f), t, ValidationSession.Name);
			}
			ProjectSettingBool canValidateInBackground = GlobalConfig<GlobalValidationConfig>.Instance.ValidateInBackground;
			if ((bool)canValidateInBackground && !isValidating && Button(ref toolbarRect, fromLeft: true, SdfIconType.Play, isOn: false, null, "Validate assets, open scenes and global validators in background."))
			{
				SceneView.RepaintAll();
				ValidationSession.PopulateQueue(clearCurrentQueue: false, populateUnloadedScenes: false);
				if (!ValidationSession.IsValidatingInBackground)
				{
					ValidationSession.StartSession(GlobalConfig<GlobalValidationConfig>.Instance.WatchForChanges, GlobalConfig<GlobalValidationConfig>.Instance.ValidateInBackground);
				}
			}
			if ((bool)canValidateInBackground && isValidating && Button(ref toolbarRect, fromLeft: true, SdfIconType.SkipForwardFill, isOn: false, null, "Complete validation now."))
			{
				InvokeEndOfRepaint(delegate
				{
					ValidationSession.PopulateQueue(clearCurrentQueue: false, populateUnloadedScenes: false);
					if (!ValidationSession.IsValidatingInBackground)
					{
						ValidationSession.StartSession(GlobalConfig<GlobalValidationConfig>.Instance.WatchForChanges, GlobalConfig<GlobalValidationConfig>.Instance.ValidateInBackground);
					}
					ValidationSession.ValidateQueuedUpWorkNow();
					SceneView.RepaintAll();
				});
			}
			GUIHelper.PushGUIEnabled(isValidating);
			if (ValidationSession.IsValidatingInBackground && isValidating && Button(ref toolbarRect, fromLeft: true, SdfIconType.StopFill, isOn: false, null, "Stop validating and clear queued up work."))
			{
				InvokeEndOfRepaint(delegate
				{
					ValidationSession.Clear(clearResults: false, clearQueue: true);
					SceneView.RepaintAll();
				});
			}
			GUIHelper.PopGUIEnabled();
			if (!isValidating && Button(ref toolbarRect, fromLeft: true, SdfIconType.PlayFill, isOn: false, null, "Validate the entire session now, including currently closed scenes."))
			{
				InvokeEndOfRepaint(delegate
				{
					ValidationSession.ValidateEverythingNow(openClosedScenes: true, showProgressBar: true);
					SceneView.RepaintAll();
				});
			}
			if (Button(ref toolbarRect, fromLeft: true, SdfIconType.TrashFill, isOn: false, null, "Clears all results and the current validation queue."))
			{
				InvokeEndOfRepaint(delegate
				{
					ValidationSession.Clear(clearResults: true, clearQueue: true);
					ProjectWatcher.RestartWatching(skipLoadEvents: true);
					SceneView.RepaintAll();
				});
			}
			GUIHelper.PopGUIEnabled();
			Rect searchFieldRect = toolbarRect.TakeFromRight(toolbarRect.width).HorizontalPadding(ValidatorGui.ContentPadding).AlignCenterY(EditorGUIUtility.singleLineHeight);
			if (searchFieldRect.width > 10f)
			{
				searchFieldRect.yMin += 1f;
				string oldSearchTerm = ValidationSession.Results.SearchTerm;
				string newSearchTerm = searchField.Draw(searchFieldRect, oldSearchTerm);
				if (oldSearchTerm != newSearchTerm)
				{
					ValidationSession.Results.SearchTerm = newSearchTerm;
				}
			}
			static bool Button(ref Rect rectIn, bool fromLeft, SdfIconType? icon, bool isOn, string text, string tooltip, Color? color = null)
			{
				float textWidth = 0f;
				float iconWidth = 0f;
				GUIStyle style = ValidatorGui.LabelVerticalCentered;
				GUIContent content = ((text == null) ? null : new GUIContent(text));
				if (icon.HasValue)
				{
					iconWidth = rectIn.height + 8f;
				}
				if (content != null)
				{
					textWidth = ((!(text == "On")) ? style.CalcSize(content).x : style.CalcSize(new GUIContent("Off")).x);
					if (!icon.HasValue)
					{
						textWidth += 10f;
					}
				}
				Rect rect;
				if (fromLeft)
				{
					rect = rectIn.TakeFromLeft(iconWidth + textWidth);
					EditorGUI.DrawRect(rectIn.TakeFromLeft(1f), ValidatorGui.BorderColor);
				}
				else
				{
					rect = rectIn.TakeFromRight(iconWidth + textWidth);
					EditorGUI.DrawRect(rectIn.TakeFromRight(1f), ValidatorGui.BorderColor);
				}
				bool clicked = GUI.Button(rect, new GUIContent("", tooltip), GUIStyle.none);
				Color bg2 = ValidatorGui.ToolbarBgColor;
				bool mouseOver = rect.Contains(Event.current.mousePosition);
				Color iconCol = ValidatorGui.BtnContentColor;
				if (isOn || mouseOver)
				{
					Color hColor = ((icon.HasValue && icon.Value == SdfIconType.UiChecks) ? ValidatorGui.BtnActiveBgColor : ValidatorGui.HighlightedBgColor);
					bg2 = ((mouseOver && !isOn) ? ValidatorGui.BtnMouseOverBgColor : hColor);
					EditorGUI.DrawRect(rect, bg2);
					iconCol = ValidatorGui.BtnMouseOverContentColor;
					style = ValidatorGui.ActiveLabelVerticalCentered;
				}
				if (icon.HasValue)
				{
					if (color.HasValue)
					{
						iconCol = color.Value;
					}
					int pad = 4;
					Rect iconRect = rect.TakeFromLeft(iconWidth);
					if (icon.Value == SdfIconType.CheckCircleFill)
					{
						pad++;
					}
					else if (icon.Value == SdfIconType.UiChecks)
					{
						iconRect.y += 1f;
					}
					SdfIcons.DrawIcon(iconRect.VerticalPadding(pad - 1, pad), icon.Value, iconCol, bg2);
				}
				if (content != null)
				{
					Rect labelRect = rect.TakeFromRight(textWidth);
					if (icon.HasValue)
					{
						labelRect.x -= 5f;
						GUI.Label(labelRect, content, style);
					}
					else
					{
						labelRect.x += 5f;
						GUI.Label(labelRect, content, style);
					}
				}
				return clicked;
			}
		}

		private void InvokeEndOfRepaint(Action action)
		{
			GUIHelper.RequestRepaint();
			delayedAction = (Action)Delegate.Combine(delayedAction, (Action)delegate
			{
				action();
				Window.Repaint();
				GUIHelper.ExitGUI(removeFocusControl: true);
			});
			Window.Repaint();
		}

		private (Texture texture, SdfIconType sdfIcon) GetFilterItemIcon<T>(ValidationSessionResultCollector.Filter<T>.FilteredItem item, out bool highlightIcon)
		{
			if (typeof(T) == typeof(SceneReference))
			{
				highlightIcon = true;
				T value = item.Value;
				if (value is SceneReference { GUID: null })
				{
					return (texture: EditorIcons.UnityFolderIcon, sdfIcon: SdfIconType.None);
				}
				return (texture: ValidatorGui.SceneAssetIcon, sdfIcon: SdfIconType.None);
			}
			if (item.Value is Type t && t == typeof(NullReferenceException))
			{
				highlightIcon = true;
				return (texture: EditorIcons.Transparent.Active, sdfIcon: SdfIconType.None);
			}
			if (typeof(T) == typeof(Type))
			{
				highlightIcon = true;
				return (texture: GUIHelper.GetAssetThumbnail(null, (Type)(object)item.Value, preferObjectPreviewOverFileIcon: false), sdfIcon: SdfIconType.None);
			}
			if (typeof(T) == typeof(FixIdentifier))
			{
				highlightIcon = false;
				return (texture: null, sdfIcon: SdfIconType.Tools);
			}
			highlightIcon = false;
			throw new NotImplementedException();
		}

		public static void OpenRuleSettingsWindow(Type ruleType, ConfigSourceType configSourceType)
		{
			if (ruleType == null)
			{
				throw new ArgumentNullException("ruleType");
			}
			ValidationSessionEditor window = ActiveEditors.FirstOrDefault();
			if (window == null)
			{
				window = OdinValidatorWindow.OpenWindow(ValidationProfile.MainValidationProfile);
			}
			int ruleIndex = -1;
			CombinedRuleInstance rule = null;
			for (int i = 0; i < window.rules.Rules.Length; i++)
			{
				if (window.rules.Rules[i].Default.ValidatorType == ruleType)
				{
					ruleIndex = i;
					rule = window.rules.Rules[i];
					break;
				}
			}
			if (ruleIndex == -1 || rule == null)
			{
				Debug.LogError("Could not find rule of type " + ruleType.GetNiceName());
				return;
			}
			window.SelectedMenu = MenuOptions.Rules;
			window.MenuVisibility = true;
			window.selectedRuleIndex = ruleIndex;
			window.selectedRuleSrc.Value = configSourceType;
			window.selectedRuleDataDrawer?.Dispose();
			window.selectedRuleDataDrawer = PropertyTree.Create(new RuleDataDrawer(rule, window.selectedRuleSrc.Value));
			window.Window.Focus();
		}

		public void Dispose()
		{
			ValidationSession.Results.OnResultsChanged -= RepaintWindow;
			currentResultTree?.Dispose();
			metaDataTree?.Dispose();
			issueFixerTree?.Dispose();
			selectedRuleDataDrawer?.Dispose();
			metaDataTree = null;
			currentResultTree = null;
			issueFixerTree = null;
			currentFix = null;
			ActiveEditors.Remove(this);
		}
	}
}
