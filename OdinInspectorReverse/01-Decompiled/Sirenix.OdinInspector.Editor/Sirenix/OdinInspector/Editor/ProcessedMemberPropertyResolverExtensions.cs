using System;
using System.Collections.Generic;
using System.Reflection;
using Sirenix.Utilities;

namespace Sirenix.OdinInspector.Editor
{
	public static class ProcessedMemberPropertyResolverExtensions
	{
		public static Type ProcessingOwnerType { get; set; }

		public static void AddValue<TOwner, TValue>(this IList<InspectorPropertyInfo> infos, string name, ValueGetter<TOwner, TValue> getter, ValueSetter<TOwner, TValue> setter)
		{
			infos.AddValue(name, getter, setter, 0f, SerializationBackend.None, (Attribute[])null);
		}

		public static void AddValue<TOwner, TValue>(this IList<InspectorPropertyInfo> infos, string name, ValueGetter<TOwner, TValue> getter, ValueSetter<TOwner, TValue> setter, params Attribute[] attributes)
		{
			infos.AddValue(name, getter, setter, 0f, SerializationBackend.None, attributes);
		}

		public static void AddValue<TOwner, TValue>(this IList<InspectorPropertyInfo> infos, string name, ValueGetter<TOwner, TValue> getter, ValueSetter<TOwner, TValue> setter, float order = 0f, SerializationBackend backend = null)
		{
			infos.AddValue(name, getter, setter, 0f, SerializationBackend.None, (Attribute[])null);
		}

		public static void AddValue<TOwner, TValue>(this IList<InspectorPropertyInfo> infos, string name, ValueGetter<TOwner, TValue> getter, ValueSetter<TOwner, TValue> setter, float order = 0f, SerializationBackend backend = null, params Attribute[] attributes)
		{
			infos.Add(InspectorPropertyInfo.CreateValue(name, order, backend, new GetterSetter<TOwner, TValue>(getter, setter), attributes));
		}

		public static void AddValue<TValue>(this IList<InspectorPropertyInfo> infos, string name, Func<TValue> getter, Action<TValue> setter)
		{
			infos.AddValue(name, getter, setter, 0f, SerializationBackend.None, (Attribute[])null);
		}

		public static void AddValue<TValue>(this IList<InspectorPropertyInfo> infos, string name, Func<TValue> getter, Action<TValue> setter, params Attribute[] attributes)
		{
			infos.AddValue(name, getter, setter, 0f, SerializationBackend.None, attributes);
		}

		public static void AddValue<TValue>(this IList<InspectorPropertyInfo> infos, string name, Func<TValue> getter, Action<TValue> setter, float order = 0f, SerializationBackend backend = null)
		{
			infos.AddValue(name, getter, setter, 0f, SerializationBackend.None, (Attribute[])null);
		}

		public static void AddValue<TValue>(this IList<InspectorPropertyInfo> infos, string name, Func<TValue> getter, Action<TValue> setter, float order = 0f, SerializationBackend backend = null, params Attribute[] attributes)
		{
			infos.Add(InspectorPropertyInfo.CreateValue(name, order, backend, new GetterSetter<object, TValue>(getter, setter), attributes));
		}

		public static void AddDelegate(this IList<InspectorPropertyInfo> infos, string name, Action @delegate)
		{
			infos.AddDelegate(name, (Delegate)@delegate, 0f, (Attribute[])null);
		}

		public static void AddDelegate<T1>(this IList<InspectorPropertyInfo> infos, string name, Action<T1> @delegate)
		{
			infos.AddDelegate(name, (Delegate)@delegate, 0f, (Attribute[])null);
		}

		public static void AddDelegate<T1, T2>(this IList<InspectorPropertyInfo> infos, string name, Action<T1, T2> @delegate)
		{
			infos.AddDelegate(name, (Delegate)@delegate, 0f, (Attribute[])null);
		}

		public static void AddDelegate<T1, T2, T3>(this IList<InspectorPropertyInfo> infos, string name, Action<T1, T2, T3> @delegate)
		{
			infos.AddDelegate(name, (Delegate)@delegate, 0f, (Attribute[])null);
		}

