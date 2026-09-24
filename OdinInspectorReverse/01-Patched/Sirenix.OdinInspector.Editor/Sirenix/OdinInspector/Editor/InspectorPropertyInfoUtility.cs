using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Sirenix.OdinInspector.Editor.Internal;
using Sirenix.OdinInspector.Internal;
using Sirenix.Serialization;
using Sirenix.Utilities;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

namespace Sirenix.OdinInspector.Editor
{
	public static class InspectorPropertyInfoUtility
	{
		private struct GroupDataAndInfo
		{
			public GroupData Data;

			public InspectorPropertyInfo Info;
		}

		private struct GroupAttributeInfo
		{
			public InspectorPropertyInfo InspectorPropertyInfo;

			public PropertyGroupAttribute Attribute;

			public bool Exclude;
		}

		private class GroupData
		{
			public string Name;

			public string ID;

			public GroupData Parent;

			public PropertyGroupAttribute ConsolidatedAttribute;

			public List<GroupAttributeInfo> Attributes = new List<GroupAttributeInfo>();

			public readonly List<GroupData> ChildGroups = new List<GroupData>();
		}

		private static readonly Dictionary<Type, bool> TypeDefinesShowOdinSerializedPropertiesInInspectorAttribute_Cache = new Dictionary<Type, bool>(FastTypeComparer.Instance);

		private static readonly HashSet<string> AlwaysSkipUnityProperties = new HashSet<string>
		{
			"m_PathID", "m_FileID", "m_ObjectHideFlags", "m_SerializedDataModeController", "m_PrefabParentObject", "m_PrefabInternal", "m_PrefabInternal", "m_GameObject", "m_Enabled", "m_Script",
			"m_EditorHideFlags", "m_EditorClassIdentifier"
		};

		private static Type System_Object_Type = typeof(object);

		private static Type UnityEngine_Object_Type = typeof(UnityEngine.Object);

		private static Type UnityEngine_Component_Type = typeof(Component);

		private static Type UnityEngine_MonoBehaviour_Type = typeof(MonoBehaviour);

		private static Type UnityEngine_EditorWindow_Type = typeof(EditorWindow);

		private static Type UnityEngine_Behaviour_Type = typeof(Behaviour);

		private static Type UnityEngine_ScriptableObject_Type = typeof(ScriptableObject);

		private static readonly HashSet<string> AlwaysSkipUnityPropertiesForComponents = new HashSet<string> { "m_Name" };

		private static readonly DoubleLookupDictionary<Type, string, string> UnityPropertyMemberNameReplacements = new DoubleLookupDictionary<Type, string, string>
		{
			{
				typeof(Bounds),
				new Dictionary<string, string> { { "m_Extent", "m_Extents" } }
			},
			{
				typeof(LayerMask),
				new Dictionary<string, string> { { "m_Bits", "m_Mask" } }
			}
		};

		private static readonly Dictionary<Type, MemberInfo[]> TypeMembers_Cache = new Dictionary<Type, MemberInfo[]>(FastTypeComparer.Instance);

		private static readonly HashSet<Type> NeverProcessUnityPropertiesFor = new HashSet<Type>
		{
			typeof(Vector2),
			typeof(Vector3),
			typeof(Vector4),
			typeof(Vector2Int),
			typeof(Vector3Int),
			typeof(Matrix4x4),
			typeof(Color),
			typeof(Color32),
			typeof(Quaternion),
			typeof(AnimationCurve),
			typeof(Gradient),
			typeof(Coroutine)
		};

		private static readonly HashSet<Type> AlwaysSkipUnityPropertiesDeclaredBy = new HashSet<Type>
		{
			typeof(UnityEngine.Object),
			typeof(ScriptableObject),
			typeof(Component),
			typeof(Behaviour),
			typeof(MonoBehaviour),
			typeof(StateMachineBehaviour)
		};

		private static readonly List<Attribute> ChildProcessedAttributes = new List<Attribute>();

		private static readonly HashSet<string> ExistingPropertyNames_Cache = new HashSet<string>();

		private static readonly Dictionary<InspectorPropertyInfo, float> GroupMemberOrders_Cached = new Dictionary<InspectorPropertyInfo, float>();

		private static readonly Dictionary<string, GroupData> GroupTree_Cached = new Dictionary<string, GroupData>();

		private static readonly Dictionary<InspectorPropertyInfo, InspectorPropertyInfo> RemovedMembers_Cached = new Dictionary<InspectorPropertyInfo, InspectorPropertyInfo>();

		private static Dictionary<SerializationBackend, Dictionary<Type, List<InspectorPropertyInfo>>> UnityPropertyInfoCache = new Dictionary<SerializationBackend, Dictionary<Type, List<InspectorPropertyInfo>>>();

		/// <summary>
		/// Gets all <see cref="T:Sirenix.OdinInspector.Editor.InspectorPropertyInfo" />s for a given type.
		/// </summary>
		/// <param name="parentProperty">The parent property.</param>
		/// <param name="type">The type to get infos for.</param>
		/// <param name="includeSpeciallySerializedMembers">if set to true members that are serialized by Odin will be included.</param>
		public static InspectorPropertyInfo[] GetDefaultPropertiesForType(InspectorProperty parentProperty, Type type, bool includeSpeciallySerializedMembers)
		{
			if (type == null)
			{
				throw new ArgumentNullException("type");
			}
			return CreateDefaultInspectorProperties(parentProperty, type, includeSpeciallySerializedMembers);
		}

		private static T Find<T>(this IList<Attribute> attributes) where T : Attribute
		{
			for (int i = 0; i < attributes.Count; i++)
			{
				if (attributes[i] is T)
				{
					return attributes[i] as T;
				}
			}
			return null;
		}

		private static bool Contains<T>(this IList<Attribute> attributes) where T : Attribute
		{
			for (int i = 0; i < attributes.Count; i++)
			{
				if (attributes[i] is T)
				{
					return true;
				}
			}
			return false;
		}

		public static bool TryCreate(InspectorProperty parentProperty, MemberInfo member, bool includeSpeciallySerializedMembers, out InspectorPropertyInfo result)
		{
			if (ChildProcessedAttributes.Count > 0)
			{
				ChildProcessedAttributes.Clear();
			}
			List<Attribute> attributes = ChildProcessedAttributes;
			ProcessAttributes(parentProperty, member, attributes);
			if (parentProperty.IsDesigned)
			{
				return DesignerInspectorPropertyInfoUtility.TryCreate(parentProperty, member, attributes, out result);
			}
			CheckForShowHideAttributes(attributes, out var hasShowAttribute, out var hasHideAttribute);
			if (hasHideAttribute && !hasShowAttribute)
			{
				result = null;
				return false;
			}
			SerializationBackend backend;
			bool isVisibleByDefault = IsMemberVisibleByDefault(parentProperty, member, out backend);
			if (isVisibleByDefault || hasShowAttribute)
			{
				return TryCreate(member, backend, allowEditable: true, out result, attributes, isDesignerTree: false);
			}
			result = null;
			return false;
		}

