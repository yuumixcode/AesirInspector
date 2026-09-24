using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector.Editor.Internal;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor
{
	[InitializeOnLoad]
	public class UndoTracker : SessionSingletonSO<UndoTracker>
	{
		public struct UndoPropertyModificationGroup
		{
			public UnityEngine.Object Target;

			public UndoPropertyModification[] Modifications;
		}

		public int CurrentIndex;

		private static int LastSeenUndoGroup;

		public static event Action<UndoPropertyModificationGroup[]> OnObjectValueModified;

		public static event Action<List<UnityEngine.Object>> OnUndoPerformed;

		public static event Action<List<UnityEngine.Object>> OnRedoPerformed;

		static UndoTracker()
		{
			Undo.postprocessModifications = (Undo.PostprocessModifications)Delegate.Combine(Undo.postprocessModifications, new Undo.PostprocessModifications(PostProcessModifications));
			Undo.undoRedoPerformed = (Undo.UndoRedoCallback)Delegate.Combine(Undo.undoRedoPerformed, new Undo.UndoRedoCallback(UndoRedoPerformed));
		}

		private static List<UnityEngine.Object> GetUndoGroupObjs(int index)
		{
			if (!SessionSingletonSO<UndoTrackerStateContainer>.Instance.UndoGroups.TryGetValue(index, out var objs))
			{
				objs = new List<UnityEngine.Object>();
				SessionSingletonSO<UndoTrackerStateContainer>.Instance.UndoGroups.Add(index, objs);
			}
			return objs;
		}

		private static UndoPropertyModification[] PostProcessModifications(UndoPropertyModification[] modifications)
		{
			for (int i = 0; i < modifications.Length; i++)
			{
				UndoPropertyModification mod = modifications[i];
				PropertyModification value = mod.currentValue ?? mod.previousValue;
				if (value != null && (value.target == SessionSingletonSO<UndoTracker>.Instance || value.target == SessionSingletonSO<UndoTrackerStateContainer>.Instance || (value.target.GetType() == typeof(Transform) && value.propertyPath == "m_RootOrder")))
				{
					return modifications;
				}
			}
			try
			{
				int group = Undo.GetCurrentGroup();
				if (group != LastSeenUndoGroup)
				{
					List<int> toRemove = new List<int>();
					foreach (int key in SessionSingletonSO<UndoTrackerStateContainer>.Instance.UndoGroups.Keys)
					{
						if (key > SessionSingletonSO<UndoTracker>.Instance.CurrentIndex)
						{
							toRemove.Add(key);
						}
					}
					foreach (int key2 in toRemove)
					{
						SessionSingletonSO<UndoTrackerStateContainer>.Instance.UndoGroups.Remove(key2);
					}
					Undo.RecordObject(SessionSingletonSO<UndoTracker>.Instance, Undo.GetCurrentGroupName());
					SessionSingletonSO<UndoTracker>.Instance.CurrentIndex++;
					Undo.FlushUndoRecordObjects();
					LastSeenUndoGroup = group;
				}
				List<UnityEngine.Object> groupMods = GetUndoGroupObjs(SessionSingletonSO<UndoTracker>.Instance.CurrentIndex);
				SessionSingletonSO<UndoTrackerStateContainer>.Instance.LatestIndex = SessionSingletonSO<UndoTracker>.Instance.CurrentIndex;
				for (int j = 0; j < modifications.Length; j++)
				{
					UndoPropertyModification mod2 = modifications[j];
					PropertyModification value2 = mod2.currentValue ?? mod2.previousValue;
					if (value2 != null && !groupMods.Contains(value2.target))
					{
						groupMods.Add(value2.target);
					}
				}
				try
				{
					if (UndoTracker.OnObjectValueModified != null)
					{
						UndoPropertyModificationGroup[] modificationsGrouped = (from x in modifications
							where x.currentValue != null || x.previousValue != null
							group x by (x.currentValue ?? x.previousValue).target into x
							select new UndoPropertyModificationGroup
							{
								Target = x.Key,
								Modifications = x.ToArray()
							}).ToArray();
						UndoTracker.OnObjectValueModified(modificationsGrouped);
					}
				}
				catch (Exception exception)
				{
					Debug.LogException(exception);
				}
			}
			catch (Exception exception2)
			{
				Debug.LogException(exception2);
			}
			return modifications;
		}

		private static void UndoRedoPerformed()
		{
			int latestIndex = SessionSingletonSO<UndoTrackerStateContainer>.Instance.LatestIndex;
			int currentIndex = SessionSingletonSO<UndoTracker>.Instance.CurrentIndex;
			SessionSingletonSO<UndoTrackerStateContainer>.Instance.LatestIndex = currentIndex;
			if (currentIndex < latestIndex)
			{
				if (UndoTracker.OnUndoPerformed == null)
				{
					return;
				}
				{
					foreach (KeyValuePair<int, List<UnityEngine.Object>> group in from n in SessionSingletonSO<UndoTrackerStateContainer>.Instance.UndoGroups
						where n.Key > currentIndex && n.Key <= latestIndex
						orderby n.Key descending
						select n)
					{
						try
						{
							UndoTracker.OnUndoPerformed(group.Value);
						}
						catch (Exception exception)
						{
							Debug.LogException(exception);
						}
					}
					return;
				}
			}
			if (currentIndex <= latestIndex || UndoTracker.OnRedoPerformed == null)
			{
				return;
			}
			foreach (KeyValuePair<int, List<UnityEngine.Object>> group2 in from n in SessionSingletonSO<UndoTrackerStateContainer>.Instance.UndoGroups
				where n.Key <= currentIndex && n.Key > latestIndex
				orderby n.Key
				select n)
			{
				try
				{
					UndoTracker.OnRedoPerformed(group2.Value);
				}
				catch (Exception exception2)
				{
					Debug.LogException(exception2);
				}
			}
		}
	}
}
