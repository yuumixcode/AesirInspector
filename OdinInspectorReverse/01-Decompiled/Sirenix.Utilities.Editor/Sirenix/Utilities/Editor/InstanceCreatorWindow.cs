using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using UnityEditor;
using UnityEngine;

namespace Sirenix.Utilities.Editor
{
	internal class InstanceCreatorWindow : EditorWindow
	{
		private class NullType
		{
		}

		private class TypeTreeNode
		{
			public AssemblyCategory AssemblyCategory = AssemblyCategory.Unknown;

			public List<TypeTreeNode> ChildNodes;

			public bool ForceSetSelected;

			public bool IsVisible;

			public string Namespace;

			public TypeTreeNode ParentNode;

			public Type Type;

			private static Texture typeIcon = GUIHelper.GetAssetThumbnail(null, typeof(MonoBehaviour), preferObjectPreviewOverFileIcon: false);

			private bool isAssemblyFlagNode;

			private bool isNameSpaceNode;

			private bool isTypeNode;

			private GUIContent label;

			private string labelName;

			private string searchString;

			private bool matchesSearchTerm;

			private InstanceCreatorWindow window;

			private bool drawHasNoEmptyConstructor;

			private GUIContent hasNoEmptyConstructorLabel;

			public TypeTreeNode(Type type)
			{
				Type = type;
			}

			public TypeTreeNode(string @namespace)
			{
				Namespace = @namespace;
			}

			public TypeTreeNode(AssemblyCategory assemblyTypeFlags)
			{
				AssemblyCategory = assemblyTypeFlags;
			}

			public void PrependType(Type type)
			{
				if (ChildNodes == null)
				{
					ChildNodes = new List<TypeTreeNode>();
				}
				Type specialType = ((type == typeof(NullType)) ? InstanceCreator.Type : type);
				AssemblyCategory flag = AssemblyUtilities.GetAssemblyCategory(specialType.Assembly);
				TypeTreeNode node = ChildNodes.FirstOrDefault((TypeTreeNode x) => x.AssemblyCategory == flag);
				if (node == null)
				{
					node = new TypeTreeNode(flag);
					ChildNodes.Insert(0, node);
				}
				if (node.ChildNodes == null)
				{
					node.ChildNodes = new List<TypeTreeNode>();
				}
				if (string.IsNullOrEmpty(specialType.Namespace))
				{
					node.ChildNodes.Insert(0, new TypeTreeNode(type));
					return;
				}
				TypeTreeNode nsNode = node.ChildNodes.FirstOrDefault((TypeTreeNode x) => x.Namespace == specialType.Namespace);
				if (nsNode == null)
				{
					nsNode = new TypeTreeNode(specialType.Namespace);
					node.ChildNodes.Insert(0, nsNode);
				}
				if (nsNode.ChildNodes == null)
				{
					nsNode.ChildNodes = new List<TypeTreeNode>();
				}
				nsNode.ChildNodes.Insert(0, new TypeTreeNode(type));
			}

