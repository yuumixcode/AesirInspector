using System;
using System.Collections.Generic;
using Sirenix.Serialization;
using Sirenix.Utilities;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor
{
	/// <summary>
	/// <para>This is a class for creating, getting and modifying a property's various states. An instance of this class always comes attached to an InspectorProperty.</para>
	/// <para>See Odin's tutorials for more information about usage of the state system.</para>
	/// </summary>
	public sealed class PropertyState
	{
		private class CustomState
		{
			public object Value;

			public object ValueLastLayout;

			public object DefaultValue;

			public Type Type;

			public ILocalPersistentContext PersistentValue;
		}

		private bool visible;

		private bool visibleLastLayout;

		private bool enabled;

		private bool enabledLastLayout;

		private LocalPersistentContext<bool> expanded;

		private bool expandedLastLayout;

		private InspectorProperty property;

		private int index;

		private Dictionary<string, CustomState> customStates;

		/// <summary>
		/// If set to true, all state changes for this property will be logged to the console.
		/// </summary>
		public bool LogChanges;

		/// <summary>
		/// Whether the property is visible in the inspector.
		/// </summary>
		public bool Visible
		{
			get
			{
				return visible;
			}
			set
			{
				if (visible != value)
				{
					if (LogChanges)
					{
						LogChange("Visible", visible, value);
					}
					visible = value;
					SendStateChangedNotifications("Visible");
				}
			}
		}

		/// <summary>
		/// Whether the Visible state was true or not during the last layout event.
		/// </summary>
		public bool VisibleLastLayout => visibleLastLayout;

		/// <summary>
		/// Whether the property is enabled in the inspector.
		/// </summary>
		public bool Enabled
		{
			get
			{
				return enabled;
			}
			set
			{
				if (enabled != value)
				{
					if (LogChanges)
					{
						LogChange("Enabled", enabled, value);
					}
					enabled = value;
					SendStateChangedNotifications("Enabled");
				}
			}
		}

		/// <summary>
		/// Whether the Enabled state was true or not during the last layout event.
		/// </summary>
		public bool EnabledLastLayout => enabledLastLayout;

		/// <summary>
		/// Whether the property is expanded in the inspector.
		/// </summary>
		public bool Expanded
		{
			get
			{
				if (expanded == null)
				{
					expanded = GetPersistentContext("expanded", expandedLastLayout);
				}
				return expanded.Value;
			}
			set
			{
				if (expanded == null)
				{
					expanded = GetPersistentContext("expanded", expandedLastLayout);
				}
				if (expanded.Value != value)
				{
					if (LogChanges)
					{
						LogChange("Expanded", expanded.Value, value);
					}
					expanded.Value = value;
					SendStateChangedNotifications("Expanded");
				}
			}
		}

		/// <summary>
		/// Whether the Expanded state was true or not during the last layout event.
		/// </summary>
		public bool ExpandedLastLayout => expandedLastLayout;

		public PropertyState(InspectorProperty property, int index)
		{
			this.property = property;
			this.index = index;
			if (this.property.ChildResolver is ICollectionResolver)
			{
				expandedLastLayout = GlobalConfig<GeneralDrawerConfig>.Instance.ExpandFoldoutByDefault;
			}
			else
			{
				expandedLastLayout = GlobalConfig<GeneralDrawerConfig>.Instance.OpenListsByDefault;
			}
			Reset();
			visibleLastLayout = true;
			enabledLastLayout = true;
			Update();
		}

		/// <summary>
		/// Creates a custom state with a given name.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="key"></param>
		/// <param name="persistent"></param>
		/// <param name="defaultValue"></param>
		public void Create<T>(string key, bool persistent, T defaultValue)
		{
			if (customStates == null)
			{
				customStates = new Dictionary<string, CustomState>();
			}
			else if (customStates.ContainsKey(key))
			{
				throw new InvalidOperationException("The state '" + key + "' already exists on the property '" + property.Path + "'; can't create a new one with the same key.");
			}
			CustomState state = new CustomState();
			state.Type = typeof(T);
			if (persistent)
			{
				state.PersistentValue = GetPersistentContext(key, defaultValue);
				state.ValueLastLayout = state.PersistentValue.WeakValue;
			}
			else
			{
				state.Value = defaultValue;
				state.ValueLastLayout = state.Value;
				state.DefaultValue = defaultValue;
			}
			customStates.Add(key, state);
			SendStateChangedNotifications(key);
		}

		/// <summary>
		/// Determines whether a state with the given key exists.
		/// </summary>
		/// <param name="key">The key to check.</param>
		/// <returns>True if the state exists, otherwise, false.</returns>
		public bool Exists(string key)
		{
			bool isPersistent;
			Type valueType;
			return Exists(key, out isPersistent, out valueType);
		}

		/// <summary>
		/// Determines whether a state with the given key exists.
		/// </summary>
		/// <param name="key">The key to check.</param>
		/// <param name="isPersistent">If the state exists, this out parameter will be true if the state is persistent.</param>
		/// <returns>True if the state exists, otherwise, false.</returns>
		public bool Exists(string key, out bool isPersistent)
		{
			Type valueType;
			return Exists(key, out isPersistent, out valueType);
		}

		/// <summary>
		/// Determines whether a state with the given key exists.
		/// </summary>
		/// <param name="key">The key to check.</param>
		/// <param name="valueType">If the state exists, this out parameter will contain the type of value that the state contains.</param>
		/// <returns>True if the state exists, otherwise, false.</returns>
		public bool Exists(string key, out Type valueType)
		{
			bool isPersistent;
			return Exists(key, out isPersistent, out valueType);
		}

		/// <summary>
		/// Determines whether a state with the given key exists.
		/// </summary>
		/// <param name="key">The key to check.</param>
		/// <param name="isPersistent">If the state exists, this out parameter will be true if the state is persistent.</param>
		/// <param name="valueType">If the state exists, this out parameter will contain the type of value that the state contains.</param>
		/// <returns>True if the state exists, otherwise, false.</returns>
		public bool Exists(string key, out bool isPersistent, out Type valueType)
		{
			switch (key)
			{
			case "Expanded":
				isPersistent = true;
				valueType = typeof(bool);
				return true;
			case "Visible":
				isPersistent = false;
				valueType = typeof(bool);
				return true;
			case "Enabled":
				isPersistent = false;
				valueType = typeof(bool);
				return true;
			default:
			{
				if (customStates != null && customStates.TryGetValue(key, out var state))
				{
					isPersistent = state.PersistentValue != null;
					valueType = state.Type;
					return true;
				}
				isPersistent = false;
				valueType = null;
				return false;
			}
			}
		}

		/// <summary>
		/// Gets the value of a given state as an instance of type T.
		/// </summary>
		/// <typeparam name="T">The type to get the state value as. An <see cref="T:System.InvalidOperationException" /> will be thrown if the state's value type cannot be assigned to T.</typeparam>
		/// <param name="key">The key of the state to get. An <see cref="T:System.InvalidOperationException" /> will be thrown if a state with the given key does not exist.</param>
		/// <returns>The value of the state.</returns>
		public T Get<T>(string key)
		{
			if (customStates != null && customStates.TryGetValue(key, out var state))
			{
				try
				{
					return (T)((state.PersistentValue != null) ? state.PersistentValue.WeakValue : state.Value);
				}
				catch (InvalidCastException)
				{
					throw new InvalidOperationException("Cannot get property state '" + key + "' as a '" + typeof(T).GetNiceName() + "'; the state is of type '" + state.Type.GetNiceName() + "'.");
				}
			}
			throw new InvalidOperationException("The state '" + key + "' does not exist on the property '" + property.Path + "'.");
		}

		/// <summary>
		/// Gets the value that a given state contained last layout as an instance of type T.
		/// </summary>
		/// <typeparam name="T">The type to get the state value as. An <see cref="T:System.InvalidOperationException" /> will be thrown if the state's value type cannot be assigned to T.</typeparam>
		/// <param name="key">The key of the state to get. An <see cref="T:System.InvalidOperationException" /> will be thrown if a state with the given key does not exist.</param>
		/// <returns>The value of the state during the last layout event.</returns>
		public T GetLastLayout<T>(string key)
		{
			if (customStates != null && customStates.TryGetValue(key, out var state))
			{
				try
				{
					return (T)state.ValueLastLayout;
				}
				catch (InvalidCastException)
				{
					throw new InvalidOperationException("Cannot get property state '" + key + "' as a '" + typeof(T).GetNiceName() + "'; the state is of type '" + state.Type.GetNiceName() + "'.");
				}
			}
			throw new InvalidOperationException("The state '" + key + "' does not exist on the property '" + property.Path + "'.");
		}

		/// <summary>
		/// Sets the value of a given state to a given value.
		/// </summary>
		/// <typeparam name="T">The type to set the state value as. An <see cref="T:System.InvalidOperationException" /> will be thrown if T cannot be assigned to the state's value type.</typeparam>
		/// <param name="key">The key of the state to set the value of. An <see cref="T:System.InvalidOperationException" /> will be thrown if a state with the given key does not exist.</param>
		/// <param name="value">The value to set.</param>
		public void Set<T>(string key, T value)
		{
			if (customStates != null && customStates.TryGetValue(key, out var state))
			{
				if (typeof(T) != state.Type)
				{
					throw new InvalidOperationException("Cannot set property state '" + key + "' as a '" + typeof(T).GetNiceName() + "'; the state is of type '" + state.Type.GetNiceName() + "'.");
				}
				T current = (T)((state.PersistentValue != null) ? state.PersistentValue.WeakValue : state.Value);
				if (!PropertyValueEntry<T>.EqualityComparer(current, value))
				{
					if (LogChanges)
					{
						LogChange(key, current, value);
					}
					if (state.PersistentValue != null)
					{
						state.PersistentValue.WeakValue = value;
					}
					else
					{
						state.Value = value;
					}
					SendStateChangedNotifications(key);
				}
				return;
			}
			throw new InvalidOperationException("The state '" + key + "' does not exist on the property '" + property.Path + "'.");
		}

		private LocalPersistentContext<T> GetPersistentContext<T>(string key, T defaultValue)
		{
			return PersistentContext.GetLocal(TwoWaySerializationBinder.Default.BindToName(property.Tree.TargetType).GetHashCode(), property.Path, index, key, defaultValue);
		}

		internal void Update()
		{
			if (Event.current != null && Event.current.type != EventType.Layout)
			{
				return;
			}
			visibleLastLayout = visible;
			enabledLastLayout = enabled;
			if (expanded != null)
			{
				expandedLastLayout = expanded.Value;
			}
			if (customStates == null)
			{
				return;
			}
			foreach (CustomState state in customStates.GFValueIterator())
			{
				state.ValueLastLayout = ((state.PersistentValue != null) ? state.PersistentValue.WeakValue : state.Value);
			}
		}

		/// <summary>
		/// Cleans the property state and prepares it for cached reuse of its containing PropertyTree. This will also reset the state.
		/// </summary>
		public void CleanForCachedReuse()
		{
			if (customStates != null)
			{
				customStates.Clear();
			}
			Reset();
		}

		/// <summary>
		/// Resets all states to their default values. Persistent states will be updated to their persistent cached value if one exists.
		/// </summary>
		public void Reset()
		{
			enabled = true;
			visible = true;
			if (expanded != null)
			{
				expanded.UpdateLocalValue();
			}
			if (customStates == null)
			{
				return;
			}
			foreach (CustomState state in customStates.GFValueIterator())
			{
				if (state.PersistentValue != null)
				{
					state.PersistentValue.UpdateLocalValue();
				}
				else
				{
					state.Value = state.DefaultValue;
				}
			}
		}

		private void LogChange<T>(string state, T oldValue, T newValue)
		{
			string[] obj = new string[9] { "Property '", property.Path, "'s '", state, "' state changed from '", null, null, null, null };
			T val = oldValue;
			obj[5] = val?.ToString();
			obj[6] = "' to '";
			val = newValue;
			obj[7] = val?.ToString();
			obj[8] = "'";
			string str = string.Concat(obj);
			str = ((Event.current != null) ? (str + " during IMGUI event '" + Event.current.type.ToString() + "'") : (str + " while outside IMGUI context"));
			Debug.Log(str);
		}

		private void SendStateChangedNotifications(string state)
		{
			if (property.Tree.TreeIsSetupForIMGUIDrawing_TEMP_INTERNAL && Event.current != null)
			{
				OdinDrawer[] chain = property.GetActiveDrawerChain().BakedDrawerArray;
				for (int i = 0; i < chain.Length; i++)
				{
					if (chain[i] is IOnSelfStateChangedNotification notification)
					{
						try
						{
							notification.OnSelfStateChanged(state);
						}
						catch (Exception exception)
						{
							Debug.LogException(exception);
						}
					}
				}
			}
			StateUpdater[] stateUpdaters = property.StateUpdaters;
			for (int j = 0; j < stateUpdaters.Length; j++)
			{
				if (stateUpdaters[j] is IOnSelfStateChangedNotification notification2)
				{
					try
					{
						notification2.OnSelfStateChanged(state);
					}
					catch (Exception exception2)
					{
						Debug.LogException(exception2);
					}
				}
			}
			InspectorProperty parent = property.Parent;
			if (parent == null && property != property.Tree.RootProperty)
			{
				parent = property.Tree.RootProperty;
			}
			if (parent == null)
			{
				return;
			}
			int index = property.Index;
			if (property.Tree.TreeIsSetupForIMGUIDrawing_TEMP_INTERNAL && Event.current != null)
			{
				OdinDrawer[] chain2 = parent.GetActiveDrawerChain().BakedDrawerArray;
				for (int k = 0; k < chain2.Length; k++)
				{
					if (chain2[k] is IOnChildStateChangedNotification notification3)
					{
						try
						{
							notification3.OnChildStateChanged(index, state);
						}
						catch (Exception exception3)
						{
							Debug.LogException(exception3);
						}
					}
				}
			}
			StateUpdater[] stateUpdaters2 = parent.StateUpdaters;
			for (int l = 0; l < stateUpdaters2.Length; l++)
			{
				if (stateUpdaters2[l] is IOnChildStateChangedNotification notification4)
				{
					try
					{
						notification4.OnChildStateChanged(index, state);
					}
					catch (Exception exception4)
					{
						Debug.LogException(exception4);
					}
				}
			}
			InspectorProperty current = parent;
			while (current != null)
			{
				if (property.Tree.TreeIsSetupForIMGUIDrawing_TEMP_INTERNAL && Event.current != null)
				{
					OdinDrawer[] chain3 = current.GetActiveDrawerChain().BakedDrawerArray;
					for (int m = 0; m < chain3.Length; m++)
					{
						if (chain3[m] is IRecursiveOnChildStateChangedNotification notification5)
						{
							try
							{
								notification5.OnChildStateChanged(property, state);
							}
							catch (Exception exception5)
							{
								Debug.LogException(exception5);
							}
						}
					}
				}
				stateUpdaters2 = current.StateUpdaters;
				for (int n = 0; n < stateUpdaters2.Length; n++)
				{
					if (stateUpdaters2[n] is IRecursiveOnChildStateChangedNotification notification6)
					{
						try
						{
							notification6.OnChildStateChanged(property, state);
						}
						catch (Exception exception6)
						{
							Debug.LogException(exception6);
						}
					}
				}
				InspectorProperty nextCurrent = current.Parent;
				if (nextCurrent == null && current != current.Tree.RootProperty)
				{
					nextCurrent = current.Tree.RootProperty;
				}
				current = nextCurrent;
			}
		}
	}
}
