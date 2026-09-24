using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Sirenix.OdinInspector.Editor.Internal.IntermediateData;
using Sirenix.Serialization;
using Sirenix.Utilities;

namespace Sirenix.OdinInspector.Editor.Internal
{
	internal ref struct DesignerReader
	{
		private static readonly List<GroupPatch> GroupBuffer = new List<GroupPatch>(16);

		private static readonly List<PropertyPatch> PropertyBuffer = new List<PropertyPatch>(32);

		private static readonly RefList<AttributePatch> PatchBuffer = new RefList<AttributePatch>(16);

		public readonly CharView View;

		public int Offset;

		public bool AtEnd => Offset >= Length - 1;

		public int Length => View.Length;

		public char Current => View[Offset];

		public DesignerReader(CharView view)
		{
			View = view;
			Offset = 0;
			GroupBuffer.Clear();
			PropertyBuffer.Clear();
			PatchBuffer.Clear();
		}

		public DesignerVersion ReadHeader()
		{
			DesignerLineParser line = ReadAndParseLine();
			DesignerVersion result = DesignerVersion.Invalid;
			if (!line.TryConsume("OVDF"))
			{
				return result;
			}
			line.ConsumeWhitespace();
			if (!line.TryConsume('v'))
			{
				return result;
			}
			int major = line.ReadInt32();
			line.Consume('.');
			int minor = line.ReadInt32();
			result.Major = major;
			result.Minor = minor;
			return result;
		}

		public Type ReadSystemType(out string binding)
		{
			SkipWhitespace();
			CharSlice line = ReadUntilEOL();
			line = line.Trim();
			binding = line.ToString();
			return TwoWaySerializationBinder.Default.BindToType(binding);
		}

		public string ReadMetaGuid()
		{
			SkipWhitespace();
			DesignerLineParser parser = ReadAndParseLine();
			if (!parser.TryConsume("MetaGuid:"))
			{
				return null;
			}
			return parser.ReadWord();
		}

		public bool IsAtBlockHeader()
		{
			if (AtEnd)
			{
				return false;
			}
			SkipWhitespace();
			return Current == '#';
		}

		public void ReadBlockHeader(out DesignerBlockHeader header)
		{
			header = DesignerBlockHeader.Invalid;
			DesignerLineParser line = ReadAndParseLine();
			if (line.TryConsume('#'))
			{
				line.Consume(' ');
				header.IsGroupFromCode = line.TryConsume('"');
				header.IsReference = line.TryConsume('$');
				if (header.IsGroupFromCode)
				{
					header.Id = line.ReadUntilLast('"');
				}
				else
				{
					header.Id = line.ReadRemainder();
				}
				header.DesiredIndex = -1;
			}
		}

		public DesignerSpecialVariables GetSpecialVariableForLine()
		{
			if (AtEnd)
			{
				return DesignerSpecialVariables.None;
			}
			SkipWhitespace();
			if (IsAt("Position:"))
			{
				return DesignerSpecialVariables.Position;
			}
			if (IsAt("Visibility:"))
			{
				return DesignerSpecialVariables.Visibility;
			}
			return DesignerSpecialVariables.None;
		}

		public void ReadSpecialVariable(ref DesignerBlockHeader header, DesignerSpecialVariables varType)
		{
			DesignerLineParser line = ReadAndParseLine();
			switch (varType)
			{
			case DesignerSpecialVariables.Position:
			{
				if (!line.TryConsume("Position:"))
				{
					break;
				}
				line.ConsumeWhitespace();
				bool isRef = line.TryConsume('$');
				string parentId = (line.TryConsume('"') ? line.ReadUntilLast('"') : line.ReadWordUntil(':'));
				if (isRef)
				{
					if (parentId == "root")
					{
						header.ParentId = string.Empty;
					}
					else
					{
						header.ParentId = parentId;
					}
				}
				else
				{
					header.ParentId = parentId;
					line.Consume('"');
				}
				if (line.TryConsume(':'))
				{
					header.DesiredIndex = line.ReadInt32();
				}
				break;
			}
			case DesignerSpecialVariables.Visibility:
			{
				if (!line.TryConsume("Visibility:"))
				{
					break;
				}
				line.ConsumeWhitespace();
				string value = line.ReadWord();
				if (!(value == "Shown"))
				{
					if (value == "Hidden")
					{
						header.Visibility = PropertyVisibilityState.Hidden;
					}
				}
				else
				{
					header.Visibility = PropertyVisibilityState.Shown;
				}
				break;
			}
			}
		}

		public bool IsAtAttributePatchHeader()
		{
			if (AtEnd)
			{
				return false;
			}
			SkipWhitespace();
			switch (Current)
			{
			case '*':
			case '+':
			case '-':
				return true;
			default:
				return false;
			}
		}

		public void ReaderAttributePatchHeader(ref AttributePatch patch)
		{
			DesignerLineParser line = ReadAndParseLine();
			if (line.TryConsume('+'))
			{
				patch.PatchType = AttributePatchType.Add;
			}
			else if (line.TryConsume('-'))
			{
				patch.PatchType = AttributePatchType.Remove;
			}
			else
			{
				if (!line.TryConsume('*'))
				{
					return;
				}
				patch.PatchType = AttributePatchType.Modify;
			}
			line.ConsumeWhitespace();
			if (line.TryConsume('['))
			{
				string binding = line.ReadWordUntil(']');
				Type type = TwoWaySerializationBinder.Default.BindToType(binding);
				if (type == null)
				{
					patch.AttributeType = null;
					return;
				}
				patch.AttributeType = type;
				patch.MemberDeltas = RefList<MemberDelta>.Empty;
			}
		}

		public bool IsAtMemberDelta()
		{
			if (AtEnd)
			{
				return false;
			}
			SkipWhitespace();
			if (!char.IsLetter(Current))
			{
				return Current == '_';
			}
			return true;
		}

		public void ReadMemberDelta(ref AttributePatch patch)
		{
			DesignerLineParser line = ReadAndParseLine();
			if (!(patch.AttributeType == null))
			{
				if (patch.MemberDeltas.Capacity == 0)
				{
					patch.MemberDeltas = new RefList<MemberDelta>();
				}
				MemberDelta delta = default(MemberDelta);
				line.ConsumeWhitespace();
				string name = line.ReadWordUntil('=');
				delta.Member = patch.AttributeType.GetMember(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).FirstOrDefault();
				if (!(delta.Member == null))
				{
					line.ConsumeWhitespace();
					line.Consume('=');
					line.ConsumeWhitespace();
					delta.Value = (line.TryConsume("null") ? null : line.ReadValue(delta.Member.GetReturnType()));
					patch.MemberDeltas.Add(ref delta);
				}
			}
		}

		public bool IsAt(string str)
		{
			if (Length - Offset < str.Length)
			{
				return false;
			}
			for (int i = 0; i < str.Length; i++)
			{
				if (View[Offset + i] != str[i])
				{
					return false;
				}
			}
			return true;
		}

		public void SkipWhitespace()
		{
			while (!AtEnd && char.IsWhiteSpace(Current))
			{
				Offset++;
			}
		}

		public CharSlice ReadUntilEOL()
		{
			int start = Offset;
			while (Offset < Length)
			{
				char current = Current;
				if (current == '\n' || current == '\r')
				{
					break;
				}
				Offset++;
			}
			CharSlice result = View.Slice(start, Offset);
			SkipWhitespace();
			return result;
		}

		public DesignerLineParser ReadAndParseLine()
		{
			SkipWhitespace();
			DesignerLineParser result = new DesignerLineParser(ReadUntilEOL());
			SkipWhitespace();
			return result;
		}
	}
}