			public void DrawItem()
			{
				bool hasLabel = labelName != null && (matchesSearchTerm || !window.hasSearchTerm);
				if (hasLabel)
				{
					bool isSelected = false;
					bool isChosen = false;
					if (ChildNodes != null)
					{
						if (!window.hideFoldoutLabels)
						{
							SirenixEditorGUI.BeginMenuListItem(out isSelected, out isChosen, ForceSetSelected);
							if (matchesSearchTerm)
							{
								SirenixEditorGUI.Foldout(isVisible: true, label);
							}
							else
							{
								IsVisible = SirenixEditorGUI.Foldout(IsVisible, label);
							}
							SirenixEditorGUI.EndMenuListItem();
						}
					}
					else
					{
						SirenixEditorGUI.BeginMenuListItem(out isSelected, out isChosen, ForceSetSelected);
						if (isTypeNode && Type == typeof(NullType))
						{
							EditorGUILayout.LabelField(label, SirenixGUIStyles.LeftAlignedGreyMiniLabel);
						}
						else
						{
							EditorGUILayout.LabelField(label);
						}
						if (drawHasNoEmptyConstructor)
						{
							Rect rect = GUILayoutUtility.GetLastRect();
							rect.width -= 16f;
							EditorIcons.AlertTriangle.Draw(new Rect(rect.xMax, rect.yMin, 16f, 16f));
							GUI.Label(rect, hasNoEmptyConstructorLabel, SirenixGUIStyles.RightAlignedGreyMiniLabel);
						}
						SirenixEditorGUI.EndMenuListItem();
					}
					if (isSelected && Event.current.type == EventType.KeyDown)
					{
						if (Event.current.keyCode == KeyCode.RightArrow)
						{
							IsVisible = true;
						}
						else if (Event.current.keyCode == KeyCode.LeftArrow)
						{
							IsVisible = false;
						}
						else if (Event.current.keyCode == KeyCode.Return)
						{
							isChosen = true;
						}
					}
					if (isChosen)
					{
						IsVisible = !IsVisible;
						if (isTypeNode)
						{
							window.chosenType = Type;
						}
					}
					ForceSetSelected = false;
				}
				if (labelName == null)
				{
					IsVisible = true;
				}
				if (ChildNodes == null)
				{
					return;
				}
				if (matchesSearchTerm || SirenixEditorGUI.BeginFadeGroup(this, IsVisible))
				{
					if (hasLabel && !window.hideFoldoutLabels)
					{
						EditorGUI.indentLevel++;
					}
					for (int i = 0; i < ChildNodes.Count; i++)
					{
						ChildNodes[i].DrawItem();
					}
					if (hasLabel && !window.hideFoldoutLabels)
					{
						EditorGUI.indentLevel--;
					}
				}
				if (!matchesSearchTerm)
				{
					SirenixEditorGUI.EndFadeGroup();
				}
			}

			public IEnumerable<TypeTreeNode> EnumerateTree()
			{
				yield return this;
				if (ChildNodes == null)
				{
					yield break;
				}
				foreach (TypeTreeNode item in ChildNodes)
				{
					foreach (TypeTreeNode item2 in item.EnumerateTree())
					{
						yield return item2;
					}
				}
			}

			public void Initialize(InstanceCreatorWindow window, TypeTreeNode parent)
			{
				ParentNode = parent;
				isNameSpaceNode = !string.IsNullOrEmpty(Namespace);
				this.window = window;
				isTypeNode = Type != null;
				isAssemblyFlagNode = AssemblyCategory != AssemblyCategory.Unknown;
				labelName = null;
				if (isNameSpaceNode)
				{
					if (ChildNodes != null && ChildNodes.Count > 0)
					{
						labelName = Namespace;
						label = new GUIContent(labelName);
					}
				}
				else if (isAssemblyFlagNode)
				{
					if (ChildNodes != null && ChildNodes.Count > 0)
					{
						labelName = AssemblyCategory.ToString().SplitPascalCase();
						label = new GUIContent(labelName);
					}
				}
				else if (isTypeNode)
				{
					labelName = ((Type == typeof(NullType)) ? ("Null (" + InstanceCreator.Type.GetNiceName() + ")") : Type.GetNiceName());
					label = new GUIContent(labelName, typeIcon);
					if (Type.IsValueType || Type == typeof(string) || Type.IsArray || (tupleInterface != null && tupleInterface.IsAssignableFrom(Type)))
					{
						drawHasNoEmptyConstructor = false;
					}
					else
					{
						drawHasNoEmptyConstructor = Type.GetConstructor(Type.EmptyTypes) == null;
					}
					if (drawHasNoEmptyConstructor)
					{
						hasNoEmptyConstructorLabel = new GUIContent("No default constructor found");
					}
				}
				if (Type != null)
				{
					searchString = Type.GetNiceFullName();
				}
				else
				{
					searchString = labelName;
				}
				if (ChildNodes == null)
				{
					return;
				}
				foreach (TypeTreeNode item in ChildNodes)
				{
					item.Initialize(this.window, this);
				}
			}

