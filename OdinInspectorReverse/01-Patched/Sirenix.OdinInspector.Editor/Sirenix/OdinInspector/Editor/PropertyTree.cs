using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Sirenix.OdinInspector.Editor.Internal;
using Sirenix.OdinInspector.Editor.Validation;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor
{
	/// <summary>
	/// <para>Represents a set of values of the same type as a tree of properties that can be drawn in the inspector, and provides an array of utilities for querying the tree of properties.</para>
	/// </summary>
	public abstract class PropertyTree : IDisposable
	{
		private struct PropertyPathResult
		{
			public InspectorProperty Property;

			public InspectorProperty ClosestProperty;
		}

		/// <summary>
		/// Delegate for on property value changed callback.
		/// </summary>
		public delegate void OnPropertyValueChangedDelegate(InspectorProperty property, int selectionIndex);

		private Dictionary<string, Dictionary<Type, SerializedProperty>> emittedUnityPropertyCache = new Dictionary<string, Dictionary<Type, SerializedProperty>>();

		private static GUIFrameCounter frameCounter;

		private static int drawnInspectorDepthCount;

		private static ValueGetter<SerializedObject, IntPtr> SerializedObject_nativeObjectPtrGetter;

		private MethodInfo onValidateMethod;

		private OdinAttributeProcessorLocator attributeProcessorLocator;

		private OdinPropertyResolverLocator propertyResolverLocator;

		private DrawerChainResolver drawerChainResolver;

		private StateUpdaterLocator stateUpdaterLocator;

		private SerializationBackend serializationBackend;

		internal float ContextWidth;

		public bool RecordUndoForChanges;

		/// <summary>
		/// This will be replaced by an IMGUIDrawingComponent in patch 3.2.
		/// </summary>
		internal bool TreeIsSetupForIMGUIDrawing_TEMP_INTERNAL = true;

		internal bool IsMadeForDesignerEditor;

		internal bool IsDesignerTree;

		protected SerializedProperty monoScriptProperty;

		protected bool monoScriptPropertyHasBeenGotten;

		internal bool hasSetupSearchFilter;

		private PropertySearchFilter searchFilter;

		public bool AllowSearchFiltering = true;

		private static WeakReferenceEventListener<PropertyTree> UndoEventListener;

		public static readonly EditorPrefBool EnableLeakDetection;

		/// <summary>
		/// The component providers that create components for each property in the tree. If you change this list after the tree has been used, you should call tree.RootProperty.RefreshSetup() to make the changes update properly throughout the tree.
		/// </summary>
		public readonly List<ComponentProvider> ComponentProviders = new List<ComponentProvider>();

		private StackTrace allocationTrace;

		private volatile bool disposedValue;

		/// <summary>
		/// The <see cref="T:UnityEditor.SerializedObject" /> that this tree represents, if the tree was created for a <see cref="T:UnityEditor.SerializedObject" />.
		/// </summary>
		public abstract SerializedObject UnitySerializedObject { get; }

		/// <summary>
		/// The current update ID of the tree. This is incremented once, each update, and is used by <see cref="M:Sirenix.OdinInspector.Editor.InspectorProperty.Update(System.Boolean)" /> to avoid updating multiple times in the same update round.
		/// </summary>
		public abstract int UpdateID { get; }

		/// <summary>
		/// The type of the values that the property tree represents.
		/// </summary>
		public abstract Type TargetType { get; }

		/// <summary>
		/// The actual values that the property tree represents.
		/// </summary>
		public abstract ImmutableList<object> WeakTargets { get; }

		/// <summary>
		/// The number of root properties in the tree.
		/// </summary>
		public abstract int RootPropertyCount { get; }

		/// <summary>
		/// The prefab modification handler of the tree.
		/// </summary>
		public abstract PrefabModificationHandler PrefabModificationHandler { get; }

		/// <summary>
		/// Whether this property tree also represents members that are specially serialized by Odin.
		/// </summary>
		[Obsolete("This value is no longer guaranteed to be correct, as it may have different answers for different properties in the tree. Instead look at InspectorProperty.SerializationRoot to determine whether specially serialized members might be included.", true)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public abstract bool IncludesSpeciallySerializedMembers { get; }

		/// <summary>
		/// Gets a value indicating whether or not to draw the mono script object field at the top of the property tree.
		/// </summary>
		public bool DrawMonoScriptObjectField { get; set; }

		/// <summary>
		/// Gets a value indicating whether or not the PropertyTree is inspecting a static type.
		/// </summary>
		public bool IsStatic { get; protected set; }

		/// <summary>
		/// The serialization backend used to determine how to draw this property tree. Set this to control.
		/// </summary>
		public SerializationBackend SerializationBackend
		{
			get
			{
				if (serializationBackend == null)
				{
					bool odinSerialized = InspectorPropertyInfoUtility.TypeDefinesShowOdinSerializedPropertiesInInspectorAttribute_Cached(TargetType);
					serializationBackend = (odinSerialized ? SerializationBackend.Odin : SerializationBackend.Unity);
				}
				return serializationBackend;
			}
			set
			{
				if (serializationBackend != value)
				{
					serializationBackend = value;
					DisposeAndResetRootProperty();
				}
			}
		}

		/// <summary>
		/// Gets or sets the <see cref="T:Sirenix.OdinInspector.Editor.OdinAttributeProcessorLocator" /> for the PropertyTree.
		/// </summary>
		public OdinAttributeProcessorLocator AttributeProcessorLocator
		{
			get
			{
				if (attributeProcessorLocator == null)
				{
					attributeProcessorLocator = DefaultOdinAttributeProcessorLocator.Instance;
				}
				return attributeProcessorLocator;
			}
			set
			{
				if (attributeProcessorLocator != value)
				{
					attributeProcessorLocator = value;
					RootProperty.RefreshSetup();
				}
			}
		}

		/// <summary>
		/// Gets or sets the <see cref="T:Sirenix.OdinInspector.Editor.OdinPropertyResolverLocator" /> for the PropertyTree.
		/// </summary>
		public OdinPropertyResolverLocator PropertyResolverLocator
		{
			get
			{
				if (propertyResolverLocator == null)
				{
					propertyResolverLocator = DefaultOdinPropertyResolverLocator.Instance;
				}
				return propertyResolverLocator;
			}
			set
			{
				if (propertyResolverLocator != value)
				{
					propertyResolverLocator = value;
					RootProperty.RefreshSetup();
				}
			}
		}

		/// <summary>
		/// Gets or sets the <see cref="T:Sirenix.OdinInspector.Editor.DrawerChainResolver" /> for the PropertyTree.
		/// </summary>
		public DrawerChainResolver DrawerChainResolver
		{
			get
			{
				if (drawerChainResolver == null)
				{
					drawerChainResolver = DefaultDrawerChainResolver.Instance;
				}
				return drawerChainResolver;
			}
			set
			{
				if (drawerChainResolver != value)
				{
					drawerChainResolver = value;
					RootProperty.RefreshSetup();
				}
			}
		}

		/// <summary>
		/// Gets or sets the <see cref="T:Sirenix.OdinInspector.Editor.StateUpdaterLocator" /> for the PropertyTree.
		/// </summary>
		public StateUpdaterLocator StateUpdaterLocator
		{
			get
			{
				if (stateUpdaterLocator == null)
				{
					stateUpdaterLocator = DefaultStateUpdaterLocator.Instance;
				}
				return stateUpdaterLocator;
			}
			set
			{
				if (stateUpdaterLocator != value)
				{
					stateUpdaterLocator = value;
					RootProperty.RefreshSetup();
				}
			}
		}

		protected abstract bool HasRootPropertyYet { get; }

		/// <summary>
		/// Gets the root property of the tree.
		/// </summary>
		public abstract InspectorProperty RootProperty { get; }

		/// <summary>
		/// Gets the secret root property of the tree, which hosts the property resolver used to resolve the "actual" root properties of the tree.
		/// </summary>
		[Obsolete("Use RootProperty instead; the root is no longer considered 'secret'.", false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public abstract InspectorProperty SecretRootProperty { get; }

		/// <summary>
		/// An event that is invoked whenever an undo or a redo is performed in the inspector.
		/// The advantage of using this event on a property tree instance instead of
		/// <see cref="F:UnityEditor.Undo.undoRedoPerformed" /> is that this event will be desubscribed from
		/// <see cref="F:UnityEditor.Undo.undoRedoPerformed" /> when the selection changes and the property
		/// tree is no longer being used, allowing the GC to collect the property tree.
		/// </summary>
		public event Action OnUndoRedoPerformed;

		/// <summary>
		/// This event is invoked whenever the value of any property in the entire property tree is changed through the property system.
		/// </summary>
		public event OnPropertyValueChangedDelegate OnPropertyValueChanged;

		static PropertyTree()
		{
			frameCounter = new GUIFrameCounter();
			drawnInspectorDepthCount = 0;
			EnableLeakDetection = new EditorPrefBool("OdinPropertyTree_EnableLeakDetection", defaultValue: true);
			string nativeObjectPtrName = (UnityVersion.IsVersionOrGreater(2018, 3) ? "m_NativeObjectPtr" : "m_Property");
			FieldInfo nativeObjectPtrField = typeof(SerializedObject).GetField(nativeObjectPtrName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if (nativeObjectPtrField != null)
			{
				SerializedObject_nativeObjectPtrGetter = EmitUtilities.CreateInstanceFieldGetter<SerializedObject, IntPtr>(nativeObjectPtrField);
			}
			else
			{
				UnityEngine.Debug.LogWarning("The internal Unity field SerializedObject.m_Property (< 2018.3)/SerializedObject.m_NativeObjectPtr (>= 2018.3) has been renamed in this version of Unity!");
			}
			UndoEventListener = new WeakReferenceEventListener<PropertyTree>(delegate(PropertyTree tree, object[] args)
			{
				tree.InvokeOnUndoRedoPerformed();
			});
			Undo.undoRedoPerformed = (Undo.UndoRedoCallback)Delegate.Combine(Undo.undoRedoPerformed, new Undo.UndoRedoCallback(GlobalUndoEventSubscription));
		}

		private static void GlobalUndoEventSubscription()
		{
			UndoEventListener.InvokeEvent(null);
		}

		/// <summary>
		/// Creates a new <see cref="T:Sirenix.OdinInspector.Editor.PropertyTree" /> for all target values of a <see cref="T:UnityEditor.SerializedObject" />.
		/// </summary>
		public PropertyTree()
		{
			if (typeof(UnityEngine.Object).IsAssignableFrom(TargetType))
			{
				onValidateMethod = GetOnValidateMethod(TargetType);
				UndoEventListener.SubscribeListener(this);
			}
			if (EnableLeakDetection.Value)
			{
				allocationTrace = new StackTrace(fNeedFileInfo: true);
			}
		}

		private static MethodInfo GetOnValidateMethod(Type type)
		{
			MethodInfo method = type.GetMethod("OnValidate", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if (method == null)
			{
				type = type.BaseType;
				while (method == null && type != null)
				{
					method = type.GetMethod("OnValidate", BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
					type = type.BaseType;
				}
			}
			return method;
		}

		internal void InvokeOnPropertyValueChanged(InspectorProperty property, int selectionIndex)
		{
			if (this.OnPropertyValueChanged == null)
			{
				return;
			}
			try
			{
				this.OnPropertyValueChanged(property, selectionIndex);
			}
			catch (ExitGUIException ex)
			{
				throw ex;
			}
			catch (Exception ex2)
			{
				if (ex2.IsExitGUIException())
				{
					throw ex2.AsExitGUIException();
				}
				UnityEngine.Debug.LogException(ex2);
			}
		}

		public abstract void CleanForCachedReuse();

		public abstract void SetTargets(params object[] newTargets);

		public abstract void SetSerializedObject(SerializedObject serializedObject);

		/// <summary>
		/// Registers that a given property is dirty and needs its changes to be applied at the end of the current frame.
		/// </summary>
		public abstract void RegisterPropertyDirty(InspectorProperty property);

		/// <summary>
		/// Schedules a delegate to be invoked at the end of the current GUI frame.
		/// </summary>
		/// <param name="action">The action delegate to be delayed.</param>
		public abstract void DelayAction(Action action);

		/// <summary>
		/// Schedules a delegate to be invoked at the end of the next Repaint GUI frame.
		/// </summary>
		/// <param name="action">The action to be delayed.</param>
		public abstract void DelayActionUntilRepaint(Action action);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static int StringIndexOf(string str, char c, int start)
		{
			for (int i = start; i < str.Length; i++)
			{
				if (str[i] == c)
				{
					return i;
				}
			}
			return -1;
		}

		private InspectorProperty TryFindChildMemberPropertyWithNameFromGroups(StringSlice name, InspectorProperty property)
		{
			if (property.ChildResolver is ICollectionResolver)
			{
				return null;
			}
			for (int i = 0; i < property.Children.Count; i++)
			{
				InspectorProperty child = property.Children.Get(i);
				switch (child.Info.PropertyType)
				{
				case PropertyType.Value:
					if (child.Info.HasSingleBackingMember && child.Name == name)
					{
						return child;
					}
					break;
				case PropertyType.Group:
				{
					InspectorProperty found = TryFindChildMemberPropertyWithNameFromGroups(name, child);
					if (found != null)
					{
						return found;
					}
					break;
				}
				default:
					throw new NotImplementedException(child.Info.PropertyType.ToString());
				case PropertyType.Method:
					break;
				}
			}
			return null;
		}

		/// <summary>
		/// Enumerates over the properties of the tree.
		/// </summary>
		/// <param name="includeChildren">Whether to include children of the root properties or not. If set to true, every property in the entire tree will be enumerated.</param>
		/// <param name="onlyVisible">Whether to only include visible properties. Properties whose parents are invisible are considered invisible.</param>
		public virtual IEnumerable<InspectorProperty> EnumerateTree(bool includeChildren = true, bool onlyVisible = false)
		{
			if (includeChildren)
			{
				if (RootProperty.Children.Count == 0)
				{
					yield break;
				}
				for (InspectorProperty current = RootProperty.Children.Get(0); current != null; current = current.NextProperty(includeChildren: true, onlyVisible))
				{
					if (!onlyVisible || current.State.Visible)
					{
						yield return current;
					}
				}
				yield break;
			}
			for (int i = 0; i < RootProperty.Children.Count; i++)
			{
				InspectorProperty child = RootProperty.Children.Get(i);
				if (!onlyVisible || child.State.Visible)
				{
					yield return RootProperty.Children.Get(i);
				}
			}
		}

		/// <summary>
		/// Gets the property at the given path. Note that this is the path found in <see cref="P:Sirenix.OdinInspector.Editor.InspectorProperty.Path" />, not the Unity path.
		/// </summary>
		/// <param name="path">The path of the property to get.</param>
		public virtual InspectorProperty GetPropertyAtPath(string path)
		{
			InspectorProperty closest;
			return GetPropertyAtPath(path, out closest);
		}

		/// <summary>
		/// Gets the property at the given path. Note that this is the path found in <see cref="P:Sirenix.OdinInspector.Editor.InspectorProperty.Path" />, not the Unity path.
		/// </summary>
		/// <param name="path">The path of the property to get.</param>
		/// <param name="closestProperty"></param>
		public virtual InspectorProperty GetPropertyAtPath(string path, out InspectorProperty closestProperty)
		{
			if (path == "$ROOT")
			{
				closestProperty = RootProperty;
				return RootProperty;
			}
			closestProperty = null;
			int currentPathIndex = 0;
			int nextSeparator = StringIndexOf(path, '.', currentPathIndex);
			StringSlice step = ((nextSeparator == -1) ? new StringSlice(path) : path.Slice(currentPathIndex, nextSeparator - currentPathIndex));
			InspectorProperty current = RootProperty;
			PropertyPathResult result = default(PropertyPathResult);
			while (true)
			{
				result.ClosestProperty = current;
				current = current.Children.Get(ref step);
				if (current == null || nextSeparator == -1)
				{
					break;
				}
				currentPathIndex = nextSeparator + 1;
				nextSeparator = StringIndexOf(path, '.', currentPathIndex);
				step = ((nextSeparator == -1) ? path.Slice(currentPathIndex) : path.Slice(currentPathIndex, nextSeparator - currentPathIndex));
			}
			result.Property = current;
			if (result.Property == null && result.ClosestProperty != null)
			{
				int lastDot = path.LastIndexOf('.');
				if (lastDot > 0)
				{
					StringSlice lastPathStep = path.Slice(lastDot + 1);
					result.Property = result.ClosestProperty.Children.Get(ref lastPathStep);
				}
			}
			closestProperty = result.ClosestProperty;
			return result.Property;
		}

		/// <summary>
		/// Gets the property at the given Unity path.
		/// </summary>
		/// <param name="path">The Unity path of the property to get.</param>
		public virtual InspectorProperty GetPropertyAtUnityPath(string path)
		{
			InspectorProperty closest;
			return GetPropertyAtUnityPath(path, out closest);
		}

		/// <summary>
		/// Gets the property at the given Unity path.
		/// </summary>
		/// <param name="path">The Unity path of the property to get.</param>
		/// <param name="closestProperty"></param>
		public virtual InspectorProperty GetPropertyAtUnityPath(string path, out InspectorProperty closestProperty)
		{
			closestProperty = null;
			PropertyPathResult result = default(PropertyPathResult);
			result.ClosestProperty = null;
			int currentPathIndex = 0;
			int nextSeparator = StringIndexOf(path, '.', currentPathIndex);
			StringSlice step = ((nextSeparator == -1) ? new StringSlice(path) : path.Slice(currentPathIndex, nextSeparator - currentPathIndex));
			InspectorProperty current = RootProperty;
			while (true)
			{
				InspectorProperty next = current.Children.Get(ref step);
				if (next == null && step == "Array" && nextSeparator != -1)
				{
					int tempNextPathIndex = nextSeparator + 1;
					int tempNextSeparator = StringIndexOf(path, '.', tempNextPathIndex);
					StringSlice tempNextStep = ((tempNextSeparator == -1) ? path.Slice(tempNextPathIndex) : path.Slice(tempNextPathIndex, tempNextSeparator - tempNextPathIndex));
					if (tempNextStep.StartsWith("data[") && int.TryParse(tempNextStep.Slice(5, tempNextStep.Length - 6).ToString(), out var index))
					{
						string indexName = CollectionResolverUtilities.DefaultIndexToChildName(index);
						next = current.Children.Get(indexName);
						if (next != null)
						{
							currentPathIndex = tempNextPathIndex;
							nextSeparator = tempNextSeparator;
							step = tempNextStep;
						}
					}
				}
				if (next == null && !(current.ChildResolver is ICollectionResolver))
				{
					next = TryFindChildMemberPropertyWithNameFromGroups(step, current);
				}
				current = next;
				if (next == null || nextSeparator == -1)
				{
					break;
				}
				result.ClosestProperty = current;
				currentPathIndex = nextSeparator + 1;
				nextSeparator = StringIndexOf(path, '.', currentPathIndex);
				step = ((nextSeparator == -1) ? path.Slice(currentPathIndex) : path.Slice(currentPathIndex, nextSeparator - currentPathIndex));
			}
			result.Property = current;
			closestProperty = result.ClosestProperty;
			return result.Property;
		}

		/// <summary>
		/// Gets the property at the given deep reflection path.
		/// </summary>
		/// <param name="path">The deep reflection path of the property to get.</param>
		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("Use GetPropertyAtPrefabModificationPath instead.", false)]
		public InspectorProperty GetPropertyAtDeepReflectionPath(string path)
		{
			return GetPropertyAtPrefabModificationPath(path);
		}

		/// <summary>
		/// Gets the property at the given Odin prefab modification path.
		/// </summary>
		/// <param name="path">The prefab modification path of the property to get.</param>
		public virtual InspectorProperty GetPropertyAtPrefabModificationPath(string path)
		{
			InspectorProperty closest;
			return GetPropertyAtPrefabModificationPath(path, out closest);
		}

		/// <summary>
		/// Gets the property at the given Odin prefab modification path.
		/// </summary>
		/// <param name="path">The prefab modification path of the property to get.</param>
		/// <param name="closestProperty"></param>
		public virtual InspectorProperty GetPropertyAtPrefabModificationPath(string path, out InspectorProperty closestProperty)
		{
			closestProperty = null;
			PropertyPathResult result = default(PropertyPathResult);
			result.ClosestProperty = null;
			int currentPathIndex = 0;
			int nextSeparator = StringIndexOf(path, '.', currentPathIndex);
			StringSlice step = ((nextSeparator == -1) ? new StringSlice(path) : path.Slice(currentPathIndex, nextSeparator - currentPathIndex));
			InspectorProperty current = RootProperty;
			while (true)
			{
				InspectorProperty next = current.Children.Get(ref step);
				if (next == null && !(current.ChildResolver is ICollectionResolver))
				{
					next = TryFindChildMemberPropertyWithNameFromGroups(step, current);
				}
				current = next;
				if (next == null || nextSeparator == -1)
				{
					break;
				}
				result.ClosestProperty = current;
				currentPathIndex = nextSeparator + 1;
				nextSeparator = StringIndexOf(path, '.', currentPathIndex);
				step = ((nextSeparator == -1) ? path.Slice(currentPathIndex) : path.Slice(currentPathIndex, nextSeparator - currentPathIndex));
			}
			result.Property = current;
			closestProperty = result.ClosestProperty;
			return result.Property;
		}

		/// <summary>
		/// <para>Draw the property tree, and handles management of undo, as well as marking scenes and drawn assets dirty.</para>
		/// <para>
		/// This is a shorthand for calling
		/// <see cref="M:Sirenix.OdinInspector.Editor.InspectorUtilities.BeginDrawPropertyTree(Sirenix.OdinInspector.Editor.PropertyTree,System.Boolean)" />,
		/// <see cref="M:Sirenix.OdinInspector.Editor.InspectorUtilities.DrawPropertiesInTree(Sirenix.OdinInspector.Editor.PropertyTree)" /> and .
		/// <see cref="M:Sirenix.OdinInspector.Editor.InspectorUtilities.EndDrawPropertyTree(Sirenix.OdinInspector.Editor.PropertyTree)" />.
		/// </para>
		/// </summary>
		public void Draw(bool applyUndo = true)
		{
			BeginDraw(applyUndo);
			DrawProperties();
			EndDraw();
		}

		public void BeginDraw(bool withUndo)
		{
			if (Event.current.type == EventType.Repaint)
			{
				ContextWidth = GUIHelper.ContextWidth;
			}
			GUIHelper.BetterContextWidth = ContextWidth;
			if (frameCounter.Update().IsNewFrame)
			{
				drawnInspectorDepthCount = 0;
			}
			drawnInspectorDepthCount++;
			if (this == null)
			{
				throw new ArgumentNullException("tree");
			}
			if (!IsStatic)
			{
				for (int i = 0; i < WeakTargets.Count; i++)
				{
					if (WeakTargets[i] == null)
					{
						GUILayout.Label("An inspected object has been destroyed; please refresh the inspector.");
						return;
					}
				}
			}
			UpdateTree();
			RecordUndoForChanges = false;
			if (withUndo)
			{
				if (!TargetType.ImplementsOrInherits(typeof(UnityEngine.Object)))
				{
					UnityEngine.Debug.LogError("Automatic inspector undo only works when you're inspecting a type derived from UnityEngine.Object, and you are inspecting '" + TargetType.GetNiceName() + "'.");
				}
				else
				{
					RecordUndoForChanges = true;
				}
			}
			RootProperty.OnStateUpdate(UpdateID);
			if (PrefabModificationHandler.HasNestedOdinPrefabData)
			{
				SirenixEditorGUI.MessageBox("A selected object is serialized by Odin, is a prefab, and contains nested prefab data (IE, more than one possible layer of prefab modifications). This is NOT CURRENTLY SUPPORTED by Odin - therefore, modification of all Odin-serialized values has been disabled for this object.\n\nThere is a strong likelihood that Odin-serialized values will be corrupt and/or wrong in other ways, as well as a very real risk that your computer may spontaneously combust and turn into a flaming wheel of cheese.", MessageType.Error, GlobalConfig<GeneralDrawerConfig>.Instance.MessageBoxFontSize);
			}
			if (!DrawMonoScriptObjectField)
			{
				return;
			}
			if (!monoScriptPropertyHasBeenGotten)
			{
				if (UnitySerializedObject != null)
				{
					monoScriptProperty = GetUnitySerializedObjectNoUpdate().FindProperty("m_Script");
				}
				monoScriptPropertyHasBeenGotten = true;
			}
			if (monoScriptProperty != null)
			{
				GUIHelper.PushGUIEnabled(enabled: false);
				EditorGUILayout.PropertyField(monoScriptProperty);
				GUIHelper.PopGUIEnabled();
			}
		}

		public void DrawProperties()
		{
			InitSearchFilter();
			if (!AllowSearchFiltering || searchFilter == null || !DrawSearch())
			{
				RootProperty.Draw(null);
			}
		}

		/// <summary>
		/// <para>Draws a search bar for the property tree, and draws the search results if the search bar is used.</para>
		/// <para>If this method returns true, the property tree should generally not be drawn normally afterwards.</para>
		/// <para>Note that this method will throw exceptions if the property tree is not set up to be searchable; for that, see <see cref="M:Sirenix.OdinInspector.Editor.PropertyTree.SetSearchable(System.Boolean,Sirenix.OdinInspector.SearchableAttribute)" />.</para>
		/// </summary>
		/// <returns>True if the property tree is being searched and is currently drawing its search results, otherwise false.</returns>
		public bool DrawSearch()
		{
			if (AllowSearchFiltering && searchFilter != null)
			{
				searchFilter.DrawDefaultSearchFieldLayout(null);
				if (searchFilter.HasSearchResults)
				{
					searchFilter.DrawSearchResults();
					return true;
				}
				return false;
			}
			throw new InvalidOperationException("Search is not currently enabled on this PropertyTree. Call SetSearchable(true) first.");
		}

		public void EndDraw()
		{
			InvokeDelayedActions();
			SerializedObject so = GetUnitySerializedObjectNoUpdate();
			if (so != null)
			{
				if (SerializedObject_nativeObjectPtrGetter != null)
				{
					IntPtr ptr = SerializedObject_nativeObjectPtrGetter(ref so);
					if (ptr == IntPtr.Zero)
					{
						return;
					}
				}
				if (RecordUndoForChanges)
				{
					so.ApplyModifiedProperties();
				}
				else
				{
					so.ApplyModifiedPropertiesWithoutUndo();
				}
			}
			bool appliedOdinChanges = false;
			if (ApplyChanges())
			{
				appliedOdinChanges = true;
				GUIHelper.RequestRepaint();
			}
			InvokeDelayedActions();
			if (appliedOdinChanges)
			{
				InvokeOnValidate();
				if (PrefabModificationHandler.HasPrefabs)
				{
					ImmutableList<object> targets = WeakTargets;
					for (int i = 0; i < targets.Count; i++)
					{
						if (!(PrefabModificationHandler.TargetPrefabs[i] == null))
						{
							UnityEngine.Object target = (UnityEngine.Object)targets[i];
							PrefabUtility.RecordPrefabInstancePropertyModifications(target);
						}
					}
				}
			}
			if (RecordUndoForChanges)
			{
				if (appliedOdinChanges && Application.platform == RuntimePlatform.OSXEditor)
				{
					Undo.IncrementCurrentGroup();
					foreach (object target2 in WeakTargets)
					{
						if (target2 is UnityEngine.Object)
						{
							UnityEngine.Object obj = target2 as UnityEngine.Object;
							Undo.RecordObject(obj, "Odin change to " + obj.name);
						}
					}
				}
				Undo.FlushUndoRecordObjects();
			}
			drawnInspectorDepthCount--;
		}

		/// <summary>
		/// Gets a Unity property for the given Odin or Unity path. If there is no <see cref="T:UnityEditor.SerializedObject" /> for this property tree, or no such property is found in the <see cref="T:UnityEditor.SerializedObject" />, a property will be emitted using <see cref="T:Sirenix.OdinInspector.Editor.UnityPropertyEmitter" />.
		/// </summary>
		/// <param name="path">The Odin or Unity path to the property to get.</param>
		public SerializedProperty GetUnityPropertyForPath(string path)
		{
			FieldInfo fieldInfo;
			return GetUnityPropertyForPath(path, out fieldInfo);
		}

		/// <summary>
		/// Gets a Unity property for the given Odin or Unity path. If there is no <see cref="T:UnityEditor.SerializedObject" /> for this property tree, or no such property is found in the <see cref="T:UnityEditor.SerializedObject" />, a property will be emitted using <see cref="T:Sirenix.OdinInspector.Editor.UnityPropertyEmitter" />.
		/// </summary>
		/// <param name="path">The Odin or Unity path to the property to get.</param>
		/// <param name="backingField">The backing field of the Unity property.</param>
		public virtual SerializedProperty GetUnityPropertyForPath(string path, out FieldInfo backingField)
		{
			backingField = null;
			InspectorProperty prop = GetPropertyAtPath(path);
			string unityPath = ((prop != null) ? prop.UnityPropertyPath : InspectorUtilities.ConvertToUnityPropertyPath(path));
			SerializedProperty result = null;
			SerializedObject so = UnitySerializedObject;
			if (so != null)
			{
				result = so.FindProperty(unityPath);
				if (result != null && prop != null)
				{
					backingField = prop.Info.GetMemberInfo() as FieldInfo;
					if (backingField == null && prop.Parent != null && prop.Parent.ChildResolver is ICollectionResolver)
					{
						backingField = prop.Parent.Info.GetMemberInfo() as FieldInfo;
					}
				}
			}
			if (result == null && prop != null && prop.Info.PropertyType == PropertyType.Value)
			{
				if (!emittedUnityPropertyCache.TryGetValue(path, out var innerDict))
				{
					innerDict = new Dictionary<Type, SerializedProperty>(FastTypeComparer.Instance);
					emittedUnityPropertyCache.Add(path, innerDict);
				}
				if (!innerDict.TryGetValue(prop.ValueEntry.TypeOfValue, out result))
				{
					result = UnityPropertyEmitter.CreateEmittedScriptableObjectProperty(prop.Info.PropertyName, prop.ValueEntry.TypeOfValue, WeakTargets.Count);
					innerDict.Add(prop.ValueEntry.TypeOfValue, result);
				}
				else if (result != null && result.serializedObject.targetObject == null)
				{
					result = UnityPropertyEmitter.CreateEmittedScriptableObjectProperty(prop.Info.PropertyName, prop.ValueEntry.TypeOfValue, WeakTargets.Count);
					innerDict[prop.ValueEntry.TypeOfValue] = result;
				}
				result?.serializedObject.Update();
			}
			return result;
		}

		/// <summary>
		/// Checks whether a given object instance is referenced anywhere in the tree, and if it is, gives the path of the first time the object reference was encountered as an out parameter.
		/// </summary>
		/// <param name="value">The reference value to check.</param>
		/// <param name="referencePath">The first found path of the object.</param>
		public abstract bool ObjectIsReferenced(object value, out string referencePath);

		/// <summary>
		/// Gets the number of references to a given object instance in this tree.
		/// </summary>
		public abstract int GetReferenceCount(object reference);

		/// <summary>
		/// Updates all properties in the entire tree, and validates the prefab state of the tree, if applicable.
		/// </summary>
		public abstract void UpdateTree();

		/// <summary>
		/// Replaces all occurrences of a value with another value, in the entire tree.
		/// </summary>
		/// <param name="from">The value to find all instances of.</param>
		/// <param name="to">The value to replace the found values with.</param>
		public abstract void ReplaceAllReferences(object from, object to);

		/// <summary>
		/// Gets the root tree property at a given index.
		/// </summary>
		/// <param name="index">The index of the property to get.</param>
		public abstract InspectorProperty GetRootProperty(int index);

		/// <summary>
		/// Invokes the actions that have been delayed using <see cref="M:Sirenix.OdinInspector.Editor.PropertyTree.DelayAction(System.Action)" /> and <see cref="M:Sirenix.OdinInspector.Editor.PropertyTree.DelayActionUntilRepaint(System.Action)" />.
		/// </summary>
		public abstract void InvokeDelayedActions();

		/// <summary>
		/// Applies all changes made with properties to the inspected target tree values, and marks all changed Unity objects dirty.
		/// </summary>
		/// <returns>true if any values were changed, otherwise false</returns>
		public abstract bool ApplyChanges();

		internal abstract SerializedObject GetUnitySerializedObjectNoUpdate();

		/// <summary>
		/// Invokes the OnValidate method on the property tree's targets if they are derived from <see cref="T:UnityEngine.Object" /> and have the method defined.
		/// </summary>
		public void InvokeOnValidate()
		{
			if (!(onValidateMethod != null))
			{
				return;
			}
			for (int i = 0; i < WeakTargets.Count; i++)
			{
				try
				{
					onValidateMethod.Invoke(WeakTargets[i], null);
				}
				catch (Exception exception)
				{
					UnityEngine.Debug.LogException(exception);
				}
			}
		}

		/// <summary>
		/// Registers an object reference to a given path; this is used to ensure that objects are always registered after having been encountered once.
		/// </summary>
		/// <param name="reference">The referenced object.</param>
		/// <param name="property">The property that contains the reference.</param>
		internal abstract void ForceRegisterObjectReference(object reference, InspectorProperty property);

		/// <summary>
		/// Creates a PropertyTree to inspect the static values of the given type.
		/// </summary>
		/// <param name="type">The type to inspect.</param>
		/// <returns>A PropertyTree instance for inspecting the type.</returns>
		public static PropertyTree CreateStatic(Type type)
		{
			if (type == null)
			{
				throw new ArgumentNullException("type");
			}
			return ((PropertyTree)Activator.CreateInstance(typeof(PropertyTree<>).MakeGenericType(type))).SetUpForIMGUIDrawing();
		}

		/// <summary>
		/// Creates a new <see cref="T:Sirenix.OdinInspector.Editor.PropertyTree" /> for a given target value.
		/// </summary>
		/// <param name="target">The target to create a tree for.</param>
		/// <exception cref="T:System.ArgumentNullException">target is null</exception>
		public static PropertyTree Create(object target)
		{
			if (target == null)
			{
				throw new ArgumentNullException("target");
			}
			return Create(new object[1] { target }, null, null);
		}

		/// <summary>
		/// Creates a new <see cref="T:Sirenix.OdinInspector.Editor.PropertyTree" /> for a given target value.
		/// </summary>
		/// <param name="target">The target to create a tree for.</param>
		/// <param name="backend">The serialization backend to use for the tree root.</param>
		/// <exception cref="T:System.ArgumentNullException">target is null</exception>
		public static PropertyTree Create(object target, SerializationBackend backend)
		{
			if (target == null)
			{
				throw new ArgumentNullException("target");
			}
			return Create(new object[1] { target }, null, backend);
		}

		/// <summary>
		/// <para>Creates a new <see cref="T:Sirenix.OdinInspector.Editor.PropertyTree" /> for a set of given target values.</para>
		/// <para>Note that the targets all need to be of the same type.</para>
		/// </summary>
		/// <param name="targets">The targets to create a tree for.</param>
		/// <exception cref="T:System.ArgumentNullException">targets is null</exception>
		public static PropertyTree Create(params object[] targets)
		{
			if (targets == null)
			{
				throw new ArgumentNullException("targets");
			}
			return Create((IList)targets);
		}

		/// <summary>
		/// Creates a new <see cref="T:Sirenix.OdinInspector.Editor.PropertyTree" /> for all target values of a <see cref="T:UnityEditor.SerializedObject" />.
		/// </summary>
		/// <param name="serializedObject">The serialized object to create a tree for.</param>
		/// <exception cref="T:System.ArgumentNullException">serializedObject is null</exception>
		public static PropertyTree Create(SerializedObject serializedObject)
		{
			if (serializedObject == null)
			{
				throw new ArgumentNullException("serializedObject");
			}
			return Create(serializedObject.targetObjects, serializedObject);
		}

		/// <summary>
		/// Creates a new <see cref="T:Sirenix.OdinInspector.Editor.PropertyTree" /> for all target values of a <see cref="T:UnityEditor.SerializedObject" />.
		/// </summary>
		/// <param name="serializedObject">The serialized object to create a tree for.</param>
		/// <exception cref="T:System.ArgumentNullException">serializedObject is null</exception>
		/// <param name="backend">The serialization backend to use for the tree root.</param>
		public static PropertyTree Create(SerializedObject serializedObject, SerializationBackend backend)
		{
			if (serializedObject == null)
			{
				throw new ArgumentNullException("serializedObject");
			}
			return Create(serializedObject.targetObjects, serializedObject, backend);
		}

		/// <summary>
		/// <para>Creates a new <see cref="T:Sirenix.OdinInspector.Editor.PropertyTree" /> for a set of given target values.</para>
		/// <para>Note that the targets all need to be of the same type.</para>
		/// </summary>
		/// <param name="targets">The targets to create a tree for.</param>
		public static PropertyTree Create(IList targets)
		{
			return Create(targets, null, null);
		}

		/// <summary>
		/// <para>Creates a new <see cref="T:Sirenix.OdinInspector.Editor.PropertyTree" /> for a set of given target values.</para>
		/// <para>Note that the targets all need to be of the same type.</para>
		/// </summary>
		/// <param name="targets">The targets to create a tree for.</param>
		/// <param name="backend">The serialization backend to use for the tree root.</param>
		public static PropertyTree Create(IList targets, SerializationBackend backend)
		{
			return Create(targets, null, backend);
		}

		/// <summary>
		/// <para>Creates a new <see cref="T:Sirenix.OdinInspector.Editor.PropertyTree" /> for a set of given target values, represented by a given <see cref="T:UnityEditor.SerializedObject" />.</para>
		/// <para>Note that the targets all need to be of the same type.</para>
		/// </summary>
		/// <param name="targets">The targets to create a tree for.</param>
		/// <param name="serializedObject">The serialized object to create a tree for. Note that the target values of the given <see cref="T:UnityEditor.SerializedObject" /> must be the same values given in the targets parameter.</param>
		public static PropertyTree Create(IList targets, SerializedObject serializedObject)
		{
			return Create(targets, serializedObject, null);
		}

		/// <summary>
		/// <para>Creates a new <see cref="T:Sirenix.OdinInspector.Editor.PropertyTree" /> for a set of given target values, represented by a given <see cref="T:UnityEditor.SerializedObject" />.</para>
		/// <para>Note that the targets all need to be of the same type.</para>
		/// </summary>
		/// <param name="targets">The targets to create a tree for.</param>
		/// <param name="serializedObject">The serialized object to create a tree for. Note that the target values of the given <see cref="T:UnityEditor.SerializedObject" /> must be the same values given in the targets parameter.</param>
		/// <param name="backend">The serialization backend to use for the tree root.</param>
		public static PropertyTree Create(IList targets, SerializedObject serializedObject, SerializationBackend backend)
		{
			if (targets == null)
			{
				throw new ArgumentNullException("targets");
			}
			if (targets.Count == 0)
			{
				throw new ArgumentException("There must be at least one target.");
			}
			if (serializedObject != null)
			{
				bool valid = true;
				UnityEngine.Object[] targetObjects = serializedObject.targetObjects;
				if (targets.Count != targetObjects.Length)
				{
					valid = false;
				}
				else
				{
					for (int i = 0; i < targets.Count; i++)
					{
						if (targets[i] != targetObjects[i])
						{
							valid = false;
							break;
						}
					}
				}
				if (!valid)
				{
					throw new ArgumentException("Given target array must be identical in length and content to the target objects array in the given serializedObject.");
				}
			}
			Type targetType = null;
			for (int j = 0; j < targets.Count; j++)
			{
				object target = targets[j];
				if (target == null)
				{
					throw new ArgumentException("Target at index " + j + " was null.");
				}
				Type otherType;
				if (j == 0)
				{
					targetType = target.GetType();
				}
				else if (targetType != (otherType = target.GetType()) && !targetType.IsAssignableFrom(otherType))
				{
					if (!otherType.IsAssignableFrom(targetType))
					{
						throw new ArgumentException("Expected targets of type " + targetType.Name + ", but got an incompatible target of type " + otherType.Name + " at index " + j + ".");
					}
					targetType = otherType;
				}
			}
			Type treeType = typeof(PropertyTree<>).MakeGenericType(targetType);
			Array targetArray;
			if (targets.GetType().IsArray && targets.GetType().GetElementType() == targetType)
			{
				targetArray = (Array)targets;
			}
			else
			{
				targetArray = Array.CreateInstance(targetType, targets.Count);
				targets.CopyTo(targetArray, 0);
			}
			if (serializedObject == null && typeof(UnityEngine.Object).IsAssignableFrom(targetType))
			{
				UnityEngine.Object[] objs = new UnityEngine.Object[targets.Count];
				targets.CopyTo(objs, 0);
				serializedObject = new SerializedObject(objs);
			}
			return ((PropertyTree)Activator.CreateInstance(treeType, targetArray, serializedObject, backend)).SetUpForIMGUIDrawing();
		}

		internal static PropertyTree Create(Type type)
		{
			if (type == null)
			{
				throw new ArgumentNullException("type");
			}
			PropertyTree instance = (PropertyTree)Activator.CreateInstance(typeof(PropertyTree<>).MakeGenericType(type));
			instance = instance.SetupForDesignerEditor();
			instance.IsStatic = false;
			return instance;
		}

		private void InvokeOnUndoRedoPerformed()
		{
			if (this.OnUndoRedoPerformed != null)
			{
				this.OnUndoRedoPerformed();
			}
		}

		protected void InitSearchFilter()
		{
			if (!hasSetupSearchFilter)
			{
				SearchableAttribute searchAttr = RootProperty.GetAttribute<SearchableAttribute>();
				if (searchAttr != null)
				{
					searchFilter = new PropertySearchFilter(RootProperty, searchAttr);
				}
				else
				{
					searchFilter = null;
				}
				hasSetupSearchFilter = true;
			}
		}

		protected abstract void DisposeAndResetRootProperty();

		/// <summary>
		/// <para>Sets whether the property tree should be searchable or not, and allows the passing in of a custom SearchableAttribute instance to configure the search.</para>
		/// </summary>
		/// <param name="searchable">Whether the tree should be set to be searchable or not.</param>
		/// <param name="config">If the tree is set to be searchable, then if this parameter is not null, it will be used to configure the property tree search. If the parameter is null, the SearchableAttribute on the tree's <see cref="P:Sirenix.OdinInspector.Editor.PropertyTree.RootProperty" /> will be used. If that property has no such attribute, then default search settings will be applied.</param>
		public void SetSearchable(bool searchable, SearchableAttribute config = null)
		{
			AllowSearchFiltering = searchable;
			if (searchable)
			{
				searchFilter = new PropertySearchFilter(RootProperty, config ?? RootProperty.GetAttribute<SearchableAttribute>() ?? new SearchableAttribute());
			}
			else
			{
				searchFilter = null;
			}
		}

		protected virtual void Dispose(bool finalizer)
		{
			if (disposedValue)
			{
				return;
			}
			if (finalizer)
			{
				if (allocationTrace != null)
				{
					UnityEngine.Debug.LogWarning("An Odin PropertyTree instance is being garbage collected without first having been disposed. PropertyTree instances must be disposed once they are no longer needed. This instance was allocated at the following location: \n\n" + allocationTrace.ToString());
				}
				UnityEditorEventUtility.DelayActionThreadSafe(ActuallyDispose);
			}
			else
			{
				ActuallyDispose();
			}
		}

		~PropertyTree()
		{
			Dispose(finalizer: true);
		}

		public void Dispose()
		{
			Dispose(finalizer: false);
		}

		private void ActuallyDispose()
		{
			ApplyChanges();
			if (HasRootPropertyYet)
			{
				RootProperty.Dispose();
			}
			UndoEventListener.DesubscribeListener(this);
			if (drawerChainResolver is IDisposable)
			{
				(drawerChainResolver as IDisposable).Dispose();
			}
			if (attributeProcessorLocator is IDisposable)
			{
				(attributeProcessorLocator as IDisposable).Dispose();
			}
			if (propertyResolverLocator is IDisposable)
			{
				(propertyResolverLocator as IDisposable).Dispose();
			}
			this.OnUndoRedoPerformed = null;
			this.OnPropertyValueChanged = null;
			DisposeInheritedStuff();
			disposedValue = true;
		}

		protected abstract void DisposeInheritedStuff();

		public PropertyTree SetUpForIMGUIDrawing()
		{
			TreeIsSetupForIMGUIDrawing_TEMP_INTERNAL = true;
			ComponentProviders.Clear();
			ComponentProviders.Add(new ValidationComponentProvider(new DefaultValidatorLocator
			{
				CustomValidatorFilter = (Type type) => !type.IsDefined<NoValidationInInspectorAttribute>(inherit: true)
			}));
			if (HasRootPropertyYet)
			{
				RootProperty.RefreshSetup();
			}
			return this;
		}

		public PropertyTree SetUpForValidation()
		{
			TreeIsSetupForIMGUIDrawing_TEMP_INTERNAL = false;
			ComponentProviders.Clear();
			ComponentProviders.Add(new ValidationComponentProvider());
			if (HasRootPropertyYet)
			{
				RootProperty.RefreshSetup();
			}
			return this;
		}

		internal PropertyTree SetupForDesignerEditor()
		{
			TreeIsSetupForIMGUIDrawing_TEMP_INTERNAL = false;
			IsMadeForDesignerEditor = true;
			ComponentProviders.Clear();
			if (HasRootPropertyYet)
			{
				RootProperty.RefreshSetup();
			}
			return this;
		}
	}
	/// <summary>
	/// <para>Represents a set of strongly typed values as a tree of properties that can be drawn in the inspector, and provides an array of utilities for querying the tree of properties.</para>
	/// <para>This class also handles management of prefab modifications.</para>
	/// </summary>
	public sealed class PropertyTree<T> : PropertyTree
	{
		private static readonly bool TargetIsValueType = typeof(T).IsValueType;

		private static readonly bool TargetIsUnityObject = typeof(UnityEngine.Object).IsAssignableFrom(typeof(T));

		private Dictionary<object, int> objectReferenceCounts = new Dictionary<object, int>(ReferenceEqualityComparer<object>.Default);

		private Dictionary<object, string> objectReferences = new Dictionary<object, string>(ReferenceEqualityComparer<object>.Default);

		private List<Action> delayedActions = new List<Action>();

		private List<Action> delayedRepaintActions = new List<Action>();

		private List<InspectorProperty> dirtyProperties = new List<InspectorProperty>();

		private T[] targets;

		private InspectorProperty rootProperty;

		private SerializedObject serializedObject;

		private int serializedObjectUpdateID;

		private int updateID = 1;

		private object[] weakTargets;

		private ImmutableList<T> immutableTargets;

		private ImmutableList<object> immutableWeakTargets;

		private bool includesSpeciallySerializedMembers;

		private PrefabModificationHandler prefabModificationHandler;

		private int prefabModificationHandler_lastUpdateID;

		private static readonly bool includesSpeciallySerializedMembers_StaticCache = InspectorPropertyInfoUtility.TypeDefinesShowOdinSerializedPropertiesInInspectorAttribute_Cached(typeof(T));

		protected override bool HasRootPropertyYet => rootProperty != null;

		/// <summary>
		/// Gets the root property of the tree.
		/// </summary>
		public override InspectorProperty RootProperty
		{
			get
			{
				if (rootProperty == null)
				{
					rootProperty = InspectorProperty.Create(this, null, InspectorPropertyInfo.CreateValue("$ROOT", 0f, base.SerializationBackend, (IValueGetterSetter)new GetterSetter<int, T>(delegate(ref int index)
					{
						return targets[index];
					}, delegate(ref int index, T value)
					{
						targets[index] = value;
					}), (Attribute[])null), 0, isRoot: true);
					rootProperty.Update(forceUpdate: true);
				}
				return rootProperty;
			}
		}

		/// <summary>
		/// Gets the secret root property of the PropertyTree.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("Use RootProperty instead; the root is no longer considered 'secret'.", false)]
		public override InspectorProperty SecretRootProperty => RootProperty;

		/// <summary>
		/// Gets the <see cref="F:Sirenix.OdinInspector.Editor.PropertyTree`1.prefabModificationHandler" /> for the PropertyTree.
		/// </summary>
		public override PrefabModificationHandler PrefabModificationHandler
		{
			get
			{
				if (prefabModificationHandler == null)
				{
					prefabModificationHandler = new PrefabModificationHandler(this);
				}
				if (TargetIsUnityObject && prefabModificationHandler_lastUpdateID != updateID)
				{
					prefabModificationHandler.Update();
					prefabModificationHandler_lastUpdateID = updateID;
				}
				return prefabModificationHandler;
			}
		}

		/// <summary>
		/// The current update ID of the tree. This is incremented once, each update, and is used by <see cref="M:Sirenix.OdinInspector.Editor.InspectorProperty.Update(System.Boolean)" /> to avoid updating multiple times in the same update round.
		/// </summary>
		public override int UpdateID => updateID;

		/// <summary>
		/// The <see cref="T:UnityEditor.SerializedObject" /> that this tree represents, if the tree was created for a <see cref="T:UnityEditor.SerializedObject" />.
		/// </summary>
		public override SerializedObject UnitySerializedObject
		{
			get
			{
				if (serializedObject != null && serializedObjectUpdateID != updateID)
				{
					serializedObjectUpdateID = updateID;
					serializedObject.Update();
				}
				return serializedObject;
			}
		}

		/// <summary>
		/// The type of the values that the property tree represents.
		/// </summary>
		public override Type TargetType => typeof(T);

		/// <summary>
		/// The strongly types actual values that the property tree represents.
		/// </summary>
		public ImmutableList<T> Targets
		{
			get
			{
				if (immutableTargets == null)
				{
					immutableTargets = new ImmutableList<T>(targets);
				}
				return immutableTargets;
			}
		}

		/// <summary>
		/// The weakly types actual values that the property tree represents.
		/// </summary>
		public override ImmutableList<object> WeakTargets
		{
			get
			{
				if (immutableWeakTargets == null)
				{
					if (weakTargets == null)
					{
						weakTargets = new object[targets.Length];
						targets.CopyTo(weakTargets, 0);
					}
					immutableWeakTargets = new ImmutableList<object>(weakTargets);
				}
				else if (TargetIsValueType)
				{
					targets.CopyTo(weakTargets, 0);
				}
				return immutableWeakTargets;
			}
		}

		/// <summary>
		/// The number of root properties in the tree.
		/// </summary>
		public override int RootPropertyCount => RootProperty.Children.Count;

		/// <summary>
		/// Whether this property tree also represents members that are specially serialized by Odin.
		/// </summary>
		[Obsolete("This value is no longer guaranteed to be correct, as it may have different answers for different properties in the tree. Instead look at InspectorProperty.SerializationRoot to determine whether specially serialized members might be included.", true)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public override bool IncludesSpeciallySerializedMembers
		{
			get
			{
				throw new NotSupportedException();
			}
		}

		internal override SerializedObject GetUnitySerializedObjectNoUpdate()
		{
			return serializedObject;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:Sirenix.OdinInspector.Editor.PropertyTree`1" /> class, inspecting only the target (<see cref="!:T" />) type's static members.
		/// </summary>
		public PropertyTree()
		{
			base.IsStatic = true;
			targets = new T[1];
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:Sirenix.OdinInspector.Editor.PropertyTree`1" /> class.
		/// </summary>
		/// <param name="serializedObject">The serialized object to represent.</param>
		public PropertyTree(SerializedObject serializedObject)
			: this(serializedObject.targetObjects.Cast<T>().ToArray(), serializedObject)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:Sirenix.OdinInspector.Editor.PropertyTree`1" /> class.
		/// </summary>
		/// <param name="targets">The targets to represent.</param>
		public PropertyTree(T[] targets)
			: this(targets, (SerializedObject)null)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:Sirenix.OdinInspector.Editor.PropertyTree`1" /> class.
		/// </summary>
		/// <param name="targets">The targets to represent.</param>
		/// <param name="serializedObject">The serialized object to represent. Note that the target values of the given <see cref="T:UnityEditor.SerializedObject" /> must be the same values given in the targets parameter.</param>
		/// <exception cref="T:System.ArgumentNullException">targets is null</exception>
		/// <exception cref="T:System.ArgumentException">
		/// There must be at least one target.
		/// or
		/// A given target is a null value.
		/// </exception>
		public PropertyTree(T[] targets, SerializedObject serializedObject)
			: this(targets, serializedObject, (SerializationBackend)null)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:Sirenix.OdinInspector.Editor.PropertyTree`1" /> class.
		/// </summary>
		/// <param name="targets">The targets to represent.</param>
		/// <param name="serializedObject">The serialized object to represent. Note that the target values of the given <see cref="T:UnityEditor.SerializedObject" /> must be the same values given in the targets parameter.</param>
		/// <param name="backend">The serialization backend to use for the tree root.</param>
		/// <exception cref="T:System.ArgumentNullException">targets is null</exception>
		/// <exception cref="T:System.ArgumentException">
		/// There must be at least one target.
		/// or
		/// A given target is a null value.
		/// </exception>
		public PropertyTree(T[] targets, SerializedObject serializedObject, SerializationBackend backend = null)
		{
			try
			{
				if (targets == null)
				{
					throw new ArgumentNullException("targets");
				}
				if (targets.Length == 0)
				{
					throw new ArgumentException("There must be at least one target.");
				}
				for (int i = 0; i < targets.Length; i++)
				{
					if (targets[i] == null)
					{
						throw new ArgumentException("A target at index '" + i + "' is a null value.");
					}
				}
				includesSpeciallySerializedMembers = includesSpeciallySerializedMembers_StaticCache;
				this.serializedObject = serializedObject;
				this.targets = targets;
				if (backend != null)
				{
					base.SerializationBackend = backend;
				}
			}
			catch (Exception)
			{
				Dispose();
				throw;
			}
		}

		/// <summary>
		/// Applies all changes made with properties to the inspected target tree values.
		/// </summary>
		/// <returns>
		/// true if any values were changed, otherwise false
		/// </returns>
		public override bool ApplyChanges()
		{
			bool changed = false;
			for (int i = 0; i < dirtyProperties.Count; i++)
			{
				InspectorProperty property = dirtyProperties[i];
				if (property.ChildResolver is IApplyableResolver resolver && resolver.ApplyChanges())
				{
					changed = true;
					if (property.BaseValueEntry != null)
					{
						for (int j = 0; j < property.BaseValueEntry.ValueCount; j++)
						{
							property.BaseValueEntry.TriggerOnValueChanged(j);
						}
						if (property.BaseValueEntry.ValueChangedFromPrefab)
						{
							for (int k = 0; k < Targets.Count; k++)
							{
								PrefabModificationHandler.RegisterPrefabValueModification(property, k);
							}
						}
					}
				}
				if (property.ValueEntry != null && property.ValueEntry.ApplyChanges())
				{
					changed = true;
				}
				if (!changed)
				{
					continue;
				}
				InspectorProperty serializationRoot = property.SerializationRoot;
				for (int l = 0; l < serializationRoot.ValueEntry.ValueCount; l++)
				{
					UnityEngine.Object unityObj = serializationRoot.ValueEntry.WeakValues[l] as UnityEngine.Object;
					if (unityObj != null)
					{
						InspectorUtilities.RegisterUnityObjectDirty(unityObj);
					}
				}
			}
			dirtyProperties.Clear();
			if (changed && PrefabModificationHandler != null && PrefabModificationHandler.HasPrefabs && UnitySerializedObject != null)
			{
				DelayActionUntilRepaint(delegate
				{
					DelayActionUntilRepaint(delegate
					{
						for (int m = 0; m < WeakTargets.Count; m++)
						{
							if (WeakTargets[m] is ISerializationCallbackReceiver serializationCallbackReceiver)
							{
								serializationCallbackReceiver.OnBeforeSerialize();
							}
							PrefabUtility.RecordPrefabInstancePropertyModifications((UnityEngine.Object)WeakTargets[m]);
						}
					});
				});
			}
			return changed;
		}

		/// <summary>
		/// Registers that a given property is dirty and needs its changes to be applied at the end of the current frame.
		/// </summary>
		/// <param name="property"></param>
		/// <exception cref="T:System.NotImplementedException"></exception>
		public override void RegisterPropertyDirty(InspectorProperty property)
		{
			dirtyProperties.Add(property);
		}

		/// <summary>
		/// Updates all properties in the entire tree, and validates the prefab state of the tree, if applicable.
		/// </summary>
		public override void UpdateTree()
		{
			ApplyChanges();
			updateID++;
			objectReferences.Clear();
			objectReferenceCounts.Clear();
			RootProperty.Update();
		}

		/// <summary>
		/// Checks whether a given object instance is referenced anywhere in the tree, and if it is, gives the path of the first time the object reference was encountered as an out parameter.
		/// </summary>
		/// <param name="value">The reference value to check.</param>
		/// <param name="referencePath">The first found path of the object.</param>
		public override bool ObjectIsReferenced(object value, out string referencePath)
		{
			if (value is UnityEngine.Object)
			{
				referencePath = null;
				return false;
			}
			return objectReferences.TryGetValue(value, out referencePath);
		}

		/// <summary>
		/// Gets the number of references to a given object instance in this tree.
		/// </summary>
		/// <param name="reference"></param>
		public override int GetReferenceCount(object reference)
		{
			objectReferenceCounts.TryGetValue(reference, out var count);
			return count;
		}

		private InspectorProperty TryFindChildMemberPropertyWithNameFromGroups(StringSlice name, InspectorProperty property)
		{
			if (property.ChildResolver is ICollectionResolver)
			{
				return null;
			}
			for (int i = 0; i < property.Children.Count; i++)
			{
				InspectorProperty child = property.Children.Get(i);
				switch (child.Info.PropertyType)
				{
				case PropertyType.Value:
					if (child.Info.HasSingleBackingMember && child.Name == name)
					{
						return child;
					}
					break;
				case PropertyType.Group:
				{
					InspectorProperty found = TryFindChildMemberPropertyWithNameFromGroups(name, child);
					if (found != null)
					{
						return found;
					}
					break;
				}
				default:
					throw new NotImplementedException(child.Info.PropertyType.ToString());
				case PropertyType.Method:
					break;
				}
			}
			return null;
		}

		/// <summary>
		/// Enumerates over the properties of the tree. WARNING: For tree that have large targets with lots of data, this may involve massive amounts of work as the full tree structure is resolved. USE THIS METHOD SPARINGLY AND ONLY WHEN ABSOLUTELY NECESSARY!
		/// </summary>
		/// <param name="includeChildren">Whether to include children of the root properties or not. If set to true, every property in the entire tree will be enumerated.</param>
		/// /// <param name="onlyVisible">Whether to only include visible properties. Properties whose parents are invisible are considered invisible.</param>
		public override IEnumerable<InspectorProperty> EnumerateTree(bool includeChildren = true, bool onlyVisible = false)
		{
			if (includeChildren)
			{
				if (RootProperty.Children.Count == 0)
				{
					yield break;
				}
				for (InspectorProperty current = RootProperty.Children.Get(0); current != null; current = current.NextProperty(includeChildren: true, onlyVisible))
				{
					if (!onlyVisible || current.State.Visible)
					{
						yield return current;
					}
				}
				yield break;
			}
			for (int i = 0; i < RootProperty.Children.Count; i++)
			{
				InspectorProperty child = RootProperty.Children.Get(i);
				if (!onlyVisible || child.State.Visible)
				{
					yield return RootProperty.Children.Get(i);
				}
			}
		}

		/// <summary>
		/// Replaces all occurrences of a value with another value, in the entire tree.
		/// </summary>
		/// <param name="from">The value to find all instances of.</param>
		/// <param name="to">The value to replace the found values with.</param>
		/// <exception cref="T:System.ArgumentNullException"></exception>
		/// <exception cref="T:System.ArgumentException">The value to replace with must either be null or be the same type as the value to replace (" + from.GetType().Name + ").</exception>
		public override void ReplaceAllReferences(object from, object to)
		{
			if (from == null)
			{
				throw new ArgumentNullException();
			}
			if (to != null && from.GetType() != to.GetType())
			{
				throw new ArgumentException("The value to replace with must either be null or be the same type as the value to replace (" + from.GetType().Name + ").");
			}
			foreach (InspectorProperty prop in EnumerateTree())
			{
				if (prop.Info.PropertyType != PropertyType.Value || prop.Info.TypeOfValue.IsValueType)
				{
					continue;
				}
				IPropertyValueEntry valueEntry = prop.ValueEntry;
				for (int i = 0; i < valueEntry.ValueCount; i++)
				{
					object obj = valueEntry.WeakValues[i];
					if (from == obj)
					{
						valueEntry.WeakValues[i] = to;
					}
				}
			}
		}

		internal override void ForceRegisterObjectReference(object reference, InspectorProperty property)
		{
			objectReferences[reference] = property.Path;
		}

		/// <summary>
		/// Gets the root tree property at a given index.
		/// </summary>
		/// <param name="index">The index of the property to get.</param>
		public override InspectorProperty GetRootProperty(int index)
		{
			return RootProperty.Children.Get(index);
		}

		/// <summary>
		/// Schedules a delegate to be invoked at the end of the current GUI frame.
		/// </summary>
		/// <param name="action">The action delegate to be delayed.</param>
		/// <exception cref="T:System.ArgumentNullException">action</exception>
		public override void DelayAction(Action action)
		{
			if (action == null)
			{
				throw new ArgumentNullException("action");
			}
			delayedActions.Add(action);
		}

		/// <summary>
		/// Schedules a delegate to be invoked at the end of the next Repaint GUI frame.
		/// </summary>
		/// <param name="action">The action to be delayed.</param>
		/// <exception cref="T:System.ArgumentNullException">action</exception>
		public override void DelayActionUntilRepaint(Action action)
		{
			if (action == null)
			{
				throw new ArgumentNullException("action");
			}
			delayedRepaintActions.Add(action);
			GUIHelper.RequestRepaint();
		}

		/// <summary>
		/// Invokes the actions that have been delayed using <see cref="M:Sirenix.OdinInspector.Editor.PropertyTree`1.DelayAction(System.Action)" /> and <see cref="M:Sirenix.OdinInspector.Editor.PropertyTree`1.DelayActionUntilRepaint(System.Action)" />.
		/// </summary>
		public override void InvokeDelayedActions()
		{
			for (int i = 0; i < delayedActions.Count; i++)
			{
				try
				{
					delayedActions[i]();
				}
				catch (Exception exception)
				{
					UnityEngine.Debug.LogException(exception);
				}
			}
			delayedActions.Clear();
			if ((Event.current == null || Event.current.type != EventType.Repaint) && TreeIsSetupForIMGUIDrawing_TEMP_INTERNAL)
			{
				return;
			}
			for (int j = 0; j < delayedRepaintActions.Count; j++)
			{
				try
				{
					delayedRepaintActions[j]();
				}
				catch (Exception exception2)
				{
					UnityEngine.Debug.LogException(exception2);
				}
			}
			delayedRepaintActions.Clear();
		}

		public override void CleanForCachedReuse()
		{
			PropertyChildren rootChild = RootProperty.Children;
			PropertyChildren.ExistingChildEnumerator enumerator = rootChild.GetExistingChildren().GetEnumerator();
			while (enumerator.MoveNext())
			{
				InspectorProperty child = enumerator.Current;
				child.CleanForCachedReuse();
			}
			delayedActions.Clear();
			delayedRepaintActions.Clear();
			if (prefabModificationHandler != null)
			{
				prefabModificationHandler.CleanForCachedReuse();
			}
			updateID++;
		}

		public override void SetTargets(params object[] newTargets)
		{
			serializedObject = null;
			monoScriptProperty = null;
			monoScriptPropertyHasBeenGotten = false;
			if (targets.Length != newTargets.Length)
			{
				throw new ArgumentException("Target count of tree cannot be changed");
			}
			for (int i = 0; i < targets.Length; i++)
			{
				T target = (T)newTargets[i];
				if (target == null)
				{
					throw new NullReferenceException("Tree target cannot be null");
				}
				targets[i] = target;
			}
			targets.CopyTo(weakTargets, 0);
			UpdateTree();
		}

		public override void SetSerializedObject(SerializedObject serializedObject)
		{
			this.serializedObject = serializedObject;
			monoScriptProperty = null;
			monoScriptPropertyHasBeenGotten = false;
			UnityEngine.Object[] newTargets = serializedObject.targetObjects;
			if (targets.Length != newTargets.Length)
			{
				throw new ArgumentException("Target count of tree cannot be changed");
			}
			for (int i = 0; i < targets.Length; i++)
			{
				T target = (T)(object)newTargets[i];
				if (target == null)
				{
					throw new NullReferenceException("Tree target cannot be null");
				}
				targets[i] = target;
			}
			targets.CopyTo(weakTargets, 0);
			UpdateTree();
		}

		protected override void DisposeInheritedStuff()
		{
			if (prefabModificationHandler != null)
			{
				prefabModificationHandler.Dispose();
				prefabModificationHandler = null;
			}
		}

		protected override void DisposeAndResetRootProperty()
		{
			if (rootProperty != null)
			{
				rootProperty.Dispose();
				rootProperty = null;
			}
		}
	}
}