		public static void AddDelegate<T1, T2, T3, T4>(this IList<InspectorPropertyInfo> infos, string name, Action<T1, T2, T3, T4> @delegate)
		{
			infos.AddDelegate(name, (Delegate)@delegate, 0f, (Attribute[])null);
		}

		public static void AddDelegate<TResult>(this IList<InspectorPropertyInfo> infos, string name, Func<TResult> @delegate)
		{
			infos.AddDelegate(name, (Delegate)@delegate, 0f, (Attribute[])null);
		}

		public static void AddDelegate<T1, TResult>(this IList<InspectorPropertyInfo> infos, string name, Func<T1, TResult> @delegate)
		{
			infos.AddDelegate(name, (Delegate)@delegate, 0f, (Attribute[])null);
		}

		public static void AddDelegate<T1, T2, TResult>(this IList<InspectorPropertyInfo> infos, string name, Func<T1, T2, TResult> @delegate)
		{
			infos.AddDelegate(name, (Delegate)@delegate, 0f, (Attribute[])null);
		}

		public static void AddDelegate<T1, T2, T3, TResult>(this IList<InspectorPropertyInfo> infos, string name, Func<T1, T2, T3, TResult> @delegate)
		{
			infos.AddDelegate(name, (Delegate)@delegate, 0f, (Attribute[])null);
		}

		public static void AddDelegate<T1, T2, T3, T4, TResult>(this IList<InspectorPropertyInfo> infos, string name, Func<T1, T2, T3, T4, TResult> @delegate)
		{
			infos.AddDelegate(name, (Delegate)@delegate, 0f, (Attribute[])null);
		}

		public static void AddDelegate(this IList<InspectorPropertyInfo> infos, string name, Action @delegate, params Attribute[] attributes)
		{
			infos.AddDelegate(name, (Delegate)@delegate, 0f, attributes);
		}

		public static void AddDelegate<T1>(this IList<InspectorPropertyInfo> infos, string name, Action<T1> @delegate, params Attribute[] attributes)
		{
			infos.AddDelegate(name, (Delegate)@delegate, 0f, attributes);
		}

		public static void AddDelegate<T1, T2>(this IList<InspectorPropertyInfo> infos, string name, Action<T1, T2> @delegate, params Attribute[] attributes)
		{
			infos.AddDelegate(name, (Delegate)@delegate, 0f, attributes);
		}

		public static void AddDelegate<T1, T2, T3>(this IList<InspectorPropertyInfo> infos, string name, Action<T1, T2, T3> @delegate, params Attribute[] attributes)
		{
			infos.AddDelegate(name, (Delegate)@delegate, 0f, attributes);
		}

		public static void AddDelegate<T1, T2, T3, T4>(this IList<InspectorPropertyInfo> infos, string name, Action<T1, T2, T3, T4> @delegate, params Attribute[] attributes)
		{
			infos.AddDelegate(name, (Delegate)@delegate, 0f, attributes);
		}

		public static void AddDelegate<TResult>(this IList<InspectorPropertyInfo> infos, string name, Func<TResult> @delegate, params Attribute[] attributes)
		{
			infos.AddDelegate(name, (Delegate)@delegate, 0f, attributes);
		}

		public static void AddDelegate<T1, TResult>(this IList<InspectorPropertyInfo> infos, string name, Func<T1, TResult> @delegate, params Attribute[] attributes)
		{
			infos.AddDelegate(name, (Delegate)@delegate, 0f, attributes);
		}

		public static void AddDelegate<T1, T2, TResult>(this IList<InspectorPropertyInfo> infos, string name, Func<T1, T2, TResult> @delegate, params Attribute[] attributes)
		{
			infos.AddDelegate(name, (Delegate)@delegate, 0f, attributes);
		}

		public static void AddDelegate<T1, T2, T3, TResult>(this IList<InspectorPropertyInfo> infos, string name, Func<T1, T2, T3, TResult> @delegate, params Attribute[] attributes)
		{
			infos.AddDelegate(name, (Delegate)@delegate, 0f, attributes);
		}

		public static void AddDelegate<T1, T2, T3, T4, TResult>(this IList<InspectorPropertyInfo> infos, string name, Func<T1, T2, T3, T4, TResult> @delegate, params Attribute[] attributes)
		{
			infos.AddDelegate(name, (Delegate)@delegate, 0f, attributes);
		}