			public void UpdateSearchTerm()
			{
				matchesSearchTerm = false;
				if (ChildNodes != null)
				{
					foreach (TypeTreeNode item in ChildNodes)
					{
						item.UpdateSearchTerm();
					}
				}
				if (isTypeNode && searchString != null && window.hasSearchTerm && FuzzySearch.Contains(window.searchTerm, searchString))
				{
					SetMatchesSearchTermRecursive(value: true);
				}
			}

			private void SetMatchesSearchTermRecursive(bool value)
			{
				matchesSearchTerm = value;
				if (ParentNode != null)
				{
					ParentNode.SetMatchesSearchTermRecursive(value);
				}
			}
		}

		private static readonly Type tupleInterface = typeof(string).Assembly.GetType("System.ITuple") ?? typeof(string).Assembly.GetType("System.ITupleInternal");

		private static Dictionary<Type, TypeTreeNode> typeTrees = new Dictionary<Type, TypeTreeNode>();

		private Type chosenType;

		private bool hasSearchTerm;

		private TypeTreeNode RootNode;

		private bool searchChanged;

		private string searchTerm;

		private bool autoSelectFirst;

		private bool hideFoldoutLabels;

		private static GUIStyle toolbarBackgroundChainedTop;

		private static GUIStyle ToolbarBackgroundChainedTop
		{
			get
			{
				if (toolbarBackgroundChainedTop == null)
				{
					toolbarBackgroundChainedTop = new GUIStyle(SirenixGUIStyles.ToolbarBackground)
					{
						overflow = new RectOffset(1, 1, 1, 0)
					};
				}
				return toolbarBackgroundChainedTop;
			}
		}

		internal void Initialize()
		{
			autoSelectFirst = Event.current != null && UnityShims.Misc.GetEventModifiers(Event.current) == 2;
			RootNode = GetTypeTree(InstanceCreator.Type);
			if (chosenType != null)
			{
				SetSelectedType();
				chosenType = null;
			}
			else
			{
				base.titleContent = new GUIContent("Instance Creator");
				base.titleContent = new GUIContent("Create Instance of " + InstanceCreator.Type.GetNiceName());
			}
		}

		private static bool CanCreateInstance(Type baseType, ref Type type)
		{
			if (type.IsArray && type.InheritsFrom(baseType))
			{
				return true;
			}
			if (type.IsAbstract || type.IsInterface)
			{
				return false;
			}
			if (type.InheritsFrom(typeof(UnityEngine.Object)))
			{
				return false;
			}
			bool baseTypeIsGenericType = baseType.IsGenericType;
			if (baseTypeIsGenericType && (baseType.GetGenericTypeDefinition() == typeof(Dictionary<, >) || baseType.GetGenericTypeDefinition() == typeof(IDictionary<, >)) && type != typeof(Dictionary<, >) && type != typeof(NullType))
			{
				return false;
			}
			if (baseTypeIsGenericType && type.IsGenericTypeDefinition)
			{
				Type def = baseType.GetGenericTypeDefinition();
				if (type.ImplementsOpenGenericType(def))
				{
					Type[] args = baseType.GetGenericArguments();
					if (type.AreGenericConstraintsSatisfiedBy(args))
					{
						Type newType = type.MakeGenericType(args);
						if (baseType.IsAssignableFrom(newType))
						{
							type = newType;
							return true;
						}
					}
				}
			}
			if ((!type.IsGenericTypeDefinition || !baseType.IsGenericType || type.GetGenericArguments().Length != baseType.GetGenericArguments().Length) && type.IsGenericTypeDefinition)
			{
				return false;
			}
			if (type == typeof(NullType) || (baseType == typeof(object) && (type.IsValueType || type == typeof(string) || type.IsEnum) && !type.IsDefined<CompilerGeneratedAttribute>()))
			{
				return true;
			}
			if (baseType.IsInterface && baseType.GetGenericArguments().Length != 0 && type.ImplementsOpenGenericInterface(baseType.GetGenericTypeDefinition()) && type.IsGenericTypeDefinition && !type.AreGenericConstraintsSatisfiedBy(baseType.GetGenericArguments()))
			{
				return false;
			}
			if (!type.InheritsFrom(typeof(UnityEngine.Object)) && !type.InheritsFrom(typeof(ScriptableObject)) && !type.InheritsFrom(typeof(SerializedObject)) && !type.Name.StartsWith("<", StringComparison.InvariantCulture))
			{
				return type.InheritsFrom(baseType);
			}
			return false;
		}

