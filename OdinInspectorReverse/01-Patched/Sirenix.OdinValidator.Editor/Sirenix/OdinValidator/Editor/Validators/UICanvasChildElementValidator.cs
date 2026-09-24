using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor.Validation;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinValidator.Editor.Validators
{
	public class UICanvasChildElementValidator : RootObjectValidator<GameObject>
	{
		internal class FixArgs
		{
			[ValueDropdown("GetCanvases")]
			public Canvas Canvas;

			private IEnumerable<Canvas> GetCanvases()
			{
				return Resources.FindObjectsOfTypeAll<Canvas>();
			}
		}

		[Tooltip("The severity of the validation result.")]
		public ValidatorSeverity ValidatorSeverity = ValidatorSeverity.Warning;

		[Tooltip("The layers that are considered UI layers.")]
		public List<string> UILayers = new List<string> { "UI" };

		protected override void Validate(ValidationResult result)
		{
			if (!PrefabUtility.IsPartOfAnyPrefab(base.Object) && UILayers.Any((string layerName) => base.Object.layer == LayerMask.NameToLayer(layerName)) && base.Object.GetComponentsInParent<Canvas>(includeInactive: true).Count() == 0)
			{
				result.Add(ValidatorSeverity, "UI object is not a child of a canvas.").WithFix(Fix.Create("Add To Selected Canvas", delegate(FixArgs args)
				{
					base.Object.transform.SetParent(args.Canvas.transform);
				}));
			}
		}
	}
}
