using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using UnityEditor;
using UnityEngine;

namespace Sirenix.Reflection.Editor
{
	[Serializable]
	[StructLayout(LayoutKind.Explicit, Size = 8)]
	public struct OdinEntityId : IEquatable<OdinEntityId>, IComparable<OdinEntityId>
	{
		public static class Internal
		{
			private const string NOT_SUPPORTED_EXCEPTION_BAD_INSTANCE_ID_USAGE_MSG = "[Odin] Since Unity's current ID system does not use instance IDs, an instance ID cannot be reliably bridged to it. If this was called indirectly through one of Odin's public APIs, refer to any overloads that use OdinEntityId instead.";

			public static int ToInstanceId(OdinEntityId entityId)
			{
				if (Backend != OdinEntityIdBackend.InstanceId)
				{
					throw new NotSupportedException("[Odin] Since Unity's current ID system does not use instance IDs, an instance ID cannot be reliably bridged to it. If this was called indirectly through one of Odin's public APIs, refer to any overloads that use OdinEntityId instead.");
				}
				return entityId.Int32Data;
			}

			public static OdinEntityId FromInstanceId(int instanceID)
			{
				if (Backend != OdinEntityIdBackend.InstanceId)
				{
					throw new NotSupportedException("[Odin] Since Unity's current ID system does not use instance IDs, an instance ID cannot be reliably bridged to it. If this was called indirectly through one of Odin's public APIs, refer to any overloads that use OdinEntityId instead.");
				}
				return new OdinEntityId
				{
					Int32Data = instanceID
				};
			}

			public static int ToInt32(OdinEntityId entityId)
			{
				return entityId.Int32Data;
			}
		}

		internal enum SourceKind
		{
			Value,
			Address
		}

		public const int Size = 8;

		[FieldOffset(0)]
		[NonSerialized]
		internal int Int32Data;

		[FieldOffset(0)]
		[SerializeField]
		internal ulong UlongData;

		internal static readonly OdinEntityIdBackend Backend;

		internal static readonly int EntityIdSize;

		private static Func<OdinEntityId, UnityEngine.Object> EntityIDToObjectFunc;

		private static Func<UnityEngine.Object, OdinEntityId> Object_GetEntityId_Func;

		private static Func<string, OdinEntityId, OdinEntityId> GetSessionStateId_Func;

		private static Action<string, OdinEntityId> SetSessionStateId_Func;

		private static Func<OdinEntityId, bool> AssetDatabase_Contains_Func;

		private static Func<OdinEntityId, string> AssetDatabase_GetAssetPath_Func;

		private static Func<OdinEntityId, string> EntityIdToStringFunc;

		public static OdinEntityId None => new OdinEntityId
		{
			UlongData = 0uL
		};

		public bool IsValid => UlongData != 0;

		public OdinEntityId(UnityEngine.Object obj)
		{
			this = FromObject(obj);
		}

