using System;
using System.ComponentModel;

namespace Sirenix.Serialization
{
	/// <summary>
	/// Implements functionality that is shared by both data readers and data writers.
	/// </summary>
	public abstract class BaseDataReaderWriter
	{
		private NodeInfo[] nodes = new NodeInfo[32];

		private int nodesLength;

		/// <summary>
		/// Gets or sets the context's or writer's serialization binder.
		/// </summary>
		/// <value>
		/// The reader's or writer's serialization binder.
		/// </value>
		[Obsolete("Use the Binder member on the writer's SerializationContext/DeserializationContext instead.", false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public TwoWaySerializationBinder Binder
		{
			get
			{
				if (this is IDataWriter)
				{
					return (this as IDataWriter).Context.Binder;
				}
				if (this is IDataReader)
				{
					return (this as IDataReader).Context.Binder;
				}
				return TwoWaySerializationBinder.Default;
			}
			set
			{
				if (this is IDataWriter)
				{
					(this as IDataWriter).Context.Binder = value;
				}
				else if (this is IDataReader)
				{
					(this as IDataReader).Context.Binder = value;
				}
			}
		}

		/// <summary>
		/// Gets a value indicating whether the reader or writer is in an array node.
		/// </summary>
		/// <value>
		/// <c>true</c> if the reader or writer is in an array node; otherwise, <c>false</c>.
		/// </value>
		public bool IsInArrayNode
		{
			get
			{
				if (nodesLength != 0)
				{
					return nodes[nodesLength - 1].IsArray;
				}
				return false;
			}
		}

		/// <summary>
		/// Gets the current node depth. In other words, the current count of the node stack.
		/// </summary>
		/// <value>
		/// The current node depth.
		/// </value>
		protected int NodeDepth => nodesLength;

		/// <summary>
		/// Gets the current nodes array. The amount of nodes contained in it is stored in the <see cref="P:Sirenix.Serialization.BaseDataReaderWriter.NodeDepth" /> property. The remainder of the array's length is buffer space.
		/// </summary>
		/// <value>
		/// The current node array.
		/// </value>
		protected NodeInfo[] NodesArray => nodes;

		/// <summary>
		/// Gets the current node, or <see cref="F:Sirenix.Serialization.NodeInfo.Empty" /> if there is no current node.
		/// </summary>
		/// <value>
		/// The current node.
		/// </value>
		protected NodeInfo CurrentNode
		{
			get
			{
				if (nodesLength != 0)
				{
					return nodes[nodesLength - 1];
				}
				return NodeInfo.Empty;
			}
		}

		/// <summary>
		/// Pushes a node onto the node stack.
		/// </summary>
		/// <param name="node">The node to push.</param>
		protected void PushNode(NodeInfo node)
		{
			if (nodesLength == nodes.Length)
			{
				ExpandNodes();
			}
			nodes[nodesLength] = node;
			nodesLength++;
		}

		/// <summary>
		/// Pushes a node with the given name, id and type onto the node stack.
		/// </summary>
		/// <param name="name">The name of the node.</param>
		/// <param name="id">The id of the node.</param>
		/// <param name="type">The type of the node.</param>
		protected void PushNode(string name, int id, Type type)
		{
			if (nodesLength == nodes.Length)
			{
				ExpandNodes();
			}
			nodes[nodesLength] = new NodeInfo(name, id, type, isArray: false);
			nodesLength++;
		}

		/// <summary>
		/// Pushes an array node onto the node stack. This uses values from the current node to provide extra info about the array node.
		/// </summary>
		protected void PushArray()
		{
			if (nodesLength == nodes.Length)
			{
				ExpandNodes();
			}
			if (nodesLength == 0 || nodes[nodesLength - 1].IsArray)
			{
				nodes[nodesLength] = new NodeInfo(null, -1, null, isArray: true);
			}
			else
			{
				NodeInfo current = nodes[nodesLength - 1];
				nodes[nodesLength] = new NodeInfo(current.Name, current.Id, current.Type, isArray: true);
			}
			nodesLength++;
		}

		private void ExpandNodes()
		{
			NodeInfo[] newArr = new NodeInfo[nodes.Length * 2];
			NodeInfo[] oldNodes = nodes;
			for (int i = 0; i < oldNodes.Length; i++)
			{
				newArr[i] = oldNodes[i];
			}
			nodes = newArr;
		}

		/// <summary>
		/// Pops the current node off of the node stack.
		/// </summary>
		/// <param name="name">The name of the node to pop.</param>
		/// <exception cref="T:System.InvalidOperationException">
		/// There are no nodes to pop.
		/// or
		/// Tried to pop node with given name, but the current node's name was different.
		/// </exception>
		protected void PopNode(string name)
		{
			if (nodesLength == 0)
			{
				throw new InvalidOperationException("There are no nodes to pop.");
			}
			nodesLength--;
		}

		/// <summary>
		/// Pops the current node if the current node is an array node.
		/// </summary>
		protected void PopArray()
		{
			if (nodesLength == 0)
			{
				throw new InvalidOperationException("There are no nodes to pop.");
			}
			if (!nodes[nodesLength - 1].IsArray)
			{
				throw new InvalidOperationException("Was not in array when exiting array.");
			}
			nodesLength--;
		}

		protected void ClearNodes()
		{
			nodesLength = 0;
		}
	}
}
