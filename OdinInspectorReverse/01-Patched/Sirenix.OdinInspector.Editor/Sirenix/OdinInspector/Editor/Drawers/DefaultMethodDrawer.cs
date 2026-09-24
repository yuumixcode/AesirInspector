using System;
using System.Linq;
using System.Reflection;
using Sirenix.OdinInspector.Editor.ActionResolvers;
using Sirenix.OdinInspector.Editor.ValueResolvers;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	/// <summary>
	/// The default method drawer that draws most buttons.
	/// </summary>
	[DrawerPriority(0.0, 0.0, 0.11)]
	public sealed class DefaultMethodDrawer : MethodDrawer
	{
		[ShowOdinSerializedPropertiesInInspector]
		private class MethodResultInspector
		{
			[HideReferenceObjectPicker]
			[HideLabel]
			public object Value;
		}

		internal static bool DontDrawMethodParameters;

		private bool drawParameters;

		private bool hasReturnValue;

		private bool shouldDrawResult;

		private string name;

		private ButtonAttribute buttonAttribute;

		private GUIStyle style;

		private GUIStyle toggleBtnStyle;

		private ValueResolver<string> labelGetter;

		private GUIContent label;

		private ButtonStyle btnStyle;

		private bool expanded;

		private Color btnColor;

		private bool hasGUIColorAttribute;

		private bool hasInvokedOnce;

		private ActionResolver buttonActionResolver;

		private ValueResolver<object> buttonValueResolver;

		private bool hideLabel;

		private ValueResolver<string> tooltipValueResolver;

		private int buttonHeight;

		private float buttonAlignment;

		private IconAlignment buttonIconAlignment;

		private bool stretch;

		private bool drawnByGroup;

		/// <summary>
		/// Initializes this instance.
		/// </summary>
		protected override void Initialize()
		{
			expanded = false;
			buttonAttribute = base.Property.GetAttribute<ButtonAttribute>();
			hasGUIColorAttribute = base.Property.GetAttribute<GUIColorAttribute>() != null;
			drawParameters = base.Property.Children.Count > 0 && !DontDrawMethodParameters && (buttonAttribute == null || buttonAttribute.DisplayParameters);
			hasReturnValue = base.Property.Children.Count > 0 && base.Property.Children[base.Property.Children.Count - 1].Name == "$Result";
			hideLabel = base.Property.Attributes.HasAttribute<HideLabelAttribute>() || buttonAttribute?.Name == string.Empty;
			string tooltip = base.Property.GetAttribute<PropertyTooltipAttribute>()?.Tooltip;
			if (tooltip != null)
			{
				tooltipValueResolver = ValueResolver.GetForString(base.Property, tooltip);
			}
			buttonHeight = base.Property.Context.GetGlobal("ButtonHeight", (buttonAttribute != null && buttonAttribute.HasDefinedButtonHeight) ? buttonAttribute.ButtonHeight : GlobalConfig<GeneralDrawerConfig>.Instance.ButtonHeight).Value;
			buttonHeight = Mathf.Max(buttonHeight, (int)EditorGUIUtility.singleLineHeight);
			buttonIconAlignment = base.Property.Context.GetGlobal("IconAlignment", (buttonAttribute != null && buttonAttribute.HasDefinedButtonIconAlignment) ? buttonAttribute.IconAlignment : GlobalConfig<GeneralDrawerConfig>.Instance.ButtonIconAlignment).Value;
			buttonAlignment = base.Property.Context.GetGlobal("ButtonAlignment", (buttonAttribute != null && buttonAttribute.HasDefinedButtonAlignment) ? buttonAttribute.ButtonAlignment : GlobalConfig<GeneralDrawerConfig>.Instance.ButtonAlignment).Value;
			stretch = base.Property.Context.GetGlobal("StretchButton", (buttonAttribute != null && buttonAttribute.HasDefinedStretch) ? buttonAttribute.Stretch : GlobalConfig<GeneralDrawerConfig>.Instance.StretchButtons).Value;
			style = base.Property.Context.GetGlobal("ButtonStyle", (buttonHeight > 20) ? SirenixGUIStyles.Button : EditorStyles.miniButton).Value;
			drawnByGroup = base.Property.Context.GetGlobal("DrawnByGroup", defaultValue: false).Value;
			shouldDrawResult = GlobalConfig<GeneralDrawerConfig>.Instance.ShowButtonResultsByDefault;
			if (buttonAttribute != null)
			{
				if (!buttonAttribute.DisplayParameters)
				{
					if (hasReturnValue)
					{
						buttonValueResolver = ValueResolver.Get<object>(base.Property, null);
					}
					else
					{
						buttonActionResolver = ActionResolver.Get(base.Property, null);
					}
				}
				btnStyle = buttonAttribute.Style;
				expanded = buttonAttribute.Expanded;
				if (!string.IsNullOrEmpty(buttonAttribute.Name))
				{
					labelGetter = ValueResolver.GetForString(base.Property, buttonAttribute.Name);
				}
				if (buttonAttribute.DrawResultIsSet)
				{
					shouldDrawResult = buttonAttribute.DrawResult;
				}
			}
			if (!shouldDrawResult && hasReturnValue && base.Property.Children.Count == 1)
			{
				drawParameters = false;
			}
			if (drawParameters && btnStyle == ButtonStyle.FoldoutButton && !expanded)
			{
				if (buttonHeight > 20)
				{
					style = SirenixGUIStyles.ButtonLeft;
					toggleBtnStyle = SirenixGUIStyles.ButtonRight;
				}
				else
				{
					style = EditorStyles.miniButtonLeft;
					toggleBtnStyle = EditorStyles.miniButtonRight;
				}
			}
		}

		/// <summary>
		/// Draws the property layout.
		/// </summary>
		protected override void DrawPropertyLayout(GUIContent lbl)
		{
			if (buttonActionResolver != null && buttonActionResolver.HasError)
			{
				buttonActionResolver.DrawError();
			}
			if (buttonValueResolver != null && buttonValueResolver.HasError)
			{
				buttonValueResolver.DrawError();
			}
			labelGetter?.DrawError();
			tooltipValueResolver?.DrawError();
			label = new GUIContent(hideLabel ? "" : ((labelGetter != null) ? labelGetter.GetValue() : base.Property.NiceName));
			if (tooltipValueResolver != null)
			{
				label.tooltip = tooltipValueResolver.GetValue() ?? "";
			}
			else if (!string.IsNullOrEmpty(lbl?.tooltip))
			{
				label.tooltip = lbl?.tooltip;
			}
			else
			{
				label.tooltip = label.text;
			}
			base.Property.Label = label;
			btnColor = GUI.color;
			Color contentColor = (hasGUIColorAttribute ? GUIColorAttributeDrawer.CurrentOuterColor : btnColor);
			GUIHelper.PushColor(contentColor);
			int h = base.Property.Context.GetGlobal("ButtonHeight", 0).Value;
			GUIStyle s = base.Property.Context.GetGlobal("ButtonStyle", (GUIStyle)null).Value;
			if ((buttonHeight != h && h != 0) || (s != null && style != s))
			{
				Initialize();
			}
			if (!drawParameters)
			{
				DrawNormalButton();
			}
			else if (btnStyle == ButtonStyle.FoldoutButton)
			{
				if (expanded)
				{
					DrawNormalButton();
					EditorGUI.indentLevel++;
					DrawParameters(appendButton: false);
					EditorGUI.indentLevel--;
				}
				else
				{
					DrawFoldoutButton();
				}
			}
			else if (btnStyle == ButtonStyle.CompactBox)
			{
				DrawCompactBoxButton();
			}
			else if (btnStyle == ButtonStyle.Box)
			{
				DrawBoxButton();
			}
			GUIHelper.PopColor();
		}

		private void DrawBoxButton()
		{
			SirenixEditorGUI.BeginBox();
			SirenixEditorGUI.BeginToolbarBoxHeader();
			if (expanded)
			{
				EditorGUILayout.LabelField(label);
			}
			else
			{
				base.Property.State.Expanded = SirenixEditorGUI.Foldout(base.Property.State.Expanded, label);
			}
			SirenixEditorGUI.EndToolbarBoxHeader();
			DrawParameters(appendButton: true);
			SirenixEditorGUI.EndToolbarBox();
		}

		private void DrawCompactBoxButton()
		{
			bool hasIcon = buttonAttribute != null && buttonAttribute.HasDefinedIcon;
			SirenixEditorGUI.BeginBox();
			Rect rect = SirenixEditorGUI.BeginToolbarBoxHeader().AlignRight(hasIcon ? 90 : 70).Padding(1f);
			rect.height -= 1f;
			GUIHelper.PushColor(btnColor);
			GUIContent invokeLabel = new GUIContent(hideLabel ? "" : "Invoke", label.tooltip);
			if (hasIcon)
			{
				if (SirenixEditorGUI.SDFIconButton(rect, invokeLabel, buttonAttribute.Icon, buttonIconAlignment))
				{
					InvokeButton();
				}
			}
			else if (GUI.Button(rect, invokeLabel))
			{
				InvokeButton();
			}
			GUIHelper.PopColor();
			if (expanded)
			{
				EditorGUILayout.LabelField(label);
			}
			else
			{
				base.Property.State.Expanded = SirenixEditorGUI.Foldout(base.Property.State.Expanded, label);
			}
			SirenixEditorGUI.EndToolbarBoxHeader();
			DrawParameters(appendButton: false);
			SirenixEditorGUI.EndToolbarBox();
		}

		private void DrawNormalButton()
		{
			SirenixEditorGUI.CalculateMinimumSDFIconButtonWidth(label?.text, style, buttonAttribute?.HasDefinedIcon ?? false, buttonHeight, out var _, out var _, out var _, out var minimalButtonWidth);
			Rect btnRect = EditorGUI.IndentedRect(GUILayoutUtility.GetRect(0f, buttonHeight, GUILayout.MinWidth(minimalButtonWidth)));
			if (!stretch && !drawnByGroup)
			{
				float availableEmptySpace = btnRect.width - minimalButtonWidth;
				btnRect.x += buttonAlignment * availableEmptySpace;
				btnRect.width = minimalButtonWidth;
			}
			GUIHelper.PushColor(btnColor);
			if (SirenixEditorGUI.SDFIconButton(btnRect, label, buttonAttribute?.Icon ?? SdfIconType.None, buttonIconAlignment, style))
			{
				InvokeButton();
			}
			GUIHelper.PopColor();
		}

		private void DrawFoldoutButton()
		{
			Rect btnRect = GUILayoutUtility.GetRect(GUIContent.none, style, GUILayoutOptions.Height(buttonHeight));
			btnRect = EditorGUI.IndentedRect(btnRect);
			GUIHelper.PushColor(btnColor);
			Rect foldoutRect = btnRect.AlignRight(20f);
			if (GUI.Button(foldoutRect, GUIContent.none, toggleBtnStyle))
			{
				base.Property.State.Expanded = !base.Property.State.Expanded;
			}
			btnRect.width -= foldoutRect.width;
			if (!base.Property.State.Expanded)
			{
				foldoutRect.x -= 1f;
				foldoutRect.yMin -= 1f;
			}
			if (base.Property.State.Expanded)
			{
				EditorIcons.TriangleDown.Draw(foldoutRect, 16f);
			}
			else
			{
				EditorIcons.TriangleLeft.Draw(foldoutRect, 16f);
			}
			if (buttonAttribute != null && buttonAttribute.HasDefinedIcon)
			{
				if (SirenixEditorGUI.SDFIconButton(btnRect, label, buttonAttribute.Icon, buttonIconAlignment, style))
				{
					InvokeButton();
				}
			}
			else if (GUI.Button(btnRect, label, style))
			{
				InvokeButton();
			}
			GUIHelper.PopColor();
			EditorGUI.indentLevel++;
			DrawParameters(appendButton: false);
			EditorGUI.indentLevel--;
		}

		private void DrawParameters(bool appendButton)
		{
			if (SirenixEditorGUI.BeginFadeGroup(this, base.Property.State.Expanded || expanded))
			{
				GUILayout.Space(0f);
				for (int i = 0; i < base.Property.Children.Count; i++)
				{
					bool isResult = false;
					if (hasReturnValue && i == base.Property.Children.Count - 1)
					{
						if (!shouldDrawResult || (!hasInvokedOnce && i != 0))
						{
							break;
						}
						if (i != 0)
						{
							SirenixEditorGUI.DrawThickHorizontalSeparator();
						}
						isResult = true;
					}
					if (isResult && !hasInvokedOnce)
					{
						GUIHelper.PushGUIEnabled(enabled: false);
					}
					base.Property.Children[i].Draw();
					if (isResult && !hasInvokedOnce)
					{
						GUIHelper.PopGUIEnabled();
					}
				}
				if (appendButton)
				{
					Rect rect = EditorGUILayout.BeginVertical(SirenixGUIStyles.BottomBoxPadding).Expand(3f);
					SirenixEditorGUI.DrawHorizontalLineSeperator(rect.x, rect.y, rect.width);
					DrawNormalButton();
					EditorGUILayout.EndVertical();
				}
			}
			SirenixEditorGUI.EndFadeGroup();
		}

		private void InvokeButton()
		{
			try
			{
				bool inspectResultInDropdown = hasReturnValue && Event.current.button == 1;
				GUIHelper.RemoveFocusControl();
				GUIHelper.RequestRepaint();
				if (((base.Property.Info.GetMemberInfo() as MethodInfo) ?? base.Property.Info.GetMethodDelegate().Method).IsGenericMethodDefinition)
				{
					Debug.LogError("Cannot invoke a generic method definition.");
					return;
				}
				if (buttonAttribute == null || buttonAttribute.DirtyOnClick)
				{
					if (base.Property.ParentValueProperty != null)
					{
						base.Property.ParentValueProperty.RecordForUndo("Clicked Button '" + base.Property.NiceName + "'", forceCompleteObjectUndo: true);
					}
					foreach (UnityEngine.Object target in base.Property.SerializationRoot.ValueEntry.WeakValues.OfType<UnityEngine.Object>())
					{
						InspectorUtilities.RegisterUnityObjectDirty(target);
					}
				}
				if (buttonActionResolver != null)
				{
					buttonActionResolver.DoActionForAllSelectionIndices();
				}
				else if (buttonValueResolver != null)
				{
					for (int i = 0; i < base.Property.Tree.WeakTargets.Count; i++)
					{
						object result = buttonValueResolver.GetValue(i);
						base.Property.Children[base.Property.Children.Count - 1].ValueEntry.WeakValues[i] = result;
					}
				}
				else
				{
					MethodInfo methodInfo = (MethodInfo)base.Property.Info.GetMemberInfo();
					if (methodInfo != null)
					{
						InvokeMethodInfo(methodInfo);
					}
					else
					{
						InvokeDelegate();
					}
				}
				if (inspectResultInDropdown)
				{
					object resultValue = base.Property.Children[base.Property.Children.Count - 1].ValueEntry.WeakSmartValue;
					OdinEditorWindow.InspectObjectInDropDown(new MethodResultInspector
					{
						Value = resultValue
					});
				}
			}
			finally
			{
				GUIHelper.ExitGUI(removeFocusControl: true);
			}
		}

		private void InvokeDelegate()
		{
			try
			{
				int argCount = (hasReturnValue ? (base.Property.Children.Count - 1) : base.Property.Children.Count);
				object[] arguments = new object[argCount];
				for (int j = 0; j < arguments.Length; j++)
				{
					arguments[j] = base.Property.Children[j].ValueEntry.WeakSmartValue;
				}
				object result = base.Property.Info.GetMethodDelegate().DynamicInvoke(arguments);
				for (int i = 0; i < arguments.Length; i++)
				{
					base.Property.Children[i].ValueEntry.WeakSmartValue = arguments[i];
				}
				if (hasReturnValue)
				{
					base.Property.Children[base.Property.Children.Count - 1].ValueEntry.WeakSmartValue = result;
				}
				if (!hasInvokedOnce)
				{
					base.Property.Tree.DelayActionUntilRepaint(delegate
					{
						hasInvokedOnce = true;
					});
				}
			}
			catch (TargetInvocationException ex)
			{
				if (ex.IsExitGUIException())
				{
					throw ex.AsExitGUIException();
				}
				Debug.LogException(ex);
			}
			catch (ExitGUIException ex2)
			{
				throw ex2;
			}
			catch (Exception ex3)
			{
				if (ex3.IsExitGUIException())
				{
					throw ex3.AsExitGUIException();
				}
				Debug.LogException(ex3);
			}
		}

		private void InvokeMethodInfo(MethodInfo methodInfo)
		{
			InspectorProperty parentValueProperty = base.Property.ParentValueProperty;
			ImmutableList targets = base.Property.ParentValues;
			int argCount = (hasReturnValue ? (base.Property.Children.Count - 1) : base.Property.Children.Count);
			for (int i = 0; i < targets.Count; i++)
			{
				object value = targets[i];
				if (value == null && !methodInfo.IsStatic)
				{
					continue;
				}
				try
				{
					object[] arguments = new object[argCount];
					for (int j = 0; j < arguments.Length; j++)
					{
						arguments[j] = base.Property.Children[j].ValueEntry.WeakSmartValue;
					}
					object result = ((!methodInfo.IsStatic) ? methodInfo.Invoke(value, arguments) : methodInfo.Invoke(null, arguments));
					for (int k = 0; k < arguments.Length; k++)
					{
						base.Property.Children[k].ValueEntry.WeakSmartValue = arguments[k];
					}
					if (hasReturnValue)
					{
						base.Property.Children[base.Property.Children.Count - 1].ValueEntry.WeakSmartValue = result;
					}
					if (!hasInvokedOnce)
					{
						base.Property.Tree.DelayActionUntilRepaint(delegate
						{
							hasInvokedOnce = true;
						});
					}
				}
				catch (TargetInvocationException ex)
				{
					if (ex.IsExitGUIException())
					{
						throw ex.AsExitGUIException();
					}
					Debug.LogException(ex);
				}
				catch (ExitGUIException ex2)
				{
					throw ex2;
				}
				catch (Exception ex3)
				{
					if (ex3.IsExitGUIException())
					{
						throw ex3.AsExitGUIException();
					}
					Debug.LogException(ex3);
				}
				if (parentValueProperty != null && value.GetType().IsValueType)
				{
					parentValueProperty.ValueEntry.WeakValues[i] = value;
				}
			}
		}
	}
}