		public static void AddDelegate(this IList<InspectorPropertyInfo> infos, string name, Action @delegate, float order = 0f, params Attribute[] attributes)
		{
			infos.AddDelegate(name, (Delegate)@delegate, order, attributes);
		}

		public static void AddDelegate<T1>(this IList<InspectorPropertyInfo> infos, string name, Action<T1> @delegate, float order = 0f, params Attribute[] attributes)
		{
			infos.AddDelegate(name, (Delegate)@delegate, order, attributes);
		}

		public static void AddDelegate<T1, T2>(this IList<InspectorPropertyInfo> infos, string name, Action<T1, T2> @delegate, float order = 0f, params Attribute[] attributes)
		{
			infos.AddDelegate(name, (Delegate)@delegate, order, attributes);
		}

		public static void AddDelegate<T1, T2, T3>(this IList<InspectorPropertyInfo> infos, string name, Action<T1, T2, T3> @delegate, float order = 0f, params Attribute[] attributes)
		{
			infos.AddDelegate(name, (Delegate)@delegate, order, attributes);
		}

		public static void AddDelegate<T1, T2, T3, T4>(this IList<InspectorPropertyInfo> infos, string name, Action<T1, T2, T3, T4> @delegate, float order = 0f, params Attribute[] attributes)
		{
			infos.AddDelegate(name, (Delegate)@delegate, order, attributes);
		}

		public static void AddDelegate<TResult>(this IList<InspectorPropertyInfo> infos, string name, Func<TResult> @delegate, float order = 0f, params Attribute[] attributes)
		{
			infos.AddDelegate(name, (Delegate)@delegate, order, attributes);
		}

		public static void AddDelegate<T1, TResult>(this IList<InspectorPropertyInfo> infos, string name, Func<T1, TResult> @delegate, float order = 0f, params Attribute[] attributes)
		{
			infos.AddDelegate(name, (Delegate)@delegate, order, attributes);
		}

		public static void AddDelegate<T1, T2, TResult>(this IList<InspectorPropertyInfo> infos, string name, Func<T1, T2, TResult> @delegate, float order = 0f, params Attribute[] attributes)
		{
			infos.AddDelegate(name, (Delegate)@delegate, order, attributes);
		}

		public static void AddDelegate<T1, T2, T3, TResult>(this IList<InspectorPropertyInfo> infos, string name, Func<T1, T2, T3, TResult> @delegate, float order = 0f, params Attribute[] attributes)
		{
			infos.AddDelegate(name, (Delegate)@delegate, order, attributes);
		}

		public static void AddDelegate<T1, T2, T3, T4, TResult>(this IList<InspectorPropertyInfo> infos, string name, Func<T1, T2, T3, T4, TResult> @delegate, float order = 0f, params Attribute[] attributes)
		{
			infos.AddDelegate(name, (Delegate)@delegate, order, attributes);
		}

		public static void AddDelegate(this IList<InspectorPropertyInfo> infos, string name, Delegate @delegate, float order = 0f, params Attribute[] attributes)
		{
			infos.Add(InspectorPropertyInfo.CreateForDelegate(name, order, typeof(object), @delegate, attributes));
		}

		public static void AddMember(this IList<InspectorPropertyInfo> infos, MemberInfo member)
		{
			infos.AddMember(member, allowEditable: true, SerializationBackend.None, (Attribute[])null);
		}

		public static void AddMember(this IList<InspectorPropertyInfo> infos, MemberInfo member, params Attribute[] attributes)
		{
			infos.AddMember(member, allowEditable: true, SerializationBackend.None, attributes);
		}

		public static void AddMember(this IList<InspectorPropertyInfo> infos, MemberInfo member, bool allowEditable = true, SerializationBackend backend = null, params Attribute[] attributes)
		{
			infos.Add(InspectorPropertyInfo.CreateForMember(member, allowEditable, backend, attributes ?? new Attribute[0]));
		}

		public static void AddProcessedMember(this IList<InspectorPropertyInfo> infos, InspectorProperty parentProperty, MemberInfo member)
		{
			infos.AddProcessedMember(parentProperty, member, allowEditable: true, SerializationBackend.None, (Attribute[])null);
		}

