using System;
using System.Collections.Generic;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	internal class IValueDropdownEqualityComparer : IEqualityComparer<object>
	{
		private bool isTypeLookup;

		public IValueDropdownEqualityComparer(bool isTypeLookup)
		{
			this.isTypeLookup = isTypeLookup;
		}

		public new bool Equals(object x, object y)
		{
			if (x is ValueDropdownItem)
			{
				x = ((ValueDropdownItem)x).Value;
			}
			if (y is ValueDropdownItem)
			{
				y = ((ValueDropdownItem)y).Value;
			}
			if (EqualityComparer<object>.Default.Equals(x, y))
			{
				return true;
			}
			if (x == null != (y == null))
			{
				return false;
			}
			if (isTypeLookup)
			{
				Type tx = (x as Type) ?? x.GetType();
				Type ty = (y as Type) ?? y.GetType();
				if (tx == ty)
				{
					return true;
				}
			}
			return false;
		}

		public int GetHashCode(object obj)
		{
			if (obj == null)
			{
				return -1;
			}
			if (obj is ValueDropdownItem)
			{
				obj = ((ValueDropdownItem)obj).Value;
			}
			if (obj == null)
			{
				return -1;
			}
			if (isTypeLookup)
			{
				Type t = (obj as Type) ?? obj.GetType();
				return t.GetHashCode();
			}
			return obj.GetHashCode();
		}
	}
}
