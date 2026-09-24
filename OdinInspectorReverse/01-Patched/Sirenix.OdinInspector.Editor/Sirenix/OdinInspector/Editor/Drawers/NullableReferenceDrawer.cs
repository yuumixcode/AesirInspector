using System;
using System.Collections;
using Sirenix.OdinInspector.Editor.Internal;
using Sirenix.Serialization;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	[DrawerPriority(0.0, 0.0, 2000.0)]
	[AllowGUIEnabledForReadonly]
	public sealed class NullableReferenceDrawer<T> : OdinValueDrawer<T>, IDefinesGenericMenuItems
	{
		private bool allowSceneObjects;

		private bool shouldDrawReferencePicker;

		private bool drawChildren;

		private bool isValueUnityType;

		private SearchField searchField;

		private PropertySearchFilter searchFilter;

		private InlinePropertyAttribute inlineAttribute;

		private PolymorphicDrawerSettingsAttribute polymorphicSettings;

		private OdinDrawer[] bakedDrawerArray;

		private bool ShowBaseType
		{
			get
			{
				if (polymorphicSettings == null)
				{
					return GlobalConfig<GeneralDrawerConfig>.Instance.showBaseType;
				}
				if (polymorphicSettings.ShowBaseTypeIsSet)
				{
					return polymorphicSettings.ShowBaseType;
				}
				return GlobalConfig<GeneralDrawerConfig>.Instance.showBaseType;
			}
		}

		private bool ReadOnlyIfNotNullReference => polymorphicSettings?.ReadOnlyIfNotNullReference ?? false;

		protected override void Initialize()
		{
			bakedDrawerArray = base.Property.GetActiveDrawerChain().BakedDrawerArray;
			SearchableAttribute searchableAttribute = base.Property.GetAttribute<SearchableAttribute>();
			if (searchableAttribute != null)
			{
				searchFilter = new PropertySearchFilter(base.Property, searchableAttribute);
				searchField = new SearchField();
			}
			isValueUnityType = typeof(UnityEngine.Object).IsAssignableFrom(base.ValueEntry.TypeOfValue);
			inlineAttribute = base.Property.Attributes.GetAttribute<InlinePropertyAttribute>();
			polymorphicSettings = base.Property.GetAttribute<PolymorphicDrawerSettingsAttribute>();
			allowSceneObjects = InspectorPropertyInfoUtility.InspectorPropertySupportsAssigningSceneReferences(base.Property);
		}

		public void PopulateGenericMenu(InspectorProperty property, GenericMenu genericMenu)
		{
			IPropertyValueEntry<T> entry = property.ValueEntry as IPropertyValueEntry<T>;
			bool isReadOnly = entry.ValueState != PropertyValueState.NullReference && ReadOnlyIfNotNullReference;
			bool isChangeable = property.ValueEntry.SerializationBackend.SupportsPolymorphism && !entry.BaseValueType.IsValueType && entry.BaseValueType != typeof(string) && property.GetAttribute<TypeFilterAttribute>() == null;
			if (!GlobalConfig<GeneralDrawerConfig>.Instance.useNewObjectSelector || GlobalConfig<GeneralDrawerConfig>.Instance.useOldPolymorphicField)
			{
				if (!isChangeable)
				{
					return;
				}
				if (entry.IsEditable && !isReadOnly)
				{
					ObjectPicker objectPicker = ObjectPicker.GetObjectPicker(entry, entry.BaseValueType);
					Rect rect = entry.Property.LastDrawnValueRect;
					rect.position = GUIUtility.GUIToScreenPoint(rect.position);
					rect.height = 20f;
					genericMenu.AddItem(new GUIContent("Change Type"), on: false, delegate
					{
						objectPicker.ShowObjectPicker(entry.WeakSmartValue, allowSceneObjects: false, rect);
					});
				}
				else
				{
					genericMenu.AddDisabledItem(new GUIContent("Change Type"));
				}
			}
			else
			{
				if (!isChangeable)
				{
					return;
				}
				bool isSystemTypeDrawer = base.ValueEntry.BaseValueType == typeof(Type);
				if (!isSystemTypeDrawer)
				{
					if (entry.IsEditable && !isReadOnly)
					{
						Rect rect2 = entry.Property.LastDrawnValueRect;
						rect2.position = GUIUtility.GUIToScreenPoint(rect2.position);
						rect2.height = 20f;
						genericMenu.AddItem(new GUIContent("Change Type"), on: false, delegate
						{
							base.Property.Tree.DelayActionUntilRepaint(delegate
							{
								if (Event.current.type != EventType.Layout)
								{
									Rect lastDrawnValueRect = base.Property.LastDrawnValueRect;
									lastDrawnValueRect = lastDrawnValueRect.AlignCenter(600f);
									int id = -2147475450;
									OdinObjectSelector.Show(lastDrawnValueRect, base.Property, id, base.Property, allowSceneObjects);
								}
							});
							GUIHelper.RequestRepaint();
						});
					}
					else
					{
						genericMenu.AddDisabledItem(new GUIContent("Change Type"));
					}
				}
				Type currentType = ((!isSystemTypeDrawer) ? ((base.ValueEntry.WeakSmartValue == null) ? base.ValueEntry.BaseValueType : base.ValueEntry.TypeOfValue) : ((base.ValueEntry.WeakSmartValue == null) ? base.ValueEntry.BaseValueType : (base.ValueEntry.WeakSmartValue as Type)));
				if (TypeRegistry.IsModifiableType(currentType))
				{
					genericMenu.AddItem(new GUIContent("Customize Type"), on: false, delegate
					{
						TypeRegistryUserConfigWindow window = EditorWindow.GetWindow<TypeRegistryUserConfigWindow>();
						if (base.ValueEntry.WeakSmartValue == null)
						{
							window.TypeToScrollTo = base.ValueEntry.BaseValueType;
						}
						else if (base.ValueEntry.WeakSmartValue is Type typeToScrollTo)
						{
							window.TypeToScrollTo = typeToScrollTo;
						}
						else
						{
							window.TypeToScrollTo = base.ValueEntry.TypeOfValue;
						}
					});
				}
				else
				{
					genericMenu.AddDisabledItem(new GUIContent("Customize Type"));
				}
			}
		}

		protected override void DrawPropertyLayout(GUIContent label)
		{
			if (base.ValueEntry.ValueState == PropertyValueState.ReferenceValueConflict)
			{
				CallNextDrawer(label);
			}
			if (Event.current.type == EventType.Layout)
			{
				shouldDrawReferencePicker = ShouldDrawReferenceObjectPicker(base.ValueEntry);
				drawChildren = base.Property.Children.Count > 0;
				if (base.Property.Children.Count > 0)
				{
					drawChildren = true;
				}
				else if (base.ValueEntry.ValueState != PropertyValueState.None)
				{
					drawChildren = false;
				}
				else
				{
					drawChildren = bakedDrawerArray[bakedDrawerArray.Length - 2] != this;
				}
				if (OdinObjectSelector.SelectorProperty == base.Property && (OdinObjectSelector.SelectorObject == null || (!typeof(UnityEngine.Object).IsAssignableFrom(base.ValueEntry.TypeOfValue) && typeof(UnityEngine.Object).IsAssignableFrom(OdinObjectSelector.SelectorObject?.GetType()))))
				{
					drawChildren = false;
				}
			}
			if (base.ValueEntry.ValueState == PropertyValueState.NullReference)
			{
				if (isValueUnityType)
				{
					CallNextDrawer(label);
				}
				else
				{
					if (!base.ValueEntry.SerializationBackend.SupportsPolymorphism && base.ValueEntry.IsEditable)
					{
						SirenixEditorGUI.MessageBox("Unity-backed value is null. This should already be fixed by the FixUnityNullDrawer! It is likely that this type has been incorrectly guessed by Odin to be serialized by Unity when it is actually not. Please create an issue on Odin's issue tracker stating how to reproduce this error message.", MessageType.Error, GlobalConfig<GeneralDrawerConfig>.Instance.MessageBoxFontSize);
					}
					DrawField(label);
				}
			}
			else if (shouldDrawReferencePicker)
			{
				DrawField(label);
			}
			else
			{
				CallNextDrawer(label);
			}
			if (!GlobalConfig<GeneralDrawerConfig>.Instance.useNewObjectSelector || GlobalConfig<GeneralDrawerConfig>.Instance.useOldPolymorphicField)
			{
				ObjectPicker objectPicker = ObjectPicker.GetObjectPicker(base.ValueEntry, base.ValueEntry.BaseValueType);
				if (objectPicker.IsReadyToClaim)
				{
					object obj = objectPicker.ClaimObject();
					base.ValueEntry.Property.Tree.DelayActionUntilRepaint(delegate
					{
						base.ValueEntry.WeakValues[0] = obj;
						for (int i = 1; i < base.ValueEntry.ValueCount; i++)
						{
							base.ValueEntry.WeakValues[i] = Sirenix.Serialization.SerializationUtility.CreateCopy(obj);
						}
					});
				}
			}
			if (GlobalConfig<GeneralDrawerConfig>.Instance.useNewObjectSelector && OdinObjectSelector.IsReadyToClaim(base.Property, -2147475450))
			{
				OdinObjectSelector.ClaimAndAssign(base.Property);
			}
		}

		private void DrawReferencePicker(Rect position, int id)
		{
			bool lastMixedValue = EditorGUI.showMixedValue;
			if (base.ValueEntry.ValueState == PropertyValueState.ReferenceValueConflict)
			{
				EditorGUI.showMixedValue = true;
			}
			bool isReadOnly = base.ValueEntry.ValueState != PropertyValueState.NullReference && ReadOnlyIfNotNullReference;
			OdinInternalEditorFields.PolymorphicFieldArgs polymorphicArgs = OdinInternalEditorFields.PolymorphicFieldArgs.CreateForProperty(base.Property, position, id);
			polymorphicArgs.AllowSceneObjects = allowSceneObjects;
			polymorphicArgs.ReadOnly = isReadOnly;
			polymorphicArgs.ShowBaseType = ShowBaseType;
			if (GlobalConfig<GeneralDrawerConfig>.Instance.useOldPolymorphicField)
			{
				EditorGUI.BeginChangeCheck();
				bool prev = EditorGUI.showMixedValue;
				if (base.ValueEntry.ValueState == PropertyValueState.ReferenceValueConflict)
				{
					EditorGUI.showMixedValue = true;
				}
				object newValue = OdinInternalEditorFields.PolymorphicObjectField(in polymorphicArgs);
				EditorGUI.showMixedValue = prev;
				if (EditorGUI.EndChangeCheck())
				{
					base.ValueEntry.Property.Tree.DelayActionUntilRepaint(delegate
					{
						base.ValueEntry.WeakValues[0] = newValue;
						for (int i = 1; i < base.ValueEntry.ValueCount; i++)
						{
							base.ValueEntry.WeakValues[i] = Sirenix.Serialization.SerializationUtility.CreateCopy(newValue);
						}
					});
				}
			}
			else
			{
				OdinInternalEditorFields.PolymorphicObjectField(in polymorphicArgs);
			}
			EditorGUI.showMixedValue = lastMixedValue;
		}

		private void DrawField(GUIContent label)
		{
			if (inlineAttribute != null)
			{
				DrawFieldInline(label);
				return;
			}
			Rect position = EditorGUILayout.GetControlRect();
			Rect labelPosition = Rect.zero;
			bool hasLabel = label != null && !string.IsNullOrEmpty(label.text);
			position = ((!hasLabel && (searchFilter == null || !drawChildren)) ? EditorGUI.IndentedRect(position) : SirenixEditorGUI.PrefixRect(position, out labelPosition));
			int id = GUIUtility.GetControlID(OdinInternalEditorFields.PolymorphicFieldHash, FocusType.Keyboard, position);
			GUIHelper.PushIndentLevel(0);
			if (drawChildren)
			{
				if (hasLabel)
				{
					if (searchFilter != null)
					{
						int additionalSize = ((!EditorGUIUtility.hierarchyMode) ? SirenixEditorGUI.FoldoutWidth : 0);
						DrawSearchFilter(labelPosition.TakeFromRight(labelPosition.width - EditorStyles.label.CalcWidth(label) - (float)additionalSize));
					}
					base.Property.State.Expanded = SirenixEditorGUI.Foldout(ref labelPosition, id, label, base.Property.State.Expanded, toggleOnLabelClick: true);
				}
				else if (searchFilter != null)
				{
					base.Property.State.Expanded = SirenixEditorGUI.Foldout(ref labelPosition, id, label, base.Property.State.Expanded, toggleOnLabelClick: true);
					DrawSearchFilter(labelPosition);
				}
				else
				{
					base.Property.State.Expanded = SirenixEditorGUI.Foldout(ref position, id, label, base.Property.State.Expanded, toggleOnLabelClick: true);
				}
			}
			else if (hasLabel)
			{
				EditorGUI.HandlePrefixLabel(labelPosition, labelPosition, label, id);
			}
			DrawReferencePicker(position, id);
			GUIHelper.PopIndentLevel();
			if (!drawChildren)
			{
				return;
			}
			bool toggle = base.ValueEntry.ValueState != PropertyValueState.NullReference && base.Property.State.Expanded;
			if (SirenixEditorGUI.BeginFadeGroup(this, toggle))
			{
				if (searchFilter != null && searchFilter.HasSearchResults)
				{
					searchFilter.DrawSearchResults();
				}
				else
				{
					EditorGUI.indentLevel++;
					if (hasLabel)
					{
						CallNextDrawer(null);
					}
					else
					{
						CallNextDrawer(null);
					}
					EditorGUI.indentLevel--;
				}
			}
			SirenixEditorGUI.EndFadeGroup();
		}

		private void DrawFieldInline(GUIContent label)
		{
			bool shouldPushLabelWidth = inlineAttribute.LabelWidth > 0;
			if (label == null || label == GUIContent.none)
			{
				if (shouldPushLabelWidth)
				{
					GUIHelper.PushLabelWidth(inlineAttribute.LabelWidth);
				}
				Rect position = EditorGUILayout.GetControlRect();
				int id = GUIUtility.GetControlID(OdinInternalEditorFields.PolymorphicFieldHash, FocusType.Keyboard, position);
				DrawReferencePicker(position, id);
				if (drawChildren)
				{
					CallNextDrawer(null);
				}
				if (shouldPushLabelWidth)
				{
					GUIHelper.PopLabelWidth();
				}
				return;
			}
			SirenixEditorGUI.BeginVerticalPropertyLayout(label);
			Rect position2 = EditorGUILayout.GetControlRect();
			int id2 = GUIUtility.GetControlID(OdinInternalEditorFields.PolymorphicFieldHash, FocusType.Keyboard, position2);
			DrawReferencePicker(position2, id2);
			if (shouldPushLabelWidth)
			{
				GUIHelper.PushLabelWidth(inlineAttribute.LabelWidth);
			}
			if (drawChildren)
			{
				CallNextDrawer(null);
			}
			if (shouldPushLabelWidth)
			{
				GUIHelper.PopLabelWidth();
			}
			GUILayout.Space(UnityVersion.IsVersionOrGreater(2019, 3) ? 5 : 4);
			SirenixEditorGUI.EndVerticalPropertyLayout();
		}

		private void DrawSearchFilter(Rect position)
		{
			if (searchFilter == null)
			{
				return;
			}
			position = position.Padding(2f, 0f);
			if (position.width < 16f)
			{
				GUI.Label(position, "...");
				return;
			}
			string newTerm = searchField.Draw(position, searchFilter.SearchTerm, "Find Property...");
			if (newTerm == searchFilter.SearchTerm)
			{
				return;
			}
			searchFilter.SearchTerm = newTerm;
			base.Property.Tree.DelayActionUntilRepaint(delegate
			{
				if (!string.IsNullOrEmpty(newTerm))
				{
					base.Property.State.Expanded = true;
				}
				searchFilter.UpdateSearch();
				GUIHelper.RequestRepaint();
			});
		}

		/// <summary>
		/// Returns a value that indicates if this drawer can be used for the given property.
		/// </summary>
		protected override bool CanDrawValueProperty(InspectorProperty property)
		{
			if (property.IsTreeRoot)
			{
				return false;
			}
			Type type = property.ValueEntry.BaseValueType;
			if ((type.IsClass || type.IsInterface) && type != typeof(string))
			{
				return !typeof(UnityEngine.Object).IsAssignableFrom(type);
			}
			return false;
		}

		private static bool ShouldDrawReferenceObjectPicker(IPropertyValueEntry<T> entry)
		{
			if (entry.SerializationBackend.SupportsPolymorphism && !entry.BaseValueType.IsValueType && entry.BaseValueType != typeof(string) && !(entry.Property.ChildResolver is ICollectionResolver) && !entry.BaseValueType.IsArray && entry.IsEditable && !entry.BaseValueType.InheritsFrom(typeof(IDictionary)) && !(entry.WeakSmartValue as UnityEngine.Object))
			{
				return entry.Property.GetAttribute<HideReferenceObjectPickerAttribute>() == null;
			}
			return false;
		}
	}
}
