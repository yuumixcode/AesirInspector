using System;

namespace Sirenix.OdinInspector.Editor.ValueResolvers
{
	public struct NamedValue
	{
		public string Name;

		public Type Type;

		public object CurrentValue;

		public ValueResolverFunc<object> ValueGetter;

		public NamedValue(string name, Type type)
		{
			Name = name;
			Type = type;
			CurrentValue = null;
			ValueGetter = null;
		}

		public NamedValue(string name, Type type, object value)
		{
			Name = name;
			Type = type;
			CurrentValue = value;
			ValueGetter = null;
		}

		public NamedValue(string name, Type type, ValueResolverFunc<object> valueGetter)
		{
			Name = name;
			Type = type;
			CurrentValue = null;
			ValueGetter = valueGetter;
		}

		public void Update(ref ValueResolverContext context, int selectionIndex)
		{
			if (ValueGetter != null)
			{
				CurrentValue = ValueGetter(ref context, selectionIndex);
			}
		}
	}
}
