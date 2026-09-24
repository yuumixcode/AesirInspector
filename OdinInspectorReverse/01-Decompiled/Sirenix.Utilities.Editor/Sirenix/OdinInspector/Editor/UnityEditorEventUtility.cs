using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;

namespace Sirenix.OdinInspector.Editor
{
	/// <summary>
	/// This class contains utility methods for subscribing to various UnityEditor events reliably and safely across all Odin-supported versions of Unity.
	/// </summary>
	[InitializeOnLoad]
	public static class UnityEditorEventUtility
	{
		private static readonly MemberInfo EditorApplication_delayCall_Member;

		private static readonly EventInfo SceneView_duringSceneGui_Event;

		private static readonly FieldInfo SceneView_onSceneGUIDelegate_Field;

		private static readonly Type SceneView_OnSceneFunc_Type;

		private static readonly object actionsToSubscribe_LOCK;

		private static readonly List<Action> actionsToSubscribe;

		private static readonly EventInfo onProjectChangedEvent;

		public static readonly bool HasOnProjectChanged;

		public static event Action OnProjectChanged
		{
			add
			{
				if (onProjectChangedEvent != null)
				{
					onProjectChangedEvent.AddEventHandler(null, value);
					return;
				}
				throw new NotImplementedException("EditorApplication.projectChanged is not implemented in this version of Unity.");
			}
			remove
			{
				if (onProjectChangedEvent != null)
				{
					onProjectChangedEvent.RemoveEventHandler(null, value);
					return;
				}
				throw new NotImplementedException("EditorApplication.projectChanged is not implemented in this version of Unity.");
			}
		}

		/// <summary>
		/// In 2019.1+, this event subscribes to SceneView.duringSceneGui. In 2018.4 and lower, it subscribes to SceneView.onSceneGUIDelegate.
		/// </summary>
		public static event Action<SceneView> DuringSceneGUI
		{
			add
			{
				if (SceneView_duringSceneGui_Event == null && (SceneView_onSceneGUIDelegate_Field == null || SceneView_OnSceneFunc_Type == null))
				{
					throw new InvalidOperationException("SceneView.duringSceneGui event and SceneView.onSceneGUIDelegate field could both not be found in the SceneView class, and parts of Odin will be broken. Please report this issue to the issue tracker.");
				}
				if (SceneView_duringSceneGui_Event != null)
				{
					SceneView_duringSceneGui_Event.AddMethod.Invoke(null, new object[1] { value });
				}
				else
				{
					Delegate del = SceneView_onSceneGUIDelegate_Field.GetValue(null) as Delegate;
					del = Delegate.Combine(del, value.ConvertDelegate(SceneView_OnSceneFunc_Type));
					SceneView_onSceneGUIDelegate_Field.SetValue(null, del);
				}
			}
			remove
			{
				if (SceneView_duringSceneGui_Event == null && (SceneView_onSceneGUIDelegate_Field == null || SceneView_OnSceneFunc_Type == null))
				{
					throw new InvalidOperationException("SceneView.duringSceneGui event and SceneView.onSceneGUIDelegate field could both not be found in the SceneView class, and parts of Odin will be broken. Please report this issue to the issue tracker.");
				}
				if (SceneView_duringSceneGui_Event != null)
				{
					SceneView_duringSceneGui_Event.RemoveMethod.Invoke(null, new object[1] { value });
				}
				else
				{
					Delegate del = SceneView_onSceneGUIDelegate_Field.GetValue(null) as Delegate;
					del = Delegate.Remove(del, value.ConvertDelegate(SceneView_OnSceneFunc_Type));
					SceneView_onSceneGUIDelegate_Field.SetValue(null, del);
				}
			}
		}

