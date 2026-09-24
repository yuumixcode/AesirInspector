using System;
using System.Collections.Generic;
using System.IO;
using Sirenix.OdinInspector.Editor.Internal.IntermediateData;
using Sirenix.Serialization;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Internal
{
	internal readonly ref struct DesignerWriter
	{
		public readonly StreamWriter Writer;

		public static HashSet<char> Base58Chars;

		public DesignerWriter(StreamWriter writer)
		{
			Writer = writer;
		}

		public void WriteHeader(Type targetType)
		{
			Writer.Write("OVDF");
			Writer.Write(" v");
			Writer.Write(DesignerFormat.CurrentVersion.Major);
			Writer.Write('.');
			Writer.Write(DesignerFormat.CurrentVersion.Minor);
			Writer.WriteLine();
			Writer.WriteLine(TwoWaySerializationBinder.Default.BindToName(targetType));
			string guid = DesignerUtils.TryGetScriptGuid(targetType);
			if (guid != null)
			{
				Writer.Write("MetaGuid:");
				Writer.WriteLine(guid);
			}
		}

		public void WriteBlockHeader(ref DesignerBlockHeader header)
		{
			Writer.WriteLine();
			Writer.Write('#');
			Writer.Write(' ');
			if (header.IsGroupFromCode)
			{
				Writer.Write('"');
				Writer.Write(header.Id);
				Writer.WriteLine('"');
			}
			else
			{
				if (header.IsReference)
				{
					Writer.Write('$');
				}
				Writer.WriteLine(header.Id);
			}
			if (header.ParentId != null)
			{
				Writer.Write("Position");
				Writer.Write(':');
				Writer.Write(' ');
				if (DetermineIfReference(header.ParentId))
				{
					Writer.Write('$');
					Writer.Write((header.ParentId == string.Empty) ? "root" : header.ParentId);
				}
				else
				{
					Writer.Write('"');
					Writer.Write(header.ParentId);
					Writer.Write('"');
				}
				if (header.DesiredIndex >= 0)
				{
					Writer.Write(':');
					Writer.WriteLine(header.DesiredIndex);
				}
				else
				{
					Writer.WriteLine();
				}
			}
			if (header.Visibility != PropertyVisibilityState.Default)
			{
				Writer.Write("Visibility");
				Writer.Write(':');
				Writer.Write(' ');
				Writer.WriteLine(header.Visibility);
			}
		}

		public static bool DetermineIfReference(string id)
		{
			if (id == string.Empty)
			{
				return true;
			}
			if (id.Length != 21 && id.Length != 22)
			{
				return false;
			}
			if (Base58Chars == null)
			{
				Base58Chars = new HashSet<char>("123456789ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz");
			}
			for (int i = 0; i < id.Length; i++)
			{
				if (!Base58Chars.Contains(id[i]))
				{
					return false;
				}
			}
			return true;
		}

		public void WritePatches(RefList<AttributePatch> patches)
		{
			for (int i = 0; i < patches.Length; i++)
			{
				WritePatch(ref patches[i]);
			}
		}

		public void WritePatch(ref AttributePatch patch)
		{
			switch (patch.PatchType)
			{
			case AttributePatchType.Add:
				Writer.Write('+');
				break;
			case AttributePatchType.Remove:
				Writer.Write('-');
				break;
			case AttributePatchType.Modify:
				if (patch.MemberDeltas == null || patch.MemberDeltas.Length == 0)
				{
					return;
				}
				Writer.Write('*');
				break;
			case AttributePatchType.None:
			case AttributePatchType.ModifyUnused:
				return;
			default:
				throw new ArgumentOutOfRangeException();
			}
			Writer.Write(' ');
			Writer.Write('[');
			Writer.Write(patch.AttributeType.FullName);
			Writer.WriteLine(']');
			for (int i = 0; i < patch.MemberDeltas.Length; i++)
			{
				ref MemberDelta delta = ref patch.MemberDeltas[i];
				Writer.Write('\t');
				Writer.Write(delta.Member.Name);
				Writer.Write(' ');
				Writer.Write('=');
				Writer.Write(' ');
				if (delta.Value == null)
				{
					Writer.Write("null");
				}
				else if (delta.Value is string str)
				{
					Writer.Write('"');
					if (str.Length > 0)
					{
						Writer.Write(str.Replace("\t", "\\t").Replace("\n", "\\n").Replace("\r", "\\r")
							.Replace("\"", "\\\""));
					}
					Writer.Write('"');
				}
				else if (delta.Value is Type type)
				{
					Writer.Write(TwoWaySerializationBinder.Default.BindToName(type));
				}
				else if (delta.Value is Color color)
				{
					Writer.Write('#');
					Writer.Write(ColorUtility.ToHtmlStringRGBA(color));
				}
				else
				{
					Writer.Write(delta.Value);
				}
				Writer.WriteLine();
			}
		}
	}
}
