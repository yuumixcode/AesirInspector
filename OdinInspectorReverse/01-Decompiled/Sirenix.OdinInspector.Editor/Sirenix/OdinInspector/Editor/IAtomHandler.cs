using System;

namespace Sirenix.OdinInspector.Editor
{
	public interface IAtomHandler
	{
		Type AtomType { get; }

		object CreateInstance();

		void Copy(ref object from, ref object to);

		bool Compare(object a, object b);
	}
	public interface IAtomHandler<T> : IAtomHandler
	{
		new T CreateInstance();

		void Copy(ref T from, ref T to);

		bool Compare(T a, T b);
	}
}
