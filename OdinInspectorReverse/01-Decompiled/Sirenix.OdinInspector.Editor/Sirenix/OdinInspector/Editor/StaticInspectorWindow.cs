using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Sirenix.OdinInspector.Editor.Internal;
using Sirenix.Reflection.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor
{
	/// <summary>
	/// Access the StaticInspectorWindow from Tools &gt; Odin &gt; Inspector &gt; Static Inspector.
	/// </summary>
	public class StaticInspectorWindow : OdinEditorWindow
	{
		/// <summary>
		/// Member filter for access modifiers.
		/// </summary>
		[Flags]
		public enum AccessModifierFlags
		{
			/// <summary>
			/// include public members.
			/// </summary>
			Public = 2,
			/// <summary>
			/// Include Non-public members.
			/// </summary>
			Private = 4,
			/// <summary>
			/// Include both public and non-public members.
			/// </summary>
			All = 6
		}

		/// <summary>
		/// Member filter for member types.
		/// </summary>
		[Flags]
		public enum MemberTypeFlags
		{
			/// <summary>
			/// No members included.
			/// </summary>
			None = 0,
			/// <summary>
			/// Include field members.
			/// </summary>
			Fields = 1,
			/// <summary>
			/// Include property members.
			/// </summary>
			Properties = 2,
			/// <summary>
			/// Include method members.
			/// </summary>
			Methods = 4,
			/// <summary>
			/// Include group members.
			/// </summary>
			Groups = 8,
			/// <summary>
			/// Include members from the base types.
			/// </summary>
			BaseTypeMembers = 0x10,
			/// <summary>
			/// Include members marked with the Obsolete attribute.
			/// </summary>
			Obsolete = 0x20,
			/// <summary>
			/// Include all members except members marked with the Obsolete attribute.
			/// </summary>
			AllButObsolete = 0x1F
		}

		private static GUIStyle btnStyle;

		private const string TargetTypeAssemblyCategoriesPrefKey = "OdinStaticInspectorWindow.TargetTypeAssemblyCategories";

		private const string MemberTypeFlagsPrefKey = "OdinStaticInspectorWindow.MemberTypeFlags";

		private const string AccessModifierFlagsPrefKey = "OdinStaticInspectorWindow.AccessModifierFlags";

		[HideInInspector]
		[SerializeField]
		private Type targetType;

		[HideInInspector]
		[SerializeField]
		private AssemblyCategory targetTypeFlags;

		[HideInInspector]
		[SerializeField]
		private MemberTypeFlags memberTypes;

		[HideInInspector]
		[SerializeField]
		private AccessModifierFlags accessModifiers;

		[HideInInspector]
		[SerializeField]
		private string showMemberNameFilter;

		[HideInInspector]
		[SerializeField]
		private string searchFilter;

		[NonSerialized]
		private PropertyTree tree;

		[NonSerialized]
		private AccessModifierFlags currAccessModifiers;

		[NonSerialized]
		private MemberTypeFlags currMemberTypes;

		[NonSerialized]
		private int focusSearch;

		/// <summary>
		/// Shows the window.
		/// </summary>
		public static void ShowWindow()
		{
			InspectType(null);
		}

		/// <summary>
		/// Opens a new static inspector window for the given type.
		/// </summary>
		public static StaticInspectorWindow InspectType(Type type, AccessModifierFlags? accessModifies = null, MemberTypeFlags? memberTypeFlags = null)
		{
			StaticInspectorWindow window = ScriptableObject.CreateInstance<StaticInspectorWindow>();
			window.titleContent = new GUIContent("Static Inspector", EditorIcons.MagnifyingGlass.Highlighted);
			window.position = GUIHelper.GetEditorWindowRect().AlignCenter(700f, 400f);
			window.targetTypeFlags = (AssemblyCategory)EditorPrefs.GetInt("OdinStaticInspectorWindow.TargetTypeAssemblyCategories", 7);
			if (accessModifies.HasValue)
			{
				window.accessModifiers = accessModifies.Value;
			}
			else
			{
				window.accessModifiers = (AccessModifierFlags)EditorPrefs.GetInt("OdinStaticInspectorWindow.AccessModifierFlags", 6);
			}
			if (memberTypeFlags.HasValue)
			{
				window.memberTypes = memberTypeFlags.Value;
			}
			else
			{
				window.memberTypes = (MemberTypeFlags)EditorPrefs.GetInt("OdinStaticInspectorWindow.MemberTypeFlags", 15);
			}
			window.currMemberTypes = window.memberTypes;
			window.currAccessModifiers = window.accessModifiers;
			window.focusSearch = 0;
			window.targetType = type;
			window.Show();
			if (type != null)
			{
				window.titleContent = new GUIContent(type.GetNiceName());
			}
			window.Repaint();
			return window;
		}

		private OdinSelector<Type> SelectType(Rect arg)
		{
			OdinSelector<Type> p = TypeSelectorHandler_WILL_BE_DEPRECATED.InstantiateSelector(targetTypeFlags, supportsMultiSelect: false, null, showNoneItem: false, showCategories: true);
			p.SelectionChanged += delegate(IEnumerable<Type> types)
			{
				focusSearch = 0;
				Type type = types.FirstOrDefault();
				if (type != null)
				{
					targetType = type;
					base.titleContent = new GUIContent(targetType.GetNiceName());
				}
			};
			p.SetSelection(targetType);
			p.ShowInPopup(new Rect(-300f, 0f, 300f, 0f));
			return p;
		}

		/// <summary>
		/// Draws the Odin Editor Window.
		/// </summary>
		protected override void OnImGUI()
		{
			btnStyle = btnStyle ?? new GUIStyle(EditorStyles.toolbarDropDown);
			btnStyle.fixedHeight = 21f;
			btnStyle.stretchHeight = false;
			DrawFirstToolbar();
			if (targetType != null)
			{
				DrawSecondToolbar();
			}
			base.OnImGUI();
		}

		private void DrawFirstToolbar()
		{
			GUILayout.Space(1f);
			string typeName = "       " + ((targetType == null) ? "Select Type" : targetType.GetNiceFullName()) + "   ";
			Rect rect = GUILayoutUtility.GetRect(0f, 21f, SirenixGUIStyles.ToolbarBackground);
			Rect rect2 = rect.AlignRight(80f);
			Rect rect3 = rect.SetXMax(rect2.xMin);
			if (GlobalConfig<GeneralDrawerConfig>.Instance.useOldTypeSelector)
			{
				OdinSelector<Type>.DrawSelectorDropdown(rect3, new GUIContent(typeName), SelectType, btnStyle);
			}
			else
			{
				OdinSelector<Type>.DrawSelectorDropdown(rect3, new GUIContent(typeName), SelectType, btnStyle);
			}
			EditorGUI.BeginChangeCheck();
			targetTypeFlags = EnumSelector<AssemblyCategory>.DrawEnumField(rect2, null, new GUIContent("Type Filter"), targetTypeFlags, btnStyle);
			if (EditorGUI.EndChangeCheck())
			{
				EditorPrefs.SetInt("OdinStaticInspectorWindow.TargetTypeAssemblyCategories", (int)targetTypeFlags);
			}
			if (Event.current.type == EventType.Repaint)
			{
				Texture2D icon = GUIHelper.GetAssetThumbnail(null, targetType ?? typeof(int), preferObjectPreviewOverFileIcon: false);
				if (icon != null)
				{
					rect3.x += 8f;
					GUI.DrawTexture(rect3.AlignLeft(16f).AlignMiddle(16f), icon, ScaleMode.ScaleToFit);
				}
			}
		}

		private void DrawSecondToolbar()
		{
			Rect rect = GUILayoutUtility.GetRect(0f, 21f);
			if (Event.current.type == EventType.Repaint)
			{
				SirenixGUIStyles.ToolbarBackground.Draw(rect, GUIContent.none, 0);
				SirenixEditorGUI.DrawBorders(rect, 0, 0, 0, 1);
			}
			Rect accessRect = rect.AlignRight(80f);
			Rect memberRect = accessRect.SubX(100f).SetWidth(100f);
			Rect searchRect = rect.SetXMax(memberRect.xMin);
			EditorGUI.BeginChangeCheck();
			memberTypes = EnumSelector<MemberTypeFlags>.DrawEnumField(memberRect, null, memberTypes, btnStyle);
			accessModifiers = EnumSelector<AccessModifierFlags>.DrawEnumField(accessRect, null, accessModifiers, btnStyle);
			if (EditorGUI.EndChangeCheck())
			{
				EditorPrefs.SetInt("OdinStaticInspectorWindow.AccessModifierFlags", (int)accessModifiers);
				EditorPrefs.SetInt("OdinStaticInspectorWindow.MemberTypeFlags", (int)memberTypes);
			}
			DrawSearchField(searchRect);
		}

		private void DrawSearchField(Rect rect)
		{
			rect = rect.HorizontalPadding(5f).AlignMiddle(16f);
			rect.xMin += 3f;
			searchFilter = SirenixEditorGUI.SearchField(rect, searchFilter, focusSearch++ < 4, "SirenixSearchField" + OdinEntityId.FromObject(this).ToString());
		}

		/// <summary>
		/// Draws the editor for the this.CurrentDrawingTargets[index].
		/// </summary>
		protected override void DrawEditor(int index)
		{
			DrawGettingStartedHelp();
			DrawTree();
			GUILayout.FlexibleSpace();
		}

		private void DrawGettingStartedHelp()
		{
			if (targetType == null)
			{
				SirenixEditorGUI.MessageBox("Select a type here to begin static inspection.", MessageType.Info, GlobalConfig<GeneralDrawerConfig>.Instance.MessageBoxFontSize);
			}
		}

		private void DrawTree()
		{
			if (targetType == null)
			{
				if (tree != null)
				{
					tree.Dispose();
				}
				tree = null;
				return;
			}
			if (Event.current.type == EventType.Layout)
			{
				currMemberTypes = memberTypes;
				currAccessModifiers = accessModifiers;
			}
			if (tree == null || tree.TargetType != targetType)
			{
				if (targetType.IsGenericType && !targetType.IsFullyConstructedGenericType())
				{
					SirenixEditorGUI.MessageBox("Cannot statically inspect generic type definitions", MessageType.Error, GlobalConfig<GeneralDrawerConfig>.Instance.MessageBoxFontSize);
					return;
				}
				if (tree != null)
				{
					tree.Dispose();
				}
				tree = PropertyTree.CreateStatic(targetType);
			}
			bool allowObsoleteMembers = (currMemberTypes & MemberTypeFlags.Obsolete) == MemberTypeFlags.Obsolete;
			PropertyContext<bool> allowObsoleteMembersContext = tree.RootProperty.Context.GetGlobal("ALLOW_OBSOLETE_STATIC_MEMBERS", defaultValue: false);
			if (allowObsoleteMembersContext.Value != allowObsoleteMembers)
			{
				allowObsoleteMembersContext.Value = allowObsoleteMembers;
				tree.RootProperty.RefreshSetup();
			}
			tree.BeginDraw(withUndo: false);
			bool drawPropertiesNormally = true;
			if (tree.AllowSearchFiltering && tree.RootProperty.Attributes.HasAttribute<SearchableAttribute>())
			{
				SearchableAttribute attr = tree.RootProperty.GetAttribute<SearchableAttribute>();
				if (attr.Recursive)
				{
					SirenixEditorGUI.WarningMessageBox("This type has been marked as recursively searchable. Be *CAREFUL* with using this search - recursively searching a static inspector can be *very dangerous* and can lead to freezes, crashes or other nasty errors if the static inspector search ends up recursing deeply into, for example, the .NET runtime internals, which would result in recursively searching through hundreds of thousands to millions of internal properties.");
				}
				if (tree.DrawSearch())
				{
					drawPropertiesNormally = false;
				}
			}
			if (drawPropertiesNormally)
			{
				foreach (InspectorProperty prop in tree.EnumerateTree(includeChildren: false))
				{
					if (DrawProperty(prop))
					{
						if (prop.Info.PropertyType != PropertyType.Group && prop.Info.GetMemberInfo() != null && prop.Info.GetMemberInfo().DeclaringType != targetType)
						{
							prop.Draw(new GUIContent(prop.Info.GetMemberInfo().DeclaringType.GetNiceName() + " -> " + prop.NiceName));
						}
						else
						{
							prop.Draw();
						}
					}
					else
					{
						prop.Update();
					}
				}
			}
			tree.EndDraw();
		}

		private bool DrawProperty(InspectorProperty property)
		{
			if (!string.IsNullOrEmpty(searchFilter) && !StringExtensions.Contains(property.NiceName.Replace(" ", ""), searchFilter.Replace(" ", ""), StringComparison.InvariantCultureIgnoreCase))
			{
				return false;
			}
			if (property.Info.PropertyType == PropertyType.Group)
			{
				return (currMemberTypes & MemberTypeFlags.Groups) == MemberTypeFlags.Groups;
			}
			MemberInfo member = property.Info.GetMemberInfo();
			if (member != null)
			{
				if ((currMemberTypes & MemberTypeFlags.BaseTypeMembers) != MemberTypeFlags.BaseTypeMembers && member.DeclaringType != null && member.DeclaringType != targetType)
				{
					return false;
				}
				bool showPublic = (currAccessModifiers & AccessModifierFlags.Public) == AccessModifierFlags.Public;
				bool showPrivate = (currAccessModifiers & AccessModifierFlags.Private) == AccessModifierFlags.Private;
				bool showFields = (currMemberTypes & MemberTypeFlags.Fields) == MemberTypeFlags.Fields;
				bool showProperties = (currMemberTypes & MemberTypeFlags.Properties) == MemberTypeFlags.Properties;
				if (!showPublic || !showPrivate)
				{
					bool isPublic = true;
					FieldInfo fieldInfo = member as FieldInfo;
					PropertyInfo propertyInfo = member as PropertyInfo;
					MethodInfo methodInfo = member as MethodInfo;
					if (fieldInfo != null)
					{
						isPublic = fieldInfo.IsPublic;
					}
					else if (propertyInfo != null)
					{
						MethodInfo getMethod = propertyInfo.GetGetMethod();
						isPublic = getMethod != null && getMethod.IsPublic;
					}
					else if (methodInfo != null)
					{
						isPublic = methodInfo.IsPublic;
					}
					if (isPublic && !showPublic)
					{
						return false;
					}
					if (!isPublic && !showPrivate)
					{
						return false;
					}
				}
				if (member is FieldInfo && !showFields)
				{
					return false;
				}
				if (member is PropertyInfo && !showProperties)
				{
					return false;
				}
			}
			if (property.Info.PropertyType == PropertyType.Method && (currMemberTypes & MemberTypeFlags.Methods) != MemberTypeFlags.Methods)
			{
				return false;
			}
			return true;
		}

		protected override void OnDisable()
		{
			base.OnDisable();
			if (tree != null)
			{
				tree.Dispose();
			}
		}
	}
}