		internal static void CheckForShowHideAttributes(List<Attribute> attributes, out bool hasShowAttribute, out bool hasHideAttribute)
		{
			hasShowAttribute = false;
			hasHideAttribute = false;
			for (int i = 0; i < attributes.Count; i++)
			{
				Attribute attribute = attributes[i];
				if (attribute is ShowInInspectorAttribute)
				{
					hasShowAttribute = true;
				}
				else if (attribute is HideInInspector)
				{
					hasHideAttribute = true;
				}
			}
		}

		internal static bool IsMemberVisibleByDefault(InspectorProperty parentProperty, MemberInfo member, out SerializationBackend backend)
		{
			if (member.IsStatic())
			{
				backend = SerializationBackend.None;
				return false;
			}
			SerializationBackend parentBackend = GetSerializationBackendOfProperty(parentProperty);
			backend = GetSerializationBackend(parentProperty, member, parentBackend);
			if (parentBackend == SerializationBackend.None)
			{
				SerializationBackend potentialBackend = GetSerializationBackend(parentProperty, member, SerializationBackend.Odin);
				if (potentialBackend != SerializationBackend.None)
				{
					return true;
				}
			}
			return backend != SerializationBackend.None;
		}

		private static List<Attribute> CopyList(List<Attribute> attributes)
		{
			int count = attributes.Count;
			List<Attribute> result = new List<Attribute>(count);
			for (int i = 0; i < count; i++)
			{
				result.Add(attributes[i]);
			}
			return result;
		}

		internal static bool TryCreate(MemberInfo member, SerializationBackend backend, bool allowEditable, out InspectorPropertyInfo result, List<Attribute> attributes, bool isDesignerTree)
		{
			result = null;
			if (member is FieldInfo)
			{
				result = InspectorPropertyInfo.CreateForMember(member, allowEditable, backend, CopyList(attributes), isDesignerTree, null);
			}
			else if (member is PropertyInfo)
			{
				PropertyInfo propInfo = member as PropertyInfo;
				PropertyInfo nonAliasedPropInfo = propInfo.DeAliasProperty();
				bool valid = true;
				if (!nonAliasedPropInfo.CanRead || !propInfo.CanRead)
				{
					valid = false;
				}
				if (valid)
				{
					result = InspectorPropertyInfo.CreateForMember(member, allowEditable, backend, CopyList(attributes), isDesignerTree, null);
				}
			}
			else if (member is MethodInfo)
			{
				MethodInfo methodInfo = member as MethodInfo;
				if (methodInfo.IsGenericMethodDefinition)
				{
					return false;
				}
				result = InspectorPropertyInfo.CreateForMember(member, allowEditable: false, SerializationBackend.None, CopyList(attributes), isDesignerTree, null);
			}
			if (result != null)
			{
				PropertyOrderAttribute orderAttr = result.GetAttribute<PropertyOrderAttribute>();
				if (orderAttr != null)
				{
					result.Order = orderAttr.Order;
				}
				return true;
			}
			return false;
		}

		private static int GetMemberCategoryOrder(MemberInfo member)
		{
			if (member == null)
			{
				return 0;
			}
			if (member is FieldInfo)
			{
				return 0;
			}
			if (member is PropertyInfo)
			{
				return 1;
			}
			if (member is MethodInfo)
			{
				return 2;
			}
			return 3;
		}

		/// <summary>
		/// Gets an aliased version of a member, with the declaring type name included in the member name, so that there are no conflicts with private fields and properties with the same name in different classes in the same inheritance hierarchy.
		/// </summary>
		public static MemberInfo GetPrivateMemberAlias(MemberInfo member, string prefixString = null, string separatorString = null)
		{
			if (member is FieldInfo)
			{
				if (separatorString != null)
				{
					return new MemberAliasFieldInfo(member as FieldInfo, prefixString ?? member.DeclaringType.Name, separatorString);
				}
				return new MemberAliasFieldInfo(member as FieldInfo, prefixString ?? member.DeclaringType.Name);
			}
			if (member is PropertyInfo)
			{
				if (separatorString != null)
				{
					return new MemberAliasPropertyInfo(member as PropertyInfo, prefixString ?? member.DeclaringType.Name, separatorString);
				}
				return new MemberAliasPropertyInfo(member as PropertyInfo, prefixString ?? member.DeclaringType.Name);
			}
			if (member is MethodInfo)
			{
				if (separatorString != null)
				{
					return new MemberAliasMethodInfo(member as MethodInfo, prefixString ?? member.DeclaringType.Name, separatorString);
				}
				return new MemberAliasMethodInfo(member as MethodInfo, prefixString ?? member.DeclaringType.Name);
			}
			throw new NotImplementedException();
		}

		public static List<InspectorPropertyInfo> CreateMemberProperties(InspectorProperty parentProperty, Type type, bool includeSpeciallySerializedMembers)
		{
			List<InspectorPropertyInfo> rootProperties = new List<InspectorPropertyInfo>();
			AssemblyCategory assemblyFlag = AssemblyUtilities.GetAssemblyCategory(type.Assembly);
			if (assemblyFlag == AssemblyCategory.UnityEngine && !typeof(UnityEngine.Object).IsAssignableFrom(type) && !NeverProcessUnityPropertiesFor.Contains(type) && (!(UnityNetworkingUtility.SyncListType != null) || !type.ImplementsOpenGenericClass(UnityNetworkingUtility.SyncListType)) && !typeof(UnityAction).IsAssignableFrom(type) && !type.ImplementsOpenGenericClass(typeof(UnityAction<>)) && !type.ImplementsOpenGenericClass(typeof(UnityAction<, >)) && !type.ImplementsOpenGenericClass(typeof(UnityAction<, , >)) && !type.ImplementsOpenGenericClass(typeof(UnityAction<, , , >)))
			{
				PopulateUnityProperties(parentProperty, type, rootProperties);
			}
			if (rootProperties.Count == 0)
			{
				PopulateMemberInspectorProperties(parentProperty, type, includeSpeciallySerializedMembers, rootProperties);
			}
			rootProperties = (from n in rootProperties
				orderby n.Order, GetMemberCategoryOrder(n.GetMemberInfo())
				select n).ToList();
			if (parentProperty.Tree.IsMadeForDesignerEditor)
			{
				for (int i = 0; i < rootProperties.Count; i++)
				{
					rootProperties[i].DesignerIsFromCodeAndNotProcessor = true;
				}
			}
			return rootProperties;
		}

		public static InspectorPropertyInfo[] PerformAndBakePostGroupOrdering(List<InspectorPropertyInfo> rootProperties, Dictionary<InspectorPropertyInfo, float> groupMemberOrdering = null)
		{
			IOrderedEnumerable<InspectorPropertyInfo> result = rootProperties.OrderBy((InspectorPropertyInfo n) => (n.PropertyType == PropertyType.Group && n.Order == 0f) ? (FindFirstMemberOfGroup(n)?.Order ?? 0f) : n.Order);
			if (groupMemberOrdering != null)
			{
				result = result.ThenBy((InspectorPropertyInfo n) => groupMemberOrdering[n]);
			}
			return result.ThenBy((InspectorPropertyInfo n) => GetMemberCategoryOrder(n.GetMemberInfo())).ToArray();
		}

