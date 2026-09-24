using System.Collections.Generic;
using Sirenix.Serialization.Utilities;
using UnityEngine;

namespace Sirenix.Serialization
{
	/// <summary>
	/// Resolves external index references to Unity objects.
	/// </summary>
	/// <seealso cref="T:Sirenix.Serialization.IExternalIndexReferenceResolver" />
	/// <seealso cref="T:Sirenix.Serialization.Utilities.ICacheNotificationReceiver" />
	public sealed class UnityReferenceResolver : IExternalIndexReferenceResolver, ICacheNotificationReceiver
	{
		private Dictionary<Object, int> referenceIndexMapping = new Dictionary<Object, int>(32, ReferenceEqualityComparer<Object>.Default);

		private List<Object> referencedUnityObjects;

		/// <summary>
		/// Initializes a new instance of the <see cref="T:Sirenix.Serialization.UnityReferenceResolver" /> class.
		/// </summary>
		public UnityReferenceResolver()
		{
			referencedUnityObjects = new List<Object>();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:Sirenix.Serialization.UnityReferenceResolver" /> class with a list of Unity objects.
		/// </summary>
		/// <param name="referencedUnityObjects">The referenced Unity objects.</param>
		public UnityReferenceResolver(List<Object> referencedUnityObjects)
		{
			SetReferencedUnityObjects(referencedUnityObjects);
		}

		/// <summary>
		/// Gets the currently referenced Unity objects.
		/// </summary>
		/// <returns>A list of the currently referenced Unity objects.</returns>
		public List<Object> GetReferencedUnityObjects()
		{
			return referencedUnityObjects;
		}

		/// <summary>
		/// Sets the referenced Unity objects of the resolver to a given list, or a new list if the value is null.
		/// </summary>
		/// <param name="referencedUnityObjects">The referenced Unity objects to set, or null if a new list is required.</param>
		public void SetReferencedUnityObjects(List<Object> referencedUnityObjects)
		{
			if (referencedUnityObjects == null)
			{
				referencedUnityObjects = new List<Object>();
			}
			this.referencedUnityObjects = referencedUnityObjects;
			referenceIndexMapping.Clear();
			for (int i = 0; i < this.referencedUnityObjects.Count; i++)
			{
				if ((object)this.referencedUnityObjects[i] != null && !referenceIndexMapping.ContainsKey(this.referencedUnityObjects[i]))
				{
					referenceIndexMapping.Add(this.referencedUnityObjects[i], i);
				}
			}
		}

		/// <summary>
		/// Determines whether the specified value can be referenced externally via this resolver.
		/// </summary>
		/// <param name="value">The value to reference.</param>
		/// <param name="index">The index of the resolved value, if it can be referenced.</param>
		/// <returns>
		///   <c>true</c> if the reference can be resolved, otherwise <c>false</c>.
		/// </returns>
		public bool CanReference(object value, out int index)
		{
			if (referencedUnityObjects == null)
			{
				referencedUnityObjects = new List<Object>(32);
			}
			if (value is Object obj)
			{
				if (!referenceIndexMapping.TryGetValue(obj, out index))
				{
					index = referencedUnityObjects.Count;
					referenceIndexMapping.Add(obj, index);
					referencedUnityObjects.Add(obj);
				}
				return true;
			}
			index = -1;
			return false;
		}

		/// <summary>
		/// Tries to resolve the given reference index to a reference value.
		/// </summary>
		/// <param name="index">The index to resolve.</param>
		/// <param name="value">The resolved value.</param>
		/// <returns>
		///   <c>true</c> if the index could be resolved to a value, otherwise <c>false</c>.
		/// </returns>
		public bool TryResolveReference(int index, out object value)
		{
			if (referencedUnityObjects == null || index < 0 || index >= referencedUnityObjects.Count)
			{
				value = null;
				return true;
			}
			value = referencedUnityObjects[index];
			return true;
		}

		/// <summary>
		/// Resets this instance.
		/// </summary>
		public void Reset()
		{
			referencedUnityObjects = null;
			referenceIndexMapping.Clear();
		}

		void ICacheNotificationReceiver.OnFreed()
		{
			Reset();
		}

		void ICacheNotificationReceiver.OnClaimed()
		{
		}
	}
}
