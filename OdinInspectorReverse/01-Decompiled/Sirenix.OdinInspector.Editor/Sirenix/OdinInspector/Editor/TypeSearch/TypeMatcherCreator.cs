namespace Sirenix.OdinInspector.Editor.TypeSearch
{
	public abstract class TypeMatcherCreator
	{
		public abstract bool TryCreateMatcher(TypeSearchInfo info, out TypeMatcher matcher);
	}
}