		internal static void PerformPostGroupOrdering(List<InspectorPropertyInfo> rootProperties, Dictionary<InspectorPropertyInfo, float> groupMemberOrdering = null)
		{
			if (groupMemberOrdering == null)
			{
				rootProperties.Sort(delegate(InspectorPropertyInfo lhs, InspectorPropertyInfo rhs)
				{
					float infoOrderForPostGroupOrdering = GetInfoOrderForPostGroupOrdering(lhs);
					float infoOrderForPostGroupOrdering2 = GetInfoOrderForPostGroupOrdering(rhs);
					int num = infoOrderForPostGroupOrdering.CompareTo(infoOrderForPostGroupOrdering2);
					if (num != 0)
					{
						return num;
					}
					int memberCategoryOrder = GetMemberCategoryOrder(lhs.GetMemberInfo());
					int memberCategoryOrder2 = GetMemberCategoryOrder(rhs.GetMemberInfo());
					return memberCategoryOrder.CompareTo(memberCategoryOrder2);
				});
				return;
			}
			rootProperties.Sort(delegate(InspectorPropertyInfo lhs, InspectorPropertyInfo rhs)
			{
				float infoOrderForPostGroupOrdering = GetInfoOrderForPostGroupOrdering(lhs);
				float infoOrderForPostGroupOrdering2 = GetInfoOrderForPostGroupOrdering(rhs);
				int num = infoOrderForPostGroupOrdering.CompareTo(infoOrderForPostGroupOrdering2);
				if (num != 0)
				{
					return num;
				}
				float num2 = groupMemberOrdering[lhs];
				float value = groupMemberOrdering[rhs];
				num = num2.CompareTo(value);
				if (num != 0)
				{
					return num;
				}
				int memberCategoryOrder = GetMemberCategoryOrder(lhs.GetMemberInfo());
				int memberCategoryOrder2 = GetMemberCategoryOrder(rhs.GetMemberInfo());
				return memberCategoryOrder.CompareTo(memberCategoryOrder2);
			});
		}

		private static float GetInfoOrderForPostGroupOrdering(InspectorPropertyInfo info)
		{
			if (info.PropertyType == PropertyType.Group && info.Order == 0f)
			{
				return FindFirstMemberOfGroup(info)?.Order ?? 0f;
			}
			return info.Order;
		}

		private static InspectorPropertyInfo[] CreateDefaultInspectorProperties(InspectorProperty parentProperty, Type type, bool includeSpeciallySerializedMembers)
		{
			List<InspectorPropertyInfo> rootProperties = CreateMemberProperties(parentProperty, type, includeSpeciallySerializedMembers);
			Dictionary<InspectorPropertyInfo, float> groupMemberOrders = GroupMemberOrders_Cached;
			BuildPropertyGroups(parentProperty, type, rootProperties, includeSpeciallySerializedMembers, ref groupMemberOrders);
			InspectorPropertyInfo[] result = PerformAndBakePostGroupOrdering(rootProperties, groupMemberOrders);
			if (groupMemberOrders.Count > 0)
			{
				groupMemberOrders.Clear();
			}
			return result;
		}

		private static InspectorPropertyInfo FindFirstMemberOfGroup(InspectorPropertyInfo groupInfo)
		{
			for (int i = 0; i < groupInfo.GetGroupInfos().Length; i++)
			{
				InspectorPropertyInfo info = groupInfo.GetGroupInfos()[i];
				if (info.PropertyType == PropertyType.Group)
				{
					InspectorPropertyInfo result = FindFirstMemberOfGroup(info);
					if (result != null)
					{
						return result;
					}
					continue;
				}
				return info;
			}
			return null;
		}

		public static InspectorPropertyInfo[] BuildPropertyGroupsAndFinalize(InspectorProperty parentProperty, Type typeOfOwner, List<InspectorPropertyInfo> rootMemberProperties, bool includeSpeciallySerializedMembers)
		{
			if (rootMemberProperties == null || rootMemberProperties.Count == 0)
			{
				return Array.Empty<InspectorPropertyInfo>();
			}
			ExistingPropertyNames_Cache.Clear();
			for (int i = 0; i < rootMemberProperties.Count; i++)
			{
				if (!ExistingPropertyNames_Cache.Add(rootMemberProperties[i].PropertyName))
				{
					Debug.LogError($"Property with the name '{rootMemberProperties[i].PropertyName}' is already added at '{parentProperty.Path}' for type '{parentProperty.Info.TypeOfValue}'.");
					rootMemberProperties.RemoveAt(i--);
				}
			}
			ulong hash;
			bool canResultBeCached;
			if (!parentProperty.Tree.IsMadeForDesignerEditor)
			{
				hash = DesignerInspectorPropertyInfoUtility.CalcHash(rootMemberProperties);
				if (FinalizedInspectorInfoCache.TryGet(parentProperty, hash, out var cachedInfos))
				{
					return cachedInfos;
				}
				canResultBeCached = FinalizedInspectorInfoCache.CanBeCached(parentProperty, hash, rootMemberProperties);
			}
			else
			{
				canResultBeCached = false;
				hash = 0uL;
			}
			for (int j = 0; j < rootMemberProperties.Count; j++)
			{
				rootMemberProperties[j].UpdateOrderFromAttributes();
			}
			Dictionary<InspectorPropertyInfo, float> groupMemberOrders = GroupMemberOrders_Cached;
			TypePatch typePatch = null;
			if (parentProperty.IsDesigned)
			{
				typePatch = TypePatchCache.Get(parentProperty);
			}
			InspectorPropertyInfo[] result;
			if (typePatch != null)
			{
				if (parentProperty.Tree.IsMadeForDesignerEditor)
				{
					for (int k = 0; k < rootMemberProperties.Count; k++)
					{
						InspectorPropertyInfo info = rootMemberProperties[k];
						info.DesignerAttributesFromCode = new HashSet<Type>();
						for (int l = 0; l < info.attributes.Count; l++)
						{
							Attribute attribute = info.attributes[l];
							if (attribute != null)
							{
								info.DesignerAttributesFromCode.Add(attribute.GetType());
							}
						}
					}
				}
				BuildPropertyGroups(parentProperty, typeOfOwner, rootMemberProperties, includeSpeciallySerializedMembers, ref groupMemberOrders);
				PerformPostGroupOrdering(rootMemberProperties, groupMemberOrders);
				DesignerInspectorPropertyInfoUtility.ApplyPatch(typePatch, parentProperty, rootMemberProperties);
				result = rootMemberProperties.ToArray();
			}
			else
			{
				BuildPropertyGroups(parentProperty, typeOfOwner, rootMemberProperties, includeSpeciallySerializedMembers, ref groupMemberOrders);
				result = PerformAndBakePostGroupOrdering(rootMemberProperties, groupMemberOrders);
			}
			if (groupMemberOrders.Count > 0)
			{
				groupMemberOrders.Clear();
			}
			if (canResultBeCached)
			{
				FinalizedInspectorInfoCache.Add(parentProperty, hash, result);
			}
			return result;
		}

