using System;
using System.Collections.Generic;
using System.Reflection;
using Sirenix.Utilities;
using UnityEngine.Serialization;

namespace Sirenix.OdinInspector.Editor.Internal
{
	internal static class DesignerInspectorPropertyInfoUtility
	{
		private static readonly List<InspectorPropertyInfo> CurrentInfos = new List<InspectorPropertyInfo>(16);

		private static readonly List<InspectorPropertyInfo> InfosWithDesiredIndex = new List<InspectorPropertyInfo>(16);

		public static readonly Dictionary<Type, int> AttributeMap = new Dictionary<Type, int>(8);

		public static readonly MutablePropertyInfo Root = new MutablePropertyInfo
		{
			Children = new List<MutablePropertyInfo>(32)
		};

		public static readonly Dictionary<string, MutablePropertyInfo> Groups = new Dictionary<string, MutablePropertyInfo>(16);

		public static readonly Dictionary<string, MutablePropertyInfo> Properties = new Dictionary<string, MutablePropertyInfo>(32);

		public static readonly Dictionary<string, MutablePropertyInfo> PropertiesFormerSerialized = new Dictionary<string, MutablePropertyInfo>(32);

		public static readonly List<MutablePropertyInfo> ChildrenFromRemovedParentsBuffer = new List<MutablePropertyInfo>(16);

		public static void PrioritizeDesiredIndex(InspectorPropertyInfo[] infos)
		{
			CurrentInfos.Clear();
			InfosWithDesiredIndex.Clear();
			foreach (InspectorPropertyInfo current in infos)
			{
				if (current.DesignerDesiredIndex == -1)
				{
					CurrentInfos.Add(current);
				}
				else
				{
					InfosWithDesiredIndex.Add(current);
				}
			}
			InfosWithDesiredIndex.Sort(CompareByDesiredIndexThenByHierarchyDepth);
			for (int j = 0; j < InfosWithDesiredIndex.Count; j++)
			{
				InspectorPropertyInfo current2 = InfosWithDesiredIndex[j];
				bool isAdded = false;
				for (int k = 0; k < CurrentInfos.Count; k++)
				{
					InspectorPropertyInfo other = CurrentInfos[k];
					if (k >= current2.DesignerDesiredIndex || other.DesignerDesiredIndex == current2.DesignerDesiredIndex)
					{
						CurrentInfos.Insert(k, current2);
						isAdded = true;
						break;
					}
				}
				if (!isAdded)
				{
					CurrentInfos.Add(current2);
				}
			}
			for (int l = 0; l < infos.Length; l++)
			{
				infos[l] = CurrentInfos[l];
			}
		}

		public static int CompareByDesiredIndexThenByHierarchyDepth(InspectorPropertyInfo a, InspectorPropertyInfo b)
		{
			int cmp = a.DesignerDesiredIndex.CompareTo(b.DesignerDesiredIndex);
			if (cmp == 0)
			{
				return a.DesignerHierarchyDepth.CompareTo(b.DesignerHierarchyDepth);
			}
			return cmp;
		}

		public static void ApplySelfPatches(InspectorProperty property, List<Attribute> attributes)
		{
			if (DesignerUtils.IsExcludedFromDesigner(property))
			{
				return;
			}
			TypePatch patch = TypePatchCache.Get(property);
			if (patch == null)
			{
				return;
			}
			foreach (TypePatch current in patch.TraverseFromRoot())
			{
				RefList<AttributePatch> selfPatches = current.SelfPatches;
				if (current.HasEditorVariant)
				{
					selfPatches = current.EditorVariant.SelfPatches;
				}
				AttributeMap.Clear();
				for (int i = 0; i < attributes.Count; i++)
				{
					Attribute attribute = attributes[i];
					AttributeMap[attribute.GetType()] = i;
				}
				for (int j = 0; j < selfPatches.Length; j++)
				{
					ref AttributePatch attributePatch = ref selfPatches[j];
					if (attributePatch.AttributeType == null)
					{
						continue;
					}
					Attribute instance = null;
					if (AttributeMap.TryGetValue(attributePatch.AttributeType, out var instanceIndex))
					{
						instance = attributes[instanceIndex];
					}
					switch (attributePatch.PatchType)
					{
					case AttributePatchType.Add:
						if (instance == null)
						{
							instance = DesignerAttributeCreator.Create(attributePatch.AttributeType);
							attributes.Add(instance);
						}
						else
						{
							instance = (attributes[instanceIndex] = FastDeepCopier.DeepCopy(instance));
						}
						break;
					case AttributePatchType.Remove:
						if (instance != null)
						{
							attributes.Remove(instance);
						}
						continue;
					case AttributePatchType.Modify:
					case AttributePatchType.ModifyUnused:
						if (instance == null)
						{
							attributePatch.PatchType = AttributePatchType.ModifyUnused;
							continue;
						}
						attributePatch.PatchType = AttributePatchType.Modify;
						instance = (attributes[instanceIndex] = FastDeepCopier.DeepCopy(instance));
						break;
					}
					object instanceObj = instance;
					for (int k = 0; k < attributePatch.MemberDeltas.Length; k++)
					{
						ref MemberDelta delta = ref attributePatch.MemberDeltas[k];
						WeakValueSetter setter = DesignerAttributeSetters.GetSetter(delta.Member);
						setter(ref instanceObj, delta.Value);
					}
				}
			}
		}

