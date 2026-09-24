using System;
using Sirenix.OdinInspector.Editor.ActionResolvers;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	/// <summary>
	/// Draws properties marked with <see cref="T:Sirenix.OdinInspector.OnCollectionChangedAttribute" />.
	/// </summary>
	/// <seealso cref="T:Sirenix.OdinInspector.OnCollectionChangedAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.OnValueChangedAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.OnInspectorGUIAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.ValidateInputAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.InfoBoxAttribute" />
	[DrawerPriority(DrawerPriorityLevel.SuperPriority)]
	public sealed class OnCollectionChangedAttributeDrawer : OdinAttributeDrawer<OnCollectionChangedAttribute>, IDisposable
	{
		private static readonly NamedValue[] ActionArgs = new NamedValue[1]
		{
			new NamedValue("info", typeof(CollectionChangeInfo))
		};

		private ActionResolver onBefore;

		private ActionResolver onAfter;

		private ICollectionResolver resolver;

		protected override bool CanDrawAttributeProperty(InspectorProperty property)
		{
			return property.ChildResolver is ICollectionResolver;
		}

		protected override void Initialize()
		{
			resolver = (ICollectionResolver)base.Property.ChildResolver;
			if (base.Attribute.Before != null)
			{
				onBefore = ActionResolver.Get(base.Property, base.Attribute.Before, ActionArgs);
				if (!onBefore.HasError)
				{
					resolver.OnBeforeChange += OnBeforeChange;
				}
			}
			if (base.Attribute.After != null)
			{
				onAfter = ActionResolver.Get(base.Property, base.Attribute.After, ActionArgs);
				if (!onAfter.HasError)
				{
					resolver.OnAfterChange += OnAfterChange;
				}
			}
			if ((onAfter == null || !onAfter.HasError) && (onBefore == null || !onBefore.HasError))
			{
				base.SkipWhenDrawing = true;
			}
		}

		private void OnBeforeChange(CollectionChangeInfo info)
		{
			onBefore.Context.NamedValues.Set("info", info);
			onBefore.DoAction(info.SelectionIndex);
		}

		private void OnAfterChange(CollectionChangeInfo info)
		{
			onAfter.Context.NamedValues.Set("info", info);
			onAfter.DoAction(info.SelectionIndex);
		}

		protected override void DrawPropertyLayout(GUIContent label)
		{
			ActionResolver.DrawErrors(onBefore, onAfter);
			CallNextDrawer(label);
		}

		public void Dispose()
		{
			if (onBefore != null)
			{
				resolver.OnBeforeChange -= OnBeforeChange;
			}
			if (onAfter != null)
			{
				resolver.OnAfterChange -= OnAfterChange;
			}
		}
	}
}