		public static void BuildPropertyGroups(InspectorProperty parentProperty, Type typeOfOwner, List<InspectorPropertyInfo> rootMemberProperties, bool includeSpeciallySerializedMembers, ref Dictionary<InspectorPropertyInfo, float> groupMemberOrders)
		{
			if (rootMemberProperties.Count == 0)
			{
				return;
			}
			if (groupMemberOrders == null)
			{
				groupMemberOrders = new Dictionary<InspectorPropertyInfo, float>(rootMemberProperties.Count);
			}
			else if (groupMemberOrders.Count > 0)
			{
				groupMemberOrders.Clear();
			}
			for (int i = 0; i < rootMemberProperties.Count; i++)
			{
				groupMemberOrders.Add(rootMemberProperties[i], i);
			}
			Dictionary<InspectorPropertyInfo, float> lambdaRefMemberOrders = groupMemberOrders;
			Dictionary<string, GroupData> groupTree = GroupTree_Cached;
			if (groupTree.Count > 0)
			{
				groupTree.Clear();
			}
			for (int j = 0; j < rootMemberProperties.Count; j++)
			{
				InspectorPropertyInfo propInfo = rootMemberProperties[j];
				ImmutableList<Attribute> attributes = propInfo.Attributes;
				for (int k = 0; k < attributes.Count; k++)
				{
					if (attributes[k] is PropertyGroupAttribute attr)
					{
						RegisterGroupAttribute(propInfo, attr, groupTree);
					}
				}
			}
			List<string> toRemove = new List<string>();
			foreach (KeyValuePair<string, GroupData> entry in groupTree)
			{
				if (!ProcessGroups(entry.Value, groupTree))
				{
					toRemove.Add(entry.Key);
				}
			}
			for (int l = 0; l < toRemove.Count; l++)
			{
				groupTree.Remove(toRemove[l]);
			}
			List<GroupDataAndInfo> groups = new List<GroupDataAndInfo>();
			foreach (GroupData groupData in groupTree.Values)
			{
				InspectorPropertyInfo info = CreatePropertyGroups(parentProperty, typeOfOwner, groupData, lambdaRefMemberOrders, includeSpeciallySerializedMembers);
				groups.Add(new GroupDataAndInfo
				{
					Data = groupData,
					Info = info
				});
			}
			if (groupTree.Count > 0)
			{
				groupTree.Clear();
			}
			Dictionary<InspectorPropertyInfo, InspectorPropertyInfo> removedMembers = RemovedMembers_Cached;
			if (removedMembers.Count > 0)
			{
				removedMembers.Clear();
			}
			for (int m = 0; m < groups.Count; m++)
			{
				GroupDataAndInfo group = groups[m];
				IOrderedEnumerable<InspectorPropertyInfo> members = from n in RecurseGroupMembers(@group.Data)
					orderby lambdaRefMemberOrders[n]
					select n;
				InspectorPropertyInfo firstMember = members.First();
				int index = rootMemberProperties.IndexOf(firstMember);
				string finalGroupName = "#" + group.Data.Name;
				int hiddenPropertyIndex = rootMemberProperties.FindIndex((InspectorPropertyInfo n) => n.PropertyName == finalGroupName);
				if (hiddenPropertyIndex >= 0)
				{
					InspectorPropertyInfo hiddenProperty = rootMemberProperties[hiddenPropertyIndex];
					if (TryHidePropertyWithGroup(parentProperty, hiddenProperty, group.Info, includeSpeciallySerializedMembers, out var newAliasForHiddenProperty))
					{
						rootMemberProperties[hiddenPropertyIndex] = newAliasForHiddenProperty;
						removedMembers[hiddenProperty] = group.Info;
						groupMemberOrders[newAliasForHiddenProperty] = groupMemberOrders[hiddenProperty];
					}
				}
				if (index >= 0)
				{
					removedMembers.Add(rootMemberProperties[index], group.Info);
					groupMemberOrders[group.Info] = groupMemberOrders[rootMemberProperties[index]];
					rootMemberProperties[index] = group.Info;
				}
				else
				{
					InspectorPropertyInfo removedByGroup = removedMembers[firstMember];
					index = rootMemberProperties.IndexOf(removedByGroup);
					rootMemberProperties.Insert(index + 1, group.Info);
					groupMemberOrders[group.Info] = groupMemberOrders[rootMemberProperties[index]] + 0.1f;
				}
			}
			for (int i2 = 0; i2 < groups.Count; i2++)
			{
				GroupDataAndInfo group2 = groups[i2];
				IEnumerable<InspectorPropertyInfo> members2 = RecurseGroupMembers(group2.Data);
				foreach (InspectorPropertyInfo member in members2)
				{
					if (!removedMembers.ContainsKey(member))
					{
						removedMembers.Add(member, group2.Info);
					}
					rootMemberProperties.Remove(member);
				}
			}
			if (removedMembers.Count > 0)
			{
				removedMembers.Clear();
			}
		}

		private static void RegisterGroupAttribute(InspectorPropertyInfo member, PropertyGroupAttribute attribute, Dictionary<string, GroupData> groupTree)
		{
			string[] path = attribute.GroupID.Split(new char[1] { '/' });
			string firstPathStep = path[0];
			if (!groupTree.TryGetValue(firstPathStep, out var currentGroup))
			{
				currentGroup = new GroupData();
				currentGroup.ID = firstPathStep;
				currentGroup.Name = firstPathStep;
				groupTree.Add(firstPathStep, currentGroup);
			}
			for (int i = 1; i < path.Length; i++)
			{
				string step = path[i];
				GroupData nextGroup = currentGroup.ChildGroups.FirstOrDefault((GroupData n) => n.Name == step);
				if (nextGroup == null)
				{
					nextGroup = new GroupData();
					nextGroup.ID = string.Join("/", path.Take(i + 1).ToArray());
					nextGroup.Name = step;
					nextGroup.Parent = currentGroup;
					currentGroup.ChildGroups.Add(nextGroup);
				}
				currentGroup = nextGroup;
			}
			GroupAttributeInfo info = new GroupAttributeInfo
			{
				InspectorPropertyInfo = member,
				Attribute = attribute
			};
			currentGroup.Attributes.Add(info);
		}

