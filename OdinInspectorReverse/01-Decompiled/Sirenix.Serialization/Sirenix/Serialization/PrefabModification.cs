using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Sirenix.Serialization.Utilities;
using UnityEngine;

namespace Sirenix.Serialization
{
	/// <summary>
	/// An Odin-serialized prefab modification, containing all the information necessary to apply the modification.
	/// </summary>
	public sealed class PrefabModification
	{
		/// <summary>
		/// The type of modification to be made.
		/// </summary>
		public PrefabModificationType ModificationType;

		/// <summary>
		/// The deep reflection path at which to make the modification.
		/// </summary>
		public string Path;

		/// <summary>
		/// A list of all deep reflection paths in the target object where the value referenced by this modification was also located.
		/// </summary>
		public List<string> ReferencePaths;

		/// <summary>
		/// The modified value to set.
		/// </summary>
		public object ModifiedValue;

		/// <summary>
		/// The new list length to set.
		/// </summary>
		public int NewLength;

		/// <summary>
		/// The dictionary keys to add.
		/// </summary>
		public object[] DictionaryKeysAdded;

		/// <summary>
		/// The dictionary keys to remove.
		/// </summary>
		public object[] DictionaryKeysRemoved;

		/// <summary>
		/// Applies the modification to the given Object.
		/// </summary>
		public void Apply(UnityEngine.Object unityObject)
		{
			if (ModificationType == PrefabModificationType.Value)
			{
				ApplyValue(unityObject);
				return;
			}
			if (ModificationType == PrefabModificationType.ListLength)
			{
				ApplyListLength(unityObject);
				return;
			}
			if (ModificationType == PrefabModificationType.Dictionary)
			{
				ApplyDictionaryModifications(unityObject);
				return;
			}
			throw new NotImplementedException(ModificationType.ToString());
		}

		private void ApplyValue(UnityEngine.Object unityObject)
		{
			Type valueType = null;
			if (ModifiedValue != null)
			{
				valueType = ModifiedValue.GetType();
			}
			if (valueType != null && ReferencePaths != null && ReferencePaths.Count > 0)
			{
				for (int i = 0; i < ReferencePaths.Count; i++)
				{
					string path = ReferencePaths[i];
					try
					{
						object refValue = GetInstanceFromPath(path, unityObject);
						if (refValue != null && refValue.GetType() == valueType)
						{
							ModifiedValue = refValue;
							break;
						}
					}
					catch (Exception)
					{
					}
				}
			}
			SetInstanceToPath(Path, unityObject, ModifiedValue);
		}

		private void ApplyListLength(UnityEngine.Object unityObject)
		{
			object listObj = GetInstanceFromPath(Path, unityObject);
			if (listObj == null)
			{
				return;
			}
			Type listType = listObj.GetType();
			if (listType.IsArray)
			{
				Array array = (Array)listObj;
				if (NewLength != array.Length)
				{
					Array newArray = Array.CreateInstance(listType.GetElementType(), NewLength);
					if (NewLength > array.Length)
					{
						Array.Copy(array, 0, newArray, 0, array.Length);
						ReplaceAllReferencesInGraph(unityObject, array, newArray);
					}
					else
					{
						Array.Copy(array, 0, newArray, 0, newArray.Length);
						ReplaceAllReferencesInGraph(unityObject, array, newArray);
					}
				}
			}
			else if (typeof(IList).IsAssignableFrom(listType))
			{
				IList list = (IList)listObj;
				Type listElementType = (listType.ImplementsOpenGenericInterface(typeof(IList<>)) ? listType.GetArgumentsOfInheritedOpenGenericInterface(typeof(IList<>))[0] : null);
				bool elementIsValueType = listElementType != null && listElementType.IsValueType;
				int count = 0;
				while (list.Count < NewLength)
				{
					if (elementIsValueType)
					{
						list.Add(Activator.CreateInstance(listElementType));
					}
					else
					{
						list.Add(null);
					}
					count++;
				}
				while (list.Count > NewLength)
				{
					list.RemoveAt(list.Count - 1);
				}
			}
			else
			{
				if (!listType.ImplementsOpenGenericInterface(typeof(IList<>)))
				{
					return;
				}
				Type elementType = listType.GetArgumentsOfInheritedOpenGenericInterface(typeof(IList<>))[0];
				Type collectionType = typeof(ICollection<>).MakeGenericType(elementType);
				bool elementIsValueType2 = elementType.IsValueType;
				PropertyInfo countProp = collectionType.GetProperty("Count");
				int count2 = (int)countProp.GetValue(listObj, null);
				if (count2 < NewLength)
				{
					int add = NewLength - count2;
					MethodInfo addMethod = collectionType.GetMethod("Add");
					for (int i = 0; i < add; i++)
					{
						if (elementIsValueType2)
						{
							addMethod.Invoke(listObj, new object[1] { Activator.CreateInstance(elementType) });
						}
						else
						{
							addMethod.Invoke(listObj, new object[1]);
						}
						count2++;
					}
				}
				else if (count2 > NewLength)
				{
					int remove = count2 - NewLength;
					Type listInterfaceType = typeof(IList<>).MakeGenericType(elementType);
					MethodInfo removeAtMethod = listInterfaceType.GetMethod("RemoveAt");
					for (int j = 0; j < remove; j++)
					{
						removeAtMethod.Invoke(listObj, new object[1] { count2 - (remove + 1) });
					}
				}
			}
		}

