using System;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	/// <summary>
	/// Property drawer for nullables.
	/// </summary>
	public sealed class NullableDrawer<T> : OdinValueDrawer<T?>, IDisposable, IDefinesGenericMenuItems where T : struct
	{
		[ShowOdinSerializedPropertiesInInspector]
		private class Wrapper
		{
			public NullableValue<T> Value;

			public void SetValue(T? value)
			{
				if (value.HasValue)
				{
					Value = new NullableValue<T>();
					Value.Value = value.Value;
				}
			}
		}

		private PropertyTree<Wrapper> tree;

		protected override void Initialize()
		{
			Wrapper[] wrappers = new Wrapper[base.ValueEntry.ValueCount];
			for (int i = 0; i < wrappers.Length; i++)
			{
				Wrapper wrapper = new Wrapper();
				wrappers[i] = wrapper;
			}
			tree = new PropertyTree<Wrapper>(wrappers);
			tree.UpdateTree();
		}

		/// <summary>
		/// Draws the property.
		/// </summary>
		protected override void DrawPropertyLayout(GUIContent label)
		{
			IPropertyValueEntry<T?> entry = base.ValueEntry;
			for (int i = 0; i < tree.Targets.Count; i++)
			{
				tree.Targets[i].SetValue(entry.Values[i]);
			}
			tree.GetRootProperty(0).Label = label;
			tree.Draw(applyUndo: false);
			for (int j = 0; j < tree.Targets.Count; j++)
			{
				Wrapper value = tree.Targets[j];
				if (value.Value == null)
				{
					entry.Values[j] = null;
				}
				else
				{
					entry.Values[j] = value.Value.Value;
				}
			}
		}

		void IDefinesGenericMenuItems.PopulateGenericMenu(InspectorProperty property, GenericMenu genericMenu)
		{
			GUIContent content = new GUIContent("Set to null");
			IPropertyValueEntry<T?> entry = (IPropertyValueEntry<T?>)property.ValueEntry;
			if (entry.IsEditable && entry.SmartValue.HasValue)
			{
				genericMenu.AddItem(content, on: false, delegate
				{
					property.Tree.DelayActionUntilRepaint(delegate
					{
						entry.SmartValue = null;
					});
				});
			}
			else
			{
				genericMenu.AddDisabledItem(content);
			}
		}

		public void Dispose()
		{
			tree?.Dispose();
		}
	}
}