		private TypeTreeNode GetTypeTree(Type type)
		{
			if (!typeTrees.TryGetValue(type, out var rootNode))
			{
				AssemblyCategory typeFlag = AssemblyUtilities.GetAssemblyCategory(type.Assembly);
				List<Type> list = new List<Type>();
				list.Add(typeof(NullType));
				list.Add(type);
				list.Add(typeof(string));
				list.Add(typeof(List<>).MakeGenericType(type));
				list.Add(type.MakeArrayType());
				list.Add(type.MakeArrayType(2));
				list.Add(type.MakeArrayType(3));
				List<Type> additionalTypes = list;
				var additions = additionalTypes.Select((Type x) => new
				{
					type = x,
					flag = ((x == typeof(NullType) || x.IsArray) ? typeFlag : AssemblyUtilities.GetAssemblyCategory(x.Assembly))
				}).ToArray();
				Dictionary<Type, TypeTreeNode> dictionary = typeTrees;
				Type key = type;
				TypeTreeNode obj = new TypeTreeNode((Type)null)
				{
					ChildNodes = (from x in AssemblyUtilities.GetAllAssemblies()
						select new
						{
							assembly = x,
							flag = AssemblyUtilities.GetAssemblyCategory(x)
						} into a
						group a by a.flag into a
						select new TypeTreeNode(a.Key)
						{
							IsVisible = ((a.Key & AssemblyCategory.ProjectSpecific) != 0),
							ChildNodes = (from t in (from t in (from x in AssemblyUtilities.GetTypes(a.Key)
										where x != type && x != typeof(NullType)
										select x).PrependWith(from x in additions
										where x.flag == a.Key
										select x.type)
									select (!CanCreateInstance(type, ref t)) ? null : t into t
									where t != null
									select t).Distinct()
								group t by ((t == typeof(NullType) || t.IsArray) ? type : t).Namespace ?? "ALSO!GROUP!ME" into g
								select new TypeTreeNode((g.Key == "ALSO!GROUP!ME") ? null : g.Key)
								{
									ChildNodes = (from t in g
										select new TypeTreeNode(t) into t
										where t.Type == null || !Enumerable.Contains(t.Type.GetNiceName(), '$')
										orderby t.Type == typeof(NullType), (!(t.Type == null)) ? t.Type.Name : ""
										select t).ToList()
								} into x
								where x.Type == null || !Enumerable.Contains(x.Type.GetNiceName(), '$')
								orderby x.Namespace, x.Type
								select x).ToList()
						} into x
						orderby x.AssemblyCategory
						select x).ToList()
				};
				TypeTreeNode typeTreeNode = obj;
				dictionary[key] = obj;
				rootNode = typeTreeNode;
				rootNode.IsVisible = true;
				rootNode.Initialize(this, null);
			}
			else
			{
				rootNode.Initialize(this, null);
			}
			hideFoldoutLabels = rootNode.EnumerateTree().Count((TypeTreeNode x) => x.Type != null) < 20;
			if (hideFoldoutLabels)
			{
				foreach (TypeTreeNode item in rootNode.EnumerateTree())
				{
					item.IsVisible = true;
				}
			}
			else
			{
				TypeTreeNode firstCollection = rootNode.EnumerateTree().FirstOrDefault((TypeTreeNode x) => x.ChildNodes != null && x.ChildNodes.Count > 0);
				if (firstCollection != null)
				{
					firstCollection.IsVisible = true;
				}
			}
			TypeTreeNode firstTypeNode = rootNode.EnumerateTree().FirstOrDefault((TypeTreeNode x) => x.Type != null);
			if (firstTypeNode != null)
			{
				firstTypeNode.ForceSetSelected = true;
			}
			if (autoSelectFirst && firstTypeNode != null)
			{
				chosenType = firstTypeNode.Type;
			}
			return rootNode;
		}

