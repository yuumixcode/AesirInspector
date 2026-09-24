using System;
using System.Collections.Generic;
using System.Reflection;
using Sirenix.OdinInspector.Internal;
using Sirenix.Utilities;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Internal
{
	internal class DesignerEditorNode
	{
		public const float GROUP_HEADER = 28f;

		public const float GROUP_CONTENT_MIN = 30f;

		public const float COLUMN_CONTENT_MIN = 58f;

		public const int GROUP_PADDING = 8;

		public const float NODE_SPACE = 8f;

		public const float NODE_SPACE_HALF = 4f;

		public const float PROPERTY_HEIGHT = 30f;

		public const float COLUMN_SIZE_FIELD_HEIGHT = 20f;

		public int ControlId;

		public string Text;

		public DesignerEditorNodeType NodeType;

		public DesignerEditorNode Parent;

		public string Path;

		public string Label;

		public string PropertyName;

		public string SerializedName;

		public bool IsExcluded;

		public bool IsShownInInspector;

		public Rect Rect;

		public Rect DrawRect;

		public List<DesignerEditorNode> Children;

		public Color NodeColor;

		public int FlatIndex;

		public HashSet<Type> CodeAttributes;

		public GroupPatch GroupPatch;

		public PropertyGroupAttribute GroupAttribute;

		public bool IsRow;

		public bool IsColumn;

		public bool IsGroupAddedByDesigner;

		public PropertyPatch PropertyPatch;

		public MemberInfo Member;

		public Type DeclaringType;

		public string PropertyTypeLabel;

		public Type PropertyValueType;

		public Type PropertyElementType;

		public MemberTypes MemberType;

		public object PropertyInstance;

		public List<Attribute> Attributes;

		public float LayoutHeight => Rect.height;

		public DesignerEditorNode(DesignerEditorNodeType nodeType)
		{
			NodeType = nodeType;
		}

		public string GetDesignerId()
		{
			switch (NodeType)
			{
			case DesignerEditorNodeType.Root:
				return string.Empty;
			case DesignerEditorNodeType.Group:
				if (GroupPatch != null)
				{
					return GroupPatch.Id;
				}
				return GroupAttribute.GroupID;
			case DesignerEditorNodeType.Member:
				return PropertyName;
			default:
				return null;
			}
		}

		public int GetVisualIndex()
		{
			if (Parent == null)
			{
				return -1;
			}
			return Parent.Children.IndexOf(this);
		}

		public int GetIndex()
		{
			if (Parent == null)
			{
				return -1;
			}
			for (int i = 0; i < Parent.Children.Count; i++)
			{
				DesignerEditorNode current = Parent.Children[i];
				if (current == this)
				{
					return i;
				}
			}
			return -1;
		}

		public void InitializeRoot(InspectorProperty property, Type type)
		{
			Parent = null;
			PropertyName = property.Info.PropertyName;
			SerializedName = "$root";
			IsExcluded = false;
			IsShownInInspector = true;
			CodeAttributes = null;
			if (Attributes == null)
			{
				Attributes = new List<Attribute>();
			}
			else
			{
				Attributes.Clear();
			}
			OdinAttributeProcessorLocator processorLocator = property.Tree.AttributeProcessorLocator;
			List<OdinAttributeProcessor> selfProcessors = processorLocator.GetSelfProcessors(property);
			for (int i = 0; i < selfProcessors.Count; i++)
			{
				OdinAttributeProcessor processor = selfProcessors[i];
				if (processor.CanProcessSelfAttributes(property))
				{
					processor.ProcessSelfAttributes(property, Attributes);
				}
			}
			if (Attributes.Count > 0)
			{
				CodeAttributes = new HashSet<Type>();
				foreach (Attribute attribute in Attributes)
				{
					if (attribute != null)
					{
						CodeAttributes.Add(attribute.GetType());
					}
				}
			}
			if (DesignerRegistry.IsPropertyDesigned(property))
			{
				DesignerInspectorPropertyInfoUtility.ApplySelfPatches(property, Attributes);
			}
		}

		public void InitializeMember(InspectorProperty property, DesignerEditorNode parent, MemberInfo member, Color color)
		{
			Text = string.Empty;
			Parent = parent;
			Path = property.Path;
			Label = property.Label.text;
			SerializedName = (PropertyName = property.Info.PropertyName);
			IsExcluded = DesignerUtils.IsExcludedFromDesigner(property);
			IsShownInInspector = property.Info.IsShownInInspector;
			Member = member;
			PropertyInstance = property.ValueEntry?.WeakSmartValue;
			NodeColor = color;
			CodeAttributes = property.Info.DesignerAttributesFromCode;
			if (property.ChildResolver is ICollectionResolver collectionResolver)
			{
				PropertyElementType = collectionResolver.ElementType;
			}
			else
			{
				PropertyElementType = null;
			}
			if (Member != null)
			{
				MemberType = Member.MemberType;
				if (Member is MethodBase method)
				{
					SerializedName = DesignerUtils.CreateSerializedMethodName(method);
				}
			}
			else if (property.Info.PropertyType == PropertyType.Method)
			{
				MemberType = MemberTypes.Method;
			}
			else
			{
				MemberType = MemberTypes.Property;
			}
			if (Attributes == null)
			{
				Attributes = new List<Attribute>(DesignerUtils.Round4(property.Attributes.Count));
			}
			else if (Attributes.Capacity < property.Attributes.Count)
			{
				Attributes.Capacity = DesignerUtils.Round4(property.Attributes.Count);
			}
			Attributes.AddRange(property.Attributes);
			parent.Children.Add(this);
			if (IsShownInInspector)
			{
				for (DesignerEditorNode current = Parent; current != null; current = current.Parent)
				{
					current.IsShownInInspector = true;
				}
			}
			if (property.Info.PropertyType == PropertyType.Method)
			{
				MethodBase method2 = null;
				method2 = ((!(Member != null) || !(Member is MethodBase memberMethod)) ? property.Info.GetMethodDelegate()?.Method : memberMethod);
				if (method2 != null)
				{
					PropertyTypeLabel = DesignerGUI.GetMethodTypeLabel(method2);
				}
				PropertyValueType = null;
			}
			else if (member != null)
			{
				PropertyValueType = member.GetReturnType();
				PropertyTypeLabel = member.GetReturnType().GetNiceName();
			}
			else
			{
				PropertyValueType = property.Info.TypeOfValue;
				PropertyTypeLabel = property.Info.TypeOfValue.GetNiceName();
			}
			if (!string.IsNullOrEmpty(PropertyTypeLabel))
			{
				PropertyTypeLabel = ObjectNames.NicifyVariableName(PropertyTypeLabel);
			}
		}

		public void InitializeGroup(InspectorProperty property, DesignerEditorNode parent)
		{
			Text = string.Empty;
			Parent = parent;
			Path = property.Path;
			Label = ((PropertyGroupAttribute)property.Attributes[0]).GroupName;
			PropertyName = property.Info.PropertyName;
			IsExcluded = property.GetAttribute<ExcludeInOdinDesignerAttribute>() != null || DesignerRegistry.IsTypeOfOwnerExcluded(property.Info);
			IsShownInInspector = true;
			GroupAttribute = (PropertyGroupAttribute)property.Attributes[0];
			IsRow = GroupAttribute is ColumnGroupAttribute;
			IsColumn = GroupAttribute is ColumnGroupAttribute.ColumnSubGroupAttribute;
			IsGroupAddedByDesigner = !string.IsNullOrEmpty(property.Info.DesignerId);
			CodeAttributes = null;
			if (GroupAttribute is ColumnGroupAttribute.ColumnSubGroupAttribute column)
			{
				Text = column.Size.ToString();
			}
			if (Children == null)
			{
				Children = new List<DesignerEditorNode>(DesignerUtils.Round4(property.Children.Count));
			}
			else if (Children.Capacity < property.Children.Count)
			{
				Children.Capacity = DesignerUtils.Round4(property.Children.Count);
			}
			parent.Children.Add(this);
		}

		public void DrawDragPreview(DesignerEditorContext context)
		{
			if (context.HoverNode != null)
			{
				EditorGUI.DrawRect(Rect, Colors.Accents.Fields);
				GUI.Label(Rect, context.DragNode.Label);
			}
		}

		public DesignerEditorNode FindNodeByProperty(InspectorProperty property)
		{
			InspectorProperty parent = property.ParentValueProperty;
			string path = property.Path;
			if (parent != null && !parent.IsTreeRoot)
			{
				path = path.Replace(parent.Path, "");
				path = path.TrimStart(new char[1] { '.' });
			}
			return FindNodeByPath(path);
		}

		public DesignerEditorNode FindNodeByPath(string path)
		{
			if (Path == path)
			{
				return this;
			}
			if (NodeType == DesignerEditorNodeType.Member)
			{
				return null;
			}
			for (int i = 0; i < Children.Count; i++)
			{
				DesignerEditorNode childResult = Children[i].FindNodeByPath(path);
				if (childResult != null)
				{
					return childResult;
				}
			}
			return null;
		}

		public void ResetDesiredIndex()
		{
			if (GroupPatch != null)
			{
				if (GroupPatch.DesiredIndex != -1)
				{
					GroupPatch.DesiredIndex = GetVisualIndex() + 1;
				}
			}
			else if (PropertyPatch != null && PropertyPatch.HasDesiredIndex)
			{
				PropertyPatch.DesiredIndex = GetVisualIndex() + 1;
			}
		}

		public void Draw(Rect rect, DesignerEditorContext context)
		{
			switch (NodeType)
			{
			case DesignerEditorNodeType.Group:
				if (IsRow)
				{
					DesignerGroupGUI.DrawRow(this, rect);
				}
				else if (IsColumn)
				{
					DesignerGroupGUI.DrawColumn(this, rect, context);
				}
				else if (GroupAttribute is ISubGroupProviderAttribute)
				{
					DesignerGroupGUI.DrawSubGroupOwner(rect, rect.AlignTop(28f), this, context);
				}
				else
				{
					DesignerGroupGUI.DrawGroup(rect, rect.AlignTop(28f), this, context);
				}
				break;
			case DesignerEditorNodeType.Member:
				DesignerGUI.DrawMember(rect, this, context, IsShownInInspector, context.IsShowHideMode);
				break;
			}
		}

		public void UpdatePositionIfNeeded(string parentId, int index)
		{
			if (GroupPatch != null)
			{
				if (GroupPatch.ParentId != null)
				{
					GroupPatch.ParentId = parentId;
					GroupPatch.DesiredIndex = index;
				}
			}
			else if (PropertyPatch?.ParentId != null)
			{
				PropertyPatch.ParentId = parentId;
				PropertyPatch.DesiredIndex = index;
			}
		}

		public void UpdatePositionOnChildrenIfNeeded()
		{
			string parentId = GetDesignerId();
			for (int i = 0; i < Children.Count; i++)
			{
				Children[i].UpdatePositionIfNeeded(parentId, i);
			}
		}

		public void OpenContextMenu(DesignerEditorContext context)
		{
			switch (NodeType)
			{
			case DesignerEditorNodeType.Group:
			{
				if (IsRow || IsColumn)
				{
					break;
				}
				GenericMenu menu = new GenericMenu();
				if (GroupPatch != null && !GroupPatch.IsAddedByDesigner && GroupPatch.ParentId != null)
				{
					menu.AddItem(new GUIContent("Reset Position"), on: false, delegate
					{
						context.BeginUndo();
						GroupPatch.ParentId = null;
						GroupPatch.DesiredIndex = -1;
						context.EndUndo(isEditorOutOfSync: true);
					});
				}
				else
				{
					menu.AddDisabledItem(new GUIContent("Reset Position"));
				}
				menu.ShowAsContext();
				break;
			}
			case DesignerEditorNodeType.Member:
			{
				GenericMenu contextMenu = new GenericMenu();
				if (PropertyPatch?.ParentId != null)
				{
					contextMenu.AddItem(new GUIContent("Reset Position"), on: false, delegate
					{
						context.BeginUndo();
						PropertyPatch.ParentId = null;
						PropertyPatch.DesiredIndex = -1;
						context.EndUndo(isEditorOutOfSync: true);
					});
				}
				else
				{
					contextMenu.AddDisabledItem(new GUIContent("Reset Position"));
				}
				if (PropertyPatch != null && PropertyPatch.Visibility != PropertyVisibilityState.Default)
				{
					contextMenu.AddItem(new GUIContent("Reset Visibility"), on: false, delegate
					{
						context.BeginUndo();
						PropertyPatch.Visibility = PropertyVisibilityState.Default;
						context.EndUndo(isEditorOutOfSync: true);
					});
				}
				else
				{
					contextMenu.AddDisabledItem(new GUIContent("Reset Visibility"));
				}
				contextMenu.ShowAsContext();
				break;
			}
			}
		}

		public void SetupFlatIndicesRecursive(ref int index)
		{
			FlatIndex = index++;
			if (Children != null)
			{
				for (int i = 0; i < Children.Count; i++)
				{
					Children[i].SetupFlatIndicesRecursive(ref index);
				}
			}
		}
	}
}
