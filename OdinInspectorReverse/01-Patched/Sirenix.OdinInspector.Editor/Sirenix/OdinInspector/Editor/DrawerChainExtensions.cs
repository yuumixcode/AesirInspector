using System;

namespace Sirenix.OdinInspector.Editor
{
	public static class DrawerChainExtensions
	{
		public static BakedDrawerChain Bake(this DrawerChain chain)
		{
			if (chain == null)
			{
				throw new ArgumentNullException("chain");
			}
			if (chain is BakedDrawerChain baked)
			{
				baked.Rebake();
				return baked;
			}
			return new BakedDrawerChain(chain);
		}
	}
}