		/// <summary>
		/// In 2020.1, Unity changed EditorApplication.delayCall from a field to an event, meaning 
		/// we now have to use reflection to access it consistently across all versions of Unity.
		/// </summary>
		public static event Action EditorApplication_delayCall
		{
			add
			{
				if (EditorApplication_delayCall_Member == null)
				{
					throw new InvalidOperationException("EditorApplication.delayCall field or event could not be found, and parts of Odin will be broken. Please report this issue to the issue tracker.");
				}
				if (EditorApplication_delayCall_Member is FieldInfo)
				{
					EditorApplication.CallbackFunction val = (EditorApplication.CallbackFunction)(EditorApplication_delayCall_Member as FieldInfo).GetValue(null);
					val = (EditorApplication.CallbackFunction)Delegate.Combine(val, value.ConvertDelegate<EditorApplication.CallbackFunction>());
					(EditorApplication_delayCall_Member as FieldInfo).SetValue(null, val);
				}
				else if (EditorApplication_delayCall_Member is EventInfo)
				{
					(EditorApplication_delayCall_Member as EventInfo).AddEventHandler(null, value);
				}
				else if (EditorApplication_delayCall_Member == null)
				{
					throw new InvalidOperationException("EditorApplication.delayCall was not a field or an event. Odin will be broken.");
				}
			}
			remove
			{
				if (EditorApplication_delayCall_Member == null)
				{
					throw new InvalidOperationException("EditorApplication.delayCall field or event could not be found. Odin will be broken.");
				}
				if (EditorApplication_delayCall_Member is FieldInfo)
				{
					EditorApplication.CallbackFunction val = (EditorApplication.CallbackFunction)(EditorApplication_delayCall_Member as FieldInfo).GetValue(null);
					val = (EditorApplication.CallbackFunction)Delegate.Remove(val, value.ConvertDelegate<EditorApplication.CallbackFunction>());
					(EditorApplication_delayCall_Member as FieldInfo).SetValue(null, val);
				}
				else if (EditorApplication_delayCall_Member is EventInfo)
				{
					(EditorApplication_delayCall_Member as EventInfo).RemoveEventHandler(null, value);
				}
				else if (EditorApplication_delayCall_Member == null)
				{
					throw new InvalidOperationException("EditorApplication.delayCall was not a field or an event. Odin will be broken.");
				}
			}
		}

