using System;
using Sirenix.OdinInspector.Editor.ActionResolvers;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	/// <summary>
	/// Draws properties marked with <see cref="T:Sirenix.OdinInspector.OnValueChangedAttribute" />.
	/// </summary>
	/// <seealso cref="T:Sirenix.OdinInspector.OnCollectionChangedAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.OnValueChangedAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.OnInspectorGUIAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.ValidateInputAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.InfoBoxAttribute" />
	[DrawerPriority(DrawerPriorityLevel.SuperPriority)]
	public sealed class OnValueChangedAttributeDrawer<T> : OdinAttributeDrawer<OnValueChangedAttribute, T>, IDisposable
	{
		private ActionResolver onChangeAction;

		private bool subscribedToOnUndoRedo;

		protected override void Initialize()
		{
			if (base.Attribute.InvokeOnUndoRedo)
			{
				base.Property.Tree.OnUndoRedoPerformed += OnUndoRedo;
				subscribedToOnUndoRedo = true;
			}
			onChangeAction = ActionResolver.Get(base.Property, base.Attribute.Action);
			Action<int> triggerAction = TriggerAction;
			base.ValueEntry.OnValueChanged += triggerAction;
			if (base.Attribute.IncludeChildren || typeof(T).IsValueType)
			{
				base.ValueEntry.OnChildValueChanged += triggerAction;
			}
			if (base.Attribute.InvokeOnInitialize && !onChangeAction.HasError)
			{
				onChangeAction.DoActionForAllSelectionIndices();
			}
		}

		protected override void DrawPropertyLayout(GUIContent label)
		{
			onChangeAction.DrawError();
			CallNextDrawer(label);
		}

		private void OnUndoRedo()
		{
			for (int i = 0; i < base.ValueEntry.ValueCount; i++)
			{
				TriggerAction(i);
			}
		}

		private void TriggerAction(int selectionIndex)
		{
			base.Property.Tree.DelayActionUntilRepaint(delegate
			{
				onChangeAction.DoAction(selectionIndex);
			});
		}

		public void Dispose()
		{
			if (subscribedToOnUndoRedo)
			{
				base.Property.Tree.OnUndoRedoPerformed -= OnUndoRedo;
			}
		}
	}
}