		static OdinEntityId()
		{
			Backend = OdinEntityIdBackend.Invalid;
			EntityIdSize = 0;
			EntityIdToStringFunc = null;
			Type entityIdType = typeof(UnityEngine.Object).Assembly.GetType("UnityEngine.EntityId");
			if (entityIdType != null)
			{
				StructLayoutAttribute structLayout = entityIdType.StructLayoutAttribute;
				if (structLayout == null)
				{
					Debug.LogError("Couldn't find a StructLayout attribute for UnityEngine.EntityId in this version of Unity. Some of Odin's APIs related to entity IDs will not function correctly in this version of Unity.");
					return;
				}
				EntityIdSize = structLayout.Size;
				if (structLayout.Size != 4 && structLayout.Size != 8)
				{
					Debug.LogError(string.Format("{0} is {1} in this version of Unity, instead of the expected 4 or 8. Some of Odin's APIs related to entity IDs will not function correctly in this version of Unity.", "UnityEngine.EntityId", structLayout.Size));
					return;
				}
				MethodInfo entityIdToObjectMethod = typeof(EditorUtility).GetMethod("EntityIdToObject", BindingFlags.Static | BindingFlags.Public, null, new Type[1] { entityIdType }, null);
				MethodInfo getEntityIdMethod = typeof(UnityEngine.Object).GetMethod("GetEntityId", BindingFlags.Instance | BindingFlags.Public, null, new Type[0], null);
				MethodInfo sessionState_GetEntityId_Method = typeof(SessionState).GetMethod("GetEntityId", BindingFlags.Static | BindingFlags.Public, null, new Type[2]
				{
					typeof(string),
					entityIdType
				}, null);
				MethodInfo sessionState_SetEntityId_Method = typeof(SessionState).GetMethod("SetEntityId", BindingFlags.Static | BindingFlags.Public, null, new Type[2]
				{
					typeof(string),
					entityIdType
				}, null);
				MethodInfo assetDatabase_Contains_EntityId_Method = typeof(AssetDatabase).GetMethod("Contains", BindingFlags.Static | BindingFlags.Public, null, new Type[1] { entityIdType }, null);
				MethodInfo assetDatabase_GetAssetPath_EntityId_Method = typeof(AssetDatabase).GetMethod("GetAssetPath", BindingFlags.Static | BindingFlags.Public, null, new Type[1] { entityIdType }, null);
				bool useIntSessionState = EntityIdSize == 4;
				if (entityIdToObjectMethod != null && getEntityIdMethod != null && (useIntSessionState || (sessionState_GetEntityId_Method != null && sessionState_SetEntityId_Method != null)) && assetDatabase_Contains_EntityId_Method != null && assetDatabase_GetAssetPath_EntityId_Method != null)
				{
					DynamicMethod entityIdToObjectMethodGenerator = new DynamicMethod("Sirenix.Utilities.Editor.EntityIdUtility.OdinEntityIdToObject_Emitted", typeof(UnityEngine.Object), new Type[1] { typeof(OdinEntityId) }, restrictedSkipVisibility: true);
					ILGenerator il = entityIdToObjectMethodGenerator.GetILGenerator();
					EmitBitCastToEvalStack(il, typeof(OdinEntityId), entityIdType, 8, EntityIdSize, SourceKind.Address, delegate
					{
						il.Emit(OpCodes.Ldarga_S, (byte)0);
					});
					il.Emit(OpCodes.Call, entityIdToObjectMethod);
					il.Emit(OpCodes.Ret);
					EntityIDToObjectFunc = (Func<OdinEntityId, UnityEngine.Object>)entityIdToObjectMethodGenerator.CreateDelegate(typeof(Func<OdinEntityId, UnityEngine.Object>), null);
					DynamicMethod getEntityIdMethodGenerator = new DynamicMethod("Sirenix.Utilities.Editor.EntityIdUtility.GetEntityId_Emitted", typeof(OdinEntityId), new Type[1] { typeof(UnityEngine.Object) }, restrictedSkipVisibility: true);
					ILGenerator il2 = getEntityIdMethodGenerator.GetILGenerator();
					EmitBitCastToEvalStack(il2, entityIdType, typeof(OdinEntityId), EntityIdSize, 8, SourceKind.Value, delegate
					{
						il2.Emit(OpCodes.Ldarg_0);
						il2.Emit(OpCodes.Callvirt, getEntityIdMethod);
					});
					il2.Emit(OpCodes.Ret);
					Object_GetEntityId_Func = (Func<UnityEngine.Object, OdinEntityId>)getEntityIdMethodGenerator.CreateDelegate(typeof(Func<UnityEngine.Object, OdinEntityId>), null);
					if (useIntSessionState)
					{
						GetSessionStateId_Func = (string key, OdinEntityId id) => new OdinEntityId
						{
							Int32Data = SessionState.GetInt(key, id.Int32Data)
						};
					}
					else
					{
						DynamicMethod getSessionStateIdMethodGenerator = new DynamicMethod("Sirenix.Utilities.Editor.EntityIdUtility.GetSessionStateId_Emitted", typeof(OdinEntityId), new Type[2]
						{
							typeof(string),
							typeof(OdinEntityId)
						}, restrictedSkipVisibility: true);
						ILGenerator il3 = getSessionStateIdMethodGenerator.GetILGenerator();
						il3.Emit(OpCodes.Ldarg_0);
						EmitBitCastToEvalStack(il3, typeof(OdinEntityId), entityIdType, 8, EntityIdSize, SourceKind.Address, delegate
						{
							il3.Emit(OpCodes.Ldarga_S, (byte)1);
						});
						EmitBitCastToEvalStack(il3, entityIdType, typeof(OdinEntityId), EntityIdSize, 8, SourceKind.Value, delegate
						{
							il3.Emit(OpCodes.Call, sessionState_GetEntityId_Method);
						});
						il3.Emit(OpCodes.Ret);
						GetSessionStateId_Func = (Func<string, OdinEntityId, OdinEntityId>)getSessionStateIdMethodGenerator.CreateDelegate(typeof(Func<string, OdinEntityId, OdinEntityId>), null);
					}
					if (useIntSessionState)
					{
						SetSessionStateId_Func = delegate(string key, OdinEntityId id)
						{
							SessionState.SetInt(key, id.Int32Data);
						};
					}
					else
					{
						DynamicMethod setSessionStateIdMethodGenerator = new DynamicMethod("Sirenix.Utilities.Editor.EntityIdUtility.SetSessionStateId_Emitted", null, new Type[2]
						{
							typeof(string),
							typeof(OdinEntityId)
						}, restrictedSkipVisibility: true);
						ILGenerator il4 = setSessionStateIdMethodGenerator.GetILGenerator();
						il4.Emit(OpCodes.Ldarg_0);
						EmitBitCastToEvalStack(il4, typeof(OdinEntityId), entityIdType, 8, EntityIdSize, SourceKind.Address, delegate
						{
							il4.Emit(OpCodes.Ldarga_S, (byte)1);
						});
						il4.Emit(OpCodes.Call, sessionState_SetEntityId_Method);
						il4.Emit(OpCodes.Ret);
						SetSessionStateId_Func = (Action<string, OdinEntityId>)setSessionStateIdMethodGenerator.CreateDelegate(typeof(Action<string, OdinEntityId>), null);
					}
					DynamicMethod assetDatabaseContainsMethodGenerator = new DynamicMethod("Sirenix.Utilities.Editor.EntityIdUtility.AssetDatabase_Contains_Emitted", typeof(bool), new Type[1] { typeof(OdinEntityId) }, restrictedSkipVisibility: true);
					ILGenerator il5 = assetDatabaseContainsMethodGenerator.GetILGenerator();
					EmitBitCastToEvalStack(il5, typeof(OdinEntityId), entityIdType, 8, EntityIdSize, SourceKind.Address, delegate
					{
						il5.Emit(OpCodes.Ldarga_S, (byte)0);
					});
					il5.Emit(OpCodes.Call, assetDatabase_Contains_EntityId_Method);
					il5.Emit(OpCodes.Ret);
					AssetDatabase_Contains_Func = (Func<OdinEntityId, bool>)assetDatabaseContainsMethodGenerator.CreateDelegate(typeof(Func<OdinEntityId, bool>), null);
					DynamicMethod assetDatabaseGetAssetPathMethodGenerator = new DynamicMethod("Sirenix.Utilities.Editor.EntityIdUtility.AssetDatabase_GetAssetPath_Emitted", typeof(string), new Type[1] { typeof(OdinEntityId) }, restrictedSkipVisibility: true);
					ILGenerator il6 = assetDatabaseGetAssetPathMethodGenerator.GetILGenerator();
					EmitBitCastToEvalStack(il6, typeof(OdinEntityId), entityIdType, 8, EntityIdSize, SourceKind.Address, delegate
					{
						il6.Emit(OpCodes.Ldarga_S, (byte)0);
					});
					il6.Emit(OpCodes.Call, assetDatabase_GetAssetPath_EntityId_Method);
					il6.Emit(OpCodes.Ret);
					AssetDatabase_GetAssetPath_Func = (Func<OdinEntityId, string>)assetDatabaseGetAssetPathMethodGenerator.CreateDelegate(typeof(Func<OdinEntityId, string>), null);
					MethodInfo toStringMethod = null;
					MethodInfo[] declaredMethods = entityIdType.GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public);
					foreach (MethodInfo method in declaredMethods)
					{
						if (!(method.Name != "ToString") && method.GetParameters().Length == 0)
						{
							toStringMethod = method;
							break;
						}
					}
					if (toStringMethod != null)
					{
						DynamicMethod toStringGenerator = new DynamicMethod("Sirenix.Utilities.Editor.EntityIdUtility.EntityIdToStringFromOdinEntityId_Emitted", typeof(string), new Type[1] { typeof(OdinEntityId) }, restrictedSkipVisibility: true);
						ILGenerator il7 = toStringGenerator.GetILGenerator();
						il7.Emit(OpCodes.Ldarga_S, (byte)0);
						il7.Emit(OpCodes.Constrained, entityIdType);
						il7.Emit(OpCodes.Callvirt, toStringMethod);
						il7.Emit(OpCodes.Ret);
						EntityIdToStringFunc = (Func<OdinEntityId, string>)toStringGenerator.CreateDelegate(typeof(Func<OdinEntityId, string>), null);
					}
					Backend = OdinEntityIdBackend.EntityId;
				}
			}
			if (Backend != OdinEntityIdBackend.Invalid)
			{
				return;
			}
			MethodInfo instanceIdToObjectMethod = typeof(EditorUtility).GetMethod("InstanceIDToObject", BindingFlags.Static | BindingFlags.Public);
			MethodInfo getInstanceIdMethod = typeof(UnityEngine.Object).GetMethod("GetInstanceID", BindingFlags.Instance | BindingFlags.Public, null, new Type[0], null);
			MethodInfo assetDatabase_Contains_InstanceId_Method = typeof(AssetDatabase).GetMethod("Contains", BindingFlags.Static | BindingFlags.Public, null, new Type[1] { typeof(int) }, null);
			MethodInfo assetDatabase_GetAssetPath_InstanceId_Method = typeof(AssetDatabase).GetMethod("GetAssetPath", BindingFlags.Static | BindingFlags.Public, null, new Type[1] { typeof(int) }, null);
			if (instanceIdToObjectMethod != null && getInstanceIdMethod != null && assetDatabase_Contains_InstanceId_Method != null && assetDatabase_GetAssetPath_InstanceId_Method != null)
			{
				DynamicMethod entityIdToObjectMethodGenerator2 = new DynamicMethod("Sirenix.Utilities.Editor.EntityIdUtility.OdinEntityIdToObject_Emitted", typeof(UnityEngine.Object), new Type[1] { typeof(OdinEntityId) }, restrictedSkipVisibility: true);
				ILGenerator il8 = entityIdToObjectMethodGenerator2.GetILGenerator();
				EmitBitCastToEvalStack(il8, typeof(OdinEntityId), typeof(int), 8, 4, SourceKind.Address, delegate
				{
					il8.Emit(OpCodes.Ldarga_S, (byte)0);
				});
				il8.Emit(OpCodes.Call, instanceIdToObjectMethod);
				il8.Emit(OpCodes.Ret);
				EntityIDToObjectFunc = (Func<OdinEntityId, UnityEngine.Object>)entityIdToObjectMethodGenerator2.CreateDelegate(typeof(Func<OdinEntityId, UnityEngine.Object>), null);
				DynamicMethod getEntityIdMethodGenerator2 = new DynamicMethod("Sirenix.Utilities.Editor.EntityIdUtility.GetEntityId_Emitted", typeof(OdinEntityId), new Type[1] { typeof(UnityEngine.Object) }, restrictedSkipVisibility: true);
				ILGenerator il9 = getEntityIdMethodGenerator2.GetILGenerator();
				EmitBitCastToEvalStack(il9, typeof(int), typeof(OdinEntityId), 4, 8, SourceKind.Value, delegate
				{
					il9.Emit(OpCodes.Ldarg_0);
					il9.Emit(OpCodes.Callvirt, getInstanceIdMethod);
				});
				il9.Emit(OpCodes.Ret);
				Object_GetEntityId_Func = (Func<UnityEngine.Object, OdinEntityId>)getEntityIdMethodGenerator2.CreateDelegate(typeof(Func<UnityEngine.Object, OdinEntityId>), null);
				GetSessionStateId_Func = (string key, OdinEntityId id) => new OdinEntityId
				{
					Int32Data = SessionState.GetInt(key, id.Int32Data)
				};
				SetSessionStateId_Func = delegate(string key, OdinEntityId id)
				{
					SessionState.SetInt(key, id.Int32Data);
				};
				DynamicMethod assetDatabaseContainsMethodGenerator2 = new DynamicMethod("Sirenix.Utilities.Editor.EntityIdUtility.AssetDatabase_Contains_Emitted", typeof(bool), new Type[1] { typeof(OdinEntityId) }, restrictedSkipVisibility: true);
				ILGenerator il10 = assetDatabaseContainsMethodGenerator2.GetILGenerator();
				EmitBitCastToEvalStack(il10, typeof(OdinEntityId), typeof(int), 8, 4, SourceKind.Address, delegate
				{
					il10.Emit(OpCodes.Ldarga_S, (byte)0);
				});
				il10.Emit(OpCodes.Call, assetDatabase_Contains_InstanceId_Method);
				il10.Emit(OpCodes.Ret);
				AssetDatabase_Contains_Func = (Func<OdinEntityId, bool>)assetDatabaseContainsMethodGenerator2.CreateDelegate(typeof(Func<OdinEntityId, bool>), null);
				DynamicMethod assetDatabaseGetAssetPathMethodGenerator2 = new DynamicMethod("Sirenix.Utilities.Editor.EntityIdUtility.AssetDatabase_GetAssetPath_Emitted", typeof(string), new Type[1] { typeof(OdinEntityId) }, restrictedSkipVisibility: true);
				ILGenerator il11 = assetDatabaseGetAssetPathMethodGenerator2.GetILGenerator();
				EmitBitCastToEvalStack(il11, typeof(OdinEntityId), typeof(int), 8, 4, SourceKind.Address, delegate
				{
					il11.Emit(OpCodes.Ldarga_S, (byte)0);
				});
				il11.Emit(OpCodes.Call, assetDatabase_GetAssetPath_InstanceId_Method);
				il11.Emit(OpCodes.Ret);
				AssetDatabase_GetAssetPath_Func = (Func<OdinEntityId, string>)assetDatabaseGetAssetPathMethodGenerator2.CreateDelegate(typeof(Func<OdinEntityId, string>), null);
				Backend = OdinEntityIdBackend.InstanceId;
				EntityIdSize = 4;
			}
			else
			{
				Debug.LogError("Odin could not find either EditorUtility.EntityIdToObject(EntityId entityId), EditorUtility.InstanceIDToObject(int instanceID), UnityEngine.Object.GetInstanceID(), UnityEngine.Object.GetEntityId(), AssetDatabase.Contains(instanceID/entityId), or AssetDatabase.GetAssetPath(instanceID/entityId); some of Odin's functionality will be broken in this version of Unity.");
			}
		}