		static UnityEditorEventUtility()
		{
			EditorApplication_delayCall_Member = typeof(EditorApplication).GetMember("delayCall", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic).FirstOrDefault();
			SceneView_duringSceneGui_Event = typeof(SceneView).GetEvent("duringSceneGui", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
			SceneView_onSceneGUIDelegate_Field = typeof(SceneView).GetField("onSceneGUIDelegate", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
			SceneView_OnSceneFunc_Type = typeof(SceneView).GetNestedType("OnSceneFunc");
			actionsToSubscribe_LOCK = new object();
			actionsToSubscribe = new List<Action>();
			onProjectChangedEvent = typeof(EditorApplication).GetEvent("projectChanged");
			HasOnProjectChanged = onProjectChangedEvent != null;
			EditorApplication.update = (EditorApplication.CallbackFunction)Delegate.Combine(EditorApplication.update, new EditorApplication.CallbackFunction(OnUpdate));
		}

		/// <summary>
		/// Sometimes, someone accidentally overrides a delay action subscription to <see cref="F:UnityEditor.EditorApplication.delayCall" /> 
		/// by setting the value instead of using the += operator as should be done,
		/// which can be done because in many versions of Unity it is a field, and not an event. 
		/// (In some versions of Unity it is an event, though, and in this case, this method acts as a wrapper 
		/// to subscribe reliably, no matter the nature of the backing event.)
		/// This method subscribes to a lot of different callbacks, in the hopes of catching at least one.
		/// <para />
		/// As opposed to <see cref="M:Sirenix.OdinInspector.Editor.UnityEditorEventUtility.DelayAction(System.Action)" />, this method is safe to call from any thread, and will
		/// delay the actual subscription to a safe time.
		/// </summary>
		public static void DelayActionThreadSafe(Action action)
		{
			lock (actionsToSubscribe_LOCK)
			{
				actionsToSubscribe.Add(action);
			}
		}

		/// <summary>
		/// Sometimes, an idiot overrides a delay action subscription to <see cref="F:UnityEditor.EditorApplication.delayCall" />,
		/// which can be done because the people at Unity didn't know what events were once upon a time.
		/// This method subscribes to a lot of different callbacks, in the hopes of catching at least one.
		/// </summary>
		public static void DelayAction(Action action)
		{
			DelayAction(action, excludeGuiEventHooks: false);
		}

		/// <summary>
		/// Sometimes, an idiot overrides a delay action subscription to <see cref="F:UnityEditor.EditorApplication.delayCall" />,
		/// which can be done because the people at Unity didn't know what events were once upon a time.
		/// This method subscribes to a lot of different callbacks, in the hopes of catching at least one.
		/// </summary>
		public static void DelayAction(Action action, bool excludeGuiEventHooks)
		{
			bool executed = false;
			Action execute = null;
			EditorApplication.ProjectWindowItemCallback projectWindowItemOnGUI = null;
			if (!excludeGuiEventHooks)
			{
				projectWindowItemOnGUI = delegate
				{
					execute();
				};
			}
			EditorApplication.CallbackFunction update = delegate
			{
				execute();
			};
			Action delayCall = delegate
			{
				execute();
			};
			execute = delegate
			{
				if (!executed)
				{
					executed = true;
					try
					{
						action();
					}
					finally
					{
						if (!excludeGuiEventHooks)
						{
							EditorApplication.projectWindowItemOnGUI = (EditorApplication.ProjectWindowItemCallback)Delegate.Remove(EditorApplication.projectWindowItemOnGUI, projectWindowItemOnGUI);
						}
						EditorApplication.update = (EditorApplication.CallbackFunction)Delegate.Remove(EditorApplication.update, update);
						EditorApplication_delayCall -= delayCall;
					}
				}
			};
			if (!excludeGuiEventHooks)
			{
				EditorApplication.projectWindowItemOnGUI = (EditorApplication.ProjectWindowItemCallback)Delegate.Combine(EditorApplication.projectWindowItemOnGUI, projectWindowItemOnGUI);
			}
			EditorApplication.update = (EditorApplication.CallbackFunction)Delegate.Combine(EditorApplication.update, update);
			EditorApplication_delayCall += delayCall;
		}

		private static void OnUpdate()
		{
			lock (actionsToSubscribe_LOCK)
			{
				if (actionsToSubscribe.Count > 0)
				{
					for (int i = 0; i < actionsToSubscribe.Count; i++)
					{
						DelayAction(actionsToSubscribe[i]);
					}
					actionsToSubscribe.Clear();
				}
			}
		}

		private static Delegate ConvertDelegate(this Delegate src, Type delegateType)
		{
			if ((object)src == null || src.GetType() == delegateType)
			{
				return src;
			}
			Delegate[] list = src.GetInvocationList();
			if (list.Length == 1)
			{
				return Delegate.CreateDelegate(delegateType, src.Target, src.Method);
			}
			for (int i = 0; i < list.Length; i++)
			{
				list[i] = list[i].ConvertDelegate(delegateType);
			}
			return Delegate.Combine(list);
		}

		private static T ConvertDelegate<T>(this Delegate src)
		{
			Type delegateType = typeof(T);
			if ((object)src == null || src.GetType() == delegateType)
			{
				return (T)(object)src;
			}
			Delegate[] list = src.GetInvocationList();
			if (list.Length == 1)
			{
				return (T)(object)Delegate.CreateDelegate(delegateType, src.Target, src.Method);
			}
			for (int i = 0; i < list.Length; i++)
			{
				list[i] = list[i].ConvertDelegate(delegateType);
			}
			return (T)(object)Delegate.Combine(list);
		}
	}
}