		private void ApplyDictionaryModifications(UnityEngine.Object unityObject)
		{
			object dictionaryObj = GetInstanceFromPath(Path, unityObject);
			if (dictionaryObj == null)
			{
				return;
			}
			Type type = dictionaryObj.GetType();
			if (!type.ImplementsOpenGenericInterface(typeof(IDictionary<, >)))
			{
				return;
			}
			Type[] typeArgs = type.GetArgumentsOfInheritedOpenGenericInterface(typeof(IDictionary<, >));
			Type iType = typeof(IDictionary<, >).MakeGenericType(typeArgs);
			if (DictionaryKeysRemoved != null && DictionaryKeysRemoved.Length != 0)
			{
				MethodInfo method = iType.GetMethod("Remove", new Type[1] { typeArgs[0] });
				object[] parameters = new object[1];
				for (int i = 0; i < DictionaryKeysRemoved.Length; i++)
				{
					parameters[0] = DictionaryKeysRemoved[i];
					if (parameters[0] != null && typeArgs[0].IsAssignableFrom(parameters[0].GetType()))
					{
						method.Invoke(dictionaryObj, parameters);
					}
				}
			}
			if (DictionaryKeysAdded == null || DictionaryKeysAdded.Length == 0)
			{
				return;
			}
			MethodInfo method2 = iType.GetMethod("set_Item", typeArgs);
			object[] parameters2 = new object[2]
			{
				null,
				typeArgs[1].IsValueType ? Activator.CreateInstance(typeArgs[1]) : null
			};
			for (int j = 0; j < DictionaryKeysAdded.Length; j++)
			{
				parameters2[0] = DictionaryKeysAdded[j];
				if (parameters2[0] != null && typeArgs[0].IsAssignableFrom(parameters2[0].GetType()))
				{
					method2.Invoke(dictionaryObj, parameters2);
				}
			}
		}

