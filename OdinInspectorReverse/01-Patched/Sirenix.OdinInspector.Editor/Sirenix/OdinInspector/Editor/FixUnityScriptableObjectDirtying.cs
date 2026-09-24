using System;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor
{
	/// <summary>
	/// This class fixes a bug where Unity's Undo.RecordObject does not mark ScriptableObjects dirty when
	/// a change is recorded for them. It does this by subscribing to the Undo.postprocessModifications
	/// event, and marking all modified ScriptableObjects dirty manually.
	/// </summary>
	[InitializeOnLoad]
	internal static class FixUnityScriptableObjectDirtying
	{
		static FixUnityScriptableObjectDirtying()
		{
			Undo.postprocessModifications = (Undo.PostprocessModifications)Delegate.Combine(Undo.postprocessModifications, (Undo.PostprocessModifications)delegate(UndoPropertyModification[] mods)
			{
				try
				{
					for (int i = 0; i < mods.Length; i++)
					{
						UndoPropertyModification undoPropertyModification = mods[i];
						if (undoPropertyModification.currentValue.target is ScriptableObject)
						{
							EditorUtility.SetDirty(undoPropertyModification.currentValue.target);
						}
					}
				}
				catch
				{
				}
				return mods;
			});
		}
	}
}