		public UnityEngine.Object ToObject()
		{
			if (Backend == OdinEntityIdBackend.Invalid)
			{
				return null;
			}
			return EntityIDToObjectFunc(this);
		}

		public static OdinEntityId FromObject(UnityEngine.Object obj)
		{
			if (Backend == OdinEntityIdBackend.Invalid || (object)obj == null)
			{
				return None;
			}
			return Object_GetEntityId_Func(obj);
		}

		public bool IsInAssetDatabase()
		{
			if (Backend == OdinEntityIdBackend.Invalid)
			{
				return false;
			}
			return AssetDatabase_Contains_Func(this);
		}

		public string GetAssetPath()
		{
			if (Backend == OdinEntityIdBackend.Invalid)
			{
				return null;
			}
			return AssetDatabase_GetAssetPath_Func(this);
		}

		public static OdinEntityId GetSessionStateId(string key, OdinEntityId initialId)
		{
			if (Backend == OdinEntityIdBackend.Invalid)
			{
				return None;
			}
			return GetSessionStateId_Func(key, initialId);
		}

		public static void SetSessionStateId(string key, OdinEntityId id)
		{
			if (Backend != OdinEntityIdBackend.Invalid)
			{
				SetSessionStateId_Func(key, id);
			}
		}

		public bool Equals(OdinEntityId other)
		{
			return UlongData == other.UlongData;
		}

