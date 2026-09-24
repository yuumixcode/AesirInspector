using System;

namespace Sirenix.OdinInspector.Editor
{
	public interface ILocalPersistentContext
	{
		Type Type { get; }

		object WeakValue { get; set; }

		void UpdateLocalValue();
	}
}
