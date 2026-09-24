using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using Sirenix.OdinInspector.Editor.Drawers;
using Sirenix.OdinInspector.Editor.Internal;
using Sirenix.OdinInspector.Editor.Validation;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor
{
	/// <summary>
	/// Represents a property in the inspector, and provides the hub for all functionality related to that property.
	/// </summary>
	public sealed class InspectorProperty : IDisposable
	{
		private int maxDrawCount;

		private Stack<int> drawCountStack = new Stack<int>();

		private int lastUpdatedTreeID = -1;

		private string unityPropertyPath;

		private string prefabModificationPath;

		private List<int> drawerChainIndices = new List<int>();

		private List<BakedDrawerChain> drawerChains;

		internal readonly List<Attribute> processedAttributes = new List<Attribute>();

		private ImmutableList<Attribute> processedAttributesImmutable;

		private bool? supportsPrefabModifications;

		private List<PropertyComponent> components = new List<PropertyComponent>();

		private ImmutableList<PropertyComponent> componentsImmutable;

		private List<PropertyState> states;

		private List<Rect> lastDrawnValueRects = new List<Rect>();

		private int lastUpdatedStateUpdatersID = -1;

		private StateUpdater[] stateUpdaters;

		public bool AnimateVisibility = true;

		internal bool IsDesigned;

		public bool IsTreeRoot => this == Tree.RootProperty;

		/// <summary>
		/// Gets the property which is the ultimate root of this property's serialization.
		/// </summary>
		public InspectorProperty SerializationRoot { get; private set; }

		/// <summary>
		/// The name of the property.
		/// </summary>
		public string Name { get; private set; }

		/// <summary>
		/// The nice name of the property, usually as converted by <see cref="M:UnityEditor.ObjectNames.NicifyVariableName(System.String)" />.
		/// </summary>
		public string NiceName { get; private set; }

		/// <summary>
		/// The cached label of the property, usually containing <see cref="P:Sirenix.OdinInspector.Editor.InspectorProperty.NiceName" />.
		/// </summary>
		public GUIContent Label { get; set; }

		/// <summary>
		/// The full Odin path of the property. To get the Unity property path, see <see cref="P:Sirenix.OdinInspector.Editor.InspectorProperty.UnityPropertyPath" />.
		/// </summary>
		public string Path { get; private set; }

		/// <summary>
		/// The child index of this property.
		/// </summary>
		public int Index { get; private set; }

		/// <summary>
		/// Gets the resolver for this property's children.
		/// </summary>
		public OdinPropertyResolver ChildResolver { get; private set; }

		/// <summary>
		/// <para>The current recursive draw depth, incremented for each time that the property has caused itself to be drawn recursively.</para>
		/// <para>Note that this is the <i>current</i> recursion level, not the total amount of recursions so far this frame.</para>
		/// </summary>
		public int RecursiveDrawDepth => drawCountStack.Count;

		/// <summary>
		/// The amount of times that the property has been drawn so far this frame.
		/// </summary>
		public int DrawCount
		{
			get
			{
				if (drawCountStack.Count == 0)
				{
					return maxDrawCount;
				}
				return drawCountStack.Peek();
			}
		}

		/// <summary>
		/// How deep in the drawer chain the property currently is, in the current drawing session as determined by <see cref="P:Sirenix.OdinInspector.Editor.InspectorProperty.DrawCount" />.
		/// </summary>
		public int DrawerChainIndex
		{
			get
			{
				while (drawerChainIndices.Count <= DrawCount)
				{
					drawerChainIndices.Add(0);
				}
				return drawerChainIndices[DrawCount];
			}
		}

		/// <summary>
		/// Whether this property supports having prefab modifications applied or not.
		/// </summary>
		public bool SupportsPrefabModifications
		{
			get
			{
				if (!supportsPrefabModifications.HasValue)
				{
					if (!Tree.PrefabModificationHandler.HasPrefabs)
					{
						supportsPrefabModifications = false;
					}
					else if (Tree.PrefabModificationHandler.HasNestedOdinPrefabData)
					{
						supportsPrefabModifications = false;
					}
					else if (this == Tree.RootProperty)
					{
						supportsPrefabModifications = false;
					}
					else if (ValueEntry == null || (ParentValueProperty != null && !ParentValueProperty.IsTreeRoot && !ParentValueProperty.SupportsPrefabModifications))
					{
						supportsPrefabModifications = false;
					}
					else if (ValueEntry.SerializationBackend == SerializationBackend.None)
					{
						supportsPrefabModifications = false;
					}
					else if (GetAttribute<DoesNotSupportPrefabModificationsAttribute>() != null || Info.GetAttribute<DoesNotSupportPrefabModificationsAttribute>() != null)
					{
						supportsPrefabModifications = false;
					}
					else if (ChildResolver is IMaySupportPrefabModifications)
					{
						supportsPrefabModifications = (ChildResolver as IMaySupportPrefabModifications).MaySupportPrefabModifications;
					}
					else
					{
						supportsPrefabModifications = false;
					}
				}
				return supportsPrefabModifications.Value;
			}
		}

		/// <summary>
		/// Gets an immutable list of the components attached to the property.
		/// </summary>
		public ImmutableList<PropertyComponent> Components
		{
			get
			{
				if (componentsImmutable == null)
				{
					if (components == null)
					{
						CreateComponents();
					}
					componentsImmutable = new ImmutableList<PropertyComponent>(components);
				}
				return componentsImmutable;
			}
		}

		/// <summary>
		/// Gets an immutable list of processed attributes for the property.
		/// </summary>
		public ImmutableList<Attribute> Attributes
		{
			get
			{
				if (processedAttributesImmutable == null)
				{
					processedAttributesImmutable = new ImmutableList<Attribute>(processedAttributes);
				}
				return processedAttributesImmutable;
			}
		}

		/// <summary>
		/// Gets an array of the state updaters of the property. Don't change the contents of this array!
		/// </summary>
		public StateUpdater[] StateUpdaters
		{
			get
			{
				if (stateUpdaters == null)
				{
					GetNewStateUpdaters();
					UpdateStates(Tree.UpdateID);
				}
				return stateUpdaters;
			}
		}

		/// <summary>
		/// The value entry that represents the base value of this property.
		/// </summary>
		public PropertyValueEntry BaseValueEntry { get; private set; }

		/// <summary>
		/// The value entry that represents the strongly typed value of the property; this is possibly an alias entry in case of polymorphism.
		/// </summary>
		public IPropertyValueEntry ValueEntry { get; private set; }

		/// <summary>
		/// The parent of the property. If null, this property is a root-level property in the <see cref="T:Sirenix.OdinInspector.Editor.PropertyTree" />.
		/// </summary>
		public InspectorProperty Parent { get; private set; }

		/// <summary>
		/// The <see cref="T:Sirenix.OdinInspector.Editor.InspectorPropertyInfo" /> of this property.
		/// </summary>
		public InspectorPropertyInfo Info { get; private set; }

		/// <summary>
		/// The <see cref="T:Sirenix.OdinInspector.Editor.PropertyTree" /> that this property exists in.
		/// </summary>
		public PropertyTree Tree { get; private set; }

		/// <summary>
		/// The children of this property.
		/// </summary>
		public PropertyChildren Children { get; private set; }

		/// <summary>
		/// The context container of this property.
		/// </summary>
		public PropertyContextContainer Context { get; private set; }

		/// <summary>
		/// The last rect that this property was drawn within.
		/// </summary>
		public Rect LastDrawnValueRect
		{
			get
			{
				if (DrawCount <= 0)
				{
					return default(Rect);
				}
				if (DrawCount > lastDrawnValueRects.Count)
				{
					lastDrawnValueRects.SetLength(DrawCount);
				}
				return lastDrawnValueRects[DrawCount - 1];
			}
		}

		public Rect PrefabModificationBarSourceRectOverride { get; set; }

		/// <summary>
		/// The type on which this property is declared. This is the same as <see cref="P:Sirenix.OdinInspector.Editor.InspectorPropertyInfo.TypeOfOwner" />.
		/// </summary>
		public Type ParentType { get; private set; }

		/// <summary>
		/// The parent values of this property, by selection index; this represents the values that 'own' this property, on which it is declared.
		/// </summary>
		public ImmutableList ParentValues { get; private set; }

		public InspectorProperty ParentValueProperty { get; private set; }

		/// <summary>
		/// <para>The full Unity property path of this property; note that this is merely a converted version of <see cref="P:Sirenix.OdinInspector.Editor.InspectorProperty.Path" />, and not necessarily a path to an actual Unity property.</para>
		/// <para>In the case of Odin-serialized data, for example, no Unity properties will exist at this path.</para>
		/// </summary>
		public string UnityPropertyPath
		{
			get
			{
				if (unityPropertyPath == null)
				{
					InspectorProperty parent = Parent;
					if (parent != null && !parent.IsTreeRoot)
					{
						if (parent.ChildResolver is IOverridesUnityPropertyNames nameGetter)
						{
							unityPropertyPath = parent.UnityPropertyPath + "." + nameGetter.GetUnityPropertyName(Index);
						}
						else
						{
							unityPropertyPath = parent.UnityPropertyPath + "." + InspectorUtilities.ConvertToUnityPropertyPath(Name);
						}
					}
					else
					{
						unityPropertyPath = InspectorUtilities.ConvertToUnityPropertyPath(Path);
					}
				}
				return unityPropertyPath;
			}
		}

		/// <summary>
		/// <para>The full path of this property as used by deep reflection, containing all the necessary information to find this property through reflection only. This is used as the path for prefab modifications.</para>
		/// </summary>
		[Obsolete("Use PrefabModificationPath instead, which serves the exact same function.", false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public string DeepReflectionPath => PrefabModificationPath;

		/// <summary>
		/// <para>The full path of this property as used by prefab modifications and the deep reflection system, containing all the necessary information to find this property through reflection only.</para>
		/// </summary>
		public string PrefabModificationPath
		{
			get
			{
				if (prefabModificationPath == null)
				{
					prefabModificationPath = InspectorUtilities.ConvertToDeepReflectionPath(Path);
				}
				return prefabModificationPath;
			}
		}

		/// <summary>
		/// The PropertyState of the property at the current draw count index.
		/// </summary>
		public PropertyState State
		{
			get
			{
				int index = DrawCount - 1;
				if (index < 0)
				{
					index = 0;
				}
				if (states == null)
				{
					states = new List<PropertyState>();
				}
				while (states.Count <= index)
				{
					states.Add(null);
				}
				PropertyState state = states[index];
				if (state == null)
				{
					state = new PropertyState(this, index);
					states[index] = state;
				}
				return state;
			}
		}

		private InspectorProperty()
		{
		}

		/// <summary>
		/// Gets the component of a given type on the property, or null if the property does not have a component of the given type.
		/// </summary>
		public T GetComponent<T>() where T : PropertyComponent
		{
			if (components == null || components.Count != Tree.ComponentProviders.Count)
			{
				CreateComponents();
			}
			for (int i = 0; i < components.Count; i++)
			{
				if (components[i] is T result)
				{
					return result;
				}
			}
			return null;
		}

		/// <summary>
		/// Marks the property's serialization root values dirty if they are derived from UnityEngine.Object.
		/// </summary>
		public void MarkSerializationRootDirty()
		{
			if (SerializationRoot == null)
			{
				return;
			}
			foreach (object value in SerializationRoot.ValueEntry.WeakValues)
			{
				UnityEngine.Object obj = value as UnityEngine.Object;
				if (obj != null)
				{
					InspectorUtilities.RegisterUnityObjectDirty(obj);
				}
			}
		}

		/// <summary>
		/// Records the property's serialization root for undo to prepare for undoable changes, with a custom string that includes the property path and Unity object name. If a message is specified, it is included in the custom undo string.
		/// </summary>
		public void RecordForUndo(string message = null, bool forceCompleteObjectUndo = false)
		{
			if (!Tree.RecordUndoForChanges)
			{
				return;
			}
			InspectorProperty serializationRoot = SerializationRoot;
			if (serializationRoot == null)
			{
				return;
			}
			if (!forceCompleteObjectUndo && ValueEntry != null && UnityPolymorphicSerializationBackend.SerializeReferenceAttribute != null)
			{
				ImmutableList<Attribute> attrs = Info.Attributes;
				for (int i = 0; i < attrs.Count; i++)
				{
					if (attrs[i].GetType() == UnityPolymorphicSerializationBackend.SerializeReferenceAttribute)
					{
						forceCompleteObjectUndo = true;
						break;
					}
				}
			}
			for (int j = 0; j < serializationRoot.ValueEntry.ValueCount; j++)
			{
				UnityEngine.Object unityObj = serializationRoot.ValueEntry.WeakValues[j] as UnityEngine.Object;
				if (unityObj != null)
				{
					string recordMessage = ((this != Tree.RootProperty) ? ((message == null) ? ("Change " + PrefabModificationPath + " on " + unityObj.name) : ("Change " + PrefabModificationPath + " on " + unityObj.name + ": " + message)) : ((message == null) ? ("Change " + unityObj.name) : ("Change " + unityObj.name + ": " + message)));
					if (forceCompleteObjectUndo)
					{
						Undo.RegisterCompleteObjectUndo(unityObj, recordMessage);
					}
					else
					{
						Undo.RecordObject(unityObj, recordMessage);
					}
				}
			}
		}

		public IPropertyValueEntry<T> TryGetTypedValueEntry<T>()
		{
			if (ValueEntry == null || BaseValueEntry == null)
			{
				return null;
			}
			if (ValueEntry is IPropertyValueEntry<T> result1)
			{
				return result1;
			}
			if (BaseValueEntry is IPropertyValueEntry<T> result2)
			{
				return result2;
			}
			if (!typeof(T).IsAssignableFrom(ValueEntry.TypeOfValue))
			{
				return null;
			}
			return (IPropertyValueEntry<T>)PropertyValueEntry.CreateAlias(BaseValueEntry, typeof(T));
		}

		/// <summary>
		/// Gets the first attribute of a given type on this property.
		/// </summary>
		public T GetAttribute<T>() where T : Attribute
		{
			for (int i = 0; i < processedAttributes.Count; i++)
			{
				if (processedAttributes[i] is T result)
				{
					return result;
				}
			}
			return null;
		}

		/// <summary>
		/// Gets the first attribute of a given type on this property, which is not contained in a given hashset.
		/// </summary>
		/// <param name="exclude">The attributes to exclude.</param>
		public T GetAttribute<T>(HashSet<Attribute> exclude) where T : Attribute
		{
			for (int i = 0; i < processedAttributes.Count; i++)
			{
				if (processedAttributes[i] is T attr && (exclude == null || !exclude.Contains(attr)))
				{
					return attr;
				}
			}
			return null;
		}

		/// <summary>
		/// Gets all attributes of a given type on the property.
		/// </summary>
		public IEnumerable<T> GetAttributes<T>() where T : Attribute
		{
			for (int i = 0; i < processedAttributes.Count; i++)
			{
				if (processedAttributes[i] is T result)
				{
					yield return result;
				}
			}
		}

		/// <summary>
		/// Returns a <see cref="T:System.String" /> that represents this instance.
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.String" /> that represents this instance.
		/// </returns>
		public override string ToString()
		{
			return "InspectorProperty (" + Path + ")";
		}

		public BakedDrawerChain GetActiveDrawerChain()
		{
			bool isNewlyCreated;
			return GetActiveDrawerChain(out isNewlyCreated);
		}

		private BakedDrawerChain GetActiveDrawerChain(out bool isNewlyCreated)
		{
			if (drawerChains == null)
			{
				drawerChains = new List<BakedDrawerChain>();
			}
			int index = DrawCount - 1;
			if (index < 0)
			{
				index = 0;
			}
			BakedDrawerChain result;
			if (drawerChains.Count <= index)
			{
				result = Tree.DrawerChainResolver.GetDrawerChain(this).Bake();
				drawerChains.Add(result);
				ExitGUIException exitGUI = null;
				for (int i = 0; i < result.BakedDrawerArray.Length; i++)
				{
					try
					{
						result.BakedDrawerArray[i].Initialize(this);
					}
					catch (Exception ex)
					{
						if (ex.IsExitGUIException())
						{
							exitGUI = ex.AsExitGUIException();
						}
						else
						{
							Debug.LogException(ex);
						}
					}
				}
				isNewlyCreated = true;
				if (exitGUI != null)
				{
					throw exitGUI;
				}
			}
			else
			{
				isNewlyCreated = false;
				result = drawerChains[index];
			}
			return result;
		}

		public void RefreshSetup()
		{
			RefreshSetup(disposeOld: true);
		}

		private void RefreshSetup(bool disposeOld)
		{
			if (disposeOld)
			{
				DisposeExistingSetup();
			}
			if (stateUpdaters != null)
			{
				stateUpdaters = null;
			}
			if (drawerChains != null)
			{
				drawerChains.Clear();
			}
			if (states != null)
			{
				for (int i = 0; i < states.Count; i++)
				{
					if (states[i] != null)
					{
						states[i].Reset();
					}
				}
			}
			if (components == null || components.Count != Tree.ComponentProviders.Count)
			{
				CreateComponents();
			}
			else
			{
				for (int j = 0; j < components.Count; j++)
				{
					components[j].Reset();
				}
			}
			IsDesigned = DesignerRegistry.IsPropertyDesigned(this);
			RefreshProcessedAttributes();
			ChildResolver = Tree.PropertyResolverLocator.GetResolver(this);
			Children = new PropertyChildren(this);
			Children.Update();
			GetNewStateUpdaters();
			UpdateStates(Tree.UpdateID);
		}

		private void CreateComponents()
		{
			if (components == null)
			{
				components = new List<PropertyComponent>(Tree.ComponentProviders.Count);
			}
			else
			{
				for (int i = 0; i < components.Count; i++)
				{
					if (components[i] is IDisposable disposable)
					{
						try
						{
							disposable.Dispose();
						}
						catch (Exception exception)
						{
							Debug.LogException(exception);
						}
					}
				}
				components.Clear();
			}
			for (int j = 0; j < Tree.ComponentProviders.Count; j++)
			{
				components.Add(Tree.ComponentProviders[j].CreateComponent(this));
			}
		}

		private void RefreshProcessedAttributes()
		{
			processedAttributes.Clear();
			for (int i = 0; i < Info.Attributes.Count; i++)
			{
				processedAttributes.Add(Info.Attributes[i]);
			}
			List<OdinAttributeProcessor> processors = Tree.AttributeProcessorLocator.GetSelfProcessors(this);
			for (int j = 0; j < processors.Count; j++)
			{
				try
				{
					processors[j].ProcessSelfAttributes(this, processedAttributes);
				}
				catch (Exception exception)
				{
					Debug.LogException(exception);
				}
			}
			if (IsDesigned)
			{
				DesignerInspectorPropertyInfoUtility.ApplySelfPatches(this, processedAttributes);
			}
		}

		internal void OnStateUpdate(int treeID)
		{
			Update();
			UpdateStates(treeID);
			PropertyChildren.ExistingChildEnumerator enumerator = Children.GetExistingChildren().GetEnumerator();
			while (enumerator.MoveNext())
			{
				InspectorProperty prop = enumerator.Current;
				prop.OnStateUpdate(treeID);
			}
		}

		/// <summary>
		/// Draws this property in the inspector.
		/// </summary>
		public void Draw()
		{
			Draw(Label);
		}

		/// <summary>
		/// Draws this property in the inspector with a given default label. This default label may be overridden by attributes on the drawn property.
		/// </summary>
		public void Draw(GUIContent defaultLabel)
		{
			Update();
			bool popGUIEnabled = false;
			bool popDraw = true;
			try
			{
				PushDraw();
				BakedDrawerChain chain = GetActiveDrawerChain();
				PropertyState state = State;
				bool fadeGroup = AnimateVisibility;
				if (fadeGroup ? SirenixEditorGUI.BeginFadeGroup(state, state.VisibleLastLayout) : state.VisibleLastLayout)
				{
					if (RecursiveDrawDepth + InlineEditorAttributeDrawer.CurrentInlineEditorDrawDepth > GlobalConfig<GeneralDrawerConfig>.Instance.MaxRecursiveDrawDepth)
					{
						SirenixEditorGUI.MessageBox("The property '" + NiceName + "' has exceeded the maximum recursive draw depth limit of " + GlobalConfig<GeneralDrawerConfig>.Instance.MaxRecursiveDrawDepth + ".", MessageType.Error, GlobalConfig<GeneralDrawerConfig>.Instance.MessageBoxFontSize);
						return;
					}
					if (!IsTreeRoot && ValueEntry != null && ValueEntry.SerializationBackend == SerializationBackend.Odin && !SupportsPrefabModifications && Tree.PrefabModificationHandler.HasPrefabs && !GUIHelper.IsDrawingDictionaryKey && Info.PropertyType == PropertyType.Value)
					{
						if (ParentValueProperty != null && (ParentValueProperty.IsTreeRoot || ParentValueProperty.SupportsPrefabModifications) && GlobalConfig<GeneralDrawerConfig>.Instance.ShowPrefabModificationsDisabledMessage)
						{
							string objText = (Tree.PrefabModificationHandler.HasNestedOdinPrefabData ? "this instance" : "prefab instances");
							SirenixEditorGUI.MessageBox("The property '" + NiceName + "' does not support being modified on " + objText + ". (You can disable this message in the general drawer config.)", MessageType.Info, GlobalConfig<GeneralDrawerConfig>.Instance.MessageBoxFontSize);
						}
						GUIHelper.PushGUIEnabled(enabled: false);
						popGUIEnabled = true;
					}
					chain.Reset();
					EventType e = Event.current.type;
					int currDrawCount = DrawCount;
					bool measureBox = e == EventType.Repaint;
					bool boxHasBeenMeasured = false;
					Rect measuredBox = default(Rect);
					if (measureBox)
					{
						GUIHelper.BeginLayoutMeasuring();
					}
					try
					{
						if (stateUpdaters != null)
						{
							for (int i = 0; i < stateUpdaters.Length; i++)
							{
								StateUpdater updater = stateUpdaters[i];
								if (updater.ErrorMessage != null)
								{
									SirenixEditorGUI.MessageBox("Error in state updater '" + updater.GetType().GetNiceName() + "':\n\n" + updater.ErrorMessage, MessageType.Error, GlobalConfig<GeneralDrawerConfig>.Instance.MessageBoxFontSize);
								}
							}
						}
						if (chain.MoveNext())
						{
							bool setIsBoldState = ValueEntry != null && e == EventType.Repaint;
							bool isPrefabChanged = false;
							if (setIsBoldState)
							{
								isPrefabChanged = !IsTreeRoot && (ValueEntry.ValueChangedFromPrefab || ValueEntry.ChildValueChangedFromPrefab);
								if (Parent != null && Parent.ChildResolver.IsCollection && Parent.ValueEntry.ListLengthChangedFromPrefab)
								{
									InspectorProperty correspondingProp = Tree.PrefabModificationHandler.PrefabPropertyTree.GetPropertyAtPath(Parent.Path);
									if (correspondingProp != null && Index >= correspondingProp.Children.Count)
									{
										isPrefabChanged = true;
									}
								}
								bool boldState = isPrefabChanged;
								if (GUIHelper.IsDrawingDictionaryKey)
								{
									boldState |= GUIHelper.IsBoldLabel;
								}
								GUIHelper.PushIsBoldLabel(boldState);
							}
							bool popPushGUIDisabled = ValueEntry != null && !ValueEntry.IsEditable;
							popPushGUIDisabled |= !state.EnabledLastLayout;
							if (popPushGUIDisabled)
							{
								GUIHelper.PushGUIEnabled(enabled: false);
							}
							chain.Current.DrawProperty(defaultLabel);
							if (popPushGUIDisabled)
							{
								GUIHelper.PopGUIEnabled();
							}
							if (setIsBoldState)
							{
								GUIHelper.PopIsBoldLabel();
							}
							if (measureBox)
							{
								measuredBox = GUIHelper.EndLayoutMeasuring();
								boxHasBeenMeasured = true;
							}
							if (isPrefabChanged && e == EventType.Repaint && GlobalConfig<GeneralDrawerConfig>.Instance.ShowPrefabModifiedValueBar)
							{
								Color prefabChangeMarginBarColor = new Color(0.003921569f, 0.6f, 0.9215686f, 1f);
								Rect rectOverride = PrefabModificationBarSourceRectOverride;
								Rect rect;
								if (rectOverride != default(Rect))
								{
									rect = rectOverride;
									rect.width = 2f;
									rect.x -= 2.5f;
								}
								else
								{
									rect = measuredBox;
									rect.width = 2f;
									rect.x -= 2.5f;
									rect.x += GUIHelper.CurrentIndentAmount;
									if (Children.Count > 0)
									{
										rect.height = EditorGUIUtility.singleLineHeight;
									}
									_ = ChildResolver is ICollectionResolver;
								}
								GUIHelper.PushGUIEnabled(enabled: true);
								SirenixEditorGUI.DrawSolidRect(rect, prefabChangeMarginBarColor);
								GUIHelper.PopGUIEnabled();
							}
						}
						else if (Info.PropertyType == PropertyType.Method)
						{
							EditorGUILayout.LabelField(NiceName, "No drawers could be found for the method property '" + Name + "'.");
						}
						else if (Info.PropertyType == PropertyType.Group)
						{
							PropertyGroupAttribute attr = GetAttribute<PropertyGroupAttribute>() ?? Info.GetAttribute<PropertyGroupAttribute>();
							if (attr != null)
							{
								EditorGUILayout.LabelField(NiceName, "No drawers could be found for the property group '" + Name + "' with property group attribute type '" + attr.GetType().GetNiceName() + "'.");
							}
							else
							{
								EditorGUILayout.LabelField(NiceName, "No drawers could be found for the property group '" + Name + "'.");
							}
						}
					}
					catch (Exception ex)
					{
						if (ex.IsExitGUIException())
						{
							popDraw = false;
							throw ex.AsExitGUIException();
						}
						string msg = "This error occurred while being drawn by Odin. \nCurrent IMGUI event: " + Event.current.type.ToString() + "\nOdin Property Path: " + Path + "\nOdin Drawer Chain:\n" + string.Join("\n", chain.BakedDrawerArray.Select((OdinDrawer n) => " > " + n.GetType().GetNiceName()).ToArray()) + ".";
						Debug.LogException(new OdinPropertyException(msg, ex));
					}
					if (measureBox)
					{
						if (!boxHasBeenMeasured)
						{
							measuredBox = GUIHelper.EndLayoutMeasuring();
						}
						if (currDrawCount > lastDrawnValueRects.Count)
						{
							lastDrawnValueRects.SetLength(currDrawCount);
						}
						lastDrawnValueRects[currDrawCount - 1] = measuredBox;
					}
				}
				if (fadeGroup)
				{
					SirenixEditorGUI.EndFadeGroup();
				}
			}
			finally
			{
				if (popDraw)
				{
					PopDraw();
				}
				if (popGUIEnabled)
				{
					GUIHelper.PopGUIEnabled();
				}
			}
		}

		/// <summary>
		/// Push a draw session. This is used by <see cref="P:Sirenix.OdinInspector.Editor.InspectorProperty.DrawCount" /> and <see cref="P:Sirenix.OdinInspector.Editor.InspectorProperty.RecursiveDrawDepth" />.
		/// </summary>
		public void PushDraw()
		{
			maxDrawCount++;
			drawCountStack.Push(maxDrawCount);
		}

		/// <summary>
		/// Increments the current drawer chain index. This is used by <see cref="P:Sirenix.OdinInspector.Editor.InspectorProperty.DrawerChainIndex" />.
		/// </summary>
		public void IncrementDrawerChainIndex()
		{
			while (drawerChainIndices.Count <= DrawCount)
			{
				drawerChainIndices.Add(0);
			}
			drawerChainIndices[DrawCount]++;
		}

		/// <summary>
		/// Pop a draw session. This is used by <see cref="P:Sirenix.OdinInspector.Editor.InspectorProperty.DrawCount" /> and <see cref="P:Sirenix.OdinInspector.Editor.InspectorProperty.RecursiveDrawDepth" />.
		/// </summary>
		public void PopDraw()
		{
			drawCountStack.Pop();
		}

		public bool IsReachableFromRoot()
		{
			bool reachable = false;
			try
			{
				if (Parent == null)
				{
					InspectorProperty root = Tree.RootProperty;
					reachable = this == root || root.Children[Name] == this;
					return reachable;
				}
				if (!Parent.IsReachableFromRoot())
				{
					return false;
				}
				reachable = Parent.Children[Name] == this;
				return reachable;
			}
			finally
			{
				if (reachable)
				{
					Update();
				}
			}
		}

		/// <summary>
		/// Gets the next property in the <see cref="T:Sirenix.OdinInspector.Editor.PropertyTree" />, or null if none is found.
		/// </summary>
		/// <param name="includeChildren">Whether to include children or not.</param>
		/// <param name="visibleOnly">Whether to only include visible properties.</param>
		public InspectorProperty NextProperty(bool includeChildren = true, bool visibleOnly = false)
		{
			if (includeChildren)
			{
				if (visibleOnly)
				{
					for (int i = 0; i < Children.Count; i++)
					{
						InspectorProperty child = Children[i];
						if (child.State.Visible)
						{
							return child;
						}
					}
				}
				else if (Children.Count > 0)
				{
					return Children.Get(0);
				}
			}
			InspectorProperty former = null;
			InspectorProperty current = this;
			InspectorProperty treeRoot = Tree.RootProperty;
			while (true)
			{
				former = current;
				current = current.Parent;
				if (current != null && current != treeRoot && former.Index + 1 >= former.Parent.Children.Count)
				{
					continue;
				}
				if (current == null)
				{
					break;
				}
				if (visibleOnly)
				{
					for (int j = former.Index + 1; j < current.Children.Count; j++)
					{
						InspectorProperty child2 = current.Children[j];
						if (child2.State.Visible)
						{
							return child2;
						}
					}
				}
				else if (former.Index + 1 < current.Children.Count)
				{
					return current.Children[former.Index + 1];
				}
			}
			return null;
		}

		/// <summary>
		/// Finds the first parent property that matches a given predicate.
		/// </summary>
		public InspectorProperty FindParent(Func<InspectorProperty, bool> predicate, bool includeSelf)
		{
			for (InspectorProperty current = (includeSelf ? this : Parent); current != null; current = current.Parent)
			{
				if (predicate(current))
				{
					return current;
				}
			}
			return null;
		}

		/// <summary>
		/// Finds the first child recursively, that matches a given predicate.
		/// </summary>
		public InspectorProperty FindChild(Func<InspectorProperty, bool> predicate, bool includeSelf)
		{
			if (includeSelf && predicate(this))
			{
				return this;
			}
			return Children.Recurse().FirstOrDefault(predicate);
		}

		internal void ClearDrawCount()
		{
			maxDrawCount = 0;
			drawCountStack.Clear();
			for (int i = 0; i < drawerChainIndices.Count; i++)
			{
				drawerChainIndices[i] = 0;
			}
		}

		/// <summary>
		/// Updates the property. This method resets the temporary context, and updates the value entry and the property children.
		/// </summary>
		/// <param name="forceUpdate">If true, the property will update regardless of whether it has already updated for the current <see cref="P:Sirenix.OdinInspector.Editor.PropertyTree.UpdateID" />.</param>
		public bool Update(bool forceUpdate = false)
		{
			bool newId = Tree.UpdateID != lastUpdatedTreeID;
			if (!forceUpdate && !newId)
			{
				return false;
			}
			if (newId)
			{
				ClearDrawCount();
				PrefabModificationBarSourceRectOverride = default(Rect);
			}
			lastUpdatedTreeID = Tree.UpdateID;
			UpdateValueEntry();
			if (stateUpdaters == null || Children == null || ChildResolver == null)
			{
				RefreshSetup(disposeOld: false);
			}
			else if (ValueEntry != null && ChildResolver.ResolverForType != null && ValueEntry.TypeOfValue != ChildResolver.ResolverForType)
			{
				RefreshSetup(disposeOld: true);
			}
			else
			{
				Children.Update();
			}
			if (ValueEntry != null)
			{
				BaseValueEntry.RefreshPrefabModificationState();
			}
			UpdateStates(lastUpdatedTreeID);
			return true;
		}

		private void UpdateStates(int treeID)
		{
			if (Tree.IsDesignerTree)
			{
				return;
			}
			if (stateUpdaters == null)
			{
				GetNewStateUpdaters();
			}
			if (lastUpdatedStateUpdatersID == treeID)
			{
				return;
			}
			lastUpdatedStateUpdatersID = treeID;
			for (int i = 0; i < stateUpdaters.Length; i++)
			{
				try
				{
					stateUpdaters[i].OnStateUpdate();
				}
				catch (Exception exception)
				{
					Debug.LogException(exception);
				}
			}
			if (states == null)
			{
				return;
			}
			for (int j = 0; j < states.Count; j++)
			{
				if (states[j] != null)
				{
					states[j].Update();
				}
			}
		}

		private void GetNewStateUpdaters()
		{
			if (Tree.IsDesignerTree)
			{
				stateUpdaters = null;
				return;
			}
			stateUpdaters = Tree.StateUpdaterLocator.GetStateUpdaters(this);
			for (int i = 0; i < stateUpdaters.Length; i++)
			{
				try
				{
					stateUpdaters[i].Initialize(this);
				}
				catch (Exception exception)
				{
					Debug.LogException(exception);
				}
			}
		}

		/// <summary>
		/// Populates a generic menu with items from all drawers for this property that implement <see cref="T:Sirenix.OdinInspector.Editor.IDefinesGenericMenuItems" />.
		/// </summary>
		public void PopulateGenericMenu(GenericMenu genericMenu)
		{
			if (genericMenu == null)
			{
				throw new ArgumentNullException("genericMenu");
			}
			if (Tree.TreeIsSetupForIMGUIDrawing_TEMP_INTERNAL)
			{
				OdinDrawer[] drawers = GetActiveDrawerChain().BakedDrawerArray;
				int count = DrawCount;
				int prevIndex = DrawerChainIndex;
				try
				{
					for (int i = 0; i < drawers.Length; i++)
					{
						if (drawers[i] is IDefinesGenericMenuItems drawer)
						{
							drawerChainIndices[count] = i + 1;
							drawer.PopulateGenericMenu(this, genericMenu);
						}
					}
				}
				finally
				{
					drawerChainIndices[count] = prevIndex;
				}
				ValidationComponent validatorComponent = GetComponent<ValidationComponent>();
				if (validatorComponent == null)
				{
					return;
				}
				IList<Validator> validatorList = validatorComponent.GetValidators();
				int length = validatorList.Count;
				bool addSeparatorBeforeMenuItem = true;
				for (int j = 0; j < length; j++)
				{
					Validator validator = validatorList[j];
					if (validator is IDefinesGenericMenuItems definer)
					{
						if (addSeparatorBeforeMenuItem)
						{
							genericMenu.AddSeparator(string.Empty);
							addSeparatorBeforeMenuItem = false;
						}
						try
						{
							definer.PopulateGenericMenu(this, genericMenu);
						}
						catch (Exception exception)
						{
							Debug.LogException(exception);
						}
					}
				}
				return;
			}
			throw new InvalidOperationException("This property's tree is not currently set up for drawing and cannot create generic menus.");
		}

		/// <summary>
		/// Determines whether this property is the child of another property in the hierarchy.
		/// </summary>
		/// <param name="other">The property to check whether this property is the child of.</param>
		/// <exception cref="T:System.ArgumentNullException">other is null</exception>
		public bool IsChildOf(InspectorProperty other)
		{
			if (other == null)
			{
				throw new ArgumentNullException("other");
			}
			for (InspectorProperty parent = Parent; parent != null; parent = parent.Parent)
			{
				if (parent == other)
				{
					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// Determines whether this property is a parent of another property in the hierarchy.
		/// </summary>
		/// <param name="other">The property to check whether this property is the parent of.</param>
		/// <exception cref="T:System.ArgumentNullException">other is null</exception>
		public bool IsParentOf(InspectorProperty other)
		{
			if (other == null)
			{
				throw new ArgumentNullException("other");
			}
			for (InspectorProperty parent = other.Parent; parent != null; parent = parent.Parent)
			{
				if (parent == this)
				{
					return true;
				}
			}
			return false;
		}

		internal static InspectorProperty Create(PropertyTree tree, InspectorProperty parent, InspectorPropertyInfo info, int index, bool isRoot)
		{
			if (tree == null)
			{
				throw new ArgumentNullException("tree");
			}
			if (info == null)
			{
				throw new ArgumentNullException("info");
			}
			if (parent != null)
			{
				if (tree != parent.Tree)
				{
					throw new ArgumentException("The given tree and the given parent's tree are not the same tree.");
				}
				if (index < 0 || index >= parent.Children.Count)
				{
					throw new IndexOutOfRangeException("The given index for the property to create is out of bounds.");
				}
			}
			else if (!isRoot)
			{
				throw new ArgumentException("The property to be created has been given no parent, and is not the tree root.");
			}
			InspectorProperty property = new InspectorProperty();
			property.Tree = tree;
			property.Info = info;
			property.Parent = parent;
			property.Index = index;
			property.Context = new PropertyContextContainer(property);
			if (parent != null)
			{
				property.Path = parent.Children.GetPath(index);
			}
			else
			{
				property.Path = info.PropertyName;
			}
			if (property.Path == null)
			{
				Debug.Log("Property path is null for property " + ObjectNames.NicifyVariableName(info.PropertyName.TrimStart('#', '$')) + "!");
			}
			if (parent != null)
			{
				InspectorProperty current = property;
				do
				{
					current = current.Parent;
				}
				while (current != null && current.BaseValueEntry == null);
				property.ParentValueProperty = current;
			}
			if (property.ParentValueProperty != null)
			{
				property.ParentType = property.ParentValueProperty.ValueEntry.TypeOfValue;
				property.ParentValues = new ImmutableList(property.ParentValueProperty.ValueEntry.WeakValues);
			}
			else
			{
				property.ParentType = tree.TargetType;
				property.ParentValues = new ImmutableList(tree.WeakTargets);
			}
			InspectorProperty current2 = property.ParentValueProperty;
			while (current2 != null && !current2.ValueEntry.TypeOfValue.InheritsFrom(typeof(UnityEngine.Object)))
			{
				current2 = current2.ParentValueProperty;
			}
			if (current2 != null)
			{
				property.SerializationRoot = current2;
			}
			else
			{
				property.SerializationRoot = (isRoot ? property : tree.RootProperty);
			}
			property.Name = info.PropertyName;
			MethodInfo mi = property.Info.GetMemberInfo() as MethodInfo;
			if (mi != null)
			{
				string name = property.Name;
				int parensIndex = name.IndexOf('(');
				if (parensIndex >= 0)
				{
					name = name.Substring(0, parensIndex);
				}
				property.NiceName = name.TrimStart('#', '$').SplitPascalCase();
			}
			else if (property.Name.Length > 1 && property.Name[0] == '$' && property.Parent != null && property.Parent.ChildResolver is IOrderedCollectionResolver)
			{
				property.NiceName = property.Parent.NiceName + " [" + property.Name.Substring(1) + "]";
			}
			else
			{
				property.NiceName = ObjectNames.NicifyVariableName(property.Name.TrimStart('#', '$'));
			}
			property.Label = new GUIContent(property.NiceName);
			if (property.Info.PropertyType == PropertyType.Value)
			{
				property.BaseValueEntry = PropertyValueEntry.Create(property, info.TypeOfValue, isRoot);
				property.ValueEntry = property.BaseValueEntry;
			}
			property.CreateComponents();
			property.IsDesigned = DesignerRegistry.IsPropertyDesigned(property);
			if (!isRoot)
			{
				property.RefreshProcessedAttributes();
				property.ChildResolver = tree.PropertyResolverLocator.GetResolver(property);
				property.Children = new PropertyChildren(property);
			}
			return property;
		}

		private void UpdateValueEntry()
		{
			if (Info.PropertyType != PropertyType.Value)
			{
				if (ValueEntry != null || BaseValueEntry != null)
				{
					ValueEntry = null;
					BaseValueEntry = null;
					RefreshSetup();
				}
				return;
			}
			BaseValueEntry.Update();
			if (!Info.TypeOfValue.IsValueType)
			{
				Type containedType = BaseValueEntry.TypeOfValue;
				if (containedType != BaseValueEntry.BaseValueType)
				{
					if (ValueEntry == null || (ValueEntry.IsAlias && (ValueEntry as PropertyValueEntryAlias).TValueGenericTypeArgument != containedType) || (!ValueEntry.IsAlias && ValueEntry.TypeOfValue != ValueEntry.BaseValueType))
					{
						DisposeExistingSetup();
						ValueEntry = PropertyValueEntry.CreateAlias(BaseValueEntry, containedType);
						RefreshSetup(disposeOld: false);
					}
				}
				else if (ValueEntry != BaseValueEntry)
				{
					DisposeExistingSetup();
					ValueEntry = BaseValueEntry;
					RefreshSetup(disposeOld: false);
				}
			}
			else if (ValueEntry == null)
			{
				DisposeExistingSetup();
				ValueEntry = BaseValueEntry;
				RefreshSetup(disposeOld: false);
			}
			if (ValueEntry != BaseValueEntry)
			{
				ValueEntry.Update();
			}
		}

		public void Dispose()
		{
			DisposeExistingSetup();
		}

		public void CleanForCachedReuse()
		{
			PropertyChildren.ExistingChildEnumerator enumerator = Children.GetExistingChildren().GetEnumerator();
			while (enumerator.MoveNext())
			{
				InspectorProperty child = enumerator.Current;
				child.CleanForCachedReuse();
			}
			if (drawerChains != null)
			{
				foreach (BakedDrawerChain drawerChain in drawerChains)
				{
					OdinDrawer[] bakedDrawerArray = drawerChain.BakedDrawerArray;
					foreach (OdinDrawer drawer in bakedDrawerArray)
					{
						if (drawer is IDisposable disposable)
						{
							try
							{
								disposable.Dispose();
							}
							catch (Exception exception)
							{
								Debug.LogException(exception);
							}
						}
					}
				}
				drawerChains.Clear();
			}
			if (stateUpdaters != null)
			{
				for (int j = 0; j < stateUpdaters.Length; j++)
				{
					if (stateUpdaters[j] is IDisposable disposable2)
					{
						try
						{
							disposable2.Dispose();
						}
						catch (Exception exception2)
						{
							Debug.LogException(exception2);
						}
					}
				}
				stateUpdaters = null;
			}
			if (components != null)
			{
				for (int k = 0; k < components.Count; k++)
				{
					if (components[k] is IDisposable disposable3)
					{
						try
						{
							disposable3.Dispose();
						}
						catch (Exception exception3)
						{
							Debug.LogException(exception3);
						}
					}
				}
			}
			if (states != null)
			{
				for (int l = 0; l < states.Count; l++)
				{
					states[l]?.CleanForCachedReuse();
				}
			}
			components = null;
			componentsImmutable = null;
		}

		private void DisposeExistingSetup()
		{
			if (drawerChains != null)
			{
				foreach (BakedDrawerChain drawerChain in drawerChains)
				{
					OdinDrawer[] bakedDrawerArray = drawerChain.BakedDrawerArray;
					foreach (OdinDrawer drawer in bakedDrawerArray)
					{
						if (drawer is IDisposable disposable)
						{
							try
							{
								disposable.Dispose();
							}
							catch (Exception exception)
							{
								Debug.LogException(exception);
							}
						}
					}
				}
				drawerChains.Clear();
			}
			if (stateUpdaters != null)
			{
				for (int j = 0; j < stateUpdaters.Length; j++)
				{
					if (stateUpdaters[j] is IDisposable disposable2)
					{
						try
						{
							disposable2.Dispose();
						}
						catch (Exception exception2)
						{
							Debug.LogException(exception2);
						}
					}
				}
				stateUpdaters = null;
			}
			if (components != null)
			{
				for (int k = 0; k < components.Count; k++)
				{
					if (components[k] is IDisposable disposable3)
					{
						try
						{
							disposable3.Dispose();
						}
						catch (Exception exception3)
						{
							Debug.LogException(exception3);
						}
					}
				}
				components.Clear();
			}
			if (ChildResolver is IDisposable)
			{
				try
				{
					(ChildResolver as IDisposable).Dispose();
				}
				catch (Exception exception4)
				{
					Debug.LogException(exception4);
				}
			}
			if (ValueEntry != null)
			{
				try
				{
					ValueEntry.Dispose();
				}
				catch (Exception exception5)
				{
					Debug.LogException(exception5);
				}
			}
			if (Children == null)
			{
				return;
			}
			PropertyChildren.ExistingChildEnumerator enumerator2 = Children.GetExistingChildren().GetEnumerator();
			while (enumerator2.MoveNext())
			{
				InspectorProperty child = enumerator2.Current;
				try
				{
					child.Dispose();
				}
				catch (Exception exception6)
				{
					Debug.LogException(exception6);
				}
			}
		}

		private static InspectorProperty PropertyQueryLookup(InspectorProperty context, string path)
		{
			InspectorProperty parent = context.ParentValueProperty;
			while (parent != null && !parent.Info.HasBackingMembers)
			{
				parent = parent.ParentValueProperty;
			}
			if (parent == null)
			{
				parent = context.Tree.RootProperty;
			}
			InspectorProperty result = parent.Children[path];
			if (result == null)
			{
				result = ((parent != context.Tree.RootProperty) ? context.Tree.GetPropertyAtPath(parent.Path + "." + path) : context.Tree.GetPropertyAtPath(path));
			}
			if (result == null)
			{
				throw new Exception("Property query could not find the property '" + path + "' in the context of the property '" + context.NiceName + "'.");
			}
			if (context.Tree.TreeIsSetupForIMGUIDrawing_TEMP_INTERNAL && Event.current != null)
			{
				result.GetActiveDrawerChain();
			}
			return result;
		}
	}
}
