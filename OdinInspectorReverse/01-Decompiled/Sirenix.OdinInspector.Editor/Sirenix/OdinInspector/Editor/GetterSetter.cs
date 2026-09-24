using System;
using System.Reflection;
using Sirenix.Utilities;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor
{
	/// <summary>
	/// Responsible for getting and setting values on properties.
	/// </summary>
	/// <typeparam name="TOwner">The type of the owner.</typeparam>
	/// <typeparam name="TValue">The type of the value.</typeparam>
	/// <seealso cref="T:Sirenix.OdinInspector.Editor.IValueGetterSetter`2" />
	public class GetterSetter<TOwner, TValue> : IValueGetterSetter<TOwner, TValue>, IValueGetterSetter
	{
		private ValueGetter<TOwner, TValue> getter;

		private ValueSetter<TOwner, TValue> setter;

		private Func<TValue> staticGetter;

		private Action<TValue> staticSetter;

		/// <summary>
		/// Whether the value is readonly.
		/// </summary>
		public bool IsReadonly
		{
			get
			{
				if (setter == null)
				{
					return staticSetter == null;
				}
				return false;
			}
		}

		/// <summary>
		/// Gets the type of the owner.
		/// </summary>
		public Type OwnerType => typeof(TOwner);

		/// <summary>
		/// Gets the type of the value.
		/// </summary>
		public Type ValueType => typeof(TValue);

		/// <summary>
		/// Initializes a new instance of the <see cref="T:Sirenix.OdinInspector.Editor.GetterSetter`2" /> class.
		/// </summary>
		/// <param name="memberInfo">The field member to represent.</param>
		/// <param name="isReadOnly">if set to <c>true</c> [is readonly].</param>
		public GetterSetter(MemberInfo memberInfo, bool isReadOnly)
		{
			if (memberInfo == null)
			{
				throw new ArgumentNullException("memberInfo");
			}
			if (memberInfo.IsStatic())
			{
				staticGetter = GetCachedStaticGetter(memberInfo);
				if (!isReadOnly)
				{
					staticSetter = GetCachedStaticSetter(memberInfo);
				}
			}
			else
			{
				getter = GetCachedGetter(memberInfo);
				if (!isReadOnly)
				{
					setter = GetCachedSetter(memberInfo);
				}
			}
		}

		private static ValueGetter<TOwner, TValue> GetCachedGetter(MemberInfo member)
		{
			if (GetterSetterCaches<TOwner>.Getters.TryGetInnerValue(member, typeof(TValue), out var del))
			{
				return (ValueGetter<TOwner, TValue>)del;
			}
			FieldInfo fieldInfo = member as FieldInfo;
			PropertyInfo propertyInfo = member as PropertyInfo;
			ValueGetter<TOwner, TValue> result;
			if (fieldInfo != null)
			{
				result = EmitUtilities.CreateInstanceFieldGetter<TOwner, TValue>(fieldInfo);
			}
			else
			{
				if (!(propertyInfo != null))
				{
					throw new ArgumentException("Cannot create a GetterSetter for a member of type + " + member.GetType().Name + "!");
				}
				result = EmitUtilities.CreateInstancePropertyGetter<TOwner, TValue>(propertyInfo);
			}
			GetterSetterCaches<TOwner>.Getters.AddInner(member, typeof(TValue), result);
			return result;
		}

		private static ValueSetter<TOwner, TValue> GetCachedSetter(MemberInfo member)
		{
			if (GetterSetterCaches<TOwner>.Setters.TryGetInnerValue(member, typeof(TValue), out var del))
			{
				return (ValueSetter<TOwner, TValue>)del;
			}
			ValueSetter<TOwner, TValue> result = null;
			FieldInfo fieldInfo = member as FieldInfo;
			PropertyInfo propertyInfo = member as PropertyInfo;
			if (fieldInfo != null)
			{
				if (!fieldInfo.IsLiteral)
				{
					result = EmitUtilities.CreateInstanceFieldSetter<TOwner, TValue>(fieldInfo);
				}
			}
			else
			{
				if (!(propertyInfo != null))
				{
					throw new ArgumentException("Cannot create a GetterSetter for a member of type + " + member.GetType().Name + "!");
				}
				if (propertyInfo.CanWrite)
				{
					result = EmitUtilities.CreateInstancePropertySetter<TOwner, TValue>(propertyInfo);
				}
			}
			GetterSetterCaches<TOwner>.Setters.AddInner(member, typeof(TValue), result);
			return result;
		}

		private static Func<TValue> GetCachedStaticGetter(MemberInfo member)
		{
			if (GetterSetterCaches<TOwner>.Getters.TryGetInnerValue(member, typeof(TValue), out var del))
			{
				return (Func<TValue>)del;
			}
			FieldInfo fieldInfo = member as FieldInfo;
			PropertyInfo propertyInfo = member as PropertyInfo;
			Func<TValue> result;
			if (fieldInfo != null)
			{
				result = EmitUtilities.CreateStaticFieldGetter<TValue>(fieldInfo);
			}
			else
			{
				if (!(propertyInfo != null))
				{
					throw new ArgumentException("Cannot create a GetterSetter for a member of type + " + member.GetType().Name + "!");
				}
				result = EmitUtilities.CreateStaticPropertyGetter<TValue>(propertyInfo);
			}
			GetterSetterCaches<TOwner>.Getters.AddInner(member, typeof(TValue), result);
			return result;
		}

		private static Action<TValue> GetCachedStaticSetter(MemberInfo member)
		{
			if (GetterSetterCaches<TOwner>.Setters.TryGetInnerValue(member, typeof(TValue), out var del))
			{
				return (Action<TValue>)del;
			}
			Action<TValue> result = null;
			FieldInfo fieldInfo = member as FieldInfo;
			PropertyInfo propertyInfo = member as PropertyInfo;
			if (fieldInfo != null)
			{
				if (!fieldInfo.IsLiteral)
				{
					result = EmitUtilities.CreateStaticFieldSetter<TValue>(fieldInfo);
				}
			}
			else
			{
				if (!(propertyInfo != null))
				{
					throw new ArgumentException("Cannot create a GetterSetter for a member of type + " + member.GetType().Name + "!");
				}
				if (propertyInfo.CanWrite)
				{
					result = EmitUtilities.CreateStaticPropertySetter<TValue>(propertyInfo);
				}
			}
			GetterSetterCaches<TOwner>.Setters.AddInner(member, typeof(TValue), result);
			return result;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:Sirenix.OdinInspector.Editor.GetterSetter`2" /> class.
		/// </summary>
		/// <param name="getter">The getter.</param>
		/// <param name="setter">The setter.</param>
		/// <exception cref="T:System.ArgumentNullException">getter</exception>
		public GetterSetter(ValueGetter<TOwner, TValue> getter, ValueSetter<TOwner, TValue> setter)
		{
			if (getter == null)
			{
				throw new ArgumentNullException("getter");
			}
			this.getter = getter;
			this.setter = setter;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:Sirenix.OdinInspector.Editor.GetterSetter`2" /> class.
		/// </summary>
		/// <param name="getter">The getter.</param>
		/// <param name="setter">The setter.</param>
		/// <exception cref="T:System.ArgumentNullException">getter</exception>
		public GetterSetter(Func<TValue> getter, Action<TValue> setter)
		{
			if (getter == null)
			{
				throw new ArgumentNullException("getter");
			}
			this.getter = delegate
			{
				return getter();
			};
			if (setter != null)
			{
				this.setter = delegate(ref TOwner owner, TValue value)
				{
					setter(value);
				};
			}
		}

		internal GetterSetter()
		{
			getter = delegate
			{
				return default(TValue);
			};
			setter = null;
		}

		/// <summary>
		/// Gets the value from a given owner.
		/// </summary>
		/// <param name="owner">The owner.</param>
		/// <returns>The found value.</returns>
		/// <exception cref="T:System.ArgumentNullException">owner is null</exception>
		public TValue GetValue(ref TOwner owner)
		{
			if (getter == null)
			{
				return staticGetter();
			}
			return getter(ref owner);
		}

		/// <summary>
		/// Gets the value from a given weakly typed owner.
		/// </summary>
		/// <param name="owner">The weakly typed owner.</param>
		/// <returns>The found value.</returns>
		public object GetValue(object owner)
		{
			TOwner castOwner = (TOwner)owner;
			return GetValue(ref castOwner);
		}

		/// <summary>
		/// Sets the weakly typed value on a given weakly typed owner.
		/// </summary>
		/// <param name="owner">The owner.</param>
		/// <param name="value">The value.</param>
		public void SetValue(ref TOwner owner, TValue value)
		{
			if (IsReadonly)
			{
				Debug.LogError("Tried to set a value on a readonly getter setter!");
			}
			else if (setter != null)
			{
				setter(ref owner, value);
			}
			else if (staticSetter != null)
			{
				staticSetter(value);
			}
			else
			{
				Debug.Log("WTF TOR!?");
			}
		}

		/// <summary>
		/// Sets the value on a given owner.
		/// </summary>
		/// <param name="owner">The owner.</param>
		/// <param name="value">The value.</param>
		public void SetValue(object owner, object value)
		{
			TOwner castOwner = (TOwner)owner;
			SetValue(ref castOwner, (TValue)value);
		}
	}
}
