using System;
using Sirenix.Utilities;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor
{
	public abstract class AttributeStateUpdater<TAttribute> : StateUpdater where TAttribute : Attribute
	{
		private TAttribute attribute;

		/// <summary>
		/// Gets the attribute that the OdinAttributeStateUpdater applies to.
		/// </summary>
		public TAttribute Attribute
		{
			get
			{
				if (attribute == null)
				{
					StateUpdater[] updaters = base.Property.StateUpdaters;
					int count = 0;
					foreach (StateUpdater updater in updaters)
					{
						if (updater.GetType() == GetType())
						{
							if (this == updater)
							{
								break;
							}
							count++;
						}
					}
					int savedCount = count;
					Type type = typeof(TAttribute);
					for (int j = 0; j < base.Property.Attributes.Count; j++)
					{
						Attribute attr = base.Property.Attributes[j];
						if (!(attr.GetType() != type))
						{
							if (count == 0)
							{
								attribute = (TAttribute)attr;
								break;
							}
							count--;
						}
					}
					if (attribute == null)
					{
						Debug.LogError("Could not find attribute '" + typeof(TAttribute).GetNiceName() + "' number " + savedCount + " for the state updater '" + GetType().GetNiceName() + "' number " + savedCount + "; not enough attributes of the required type on the property - why are there more drawers for the attribute than there are attributes?");
						attribute = base.Property.GetAttribute<TAttribute>();
					}
				}
				return attribute;
			}
		}
	}
	public abstract class AttributeStateUpdater<TAttribute, TValue> : AttributeStateUpdater<TAttribute> where TAttribute : Attribute
	{
		private IPropertyValueEntry<TValue> valueEntry;

		/// <summary>
		/// Gets the strongly typed ValueEntry of the OdinAttributeStateUpdater's property.
		/// </summary>
		public IPropertyValueEntry<TValue> ValueEntry
		{
			get
			{
				if (valueEntry == null)
				{
					valueEntry = base.Property.TryGetTypedValueEntry<TValue>();
					if (valueEntry == null)
					{
						base.Property.Update(forceUpdate: true);
						valueEntry = base.Property.TryGetTypedValueEntry<TValue>();
					}
				}
				return valueEntry;
			}
		}
	}
}
