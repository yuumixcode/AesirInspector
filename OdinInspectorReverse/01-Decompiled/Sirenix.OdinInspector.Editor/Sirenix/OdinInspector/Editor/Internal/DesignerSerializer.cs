using System;
using System.Collections.Generic;
using System.IO;
using Sirenix.OdinInspector.Editor.Internal.IntermediateData;
using UnityEditor;

namespace Sirenix.OdinInspector.Editor.Internal
{
	internal static class DesignerSerializer
	{
		private static RefList<AttributePatch> attributePatches = new RefList<AttributePatch>();

		public static void Serialize(DesignerFile file, TypePatch typePatch)
		{
			string path = file.Path;
			string dir = Path.GetDirectoryName(path);
			if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
			{
				Directory.CreateDirectory(dir);
			}
			if (!typePatch.HasChanges())
			{
				if (File.Exists(path))
				{
					string guid = AssetDatabase.AssetPathToGUID(path);
					if (!string.IsNullOrEmpty(guid))
					{
						AssetDatabase.DeleteAsset(path);
					}
					else
					{
						File.Delete(path);
					}
				}
				else
				{
					OVDFFileWatcher.Remove(file);
				}
				return;
			}
			using StreamWriter streamWriter = new StreamWriter(path, append: false);
			DesignerWriter writer = new DesignerWriter(streamWriter);
			writer.WriteHeader(typePatch.TargetType);
			if (typePatch.SelfPatches.Length > 0)
			{
				DesignerBlockHeader selfHeader = DesignerBlockHeader.Self;
				writer.WriteBlockHeader(ref selfHeader);
				writer.WritePatches(typePatch.SelfPatches);
			}
			for (int i = 0; i < typePatch.GroupPatches.Count; i++)
			{
				GroupPatch patch = typePatch.GroupPatches[i];
				DesignerBlockHeader header = new DesignerBlockHeader(ref patch);
				writer.WriteBlockHeader(ref header);
				writer.WritePatch(ref patch.GroupAttributePatch);
			}
			for (int j = 0; j < typePatch.PropertyPatches.Count; j++)
			{
				PropertyPatch patch2 = typePatch.PropertyPatches[j];
				DesignerBlockHeader header2 = new DesignerBlockHeader(ref patch2);
				writer.WriteBlockHeader(ref header2);
				writer.WritePatches(patch2.AttributePatches);
			}
			streamWriter.Flush();
		}

		public static void Deserialize(string path, TypePatch typePatch)
		{
			if (!File.Exists(path))
			{
				return;
			}
			DesignerTempFileBuffer fileBuffer = DesignerIOUtils.ReadFile(path);
			DesignerReader reader = new DesignerReader(new CharView(fileBuffer.Buffer, fileBuffer.Length));
			if (!reader.ReadHeader().IsValid)
			{
				return;
			}
			string binding;
			Type type = reader.ReadSystemType(out binding);
			if (reader.IsAt("MetaGuid:"))
			{
				if (type == null)
				{
					string guid = reader.ReadMetaGuid();
					Type assetType = DesignerUtils.GetTypeFromScriptGuid(guid);
					if (assetType != null)
					{
						type = assetType;
					}
				}
				else
				{
					reader.ReadUntilEOL();
				}
			}
			if (typePatch.TargetType != null && typePatch.TargetType != type)
			{
				return;
			}
			typePatch.TargetType = type;
			typePatch.SelfPatches = RefList<AttributePatch>.Empty;
			typePatch.GroupPatches = null;
			typePatch.PropertyPatches = null;
			while (!reader.AtEnd)
			{
				if (reader.IsAtBlockHeader())
				{
					attributePatches.Clear();
					reader.ReadBlockHeader(out var header);
					while (true)
					{
						DesignerSpecialVariables lineVarType = reader.GetSpecialVariableForLine();
						if (lineVarType == DesignerSpecialVariables.None)
						{
							break;
						}
						reader.ReadSpecialVariable(ref header, lineVarType);
					}
					while (reader.IsAtAttributePatchHeader())
					{
						AttributePatch attributePatch = default(AttributePatch);
						reader.ReaderAttributePatchHeader(ref attributePatch);
						while (reader.IsAtMemberDelta())
						{
							reader.ReadMemberDelta(ref attributePatch);
						}
						if (attributePatch.AttributeType != null)
						{
							attributePatches.Add(ref attributePatch);
						}
					}
					if (header.IsReference)
					{
						if (header.Id == "self")
						{
							typePatch.SelfPatches = attributePatches.CopyTrim();
							continue;
						}
						GroupPatch group = new GroupPatch
						{
							ParentId = header.ParentId,
							Id = header.Id,
							DesiredIndex = header.DesiredIndex,
							IsAddedByDesigner = true
						};
						if (attributePatches.Length > 0)
						{
							group.GroupAttributePatch = attributePatches[0];
						}
						else
						{
							group.GroupAttributePatch.MemberDeltas = RefList<MemberDelta>.Empty;
						}
						if (typePatch.GroupPatches == null)
						{
							typePatch.GroupPatches = new List<GroupPatch>();
						}
						typePatch.GroupPatches.Add(group);
					}
					else if (header.IsGroupFromCode)
					{
						GroupPatch group2 = new GroupPatch
						{
							ParentId = header.ParentId,
							Id = header.Id,
							DesiredIndex = header.DesiredIndex,
							IsAddedByDesigner = false
						};
						if (attributePatches.Length > 0)
						{
							group2.GroupAttributePatch = attributePatches[0];
						}
						else
						{
							group2.GroupAttributePatch.MemberDeltas = RefList<MemberDelta>.Empty;
						}
						if (typePatch.GroupPatches == null)
						{
							typePatch.GroupPatches = new List<GroupPatch>();
						}
						typePatch.GroupPatches.Add(group2);
					}
					else
					{
						PropertyPatch patch = new PropertyPatch
						{
							ParentId = header.ParentId,
							DesiredIndex = header.DesiredIndex,
							Name = header.Id,
							AttributePatches = attributePatches.CopyTrim(),
							Visibility = header.Visibility
						};
						if (typePatch.PropertyPatches == null)
						{
							typePatch.PropertyPatches = new List<PropertyPatch>();
						}
						typePatch.PropertyPatches.Add(patch);
					}
				}
				else
				{
					reader.ReadUntilEOL();
				}
			}
		}
	}
}
