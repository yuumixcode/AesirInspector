using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.Serialization;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	/// <summary>
	/// Property drawer for <see cref="T:System.Collections.Generic.IDictionary`2" />.
	/// </summary>
	public class DictionaryDrawer<TDictionary, TKey, TValue> : OdinValueDrawer<TDictionary>, IDisposable where TDictionary : IDictionary<TKey, TValue>
	{
		private const string CHANGE_ID = "DICTIONARY_DRAWER";

		private static readonly bool KeyIsValueType = typeof(TKey).IsValueType;

		private static GUIStyle listItemStyle;

		private GUIPagingHelper paging = new GUIPagingHelper();

		private GeneralDrawerConfig config;

		private LocalPersistentContext<float> keyWidthOffset;

		private bool showAddKeyGUI;

		private bool? newKeyIsValid;

		private string newKeyErrorMessage;

		private TKey newKey;

		private TValue newValue;

		private BaseKeyValueMapResolver<TDictionary> keyValueMapResolver;

		private DictionaryDrawerSettings attrSettings;

		private bool disableAddKey;

		private GUIContent keyLabel;

		private GUIContent emptySpaceLabel;

		private GUIContent valueLabel;

		private float keyLabelWidth;

		private float valueLabelWidth;

		private TempKeyValuePair<TKey, TValue> tempKeyValue;

		private PropertyTree keyEntryPropertyTree;

		private IPropertyValueEntry<TKey> tempKeyEntry;

		private IPropertyValueEntry<TValue> tempValueEntry;

		private MultiCollectionFilter<BaseKeyValueMapResolver<TDictionary>> filter;

		private static GUIStyle foldoutHeaderStyle;

		private static GUIStyle oneLineMargin;

		private static GUIStyle headerMargin;

		private static GUIStyle FoldoutHeaderStyle
		{
			get
			{
				if (foldoutHeaderStyle == null)
				{
					foldoutHeaderStyle = new GUIStyle
					{
						padding = new RectOffset(4, 4, 2, 4)
					};
				}
				return foldoutHeaderStyle;
			}
		}

		private static GUIStyle OneLineMargin
		{
			get
			{
				if (oneLineMargin == null)
				{
					oneLineMargin = new GUIStyle
					{
						margin = new RectOffset(8, 0, 0, 0)
					};
				}
				return oneLineMargin;
			}
		}

		private static GUIStyle HeaderMargin
		{
			get
			{
				if (headerMargin == null)
				{
					headerMargin = new GUIStyle
					{
						margin = new RectOffset(40, 0, 0, 0)
					};
				}
				return headerMargin;
			}
		}

		protected override bool CanDrawValueProperty(InspectorProperty property)
		{
			if (property.ChildResolver is BaseKeyValueMapResolver<TDictionary> resolver)
			{
				return resolver.ElementType == typeof(EditableKeyValuePair<TKey, TValue>);
			}
			return false;
		}

		protected override void Initialize()
		{
			BaseKeyValueMapResolver<TDictionary> resolver = base.Property.ChildResolver as BaseKeyValueMapResolver<TDictionary>;
			filter = new MultiCollectionFilter<BaseKeyValueMapResolver<TDictionary>>(base.Property, resolver);
			listItemStyle = new GUIStyle(GUIStyle.none)
			{
				padding = new RectOffset(7, 20, 3, 3)
			};
			IPropertyValueEntry<TDictionary> entry = base.ValueEntry;
			attrSettings = entry.Property.GetAttribute<DictionaryDrawerSettings>() ?? new DictionaryDrawerSettings();
			keyWidthOffset = this.GetPersistentValue("KeyColumnWidth", attrSettings.KeyColumnWidth);
			disableAddKey = entry.Property.Tree.PrefabModificationHandler.HasPrefabs && entry.SerializationBackend == SerializationBackend.Odin && !entry.Property.SupportsPrefabModifications;
			keyLabel = new GUIContent(attrSettings.KeyLabel);
			valueLabel = new GUIContent(attrSettings.ValueLabel);
			emptySpaceLabel = new GUIContent(" ");
			keyLabelWidth = EditorStyles.label.CalcSize(keyLabel).x + 20f;
			valueLabelWidth = EditorStyles.label.CalcSize(valueLabel).x + 20f;
			if (!disableAddKey)
			{
				tempKeyValue = new TempKeyValuePair<TKey, TValue>();
				keyEntryPropertyTree = PropertyTree.Create(tempKeyValue);
				keyEntryPropertyTree.UpdateTree();
				tempKeyEntry = (IPropertyValueEntry<TKey>)keyEntryPropertyTree.GetPropertyAtPath("Key").ValueEntry;
				tempValueEntry = (IPropertyValueEntry<TValue>)keyEntryPropertyTree.GetPropertyAtPath("Value").ValueEntry;
			}
		}

		/// <summary>
		/// Draws the property.
		/// </summary>
		protected override void DrawPropertyLayout(GUIContent label)
		{
			IPropertyValueEntry<TDictionary> entry = base.ValueEntry;
			if (entry.SerializationBackend.IsUnity && base.Property.Tree.WeakTargets.Count > 1)
			{
				SirenixEditorGUI.MessageBox("Multi-object editing of Unity-serialized dictionaries is not supported by Unity.", MessageType.Info, GlobalConfig<GeneralDrawerConfig>.Instance.MessageBoxFontSize);
				return;
			}
			keyValueMapResolver = entry.Property.ChildResolver as BaseKeyValueMapResolver<TDictionary>;
			config = GlobalConfig<GeneralDrawerConfig>.Instance;
			paging.NumberOfItemsPerPage = config.NumberOfItemsPrPage;
			listItemStyle.padding.right = ((!entry.IsEditable || attrSettings.IsReadOnly) ? 4 : 20);
			SirenixEditorGUI.BeginIndentedVertical(SirenixGUIStyles.PropertyMargin);
			paging.Update(filter.GetCount());
			DrawToolbar(entry, label);
			paging.Update(filter.GetCount());
			GUIHelper.PushIsBoldLabel(isBold: false);
			if (!disableAddKey && !attrSettings.IsReadOnly)
			{
				DrawAddKey(entry);
			}
			GUIHelper.BeginLayoutMeasuring();
			if (SirenixEditorGUI.BeginFadeGroup(UniqueDrawerKey.Create(entry.Property, this), base.Property.State.Expanded, out var t))
			{
				Rect rect = SirenixEditorGUI.BeginVerticalList(false, true);
				if (attrSettings.DisplayMode == DictionaryDisplayOptions.OneLine)
				{
					float maxWidth = rect.width - 90f;
					rect.xMin = rect.xMin + keyWidthOffset.Value + 8f;
					rect.xMax = rect.xMin + 10f;
					GUIHelper.PushGUIEnabled(enabled: true);
					keyWidthOffset.Value += SirenixEditorGUI.SlideRect(rect).x;
					GUIHelper.PopGUIEnabled();
					if (Event.current.type == EventType.Repaint)
					{
						keyWidthOffset.Value = Mathf.Clamp(keyWidthOffset.Value, 30f, maxWidth);
					}
					if (paging.ElementCount != 0)
					{
						Rect headerRect = SirenixEditorGUI.BeginListItem(false, null);
						GUILayout.Space(14f);
						if (Event.current.type == EventType.Repaint)
						{
							GUI.Label(headerRect.SetWidth(keyWidthOffset.Value + 13f), keyLabel, SirenixGUIStyles.LabelCentered);
							GUI.Label(headerRect.AddXMin(keyWidthOffset.Value + 13f), valueLabel, SirenixGUIStyles.LabelCentered);
							SirenixEditorGUI.DrawSolidRect(headerRect.AlignBottom(1f), SirenixGUIStyles.BorderColor);
						}
						SirenixEditorGUI.EndListItem();
					}
				}
				GUIHelper.PushHierarchyMode(hierarchyMode: false);
				DrawElements(entry, label);
				GUIHelper.PopHierarchyMode();
				SirenixEditorGUI.EndVerticalList();
			}
			SirenixEditorGUI.EndFadeGroup();
			Rect outerRect = GUIHelper.EndLayoutMeasuring();
			if (t > 0.01f && Event.current.type == EventType.Repaint)
			{
				Color col = SirenixGUIStyles.BorderColor;
				outerRect.yMin -= 1f;
				SirenixEditorGUI.DrawBorders(outerRect, 1, col);
				col.a *= t;
				if (attrSettings.DisplayMode == DictionaryDisplayOptions.OneLine)
				{
					outerRect.width = 1f;
					outerRect.x += keyWidthOffset.Value + 13f;
					SirenixEditorGUI.DrawSolidRect(outerRect, col);
				}
			}
			GUIHelper.PopIsBoldLabel();
			SirenixEditorGUI.EndIndentedVertical();
		}

		private void DrawAddKey(IPropertyValueEntry<TDictionary> entry)
		{
			if (!entry.IsEditable || attrSettings.IsReadOnly)
			{
				return;
			}
			if (SirenixEditorGUI.BeginFadeGroup(this, showAddKeyGUI))
			{
				Rect rect = EditorGUILayout.BeginVertical(SirenixGUIStyles.ContentPadding);
				EditorGUI.DrawRect(rect, SirenixGUIStyles.BoxBackgroundColor);
				SirenixEditorGUI.DrawBorders(rect, 1, 1, 0, 0);
				if (typeof(TKey) == typeof(string) && newKey == null)
				{
					newKey = (TKey)(object)"";
					newKeyIsValid = null;
				}
				if (!newKeyIsValid.HasValue)
				{
					newKeyIsValid = CheckNewKeyIsValid(entry, newKey, out newKeyErrorMessage);
				}
				tempKeyEntry.Property.Tree.BeginDraw(withUndo: false);
				tempKeyEntry.Property.Update();
				EditorGUI.BeginChangeCheck();
				tempKeyEntry.Property.Draw(keyLabel);
				bool changed1 = EditorGUI.EndChangeCheck();
				bool changed2 = tempKeyEntry.ApplyChanges();
				if (changed1 || changed2)
				{
					newKey = tempKeyValue.Key;
					UnityEditorEventUtility.EditorApplication_delayCall += delegate
					{
						newKeyIsValid = null;
					};
					GUIHelper.RequestRepaint();
				}
				tempValueEntry.Property.Update();
				tempValueEntry.Property.Draw(valueLabel);
				tempValueEntry.ApplyChanges();
				newValue = tempKeyValue.Value;
				tempKeyEntry.Property.Tree.InvokeDelayedActions();
				if (tempKeyEntry.Property.Tree.ApplyChanges())
				{
					newKey = tempKeyValue.Key;
					UnityEditorEventUtility.EditorApplication_delayCall += delegate
					{
						newKeyIsValid = null;
					};
					GUIHelper.RequestRepaint();
				}
				tempKeyEntry.Property.Tree.EndDraw();
				GUIHelper.PushGUIEnabled(GUI.enabled && newKeyIsValid.Value);
				if (GUILayout.Button(newKeyIsValid.Value ? "Add" : newKeyErrorMessage))
				{
					object[] keys = new object[entry.ValueCount];
					object[] values = new object[entry.ValueCount];
					for (int i = 0; i < keys.Length; i++)
					{
						keys[i] = Sirenix.Serialization.SerializationUtility.CreateCopy(newKey);
					}
					for (int i2 = 0; i2 < values.Length; i2++)
					{
						values[i2] = Sirenix.Serialization.SerializationUtility.CreateCopy(newValue);
					}
					keyValueMapResolver.QueueSet(keys, values);
					UnityEditorEventUtility.EditorApplication_delayCall += delegate
					{
						newKeyIsValid = null;
					};
					GUIHelper.RequestRepaint();
					entry.Property.Tree.DelayActionUntilRepaint(delegate
					{
						newValue = default(TValue);
						tempKeyValue.Value = default(TValue);
						tempValueEntry.Update();
					});
				}
				GUIHelper.PopGUIEnabled();
				EditorGUILayout.EndVertical();
			}
			SirenixEditorGUI.EndFadeGroup();
		}

		private void DrawToolbar(IPropertyValueEntry<TDictionary> entry, GUIContent label)
		{
			SirenixEditorGUI.BeginHorizontalToolbar();
			if (entry.ListLengthChangedFromPrefab)
			{
				GUIHelper.PushIsBoldLabel(isBold: true);
			}
			if (paging.ElementCount == 0)
			{
				if (label != null)
				{
					GUILayout.Label(label, GUILayoutOptions.ExpandWidth(expand: false));
				}
			}
			else
			{
				bool newState = ((label != null) ? SirenixEditorGUI.Foldout(base.Property.State.Expanded, label) : SirenixEditorGUI.Foldout(base.Property.State.Expanded, ""));
				if (!newState && base.Property.State.Expanded)
				{
					showAddKeyGUI = false;
				}
				base.Property.State.Expanded = newState;
			}
			if (entry.ListLengthChangedFromPrefab)
			{
				GUIHelper.PopIsBoldLabel();
			}
			GUILayout.FlexibleSpace();
			filter.Draw();
			if (config.ShowItemCount)
			{
				if (entry.ValueState == PropertyValueState.CollectionLengthConflict)
				{
					int min = entry.Values.Min((TDictionary x) => x.Count);
					int max = entry.Values.Max((TDictionary x) => x.Count);
					string lbl = min + " / " + max + " items";
					float lblWidth = EditorStyles.centeredGreyMiniLabel.CalcWidth(lbl) + 8f;
					Rect r = GUILayoutUtility.GetRect(lblWidth, 18f, GUILayoutOptions.ExpandWidth().ExpandHeight());
					GUI.Label(r, lbl, EditorStyles.centeredGreyMiniLabel);
				}
				else
				{
					string lbl2 = ((paging.ElementCount == 0) ? "Empty" : (paging.ElementCount + " items"));
					float lblWidth2 = EditorStyles.centeredGreyMiniLabel.CalcWidth(lbl2) + 8f;
					Rect r2 = GUILayoutUtility.GetRect(lblWidth2, 18f, GUILayoutOptions.ExpandWidth().ExpandHeight());
					GUI.Label(r2.SubY(1f), lbl2, EditorStyles.centeredGreyMiniLabel);
				}
			}
			if ((!config.HidePagingWhileCollapsed || base.Property.State.Expanded) && (!config.HidePagingWhileOnlyOnePage || paging.PageCount != 1))
			{
				bool wasEnabled = GUI.enabled;
				bool pagingIsRelevant = paging.IsEnabled && paging.PageCount != 1;
				GUI.enabled = wasEnabled && pagingIsRelevant && !paging.IsOnFirstPage;
				if (SirenixEditorGUI.ToolbarButton(EditorIcons.ArrowLeft, ignoreGUIEnabled: true))
				{
					if (Event.current.button == 0)
					{
						paging.CurrentPage--;
					}
					else
					{
						paging.CurrentPage = 0;
					}
				}
				GUI.enabled = wasEnabled && pagingIsRelevant;
				GUILayoutOptions.GUILayoutOptionsInstance width = GUILayoutOptions.Width(10 + paging.PageCount.ToString().Length * 10);
				paging.CurrentPage = EditorGUILayout.IntField(paging.CurrentPage + 1, width) - 1;
				GUILayout.Label(GUIHelper.TempContent("/ " + paging.PageCount));
				GUI.enabled = wasEnabled && pagingIsRelevant && !paging.IsOnLastPage;
				if (SirenixEditorGUI.ToolbarButton(EditorIcons.ArrowRight, ignoreGUIEnabled: true))
				{
					if (Event.current.button == 0)
					{
						paging.CurrentPage++;
					}
					else
					{
						paging.CurrentPage = paging.PageCount - 1;
					}
				}
				GUI.enabled = wasEnabled && paging.PageCount != 1;
				if (config.ShowExpandButton && SirenixEditorGUI.ToolbarButton(paging.IsEnabled ? EditorIcons.ArrowDown : EditorIcons.ArrowUp, ignoreGUIEnabled: true))
				{
					paging.IsEnabled = !paging.IsEnabled;
				}
				GUI.enabled = wasEnabled;
			}
			if (!disableAddKey && !attrSettings.IsReadOnly && SirenixEditorGUI.ToolbarButton(SdfIconType.Plus))
			{
				showAddKeyGUI = !showAddKeyGUI;
				if (showAddKeyGUI)
				{
					base.Property.State.Expanded = true;
				}
			}
			SirenixEditorGUI.EndHorizontalToolbar();
		}

		private void DrawElements(IPropertyValueEntry<TDictionary> entry, GUIContent label)
		{
			int i;
			for (i = paging.StartIndex; i < paging.EndIndex; i++)
			{
				InspectorProperty keyValuePairProperty = filter[i];
				EditableKeyValuePair<TKey, TValue> keyValuePairValue = (keyValuePairProperty.ValueEntry as IPropertyValueEntry<EditableKeyValuePair<TKey, TValue>>).SmartValue;
				Rect rect = SirenixEditorGUI.BeginListItem(false, listItemStyle);
				if (attrSettings.DisplayMode != DictionaryDisplayOptions.OneLine)
				{
					bool defaultExpanded = attrSettings.DisplayMode switch
					{
						DictionaryDisplayOptions.CollapsedFoldout => false, 
						DictionaryDisplayOptions.ExpandedFoldout => true, 
						_ => SirenixEditorGUI.ExpandFoldoutByDefault, 
					};
					LocalPersistentContext<bool> isExpanded = keyValuePairProperty.Context.GetPersistent(this, "Expanded", defaultExpanded);
					Rect headerRect = EditorGUILayout.BeginVertical(FoldoutHeaderStyle, GUILayout.MinHeight(22f));
					GUILayout.Space(1f);
					GUILayout.Space(-1f);
					GUI.DrawTexture(headerRect, Texture2D.whiteTexture, ScaleMode.StretchToFill, alphaBlend: false, 1f, new Color(0.24f, 0.24f, 0.24f), Vector4.zero, new Vector4(3f, 3f, isExpanded.Value ? 0f : 3f, isExpanded.Value ? 0f : 3f));
					GUI.DrawTexture(headerRect, Texture2D.whiteTexture, ScaleMode.StretchToFill, alphaBlend: false, 1f, SirenixGUIStyles.BorderColor, Vector4.one, new Vector4(3f, 3f, isExpanded.Value ? 0f : 3f, isExpanded.Value ? 0f : 3f));
					if (keyValuePairValue.IsInvalidKey)
					{
						GUIHelper.PushColor(Color.red);
					}
					GUIHelper.PushIsDrawingDictionaryKey(enabled: true);
					Rect foldoutRect = headerRect.AddX(4f).AlignLeft(45f);
					EditorGUIUtility.AddCursorRect(foldoutRect, MouseCursor.Arrow);
					isExpanded.Value = SirenixEditorGUI.Foldout(foldoutRect, isExpanded.Value, keyLabel);
					GUIHelper.PushLabelWidth(keyLabelWidth);
					InspectorProperty keyProperty = keyValuePairProperty.Children[0];
					DrawKeyProperty(keyProperty, emptySpaceLabel);
					GUIHelper.PopLabelWidth();
					GUIHelper.PopIsDrawingDictionaryKey();
					if (keyValuePairValue.IsInvalidKey)
					{
						GUIHelper.PopColor();
					}
					EditorGUILayout.EndVertical();
					if (SirenixEditorGUI.BeginFadeGroup(isExpanded, isExpanded.Value))
					{
						Rect contentRect = EditorGUILayout.BeginVertical(FoldoutHeaderStyle);
						GUILayout.Space(1f);
						GUI.DrawTexture(contentRect, Texture2D.whiteTexture, ScaleMode.StretchToFill, alphaBlend: false, 1f, new Color(1f, 1f, 1f, 0.042f), Vector4.zero, new Vector4(0f, 0f, 3f, 3f));
						GUI.DrawTexture(contentRect, Texture2D.whiteTexture, ScaleMode.StretchToFill, alphaBlend: false, 1f, SirenixGUIStyles.BorderColor, new Vector4(1f, 0f, 1f, 1f), new Vector4(0f, 0f, 3f, 3f));
						keyValuePairProperty.Children[1].Draw(null);
						EditorGUILayout.EndVertical();
					}
					SirenixEditorGUI.EndFadeGroup();
				}
				else
				{
					GUILayout.BeginHorizontal();
					GUILayout.BeginVertical(GUILayoutOptions.Width(keyWidthOffset.Value));
					InspectorProperty keyProperty2 = keyValuePairProperty.Children[0];
					if (keyValuePairValue.IsInvalidKey)
					{
						GUIHelper.PushColor(Color.red);
					}
					if (attrSettings.IsReadOnly)
					{
						GUIHelper.PushGUIEnabled(enabled: false);
					}
					GUIHelper.PushIsDrawingDictionaryKey(enabled: true);
					GUIHelper.PushLabelWidth(10f);
					DrawKeyProperty(keyProperty2, null);
					GUIHelper.PopLabelWidth();
					GUIHelper.PopIsDrawingDictionaryKey();
					if (attrSettings.IsReadOnly)
					{
						GUIHelper.PopGUIEnabled();
					}
					if (keyValuePairValue.IsInvalidKey)
					{
						GUIHelper.PopColor();
					}
					GUILayout.EndVertical();
					GUILayout.BeginVertical(OneLineMargin);
					GUIHelper.PushHierarchyMode(hierarchyMode: false);
					InspectorProperty valueEntry = keyValuePairProperty.Children[1];
					float tmp = GUIHelper.ActualLabelWidth;
					GUIHelper.BetterLabelWidth = 150f;
					valueEntry.Draw(null);
					GUIHelper.BetterLabelWidth = tmp;
					GUIHelper.PopHierarchyMode();
					GUILayout.EndVertical();
					GUILayout.EndHorizontal();
				}
				if (entry.IsEditable && !attrSettings.IsReadOnly && SirenixEditorGUI.SDFIconButton(new Rect(rect.xMax - 24f + 5f, rect.y + 4f + (float)(((int)rect.height - 23) / 2), 14f, 14f), SdfIconType.X, IconAlignment.LeftOfText, SirenixGUIStyles.IconButton))
				{
					keyValueMapResolver.QueueRemoveKey((from n in Enumerable.Range(0, entry.ValueCount)
						select keyValueMapResolver.GetKey(n, filter.GetCollectionIndex(i))).ToArray());
					UnityEditorEventUtility.EditorApplication_delayCall += delegate
					{
						newKeyIsValid = null;
					};
					filter.Update();
					GUIHelper.RequestRepaint();
				}
				SirenixEditorGUI.EndListItem();
			}
			if (paging.IsOnLastPage && entry.ValueState == PropertyValueState.CollectionLengthConflict)
			{
				SirenixEditorGUI.BeginListItem(false, null);
				GUILayout.Label(GUIHelper.TempContent("------"), EditorStyles.centeredGreyMiniLabel);
				SirenixEditorGUI.EndListItem();
			}
		}

		private void DrawKeyProperty(InspectorProperty keyProperty, GUIContent keyLabel)
		{
			EditorGUI.BeginChangeCheck();
			keyProperty.Draw(keyLabel);
			bool guiChanged = EditorGUI.EndChangeCheck();
			bool valuesAreDirty = ValuesAreDirty(keyProperty);
			if (!guiChanged && valuesAreDirty)
			{
				keyValueMapResolver.ValueApplyIsTemporary = true;
				ApplyChangesToProperty(keyProperty);
				keyValueMapResolver.ValueApplyIsTemporary = false;
			}
			else if (guiChanged && !valuesAreDirty)
			{
				MarkPropertyDirty(keyProperty);
			}
		}

		private static void MarkPropertyDirty(InspectorProperty keyProperty)
		{
			keyProperty.ValueEntry.WeakValues.ForceMarkDirty();
			if (KeyIsValueType)
			{
				for (int i = 0; i < keyProperty.Children.Count; i++)
				{
					MarkPropertyDirty(keyProperty.Children[i]);
				}
			}
		}

		private static void ApplyChangesToProperty(InspectorProperty keyProperty)
		{
			if (keyProperty.ValueEntry != null && keyProperty.ValueEntry.WeakValues.AreDirty)
			{
				keyProperty.ValueEntry.ApplyChanges();
			}
			if (KeyIsValueType)
			{
				for (int i = 0; i < keyProperty.Children.Count; i++)
				{
					ApplyChangesToProperty(keyProperty.Children[i]);
				}
			}
		}

		private static bool ValuesAreDirty(InspectorProperty keyProperty)
		{
			if (keyProperty.ValueEntry != null && keyProperty.ValueEntry.WeakValues.AreDirty)
			{
				return true;
			}
			if (KeyIsValueType)
			{
				for (int i = 0; i < keyProperty.Children.Count; i++)
				{
					if (ValuesAreDirty(keyProperty.Children[i]))
					{
						return true;
					}
				}
			}
			return false;
		}

		private static bool CheckNewKeyIsValid(IPropertyValueEntry<TDictionary> entry, TKey key, out string errorMessage)
		{
			if (!KeyIsValueType && key == null)
			{
				errorMessage = "Key cannot be null.";
				return false;
			}
			if (!entry.SmartValue.ContainsKey(key))
			{
				errorMessage = "";
				return true;
			}
			errorMessage = "An item with the same key already exists.";
			return false;
		}

		public void Dispose()
		{
			if (keyEntryPropertyTree != null)
			{
				keyEntryPropertyTree.Dispose();
				keyEntryPropertyTree = null;
			}
			filter?.Dispose();
		}
	}
}
