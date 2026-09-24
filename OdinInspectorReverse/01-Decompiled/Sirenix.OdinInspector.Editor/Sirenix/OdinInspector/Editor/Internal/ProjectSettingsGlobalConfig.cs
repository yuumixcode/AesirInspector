using System;
using Sirenix.Utilities;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Internal
{
	public abstract class ProjectSettingsGlobalConfig<T> : GlobalConfig<T>, ISerializationCallbackReceiver where T : ProjectSettingsGlobalConfig<T>, new()
	{
		[NonSerialized]
		private bool initialized;

		[PropertyOrder(float.MinValue)]
		[OnInspectorInit]
		private void EnsureInitialized()
		{
			if (!initialized)
			{
				initialized = true;
				ProjectSettingsUtility.InitAllProjectSettingFieldsFromAttributes(this);
			}
		}

		protected virtual void Awake()
		{
			EnsureInitialized();
		}

		protected virtual void OnEnable()
		{
			EnsureInitialized();
		}

		public virtual void OnAfterDeserialize()
		{
			EnsureInitialized();
		}

		public virtual void OnBeforeSerialize()
		{
		}

		protected override void OnConfigInstanceFirstAccessed()
		{
			EnsureInitialized();
		}
	}
}