		private void OnGUI()
		{
			Focus();
			base.wantsMouseMove = true;
			if (InstanceCreator.Type == null)
			{
				Close();
				return;
			}
			RootNode = RootNode ?? GetTypeTree(InstanceCreator.Type);
			SirenixEditorGUI.BeginHorizontalToolbar(ToolbarBackgroundChainedTop);
			searchChanged = searchTerm != (searchTerm = SirenixEditorGUI.ToolbarSearchField(searchTerm, forceFocus: true)) || searchChanged;
			SirenixEditorGUI.EndHorizontalToolbar();
			SirenixEditorGUI.BeginVerticalMenuList(this);
			RootNode.DrawItem();
			SirenixEditorGUI.EndVerticalMenuList();
			SirenixEditorGUI.DrawBorders(new Rect(0f, 0f, base.position.width, base.position.height), 1, 1, 1, 1, SirenixGUIStyles.BorderColor);
			if (Event.current.type == EventType.Repaint)
			{
				hasSearchTerm = searchTerm != null && (searchTerm.Length >= 2 || (searchTerm.Length == 1 && !char.IsLetter(searchTerm[0])));
				if (searchChanged)
				{
					RootNode.UpdateSearchTerm();
					searchChanged = false;
				}
			}
			if (chosenType != null)
			{
				SetSelectedType();
				chosenType = null;
				Close();
			}
			else
			{
				this.RepaintIfRequested();
			}
		}

		private void SelectChosenType()
		{
			if (chosenType != null)
			{
				if (chosenType.InheritsFrom(typeof(ScriptableObject)))
				{
					InstanceCreator.CreatedInstance = ScriptableObject.CreateInstance(chosenType);
				}
				else
				{
					InstanceCreator.CreatedInstance = CreateInstance();
				}
				InstanceCreator.HasCreatedInstance = true;
				chosenType = null;
				Close();
			}
		}

		private void SetSelectedType()
		{
			if (chosenType == typeof(NullType))
			{
				InstanceCreator.CreatedInstance = null;
			}
			else if (chosenType.InheritsFrom(typeof(ScriptableObject)))
			{
				InstanceCreator.CreatedInstance = ScriptableObject.CreateInstance(chosenType);
			}
			else
			{
				InstanceCreator.CreatedInstance = CreateInstance();
			}
			InstanceCreator.HasCreatedInstance = true;
		}

		private object CreateInstance()
		{
			if (chosenType.IsArray)
			{
				return Array.CreateInstance(chosenType.GetElementType(), new long[chosenType.GetArrayRank()]);
			}
			if (chosenType == typeof(string))
			{
				return "";
			}
			if (tupleInterface != null && tupleInterface.IsAssignableFrom(chosenType))
			{
				return FormatterServices.GetUninitializedObject(chosenType);
			}
			if (chosenType.GetConstructor(Type.EmptyTypes) != null)
			{
				if (chosenType.IsGenericTypeDefinition)
				{
					Type genericType = chosenType.MakeGenericType(InstanceCreator.Type.GetGenericArguments());
					return Activator.CreateInstance(genericType);
				}
				return Activator.CreateInstance(chosenType);
			}
			return FormatterServices.GetUninitializedObject(chosenType);
		}

		public float GetWindowHeight()
		{
			int count = (from x in RootNode.EnumerateTree()
				where x.Type != null
				select x).Count();
			return Mathf.Clamp(count * 22 + 23, 45, 400);
		}
	}
}
