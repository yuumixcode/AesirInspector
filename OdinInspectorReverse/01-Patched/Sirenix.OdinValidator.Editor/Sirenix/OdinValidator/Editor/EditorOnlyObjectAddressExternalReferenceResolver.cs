using Sirenix.Serialization;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinValidator.Editor
{
	public class EditorOnlyObjectAddressExternalReferenceResolver : IExternalStringReferenceResolver
	{
		public static readonly EditorOnlyObjectAddressExternalReferenceResolver Instance = new EditorOnlyObjectAddressExternalReferenceResolver();

		public IExternalStringReferenceResolver NextResolver
		{
			get
			{
				return null;
			}
			set
			{
			}
		}

		public bool CanReference(object value, out string id)
		{
			if (!(value is Object unityObj))
			{
				id = null;
				return false;
			}
			if (unityObj == null)
			{
				id = "NULL";
				return true;
			}
			if (!AssetDatabase.Contains(unityObj))
			{
				id = "NON-ASSET";
				return true;
			}
			if (!ObjectAddress.TryCreateObjectAddress(unityObj, out var address, out var error))
			{
				Debug.LogWarning("Could not create object address for AssetDatabase object, error was: " + error);
				id = "INVALID";
				return true;
			}
			id = address.ToString();
			return true;
		}

		public bool TryResolveReference(string id, out object value)
		{
			ObjectAddress address = ObjectAddress.Parse(id);
			if (address.IsBroken || address.Type == ObjectAddress.AddressType.Unknown)
			{
				value = null;
				return true;
			}
			if (!address.TryGetObjectReference(openSceneIfNeeded: false, autoSaveIfOpenScene: false, out var result, out var error))
			{
				Debug.LogWarning("Failed to resolve editor-time object address '" + id + "' during deserialization with error: " + error);
				value = null;
				return true;
			}
			value = result;
			return true;
		}
	}
}
