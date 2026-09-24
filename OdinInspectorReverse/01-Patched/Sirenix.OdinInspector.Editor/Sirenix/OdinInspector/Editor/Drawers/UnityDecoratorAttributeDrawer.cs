using System;
using System.Reflection;
using Sirenix.OdinInspector.Editor.Internal.UIToolkitIntegration;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	/// <summary>
	/// Draws all Unity DecoratorDrawers within prepend attribute drawers within Odin.
	/// </summary>
	[DrawerPriority(0.0, 1.0, 0.0)]
	[OdinDontRegister]
	public sealed class UnityDecoratorAttributeDrawer<TDrawer, TAttribute, TAttributeConstraint> : OdinAttributeDrawer<TAttribute>, IDisposable where TDrawer : DecoratorDrawer, new() where TAttribute : TAttributeConstraint where TAttributeConstraint : PropertyAttribute
	{
		private static readonly FieldInfo InternalAttributeFieldInfo;

		private static readonly ValueSetter<TDrawer, Attribute> SetAttribute;

		private TDrawer drawer = new TDrawer();

		private bool hasCheckedForVisualElement;

		private OdinImGuiElement imguiElement;

		/// <summary>
		/// Initializes the <see cref="!:UnityDecoratorAttributeDrawer&lt;TDrawer, TAttribute&gt;" /> class.
		/// </summary>
		static UnityDecoratorAttributeDrawer()
		{
			InternalAttributeFieldInfo = typeof(TDrawer).GetField("m_Attribute", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if (InternalAttributeFieldInfo == null)
			{
				Debug.LogError("Could not find the internal Unity field 'DecoratorDrawer.m_Attribute'; UnityDecoratorDrawer alias '" + typeof(UnityDecoratorAttributeDrawer<TDrawer, TAttribute, TAttributeConstraint>).GetNiceName() + "' has been disabled.");
			}
			else
			{
				SetAttribute = EmitUtilities.CreateInstanceFieldSetter<TDrawer, Attribute>(InternalAttributeFieldInfo);
			}
		}

		public void Dispose()
		{
			if (imguiElement != null)
			{
				imguiElement.RemoveFromHierarchy();
				imguiElement = null;
			}
		}

		/// <summary>
		/// Draws the property.
		/// </summary>
		protected override void DrawPropertyLayout(GUIContent label)
		{
			if (base.Property.Parent != null && base.Property.Parent.ChildResolver is ICollectionResolver)
			{
				CallNextDrawer(label);
				return;
			}
			if (SetAttribute == null)
			{
				SirenixEditorGUI.MessageBox("Could not find the internal Unity field 'DecoratorDrawer.m_Attribute'; UnityDecoratorDrawer alias '" + typeof(UnityDecoratorAttributeDrawer<TDrawer, TAttribute, TAttributeConstraint>).GetNiceName() + "' has been disabled.", MessageType.Error, GlobalConfig<GeneralDrawerConfig>.Instance.MessageBoxFontSize);
				return;
			}
			SetAttribute(ref this.drawer, base.Attribute);
			if (!hasCheckedForVisualElement && GlobalConfig<GeneralDrawerConfig>.Instance.EnableUIToolkitSupport)
			{
				hasCheckedForVisualElement = true;
				if (DrawerUtilities.DecoratorDrawerCreatePropertyGUIMethod != null && DrawerUtilities.DecoratorDrawerCreatePropertyGUIMethod.Invoke(this.drawer, null) is VisualElement element)
				{
					imguiElement = new OdinImGuiElement(element);
				}
			}
			bool willDrawPropertyField = false;
			foreach (OdinDrawer drawer in base.Property.GetActiveDrawerChain())
			{
				if (drawer is IUnityPropertyFieldDrawer pDrawer && !drawer.SkipWhenDrawing && pDrawer.WillDrawPropertyField)
				{
					willDrawPropertyField = true;
					break;
				}
			}
			if (!willDrawPropertyField)
			{
				if (GlobalConfig<GeneralDrawerConfig>.Instance.EnableUIToolkitSupport && imguiElement != null)
				{
					ImguiElementUtils.EmbedVisualElementAndDrawItHere(imguiElement);
				}
				else
				{
					float height = this.drawer.GetHeight();
					Rect position = EditorGUILayout.GetControlRect(false, height);
					this.drawer.OnGUI(position);
				}
			}
			CallNextDrawer(label);
		}
	}
}
