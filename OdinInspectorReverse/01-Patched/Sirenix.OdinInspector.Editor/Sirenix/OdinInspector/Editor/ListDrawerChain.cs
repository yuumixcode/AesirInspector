using System.Collections.Generic;

namespace Sirenix.OdinInspector.Editor
{
	public class ListDrawerChain : DrawerChain
	{
		private int index = -1;

		private IList<OdinDrawer> list;

		public override OdinDrawer Current
		{
			get
			{
				if (index >= 0 && index < list.Count)
				{
					return list[index];
				}
				return null;
			}
		}

		public ListDrawerChain(InspectorProperty property, IList<OdinDrawer> list)
			: base(property)
		{
			this.list = list;
		}

		public override bool MoveNext()
		{
			index++;
			return Current != null;
		}

		public override void Reset()
		{
			index = -1;
		}
	}
}
