using System;
using System.Collections.Generic;
using System.Reflection;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor
{
	public class ButtonParameterPropertyResolver : OdinPropertyResolver
	{
		private class GetterSetter<T> : IValueGetterSetter<object, T>, IValueGetterSetter
		{
			private readonly object[] parameterValues;

			private readonly int index;

			public bool IsReadonly => false;

			public Type OwnerType => typeof(object);

			public Type ValueType => typeof(T);

			public GetterSetter(object[] parameterValues, int index)
			{
				this.parameterValues = parameterValues;
				this.index = index;
			}

			public T GetValue(ref object owner)
			{
				object value = parameterValues[index];
				if (value == null)
				{
					return default(T);
				}
				try
				{
					return (T)value;
				}
				catch
				{
					return default(T);
				}
			}

			public object GetValue(object owner)
			{
				return parameterValues[index];
			}

			public void SetValue(ref object owner, T value)
			{
				parameterValues[index] = value;
			}

			public void SetValue(object owner, object value)
			{
				parameterValues[index] = value;
			}
		}

		public const string RETURN_VALUE_NAME = "$Result";

		private Dictionary<int, InspectorPropertyInfo> childInfos = new Dictionary<int, InspectorPropertyInfo>();

		private Dictionary<StringSlice, int> indexNameLookup = new Dictionary<StringSlice, int>(StringSliceEqualityComparer.Instance);

		private MethodInfo methodInfo;

		private ParameterInfo[] parameters;

		private object[] parameterValues;

		private object returnedValue;

		private Type returnType;

		public override bool CanResolveForPropertyFilter(InspectorProperty property)
		{
			if (property.Info.PropertyType != PropertyType.Method)
			{
				return false;
			}
			MethodInfo info = property.Info.GetMemberInfo() as MethodInfo;
			if (info == null)
			{
				info = property.Info.GetMethodDelegate().Method;
			}
			if (info.IsGenericMethodDefinition)
			{
				return false;
			}
			return true;
		}

		protected override void Initialize()
		{
			methodInfo = (base.Property.Info.GetMemberInfo() as MethodInfo).DeAliasMethod();
			if (methodInfo == null)
			{
				methodInfo = base.Property.Info.GetMethodDelegate().Method.DeAliasMethod();
			}
			returnType = methodInfo.ReturnType;
			if (returnType == typeof(void))
			{
				returnType = null;
			}
			if (returnType == null)
			{
				parameters = methodInfo.GetParameters();
				parameterValues = new object[parameters.Length];
			}
			else
			{
				ParameterInfo[] temp = methodInfo.GetParameters();
				parameters = new ParameterInfo[temp.Length + 1];
				parameterValues = new object[parameters.Length];
				for (int i = 0; i < temp.Length; i++)
				{
					parameters[i] = temp[i];
				}
				parameters[parameters.Length - 1] = methodInfo.ReturnParameter;
			}
			for (int j = 0; j < parameters.Length; j++)
			{
				string name = ((returnType != null && j == parameters.Length - 1) ? "$Result" : parameters[j].Name);
				indexNameLookup[name] = j;
				object val = parameters[j].DefaultValue;
				if (val != DBNull.Value)
				{
					parameterValues[j] = val;
				}
			}
		}

		public override int ChildNameToIndex(string name)
		{
			if (indexNameLookup.TryGetValue(name, out var index))
			{
				return index;
			}
			return -1;
		}

		public override int ChildNameToIndex(ref StringSlice name)
		{
			if (indexNameLookup.TryGetValue(name, out var index))
			{
				return index;
			}
			return -1;
		}

		public override InspectorPropertyInfo GetChildInfo(int childIndex)
		{
			if (childInfos.TryGetValue(childIndex, out var info))
			{
				return info;
			}
			ParameterInfo parameter = parameters[childIndex];
			Type type = parameter.ParameterType;
			if (type.IsByRef)
			{
				type = type.GetElementType();
			}
			Type getterSetterType = null;
			IValueGetterSetter getterSetter = null;
			try
			{
				getterSetterType = typeof(GetterSetter<>).MakeGenericType(type);
				getterSetter = Activator.CreateInstance(getterSetterType, parameterValues, childIndex) as IValueGetterSetter;
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
			info = InspectorPropertyInfo.CreateValue((returnType != null && childIndex == parameters.Length - 1) ? "$Result" : parameter.Name, childIndex, SerializationBackend.None, getterSetter, parameter.GetAttributes());
			childInfos[childIndex] = info;
			return info;
		}

		protected override int CalculateChildCount()
		{
			return parameterValues.Length;
		}
	}
}
