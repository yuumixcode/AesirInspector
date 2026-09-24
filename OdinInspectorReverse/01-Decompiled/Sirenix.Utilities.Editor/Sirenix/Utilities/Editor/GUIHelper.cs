using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Sirenix.OdinInspector.Editor;
using Sirenix.Reflection.Editor;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Sirenix.Utilities.Editor
{
	/// <summary>
	/// Various helper function for GUI.
	/// </summary>
	[InitializeOnLoad]
	public static class GUIHelper
	{
		[NonSerialized]
		private static object defaultConfigKey;

		private static readonly GUIContent tmpContent;

		private static readonly GUIScopeStack<bool> GUIEnabledStack;

		private static readonly GUIScopeStack<bool> HierarchyModeStack;

		private static readonly GUIScopeStack<bool> IsBoldLabelStack;

		private static readonly GUIScopeStack<Matrix4x4> MatrixStack;

		private static readonly GUIScopeStack<Color> ColorStack;

		private static readonly GUIScopeStack<EventType> EventTypeStack;

		private static readonly GUIScopeStack<Color> ContentColorStack;

		private static readonly GUIScopeStack<Color> LabelColorStack;

		private static readonly GUIScopeStack<int> IndentLevelStack;

		private static readonly GUIScopeStack<int> LayoutMeasureInfoStack;

		private static readonly GUIScopeStack<bool> ResponsiveVectorComponentFieldsStack;

		private static readonly GUIScopeStack<float> FadeGroupDurationStack;

		private static readonly GUIScopeStack<float> TabPageSlideAnimationDurationStack;

		private static readonly GUIScopeStack<bool> IsDrawingDictionaryKeyStack;

		private static readonly GUIScopeStack<float> labelWidthStack;

		private static readonly GUIScopeStack<float> ContextWidthStackOdinVersion;

		private static readonly Type HostViewType;

		private static readonly Func<object, EditorWindow> ActualViewGetter;

		private static readonly Func<Vector2> EditorScreenPointOffsetGetter;

		private static readonly Func<RectOffset> CurrentWindowBorderSizeGetter;

		private static readonly Func<bool> CurrentWindowHasFocusGetter;

		private static readonly Action<bool> SetBoldDefaultFontSetter;

		private static readonly Func<bool> GetBoldDefaultFontGetter;

		private static readonly Func<GUIStyle> GetTopLevelLayoutStyleGetter;

		private static readonly Func<EditorWindow, bool> GetIsDockedWindowGetter;

		private static readonly Func<Rect> GetEditorWindowRectGetter;

		private static readonly Type inspectorWindowType;

		private static readonly Func<float> ContextWidthGetter;

		private static readonly Action<float> ContextWidthSetter;

		private static readonly Func<Stack<float>> ContextWidthStackGetter;

		private static readonly Func<float> ActualLabelWidthGetter;

		private static float betterContextWidth;

		private static Type Type_UnityEngine_UI_Image;

		private static Type Type_UnityEngine_UI_RawImage;

		private static PropertyInfo Property_UnityEngine_UI_Image_Sprite;

		private static PropertyInfo Property_UnityEngine_UI_RawImage_Texture;

		internal static int confirmedPopupControlId;

		internal static int focusedControlId;

		internal static readonly Func<object> GUIViewGetter;

		private static Stopwatch smartProgressBarWatch;

		private static int smartProgressBarDisplaysSinceLastUpdate;

		/// <summary>
		/// Gets a value indicating whether a repaint has been requested.
		/// </summary>
		/// <value>
		///   <c>true</c> if repaint has been requested. Otherwise <c>false</c>.
		/// </value>
		public static bool RepaintRequested;

		/// <summary>
		/// Whether the inspector is currently in the progress of drawing a dictionary key.
		/// </summary>
		public static bool IsDrawingDictionaryKey
		{
			get
			{
				if (IsDrawingDictionaryKeyStack.Count != 0)
				{
					return IsDrawingDictionaryKeyStack.Peek();
				}
				return false;
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether labels are currently bold.
		/// </summary>
		/// <value>
		/// <c>true</c> if this instance is bold label; otherwise, <c>false</c>.
		/// </value>
		public static bool IsBoldLabel
		{
			get
			{
				return GetBoldDefaultFontGetter();
			}
			set
			{
				SetBoldDefaultFontSetter(value);
			}
		}

		/// <summary>
		/// Gets the size of the current window border.
		/// </summary>
		/// <value>
		/// The size of the current window border.
		/// </value>
		public static RectOffset CurrentWindowBorderSize => CurrentWindowBorderSizeGetter();

		/// <summary>
		/// Gets the editor screen point offset.
		/// </summary>
		/// <value>
		/// The editor screen point offset.
		/// </value>
		public static Vector2 EditorScreenPointOffset => EditorScreenPointOffsetGetter();

		/// <summary>
		/// Gets the current editor gui context width. Only set these if you know what it does.
		/// Setting this has been removed. Use PushContextWidth and PopContextWidth instead.
		/// </summary>
		public static float ContextWidth => ContextWidthGetter();

		/// <summary>
		/// Unity EditorGUIUtility.labelWidth only works reliablly in Repaint events.
		/// BetterLabelWidth does a better job at giving you the correct LabelWidth in non-repaint events.
		/// </summary>
		public static float BetterLabelWidth
		{
			get
			{
				if (BetterContextWidth == 0f)
				{
					return EditorGUIUtility.labelWidth;
				}
				PushContextWidth(BetterContextWidth);
				float val = EditorGUIUtility.labelWidth;
				PopContextWidth();
				return val;
			}
			set
			{
				EditorGUIUtility.labelWidth = value;
			}
		}

		/// <summary>
		/// Odin will set this for you whenever an Odin property tree is drawn.
		/// But if you're using BetterLabelWidth and BetterContextWidth without Odin, then
		/// you need to set BetterContextWidth in the beginning of each GUIEvent.
		/// </summary>
		public static float BetterContextWidth
		{
			get
			{
				if (betterContextWidth == 0f)
				{
					return ContextWidth;
				}
				return betterContextWidth;
			}
			set
			{
				betterContextWidth = value;
			}
		}

		/// <summary>
		/// Gets the current indent amount.
		/// </summary>
		/// <value>
		/// The current indent amount.
		/// </value>
		public static float CurrentIndentAmount => EditorGUI.indentLevel * 15;

		/// <summary>
		/// Gets the mouse screen position.
		/// </summary>
		/// <value>
		/// The mouse screen position.
		/// </value>
		public static Vector2 MouseScreenPosition => GUIUtility.GUIToScreenPoint(Event.current.mousePosition);

		/// <summary>
		/// Gets the current editor window.
		/// </summary>
		/// <value>
		/// The current editor window.
		/// </value>
		public static EditorWindow CurrentWindow
		{
			get
			{
				object view = GUIViewGetter();
				if (view == null)
				{
					return null;
				}
				if (!HostViewType.IsAssignableFrom(view.GetType()))
				{
					return null;
				}
				return ActualViewGetter(view);
			}
		}

		/// <summary>
		/// Gets a value indicating whether the current editor window is focused.
		/// </summary>
		/// <value>
		/// <c>true</c> if the current window has focus. Otherwise, <c>false</c>.
		/// </value>
		public static bool CurrentWindowHasFocus => CurrentWindowHasFocusGetter();

		/// <summary>
		/// Gets the Entity Id of the current editor window.
		/// </summary>
		/// <value>
		/// The Entity Id of the current editor window.
		/// </value>
		public static OdinEntityId CurrentWindowEntityId
		{
			get
			{
				try
				{
					UnityEngine.Object current = GUIView_Internals.CurrentAsObject;
					if (current == null)
					{
						return OdinEntityId.None;
					}
					return OdinEntityId.FromObject(current);
				}
				catch
				{
					return OdinEntityId.None;
				}
			}
		}

		/// <summary>
		/// Gets the ID of the current editor window.
		/// </summary>
		/// <value>
		/// The ID of the current editor window.
		/// </value>
		[Obsolete("Use CurrentWindowEntityId instead.", false)]
		public static int CurrentWindowInstanceID
		{
			get
			{
				OdinEntityId result = CurrentWindowEntityId;
				if (result == OdinEntityId.None)
				{
					return -1;
				}
				return OdinEntityId.Internal.ToInstanceId(result);
			}
		}

		/// <summary>
		/// Gets or sets the actual EditorGUIUtility.LabelWidth, regardless of the current hierarchy mode or context width.
		/// </summary>
		public static float ActualLabelWidth
		{
			get
			{
				return ActualLabelWidthGetter();
			}
			set
			{
				BetterLabelWidth = value;
			}
		}

		/// <summary>
		/// Gets the bold default font.
		/// </summary>
		public static bool GetBoldDefaultFont()
		{
			return GetBoldDefaultFontGetter();
		}

		static GUIHelper()
		{
			defaultConfigKey = new object();
			tmpContent = new GUIContent("");
			GUIEnabledStack = new GUIScopeStack<bool>();
			HierarchyModeStack = new GUIScopeStack<bool>();
			IsBoldLabelStack = new GUIScopeStack<bool>();
			MatrixStack = new GUIScopeStack<Matrix4x4>();
			ColorStack = new GUIScopeStack<Color>();
			EventTypeStack = new GUIScopeStack<EventType>();
			ContentColorStack = new GUIScopeStack<Color>();
			LabelColorStack = new GUIScopeStack<Color>();
			IndentLevelStack = new GUIScopeStack<int>();
			LayoutMeasureInfoStack = new GUIScopeStack<int>();
			ResponsiveVectorComponentFieldsStack = new GUIScopeStack<bool>();
			FadeGroupDurationStack = new GUIScopeStack<float>();
			TabPageSlideAnimationDurationStack = new GUIScopeStack<float>();
			IsDrawingDictionaryKeyStack = new GUIScopeStack<bool>();
			labelWidthStack = new GUIScopeStack<float>();
			ContextWidthStackOdinVersion = new GUIScopeStack<float>();
			Type_UnityEngine_UI_Image = AssemblyUtilities.GetTypeByCachedFullName("UnityEngine.UI.Image");
			Type_UnityEngine_UI_RawImage = AssemblyUtilities.GetTypeByCachedFullName("UnityEngine.UI.RawImage");
			Property_UnityEngine_UI_Image_Sprite = ((Type_UnityEngine_UI_Image == null) ? null : Type_UnityEngine_UI_Image.GetProperty("sprite"));
			Property_UnityEngine_UI_RawImage_Texture = ((Type_UnityEngine_UI_RawImage == null) ? null : Type_UnityEngine_UI_RawImage.GetProperty("texture"));
			confirmedPopupControlId = -1;
			focusedControlId = -1;
			smartProgressBarWatch = Stopwatch.StartNew();
			smartProgressBarDisplaysSinceLastUpdate = 0;
			HostViewType = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.HostView");
			Type guiLayoutEntryType = typeof(GUI).Assembly.GetType("UnityEngine.GUILayoutEntry");
			Type guiViewType = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.GUIView");
			inspectorWindowType = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.InspectorWindow");
			EditorScreenPointOffsetGetter = DeepReflection.CreateValueGetter<Vector2>(typeof(GUIUtility), "s_EditorScreenPointOffset", allowEmit: false);
			CurrentWindowHasFocusGetter = DeepReflection.CreateValueGetter<bool>(guiViewType, "current.hasFocus", allowEmit: false);
			GetIsDockedWindowGetter = DeepReflection.CreateValueGetter<EditorWindow, bool>("docked", allowEmit: false);
			ContextWidthGetter = DeepReflection.CreateValueGetter<float>(typeof(EditorGUIUtility), "contextWidth", allowEmit: false);
			FieldInfo contextWidthField = typeof(EditorGUIUtility).GetField("s_ContextWidth", BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
			if (contextWidthField != null)
			{
				ContextWidthSetter = EmitUtilities.CreateStaticFieldSetter<float>(contextWidthField);
			}
			else
			{
				contextWidthField = typeof(EditorGUIUtility).GetField("s_ContextWidthStack", BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
				ContextWidthStackGetter = EmitUtilities.CreateStaticFieldGetter<Stack<float>>(contextWidthField);
			}
			ActualLabelWidthGetter = EmitUtilities.CreateStaticFieldGetter<float>(typeof(EditorGUIUtility).GetField("s_LabelWidth", BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy));
			GUIViewGetter = DeepReflection.CreateWeakStaticValueGetter(guiViewType, guiViewType, "current", allowEmit: false);
			ActualViewGetter = DeepReflection.CreateWeakInstanceValueGetter<EditorWindow>(HostViewType, "actualView", allowEmit: false);
			Func<object, RectOffset> borderSizeGetter = DeepReflection.CreateWeakInstanceValueGetter<RectOffset>(HostViewType, "borderSize", allowEmit: false);
			CurrentWindowBorderSizeGetter = () => borderSizeGetter(GUIViewGetter());
			SetBoldDefaultFontSetter = (Action<bool>)Delegate.CreateDelegate(typeof(Action<bool>), typeof(EditorGUIUtility).GetMethod("SetBoldDefaultFont", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, null, new Type[1] { typeof(bool) }, null));
			GetBoldDefaultFontGetter = (Func<bool>)Delegate.CreateDelegate(typeof(Func<bool>), typeof(EditorGUIUtility).GetMethod("GetBoldDefaultFont", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, null, Type.EmptyTypes, null));
			GetTopLevelLayoutStyleGetter = DeepReflection.CreateValueGetter<GUIStyle>(typeof(GUILayoutUtility), "current.topLevel.style", allowEmit: false);
			GetEditorWindowRectGetter = DeepReflection.CreateValueGetter<Rect>(typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.Toolbar"), "get.parent.screenPosition", allowEmit: false);
		}

		public static void DisplaySmartUpdatingProgressBar(string title, string details, float progress, int updateIntervalByMS = 200, int updateIntervalByCall = 50)
		{
			if (smartProgressBarWatch.ElapsedMilliseconds >= updateIntervalByMS || ++smartProgressBarDisplaysSinceLastUpdate >= updateIntervalByCall)
			{
				smartProgressBarWatch.Stop();
				smartProgressBarWatch.Reset();
				smartProgressBarWatch.Start();
				smartProgressBarDisplaysSinceLastUpdate = 0;
				EditorUtility.DisplayProgressBar(title, details, progress);
			}
		}

		public static void DrawLastControlId([CallerMemberName] string name = null, [CallerLineNumber] int lineNumber = 0)
		{
			GUILayout.Label($"{EditorGUIUtility_Internals.LastControlID} : {name} : {lineNumber}");
		}

		public static bool DisplaySmartUpdatingCancellableProgressBar(string title, string details, float progress, int updateIntervalByMS, int updateIntervalByCall = 50)
		{
			if (smartProgressBarWatch.ElapsedMilliseconds >= updateIntervalByMS || ++smartProgressBarDisplaysSinceLastUpdate >= updateIntervalByCall)
			{
				smartProgressBarWatch.Stop();
				smartProgressBarWatch.Reset();
				smartProgressBarWatch.Start();
				smartProgressBarDisplaysSinceLastUpdate = 0;
				if (EditorUtility.DisplayCancelableProgressBar(title, details, progress))
				{
					return true;
				}
			}
			return false;
		}

		public static bool ShouldDisplaySmartCancellableProgressBar(int updateIntervalByMS = 150)
		{
			return smartProgressBarWatch.ElapsedMilliseconds >= updateIntervalByMS;
		}

		public static bool DisplaySmartUpdatingCancellableProgressBar(string title, string details, float progress, int updateIntervalByMS = 150)
		{
			if (ShouldDisplaySmartCancellableProgressBar(updateIntervalByMS))
			{
				smartProgressBarWatch.Stop();
				smartProgressBarWatch.Reset();
				smartProgressBarWatch.Start();
				smartProgressBarDisplaysSinceLastUpdate = 0;
				if (EditorUtility.DisplayCancelableProgressBar(title, details, progress))
				{
					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// An alternative to GUI.FocusControl(null), which does not take focus away from the current GUI.Window.
		/// </summary>
		public static void RemoveFocusControl()
		{
			GUIUtility.hotControl = 0;
			DragAndDrop.activeControlID = 0;
			GUIUtility.keyboardControl = 0;
			focusedControlId = 0;
			confirmedPopupControlId = 0;
		}

		/// <summary>
		/// Hides the following draw calls. Remember to call <see cref="M:Sirenix.Utilities.Editor.GUIHelper.EndDrawToNothing" /> when done.
		/// </summary>
		public static void BeginDrawToNothing()
		{
			PushGUIEnabled(enabled: false);
			GUILayout.BeginArea(new Rect(-9999f, -9999f, 1f, 1f), SirenixGUIStyles.None);
			GUILayout.BeginVertical();
		}

		/// <summary>
		/// Unhides the following draw calls after having called <see cref="M:Sirenix.Utilities.Editor.GUIHelper.BeginDrawToNothing" />.
		/// </summary>
		public static void EndDrawToNothing()
		{
			GUILayout.EndVertical();
			GUILayout.EndArea();
			PopGUIEnabled();
		}

		/// <summary>
		/// Determines whether the specified EditorWindow is docked.
		/// </summary>
		/// <param name="window">The editor window.</param>
		/// <returns><c>true</c> if the editor window is docked. Otherwise <c>false</c>.</returns>
		public static bool IsDockedWindow(EditorWindow window)
		{
			return GetIsDockedWindowGetter(window);
		}

		/// <summary>
		/// Not yet documented.
		/// </summary>
		public static Rect GetEditorWindowRect()
		{
			return GetEditorWindowRectGetter();
		}

		/// <summary>
		/// Opens a new inspector window for the specified object.
		/// </summary>
		/// <param name="unityObj">The unity object.</param>
		/// <exception cref="T:System.ArgumentNullException">unityObj</exception>
		public static void OpenInspectorWindow(UnityEngine.Object unityObj)
		{
			if (unityObj == null)
			{
				throw new ArgumentNullException("unityObj");
			}
			Rect windowRect = GetEditorWindowRect();
			Vector2 windowSize = new Vector2(450f, Mathf.Min(windowRect.height * 0.7f, 500f));
			UnityShims.Rect.Ctor(out var rect, windowRect.center - windowSize * 0.5f, windowSize);
			EditorWindow inspectorInstance = ScriptableObject.CreateInstance(inspectorWindowType) as EditorWindow;
			inspectorInstance.Show();
			UnityEngine.Object[] prevSelection = Selection.objects;
			Selection.activeObject = unityObj;
			inspectorWindowType.GetProperty("isLocked", BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy).GetSetMethod().Invoke(inspectorInstance, new object[1] { true });
			Selection.objects = prevSelection;
			inspectorInstance.position = rect;
			if (!unityObj.GetType().InheritsFrom(typeof(Texture2D)))
			{
				return;
			}
			UnityEditorEventUtility.EditorApplication_delayCall += delegate
			{
				UnityEditorEventUtility.EditorApplication_delayCall += delegate
				{
					Selection.objects = prevSelection;
				};
			};
		}

		internal static void OpenEditorInOdinDropDown(UnityEngine.Object obj, Rect btnRect)
		{
			Type odinEditorWindow = AssemblyUtilities.GetTypeByCachedFullName("Sirenix.OdinInspector.Editor.OdinEditorWindow");
			odinEditorWindow.GetMethods(BindingFlags.Static | BindingFlags.Public).First((MethodInfo x) => x.Name == "InspectObjectInDropDown" && x.GetParameters().Last().ParameterType == typeof(float)).Invoke(null, new object[3] { obj, btnRect, 400 });
		}

		/// <summary>
		/// Requests a repaint.
		/// </summary>
		public static void RequestRepaint()
		{
			RepaintRequested = true;
		}

		/// <summary>
		/// Calls <see cref="M:UnityEditor.HandleUtility.Repaint" />, if the <see cref="P:Sirenix.Utilities.Editor.GUIHelper.CurrentWindow" /> is not NULL.
		/// </summary>
		public static void SafeHandleUtilityRepaint()
		{
			if (CurrentWindow != null)
			{
				HandleUtility.Repaint();
			}
		}

		/// <summary>
		/// Requests a repaint.
		/// </summary>
		[Obsolete("RequestRepaint with numberOfFramesToRepaint is no longer supported. Use RequestRepaint() Instead.", false)]
		public static void RequestRepaint(int numberOfFramesToRepaint)
		{
			RepaintRequested = true;
		}

		/// <summary>
		/// Begins the layout measuring. Remember to end with <see cref="M:Sirenix.Utilities.Editor.GUIHelper.EndLayoutMeasuring(System.Int32)" />.
		/// </summary>
		public static void BeginLayoutMeasuring()
		{
			if (Event.current.type != EventType.Layout)
			{
				LayoutMeasureInfoStack.Push(GUILayoutUtility_Internals.TopLevel.Cursor);
			}
		}

		/// <summary>
		/// Begins the layout measuring. Remember to end with <see cref="M:Sirenix.Utilities.Editor.GUIHelper.EndLayoutMeasuring(System.Int32)" />.
		/// </summary>
		public static void BeginLayoutMeasuring(out int cursor)
		{
			if (Event.current.type != EventType.Layout)
			{
				cursor = GUILayoutUtility_Internals.TopLevel.Cursor;
			}
			else
			{
				cursor = 0;
			}
		}

		/// <summary>
		/// Ends the layout measuring started by <see cref="M:Sirenix.Utilities.Editor.GUIHelper.EndLayoutMeasuring(System.Int32)" />
		/// </summary>
		/// <returns>The measured rect.</returns>
		public static Rect EndLayoutMeasuring(int cursor)
		{
			if (Event.current.type != EventType.Layout)
			{
				int to = GUILayoutUtility_Internals.TopLevel.Cursor;
				return GUILayoutUtility_Internals.MeasureLayout(cursor, to);
			}
			return new Rect(0f, 0f, 0f, 0f);
		}

		/// <summary>
		/// Ends the layout measuring started by <see cref="M:Sirenix.Utilities.Editor.GUIHelper.EndLayoutMeasuring(System.Int32)" />
		/// </summary>
		/// <returns>The measured rect.</returns>
		public static Rect EndLayoutMeasuring()
		{
			if (Event.current.type != EventType.Layout)
			{
				int from = LayoutMeasureInfoStack.Pop();
				int to = GUILayoutUtility_Internals.TopLevel.Cursor;
				return GUILayoutUtility_Internals.MeasureLayout(from, to);
			}
			return new Rect(0f, 0f, 0f, 0f);
		}

		/// <summary>
		/// Gets the current layout rect.
		/// </summary>
		/// <returns>The current layout rect.</returns>
		public static Rect GetCurrentLayoutRect()
		{
			return GUILayoutUtility_Internals.TopLevel.Rect;
		}

		/// <summary>
		/// Gets the current layout rect.
		/// </summary>
		/// <returns>The current layout rect.</returns>
		public static GUIStyle GetCurrentLayoutStyle()
		{
			try
			{
				return GetTopLevelLayoutStyleGetter() ?? GUIStyle.none;
			}
			catch
			{
				return GUIStyle.none;
			}
		}

		/// <summary>
		/// Gets the playmode color tint.
		/// </summary>
		/// <returns>The playmode color tint.</returns>
		public static Color GetPlaymodeTint()
		{
			Color tint = Color.white;
			string[] a = EditorPrefs.GetString("Playmode tint", "playmode tint;1;1;1;1").Split(new char[1] { ';' });
			float.TryParse(a[1], out tint.r);
			float.TryParse(a[2], out tint.g);
			float.TryParse(a[3], out tint.b);
			float.TryParse(a[4], out tint.a);
			return tint;
		}

		/// <summary>
		/// Pushes a context width to the context width stack.
		/// Remember to pop the value again with <see cref="M:Sirenix.Utilities.Editor.GUIHelper.PopContextWidth" />.
		/// </summary>
		/// <param name="width">The width to push.</param>
		public static void PushContextWidth(float width)
		{
			if (ContextWidthSetter != null)
			{
				ContextWidthStackOdinVersion.Push(width);
				ContextWidthSetter(width);
			}
			else
			{
				Stack<float> unityStack = ContextWidthStackGetter();
				unityStack.Push(width);
			}
		}

		/// <summary>
		/// Pops a value pushed by <see cref="M:Sirenix.Utilities.Editor.GUIHelper.PushContextWidth(System.Single)" />.
		/// </summary>
		public static void PopContextWidth()
		{
			if (ContextWidthSetter != null)
			{
				float width = ContextWidthStackOdinVersion.Pop();
				ContextWidthSetter(width);
				return;
			}
			Stack<float> unityStack = ContextWidthStackGetter();
			if (unityStack.Count > 0)
			{
				unityStack.Pop();
			}
		}

		/// <summary>
		/// Pushes a color to the GUI color stack. Remember to pop the color with <see cref="M:Sirenix.Utilities.Editor.GUIHelper.PopColor" />.
		/// </summary>
		/// <param name="color">The color to push the GUI color..</param>
		/// <param name="blendAlpha">if set to <c>true</c> blend with alpha.</param>
		public static void PushColor(Color color, bool blendAlpha = false)
		{
			ColorStack.Push(GUI.color);
			if (blendAlpha)
			{
				color.a *= GUI.color.a;
			}
			GUI.color = color;
		}

		/// <summary>
		/// Takes a screenshot of the GUI within the specified rect.
		/// </summary>
		/// <param name="rect">The rect.</param>
		/// <returns>The screenshot as a render texture.</returns>
		public static RenderTexture TakeGUIScreenshot(Rect rect)
		{
			RenderTexture rt = RenderTexture.GetTemporary((int)rect.width, (int)rect.height);
			Graphics.Blit(RenderTexture.active, rt);
			return rt;
		}

		/// <summary>
		/// Pops the GUI color pushed by <see cref="M:Sirenix.Utilities.Editor.GUIHelper.PushColor(UnityEngine.Color,System.Boolean)" />.
		/// </summary>
		public static void PopColor()
		{
			GUI.color = ColorStack.Pop();
		}

		/// <summary>
		/// Pushes a state to the GUI enabled stack. Remember to pop the state with <see cref="M:Sirenix.Utilities.Editor.GUIHelper.PopGUIEnabled" />.
		/// </summary>
		/// <param name="enabled">If set to <c>true</c> GUI will be enabled. Otherwise GUI will be disabled.</param>
		public static void PushGUIEnabled(bool enabled)
		{
			GUIEnabledStack.Push(GUI.enabled);
			GUI.enabled = enabled;
		}

		/// <summary>
		/// Pops the GUI enabled pushed by <see cref="M:Sirenix.Utilities.Editor.GUIHelper.PushGUIEnabled(System.Boolean)" />
		/// </summary>
		public static void PopGUIEnabled()
		{
			GUI.enabled = GUIEnabledStack.Pop();
		}

		/// <summary>
		/// Pushes a state to the IsDrawingDictionaryKey stack. Remember to pop the state with <see cref="M:Sirenix.Utilities.Editor.GUIHelper.PopIsDrawingDictionaryKey" />.
		/// </summary>
		public static void PushIsDrawingDictionaryKey(bool enabled)
		{
			IsDrawingDictionaryKeyStack.Push(enabled);
		}

		/// <summary>
		/// Pops the state pushed by <see cref="M:Sirenix.Utilities.Editor.GUIHelper.PushIsDrawingDictionaryKey(System.Boolean)" />
		/// </summary>
		public static void PopIsDrawingDictionaryKey()
		{
			IsDrawingDictionaryKeyStack.Pop();
		}

		/// <summary>
		/// Pushes the hierarchy mode to the stack. Remember to pop the state with <see cref="M:Sirenix.Utilities.Editor.GUIHelper.PopHierarchyMode" />.
		/// </summary>
		/// <param name="hierarchyMode">The hierachy mode state to push.</param>
		/// <param name="preserveCurrentLabelWidth">Changing hierachy mode also changes how label-widths are calcualted. By default, we try to keep the current label width.</param>
		public static void PushHierarchyMode(bool hierarchyMode, bool preserveCurrentLabelWidth = true)
		{
			float actualLabelWidth = ActualLabelWidth;
			labelWidthStack.Push(actualLabelWidth);
			float currentLabelWidth = (preserveCurrentLabelWidth ? BetterLabelWidth : actualLabelWidth);
			HierarchyModeStack.Push(EditorGUIUtility.hierarchyMode);
			EditorGUIUtility.hierarchyMode = hierarchyMode;
			BetterLabelWidth = currentLabelWidth;
		}

		/// <summary>
		/// Pops the hierarchy mode pushed by <see cref="!:PushHierarchyMode(bool)" />.
		/// </summary>
		public static void PopHierarchyMode()
		{
			EditorGUIUtility.hierarchyMode = HierarchyModeStack.Pop();
			ActualLabelWidth = labelWidthStack.Pop();
		}

		/// <summary>
		/// Pushes bold label state to the stack. Remember to pop with <see cref="M:Sirenix.Utilities.Editor.GUIHelper.PopIsBoldLabel" />.
		/// </summary>
		/// <param name="isBold">Value indicating if labels should be bold or not.</param>
		public static void PushIsBoldLabel(bool isBold)
		{
			IsBoldLabelStack.Push(IsBoldLabel);
			IsBoldLabel = isBold;
		}

		/// <summary>
		/// Pops the bold label state pushed by <see cref="M:Sirenix.Utilities.Editor.GUIHelper.PushIsBoldLabel(System.Boolean)" />.
		/// </summary>
		public static void PopIsBoldLabel()
		{
			IsBoldLabel = IsBoldLabelStack.Pop();
		}

		/// <summary>
		/// Pushes the indent level to the stack. Remember to pop with <see cref="M:Sirenix.Utilities.Editor.GUIHelper.PopIndentLevel" />.
		/// </summary>
		/// <param name="indentLevel">The indent level to push.</param>
		public static void PushIndentLevel(int indentLevel)
		{
			IndentLevelStack.Push(EditorGUI.indentLevel);
			EditorGUI.indentLevel = indentLevel;
		}

		/// <summary>
		/// Pops the indent level pushed by <see cref="M:Sirenix.Utilities.Editor.GUIHelper.PushIndentLevel(System.Int32)" />.
		/// </summary>
		public static void PopIndentLevel()
		{
			EditorGUI.indentLevel = IndentLevelStack.Pop();
		}

		/// <summary>
		/// Pushes the content color to the stack. Remember to pop with <see cref="M:Sirenix.Utilities.Editor.GUIHelper.PopContentColor" />.
		/// </summary>
		/// <param name="color">The content color to push..</param>
		/// <param name="blendAlpha">If set to <c>true</c> blend with alpha.</param>
		public static void PushContentColor(Color color, bool blendAlpha = false)
		{
			ContentColorStack.Push(GUI.contentColor);
			if (blendAlpha)
			{
				color.a *= GUI.contentColor.a;
			}
			GUI.contentColor = color;
		}

		/// <summary>
		/// Pops the content color pushed by <see cref="M:Sirenix.Utilities.Editor.GUIHelper.PushContentColor(UnityEngine.Color,System.Boolean)" />.
		/// </summary>
		public static void PopContentColor()
		{
			GUI.contentColor = ContentColorStack.Pop();
		}

		/// <summary>
		/// Pushes the label color to the stack. Remember to pop with <see cref="M:Sirenix.Utilities.Editor.GUIHelper.PopLabelColor" />.
		/// </summary>
		/// <param name="color">The label color to push.</param>
		public static void PushLabelColor(Color color)
		{
			LabelColorStack.Push(EditorStyles.label.normal.textColor);
			EditorStyles.label.normal.textColor = color;
			SirenixGUIStyles.Foldout.normal.textColor = color;
			SirenixGUIStyles.Foldout.onNormal.textColor = color;
		}

		/// <summary>
		/// Pops the label color pushed by <see cref="M:Sirenix.Utilities.Editor.GUIHelper.PushLabelColor(UnityEngine.Color)" />.
		/// </summary>
		public static void PopLabelColor()
		{
			Color color = LabelColorStack.Pop();
			EditorStyles.label.normal.textColor = color;
			SirenixGUIStyles.Foldout.normal.textColor = color;
			SirenixGUIStyles.Foldout.onNormal.textColor = color;
		}

		/// <summary>
		/// Pushes the GUI position offset to the stack. Remember to pop with <see cref="M:Sirenix.Utilities.Editor.GUIHelper.PopGUIPositionOffset" />.
		/// </summary>
		/// <param name="offset">The GUI offset.</param>
		public static void PushGUIPositionOffset(Vector2 offset)
		{
			PushMatrix(GUI.matrix * Matrix4x4.TRS(UnityShims.Vector2.op_Implicit(offset), Quaternion.identity, Vector3.one));
		}

		/// <summary>
		/// Pops the GUI position offset pushed by <see cref="M:Sirenix.Utilities.Editor.GUIHelper.PushGUIPositionOffset(UnityEngine.Vector2)" />.
		/// </summary>
		public static void PopGUIPositionOffset()
		{
			PopMatrix();
		}

		/// <summary>
		/// Pushes a GUI matrix to the stack. Remember to pop with <see cref="M:Sirenix.Utilities.Editor.GUIHelper.PopMatrix" />.
		/// </summary>
		/// <param name="matrix">The GUI matrix to push.</param>
		public static void PushMatrix(Matrix4x4 matrix)
		{
			MatrixStack.Push(GUI.matrix);
			GUI.matrix = matrix;
		}

		/// <summary>
		/// Pops the GUI matrix pushed by <see cref="M:Sirenix.Utilities.Editor.GUIHelper.PushMatrix(UnityEngine.Matrix4x4)" />.
		/// </summary>
		public static void PopMatrix()
		{
			GUI.matrix = MatrixStack.Pop();
		}

		/// <summary>
		/// Ignores input on following GUI calls. Remember to end with <see cref="M:Sirenix.Utilities.Editor.GUIHelper.EndIgnoreInput" />.
		/// </summary>
		public static void BeginIgnoreInput()
		{
			EventType e = Event.current.type;
			PushEventType((e == EventType.Layout || e == EventType.Repaint || e == EventType.Used) ? e : EventType.Ignore);
		}

		/// <summary>
		/// Ends the ignore input started by <see cref="M:Sirenix.Utilities.Editor.GUIHelper.BeginIgnoreInput" />.
		/// </summary>
		public static void EndIgnoreInput()
		{
			PopEventType();
		}

		/// <summary>
		/// Pushes the event type to the stack. Remember to pop with <see cref="M:Sirenix.Utilities.Editor.GUIHelper.PopEventType" />.
		/// </summary>
		/// <param name="eventType">The type of event to push.</param>
		public static void PushEventType(EventType eventType)
		{
			EventTypeStack.Push(Event.current.type);
			Event.current.type = eventType;
		}

		/// <summary>
		/// Pops the event type pushed by <see cref="M:Sirenix.Utilities.Editor.GUIHelper.PopEventType" />.
		/// </summary>
		public static void PopEventType()
		{
			Event.current.type = EventTypeStack.Pop();
		}

		/// <summary>
		/// Pushes the width to the editor GUI label width to the stack. Remmeber to Pop with <see cref="M:Sirenix.Utilities.Editor.GUIHelper.PopLabelWidth" />.
		/// </summary>
		/// <param name="labelWidth">The editor GUI label width to push.</param>
		public static void PushLabelWidth(float labelWidth)
		{
			labelWidthStack.Push(ActualLabelWidth);
			BetterLabelWidth = labelWidth;
		}

		/// <summary>
		/// Pops editor gui label widths pushed by <see cref="M:Sirenix.Utilities.Editor.GUIHelper.PushLabelWidth(System.Single)" />.
		/// </summary>
		public static void PopLabelWidth()
		{
			BetterLabelWidth = labelWidthStack.Pop();
		}

		/// <summary>
		/// Pushes the value to the responsive vector component fields stack. Remeber to pop with <see cref="M:Sirenix.Utilities.Editor.GUIHelper.PopResponsiveVectorComponentFields" />.
		/// </summary>
		public static void PushResponsiveVectorComponentFields(bool responsive)
		{
			ResponsiveVectorComponentFieldsStack.Push(SirenixEditorFields.ResponsiveVectorComponentFields);
			SirenixEditorFields.ResponsiveVectorComponentFields = responsive;
		}

		/// <summary>
		/// Pops responsive vector component fields value pushed by <see cref="M:Sirenix.Utilities.Editor.GUIHelper.PushResponsiveVectorComponentFields(System.Boolean)" />.
		/// </summary>
		public static void PopResponsiveVectorComponentFields()
		{
			SirenixEditorFields.ResponsiveVectorComponentFields = ResponsiveVectorComponentFieldsStack.Pop();
		}

		/// <summary>
		/// Pushes the value to the fade group duration stack. Remeber to pop with <see cref="M:Sirenix.Utilities.Editor.GUIHelper.PopFadeGroupDuration" />.
		/// </summary>
		public static void PushFadeGroupDuration(float duration)
		{
			FadeGroupDurationStack.Push(SirenixEditorGUI.DefaultFadeGroupDuration);
			SirenixEditorGUI.DefaultFadeGroupDuration = duration;
		}

		/// <summary>
		/// Pops fade group duration value pushed by <see cref="M:Sirenix.Utilities.Editor.GUIHelper.PushFadeGroupDuration(System.Single)" />.
		/// </summary>
		public static void PopFadeGroupDuration()
		{
			SirenixEditorGUI.DefaultFadeGroupDuration = FadeGroupDurationStack.Pop();
		}

		/// <summary>
		/// Pushes the value to the tab page slide animation duration stack. Remember to pop with <see cref="M:Sirenix.Utilities.Editor.GUIHelper.PopTabPageSlideAnimationDuration" />.
		/// </summary>
		/// <param name="duration"></param>
		public static void PushTabPageSlideAnimationDuration(float duration)
		{
			TabPageSlideAnimationDurationStack.Push(SirenixEditorGUI.TabPageSlideAnimationDuration);
			SirenixEditorGUI.TabPageSlideAnimationDuration = duration;
		}

		/// <summary>
		/// Pops tab page slide animation duration value pushed by <see cref="M:Sirenix.Utilities.Editor.GUIHelper.PushTabPageSlideAnimationDuration(System.Single)" />.
		/// </summary>
		public static void PopTabPageSlideAnimationDuration()
		{
			SirenixEditorGUI.TabPageSlideAnimationDuration = TabPageSlideAnimationDurationStack.Pop();
		}

		/// <summary>
		/// Clears the repaint request.
		/// </summary>
		public static void ClearRepaintRequest()
		{
			RepaintRequested = false;
		}

		/// <summary>
		/// Gets a temporary value context.
		/// </summary>
		/// <typeparam name="TValue">The type of the config value.</typeparam>
		/// <param name="key">The key for the config.</param>
		/// <param name="name">The name of the config.</param>
		/// <returns>GUIConfig for the specified key and name.</returns>
		public static GUIContext<TValue> GetTemporaryContext<TValue>(object key, string name) where TValue : class, new()
		{
			GUIContext<TValue> config = GUIContextCache<object, string, TValue>.GetConfig(key, name);
			if (!config.HasValue)
			{
				config.HasValue = true;
				config.Value = new TValue();
			}
			return config;
		}

		/// <summary>
		/// Gets a temporary value context.
		/// </summary>
		/// <typeparam name="TValue">The type of the value.</typeparam>
		/// <param name="key">The key for the config.</param>
		/// <param name="id">The ID for the config.</param>
		/// <returns>GUIConfig for the specified key and ID.</returns>
		public static GUIContext<TValue> GetTemporaryContext<TValue>(object key, int id) where TValue : class, new()
		{
			GUIContext<TValue> config = GUIContextCache<object, int, TValue>.GetConfig(key, id);
			if (!config.HasValue)
			{
				config.HasValue = true;
				config.Value = new TValue();
			}
			return config;
		}

		/// <summary>
		/// Gets a temporary value context.
		/// </summary>
		/// <typeparam name="TValue">The type of the value.</typeparam>
		/// <param name="primaryKey">The primary key.</param>
		/// <param name="secondaryKey">The secondary key.</param>
		/// <returns>GUIConfig for the specified primary and secondary key.</returns>
		public static GUIContext<TValue> GetTemporaryContext<TValue>(object primaryKey, object secondaryKey) where TValue : class, new()
		{
			GUIContext<TValue> config = GUIContextCache<object, object, TValue>.GetConfig(primaryKey, secondaryKey);
			if (!config.HasValue)
			{
				config.HasValue = true;
				config.Value = new TValue();
			}
			return config;
		}

		/// <summary>
		/// Gets a temporary value context.
		/// </summary>
		/// <typeparam name="TValue">The type of the value.</typeparam>
		/// <param name="key">The key for the context.</param>
		/// <returns>GUIConfig for the specified key.</returns>
		public static GUIContext<TValue> GetTemporaryContext<TValue>(object key) where TValue : class, new()
		{
			GUIContext<TValue> config = GUIContextCache<object, object, TValue>.GetConfig(defaultConfigKey, key);
			if (!config.HasValue)
			{
				config.HasValue = true;
				config.Value = new TValue();
			}
			return config;
		}

		/// <summary>
		/// Gets a temporary nullable value context.
		/// </summary>
		/// <param name="key">Key for context.</param>
		/// <param name="name">Name for the context.</param>
		public static GUIContext<TValue> GetTemporaryNullableContext<TValue>(object key, string name) where TValue : class
		{
			return GUIContextCache<object, string, TValue>.GetConfig(key, name);
		}

		/// <summary>
		/// Gets a temporary nullable value context.
		/// </summary>
		/// <param name="key">Key for context.</param>
		/// <param name="id">Id of the context.</param>
		public static GUIContext<TValue> GetTemporaryNullableContext<TValue>(object key, int id) where TValue : class
		{
			return GUIContextCache<object, int, TValue>.GetConfig(key, id);
		}

		/// <summary>
		/// Gets a temporary nullable value context.
		/// </summary>
		/// <param name="primaryKey">Primary key for the context.</param>
		/// <param name="secondaryKey">Secondary key for the context.</param>
		public static GUIContext<TValue> GetTemporaryNullableContext<TValue>(object primaryKey, object secondaryKey) where TValue : class
		{
			return GUIContextCache<object, object, TValue>.GetConfig(primaryKey, secondaryKey);
		}

		/// <summary>
		/// Gets a temporary nullable value context.
		/// </summary>
		/// <param name="key">Key for the context.</param>
		public static GUIContext<TValue> GetTemporaryNullableContext<TValue>(object key) where TValue : class
		{
			return GUIContextCache<object, object, TValue>.GetConfig(defaultConfigKey, key);
		}

		/// <summary>
		/// Gets a temporary context.
		/// </summary>
		/// <param name="key">Key for the context.</param>
		/// <param name="name">Name for the context.</param>
		/// <param name="defaultValue">Default value of the context.</param>
		public static GUIContext<TValue> GetTemporaryContext<TValue>(object key, string name, TValue defaultValue) where TValue : struct
		{
			GUIContext<TValue> config = GUIContextCache<object, string, TValue>.GetConfig(key, name);
			if (!config.HasValue)
			{
				config.HasValue = true;
				config.Value = defaultValue;
			}
			return config;
		}

		/// <summary>
		/// Gets a temporary context.
		/// </summary>
		/// <param name="key">Key for the context.</param>
		/// <param name="id">Id for the context.</param>
		/// <param name="defaultValue">Default value of the context.</param>
		public static GUIContext<TValue> GetTemporaryContext<TValue>(object key, int id, TValue defaultValue) where TValue : struct
		{
			GUIContext<TValue> config = GUIContextCache<object, int, TValue>.GetConfig(key, id);
			if (!config.HasValue)
			{
				config.HasValue = true;
				config.Value = defaultValue;
			}
			return config;
		}

		/// <summary>
		/// Gets a temporary context.
		/// </summary>
		/// <param name="primaryKey">Primary key for the context.</param>
		/// <param name="secondaryKey">Secondary key for the context.</param>
		/// <param name="defaultValue">Default value of the context.</param>
		public static GUIContext<TValue> GetTemporaryContext<TValue>(object primaryKey, object secondaryKey, TValue defaultValue) where TValue : struct
		{
			GUIContext<TValue> config = GUIContextCache<object, object, TValue>.GetConfig(primaryKey, secondaryKey);
			if (!config.HasValue)
			{
				config.HasValue = true;
				config.Value = defaultValue;
			}
			return config;
		}

		/// <summary>
		/// Gets a temporary context.
		/// </summary>
		/// <param name="key">Key for the context.</param>
		/// <param name="defaultValue">Default value of the context.</param>
		public static GUIContext<TValue> GetTemporaryContext<TValue>(object key, TValue defaultValue) where TValue : struct
		{
			GUIContext<TValue> config = GUIContextCache<object, object, TValue>.GetConfig(defaultValue, key);
			if (!config.HasValue)
			{
				config.HasValue = true;
				config.Value = defaultValue;
			}
			return config;
		}

		/// <summary>
		/// Gets a temporary GUIContent with the specified text.
		/// </summary>
		/// <param name="t">The text for the GUIContent.</param>
		/// <returns>Temporary GUIContent instance.</returns>
		public static GUIContent TempContent(string t)
		{
			tmpContent.image = null;
			tmpContent.text = t;
			tmpContent.tooltip = null;
			return tmpContent;
		}

		/// <summary>
		/// Gets a temporary GUIContent with the specified text and tooltip.
		/// </summary>
		/// <param name="t">The text for the GUIContent.</param>
		/// <param name="tooltip">The tooltip for the GUIContent.</param>
		/// <returns>Temporary GUIContent instance.</returns>
		public static GUIContent TempContent(string t, string tooltip)
		{
			tmpContent.image = null;
			tmpContent.text = t;
			tmpContent.tooltip = tooltip;
			return tmpContent;
		}

		/// <summary>
		/// Gets a temporary GUIContent with the specified image and tooltip.
		/// </summary>
		/// <param name="image">The image for the GUIContent.</param>
		/// <param name="tooltip">The tooltip for the GUIContent.</param>
		/// <returns>Temporary GUIContent instance.</returns>
		public static GUIContent TempContent(Texture image, string tooltip = null)
		{
			tmpContent.image = image;
			tmpContent.text = null;
			tmpContent.tooltip = tooltip;
			return tmpContent;
		}

		/// <summary>
		/// Gets a temporary GUIContent with the specified text, image and tooltip.
		/// </summary>
		/// <param name="text">The text for the GUIContent.</param>
		/// <param name="image">The image for the GUIContent.</param>
		/// <param name="tooltip">The tooltip for the GUIContent.</param>
		/// <returns>Temporary GUIContent instance.</returns>
		public static GUIContent TempContent(string text, Texture image, string tooltip = null)
		{
			tmpContent.image = image;
			tmpContent.text = text;
			tmpContent.tooltip = tooltip;
			return tmpContent;
		}

		/// <summary>
		/// Indents the rect by the current indent amount.
		/// </summary>
		/// <param name="rect">The rect to indent.</param>
		/// <returns>Indented rect.</returns>
		public static Rect IndentRect(Rect rect)
		{
			float indent = CurrentIndentAmount;
			rect.x += indent;
			rect.width -= indent;
			return rect;
		}

		/// <summary>
		/// Indents the rect by the current indent amount.
		/// </summary>
		/// <param name="rect">The rect to indent.</param>
		public static void IndentRect(ref Rect rect)
		{
			float indent = CurrentIndentAmount;
			rect.x += indent;
			rect.width -= indent;
		}

		public static void PingObject(UnityEngine.Object obj)
		{
			if (obj == null)
			{
				return;
			}
			if (AssetDatabase.Contains(obj) && !AssetDatabase.IsMainAsset(obj))
			{
				UnityEngine.Object root = AssetDatabase.LoadMainAssetAtPath(AssetDatabase.GetAssetPath(obj));
				if (root is Component)
				{
					root = (root as Component).gameObject;
				}
				EditorGUIUtility.PingObject(root);
			}
			if (obj is Component)
			{
				obj = (obj as Component).gameObject;
			}
			EditorGUIUtility.PingObject(obj);
		}

		public static void SelectObject(UnityEngine.Object obj)
		{
			if (obj == null)
			{
				return;
			}
			if (AssetDatabase.Contains(obj) && !AssetDatabase.IsMainAsset(obj))
			{
				UnityEngine.Object root = AssetDatabase.LoadMainAssetAtPath(AssetDatabase.GetAssetPath(obj));
				if (root is Component)
				{
					root = (root as Component).gameObject;
				}
				Selection.activeObject = root;
			}
			else
			{
				if (obj is Component)
				{
					obj = (obj as Component).gameObject;
				}
				Selection.activeObject = obj;
			}
		}

		/// <summary>
		/// Repaints the EditorWindow if a repaint has been requested.
		/// </summary>
		/// <param name="window">The window to repaint.</param>
		public static void RepaintIfRequested(this EditorWindow window)
		{
			if (RepaintRequested || (Event.current != null && (Event.current.type == EventType.Used || Event.current.isMouse)))
			{
				if ((bool)window)
				{
					window.Repaint();
				}
				ClearRepaintRequest();
			}
		}

		/// <summary>
		/// Repaints the editor if a repaint has been requested. If the currently rendering window is not an InspectorWindow, Repaint() will be called on the current window as well.
		/// </summary>
		/// <param name="editor">The editor to repaint.</param>
		public static void RepaintIfRequested(this UnityEditor.Editor editor)
		{
			if (RepaintRequested || (Event.current != null && (Event.current.type == EventType.Used || Event.current.isMouse)))
			{
				if ((bool)editor)
				{
					editor.Repaint();
				}
				if ((bool)CurrentWindow)
				{
					CurrentWindow.Repaint();
				}
				ClearRepaintRequest();
			}
		}

		private static Texture2D FindTexture(string name)
		{
			GUIContent icon = EditorGUIUtility.IconContent(name);
			if (icon != null)
			{
				return icon.image as Texture2D;
			}
			return null;
		}

		private static Texture2D SafeGetMiniTypeThumbnail(Type type)
		{
			Texture2D texture = null;
			try
			{
				texture = AssetPreview.GetMiniTypeThumbnail(type);
			}
			catch
			{
			}
			if (texture == null)
			{
				try
				{
					texture = FindTexture("DefaultAsset Icon");
				}
				catch
				{
				}
			}
			return texture;
		}

		/// <summary>
		/// Gets the best thumbnail icon given the provided arguments provided.
		/// </summary>
		/// <param name="obj"></param>
		/// <param name="type"></param>
		/// <param name="preferObjectPreviewOverFileIcon"></param>
		/// <returns></returns>
		public static Texture2D GetAssetThumbnail(UnityEngine.Object obj, Type type, bool preferObjectPreviewOverFileIcon)
		{
			if (preferObjectPreviewOverFileIcon && (bool)obj)
			{
				Texture2D icon = AssetPreview.GetAssetPreview(obj);
				if (icon != null && icon != FindTexture("DefaultAsset Icon"))
				{
					return icon;
				}
			}
			if (obj != null && type == null)
			{
				type = obj.GetType();
			}
			if (type != null)
			{
				Type iconType = type;
				Texture2D icon2 = SafeGetMiniTypeThumbnail(type);
				Texture2D defaultIcon = FindTexture("DefaultAsset Icon");
				while ((icon2 == null || icon2 == defaultIcon) && iconType != null && iconType.BaseType != typeof(object))
				{
					iconType = iconType.BaseType;
					icon2 = SafeGetMiniTypeThumbnail(iconType);
				}
				if (!(icon2 == defaultIcon) && !(icon2 == null))
				{
					return icon2;
				}
				if (type.InheritsFrom<Component>())
				{
					return FindTexture("cs Script Icon");
				}
				if (type.InheritsFrom<ScriptableObject>())
				{
					return FindTexture("ScriptableObject Icon");
				}
			}
			if ((bool)obj)
			{
				if (AssetDatabase.Contains(obj))
				{
					string path = AssetDatabase.GetAssetPath(obj);
					Texture2D assetIcon = InternalEditorUtility.GetIconForFile(path);
					if (assetIcon != null)
					{
						return assetIcon;
					}
				}
				return FindTexture("DefaultAsset Icon");
			}
			return FindTexture("cs Script Icon");
		}

		/// <summary>
		/// Gets a preview texture for the provided object.
		/// </summary>
		/// <param name="objectToPreview"></param>
		/// <exception cref="T:System.ArgumentNullException"></exception>
		public static Texture GetPreviewTexture(UnityEngine.Object objectToPreview)
		{
			if (objectToPreview == null)
			{
				throw new ArgumentNullException("objectToPreview", "You tried to pass null into GUIHelper.GetPreviewTexture which is not allowed.");
			}
			Texture previewTexture = null;
			RenderTexture renderTexture = objectToPreview as RenderTexture;
			if ((bool)renderTexture)
			{
				previewTexture = renderTexture;
			}
			if (Type_UnityEngine_UI_Image != null && Property_UnityEngine_UI_Image_Sprite != null && Type_UnityEngine_UI_Image.IsInstanceOfType(objectToPreview))
			{
				objectToPreview = (UnityEngine.Object)Property_UnityEngine_UI_Image_Sprite.GetValue(objectToPreview, null);
			}
			if (Type_UnityEngine_UI_RawImage != null && Property_UnityEngine_UI_RawImage_Texture != null && Type_UnityEngine_UI_RawImage.IsInstanceOfType(objectToPreview))
			{
				previewTexture = (Texture)Property_UnityEngine_UI_RawImage_Texture.GetValue(objectToPreview, null);
			}
			if (previewTexture == null)
			{
				previewTexture = GetAssetThumbnail(objectToPreview, objectToPreview.GetType(), preferObjectPreviewOverFileIcon: true);
			}
			return previewTexture;
		}

		public static void ExitGUI(bool removeFocusControl)
		{
			if (removeFocusControl)
			{
				RemoveFocusControl();
			}
			GUIUtility.ExitGUI();
		}

		/// <summary>
		/// Measures the size of a given <see cref="T:System.String" />, if it would be presented with this <see cref="T:UnityEngine.GUIStyle" />.
		/// </summary>
		/// <param name="style">The <see cref="T:UnityEngine.GUIStyle" /> to present the <see cref="T:System.String" /> as.</param>
		/// <param name="text">The <see cref="T:System.String" /> to measure.</param>
		/// <returns>A <see cref="T:UnityEngine.Vector2" /> consisting of the width (<see cref="F:UnityEngine.Vector2.x" />) &amp; height (<see cref="F:UnityEngine.Vector2.y" />), as the size of the <see cref="T:System.String" /> in GUI-space.</returns>
		public static Vector2 CalcSize(this GUIStyle style, string text)
		{
			return style.CalcSize(TempContent(text));
		}

		/// <summary>
		/// Measures the height of a given <see cref="T:System.String" />, if it would be presented with this <see cref="T:UnityEngine.GUIStyle" />.
		/// </summary>
		/// <param name="style">The <see cref="T:UnityEngine.GUIStyle" /> to present the <see cref="T:System.String" /> as.</param>
		/// <param name="text">The <see cref="T:System.String" /> to measure.</param>
		/// <param name="width">The width of the area the <see cref="T:System.String" /> is being presented in.</param>
		/// <returns>The height of the <see cref="T:System.String" />.</returns>
		public static float CalcHeight(this GUIStyle style, string text, float width)
		{
			return style.CalcHeight(TempContent(text), width);
		}

		/// <summary>
		/// Measures the width of a given <see cref="T:System.String" />, if it would be presented with this <see cref="T:UnityEngine.GUIStyle" />.
		/// </summary>
		/// <param name="style">The <see cref="T:UnityEngine.GUIStyle" /> to present the <see cref="T:System.String" /> as.</param>
		/// <param name="text">The <see cref="T:System.String" /> to measure.</param>
		/// <returns>The width of the <see cref="T:System.String" />.</returns>
		public static float CalcWidth(this GUIStyle style, string text)
		{
			return style.CalcSize(TempContent(text)).x;
		}

		/// <summary>
		/// Measures the width of a given <see cref="T:UnityEngine.GUIContent" />, if it would be presented with this <see cref="T:UnityEngine.GUIStyle" />.
		/// </summary>
		/// <param name="style">The <see cref="T:UnityEngine.GUIStyle" /> to present the <see cref="T:UnityEngine.GUIContent" /> as.</param>
		/// <param name="content">The <see cref="T:UnityEngine.GUIContent" /> to measure.</param>
		/// <returns>The width of the <see cref="T:UnityEngine.GUIContent" />.</returns>
		public static float CalcWidth(this GUIStyle style, GUIContent content)
		{
			return style.CalcSize(content).x;
		}

		/// <summary>
		/// Measures the min- &amp; max width of a given <see cref="T:System.String" />, if it would be presented with this <see cref="T:UnityEngine.GUIStyle" />.
		/// </summary>
		/// <param name="style">The <see cref="T:UnityEngine.GUIStyle" /> to present the <see cref="T:System.String" /> as.</param>
		/// <param name="text">The <see cref="T:System.String" /> to measure.</param>
		/// <param name="minWidth">The minimum width of the <see cref="T:System.String" />.</param>
		/// <param name="maxWidth">The maximum width of the <see cref="T:System.String" />.</param>
		/// <returns>The min- &amp; max width of the <see cref="T:System.String" />, as <c>out</c> parameters.</returns>
		public static void CalcMinMaxWidth(this GUIStyle style, string text, out float minWidth, out float maxWidth)
		{
			style.CalcMinMaxWidth(TempContent(text), out minWidth, out maxWidth);
		}
	}
}
