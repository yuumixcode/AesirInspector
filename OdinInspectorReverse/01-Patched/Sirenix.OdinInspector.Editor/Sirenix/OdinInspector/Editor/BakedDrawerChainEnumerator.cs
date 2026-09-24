using System;
using System.Collections;
using System.Collections.Generic;

namespace Sirenix.OdinInspector.Editor
{
	public struct BakedDrawerChainEnumerator : IEnumerator<OdinDrawer>, IDisposable, IEnumerator
	{
		private BakedDrawerChain chain;

		private int index;

		private OdinDrawer current;

		public OdinDrawer Current => current;

		object IEnumerator.Current => current;

		public BakedDrawerChainEnumerator(BakedDrawerChain chain)
		{
			this.chain = chain;
			index = -1;
			current = null;
		}

		public void Dispose()
		{
		}

		public bool MoveNext()
		{
			if (index + 1 < chain.BakedDrawerArray.Length)
			{
				index++;
				current = chain.BakedDrawerArray[index];
				return true;
			}
			current = null;
			return false;
		}

		public void Reset()
		{
			index = -1;
		}
	}
}
