using System;
using Sirenix.OdinInspector.Editor.Drawers;
using Sirenix.Serialization;

namespace Sirenix.OdinInspector.Editor
{
	/// <summary>
	/// OdinDrawer extensions.
	/// </summary>
	public static class OdinDrawerExtensions
	{
		[Serializable]
		private struct DrawerStateSignature : IEquatable<DrawerStateSignature>
		{
			public int RecursiveDrawDepth;

			public int CurrentInlineEditorDrawDepth;

			public int DrawerChainIndex;

			public DrawerStateSignature(int recursiveDrawDepth, int currentInlineEditorDrawDepth, int drawerChainIndex)
			{
				RecursiveDrawDepth = recursiveDrawDepth;
				CurrentInlineEditorDrawDepth = currentInlineEditorDrawDepth;
				DrawerChainIndex = drawerChainIndex;
			}

			public override int GetHashCode()
			{
				int hash = 17;
				hash = hash * 31 + RecursiveDrawDepth;
				hash = hash * 31 + CurrentInlineEditorDrawDepth;
				return hash * 31 + DrawerChainIndex;
			}

			public override bool Equals(object obj)
			{
				if (obj is DrawerStateSignature)
				{
					return Equals((DrawerStateSignature)obj);
				}
				return false;
			}

			public bool Equals(DrawerStateSignature other)
			{
				if (RecursiveDrawDepth == other.RecursiveDrawDepth && CurrentInlineEditorDrawDepth == other.CurrentInlineEditorDrawDepth)
				{
					return DrawerChainIndex == other.DrawerChainIndex;
				}
				return false;
			}
		}

		/// <summary>
		/// Gets a persistent value that will survive past multiple Unity Editor Application sessions. 
		/// The value is stored in the PersistentContextCache, which has a customizable max cache size.
		/// </summary>
		public static LocalPersistentContext<T> GetPersistentValue<T>(this OdinDrawer drawer, string key, T defaultValue = default(T))
		{
			int a = TwoWaySerializationBinder.Default.BindToName(drawer.GetType()).GetHashCode();
			int b = TwoWaySerializationBinder.Default.BindToName(drawer.Property.Tree.TargetType).GetHashCode();
			int c = drawer.Property.Path.GetHashCode();
			int d = new DrawerStateSignature(drawer.Property.RecursiveDrawDepth, InlineEditorAttributeDrawer.CurrentInlineEditorDrawDepth, drawer.Property.DrawerChainIndex).GetHashCode();
			if (PersistentContext.Get(a, b, c, d, key, out GlobalPersistentContext<T> global))
			{
				global.Value = defaultValue;
			}
			return LocalPersistentContext<T>.Create(global);
		}
	}
}
