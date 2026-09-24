using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector.Editor.Internal;
using Sirenix.Serialization;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor
{
	/// <summary>
	/// <para>Handles all prefab modifications that apply to the targets of a property tree, if any. This class determines which properties have modifications, what the modifications are, auto-applies modifications if the current instance values do not correspond to the prefab values, and also provides an API for modifying those modifications.</para>
	/// <para>NOTE: This class is liable to see a lot of changes, as the prefab modification system is slated to be redesigned for increased extendability in the future. Do not depend overly on the current API.</para>
	/// </summary>
	public sealed class PrefabModificationHandler : IDisposable
	{
		private readonly bool targetSupportsPrefabSerialization;

		private ImmutableList<UnityEngine.Object> immutableTargetPrefabs;

		private bool hasPrefabs;

		private bool allTargetsHaveSamePrefab;

		private Dictionary<string, PrefabModification>[] prefabValueModifications;

		private Dictionary<string, PrefabModification>[] prefabListLengthModifications;

		private Dictionary<string, PrefabModification>[] prefabDictionaryModifications;

		private PropertyTree prefabPropertyTree;

		private int[] prefabPropertyTreeIndexMap;

		private bool allowAutoRegisterPrefabModifications = true;

		private bool needsToRebuildPropertyModificationLookup = true;

		private PathLookup<PropertyModification>[] unityModLookups;

		public PropertyTree Tree { get; private set; }

		/// <summary>
		/// The prefabs for each prefab instance represented by the property tree, if any.
		/// </summary>
		public ImmutableList<UnityEngine.Object> TargetPrefabs => immutableTargetPrefabs;

		/// <summary>
		/// Whether any of the values the property tree represents are prefab instances.
		/// </summary>
		public bool HasPrefabs => hasPrefabs;

		/// <summary>
		/// A prefab tree for the prefabs of this property tree's prefab instances, if any exist.
		/// </summary>
		public PropertyTree PrefabPropertyTree => prefabPropertyTree;

		public bool HasNestedOdinPrefabData { get; private set; }

		public PrefabModificationHandler(PropertyTree tree)
		{
			Tree = tree;
			prefabValueModifications = new Dictionary<string, PrefabModification>[tree.WeakTargets.Count];
			prefabListLengthModifications = new Dictionary<string, PrefabModification>[tree.WeakTargets.Count];
			prefabDictionaryModifications = new Dictionary<string, PrefabModification>[tree.WeakTargets.Count];
			prefabPropertyTreeIndexMap = new int[tree.WeakTargets.Count];
			unityModLookups = new PathLookup<PropertyModification>[tree.WeakTargets.Count];
			Type targetType = tree.TargetType;
			targetSupportsPrefabSerialization = !tree.IsStatic && typeof(UnityEngine.Object).IsAssignableFrom(targetType) && typeof(ISupportsPrefabSerialization).IsAssignableFrom(targetType);
		}

		public void Update()
		{
			hasPrefabs = false;
			HasNestedOdinPrefabData = false;
			if (Tree.IsStatic)
			{
				return;
			}
			UnityEngine.Object[] prefabs = new UnityEngine.Object[Tree.WeakTargets.Count];
			if (typeof(UnityEngine.Object).IsAssignableFrom(Tree.TargetType))
			{
				int prefabCount = 0;
				for (int i = 0; i < Tree.WeakTargets.Count; i++)
				{
					UnityEngine.Object target = (UnityEngine.Object)Tree.WeakTargets[i];
					bool isPrefab = false;
					UnityEngine.Object prefab = null;
					if (target != null)
					{
						prefab = PrefabUtility.GetCorrespondingObjectFromSource(target);
						isPrefab = prefab != null;
						if (!HasNestedOdinPrefabData && OdinPrefabSerializationEditorUtility.ObjectHasNestedOdinPrefabData(target))
						{
							HasNestedOdinPrefabData = true;
						}
					}
					if (isPrefab)
					{
						prefabs[i] = prefab;
						hasPrefabs = true;
						prefabPropertyTreeIndexMap[i] = prefabCount;
						prefabCount++;
						if (!targetSupportsPrefabSerialization)
						{
							continue;
						}
						ISupportsPrefabSerialization cast = (ISupportsPrefabSerialization)Tree.WeakTargets[i];
						List<PrefabModification> modificationList = UnitySerializationUtility.PrefabModificationCache.DeserializePrefabModificationsCached(target, cast.SerializationData.PrefabModifications, cast.SerializationData.PrefabModificationsReferencedUnityObjects);
						Dictionary<string, PrefabModification> listLengthModifications = prefabListLengthModifications[i] ?? new Dictionary<string, PrefabModification>();
						Dictionary<string, PrefabModification> valueModifications = prefabValueModifications[i] ?? new Dictionary<string, PrefabModification>();
						Dictionary<string, PrefabModification> dictionaryModifications = prefabDictionaryModifications[i] ?? new Dictionary<string, PrefabModification>();
						listLengthModifications.Clear();
						valueModifications.Clear();
						dictionaryModifications.Clear();
						for (int j = 0; j < modificationList.Count; j++)
						{
							PrefabModification mod = modificationList[j];
							switch (mod.ModificationType)
							{
							case PrefabModificationType.Value:
								if (!valueModifications.ContainsKey(mod.Path))
								{
									valueModifications[mod.Path] = mod;
								}
								break;
							case PrefabModificationType.ListLength:
								if (!listLengthModifications.ContainsKey(mod.Path))
								{
									listLengthModifications[mod.Path] = mod;
								}
								break;
							case PrefabModificationType.Dictionary:
								if (!dictionaryModifications.ContainsKey(mod.Path))
								{
									dictionaryModifications[mod.Path] = mod;
								}
								break;
							default:
								throw new NotImplementedException(mod.ModificationType.ToString());
							}
						}
						List<PrefabModification> registeredModifications = UnitySerializationUtility.GetRegisteredPrefabModifications(target);
						if (registeredModifications != null)
						{
							for (int k = 0; k < registeredModifications.Count; k++)
							{
								PrefabModification mod2 = registeredModifications[k];
								if (mod2.ModificationType == PrefabModificationType.Value)
								{
									valueModifications[mod2.Path] = mod2;
								}
								else if (mod2.ModificationType == PrefabModificationType.ListLength)
								{
									listLengthModifications[mod2.Path] = mod2;
								}
								else if (mod2.ModificationType == PrefabModificationType.Dictionary)
								{
									dictionaryModifications[mod2.Path] = mod2;
								}
							}
						}
						prefabListLengthModifications[i] = listLengthModifications;
						prefabValueModifications[i] = valueModifications;
						prefabDictionaryModifications[i] = dictionaryModifications;
					}
					else
					{
						prefabPropertyTreeIndexMap[i] = -1;
					}
				}
				if (prefabCount > 0)
				{
					UnityEngine.Object[] prefabsNoNull = new UnityEngine.Object[prefabCount];
					for (int l = 0; l < prefabs.Length; l++)
					{
						int index = prefabPropertyTreeIndexMap[l];
						if (index >= 0)
						{
							prefabsNoNull[index] = prefabs[l];
						}
					}
					if (prefabPropertyTree != null)
					{
						if (prefabPropertyTree.WeakTargets.Count != prefabsNoNull.Length)
						{
							prefabPropertyTree.Dispose();
							object[] targets = prefabsNoNull;
							prefabPropertyTree = PropertyTree.Create(targets);
						}
						else
						{
							for (int m = 0; m < prefabPropertyTree.WeakTargets.Count; m++)
							{
								if (prefabPropertyTree.WeakTargets[m] != prefabsNoNull[m])
								{
									PropertyTree propertyTree = prefabPropertyTree;
									object[] targets = prefabsNoNull;
									propertyTree.SetTargets(targets);
									break;
								}
							}
						}
					}
					else
					{
						object[] targets = prefabsNoNull;
						prefabPropertyTree = PropertyTree.Create(targets);
					}
					prefabPropertyTree.UpdateTree();
				}
				allTargetsHaveSamePrefab = false;
				if (prefabCount == Tree.WeakTargets.Count)
				{
					allTargetsHaveSamePrefab = true;
					UnityEngine.Object firstPrefab = prefabs[0];
					for (int n = 1; n < prefabs.Length; n++)
					{
						if ((object)firstPrefab != prefabs[n])
						{
							allTargetsHaveSamePrefab = false;
							break;
						}
					}
				}
			}
			immutableTargetPrefabs = new ImmutableList<UnityEngine.Object>(prefabs.Cast<UnityEngine.Object>().ToArray());
			needsToRebuildPropertyModificationLookup = true;
		}

		private void RebuildUnityPropertyModificationsLookup()
		{
			if (HasPrefabs && !Application.isPlaying)
			{
				for (int i = 0; i < Tree.WeakTargets.Count; i++)
				{
					UnityEngine.Object targetPrefab = TargetPrefabs[i];
					if (!(targetPrefab != null))
					{
						continue;
					}
					UnityEngine.Object target = (UnityEngine.Object)Tree.WeakTargets[i];
					bool targetIsPrefab = PrefabUtility.GetCorrespondingObjectFromSource(target) != null;
					PropertyModification[] mods = PrefabUtility.GetPropertyModifications(target);
					if (mods == null || mods.Length == 0)
					{
						continue;
					}
					PathLookup<PropertyModification> lookup = unityModLookups[i];
					if (lookup == null)
					{
						lookup = new PathLookup<PropertyModification>();
						unityModLookups[i] = lookup;
					}
					lookup.BeginRebuild();
					try
					{
						foreach (PropertyModification mod in mods)
						{
							if (!(mod.target != targetPrefab))
							{
								lookup.AddValue(mod.propertyPath, mod);
							}
						}
					}
					finally
					{
						lookup.FinishRebuild();
					}
				}
			}
			needsToRebuildPropertyModificationLookup = false;
		}

		public void CleanForCachedReuse()
		{
			if (prefabPropertyTree != null)
			{
				prefabPropertyTree.CleanForCachedReuse();
			}
		}

		public void Dispose()
		{
			if (prefabPropertyTree != null)
			{
				prefabPropertyTree.Dispose();
				prefabPropertyTree = null;
			}
		}

		private static bool FastStringEndsWith(string str, string endsWith)
		{
			int strLength = str.Length;
			int endsWithLength = endsWith.Length;
			if (strLength < endsWithLength)
			{
				return false;
			}
			if (strLength == endsWithLength)
			{
				return str == endsWith;
			}
			for (int i = 1; i <= endsWithLength; i++)
			{
				if (str[strLength - i] != endsWith[endsWithLength - i])
				{
					return false;
				}
			}
			return true;
		}

		private bool TargetHasRegisteredModificationsWaitingForApply()
		{
			for (int i = 0; i < Tree.WeakTargets.Count; i++)
			{
				UnityEngine.Object value = (UnityEngine.Object)Tree.WeakTargets[i];
				if (UnitySerializationUtility.HasModificationsWaitingForDelayedApply(value))
				{
					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// Gets the Unity PropertyModification for the property at this path, if there are any.
		/// </summary>
		/// <param name="path">The property path to get the modification for.</param>
		/// <param name="selectionIndex">The index of the tree target to get the modification for.</param>
		/// <param name="childrenHaveModifications">Whether any children of the path have modifications registered.</param>
		/// <returns></returns>
		public PropertyModification GetUnityPropertyModification(StringSlice path, int selectionIndex, out bool childrenHaveModifications)
		{
			if (needsToRebuildPropertyModificationLookup)
			{
				RebuildUnityPropertyModificationsLookup();
			}
			childrenHaveModifications = false;
			PathLookup<PropertyModification> lookup = unityModLookups[selectionIndex];
			if (lookup == null)
			{
				return null;
			}
			if (lookup.TryGetValue(path, out var _, out childrenHaveModifications, out var mod))
			{
				return mod;
			}
			return null;
		}

		/// <summary>
		/// Gets the Odin prefab modification type of a given property, if any.
		/// </summary>
		/// <param name="property">The property to check.</param>
		/// <param name="forceAutoRegister"></param>
		/// <returns>
		/// The prefab modification type of the property if it has one, otherwise null.
		/// </returns>
		public PrefabModificationType? GetPrefabModificationType(InspectorProperty property, bool forceAutoRegister = false)
		{
			if (!HasPrefabs)
			{
				return null;
			}
			bool registerModification;
			PrefabModificationType? result = PrivateGetPrefabModificationType(property, out registerModification);
			if (result.HasValue && (forceAutoRegister || (allowAutoRegisterPrefabModifications && registerModification && allTargetsHaveSamePrefab && !TargetHasRegisteredModificationsWaitingForApply())))
			{
				switch (result.Value)
				{
				case PrefabModificationType.Value:
				{
					for (int j = 0; j < Tree.WeakTargets.Count; j++)
					{
						RegisterPrefabValueModification(property, j);
					}
					break;
				}
				case PrefabModificationType.ListLength:
				{
					for (int k = 0; k < Tree.WeakTargets.Count; k++)
					{
						property.Children.Update();
						RegisterPrefabListLengthModification(property, k, property.Children.Count);
					}
					break;
				}
				case PrefabModificationType.Dictionary:
				{
					for (int i = 0; i < Tree.WeakTargets.Count; i++)
					{
						property.Children.Update();
						RegisterPrefabDictionaryDeltaModification(property, i);
					}
					break;
				}
				}
			}
			return result;
		}

		private static bool PropertyCanHaveModifications(InspectorProperty property)
		{
			if (!property.SupportsPrefabModifications)
			{
				return false;
			}
			if (property.ValueEntry != null && property.ValueEntry.TypeOfValue.IsGenericType && property.ValueEntry.TypeOfValue.GetGenericTypeDefinition() == typeof(EditableKeyValuePair<, >))
			{
				return false;
			}
			return true;
		}

		private PrefabModificationType? PrivateGetPrefabModificationType(InspectorProperty property, out bool registerModification)
		{
			if (Application.isPlaying || !HasPrefabs || !allTargetsHaveSamePrefab)
			{
				registerModification = false;
				return null;
			}
			if (!PropertyCanHaveModifications(property))
			{
				registerModification = false;
				if (property.Index == 0 && property.Parent != null && property.Parent.Parent != null && property.Parent.ValueEntry != null && property.Parent.ValueEntry.TypeOfValue.IsGenericType && property.Parent.ValueEntry.TypeOfValue.GetGenericTypeDefinition() == typeof(EditableKeyValuePair<, >))
				{
					InspectorProperty dictionaryProp = property.Parent.Parent;
					for (int i = 0; i < prefabDictionaryModifications.Length; i++)
					{
						object key = (dictionaryProp.ChildResolver as IKeyValueMapResolver).GetKey(i, property.Parent.Index);
						Dictionary<string, PrefabModification> mods = prefabDictionaryModifications[i];
						if (mods != null && mods.TryGetValue(dictionaryProp.PrefabModificationPath, out var mod) && mod.DictionaryKeysAdded != null && mod.DictionaryKeysAdded.Contains(key))
						{
							return PrefabModificationType.Value;
						}
					}
				}
				return null;
			}
			registerModification = true;
			for (int j = 0; j < prefabValueModifications.Length; j++)
			{
				Dictionary<string, PrefabModification> mods2 = prefabValueModifications[j];
				if (mods2 == null)
				{
					continue;
				}
				InspectorProperty prop = property;
				do
				{
					if (mods2.ContainsKey(prop.PrefabModificationPath))
					{
						IPropertyValueEntry entry = prop.ValueEntry;
						if (entry != null)
						{
							PrefabModification mod2 = mods2[prop.PrefabModificationPath];
							registerModification = entry.ValueIsPrefabDifferent(mod2.ModifiedValue, j);
						}
						else
						{
							registerModification = false;
						}
						return PrefabModificationType.Value;
					}
					prop = prop.ParentValueProperty;
				}
				while (prop != null && prop.ValueEntry.TypeOfValue.IsValueType);
			}
			for (int k = 0; k < prefabListLengthModifications.Length; k++)
			{
				Dictionary<string, PrefabModification> mods3 = prefabListLengthModifications[k];
				if (mods3 != null && mods3.TryGetValue(property.PrefabModificationPath, out var mod3))
				{
					registerModification = mod3.NewLength != property.Children.Count;
					return PrefabModificationType.ListLength;
				}
			}
			for (int l = 0; l < prefabDictionaryModifications.Length; l++)
			{
				Dictionary<string, PrefabModification> mods4 = prefabDictionaryModifications[l];
				if (mods4 != null && mods4.ContainsKey(property.PrefabModificationPath))
				{
					registerModification = false;
					return PrefabModificationType.Dictionary;
				}
			}
			if (prefabPropertyTree == null || property.ValueEntry == null)
			{
				registerModification = false;
				return null;
			}
			InspectorProperty prefabProperty = prefabPropertyTree.GetPropertyAtPrefabModificationPath(property.PrefabModificationPath);
			if (prefabProperty == null || prefabProperty.ValueEntry == null)
			{
				return PrefabModificationType.Value;
			}
			if (!prefabProperty.ValueEntry.TypeOfValue.IsValueType && prefabProperty.ValueEntry.TypeOfValue != property.ValueEntry.TypeOfValue)
			{
				return PrefabModificationType.Value;
			}
			for (int m = 0; m < prefabProperty.ValueEntry.ValueCount; m++)
			{
				if (property.ValueEntry.ValueIsPrefabDifferent(prefabProperty.ValueEntry.WeakValues[m], m))
				{
					return PrefabModificationType.Value;
				}
			}
			if (prefabProperty.ValueEntry.TypeOfValue.IsValueType && !prefabProperty.ValueEntry.ValueTypeValuesAreEqual(property.ValueEntry))
			{
				return PrefabModificationType.Value;
			}
			if (typeof(UnityEngine.Object).IsAssignableFrom(property.ValueEntry.TypeOfValue) && property.ValueEntry.WeakSmartValue != prefabProperty.ValueEntry.WeakSmartValue)
			{
				UnityEngine.Object instanceValue = (UnityEngine.Object)property.ValueEntry.WeakSmartValue;
				UnityEngine.Object prefabValue = (UnityEngine.Object)prefabProperty.ValueEntry.WeakSmartValue;
				if (instanceValue == null || prefabValue == null)
				{
					return PrefabModificationType.Value;
				}
				UnityEngine.Object instanceParentAsset = OdinPrefabSerializationEditorUtility.GetCorrespondingObjectFromSource(instanceValue);
				if (instanceParentAsset != prefabValue)
				{
					return PrefabModificationType.Value;
				}
			}
			if (prefabProperty.Children != null && property.Children != null && prefabProperty.ChildResolver is ICollectionResolver && prefabProperty.Children.Count != property.Children.Count)
			{
				if (prefabProperty.ChildResolver is IKeyValueMapResolver)
				{
					return PrefabModificationType.Dictionary;
				}
				return PrefabModificationType.ListLength;
			}
			registerModification = false;
			return null;
		}

		/// <summary>
		/// Registers a modification of type <see cref="F:Sirenix.Serialization.PrefabModificationType.ListLength" /> for a given property.
		/// </summary>
		/// <param name="property">The property to register a modification for.</param>
		/// <param name="targetIndex">Selection index of the target to register a modification for.</param>
		/// <param name="newLength">The modified list length.</param>
		/// <exception cref="T:System.ArgumentException">
		/// Property " + property.Path + " does not have a value entry; cannot register prefab modification to this property.
		/// or
		/// newLength cannot be negative!
		/// </exception>
		public void RegisterPrefabListLengthModification(InspectorProperty property, int targetIndex, int newLength)
		{
			if (!targetSupportsPrefabSerialization)
			{
				Debug.LogError("Target of type " + Tree.TargetType?.ToString() + " does not support prefab serialization! Did you apply [ShowOdinSerializedPropertiesInInspector] without implementing the ISerializationCallbackReceiver and ISupportsPrefabSerialization interface as noted in the Serialize Anything section of the manual?");
				return;
			}
			if (property.ValueEntry == null)
			{
				throw new ArgumentException("Property " + property.Path + " does not have a value entry; cannot register prefab modification to this property.");
			}
			if (!PropertyCanHaveModifications(property))
			{
				return;
			}
			Dictionary<string, PrefabModification> listLengthMods = prefabListLengthModifications[targetIndex];
			Dictionary<string, PrefabModification> valueMods = prefabValueModifications[targetIndex];
			Dictionary<string, PrefabModification> dictionaryMods = prefabDictionaryModifications[targetIndex];
			if (listLengthMods == null)
			{
				Debug.LogError("Target of type " + Tree.TargetType?.ToString() + " at index " + targetIndex + " is not a prefab!");
				return;
			}
			if (newLength < 0)
			{
				throw new ArgumentException("newLength cannot be negative!");
			}
			try
			{
				allowAutoRegisterPrefabModifications = false;
				PrefabModification mod = new PrefabModification
				{
					ModificationType = PrefabModificationType.ListLength,
					Path = property.PrefabModificationPath,
					NewLength = newLength
				};
				Update();
				RemovePrefabModificationsForInvalidIndices(property, listLengthMods, valueMods, dictionaryMods, newLength);
				listLengthMods[property.PrefabModificationPath] = mod;
				UnitySerializationUtility.RegisterPrefabModificationsChange((UnityEngine.Object)Tree.WeakTargets[targetIndex], GetPrefabModifications(targetIndex));
			}
			finally
			{
				allowAutoRegisterPrefabModifications = true;
			}
		}

		/// <summary>
		/// Registers a modification of type <see cref="F:Sirenix.Serialization.PrefabModificationType.Value" /> for a given property.
		/// </summary>
		/// <param name="property">The property to register a modification for.</param>
		/// <param name="targetIndex">Selection index of the target to register a modification for.</param>
		/// <param name="forceImmediate">Whether to force the change to be registered immediately, rather than at the end of frame.</param>
		/// <exception cref="T:System.ArgumentException">Property " + property.Path + " does not have a value entry; cannot register prefab modification to this property.</exception>
		public void RegisterPrefabValueModification(InspectorProperty property, int targetIndex, bool forceImmediate = false)
		{
			if (!targetSupportsPrefabSerialization)
			{
				Debug.LogError("Target of type " + Tree.TargetType?.ToString() + " does not support prefab serialization! Did you apply [ShowOdinSerializedPropertiesInInspector] without implementing the ISerializationCallbackReceiver and ISupportsPrefabSerialization interface as noted in the Serialize Anything section of the manual?");
				return;
			}
			if (property.ValueEntry == null)
			{
				throw new ArgumentException("Property " + property.Path + " does not have a value entry; cannot register prefab modification to this property.");
			}
			if (!PropertyCanHaveModifications(property))
			{
				return;
			}
			Dictionary<string, PrefabModification> valueMods = prefabValueModifications[targetIndex];
			Dictionary<string, PrefabModification> listLengthMods = prefabListLengthModifications[targetIndex];
			Dictionary<string, PrefabModification> dictionaryMods = prefabDictionaryModifications[targetIndex];
			if (valueMods == null)
			{
				Debug.LogError("Target of type " + Tree.TargetType?.ToString() + " at index " + targetIndex + " is not a prefab!");
				return;
			}
			try
			{
				allowAutoRegisterPrefabModifications = false;
				string propPath = property.PrefabModificationPath;
				PrefabModification mod = new PrefabModification();
				Dictionary<string, PrefabModification> extraModChanges = null;
				property.Update(forceUpdate: true);
				mod.Path = propPath;
				mod.ModifiedValue = property.ValueEntry.WeakValues[targetIndex];
				object value = property.ValueEntry.WeakValues[targetIndex];
				if (value != null && !(value is UnityEngine.Object) && (property.ValueEntry.ValueState == PropertyValueState.Reference || (!property.BaseValueEntry.TypeOfValue.IsValueType && !(property.BaseValueEntry.TypeOfValue == typeof(string)))))
				{
					mod.ReferencePaths = new List<string>();
					foreach (InspectorProperty prop in Tree.EnumerateTree())
					{
						if (prop.ValueEntry == null || prop.Info.TypeOfValue.IsValueType || prop.Path == property.Path)
						{
							continue;
						}
						prop.Update(forceUpdate: true);
						if (value != prop.ValueEntry.WeakValues[targetIndex])
						{
							continue;
						}
						if (mod.ReferencePaths.Count < 5)
						{
							mod.ReferencePaths.Add(prop.PrefabModificationPath);
						}
						if (!valueMods.TryGetValue(prop.PrefabModificationPath, out var refMod) || (refMod.ReferencePaths != null && refMod.ReferencePaths.Contains(property.PrefabModificationPath)))
						{
							continue;
						}
						if (refMod.ReferencePaths == null)
						{
							refMod.ReferencePaths = new List<string>();
						}
						if (refMod.ReferencePaths.Count < 5)
						{
							refMod.ReferencePaths.Add(property.PrefabModificationPath);
							if (extraModChanges == null)
							{
								extraModChanges = new Dictionary<string, PrefabModification>();
							}
							extraModChanges[prop.PrefabModificationPath] = refMod;
						}
					}
				}
				else if (value is UnityEngine.Object)
				{
					mod.ReferencePaths = null;
				}
				if (forceImmediate)
				{
					Update();
					RemoveInvalidPrefabModifications("", listLengthMods, valueMods, dictionaryMods);
					valueMods[propPath] = mod;
					listLengthMods.Remove(propPath);
					dictionaryMods.Remove(propPath);
					if (extraModChanges != null)
					{
						foreach (KeyValuePair<string, PrefabModification> item in extraModChanges)
						{
							valueMods[item.Key] = item.Value;
						}
					}
					UnitySerializationUtility.RegisterPrefabModificationsChange((UnityEngine.Object)Tree.WeakTargets[targetIndex], GetPrefabModifications(targetIndex));
					return;
				}
				Tree.DelayAction(delegate
				{
					Update();
					Tree.UpdateTree();
					RemoveInvalidPrefabModifications("", listLengthMods, valueMods, dictionaryMods);
					valueMods[propPath] = mod;
					listLengthMods.Remove(propPath);
					dictionaryMods.Remove(propPath);
					if (extraModChanges != null)
					{
						foreach (KeyValuePair<string, PrefabModification> current in extraModChanges)
						{
							valueMods[current.Key] = current.Value;
						}
					}
					UnitySerializationUtility.RegisterPrefabModificationsChange((UnityEngine.Object)Tree.WeakTargets[targetIndex], GetPrefabModifications(targetIndex));
				});
			}
			finally
			{
				allowAutoRegisterPrefabModifications = true;
			}
		}

		/// <summary>
		/// Calculates a delta between the current dictionary property and its prefab counterpart, and registers that delta as a <see cref="F:Sirenix.Serialization.PrefabModificationType.Dictionary" /> modification.
		/// </summary>
		/// <param name="property">The property to register a modification for.</param>
		/// <param name="targetIndex">Selection index of the target.</param>
		/// <exception cref="T:System.ArgumentException">Property " + property.Path + " does not have a value entry; cannot register prefab modification to this property.</exception>
		public void RegisterPrefabDictionaryDeltaModification(InspectorProperty property, int targetIndex)
		{
			if (!targetSupportsPrefabSerialization)
			{
				Debug.LogError("Target of type " + Tree.TargetType?.ToString() + " does not support prefab serialization! Did you apply [ShowOdinSerializedPropertiesInInspector] without implementing the ISerializationCallbackReceiver and ISupportsPrefabSerialization interface as noted in the Serialize Anything section of the manual?");
				return;
			}
			if (property.ValueEntry == null)
			{
				throw new ArgumentException("Property " + property.Path + " does not have a value entry; cannot register prefab modification to this property.");
			}
			if (!PropertyCanHaveModifications(property))
			{
				return;
			}
			Dictionary<string, PrefabModification> valueMods = prefabValueModifications[targetIndex];
			Dictionary<string, PrefabModification> listLengthMods = prefabListLengthModifications[targetIndex];
			Dictionary<string, PrefabModification> dictionaryMods = prefabDictionaryModifications[targetIndex];
			if (valueMods == null)
			{
				Debug.LogError("Target of type " + Tree.TargetType?.ToString() + " at index " + targetIndex + " is not a prefab!");
				return;
			}
			try
			{
				allowAutoRegisterPrefabModifications = false;
				string propPath = property.PrefabModificationPath;
				if (valueMods.TryGetValue(propPath, out var mod))
				{
					RegisterPrefabValueModification(property, targetIndex);
					return;
				}
				bool modificationIsNew = false;
				if (!dictionaryMods.TryGetValue(propPath, out mod))
				{
					modificationIsNew = true;
					mod = new PrefabModification();
					mod.ModificationType = PrefabModificationType.Dictionary;
					mod.Path = propPath;
				}
				InspectorProperty prefabProperty = prefabPropertyTree.GetPropertyAtPath(property.Path);
				if (prefabProperty == null)
				{
					return;
				}
				int prefabIndex = prefabPropertyTreeIndexMap[targetIndex];
				IDictionary prefabDict = prefabProperty.ValueEntry.WeakValues[prefabIndex] as IDictionary;
				IDictionary propDict = property.ValueEntry.WeakValues[targetIndex] as IDictionary;
				if (prefabDict == null || propDict == null)
				{
					return;
				}
				foreach (object key in prefabDict.Keys)
				{
					if (!propDict.Contains(key))
					{
						if (mod.DictionaryKeysRemoved == null)
						{
							mod.DictionaryKeysRemoved = new object[1] { key };
						}
						else
						{
							mod.DictionaryKeysRemoved = ArrayUtilities.CreateNewArrayWithAddedElement(mod.DictionaryKeysRemoved, key);
						}
					}
				}
				foreach (object key2 in propDict.Keys)
				{
					if (!prefabDict.Contains(key2))
					{
						if (mod.DictionaryKeysAdded == null)
						{
							mod.DictionaryKeysAdded = new object[1] { key2 };
						}
						else
						{
							mod.DictionaryKeysAdded = ArrayUtilities.CreateNewArrayWithAddedElement(mod.DictionaryKeysAdded, key2);
						}
					}
				}
				if (!modificationIsNew || (mod.DictionaryKeysAdded != null && mod.DictionaryKeysAdded.Length != 0) || (mod.DictionaryKeysRemoved != null && mod.DictionaryKeysRemoved.Length != 0))
				{
					Update();
					RemoveInvalidPrefabModifications("", listLengthMods, valueMods, dictionaryMods);
					dictionaryMods[propPath] = mod;
					UnitySerializationUtility.RegisterPrefabModificationsChange((UnityEngine.Object)Tree.WeakTargets[targetIndex], GetPrefabModifications(targetIndex));
				}
			}
			finally
			{
				allowAutoRegisterPrefabModifications = true;
			}
		}

		/// <summary>
		/// Adds a remove key modification to the dictionary modifications of a given property.
		/// </summary>
		/// <param name="property">The property to register a modification for.</param>
		/// <param name="targetIndex">Selection index of the target.</param>
		/// <param name="key">The key to be removed.</param>
		/// <exception cref="T:System.ArgumentException">Property " + property.Path + " does not have a value entry; cannot register prefab modification to this property.</exception>
		public void RegisterPrefabDictionaryRemoveKeyModification(InspectorProperty property, int targetIndex, object key)
		{
			if (!targetSupportsPrefabSerialization)
			{
				Debug.LogError("Target of type " + Tree.TargetType?.ToString() + " does not support prefab serialization! Did you apply [ShowOdinSerializedPropertiesInInspector] without implementing the ISerializationCallbackReceiver and ISupportsPrefabSerialization interface as noted in the Serialize Anything section of the manual?");
				return;
			}
			if (property.ValueEntry == null)
			{
				throw new ArgumentException("Property " + property.Path + " does not have a value entry; cannot register prefab modification to this property.");
			}
			if (!PropertyCanHaveModifications(property))
			{
				return;
			}
			Dictionary<string, PrefabModification> valueMods = prefabValueModifications[targetIndex];
			Dictionary<string, PrefabModification> listLengthMods = prefabListLengthModifications[targetIndex];
			Dictionary<string, PrefabModification> dictionaryMods = prefabDictionaryModifications[targetIndex];
			if (valueMods == null)
			{
				Debug.LogError("Target of type " + Tree.TargetType?.ToString() + " at index " + targetIndex + " is not a prefab!");
				return;
			}
			try
			{
				allowAutoRegisterPrefabModifications = false;
				string propPath = property.PrefabModificationPath;
				if (!dictionaryMods.TryGetValue(propPath, out var mod))
				{
					mod = new PrefabModification();
					mod.ModificationType = PrefabModificationType.Dictionary;
					mod.Path = propPath;
				}
				bool actuallySetRemoveKey = true;
				if (mod.DictionaryKeysAdded != null)
				{
					for (int i = 0; i < mod.DictionaryKeysAdded.Length; i++)
					{
						if (key.Equals(mod.DictionaryKeysAdded[i]))
						{
							mod.DictionaryKeysAdded = ArrayUtilities.CreateNewArrayWithRemovedElement(mod.DictionaryKeysAdded, i);
							actuallySetRemoveKey = false;
							i--;
						}
					}
				}
				if (actuallySetRemoveKey)
				{
					if (mod.DictionaryKeysRemoved == null)
					{
						mod.DictionaryKeysRemoved = new object[1] { key };
					}
					else
					{
						mod.DictionaryKeysRemoved = ArrayUtilities.CreateNewArrayWithAddedElement(mod.DictionaryKeysRemoved, key);
					}
				}
				Update();
				RemoveInvalidPrefabModifications("", listLengthMods, valueMods, dictionaryMods);
				dictionaryMods[propPath] = mod;
				UnitySerializationUtility.RegisterPrefabModificationsChange((UnityEngine.Object)Tree.WeakTargets[targetIndex], GetPrefabModifications(targetIndex));
			}
			finally
			{
				allowAutoRegisterPrefabModifications = true;
			}
		}

		/// <summary>
		/// Adds an add key modification to the dictionary modifications of a given property.
		/// </summary>
		/// <param name="property">The property to register a modification for.</param>
		/// <param name="targetIndex">Selection index of the target.</param>
		/// <param name="key">The key to be added.</param>
		/// <exception cref="T:System.ArgumentException">Property " + property.Path + " does not have a value entry; cannot register prefab modification to this property.</exception>
		public void RegisterPrefabDictionaryAddKeyModification(InspectorProperty property, int targetIndex, object key)
		{
			if (!targetSupportsPrefabSerialization)
			{
				Debug.LogError("Target of type " + Tree.TargetType?.ToString() + " does not support prefab serialization! Did you apply [ShowOdinSerializedPropertiesInInspector] without implementing the ISerializationCallbackReceiver and ISupportsPrefabSerialization interface as noted in the Serialize Anything section of the manual?");
				return;
			}
			if (property.ValueEntry == null)
			{
				throw new ArgumentException("Property " + property.Path + " does not have a value entry; cannot register prefab modification to this property.");
			}
			if (!PropertyCanHaveModifications(property))
			{
				return;
			}
			Dictionary<string, PrefabModification> valueMods = prefabValueModifications[targetIndex];
			Dictionary<string, PrefabModification> listLengthMods = prefabListLengthModifications[targetIndex];
			Dictionary<string, PrefabModification> dictionaryMods = prefabDictionaryModifications[targetIndex];
			if (valueMods == null)
			{
				Debug.LogError("Target of type " + Tree.TargetType?.ToString() + " at index " + targetIndex + " is not a prefab!");
				return;
			}
			try
			{
				allowAutoRegisterPrefabModifications = false;
				string propPath = property.PrefabModificationPath;
				if (!dictionaryMods.TryGetValue(propPath, out var mod))
				{
					mod = new PrefabModification();
					mod.ModificationType = PrefabModificationType.Dictionary;
					mod.Path = propPath;
				}
				if (mod.DictionaryKeysAdded == null)
				{
					mod.DictionaryKeysAdded = new object[1] { key };
				}
				else
				{
					mod.DictionaryKeysAdded = ArrayUtilities.CreateNewArrayWithAddedElement(mod.DictionaryKeysAdded, key);
				}
				if (mod.DictionaryKeysRemoved != null)
				{
					for (int i = 0; i < mod.DictionaryKeysRemoved.Length; i++)
					{
						if (key.Equals(mod.DictionaryKeysRemoved[i]))
						{
							mod.DictionaryKeysRemoved = ArrayUtilities.CreateNewArrayWithRemovedElement(mod.DictionaryKeysRemoved, i);
							i--;
						}
					}
				}
				Update();
				RemoveInvalidPrefabModifications("", listLengthMods, valueMods, dictionaryMods);
				dictionaryMods[propPath] = mod;
				UnitySerializationUtility.RegisterPrefabModificationsChange((UnityEngine.Object)Tree.WeakTargets[targetIndex], GetPrefabModifications(targetIndex));
			}
			finally
			{
				allowAutoRegisterPrefabModifications = true;
			}
		}

		/// <summary>
		/// Removes all dictionary modifications on a property for a given dictionary key value.
		/// </summary>
		/// <param name="property">The property to remove a key modification for.</param>
		/// <param name="targetIndex">Selection index of the target.</param>
		/// <param name="key">The key to remove modifications for.</param>
		/// <exception cref="T:System.ArgumentNullException">key</exception>
		public void RemovePrefabDictionaryModification(InspectorProperty property, int targetIndex, object key)
		{
			if (key == null)
			{
				throw new ArgumentNullException("key");
			}
			if (property.ValueEntry == null || !typeof(UnityEngine.Object).IsAssignableFrom(Tree.TargetType))
			{
				return;
			}
			if (!targetSupportsPrefabSerialization)
			{
				Debug.LogError("Target of type " + Tree.TargetType?.ToString() + " does not support prefab serialization! Did you apply [ShowOdinSerializedPropertiesInInspector] without implementing the ISerializationCallbackReceiver and ISupportsPrefabSerialization interface as noted in the Serialize Anything section of the manual?");
				return;
			}
			Dictionary<string, PrefabModification> listLengthMods = prefabListLengthModifications[targetIndex];
			Dictionary<string, PrefabModification> valueMods = prefabValueModifications[targetIndex];
			Dictionary<string, PrefabModification> dictionaryMods = prefabDictionaryModifications[targetIndex];
			if (listLengthMods == null || valueMods == null || dictionaryMods == null)
			{
				Debug.LogError("Target of type " + Tree.TargetType?.ToString() + " at index " + targetIndex + " is not a prefab!");
				return;
			}
			try
			{
				allowAutoRegisterPrefabModifications = false;
				bool changed = false;
				string removePath = property.PrefabModificationPath;
				if (dictionaryMods.TryGetValue(removePath, out var mod))
				{
					if (mod.DictionaryKeysRemoved != null)
					{
						for (int i = 0; i < mod.DictionaryKeysRemoved.Length; i++)
						{
							if (key.Equals(mod.DictionaryKeysRemoved[i]))
							{
								changed = true;
								mod.DictionaryKeysRemoved = ArrayUtilities.CreateNewArrayWithRemovedElement(mod.DictionaryKeysRemoved, i);
								i--;
							}
						}
					}
					if (mod.DictionaryKeysAdded != null)
					{
						for (int j = 0; j < mod.DictionaryKeysAdded.Length; j++)
						{
							if (key.Equals(mod.DictionaryKeysAdded[j]))
							{
								changed = true;
								mod.DictionaryKeysAdded = ArrayUtilities.CreateNewArrayWithRemovedElement(mod.DictionaryKeysAdded, j);
								j--;
							}
						}
						if (changed)
						{
							string checkPath = removePath + "." + DictionaryKeyUtility.GetDictionaryKeyString(key);
							HashSet<string> toRemove = new HashSet<string>();
							foreach (string path in listLengthMods.Keys.AppendWith(valueMods.Keys).AppendWith(dictionaryMods.Keys))
							{
								if (path.StartsWith(checkPath, StringComparison.InvariantCulture))
								{
									toRemove.Add(path);
								}
							}
							foreach (string path2 in toRemove)
							{
								listLengthMods.Remove(path2);
								valueMods.Remove(path2);
								dictionaryMods.Remove(path2);
							}
						}
					}
					if ((mod.DictionaryKeysRemoved == null || mod.DictionaryKeysRemoved.Length == 0) && (mod.DictionaryKeysRemoved == null || mod.DictionaryKeysRemoved.Length == 0))
					{
						dictionaryMods.Remove(removePath);
					}
					changed = true;
				}
				if (changed)
				{
					UnitySerializationUtility.RegisterPrefabModificationsChange((UnityEngine.Object)Tree.WeakTargets[targetIndex], GetPrefabModifications(targetIndex));
				}
			}
			finally
			{
				allowAutoRegisterPrefabModifications = true;
			}
		}

		/// <summary>
		/// Removes all prefab modifications of a given type on a given property.
		/// </summary>
		/// <param name="property">The property to remove modifications for.</param>
		/// <param name="targetIndex">Selection index of the target.</param>
		/// <param name="modificationType">Type of the modification to remove.</param>
		public void RemovePrefabModification(InspectorProperty property, int targetIndex, PrefabModificationType modificationType)
		{
			if (property.ValueEntry == null || !typeof(UnityEngine.Object).IsAssignableFrom(Tree.TargetType))
			{
				return;
			}
			try
			{
				allowAutoRegisterPrefabModifications = false;
				if (property.ValueEntry.SerializationBackend.IsUnity)
				{
					UnityEngine.Object target = (UnityEngine.Object)Tree.WeakTargets[targetIndex];
					UnityEngine.Object prefab = TargetPrefabs[targetIndex];
					List<PropertyModification> unityMods = PrefabUtility.GetPropertyModifications(target).ToList();
					switch (modificationType)
					{
					case PrefabModificationType.Value:
					{
						for (int j = 0; j < unityMods.Count; j++)
						{
							PropertyModification mod2 = unityMods[j];
							if (mod2.target == prefab && mod2.propertyPath.StartsWith(property.UnityPropertyPath, StringComparison.InvariantCulture))
							{
								unityMods.RemoveAt(j);
								j--;
							}
						}
						break;
					}
					case PrefabModificationType.ListLength:
					{
						string sizePath = property.UnityPropertyPath + ".Array.size";
						for (int i = 0; i < unityMods.Count; i++)
						{
							PropertyModification mod = unityMods[i];
							if (mod.target == prefab && mod.propertyPath == sizePath)
							{
								unityMods.RemoveAt(i);
								i--;
							}
						}
						RemovePrefabModificationsForInvalidIndices(property, prefab, unityMods);
						break;
					}
					}
					PrefabUtility.SetPropertyModifications(target, unityMods.ToArray());
					string name = Undo.GetCurrentGroupName();
					Undo.FlushUndoRecordObjects();
					Tree.RootProperty.RecordForUndo();
					PrefabUtility.SetPropertyModifications(target, unityMods.ToArray());
					(property.ValueEntry as PropertyValueEntry).TriggerOnValueChanged(targetIndex);
				}
				else
				{
					if (property.ValueEntry.SerializationBackend != SerializationBackend.Odin)
					{
						return;
					}
					if (!targetSupportsPrefabSerialization)
					{
						Debug.LogError("Target of type " + Tree.TargetType?.ToString() + " does not support prefab serialization! Did you apply [ShowOdinSerializedPropertiesInInspector] without implementing the ISerializationCallbackReceiver and ISupportsPrefabSerialization interface as noted in the Serialize Anything section of the manual?");
						return;
					}
					Dictionary<string, PrefabModification> listLengthMods = prefabListLengthModifications[targetIndex];
					Dictionary<string, PrefabModification> valueMods = prefabValueModifications[targetIndex];
					Dictionary<string, PrefabModification> dictionaryMods = prefabDictionaryModifications[targetIndex];
					if (listLengthMods == null || valueMods == null || dictionaryMods == null)
					{
						Debug.LogError("Target of type " + Tree.TargetType?.ToString() + " at index " + targetIndex + " is not a prefab!");
						return;
					}
					string removePath = property.PrefabModificationPath;
					bool removed = false;
					PrefabModification mod3;
					if (modificationType == PrefabModificationType.Value && valueMods.ContainsKey(removePath))
					{
						Update();
						valueMods.Remove(removePath);
						string checkPath = removePath + ".";
						HashSet<string> toRemove = new HashSet<string>();
						foreach (string path in listLengthMods.Keys.AppendWith(valueMods.Keys).AppendWith(dictionaryMods.Keys))
						{
							if (path.StartsWith(checkPath, StringComparison.InvariantCulture))
							{
								toRemove.Add(path);
							}
						}
						foreach (string path2 in toRemove)
						{
							listLengthMods.Remove(path2);
							valueMods.Remove(path2);
							dictionaryMods.Remove(path2);
						}
						removed = true;
					}
					else if (modificationType == PrefabModificationType.ListLength && listLengthMods.TryGetValue(removePath, out mod3))
					{
						Update();
						listLengthMods.Remove(removePath);
						InspectorProperty prefabProperty = prefabPropertyTree.GetPropertyAtPath(property.Path);
						if (prefabProperty != null)
						{
							int prefabChildCount = ((prefabProperty.ChildResolver is ICollectionResolver collectionResolver) ? collectionResolver.MaxCollectionLength : prefabProperty.Children.Count);
							RemovePrefabModificationsForInvalidIndices(property, listLengthMods, valueMods, dictionaryMods, prefabChildCount);
						}
						removed = true;
					}
					else if (modificationType == PrefabModificationType.Dictionary && dictionaryMods.TryGetValue(removePath, out mod3))
					{
						Update();
						dictionaryMods.Remove(removePath);
						if (mod3.DictionaryKeysAdded != null)
						{
							HashSet<string> toRemove2 = new HashSet<string>();
							for (int k = 0; k < mod3.DictionaryKeysAdded.Length; k++)
							{
								string keyStr = DictionaryKeyUtility.GetDictionaryKeyString(mod3.DictionaryKeysAdded[k]);
								string checkPath2 = removePath + "." + keyStr;
								foreach (string path3 in listLengthMods.Keys.AppendWith(valueMods.Keys).AppendWith(dictionaryMods.Keys))
								{
									if (path3.StartsWith(checkPath2, StringComparison.InvariantCulture))
									{
										toRemove2.Add(path3);
									}
								}
							}
							foreach (string path4 in toRemove2)
							{
								listLengthMods.Remove(path4);
								valueMods.Remove(path4);
								dictionaryMods.Remove(path4);
							}
						}
						removed = true;
					}
					if (removed)
					{
						UnitySerializationUtility.RegisterPrefabModificationsChange((UnityEngine.Object)Tree.WeakTargets[targetIndex], GetPrefabModifications(targetIndex));
						(property.ValueEntry as PropertyValueEntry).TriggerOnValueChanged(targetIndex);
					}
				}
			}
			finally
			{
				allowAutoRegisterPrefabModifications = true;
			}
		}

		private void RemoveInvalidPrefabModifications(string startPath, Dictionary<string, PrefabModification> listLengthMods, Dictionary<string, PrefabModification> valueMods, Dictionary<string, PrefabModification> dictionaryMods)
		{
			HashSet<string> toRemove = new HashSet<string>();
			foreach (string path in listLengthMods.Keys.AppendWith(valueMods.Keys).AppendWith(dictionaryMods.Keys))
			{
				if (path.StartsWith(startPath))
				{
					InspectorProperty prop = Tree.GetPropertyAtPrefabModificationPath(path);
					if (prop == null || !prop.SupportsPrefabModifications)
					{
						toRemove.Add(path);
					}
				}
			}
			foreach (string path2 in toRemove)
			{
				listLengthMods.Remove(path2);
				valueMods.Remove(path2);
				dictionaryMods.Remove(path2);
			}
		}

		private void RemovePrefabModificationsForInvalidIndices(InspectorProperty property, Dictionary<string, PrefabModification> listLengthMods, Dictionary<string, PrefabModification> valueMods, Dictionary<string, PrefabModification> dictionaryMods, int newLength)
		{
			string removePath = property.PrefabModificationPath;
			HashSet<string> toRemove = new HashSet<string>();
			string checkPath = removePath + ".[";
			foreach (string path in listLengthMods.Keys.AppendWith(valueMods.Keys).AppendWith(dictionaryMods.Keys))
			{
				if (!path.StartsWith(checkPath, StringComparison.InvariantCulture))
				{
					continue;
				}
				int arrayEndIndex = path.IndexOf("]", checkPath.Length, StringComparison.InvariantCulture);
				if (arrayEndIndex > checkPath.Length)
				{
					string indexStr = path.Substring(checkPath.Length, arrayEndIndex - checkPath.Length);
					if (int.TryParse(indexStr, out var index) && index >= newLength)
					{
						toRemove.Add(path);
					}
				}
			}
			foreach (string path2 in toRemove)
			{
				listLengthMods.Remove(path2);
				valueMods.Remove(path2);
				dictionaryMods.Remove(path2);
			}
		}

		private void RemovePrefabModificationsForInvalidIndices(InspectorProperty property, UnityEngine.Object prefab, List<PropertyModification> unityMods)
		{
			string checkPath = property.UnityPropertyPath + ".Array.data[";
			InspectorProperty prefabProperty = prefabPropertyTree.GetPropertyAtPath(property.Path);
			if (prefabProperty == null)
			{
				return;
			}
			HashSet<string> toRemove = new HashSet<string>();
			int prefabChildCount = ((prefabProperty.ChildResolver is ICollectionResolver collectionResolver) ? collectionResolver.MaxCollectionLength : prefabProperty.Children.Count);
			if (prefabChildCount < property.Children.Count)
			{
				foreach (PropertyModification mod in unityMods)
				{
					string path = mod.propertyPath;
					if (!path.StartsWith(checkPath, StringComparison.InvariantCulture))
					{
						continue;
					}
					int arrayEndIndex = path.IndexOf("]", checkPath.Length, StringComparison.InvariantCulture);
					if (arrayEndIndex > checkPath.Length)
					{
						string indexStr = path.Substring(checkPath.Length, arrayEndIndex - checkPath.Length);
						if (int.TryParse(indexStr, out var index) && index >= prefabChildCount)
						{
							toRemove.Add(path);
						}
					}
				}
			}
			for (int i = 0; i < unityMods.Count; i++)
			{
				PropertyModification mod2 = unityMods[i];
				if (mod2.target == prefab && toRemove.Contains(mod2.propertyPath))
				{
					unityMods.RemoveAt(i);
					i--;
				}
			}
		}

		/// <summary>
		/// Gets all prefab modifications in this property tree for a given selection index.
		/// </summary>
		/// <param name="targetIndex"></param>
		/// <returns></returns>
		public List<PrefabModification> GetPrefabModifications(int targetIndex)
		{
			if (!targetSupportsPrefabSerialization)
			{
				return new List<PrefabModification>();
			}
			Dictionary<string, PrefabModification> valueMods = prefabValueModifications[targetIndex];
			Dictionary<string, PrefabModification> listLengthMods = prefabListLengthModifications[targetIndex];
			Dictionary<string, PrefabModification> dictionaryMods = prefabDictionaryModifications[targetIndex];
			if (valueMods == null || listLengthMods == null || dictionaryMods == null)
			{
				return new List<PrefabModification>();
			}
			return listLengthMods.Values.AppendWith(valueMods.Values).AppendWith(dictionaryMods.Values).ToList();
		}
	}
}
