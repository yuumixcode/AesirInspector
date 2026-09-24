using System;
using System.Collections.Generic;

namespace Sirenix.OdinInspector.Editor
{
	public abstract class OdinPropertyProcessor
	{
		public InspectorProperty Property { get; private set; }

		public abstract void ProcessMemberProperties(List<InspectorPropertyInfo> propertyInfos);

		public virtual bool CanProcessForProperty(InspectorProperty property)
		{
			return true;
		}

		protected virtual void Initialize()
		{
		}

		public static OdinPropertyProcessor Create(Type processorType, InspectorProperty property)
		{
			if (processorType == null)
			{
				throw new ArgumentNullException("processorType");
			}
			if (property == null)
			{
				throw new ArgumentNullException("property");
			}
			if (!typeof(OdinPropertyProcessor).IsAssignableFrom(processorType))
			{
				throw new ArgumentException("Type is not a MemberPropertyProcessor");
			}
			OdinPropertyProcessor result = (OdinPropertyProcessor)Activator.CreateInstance(processorType);
			result.Property = property;
			result.Initialize();
			return result;
		}

		public static T Create<T>(InspectorProperty property) where T : OdinPropertyProcessor, new()
		{
			if (property == null)
			{
				throw new ArgumentNullException("property");
			}
			T result = new T();
			result.Property = property;
			result.Initialize();
			return result;
		}
	}
	public abstract class OdinPropertyProcessor<TValue> : OdinPropertyProcessor
	{
		private IPropertyValueEntry<TValue> valueEntry;

		public IPropertyValueEntry<TValue> ValueEntry
		{
			get
			{
				if (valueEntry == null)
				{
					valueEntry = base.Property.TryGetTypedValueEntry<TValue>();
				}
				return valueEntry;
			}
		}
	}
	public abstract class OdinPropertyProcessor<TValue, TAttribute> : OdinPropertyProcessor<TValue> where TAttribute : Attribute
	{
	}
}
