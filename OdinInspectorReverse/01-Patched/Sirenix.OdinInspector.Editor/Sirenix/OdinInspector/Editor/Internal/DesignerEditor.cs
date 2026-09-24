using System;
using Clipboard = Sirenix.Utilities.Editor.Clipboard;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Sirenix.Reflection.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Internal
{
	internal class DesignerEditor
	{
		public readonly EditorKey Key;

		public static bool IsEditorRendering;

		public static DesignerEditor CurrentEditor;

		public ulong SyncTag;

		public bool ForceSync;

		public string SyncException;

		public Vector2 SyncExceptionScroll = Vector2.zero;

		public HashSet<EditorWindow> WindowsRetainingThis = new HashSet<EditorWindow>();

		public Action OnNextFrame;

		public bool IsGenericClosedByDesigner;

		public bool HasValidPropertyResolver;

		public Type PropertyResolverType;

		public object Instance;

		public string InstancePath;

		public DesignerEditorContext Context;

		public EditorTypePatch TypePatch;

		public DesignerEditorWindow Window;

		public DesignerAttributePopup Popup;

		public DesignerEditorNode RootNode;

		public SmoothScrollView NodeScrollView;

		public SearchField SearchField;

		public Rect SearchRect = Rect.zero;

		public Type ElementType;

		public RefList<DesignerLayoutNode> Layout = new RefList<DesignerLayoutNode>(32);

		public Dictionary<string, DesignerEditorNode> Nodes = new Dictionary<string, DesignerEditorNode>(32);

		public HashSet<DesignerEditorNode> NodesInUse = new HashSet<DesignerEditorNode>();

		public VirtualizedScrollView ScrollView;

		private DragAndDropState dragAndDropState = new DragAndDropState();

		private ProjectionPass projectionPass = new ProjectionPass();

		private static readonly Dictionary<string, GroupPatch> GroupHandles = new Dictionary<string, GroupPatch>();

		private static readonly Dictionary<string, PropertyPatch> PropertyHandles = new Dictionary<string, PropertyPatch>();

		public void RetainIn(EditorWindow window)
		{
			TypePatch.RetainIn(window);
			WindowsRetainingThis.Add(window);
		}

		public void RetainAncestorsIn(EditorWindow window)
		{
			Type next = DesignerUtils.GetBaseType(TypePatch.TargetType);
			while (next != null)
			{
				DesignerEditors.Get(next, null, null)?.RetainIn(window);
				next = DesignerUtils.GetBaseType(next);
			}
		}

		public void ReleaseFrom(EditorWindow window)
		{
			WindowsRetainingThis.Remove(window);
			TypePatch.ReleaseFrom(window);
			if (WindowsRetainingThis.Count <= 0)
			{
				DesignerEditors.Remove(this);
			}
		}

		public void OnGUI(Rect rect, OdinEditorWindow callingWindow)
		{
			CurrentEditor = this;
			IsEditorRendering = true;
			if (ScrollView == null)
			{
				ScrollView = new VirtualizedScrollView();
			}
			TypePatch?.HandleDiskSynchronization();
			SyncIfNeeded();
			if (SyncException != null)
			{
				Rect syncRect = rect.Padding(16f);
				SirenixEditorGUI.DrawRoundRect(syncRect, new Color(0.12f, 0.12f, 0.12f), 4f, new Color(0.33f, 0.33f, 0.33f), 1f);
				Rect syncButtonRect = syncRect.TakeFromBottom(32f);
				if (GUI.Button(syncButtonRect.Padding(4f).AlignCenterX(180f), "Copy Exception To Clipboard"))
				{
					Clipboard.Copy(SyncException);
					callingWindow.ShowToast(ToastPosition.BottomRight, SdfIconType.Clipboard, "Copied Exception To Clipboard", Color.clear, 2.5f);
				}
				GUILayout.BeginArea(syncRect.Padding(8f));
				SyncExceptionScroll = GUILayout.BeginScrollView(SyncExceptionScroll);
				GUILayout.Label(SyncException, DesignerStyles.RichLabelCenteredWordWrap);
				GUILayout.EndScrollView();
				GUILayout.EndArea();
				return;
			}
			if (!HasValidPropertyResolver)
			{
				GUI.Label(rect.Padding(16f), "Property Resolver <b>" + ((PropertyResolverType != null) ? PropertyResolverType.GetNiceName() : "null") + "</b> is not supported by the Visual Designer", DesignerStyles.RichLabelCenteredWordWrap);
				if (OnNextFrame != null)
				{
					OnNextFrame();
					OnNextFrame = null;
				}
				HandleContextInteractions(rect, callingWindow);
				return;
			}
			Rect viewport = rect.Padding(20f);
			List<Slot> slots = LayoutPass.Run(Context, RootNode, viewport);
			ScrollView.Reset();
			foreach (Slot slot in slots)
			{
				ScrollView.AllocateRect(slot.Rect);
			}
			if (dragAndDropState.DragActive)
			{
				ScrollView.AutoScrollNearEdges();
			}
			ScrollView.Begin();
			List<VirtualizedScrollView.VisibleSlot> visibleSlots = ScrollView.GetVisibleSlots(viewport, rect);
			Context.HoverNode = null;
			for (int i = visibleSlots.Count - 1; i >= 0; i--)
			{
				VirtualizedScrollView.VisibleSlot visibleSlot = visibleSlots[i];
				Slot slot2 = slots[visibleSlot.Index];
				InputPass.Run(Context, slot2, visibleSlot.Rect, dragAndDropState);
			}
			if (OnNextFrame != null)
			{
				OnNextFrame();
				OnNextFrame = null;
			}
			projectionPass.Begin(slots, dragAndDropState);
			for (int j = 0; j < visibleSlots.Count; j++)
			{
				VirtualizedScrollView.VisibleSlot vs = visibleSlots[j];
				Slot slot3 = slots[vs.Index];
				Rect projectedRect = projectionPass.Run(slot3, vs.Rect);
				RenderPass.Run(slot3, projectedRect, dragAndDropState, Context);
			}
			RenderPass.DrawDropZoneGhost(projectionPass.GetGhostRect(viewport), dragAndDropState);
			projectionPass.End();
			ScrollView.End();
			SearchField = SearchField ?? new SearchField();
			if (SearchRect != Rect.zero)
			{
				int filteredNodeCount = Context.FilteredNodes.Count;
				if (filteredNodeCount > 0)
				{
					string countLabel = $"{Context.SelectedFilteredNodeIndex + 1}/{filteredNodeCount}";
					float countLabelWidth = SirenixGUIStyles.LabelCentered.CalcWidth(countLabel) + 10f;
					Rect navRect = SearchRect.TakeFromRight(50f + countLabelWidth);
					Rect countRect = navRect.TakeFromLeft(countLabelWidth);
					GUI.Label(countRect, countLabel, SirenixGUIStyles.LabelCentered);
					if (DesignerGUI.DrawSimpleIconButton(navRect.Split(0, 2), SdfIconType.CaretUpFill, 8, DesignerGUI.Tooltips.SearchPrevious) || Event.current.OnKeyDown(KeyCode.UpArrow) || (Event.current.shift && Event.current.OnKeyDown(KeyCode.Return)) || (Event.current.control && Event.current.OnKeyDown(KeyCode.K)) || (Event.current.control && Event.current.OnKeyDown(KeyCode.P)))
					{
						if (Context.SelectedFilteredNodeIndex == 0)
						{
							return;
						}
						Context.SelectedFilteredNodeIndex--;
						ScrollToSearchedNode(slots, viewport);
					}
					if (DesignerGUI.DrawSimpleIconButton(navRect.Split(1, 2), SdfIconType.CaretDownFill, 8, DesignerGUI.Tooltips.SearchNext) || Event.current.OnKeyDown(KeyCode.DownArrow) || Event.current.OnKeyDown(KeyCode.Return) || (Event.current.control && Event.current.OnKeyDown(KeyCode.J)) || (Event.current.control && Event.current.OnKeyDown(KeyCode.N)))
					{
						if (Context.SelectedFilteredNodeIndex == Context.FilteredNodes.Count - 1)
						{
							return;
						}
						Context.SelectedFilteredNodeIndex++;
						ScrollToSearchedNode(slots, viewport);
					}
				}
				Context.SearchTerm = DesignerGUI.DrawSearch(SearchRect, Context.SearchTerm, SearchField, out var isSearchChanged);
				if (isSearchChanged)
				{
					Context.IsSearching = !Context.SearchTerm.IsNullOrWhitespace();
					Context.RebuildFilteredNodes(RootNode);
					ScrollToSearchedNode(slots, viewport);
				}
			}
			if (Context.DragNode != null)
			{
				EditorGUIUtility.AddCursorRect(rect, MouseCursor.MoveArrow);
				GUI.Label(rect, Context.DragNode.Path);
			}
			HandleContextInteractions(rect, callingWindow);
			if (Event.current.OnKeyDown(KeyCode.Escape))
			{
				Context.Select(null);
			}
			IsEditorRendering = false;
		}

		private void ScrollToSearchedNode(List<Slot> slots, Rect viewport)
		{
			if (Context.SelectedFilteredNodeIndex <= Context.FilteredNodes.Count - 1)
			{
				DesignerEditorNode node = Context.FilteredNodes[Context.SelectedFilteredNodeIndex];
				Context.Select(node);
				Slot slotToScrollTo = slots.FirstOrDefault((Slot s) => s.Node.PropertyName == node.PropertyName);
				if (slotToScrollTo != null)
				{
					ScrollView.ScrollTo(slotToScrollTo.Index);
				}
			}
		}

		public DesignerEditorWindow OpenWindow()
		{
			if (Window != null)
			{
				Window.SetupWindow(this);
				Window.Focus();
				return Window;
			}
			DesignerEditorWindow wnd = ScriptableObject.CreateInstance<DesignerEditorWindow>();
			wnd.hideFlags = HideFlags.HideAndDontSave;
			wnd.SetupWindow(this);
			wnd.Show();
			EditorWindow_Internal.DontSaveToLayout(wnd);
			Window = wnd;
			return wnd;
		}

		public void OpenPopup(Rect contextRect, DesignerEditorNode node)
		{
			if (node != null)
			{
				GUIHelper.RemoveFocusControl();
				Context.Select(node);
				Sync(updateSelectionAttributes: true);
				DesignerAttributePopup popup = DesignerAttributePopup.Open(contextRect, this);
				Popup = popup;
			}
		}

		public void SaveChanges(bool isTriggeredByAutoSave, OdinEditorWindow toastWindow)
		{
			TypePatch.SaveChanges(isTriggeredByAutoSave, toastWindow);
		}

		public void Destroy()
		{
		}

		public void SyncIfNeeded()
		{
			if (SyncTag != TypePatch.SyncTag || ForceSync)
			{
				Sync(updateSelectionAttributes: true);
			}
		}

		public void SyncDependentEditors()
		{
			List<DesignerEditor> snapshot = DesignerEditors.SnapshotCurrentEditorsTmp();
			foreach (DesignerEditor editor in snapshot)
			{
				if (editor.TypePatch.InheritsFrom(TypePatch))
				{
					editor.ForceSync = true;
				}
			}
		}

		public void Sync(bool updateSelectionAttributes)
		{
			SyncException = null;
			_ = RootNode;
			RootNode?.Children.Clear();
			if (TypePatch == null)
			{
				return;
			}
			GroupHandles.Clear();
			PropertyHandles.Clear();
			for (int i = 0; i < TypePatch.GroupPatches.Count; i++)
			{
				GroupPatch groupPatch = TypePatch.GroupPatches[i];
				GroupHandles[groupPatch.Id] = groupPatch;
			}
			for (int j = 0; j < TypePatch.PropertyPatches.Count; j++)
			{
				PropertyPatch propertyPatch = TypePatch.PropertyPatches[j];
				PropertyHandles[propertyPatch.Name] = propertyPatch;
			}
			DesignerPatcher.Reset(null);
			NodesInUse.Clear();
			FinalizedInspectorInfoCache.Clear();
			Type targetType = TypePatch.TargetType;
			PropertyTree tree = null;
			try
			{
				InspectorProperty rootProperty;
				if (Instance == null)
				{
					Type closedTargetType = DesignerUtils.GetClosedVariant(targetType);
					DesignerPropertyTree designerTree;
					tree = (designerTree = new DesignerPropertyTree(targetType, closedTargetType));
					IsGenericClosedByDesigner = designerTree.HasGenericBeenClosedByDesigner;
					tree.SetupForDesignerEditor();
					rootProperty = designerTree.RootProperty;
				}
				else
				{
					tree = PropertyTree.Create(Instance);
					IsGenericClosedByDesigner = false;
					tree.SetupForDesignerEditor();
					rootProperty = tree.RootProperty;
					if (!string.IsNullOrEmpty(InstancePath))
					{
						rootProperty = rootProperty.FindChild((InspectorProperty property) => property.Path == InstancePath, includeSelf: true);
					}
				}
				Type resolverType = rootProperty.ChildResolver?.GetType();
				if (resolverType != null)
				{
					if (IsGenericClosedByDesigner && resolverType.IsGenericType)
					{
						PropertyResolverType = resolverType.GetGenericTypeDefinition();
					}
					else
					{
						PropertyResolverType = resolverType;
					}
				}
				else
				{
					PropertyResolverType = null;
				}
				if (RootNode == null)
				{
					RootNode = new DesignerEditorNode(DesignerEditorNodeType.Root);
				}
				RootNode.InitializeRoot(rootProperty, TypePatch.TargetType);
				HasValidPropertyResolver = DesignerUtils.IsPropertyResolverSupported(rootProperty.ChildResolver);
				if (HasValidPropertyResolver)
				{
					if (RootNode.Children == null)
					{
						RootNode.Children = new List<DesignerEditorNode>(DesignerUtils.Round8(rootProperty.Children.Count));
					}
					else if (RootNode.Children.Capacity < rootProperty.Children.Count)
					{
						RootNode.Children.Capacity = DesignerUtils.Round8(rootProperty.Children.Count);
					}
					for (int i2 = 0; i2 < rootProperty.Children.Count; i2++)
					{
						PropertyToNode(rootProperty.Children[i2], RootNode, i2, targetType);
					}
				}
				else if (RootNode.Children == null)
				{
					RootNode.Children = new List<DesignerEditorNode>();
				}
				if (rootProperty.ChildResolver is ICollectionResolver collectionResolver)
				{
					ElementType = collectionResolver.ElementType;
				}
				else
				{
					ElementType = null;
				}
			}
			catch (Exception ex)
			{
				SyncException = "<b>" + ex?.GetType().GetNiceName() + "</b>: " + ex?.Message + "\n\n" + ex.StackTrace;
			}
			if (tree != null)
			{
				tree.Dispose();
				tree = null;
			}
			int flatIndex = 0;
			RootNode?.SetupFlatIndicesRecursive(ref flatIndex);
			SyncTag = TypePatch.SyncTag;
			ForceSync = false;
			Context.AfterSync();
			SyncDependentEditors();
			DesignerUtils.RefreshInspectorAndEditors();
		}

		public DesignerEditor(EditorKey key)
		{
			Key = key;
		}

		private void PropertyToNode(InspectorProperty property, DesignerEditorNode parent, int index, Type targetType)
		{
			if (property.ParentValueProperty.ChildResolver.IsCollection)
			{
				return;
			}
			if (property.Info.PropertyType == PropertyType.Group)
			{
				DesignerEditorNode group = GetNode(property);
				group.InitializeGroup(property, parent);
				group.NodeColor = Color.white;
				NodesInUse.Add(group);
				string designerId = property.Info.DesignerId;
				if (string.IsNullOrEmpty(designerId))
				{
					designerId = group.GroupAttribute.GroupID;
				}
				GroupHandles.TryGetValue(designerId, out group.GroupPatch);
				group.SerializedName = group.GetDesignerId();
				for (int i = 0; i < property.Children.Count; i++)
				{
					PropertyToNode(property.Children[i], group, i, targetType);
				}
				return;
			}
			DesignerEditorNode member = GetNode(property);
			member.Attributes?.Clear();
			MemberInfo memberInfo = property.Info.GetMemberInfo();
			if (IsGenericClosedByDesigner && memberInfo != null)
			{
				MemberInfo replacement = memberInfo.MemberType switch
				{
					MemberTypes.Field => targetType.GetField(memberInfo.Name, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic), 
					MemberTypes.Property => targetType.GetProperty(memberInfo.Name, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic), 
					MemberTypes.Method => (!(memberInfo is MethodInfo method)) ? null : DesignerUtils.GetOpenGenericVariantOrSelf(method), 
					_ => null, 
				};
				if (replacement != null)
				{
					memberInfo = replacement;
				}
			}
			Type declType = ((memberInfo != null) ? DesignerUtils.GetDeclType(memberInfo) : TypePatch.TargetType);
			Color memberColor = DesignerUtils.GetDeclTypeColor(TypePatch.TargetType, declType);
			member.InitializeMember(property, parent, memberInfo, memberColor);
			member.DeclaringType = declType;
			if (memberInfo != null)
			{
				if (memberInfo.DeclaringType == typeof(ValueType))
				{
					member.IsExcluded = true;
				}
				else
				{
					MemberTypes memberType = memberInfo.MemberType;
					if (memberType == MemberTypes.Method)
					{
						MethodInfo methodInfo = (MethodInfo)memberInfo;
						if (methodInfo.IsSpecialName)
						{
							member.IsExcluded = true;
						}
					}
				}
			}
			NodesInUse.Add(member);
			if (PropertyHandles.TryGetValue(member.SerializedName, out var propertyPatch))
			{
				member.PropertyPatch = propertyPatch;
			}
			else
			{
				member.PropertyPatch = null;
			}
		}

		private DesignerEditorNode GetNode(InspectorProperty property)
		{
			DesignerEditorNode result;
			if (property.Info.PropertyType == PropertyType.Group)
			{
				string id = ((!string.IsNullOrEmpty(property.Info.DesignerId)) ? property.Info.DesignerId : property.Path);
				if (!Nodes.TryGetValue(id, out result))
				{
					DesignerEditorNode designerEditorNode = (Nodes[id] = new DesignerEditorNode(DesignerEditorNodeType.Group));
					result = designerEditorNode;
				}
				result.Children?.Clear();
				return result;
			}
			if (!Nodes.TryGetValue(property.Name, out result))
			{
				DesignerEditorNode designerEditorNode = (Nodes[property.Name] = new DesignerEditorNode(DesignerEditorNodeType.Member));
				result = designerEditorNode;
			}
			return result;
		}

		public DesignerEditorNode GetOrMakeGroup(GroupPatch column)
		{
			if (Nodes.TryGetValue(column.Id, out var node))
			{
				node.GroupPatch = column;
				return node;
			}
			node = new DesignerEditorNode(DesignerEditorNodeType.Group);
			node.GroupPatch = column;
			return Nodes[column.Id] = node;
		}

		public void HandleContextInteractions(Rect rect, OdinEditorWindow callingWindow)
		{
			if (!Event.current.OnMouseDown(rect, 1))
			{
				return;
			}
			GenericMenu contextMenu = new GenericMenu();
			if (TypePatch.IsDirty)
			{
				contextMenu.AddItem(new GUIContent("Save Changes"), on: false, delegate
				{
					SaveChanges(isTriggeredByAutoSave: false, callingWindow);
				});
			}
			else
			{
				contextMenu.AddDisabledItem(new GUIContent("Save Changes"));
			}
			contextMenu.AddItem(new GUIContent("Revert Changes Since Last Save"), on: false, delegate
			{
				Context.BeginUndo();
				TypePatch.Initialize(TypePatchCache.Get(TypePatch.TargetType));
				DesignerPatcher.Reset(null);
				Context.EndUndo(isEditorOutOfSync: true);
				TypePatch.ClearDirty();
			});
			if (TypePatch.HasChanges())
			{
				contextMenu.AddItem(new GUIContent("Reset"), on: false, delegate
				{
					Context.BeginUndo();
					TypePatch.SelfPatches.Clear();
					TypePatch.GroupPatches.Clear();
					TypePatch.PropertyPatches.Clear();
					Context.EndUndo(isEditorOutOfSync: true);
				});
			}
			else
			{
				contextMenu.AddDisabledItem(new GUIContent("Reset"));
			}
			contextMenu.ShowAsContext();
		}
	}
}
