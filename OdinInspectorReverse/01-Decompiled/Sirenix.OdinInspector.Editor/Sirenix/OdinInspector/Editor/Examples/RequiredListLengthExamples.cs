using System.Collections.Generic;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Examples
{
	[AttributeExample(typeof(RequiredListLengthAttribute), "An attribute that can be applied to lists, arrays and other types of collections to ensure they contain a specified number of elements.")]
	internal class RequiredListLengthExamples
	{
		[RequiredListLength(10)]
		public int[] fixedLength;

		[RequiredListLength(1, null)]
		public int[] minLength;

		[RequiredListLength(null, 10, PrefabKind = PrefabKind.InstanceInScene)]
		public List<int> maxLength;

		[RequiredListLength(3, 10)]
		public List<int> minAndMaxLength;

		public int SomeNumber;

		[RequiredListLength("@this.SomeNumber")]
		public List<GameObject> matchLengthOfOther;

		[RequiredListLength("@this.SomeNumber", null)]
		public int[] minLengthExpression;

		[RequiredListLength(null, "@this.SomeNumber")]
		public List<int> maxLengthExpression;
	}
}
