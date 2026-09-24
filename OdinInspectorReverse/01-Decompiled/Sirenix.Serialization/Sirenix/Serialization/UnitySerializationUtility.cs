using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text;
using Sirenix.Serialization.Utilities;
using Sirenix.Utilities;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

namespace Sirenix.Serialization
{
	/// <summary>
	/// Provides an array of utility wrapper methods for easy serialization and deserialization of Unity objects of any type.
	/// Note that, during serialization, it is always assumed that we are running on Unity's main thread. Deserialization can
	/// happen on any thread, and all API's interacting with deserialization are thread-safe.
	/// <para />
	/// Note that setting the IndexReferenceResolver on contexts passed into methods on this class will have no effect, as it will always
	/// be set to a UnityReferenceResolver.
	/// </summary>
	public static class UnitySerializationUtility
	{
		private static class PrefabDeserializeUtility
		{
			private static int updateCount;

			[NonSerialized]
			public static readonly HashSet<UnityEngine.Object> PrefabsWithValuesApplied;

			[NonSerialized]
			private static readonly Dictionary<UnityEngine.Object, HashSet<object>> SceneObjectsToKeepOnApply;

			[NonSerialized]
			public static readonly object DeserializePrefabs_LOCK;

			private static readonly List<UnityEngine.Object> toRemove;

			static PrefabDeserializeUtility()
			{
				updateCount = 0;
				PrefabsWithValuesApplied = new HashSet<UnityEngine.Object>(Sirenix.Serialization.Utilities.ReferenceEqualityComparer<UnityEngine.Object>.Default);
				SceneObjectsToKeepOnApply = new Dictionary<UnityEngine.Object, HashSet<object>>(Sirenix.Serialization.Utilities.ReferenceEqualityComparer<UnityEngine.Object>.Default);
				DeserializePrefabs_LOCK = new object();
				toRemove = new List<UnityEngine.Object>();
				EditorApplication.update = (EditorApplication.CallbackFunction)Delegate.Combine(EditorApplication.update, new EditorApplication.CallbackFunction(OnEditorUpdate));
			}

			/// <summary>
			/// Note: it is assumed that code calling this is holding the DeserializePrefabCaches_LOCK lock, and will continue to hold it while the returned hashset is being modified
			/// </summary>
			public static HashSet<object> GetSceneObjectsToKeepSet(UnityEngine.Object unityObject, bool createIfDoesntExist)
			{
				if (!SceneObjectsToKeepOnApply.TryGetValue(unityObject, out var keep))
				{
					keep = new HashSet<object>(Sirenix.Serialization.Utilities.ReferenceEqualityComparer<object>.Default);
					SceneObjectsToKeepOnApply.Add(unityObject, keep);
				}
				return keep;
			}

			public static void CleanSceneObjectToKeepOnApply()
			{
				lock (DeserializePrefabs_LOCK)
				{
					foreach (UnityEngine.Object obj in SceneObjectsToKeepOnApply.Keys)
					{
						if (obj == null)
						{
							toRemove.Add(obj);
						}
					}
					for (int i = 0; i < toRemove.Count; i++)
					{
						SceneObjectsToKeepOnApply.Remove(toRemove[i]);
					}
					toRemove.Clear();
				}
			}

			private static void OnEditorUpdate()
			{
				lock (DeserializePrefabs_LOCK)
				{
					updateCount++;
					if (updateCount >= 1000)
					{
						SceneObjectsToKeepOnApply.Clear();
						updateCount = 0;
					}
				}
			}
		}

		private struct CachedSerializationBackendResult
		{
			public bool HasCalculatedSerializeUnityFieldsTrueResult;

			public bool HasCalculatedSerializeUnityFieldsFalseResult;

			public bool SerializeUnityFieldsTrueResult;

			public bool SerializeUnityFieldsFalseResult;
		}

		[InitializeOnLoad]
		private static class PrefabSelectionTracker
		{
			private static readonly object LOCK;

			private static readonly HashSet<UnityEngine.Object> selectedPrefabObjects;

			static PrefabSelectionTracker()
			{
				LOCK = new object();
				selectedPrefabObjects = new HashSet<UnityEngine.Object>(Sirenix.Serialization.Utilities.ReferenceEqualityComparer<UnityEngine.Object>.Default);
				Selection.selectionChanged = (Action)Delegate.Combine(Selection.selectionChanged, new Action(OnSelectionChanged));
				OnSelectionChanged();
			}

			public static bool IsCurrentlySelectedPrefabRoot(UnityEngine.Object obj)
			{
				lock (LOCK)
				{
					return selectedPrefabObjects.Contains(obj);
				}
			}

			private static void OnSelectionChanged()
			{
				lock (LOCK)
				{
					selectedPrefabObjects.Clear();
					IEnumerable<GameObject> rootPrefabs = (from n in Selection.objects.Where(delegate(UnityEngine.Object n)
						{
							if (!(n is GameObject))
							{
								return false;
							}
							PrefabType prefabType = PrefabUtility.GetPrefabType(n);
							return prefabType == PrefabType.Prefab || prefabType == PrefabType.ModelPrefab || prefabType == PrefabType.PrefabInstance || prefabType == PrefabType.ModelPrefabInstance;
						})
						select PrefabUtility.FindPrefabRoot((GameObject)n)).Distinct();
					foreach (GameObject root in rootPrefabs)
					{
						RegisterRecursive(root);
					}
				}
			}

			private static void RegisterRecursive(GameObject go)
			{
				selectedPrefabObjects.Add(go);
				Component[] components = go.GetComponents<Component>();
				for (int i = 0; i < components.Length; i++)
				{
					selectedPrefabObjects.Add(components[i]);
				}
				Transform transform = go.transform;
				for (int j = 0; j < transform.childCount; j++)
				{
					Transform child = transform.GetChild(j);
					RegisterRecursive(child.gameObject);
				}
			}
		}

		public static class PrefabModificationCache
		{
			private static readonly Dictionary<object, List<PrefabModification>> CachedDeserializedModifications = new Dictionary<object, List<PrefabModification>>(Sirenix.Serialization.Utilities.ReferenceEqualityComparer<object>.Default);

			private static readonly Dictionary<object, int> CachedDeserializedModificationTimes = new Dictionary<object, int>(Sirenix.Serialization.Utilities.ReferenceEqualityComparer<object>.Default);

			private static readonly object Caches_LOCK = new object();

			private static int counter = 0;

			public static List<PrefabModification> DeserializePrefabModificationsCached(UnityEngine.Object obj, List<string> modifications, List<UnityEngine.Object> referencedUnityObjects)
			{
				lock (Caches_LOCK)
				{
					if (!CachedDeserializedModifications.TryGetValue(obj, out var result))
					{
						result = DeserializePrefabModifications(modifications, referencedUnityObjects);
						CachedDeserializedModifications.Add(obj, result);
					}
					CachedDeserializedModificationTimes[obj] = ++counter;
					PrunePrefabModificationsCache();
					return result;
				}
			}

			public static void CachePrefabModifications(UnityEngine.Object obj, List<PrefabModification> modifications)
			{
				lock (Caches_LOCK)
				{
					CachedDeserializedModifications[obj] = modifications;
					CachedDeserializedModificationTimes[obj] = ++counter;
					PrunePrefabModificationsCache();
				}
			}

			private static void PrunePrefabModificationsCache()
			{
				if (CachedDeserializedModifications.Count != CachedDeserializedModificationTimes.Count)
				{
					CachedDeserializedModifications.Clear();
					CachedDeserializedModificationTimes.Clear();
				}
				int removeCount = CachedDeserializedModificationTimes.Count - 10;
				for (int i = 0; i < removeCount; i++)
				{
					object lowestObj = null;
					int lowestTime = int.MaxValue;
					foreach (KeyValuePair<object, int> pair in CachedDeserializedModificationTimes)
					{
						if (pair.Value < lowestTime)
						{
							lowestObj = pair.Key;
							lowestTime = pair.Value;
						}
					}
					CachedDeserializedModifications.Remove(lowestObj);
					if (!CachedDeserializedModificationTimes.Remove(lowestObj))
					{
						UnityEngine.Debug.LogError("A Unity object instance of type '" + Sirenix.Serialization.Utilities.TypeExtensions.GetNiceName(lowestObj.GetType()) + "' has likely become corrupt or destroyed somehow, yet deserialization has been invoked for it. If you're in the editor, you can click this log message to attempt to highlight the object. (It probably won't work, but there's a chance. If the highlighting doesn't work, the object instance is so broken that Odin cannot give you any more info about it than this message contains. Good luck!)", lowestObj as UnityEngine.Object);
						CachedDeserializedModifications.Clear();
						CachedDeserializedModificationTimes.Clear();
					}
				}
			}
		}

		public static readonly Type SerializeReferenceAttributeType = typeof(SerializeField).Assembly.GetType("UnityEngine.SerializeReference");

		private static readonly Assembly String_Assembly = typeof(string).Assembly;

		private static readonly Assembly HashSet_Assembly = typeof(HashSet<>).Assembly;

		private static readonly Assembly LinkedList_Assembly = typeof(LinkedList<>).Assembly;

		private static bool isDoingDomainReload;

		/// <summary>
		/// From the new scriptable build pipeline package
		/// </summary>
		[NonSerialized]
		private static readonly Type SBP_ContentPipelineType = TwoWaySerializationBinder.Default.BindToType("UnityEditor.Build.Pipeline.ContentPipeline");

		[NonSerialized]
		private static readonly MethodInfo PrefabUtility_IsComponentAddedToPrefabInstance_MethodInfo = typeof(PrefabUtility).GetMethod("IsComponentAddedToPrefabInstance");

		[NonSerialized]
		private static readonly HashSet<UnityEngine.Object> UnityObjectsWaitingForDelayedModificationApply = new HashSet<UnityEngine.Object>(Sirenix.Serialization.Utilities.ReferenceEqualityComparer<UnityEngine.Object>.Default);

