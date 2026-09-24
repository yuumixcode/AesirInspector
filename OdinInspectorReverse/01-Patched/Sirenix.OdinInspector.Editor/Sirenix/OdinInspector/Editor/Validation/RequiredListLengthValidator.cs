using System;
using System.Collections;
using Sirenix.OdinInspector.Editor.ValueResolvers;

namespace Sirenix.OdinInspector.Editor.Validation
{
	public class RequiredListLengthValidator : AttributeValidator<RequiredListLengthAttribute>
	{
		private ValueResolver<int> minLength;

		private ValueResolver<int> maxLength;

		protected override void Initialize()
		{
			minLength = ValueResolver.Get(base.Property, base.Attribute.MinLengthGetter, base.Attribute.MinLength);
			maxLength = ValueResolver.Get(base.Property, base.Attribute.MaxLengthGetter, base.Attribute.MaxLength);
		}

		public override bool CanValidateProperty(InspectorProperty property)
		{
			if (property.ChildResolver is ICollectionResolver && property.ValueEntry != null && typeof(IList).IsAssignableFrom(property.Info.TypeOfValue))
			{
				RequiredListLengthAttribute attr = property.GetAttribute<RequiredListLengthAttribute>();
				if (attr.PrefabKindIsSet)
				{
					PrefabKind targetKind = attr.PrefabKind;
					PrefabKind kind = OdinPrefabUtility.GetPrefabKind(property);
					if ((kind & targetKind) != PrefabKind.None)
					{
						return true;
					}
					return false;
				}
				return true;
			}
			return false;
		}

		protected override void Validate(ValidationResult result)
		{
			if (minLength.HasError)
			{
				result.AddError(minLength.ErrorMessage);
				return;
			}
			if (maxLength.HasError)
			{
				result.AddError(maxLength.ErrorMessage);
				return;
			}
			InspectorProperty p = base.Property;
			bool hasMin = base.Attribute.MinLengthIsSet || base.Attribute.MinLengthGetter != null;
			bool hasMax = base.Attribute.MaxLengthIsSet || base.Attribute.MaxLengthGetter != null;
			if (!hasMax && !hasMin)
			{
				return;
			}
			object val = p.ValueEntry.WeakSmartValue;
			if (val == null)
			{
				result.AddError(p.NiceName + " is required");
				return;
			}
			IList collection = p.ValueEntry.WeakValues[0] as IList;
			int count = collection.Count;
			int min = minLength.GetValue();
			int max = maxLength.GetValue();
			if (min == max && count != min && hasMin && hasMin)
			{
				WithFix(ref result.AddError($"Collection should have exactly <b>{min}</b> number of elements, but has <color=red>{count}</color>."), p, min);
			}
			else if (hasMin && hasMax)
			{
				if (min > max)
				{
					result.AddError($"The minimum required length ({min}) is more than the maximum required length ({max}).");
				}
				else if (count < min)
				{
					WithFix(ref result.AddError($"Collection should contain between <b>{min} and {max}</b> number of elements, but only has <color=red>{count}</color>."), p, min);
				}
				else if (count > max)
				{
					WithFix(ref result.AddError($"Collection should contain between <b>{min} and {max}</b> number of elements, but has <color=red>{count}</color>."), p, max);
				}
			}
			else if (hasMin && count < min)
			{
				WithFix(ref result.AddError($"Collection should contain at least <b>{min}</b> number of elements, but only has <color=red>{count}</color>."), p, min);
			}
			else if (hasMax && count > max)
			{
				WithFix(ref result.AddError($"Collection should not contain more than <b>{max}</b> number of elements, but has <color=red>{count}</color>."), p, max);
			}
		}

		public static object GetDefault(Type type)
		{
			if (type.IsValueType)
			{
				return Activator.CreateInstance(type);
			}
			return null;
		}

		private static void WithFix(ref ResultItem err, InspectorProperty p, int targetSize)
		{
			if (!(p.ChildResolver is IOrderedCollectionResolver) || p.ParentValues.Count != 1)
			{
				return;
			}
			err.EnableRichText();
			err.WithFix("Set length to " + targetSize, delegate
			{
				p.ChildResolver.ForceUpdateChildCount();
				IList list = p.ValueEntry.WeakValues[0] as IList;
				int count = list.Count;
				IOrderedCollectionResolver orderedCollectionResolver = p.ChildResolver as IOrderedCollectionResolver;
				int num = Math.Abs(count - targetSize);
				if (targetSize > count)
				{
					for (int i = 0; i < num; i++)
					{
						orderedCollectionResolver.QueueAdd(GetDefault(orderedCollectionResolver.ElementType), 0);
					}
				}
				else
				{
					for (int j = 0; j < num; j++)
					{
						orderedCollectionResolver.QueueRemoveAt(targetSize - (1 + j), 0);
					}
				}
				orderedCollectionResolver.ApplyChanges();
			});
		}
	}
}
