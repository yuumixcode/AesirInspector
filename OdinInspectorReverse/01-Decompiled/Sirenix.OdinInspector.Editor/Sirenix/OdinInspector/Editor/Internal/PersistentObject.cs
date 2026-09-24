using Sirenix.Reflection.Editor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Internal
{
	internal static class PersistentObject
	{
		public static TScriptableObject GetOrCreate<TScriptableObject>(string key) where TScriptableObject : ScriptableObject
		{
			OdinEntityId id = OdinEntityId.GetSessionStateId(key, OdinEntityId.None);
			TScriptableObject result = null;
			if (id.IsValid)
			{
				result = id.ToObject() as TScriptableObject;
			}
			if (result == null)
			{
				result = ScriptableObject.CreateInstance<TScriptableObject>();
				result.hideFlags = HideFlags.HideAndDontSave;
				Object.DontDestroyOnLoad(result);
				OdinEntityId.SetSessionStateId(key, OdinEntityId.FromObject(result));
			}
			return result;
		}
	}
}
