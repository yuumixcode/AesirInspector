using System;
using System.Collections.Generic;
using System.Reflection;
using Sirenix.Utilities.Editor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor
{
	public sealed class RectPropertyResolver : OdinPropertyResolver<Rect>
	{
		public override int ChildNameToIndex(string name)
		{
			StringSlice slice = name;
			return ChildNameToIndex(ref slice);
		}

		public override int ChildNameToIndex(ref StringSlice name)
		{
			if (name == "position")
			{
				return 0;
			}
			if (name == "size")
			{
				return 1;
			}
			return -1;
		}

		public override InspectorPropertyInfo GetChildInfo(int childIndex)
		{
			switch (childIndex)
			{
			case 0:
			{
				PropertyInfo member2 = typeof(Rect).GetProperty("position");
				List<Attribute> attributes2 = new List<Attribute>();
				return InspectorPropertyInfo.CreateForMember(base.Property, member2, allowEditable: true, attributes2);
			}
			case 1:
			{
				PropertyInfo member = typeof(Rect).GetProperty("size");
				List<Attribute> attributes = new List<Attribute>();
				return InspectorPropertyInfo.CreateForMember(base.Property, member, allowEditable: true, attributes);
			}
			default:
				throw new NotImplementedException();
			}
		}

		protected override int GetChildCount(Rect value)
		{
			return 2;
		}
	}
}
