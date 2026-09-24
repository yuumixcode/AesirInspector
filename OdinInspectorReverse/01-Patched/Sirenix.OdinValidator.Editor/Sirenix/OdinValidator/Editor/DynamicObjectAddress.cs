using System;
using System.Collections.Generic;
using Sirenix.Reflection.Editor;
using UnityEngine;

namespace Sirenix.OdinValidator.Editor
{
	public class DynamicObjectAddress : ISerializationCallbackReceiver, IEquatable<DynamicObjectAddress>
	{
		private static object LOCK = new object();

		private static Dictionary<OdinEntityId, WeakReference<DynamicObjectAddress>> lookupByID = new Dictionary<OdinEntityId, WeakReference<DynamicObjectAddress>>();

		private static Dictionary<ObjectAddress, WeakReference<DynamicObjectAddress>> lookupByAddress = new Dictionary<ObjectAddress, WeakReference<DynamicObjectAddress>>();

		[NonSerialized]
		public static readonly DynamicObjectAddress Unknown = new DynamicObjectAddress
		{
			address = ObjectAddress.Unknown
		};

		[NonSerialized]
		private OdinEntityId entityId;

		[NonSerialized]
		private ObjectAddress address;

		[NonSerialized]
		private UnityEngine.Object obj;

		[NonSerialized]
		private bool isBroken;

		[Obsolete("Use LatestEntityId instead.", false)]
		public int LatestInstanceID => OdinEntityId.Internal.ToInstanceId(entityId);

		public OdinEntityId LatestEntityId => entityId;

		public ObjectAddress LatestAddress => address;

		public bool IsBroken => isBroken;

		public bool IsUnloaded
		{
			get
			{
				if (obj == null && !IsBroken)
				{
					return this != Unknown;
				}
				return false;
			}
		}

		private DynamicObjectAddress()
		{
		}

		~DynamicObjectAddress()
		{
			if (this == Unknown || isBroken)
			{
				return;
			}
			lock (LOCK)
			{
				if ((!lookupByID.TryGetValue(entityId, out var currentRef) || !currentRef.TryGetTarget(out var current)) && (!lookupByAddress.TryGetValue(address, out currentRef) || !currentRef.TryGetTarget(out current)))
				{
					lookupByID.Remove(entityId);
					lookupByAddress.Remove(address);
				}
			}
		}

		public void Refresh()
		{
			if (IsUnloaded)
			{
				TryGetObjectReference(openSceneIfNeeded: false, autoSaveIfOpenScene: false, out var _, out var _);
			}
		}

		public static bool TryGet(OdinEntityId entityId, out DynamicObjectAddress address)
		{
			lock (LOCK)
			{
				address = null;
				WeakReference<DynamicObjectAddress> reference;
				return lookupByID.TryGetValue(entityId, out reference) && reference.TryGetTarget(out address);
			}
		}

		[Obsolete("Use TryGet(OdinEntityId, out DynamicObjectAddress) instead.", false)]
		public static bool TryGet(int instanceID, out DynamicObjectAddress address)
		{
			return TryGet(OdinEntityId.Internal.FromInstanceId(instanceID), out address);
		}

		public static bool TryGet(ObjectAddress objectAddress, out DynamicObjectAddress address)
		{
			lock (LOCK)
			{
				address = null;
				WeakReference<DynamicObjectAddress> reference;
				return lookupByAddress.TryGetValue(objectAddress, out reference) && reference.TryGetTarget(out address);
			}
		}

		public static DynamicObjectAddress CreateBroken(ObjectAddress address)
		{
			if (!address.IsBroken)
			{
				throw new ArgumentException("You must pass a broken ObjectAddress to this method.");
			}
			return new DynamicObjectAddress
			{
				address = address,
				entityId = default(OdinEntityId),
				isBroken = true
			};
		}

		public static DynamicObjectAddress GetOrCreate(OdinEntityId entityId, ObjectAddress address)
		{
			if (address.IsBroken)
			{
				throw new ArgumentException("You cannot pass a broken ObjectAddress to this method.");
			}
			UnityEngine.Object obj = entityId.ToObject();
			if (obj == null)
			{
				throw new Exception($"Given Entity Id '{entityId}' is not currently alive and gettable via OdinEntityId.OdinEntityIDToObject.");
			}
			return GetOrCreate(obj, address);
		}

		[Obsolete("Use GetOrCreate(OdinEntityId, ObjectAddress) instead.", false)]
		public static DynamicObjectAddress GetOrCreate(int instanceID, ObjectAddress address)
		{
			return GetOrCreate(OdinEntityId.Internal.FromInstanceId(instanceID), address);
		}

