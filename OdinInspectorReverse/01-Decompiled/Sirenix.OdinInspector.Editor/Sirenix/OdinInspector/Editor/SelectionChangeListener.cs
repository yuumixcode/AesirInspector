using System;
using Sirenix.OdinInspector.Editor.Internal;
using UnityEditor;

namespace Sirenix.OdinInspector.Editor
{
	internal static class SelectionChangeListener
	{
		public static readonly WeakReferenceEventListener<ISelectionChangeListener> Listeners;

		static SelectionChangeListener()
		{
			Listeners = new WeakReferenceEventListener<ISelectionChangeListener>(delegate(ISelectionChangeListener listener, object[] args)
			{
				listener.OnSelectionChanged();
			});
			Selection.selectionChanged = (Action)Delegate.Combine(Selection.selectionChanged, new Action(OnSelectionChanged));
		}

		private static void OnSelectionChanged()
		{
			Listeners.InvokeEvent(null);
		}
	}
}
