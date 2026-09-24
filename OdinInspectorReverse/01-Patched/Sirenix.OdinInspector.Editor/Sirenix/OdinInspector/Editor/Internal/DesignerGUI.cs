using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Sirenix.Reflection.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Internal
{
	internal static class DesignerGUI
	{
		internal static class AttributeLabelCache
		{
			private static readonly Dictionary<Type, string> _cache = new Dictionary<Type, string>();

			public static string GetLabel(Type type)
			{
				if (type == null)
				{
					return string.Empty;
				}
				if (_cache.TryGetValue(type, out var cached))
				{
					return cached;
				}
				string name = type.Name;
				if (name.EndsWith("Attribute", StringComparison.OrdinalIgnoreCase))
				{
					name = name.Substring(0, name.Length - "Attribute".Length);
				}
				string label = ObjectNames.NicifyVariableName(name);
				_cache[type] = label;
				return label;
			}
		}

		public static class Tooltips
		{
			public static readonly string BugReport = "Report a bug or issue on Discord";

			public static readonly string EditClassAttributes = "Edit attributes defined on the class itself";

			public static readonly string ToggleVisibility = "Quickly toggle visibility mode to hide or show inspector elements";

			public static readonly string AddGroup = "Add a new group to the inspector";

			public static readonly string ReturnToAttributeList = "Return to the active attribute list";

			public static readonly string SaveChanges = "Save changes to disk";

			public static readonly string EditAttributes = "Modify attributes to adjust visuals, behavior, and validation";

			public static readonly string AddAttribute = "Add a new attribute";

			public static readonly string EditGroupParameters = "Edit group settings such as name and visibility";

			public static readonly string EditClass = "Open a designer window for the parent type";

			public static readonly string EditElement = "Open a designer window for this element’s type";

			public static readonly string DragAndDrop = "Click and drag to reorder elements or move them into groups";

			public static readonly string ToggleInInspector = "Show / Hide this element in the inspector";

			public static readonly string Pin = "Pin this window to prevent it from closing";

			public static readonly string PopoutExample = "Pop out this example into Odin's separate \"Attributes Example Window\"";

			public static readonly string OpenVisualDesigner = "Open the full Visual Designer window for this type";

			public static readonly string AddSubGroup = "Add a new sub-group within this group";

			public static readonly string SearchPrevious = "Navigate to previous search result (↑ / Ctrl+K / Ctrl+P / Shift+Enter)";

			public static readonly string SearchNext = "Navigate to next search result (↓ / Enter / Ctrl+J / Ctrl+N)";

			public static readonly string AttributeContextMenu = "Right-click to open attribute context menu";

			public static readonly string AttributeOverview = "Open the attribute overview";

			public static readonly string CloseWindow = "Close this window";

			public const string LOCK_TOGGLE = "Toggle to lock or unlock inherited members. When the lock is filled, inherited members cannot be edited; when the lock is open, they can be edited.";
		}

		private static string delayedTextBuffer;

		public static void DrawRoundBlur6(Rect position, Color color)
		{
			position = position.Expand(8f, 8f, 8f, 10f);
			SirenixEditorGUI.DrawTextureSliced(position, DesignerTextures.RoundBlur6, color, 10);
		}

		public static void DrawMember(Rect rect, DesignerEditorNode node, DesignerEditorContext context, bool shownInInspector, bool showHideMode)
		{
			bool isLocked = node.NodeColor != Color.white && context.IsNonDeclMembersLocked;
			if (isLocked)
			{
				GUIHelper.PushGUIEnabled(enabled: false);
			}
			if (context.IsShowHideMode)
			{
				Rect x = rect.TakeFromRight(rect.height);
				x = x.Expand(0f, 1f);
				if (shownInInspector)
				{
					DrawRoundBlur6(x.Expand(0f, 1f, -1f, 0f), Colors.Shadow);
					SirenixEditorGUI.DrawRoundRect(x, Colors.Node.Bg, 3f, Colors.Node.LightBorder, 1f);
				}
				else
				{
					SirenixEditorGUI.DrawRoundRect(x, Colors.Node.BgHidden, 3f, Colors.Node.LightBorder, 1f);
				}
				DrawIcon(x.AlignCenter(13f, 13f), shownInInspector ? SdfIconType.EyeFill : SdfIconType.EyeSlashFill, EditorStyles.label.normal.textColor, Tooltips.ToggleInInspector);
				rect.TakeFromRight(6f);
				if (Event.current.OnMouseDown(x, 0))
				{
					context.BeginUndo();
					if (node.PropertyPatch == null)
					{
						node.PropertyPatch = context.CreateProperty(node.SerializedName);
					}
					PropertyPatch patch = node.PropertyPatch;
					patch.Visibility = ((!shownInInspector) ? PropertyVisibilityState.Shown : PropertyVisibilityState.Hidden);
					node.IsShownInInspector = !node.IsShownInInspector;
					context.EndUndo(isEditorOutOfSync: false);
				}
			}
			Event e = Event.current;
			Rect unchangedRect = rect;
			int hash = node.GetHashCode();
			bool isSelectedNode = node == context.SelectedNode;
			bool isHovering = rect.Contains(e.mousePosition);
			bool isHiddenInInspectorButShownInDesigner = !shownInInspector && showHideMode;
			bool matchesSearch = context.SelectedFilteredNode != null && context.SelectedFilteredNode == node;
			bool shouldBeHighlighted = node.MemberType == context.MemberTypeToHighlight || matchesSearch;
			Color bgColor = (isHiddenInInspectorButShownInDesigner ? Colors.Node.BgHidden : (isSelectedNode ? Colors.Node.BgSelected : (isHovering ? Colors.Node.BgHover : Colors.Node.Bg)));
			string typeLabel = node.PropertyTypeLabel;
			Color haloColor = Color.clear;
			switch (node.MemberType)
			{
			case MemberTypes.Field:
				haloColor = Colors.Accents.Fields;
				break;
			case MemberTypes.Method:
				haloColor = Colors.Accents.Method;
				break;
			case MemberTypes.Property:
				haloColor = Colors.Accents.Properties;
				break;
			}
			Color borderColor = ((node == context.SelectedNode || shouldBeHighlighted) ? haloColor : Colors.Node.LightBorder);
			ref SirenixAnimationUtility.InterpolatedFloat leftGlow = ref SirenixAnimationUtility.GetTemporaryFloat($"{hash}_glow", 0f);
			leftGlow.ChangeDestination(isSelectedNode ? 1f : 0f);
			leftGlow.Move(1f, Easing.OutQuad);
			float leftGlowT = leftGlow;
			if (shouldBeHighlighted)
			{
				leftGlowT = 1f;
			}
			if (leftGlowT > 0f)
			{
				SirenixEditorGUI.DrawRoundRect(unchangedRect.Expand(1f), Color.clear, 3f, Colors.Node.Border, 1.5f);
				Rect fadeRect = unchangedRect.AlignLeft(unchangedRect.width * leftGlowT);
				GUI.DrawTexture(fadeRect.Expand(2f, 0f, 1f, 1f), DesignerTextures.LeftToRightFade, ScaleMode.StretchToFill, alphaBlend: true, 0f, borderColor, 0f, 5f);
			}
			else
			{
				SirenixEditorGUI.DrawRoundRect(unchangedRect.Expand(1f), Color.clear, 3f, borderColor, 1.5f);
			}
			ref SirenixAnimationUtility.InterpolatedFloat hover = ref SirenixAnimationUtility.GetTemporaryFloat($"{hash}_hover", 0f);
			hover.ChangeDestination(isHovering ? 1f : 0f);
			hover.Move(3.6363635f, Easing.OutQuad);
			SirenixEditorGUI.DrawRoundRect(unchangedRect, bgColor, 3f);
			if (node.NodeColor != Color.white)
			{
				SirenixEditorGUI.DrawRoundRect(unchangedRect, node.NodeColor - new Color(0f, 0f, 0f, 0.85f), 3f);
			}
			int count = 0;
			if (node.Attributes != null)
			{
				for (int i = 0; i < node.Attributes.Count; i++)
				{
					Attribute current = node.Attributes[i];
					if (DesignerRegistry.IsValidAttribute(current))
					{
						count++;
					}
				}
			}
			string attributeCountLabel = ((count > 99) ? "99+" : count.ToString());
			float attributeCountWidth = ((count > 99) ? 23f : ((count >= 10) ? 16f : 10f));
			float labelWidth = SirenixGUIStyles.Label.CalcWidth(node.Label);
			float typeLabelWidth = DesignerStyles.Node.MiniLabel.CalcWidth(typeLabel);
			float attributeButtonWidth = rect.height;
			float classButtonWidth = rect.height;
			int accentWidth = 4;
			float leftPadWidth = 9f;
			int baseRightPadWidth = 6;
			int typeLabelLeftPadWidth = 6;
			int accentLeftPadWidth = 6;
			bool showAttributeButton = true;
			bool showClassButton = true;
			bool showTypeLabel = true;
			bool showAttributeCount = true;
			bool showAccent = true;
			if (!Fits())
			{
				showAccent = false;
			}
			if (!Fits())
			{
				showAttributeCount = false;
			}
			if (!Fits())
			{
				showTypeLabel = false;
			}
			if (!Fits())
			{
				showClassButton = false;
			}
			if (!Fits())
			{
				showAttributeButton = false;
			}
			float rightPadWidth = ((showAttributeButton && !showAttributeCount && !showAccent) ? 0f : ((float)baseRightPadWidth));
			rect.TakeFromLeft(leftPadWidth);
			Rect rightPadRect = default(Rect);
			if (rightPadWidth > 0f)
			{
				rightPadRect = rect.TakeFromRight(rightPadWidth);
			}
			Rect accentRect = default(Rect);
			Rect accentLeftPadRect = default(Rect);
			if (showAccent)
			{
				accentRect = rect.TakeFromRight(accentWidth).VerticalPadding(6f);
				accentLeftPadRect = rect.TakeFromRight(accentLeftPadWidth);
			}
			Rect attributeCountRect = default(Rect);
			if (showAttributeCount)
			{
				attributeCountRect = rect.TakeFromRight(attributeCountWidth);
			}
			Rect attributeButtonRect = default(Rect);
			if (showAttributeButton)
			{
				attributeButtonRect = rect.TakeFromRight(attributeButtonWidth);
			}
			Rect classButtonRect = default(Rect);
			if (showClassButton)
			{
				classButtonRect = rect.TakeFromRight(classButtonWidth);
			}
			Rect typeLabelRect = default(Rect);
			Rect typeLabelLeftPadRect = default(Rect);
			if (showTypeLabel)
			{
				typeLabelRect = rect.TakeFromRight(typeLabelWidth);
				typeLabelLeftPadRect = rect.TakeFromRight(typeLabelLeftPadWidth);
			}
			Rect labelRect = rect.TakeFromLeft(labelWidth);
			DrawHaloBar(unchangedRect, $"{hash}_halo", Colors.Node.Halo, (isSelectedNode || shouldBeHighlighted) ? haloColor : Color.clear, haloColor, 0.5f, isSelectedNode ? 0.3f : 0.15f, isSelectedNode || shouldBeHighlighted);
			if (isLocked)
			{
				GUIHelper.PopGUIEnabled();
				Color bgColorOverlay = node.NodeColor;
				Color.RGBToHSV(bgColorOverlay, out var h, out var s, out var v);
				s *= 0.2f;
				v *= 0.2f;
				bgColorOverlay = Color.HSVToRGB(h, s, v);
				bgColorOverlay.a = 0.2f;
				SirenixEditorGUI.DrawRoundRect(unchangedRect, bgColorOverlay, 3f, new Color(1f, 1f, 1f, 0.15f), 1f);
				Rect lockRect = unchangedRect.AlignCenter(22f, 22f);
				EditorGUI.DrawRect(lockRect.Padding(4f, 1f), new Color(0.9f, 0.9f, 0.9f, 1f));
				s *= 2f;
				v *= 1.8f;
				SdfIcons.DrawIcon(lockRect, SdfIconType.FileLock2Fill, Color.HSVToRGB(h, s, v));
				GUIContent content = GUIHelper.TempContent(string.Empty, "This member is locked because it’s inherited. Use the toggle button at the bottom to unlock it.");
				GUI.Label(unchangedRect.AlignCenterX(30f), content);
				GUIHelper.PushGUIEnabled(enabled: false);
			}
			ref SirenixAnimationUtility.InterpolatedFloat pressed = ref SirenixAnimationUtility.GetTemporaryFloat($"{hash}_pressed", 0f);
			GUIContext<bool> hasRun = GUIHelper.GetTemporaryContext("951251851895189", $"{hash}_wasPressed", defaultValue: false);
			float pressedT = pressed.GetValue();
			if (isSelectedNode && pressedT == 0f && !hasRun.Value)
			{
				pressed.ChangeDestination(1f);
			}
			if (pressedT >= 0.99f)
			{
				pressed.ChangeDestination(0f);
				hasRun.Value = true;
			}
			if (!isSelectedNode && pressedT == 0f)
			{
				hasRun.Value = false;
			}
			pressed.Move(3.3333333f);
			if (pressedT > 0f)
			{
				Color c1 = Colors.Node.PressedBg;
				c1.a *= pressedT;
				Color c2 = Colors.Node.PressedBlur;
				c2.a *= pressedT;
				SirenixEditorGUI.DrawRoundRect(unchangedRect, c1, 3f);
				SirenixEditorGUI.DrawTextureSliced(unchangedRect, DesignerTextures.RoundBlur6Inverted, c2, 10);
			}
			Rect attributeButtonAreaRect = default(Rect);
			if (showAttributeButton)
			{
				attributeButtonAreaRect = Encapsulate(attributeButtonRect, showAttributeCount ? attributeCountRect : default(Rect), showAccent ? accentRect : default(Rect), (rightPadWidth > 0f) ? rightPadRect : default(Rect), showAccent ? accentLeftPadRect : default(Rect));
			}
			Rect classButtonAreaRect = default(Rect);
			if (showClassButton)
			{
				classButtonAreaRect = Encapsulate(classButtonRect, showTypeLabel ? typeLabelRect : default(Rect), showTypeLabel ? typeLabelLeftPadRect : default(Rect));
			}
			if (showAttributeButton && e.IsHovering(attributeButtonAreaRect))
			{
				SirenixEditorGUI.DrawRoundRect(attributeButtonAreaRect, Colors.Node.ButtonHover, 0f, 3f, 0f, 3f);
				e.OnMouseDown(attributeButtonAreaRect, 0);
			}
			if (showAttributeButton)
			{
				SdfIcons.DrawIcon(attributeButtonRect.Padding(8f), SdfIconType.PencilSquare, Colors.Icons.Default);
				GUI.Label(attributeButtonAreaRect, new GUIContent("", Tooltips.EditAttributes), GUIStyle.none);
			}
			if ((showAttributeButton && e.OnMouseUp(attributeButtonAreaRect, 0)) || (e.OnMouseUp(unchangedRect, 0, useEvent: false) && e.clickCount >= 2) || (GUIUtility.keyboardControl == 0 && (e.OnKeyDown(KeyCode.Space, useEvent: false) || e.OnKeyDown(KeyCode.Return, useEvent: false)) && isSelectedNode))
			{
				context.Select(node);
				Rect contextRect = GUIUtility.GUIToScreenRect(unchangedRect);
				if (e.clickCount > 1)
				{
					contextRect.x = GUIUtility.GUIToScreenPoint(e.mousePosition).x - 175f;
				}
				context.Editor.OpenPopup(contextRect, node);
				GUIHelper.ExitGUI(removeFocusControl: true);
			}
			bool isArray = node.PropertyValueType != null && node.PropertyValueType.IsArray;
			Type typeToDesign = (isArray ? node.PropertyElementType : node.PropertyValueType);
			if (isLocked)
			{
				GUIHelper.PushGUIEnabled(enabled: true);
			}
			bool isClassEditButtonDisabled = !DesignerUtils.CanTypeBeDesigned(typeToDesign);
			if (isClassEditButtonDisabled)
			{
				GUIHelper.PushGUIEnabled(enabled: false);
			}
			if (showClassButton && e.IsHovering(classButtonAreaRect))
			{
				SirenixEditorGUI.DrawRoundRect(classButtonAreaRect, Colors.Node.ButtonHover, 0f);
				if (e.OnMouseUp(classButtonAreaRect, 0))
				{
					context.Select(node);
					DesignerEditors.Get(typeToDesign, null, null)?.OpenWindow();
				}
			}
			if (showClassButton)
			{
				SdfIcons.DrawIcon(classButtonRect.Padding(8f), SdfIconType.BoxArrowInUpRight, Colors.Icons.Default);
				GUI.Label(classButtonAreaRect, GUIHelper.TempContent(string.Empty, isArray ? "Customize Element Type in Visual Designer" : "Customize Member Type in Visual Designer"), GUIStyle.none);
			}
			if (isClassEditButtonDisabled)
			{
				GUIHelper.PopGUIEnabled();
			}
			if (isLocked)
			{
				GUIHelper.PopGUIEnabled();
			}
			if (showAttributeButton)
			{
				EditorGUI.DrawRect(attributeButtonAreaRect.AlignLeft(1f), SirenixGUIStyles.BorderColor);
			}
			if (showAttributeCount)
			{
				GUI.Label(attributeCountRect, attributeCountLabel, DesignerStyles.Node.MiniLabel);
			}
			if (showAccent)
			{
				SirenixEditorGUI.DrawRoundRect(accentRect, haloColor, float.MaxValue);
			}
			if (showTypeLabel)
			{
				GUI.Label(typeLabelRect, typeLabel, DesignerStyles.Node.MiniLabel);
			}
			if (labelRect.width < labelWidth)
			{
				float ellipsisWidth = SirenixGUIStyles.Label.CalcWidth("…");
				GUI.Label(labelRect.TakeFromRight(ellipsisWidth), "…", SirenixGUIStyles.Label);
			}
			GUI.Label(labelRect, node.Label, DesignerStyles.Node.LabelStyle);
			if (e.OnMouseUp(unchangedRect, 0))
			{
				context.Select(node);
			}
			if (isLocked)
			{
				GUIHelper.PopGUIEnabled();
			}
			bool Fits()
			{
				return leftPadWidth + RightSideWidth() <= rect.width;
			}
			float RightSideWidth()
			{
				float w = 0f;
				float computedRightPad = ((showAttributeButton && !showAttributeCount && !showAccent) ? 0f : ((float)baseRightPadWidth));
				w += computedRightPad;
				if (showAccent)
				{
					w += (float)(accentWidth + accentLeftPadWidth);
				}
				if (showAttributeCount)
				{
					w += attributeCountWidth;
				}
				if (showAttributeButton)
				{
					w += attributeButtonWidth;
				}
				if (showClassButton)
				{
					w += classButtonWidth;
				}
				if (showTypeLabel)
				{
					w += typeLabelWidth + (float)typeLabelLeftPadWidth;
				}
				return w;
			}
		}

		public static Rect Encapsulate(params Rect[] rects)
		{
			if (rects == null || rects.Length == 0)
			{
				return Rect.zero;
			}
			float minX = rects[0].xMin;
			float minY = rects[0].yMin;
			float maxX = rects[0].xMax;
			float maxY = rects[0].yMax;
			for (int i = 1; i < rects.Length; i++)
			{
				Rect r = rects[i];
				if (!(r == Rect.zero))
				{
					if (r.xMin < minX)
					{
						minX = r.xMin;
					}
					if (r.yMin < minY)
					{
						minY = r.yMin;
					}
					if (r.xMax > maxX)
					{
						maxX = r.xMax;
					}
					if (r.yMax > maxY)
					{
						maxY = r.yMax;
					}
				}
			}
			return Rect.MinMaxRect(minX, minY, maxX, maxY);
		}

		public static void DrawMemberMinimal(Rect rect, DesignerEditorNode node, bool shownInInspector, bool showHideMode)
		{
			GUIHelper.PushColor(new Color(1f, 1f, 1f, 1f));
			bool isHiddenInInspectorButShownInDesigner = !shownInInspector && showHideMode;
			Color bgColor = (isHiddenInInspectorButShownInDesigner ? Colors.Node.BgHidden : Colors.Node.BgSelected);
			DrawRoundBlur6(rect.Expand(1f, 2f, -1f, 1f), Colors.Shadow);
			Color accentColor = Color.clear;
			switch (node.Member.MemberType)
			{
			case MemberTypes.Field:
				accentColor = Colors.Accents.Fields;
				break;
			case MemberTypes.Method:
				accentColor = Colors.Accents.Method;
				break;
			case MemberTypes.Property:
				accentColor = Colors.Accents.Properties;
				break;
			}
			SirenixEditorGUI.DrawRoundRect(rect, bgColor, 3f, Colors.Node.Border, 1f);
			rect = rect.HorizontalPadding(10f, 6f);
			Rect accentRect = rect.TakeFromRight(5f).VerticalPadding(5f);
			DrawMemberAccent(accentRect, accentColor);
			rect.TakeFromRight(10f);
			string typeLabel = ((node.Member.MemberType == MemberTypes.Method) ? GetMethodTypeLabel(node.Member) : node.Member.GetReturnType().GetNiceName());
			float typeLabelWidth = SirenixGUIStyles.RightAlignedGreyMiniLabel.CalcWidth(typeLabel);
			Rect typeLabelRect = rect.TakeFromRight(typeLabelWidth);
			Color c = SirenixGUIStyles.RightAlignedGreyMiniLabel.normal.textColor;
			SirenixGUIStyles.RightAlignedGreyMiniLabel.normal.textColor = new Color(c.r, c.g, c.b, isHiddenInInspectorButShownInDesigner ? 0.5f : 1f);
			GUI.Label(typeLabelRect, typeLabel, SirenixGUIStyles.RightAlignedGreyMiniLabel);
			SirenixGUIStyles.RightAlignedGreyMiniLabel.normal.textColor = c;
			DrawLabelWithEllipsis(rect, node.Label, bold: false, isHiddenInInspectorButShownInDesigner);
			GUIHelper.PopColor();
		}

		public static void DrawGroupMinimal(Rect rect, DesignerEditorNode node)
		{
			GUIHelper.PushColor(new Color(1f, 1f, 1f, 0.8f));
			DrawRoundBlur6(rect.Expand(1f, 2f, -1f, 1f), Colors.Shadow);
			SirenixEditorGUI.DrawRoundRect(rect, Colors.Node.BgSelected, 3f, Colors.Node.Border, 1f);
			rect = rect.HorizontalPadding(10f, 6f);
			DrawLabelWithEllipsis(rect, node.Label, bold: false, isHiddenInInspectorButShownInDesigner: false);
			GUIHelper.PopColor();
		}

		public static void DrawHaloBar(Rect rect, string key, Color defaultHalo, Color leftHalo, Color rightHalo, float leftThreshold = 0f, float rightThreshold = 0f, bool freezeLeft = false, bool freezeRight = false)
		{
			Event e = Event.current;
			bool hovering = e.IsHovering(rect);
			ref SirenixAnimationUtility.InterpolatedFloat tfHover = ref SirenixAnimationUtility.GetTemporaryFloat(key + "_hovering_DrawHaloBar", 0f);
			if (e.type == EventType.Repaint)
			{
				tfHover.ChangeDestination(hovering ? 1f : 0f);
			}
			tfHover.Move(2.5f);
			if (freezeLeft)
			{
				GUI.BeginClip(rect);
				Rect cursorRect = new Rect(-228f, -228f, 456f, 456f);
				GUI.DrawTexture(cursorRect, DesignerTextures.GradientHover, ScaleMode.ScaleToFit, alphaBlend: true, 1f, leftHalo, Vector4.zero, Vector4.zero);
				GUI.EndClip();
			}
			if (freezeRight)
			{
				GUI.BeginClip(rect);
				Rect cursorRect2 = new Rect(rect.width - 228f, rect.height - 228f, 456f, 456f);
				GUI.DrawTexture(cursorRect2, DesignerTextures.GradientHover, ScaleMode.ScaleToFit, alphaBlend: true, 1f, rightHalo, Vector4.zero, Vector4.zero);
				GUI.EndClip();
			}
			if ((float)tfHover > 0f)
			{
				GUI.BeginClip(rect);
				float mouseX = e.mousePosition.x;
				float blendFactor = 0f;
				Color haloColor = defaultHalo;
				float blendStart = rect.width * leftThreshold;
				if (leftHalo != Color.clear && mouseX < blendStart)
				{
					blendFactor = Mathf.InverseLerp(blendStart, 0f, mouseX);
					haloColor = leftHalo;
				}
				blendStart = rect.width - rect.width * rightThreshold;
				if (rightHalo != Color.clear && mouseX > blendStart)
				{
					blendFactor = Mathf.InverseLerp(blendStart, rect.width, mouseX);
					haloColor = rightHalo;
				}
				float cursorSize = 200f + blendFactor * 256f;
				Rect cursorRect3 = new Rect(0f, 0f, cursorSize, cursorSize);
				cursorRect3.x = Event.current.mousePosition.x - cursorRect3.height * 0.5f;
				cursorRect3.y = Event.current.mousePosition.y - cursorRect3.width * 0.5f;
				Color cursorColor = Color.Lerp(defaultHalo, haloColor, blendFactor);
				cursorColor.a = tfHover;
				GUI.DrawTexture(cursorRect3.Expand(50f), DesignerTextures.GradientHover, ScaleMode.ScaleToFit, alphaBlend: true, 1f, cursorColor, Vector4.zero, Vector4.zero);
				GUI.EndClip();
			}
		}

		public static bool DrawHaloButton(Rect rect, string key, string label, string tooltip)
		{
			int controlId = GUIUtility.GetControlID(FocusType.Keyboard);
			bool isActive = Event.current.type == EventType.MouseDown && Event.current.button == 0;
			if (isActive && rect.Contains(Event.current.mousePosition))
			{
				Event.current.Use();
				GUIUtility.hotControl = controlId;
				GUIUtility.keyboardControl = controlId;
			}
			bool pressed = Event.current.OnMouseUp(rect, 0);
			if (pressed)
			{
				GUIHelper.RemoveFocusControl();
			}
			bool isMouseOver = Event.current.IsMouseOver(rect);
			ref SirenixAnimationUtility.InterpolatedFloat tfHover = ref SirenixAnimationUtility.GetTemporaryFloat(key + "_hovering_DrawHaloButton", 0f);
			ref SirenixAnimationUtility.InterpolatedFloat tfActive = ref SirenixAnimationUtility.GetTemporaryFloat(key + "_active_DrawHaloButton", 0f);
			tfHover.ChangeDestination(isMouseOver ? 1f : 0f);
			tfActive.ChangeDestination(isActive ? 1f : 0f);
			tfHover.Move(4f);
			tfActive.Move(6.6666665f);
			float old = Colors.AttributePopup.ButtonBg.a;
			Colors.AttributePopup.ButtonBg.a = GUI.color.a;
			GUI.DrawTexture(rect, DesignerTextures.GradientButton, ScaleMode.StretchToFill, alphaBlend: true, 1f, Colors.AttributePopup.ButtonBg, Vector4.zero, Vector4.zero);
			Colors.AttributePopup.ButtonBg.a = old;
			if ((float)tfHover > 0f)
			{
				Color c = Colors.AttributePopup.ButtonOverlayHover;
				c.a = tfHover;
				EditorGUI.DrawRect(rect, c);
				DrawHaloBar(rect, key, Colors.AttributePopup.ButtonHaloBg, Color.clear, Color.clear);
			}
			if ((float)tfActive > 0f)
			{
				Color c2 = Colors.AttributePopup.ButtonOverlayActive;
				c2.a = tfActive;
				EditorGUI.DrawRect(rect, c2);
				DrawHaloBar(rect, key, Colors.AttributePopup.ButtonBgActive, Color.clear, Color.clear);
			}
			SirenixEditorGUI.DrawRoundRect(rect.Expand(1f), Color.clear, 2f, Colors.Button.Border, 1.125f);
			GUI.Label(rect, new GUIContent(label, tooltip), SirenixGUIStyles.LabelCentered);
			return pressed;
		}

		public static string GetMethodTypeLabel(MethodBase method)
		{
			StringBuilder builder = new StringBuilder();
			builder.Append(method.GetReturnType().GetNiceName() ?? "");
			ParameterInfo[] parameterInfos = method.GetParameters();
			if (parameterInfos.Length != 0)
			{
				builder.Append(" (");
				for (int i = 0; i < parameterInfos.Length; i++)
				{
					ParameterInfo parameterInfo = parameterInfos[i];
					builder.Append(parameterInfo.ParameterType.GetNiceName());
					if (i != parameterInfos.Length - 1)
					{
						builder.Append(", ");
					}
				}
				builder.Append(")");
			}
			return builder.ToString();
		}

		private static string GetMethodTypeLabel(MemberInfo memberInfo)
		{
			MethodBase method = memberInfo as MethodBase;
			return GetMethodTypeLabel(method);
		}

		internal static void DrawMemberAccent(Rect rect, Color color)
		{
			SirenixEditorGUI.DrawRoundRect(rect, color, 3f, 3f, 3f, 3f);
		}

		private static void DrawLabelWithEllipsis(Rect rect, string label, bool bold, bool isHiddenInInspectorButShownInDesigner)
		{
			float labelWidth = SirenixGUIStyles.Label.CalcWidth(label);
			GUIStyle style = (bold ? DesignerStyles.Node.BoldLabelStyle : DesignerStyles.Node.LabelStyle);
			Color c = style.normal.textColor;
			style.normal.textColor = new Color(c.r, c.g, c.b, isHiddenInInspectorButShownInDesigner ? 0.5f : 1f);
			if (labelWidth >= rect.width)
			{
				float ellipsisWidth = SirenixGUIStyles.Label.CalcWidth("…");
				Rect ellipsisRect = rect.TakeFromRight(ellipsisWidth);
				GUI.Label(ellipsisRect, "…", style);
			}
			GUI.Label(rect, label, style);
			style.normal.textColor = c;
		}

		public static string DrawSearch(Rect rect, string searchTerm, SearchField searchField, out bool changed)
		{
			changed = false;
			EditorGUI.BeginChangeCheck();
			searchTerm = searchField.Draw(rect.Padding(4f), searchTerm, "Search");
			if (EditorGUI.EndChangeCheck())
			{
				changed = true;
			}
			return searchTerm;
		}

		public static bool DrawIconSlideButton(Rect rect, string key, string tooltip, SdfIconType defaultIcon, SdfIconType slideIcon, Color defaultColor, Color slideColor)
		{
			Event e = Event.current;
			bool pressed = DrawHaloButton(rect, key + "_halo_DrawIconSlideButton", string.Empty, tooltip);
			ref SirenixAnimationUtility.InterpolatedFloat t = ref SirenixAnimationUtility.GetTemporaryFloat(key + "_slide_DrawIconSlideButton", 0f);
			t.ChangeDestination(e.IsHovering(rect) ? 1f : 0f);
			t.Move(5f, Easing.OutQuad);
			float tValue = t.GetValue();
			GUI.BeginClip(rect);
			float w = rect.width;
			float h = rect.height;
			float iw = Mathf.Max(0f, w - 8f);
			float ih = Mathf.Max(0f, h - 8f);
			float xDefault = Mathf.Lerp(4f, 0f - w, tValue);
			float xSlide = Mathf.Lerp(w * 2f, 4f, tValue);
			Rect rDefault = new Rect(xDefault, 4f, iw, ih);
			Rect rSlide = new Rect(xSlide, 4f, iw, ih);
			SdfIcons.DrawIcon(rDefault, defaultIcon, defaultColor);
			SdfIcons.DrawIcon(rSlide, slideIcon);
			GUI.EndClip();
			if (tValue == 1f)
			{
				SirenixEditorGUI.DrawRoundRect(rect.Expand(1f), Color.clear, 2f, slideColor, 1f);
			}
			else if (tValue > 0f)
			{
				defaultColor.a *= 1f - tValue;
				slideColor.a *= tValue;
				SirenixEditorGUI.DrawRoundRect(rect.Expand(1f), Color.clear, 2f, defaultColor, 1f);
				SirenixEditorGUI.DrawRoundRect(rect.Expand(1f), Color.clear, 2f, slideColor, 1f);
			}
			else
			{
				SirenixEditorGUI.DrawRoundRect(rect.Expand(1f), Color.clear, 2f, defaultColor, 1f);
			}
			return pressed;
		}

		public static string DoColumnSizeField(Rect rect, int id, string source, out bool isConfirmed)
		{
			Color borderColor = (Event.current.IsHovering(rect) ? Colors.Group.ColumnInputBorderHover : Colors.Group.ColumnInputBorder);
			SirenixEditorGUI.DrawRoundRect(rect, Colors.Group.ColumnInputBg, 4f, borderColor, 1f);
			EditorGUI.BeginChangeCheck();
			source = DelayedTextField(rect, null, source, DesignerStyles.Row.ColumnLabelStyle);
			isConfirmed = EditorGUI.EndChangeCheck();
			EditorGUIUtility.AddCursorRect(rect, MouseCursor.Text);
			return source;
		}

		public static void DrawIcon(Rect rect, SdfIconType icon, Color color, string tooltip)
		{
			SdfIcons.DrawIcon(rect, icon, color);
			GUI.Label(rect, new GUIContent("", tooltip));
		}

		public static bool DrawSimpleIconButton(Rect rect, SdfIconType icon, int iconPadding)
		{
			return DrawSimpleIconButton(rect, icon, iconPadding, null);
		}

		public static bool DrawSimpleIconButton(Rect rect, SdfIconType icon, int iconPadding, string tooltip)
		{
			int controlId = GUIUtility.GetControlID(FocusType.Passive, rect);
			bool hovering;
			bool active;
			bool pressed = SirenixEditorGUI.DoButton(rect, controlId, out hovering, out active);
			if (hovering)
			{
				EditorGUI.DrawRect(rect, Colors.Icons.Hover);
			}
			SdfIcons.DrawIcon(rect.Padding(iconPadding), icon, Colors.Icons.Default);
			if (!string.IsNullOrEmpty(tooltip))
			{
				GUI.Label(rect, new GUIContent("", tooltip), GUIStyle.none);
			}
			return pressed;
		}

		public static string DelayedTextField(Rect rect, string label, string value, GUIStyle style)
		{
			int control = GUIUtility.GetControlID(FocusType.Passive);
			if (SirenixEditorFields.OnLocalControlRelease(rect, control))
			{
				GUI.changed = true;
				value = delayedTextBuffer;
			}
			string buffer = value;
			if (SirenixEditorFields.localHotControl == control)
			{
				buffer = delayedTextBuffer;
			}
			EditorGUI.BeginChangeCheck();
			int textFieldId = GUIUtility.GetControlID(FocusType.Keyboard, rect);
			buffer = EditorGUI_Internals.DoTextField(textFieldId, EditorGUI.IndentedRect(rect), buffer, style, null, out var _, reset: false, multiline: false, passwordField: false);
			if (EditorGUI.EndChangeCheck())
			{
				GUI.changed = false;
				SirenixEditorFields.localHotControl = control;
				delayedTextBuffer = buffer;
			}
			return value;
		}
	}
}