		public override bool Equals(object obj)
		{
			if (obj is OdinEntityId other)
			{
				return Equals(other);
			}
			return false;
		}

		public int CompareTo(OdinEntityId other)
		{
			if (EntityIdSize == 4)
			{
				return Int32Data.CompareTo(other.Int32Data);
			}
			return UlongData.CompareTo(other.UlongData);
		}

		public static bool operator ==(OdinEntityId lhs, OdinEntityId rhs)
		{
			return lhs.UlongData == rhs.UlongData;
		}

		public static bool operator !=(OdinEntityId lhs, OdinEntityId rhs)
		{
			return lhs.UlongData != rhs.UlongData;
		}

		public static bool operator <(OdinEntityId lhs, OdinEntityId rhs)
		{
			if (EntityIdSize == 4)
			{
				return lhs.Int32Data < rhs.Int32Data;
			}
			return lhs.UlongData < rhs.UlongData;
		}

		public static bool operator >(OdinEntityId lhs, OdinEntityId rhs)
		{
			if (EntityIdSize == 4)
			{
				return lhs.Int32Data > rhs.Int32Data;
			}
			return lhs.UlongData > rhs.UlongData;
		}

		public static bool operator <=(OdinEntityId lhs, OdinEntityId rhs)
		{
			if (EntityIdSize == 4)
			{
				return lhs.Int32Data <= rhs.Int32Data;
			}
			return lhs.UlongData <= rhs.UlongData;
		}

