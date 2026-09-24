using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.OdinInspector.Editor.Internal;
using Sirenix.Serialization;
using UnityEditor;
using UnityEngine;

namespace Sirenix.Utilities.Editor
{
	/// <summary>
	/// The Odin Inspector Serialization Debugger Window.
	/// </summary>
	/// <seealso cref="T:Sirenix.OdinInspector.Editor.OdinEditorWindow" />
	public sealed class SerializationDebuggerWindow : OdinEditorWindow
	{
		private const string TargetTypePrefKey = "SerializationDebuggerWindow.TargetType";

		[SerializeField]
		[HideInInspector]
		private Type targetType;

		[HideInInspector]
		[SerializeField]
		private bool odinContext;

		[NonSerialized]
		private OdinMenuTree serializationInfoTree;

		[NonSerialized]
		private SerializationBackendFlags backendFlags;

		[NonSerialized]
		private GUIStyle noteStyle;

		[OnInspectorGUI]
		private void DrawTopBar()
		{
			Rect rect = SirenixEditorGUI.BeginHorizontalToolbar();
			Rect iconRect = rect.AlignLeft(20f).AlignMiddle(20f);
			iconRect.x += 8f;
			GUI.color = (((backendFlags & SerializationBackendFlags.Odin) != SerializationBackendFlags.None) ? Color.white : new Color(1f, 1f, 1f, 0.2f));
			GUI.DrawTexture(iconRect.Padding(2f), EditorIcons.OdinInspectorLogo, ScaleMode.ScaleToFit);
			iconRect.x += 28f;
			GUI.color = (((backendFlags & SerializationBackendFlags.Unity) != SerializationBackendFlags.None) ? Color.white : new Color(1f, 1f, 1f, 0.2f));
			GUI.DrawTexture(iconRect.Padding(2f), EditorIcons.UnityLogo, ScaleMode.ScaleToFit);
			GUI.color = Color.white;
			string typeName = "   " + ((targetType == null) ? "Select Type" : targetType.GetNiceName().SplitPascalCase()) + "   ";
			GUILayout.Space(iconRect.xMax + 3f);
			bool selectB = SirenixEditorGUI.ToolbarButton(new GUIContent(typeName));
			GUILayout.FlexibleSpace();
			bool selectA = SirenixEditorGUI.ToolbarButton(EditorIcons.TriangleDown);
			if (selectA || selectB)
			{
				Rect btnRect = GUIHelper.GetCurrentLayoutRect().HorizontalPadding(20f).AlignTop(20f);
				btnRect = btnRect.AlignRight(400f);
				IOrderedEnumerable<Type> source = from x in AssemblyUtilities.GetTypes(AssemblyCategory.ProjectSpecific)
					where !x.IsAbstract && x.IsClass && x.InheritsFrom<UnityEngine.Object>()
					where !x.Assembly.FullName.StartsWith("Sirenix")
					orderby AssemblyUtilities.GetAssemblyCategory(x.Assembly)
					orderby AssemblyUtilities.GetAssemblyCategory(x.Assembly), x.Namespace, x.Name descending
					select x;
				bool? showNoneItem = false;
				OdinSelector<Type> p = TypeSelectorHandler_WILL_BE_DEPRECATED.InstantiateSelector(source, supportsMultiSelect: false, null, null, showHidden: false, null, showNoneItem);
				p.SelectionChanged += delegate(IEnumerable<Type> types)
				{
					Type type = types.FirstOrDefault();
					if (type != null)
					{
						targetType = type;
						odinContext = targetType.IsDefined<ShowOdinSerializedPropertiesInInspectorAttribute>(inherit: true);
						CreateMenuTree(force: true);
					}
				};
				p.SetSelection(targetType);
				p.ShowInPopup(400f);
				if (Application.platform == RuntimePlatform.LinuxEditor)
				{
					GUIHelper.ExitGUI(removeFocusControl: true);
				}
			}
			SirenixEditorGUI.EndHorizontalToolbar();
		}

		private void CreateMenuTree(bool force)
		{
			if (!force && (!(targetType != null) || serializationInfoTree != null))
			{
				return;
			}
			EditorPrefs.SetString("SerializationDebuggerWindow.TargetType", TwoWaySerializationBinder.Default.BindToName(targetType));
			backendFlags = ((!targetType.IsDefined<ShowOdinSerializedPropertiesInInspectorAttribute>(inherit: true)) ? SerializationBackendFlags.Unity : SerializationBackendFlags.UnityAndOdin);
			List<MemberSerializationInfo> infos = MemberSerializationInfo.CreateSerializationOverview(targetType, backendFlags, odinContext);
			serializationInfoTree = new OdinMenuTree(supportsMultiSelect: false);
			serializationInfoTree.DefaultMenuStyle.Offset = 64f;
			serializationInfoTree.DefaultMenuStyle.Height = 27;
			serializationInfoTree.DefaultMenuStyle.BorderPadding = 0f;
			serializationInfoTree.Config.DrawSearchToolbar = true;
			serializationInfoTree.Config.AutoHandleKeyboardNavigation = true;
			foreach (MemberSerializationInfo item in infos)
			{
				serializationInfoTree.MenuItems.Add(new SerializationInfoMenuItem(serializationInfoTree, item.MemberInfo.Name, item));
			}
		}