		private static void ReplaceAllReferencesInGraph(object graph, object oldReference, object newReference, HashSet<object> processedReferences = null)
		{
			if (processedReferences == null)
			{
				processedReferences = new HashSet<object>(ReferenceEqualityComparer<object>.Default);
			}
			processedReferences.Add(graph);
			if (graph.GetType().IsArray)
			{
				Array array = (Array)graph;
				for (int i = 0; i < array.Length; i++)
				{
					object value = array.GetValue(i);
					if (value != null)
					{
						if (value == oldReference)
						{
							array.SetValue(newReference, i);
							value = newReference;
						}
						if (!processedReferences.Contains(value))
						{
							ReplaceAllReferencesInGraph(value, oldReference, newReference, processedReferences);
						}
					}
				}
				return;
			}
			MemberInfo[] members = FormatterUtilities.GetSerializableMembers(graph.GetType(), SerializationPolicies.Everything);
			for (int j = 0; j < members.Length; j++)
			{
				FieldInfo field = (FieldInfo)members[j];
				if (field.FieldType.IsPrimitive || field.FieldType == typeof(SerializationData) || field.FieldType == typeof(string))
				{
					continue;
				}
				object value2 = field.GetValue(graph);
				if (value2 == null)
				{
					continue;
				}
				Type valueType = value2.GetType();
				if (!valueType.IsPrimitive && !(valueType == typeof(SerializationData)) && !(valueType == typeof(string)))
				{
					if (value2 == oldReference)
					{
						field.SetValue(graph, newReference);
						value2 = newReference;
					}
					if (!processedReferences.Contains(value2))
					{
						ReplaceAllReferencesInGraph(value2, oldReference, newReference, processedReferences);
					}
				}
			}
		}

		private static object GetInstanceFromPath(string path, object instance)
		{
			string[] steps = path.Split(new char[1] { '.' });
			object currentInstance = instance;
			for (int i = 0; i < steps.Length; i++)
			{
				currentInstance = GetInstanceOfStep(steps[i], currentInstance);
				if (currentInstance == null)
				{
					return null;
				}
			}
			return currentInstance;
		}

