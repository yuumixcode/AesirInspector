using System.Collections.Generic;
using System.Linq;

namespace Sirenix.OdinInspector.Editor
{
	public class BakedDrawerChain : DrawerChain
	{
		private OdinDrawer[] bakedDrawerChain;

		private int index = -1;

		public OdinDrawer[] BakedDrawerArray => bakedDrawerChain;

		public DrawerChain BakedChain { get; private set; }

		public int CurrentIndex => index;

		public override OdinDrawer Current
		{
			get
			{
				if (index >= 0 && index < bakedDrawerChain.Length)
				{
					return bakedDrawerChain[index];
				}
				return null;
			}
		}

		public BakedDrawerChain(InspectorProperty property, IEnumerable<OdinDrawer> chain)
			: base(property)
		{
			bakedDrawerChain = chain.ToArray();
		}

		public BakedDrawerChain(DrawerChain bakedChain)
			: base(bakedChain.Property)
		{
			BakedChain = bakedChain;
			Rebake();
		}

		public override bool MoveNext()
		{
			do
			{
				index++;
				if (Current != null)
				{
					base.Property.IncrementDrawerChainIndex();
				}
			}
			while (Current != null && Current.SkipWhenDrawing);
			return Current != null;
		}

		public override IEnumerator<OdinDrawer> GetEnumerator()
		{
			return new BakedDrawerChainEnumerator(this);
		}

		public override void Reset()
		{
			index = -1;
		}

		public void Rebake()
		{
			if (BakedChain != null)
			{
				BakedChain.Reset();
				bakedDrawerChain = BakedChain.ToArray();
			}
		}
	}
}
