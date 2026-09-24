namespace Sirenix.Utilities.Editor
{
	/// <summary>
	/// This class is due to undergo refactoring.
	/// </summary>
	public class GUIContext<T> : IControlContext
	{
		internal bool HasValue;

		/// <summary>
		/// The value.
		/// </summary>
		public T Value;

		int IControlContext.LastRenderedFrameId { get; set; }

		/// <summary>
		/// Performs an implicit conversion from <see cref="T:Sirenix.Utilities.Editor.GUIContext`1" /> to <see cref="!:T" />.
		/// </summary>
		public static implicit operator T(GUIContext<T> context)
		{
			if (context == null)
			{
				return default(T);
			}
			return context.Value;
		}
	}
}
