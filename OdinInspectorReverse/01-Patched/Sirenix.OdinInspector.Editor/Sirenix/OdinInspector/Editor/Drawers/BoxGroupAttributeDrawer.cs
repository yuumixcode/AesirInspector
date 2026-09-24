using Sirenix.OdinInspector.Editor.ValueResolvers;
using Sirenix.Utilities.Editor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	/// <summary>
	/// Draws all properties grouped together with the <see cref="T:Sirenix.OdinInspector.BoxGroupAttribute" />
	/// </summary>
	/// <seealso cref="T:Sirenix.OdinInspector.BoxGroupAttribute" />
	public class BoxGroupAttributeDrawer : OdinGroupDrawer<BoxGroupAttribute>
	{
		private ValueResolver<string> labelGetter;

		/// <summary>
		/// Initializes this instance.
		/// </summary>
		protected override void Initialize()
		{
			labelGetter = ValueResolver.GetForString(base.Property, base.Attribute.LabelText ?? base.Attribute.GroupName);
		}

		/// <summary>
		/// Draws the property.
		/// </summary>
		protected override void DrawPropertyLayout(GUIContent label)
		{
			labelGetter.DrawError();
			string headerLabel = null;
			if (base.Attribute.ShowLabel)
			{
				headerLabel = labelGetter.GetValue();
				if (string.IsNullOrEmpty(headerLabel))
				{
					headerLabel = "Null";
				}
			}
			SirenixEditorGUI.BeginBox(headerLabel, base.Attribute.CenterLabel);
			for (int i = 0; i < base.Property.Children.Count; i++)
			{
				InspectorProperty child = base.Property.Children[i];
				child.Draw(child.Label);
			}
			SirenixEditorGUI.EndBox();
		}
	}
}