		[OnInspectorGUI]
		private void DrawSerializationInfoTree()
		{
			EditorGUILayout.BeginVertical(GUILayoutOptions.ExpandHeight());
			CreateMenuTree(force: false);
			if (serializationInfoTree != null)
			{
				serializationInfoTree.DrawMenuTree();
			}
			EditorGUILayout.EndVertical();
		}

		[OnInspectorGUI]
		private void DrawInfos()
		{
			if (serializationInfoTree == null)
			{
				return;
			}
			if (noteStyle == null)
			{
				noteStyle = new GUIStyle(SirenixGUIStyles.MultiLineLabel);
				noteStyle.active.textColor = noteStyle.normal.textColor;
				noteStyle.onActive.textColor = noteStyle.normal.textColor;
				noteStyle.onFocused.textColor = noteStyle.normal.textColor;
				noteStyle.focused.textColor = noteStyle.normal.textColor;
				noteStyle.margin = new RectOffset(20, 4, 0, 4);
				noteStyle.padding = new RectOffset(0, 0, 0, 0);
			}
			if (serializationInfoTree.Selection.Count > 0)
			{
				MemberSerializationInfo info = serializationInfoTree.Selection[0].Value as MemberSerializationInfo;
				GUILayout.Space(10f);
				GUILayout.BeginHorizontal();
				Rect bgRect = GUIHelper.GetCurrentLayoutRect().Expand(0f, 10f);
				SirenixEditorGUI.DrawSolidRect(bgRect, SirenixGUIStyles.DarkEditorBackground);
				SirenixEditorGUI.DrawBorders(bgRect, 0, 0, 1, 0);
				GUILayout.BeginVertical(GUILayoutOptions.MinHeight(80f));
				string[] notes = info.Notes;
				foreach (string note in notes)
				{
					Rect noteRect = GUILayoutUtility.GetRect(GUIHelper.TempContent(note), noteStyle);
					Rect dot = noteRect;
					dot.x -= 8f;
					dot.y += 5f;
					dot.height = 4f;
					dot.width = 4f;
					SirenixEditorGUI.DrawSolidRect(dot, EditorGUIUtility.isProSkin ? Color.white : Color.black);
					EditorGUI.SelectableLabel(noteRect, note, noteStyle);
					GUILayout.Space(4f);
				}
				GUILayout.EndVertical();
				Rect r = GUIHelper.GetCurrentLayoutRect();
				SirenixEditorGUI.DrawVerticalLineSeperator(r.x, r.y, r.height);
				GUILayout.EndHorizontal();
				GUILayout.Space(10f);
			}
		}

		[OnInspectorGUI]
		private void DrawGettingStartedHelp()
		{
			if (targetType == null)
			{
				GUIContent content = GUIHelper.TempContent("Select your script here to begin debugging the serialization.", EditorIcons.UnityInfoIcon);
				Vector2 size = new Vector2(Mathf.Max(base.position.width - 100f, 200f), 0f);
				size.y = SirenixGUIStyles.MessageBox.CalcHeight(content, size.x);
				GUI.Label(new Rect(50f, 40f, size.x, size.y), content, SirenixGUIStyles.MessageBox);
				EditorIcons.ArrowUp.Draw(new Rect(95f, 25f, 20f, 20f), EditorIcons.ArrowUp.Raw);
			}
		}

		/// <summary>
		/// Opens the Serialization Debugger Window with the last debugged type.
		/// </summary>
		public static void ShowWindow()
		{
			Type target = null;
			string typeName = EditorPrefs.GetString("SerializationDebuggerWindow.TargetType", null);
			if (typeName != null)
			{
				target = TwoWaySerializationBinder.Default.BindToType(typeName);
			}
			ShowWindow(target);
		}

		/// <summary>
		/// Opens the Serialization Debugger Window and debugs the given type.
		/// </summary>
		/// <param name="type">The type to debug serialization of.</param>
		public static void ShowWindow(Type type)
		{
			SerializationDebuggerWindow window = Resources.FindObjectsOfTypeAll<SerializationDebuggerWindow>().FirstOrDefault();
			if (window == null)
			{
				window = EditorWindow.GetWindow<SerializationDebuggerWindow>("Serialization Debugger");
				window.position = GUIHelper.GetEditorWindowRect().AlignCenter(500f, 400f);
			}
			window.targetType = type;
			window.Show();
			if (window.targetType != null)
			{
				window.CreateMenuTree(force: true);
				window.Repaint();
			}
		}

		private static void ComponentContextMenuItem(MenuCommand menuCommand)
		{
			ShowWindow(menuCommand.context.GetType());
		}

		/// <summary>
		/// Initializes the Serialization Debugger Window.
		/// </summary>
		protected override void OnEnable()
		{
			base.OnEnable();
			WindowPadding = default(Vector4);
			base.minSize = new Vector2(300f, 300f);
		}
	}
}
