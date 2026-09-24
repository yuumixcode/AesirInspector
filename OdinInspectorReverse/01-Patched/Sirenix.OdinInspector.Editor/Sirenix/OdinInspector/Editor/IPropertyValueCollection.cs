using System.Collections;
using System.Collections.Generic;
using Sirenix.Utilities;

namespace Sirenix.OdinInspector.Editor
{
	/// <summary>
	/// Represents a weakly typed collection of values for a <see cref="T:Sirenix.OdinInspector.Editor.PropertyValueEntry" /> - one value per selected inspector target.
	/// </summary>
	public interface IPropertyValueCollection : IList, ICollection, IEnumerable
	{
		/// <summary>
		/// Whether the values have been changed since <see cref="M:Sirenix.OdinInspector.Editor.IPropertyValueCollection.MarkClean" /> was last called.
		/// </summary>
		bool AreDirty { get; }

		/// <summary>
		/// The original values of the value collection, such as they were immediately after the last <see cref="M:Sirenix.OdinInspector.Editor.PropertyValueEntry.Update" /> call.
		/// </summary>
		IImmutableList Original { get; }

		/// <summary>
		/// Marks the value collection as being clean again. This is typically called at the end of the current GUI frame, during <see cref="M:Sirenix.OdinInspector.Editor.PropertyValueEntry.ApplyChanges" />.
		/// </summary>
		void MarkClean();

		/// <summary>
		/// Marks the value collection as being dirty, regardless of any value changes.
		/// </summary>
		void ForceMarkDirty();

		/// <summary>
		/// Reverts the value collection to its origin values (found in <see cref="P:Sirenix.OdinInspector.Editor.IPropertyValueCollection.Original" />) from the last <see cref="M:Sirenix.OdinInspector.Editor.PropertyValueEntry.Update" /> call, and marks the value collection as being clean again.
		/// </summary>
		void RevertUnappliedValues();

		/// <summary>
		/// <para>Force sets the value, ignoring whether it is editable or not.</para>
		/// <para>Note that this will fail on list element value entries where <see cref="!:IPropertyValueEntry.ListIsReadOnly" /> is true on the parent value entry.</para>
		/// </summary>
		/// <param name="index">The selection index of the value.</param>
		/// <param name="value">The value to be set.</param>
		void ForceSetValue(int index, object value);
	}
	/// <summary>
	/// Represents a strongly typed collection of values for a <see cref="T:Sirenix.OdinInspector.Editor.PropertyValueEntry`1" /> - one value per selected inspector target.
	/// </summary>
	public interface IPropertyValueCollection<T> : IPropertyValueCollection, IList, ICollection, IEnumerable, IList<T>, ICollection<T>, IEnumerable<T>
	{
		/// <summary>
		/// Gets the value at the given selection index.
		/// </summary>
		new T this[int index] { get; set; }

		/// <summary>
		/// The number of values in the collection.
		/// </summary>
		new int Count { get; }

		/// <summary>
		/// The original values of the value collection, such as they were immediately after the last <see cref="M:Sirenix.OdinInspector.Editor.PropertyValueEntry.Update" /> call.
		/// </summary>
		new IImmutableList<T> Original { get; }

		/// <summary>
		/// <para>Force sets the value, ignoring whether it is editable or not.</para>
		/// <para>Note that this will fail on list element value entries where <see cref="!:IPropertyValueEntry.ListIsReadOnly" /> is true on the parent value entry.</para>
		/// </summary>
		/// <param name="index">The selection index of the value.</param>
		/// <param name="value">The value to be set.</param>
		void ForceSetValue(int index, T value);
	}
}
