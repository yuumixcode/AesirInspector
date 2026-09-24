using System;
using System.Collections.Generic;

namespace Sirenix.OdinInspector.Editor.Validation
{
	internal class ResultItemMetaDataDrawerPropertyResolver : BaseMemberPropertyResolver<ResultItemMetaDataDrawer>
	{
		protected override InspectorPropertyInfo[] GetPropertyInfos()
		{
			ResultItemMetaDataDrawer value = base.ValueEntry.SmartValue;
			ResultItemMetaData[] array = value.MetaData;
			List<InspectorPropertyInfo> result = new List<InspectorPropertyInfo>(array.Length);
			bool hasExcludedFirstButton = false;
			for (int i = 0; i < array.Length; i++)
			{
				ResultItemMetaData item = array[i];
				if (item.Value is Delegate d)
				{
					if (item.Value is Action && value.ExcludeFirstButton && !hasExcludedFirstButton)
					{
						hasExcludedFirstButton = true;
						continue;
					}
					result.Add(InspectorPropertyInfo.CreateForDelegate("generated_" + item.Name + i, i, typeof(ResultItemMetaDataDrawer), d, Combine(item.Attributes, new ButtonAttribute(item.Name)
					{
						Expanded = true
					})));
				}
				else
				{
					result.Add(InspectorPropertyInfo.CreateValue("generated_" + item.Name + i, i, SerializationBackend.None, MakeGetterSetter(i), Combine(item.Attributes, new ReadOnlyAttribute(), string.IsNullOrEmpty(item.Name) ? ((Attribute)new HideLabelAttribute()) : ((Attribute)new LabelTextAttribute(item.Name)), new EnableGUIAttribute(), new HideReferenceObjectPickerAttribute())));
				}
			}
			return result.ToArray();
		}

		private IValueGetterSetter MakeGetterSetter(int i)
		{
			return new GetterSetter<ResultItemMetaDataDrawer, object>(delegate(ref ResultItemMetaDataDrawer parent)
			{
				return parent.MetaData[i].Value;
			}, null);
		}

		private static Attribute[] Combine(Attribute[] a, params Attribute[] b)
		{
			if (a != null && a.Length != 0)
			{
				Attribute[] combined = new Attribute[a.Length + b.Length];
				for (int i = 0; i < b.Length; i++)
				{
					combined[i] = b[i];
				}
				for (int j = 0; j < a.Length; j++)
				{
					combined[j + b.Length] = a[j];
				}
				return combined;
			}
			return b;
		}
	}
}
