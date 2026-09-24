using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Sirenix.OdinInspector.Editor.Internal;
using Sirenix.OdinInspector.Editor.Internal.UIToolkitIntegration;
using Sirenix.Serialization;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEditorInternal;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	/// <summary>
	/// Static GUI information reguarding the InlineEditor attribute.
	/// </summary>
	public static class InlineEditorAttributeDrawer
	{
		/// <summary>
		/// Gets a value indicating how many InlineEditors we are currently in.
		/// </summary>
		public static int CurrentInlineEditorDrawDepth { get; internal set; }
	}
	/// <summary>
	/// Draws properties marked with <see cref="T:Sirenix.OdinInspector.InlineEditorAttribute" />.
	/// </summary>
	/// <seealso cref="T:Sirenix.OdinInspector.InlineEditorAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.DrawWithUnityAttribute" />
	[DrawerPriority(0.0, 0.0, 3000.0)]
	public class InlineEditorAttributeDrawer<T> : OdinAttributeDrawer<InlineEditorAttribute, T>, IDisposable where T : UnityEngine.Object
	{
		private struct LayoutSettings
		{
			public GUISkin Skin;

			public Color Color;

			public Color ContentColor;

			public Color BackgroundColor;

			public bool Enabled;

			public int IndentLevel;

			public float FieldWidth;

			public float LabelWidth;

			public bool HierarchyMode;

			public bool WideMode;
		}

		public static readonly bool IsGameObject = typeof(T) == typeof(GameObject);

		private static Type animationClipEditorType = TwoWaySerializationBinder.Default.BindToType("UnityEditor.AnimationClipEditor");

		private static PropertyInfo materialForceVisibleProperty = typeof(MaterialEditor).GetProperty("forceVisible", BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);

		private static Stack<LayoutSettings> layoutSettingsStack = new Stack<LayoutSettings>();

		private UnityEditor.Editor editor;

		private UnityEditor.Editor previewEditor;

		private UnityEngine.Object target;

		private Rect inlineEditorRect;

		private Vector2 scrollPos;

		private bool drawHeader;

		private bool drawGUI;

		private bool drawPreview;

		private bool alwaysVisible;

		private bool targetIsOpenForEdit;

		private OdinImGuiElement element;

		private bool hasCheckedCurrentEditorForElement;

		private bool allowSceneObjects;

		private bool isAnimationClip;

		/// <summary>
		/// Initializes this instance.
		/// </summary>
		protected override void Initialize()
		{
			if (base.Attribute.ExpandedHasValue && InlineEditorAttributeDrawer.CurrentInlineEditorDrawDepth == 0)
			{
				base.Property.State.Expanded = base.Attribute.Expanded;
			}
			allowSceneObjects = InspectorPropertyInfoUtility.InspectorPropertySupportsAssigningSceneReferences(base.Property);
			isAnimationClip = base.Property.ValueEntry.TypeOfValue == typeof(AnimationClip);
		}

		/// <summary>
		/// Draws the property layout.
		/// </summary>
		/// <param name="label">The label.</param>
		protected override void DrawPropertyLayout(GUIContent label)
		{
			Rect valueRect;
			switch (base.Attribute.ObjectFieldMode)
			{
			case InlineEditorObjectFieldModes.Boxed:
				alwaysVisible = false;
				SirenixEditorGUI.BeginToolbarBox();
				SirenixEditorGUI.BeginToolbarBoxHeader();
				if ((bool)base.ValueEntry.SmartValue)
				{
					base.Property.State.Expanded = SirenixEditorGUI.Foldout(base.Property.State.Expanded, label, out valueRect);
					DrawPolymorphicObjectField(valueRect);
				}
				else
				{
					DrawPolymorphicObjectField(label);
				}
				SirenixEditorGUI.EndToolbarBoxHeader();
				GUIHelper.PushHierarchyMode(hierarchyMode: false);
				DrawEditor();
				GUIHelper.PopHierarchyMode();
				SirenixEditorGUI.EndToolbarBox();
				break;
			case InlineEditorObjectFieldModes.Foldout:
				alwaysVisible = false;
				if ((bool)base.ValueEntry.SmartValue)
				{
					base.Property.State.Expanded = SirenixEditorGUI.Foldout(base.Property.State.Expanded, label, out valueRect);
					DrawPolymorphicObjectField(valueRect);
				}
				else
				{
					DrawPolymorphicObjectField(label);
				}
				EditorGUI.indentLevel++;
				DrawEditor();
				EditorGUI.indentLevel--;
				break;
			case InlineEditorObjectFieldModes.Hidden:
				alwaysVisible = true;
				if (!(UnityEngine.Object)base.ValueEntry.WeakSmartValue)
				{
					DrawPolymorphicObjectField(label);
				}
				DrawEditor();
				break;
			case InlineEditorObjectFieldModes.CompletelyHidden:
				alwaysVisible = true;
				DrawEditor();
				break;
			}
		}

		private void DrawPolymorphicObjectField(Rect position)
		{
			if (base.ValueEntry.BaseValueType == typeof(object) || !typeof(UnityEngine.Object).IsAssignableFrom(base.ValueEntry.BaseValueType) || base.ValueEntry.BaseValueType.IsInterface)
			{
				OdinInternalEditorFields.PolymorphicFieldArgs polymorphicArgs = OdinInternalEditorFields.PolymorphicFieldArgs.CreateForProperty(base.Property, position, 0);
				polymorphicArgs.AllowSceneObjects = allowSceneObjects;
				if (GlobalConfig<GeneralDrawerConfig>.Instance.useOldPolymorphicField)
				{
					EditorGUI.BeginChangeCheck();
					object newValue = OdinInternalEditorFields.PolymorphicObjectField(in polymorphicArgs);
					if (!EditorGUI.EndChangeCheck())
					{
						return;
					}
					base.ValueEntry.Property.Tree.DelayActionUntilRepaint(delegate
					{
						base.ValueEntry.WeakValues[0] = newValue;
						for (int i = 1; i < base.ValueEntry.ValueCount; i++)
						{
							base.ValueEntry.WeakValues[i] = Sirenix.Serialization.SerializationUtility.CreateCopy(newValue);
						}
					});
				}
				else
				{
					OdinInternalEditorFields.PolymorphicObjectField(in polymorphicArgs);
				}
			}
			else
			{
				OdinInternalEditorFields.UnityObjectFieldArgs drawArgs = OdinInternalEditorFields.UnityObjectFieldArgs.CreateForProperty(base.Property, position, allowSceneObjects);
				if (GlobalConfig<GeneralDrawerConfig>.Instance.useOldUnityObjectField)
				{
					base.ValueEntry.WeakSmartValue = OdinInternalEditorFields.UnityObjectField(in drawArgs);
				}
				else
				{
					OdinInternalEditorFields.UnityObjectField(in drawArgs);
				}
			}
		}

		private void DrawPolymorphicObjectField(GUIContent label)
		{
			if (base.ValueEntry.BaseValueType == typeof(object) || !typeof(UnityEngine.Object).IsAssignableFrom(base.ValueEntry.BaseValueType) || base.ValueEntry.BaseValueType.IsInterface)
			{
				OdinInternalEditorFields.PolymorphicFieldArgs polymorphicArgs = OdinInternalEditorFields.PolymorphicFieldArgs.CreateForProperty(base.Property, EditorGUILayout.GetControlRect(), 0, label);
				polymorphicArgs.AllowSceneObjects = allowSceneObjects;
				if (GlobalConfig<GeneralDrawerConfig>.Instance.useOldPolymorphicField)
				{
					EditorGUI.BeginChangeCheck();
					object newValue = OdinInternalEditorFields.PolymorphicObjectField(in polymorphicArgs);
					if (!EditorGUI.EndChangeCheck())
					{
						return;
					}
					base.ValueEntry.Property.Tree.DelayActionUntilRepaint(delegate
					{
						base.ValueEntry.WeakValues[0] = newValue;
						for (int i = 1; i < base.ValueEntry.ValueCount; i++)
						{
							base.ValueEntry.WeakValues[i] = Sirenix.Serialization.SerializationUtility.CreateCopy(newValue);
						}
					});
				}
				else
				{
					OdinInternalEditorFields.PolymorphicObjectField(in polymorphicArgs);
				}
			}
			else
			{
				OdinInternalEditorFields.UnityObjectFieldArgs drawArgs = OdinInternalEditorFields.UnityObjectFieldArgs.CreateForProperty(base.Property, EditorGUILayout.GetControlRect(), allowSceneObjects, label);
				if (GlobalConfig<GeneralDrawerConfig>.Instance.useOldUnityObjectField)
				{
					base.ValueEntry.WeakSmartValue = OdinInternalEditorFields.UnityObjectField(in drawArgs);
				}
				else
				{
					OdinInternalEditorFields.UnityObjectField(in drawArgs);
				}
			}
		}

		private OdinImGuiElement TryCreateInspectorElementAndSetClasses(UnityEditor.Editor targetEditor)
		{
			if (targetEditor.GetType().GetMethod("CreateInspectorGUI", BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public) != null)
			{
				InspectorElement element = new InspectorElement(targetEditor);
				return new OdinImGuiElement(element);
			}
			return null;
		}

		private void DrawEditor()
		{
			T obj = base.ValueEntry.SmartValue;
			if (base.ValueEntry.ValueState == PropertyValueState.ReferencePathConflict)
			{
				SirenixEditorGUI.MessageBox("reference-path-conflict", MessageType.Info, GlobalConfig<GeneralDrawerConfig>.Instance.MessageBoxFontSize);
				return;
			}
			if (alwaysVisible || SirenixEditorGUI.BeginFadeGroup(this, base.Property.State.Expanded))
			{
				UpdateEditors();
				if (base.Attribute.MaxHeight != 0f)
				{
					scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayoutOptions.MaxHeight(200f));
				}
				bool prev = EditorGUI.showMixedValue;
				EditorGUI.showMixedValue = false;
				EditorGUI.BeginChangeCheck();
				DoTheDrawing();
				if (EditorGUI.EndChangeCheck())
				{
					PropertyValueEntry e = base.Property.BaseValueEntry;
					if (e != null)
					{
						for (int i = 0; i < e.ValueCount; i++)
						{
							e.TriggerOnChildValueChanged(i);
						}
					}
				}
				EditorGUI.showMixedValue = prev;
				if (base.Attribute.MaxHeight != 0f)
				{
					EditorGUILayout.EndScrollView();
				}
			}
			else if (editor != null)
			{
				DestroyEditors();
			}
			if (!alwaysVisible)
			{
				SirenixEditorGUI.EndFadeGroup();
			}
		}

		private void DoTheDrawing()
		{
			if (IsGameObject && !base.Attribute.DrawPreview)
			{
				SirenixEditorGUI.MessageBox("Odin does not currently have a full GameObject inspector window substitute implemented, so a GameObject's components cannot be directly inspected inline in the editor. Choose an InlineEditorMode that includes a preview to draw a GameObject preview.");
				OdinInternalEditorFields.UnityObjectFieldArgs drawArgs = OdinInternalEditorFields.UnityObjectFieldArgs.CreateForProperty(base.Property, EditorGUILayout.GetControlRect(), allowSceneObjects);
				drawArgs.BaseType = typeof(GameObject);
				OdinInternalEditorFields.UnityObjectField(in drawArgs);
				GUILayout.BeginHorizontal();
				GUIHelper.PushGUIEnabled(base.ValueEntry.SmartValue != null);
				string text = ((base.ValueEntry.SmartValue != null) ? ("Open Inspector window for " + base.ValueEntry.SmartValue.name) : "Open Inspector window (null)");
				if (GUILayout.Button(GUIHelper.TempContent(text)))
				{
					GUIHelper.OpenInspectorWindow(base.ValueEntry.SmartValue);
					GUIHelper.ExitGUI(removeFocusControl: true);
				}
				text = ((base.ValueEntry.SmartValue != null) ? ("Select " + base.ValueEntry.SmartValue.name) : "Select GO (null)");
				if (GUILayout.Button(GUIHelper.TempContent(text)))
				{
					Selection.activeObject = base.ValueEntry.SmartValue;
					GUIHelper.ExitGUI(removeFocusControl: true);
				}
				GUIHelper.PopGUIEnabled();
				GUILayout.EndHorizontal();
			}
			else
			{
				if (!(editor != null) || editor.SafeIsUnityNull())
				{
					return;
				}
				SaveLayoutSettings();
				InlineEditorAttributeDrawer.CurrentInlineEditorDrawDepth++;
				try
				{
					if (!targetIsOpenForEdit)
					{
						GUIHelper.PushGUIEnabled(enabled: false);
					}
					PreviewAlignment alignment = base.Attribute.PreviewAlignment;
					bool drawPreviewHorizontally = drawPreview && (alignment == PreviewAlignment.Left || alignment == PreviewAlignment.Right);
					bool drawPreviewVertically = drawPreview && (alignment == PreviewAlignment.Top || alignment == PreviewAlignment.Bottom);
					if (!drawGUI && drawPreviewHorizontally)
					{
						drawPreviewHorizontally = false;
						drawPreviewVertically = true;
						alignment = ((alignment == PreviewAlignment.Left) ? PreviewAlignment.Top : PreviewAlignment.Bottom);
					}
					if (drawPreviewHorizontally)
					{
						GUILayout.BeginHorizontal();
						if (base.Attribute.PreviewAlignment == PreviewAlignment.Left)
						{
							GUILayout.BeginVertical();
							DrawPreview(alignment);
							GUILayout.EndVertical();
						}
						GUILayout.BeginVertical();
					}
					else if (drawPreviewVertically && base.Attribute.PreviewAlignment == PreviewAlignment.Top)
					{
						DrawPreview(alignment);
					}
					if (drawHeader)
					{
						EventType tmp = Event.current.rawType;
						EditorGUILayout.BeginFadeGroup(0.9999f);
						Event.current.type = tmp;
						GUILayout.Space(0f);
						editor.DrawHeader();
						GUILayout.Space(1f);
						EditorGUILayout.EndFadeGroup();
					}
					else
					{
						GUIHelper.BeginDrawToNothing();
						editor.DrawHeader();
						GUIHelper.EndDrawToNothing();
					}
					if (drawGUI)
					{
						if (GlobalConfig<GeneralDrawerConfig>.Instance.EnableUIToolkitSupport && !hasCheckedCurrentEditorForElement)
						{
							hasCheckedCurrentEditorForElement = true;
							element = TryCreateInspectorElementAndSetClasses(editor);
						}
						if (GlobalConfig<GeneralDrawerConfig>.Instance.EnableUIToolkitSupport && element != null)
						{
							ImguiElementUtils.EmbedVisualElementAndDrawItHere(element);
						}
						else
						{
							bool prev = GlobalConfig<GeneralDrawerConfig>.Instance.ShowMonoScriptInEditor;
							try
							{
								GlobalConfig<GeneralDrawerConfig>.Instance.ShowMonoScriptInEditor = false;
								EditorGUILayout.BeginVertical();
								bool prevIsSet = InternalEditorUtility.GetIsInspectorExpanded(editor.target);
								if (!drawHeader)
								{
									InternalEditorUtility.SetIsInspectorExpanded(editor.target, isExpanded: true);
								}
								editor.OnInspectorGUI();
								if (!drawHeader)
								{
									InternalEditorUtility.SetIsInspectorExpanded(editor.target, prevIsSet);
								}
								EditorGUILayout.EndVertical();
							}
							finally
							{
								GlobalConfig<GeneralDrawerConfig>.Instance.ShowMonoScriptInEditor = prev;
							}
						}
					}
					if (drawPreviewHorizontally)
					{
						GUILayout.EndVertical();
						if (base.Attribute.PreviewAlignment == PreviewAlignment.Right)
						{
							GUILayout.BeginVertical();
							DrawPreview(alignment);
							GUILayout.EndVertical();
						}
						GUILayout.EndHorizontal();
					}
					else if (drawPreviewVertically && alignment == PreviewAlignment.Bottom)
					{
						DrawPreview(alignment);
					}
					if (!targetIsOpenForEdit)
					{
						GUIHelper.PopGUIEnabled();
					}
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
					InlineEditorAttributeDrawer.CurrentInlineEditorDrawDepth--;
					RestoreLayout();
				}
			}
		}

		private void DrawPreview(PreviewAlignment alignment)
		{
			bool isHorizontal = alignment == PreviewAlignment.Left || alignment == PreviewAlignment.Right;
			if (!drawPreview || (!previewEditor.HasPreviewGUI() && !(previewEditor.target is GameObject)))
			{
				return;
			}
			float size = (isHorizontal ? base.Attribute.PreviewWidth : base.Attribute.PreviewHeight);
			if (isAnimationClip)
			{
				if (isHorizontal)
				{
					if (size < 200f)
					{
						size = 200f;
					}
				}
				else if (size < 90f)
				{
					size = 90f;
				}
			}
			GUILayoutOption[] layoutOptions = GUILayoutOptions.EmptyGUIOptions;
			switch (alignment)
			{
			case PreviewAlignment.Left:
			case PreviewAlignment.Right:
				layoutOptions = GUILayoutOptions.Width(size).ExpandHeight();
				break;
			case PreviewAlignment.Top:
			case PreviewAlignment.Bottom:
				layoutOptions = GUILayoutOptions.ExpandWidth().Height(size);
				break;
			}
			Rect rect = EditorGUILayout.GetControlRect(hasLabel: false, size, layoutOptions);
			bool tmp = GUI.enabled;
			GUI.enabled = true;
			if (isAnimationClip && previewEditor.GetType() == animationClipEditorType)
			{
				DrawAnimationClipEditorPreview(rect);
			}
			else
			{
				previewEditor.DrawPreview(rect);
			}
			GUI.enabled = tmp;
		}

		/// <summary>
		///
		/// </summary>
		/// <param name="rect"></param>
		/// <remarks>Will set <see cref="P:UnityEngine.GUI.enabled">GUI.enabled</see> to false during some cases, to avoid the Preview eating events when it really shouldn't.</remarks>
		private void DrawAnimationClipEditorPreview(Rect rect)
		{
			if (!drawGUI)
			{
				GUIHelper.BeginDrawToNothing();
				previewEditor.OnInspectorGUI();
				GUIHelper.EndDrawToNothing();
			}
			EventType type = Event.current.type;
			if ((type == EventType.ScrollWheel || type == EventType.DragPerform) && !Event.current.IsMouseOver(rect))
			{
				GUI.enabled = false;
			}
			previewEditor.DrawPreview(rect);
		}

		private void UpdateEditors()
		{
			targetIsOpenForEdit = true;
			UnityEngine.Object unityObj = (UnityEngine.Object)base.ValueEntry.WeakSmartValue;
			if (editor != null && !unityObj)
			{
				DestroyEditors();
			}
			bool createNewEditor = unityObj != null && (editor == null || target != unityObj || target == null);
			if (createNewEditor && base.ValueEntry.ValueState == PropertyValueState.ReferenceValueConflict)
			{
				if (base.ValueEntry.WeakValues[0] == null)
				{
					createNewEditor = false;
				}
				if (createNewEditor)
				{
					Type type = base.ValueEntry.WeakValues[0].GetType();
					for (int i = 1; i < base.ValueEntry.ValueCount; i++)
					{
						if (!base.ValueEntry.Values[i] || base.ValueEntry.Values[i].GetType() != type)
						{
							createNewEditor = false;
							break;
						}
					}
				}
				if (!createNewEditor)
				{
					SirenixEditorGUI.MessageBox("Cannot perform multi-editing on objects of different type.", MessageType.Info, GlobalConfig<GeneralDrawerConfig>.Instance.MessageBoxFontSize);
				}
			}
			if (createNewEditor)
			{
				target = unityObj;
				bool isGameObject = unityObj as GameObject;
				drawHeader = (isGameObject ? base.Attribute.DrawHeader : base.Attribute.DrawHeader);
				drawGUI = !isGameObject && base.Attribute.DrawGUI;
				drawPreview = base.Attribute.DrawPreview || (isGameObject && base.Attribute.DrawGUI);
				if (editor != null)
				{
					DestroyEditors();
				}
				hasCheckedCurrentEditorForElement = false;
				editor = UnityEditor.Editor.CreateEditor(base.ValueEntry.WeakValues.FilterCast<UnityEngine.Object>().ToArray());
				Component component = target as Component;
				if (component != null)
				{
					previewEditor = UnityEditor.Editor.CreateEditor(component.gameObject);
				}
				else
				{
					previewEditor = editor;
				}
				MaterialEditor materialEditor = editor as MaterialEditor;
				if (materialEditor != null && materialForceVisibleProperty != null)
				{
					materialForceVisibleProperty.SetValue(materialEditor, true, null);
				}
				if (base.Attribute.DisableGUIForVCSLockedAssets && AssetDatabase.Contains(target))
				{
					targetIsOpenForEdit = AssetDatabase.IsOpenForEdit(target);
				}
			}
		}

		private void DestroyEditors()
		{
			targetIsOpenForEdit = true;
			if (previewEditor != editor && previewEditor != null)
			{
				try
				{
					UnityEngine.Object.DestroyImmediate(previewEditor);
				}
				catch
				{
				}
				previewEditor = null;
			}
			if (!(editor != null))
			{
				return;
			}
			if (element != null)
			{
				OdinImGuiElement capture = element;
				element = null;
				EditorApplication.delayCall = (EditorApplication.CallbackFunction)Delegate.Combine(EditorApplication.delayCall, (EditorApplication.CallbackFunction)delegate
				{
					if (capture.parent != null)
					{
						capture?.RemoveFromHierarchy();
					}
				});
			}
			try
			{
				UnityEngine.Object.DestroyImmediate(editor);
			}
			catch (Exception)
			{
			}
			editor = null;
		}

		private static void SaveLayoutSettings()
		{
			layoutSettingsStack.Push(new LayoutSettings
			{
				Skin = GUI.skin,
				Color = GUI.color,
				ContentColor = GUI.contentColor,
				BackgroundColor = GUI.backgroundColor,
				Enabled = GUI.enabled,
				IndentLevel = EditorGUI.indentLevel,
				FieldWidth = EditorGUIUtility.fieldWidth,
				LabelWidth = GUIHelper.ActualLabelWidth,
				HierarchyMode = EditorGUIUtility.hierarchyMode,
				WideMode = EditorGUIUtility.wideMode
			});
		}

		private static void RestoreLayout()
		{
			LayoutSettings settings = layoutSettingsStack.Pop();
			GUI.skin = settings.Skin;
			GUI.color = settings.Color;
			GUI.contentColor = settings.ContentColor;
			GUI.backgroundColor = settings.BackgroundColor;
			GUI.enabled = settings.Enabled;
			EditorGUI.indentLevel = settings.IndentLevel;
			EditorGUIUtility.fieldWidth = settings.FieldWidth;
			GUIHelper.BetterLabelWidth = settings.LabelWidth;
			EditorGUIUtility.hierarchyMode = settings.HierarchyMode;
			EditorGUIUtility.wideMode = settings.WideMode;
		}

		void IDisposable.Dispose()
		{
			DestroyEditors();
		}
	}
}