		public static DynamicObjectAddress GetOrCreate(UnityEngine.Object obj, ObjectAddress address)
		{
			if (obj == null)
			{
				throw new Exception("Given object to create dynamic address for is not currently alive. An object needs to be alive to create a dynamic address for it, even if it is destroyed later.");
			}
			if (address.IsBroken)
			{
				throw new ArgumentException("You cannot pass a broken ObjectAddress to this method.");
			}
			OdinEntityId entityId = OdinEntityId.FromObject(obj);
			lock (LOCK)
			{
				DynamicObjectAddress result;
				if (lookupByID.TryGetValue(entityId, out var reference))
				{
					if (reference.TryGetTarget(out result))
					{
						if (result.address != address)
						{
							lookupByAddress.Remove(result.address);
							lookupByAddress[address] = reference;
							result.address = address;
						}
						result.obj = obj;
						return result;
					}
					lookupByID.Remove(entityId);
					lookupByAddress.Remove(address);
				}
				else if (lookupByAddress.TryGetValue(address, out reference))
				{
					if (reference.TryGetTarget(out result))
					{
						if (result.entityId != entityId)
						{
							lookupByID.Remove(result.entityId);
							lookupByID[entityId] = reference;
							result.entityId = entityId;
						}
						result.obj = obj;
						return result;
					}
					lookupByID.Remove(entityId);
					lookupByAddress.Remove(address);
				}
				result = new DynamicObjectAddress
				{
					obj = obj,
					entityId = entityId,
					address = address
				};
				if (reference == null)
				{
					reference = new WeakReference<DynamicObjectAddress>(result, trackResurrection: false);
				}
				else
				{
					reference.SetTarget(result);
				}
				lookupByID[entityId] = reference;
				lookupByAddress[address] = reference;
				return result;
			}
		}

		public static List<DynamicObjectAddress> GetAllExistingAddressesForAssetGuid(string guid)
		{
			lock (LOCK)
			{
				List<DynamicObjectAddress> result = new List<DynamicObjectAddress>();
				foreach (KeyValuePair<ObjectAddress, WeakReference<DynamicObjectAddress>> pair in lookupByAddress)
				{
					if (pair.Key.AssetGUID == guid && pair.Value.TryGetTarget(out var target))
					{
						result.Add(target);
					}
				}
				return result;
			}
		}

		public void OnAfterDeserialize()
		{
			throw new NotSupportedException("It is not allowed to serialize DynamicObjectAddress.");
		}

		public void OnBeforeSerialize()
		{
			throw new NotSupportedException("It is not allowed to serialize DynamicObjectAddress.");
		}

		public bool TryGetObjectReference(bool openSceneIfNeeded, bool autoSaveIfOpenScene, out UnityEngine.Object result, out string errorMessage, out UnityEngine.Object closestObject)
		{
			return TryGetObjectReference(openSceneIfNeeded, autoSaveIfOpenScene, out result, out errorMessage, out closestObject, findClosest: true);
		}

		public bool TryGetObjectReference(bool openSceneIfNeeded, bool autoSaveIfOpenScene, out UnityEngine.Object result, out string errorMessage)
		{
			UnityEngine.Object closestObject;
			return TryGetObjectReference(openSceneIfNeeded, autoSaveIfOpenScene, out result, out errorMessage, out closestObject, findClosest: false);
		}

		private bool TryGetObjectReference(bool openSceneIfNeeded, bool autoSaveIfOpenScene, out UnityEngine.Object result, out string errorMessage, out UnityEngine.Object closestObject, bool findClosest)
		{
			closestObject = null;
			if (this == Unknown || (isBroken && !findClosest))
			{
				result = null;
				errorMessage = null;
				return false;
			}
			errorMessage = null;
			lock (LOCK)
			{
				if (obj == null)
				{
					obj = entityId.ToObject();
					if (obj == null)
					{
						if (findClosest)
						{
							address.TryGetObjectReference(openSceneIfNeeded, autoSaveIfOpenScene, out obj, out errorMessage, out closestObject);
						}
						else
						{
							address.TryGetObjectReference(openSceneIfNeeded, autoSaveIfOpenScene, out obj, out errorMessage);
						}
					}
					if (obj != null)
					{
						OdinEntityId newId = OdinEntityId.FromObject(obj);
						if (newId != entityId)
						{
							OdinEntityId oldID = entityId;
							entityId = newId;
							if (lookupByID.TryGetValue(oldID, out var reference))
							{
								lookupByID.Remove(oldID);
								lookupByID[entityId] = reference;
							}
							else if (!lookupByAddress.TryGetValue(address, out reference))
							{
								reference = new WeakReference<DynamicObjectAddress>(this, trackResurrection: false);
								lookupByID[entityId] = reference;
								lookupByAddress[address] = reference;
							}
							else
							{
								lookupByID[entityId] = reference;
							}
						}
					}
				}
			}
			if (findClosest && (bool)obj)
			{
				closestObject = obj;
			}
			result = obj;
			return obj != null;
		}

		public override bool Equals(object obj)
		{
			if (obj is DynamicObjectAddress other)
			{
				return this == other;
			}
			return false;
		}

		public bool Equals(DynamicObjectAddress other)
		{
			return this == other;
		}

		public static bool operator ==(DynamicObjectAddress a, DynamicObjectAddress b)
		{
			if ((object)a == b)
			{
				return true;
			}
			if ((object)a == null != ((object)b == null))
			{
				return false;
			}
			lock (LOCK)
			{
				return a.IsBroken == b.IsBroken && a.LatestEntityId == b.LatestEntityId && a.LatestAddress == b.LatestAddress;
			}
		}

		public static bool operator !=(DynamicObjectAddress a, DynamicObjectAddress b)
		{
			return !(a == b);
		}

		public override int GetHashCode()
		{
			return LatestAddress?.GetHashCode() ?? 0;
		}
	}
}
