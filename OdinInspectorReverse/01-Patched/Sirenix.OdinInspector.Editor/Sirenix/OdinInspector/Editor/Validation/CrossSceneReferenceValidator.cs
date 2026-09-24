using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Validation
{
	public class CrossSceneReferenceValidator : ValueValidator<Object>
	{
		protected override void Validate(ValidationResult result)
		{
			if (Application.isPlaying)
			{
				return;
			}
			Object value = base.Value;
			if (value == null)
			{
				return;
			}
			GameObject go = null;
			if (value is Component component)
			{
				go = component.gameObject;
			}
			else if (value is GameObject gameObject)
			{
				go = gameObject;
			}
			if (!(go == null))
			{
				GameObject otherGo = null;
				Component otherComponent = base.Property.Tree.RootProperty.ValueEntry.WeakSmartValue as Component;
				if (otherComponent != null)
				{
					otherGo = otherComponent.gameObject;
				}
				if (!(otherGo == null) && CheckForCrossSceneReferencing(go, otherGo))
				{
					result.AddError("Scene Mismatch (Cross Scene References Not Supported)");
				}
			}
		}

		public override bool CanValidateProperty(InspectorProperty property)
		{
			if (EditorSceneManager.preventCrossSceneReferences && property.ValueEntry.SerializationBackend != SerializationBackend.None)
			{
				return typeof(Component).IsAssignableFrom(property.Tree.RootProperty.ValueEntry.TypeOfValue);
			}
			return false;
		}

		internal static bool CheckForCrossSceneReferencing(GameObject go, GameObject go2)
		{
			if (EditorUtility.IsPersistent(go) || EditorUtility.IsPersistent(go2))
			{
				return false;
			}
			if (!go.scene.IsValid() || !go2.scene.IsValid())
			{
				return false;
			}
			return go.scene != go2.scene;
		}
	}
}
