using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Sirenix.Utilities;

namespace Sirenix.OdinInspector.Editor.Examples
{
	public static class AttributeExampleUtilities
	{
		private class CategoryComparer : IComparer<OdinMenuItem>
		{
			private static readonly Dictionary<string, int> Order = new Dictionary<string, int>
			{
				{ "Essentials", -10 },
				{ "Misc", 8 },
				{ "Meta", 9 },
				{ "Unity", 10 },
				{ "Debug", 50 }
			};

			public int Compare(OdinMenuItem x, OdinMenuItem y)
			{
				if (!Order.TryGetValue(x.Name, out var xOrder))
				{
					xOrder = 0;
				}
				if (!Order.TryGetValue(y.Name, out var yOrder))
				{
					yOrder = 0;
				}
				if (xOrder == yOrder)
				{
					return x.Name.CompareTo(y.Name);
				}
				return xOrder.CompareTo(yOrder);
			}
		}

		private static readonly CategoryComparer CategorySorter;

		private static readonly Type[] AttributeTypes;

		private static readonly Dictionary<Type, OdinRegisterAttributeAttribute> AttributeRegisterMap;

		static AttributeExampleUtilities()
		{
			CategorySorter = new CategoryComparer();
			AttributeRegisterMap = (from OdinRegisterAttributeAttribute attr in AssemblyUtilities.GetAllAssemblies().SelectMany((Assembly a) => a.GetAttributes<OdinRegisterAttributeAttribute>(inherit: true)).Concat(InternalAttributeRegistry.Attributes)
				where OdinInspectorVersion.IsEnterprise || !attr.IsEnterprise
				select attr).ToDictionary((OdinRegisterAttributeAttribute x) => x.AttributeType);
			AttributeTypes = AttributeRegisterMap.Keys.ToArray();
		}

		public static IEnumerable<Type> GetAllOdinAttributes()
		{
			return AttributeTypes;
		}

		public static IEnumerable<string> GetAttributeCategories(Type attributeType)
		{
			if (attributeType == null)
			{
				throw new ArgumentNullException("attributeType");
			}
			if (!AttributeRegisterMap.TryGetValue(attributeType, out var registration) || registration.Categories == null)
			{
				return new string[1] { "Uncategorized" };
			}
			return from x in registration.Categories.Split(new char[1] { ',' })
				select x.Trim();
		}

		public static string GetAttributeDescription(Type attributeType)
		{
			if (attributeType == null)
			{
				throw new ArgumentNullException("attributeType");
			}
			if (AttributeRegisterMap.TryGetValue(attributeType, out var registration))
			{
				return registration.Description;
			}
			return null;
		}

		public static string GetOnlineDocumentationUrl(Type attributeType)
		{
			if (attributeType == null)
			{
				throw new ArgumentNullException("attributeType");
			}
			if (AttributeRegisterMap.TryGetValue(attributeType, out var registration))
			{
				return registration.DocumentationUrl;
			}
			return null;
		}

		public static bool GetIsEnterprise(Type attributeType)
		{
			if (attributeType == null)
			{
				throw new ArgumentNullException("attributeType");
			}
			if (AttributeRegisterMap.TryGetValue(attributeType, out var registration))
			{
				return registration.IsEnterprise;
			}
			return false;
		}

		public static void BuildMenuTree(OdinMenuTree tree)
		{
			foreach (Type a in GetAllOdinAttributes())
			{
				string search = a.Name + " " + string.Join(" ", (from x in GetAttributeExampleInfos(a)
					select x.Name).ToArray());
				foreach (string c in GetAttributeCategories(a))
				{
					OdinMenuItem item = new OdinMenuItem(tree, a.GetNiceName().Replace("Attribute", "").SplitPascalCase(), a)
					{
						Value = a,
						SearchString = search
					};
					search = null;
					tree.AddMenuItemAtPath(c, item);
				}
			}
			tree.MenuItems.Sort(CategorySorter);
			tree.MarkDirty();
		}

		public static AttributeExampleInfo[] GetAttributeExampleInfos(Type attributeType)
		{
			if (attributeType == null)
			{
				throw new ArgumentNullException("attributeType");
			}
			if (!InternalAttributeExampleInfoMap.Map.TryGetValue(attributeType, out var examples))
			{
				return new AttributeExampleInfo[0];
			}
			return examples;
		}

		public static OdinAttributeExampleItem GetExample<T>() where T : Attribute
		{
			return GetExample(typeof(T));
		}

		public static OdinAttributeExampleItem GetExample(Type attributeType)
		{
			AttributeRegisterMap.TryGetValue(attributeType, out var registration);
			return new OdinAttributeExampleItem(attributeType, registration);
		}

		internal static bool HasExample(Type attributeType)
		{
			return AttributeRegisterMap.ContainsKey(attributeType);
		}
	}
}
