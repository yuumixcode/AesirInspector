using System.Collections.Generic;
using System.Reflection;

namespace Sirenix.OdinInspector.Editor
{
	/// <summary>
	/// Base class definition for OdinAttributeProcessorLocator. Responsible for finding and creating <see cref="T:Sirenix.OdinInspector.Editor.OdinAttributeProcessor" /> instances to process attributes for properties.
	/// Default OdinAttributeProcessorLocator have been implemented as <see cref="T:Sirenix.OdinInspector.Editor.DefaultOdinAttributeProcessorLocator" />.
	/// </summary>
	public abstract class OdinAttributeProcessorLocator
	{
		/// <summary>
		/// Gets a list of <see cref="T:Sirenix.OdinInspector.Editor.OdinAttributeProcessor" /> to process attributes for the specified child member of the parent property.
		/// </summary>
		/// <param name="parentProperty">The parent of the member.</param>
		/// <param name="member">Child member of the parent property.</param>
		/// <returns>List of <see cref="T:Sirenix.OdinInspector.Editor.OdinAttributeProcessor" /> to process attributes for the specified member.</returns>
		public abstract List<OdinAttributeProcessor> GetChildProcessors(InspectorProperty parentProperty, MemberInfo member);

		/// <summary>
		/// Gets a list of <see cref="T:Sirenix.OdinInspector.Editor.OdinAttributeProcessor" /> to process attributes for the specified property.
		/// </summary>
		/// <param name="property">The property to find attribute porcessors for.</param>
		/// <returns>List of <see cref="T:Sirenix.OdinInspector.Editor.OdinAttributeProcessor" /> to process attributes for the speicied member.</returns>
		public abstract List<OdinAttributeProcessor> GetSelfProcessors(InspectorProperty property);
	}
}
