using System;
using System.Collections;
using System.Collections.Generic;

namespace Sirenix.OdinInspector.Editor
{
	public abstract class DrawerChain : IEnumerator<OdinDrawer>, IDisposable, IEnumerator, IEnumerable<OdinDrawer>, IEnumerable
	{
		public InspectorProperty Property { get; private set; }

		public abstract OdinDrawer Current { get; }

		object IEnumerator.Current => Current;

		public DrawerChain(InspectorProperty property)
		{
			if (property == null)
			{
				throw new ArgumentNullException("property");
			}
			Property = property;
		}

		public abstract bool MoveNext();

		public abstract void Reset();

		void IDisposable.Dispose()
		{
			Reset();
		}

		public virtual IEnumerator<OdinDrawer> GetEnumerator()
		{
			return this;
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}
