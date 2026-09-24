using System;
using System.Collections.Generic;

namespace Sirenix.OdinInspector.Editor.Internal
{
	public sealed class WeakReferenceEventListener<TListener> where TListener : class
	{
		public Action<TListener, object[]> InvokeEventOnListener;

		public List<WeakReference<TListener>> Listeners;

		public WeakReferenceEventListener(Action<TListener, object[]> invokeEventOnListener)
		{
			InvokeEventOnListener = invokeEventOnListener;
			Listeners = new List<WeakReference<TListener>>();
		}

		public void InvokeEvent(object[] args)
		{
			for (int i = 0; i < Listeners.Count; i++)
			{
				WeakReference<TListener> listenerRef = Listeners[i];
				if (!listenerRef.TryGetTarget(out var listener) || listener == null)
				{
					Listeners.RemoveAt(i--);
				}
				else
				{
					InvokeEventOnListener(listener, args);
				}
			}
		}

		public void SubscribeListener(TListener listener)
		{
			Listeners.Add(new WeakReference<TListener>(listener, trackResurrection: false));
		}

		public void DesubscribeListener(TListener listener)
		{
			for (int i = 0; i < Listeners.Count; i++)
			{
				WeakReference<TListener> listenerRef = Listeners[i];
				if (!listenerRef.TryGetTarget(out var l) || l == null)
				{
					Listeners.RemoveAt(i--);
				}
				else if (listener == l)
				{
					Listeners.RemoveAt(i--);
				}
			}
		}
	}
}
