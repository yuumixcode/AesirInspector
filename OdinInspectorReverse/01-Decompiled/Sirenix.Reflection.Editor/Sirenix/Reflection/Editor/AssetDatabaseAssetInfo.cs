using System;
using System.Reflection;
using System.Reflection.Emit;
using UnityEditor;
using UnityEngine;

namespace Sirenix.Reflection.Editor
{
	public struct AssetDatabaseAssetInfo
	{
		internal static bool HasInitializedAccessors;

		internal static Func<object, OdinEntityId> GetId;

		internal static Func<object, string> GetGuid;

		internal static Func<object, string> GetName;

		internal static Func<object, Texture> GetIcon;

		internal static Func<object, bool> GetIsFolder;

		internal static Func<object, bool> GetIsMainRepresentation;

		internal object UnityHierarchyReference;

		public bool IsValid => UnityHierarchyReference != null;

		public OdinEntityId Id => GetId(UnityHierarchyReference);

		public string Guid => GetGuid(UnityHierarchyReference);

		public string Name => GetName(UnityHierarchyReference);

		public Texture Icon => GetIcon(UnityHierarchyReference);

		public bool IsFolder => GetIsFolder(UnityHierarchyReference);

		public bool IsMainRepresentation => GetIsMainRepresentation(UnityHierarchyReference);

		public string AssetPath => AssetDatabase.GUIDToAssetPath(Guid);

		public UnityEngine.Object UnityObject => Id.ToObject();

		internal static void EnsureInitialized()
		{
			if (HasInitializedAccessors)
			{
				return;
			}
			Assembly asm = typeof(AssetDatabase).Assembly;
			bool isHierarchyProperty = false;
			Type hierarchyType = asm.GetType("UnityEditor.HierarchyIterator");
			if (hierarchyType == null)
			{
				isHierarchyProperty = true;
				hierarchyType = asm.GetType("UnityEditor.HierarchyProperty");
			}
			if (hierarchyType == null)
			{
				Debug.LogError("[Odin] Could not find 'UnityEditor.HierarchyIterator' nor 'UnityEditor.HierarchyProperty'.");
				HasInitializedAccessors = true;
				return;
			}
			PropertyInfo idProperty;
			if (isHierarchyProperty)
			{
				idProperty = hierarchyType.GetProperty("instanceID", BindingFlags.Instance | BindingFlags.Public);
			}
			else
			{
				idProperty = hierarchyType.GetProperty("entityId", BindingFlags.Instance | BindingFlags.Public);
			}
			DynamicMethod idMethodGenrator = new DynamicMethod("Sirenix.Reflection.Editor.AssetDatabaseAssetInfo.id_getter", typeof(OdinEntityId), new Type[1] { typeof(object) }, restrictedSkipVisibility: true);
			ILGenerator il = idMethodGenrator.GetILGenerator();
			if (isHierarchyProperty)
			{
				OdinEntityId.EmitBitCastToEvalStack(il, typeof(int), typeof(OdinEntityId), 4, 8, OdinEntityId.SourceKind.Value, delegate
				{
					il.Emit(OpCodes.Ldarg_0);
					il.Emit(OpCodes.Castclass, hierarchyType);
					il.Emit(OpCodes.Callvirt, idProperty.GetGetMethod());
				});
				il.Emit(OpCodes.Ret);
			}
			else
			{
				Type entityIdType = idProperty.PropertyType;
				OdinEntityId.EmitBitCastToEvalStack(il, entityIdType, typeof(OdinEntityId), OdinEntityId.EntityIdSize, 8, OdinEntityId.SourceKind.Value, delegate
				{
					il.Emit(OpCodes.Ldarg_0);
					il.Emit(OpCodes.Castclass, hierarchyType);
					il.Emit(OpCodes.Callvirt, idProperty.GetGetMethod());
				});
				il.Emit(OpCodes.Ret);
			}
			GetId = (Func<object, OdinEntityId>)idMethodGenrator.CreateDelegate(typeof(Func<object, OdinEntityId>), null);
			GetGuid = GeneratePropertyAccessor<string>(hierarchyType, "guid");
			GetName = GeneratePropertyAccessor<string>(hierarchyType, "name");
			GetIcon = GeneratePropertyAccessor<Texture>(hierarchyType, "icon");
			GetIsFolder = GeneratePropertyAccessor<bool>(hierarchyType, "isFolder");
			GetIsMainRepresentation = GeneratePropertyAccessor<bool>(hierarchyType, "isMainRepresentation");
			HasInitializedAccessors = true;
		}

		private static Func<object, TReturnType> GeneratePropertyAccessor<TReturnType>(Type type, string propertyName)
		{
			PropertyInfo property = type.GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public);
			DynamicMethod dynamicMethod = new DynamicMethod("Sirenix.Reflection.Editor.AssetDatabaseAssetInfo." + propertyName + "_getter", typeof(TReturnType), new Type[1] { typeof(object) }, restrictedSkipVisibility: true);
			ILGenerator il = dynamicMethod.GetILGenerator();
			il.Emit(OpCodes.Ldarg_0);
			il.Emit(OpCodes.Castclass, type);
			il.Emit(OpCodes.Callvirt, property.GetGetMethod());
			il.Emit(OpCodes.Ret);
			return (Func<object, TReturnType>)dynamicMethod.CreateDelegate(typeof(Func<object, TReturnType>), null);
		}
	}
}