		[NonSerialized]
		private static readonly Dictionary<UnityEngine.Object, List<PrefabModification>> RegisteredPrefabModifications = new Dictionary<UnityEngine.Object, List<PrefabModification>>(Sirenix.Serialization.Utilities.ReferenceEqualityComparer<UnityEngine.Object>.Default);

		private static readonly Dictionary<MemberInfo, Sirenix.Serialization.Utilities.WeakValueGetter> UnityMemberGetters = new Dictionary<MemberInfo, Sirenix.Serialization.Utilities.WeakValueGetter>();

		private static readonly Dictionary<MemberInfo, Sirenix.Serialization.Utilities.WeakValueSetter> UnityMemberSetters = new Dictionary<MemberInfo, Sirenix.Serialization.Utilities.WeakValueSetter>();

		private static readonly Dictionary<MemberInfo, bool> UnityWillSerializeMembersCache = new Dictionary<MemberInfo, bool>();

		private static readonly Dictionary<Type, bool> UnityWillSerializeTypesCache = new Dictionary<Type, bool>();

		private static readonly HashSet<Type> UnityNeverSerializesTypes = new HashSet<Type> { typeof(Coroutine) };

		private static readonly HashSet<string> UnityNeverSerializesTypeNames = new HashSet<string> { "UnityEngine.AnimationState" };

		private static readonly ISerializationPolicy UnityPolicy = SerializationPolicies.Unity;

		private static readonly ISerializationPolicy EverythingPolicy = SerializationPolicies.Everything;

		private static readonly ISerializationPolicy StrictPolicy = SerializationPolicies.Strict;

		private static readonly Dictionary<MemberInfo, CachedSerializationBackendResult> OdinWillSerializeCache_UnityPolicy = new Dictionary<MemberInfo, CachedSerializationBackendResult>(Sirenix.Serialization.Utilities.ReferenceEqualityComparer<MemberInfo>.Default);

		private static readonly Dictionary<MemberInfo, CachedSerializationBackendResult> OdinWillSerializeCache_EverythingPolicy = new Dictionary<MemberInfo, CachedSerializationBackendResult>(Sirenix.Serialization.Utilities.ReferenceEqualityComparer<MemberInfo>.Default);

		private static readonly Dictionary<MemberInfo, CachedSerializationBackendResult> OdinWillSerializeCache_StrictPolicy = new Dictionary<MemberInfo, CachedSerializationBackendResult>(Sirenix.Serialization.Utilities.ReferenceEqualityComparer<MemberInfo>.Default);

		private static readonly Dictionary<ISerializationPolicy, Dictionary<MemberInfo, CachedSerializationBackendResult>> OdinWillSerializeCache_CustomPolicies = new Dictionary<ISerializationPolicy, Dictionary<MemberInfo, CachedSerializationBackendResult>>(Sirenix.Serialization.Utilities.ReferenceEqualityComparer<ISerializationPolicy>.Default);

		private static readonly MemberInfo EditorApplication_delayCall_Member = typeof(EditorApplication).GetMember("delayCall", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic).FirstOrDefault();

		/// <summary>
		/// Whether to always force editor mode serialization. This member only exists in the editor.
		/// </summary>
		public static bool ForceEditorModeSerialization { get; set; }

		/// <summary>
		/// In 2020.1, Unity changed EditorApplication.delayCall from a field to an event, meaning 
		/// we now have to use reflection to access it consistently across all versions of Unity.
		/// </summary>
		private static event Action EditorApplication_delayCall_Alias
		{
			add
			{
				if (EditorApplication_delayCall_Member == null)
				{
					throw new InvalidOperationException("EditorApplication.delayCall field or event could not be found. Odin will be broken.");
				}
				if (EditorApplication_delayCall_Member is FieldInfo)
				{
					EditorApplication.CallbackFunction val = (EditorApplication.CallbackFunction)(EditorApplication_delayCall_Member as FieldInfo).GetValue(null);
					val = (EditorApplication.CallbackFunction)Delegate.Combine(val, value.ConvertDelegate<EditorApplication.CallbackFunction>());
					(EditorApplication_delayCall_Member as FieldInfo).SetValue(null, val);
				}
				else if (EditorApplication_delayCall_Member is EventInfo)
				{
					(EditorApplication_delayCall_Member as EventInfo).AddEventHandler(null, value);
				}
				else if (EditorApplication_delayCall_Member == null)
				{
					throw new InvalidOperationException("EditorApplication.delayCall was not a field or an event. Odin will be broken.");
				}
			}
			remove
			{
				if (EditorApplication_delayCall_Member == null)
				{
					throw new InvalidOperationException("EditorApplication.delayCall field or event could not be found. Odin will be broken.");
				}
				if (EditorApplication_delayCall_Member is FieldInfo)
				{
					EditorApplication.CallbackFunction val = (EditorApplication.CallbackFunction)(EditorApplication_delayCall_Member as FieldInfo).GetValue(null);
					val = (EditorApplication.CallbackFunction)Delegate.Remove(val, value.ConvertDelegate<EditorApplication.CallbackFunction>());
					(EditorApplication_delayCall_Member as FieldInfo).SetValue(null, val);
				}
				else if (EditorApplication_delayCall_Member is EventInfo)
				{
					(EditorApplication_delayCall_Member as EventInfo).RemoveEventHandler(null, value);
				}
				else if (EditorApplication_delayCall_Member == null)
				{
					throw new InvalidOperationException("EditorApplication.delayCall was not a field or an event. Odin will be broken.");
				}
			}
		}

