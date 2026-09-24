using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector.Editor.Examples.Internal;
using Sirenix.Utilities;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Examples
{
	[ShowOdinSerializedPropertiesInInspector]
	[AttributeExample(typeof(TypeFilterAttribute), "The TypeFilter will instantiate the given type directly, It will also draw all child members in a foldout below the dropdown.")]
	[ExampleAsComponentData(Namespaces = new string[] { "System", "System.Linq", "System.Collections.Generic", "Sirenix.Utilities" })]
	internal class TypeFilterExamples
	{
		public abstract class BaseClass
		{
			public int BaseField;
		}

		public class A1 : BaseClass
		{
			public int _A1;
		}

		public class A2 : A1
		{
			public int _A2;
		}

		public class A3 : A2
		{
			public int _A3;
		}

		public class B1 : BaseClass
		{
			public int _B1;
		}

		public class B2 : B1
		{
			public int _B2;
		}

		public class B3 : B2
		{
			public int _B3;
		}

		public class C1<T> : BaseClass
		{
			public T C;
		}

		[TypeFilter("GetFilteredTypeList")]
		public BaseClass A;

		[TypeFilter("GetFilteredTypeList")]
		public BaseClass B;

		[TypeFilter("GetFilteredTypeList")]
		public BaseClass[] Array = new BaseClass[3];

		public IEnumerable<Type> GetFilteredTypeList()
		{
			IEnumerable<Type> q = from x in typeof(BaseClass).Assembly.GetTypes()
				where !x.IsAbstract
				where !x.IsGenericTypeDefinition
				where typeof(BaseClass).IsAssignableFrom(x)
				select x;
			q = q.AppendWith<Type>(typeof(C1<>).MakeGenericType(typeof(GameObject)));
			q = q.AppendWith<Type>(typeof(C1<>).MakeGenericType(typeof(AnimationCurve)));
			return q.AppendWith<Type>(typeof(C1<>).MakeGenericType(typeof(List<float>)));
		}
	}
}