		private static bool ProcessGroups(GroupData groupData, Dictionary<string, GroupData> groupTree)
		{
			if (groupData.Attributes.Count == 0)
			{
				foreach (GroupData expectingGroup in from n in RecurseGroups(groupData)
					where n.Attributes.Count > 0
					select n)
				{
					foreach (GroupAttributeInfo attrInfo in expectingGroup.Attributes)
					{
						Debug.LogError("Group attribute '" + attrInfo.Attribute.GetType().Name + "' on member '" + attrInfo.InspectorPropertyInfo.PropertyName + "' expected a group with the name '" + groupData.Name + "' to exist in declaring type '" + attrInfo.InspectorPropertyInfo.TypeOfOwner.GetNiceName() + "'. Its ID was '" + expectingGroup.ID + "'.");
					}
				}
				return false;
			}
			string groupName = groupData.Name;
			for (int i = 0; i < groupName.Length; i++)
			{
				if (groupName[i] == '.')
				{
					Debug.LogError("Group name '" + groupData.Name + "' is invalid; group names or paths cannot contain '.'!");
					return false;
				}
			}
			groupData.ConsolidatedAttribute = FastDeepCopier.DeepCopy(groupData.Attributes[0].Attribute);
			Type groupAttrType = groupData.ConsolidatedAttribute.GetType();
			for (int i2 = 1; i2 < groupData.Attributes.Count; i2++)
			{
				GroupAttributeInfo attrInfo2 = groupData.Attributes[i2];
				if (attrInfo2.Attribute.GetType() != groupAttrType)
				{
					Debug.LogError("Cannot have group attributes of different types with the same group name, on the same type (or its inherited types): Group type mismatch: the group '" + groupData.ID + "' is expecting attributes of type '" + groupAttrType.Name + "', but got an attribute of type '" + attrInfo2.Attribute.GetType().Name + "' on the property '" + attrInfo2.InspectorPropertyInfo.TypeOfOwner.GetNiceName() + "." + attrInfo2.InspectorPropertyInfo.PropertyName + "'.");
					groupData.Attributes.RemoveAt(i2--);
				}
				else
				{
					try
					{
						groupData.ConsolidatedAttribute = groupData.ConsolidatedAttribute.Combine(attrInfo2.Attribute);
					}
					catch (Exception exception)
					{
						Debug.LogException(exception);
					}
				}
			}
			if (groupData.ConsolidatedAttribute is ISubGroupProviderAttribute subGroupProvider)
			{
				string[] groupPath = groupData.ID.Split(new char[1] { '/' });
				Dictionary<string, PropertyGroupAttribute> subGroupPaths = new Dictionary<string, PropertyGroupAttribute>();
				foreach (PropertyGroupAttribute subGroupAttribute in subGroupProvider.GetSubGroupAttributes())
				{
					string[] subGroupPath = subGroupAttribute.GroupID.Split(new char[1] { '/' });
					bool valid = true;
					if (subGroupPath.Length != groupPath.Length + 1)
					{
						valid = false;
					}
					if (valid)
					{
						for (int i3 = 0; i3 < groupPath.Length; i3++)
						{
							if (subGroupPath[i3] != groupPath[i3])
							{
								valid = false;
								break;
							}
						}
					}
					if (valid)
					{
						GroupData subGroupData = groupData.ChildGroups.FirstOrDefault((GroupData n) => n.Name == subGroupAttribute.GroupName);
						if (subGroupData == null)
						{
							subGroupData = new GroupData();
							subGroupData.ID = subGroupAttribute.GroupID;
							subGroupData.Name = subGroupAttribute.GroupName;
							subGroupData.Parent = groupData;
							groupData.ChildGroups.Add(subGroupData);
						}
						if (!subGroupPaths.ContainsKey(subGroupAttribute.GroupID))
						{
							subGroupPaths.Add(subGroupAttribute.GroupID, subGroupAttribute);
						}
						GroupAttributeInfo attrInfo3 = new GroupAttributeInfo
						{
							InspectorPropertyInfo = groupData.Attributes[0].InspectorPropertyInfo,
							Attribute = subGroupAttribute,
							Exclude = true
						};
						subGroupData.Attributes.Add(attrInfo3);
					}
					else
					{
						Debug.LogError("Subgroup '" + subGroupAttribute.GroupID + "' of type '" + subGroupAttribute.GetType().Name + "' for group '" + groupData.ID + "' of type '" + groupData.ConsolidatedAttribute.GetType().Name + "' must have an ID that starts with '" + groupData.ID + "' and continue one path step further.");
					}
				}
				for (int i4 = 0; i4 < groupData.Attributes.Count; i4++)
				{
					GroupAttributeInfo attrInfo4 = groupData.Attributes[i4];
					string newPath = subGroupProvider.RepathMemberAttribute(attrInfo4.Attribute);
					if (newPath == null || !(newPath != attrInfo4.Attribute.GroupID))
					{
						continue;
					}
					if (!subGroupPaths.ContainsKey(newPath))
					{
						Debug.LogError("Member '" + attrInfo4.InspectorPropertyInfo.PropertyName + "' of " + groupData.ConsolidatedAttribute.GetType().Name + " group '" + groupData.ID + "' was repathed to subgroup at path '" + newPath + "', but no such subgroup was defined.");
					}
					else
					{
						groupData.Attributes.RemoveAt(i4--);
						attrInfo4.Attribute = subGroupPaths[newPath];
						GroupData subGroup = groupData.ChildGroups.First((GroupData n) => n.ID == newPath);
						subGroup.Attributes.Add(attrInfo4);
					}
				}
			}
			for (int i5 = 0; i5 < groupData.ChildGroups.Count; i5++)
			{
				if (!ProcessGroups(groupData.ChildGroups[i5], groupTree))
				{
					groupData.ChildGroups.RemoveAt(i5);
					i5--;
				}
			}
			HashSet<string> memberNames = new HashSet<string>();
			for (int i6 = 0; i6 < groupData.Attributes.Count; i6++)
			{
				GroupAttributeInfo attrInfo5 = groupData.Attributes[i6];
				if (attrInfo5.InspectorPropertyInfo.PropertyType != PropertyType.Group && !attrInfo5.Exclude)
				{
					string name = attrInfo5.InspectorPropertyInfo.PropertyName;
					if (!memberNames.Add(name))
					{
						groupData.Attributes.RemoveAt(i6--);
					}
				}
			}
			return true;
		}

		private static InspectorPropertyInfo CreatePropertyGroups(InspectorProperty parentProperty, Type typeOfOwner, GroupData groupData, Dictionary<InspectorPropertyInfo, float> memberOrder, bool includeSpeciallySerializedMembers)
		{
			List<InspectorPropertyInfo> children = new List<InspectorPropertyInfo>();
			foreach (GroupAttributeInfo attrInfo in groupData.Attributes)
			{
				if (!attrInfo.Exclude)
				{
					children.Add(attrInfo.InspectorPropertyInfo);
				}
			}
			foreach (GroupData childGroupData in groupData.ChildGroups)
			{
				InspectorPropertyInfo childGroup = CreatePropertyGroups(parentProperty, typeOfOwner, childGroupData, memberOrder, includeSpeciallySerializedMembers);
				InspectorPropertyInfo firstMember = null;
				float currentMinFloatOrder = 0f;
				foreach (InspectorPropertyInfo member in RecurseGroupMembers(childGroupData))
				{
					float floatOrderValue = memberOrder[member];
					if (firstMember == null || floatOrderValue < currentMinFloatOrder)
					{
						currentMinFloatOrder = floatOrderValue;
						firstMember = member;
					}
				}
				int index = children.IndexOf(firstMember);
				if (index >= 0)
				{
					memberOrder[childGroup] = memberOrder[children[index]];
					children[index] = childGroup;
				}
				else
				{
					memberOrder[childGroup] = memberOrder[firstMember];
					children.Insert(0, childGroup);
				}
				string finalGroupName = "#" + childGroup.PropertyName;
				for (int i = 0; i < children.Count; i++)
				{
					InspectorPropertyInfo child = children[i];
					if (child != childGroup && child.PropertyName == finalGroupName && TryHidePropertyWithGroup(parentProperty, child, childGroup, includeSpeciallySerializedMembers, out var newAliasForHiddenProperty))
					{
						memberOrder[newAliasForHiddenProperty] = memberOrder[children[i]];
						children[i] = newAliasForHiddenProperty;
					}
				}
			}
			foreach (GroupData childGroupData2 in groupData.ChildGroups)
			{
				IEnumerable<InspectorPropertyInfo> members = RecurseGroupMembers(childGroupData2);
				foreach (InspectorPropertyInfo member2 in members)
				{
					children.Remove(member2);
				}
			}
			children.Sort(delegate(InspectorPropertyInfo a, InspectorPropertyInfo b)
			{
				int num = a.Order.CompareTo(b.Order);
				if (num != 0)
				{
					return num;
				}
				num = memberOrder[a].CompareTo(memberOrder[b]);
				return (num != 0) ? num : GetMemberCategoryOrder(a.GetMemberInfo()).CompareTo(GetMemberCategoryOrder(b.GetMemberInfo()));
			});
			float order = groupData.ConsolidatedAttribute.Order;
			if (order == 0f)
			{
				order = float.MaxValue;
				foreach (InspectorPropertyInfo member3 in RecurseGroupMembers(groupData))
				{
					if (member3.Order < order)
					{
						order = member3.Order;
					}
				}
			}
			return InspectorPropertyInfo.CreateGroup("#" + groupData.Name, typeOfOwner, order, children.ToArray(), new List<Attribute> { groupData.ConsolidatedAttribute });
		}

