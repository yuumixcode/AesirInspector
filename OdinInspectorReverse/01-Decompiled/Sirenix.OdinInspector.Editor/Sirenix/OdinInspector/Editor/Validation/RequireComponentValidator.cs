using System;
using Sirenix.Utilities;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Validation
{
	public class RequireComponentValidator<T> : AttributeValidator<RequireComponent, T> where T : Component
	{
		private class AddWithPrefabSupport
		{
			public bool AddToPrefab = true;
		}

		public override bool CanValidateProperty(InspectorProperty property)
		{
			return property == property.Tree.RootProperty;
		}

		protected override void Validate(ValidationResult result)
		{
			T value = base.ValueEntry.SmartValue;
			bool ignore = false;
			if (value == null)
			{
				ignore = true;
			}
			if (ignore)
			{
				result.ResultType = ValidationResultType.IgnoreResult;
				return;
			}
			Validate(result, value, base.Attribute.m_Type0);
			Validate(result, value, base.Attribute.m_Type1);
			Validate(result, value, base.Attribute.m_Type2);
		}

		private void Validate(ValidationResult result, T target, Type type)
		{
			if (!(type != null) || !typeof(Component).IsAssignableFrom(type) || !(target.gameObject.GetComponent(type) == null))
			{
				return;
			}
			string msg = target.gameObject.name + " is missing required component of type '" + type.GetNiceName() + "'";
			if (!IsAbstractOrSpecialCase(type))
			{
				if (base.Property.Tree.PrefabModificationHandler.HasPrefabs)
				{
					result.AddError(msg).WithFix(Fix.Create("Add missing components", delegate(AddWithPrefabSupport x)
					{
						if (x.AddToPrefab)
						{
							T correspondingObjectFromOriginalSource = PrefabUtility.GetCorrespondingObjectFromOriginalSource(target);
							AddMissingComponents(correspondingObjectFromOriginalSource.gameObject);
						}
						else
						{
							AddMissingComponents(target.gameObject);
						}
					}));
				}
				else
				{
					result.AddError(msg).WithFix(Fix.Create("Add missing components", delegate
					{
						AddMissingComponents(target.gameObject);
					}));
				}
			}
			else
			{
				result.AddError(msg);
			}
		}

		private static bool IsAbstractOrSpecialCase(Type type)
		{
			if (type == typeof(Collider))
			{
				return true;
			}
			return type.IsAbstract;
		}

		private void AddMissingComponents(GameObject target)
		{
			if (base.Attribute.m_Type0 != null && !target.GetComponent(base.Attribute.m_Type0) && !IsAbstractOrSpecialCase(base.Attribute.m_Type0))
			{
				Undo.AddComponent(target, base.Attribute.m_Type0);
			}
			if (base.Attribute.m_Type1 != null && !target.GetComponent(base.Attribute.m_Type1) && !IsAbstractOrSpecialCase(base.Attribute.m_Type1))
			{
				Undo.AddComponent(target, base.Attribute.m_Type1);
			}
			if (base.Attribute.m_Type2 != null && !target.GetComponent(base.Attribute.m_Type2) && !IsAbstractOrSpecialCase(base.Attribute.m_Type2))
			{
				Undo.AddComponent(target, base.Attribute.m_Type2);
			}
		}
	}
}