		[InitializeOnLoadMethod]
		private static void SubscribeToDomainReloadEvents()
		{
			Type AssemblyReloadEvents_Type = TwoWaySerializationBinder.Default.BindToType("UnityEditor.AssemblyReloadEvents");
			if (!(AssemblyReloadEvents_Type == null))
			{
				EventInfo AssemblyReloadEvents_beforeAssemblyReload_Event = AssemblyReloadEvents_Type.GetEvent("beforeAssemblyReload");
				EventInfo AssemblyReloadEvents_afterAssemblyReload_Event = AssemblyReloadEvents_Type.GetEvent("afterAssemblyReload");
				Type AssemblyReloadEvents_AssemblyReloadCallback_Type = AssemblyReloadEvents_Type.GetNestedType("AssemblyReloadCallback");
				if (!(AssemblyReloadEvents_beforeAssemblyReload_Event == null) && !(AssemblyReloadEvents_afterAssemblyReload_Event == null) && !(AssemblyReloadEvents_AssemblyReloadCallback_Type == null))
				{
					MethodInfo UnitySerializationUtility_OnBeforeAssemblyReload_Method = typeof(UnitySerializationUtility).GetMethod("OnBeforeAssemblyReload", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
					MethodInfo UnitySerializationUtility_OnAfterAssemblyReload_Method = typeof(UnitySerializationUtility).GetMethod("OnAfterAssemblyReload", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
					Delegate onBeforeDelegate = Delegate.CreateDelegate(AssemblyReloadEvents_AssemblyReloadCallback_Type, UnitySerializationUtility_OnBeforeAssemblyReload_Method);
					Delegate onAfterDelegate = Delegate.CreateDelegate(AssemblyReloadEvents_AssemblyReloadCallback_Type, UnitySerializationUtility_OnAfterAssemblyReload_Method);
					AssemblyReloadEvents_beforeAssemblyReload_Event.AddEventHandler(null, onBeforeDelegate);
					AssemblyReloadEvents_afterAssemblyReload_Event.AddEventHandler(null, onAfterDelegate);
				}
			}
		}

		private static void OnBeforeAssemblyReload()
		{
			isDoingDomainReload = true;
		}

		private static void OnAfterAssemblyReload()
		{
			isDoingDomainReload = false;
		}

		/// <summary>
		/// Not yet documented.
		/// </summary>
		public static List<PrefabModification> GetRegisteredPrefabModifications(UnityEngine.Object obj)
		{
			RegisteredPrefabModifications.TryGetValue(obj, out var result);
			return result;
		}

		public static bool HasModificationsWaitingForDelayedApply(UnityEngine.Object obj)
		{
			return UnityObjectsWaitingForDelayedModificationApply.Contains(obj);
		}

		/// <summary>
		/// Checks whether Odin will serialize a given member.
		/// </summary>
		/// <param name="member">The member to check.</param>
		/// <param name="serializeUnityFields">Whether to allow serialization of members that will also be serialized by Unity.</param>
		/// <param name="policy">The policy that Odin should be using for serialization of the given member. If this parameter is null, it defaults to <see cref="P:Sirenix.Serialization.SerializationPolicies.Unity" />.</param>
		/// <returns>True if Odin will serialize the member, otherwise false.</returns>
		public static bool OdinWillSerialize(MemberInfo member, bool serializeUnityFields, ISerializationPolicy policy = null)
		{
			Dictionary<MemberInfo, CachedSerializationBackendResult> cacheForPolicy;
			if (policy == null || policy == UnityPolicy)
			{
				cacheForPolicy = OdinWillSerializeCache_UnityPolicy;
			}
			else if (policy == EverythingPolicy)
			{
				cacheForPolicy = OdinWillSerializeCache_EverythingPolicy;
			}
			else if (policy == StrictPolicy)
			{
				cacheForPolicy = OdinWillSerializeCache_StrictPolicy;
			}
			else
			{
				lock (OdinWillSerializeCache_CustomPolicies)
				{
					if (!OdinWillSerializeCache_CustomPolicies.TryGetValue(policy, out cacheForPolicy))
					{
						cacheForPolicy = new Dictionary<MemberInfo, CachedSerializationBackendResult>(Sirenix.Serialization.Utilities.ReferenceEqualityComparer<MemberInfo>.Default);
						OdinWillSerializeCache_CustomPolicies.Add(policy, cacheForPolicy);
					}
				}
			}
			lock (cacheForPolicy)
			{
				if (!cacheForPolicy.TryGetValue(member, out var result))
				{
					result = default(CachedSerializationBackendResult);
					if (serializeUnityFields)
					{
						result.SerializeUnityFieldsTrueResult = CalculateOdinWillSerialize(member, serializeUnityFields, policy ?? UnityPolicy);
						result.HasCalculatedSerializeUnityFieldsTrueResult = true;
					}
					else
					{
						result.SerializeUnityFieldsFalseResult = CalculateOdinWillSerialize(member, serializeUnityFields, policy ?? UnityPolicy);
						result.HasCalculatedSerializeUnityFieldsFalseResult = true;
					}
					cacheForPolicy.Add(member, result);
				}
				else if (serializeUnityFields && !result.HasCalculatedSerializeUnityFieldsTrueResult)
				{
					result.SerializeUnityFieldsTrueResult = CalculateOdinWillSerialize(member, serializeUnityFields, policy ?? UnityPolicy);
					result.HasCalculatedSerializeUnityFieldsTrueResult = true;
					cacheForPolicy[member] = result;
				}
				else if (!serializeUnityFields && !result.HasCalculatedSerializeUnityFieldsFalseResult)
				{
					result.SerializeUnityFieldsFalseResult = CalculateOdinWillSerialize(member, serializeUnityFields, policy ?? UnityPolicy);
					result.HasCalculatedSerializeUnityFieldsFalseResult = true;
					cacheForPolicy[member] = result;
				}
				return serializeUnityFields ? result.SerializeUnityFieldsTrueResult : result.SerializeUnityFieldsFalseResult;
			}
		}

		private static bool CalculateOdinWillSerialize(MemberInfo member, bool serializeUnityFields, ISerializationPolicy policy)
		{
			if (member.DeclaringType == typeof(UnityEngine.Object))
			{
				return false;
			}
			if (!policy.ShouldSerializeMember(member))
			{
				return false;
			}
			if (member is FieldInfo && member.IsDefined(typeof(OdinSerializeAttribute), inherit: true))
			{
				return true;
			}
			if (serializeUnityFields)
			{
				return true;
			}
			try
			{
				if (SerializeReferenceAttributeType != null && member.IsDefined(SerializeReferenceAttributeType, inherit: true))
				{
					return false;
				}
			}
			catch
			{
			}
			if (GuessIfUnityWillSerialize(member))
			{
				return false;
			}
			return true;
		}

		/// <summary>
		/// Guesses whether or not Unity will serialize a given member. This is not completely accurate.
		/// </summary>
		/// <param name="member">The member to check.</param>
		/// <returns>True if it is guessed that Unity will serialize the member, otherwise false.</returns>
		/// <exception cref="T:System.ArgumentNullException">The parameter <paramref name="member" /> is null.</exception>
		public static bool GuessIfUnityWillSerialize(MemberInfo member)
		{
			if (member == null)
			{
				throw new ArgumentNullException("member");
			}
			bool result;
			lock (UnityWillSerializeMembersCache)
			{
				if (!UnityWillSerializeMembersCache.TryGetValue(member, out result))
				{
					result = GuessIfUnityWillSerializePrivate(member);
					UnityWillSerializeMembersCache[member] = result;
				}
			}
			return result;
		}

		private static bool GuessIfUnityWillSerializePrivate(MemberInfo member)
		{
			FieldInfo fieldInfo = member as FieldInfo;
			if (fieldInfo == null || fieldInfo.IsStatic || fieldInfo.IsInitOnly)
			{
				return false;
			}
			if (Sirenix.Serialization.Utilities.MemberInfoExtensions.IsDefined<NonSerializedAttribute>(fieldInfo))
			{
				return false;
			}
			if (SerializeReferenceAttributeType != null && fieldInfo.IsDefined(SerializeReferenceAttributeType, inherit: true))
			{
				return true;
			}
			if (!typeof(UnityEngine.Object).IsAssignableFrom(fieldInfo.FieldType) && fieldInfo.FieldType == fieldInfo.DeclaringType)
			{
				return false;
			}
			if (!fieldInfo.IsPublic && !Sirenix.Serialization.Utilities.MemberInfoExtensions.IsDefined<SerializeField>(fieldInfo))
			{
				return false;
			}
			if (Sirenix.Serialization.Utilities.MemberInfoExtensions.IsDefined<FixedBufferAttribute>(fieldInfo))
			{
				return Sirenix.Serialization.Utilities.UnityVersion.IsVersionOrGreater(2017, 1);
			}
			return GuessIfUnityWillSerialize(fieldInfo.FieldType);
		}

		/// <summary>
		/// Guesses whether or not Unity will serialize a given type. This is not completely accurate.
		/// </summary>
		/// <param name="type">The type to check.</param>
		/// <returns>True if it is guessed that Unity will serialize the type, otherwise false.</returns>
		/// <exception cref="T:System.ArgumentNullException">The parameter <paramref name="type" /> is null.</exception>
		public static bool GuessIfUnityWillSerialize(Type type)
		{
			if (type == null)
			{
				throw new ArgumentNullException("type");
			}
			bool result;
			lock (UnityWillSerializeTypesCache)
			{
				if (!UnityWillSerializeTypesCache.TryGetValue(type, out result))
				{
					result = GuessIfUnityWillSerializePrivate(type);
					UnityWillSerializeTypesCache[type] = result;
				}
			}
			return result;
		}

		private static bool GuessIfUnityWillSerializePrivate(Type type)
		{
			if (UnityNeverSerializesTypes.Contains(type) || UnityNeverSerializesTypeNames.Contains(type.FullName))
			{
				return false;
			}
			if (typeof(UnityEngine.Object).IsAssignableFrom(type))
			{
				if (type.IsGenericType)
				{
					return Sirenix.Serialization.Utilities.UnityVersion.IsVersionOrGreater(2020, 1);
				}
				return true;
			}
			if (type.IsAbstract || type.IsInterface || type == typeof(object))
			{
				return false;
			}
			if (type.IsEnum)
			{
				Type underlyingType = Enum.GetUnderlyingType(type);
				if (Sirenix.Serialization.Utilities.UnityVersion.IsVersionOrGreater(5, 6))
				{
					if (underlyingType != typeof(long))
					{
						return underlyingType != typeof(ulong);
					}
					return false;
				}
				if (!(underlyingType == typeof(int)))
				{
					return underlyingType == typeof(byte);
				}
				return true;
			}
			if (type.IsPrimitive || type == typeof(string))
			{
				return true;
			}
			if (typeof(Delegate).IsAssignableFrom(type))
			{
				return false;
			}
			if (typeof(UnityEventBase).IsAssignableFrom(type))
			{
				if (type.IsGenericType && !Sirenix.Serialization.Utilities.UnityVersion.IsVersionOrGreater(2020, 1))
				{
					return false;
				}
				if (!(type == typeof(UnityEvent)))
				{
					return Sirenix.Serialization.Utilities.TypeExtensions.IsDefined<SerializableAttribute>(type, inherit: false);
				}
				return true;
			}
			if (type.IsArray)
			{
				Type elementType = type.GetElementType();
				if (type.GetArrayRank() == 1 && !elementType.IsArray && !Sirenix.Serialization.Utilities.TypeExtensions.ImplementsOpenGenericClass(elementType, typeof(List<>)))
				{
					return GuessIfUnityWillSerialize(elementType);
				}
				return false;
			}
			if (type.IsGenericType && !type.IsGenericTypeDefinition && type.GetGenericTypeDefinition() == typeof(List<>))
			{
				Type elementType2 = Sirenix.Serialization.Utilities.TypeExtensions.GetArgumentsOfInheritedOpenGenericClass(type, typeof(List<>))[0];
				if (elementType2.IsArray || Sirenix.Serialization.Utilities.TypeExtensions.ImplementsOpenGenericClass(elementType2, typeof(List<>)))
				{
					return false;
				}
				return GuessIfUnityWillSerialize(elementType2);
			}
			if (type.Assembly.FullName.StartsWith("UnityEngine", StringComparison.InvariantCulture) || type.Assembly.FullName.StartsWith("UnityEditor", StringComparison.InvariantCulture))
			{
				return true;
			}
			if (type.IsGenericType && !Sirenix.Serialization.Utilities.UnityVersion.IsVersionOrGreater(2020, 1))
			{
				return false;
			}
			if (type.Assembly == String_Assembly || type.Assembly == HashSet_Assembly || type.Assembly == LinkedList_Assembly)
			{
				return false;
			}
			if (Sirenix.Serialization.Utilities.TypeExtensions.IsDefined<SerializableAttribute>(type, inherit: false))
			{
				if (Sirenix.Serialization.Utilities.UnityVersion.IsVersionOrGreater(4, 5))
				{
					return true;
				}
				return type.IsClass;
			}
			if (!Sirenix.Serialization.Utilities.UnityVersion.IsVersionOrGreater(2018, 2))
			{
				Type current = type.BaseType;
				while (current != null && current != typeof(object))
				{
					if (current.IsGenericType && current.GetGenericTypeDefinition().FullName == "UnityEngine.Networking.SyncListStruct`1")
					{
						return true;
					}
					current = current.BaseType;
				}
			}
			return false;
		}

		/// <summary>
		/// Not yet documented.
		/// </summary>
		public static void SerializeUnityObject(UnityEngine.Object unityObject, ref SerializationData data, bool serializeUnityFields = false, SerializationContext context = null)
		{
			if (unityObject == null)
			{
				throw new ArgumentNullException("unityObject");
			}
			if (OdinPrefabSerializationEditorUtility.HasNewPrefabWorkflow && unityObject is ISupportsPrefabSerialization { SerializationData: var sData } supporter)
			{
				sData.Prefab = null;
				supporter.SerializationData = sData;
			}
			bool pretendIsPlayer = Application.isPlaying && !AssetDatabase.Contains(unityObject);
			StackFrame[] stackFrames = new StackTrace().GetFrames();
			Type buildPipelineType = typeof(BuildPipeline);
			Type prefabUtilityType = typeof(PrefabUtility);
			foreach (StackFrame frame in stackFrames)
			{
				MethodBase method = frame.GetMethod();
				if (method.DeclaringType == buildPipelineType || method.DeclaringType == SBP_ContentPipelineType)
				{
					pretendIsPlayer = true;
					break;
				}
				if (method.DeclaringType == prefabUtilityType && method.Name == "RecordPrefabInstancePropertyModifications")
				{
					return;
				}
			}
			if (ForceEditorModeSerialization)
			{
				pretendIsPlayer = false;
			}
			if (!pretendIsPlayer && !isDoingDomainReload)
			{
				UnityEngine.Object prefab = null;
				SerializationData prefabData = default(SerializationData);
				bool prefabDataIsFromSelf = false;
				if (OdinPrefabSerializationEditorUtility.ObjectIsPrefabInstance(unityObject))
				{
					prefab = OdinPrefabSerializationEditorUtility.GetCorrespondingObjectFromSource(unityObject);
					if (Sirenix.Serialization.Utilities.UnityExtensions.SafeIsUnityNull(prefab) && (object)data.Prefab != null)
					{
						prefab = data.Prefab;
					}
					if ((object)prefab != null)
					{
						if (prefab is ISupportsPrefabSerialization)
						{
							SerializationData pData = (prefab as ISupportsPrefabSerialization).SerializationData;
							if (pData.ContainsData)
							{
								prefabData = pData;
							}
							else
							{
								prefabData = data;
								prefabData.Prefab = null;
								prefabDataIsFromSelf = true;
							}
						}
						else if (prefab.GetType() != typeof(UnityEngine.Object))
						{
							prefabData = data;
							prefabData.Prefab = null;
							prefabDataIsFromSelf = true;
						}
					}
				}
				if ((object)prefab != null)
				{
					if (!prefabDataIsFromSelf && prefabData.PrefabModifications != null && prefabData.PrefabModifications.Count > 0)
					{
						try
						{
							(prefab as ISerializationCallbackReceiver).OnBeforeSerialize();
						}
						catch (Exception ex)
						{
							if (!OdinPrefabSerializationEditorUtility.HasNewPrefabWorkflow)
							{
								throw ex;
							}
						}
						EditorApplication_delayCall_Alias += delegate
						{
							if ((bool)prefab)
							{
								EditorUtility.SetDirty(prefab);
							}
						};
						prefabData = (prefab as ISupportsPrefabSerialization).SerializationData;
					}
					bool newModifications = false;
					List<UnityEngine.Object> modificationsReferencedUnityObjects = data.PrefabModificationsReferencedUnityObjects;
					List<string> modificationsToKeep;
					if (RegisteredPrefabModifications.TryGetValue(unityObject, out var modificationsList))
					{
						RegisteredPrefabModifications.Remove(unityObject);
						modificationsToKeep = SerializePrefabModifications(modificationsList, ref modificationsReferencedUnityObjects);
						newModifications = true;
					}
					else
					{
						modificationsToKeep = data.PrefabModifications;
					}
					List<UnityEngine.Object> unityObjects = data.ReferencedUnityObjects;
					data = prefabData;
					data.ReferencedUnityObjects = unityObjects;
					data.Prefab = prefab;
					data.PrefabModifications = modificationsToKeep;
					data.PrefabModificationsReferencedUnityObjects = modificationsReferencedUnityObjects;
					if (newModifications)
					{
						SetUnityObjectModifications(unityObject, ref data, prefab);
					}
					if (!(data.Prefab != null))
					{
						return;
					}
					PrefabDeserializeUtility.CleanSceneObjectToKeepOnApply();
					lock (PrefabDeserializeUtility.DeserializePrefabs_LOCK)
					{
						HashSet<object> keep = PrefabDeserializeUtility.GetSceneObjectsToKeepSet(unityObject, createIfDoesntExist: true);
						keep.Clear();
						if (data.PrefabModificationsReferencedUnityObjects == null || data.PrefabModificationsReferencedUnityObjects.Count <= 0)
						{
							return;
						}
						GameObject instanceRoot = PrefabUtility.FindPrefabRoot(((Component)unityObject).gameObject);
						foreach (UnityEngine.Object reference in data.PrefabModificationsReferencedUnityObjects)
						{
							if (reference == null || (!(reference is GameObject) && !(reference is Component)) || AssetDatabase.Contains(reference))
							{
								continue;
							}
							PrefabType referencePrefabType = PrefabUtility.GetPrefabType(reference);
							bool mightBeInPrefab = referencePrefabType == PrefabType.Prefab || referencePrefabType == PrefabType.PrefabInstance || referencePrefabType == PrefabType.ModelPrefab || referencePrefabType == PrefabType.ModelPrefabInstance;
							if (!mightBeInPrefab && PrefabUtility_IsComponentAddedToPrefabInstance_MethodInfo != null && reference is Component && (bool)PrefabUtility_IsComponentAddedToPrefabInstance_MethodInfo.Invoke(null, new object[1] { reference }))
							{
								mightBeInPrefab = true;
							}
							if (!mightBeInPrefab)
							{
								keep.Add(reference);
								continue;
							}
							GameObject gameObject = (GameObject)((reference is GameObject) ? reference : (reference as Component).gameObject);
							GameObject referenceRoot = PrefabUtility.FindPrefabRoot(gameObject);
							if (referenceRoot != instanceRoot)
							{
								keep.Add(reference);
							}
						}
						return;
					}
				}
			}
			data.Reset();
			DataFormat format = ((!(unityObject is IOverridesSerializationFormat formatOverride)) ? (GlobalConfig<GlobalSerializationConfig>.HasInstanceLoaded ? ((!pretendIsPlayer) ? GlobalConfig<GlobalSerializationConfig>.Instance.EditorSerializationFormat : GlobalConfig<GlobalSerializationConfig>.Instance.BuildSerializationFormat) : ((!pretendIsPlayer) ? DataFormat.Nodes : DataFormat.Binary)) : formatOverride.GetFormatToSerializeAs(pretendIsPlayer));
			ISerializationPolicy serializationPolicy = SerializationPolicies.Unity;
			if (unityObject is IOverridesSerializationPolicy policyOverride)
			{
				serializationPolicy = policyOverride.SerializationPolicy ?? SerializationPolicies.Unity;
				if (context != null)
				{
					context.Config.SerializationPolicy = serializationPolicy;
				}
				serializeUnityFields = policyOverride.OdinSerializesUnityFields;
			}
			if (pretendIsPlayer)
			{
				if (format == DataFormat.Nodes)
				{
					UnityEngine.Debug.LogWarning("The serialization format '" + format.ToString() + "' is disabled in play mode, and when building a player. Defaulting to the format '" + DataFormat.Binary.ToString() + "' instead.");
					format = DataFormat.Binary;
				}
				SerializeUnityObject(unityObject, ref data.SerializedBytes, ref data.ReferencedUnityObjects, format, serializeUnityFields, context);
				data.SerializedFormat = format;
				return;
			}
			if (format == DataFormat.Nodes)
			{
				if (context == null)
				{
					using Cache<SerializationContext> newContext = Cache<SerializationContext>.Claim();
					using SerializationNodeDataWriter writer = new SerializationNodeDataWriter(newContext);
					using Cache<UnityReferenceResolver> resolver = Cache<UnityReferenceResolver>.Claim();
					if (data.SerializationNodes != null)
					{
						data.SerializationNodes.Clear();
						writer.Nodes = data.SerializationNodes;
					}
					resolver.Value.SetReferencedUnityObjects(data.ReferencedUnityObjects);
					newContext.Value.Config.SerializationPolicy = serializationPolicy;
					newContext.Value.IndexReferenceResolver = resolver.Value;
					writer.Context = newContext;
					SerializeUnityObject(unityObject, writer, serializeUnityFields);
					data.SerializationNodes = writer.Nodes;
					data.ReferencedUnityObjects = resolver.Value.GetReferencedUnityObjects();
				}
				else
				{
					using SerializationNodeDataWriter writer2 = new SerializationNodeDataWriter(context);
					using Cache<UnityReferenceResolver> resolver2 = Cache<UnityReferenceResolver>.Claim();
					if (data.SerializationNodes != null)
					{
						data.SerializationNodes.Clear();
						writer2.Nodes = data.SerializationNodes;
					}
					resolver2.Value.SetReferencedUnityObjects(data.ReferencedUnityObjects);
					context.IndexReferenceResolver = resolver2.Value;
					SerializeUnityObject(unityObject, writer2, serializeUnityFields);
					data.SerializationNodes = writer2.Nodes;
					data.ReferencedUnityObjects = resolver2.Value.GetReferencedUnityObjects();
				}
			}
			else
			{
				SerializeUnityObject(unityObject, ref data.SerializedBytesString, ref data.ReferencedUnityObjects, format, serializeUnityFields, context);
			}
			data.SerializedFormat = format;
		}

		private static void SetUnityObjectModifications(UnityEngine.Object unityObject, ref SerializationData data, UnityEngine.Object prefab)
		{
			Type unityObjectType = unityObject.GetType();
			FieldInfo serializedDataField = (from field in Sirenix.Serialization.Utilities.TypeExtensions.GetAllMembers<FieldInfo>(unityObjectType, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
				where field.FieldType == typeof(SerializationData) && GuessIfUnityWillSerialize(field)
				select field).LastOrDefault();
			if (serializedDataField == null)
			{
				UnityEngine.Debug.LogError("Could not find a field of type " + typeof(SerializationData).Name + " on the serializing type " + Sirenix.Serialization.Utilities.TypeExtensions.GetNiceName(unityObjectType) + " when trying to manually set prefab modifications. It is possible that prefab instances of this type will be corrupted if changes are ever applied to prefab.", prefab);
				return;
			}
			string serializedDataPath = serializedDataField.Name + ".";
			string referencedUnityObjectsPath = serializedDataPath + "PrefabModificationsReferencedUnityObjects.Array.";
			string modificationsPath = serializedDataPath + "PrefabModifications.Array.";
			string prefabPath = serializedDataPath + "Prefab";
			List<PropertyModification> mods = PrefabUtility.GetPropertyModifications(unityObject).ToList();
			for (int i = 0; i < mods.Count; i++)
			{
				PropertyModification mod = mods[i];
				if (mod.propertyPath.StartsWith(serializedDataPath, StringComparison.InvariantCulture) && (object)mod.target == prefab)
				{
					mods.RemoveAt(i);
					i--;
				}
			}
			mods.Insert(0, new PropertyModification
			{
				target = prefab,
				propertyPath = referencedUnityObjectsPath + "size",
				value = data.PrefabModificationsReferencedUnityObjects.Count.ToString("D", CultureInfo.InvariantCulture)
			});
			mods.Insert(0, new PropertyModification
			{
				target = prefab,
				propertyPath = modificationsPath + "size",
				value = data.PrefabModifications.Count.ToString("D", CultureInfo.InvariantCulture)
			});
			mods.Add(new PropertyModification
			{
				target = prefab,
				propertyPath = prefabPath,
				objectReference = prefab
			});
			for (int i2 = 0; i2 < data.PrefabModificationsReferencedUnityObjects.Count; i2++)
			{
				mods.Add(new PropertyModification
				{
					target = prefab,
					propertyPath = referencedUnityObjectsPath + "data[" + i2.ToString("D", CultureInfo.InvariantCulture) + "]",
					objectReference = data.PrefabModificationsReferencedUnityObjects[i2]
				});
			}
			for (int i3 = 0; i3 < data.PrefabModifications.Count; i3++)
			{
				mods.Add(new PropertyModification
				{
					target = prefab,
					propertyPath = modificationsPath + "data[" + i3.ToString("D", CultureInfo.InvariantCulture) + "]",
					value = data.PrefabModifications[i3]
				});
			}
			EditorApplication_delayCall_Alias += delegate
			{
				UnityObjectsWaitingForDelayedModificationApply.Remove(unityObject);
				PrefabUtility.SetPropertyModifications(unityObject, mods.ToArray());
			};
			UnityObjectsWaitingForDelayedModificationApply.Add(unityObject);
		}

		/// <summary>
		/// Not yet documented.
		/// </summary>
		public static void SerializeUnityObject(UnityEngine.Object unityObject, ref string base64Bytes, ref List<UnityEngine.Object> referencedUnityObjects, DataFormat format, bool serializeUnityFields = false, SerializationContext context = null)
		{
			byte[] bytes = null;
			SerializeUnityObject(unityObject, ref bytes, ref referencedUnityObjects, format, serializeUnityFields, context);
			base64Bytes = Convert.ToBase64String(bytes);
		}

		/// <summary>
		/// Not yet documented.
		/// </summary>
		public static void SerializeUnityObject(UnityEngine.Object unityObject, ref byte[] bytes, ref List<UnityEngine.Object> referencedUnityObjects, DataFormat format, bool serializeUnityFields = false, SerializationContext context = null)
		{
			if (unityObject == null)
			{
				throw new ArgumentNullException("unityObject");
			}
			if (format == DataFormat.Nodes)
			{
				UnityEngine.Debug.LogError("The serialization data format '" + format.ToString() + "' is not supported by this method. You must create your own writer.");
				return;
			}
			if (referencedUnityObjects == null)
			{
				referencedUnityObjects = new List<UnityEngine.Object>();
			}
			else
			{
				referencedUnityObjects.Clear();
			}
			using Cache<CachedMemoryStream> stream = Cache<CachedMemoryStream>.Claim();
			using Cache<UnityReferenceResolver> resolver = Cache<UnityReferenceResolver>.Claim();
			resolver.Value.SetReferencedUnityObjects(referencedUnityObjects);
			if (context != null)
			{
				context.IndexReferenceResolver = resolver.Value;
				using ICache writerCache = GetCachedUnityWriter(format, stream.Value.MemoryStream, context);
				SerializeUnityObject(unityObject, writerCache.Value as IDataWriter, serializeUnityFields);
			}
			else
			{
				using Cache<SerializationContext> con = Cache<SerializationContext>.Claim();
				con.Value.Config.SerializationPolicy = SerializationPolicies.Unity;
				if (GlobalConfig<GlobalSerializationConfig>.HasInstanceLoaded)
				{
					con.Value.Config.DebugContext.ErrorHandlingPolicy = GlobalConfig<GlobalSerializationConfig>.Instance.ErrorHandlingPolicy;
					con.Value.Config.DebugContext.LoggingPolicy = GlobalConfig<GlobalSerializationConfig>.Instance.LoggingPolicy;
					con.Value.Config.DebugContext.Logger = GlobalConfig<GlobalSerializationConfig>.Instance.Logger;
				}
				else
				{
					con.Value.Config.DebugContext.ErrorHandlingPolicy = ErrorHandlingPolicy.Resilient;
					con.Value.Config.DebugContext.LoggingPolicy = LoggingPolicy.LogErrors;
					con.Value.Config.DebugContext.Logger = DefaultLoggers.UnityLogger;
				}
				con.Value.IndexReferenceResolver = resolver.Value;
				using ICache writerCache2 = GetCachedUnityWriter(format, stream.Value.MemoryStream, con);
				SerializeUnityObject(unityObject, writerCache2.Value as IDataWriter, serializeUnityFields);
			}
			bytes = stream.Value.MemoryStream.ToArray();
		}

		/// <summary>
		/// Not yet documented.
		/// </summary>
		public static void SerializeUnityObject(UnityEngine.Object unityObject, IDataWriter writer, bool serializeUnityFields = false)
		{
			if (unityObject == null)
			{
				throw new ArgumentNullException("unityObject");
			}
			if (writer == null)
			{
				throw new ArgumentNullException("writer");
			}
			try
			{
				writer.PrepareNewSerializationSession();
				MemberInfo[] members = FormatterUtilities.GetSerializableMembers(unityObject.GetType(), writer.Context.Config.SerializationPolicy);
				object unityObjectInstance = unityObject;
				foreach (MemberInfo member in members)
				{
					Sirenix.Serialization.Utilities.WeakValueGetter getter = null;
					if (!OdinWillSerialize(member, serializeUnityFields, writer.Context.Config.SerializationPolicy) || (getter = GetCachedUnityMemberGetter(member)) == null)
					{
						continue;
					}
					object value = getter(ref unityObjectInstance);
					if (value == null || !(value.GetType() == typeof(SerializationData)))
					{
						Serializer serializer = Serializer.Get(FormatterUtilities.GetContainedType(member));
						try
						{
							serializer.WriteValueWeak(member.Name, value, writer);
						}
						catch (Exception exception)
						{
							writer.Context.Config.DebugContext.LogException(exception);
						}
					}
				}
				writer.FlushToStream();
			}
			catch (SerializationAbortException innerException)
			{
				throw new SerializationAbortException("Serialization of type '" + Sirenix.Serialization.Utilities.TypeExtensions.GetNiceFullName(unityObject.GetType()) + "' aborted.", innerException);
			}
			catch (Exception ex)
			{
				UnityEngine.Debug.LogException(new Exception("Exception thrown while serializing type '" + Sirenix.Serialization.Utilities.TypeExtensions.GetNiceFullName(unityObject.GetType()) + "': " + ex.Message, ex));
			}
		}

		/// <summary>
		/// Not yet documented.
		/// </summary>
		public static void DeserializeUnityObject(UnityEngine.Object unityObject, ref SerializationData data, DeserializationContext context = null)
		{
			DeserializeUnityObject(unityObject, ref data, context, isPrefabData: false, null);
		}

		private static void DeserializeUnityObject(UnityEngine.Object unityObject, ref SerializationData data, DeserializationContext context, bool isPrefabData, List<UnityEngine.Object> prefabInstanceUnityObjects)
		{
			if (unityObject == null)
			{
				throw new ArgumentNullException("unityObject");
			}
			if (isPrefabData && prefabInstanceUnityObjects == null)
			{
				prefabInstanceUnityObjects = new List<UnityEngine.Object>();
			}
			if (OdinPrefabSerializationEditorUtility.HasNewPrefabWorkflow && unityObject is ISupportsPrefabSerialization { SerializationData: var sData } supporter)
			{
				if (!sData.ContainsData)
				{
					return;
				}
				sData.Prefab = null;
				supporter.SerializationData = sData;
			}
			if (data.SerializedBytes != null && data.SerializedBytes.Length != 0 && (data.SerializationNodes == null || data.SerializationNodes.Count == 0))
			{
				if (data.SerializedFormat == DataFormat.Nodes)
				{
					DataFormat formatGuess = ((data.SerializedBytes[0] == 123) ? DataFormat.JSON : DataFormat.Binary);
					try
					{
						string bytesStr = ProperBitConverter.BytesToHexString(data.SerializedBytes);
						UnityEngine.Debug.LogWarning("Serialization data has only bytes stored, but the serialized format is marked as being 'Nodes', which is incompatible with data stored as a byte array. Based on the appearance of the serialized bytes, Odin has guessed that the data format is '" + formatGuess.ToString() + "', and will attempt to deserialize the bytes using that format. The serialized bytes follow, converted to a hex string: " + bytesStr);
					}
					catch
					{
					}
					DeserializeUnityObject(unityObject, ref data.SerializedBytes, ref data.ReferencedUnityObjects, formatGuess, context);
				}
				else
				{
					DeserializeUnityObject(unityObject, ref data.SerializedBytes, ref data.ReferencedUnityObjects, data.SerializedFormat, context);
				}
				ApplyPrefabModifications(unityObject, data.PrefabModifications, data.PrefabModificationsReferencedUnityObjects);
				return;
			}
			Cache<DeserializationContext> cachedContext = null;
			try
			{
				if (context == null)
				{
					cachedContext = Cache<DeserializationContext>.Claim();
					context = cachedContext;
					context.Config.SerializationPolicy = SerializationPolicies.Unity;
					if (GlobalConfig<GlobalSerializationConfig>.HasInstanceLoaded)
					{
						context.Config.DebugContext.ErrorHandlingPolicy = GlobalConfig<GlobalSerializationConfig>.Instance.ErrorHandlingPolicy;
						context.Config.DebugContext.LoggingPolicy = GlobalConfig<GlobalSerializationConfig>.Instance.LoggingPolicy;
						context.Config.DebugContext.Logger = GlobalConfig<GlobalSerializationConfig>.Instance.Logger;
					}
					else
					{
						context.Config.DebugContext.ErrorHandlingPolicy = ErrorHandlingPolicy.Resilient;
						context.Config.DebugContext.LoggingPolicy = LoggingPolicy.LogErrors;
						context.Config.DebugContext.Logger = DefaultLoggers.UnityLogger;
					}
				}
				if (unityObject is IOverridesSerializationPolicy { SerializationPolicy: { } serializationPolicy })
				{
					context.Config.SerializationPolicy = serializationPolicy;
				}
				if (!isPrefabData && !Sirenix.Serialization.Utilities.UnityExtensions.SafeIsUnityNull(data.Prefab))
				{
					if (data.Prefab is ISupportsPrefabSerialization)
					{
						if ((object)data.Prefab != unityObject || data.PrefabModifications == null || data.PrefabModifications.Count <= 0)
						{
							SerializationData prefabData = (data.Prefab as ISupportsPrefabSerialization).SerializationData;
							lock (PrefabDeserializeUtility.DeserializePrefabs_LOCK)
							{
								if (PrefabDeserializeUtility.PrefabsWithValuesApplied.Contains(data.Prefab) && PrefabSelectionTracker.IsCurrentlySelectedPrefabRoot(unityObject))
								{
									PrefabDeserializeUtility.PrefabsWithValuesApplied.Remove(data.Prefab);
									List<PrefabModification> newModifications = null;
									HashSet<object> keep = PrefabDeserializeUtility.GetSceneObjectsToKeepSet(unityObject, createIfDoesntExist: false);
									if (data.PrefabModificationsReferencedUnityObjects.Count > 0 && keep != null && keep.Count > 0)
									{
										newModifications = DeserializePrefabModifications(data.PrefabModifications, data.PrefabModificationsReferencedUnityObjects);
										newModifications.RemoveAll((PrefabModification n) => n.ModifiedValue == null || !keep.Contains(n.ModifiedValue));
									}
									else
									{
										if (data.PrefabModifications != null)
										{
											data.PrefabModifications.Clear();
										}
										if (data.PrefabModificationsReferencedUnityObjects != null)
										{
											data.PrefabModificationsReferencedUnityObjects.Clear();
										}
									}
									newModifications = newModifications ?? new List<PrefabModification>();
									PrefabModificationCache.CachePrefabModifications(unityObject, newModifications);
									RegisterPrefabModificationsChange(unityObject, newModifications);
								}
							}
							if (!prefabData.ContainsData)
							{
								DeserializeUnityObject(unityObject, ref data, context, isPrefabData: true, data.ReferencedUnityObjects);
							}
							else
							{
								DeserializeUnityObject(unityObject, ref prefabData, context, isPrefabData: true, data.ReferencedUnityObjects);
							}
							ApplyPrefabModifications(unityObject, data.PrefabModifications, data.PrefabModificationsReferencedUnityObjects);
							return;
						}
						lock (PrefabDeserializeUtility.DeserializePrefabs_LOCK)
						{
							PrefabDeserializeUtility.PrefabsWithValuesApplied.Add(unityObject);
						}
					}
					else if (data.Prefab.GetType() != typeof(UnityEngine.Object))
					{
						UnityEngine.Debug.LogWarning("The type " + Sirenix.Serialization.Utilities.TypeExtensions.GetNiceName(data.Prefab.GetType()) + " no longer supports special prefab serialization (the interface " + Sirenix.Serialization.Utilities.TypeExtensions.GetNiceName(typeof(ISupportsPrefabSerialization)) + ") upon deserialization of an instance of a prefab; prefab data may be lost. Has a type been lost?");
					}
				}
				List<UnityEngine.Object> unityObjects = (isPrefabData ? prefabInstanceUnityObjects : data.ReferencedUnityObjects);
				if (data.SerializedFormat == DataFormat.Nodes)
				{
					using SerializationNodeDataReader reader = new SerializationNodeDataReader(context);
					using Cache<UnityReferenceResolver> resolver = Cache<UnityReferenceResolver>.Claim();
					resolver.Value.SetReferencedUnityObjects(unityObjects);
					context.IndexReferenceResolver = resolver.Value;
					reader.Nodes = data.SerializationNodes;
					DeserializeUnityObject(unityObject, reader);
				}
				else if (data.SerializedBytes != null && data.SerializedBytes.Length != 0)
				{
					DeserializeUnityObject(unityObject, ref data.SerializedBytes, ref unityObjects, data.SerializedFormat, context);
				}
				else
				{
					DeserializeUnityObject(unityObject, ref data.SerializedBytesString, ref unityObjects, data.SerializedFormat, context);
				}
				ApplyPrefabModifications(unityObject, data.PrefabModifications, data.PrefabModificationsReferencedUnityObjects);
			}
			finally
			{
				if (cachedContext != null)
				{
					Cache<DeserializationContext>.Release(cachedContext);
				}
			}
		}

		/// <summary>
		/// Not yet documented.
		/// </summary>
		public static void DeserializeUnityObject(UnityEngine.Object unityObject, ref string base64Bytes, ref List<UnityEngine.Object> referencedUnityObjects, DataFormat format, DeserializationContext context = null)
		{
			if (!string.IsNullOrEmpty(base64Bytes))
			{
				byte[] bytes = null;
				try
				{
					bytes = Convert.FromBase64String(base64Bytes);
				}
				catch (FormatException)
				{
					UnityEngine.Debug.LogError("Invalid base64 string when deserializing data: " + base64Bytes);
				}
				if (bytes != null)
				{
					DeserializeUnityObject(unityObject, ref bytes, ref referencedUnityObjects, format, context);
				}
			}
		}

		/// <summary>
		/// Not yet documented.
		/// </summary>
		public static void DeserializeUnityObject(UnityEngine.Object unityObject, ref byte[] bytes, ref List<UnityEngine.Object> referencedUnityObjects, DataFormat format, DeserializationContext context = null)
		{
			if (unityObject == null)
			{
				throw new ArgumentNullException("unityObject");
			}
			if (bytes == null || bytes.Length == 0)
			{
				return;
			}
			if (format == DataFormat.Nodes)
			{
				try
				{
					UnityEngine.Debug.LogError("The serialization data format '" + format.ToString() + "' is not supported by this method. You must create your own reader.");
					return;
				}
				catch
				{
					return;
				}
			}
			if (referencedUnityObjects == null)
			{
				referencedUnityObjects = new List<UnityEngine.Object>();
			}
			using Cache<CachedMemoryStream> stream = Cache<CachedMemoryStream>.Claim();
			using Cache<UnityReferenceResolver> resolver = Cache<UnityReferenceResolver>.Claim();
			stream.Value.MemoryStream.Write(bytes, 0, bytes.Length);
			stream.Value.MemoryStream.Position = 0L;
			resolver.Value.SetReferencedUnityObjects(referencedUnityObjects);
			if (context != null)
			{
				context.IndexReferenceResolver = resolver.Value;
				using ICache readerCache = GetCachedUnityReader(format, stream.Value.MemoryStream, context);
				DeserializeUnityObject(unityObject, readerCache.Value as IDataReader);
				return;
			}
			using Cache<DeserializationContext> con = Cache<DeserializationContext>.Claim();
			con.Value.Config.SerializationPolicy = SerializationPolicies.Unity;
			if (GlobalConfig<GlobalSerializationConfig>.HasInstanceLoaded)
			{
				con.Value.Config.DebugContext.ErrorHandlingPolicy = GlobalConfig<GlobalSerializationConfig>.Instance.ErrorHandlingPolicy;
				con.Value.Config.DebugContext.LoggingPolicy = GlobalConfig<GlobalSerializationConfig>.Instance.LoggingPolicy;
				con.Value.Config.DebugContext.Logger = GlobalConfig<GlobalSerializationConfig>.Instance.Logger;
			}
			else
			{
				con.Value.Config.DebugContext.ErrorHandlingPolicy = ErrorHandlingPolicy.Resilient;
				con.Value.Config.DebugContext.LoggingPolicy = LoggingPolicy.LogErrors;
				con.Value.Config.DebugContext.Logger = DefaultLoggers.UnityLogger;
			}
			con.Value.IndexReferenceResolver = resolver.Value;
			using ICache readerCache2 = GetCachedUnityReader(format, stream.Value.MemoryStream, con);
			DeserializeUnityObject(unityObject, readerCache2.Value as IDataReader);
		}

		/// <summary>
		/// Not yet documented.
		/// </summary>
		public static void DeserializeUnityObject(UnityEngine.Object unityObject, IDataReader reader)
		{
			if (unityObject == null)
			{
				throw new ArgumentNullException("unityObject");
			}
			if (reader == null)
			{
				throw new ArgumentNullException("reader");
			}
			if (unityObject is IOverridesSerializationPolicy { SerializationPolicy: { } policy })
			{
				reader.Context.Config.SerializationPolicy = policy;
			}
			try
			{
				reader.PrepareNewSerializationSession();
				Dictionary<string, MemberInfo> members = FormatterUtilities.GetSerializableMembersMap(unityObject.GetType(), reader.Context.Config.SerializationPolicy);
				int count = 0;
				object unityObjectInstance = unityObject;
				EntryType entryType;
				string name;
				while ((entryType = reader.PeekEntry(out name)) != EntryType.EndOfNode && entryType != EntryType.EndOfArray && entryType != EntryType.EndOfStream)
				{
					MemberInfo member = null;
					Sirenix.Serialization.Utilities.WeakValueSetter setter = null;
					bool skip = false;
					if (entryType == EntryType.Invalid)
					{
						string message = "Encountered invalid entry while reading serialization data for Unity object of type '" + Sirenix.Serialization.Utilities.TypeExtensions.GetNiceFullName(unityObject.GetType()) + "'. This likely means that Unity has filled Odin's stored serialization data with garbage, which can randomly happen after upgrading the Unity version of the project, or when otherwise doing things that have a lot of fragile interactions with the asset database. Locating the asset which causes this error log and causing it to reserialize (IE, modifying it and then causing it to be saved to disk) is likely to 'fix' the issue and make this message go away. Experience shows that this issue is particularly likely to occur on prefab instances, and if this is the case, the parent prefab is also under suspicion, and should be re-saved and re-imported. Note that DATA MAY HAVE BEEN LOST, and you should verify with your version control system (you're using one, right?!) that everything is alright, and if not, use it to rollback the asset to recover your data.\n\n\n";
						try
						{
							message += "A delayed error message containing the originating object's name, type and scene/asset path (if applicable) will be scheduled for logging on Unity's main thread. Search for \"DELAYED SERIALIZATION LOG\". This logging callback will also mark the object dirty if it is an asset, hopefully making the issue 'fix' itself. HOWEVER, THERE MAY STILL BE DATA LOSS.\n\n\n";
							EditorApplication_delayCall_Alias += delegate
							{
								string text = "DELAYED SERIALIZATION LOG: Name = " + ((unityObject != null) ? unityObject.name : "(DESTROYED UNITY OBJECT)") + ", Type = " + Sirenix.Serialization.Utilities.TypeExtensions.GetNiceFullName(unityObject.GetType());
								UnityEngine.Object obj2 = unityObject;
								Component component = unityObject as Component;
								if (component != null && component.gameObject.scene.IsValid())
								{
									text = text + ", ScenePath = " + component.gameObject.scene.path;
								}
								if (AssetDatabase.Contains(unityObject))
								{
									string assetPath = AssetDatabase.GetAssetPath(unityObject);
									text = text + ", AssetPath = " + assetPath;
									obj2 = AssetDatabase.LoadMainAssetAtPath(assetPath);
									if (obj2 == null)
									{
										obj2 = unityObject;
									}
									EditorUtility.SetDirty(unityObject);
									AssetDatabase.SaveAssets();
								}
								UnityEngine.Debug.LogError(text, obj2);
							};
						}
						catch
						{
							UnityEngine.Debug.LogWarning("DELAYED SERIALIZATION LOG: Delaying log to main thread failed, likely due to a race condition when subscribing to EditorApplication.delayCall; this cannot be guarded against from our code. Try to provoke the error again and hope to get luckier next time!");
						}
						message = message + "IF YOU HAVE CONSISTENT REPRODUCTION STEPS THAT MAKE THIS ISSUE REOCCUR, please report it at this issue at 'https://bitbucket.org/sirenix/odin-inspector/issues/526', and copy paste this debug message into your comment, along with any potential actions or recent changes in the project that might have happened to cause this message to occur. If the data dump in this message is cut off, please find the editor's log file (see https://docs.unity3d.com/Manual/LogFiles.html) and copy paste the full version of this message from there.\n\n\nData dump:\n\n    Reader type: " + reader.GetType().Name + "\n";
						try
						{
							message = message + "    Data dump: " + reader.GetDataDump();
						}
						finally
						{
							reader.Context.Config.DebugContext.LogError(message);
							skip = true;
						}
					}
					else if (string.IsNullOrEmpty(name))
					{
						reader.Context.Config.DebugContext.LogError("Entry of type \"" + entryType.ToString() + "\" in node \"" + reader.CurrentNodeName + "\" is missing a name.");
						skip = true;
					}
					else if (!members.TryGetValue(name, out member) || (setter = GetCachedUnityMemberSetter(member)) == null)
					{
						skip = true;
					}
					if (skip)
					{
						reader.SkipEntry();
						continue;
					}
					Type expectedType = FormatterUtilities.GetContainedType(member);
					Serializer serializer = Serializer.Get(expectedType);
					try
					{
						object value = serializer.ReadValueWeak(reader);
						setter(ref unityObjectInstance, value);
					}
					catch (Exception exception)
					{
						reader.Context.Config.DebugContext.LogException(exception);
					}
					count++;
					if (count <= 1000)
					{
						continue;
					}
					reader.Context.Config.DebugContext.LogError("Breaking out of infinite reading loop! (Read more than a thousand entries for one type!)");
					break;
				}
			}
			catch (SerializationAbortException innerException)
			{
				throw new SerializationAbortException("Deserialization of type '" + Sirenix.Serialization.Utilities.TypeExtensions.GetNiceFullName(unityObject.GetType()) + "' aborted.", innerException);
			}
			catch (Exception ex)
			{
				UnityEngine.Debug.LogException(new Exception("Exception thrown while deserializing type '" + Sirenix.Serialization.Utilities.TypeExtensions.GetNiceFullName(unityObject.GetType()) + "': " + ex.Message, ex));
			}
		}

		/// <summary>
		/// Not yet documented.
		/// </summary>
		public static List<string> SerializePrefabModifications(List<PrefabModification> modifications, ref List<UnityEngine.Object> referencedUnityObjects)
		{
			if (referencedUnityObjects == null)
			{
				referencedUnityObjects = new List<UnityEngine.Object>();
			}
			else if (referencedUnityObjects.Count > 0)
			{
				referencedUnityObjects.Clear();
			}
			if (modifications == null || modifications.Count == 0)
			{
				return new List<string>();
			}
			modifications.Sort(delegate(PrefabModification a, PrefabModification b)
			{
				int num = a.Path.CompareTo(b.Path);
				if (num == 0)
				{
					if ((a.ModificationType == PrefabModificationType.ListLength || a.ModificationType == PrefabModificationType.Dictionary) && b.ModificationType == PrefabModificationType.Value)
					{
						return 1;
					}
					if (a.ModificationType == PrefabModificationType.Value && (b.ModificationType == PrefabModificationType.ListLength || b.ModificationType == PrefabModificationType.Dictionary))
					{
						return -1;
					}
				}
				return num;
			});
			List<string> result = new List<string>();
			using Cache<SerializationContext> context = Cache<SerializationContext>.Claim();
			using Cache<CachedMemoryStream> stream = CachedMemoryStream.Claim();
			using Cache<JsonDataWriter> writerCache = Cache<JsonDataWriter>.Claim();
			using Cache<UnityReferenceResolver> resolver = Cache<UnityReferenceResolver>.Claim();
			JsonDataWriter writer = writerCache.Value;
			writer.Context = context;
			writer.Stream = stream.Value.MemoryStream;
			writer.PrepareNewSerializationSession();
			writer.FormatAsReadable = false;
			writer.EnableTypeOptimization = false;
			resolver.Value.SetReferencedUnityObjects(referencedUnityObjects);
			writer.Context.IndexReferenceResolver = resolver.Value;
			for (int i = 0; i < modifications.Count; i++)
			{
				PrefabModification mod = modifications[i];
				if (mod.ModificationType == PrefabModificationType.ListLength)
				{
					writer.MarkJustStarted();
					writer.WriteString("path", mod.Path);
					writer.WriteInt32("length", mod.NewLength);
					writer.FlushToStream();
					result.Add(GetStringFromStreamAndReset(stream.Value.MemoryStream));
				}
				else if (mod.ModificationType == PrefabModificationType.Value)
				{
					writer.MarkJustStarted();
					writer.WriteString("path", mod.Path);
					if (mod.ReferencePaths != null && mod.ReferencePaths.Count > 0)
					{
						writer.BeginStructNode("references", null);
						for (int j = 0; j < mod.ReferencePaths.Count; j++)
						{
							writer.WriteString(null, mod.ReferencePaths[j]);
						}
						writer.EndNode("references");
					}
					Serializer<object> serializer = Serializer.Get<object>();
					serializer.WriteValueWeak("value", mod.ModifiedValue, writer);
					writer.FlushToStream();
					result.Add(GetStringFromStreamAndReset(stream.Value.MemoryStream));
				}
				else if (mod.ModificationType == PrefabModificationType.Dictionary)
				{
					writer.MarkJustStarted();
					writer.WriteString("path", mod.Path);
					Serializer.Get<object[]>().WriteValue("add_keys", mod.DictionaryKeysAdded, writer);
					Serializer.Get<object[]>().WriteValue("remove_keys", mod.DictionaryKeysRemoved, writer);
					writer.FlushToStream();
					result.Add(GetStringFromStreamAndReset(stream.Value.MemoryStream));
				}
				writer.Context.ResetInternalReferences();
			}
			return result;
		}

		private static string GetStringFromStreamAndReset(Stream stream)
		{
			byte[] bytes = new byte[stream.Position];
			stream.Position = 0L;
			stream.Read(bytes, 0, bytes.Length);
			stream.Position = 0L;
			return Encoding.UTF8.GetString(bytes);
		}

		/// <summary>
		/// Not yet documented.
		/// </summary>
		public static List<PrefabModification> DeserializePrefabModifications(List<string> modifications, List<UnityEngine.Object> referencedUnityObjects)
		{
			if (modifications == null || modifications.Count == 0)
			{
				return new List<PrefabModification>();
			}
			List<PrefabModification> result = new List<PrefabModification>();
			int longestByteCount = 0;
			for (int i = 0; i < modifications.Count; i++)
			{
				int count = modifications[i].Length * 2;
				if (count > longestByteCount)
				{
					longestByteCount = count;
				}
			}
			using Cache<DeserializationContext> context = Cache<DeserializationContext>.Claim();
			using Cache<CachedMemoryStream> streamCache = CachedMemoryStream.Claim(longestByteCount);
			using Cache<JsonDataReader> readerCache = Cache<JsonDataReader>.Claim();
			using Cache<UnityReferenceResolver> resolver = Cache<UnityReferenceResolver>.Claim();
			MemoryStream stream = streamCache.Value.MemoryStream;
			JsonDataReader reader = readerCache.Value;
			reader.Context = context;
			reader.Stream = stream;
			resolver.Value.SetReferencedUnityObjects(referencedUnityObjects);
			reader.Context.IndexReferenceResolver = resolver.Value;
			for (int j = 0; j < modifications.Count; j++)
			{
				string modStr = modifications[j];
				byte[] bytes = Encoding.UTF8.GetBytes(modStr);
				stream.SetLength(bytes.Length);
				stream.Position = 0L;
				stream.Write(bytes, 0, bytes.Length);
				stream.Position = 0L;
				PrefabModification modification = new PrefabModification();
				reader.PrepareNewSerializationSession();
				EntryType entryType = reader.PeekEntry(out var entryName);
				if (entryType == EntryType.EndOfStream)
				{
					reader.SkipEntry();
				}
				while ((entryType = reader.PeekEntry(out entryName)) != EntryType.EndOfNode && entryType != EntryType.EndOfArray && entryType != EntryType.EndOfStream)
				{
					if (entryName == null)
					{
						UnityEngine.Debug.LogError("Unexpected entry of type " + entryType.ToString() + " without a name.");
						reader.SkipEntry();
					}
					else if (entryName.Equals("path", StringComparison.InvariantCultureIgnoreCase))
					{
						reader.ReadString(out modification.Path);
					}
					else if (entryName.Equals("length", StringComparison.InvariantCultureIgnoreCase))
					{
						reader.ReadInt32(out modification.NewLength);
						modification.ModificationType = PrefabModificationType.ListLength;
					}
					else if (entryName.Equals("references", StringComparison.InvariantCultureIgnoreCase))
					{
						modification.ReferencePaths = new List<string>();
						reader.EnterNode(out var _);
						while (reader.PeekEntry(out entryName) == EntryType.String)
						{
							reader.ReadString(out var path);
							modification.ReferencePaths.Add(path);
						}
						reader.ExitNode();
					}
					else if (entryName.Equals("value", StringComparison.InvariantCultureIgnoreCase))
					{
						modification.ModifiedValue = Serializer.Get<object>().ReadValue(reader);
						modification.ModificationType = PrefabModificationType.Value;
					}
					else if (entryName.Equals("add_keys", StringComparison.InvariantCultureIgnoreCase))
					{
						modification.DictionaryKeysAdded = Serializer.Get<object[]>().ReadValue(reader);
						modification.ModificationType = PrefabModificationType.Dictionary;
					}
					else if (entryName.Equals("remove_keys", StringComparison.InvariantCultureIgnoreCase))
					{
						modification.DictionaryKeysRemoved = Serializer.Get<object[]>().ReadValue(reader);
						modification.ModificationType = PrefabModificationType.Dictionary;
					}
					else
					{
						UnityEngine.Debug.LogError("Unexpected entry name '" + entryName + "' while deserializing prefab modifications.");
						reader.SkipEntry();
					}
				}
				if (modification.Path != null)
				{
					result.Add(modification);
				}
			}
			return result;
		}

		/// <summary>
		/// Not yet documented.
		/// </summary>
		public static void RegisterPrefabModificationsChange(UnityEngine.Object unityObject, List<PrefabModification> modifications)
		{
			if (unityObject == null)
			{
				throw new ArgumentNullException("unityObject");
			}
			PrefabModificationCache.CachePrefabModifications(unityObject, modifications);
			RegisteredPrefabModifications[unityObject] = modifications;
		}

		/// <summary>
		/// Creates an object with default values initialized in the style of Unity; strings will be "", classes will be instantiated recursively with default values, and so on.
		/// </summary>
		public static object CreateDefaultUnityInitializedObject(Type type)
		{
			return CreateDefaultUnityInitializedObject(type, 0);
		}

		private static object CreateDefaultUnityInitializedObject(Type type, int depth)
		{
			if (depth > 5)
			{
				return null;
			}
			if (!GuessIfUnityWillSerialize(type))
			{
				if (!type.IsValueType)
				{
					return null;
				}
				return Activator.CreateInstance(type);
			}
			if (type == typeof(string))
			{
				return "";
			}
			if (type.IsEnum)
			{
				Array values = Enum.GetValues(type);
				if (values.Length <= 0)
				{
					return Enum.ToObject(type, 0);
				}
				return values.GetValue(0);
			}
			if (type.IsPrimitive)
			{
				return Activator.CreateInstance(type);
			}
			if (type.IsArray)
			{
				return Array.CreateInstance(type.GetElementType(), 0);
			}
			if (Sirenix.Serialization.Utilities.TypeExtensions.ImplementsOpenGenericClass(type, typeof(List<>)) || typeof(UnityEventBase).IsAssignableFrom(type))
			{
				try
				{
					return Activator.CreateInstance(type);
				}
				catch
				{
					return null;
				}
			}
			if (typeof(UnityEngine.Object).IsAssignableFrom(type))
			{
				return null;
			}
			if ((type.Assembly.GetName().Name.StartsWith("UnityEngine") || type.Assembly.GetName().Name.StartsWith("UnityEditor")) && type.GetConstructor(Type.EmptyTypes) != null)
			{
				try
				{
					return Activator.CreateInstance(type);
				}
				catch (Exception exception)
				{
					UnityEngine.Debug.LogException(exception);
					return null;
				}
			}
			if (type.GetConstructor(Type.EmptyTypes) != null)
			{
				return Activator.CreateInstance(type);
			}
			object value = FormatterServices.GetUninitializedObject(type);
			FieldInfo[] fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			foreach (FieldInfo field in fields)
			{
				if (GuessIfUnityWillSerialize(field))
				{
					field.SetValue(value, CreateDefaultUnityInitializedObject(field.FieldType, depth + 1));
				}
			}
			return value;
		}

		private static void ApplyPrefabModifications(UnityEngine.Object unityObject, List<string> modificationData, List<UnityEngine.Object> referencedUnityObjects)
		{
			if (unityObject == null)
			{
				throw new ArgumentNullException("unityObject");
			}
			if (modificationData == null || modificationData.Count == 0)
			{
				PrefabModificationCache.CachePrefabModifications(unityObject, new List<PrefabModification>());
				return;
			}
			List<PrefabModification> modifications = DeserializePrefabModifications(modificationData, referencedUnityObjects);
			PrefabModificationCache.CachePrefabModifications(unityObject, modifications);
			for (int i = 0; i < modifications.Count; i++)
			{
				PrefabModification mod = modifications[i];
				try
				{
					mod.Apply(unityObject);
				}
				catch (Exception exception)
				{
					UnityEngine.Debug.Log("The following exception was thrown when trying to apply a prefab modification for path '" + mod.Path + "':");
					UnityEngine.Debug.LogException(exception);
				}
			}
		}

		private static Sirenix.Serialization.Utilities.WeakValueGetter GetCachedUnityMemberGetter(MemberInfo member)
		{
			lock (UnityMemberGetters)
			{
				if (!UnityMemberGetters.TryGetValue(member, out var result))
				{
					result = ((member is FieldInfo) ? Sirenix.Serialization.Utilities.EmitUtilities.CreateWeakInstanceFieldGetter(member.DeclaringType, member as FieldInfo) : ((!(member is PropertyInfo)) ? ((Sirenix.Serialization.Utilities.WeakValueGetter)delegate(ref object instance)
					{
						return FormatterUtilities.GetMemberValue(member, instance);
					}) : Sirenix.Serialization.Utilities.EmitUtilities.CreateWeakInstancePropertyGetter(member.DeclaringType, member as PropertyInfo)));
					UnityMemberGetters.Add(member, result);
				}
				return result;
			}
		}

		private static Sirenix.Serialization.Utilities.WeakValueSetter GetCachedUnityMemberSetter(MemberInfo member)
		{
			lock (UnityMemberSetters)
			{
				if (!UnityMemberSetters.TryGetValue(member, out var result))
				{
					result = ((member is FieldInfo) ? Sirenix.Serialization.Utilities.EmitUtilities.CreateWeakInstanceFieldSetter(member.DeclaringType, member as FieldInfo) : ((!(member is PropertyInfo)) ? ((Sirenix.Serialization.Utilities.WeakValueSetter)delegate(ref object instance, object value)
					{
						FormatterUtilities.SetMemberValue(member, instance, value);
					}) : Sirenix.Serialization.Utilities.EmitUtilities.CreateWeakInstancePropertySetter(member.DeclaringType, member as PropertyInfo)));
					UnityMemberSetters.Add(member, result);
				}
				return result;
			}
		}

		private static ICache GetCachedUnityWriter(DataFormat format, Stream stream, SerializationContext context)
		{
			ICache cache;
			switch (format)
			{
			case DataFormat.Binary:
			{
				Cache<BinaryDataWriter> c2 = Cache<BinaryDataWriter>.Claim();
				c2.Value.Stream = stream;
				cache = c2;
				break;
			}
			case DataFormat.JSON:
			{
				Cache<JsonDataWriter> c = Cache<JsonDataWriter>.Claim();
				c.Value.Stream = stream;
				cache = c;
				break;
			}
			case DataFormat.Nodes:
				throw new InvalidOperationException("Don't do this for nodes!");
			default:
				throw new NotImplementedException(format.ToString());
			}
			(cache.Value as IDataWriter).Context = context;
			return cache;
		}

		private static ICache GetCachedUnityReader(DataFormat format, Stream stream, DeserializationContext context)
		{
			ICache cache;
			switch (format)
			{
			case DataFormat.Binary:
			{
				Cache<BinaryDataReader> c2 = Cache<BinaryDataReader>.Claim();
				c2.Value.Stream = stream;
				cache = c2;
				break;
			}
			case DataFormat.JSON:
			{
				Cache<JsonDataReader> c = Cache<JsonDataReader>.Claim();
				c.Value.Stream = stream;
				cache = c;
				break;
			}
			case DataFormat.Nodes:
				throw new InvalidOperationException("Don't do this for nodes!");
			default:
				throw new NotImplementedException(format.ToString());
			}
			(cache.Value as IDataReader).Context = context;
			return cache;
		}

		private static T ConvertDelegate<T>(this Delegate src)
		{
			if ((object)src == null || src.GetType() == typeof(T))
			{
				return (T)(object)src;
			}
			if (src.GetInvocationList().Count() == 1)
			{
				return (T)(object)Delegate.CreateDelegate(typeof(T), src.Target, src.Method);
			}
			return (T)(object)src.GetInvocationList().Aggregate(null, (Delegate current, Delegate d) => Delegate.Combine(current, (Delegate)(object)d.ConvertDelegate<T>()));
		}
	}
}
