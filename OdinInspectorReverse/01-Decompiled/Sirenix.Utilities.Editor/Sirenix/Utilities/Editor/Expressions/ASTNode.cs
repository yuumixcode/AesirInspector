using System;
using System.Collections.Generic;
using System.Text;

namespace Sirenix.Utilities.Editor.Expressions
{
	internal class ASTNode
	{
		public class ASTNodeChildren
		{
			private ASTNode child0;

			private ASTNode child1;

			private ASTNode child2;

			private ASTNode child3;

			private List<ASTNode> children;

			private ASTNode owner;

			public int Count
			{
				get
				{
					if (children != null)
					{
						return children.Count;
					}
					if (child0 == null)
					{
						return 0;
					}
					if (child1 == null)
					{
						return 1;
					}
					if (child2 == null)
					{
						return 2;
					}
					if (child3 == null)
					{
						return 3;
					}
					return 4;
				}
			}

			public ASTNode this[int index]
			{
				get
				{
					if (children != null)
					{
						return children[index];
					}
					return index switch
					{
						0 => child0, 
						1 => child1, 
						2 => child2, 
						3 => child3, 
						_ => throw new IndexOutOfRangeException(), 
					};
				}
				set
				{
					if (index < 0)
					{
						throw new IndexOutOfRangeException();
					}
					if (children != null)
					{
						while (children.Count <= index)
						{
							children.Add(null);
						}
						children[index] = value;
					}
					else
					{
						switch (index)
						{
						case 0:
							child0 = value;
							break;
						case 1:
							child1 = value;
							break;
						case 2:
							child2 = value;
							break;
						case 3:
							child3 = value;
							break;
						default:
							children = new List<ASTNode> { child0, child1, child2, child3 };
							this[index] = value;
							break;
						}
					}
					value.Parent = owner;
				}
			}

			public ASTNodeChildren(ASTNode owner)
			{
				this.owner = owner;
			}

			public void Clear()
			{
				child0 = null;
				child1 = null;
				child2 = null;
				child3 = null;
				if (children != null)
				{
					children.Clear();
				}
			}
		}

		public NodeType NodeType;

		public object NodeValue;

		public int NodeStartIndex;

		public int NodeEndIndex;

		public Type TypeOfValue;

		public readonly ASTNodeChildren Children;

		public ASTNode Parent;

		public ASTNode()
		{
			Children = new ASTNodeChildren(this);
		}

		public override string ToString()
		{
			return ToPrettyPrint();
		}

		public ASTNode DeepCopy()
		{
			ASTNode copy = new ASTNode();
			copy.NodeType = NodeType;
			copy.NodeValue = NodeValue;
			copy.NodeStartIndex = NodeStartIndex;
			copy.NodeEndIndex = NodeEndIndex;
			copy.TypeOfValue = TypeOfValue;
			copy.Parent = Parent;
			for (int i = 0; i < Children.Count; i++)
			{
				copy.Children[i] = Children[i].DeepCopy();
			}
			return copy;
		}

		public string ToPrettyPrint()
		{
			StringBuilder sb = new StringBuilder();
			PrettyPrint(sb, 0);
			return sb.ToString();
		}

		private void PrettyPrint(StringBuilder sb, int depth)
		{
			if (sb.Length > 0)
			{
				sb.AppendLine();
			}
			for (int i = 0; i < depth * 4; i++)
			{
				sb.Append(' ');
			}
			sb.Append(NodeType.ToString());
			if (NodeType == NodeType.CONSTANT_NULL || NodeValue != null)
			{
				sb.Append(": ");
				sb.Append((NodeValue == null) ? "null" : NodeValue.ToString());
			}
			for (int j = 0; j < Children.Count; j++)
			{
				Children[j].PrettyPrint(sb, depth + 1);
			}
		}

		public Type GetHighestPushedStackType()
		{
			if (TypeOfValue != null)
			{
				return TypeOfValue;
			}
			for (int i = Children.Count - 1; i >= 0; i--)
			{
				Type result = Children[i].GetHighestPushedStackType();
				if (result != null)
				{
					return result;
				}
			}
			return null;
		}
	}
}
