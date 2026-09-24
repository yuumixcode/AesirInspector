using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Sirenix.Config
{
	[Serializable]
	[HideReferenceObjectPicker]
	[InlineProperty]
	[HideLabel]
	public abstract class StringSerializedCollection<T, TElement> where T : ICollection<TElement>, new()
	{
		[NonSerialized]
		[ShowInInspector]
		[LabelText("@$property.Parent.NiceName")]
		public T Collection = new T();

		[SerializeField]
		[HideInInspector]
		public List<string> serializedCollection = new List<string>();
	}
}
