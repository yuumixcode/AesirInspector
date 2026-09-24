using System;
using System.Collections.Generic;
using System.Text;
using Sirenix.Serialization;
using Sirenix.Utilities;
using UnityEditor;

namespace Sirenix.OdinInspector.Editor.Internal
{
	internal static class OVDFFriendlyFormatter
	{
		internal static string Format(OVDFParser.OVDFFile file)
		{
			if (file == null)
			{
				return "Invalid OVDF file.";
			}
			StringBuilder sb = new StringBuilder();
			sb.Append("<color=#808080>OVDF ").Append(file.Header.Version).Append(" — ")
				.Append(file.Header.TargetTypeName)
				.AppendLine("</color>");
			sb.AppendLine();
			Dictionary<string, string> nodeNames = new Dictionary<string, string>();
			int counter = 1;
			foreach (OVDFParser.OVDFObject obj in file.Objects)
			{
				if (!string.IsNullOrEmpty(obj.Id) && obj.Id.StartsWith("$"))
				{
					nodeNames[obj.Id] = "Node #" + counter++;
				}
			}
			for (int i = 0; i < file.Objects.Count; i++)
			{
				OVDFParser.OVDFObject obj2 = file.Objects[i];
				string name = obj2.Id;
				name = (string.IsNullOrEmpty(name) ? "(Unnamed)" : ((!name.StartsWith("$")) ? ("<color=#89DCEB>" + name + "</color>") : (nodeNames.TryGetValue(name, out var friendly) ? ("<color=#89DCEB>" + friendly + "</color>") : "<color=#89DCEB>Node ?</color>")));
				sb.AppendLine(name);
				if (obj2.Position != null)
				{
					string parentText = null;
					if (obj2.Position.ParentId == "$root")
					{
						parentText = "<color=#89DCEB>Root</color>";
					}
					else if (!string.IsNullOrEmpty(obj2.Position.ParentId))
					{
						parentText = ((!nodeNames.TryGetValue(obj2.Position.ParentId, out var friendlyParent)) ? ("<color=#89DCEB>" + obj2.Position.ParentId + "</color>") : ("<color=#89DCEB>" + friendlyParent + "</color>"));
					}
					if (!string.IsNullOrEmpty(parentText))
					{
						sb.Append("  • <color=#FFFFFF>Placed under</color> ").Append(parentText).Append(" <color=#FFFFFF>at position</color> ")
							.Append(obj2.Position.Index + 1)
							.AppendLine();
					}
				}
				if (!string.IsNullOrEmpty(obj2.Visibility))
				{
					sb.Append("  • <color=#FFFFFF>Visibility:</color> ").Append("<color=#B4BEFE>").Append(obj2.Visibility)
						.AppendLine("</color>");
				}
				for (int a = 0; a < obj2.Attributes.Count; a++)
				{
					OVDFParser.OVDFAttribute attr = obj2.Attributes[a];
					string action;
					string color;
					switch (attr.Prefix)
					{
					case '+':
						action = "Adds";
						color = "#A6E3A1";
						break;
					case '-':
						action = "Removes";
						color = "#F38BA8";
						break;
					default:
						action = "Modifies";
						color = "#F9E2AF";
						break;
					}
					Type type = TwoWaySerializationBinder.Default.BindToType(attr.TypeName);
					string niceTypeName = ((type == null) ? attr.TypeName : ObjectNames.NicifyVariableName(type.GetNiceName()));
					sb.Append("  • <color=").Append(color).Append(">")
						.Append(action)
						.Append(" ")
						.Append(niceTypeName)
						.AppendLine("</color>");
					for (int p = 0; p < attr.Parameters.Count; p++)
					{
						OVDFParser.OVDFProperty prop = attr.Parameters[p];
						OVDFParser.OVDFValue val = prop.Value;
						string valColor = "#B4BEFE";
						string valText;
						if (val == null || val.Value == null)
						{
							valText = "null";
						}
						else
						{
							switch (val.Kind)
							{
							case OVDFParser.OVDFValueKind.String:
								valText = $"\"{val.Value}\"";
								valColor = "#F9E2AF";
								break;
							case OVDFParser.OVDFValueKind.Bool:
							{
								valText = ((!bool.TryParse(val.Value.ToString(), out var b)) ? val.Value.ToString() : (b ? "Yes" : "No"));
								break;
							}
							case OVDFParser.OVDFValueKind.Number:
								valText = val.Value.ToString();
								valColor = "#F5C2E7";
								break;
							case OVDFParser.OVDFValueKind.Color:
								valText = val.Value.ToString();
								if (valText.StartsWith("#"))
								{
									valColor = valText;
								}
								break;
							case OVDFParser.OVDFValueKind.Null:
								valText = "null";
								break;
							default:
								valText = val.Value.ToString();
								break;
							}
						}
						sb.Append("      • <color=#CCCCCC>").Append(prop.Name).Append("</color> <color=#808080>=</color> ")
							.Append("<color=")
							.Append(valColor)
							.Append(">")
							.Append(valText)
							.AppendLine("</color>");
					}
					if (a < obj2.Attributes.Count - 1)
					{
						sb.AppendLine();
					}
				}
				if (i < file.Objects.Count - 1)
				{
					sb.AppendLine();
				}
			}
			return sb.ToString().TrimEnd(Array.Empty<char>());
		}
	}
}
