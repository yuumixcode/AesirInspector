using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Internal
{
	public interface IProjectSetting
	{
		void Reset();

		void SetInitData(string key, object defaultValue, Object serializedContainer);
	}
}