		public static ulong CalcHash(List<InspectorPropertyInfo> infos)
		{
			ulong result = 14695981039346656037uL;
			for (int i = 0; i < infos.Count; i++)
			{
				string str = infos[i].PropertyName;
				foreach (char c in str)
				{
					result ^= (byte)c;
					result *= 1099511628211L;
					result ^= (byte)((int)c >> 8);
					result *= 1099511628211L;
				}
				result ^= 0xFF;
				result *= 1099511628211L;
			}
			return result;
		}

		public static bool TryCreate(InspectorProperty parentProperty, MemberInfo member, List<Attribute> attributes, out InspectorPropertyInfo result)
		{
			SerializationBackend backend;
			bool isVisibleByDefault = InspectorPropertyInfoUtility.IsMemberVisibleByDefault(parentProperty, member, out backend);
			bool isResultValid = InspectorPropertyInfoUtility.TryCreate(member, backend, allowEditable: true, out result, attributes, parentProperty.Tree.IsDesignerTree);
			if (isResultValid)
			{
				result.IsShownInInspector = isVisibleByDefault;
			}
			return isResultValid;
		}

		public static void ApplyPatch(TypePatch typePatch, InspectorProperty parentProperty, List<InspectorPropertyInfo> infos)
		{
			Groups.Clear();
			Properties.Clear();
			PropertiesFormerSerialized.Clear();
			Type ownerType = parentProperty.ValueEntry.TypeOfValue;
			bool isEditor = parentProperty.Tree.IsMadeForDesignerEditor;
			Type targetType = typePatch.TargetType;
			for (int i = 0; i < infos.Count; i++)
			{
				InspectorPropertyInfo info = infos[i];
				ToMutableInfo(info, Root);
				if (info.PropertyType != PropertyType.Group && info.GetMemberInfo() == null)
				{
					info.IsShownInInspector = true;
				}
			}
			Groups[string.Empty] = Root;
			DesignerMethodInfoRetriever.Prepare(typePatch.TargetType);
			foreach (TypePatch current in typePatch.TraverseFromRoot())
			{
				current.GetActivePatches(out var groupPatches, out var propertyPatches);
				if (groupPatches != null)
				{
					for (int j = 0; j < groupPatches.Count; j++)
					{
						GroupPatch patch = groupPatches[j];
						if (!Groups.TryGetValue(patch.Id, out var group))
						{
							group = AddMissingGroup(patch.Id, ownerType);
						}
						group.ApplyPatch(patch);
						if (patch.HasBeenMoved)
						{
							group.DesiredIndex = patch.DesiredIndex;
							MoveMutableInfo(group, patch.ParentId, ownerType);
						}
					}
				}
				if (propertyPatches != null)
				{
					for (int k = 0; k < propertyPatches.Count; k++)
					{
						PropertyPatch patch2 = propertyPatches[k];
						string lookupName;
						if (patch2.IsComplexMethod())
						{
							MethodInfo method = DesignerMethodInfoRetriever.Get(current.TargetType, patch2.Name);
							if (method == null)
							{
								lookupName = patch2.Name;
							}
							else
							{
								MethodInfo openVariant = DesignerUtils.GetOpenGenericVariantOrSelf(method);
								lookupName = ((openVariant != method) ? DesignerUtils.CreateSerializedMethodName(openVariant) : patch2.Name);
							}
						}
						else
						{
							lookupName = patch2.Name;
						}
						bool isFormerlySerialized = false;
						if (!Properties.TryGetValue(lookupName, out var property))
						{
							if (!PropertiesFormerSerialized.TryGetValue(lookupName, out property))
							{
								continue;
							}
							isFormerlySerialized = true;
						}
						if (!property.IsExcluded())
						{
							if (isFormerlySerialized && !string.IsNullOrEmpty(property.Info?.PropertyName))
							{
								patch2.Name = property.Info.PropertyName;
							}
							property.ApplyPatch(patch2);
							if (patch2.HasBeenMoved)
							{
								property.DesiredIndex = patch2.DesiredIndex;
								MoveMutableInfo(property, patch2.ParentId, ownerType);
							}
						}
					}
				}
				Root.ConsumeDesiredIndicesRecursively();
			}
			for (int l = 0; l < Root.Children.Count; l++)
			{
				Root.Children[l].ApplyInfoModificationsFromAttributes();
			}
			ChildrenFromRemovedParentsBuffer.Clear();
			for (int i2 = Root.Children.Count - 1; i2 >= 0; i2--)
			{
				Root.Children[i2].HandleMissingAndRemovedGroups(isEditor, ChildrenFromRemovedParentsBuffer);
			}
			for (int i3 = ChildrenFromRemovedParentsBuffer.Count - 1; i3 >= 0; i3--)
			{
				MutablePropertyInfo current2 = ChildrenFromRemovedParentsBuffer[i3];
				current2.Parent = Root;
				Root.Children.Add(current2);
			}
			if (!isEditor)
			{
				Root.RemoveHiddenChildren();
				Root.SortExcludedByOrder();
			}
			Root.RowColumnCount = 0;
			Root.UsedGroupNames.Clear();
			for (int m = 0; m < Root.Children.Count; m++)
			{
				Root.Children[m].BakeToInfo(isEditor);
			}
			infos.Clear();
			for (int n = 0; n < Root.Children.Count; n++)
			{
				infos.Add(Root.Children[n].Info);
			}
			for (int num = 0; num < Root.Children.Count; num++)
			{
				MutablePropertyInfoPool.Return(Root.Children[num]);
			}
			Root.Children.Clear();
		}

