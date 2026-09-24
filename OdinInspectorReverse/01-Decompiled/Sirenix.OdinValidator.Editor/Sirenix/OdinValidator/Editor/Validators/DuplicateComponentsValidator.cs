using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.OdinInspector.Editor.Validation;
using Sirenix.Utilities;
using UnityEngine;
using UnityEngine.Serialization;

namespace Sirenix.OdinValidator.Editor.Validators
{
	public class DuplicateComponentsValidator : RootObjectValidator<GameObject>
	{
		[Serializable]
		public struct PrefabException : IEquatable<PrefabException>
		{
			[HideLabel]
			[HorizontalGroup(0f, 0, 0, 0f)]
			public GameObject Go;

			[HorizontalGroup(0f, 0, 0, 0f)]
			[HideLabel]
			public Type Type;

			public PrefabException(GameObject go, Type type)
			{
				Go = go;
				Type = type;
			}

			public override int GetHashCode()
			{
				int hashCode = 819525008;
				if ((bool)Go)
				{
					hashCode = hashCode * -1521134295 + Go.GetHashCode();
				}
				if (Type != null)
				{
					hashCode = hashCode * -1521134295 + Type.GetHashCode();
				}
				return hashCode;
			}

			public override bool Equals(object obj)
			{
				if (obj is PrefabException exception)
				{
					return Equals(exception);
				}
				return false;
			}

			public bool Equals(PrefabException other)
			{
				if (Go == other.Go)
				{
					return Type == other.Type;
				}
				return false;
			}

			public static bool operator ==(PrefabException left, PrefabException right)
			{
				return left.Equals(right);
			}

			public static bool operator !=(PrefabException left, PrefabException right)
			{
				return !(left == right);
			}
		}

		private static HashSet<Type> buffer = new HashSet<Type>();

		[Tooltip("The severity of the validation result.")]
		public ValidatorSeverity Severity = ValidatorSeverity.Warning;

		[FormerlySerializedAs("Exceptions")]
		[Tooltip("A list of component types to ignore when validating. You can also add components to this list by right-clicking duplicate components issues in the validator.")]
		public Type[] TypeExceptions = new Type[0];

		[Searchable]
		[AssetsOnly]
		[Tooltip("A list of prefabs to ignore when validating. You can also add prefabs to this list by right-clicking duplicate components issues in the validator.")]
		public List<PrefabException> PrefabExceptions = new List<PrefabException>();

		protected override void Validate(ValidationResult result)
		{
			Component[] cmps = base.Object.GetComponents(typeof(Component));
			buffer.Clear();
			foreach (Component cmp in cmps)
			{
				if (!cmp)
				{
					continue;
				}
				Type cmpType = cmp.GetType();
				if (buffer.Add(cmpType) || TypeExceptions.Contains(cmpType))
				{
					continue;
				}
				bool isPartOfPrefab = (OdinPrefabUtility.GetPrefabKind(cmp.gameObject) & (PrefabKind.PrefabInstance | PrefabKind.PrefabAsset)) != 0;
				if (isPartOfPrefab)
				{
					List<PrefabException> prefabExceptions = PrefabExceptions;
					if (prefabExceptions != null && prefabExceptions.Count > 0)
					{
						GameObject nearestPrefab = OdinPrefabUtility.GetNearestPrefabAsset(cmp);
						if (nearestPrefab != null && PrefabExceptions.Contains(new PrefabException(nearestPrefab, cmpType)))
						{
							break;
						}
					}
				}
				ref ResultItem err = ref result.Add(Severity, "GameObject contains duplicate '" + cmpType.Name + "' components.").WithModifyRuleDataContextClick("Ignore duplicates of " + cmpType.GetNiceName(), delegate(DuplicateComponentsValidator data)
				{
					data.TypeExceptions = data.TypeExceptions.AppendWith(cmpType).Distinct().ToArray();
				});
				if (!isPartOfPrefab)
				{
					break;
				}
				err.WithModifyRuleDataContextClick("Ignore duplicates from this prefab", delegate(DuplicateComponentsValidator data)
				{
					GameObject nearestPrefabAsset = OdinPrefabUtility.GetNearestPrefabAsset(cmp);
					if (nearestPrefabAsset != null)
					{
						if (data.PrefabExceptions == null)
						{
							data.PrefabExceptions = new List<PrefabException>();
						}
						data.PrefabExceptions.Add(new PrefabException(nearestPrefabAsset, cmpType));
					}
				});
				break;
			}
		}
	}
}
