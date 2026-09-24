using System.Collections.Generic;

namespace Sirenix.OdinInspector.Editor
{
	public abstract class EnumeratedDrawerChain : DrawerChain
	{
		private IEnumerator<OdinDrawer> enumerator;

		public override OdinDrawer Current
		{
			get
			{
				if (enumerator == null)
				{
					return null;
				}
				return enumerator.Current;
			}
		}

		public EnumeratedDrawerChain(InspectorProperty property)
			: base(property)
		{
		}

		public override bool MoveNext()
		{
			if (enumerator == null)
			{
				enumerator = GetEnumeratorInstance();
			}
			return enumerator.MoveNext();
		}

		public override void Reset()
		{
			if (enumerator != null)
			{
				enumerator.Dispose();
				enumerator = null;
			}
		}

		protected abstract IEnumerator<OdinDrawer> GetEnumeratorInstance();
	}
}
