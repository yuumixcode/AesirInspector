using System;

namespace Sirenix.OdinInspector.Editor.TypeSearch
{
	public abstract class TypeMatcher
	{
		public abstract string Name { get; }

		public abstract Type Match(Type[] targets, ref bool stopMatching);
	}
}