		private static bool TryHidePropertyWithGroup(InspectorProperty parentProperty, InspectorPropertyInfo hidden, InspectorPropertyInfo group, bool includeSpeciallySerializedMembers, out InspectorPropertyInfo newAliasForHiddenProperty)
		{
			if (hidden.PropertyType == PropertyType.Group)
			{
				string newGroupName = group.TypeOfOwner.GetNiceName() + "." + group.PropertyName;
				string oldGroupName = hidden.TypeOfOwner.GetNiceName() + "." + hidden.PropertyName;
				Debug.LogWarning("Property group '" + newGroupName + "' conflicts with already existing group property '" + oldGroupName + "'. Group property '" + newGroupName + "' will be removed from the property tree.");
				newAliasForHiddenProperty = null;
				return false;
			}
			if (hidden.GetMemberInfo() != null)
			{
				MemberInfo alias = GetPrivateMemberAlias(hidden.GetMemberInfo(), hidden.TypeOfOwner.GetNiceName(), " -> ");
				string aliasName = alias.Name;
				string groupName = group.TypeOfOwner.GetNiceName() + "." + group.PropertyName;
				string hiddenPropertyName = hidden.TypeOfOwner.GetNiceName() + "." + hidden.PropertyName;
				if (TryCreate(parentProperty, alias, includeSpeciallySerializedMembers, out newAliasForHiddenProperty))
				{
					Debug.LogWarning("Property group '" + groupName + "' hides member property '" + hiddenPropertyName + "'. Alias property '" + aliasName + "' created for member property '" + hiddenPropertyName + "'.");
					return true;
				}
				Debug.LogWarning("Property group '" + groupName + "' tries to hide member property '" + hiddenPropertyName + "', but failed to create alias property '" + aliasName + "' for member property '" + hiddenPropertyName + "'; group property '" + groupName + "' will be removed.");
				return false;
			}
			newAliasForHiddenProperty = null;
			return false;
		}

		private static IEnumerable<GroupData> RecurseGroups(GroupData groupData)
		{
			yield return groupData;
			for (int i = 0; i < groupData.ChildGroups.Count; i++)
			{
				GroupData childGroup = groupData.ChildGroups[i];
				foreach (GroupData item in RecurseGroups(childGroup))
				{
					yield return item;
				}
			}
		}

		private static IEnumerable<InspectorPropertyInfo> RecurseGroupMembers(GroupData groupData)
		{
			for (int i = 0; i < groupData.Attributes.Count; i++)
			{
				yield return groupData.Attributes[i].InspectorPropertyInfo;
			}
			for (int i = 0; i < groupData.ChildGroups.Count; i++)
			{
				GroupData childGroup = groupData.ChildGroups[i];
				foreach (GroupData child in RecurseGroups(childGroup))
				{
					for (int j = 0; j < child.Attributes.Count; j++)
					{
						yield return child.Attributes[j].InspectorPropertyInfo;
					}
				}
			}
		}

		private static void PopulateUnityProperties(InspectorProperty parentProperty, Type type, List<InspectorPropertyInfo> result)
		{
			SerializationBackend parentBackend = GetSerializationBackendOfProperty(parentProperty);
			if (!UnityPropertyInfoCache.TryGetValue(parentBackend, out var innerDict))
			{
				innerDict = new Dictionary<Type, List<InspectorPropertyInfo>>(FastTypeComparer.Instance);
				UnityPropertyInfoCache.Add(parentBackend, innerDict);
			}
			if (!innerDict.TryGetValue(type, out var unityProperties))
			{
				unityProperties = new List<InspectorPropertyInfo>();
				FindUnityProperties(parentProperty, type, unityProperties);
				innerDict.Add(type, unityProperties);
			}
			int count = unityProperties.Count;
			for (int i = 0; i < count; i++)
			{
				InspectorPropertyInfo copy = unityProperties[i].CreateCopy();
				result.Add(copy);
			}
		}

