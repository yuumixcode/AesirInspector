using System.Globalization;
using System.Linq;
using Sirenix.OdinInspector.Editor.ValueResolvers;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	public abstract class UnitAttributeDrawer<TPrimitive> : OdinAttributeDrawer<UnitAttribute, TPrimitive>, IDefinesGenericMenuItems
	{
		private static readonly string FloatingFieldFormatString = "G7";

		private static readonly string IntegerFieldFormatString = "#,##0";

		private UnitInfo baseUnitInfo;

		private UnitInfo displayUnitInfo;

		private string lastResolvedUnitName;

		private ValueResolver<string> displayUnitNameResolver;

		private ValueResolver<Units> displayUnitEnumResolver;

		private string errorMessage;

		private bool isFloatingPointNumber;

		protected override void Initialize()
		{
			if (base.Attribute.Base != Units.Unset)
			{
				if (!UnitNumberUtility.TryGetUnitInfo(base.Attribute.Base, out baseUnitInfo))
				{
					errorMessage = $"Failed to find unit '{base.Attribute.Base}'.";
				}
			}
			else if (!UnitNumberUtility.TryGetUnitInfoByName(base.Attribute.BaseName, out baseUnitInfo))
			{
				errorMessage = "Failed to find unit by name '" + base.Attribute.BaseName + "'.";
			}
			if (base.Attribute.Display != Units.Unset)
			{
				if (!UnitNumberUtility.TryGetUnitInfo(base.Attribute.Display, out displayUnitInfo))
				{
					errorMessage = $"Failed to find unit '{base.Attribute.Display}'.";
				}
			}
			else if (base.Attribute.DisplayName.Length == 0)
			{
				errorMessage = "No display unit set.";
			}
			else if (base.Attribute.DisplayName[0] == '$' || base.Attribute.DisplayName[0] == '@')
			{
				displayUnitEnumResolver = ValueResolver.Get<Units>(base.Property, base.Attribute.DisplayName);
				displayUnitNameResolver = ValueResolver.Get<string>(base.Property, base.Attribute.DisplayName);
				if (displayUnitNameResolver.HasError && displayUnitNameResolver.HasError)
				{
					errorMessage = displayUnitNameResolver.ErrorMessage;
				}
			}
			else if (!UnitNumberUtility.TryGetUnitInfoByName(base.Attribute.DisplayName, out displayUnitInfo))
			{
				errorMessage = "Failed to find unit by name '" + base.Attribute.DisplayName + "'.";
			}
			if (baseUnitInfo != null && displayUnitInfo != null && baseUnitInfo.UnitCategory != displayUnitInfo.UnitCategory)
			{
				errorMessage = "Cannot convert between '" + baseUnitInfo.Name + "' and '" + displayUnitInfo.Name + "'.";
			}
			isFloatingPointNumber = typeof(TPrimitive) == typeof(float) || typeof(TPrimitive) == typeof(double) || typeof(TPrimitive) == typeof(decimal);
		}

		protected override void DrawPropertyLayout(GUIContent label)
		{
			if (Event.current.type == EventType.Layout && (displayUnitEnumResolver != null || displayUnitNameResolver != null))
			{
				if (displayUnitEnumResolver != null && !displayUnitEnumResolver.HasError)
				{
					Units u = displayUnitEnumResolver.GetValue();
					if (!UnitNumberUtility.TryGetUnitInfo(u, out displayUnitInfo))
					{
						Debug.LogError($"Failed to find unit: '{u}'.");
					}
				}
				else if (displayUnitNameResolver != null && !displayUnitNameResolver.HasError)
				{
					string n = displayUnitNameResolver.GetValue();
					if (n != lastResolvedUnitName && !UnitNumberUtility.TryGetUnitInfoByName(n ?? "", out displayUnitInfo))
					{
						Debug.LogError("Failed to find unit with name: '" + n + "'.");
					}
					lastResolvedUnitName = n;
				}
				if (displayUnitInfo == null)
				{
					displayUnitInfo = baseUnitInfo;
				}
				else if (!UnitNumberUtility.CanConvertBetween(baseUnitInfo, displayUnitInfo))
				{
					Debug.LogError("Cannot convert between " + baseUnitInfo.Name + " and " + displayUnitInfo.Name + ".");
					displayUnitInfo = baseUnitInfo;
				}
			}
			if (!string.IsNullOrWhiteSpace(errorMessage))
			{
				SirenixEditorGUI.MessageBox(errorMessage, MessageType.Error, GlobalConfig<GeneralDrawerConfig>.Instance.MessageBoxFontSize);
			}
			else if (base.Attribute.DisplayAsString)
			{
				string str = string.Empty;
				if (Event.current.type == EventType.Repaint)
				{
					decimal d = ConvertUtility.Convert<decimal>(base.ValueEntry.SmartValue);
					str = UnitNumberUtility.ConvertUnitFromTo(d, baseUnitInfo, displayUnitInfo).ToString(isFloatingPointNumber ? FloatingFieldFormatString : IntegerFieldFormatString, CultureInfo.InvariantCulture) + " " + displayUnitInfo.Symbols[0];
				}
				Rect rect = EditorGUILayout.GetControlRect(GUILayoutOptions.MinWidth(0f));
				if (label != null)
				{
					rect = EditorGUI.PrefixLabel(rect, label);
				}
				EditorGUI.LabelField(rect, GUIHelper.TempContent(str));
			}
			else
			{
				EditorGUI.BeginChangeCheck();
				TPrimitive v = base.ValueEntry.SmartValue;
				v = DrawField(base.Property.ToFieldExpressionContext(), label, v, baseUnitInfo, displayUnitInfo);
				if (EditorGUI.EndChangeCheck())
				{
					base.ValueEntry.SmartValue = v;
				}
			}
		}

		protected abstract TPrimitive DrawField(FieldExpressionContext fieldExpressionContext, GUIContent label, TPrimitive value, UnitInfo baseUnitInfo, UnitInfo displayUnitInfo);

		public void PopulateGenericMenu(InspectorProperty property, GenericMenu genericMenu)
		{
			if (!base.Attribute.ForceDisplayUnit && errorMessage == null && displayUnitNameResolver == null && displayUnitEnumResolver == null)
			{
				foreach (UnitInfo x in from ui in UnitNumberUtility.GetAllUnitInfos()
					where ui.UnitCategory == baseUnitInfo.UnitCategory
					select ui)
				{
					UnitInfo unitInfo = x;
					genericMenu.AddItem(new GUIContent("Change Unit/" + unitInfo.Name), unitInfo == displayUnitInfo, delegate
					{
						displayUnitInfo = unitInfo;
					});
				}
			}
			genericMenu.AddItem(new GUIContent("Open In Unit Overview Window"), on: false, delegate
			{
				decimal value = UnitNumberUtility.ConvertUnitFromTo(ConvertUtility.Convert<decimal>(base.ValueEntry.SmartValue), baseUnitInfo, displayUnitInfo);
				UnitOverviewWindow.SelectUnitInfo(displayUnitInfo, value);
			});
		}
	}
}
