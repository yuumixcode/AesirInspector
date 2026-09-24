namespace Sirenix.OdinInspector.Editor.ValueResolvers
{
	public delegate TResult ValueResolverFunc<TResult>(ref ValueResolverContext context, int selectionIndex);
}
