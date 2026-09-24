using System;

namespace Sirenix.Serialization
{
	/// <summary>
	/// Contains information about a node during deserialization and serialization.
	/// </summary>
	public struct NodeInfo
	{
		/// <summary>
		/// An empty node.
		/// </summary>
		public static readonly NodeInfo Empty = new NodeInfo(parameter: true);

		/// <summary>
		/// The name of the node.
		/// </summary>
		public readonly string Name;

		/// <summary>
		/// The id of the node, or -1 if the node has no id.
		/// </summary>
		public readonly int Id;

		/// <summary>
		/// The type of the node, or null if the node has no type metadata.
		/// </summary>
		public readonly Type Type;

		/// <summary>
		/// Whether the node is an array or not.
		/// </summary>
		public readonly bool IsArray;

		/// <summary>
		/// Whether the node is an empty node.
		/// </summary>
		public readonly bool IsEmpty;

		/// <summary>
		/// Initializes a new instance of the <see cref="T:Sirenix.Serialization.NodeInfo" /> struct.
		/// </summary>
		/// <param name="name">The name of the node.</param>
		/// <param name="id">The id of the node.</param>
		/// <param name="type">The type of the node.</param>
		/// <param name="isArray">If set to <c>true</c> the node is an array node.</param>
		public NodeInfo(string name, int id, Type type, bool isArray)
		{
			Name = name;
			Id = id;
			Type = type;
			IsArray = isArray;
			IsEmpty = false;
		}

		private NodeInfo(bool parameter)
		{
			Name = null;
			Id = -1;
			Type = null;
			IsArray = false;
			IsEmpty = true;
		}

		/// <summary>
		/// Implements the operator == between <see cref="T:Sirenix.Serialization.NodeInfo" /> and <see cref="T:Sirenix.Serialization.NodeInfo" />.
		/// </summary>
		/// <param name="a">The first <see cref="T:Sirenix.Serialization.NodeInfo" />.</param>
		/// <param name="b">The second <see cref="T:Sirenix.Serialization.NodeInfo" />.</param>
		/// <returns>
		/// <c>true</c> if the nodes were equal; otherwise, <c>false</c>.
		/// </returns>
		public static bool operator ==(NodeInfo a, NodeInfo b)
		{
			if (a.Name == b.Name && a.Id == b.Id && a.Type == b.Type && a.IsArray == b.IsArray)
			{
				return a.IsEmpty == b.IsEmpty;
			}
			return false;
		}

		/// <summary>
		/// Implements the operator != between <see cref="T:Sirenix.Serialization.NodeInfo" /> and <see cref="T:Sirenix.Serialization.NodeInfo" />.
		/// </summary>
		/// <param name="a">The first <see cref="T:Sirenix.Serialization.NodeInfo" />.</param>
		/// <param name="b">The second <see cref="T:Sirenix.Serialization.NodeInfo" />.</param>
		/// <returns>
		/// <c>true</c> if the nodes were not equal; otherwise, <c>false</c>.
		/// </returns>
		public static bool operator !=(NodeInfo a, NodeInfo b)
		{
			return !(a == b);
		}

		/// <summary>
		/// Determines whether the specified <see cref="T:System.Object" />, is equal to this instance.
		/// </summary>
		/// <param name="obj">The <see cref="T:System.Object" /> to compare with this instance.</param>
		/// <returns>
		///   <c>true</c> if the specified <see cref="T:System.Object" /> is equal to this instance; otherwise, <c>false</c>.
		/// </returns>
		public override bool Equals(object obj)
		{
			if (obj == null)
			{
				return false;
			}
			if (obj is NodeInfo)
			{
				return (NodeInfo)obj == this;
			}
			return false;
		}

		/// <summary>
		/// Returns a hash code for this instance.
		/// </summary>
		/// <returns>
		/// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.
		/// </returns>
		public override int GetHashCode()
		{
			if (IsEmpty)
			{
				return 0;
			}
			return -2128831035 ^ (((Name == null) ? 12321 : Name.GetHashCode()) * 16777619) ^ (Id * 16777619) ^ (((Type == null) ? 1423 : Type.GetHashCode()) * 16777619) ^ ((IsArray ? 124124 : 43234) * 16777619) ^ ((IsEmpty ? 872934 : 27323) * 16777619);
		}
	}
}