		public static bool operator >=(OdinEntityId lhs, OdinEntityId rhs)
		{
			if (EntityIdSize == 4)
			{
				return lhs.Int32Data >= rhs.Int32Data;
			}
			return lhs.UlongData >= rhs.UlongData;
		}

		public override int GetHashCode()
		{
			return UlongData.GetHashCode();
		}

		public override string ToString()
		{
			if (EntityIdToStringFunc != null)
			{
				return EntityIdToStringFunc(this);
			}
			if (EntityIdSize == 4)
			{
				return Int32Data.ToString();
			}
			return UlongData.ToString();
		}

		internal static void EmitBitCastToEvalStack(ILGenerator il, Type from, Type to, int fromSize, int toSize, SourceKind sourceKind, Action emitSource)
		{
			if (fromSize >= toSize)
			{
				if (sourceKind == SourceKind.Value)
				{
					LocalBuilder source = il.DeclareLocal(from);
					emitSource();
					il.Emit(OpCodes.Stloc, source);
					il.Emit(OpCodes.Ldloca_S, source);
				}
				else
				{
					emitSource();
				}
				il.Emit(OpCodes.Ldobj, to);
				return;
			}
			LocalBuilder result = il.DeclareLocal(to);
			il.Emit(OpCodes.Ldloca_S, result);
			il.Emit(OpCodes.Initobj, to);
			il.Emit(OpCodes.Ldloca_S, result);
			if (sourceKind == SourceKind.Address)
			{
				emitSource();
				il.Emit(OpCodes.Ldobj, from);
			}
			else
			{
				emitSource();
			}
			il.Emit(OpCodes.Stobj, from);
			il.Emit(OpCodes.Ldloc, result);
		}
	}
}