		public static void MoveMutableInfo(MutablePropertyInfo info, string parentId, Type ownerType)
		{
			if (!Groups.TryGetValue(parentId, out var parent))
			{
				parent = AddMissingGroup(parentId, ownerType);
				parent.Parent = Root;
				Root.Children.Add(parent);
			}
			if (info.Parent != parent)
			{
				info.Parent?.Children.Remove(info);
				parent.Children.Add(info);
				info.Parent = parent;
			}
		}

		public static void ToMutableInfo(InspectorPropertyInfo info, MutablePropertyInfo parent)
		{
			MutablePropertyInfo mutableInfo = MutablePropertyInfoPool.Rent(info);
			mutableInfo.Parent = parent;
			mutableInfo.IsAddedByCode = true;
			parent.Children.Add(mutableInfo);
			if (info.PropertyType != PropertyType.Group)
			{
				string key;
				if (info.PropertyType == PropertyType.Method)
				{
					MemberInfo member = info.GetMemberInfo();
					if (member is MethodInfo method)
					{
						MethodInfo method2 = DesignerUtils.GetOpenGenericVariantOrSelf(method);
						key = DesignerUtils.CreateSerializedMethodName(method2);
					}
					else
					{
						key = info.PropertyName;
					}
				}
				else
				{
					key = info.PropertyName;
				}
				Properties[key] = mutableInfo;
				FormerlySerializedAsAttribute formerlySerialized = info.GetAttribute<FormerlySerializedAsAttribute>();
				if (formerlySerialized != null && !string.IsNullOrEmpty(formerlySerialized.oldName))
				{
					PropertiesFormerSerialized[formerlySerialized.oldName] = mutableInfo;
				}
			}
			else
			{
				string id = ((PropertyGroupAttribute)info.attributes[0]).GroupID;
				Groups[id] = mutableInfo;
				for (int i = 0; i < info.groupInfos.Length; i++)
				{
					ToMutableInfo(info.groupInfos[i], mutableInfo);
				}
			}
		}

		public static MutablePropertyInfo AddMissingGroup(string id, Type typeOfOwner)
		{
			InspectorPropertyInfo info = new InspectorPropertyInfo(0f, typeOfOwner, null, PropertyType.Group, SerializationBackend.None, isEditable: false)
			{
				DesignerId = id,
				attributes = new List<Attribute>(1)
			};
			MutablePropertyInfo mutableInfo = MutablePropertyInfoPool.Rent(info);
			Groups[id] = mutableInfo;
			return mutableInfo;
		}
	}
}
