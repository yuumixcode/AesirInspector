using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Sirenix.Reflection.Editor;
using Sirenix.Serialization;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Internal
{
	internal class DesignerEditorWindow : OdinEditorWindow
	{
		[Flags]
		public enum ResizeEdge
		{
			None = 0,
			Left = 1,
			Right = 2,
			Top = 4,
			Bottom = 8
		}

		[NonSerialized]
		public DesignerEditor Editor;

		[NonSerialized]
		public DesignerEditor ActiveEditor;

		[HideInInspector]
		public string EditorTypeName;

		[HideInInspector]
		public string EditorTypeGuid;

		[HideInInspector]
		public OdinEntityId EditorTargetEntityId;

		[HideInInspector]
		public string EditorTargetPath;

		[HideInInspector]
		public uint LastSavedTick;

		[HideInInspector]
		public float BreadcrumbScrollOffset;

		[HideInInspector]
		public bool LockNonDeclMembers;

		[MenuItem("CONTEXT/MonoBehaviour/Odin/Customize in Visual Designer", true)]
		public static bool ValidateCustomizeMonoBehaviour(MenuCommand menuCommand)
		{
			UnityEngine.Object context = menuCommand.context;
			if (context != null)
			{
				return DesignerUtils.CanTypeBeDesigned(context.GetType());
			}
			return false;
		}

		[MenuItem("CONTEXT/MonoBehaviour/Odin/Customize in Visual Designer")]
		public static void CustomizeMonoBehaviour(MenuCommand menuCommand)
		{
			UnityEngine.Object context = menuCommand.context;
			Type type = context?.GetType();
			if (!(type == null) && (GuessIfTypeIsDesignable(type) || EditorUtility.DisplayDialog("Odin Visual Designer", $"The type '{type}' is not recognized by the designer as a type it can design. Do you want to continue?", "Yes", "No")))
			{
				DesignerEditors.Get(context.GetType(), context, null)?.OpenWindow();
			}
		}

		private static bool GuessIfTypeIsDesignable(Type type)
		{
			if (typeof(UnityEngine.Object).IsAssignableFrom(type))
			{
				if (type.IsAbstract || type.IsGenericType)
				{
					return true;
				}
				if (!InspectorTypeDrawingConfigDrawer.IsOdinDrawingType(type))
				{
					return InspectorTypeDrawingConfigDrawer.OdinCanCreateEditorFor(type);
				}
				return true;
			}
			return true;
		}

		protected override void Initialize()
		{
			base.Initialize();
			if (EditorTypeName != null && Editor == null)
			{
				Type type = TwoWaySerializationBinder.Default.BindToType(EditorTypeName);
				if (type == null)
				{
					type = DesignerUtils.GetTypeFromScriptGuid(EditorTypeGuid);
				}
				if (!(type == null))
				{
					object instance = ((!EditorTargetEntityId.IsValid) ? null : EditorTargetEntityId.ToObject());
					Editor = DesignerEditors.Get(type, instance, EditorTargetPath);
					TypePatch typePatch = TypePatchCache.Get(type);
					typePatch.EditorVariant = Editor.TypePatch;
					SetupWindow(Editor);
				}
			}
		}

		public void SetupWindow(DesignerEditor editor)
		{
			base.titleContent = new GUIContent("Odin Visual Designer");
			Editor = editor;
			EditorTypeName = TwoWaySerializationBinder.Default.BindToName(editor.TypePatch.TargetType);
			EditorTypeGuid = editor.TypePatch.TargetGuid;
			EditorTargetEntityId = ((editor.Instance is UnityEngine.Object uobj) ? OdinEntityId.FromObject(uobj) : OdinEntityId.None);
			EditorTargetPath = editor.InstancePath;
			Editor.Window = this;
			LockNonDeclMembers = GlobalConfig<OdinVisualDesignerConfig>.Instance.LockNonDeclTypesByDefault;
			editor.RetainIn(this);
			editor.RetainAncestorsIn(this);
		}

		protected override void OnImGUI()
		{
			base.OnImGUI();
			Rect rect = base.position.SetPosition(Vector2.zero);
			if (Editor == null || Editor.TypePatch == null)
			{
				DrawEditorCouldNotBeFound(rect);
				return;
			}
			if (ActiveEditor == null)
			{
				ActiveEditor = Editor;
				ActiveEditor.ForceSync = true;
			}
			EditorGUI.DrawRect(rect, Colors.Editor.Bg);
			ActiveEditor.Context.IsNonDeclMembersLocked = LockNonDeclMembers;
			Rect headerRect = rect.TakeFromTop(35f);
			Rect breadcrumbRect = rect.TakeFromTop(35f);
			Rect footerRect = rect.TakeFromBottom(35f);
			ToastPopupArea = rect;
			DrawHeader(headerRect);
			DrawBreadcrumbs(breadcrumbRect);
			DrawFooter(footerRect);
			ActiveEditor.OnGUI(rect, this);
			if (Event.current.type != EventType.Layout)
			{
				return;
			}
			OdinVisualDesignerConfig config = GlobalConfig<OdinVisualDesignerConfig>.Instance;
			if (config.AutoSave && ActiveEditor != null && ActiveEditor.TypePatch != null && ActiveEditor.TypePatch.IsDirty)
			{
				int saveMsInterval = config.AutoSaveInterval * 1000;
				uint currentTick = (uint)Environment.TickCount;
				if (GUIUtility.hotControl == 0 && !EditorGUIUtility.editingTextField && currentTick - LastSavedTick >= saveMsInterval)
				{
					ActiveEditor.SaveChanges(isTriggeredByAutoSave: true, this);
					LastSavedTick = currentTick;
				}
			}
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();
			List<DesignerEditor> snapshot = DesignerEditors.SnapshotCurrentEditorsTmp();
			foreach (DesignerEditor editor in snapshot)
			{
				editor.ReleaseFrom(this);
			}
			Editor?.ReleaseFrom(this);
		}

		/// <summary>
		/// Handles the window's linux-like drag behaviour.
		/// </summary>
		/// <param name="isDragging">Are we currently dragging (provided by the calling window)</param>
		/// <param name="dragStartPosition">Where did the drag start (provided by the calling window)</param>
		/// <param name="editorWindow">The window to move</param>
		/// <param name="controlID">A passive control id to make sure the window keeps getting events while dragging (provided by the calling window)</param>
		/// <returns>A bool so the calling window can update its "isDragging" variable and a Vector2 so the calling window can update its "dragStartPosition" variable</returns>
		public static (bool, Vector2) HandleWindowMovement(bool isDragging, Vector2 dragStartPosition, EditorWindow editorWindow, int controlID)
		{
			Event e = Event.current;
			switch (e.type)
			{
			case EventType.MouseDown:
				isDragging = true;
				dragStartPosition = GUIUtility.GUIToScreenPoint(e.mousePosition - editorWindow.position.position);
				if (GUIUtility.hotControl == 0)
				{
					GUIUtility.hotControl = controlID;
				}
				e.Use();
				break;
			case EventType.MouseDrag:
				if (isDragging)
				{
					Vector2 screenSpaceMousePos = GUIUtility.GUIToScreenPoint(e.mousePosition);
					UnityShims.Rect.Ctor(out var newRect, screenSpaceMousePos - dragStartPosition, editorWindow.position.size);
					newRect = newRect;
					editorWindow.position = newRect;
					e.Use();
				}
				break;
			case EventType.MouseUp:
				if (isDragging)
				{
					isDragging = false;
					GUIUtility.hotControl = 0;
					e.Use();
				}
				break;
			}
			return (isDragging, dragStartPosition);
		}

		public static (bool isResizing, ResizeEdge activeEdge, Vector2 startMouseScreen, Rect startWindowRect) HandleWindowResizing(bool isResizing, ResizeEdge activeEdge, Vector2 startMouseScreen, Rect startWindowRect, EditorWindow editorWindow, int controlID, Rect windowRectLocal, ResizeEdge allowedEdges = ResizeEdge.Left | ResizeEdge.Right | ResizeEdge.Top | ResizeEdge.Bottom, float edgeThickness = 6f, float minWidth = 240f, float minHeight = 120f)
		{
			Event e = Event.current;
			Vector2 mousePosition = e.mousePosition;
			if (isResizing && e.rawType == EventType.MouseUp)
			{
				isResizing = false;
				activeEdge = ResizeEdge.None;
				if (GUIUtility.hotControl == controlID)
				{
					GUIUtility.hotControl = 0;
				}
				if (e.type != EventType.Used)
				{
					e.Use();
				}
				return (isResizing: isResizing, activeEdge: activeEdge, startMouseScreen: startMouseScreen, startWindowRect: startWindowRect);
			}
			bool overLeft = mousePosition.x <= edgeThickness;
			bool overRight = mousePosition.x >= windowRectLocal.width - edgeThickness;
			bool overTop = mousePosition.y <= edgeThickness;
			bool overBottom = mousePosition.y >= windowRectLocal.height - edgeThickness;
			ResizeEdge hoveredEdge = ResizeEdge.None;
			if (overLeft && (allowedEdges & ResizeEdge.Left) != ResizeEdge.None)
			{
				hoveredEdge |= ResizeEdge.Left;
			}
			if (overRight && (allowedEdges & ResizeEdge.Right) != ResizeEdge.None)
			{
				hoveredEdge |= ResizeEdge.Right;
			}
			if (overTop && (allowedEdges & ResizeEdge.Top) != ResizeEdge.None)
			{
				hoveredEdge |= ResizeEdge.Top;
			}
			if (overBottom && (allowedEdges & ResizeEdge.Bottom) != ResizeEdge.None)
			{
				hoveredEdge |= ResizeEdge.Bottom;
			}
			if (!isResizing && hoveredEdge != ResizeEdge.None)
			{
				MouseCursor cursor = MouseCursor.Arrow;
				bool left = (hoveredEdge & ResizeEdge.Left) != 0;
				bool right = (hoveredEdge & ResizeEdge.Right) != 0;
				bool top = (hoveredEdge & ResizeEdge.Top) != 0;
				bool bottom = (hoveredEdge & ResizeEdge.Bottom) != 0;
				if ((left && top) || (right && bottom))
				{
					cursor = MouseCursor.ResizeUpLeft;
				}
				else if ((right && top) || (left && bottom))
				{
					cursor = MouseCursor.ResizeUpRight;
				}
				else if (left || right)
				{
					cursor = MouseCursor.ResizeHorizontal;
				}
				else if (top || bottom)
				{
					cursor = MouseCursor.ResizeVertical;
				}
				EditorGUIUtility.AddCursorRect(windowRectLocal, cursor);
			}
			switch (e.type)
			{
			case EventType.MouseDown:
				if (e.button == 0 && hoveredEdge != ResizeEdge.None && GUIUtility.hotControl == 0)
				{
					isResizing = true;
					activeEdge = hoveredEdge;
					startMouseScreen = GUIUtility.GUIToScreenPoint(e.mousePosition);
					startWindowRect = editorWindow.position;
					GUIUtility.hotControl = controlID;
					e.Use();
				}
				break;
			case EventType.MouseDrag:
			{
				if (!isResizing || GUIUtility.hotControl != controlID)
				{
					break;
				}
				Vector2 mouseScreen = GUIUtility.GUIToScreenPoint(e.mousePosition);
				Vector2 delta = mouseScreen - startMouseScreen;
				Rect rect = startWindowRect;
				bool left2 = (activeEdge & ResizeEdge.Left) != 0;
				bool right2 = (activeEdge & ResizeEdge.Right) != 0;
				bool top2 = (activeEdge & ResizeEdge.Top) != 0;
				bool bottom2 = (activeEdge & ResizeEdge.Bottom) != 0;
				if (left2)
				{
					rect.xMin += delta.x;
				}
				if (right2)
				{
					rect.xMax += delta.x;
				}
				if (top2)
				{
					rect.yMin += delta.y;
				}
				if (bottom2)
				{
					rect.yMax += delta.y;
				}
				if (rect.width < minWidth)
				{
					if (left2 && !right2)
					{
						rect.xMin = rect.xMax - minWidth;
					}
					else
					{
						rect.xMax = rect.xMin + minWidth;
					}
				}
				if (rect.height < minHeight)
				{
					if (top2 && !bottom2)
					{
						rect.yMin = rect.yMax - minHeight;
					}
					else
					{
						rect.yMax = rect.yMin + minHeight;
					}
				}
				editorWindow.position = rect;
				e.Use();
				break;
			}
			case EventType.MouseUp:
				if (isResizing && GUIUtility.hotControl == controlID)
				{
					isResizing = false;
					activeEdge = ResizeEdge.None;
					GUIUtility.hotControl = 0;
					e.Use();
				}
				break;
			}
			return (isResizing: isResizing, activeEdge: activeEdge, startMouseScreen: startMouseScreen, startWindowRect: startWindowRect);
		}

		public void DrawHeader(Rect rect)
		{
			EditorGUI.DrawRect(rect, Colors.Editor.HeaderBg);
			if (ActiveEditor == null)
			{
				return;
			}
			DesignerEditorContext context = ActiveEditor.Context;
			if (context == null)
			{
				return;
			}
			Rect contentRect = rect.Padding(5f);
			Rect reportBugBtnRect = contentRect.TakeFromRight(contentRect.height);
			contentRect.width -= 4f;
			Rect editClassBtnRect = contentRect.AlignRight(contentRect.height).SubX(contentRect.height + 8f);
			if (DesignerGUI.DrawIconSlideButton(reportBugBtnRect, $"{GetHashCode()}bugbutton", DesignerGUI.Tooltips.BugReport, SdfIconType.BugFill, SdfIconType.Discord, new Color(0.7f, 0.21f, 0.21f), Color.clear))
			{
				Application.OpenURL("https://discord.gg/JFFkCr3S8q");
			}
			EditorTypePatch editorPatch = ActiveEditor.TypePatch;
			if (!(editorPatch == null))
			{
				if (OVDFFileWatcher.TryGetActiveFileRecord(editorPatch.TargetType, out var record) && record.IsReadOnly)
				{
					Rect readOnlyRect = contentRect.AlignCenter(240f);
					Rect iconRect = readOnlyRect.TakeFromLeft(readOnlyRect.height);
					SdfIcons.DrawIcon(iconRect.Padding(5f), SdfIconType.LockFill, SirenixGUIStyles.YellowWarningColor);
					GUI.Label(readOnlyRect, GUIHelper.TempContent("Read-only OVDF", record.File.Path), SirenixGUIStyles.LabelCentered);
				}
				Rect editTypeBtnRect = contentRect.AlignLeft(contentRect.height);
				Rect editElementBtnRect = contentRect.AlignLeft(contentRect.height).AddX(contentRect.height + 8f);
				Rect searchRect = contentRect.Expand(1f, 4f);
				searchRect.yMin -= 1f;
				searchRect.yMax += 1f;
				searchRect.xMin -= 2f;
				ActiveEditor.SearchRect = searchRect;
			}
		}

		public void DrawBreadcrumbs(Rect rect)
		{
			EditorGUI.DrawRect(rect, Colors.Editor.HeaderBg);
			EditorGUI.DrawRect(rect, new Color(0f, 0f, 0f, 0.2f));
			EditorGUI.DrawRect(rect.TakeFromBottom(1f), SirenixGUIStyles.BorderColor);
			Event e = Event.current;
			EditorTypePatch editorPatch = Editor?.TypePatch;
			if (editorPatch == null)
			{
				return;
			}
			float nextX = 4f;
			DesignerEditorContext activeCtx = ActiveEditor?.Context;
			Type hoverType = activeCtx.HoverNode?.DeclaringType ?? activeCtx.SelectedNode?.DeclaringType;
			GUI.BeginClip(rect);
			TypePatch nonEditorPatch = TypePatchCache.Get(editorPatch.TargetType);
			int hierarchyIndex = 0;
			foreach (TypePatch typePatch in nonEditorPatch.TraverseFromRoot())
			{
				Color color = Colors.BreadcrumbColors[hierarchyIndex % Colors.BreadcrumbColors.Length];
				if (typePatch == nonEditorPatch)
				{
					color = Color.white;
				}
				bool isHoverEditor = typePatch != nonEditorPatch && hoverType != null && hoverType == typePatch.TargetType;
				string name = typePatch.TargetType.GetNiceName();
				bool isDirty = typePatch.HasEditorVariant && typePatch.EditorVariant.IsDirty;
				bool showChevron = !typePatch.TargetType.IsValueType;
				GUIStyle textStyle = (isDirty ? SirenixGUIStyles.BoldLabel : SirenixGUIStyles.Label);
				float nameWidth = textStyle.CalcWidth(name);
				float width = 16f + nameWidth + 16f;
				float contentWidth = width;
				if (showChevron)
				{
					width += rect.height;
				}
				Rect crumbRect = new Rect(nextX + BreadcrumbScrollOffset, 0f, width, rect.height);
				nextX += width;
				Rect contentRect = crumbRect.AlignLeft(contentWidth);
				Rect editButtonRect = contentRect.Padding(0f, 4f);
				if (SirenixEditorGUI.DoButton(editButtonRect, GUIUtility.GetControlID(FocusType.Passive), out var isHovering, out var isActive))
				{
					if (typePatch == nonEditorPatch)
					{
						ActiveEditor = Editor;
					}
					else
					{
						ActiveEditor = DesignerEditors.Get(typePatch.TargetType, null, null);
					}
					if (ActiveEditor != null)
					{
						ActiveEditor.ForceSync = true;
						ActiveEditor.RetainIn(this);
					}
				}
				if (e.OnContextClick(editButtonRect))
				{
					DesignerEditor editor = DesignerEditors.Get(typePatch.TargetType, null, null);
					if (editor != null)
					{
						if (editor != ActiveEditor)
						{
							editor.Sync(updateSelectionAttributes: true);
						}
						Type editorTargetType = editor.TypePatch.TargetType;
						GenericMenu genericMenu = new GenericMenu();
						GUIContent editAttributesContent = new GUIContent("Edit Attributes");
						if (!editorTargetType.IsGenericType || !editorTargetType.ContainsGenericParameters)
						{
							genericMenu.AddItem(editAttributesContent, on: false, delegate
							{
								DesignerEditor activeEditor = ActiveEditor;
								activeEditor.OnNextFrame = (Action)Delegate.Combine(activeEditor.OnNextFrame, (Action)delegate
								{
									editor.RetainIn(this);
									DesignerEditorContext context = editor.Context;
									context.Select(editor.RootNode);
									editor.OpenPopup(Rect.zero, editor.RootNode);
								});
							});
						}
						else
						{
							genericMenu.AddDisabledItem(editAttributesContent);
						}
						if (editor.ElementType != null)
						{
							Type elementType = editor.ElementType;
							if (editor.IsGenericClosedByDesigner)
							{
								genericMenu.AddDisabledItem(new GUIContent("Customize Element Type"));
							}
							else if (!elementType.IsGenericType)
							{
								DesignerUtils.AddGenericMeuItemEditType(genericMenu, "Customize Element Type", elementType);
							}
							else
							{
								Type genericTypeDef = elementType.GetGenericTypeDefinition();
								if (genericTypeDef == typeof(EditableKeyValuePair<, >))
								{
									Type[] genericArgs = elementType.GetGenericArguments();
									if (genericArgs.Length == 2)
									{
										DesignerUtils.AddGenericMeuItemEditType(genericMenu, "Customize Key Type", genericArgs[0]);
										DesignerUtils.AddGenericMeuItemEditType(genericMenu, "Customize Value Type", genericArgs[1]);
									}
								}
								else
								{
									genericMenu.AddDisabledItem(new GUIContent("Customize Element Type"));
								}
							}
						}
						genericMenu.ShowAsContext();
					}
				}
				if (isHovering)
				{
					SirenixEditorGUI.DrawRoundRect(editButtonRect, new Color(0f, 0f, 0f, 0.2f), 3f);
				}
				if (isActive)
				{
					SirenixEditorGUI.DrawRoundRect(editButtonRect, new Color(0f, 0f, 0f, 0.1f), 3f);
				}
				Rect paddedContentRect = contentRect;
				paddedContentRect.xMin += 16f;
				paddedContentRect.xMax -= 16f;
				if (isHoverEditor)
				{
					EditorGUI.DrawRect(editButtonRect.AlignCenter(textStyle.CalcWidth(name), 1f).AddY(textStyle.lineHeight * 0.5f + 1f), EditorGUIUtility.isProSkin ? new Color(0.8f, 0.8f, 0.8f, 1f) : new Color(0.2f, 0.2f, 0.2f, 1f));
				}
				GUIHelper.PushContentColor(color);
				Rect labelRect = paddedContentRect.AlignLeft(nameWidth);
				GUI.Label(labelRect, name, textStyle);
				GUIHelper.PopContentColor();
				if (showChevron)
				{
					Rect chevronRect = crumbRect.AlignRight(rect.height);
					if (e.IsHovering(chevronRect))
					{
						SirenixEditorGUI.DrawRoundRect(chevronRect.Padding(4f), new Color(0f, 0f, 0f, 0.2f), 3f);
					}
					SdfIcons.DrawIcon(chevronRect.Padding(10f), SdfIconType.ChevronRight);
					if (GUI.Button(chevronRect.Padding(4f), GUIContent.none, GUIStyle.none))
					{
						DesignerInheritorSelector selector = new DesignerInheritorSelector(typePatch.TargetType);
						selector.SelectionConfirmed += delegate(IEnumerable<DesignerInheritorItem> items)
						{
							DesignerInheritorItem designerInheritorItem = items.FirstOrDefault();
							if (designerInheritorItem != null)
							{
								Type type = designerInheritorItem.Type;
								if (!(type == null))
								{
									DesignerEditor designerEditor = DesignerEditors.Get(type, null, null);
									if (designerEditor != Editor && designerEditor != null)
									{
										DesignerEditorWindow designerEditorWindow = designerEditor.OpenWindow();
										designerEditorWindow.position = base.position;
										Close();
										designerEditorWindow.Focus();
										GUIHelper.ExitGUI(removeFocusControl: true);
									}
								}
							}
						};
						selector.ShowInPopup(chevronRect.AlignCenterX(350f));
					}
				}
				if (ActiveEditor != null && ActiveEditor.TypePatch != null && typePatch.TargetType == ActiveEditor.TypePatch.TargetType)
				{
					SirenixEditorGUI.DrawRoundRect(editButtonRect, Color.clear, 4f, new Color(1f, 1f, 1f, 0.2f), 1f);
				}
				hierarchyIndex++;
			}
			GUI.EndClip();
			if (nextX <= rect.width)
			{
				BreadcrumbScrollOffset = 0f;
				return;
			}
			if (Event.current.type == EventType.ScrollWheel)
			{
				BreadcrumbScrollOffset -= Event.current.delta.y * 30f;
			}
			if (BreadcrumbScrollOffset > 0f)
			{
				BreadcrumbScrollOffset = 0f;
			}
			float maxScroll = rect.width - nextX - 16f;
			if (BreadcrumbScrollOffset < maxScroll)
			{
				BreadcrumbScrollOffset = maxScroll;
			}
			if (BreadcrumbScrollOffset > maxScroll)
			{
				Color c1 = Colors.Shadow;
				GUI.DrawTexture(rect.AlignRight(14f), DesignerTextures.RightToLeftFade, ScaleMode.StretchToFill, alphaBlend: true, 1f, c1, 0f, 0f);
			}
			if (BreadcrumbScrollOffset < 0f)
			{
				Color c2 = Colors.Shadow;
				GUI.DrawTexture(rect.AlignLeft(14f), DesignerTextures.LeftToRightFade, ScaleMode.StretchToFill, alphaBlend: true, 1f, c2, 0f, 0f);
			}
		}

		public void DrawFooter(Rect rect)
		{
			EditorGUI.DrawRect(rect, Colors.Editor.FooterBg);
			EditorGUI.DrawRect(rect.AlignTop(1f), SirenixGUIStyles.BorderColor);
			DesignerEditorContext context = ActiveEditor?.Context;
			if (context == null)
			{
				return;
			}
			rect = rect.Padding(6f);
			Rect legendRect = rect.TakeFromRight(179f).VerticalPadding(4f);
			Rect methodRect = legendRect.TakeFromRight(64f);
			Rect propertyRect = legendRect.TakeFromRight(70f);
			Rect fieldRect = legendRect.TakeFromRight(45f);
			Rect saveRect = rect.TakeFromLeft(179f);
			if (DesignerGUI.DrawHaloButton(saveRect, $"{GetHashCode()}saveRect_Halo", "Save Changes", DesignerGUI.Tooltips.SaveChanges))
			{
				ActiveEditor.SaveChanges(isTriggeredByAutoSave: false, this);
			}
			Event e = Event.current;
			if (e.IsMouseOver(methodRect))
			{
				context.MemberTypeToHighlight = MemberTypes.Method;
			}
			else if (e.IsMouseOver(propertyRect))
			{
				context.MemberTypeToHighlight = MemberTypes.Property;
			}
			else if (e.IsMouseOver(fieldRect))
			{
				context.MemberTypeToHighlight = MemberTypes.Field;
			}
			else
			{
				context.MemberTypeToHighlight = (MemberTypes)0;
			}
			DesignerGUI.DrawMemberAccent(methodRect.TakeFromRight(5f), Colors.Accents.Method);
			methodRect.TakeFromRight(4f);
			GUI.Label(methodRect, "Method", SirenixGUIStyles.TitleRight);
			DesignerGUI.DrawMemberAccent(propertyRect.TakeFromRight(5f), Colors.Accents.Properties);
			propertyRect.TakeFromRight(5f);
			GUI.Label(propertyRect, "Property", SirenixGUIStyles.TitleRight);
			DesignerGUI.DrawMemberAccent(fieldRect.TakeFromRight(5f), Colors.Accents.Fields);
			fieldRect.TakeFromRight(5f);
			GUI.Label(fieldRect, "Field", SirenixGUIStyles.TitleRight);
			Rect rectCenterButton = rect;
			rectCenterButton = rectCenterButton.AlignCenterX(rectCenterButton.height);
			Rect eyeToggleRect = rectCenterButton;
			Rect addGroupRect = rectCenterButton;
			Rect lockRect = rectCenterButton;
			eyeToggleRect.x -= rectCenterButton.width + 8f;
			lockRect.x += rectCenterButton.width + 8f;
			bool pressedEye = DesignerGUI.DrawHaloButton(eyeToggleRect, "ToggleShowHideMode", string.Empty, DesignerGUI.Tooltips.ToggleVisibility);
			SdfIcons.DrawIcon(eyeToggleRect.AlignCenter(16f, 16f), SdfIconType.EyeFill, Colors.Icons.Default);
			bool pressedGroup = DesignerGUI.DrawHaloButton(addGroupRect, "AddNewGroup", string.Empty, DesignerGUI.Tooltips.AddGroup);
			SdfIcons.DrawIcon(addGroupRect.AlignCenter(16f, 16f), SdfIconType.PlusSquare, Colors.Icons.Default);
			bool pressedLock = DesignerGUI.DrawHaloButton(lockRect, "LockToggle", string.Empty, "Toggle to lock or unlock inherited members. When the lock is filled, inherited members cannot be edited; when the lock is open, they can be edited.");
			SdfIcons.DrawIcon(lockRect.AlignCenter(16f, 16f), LockNonDeclMembers ? SdfIconType.LockFill : SdfIconType.UnlockFill, Colors.Icons.Default);
			if (pressedEye)
			{
				context.IsShowHideMode = !context.IsShowHideMode;
				context.Editor.ForceSync = true;
				context.RebuildFilteredNodes(context.Editor.RootNode);
			}
			if (pressedGroup)
			{
				List<Type> groups = (from t in TypeCache.GetTypesDerivedFrom<PropertyGroupAttribute>()
					where !DesignerRegistry.ExcludedGroups.Contains(t)
					select t).ToList();
				GenericSelector<Type> selector = new GenericSelector<Type>("", groups, supportsMultiSelect: false, DesignerGUI.AttributeLabelCache.GetLabel);
				selector.EnableSingleClickToSelect();
				selector.ShowInPopup(addGroupRect.Expand(200f, 0f));
				selector.SelectionConfirmed += delegate(IEnumerable<Type> types)
				{
					Type type = types.FirstOrDefault();
					if (!(type == null))
					{
						context.AddGroup(type);
						DesignerEditor editor = context.Editor;
						editor.OnNextFrame = (Action)Delegate.Combine(editor.OnNextFrame, (Action)delegate
						{
							context.Editor.ScrollView.ScrollToLastItem();
							KeyValuePair<string, DesignerEditorNode> keyValuePair = context.Editor.Nodes.LastOrDefault();
							context.Select(keyValuePair.Value);
						});
					}
				};
			}
			if (pressedLock)
			{
				LockNonDeclMembers = !LockNonDeclMembers;
			}
		}

		public void DrawEditorCouldNotBeFound(Rect rect)
		{
			string errorMessage = (string.IsNullOrEmpty(EditorTypeName) ? "Target type for the current Editor could not be found" : ("Target type <b>" + EditorTypeName + "</b> for the current Editor could not be found"));
			float maxWidth = rect.width - 32f;
			Vector2 errorMessageSize = new Vector2(maxWidth, DesignerStyles.RichLabelCenteredWordWrap.CalcHeight(errorMessage, maxWidth));
			Vector2 line0Size = new Vector2(maxWidth, DesignerStyles.RichLabelCenteredWordWrap.CalcHeight("This may have happened because:", maxWidth));
			float maxItemWidth = SirenixGUIStyles.RichTextLabel.CalcWidth("• The type was moved to a different assembly or namespace.");
			if (maxItemWidth >= maxWidth)
			{
				maxItemWidth = maxWidth;
			}
			Vector2 line1Size = new Vector2(maxItemWidth, SirenixGUIStyles.RichTextLabel.CalcHeight("• The type was renamed.", maxItemWidth));
			Vector2 line2Size = new Vector2(maxItemWidth, SirenixGUIStyles.RichTextLabel.CalcHeight("• The type was moved to a different assembly or namespace.", maxItemWidth));
			Vector2 line3Size = new Vector2(maxItemWidth, SirenixGUIStyles.RichTextLabel.CalcHeight("• The type was deleted.", maxItemWidth));
			float lineHeight = DesignerStyles.RichLabelCenteredWordWrap.lineHeight;
			float itemSectionGap = lineHeight * 1.5f;
			float totalHeight = errorMessageSize.y + itemSectionGap + line0Size.y + line1Size.y + line2Size.y + line3Size.y;
			Rect textRect = rect.AlignCenter(maxWidth, totalHeight);
			Rect errorMessageRect = textRect.TakeFromTop(errorMessageSize.y);
			textRect.TakeFromTop(itemSectionGap);
			Rect line0Rect = textRect.TakeFromTop(line0Size.y);
			Rect line1Rect = textRect.TakeFromTop(line1Size.y).AlignCenterX(maxItemWidth);
			Rect line2Rect = textRect.TakeFromTop(line2Size.y).AlignCenterX(maxItemWidth);
			Rect line3Rect = textRect.AlignCenterX(maxItemWidth);
			GUI.Label(errorMessageRect, errorMessage, DesignerStyles.RichLabelCenteredWordWrap);
			GUI.Label(line0Rect, "This may have happened because:", DesignerStyles.RichLabelCenteredWordWrap);
			GUI.Label(line1Rect, "• The type was renamed.", SirenixGUIStyles.RichTextLabel);
			GUI.Label(line2Rect, "• The type was moved to a different assembly or namespace.", SirenixGUIStyles.RichTextLabel);
			GUI.Label(line3Rect, "• The type was deleted.", SirenixGUIStyles.RichTextLabel);
		}
	}
}
