using System.Text;
using Sirenix.Serialization.Utilities;

namespace Sirenix.OdinInspector.Editor
{
	/// <summary>
	/// Contains information about a change that is going to occur/has occurred to a collection.
	/// </summary>
	/// <seealso cref="T:Sirenix.OdinInspector.Editor.CollectionChangeType" />
	public struct CollectionChangeInfo
	{
		public CollectionChangeType ChangeType;

		public object Key;

		public object Value;

		public int Index;

		public int SelectionIndex;

		public override string ToString()
		{
			using Cache<StringBuilder> sbCache = Cache<StringBuilder>.Claim();
			StringBuilder sb = sbCache.Value;
			sb.Length = 0;
			sb.Append("CollectionChangeInfo { ");
			AppendValue(sb, "ChangeType", ChangeType, prependComma: false);
			switch (ChangeType)
			{
			case CollectionChangeType.Unspecified:
				AppendValue(sb, "Key", Key);
				AppendValue(sb, "Value", Value);
				AppendValue(sb, "Index", Index);
				break;
			case CollectionChangeType.Add:
				AppendValue(sb, "Value", Value);
				break;
			case CollectionChangeType.Insert:
				AppendValue(sb, "Value", Value);
				AppendValue(sb, "Index", Index);
				break;
			case CollectionChangeType.RemoveValue:
				AppendValue(sb, "Value", Value);
				break;
			case CollectionChangeType.RemoveIndex:
				AppendValue(sb, "Index", Index);
				break;
			case CollectionChangeType.RemoveKey:
				AppendValue(sb, "Key", Key);
				break;
			case CollectionChangeType.SetKey:
				AppendValue(sb, "Key", Key);
				AppendValue(sb, "Value", Value);
				break;
			}
			AppendValue(sb, "SelectionIndex", SelectionIndex);
			sb.Append(" }");
			return sb.ToString();
		}

		private static void AppendValue(StringBuilder sb, string name, object value, bool prependComma = true)
		{
			if (prependComma)
			{
				sb.Append(", ");
			}
			sb.Append(name);
			sb.Append(" = ");
			sb.Append(value ?? "null");
		}
	}
}
