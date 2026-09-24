using System;
using Clipboard = Sirenix.Utilities.Editor.Clipboard;
using System.Collections.Generic;
using System.Reflection;
using Sirenix.OdinInspector.Editor.Examples;
using Sirenix.OdinInspector.Internal;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Internal
{
	internal class DesignerEditorContext
	{
		public DesignerEditor Editor;

		public MemberTypes MemberTypeToHighlight;

		public DesignerEditorNode SelectedNode;

		public bool IsNonDeclMembersLocked;

		public DesignerEditorNode HoverNode;

		public DesignerEditorNode PotentionalDragNode;

		public DesignerEditorNode DragNode;

		public DesignerEditorNode DropNode;

		public Vector2 NodeDragStart;

		public DropDirection DropDirection;

		public DesignerSelection Selection;

		public PropertyTree SelectionTree;

		public Vector2 scrollPosition = Vector2.zero;

		public string SearchTerm;

		public bool IsSearching;

		public List<DesignerEditorNode> FilteredNodes = new List<DesignerEditorNode>();

		public int SelectedFilteredNodeIndex;

		public bool IsShowHideMode;

		private GUIStyle _label;

		public EditorTypePatch EditorPatch => Editor.TypePatch;

		public DesignerEditorWindow EditorWindow => Editor.Window;

		public bool IsDragging => DragNode != null;

		public DesignerEditorNode SelectedFilteredNode
		{
			get
			{
				if (SelectedFilteredNodeIndex >= 0 && SelectedFilteredNodeIndex <= FilteredNodes.Count - 1)
				{
					return FilteredNodes[SelectedFilteredNodeIndex];
				}
				return null;
			}
		}

		private GUIStyle Label
		{
			get
			{
				GUIStyle obj = _label ?? new GUIStyle(SirenixGUIStyles.RichTextLabel)
				{
					wordWrap = false
				};
				GUIStyle result = obj;
				_label = obj;
				return result;
			}
		}

		public bool IsDropNode(DesignerEditorNode node)
		{
			if (IsDragging && DropDirection != DropDirection.None)
			{
				return node == HoverNode;
			}
			return false;
		}

		public DesignerEditorContext(DesignerEditor editor)
		{
			Editor = editor;
			Selection = new DesignerSelection(editor, null);
			SelectionTree = PropertyTree.Create(Selection);
			CleanupUtility.DisposeObjectOnAssemblyReload(SelectionTree);
			IsNonDeclMembersLocked = GlobalConfig<OdinVisualDesignerConfig>.Instance.LockNonDeclTypesByDefault;
			SelectionTree.OnPropertyValueChanged += delegate(InspectorProperty property, int index)
			{
				if (SelectedNode == null)
				{
					throw new NotImplementedException();
				}
				Type typeOfOwner = property.Info.TypeOfOwner;
				bool flag = typeof(PropertyOrderAttribute).IsAssignableFrom(typeOfOwner);
				if (typeof(PropertyGroupAttribute).IsAssignableFrom(typeOfOwner) && property.Name == "GroupName")
				{
					flag = true;
				}
				DesignerEditorNode selectedNode = SelectedNode;
				BeginUndo();
				if (selectedNode.NodeType == DesignerEditorNodeType.Member)
				{
					if (selectedNode.PropertyPatch == null)
					{
						selectedNode.PropertyPatch = CreateProperty(selectedNode.SerializedName);
					}
					PropertyPatch propertyPatch = selectedNode.PropertyPatch;
					DesignerPatchUtils.AddAttributeDeltaChange(property, ref propertyPatch.AttributePatches);
				}
				else if (selectedNode.NodeType == DesignerEditorNodeType.Root)
				{
					DesignerPatchUtils.AddAttributeDeltaChange(property, ref EditorPatch.SelfPatches);
				}
				else
				{
					if (selectedNode.GroupPatch == null)
					{
						selectedNode.GroupPatch = CreateGroup(selectedNode);
					}
					selectedNode.GroupPatch.AddAttributeDeltaChange(property);
				}
				EndUndo(flag);
				if (flag)
				{
					Editor.Sync(updateSelectionAttributes: false);
				}
				else
				{
					Selection.UpdatePatchChangesHashset(this, selectedNode);
					Editor.SyncDependentEditors();
				}
				DesignerUtils.RefreshInspectorAndEditors();
			};
		}

		public void Select(DesignerEditorNode node)
		{
			SelectedNode = node;
			Selection.Update(this, node);
			SelectionTree?.RootProperty.RefreshSetup();
		}

		public void AfterLayout()
		{
			HoverNode = null;
		}

		public void AfterSync()
		{
			if (SelectedNode != null)
			{
				if (Selection.Id == "$self")
				{
					SelectedNode = Editor.RootNode;
				}
				else if (!Editor.NodesInUse.Contains(SelectedNode))
				{
					SelectedNode = null;
				}
				Select(SelectedNode);
			}
		}

		public void BeginUndo()
		{
			EditorPatch.BeginUndo();
		}

		public void EndUndo(bool isEditorOutOfSync)
		{
			EditorPatch.EndUndo();
			if (!isEditorOutOfSync)
			{
				Editor.SyncTag = EditorPatch.SyncTag;
				DesignerPatcher.Reset(null);
				DesignerUtils.RefreshInspectorAndEditors();
			}
		}

		public PropertyPatch CreateProperty(string serializedName)
		{
			return EditorPatch.CreatePropertyPatch(serializedName);
		}

		public GroupPatch CreateGroup(DesignerEditorNode node)
		{
			GroupPatch result = EditorPatch.CreateGroupPatch(node.GetDesignerId());
			result.GroupAttributePatch.AttributeType = node.GroupAttribute.GetType();
			result.GroupAttributePatch.PatchType = AttributePatchType.Modify;
			result.IsAddedByDesigner = node.IsGroupAddedByDesigner;
			return result;
		}

		public GroupPatch CreateGroup(Type type)
		{
			GroupPatch result = EditorPatch.CreateGroupPatch(DesignerIds.Generate());
			result.GroupAttributePatch.AttributeType = type;
			result.GroupAttributePatch.PatchType = AttributePatchType.Add;
			result.IsAddedByDesigner = true;
			return result;
		}

		public ref RefList<AttributePatch> GetAttributePatchesForSelection()
		{
			if (SelectedNode.NodeType == DesignerEditorNodeType.Root)
			{
				return ref EditorPatch.SelfPatches;
			}
			PropertyPatch propertyPatch = SelectedNode.PropertyPatch;
			if (propertyPatch == null)
			{
				propertyPatch = CreateProperty(SelectedNode.SerializedName);
			}
			return ref propertyPatch.AttributePatches;
		}

		public void AddAttributeToSelection(Type attributeType)
		{
			if (SelectedNode != null)
			{
				BeginUndo();
				DesignerPatchUtils.AddAttribute(attributeType, ref GetAttributePatchesForSelection());
				EndUndo(isEditorOutOfSync: true);
				Editor.SyncIfNeeded();
			}
		}

		public void RemoveAttributeFromSelection(Type attributeType)
		{
			DesignerEditorNode selected = SelectedNode;
			if (selected == null)
			{
				return;
			}
			bool hasAttribute = false;
			for (int i = 0; i < selected.Attributes.Count; i++)
			{
				if (selected.Attributes[i].GetType() == attributeType)
				{
					hasAttribute = true;
					break;
				}
			}
			if (hasAttribute)
			{
				BeginUndo();
				DesignerPatchUtils.RemoveAttribute(attributeType, ref GetAttributePatchesForSelection());
				EndUndo(isEditorOutOfSync: true);
				Editor.SyncIfNeeded();
			}
		}

		public void PasteAttributeToSelection(Attribute attribute)
		{
			if (attribute != null && !(attribute is PropertyGroupAttribute) && DesignerRegistry.IsValidAttribute(attribute))
			{
				Type attributeType = attribute.GetType();
				BeginUndo();
				ref RefList<AttributePatch> attributePatches = ref GetAttributePatchesForSelection();
				DesignerPatchUtils.AddAttribute(attributeType, ref attributePatches);
				DesignerPatchUtils.TransferAttributeDeltas(attribute, ref attributePatches);
				EndUndo(isEditorOutOfSync: true);
				Editor.SyncIfNeeded();
			}
		}

		public DropDirection DetermineDropDirection()
		{
			Event e = Event.current;
			DesignerEditorNode hoverNode = HoverNode;
			Rect dropRect = hoverNode.Rect;
			float sideFactor = ((hoverNode.NodeType == DesignerEditorNodeType.Member) ? 0.15f : 0.1f);
			if (hoverNode.NodeType == DesignerEditorNodeType.Group && hoverNode.Children.Count == 0)
			{
				e.IsHovering(hoverNode.Rect.Padding(14f));
			}
			if (e.IsHovering(dropRect.AlignLeft(dropRect.width * sideFactor)))
			{
				return DropDirection.Left;
			}
			if (e.IsHovering(dropRect.AlignRight(dropRect.width * sideFactor)))
			{
				return DropDirection.Right;
			}
			if (e.IsHovering(dropRect.AlignTop(dropRect.height * 0.8f)))
			{
				return DropDirection.Top;
			}
			return DropDirection.Bottom;
		}

		public void HandleDrop()
		{
			if (DragNode == null)
			{
				return;
			}
			DragNode.DrawDragPreview(this);
			if (HoverNode == null || HoverNode == DragNode)
			{
				return;
			}
			DesignerEditorNode drop = DragAndDropUtilities.DropZone<DesignerEditorNode>(HoverNode.Rect, null);
			if (drop == null)
			{
				return;
			}
			DropDirection dropDirection = DetermineDropDirection();
			DesignerEditorNode hover = HoverNode;
			BeginUndo();
			string id = ((dropDirection == DropDirection.Center) ? hover : hover.Parent).GetDesignerId();
			if (dropDirection == DropDirection.Left || dropDirection == DropDirection.Right)
			{
				GroupPatch row = CreateGroup(typeof(ColumnGroupAttribute));
				GroupPatch left = CreateGroup(typeof(ColumnGroupAttribute.ColumnSubGroupAttribute));
				GroupPatch right = CreateGroup(typeof(ColumnGroupAttribute.ColumnSubGroupAttribute));
				row.ParentId = id;
				row.DesiredIndex = hover.GetVisualIndex();
				left.ParentId = row.Id;
				left.DesiredIndex = 0;
				right.ParentId = row.Id;
				right.DesiredIndex = 1;
				if (dropDirection == DropDirection.Left)
				{
					MoveNode(drop, left.Id, 0);
					MoveNode(hover, right.Id, 0);
				}
				else
				{
					MoveNode(hover, left.Id, 0);
					MoveNode(drop, right.Id, 0);
				}
			}
			else
			{
				int desiredIndex = hover.GetVisualIndex();
				if (dropDirection == DropDirection.Bottom)
				{
					desiredIndex++;
				}
				if (dropDirection == DropDirection.Center)
				{
					desiredIndex = 0;
				}
				MoveNode(drop, id, desiredIndex);
			}
			int index = hover.GetIndex();
			if (dropDirection == DropDirection.Bottom)
			{
				index++;
			}
			for (; index < hover.Parent.Children.Count; index++)
			{
				DesignerEditorNode node = hover.Parent.Children[index];
				if (node != drop)
				{
					node.ResetDesiredIndex();
				}
			}
			EndUndo(isEditorOutOfSync: true);
		}

		public void DrawAttributes(Rect rect)
		{
			if (Editor.Window == null && Editor.Popup != null)
			{
				Editor.SyncIfNeeded();
			}
			Event e = Event.current;
			if (SelectedNode == null)
			{
				return;
			}
			bool isSelectedParentSubGroupOwner = SelectedNode.Parent?.GroupAttribute is ISubGroupProviderAttribute;
			scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
			Rect measureRect = EditorGUILayout.BeginVertical();
			if (SelectionTree == null)
			{
				return;
			}
			SelectionTree.BeginDraw(withUndo: false);
			InspectorProperty attributes = SelectionTree.RootProperty.Children["Attributes"];
			if (attributes.Children.Count == 0)
			{
				GUIHelper.PushGUIEnabled(enabled: true);
				DrawEmptyAttributeListMessage(rect);
				GUIHelper.PopGUIEnabled();
			}
			HashSet<Type> attributesFromCode = Selection?.AttributesFromCode;
			bool holdingCtrl = ((UnityShims.EventModifiers)UnityShims.Misc.GetEventModifiers(e)).HasFlag(UnityShims.EventModifiers.Control);
			foreach (InspectorProperty attribute in attributes.Children)
			{
				bool hasChildren = attribute.Children.Count > 0;
				Rect headerRect = GUILayoutUtility.GetRect(0f, 22f, GUILayoutOptions.ExpandWidth().ExpandHeight(expand: false));
				Rect interactRect = headerRect;
				Color headerBgColor = ((Event.current.IsHovering(headerRect) || (holdingCtrl && hasChildren)) ? Colors.AttributePopup.HeaderBgHover : Colors.AttributePopup.HeaderBg);
				EditorGUI.DrawRect(headerRect, headerBgColor);
				EditorGUI.DrawRect(headerRect.AlignTop(1f).SubY(1f), Colors.AttributePopup.Border);
				EditorGUI.DrawRect(headerRect.AlignBottom(1f).AddY(1f), Colors.AttributePopup.LightBorder);
				Rect iconRect = headerRect.TakeFromLeft(headerRect.height);
				bool expanded = attribute.State.Expanded;
				SdfIconType icon = ((!hasChildren) ? SdfIconType.TagFill : (expanded ? SdfIconType.CaretDownFill : SdfIconType.CaretRightFill));
				SdfIcons.DrawIcon(iconRect.Padding(6f), icon, Colors.Icons.CaretAndTag);
				Type attributeType = attribute.ValueEntry.TypeOfValue;
				bool hasAttributeOverview = AttributeExampleUtilities.HasExample(attributeType);
				string name = ObjectNames.NicifyVariableName(attributeType.GetNiceName());
				GUI.Label(headerRect, name, SirenixGUIStyles.Label);
				Rect contextMenuIconRect = headerRect.TakeFromRight(headerRect.height);
				SdfIcons.DrawIcon(contextMenuIconRect.Padding(7f), SdfIconType.ThreeDotsVertical);
				GUI.Label(contextMenuIconRect, new GUIContent("", DesignerGUI.Tooltips.AttributeContextMenu), GUIStyle.none);
				Rect attributeOverviewIconRect = Rect.zero;
				if (hasAttributeOverview)
				{
					attributeOverviewIconRect = headerRect.TakeFromRight(headerRect.height);
					if (DesignerGUI.DrawSimpleIconButton(attributeOverviewIconRect, SdfIconType.Info, 7, DesignerGUI.Tooltips.AttributeOverview))
					{
						OpenAttributeOverview(attributeType);
					}
				}
				if (attributesFromCode != null && attributesFromCode.Contains(attributeType))
				{
					GUI.Label(headerRect.AlignRight(headerRect.height), "C#", SirenixGUIStyles.MiniLabelCentered);
				}
				if ((e.OnMouseDown(interactRect, 1) || GUI.Button(contextMenuIconRect, GUIContent.none, GUIStyle.none)) && !typeof(PropertyGroupAttribute).IsAssignableFrom(attribute.ValueEntry.TypeOfValue))
				{
					GenericMenu genericMenu = new GenericMenu();
					genericMenu.AddItem(new GUIContent("Copy Attribute"), on: false, delegate
					{
						Clipboard.Copy(attribute.ValueEntry.WeakSmartValue, CopyModes.DeepCopy);
					});
					Rect overviewAnchorRect = GetAttributeOverviewAnchorRect();
					if (hasAttributeOverview)
					{
						genericMenu.AddItem(new GUIContent("Open Attribute Overview"), on: false, delegate
						{
							OpenAttributeOverview(overviewAnchorRect, attributeType);
						});
					}
					else
					{
						genericMenu.AddDisabledItem(new GUIContent("Open Attribute Overview"));
					}
					genericMenu.AddSeparator("");
					genericMenu.AddItem(new GUIContent("Remove Attribute"), on: false, delegate
					{
						RemoveAttributeFromSelection(attribute.ValueEntry.TypeOfValue);
					});
					genericMenu.ShowAsContext();
				}
				if (!hasChildren)
				{
					continue;
				}
				if (e.OnMouseDown(interactRect, 0))
				{
					if (holdingCtrl)
					{
						bool newState = !attribute.State.Expanded;
						foreach (InspectorProperty child in attributes.Children)
						{
							child.State.Expanded = newState;
						}
					}
					else
					{
						attribute.State.Expanded = !attribute.State.Expanded;
					}
				}
				if (SirenixEditorGUI.BeginFadeGroup(attribute, attribute.State.Expanded))
				{
					GUILayout.Space(10f);
					foreach (InspectorProperty child2 in attribute.Children)
					{
						DrawAttributeProperty(child2, isSelectedParentSubGroupOwner, measureRect);
					}
					GUILayout.Space(10f);
				}
				SirenixEditorGUI.EndFadeGroup();
			}
			SelectionTree.EndDraw();
			EditorGUILayout.EndVertical();
			EditorGUILayout.EndScrollView();
			GUI.DrawTexture(new Rect(0f, 0f, rect.width, 10f), DesignerTextures.TopToBottomFade, ScaleMode.StretchToFill, alphaBlend: true, 1f, new Color(0f, 0f, 0f, Mathf.InverseLerp(0f, 30f, scrollPosition.y) * 0.6f), 0f, 0f);
			if (measureRect.height > rect.height)
			{
				float pos = scrollPosition.y + rect.height;
				GUI.DrawTexture(new Rect(0f, rect.height - 10f, rect.width, 10f), DesignerTextures.BottomToTopFade, ScaleMode.StretchToFill, alphaBlend: true, 1f, new Color(0f, 0f, 0f, Mathf.InverseLerp(measureRect.height, measureRect.height - 30f, pos) * 0.6f), 0f, 0f);
			}
		}

		private void DrawAttributeProperty(InspectorProperty property, bool isSelectedParentSubGroupOwner, Rect measureRect)
		{
			if (CanDrawAttributeProperty(property, isSelectedParentSubGroupOwner))
			{
				EditorGUILayout.BeginHorizontal();
				GUILayout.Space(10f);
				EditorGUILayout.BeginVertical();
				DrawAttributePropertyContent(property, isSelectedParentSubGroupOwner, measureRect);
				EditorGUILayout.EndVertical();
				GUILayout.Space(10f);
				EditorGUILayout.EndHorizontal();
			}
		}

		private void DrawAttributePropertyContent(InspectorProperty property, bool isSelectedParentSubGroupOwner, Rect measureRect)
		{
			if (!CanDrawAttributeProperty(property, isSelectedParentSubGroupOwner))
			{
				return;
			}
			if (property.Info.PropertyType == PropertyType.Group)
			{
				DrawAttributePropertyGroupHeader(property);
				{
					foreach (InspectorProperty child in property.Children)
					{
						DrawAttributePropertyContent(child, isSelectedParentSubGroupOwner, measureRect);
					}
					return;
				}
			}
			Rect prefabMod = EditorGUILayout.BeginVertical();
			bool isChanged = Selection.IsPropertyChanged(property);
			if (isChanged)
			{
				GUIHelper.PushIsBoldLabel(isBold: true);
			}
			property.Draw();
			if (isChanged)
			{
				GUIHelper.PushIsBoldLabel(isBold: false);
				EditorGUI.DrawRect(prefabMod.AlignLeft(2f).SetX(measureRect.x), Color.cyan);
			}
			EditorGUILayout.EndVertical();
		}

		private void DrawAttributePropertyGroupHeader(InspectorProperty property)
		{
			TitleGroupAttribute titleGroup = property.GetAttribute<TitleGroupAttribute>();
			if (titleGroup != null)
			{
				EditorGUILayout.Space();
				SirenixEditorGUI.Title(titleGroup.GroupName, titleGroup.Subtitle, (TextAlignment)titleGroup.Alignment, titleGroup.HorizontalLine, titleGroup.BoldTitle);
			}
		}

		private bool CanDrawAttributeProperty(InspectorProperty property, bool isSelectedParentSubGroupOwner)
		{
			if (property.Info.PropertyType == PropertyType.Group)
			{
				foreach (InspectorProperty child in property.Children)
				{
					if (CanDrawAttributeProperty(child, isSelectedParentSubGroupOwner))
					{
						return true;
					}
				}
				return false;
			}
			if (!DesignerUtils.CanAttributePropertyBeModified(property))
			{
				return false;
			}
			if (isSelectedParentSubGroupOwner && property.Name != "GroupName" && property.Info.GetMemberInfo().DeclaringType == typeof(PropertyGroupAttribute))
			{
				return false;
			}
			return true;
		}

		public void RebuildFilteredNodes(DesignerEditorNode root)
		{
			FilteredNodes.Clear();
			SelectedFilteredNodeIndex = 0;
			if (root != null && !string.IsNullOrWhiteSpace(SearchTerm))
			{
				Visit(root);
			}
			void Visit(DesignerEditorNode n)
			{
				if (n != null && (n.IsShownInInspector || IsShowHideMode))
				{
					if (n.NodeType != DesignerEditorNodeType.Root && !n.IsRow && !n.IsColumn && FuzzySearch.Contains(SearchTerm, n.PropertyName))
					{
						FilteredNodes.Add(n);
					}
					if (n.Children != null)
					{
						foreach (DesignerEditorNode c in n.Children)
						{
							Visit(c);
						}
					}
				}
			}
		}

		private void OpenAttributeOverview(Type attributeType)
		{
			OpenAttributeOverview(GetAttributeOverviewAnchorRect(), attributeType);
		}

		private Rect GetAttributeOverviewAnchorRect()
		{
			if (Editor.Popup != null)
			{
				return Editor.Popup.position;
			}
			if (Editor.Window != null)
			{
				return Editor.Window.position;
			}
			EditorWindow currentWindow = GUIHelper.CurrentWindow;
			if (currentWindow != null)
			{
				return currentWindow.position;
			}
			return UnityShims.Rect.Ctor(GUIUtility.GUIToScreenPoint(Event.current.mousePosition), Vector2.zero);
		}

		private static void OpenAttributeOverview(Rect anchorRect, Type attributeType)
		{
			DesignerAttributeExampleWindow.Open(UnityShims.Rect.Ctor(anchorRect.position + new Vector2(anchorRect.width + 10f, 0f), Vector2.zero), attributeType);
		}

		public void AddGroup(Type groupType)
		{
			BeginUndo();
			GroupPatch group = EditorPatch.CreateGroupPatch(DesignerIds.Generate());
			group.IsAddedByDesigner = true;
			group.GroupAttributePatch.AttributeType = groupType;
			group.GroupAttributePatch.PatchType = AttributePatchType.Add;
			group.ParentId = string.Empty;
			EndUndo(isEditorOutOfSync: true);
		}

		public void MoveNodeNextTo(DesignerEditorNode node, DesignerEditorNode target, bool above)
		{
			DesignerEditorNode lastParent = node.Parent;
			string parentId = target.Parent.GetDesignerId();
			int desiredIndex = int.MaxValue;
			MoveNode(node, parentId, desiredIndex);
			node.Parent.Children.Remove(node);
			for (int i = 0; i < target.Parent.Children.Count; i++)
			{
				DesignerEditorNode child = target.Parent.Children[i];
				if (child == target)
				{
					if (above)
					{
						target.Parent.Children.Insert(i, node);
					}
					else
					{
						target.Parent.Children.Insert(i + 1, node);
					}
					break;
				}
			}
			CleanupEmptyRowOrColumnAndCollapseSingleColumns(lastParent);
			target.Parent.UpdatePositionOnChildrenIfNeeded();
			if (target.Parent != lastParent)
			{
				lastParent.UpdatePositionOnChildrenIfNeeded();
			}
		}

		public void MoveNode(DesignerEditorNode node, string parentId, int desiredIndex)
		{
			if (node.NodeType == DesignerEditorNodeType.Member)
			{
				PropertyPatch patch = (node.PropertyPatch = node.PropertyPatch ?? CreateProperty(node.SerializedName));
				patch.ParentId = parentId;
				patch.DesiredIndex = desiredIndex;
			}
			else
			{
				GroupPatch patch2 = (node.GroupPatch = node.GroupPatch ?? CreateGroup(node));
				patch2.ParentId = parentId;
				patch2.DesiredIndex = desiredIndex;
			}
		}

		public void SplitNode(DesignerEditorNode node, DesignerEditorNode nodeToSplit, bool splitLeft)
		{
			DesignerEditorNode lastParent = node.Parent;
			DesignerEditorNode parent = nodeToSplit.Parent;
			int index = nodeToSplit.GetIndex();
			if (parent.IsRow)
			{
				GroupPatch column = CreateGroup(typeof(ColumnGroupAttribute.ColumnSubGroupAttribute));
				column.ParentId = parent.GetDesignerId();
				column.DesiredIndex = 0;
				MoveNode(node, column.Id, 0);
				DesignerEditorNode group = Editor.GetOrMakeGroup(column);
				group.Parent = parent;
				parent.Children.Insert(splitLeft ? index : (index + 1), group);
				CleanupEmptyRowOrColumnAndCollapseSingleColumns(lastParent);
				parent.UpdatePositionOnChildrenIfNeeded();
				return;
			}
			GroupPatch row = CreateGroup(typeof(ColumnGroupAttribute));
			GroupPatch left = CreateGroup(typeof(ColumnGroupAttribute.ColumnSubGroupAttribute));
			GroupPatch right = CreateGroup(typeof(ColumnGroupAttribute.ColumnSubGroupAttribute));
			row.ParentId = parent.GetDesignerId();
			row.DesiredIndex = index;
			left.ParentId = row.Id;
			left.DesiredIndex = 0;
			right.ParentId = row.Id;
			right.DesiredIndex = 1;
			if (splitLeft)
			{
				MoveNode(node, left.Id, 0);
				MoveNode(nodeToSplit, right.Id, 0);
			}
			else
			{
				MoveNode(node, right.Id, 0);
				MoveNode(nodeToSplit, left.Id, 0);
			}
			CleanupEmptyRowOrColumnAndCollapseSingleColumns(lastParent);
		}

		public void CleanupEmptyRowOrColumnAndCollapseSingleColumns(DesignerEditorNode node)
		{
		}

		public void RemoveGroup(DesignerEditorNode node)
		{
			if (node.Parent.IsColumn || node.Parent.IsRow)
			{
				DesignerEditorNode parent = node.Parent;
				if (parent.Children.Count == 1)
				{
					RemoveGroup(parent);
				}
			}
			node.Parent?.Children.Remove(node);
			if (node.GroupPatch != null)
			{
				if (node.GroupPatch.GroupAttributePatch.PatchType == AttributePatchType.Add)
				{
					Editor.TypePatch.GroupPatches.Remove(node.GroupPatch);
					return;
				}
				node.GroupPatch.GroupAttributePatch.AttributeType = node.GroupAttribute.GetType();
				node.GroupPatch.GroupAttributePatch.PatchType = AttributePatchType.Remove;
				node.GroupPatch.GroupAttributePatch.MemberDeltas.Clear();
			}
			else
			{
				GroupPatch patch = EditorPatch.CreateGroupPatch(node.GetDesignerId());
				patch.GroupAttributePatch.AttributeType = node.GroupAttribute.GetType();
				patch.GroupAttributePatch.PatchType = AttributePatchType.Remove;
				patch.IsAddedByDesigner = node.IsGroupAddedByDesigner;
			}
		}

		public void RevertAttributeMemberChangeOnSelection(InspectorProperty property)
		{
			if (SelectedNode == null)
			{
				return;
			}
			InspectorProperty attribute = property.ParentValueProperty;
			if (SelectedNode.NodeType == DesignerEditorNodeType.Root)
			{
				RefList<AttributePatch> selfPatches = EditorPatch.SelfPatches;
				for (int i = 0; i < selfPatches.Length; i++)
				{
					ref AttributePatch attributePatch = ref selfPatches[i];
					if (attributePatch.AttributeType == attribute.ValueEntry.TypeOfValue)
					{
						BeginUndo();
						attributePatch.RemoveDeltas(property);
						if (attributePatch.PatchType == AttributePatchType.Modify && attributePatch.MemberDeltas.Length == 0)
						{
							selfPatches.RemoveAt(i);
						}
						EndUndo(isEditorOutOfSync: true);
						DesignerPatcher.Reset(null);
						break;
					}
				}
			}
			else if (SelectedNode.GroupPatch != null)
			{
				GroupPatch patch = SelectedNode.GroupPatch;
				BeginUndo();
				patch.GroupAttributePatch.RemoveDeltas(property);
				EndUndo(isEditorOutOfSync: true);
				DesignerPatcher.Reset(null);
			}
			else
			{
				if (SelectedNode.PropertyPatch == null)
				{
					return;
				}
				PropertyPatch patch2 = SelectedNode.PropertyPatch;
				for (int j = 0; j < patch2.AttributePatches.Length; j++)
				{
					ref AttributePatch attributePatch2 = ref patch2.AttributePatches[j];
					if (attributePatch2.AttributeType == attribute.ValueEntry.TypeOfValue)
					{
						BeginUndo();
						attributePatch2.RemoveDeltas(property);
						if (attributePatch2.PatchType == AttributePatchType.Modify && attributePatch2.MemberDeltas.Length == 0)
						{
							patch2.AttributePatches.RemoveAt(j);
						}
						EndUndo(isEditorOutOfSync: true);
						DesignerPatcher.Reset(null);
						break;
					}
				}
			}
		}

		private void DrawEmptyAttributeListMessage(Rect rect)
		{
			Rect row1 = rect.AlignTop(20f).AddY(20f);
			Rect row2 = rect.AlignTop(20f).AddY(60f);
			Rect row3 = rect.AlignTop(20f).AddY(80f);
			Rect row4 = rect.AlignTop(20f).AddY(100f);
			string msg1 = "No attributes yet. Add attributes to this field to:";
			string msg2 = "Control visibility & visuals <color=" + Colors.AttributePopup.HintText + ">(e.g., ShowIf, LabelText)</color>";
			string msg3 = "Add behaviour & utilities <color=" + Colors.AttributePopup.HintText + ">(e.g., Button, InlineEditor)</color>";
			string msg4 = "Apply validation <color=" + Colors.AttributePopup.HintText + ">(e.g., ValidateInput, Required)</color>";
			Rect msg1Rect = row1.AlignCenterX(Label.CalcWidth(msg1));
			Rect msg2Rect = row2.AlignCenterX(Label.CalcWidth(msg2) + 30f);
			Rect msg3Rect = row3.AlignCenterX(Label.CalcWidth(msg2) + 30f);
			Rect msg4Rect = row4.AlignCenterX(Label.CalcWidth(msg2) + 30f);
			GUI.Label(msg1Rect, msg1, Label);
			SdfIcons.DrawIcon(msg2Rect.TakeFromLeft(10f).Padding(2f), SdfIconType.Dot, Colors.Icons.Default);
			SdfIcons.DrawIcon(msg2Rect.TakeFromLeft(20f).Padding(4f), SdfIconType.EyeFill, Colors.Icons.Default);
			GUI.Label(msg2Rect, msg2, Label);
			SdfIcons.DrawIcon(msg3Rect.TakeFromLeft(10f).Padding(2f), SdfIconType.Dot, Colors.Icons.Default);
			SdfIcons.DrawIcon(msg3Rect.TakeFromLeft(20f).Padding(4f), SdfIconType.PuzzleFill, Colors.Icons.Default);
			GUI.Label(msg3Rect, msg3, Label);
			SdfIcons.DrawIcon(msg4Rect.TakeFromLeft(10f).Padding(2f), SdfIconType.Dot, Colors.Icons.Default);
			SdfIcons.DrawIcon(msg4Rect.TakeFromLeft(20f).Padding(4f), SdfIconType.ExclamationTriangleFill, Colors.Icons.Default);
			GUI.Label(msg4Rect, msg4, Label);
		}

		public void HandleMouseInput(RefList<DesignerLayoutNode> layout)
		{
			int dragId = GUIUtility.GetControlID("DesignerEditorDragId".GetHashCode(), FocusType.Passive);
			Event e = Event.current;
			if (e.type == EventType.Layout)
			{
				HoverNode = null;
				for (int i = layout.Length - 1; i >= 0; i--)
				{
					ref DesignerLayoutNode node = ref layout[i];
					if (node.EditorNode != null && e.IsHovering(node.InteractRect))
					{
						HoverNode = node.EditorNode;
						break;
					}
				}
			}
			switch (e.type)
			{
			case EventType.MouseDown:
				PotentionalDragNode = HoverNode;
				NodeDragStart = e.mousePosition;
				break;
			case EventType.MouseDrag:
				if (!IsDragging && GUIUtility.hotControl == 0)
				{
					Vector2 diff = new Vector2(Mathf.Abs(e.mousePosition.x - NodeDragStart.x), Mathf.Abs(e.mousePosition.y - NodeDragStart.y));
					if (diff.x > 50f || diff.y > 32f)
					{
						GUIUtility.hotControl = dragId;
						DragNode = PotentionalDragNode;
					}
				}
				else if (IsDragging)
				{
					DropDirection = ((HoverNode != null) ? DetermineDropDirection() : DropDirection.None);
				}
				break;
			case EventType.MouseUp:
				if (GUIUtility.hotControl == dragId)
				{
					PotentionalDragNode = null;
					NodeDragStart = Vector2.zero;
					DragNode = null;
				}
				break;
			case EventType.MouseMove:
				break;
			}
		}
	}
}
