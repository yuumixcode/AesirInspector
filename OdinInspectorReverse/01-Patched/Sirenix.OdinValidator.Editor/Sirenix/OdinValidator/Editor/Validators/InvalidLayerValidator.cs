using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor.Validation;
using UnityEngine;

namespace Sirenix.OdinValidator.Editor.Validators
{
	public class InvalidLayerValidator : RootObjectValidator<GameObject>
	{
		private class LayerFix
		{
			[LayerSelector]
			public int Layer;
		}

		[ValueDropdown("@LayerSelectorAttribute.GetLayers()")]
		[IncludeMyAttributes]
		internal class LayerSelectorAttribute : Attribute
		{
			private static IEnumerable<ValueDropdownItem> GetLayers()
			{
				for (int i = 0; i < 32; i++)
				{
					string layer = LayerMask.LayerToName(i);
					if (!string.IsNullOrEmpty(layer))
					{
						yield return new ValueDropdownItem(layer, i);
					}
				}
			}
		}

		protected override void Validate(ValidationResult result)
		{
			if (string.IsNullOrEmpty(LayerMask.LayerToName(base.Object.layer)))
			{
				string msg = "Invalid Layer " + base.Object.layer;
				result.AddError(msg).WithFix(Fix.Create(delegate(LayerFix args)
				{
					base.Object.layer = args.Layer;
				}));
			}
		}
	}
}
