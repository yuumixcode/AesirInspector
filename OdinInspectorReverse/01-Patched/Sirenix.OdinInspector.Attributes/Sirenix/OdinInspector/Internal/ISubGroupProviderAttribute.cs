using System.Collections.Generic;

namespace Sirenix.OdinInspector.Internal
{
	/// <summary>
	/// Not yet documented.
	/// </summary>
	public interface ISubGroupProviderAttribute
	{
		/// <summary>
		/// Not yet documented.
		/// </summary>
		/// <returns>Not yet documented.</returns>
		IList<PropertyGroupAttribute> GetSubGroupAttributes();

		/// <summary>
		/// Not yet documented.
		/// </summary>
		/// <param name="attr">Not yet documented.</param>
		/// <returns>Not yet documented.</returns>
		string RepathMemberAttribute(PropertyGroupAttribute attr);
	}
}
