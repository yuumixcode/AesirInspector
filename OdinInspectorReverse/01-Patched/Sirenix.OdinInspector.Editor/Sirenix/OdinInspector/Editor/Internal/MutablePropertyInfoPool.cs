using System.Collections.Generic;

namespace Sirenix.OdinInspector.Editor.Internal
{
	internal static class MutablePropertyInfoPool
	{
		public static Stack<MutablePropertyInfo> AvailableGroups = new Stack<MutablePropertyInfo>(16);

		public static Stack<MutablePropertyInfo> AvailableProperties = new Stack<MutablePropertyInfo>(32);

		public static MutablePropertyInfo Rent(InspectorPropertyInfo info)
		{
			MutablePropertyInfo result = ((info.PropertyType == PropertyType.Group) ? ((AvailableGroups.Count <= 0) ? new MutablePropertyInfo
			{
				Children = new List<MutablePropertyInfo>()
			} : AvailableGroups.Pop()) : ((AvailableProperties.Count <= 0) ? new MutablePropertyInfo() : AvailableProperties.Pop()));
			result.Initialize(info);
			return result;
		}

		public static void Return(MutablePropertyInfo mutableInfo)
		{
			if (mutableInfo.IsGroup)
			{
				for (int i = 0; i < mutableInfo.Children.Count; i++)
				{
					MutablePropertyInfo current = mutableInfo.Children[i];
					Return(current);
				}
				mutableInfo.Children.Clear();
				AvailableGroups.Push(mutableInfo);
			}
			else
			{
				AvailableProperties.Push(mutableInfo);
			}
		}
	}
}
