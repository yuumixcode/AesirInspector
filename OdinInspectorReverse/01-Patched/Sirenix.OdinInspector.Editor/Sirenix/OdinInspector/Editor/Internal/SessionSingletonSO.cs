using Sirenix.Reflection.Editor;
using Sirenix.Utilities;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Internal
{
	public abstract class SessionSingletonSO<T> : ScriptableObject where T : SessionSingletonSO<T>
	{
		public static string Key = "session_so_" + typeof(T).GetNiceName();

		private static T instance;

		public static T Instance
		{
			get
			{
				if (instance == null)
				{
					OdinEntityId entityId = OdinEntityId.GetSessionStateId(Key, OdinEntityId.None);
					if (!entityId.IsValid)
					{
						instance = ScriptableObject.CreateInstance<T>();
						instance.hideFlags = HideFlags.HideAndDontSave;
						OdinEntityId.SetSessionStateId(Key, OdinEntityId.FromObject(instance));
					}
					else
					{
						instance = entityId.ToObject() as T;
						if (instance == null)
						{
							T[] instances = Resources.FindObjectsOfTypeAll<T>();
							if (instances != null && instances.Length != 0)
							{
								instance = instances[0];
							}
						}
						if (instance == null)
						{
							instance = ScriptableObject.CreateInstance<T>();
							instance.hideFlags = HideFlags.HideAndDontSave;
							OdinEntityId.SetSessionStateId(Key, OdinEntityId.FromObject(instance));
						}
					}
				}
				return instance;
			}
		}
	}
}