		private static object GetInstanceOfStep(string step, object instance)
		{
			Type type = instance.GetType();
			if (step.StartsWith("[", StringComparison.InvariantCulture) && step.EndsWith("]", StringComparison.InvariantCulture))
			{
				string indexStr = step.Substring(1, step.Length - 2);
				if (!int.TryParse(indexStr, out var index))
				{
					throw new ArgumentException("Couldn't parse an index from the path step '" + step + "'.");
				}
				if (type.IsArray)
				{
					Array array = (Array)instance;
					if (index < 0 || index >= array.Length)
					{
						return null;
					}
					return array.GetValue(index);
				}
				if (typeof(IList).IsAssignableFrom(type))
				{
					IList list = (IList)instance;
					if (index < 0 || index >= list.Count)
					{
						return null;
					}
					return list[index];
				}
				if (type.ImplementsOpenGenericInterface(typeof(IList<>)))
				{
					Type elementType = type.GetArgumentsOfInheritedOpenGenericInterface(typeof(IList<>))[0];
					Type listType = typeof(IList<>).MakeGenericType(elementType);
					MethodInfo getItemMethod = listType.GetMethod("get_Item", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
					try
					{
						return getItemMethod.Invoke(instance, new object[1] { index });
					}
					catch (Exception)
					{
						return null;
					}
				}
			}
			else if (step.StartsWith("{", StringComparison.InvariantCultureIgnoreCase) && step.EndsWith("}", StringComparison.InvariantCultureIgnoreCase))
			{
				if (type.ImplementsOpenGenericInterface(typeof(IDictionary<, >)))
				{
					Type[] dictArgs = type.GetArgumentsOfInheritedOpenGenericInterface(typeof(IDictionary<, >));
					object key = DictionaryKeyUtility.GetDictionaryKeyValue(step, dictArgs[0]);
					Type dictType = typeof(IDictionary<, >).MakeGenericType(dictArgs);
					MethodInfo getItemMethod2 = dictType.GetMethod("get_Item", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
					try
					{
						return getItemMethod2.Invoke(instance, new object[1] { key });
					}
					catch (Exception)
					{
						return null;
					}
				}
			}
			else
			{
				string privateTypeName = null;
				int plusIndex = step.IndexOf('+');
				if (plusIndex >= 0)
				{
					privateTypeName = step.Substring(0, plusIndex);
					step = step.Substring(plusIndex + 1);
				}
				IEnumerable<MemberInfo> possibleMembers = from n in type.GetAllMembers(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
					where n is FieldInfo || n is PropertyInfo
					select n;
				foreach (MemberInfo member in possibleMembers)
				{
					if (member.Name == step && (privateTypeName == null || !(member.DeclaringType.Name != privateTypeName)))
					{
						return member.GetMemberValue(instance);
					}
				}
			}
			return null;
		}

		private static void SetInstanceToPath(string path, object instance, object value)
		{
			string[] steps = path.Split(new char[1] { '.' });
			SetInstanceToPath(path, steps, 0, instance, value, out var _);
		}

		private static void SetInstanceToPath(string path, string[] steps, int index, object instance, object value, out bool setParentInstance)
		{
			setParentInstance = false;
			if (index < steps.Length - 1)
			{
				object currentInstance = GetInstanceOfStep(steps[index], instance);
				if (currentInstance != null)
				{
					SetInstanceToPath(path, steps, index + 1, currentInstance, value, out setParentInstance);
					if (setParentInstance)
					{
						TrySetInstanceOfStep(steps[index], instance, currentInstance, out setParentInstance);
					}
				}
			}
			else
			{
				TrySetInstanceOfStep(steps[index], instance, value, out setParentInstance);
			}
		}

		private static bool TrySetInstanceOfStep(string step, object instance, object value, out bool setParentInstance)
		{
			setParentInstance = false;
			try
			{
				Type type = instance.GetType();
				if (step.StartsWith("[", StringComparison.InvariantCulture) && step.EndsWith("]", StringComparison.InvariantCulture))
				{
					string indexStr = step.Substring(1, step.Length - 2);
					if (!int.TryParse(indexStr, out var index))
					{
						throw new ArgumentException("Couldn't parse an index from the path step '" + step + "'.");
					}
					if (type.IsArray)
					{
						Array array = (Array)instance;
						if (index < 0 || index >= array.Length)
						{
							return false;
						}
						array.SetValue(value, index);
						return true;
					}
					if (typeof(IList).IsAssignableFrom(type))
					{
						IList list = (IList)instance;
						if (index < 0 || index >= list.Count)
						{
							return false;
						}
						list[index] = value;
						return true;
					}
					if (type.ImplementsOpenGenericInterface(typeof(IList<>)))
					{
						Type elementType = type.GetArgumentsOfInheritedOpenGenericInterface(typeof(IList<>))[0];
						Type listType = typeof(IList<>).MakeGenericType(elementType);
						MethodInfo setItemMethod = listType.GetMethod("set_Item", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
						setItemMethod.Invoke(instance, new object[2] { index, value });
						return true;
					}
				}
				else if (step.StartsWith("{", StringComparison.InvariantCulture) && step.EndsWith("}", StringComparison.InvariantCulture))
				{
					if (type.ImplementsOpenGenericInterface(typeof(IDictionary<, >)))
					{
						Type[] dictArgs = type.GetArgumentsOfInheritedOpenGenericInterface(typeof(IDictionary<, >));
						object key = DictionaryKeyUtility.GetDictionaryKeyValue(step, dictArgs[0]);
						Type dictType = typeof(IDictionary<, >).MakeGenericType(dictArgs);
						MethodInfo containsKeyMethod = dictType.GetMethod("ContainsKey", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
						MethodInfo setItemMethod2 = dictType.GetMethod("set_Item", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
						if (!(bool)containsKeyMethod.Invoke(instance, new object[1] { key }))
						{
							return false;
						}
						setItemMethod2.Invoke(instance, new object[2] { key, value });
					}
				}
				else
				{
					string privateTypeName = null;
					int plusIndex = step.IndexOf('+');
					if (plusIndex >= 0)
					{
						privateTypeName = step.Substring(0, plusIndex);
						step = step.Substring(plusIndex + 1);
					}
					IEnumerable<MemberInfo> possibleMembers = from n in type.GetAllMembers(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
						where n is FieldInfo || n is PropertyInfo
						select n;
					foreach (MemberInfo member in possibleMembers)
					{
						if (member.Name == step && (privateTypeName == null || !(member.DeclaringType.Name != privateTypeName)))
						{
							member.SetMemberValue(instance, value);
							if (instance.GetType().IsValueType)
							{
								setParentInstance = true;
							}
							return true;
						}
					}
				}
				return false;
			}
			catch (Exception)
			{
				return false;
			}
		}
	}
}