		private static void FindUnityProperties(InspectorProperty parentProperty, Type type, List<InspectorPropertyInfo> result)
		{
			bool isDesignerTree = parentProperty.Tree != null && parentProperty.Tree.IsDesignerTree;
			SerializedProperty prop = null;
			if (type.IsAbstract || type.IsInterface || type.IsArray)
			{
				return;
			}
			UnityEngine.Object toDestroy = null;
			try
			{
				if (typeof(Component).IsAssignableFrom(type))
				{
					GameObject go = new GameObject("Odin_UnityPropertyExtractor_Temp (If you see this in the hierarchy, something went badly wrong)");
					toDestroy = go;
					Component component = ((!type.IsAssignableFrom(typeof(Transform))) ? go.AddComponent(type) : go.transform);
					SerializedObject obj = new SerializedObject(component);
					prop = obj.GetIterator();
				}
				else if (typeof(ScriptableObject).IsAssignableFrom(type))
				{
					ScriptableObject scriptableObject = ScriptableObject.CreateInstance(type);
					toDestroy = scriptableObject;
					SerializedObject obj2 = new SerializedObject(scriptableObject);
					prop = obj2.GetIterator();
				}
				else if (UnityVersion.IsVersionOrGreater(2017, 1))
				{
					GameObject go2 = new GameObject("Odin_EmittedUnityPropertyExtractor_Temp (If you see this in the hierarchy, something went badly wrong)");
					toDestroy = go2;
					try
					{
						UnityPropertyEmitter.Handle handle = UnityPropertyEmitter.CreateEmittedMonoBehaviourProperty("InspectorPropertyInfo_UnityPropertyExtractor", type, 1, ref go2);
						prop = handle.UnityProperty;
					}
					catch (Exception)
					{
						Debug.LogError("Failed to emit and instantiate a MonoBehaviour host object to extract Unity-serialized properties to display for the type '" + type.GetNiceName() + "'. This is not necessarily related to the " + type.GetNiceName() + " type being at fault, instead it can indicate that the project is in an invalid or faulty state, with corrupt assemblies or bad cached Library state. Try reimporting all assets or deleting the Library, fixing compiler errors, and verifying that all assemblies used and imported in the project are functional; reinstalling Odin and other plugins that make use of pre-compiled assemblies may help.");
					}
				}
				else
				{
					prop = UnityPropertyEmitter.CreateEmittedScriptableObjectProperty("InspectorPropertyInfo_UnityPropertyExtractor", type, 1);
					if (prop != null)
					{
						toDestroy = prop.serializedObject.targetObject;
					}
				}
				if (prop == null || !prop.Next(enterChildren: true))
				{
					return;
				}
				List<MemberInfo> members = (from n in type.GetAllMembers(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
					where (n is FieldInfo || n is PropertyInfo) && !AlwaysSkipUnityPropertiesDeclaredBy.Contains(n.DeclaringType)
					select n).ToList();
				do
				{
					if (AlwaysSkipUnityProperties.Contains(prop.name) || (typeof(Component).IsAssignableFrom(type) && AlwaysSkipUnityPropertiesForComponents.Contains(prop.name)))
					{
						continue;
					}
					string memberName = prop.name;
					if (UnityPropertyMemberNameReplacements.ContainsKeys(type, memberName))
					{
						memberName = UnityPropertyMemberNameReplacements[type][memberName];
					}
					MemberInfo member = members.FirstOrDefault((MemberInfo n) => n.Name == memberName || n.Name == prop.name);
					if (member == null)
					{
						string propName = prop.displayName.Replace(" ", "");
						bool changedPropName = false;
						if (string.Equals(propName, "material", StringComparison.InvariantCultureIgnoreCase))
						{
							changedPropName = true;
							propName = "sharedMaterial";
						}
						else if (string.Equals(propName, "mesh", StringComparison.InvariantCultureIgnoreCase))
						{
							changedPropName = true;
							propName = "sharedMesh";
						}
						member = members.FirstOrDefault((MemberInfo n) => string.Equals(n.Name, propName, StringComparison.InvariantCultureIgnoreCase) && prop.IsCompatibleWithType(n.GetReturnType()));
						if (changedPropName && member == null)
						{
							propName = prop.displayName.Replace(" ", "");
							member = members.FirstOrDefault((MemberInfo n) => string.Equals(n.Name, propName, StringComparison.InvariantCultureIgnoreCase) && prop.IsCompatibleWithType(n.GetReturnType()));
						}
					}
					if (member == null)
					{
						string propName2 = prop.displayName;
						List<MemberInfo> possibles = members.Where((MemberInfo n) => (StringExtensions.Contains(propName2, n.Name, StringComparison.InvariantCultureIgnoreCase) || StringExtensions.Contains(n.Name, propName2, StringComparison.InvariantCultureIgnoreCase)) && prop.IsCompatibleWithType(n.GetReturnType())).ToList();
						if (possibles.Count == 1)
						{
							member = possibles[0];
						}
					}
					if (member == null)
					{
						Type valueType = prop.GuessContainedType();
						if (valueType != null && SerializedPropertyUtilities.CanSetGetValue(valueType))
						{
							result.Add(InspectorPropertyInfo.CreateForUnityProperty(prop.name, type, valueType, prop.editable, (Attribute[])null));
							continue;
						}
					}
					if (member == null)
					{
						if (!(prop.name == "Array") || prop.propertyType != SerializedPropertyType.Generic)
						{
							Debug.LogWarning("Failed to find corresponding member for Unity property '" + prop.name + "/" + prop.displayName + "' on type " + type.GetNiceName() + ", and cannot alias a Unity property of type '" + prop.propertyType.ToString() + "/" + prop.type + "'. This property will be missing in the inspector.");
						}
					}
					else
					{
						members.Remove(member);
						List<Attribute> attributes = new List<Attribute>();
						ProcessAttributes(parentProperty, member, attributes);
						if (TryCreate(member, GetSerializationBackend(parentProperty, member), prop.editable, out var info, attributes, isDesignerTree))
						{
							info.PropertyName = prop.name;
							result.Add(info);
						}
					}
				}
				while (prop.Next(enterChildren: false));
			}
			catch (InvalidOperationException)
			{
			}
			finally
			{
				if (toDestroy != null)
				{
					UnityEngine.Object.DestroyImmediate(toDestroy);
				}
			}
		}

		private static void PopulateMemberInspectorProperties(InspectorProperty parentProperty, Type type, bool includeSpeciallySerializedMembers, List<InspectorPropertyInfo> properties)
		{
			if (type.IsPrimitive || type == typeof(string))
			{
				return;
			}
			Type baseType = type.BaseType;
			if ((object)baseType != null && (object)baseType != System_Object_Type && (object)baseType != UnityEngine_Object_Type && (object)baseType != UnityEngine_Component_Type && (object)baseType != UnityEngine_MonoBehaviour_Type && (object)baseType != UnityEngine_EditorWindow_Type && (object)baseType != UnityEngine_Behaviour_Type && (object)baseType != UnityEngine_ScriptableObject_Type)
			{
				PopulateMemberInspectorProperties(parentProperty, baseType, includeSpeciallySerializedMembers, properties);
			}
			if (!TypeMembers_Cache.TryGetValue(type, out var members))
			{
				members = type.GetMembers(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
				TypeMembers_Cache.Add(type, members);
			}
			foreach (MemberInfo member in members)
			{
				if (!(member is FieldInfo) && !(member is PropertyInfo) && !(member is MethodInfo))
				{
					continue;
				}
				if (member is PropertyInfo)
				{
					PropertyInfo pi = member as PropertyInfo;
					ParameterInfo[] parameters = pi.GetIndexParameters();
					if (parameters.Length != 0)
					{
						continue;
					}
				}
				if (!TryCreate(parentProperty, member, includeSpeciallySerializedMembers, out var info))
				{
					continue;
				}
				InspectorPropertyInfo previousPropertyWithName = null;
				int previousPropertyIndex = -1;
				for (int j = 0; j < properties.Count; j++)
				{
					if (properties[j].PropertyName == info.PropertyName)
					{
						previousPropertyIndex = j;
						previousPropertyWithName = properties[j];
						break;
					}
				}
				if (previousPropertyWithName != null)
				{
					bool createAlias = true;
					if (member.SignaturesAreEqual(previousPropertyWithName.GetMemberInfo()))
					{
						createAlias = false;
						properties.RemoveAt(previousPropertyIndex);
					}
					if (createAlias)
					{
						MemberInfo alias = GetPrivateMemberAlias(previousPropertyWithName.GetMemberInfo(), previousPropertyWithName.TypeOfOwner.GetNiceName(), " -> ");
						string aliasName = alias.Name;
						if (TryCreate(parentProperty, alias, includeSpeciallySerializedMembers, out var aliasedProperty))
						{
							properties[previousPropertyIndex] = aliasedProperty;
						}
						else
						{
							string hidden = info.TypeOfOwner.GetNiceName() + "." + info.GetMemberInfo().Name;
							string inherited = previousPropertyWithName.TypeOfOwner.GetNiceName() + "." + previousPropertyWithName.PropertyName;
							properties.RemoveAt(previousPropertyIndex);
						}
					}
				}
				properties.Add(info);
			}
		}

		private static SerializationBackend GetSerializationBackendOfProperty(InspectorProperty property)
		{
			if (property.ValueEntry == null)
			{
				property = property.ParentValueProperty ?? property;
			}
			return property.Info.SerializationBackend;
		}

		public static SerializationBackend GetSerializationBackend(InspectorProperty parentProperty, MemberInfo member)
		{
			return GetSerializationBackend(parentProperty, member, GetSerializationBackendOfProperty(parentProperty));
		}

		private static SerializationBackend GetSerializationBackend(InspectorProperty parentProperty, MemberInfo member, SerializationBackend parentBackend)
		{
			if (!(member is FieldInfo) && !(member is PropertyInfo))
			{
				return SerializationBackend.None;
			}
			if (parentProperty.ValueEntry == null)
			{
				parentProperty = parentProperty.ParentValueProperty ?? parentProperty;
			}
			InspectorProperty serializationRoot = ((parentProperty.ValueEntry == null || !typeof(UnityEngine.Object).IsAssignableFrom(parentProperty.ValueEntry.TypeOfValue)) ? parentProperty.SerializationRoot : parentProperty);
			if (serializationRoot.ValueEntry == null)
			{
				return SerializationBackend.None;
			}
			if (parentBackend == SerializationBackend.None && serializationRoot != parentProperty)
			{
				return SerializationBackend.None;
			}
			ISerializationPolicy policy = SerializationPolicies.Unity;
			IOverridesSerializationPolicy policyOverride = serializationRoot.ValueEntry.WeakValues[0] as IOverridesSerializationPolicy;
			if (parentBackend == SerializationBackend.Odin && policyOverride != null)
			{
				policy = policyOverride.SerializationPolicy ?? SerializationPolicies.Unity;
			}
			if (serializationRoot != parentProperty)
			{
				if (parentBackend == SerializationBackend.Odin)
				{
					if (!UnitySerializationUtility.OdinWillSerialize(member, serializeUnityFields: true, policy))
					{
						return SerializationBackend.None;
					}
					return SerializationBackend.Odin;
				}
				if (parentBackend.IsUnity)
				{
					if (SerializationBackend.UnityPolymorphic.CanSerializeMember(member))
					{
						return SerializationBackend.UnityPolymorphic;
					}
					if (!SerializationBackend.Unity.CanSerializeMember(member))
					{
						return SerializationBackend.None;
					}
					return SerializationBackend.Unity;
				}
				if (parentBackend.CanSerializeMember(member))
				{
					return parentBackend;
				}
				return SerializationBackend.None;
			}
			if (parentBackend == SerializationBackend.Odin)
			{
				bool serializeUnityFields = false;
				if (policyOverride != null)
				{
					serializeUnityFields = policyOverride.OdinSerializesUnityFields;
				}
				if (UnitySerializationUtility.OdinWillSerialize(member, serializeUnityFields, policy))
				{
					return SerializationBackend.Odin;
				}
				if (SerializationBackend.UnityPolymorphic.CanSerializeMember(member))
				{
					return SerializationBackend.UnityPolymorphic;
				}
				if (SerializationBackend.Unity.CanSerializeMember(member))
				{
					return SerializationBackend.Unity;
				}
			}
			else if (parentBackend.IsUnity)
			{
				if (SerializationBackend.UnityPolymorphic.CanSerializeMember(member))
				{
					return SerializationBackend.UnityPolymorphic;
				}
				if (SerializationBackend.Unity.CanSerializeMember(member))
				{
					return SerializationBackend.Unity;
				}
			}
			else if (parentBackend.CanSerializeMember(member))
			{
				return parentBackend;
			}
			return SerializationBackend.None;
		}

		public static void ProcessAttributes(InspectorProperty parentProperty, MemberInfo member, List<Attribute> attributes)
		{
			List<OdinAttributeProcessor> processors = parentProperty.Tree.AttributeProcessorLocator.GetChildProcessors(parentProperty, member);
			for (int i = 0; i < processors.Count; i++)
			{
				try
				{
					processors[i].ProcessChildMemberAttributes(parentProperty, member, attributes);
				}
				catch (Exception exception)
				{
					Debug.LogException(exception);
				}
			}
		}

		public static bool TypeDefinesShowOdinSerializedPropertiesInInspectorAttribute_Cached(Type type)
		{
			if (!TypeDefinesShowOdinSerializedPropertiesInInspectorAttribute_Cache.TryGetValue(type, out var result))
			{
				result = type.IsDefined(typeof(ShowOdinSerializedPropertiesInInspectorAttribute), inherit: true);
				TypeDefinesShowOdinSerializedPropertiesInInspectorAttribute_Cache.Add(type, result);
			}
			return result;
		}

		public static bool InspectorPropertySupportsAssigningSceneReferences(InspectorProperty property)
		{
			InspectorProperty root = property.SerializationRoot;
			Type valueType = ((!(property.ChildResolver is ICollectionResolver collectionResolver)) ? ((!(property.ChildResolver is IOrderedCollectionResolver orderedCollectionResolver)) ? property.ValueEntry.BaseValueType : orderedCollectionResolver.ElementType) : collectionResolver.ElementType);
			if (!valueType.IsInterface && !(valueType == typeof(GameObject)) && !typeof(Component).IsAssignableFrom(valueType) && !(valueType == typeof(UnityEngine.Object)) && !(valueType == typeof(object)))
			{
				return false;
			}
			bool allowSceneObjects;
			if (property.ValueEntry != null && property.ValueEntry.SerializationBackend == SerializationBackend.None)
			{
				allowSceneObjects = true;
			}
			else if (root.ValueEntry != null && !typeof(UnityEngine.Object).IsAssignableFrom(root.ValueEntry.TypeOfValue))
			{
				allowSceneObjects = true;
			}
			else if (root.ValueEntry == null || !(root.ValueEntry.WeakSmartValue is UnityEngine.Object uObj))
			{
				allowSceneObjects = root.ValueEntry != null && root.ValueEntry.WeakSmartValue is GameObject go && go != null && OdinPrefabUtility.IsPartOfPrefabStage(go);
			}
			else
			{
				GameObject go2 = ((uObj is GameObject) ? ((GameObject)uObj) : ((!(uObj is Component)) ? null : ((Component)uObj).gameObject));
				if (go2 != null)
				{
					PrefabKind kind = OdinPrefabUtility.GetPrefabKind(go2);
					allowSceneObjects = kind == PrefabKind.None || kind == PrefabKind.NonPrefabInstance || kind == PrefabKind.PrefabInstance || kind == PrefabKind.InstanceInScene || OdinPrefabUtility.IsPartOfPrefabStage(go2);
				}
				else
				{
					allowSceneObjects = true;
				}
			}
			if (allowSceneObjects && property.GetAttribute<AssetsOnlyAttribute>() != null)
			{
				allowSceneObjects = false;
			}
			return allowSceneObjects;
		}
	}
}
