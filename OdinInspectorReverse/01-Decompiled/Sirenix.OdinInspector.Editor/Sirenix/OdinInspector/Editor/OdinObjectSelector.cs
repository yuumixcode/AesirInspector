using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Sirenix.OdinInspector.Editor.Internal;
using Sirenix.OdinInspector.Editor.ValueResolvers;
using Sirenix.Reflection.Editor;
using Sirenix.Serialization;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor
{
	public static class OdinObjectSelector
	{
		private struct ReopenInfo
		{
			public Type Type;

			public Type BaseType;

			public bool AllowSceneObjects;

			public static ReopenInfo None => new ReopenInfo(null, null, allowSceneObjects: false);

			public bool IsNone
			{
				get
				{
					if (Type == null)
					{
						return BaseType == null;
					}
					return false;
				}
			}

			public ReopenInfo(Type type, Type baseType, bool allowSceneObjects)
			{
				Type = type;
				BaseType = baseType;
				AllowSceneObjects = allowSceneObjects;
			}
		}

		private static EventType showedInEvent = EventType.Ignore;

		private static bool wasObjectChanged;

		private static ReopenInfo reopenInfo;

		private static bool isUnitySelectorUsingCallbacks;

		/// <summary>
		/// The <see cref="T:Sirenix.OdinInspector.Editor.InspectorProperty" /> that was used in the last 'Show' call.
		/// </summary>
		internal static InspectorProperty SelectorProperty { get; private set; }

		/// <summary>
		/// The key to identify who called the selector.
		/// </summary>
		public static object SelectorKey { get; private set; }

		/// <summary>
		/// The id to identify who called the selector.
		/// </summary>
		public static int SelectorId { get; private set; }

		/// <summary>
		/// The current selected object.
		/// </summary>
		public static object SelectorObject { get; private set; }

		/// <summary>
		/// True if <see cref="T:UnityEditor.ObjectSelector" /> is used; otherwise false.
		/// </summary>
		internal static bool IsUnityObjectSelector { get; private set; }

		public static bool IsOpen { get; private set; }

		internal static bool IsSelectorReadyToClaim { get; private set; }

		/// <summary>
		/// The type of the value used in the last 'Show' call.
		/// </summary>
		internal static Type SelectorValueType { get; private set; }

		/// <summary>
		/// The base type of the value used in the last 'Show' call.
		/// </summary>
		internal static Type SelectorBaseType { get; private set; }

		private static bool IsCurrentEventValid()
		{
			return showedInEvent switch
			{
				EventType.Ignore => false, 
				EventType.Layout => Event.current.type == EventType.Layout, 
				_ => Event.current.type != EventType.Layout, 
			};
		}

		/// <summary>
		/// Shows a selector.
		/// </summary>
		/// <param name="key">The key used to identify who called the selector; this can be null.</param>
		/// <param name="id">The ID used to identify who called the selector; this can be 0.</param>
		/// <param name="value">The current value selected.</param>
		/// <param name="baseType">The base type of the 'value'.</param>
		/// <param name="allowSceneObjects">Determines if scene objects are allowed.</param>
		/// <param name="disallowNullValues">Determines if null values are allowed.</param>
		/// <param name="position">The position where the selector should appear.</param>
		/// <exception cref="T:System.NotImplementedException">Thrown if the selector was opened for a case that it didn't expect.</exception>
		/// <remarks>
		/// <para>Either '<paramref name="key" />' or '<paramref name="id" />' must be set, but both can also be set for more consistent results.</para>
		/// </remarks>
		public static void Show(object key, int id, object value, Type baseType, bool allowSceneObjects = true, bool disallowNullValues = false, Rect position = default(Rect))
		{
			Type valueType = ((!(value is UnityEngine.Object unityObj) || !(unityObj != null)) ? ((value == null) ? baseType : value.GetType()) : value.GetType());
			Show(position, key, id, value, valueType, baseType, allowSceneObjects, disallowNullValues);
		}

		/// <summary>
		/// Shows a selector.
		/// </summary>
		/// <param name="key">The key used to identify who called the selector; this can be null.</param>
		/// <param name="id">The ID used to identify who called the selector; this can be 0.</param>
		/// <param name="property">
		/// Used to provide values for all parameters in the full declaration of this method.
		/// It also allows for customization of various stages of the selector with attributes.
		/// This also sets the <see cref="P:Sirenix.OdinInspector.Editor.OdinObjectSelector.SelectorProperty" />.
		/// </param>
		/// <param name="useUnitySelector">Determines if the default Unity Object Selector should be the one to appear.</param>
		/// <exception cref="T:System.NotImplementedException">Thrown if the selector was opened for a case that it didn't expect.</exception>
		/// <remarks>
		/// <para>Either '<paramref name="key" />' or '<paramref name="id" />' must be set, but both can also be set for more consistent results.</para>
		/// </remarks>
		internal static void Show(object key, int id, InspectorProperty property, bool useUnitySelector = false)
		{
			IPropertyValueEntry entry = property.ValueEntry;
			Type valueType = ((entry.WeakSmartValue == null) ? entry.BaseValueType : entry.TypeOfValue);
			Show(Rect.zero, key, id, entry.WeakSmartValue, valueType, entry.BaseValueType, property.GetAttribute<AssetsOnlyAttribute>() == null, !entry.SerializationBackend.SupportsPolymorphism, property, useUnitySelector);
		}

		/// <summary>
		/// Shows a selector.
		/// </summary>
		/// <param name="position">The position where the selector should appear; can be <see cref="P:UnityEngine.Rect.zero" />.</param>
		/// <param name="key">The key used to identify who called the selector; this can be null.</param>
		/// <param name="id">The ID used to identify who called the selector; this can be 0.</param>
		/// <param name="property">
		/// Used to provide values for all parameters in the full declaration of this method.
		/// It also allows for customization of various stages of the selector with attributes.
		/// This also sets the <see cref="P:Sirenix.OdinInspector.Editor.OdinObjectSelector.SelectorProperty" />.
		/// </param>
		/// <param name="useUnitySelector">Determines if the default Unity Object Selector should be the one to appear.</param>
		/// <exception cref="T:System.NotImplementedException">Thrown if the selector was opened for a case that it didn't expect.</exception>
		/// <remarks>
		/// <para>Either '<paramref name="key" />' or '<paramref name="id" />' must be set, but both can also be set for more consistent results.</para>
		/// </remarks>
		internal static void Show(Rect position, object key, int id, InspectorProperty property, bool allowSceneObjects, bool useUnitySelector = false)
		{
			IPropertyValueEntry entry = property.ValueEntry;
			Type valueType = ((entry.WeakSmartValue == null) ? entry.BaseValueType : entry.TypeOfValue);
			Show(position, key, id, entry.WeakSmartValue, valueType, entry.BaseValueType, allowSceneObjects, !entry.SerializationBackend.SupportsPolymorphism, property, useUnitySelector);
		}

		/// <summary>
		/// Shows a selector.
		/// </summary>
		/// <param name="key">The key used to identify who called the selector; this can be null.</param>
		/// <param name="id">The ID used to identify who called the selector; this can be 0.</param>
		/// <param name="value">The current value selected.</param>
		/// <param name="baseType">The base type of the 'value'.</param>
		/// <param name="allowSceneObjects">Determines if scene objects are allowed.</param>
		/// <param name="disallowNullValues">Determines if null values are allowed.</param>
		/// <param name="property">Used for various stages of the selector that can be customized with attributes. This also sets the <see cref="P:Sirenix.OdinInspector.Editor.OdinObjectSelector.SelectorProperty" />.</param>
		/// <param name="useUnitySelector">Determines if the default Unity Object Selector should be the one to appear.</param>
		/// <exception cref="T:System.NotImplementedException">Thrown if the selector was opened for a case that it didn't expect.</exception>
		/// <remarks>
		/// <para>Either '<paramref name="key" />' or '<paramref name="id" />' must be set, but both can also be set for more consistent results.</para>
		/// </remarks>
		internal static void Show(object key, int id, object value, Type baseType, bool allowSceneObjects = true, bool disallowNullValues = false, InspectorProperty property = null, bool useUnitySelector = false)
		{
			Show(Rect.zero, key, id, value, (value == null) ? baseType : value.GetType(), baseType, allowSceneObjects, disallowNullValues, property, useUnitySelector);
		}

		/// <summary>
		/// Shows a selector.
		/// </summary>
		/// <param name="key">The key used to identify who called the selector; this can be null.</param>
		/// <param name="id">The ID used to identify who called the selector; this can be 0.</param>
		/// <param name="value">The current value selected.</param>
		/// <param name="valueType">The type of the 'value'.</param>
		/// <param name="baseType">The base type of the 'value'.</param>
		/// <param name="allowSceneObjects">Determines if scene objects are allowed.</param>
		/// <param name="disallowNullValues">Determines if null values are allowed.</param>
		/// <param name="property">Used for various stages of the selector that can be customized with attributes. This also sets the <see cref="P:Sirenix.OdinInspector.Editor.OdinObjectSelector.SelectorProperty" />.</param>
		/// <param name="useUnitySelector">Determines if the default Unity Object Selector should be the one to appear.</param>
		/// <exception cref="T:System.NotImplementedException">Thrown if the selector was opened for a case that it didn't expect.</exception>
		/// <remarks>
		/// <para>Either '<paramref name="key" />' or '<paramref name="id" />' must be set, but both can also be set for more consistent results.</para>
		/// </remarks>
		internal static void Show(object key, int id, object value, Type valueType, Type baseType, bool allowSceneObjects = true, bool disallowNullValues = false, InspectorProperty property = null, bool useUnitySelector = false)
		{
			Show(Rect.zero, key, id, value, valueType, baseType, allowSceneObjects, disallowNullValues, property, useUnitySelector);
		}

		/// <summary>
		/// Shows a selector.
		/// </summary>
		/// <param name="position">The position where the selector should appear; can be <see cref="P:UnityEngine.Rect.zero" />.</param>
		/// <param name="key">The key used to identify who called the selector; this can be null.</param>
		/// <param name="id">The ID used to identify who called the selector; this can be 0.</param>
		/// <param name="value">The current value selected.</param>
		/// <param name="baseType">The base type of the 'value'.</param>
		/// <param name="allowSceneObjects">Determines if scene objects are allowed.</param>
		/// <param name="disallowNullValues">Determines if null values are allowed.</param>
		/// <param name="property">Used for various stages of the selector that can be customized with attributes. This also sets the <see cref="P:Sirenix.OdinInspector.Editor.OdinObjectSelector.SelectorProperty" />.</param>
		/// <param name="useUnitySelector">Determines if the default Unity Object Selector should be the one to appear.</param>
		/// <exception cref="T:System.NotImplementedException">Thrown if the selector was opened for a case that it didn't expect.</exception>
		/// <remarks>
		/// <para>Either '<paramref name="key" />' or '<paramref name="id" />' must be set, but both can also be set for more consistent results.</para>
		/// </remarks>
		internal static void Show(Rect position, object key, int id, object value, Type baseType, bool allowSceneObjects = true, bool disallowNullValues = false, InspectorProperty property = null, bool useUnitySelector = false)
		{
			Show(position, key, id, value, (value == null) ? baseType : value.GetType(), baseType, allowSceneObjects, disallowNullValues, property, useUnitySelector);
		}

		/// <summary>
		/// Shows a selector.
		/// </summary>
		/// <param name="position">The position where the selector should appear; can be <see cref="P:UnityEngine.Rect.zero" />.</param>
		/// <param name="key">The key used to identify who called the selector; this can be null.</param>
		/// <param name="id">The ID used to identify who called the selector; this can be 0.</param>
		/// <param name="value">The current value selected.</param>
		/// <param name="valueType">The type of the 'value'.</param>
		/// <param name="baseType">The base type of the 'value'.</param>
		/// <param name="allowSceneObjects">Determines if scene objects are allowed.</param>
		/// <param name="disallowNullValues">Determines if null values are allowed.</param>
		/// <param name="property">Used for various stages of the selector that can be customized with attributes. This also sets the <see cref="P:Sirenix.OdinInspector.Editor.OdinObjectSelector.SelectorProperty" />.</param>
		/// <param name="useUnitySelector">Determines if the default Unity Object Selector should be the one to appear.</param>
		/// <exception cref="T:System.NotImplementedException">Thrown if the selector was opened for a case that it didn't expect.</exception>
		/// <remarks>
		/// <para>Either '<paramref name="key" />' or '<paramref name="id" />' must be set, but both can also be set for more consistent results.</para>
		/// </remarks>
		internal static void Show(Rect position, object key, int id, object value, Type valueType, Type baseType, bool allowSceneObjects = true, bool disallowNullValues = false, InspectorProperty property = null, bool useUnitySelector = false)
		{
			IsSelectorReadyToClaim = false;
			SelectorProperty = property;
			SelectorKey = key;
			SelectorId = id;
			SelectorObject = value;
			IsOpen = true;
			IsUnityObjectSelector = false;
			reopenInfo = ReopenInfo.None;
			SelectorValueType = valueType;
			SelectorBaseType = baseType;
			wasObjectChanged = false;
			showedInEvent = Event.current.type;
			isUnitySelectorUsingCallbacks = false;
			if (valueType == typeof(TypeSelectorV2.TypeSelectorAllUnityTypes))
			{
				string searchFilter = SirenixObjectPickerUtilities.GetSearchFilterForPolymorphicType(baseType);
				ShowUnityObjectSelector(value, typeof(UnityEngine.Object), allowSceneObjects, searchFilter, SelectorId, property);
				SirenixObjectPickerUtilities.MoveCaretToEndOfSearchFilter();
				return;
			}
			if (typeof(UnityEngine.Object).IsAssignableFrom(baseType) || useUnitySelector)
			{
				if (UnityShims.Misc.GetEventModifiers(Event.current) == 2)
				{
					SelectorObject = null;
					IsSelectorReadyToClaim = true;
				}
				else
				{
					ShowUnityObjectSelector(value, baseType, allowSceneObjects, string.Empty, SelectorId, property);
				}
				return;
			}
			if (baseType == typeof(string))
			{
				SelectorObject = "";
				IsSelectorReadyToClaim = true;
				return;
			}
			if (baseType.IsValueType)
			{
				if (disallowNullValues)
				{
					SelectorObject = UnitySerializationUtility.CreateDefaultUnityInitializedObject(baseType);
				}
				else if (baseType.IsValueType && !baseType.IsPrimitive)
				{
					SelectorObject = Activator.CreateInstance(baseType);
				}
				else
				{
					SelectorObject = UnitySerializationUtility.CreateDefaultUnityInitializedObject(baseType);
				}
				IsSelectorReadyToClaim = true;
				return;
			}
			if (typeof(Delegate).IsAssignableFrom(baseType))
			{
				SelectorObject = null;
				IsSelectorReadyToClaim = true;
				return;
			}
			if (baseType.IsClass || baseType.IsInterface)
			{
				if (disallowNullValues)
				{
					if (baseType.IsInterface)
					{
						ResetState();
						Debug.LogError("Property is serialized by Unity, where interfaces are not supported.");
					}
					else if (baseType.IsAbstract)
					{
						ResetState();
						Debug.LogError("Property is serialized by Unity, where abstract classes are not supported.");
					}
					else
					{
						SelectorObject = UnitySerializationUtility.CreateDefaultUnityInitializedObject(baseType);
						IsSelectorReadyToClaim = true;
					}
					return;
				}
				if (!GlobalConfig<GeneralDrawerConfig>.Instance.useNewObjectSelector)
				{
					if (position.width > 0f && position.height > 0f)
					{
						InstanceCreator.Show(baseType, SelectorId, position);
					}
					else
					{
						InstanceCreator.Show(baseType, SelectorId);
					}
					return;
				}
				if (Event.current != null && UnityShims.Misc.GetEventModifiers(Event.current) == 2)
				{
					Type selectedType = GetFirstValidType(baseType, property);
					if (selectedType == null)
					{
						SelectorObject = null;
					}
					else if (typeof(UnityEngine.Object).IsAssignableFrom(selectedType))
					{
						SelectorObject = null;
					}
					else
					{
						TypeRegistry.CheckedInstance checkedInstance = TypeRegistry.CreateCheckedInstance(selectedType, property);
						SelectorObject = ((checkedInstance.Result == TypeRegistry.CheckedInstanceResult.Success) ? checkedInstance.Instance : null);
					}
					IsSelectorReadyToClaim = true;
					Event.current.Use();
					return;
				}
				bool includeUnityTypes = property?.GetAttribute<SerializeReference>() == null;
				List<Type> inheritors = GetInheritors(baseType, includeUnityTypes, property);
				bool showDefaultCtorInfo = true;
				if (property != null)
				{
					PolymorphicDrawerSettingsAttribute settings = property.GetAttribute<PolymorphicDrawerSettingsAttribute>();
					if (settings != null && !string.IsNullOrEmpty(settings.CreateInstanceFunction))
					{
						showDefaultCtorInfo = false;
					}
				}
				TypeSelectorV2 selector = new TypeSelectorV2(inheritors, supportsMultiSelect: false, (value == null) ? null : valueType, null, showHidden: false, null, null, property)
				{
					useSingleClick = true,
					HideNonDefaultCtorInfo = !showDefaultCtorInfo
				};
				bool hasUnityInheritors = false;
				foreach (Type inheritor in inheritors)
				{
					if (typeof(UnityEngine.Object).IsAssignableFrom(inheritor))
					{
						hasUnityInheritors = true;
						break;
					}
				}
				selector.CategorizeUnityObjects = hasUnityInheritors;
				selector.SelectionCancelled += ResetState;
				selector.SelectionConfirmed += delegate(IEnumerable<Type> typesEnumerable)
				{
					Type[] array = typesEnumerable.ToArray();
					if (array.Length < 1)
					{
						ResetState();
					}
					else
					{
						for (int i = 0; i < array.Length; i++)
						{
							if (array[i] == typeof(TypeSelectorV2.TypeSelectorAllUnityTypes) || typeof(UnityEngine.Object).IsAssignableFrom(array[i]))
							{
								reopenInfo = new ReopenInfo(array[i], baseType, allowSceneObjects);
								return;
							}
						}
						TypeRegistry.CheckedInstance checkedInstance2 = TypeRegistry.CreateCheckedInstance(array[0], property);
						if (checkedInstance2.Result == TypeRegistry.CheckedInstanceResult.Success)
						{
							SelectorObject = checkedInstance2.Instance;
							IsSelectorReadyToClaim = true;
						}
					}
				};
				if (position.width > 0f && position.height > 0f)
				{
					selector.ShowInPopup(position);
				}
				else
				{
					selector.ShowInAux();
				}
				return;
			}
			ResetState();
			throw new NotImplementedException();
		}

		/// <summary>
		/// If the object was changed since the last time this method was called, it returns the changed object (<see cref="P:Sirenix.OdinInspector.Editor.OdinObjectSelector.SelectorObject" />); otherwise, it returns the provided value.
		/// </summary>
		/// <param name="value">The current value; the value to return if no change has occurred.</param>
		/// <param name="key">The key to identify who showed the current selector.</param>
		/// <param name="id">The ID to identify who showed the current selector.</param>
		/// <typeparam name="T">The type of object to expect as a return value.</typeparam>
		/// <returns><see cref="P:Sirenix.OdinInspector.Editor.OdinObjectSelector.SelectorObject" /> if it was marked as changed, otherwise <paramref name="value" />.</returns>
		public static T GetChangedObject<T>(T value, object key, int id)
		{
			if (!IsOpen)
			{
				return value;
			}
			if (!wasObjectChanged)
			{
				return value;
			}
			if (id != SelectorId || !object.Equals(key, SelectorKey))
			{
				return value;
			}
			if (!IsCurrentEventValid())
			{
				return value;
			}
			wasObjectChanged = false;
			GUI.changed = true;
			return (T)SelectorObject;
		}

		/// <summary>
		/// Checks if the selector's object is ready to be claimed.
		/// </summary>
		/// <param name="key">The key to identify who showed the current selector.</param>
		/// <param name="id">The ID to identify who showed the current selector.</param>
		/// <returns><c>true</c> if the selector's object (<see cref="P:Sirenix.OdinInspector.Editor.OdinObjectSelector.SelectorObject" />) is ready to be claimed; otherwise, <c>false</c>.</returns>
		public static bool IsReadyToClaim(object key, int id)
		{
			if (!IsOpen)
			{
				return false;
			}
			if (id != SelectorId || !object.Equals(key, SelectorKey))
			{
				return false;
			}
			if (!IsCurrentEventValid())
			{
				return false;
			}
			if (!GlobalConfig<GeneralDrawerConfig>.Instance.useNewObjectSelector && InstanceCreator.ControlID == id && InstanceCreator.HasCreatedInstance)
			{
				object val = InstanceCreator.GetCreatedInstance();
				SelectorObject = val;
				IsSelectorReadyToClaim = true;
				return IsSelectorReadyToClaim;
			}
			if (!IsUnityObjectSelector)
			{
				if (!reopenInfo.IsNone)
				{
					Type baseTypeToReopen = ((reopenInfo.Type == typeof(TypeSelectorV2.TypeSelectorAllUnityTypes)) ? reopenInfo.BaseType : reopenInfo.Type);
					Show(Rect.zero, SelectorKey, SelectorId, SelectorObject, reopenInfo.Type, baseTypeToReopen, reopenInfo.AllowSceneObjects, disallowNullValues: false, SelectorProperty);
				}
				return IsSelectorReadyToClaim;
			}
			if (Event.current.type == EventType.ExecuteCommand && Event.current.commandName == "ObjectSelectorCanceled")
			{
				ResetState();
				Event.current.Use();
				return false;
			}
			if (isUnitySelectorUsingCallbacks)
			{
				return IsSelectorReadyToClaim;
			}
			if (id != EditorGUIUtility.GetObjectPickerControlID())
			{
				return false;
			}
			if (Event.current.type == EventType.ExecuteCommand)
			{
				string commandName = Event.current.commandName;
				if (!(commandName == "ObjectSelectorUpdated"))
				{
					if (commandName == "ObjectSelectorClosed")
					{
						OnUnitySelectorClosed(null);
						Event.current.Use();
					}
				}
				else
				{
					UnityEngine.Object currentObject = EditorGUIUtility.GetObjectPickerObject();
					OnUnitySelectorUpdated(currentObject);
					Event.current.Use();
				}
			}
			return IsSelectorReadyToClaim;
		}

		/// <summary>
		/// Determines whether the specified <paramref name="key" /> and <paramref name="id" /> combination match the currently open selectors.
		/// </summary>
		/// <param name="key">The key to match.</param>
		/// <param name="id">The ID to match.</param>
		/// <returns><c>true</c> if the combination is a match; otherwise, <c>false</c>.</returns>
		public static bool IsCurrentSelector(object key, int id)
		{
			if (IsOpen && object.Equals(key, SelectorKey))
			{
				return SelectorId == id;
			}
			return false;
		}

		/// <summary>
		/// Claims the current <see cref="P:Sirenix.OdinInspector.Editor.OdinObjectSelector.SelectorObject" />.
		/// </summary>
		/// <returns>The <see cref="P:Sirenix.OdinInspector.Editor.OdinObjectSelector.SelectorObject" /> if it is ready to be claimed; otherwise, NULL with an accompanying error message.</returns>
		public static object Claim()
		{
			if (!IsSelectorReadyToClaim)
			{
				Debug.LogError($"Attempted to claim an object, when no object is ready to be claimed (ID: {SelectorId}, Key: {SelectorKey}, Current Object: {SelectorObject}).");
				ResetState();
				return null;
			}
			object result = SelectorObject;
			ResetState();
			return result;
		}

		/// <summary>
		/// Claims the current <see cref="P:Sirenix.OdinInspector.Editor.OdinObjectSelector.SelectorObject" /> and copies a specified amount (<paramref name="amount" />) of instances of the <see cref="P:Sirenix.OdinInspector.Editor.OdinObjectSelector.SelectorObject" />.
		/// </summary>
		/// <param name="amount">The number of instances to copy.</param>
		/// <returns>
		/// An array of <see cref="P:Sirenix.OdinInspector.Editor.OdinObjectSelector.SelectorObject" /> copies, where element 0 is always the original value, 
		/// if the <see cref="P:Sirenix.OdinInspector.Editor.OdinObjectSelector.SelectorObject" /> is ready to be claimed; otherwise, NULL with an accompanying error message.
		/// </returns>
		public static object[] ClaimMultiple(int amount)
		{
			if (!IsSelectorReadyToClaim)
			{
				Debug.LogError($"Attempted to claim an object, when no object is ready to be claimed (ID: {SelectorId}, Key: {SelectorKey}, Current Object: {SelectorObject}).");
				ResetState();
				return null;
			}
			if (amount < 1)
			{
				Debug.LogError($"Attempted to claim {amount} objects.");
				return Array.Empty<object>();
			}
			object pickedObj = SelectorObject;
			ResetState();
			if (amount <= 1)
			{
				return new object[1] { pickedObj };
			}
			object[] result = new object[amount];
			result[0] = pickedObj;
			for (int i = 1; i < result.Length; i++)
			{
				result[i] = FastDeepCopier.DeepCopy(pickedObj);
			}
			return result;
		}

		/// <summary>
		/// Claims the current <see cref="P:Sirenix.OdinInspector.Editor.OdinObjectSelector.SelectorObject" /> and, if ready, assigns it to the specified <see cref="T:Sirenix.OdinInspector.Editor.InspectorProperty" />.
		/// If the <see cref="P:Sirenix.OdinInspector.Editor.OdinObjectSelector.SelectorObject" /> is not ready to be claimed, an error message will be logged.
		/// </summary>
		/// <param name="property">The <see cref="T:Sirenix.OdinInspector.Editor.InspectorProperty" /> to assign the <see cref="P:Sirenix.OdinInspector.Editor.OdinObjectSelector.SelectorObject" /> to.</param>
		internal static object ClaimAndAssign(InspectorProperty property)
		{
			if (property == null)
			{
				Debug.LogError("Attempted to claim and assign an object to a NULL property.");
				ResetState();
				return null;
			}
			if (!IsSelectorReadyToClaim)
			{
				Debug.LogError($"Attempted to claim an object, when no object is ready to be claimed (ID: {SelectorId}, Key: {SelectorKey}, Current Object: {SelectorObject}).");
				ResetState();
				return null;
			}
			object currentObject = SelectorObject;
			property.Tree.DelayActionUntilRepaint(delegate
			{
				if (property.ValueEntry.WeakValues.Count <= 1)
				{
					property.ValueEntry.WeakSmartValue = currentObject;
				}
				else
				{
					property.ValueEntry.WeakValues[0] = currentObject;
					for (int i = 1; i < property.ValueEntry.WeakValues.Count; i++)
					{
						property.ValueEntry.WeakValues[i] = FastDeepCopier.DeepCopy(currentObject);
					}
				}
				GUIHelper.RequestRepaint();
			});
			GUIHelper.RequestRepaint();
			ResetState();
			return currentObject;
		}

		private static void ShowUnityObjectSelector(object obj, Type objType, bool allowSceneObjects, string searchFilter, int controlID, InspectorProperty property)
		{
			IsUnityObjectSelector = true;
			isUnitySelectorUsingCallbacks = ObjectSelector_Internal.CanUseCallbacks;
			if (!typeof(UnityEngine.Object).IsAssignableFrom(objType))
			{
				if (string.IsNullOrEmpty(searchFilter))
				{
					searchFilter = SirenixObjectPickerUtilities.GetSearchFilterForPolymorphicType(objType);
				}
				objType = typeof(UnityEngine.Object);
			}
			UnityEngine.Object objectBeingEdited = null;
			if (property != null)
			{
				objectBeingEdited = property.Tree.RootProperty.ValueEntry.WeakSmartValue as UnityEngine.Object;
			}
			Action<UnityEngine.Object> onUpdate = OnUnitySelectorUpdated;
			Action<UnityEngine.Object> onClose = OnUnitySelectorClosed;
			if (obj is UnityEngine.Object unityObj)
			{
				ObjectSelector_Internal.ShowObjectSelector(unityObj, objType, objectBeingEdited, allowSceneObjects, searchFilter, controlID, onClose, onUpdate);
			}
			else
			{
				ObjectSelector_Internal.ShowObjectSelector(null, objType, objectBeingEdited, allowSceneObjects, searchFilter, controlID, onClose, onUpdate);
			}
			if (!string.IsNullOrEmpty(searchFilter))
			{
				SirenixObjectPickerUtilities.MoveCaretToEndOfSearchFilter();
			}
		}

		private static Type GetFirstValidType(Type type, InspectorProperty property)
		{
			ValueResolver<bool> typeFilterFunction = null;
			if (property != null)
			{
				TypeSelectorSettingsAttribute settings = property.GetAttribute<TypeSelectorSettingsAttribute>();
				if (settings != null && !string.IsNullOrEmpty(settings.FilterTypesFunction))
				{
					typeFilterFunction = ValueResolver.Get<bool>(property, settings.FilterTypesFunction, new NamedValue[1]
					{
						new NamedValue("type", typeof(Type), null)
					});
					if (typeFilterFunction.HasError)
					{
						Debug.LogWarning(typeFilterFunction.ErrorMessage);
					}
				}
			}
			bool includeUnityTypes = property?.GetAttribute<SerializeReference>() == null;
			List<Type> inheritors = GetInheritors(type, includeUnityTypes, property);
			if (inheritors.Count <= 0)
			{
				if (!type.IsAbstract && !type.IsInterface)
				{
					if (typeFilterFunction == null)
					{
						return type;
					}
					typeFilterFunction.Context.NamedValues.Set("type", type);
					if (typeFilterFunction.GetValue())
					{
						return type;
					}
				}
				return null;
			}
			inheritors.Sort(CompareTypes);
			if (typeFilterFunction == null)
			{
				return inheritors[0];
			}
			for (int i = 0; i < inheritors.Count; i++)
			{
				typeFilterFunction.Context.NamedValues.Set("type", inheritors[i]);
				if (typeFilterFunction.GetValue())
				{
					return inheritors[i];
				}
			}
			return null;
		}

		private static List<Type> GetInheritors(Type type, bool includeUnityTypes, InspectorProperty property)
		{
			if (property == null)
			{
				return TypeRegistry.GetInstantiableInheritors(type, includeUnityTypes);
			}
			PolymorphicDrawerSettingsAttribute settings = property.GetAttribute<PolymorphicDrawerSettingsAttribute>();
			bool excludeTypesWithoutDefaultConstructor;
			if (settings != null)
			{
				excludeTypesWithoutDefaultConstructor = ((!settings.NonDefaultConstructorPreferenceIsSet) ? (GlobalConfig<GeneralDrawerConfig>.Instance.nonDefaultConstructorPreference == NonDefaultConstructorPreference.Exclude) : (settings.NonDefaultConstructorPreference == NonDefaultConstructorPreference.Exclude));
				if (!string.IsNullOrEmpty(settings.CreateInstanceFunction))
				{
					excludeTypesWithoutDefaultConstructor = false;
				}
			}
			else
			{
				excludeTypesWithoutDefaultConstructor = GlobalConfig<GeneralDrawerConfig>.Instance.nonDefaultConstructorPreference == NonDefaultConstructorPreference.Exclude;
			}
			return TypeRegistry.GetInstantiableInheritors(type, includeUnityTypes, excludeTypesWithoutDefaultConstructor);
		}

		private static int CompareTypes(Type self, Type other)
		{
			int priorityCmp = TypeRegistry.GetPriority(other).CompareTo(TypeRegistry.GetPriority(self));
			if (priorityCmp != 0)
			{
				return priorityCmp;
			}
			bool isSelfUnity = typeof(UnityEngine.Object).IsAssignableFrom(self);
			bool isOtherUnity = typeof(UnityEngine.Object).IsAssignableFrom(other);
			return isSelfUnity.CompareTo(isOtherUnity);
		}

		private static void ResetState()
		{
			SelectorProperty = null;
			SelectorKey = null;
			SelectorId = 0;
			SelectorObject = null;
			IsOpen = false;
			IsSelectorReadyToClaim = false;
			IsUnityObjectSelector = false;
			reopenInfo = ReopenInfo.None;
			SelectorValueType = null;
			SelectorBaseType = null;
			wasObjectChanged = false;
			showedInEvent = EventType.Ignore;
			isUnitySelectorUsingCallbacks = false;
		}

		private static void OnUnitySelectorUpdated(UnityEngine.Object callbackObject)
		{
			UnityEngine.Object nextObject = callbackObject;
			if (callbackObject is GameObject gameObject)
			{
				if (SelectorValueType == typeof(UnityEngine.Object) || SelectorBaseType == typeof(object) || typeof(GameObject).IsAssignableFrom(SelectorValueType))
				{
					nextObject = callbackObject;
				}
				else if (SelectorBaseType == typeof(Component) || SelectorBaseType == typeof(MonoBehaviour))
				{
					Component[] components = gameObject.GetComponents(SelectorBaseType);
					nextObject = ((components.Length == 0) ? null : components[0]);
				}
				else if (SelectorValueType == typeof(TypeSelectorV2.TypeSelectorAllUnityTypes))
				{
					nextObject = null;
					Component[] allComponents = gameObject.GetComponents(typeof(Component));
					List<Type> inheritors = GetInheritors(SelectorBaseType, includeUnityTypes: true, SelectorProperty);
					for (int i = 0; i < allComponents.Length; i++)
					{
						for (int j = 0; j < inheritors.Count; j++)
						{
							if (inheritors[j].IsInstanceOfType(allComponents[i]))
							{
								nextObject = allComponents[i];
								break;
							}
						}
						if (nextObject != null)
						{
							break;
						}
					}
				}
				else if (SelectorValueType.IsSubclassOf(typeof(Component)))
				{
					nextObject = gameObject.GetComponent(SelectorValueType);
				}
			}
			else
			{
				nextObject = callbackObject;
			}
			if (nextObject == null || SelectorBaseType.IsInstanceOfType(nextObject))
			{
				SelectorObject = nextObject;
				wasObjectChanged = true;
			}
		}

		private static void OnUnitySelectorClosed(UnityEngine.Object callbackObject)
		{
			Type selectorType = TwoWaySerializationBinder.Default.BindToType("UnityEditor.ObjectSelector, UnityEditor.CoreModule");
			if (selectorType != null)
			{
				MethodInfo getterIsSelectionCancelled = selectorType.GetMethod("SelectionCanceled", BindingFlags.Static | BindingFlags.Public);
				if (getterIsSelectionCancelled != null)
				{
					if ((bool)getterIsSelectionCancelled.Invoke(null, Array.Empty<object>()))
					{
						ResetState();
					}
					else if (IsOpen)
					{
						IsSelectorReadyToClaim = true;
					}
					return;
				}
			}
			if (IsOpen)
			{
				IsSelectorReadyToClaim = true;
			}
			else
			{
				ResetState();
			}
		}
	}
}