		public static void AddProcessedMember(this IList<InspectorPropertyInfo> infos, InspectorProperty parentProperty, MemberInfo member, params Attribute[] attributes)
		{
			infos.AddProcessedMember(parentProperty, member, allowEditable: true, SerializationBackend.None, attributes);
		}

		public static void AddProcessedMember(this IList<InspectorPropertyInfo> infos, InspectorProperty parentProperty, MemberInfo member, bool allowEditable = true, SerializationBackend backend = null, params Attribute[] attributes)
		{
			List<Attribute> list = new List<Attribute>();
			if (attributes != null)
			{
				list.AddRange(attributes);
			}
			InspectorPropertyInfoUtility.ProcessAttributes(parentProperty, member, list);
			infos.Add(InspectorPropertyInfo.CreateForMember(member, allowEditable, backend, list));
		}

		public static void AddMember(this IList<InspectorPropertyInfo> infos, string name)
		{
			infos.AddMember(name, allowEditable: true, SerializationBackend.None, (Attribute[])null);
		}

		public static void AddMember(this IList<InspectorPropertyInfo> infos, string name, params Attribute[] attributes)
		{
			infos.AddMember(name, allowEditable: true, SerializationBackend.None, attributes);
		}

		public static void AddMember(this IList<InspectorPropertyInfo> infos, string name, bool allowEditable = true, SerializationBackend backend = null, params Attribute[] attributes)
		{
			MemberInfo[] members = ProcessingOwnerType.GetMember(name, MemberTypes.Field | MemberTypes.Method | MemberTypes.Property, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
			if (members.Length == 0 || members.Length > 1)
			{
				throw new ArgumentException("Could not find precisely 1 member on type '" + ProcessingOwnerType.GetNiceName() + "' with name '" + name + "'; found " + members.Length + " members.");
			}
			infos.AddMember(members[0], allowEditable: true, SerializationBackend.None, attributes ?? new Attribute[0]);
		}

		public static void AddProcessedMember(this IList<InspectorPropertyInfo> infos, InspectorProperty parentProperty, string name)
		{
			infos.AddProcessedMember(parentProperty, name, allowEditable: true, SerializationBackend.None, (Attribute[])null);
		}

		public static void AddProcessedMember(this IList<InspectorPropertyInfo> infos, InspectorProperty parentProperty, string name, params Attribute[] attributes)
		{
			infos.AddProcessedMember(parentProperty, name, allowEditable: true, SerializationBackend.None, attributes);
		}

		public static void AddProcessedMember(this IList<InspectorPropertyInfo> infos, InspectorProperty parentProperty, string name, bool allowEditable = true, SerializationBackend backend = null, params Attribute[] attributes)
		{
			Type type = ProcessingOwnerType;
			type = ((parentProperty.ValueEntry != null) ? parentProperty.ValueEntry.TypeOfValue : ProcessingOwnerType);
			MemberInfo[] members = type.GetMember(name, MemberTypes.Field | MemberTypes.Method | MemberTypes.Property, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
			if (members.Length == 0 || members.Length > 1)
			{
				throw new ArgumentException("Could not find precisely 1 member on type '" + type.GetNiceName() + "' with name '" + name + "'; found " + members.Length + " members.");
			}
			List<Attribute> list = new List<Attribute>();
			if (attributes != null)
			{
				list.AddRange(attributes);
			}
			InspectorPropertyInfoUtility.ProcessAttributes(parentProperty, members[0], list);
			infos.Add(InspectorPropertyInfo.CreateForMember(members[0], allowEditable, backend, list));
		}

		public static bool Remove(this IList<InspectorPropertyInfo> infos, string name)
		{
			for (int i = 0; i < infos.Count; i++)
			{
				if (infos[i].PropertyName == name)
				{
					infos.RemoveAt(i);
					return true;
				}
			}
			return false;
		}

		public static InspectorPropertyInfo Find(this IList<InspectorPropertyInfo> infos, string name)
		{
			for (int i = 0; i < infos.Count; i++)
			{
				if (infos[i].PropertyName == name)
				{
					return infos[i];
				}
			}
			return null;
		}
	}
}
