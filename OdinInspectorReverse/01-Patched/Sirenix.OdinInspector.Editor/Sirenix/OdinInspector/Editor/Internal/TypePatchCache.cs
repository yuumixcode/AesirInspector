using System;
using System.Collections.Generic;
using System.IO;

namespace Sirenix.OdinInspector.Editor.Internal
{
	internal static class TypePatchCache
	{
		private static readonly Dictionary<Type, TypePatch> Patches = new Dictionary<Type, TypePatch>(64);

		public static TypePatch Get(Type type)
		{
			if (!DesignerUtils.CanTypeBeDesigned(type))
			{
				return null;
			}
			Type baseType = DesignerUtils.GetBaseType(type);
			if (!Patches.TryGetValue(type, out var result) || result.TargetType == null)
			{
				if (OVDFFileWatcher.DesignerFiles.TryGetValue(type, out var file) && File.Exists(file.Path))
				{
					result = new TypePatch();
					DesignerSerializer.Deserialize(file.Path, result);
					result.BasePatch = Get(baseType);
				}
				else
				{
					result = new TypePatch
					{
						BasePatch = Get(baseType),
						TargetType = type,
						EditorVariant = null,
						SelfPatches = RefList<AttributePatch>.Empty,
						GroupPatches = null,
						PropertyPatches = null
					};
				}
				Patches[type] = result;
			}
			if (type.IsGenericType)
			{
				DesignerRegistry.RegisterGenericVariant(type, validateTypeArgs: true);
			}
			return result;
		}

		public static TypePatch Get(InspectorProperty property)
		{
			Type type = ((property.ValueEntry != null) ? property.ValueEntry.TypeOfValue : property.Info.TypeOfValue);
			if (!(type == null))
			{
				return Get(type);
			}
			return null;
		}

		public static bool TryGet(Type type, out TypePatch patch)
		{
			return Patches.TryGetValue(type, out patch);
		}

		public static bool Has(Type type)
		{
			return Patches.ContainsKey(type);
		}

		public static void Remove(Type type)
		{
			Patches.Remove(type);
		}

		public static void TryClear(Type type)
		{
			if (TryGet(type, out var patch))
			{
				patch.SelfPatches = RefList<AttributePatch>.Empty;
				patch.GroupPatches = null;
				patch.PropertyPatches = null;
			}
		}
	}
}
