using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using Sirenix.OdinInspector.Editor.Internal;
using Sirenix.Reflection.Editor;
using Sirenix.Serialization;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Sirenix.OdinInspector.Editor
{
	/// <summary>
	/// Base class for creating editor windows using Odin.
	/// </summary>
	/// <example>
	/// <code>
	/// public class SomeWindow : OdinEditorWindow
	/// {
	///     [MenuItem("My Game/Some Window")]
	///     private static void OpenWindow()
	///     {
	///         GetWindow&lt;SomeWindow&gt;().Show();
	///     }
	///
	///     [Button(ButtonSizes.Large)]
	///     public void SomeButton() { }
	///
	///     [TableList]
	///     public SomeType[] SomeTableData;
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// <code>
	/// public class DrawSomeSingletonInAnEditorWindow : OdinEditorWindow
	/// {
	///     [MenuItem("My Game/Some Window")]
	///     private static void OpenWindow()
	///     {
	///         GetWindow&lt;DrawSomeSingletonInAnEditorWindow&gt;().Show();
	///     }
	///
	///     protected override object GetTarget()
	///     {
	///         return MySingleton.Instance;
	///     }
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// <code>
	/// private void InspectObjectInWindow()
	/// {
	///     OdinEditorWindow.InspectObject(someObject);
	/// }
	///
	/// private void InspectObjectInDropDownWithAutoHeight()
	/// {
	///     var btnRect = GUIHelper.GetCurrentLayoutRect();
	///     OdinEditorWindow.InspectObjectInDropDown(someObject, btnRect, btnRect.width);
	/// }
	///
	/// private void InspectObjectInDropDown()
	/// {
	///     var btnRect = GUIHelper.GetCurrentLayoutRect();
	///     OdinEditorWindow.InspectObjectInDropDown(someObject, btnRect, new Vector2(btnRect.width, 100));
	/// }
	///
	/// private void InspectObjectInACenteredWindow()
	/// {
	///     var window = OdinEditorWindow.InspectObject(someObject);
	///     window.position = GUIHelper.GetEditorWindowRect().AlignCenter(270, 200);
	/// }
	///
	/// private void OtherStuffYouCanDo()
	/// {
	///     var window = OdinEditorWindow.InspectObject(this.someObject);
	///
	///     window.position = GUIHelper.GetEditorWindowRect().AlignCenter(270, 200);
	///     window.titleContent = new GUIContent("Custom title", EditorIcons.RulerRect.Active);
	///     window.OnClose += () =&gt; Debug.Log("Window Closed");
	///     window.OnBeginGUI += () =&gt; GUILayout.Label("-----------");
	///     window.OnEndGUI += () =&gt; GUILayout.Label("-----------");
	/// }
	/// </code>
	/// </example>
	/// <seealso cref="T:Sirenix.OdinInspector.Editor.OdinMenuEditorWindow" />
	[ShowOdinSerializedPropertiesInInspector]
	public class OdinEditorWindow : EditorWindow, ISerializationCallbackReceiver, IHasCustomMenu
	{
		[NonSerialized]
		public Rect ToastPopupArea = Rect.zero;

		private Action _onBeginGUI;

		private Action _onEndGUI;

		private static PropertyInfo materialForceVisibleProperty = typeof(MaterialEditor).GetProperty("forceVisible", BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);

		private static bool hasUpdatedOdinEditors = false;

		private static int inspectObjectWindowCount = 3;

		private static readonly object[] EmptyObjectArray = new object[0];

		[SerializeField]
		[HideInInspector]
		private SerializationData serializationData;

		[HideInInspector]
		[SerializeField]
		private object inspectorTargetSerialized;

		[SerializeField]
		[HideInInspector]
		private float labelWidth = 0.33f;

		[NonSerialized]
		private object inspectTargetObject;

		[HideInInspector]
		[SerializeField]
		private Vector4 windowPadding = new Vector4(4f, 4f, 4f, 4f);

		[SerializeField]
		[HideInInspector]
		private bool useScrollView = true;

		[HideInInspector]
		[SerializeField]
		private bool drawUnityEditorPreview;

		[SerializeField]
		[HideInInspector]
		private int wrappedAreaMaxHeight = 1000;

		[NonSerialized]
		private int warmupRepaintCount;

		[NonSerialized]
		private bool isInitialized;

		private GUIStyle marginStyle;

		private object[] currentTargets = new object[0];

		private ImmutableList<object> currentTargetsImm;

		private UnityEditor.Editor[] editors = new UnityEditor.Editor[0];

		private PropertyTree[] propertyTrees = new PropertyTree[0];

		private Vector2 scrollPos;

		private int mouseDownId;

		private EditorWindow mouseDownWindow;

		private int mouseDownKeyboardControl;

		private Vector2 contenSize;

		private float defaultEditorPreviewHeight = 170f;

		private bool preventContentFromExpanding;

		private bool isAutoHeightAdjustmentReady;

		private bool isInsideOnGUI;

		private bool isInOurImGUIContainer;

		private List<Toast> toasts = new List<Toast>();

		public const int MIN_DROPDOWN_HEIGHT = 200;

		public const int MAX_DROPDOWN_HEIGHT = 600;

		/// <summary>
		/// Gets the label width to be used. Values between 0 and 1 are treated as percentages, and values above as pixels.
		/// </summary>
		public virtual float DefaultLabelWidth
		{
			get
			{
				return labelWidth;
			}
			set
			{
				labelWidth = value;
			}
		}

		/// <summary>
		/// Gets or sets the window padding. x = left, y = right, z = top, w = bottom.
		/// </summary>
		public virtual Vector4 WindowPadding
		{
			get
			{
				return windowPadding;
			}
			set
			{
				windowPadding = value;
			}
		}

		/// <summary>
		/// Gets a value indicating whether the window should draw a scroll view.
		/// </summary>
		public virtual bool UseScrollView
		{
			get
			{
				return useScrollView;
			}
			set
			{
				useScrollView = value;
			}
		}

		/// <summary>
		/// Gets a value indicating whether the window should draw a Unity editor preview, if possible.
		/// </summary>
		public virtual bool DrawUnityEditorPreview
		{
			get
			{
				return drawUnityEditorPreview;
			}
			set
			{
				drawUnityEditorPreview = value;
			}
		}

		/// <summary>
		/// Gets the default preview height for Unity editors.
		/// </summary>
		public virtual float DefaultEditorPreviewHeight
		{
			get
			{
				return defaultEditorPreviewHeight;
			}
			set
			{
				defaultEditorPreviewHeight = value;
			}
		}

		/// <summary>
		/// At the start of each OnGUI event when in the Layout event, the GetTargets() method is called and cached into a list which you can access from here.
		/// </summary>
		protected ImmutableList<object> CurrentDrawingTargets => currentTargetsImm;

		/// <summary>
		/// The Odin property tree drawn.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("Support for non Odin drawn editors and drawing of multiple editors has been added, so there is no longer any guarantee that there will be a PropertyTree.")]
		public PropertyTree PropertyTree
		{
			get
			{
				if (propertyTrees != null)
				{
					return propertyTrees.FirstOrDefault();
				}
				return null;
			}
		}

		/// <summary>
		/// Occurs when the window is closed.
		/// </summary>
		public event Action OnClose;

		/// <summary>
		/// Occurs at the beginning the OnGUI method.
		/// </summary>
		public event Action OnBeginGUI;

		/// <summary>
		/// Occurs at the end the OnGUI method.
		/// </summary>
		public event Action OnEndGUI;

		/// <summary>
		/// Gets the target which which the window is supposed to draw. By default it simply returns the editor window instance itself. By default, this method is called by <see cref="M:Sirenix.OdinInspector.Editor.OdinEditorWindow.GetTargets" />().
		/// </summary>
		protected virtual object GetTarget()
		{
			if (inspectTargetObject != null)
			{
				return inspectTargetObject;
			}
			if (inspectorTargetSerialized != null)
			{
				if (inspectorTargetSerialized is UnityEngine.Object uObj && !uObj)
				{
					return this;
				}
				return inspectorTargetSerialized;
			}
			return this;
		}

		/// <summary>
		/// Gets the targets to be drawn by the editor window. By default this simply yield returns the <see cref="M:Sirenix.OdinInspector.Editor.OdinEditorWindow.GetTarget" /> method.
		/// </summary>
		protected virtual IEnumerable<object> GetTargets()
		{
			yield return GetTarget();
		}

		/// <summary>
		/// <para>
		/// Pops up an editor window for the given object in a drop-down window which closes when it loses its focus.
		/// This particular overload uses a few frames to calculate the height of the content before showing the window with a height that matches its content.
		/// </para>
		/// <para>Protip: You can subscribe to OnClose if you want to know when that occurs.</para>
		/// </summary>
		public static OdinEditorWindow InspectObjectInDropDown(object obj, Rect btnRect, float windowWidth)
		{
			return InspectObjectInDropDown(obj, btnRect, new Vector2(windowWidth, 0f));
		}

		/// <summary>
		/// Measures the GUILayout content height and adjusts the window height accordingly.
		/// Note that this feature becomes pointless if any layout group expands vertically.
		/// </summary>
		/// <param name="maxHeight">The max height of the window.</param>
		/// <param name="retainInitialWindowPosition">When the window height expands below the screen bounds, it will move the window
		/// upwards when needed, enabling this will move it back down when the window height is decreased. </param>
		protected void EnableAutomaticHeightAdjustment(int maxHeight, bool retainInitialWindowPosition)
		{
			preventContentFromExpanding = true;
			wrappedAreaMaxHeight = maxHeight;
			OdinEditorWindow wnd = this;
			Vector2 initialPos = new Vector2(float.NaN, float.NaN);
			float adjustedHeight = float.NaN;
			EditorApplication.CallbackFunction callback = null;
			callback = delegate
			{
				EditorApplication.update = (EditorApplication.CallbackFunction)Delegate.Remove(EditorApplication.update, callback);
				EditorApplication.update = (EditorApplication.CallbackFunction)Delegate.Remove(EditorApplication.update, callback);
				if (!(wnd == null))
				{
					if (wnd.isAutoHeightAdjustmentReady && wnd.contenSize != Vector2.zero && !Mathf.Approximately(adjustedHeight, wnd.contenSize.y))
					{
						bool flag = !float.IsNaN(initialPos.x) && !float.IsNaN(initialPos.y);
						Rect rect = wnd.position;
						if (retainInitialWindowPosition && flag)
						{
							rect.position = initialPos;
						}
						float num = Math.Min(wnd.contenSize.y, maxHeight);
						wnd.minSize = new Vector2(wnd.minSize.x, num);
						wnd.maxSize = new Vector2(wnd.maxSize.x, num);
						rect.height = num;
						rect = EditorWindow_Internal.FitPositionInWorkingArea(wnd, rect, useMouseScreen: false);
						if (!flag)
						{
							initialPos = rect.position;
						}
						wnd.position = rect;
						adjustedHeight = wnd.contenSize.y;
					}
					EditorApplication.update = (EditorApplication.CallbackFunction)Delegate.Combine(EditorApplication.update, callback);
				}
			};
			EditorApplication.update = (EditorApplication.CallbackFunction)Delegate.Combine(EditorApplication.update, callback);
		}

		/// <summary>
		/// <para>
		/// Pops up an editor window for the given object in a drop-down window which closes when it loses its focus.
		/// </para>
		/// <para>Protip: You can subscribe to OnClose if you want to know when that occurs.</para>
		/// </summary>
		public static OdinEditorWindow InspectObjectInDropDown(object obj, Rect btnRect, Vector2 windowSize)
		{
			OdinEditorWindow window = CreateOdinEditorWindowInstanceForObject(obj);
			if (windowSize.x <= 1f)
			{
				windowSize.x = btnRect.width;
			}
			if (windowSize.x <= 1f)
			{
				windowSize.x = 400f;
			}
			btnRect.x = (int)btnRect.x;
			btnRect.width = (int)btnRect.width;
			btnRect.height = (int)btnRect.height;
			btnRect.y = (int)btnRect.y;
			windowSize.x = (int)windowSize.x;
			windowSize.y = (int)windowSize.y;
			try
			{
				EditorWindow curr = GUIHelper.CurrentWindow;
				if (curr != null)
				{
					window.OnBeginGUI += delegate
					{
						curr.Repaint();
					};
				}
			}
			catch
			{
			}
			if (!EditorGUIUtility.isProSkin)
			{
				window.OnBeginGUI += delegate
				{
					SirenixEditorGUI.DrawSolidRect(new Rect(0f, 0f, window.position.width, window.position.height), SirenixGUIStyles.MenuBackgroundColor);
				};
			}
			window.OnEndGUI += delegate
			{
				SirenixEditorGUI.DrawBorders(new Rect(0f, 0f, window.position.width, window.position.height), 1);
			};
			window.labelWidth = 0.33f;
			window.DrawUnityEditorPreview = true;
			btnRect.position = GUIUtility.GUIToScreenPoint(btnRect.position);
			bool isWindowSizeYSet = (int)windowSize.y != 0;
			if (!isWindowSizeYSet)
			{
				windowSize.y = 1f;
			}
			Rect windowPosition = btnRect;
			windowPosition.y += btnRect.height;
			windowPosition.width = windowSize.x;
			windowPosition.height = windowSize.y;
			window.position = windowPosition;
			if (!isWindowSizeYSet)
			{
				window.EnableAutomaticHeightAdjustment(600, retainInitialWindowPosition: true);
			}
			EditorWindow_Internal.ShowPopupAux(window);
			return window;
		}

		/// <summary>
		/// <para>
		/// Pops up an editor window for the given object in a drop-down window which closes when it loses its focus.
		/// </para>
		/// <para>Protip: You can subscribe to OnClose if you want to know when that occurs.</para>
		/// </summary>
		public static OdinEditorWindow InspectObjectInDropDown(object obj, Vector2 position)
		{
			Rect btnRect = new Rect(position.x, position.y, 1f, 1f);
			return InspectObjectInDropDown(obj, btnRect, 350f);
		}

		/// <summary>
		/// <para>
		/// Pops up an editor window for the given object in a drop-down window which closes when it loses its focus.
		/// </para>
		/// <para>Protip: You can subscribe to OnClose if you want to know when that occurs.</para>
		/// </summary>
		public static OdinEditorWindow InspectObjectInDropDown(object obj, float windowWidth)
		{
			Vector2 position = Event.current.mousePosition;
			Rect btnRect = new Rect(position.x, position.y, 1f, 1f);
			return InspectObjectInDropDown(obj, btnRect, windowWidth);
		}

		/// <summary>
		/// <para>
		/// Pops up an editor window for the given object in a drop-down window which closes when it loses its focus.
		/// </para>
		/// <para>Protip: You can subscribe to OnClose if you want to know when that occurs.</para>
		/// </summary>
		public static OdinEditorWindow InspectObjectInDropDown(object obj, Vector2 position, float windowWidth)
		{
			Rect btnRect = new Rect(position.x, position.y, 1f, 1f);
			return InspectObjectInDropDown(obj, btnRect, windowWidth);
		}

		/// <summary>
		/// <para>
		/// Pops up an editor window for the given object in a drop-down window which closes when it loses its focus.
		/// </para>
		/// <para>Protip: You can subscribe to OnClose if you want to know when that occurs.</para>
		/// </summary>
		public static OdinEditorWindow InspectObjectInDropDown(object obj, float width, float height)
		{
			UnityShims.Rect.Ctor(out var r, Event.current.mousePosition, Vector2.one);
			return InspectObjectInDropDown(obj, r, new Vector2(width, height));
		}

		/// <summary>
		/// <para>
		/// Pops up an editor window for the given object in a drop-down window which closes when it loses its focus.
		/// </para>
		/// <para>Protip: You can subscribe to OnClose if you want to know when that occurs.</para>
		/// </summary>
		public static OdinEditorWindow InspectObjectInDropDown(object obj)
		{
			return InspectObjectInDropDown(obj, Event.current.mousePosition);
		}

		/// <summary>
		/// Pops up an editor window for the given object.
		/// </summary>
		public static OdinEditorWindow InspectObject(object obj)
		{
			return InspectObject(obj, forceSerializeInspectedObject: false);
		}

		internal static OdinEditorWindow InspectObject(object obj, bool forceSerializeInspectedObject)
		{
			OdinEditorWindow window = CreateOdinEditorWindowInstanceForObject(obj, forceSerializeInspectedObject);
			window.Show();
			Vector2 offset = new Vector2(30f, 30f) * (inspectObjectWindowCount++ % 6 - 3);
			window.position = GUIHelper.GetEditorWindowRect().AlignCenter(400f, 300f).AddPosition(offset);
			return window;
		}

		/// <summary>
		/// Inspects the object using an existing OdinEditorWindow.
		/// </summary>
		public static OdinEditorWindow InspectObject(OdinEditorWindow window, object obj)
		{
			UnityEngine.Object uObj = obj as UnityEngine.Object;
			if ((bool)uObj)
			{
				window.inspectTargetObject = null;
				window.inspectorTargetSerialized = uObj;
			}
			else
			{
				window.inspectorTargetSerialized = null;
				window.inspectTargetObject = obj;
			}
			if ((bool)(uObj as UnityEngine.Component))
			{
				window.titleContent = new GUIContent((uObj as UnityEngine.Component).gameObject.name);
			}
			else if ((bool)uObj)
			{
				window.titleContent = new GUIContent(uObj.name);
			}
			else
			{
				window.titleContent = new GUIContent(obj.ToString());
			}
			EditorUtility.SetDirty(window);
			return window;
		}

		/// <summary>
		/// Creates an editor window instance for the specified object, without opening the window.
		/// </summary>
		public static OdinEditorWindow CreateOdinEditorWindowInstanceForObject(object obj)
		{
			return CreateOdinEditorWindowInstanceForObject(obj, forceSerializeInspectedObject: false);
		}

		/// <summary>
		/// Creates an editor window instance for the specified object, without opening the window.
		/// </summary>
		internal static OdinEditorWindow CreateOdinEditorWindowInstanceForObject(object obj, bool forceSerializeInspectedObject)
		{
			OdinEditorWindow window = ScriptableObject.CreateInstance<OdinEditorWindow>();
			GUIUtility.hotControl = 0;
			GUIUtility.keyboardControl = 0;
			if ((bool)(obj as UnityEngine.Object) || forceSerializeInspectedObject)
			{
				window.inspectorTargetSerialized = obj;
			}
			else
			{
				window.inspectTargetObject = obj;
			}
			if (obj is UnityEngine.Component com && (bool)com)
			{
				window.titleContent = new GUIContent(com.gameObject.name);
			}
			else if (obj is UnityEngine.Object uObj && (bool)uObj)
			{
				window.titleContent = new GUIContent(uObj.name);
			}
			else
			{
				window.titleContent = new GUIContent(obj.ToString());
			}
			window.position = GUIHelper.GetEditorWindowRect().AlignCenter(600f, 600f);
			EditorUtility.SetDirty(window);
			return window;
		}

		void ISerializationCallbackReceiver.OnAfterDeserialize()
		{
			UnitySerializationUtility.DeserializeUnityObject(this, ref serializationData);
			OnAfterDeserialize();
		}

		void ISerializationCallbackReceiver.OnBeforeSerialize()
		{
			UnitySerializationUtility.SerializeUnityObject(this, ref serializationData);
			OnBeforeSerialize();
		}

		private void CreateGUI()
		{
			IMGUIContainer container = new IMGUIContainer(OnImGUI);
			container.name = "Odin ImGUIContainer";
			container.style.display = DisplayStyle.Flex;
			container.style.height = Length.Percent(100f);
			container.style.width = Length.Percent(100f);
			IMGUIContainer toastContainer = new IMGUIContainer(DrawToasts);
			toastContainer.name = "Odin Toast ImGUIContainer";
			toastContainer.style.display = DisplayStyle.Flex;
			toastContainer.style.height = Length.Percent(100f);
			toastContainer.style.width = Length.Percent(100f);
			toastContainer.pickingMode = PickingMode.Ignore;
			base.rootVisualElement.Add(container);
			container.Add(toastContainer);
		}

		protected virtual void OnImGUI()
		{
			isInOurImGUIContainer = true;
			if (warmupRepaintCount <= 10 && Event.current.type == EventType.Layout)
			{
				warmupRepaintCount++;
				Repaint();
			}
			InitializeIfNeeded();
			if (Event.current.type == EventType.Layout)
			{
				_onBeginGUI = this.OnBeginGUI;
				_onEndGUI = this.OnEndGUI;
			}
			try
			{
				isInsideOnGUI = true;
				bool measureArea = preventContentFromExpanding;
				if (measureArea)
				{
					GUILayout.BeginArea(new Rect(0f, 0f, base.position.width, wrappedAreaMaxHeight));
				}
				HandleToastInput();
				if (_onBeginGUI != null)
				{
					_onBeginGUI();
				}
				if (!hasUpdatedOdinEditors)
				{
					hasUpdatedOdinEditors = true;
				}
				marginStyle = marginStyle ?? new GUIStyle
				{
					padding = new RectOffset()
				};
				if (Event.current.type == EventType.Layout)
				{
					marginStyle.padding.left = (int)WindowPadding.x;
					marginStyle.padding.right = (int)WindowPadding.y;
					marginStyle.padding.top = (int)WindowPadding.z;
					marginStyle.padding.bottom = (int)WindowPadding.w;
					UpdateEditors();
				}
				EventType prevType = Event.current.type;
				if (Event.current.type == EventType.MouseDown)
				{
					mouseDownId = GUIUtility.hotControl;
					mouseDownKeyboardControl = GUIUtility.keyboardControl;
					mouseDownWindow = EditorWindow.focusedWindow;
				}
				bool useScrollWheel = UseScrollView;
				if (useScrollWheel)
				{
					scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
				}
				Vector2 size = ((!preventContentFromExpanding) ? EditorGUILayout.BeginVertical().size : EditorGUILayout.BeginVertical(GUILayoutOptions.ExpandHeight(expand: false)).size);
				if (contenSize == Vector2.zero || Event.current.type == EventType.Repaint)
				{
					contenSize = size;
				}
				GUIHelper.PushHierarchyMode(hierarchyMode: false);
				float labelWidth = ((!(DefaultLabelWidth < 1f)) ? DefaultLabelWidth : (contenSize.x * DefaultLabelWidth));
				GUIHelper.PushLabelWidth(labelWidth);
				OnBeginDrawEditors();
				GUILayout.BeginVertical(marginStyle);
				DrawEditors();
				GUILayout.EndVertical();
				OnEndDrawEditors();
				GUIHelper.PopLabelWidth();
				GUIHelper.PopHierarchyMode();
				EditorGUILayout.EndVertical();
				if (useScrollWheel)
				{
					EditorGUILayout.EndScrollView();
				}
				if (Event.current.type == EventType.Repaint)
				{
					isAutoHeightAdjustmentReady = true;
				}
				if (_onEndGUI != null)
				{
					_onEndGUI();
				}
				this.RepaintIfRequested();
				if (measureArea)
				{
					GUILayout.EndArea();
				}
			}
			finally
			{
				isInsideOnGUI = false;
			}
		}

		/// <summary>
		/// Draws the Odin Editor Window.
		/// </summary>
		[Obsolete("Rename this to OnImGUI()", true)]
		protected virtual void OnGUI()
		{
			if (!isInOurImGUIContainer)
			{
				OnImGUI();
			}
		}

		/// <summary>
		/// Calls DrawEditor(index) for each of the currently drawing targets.
		/// </summary>
		protected virtual void DrawEditors()
		{
			for (int i = 0; i < currentTargets.Length; i++)
			{
				DrawEditor(i);
			}
		}

		protected void EnsureEditorsAreReady()
		{
			InitializeIfNeeded();
			UpdateEditors();
		}

		protected void UpdateEditors()
		{
			currentTargets = currentTargets ?? new object[0];
			editors = editors ?? new UnityEditor.Editor[0];
			propertyTrees = propertyTrees ?? new PropertyTree[0];
			IEnumerable<object> enumerable = GetTargets();
			IList<object> newTargets = ((enumerable is IList<object>) ? ((IList<object>)enumerable) : ((enumerable != null) ? ((IList<object>)enumerable.ToList()) : ((IList<object>)EmptyObjectArray)));
			if (currentTargets.Length != newTargets.Count)
			{
				if (editors.Length > newTargets.Count)
				{
					int toDestroy = editors.Length - newTargets.Count;
					for (int i = 0; i < toDestroy; i++)
					{
						UnityEditor.Editor e = editors[editors.Length - i - 1];
						if ((bool)e)
						{
							UnityEngine.Object.DestroyImmediate(e);
						}
					}
				}
				if (propertyTrees.Length > newTargets.Count)
				{
					for (int j = newTargets.Count; j < propertyTrees.Length; j++)
					{
						propertyTrees[j]?.Dispose();
					}
				}
				Array.Resize(ref currentTargets, newTargets.Count);
				Array.Resize(ref editors, newTargets.Count);
				Array.Resize(ref propertyTrees, newTargets.Count);
				Repaint();
				currentTargetsImm = new ImmutableList<object>(currentTargets);
				warmupRepaintCount = 0;
			}
			for (int k = 0; k < newTargets.Count; k++)
			{
				object newTarget = newTargets[k];
				object curTarget = currentTargets[k];
				bool hasTargetChanged;
				if (curTarget is IEnumerable curEnumerable && newTarget is IEnumerable targetEnumerable)
				{
					IEnumerator curEnumerator = curEnumerable.GetEnumerator();
					IEnumerator targetEnumerator = targetEnumerable.GetEnumerator();
					while (true)
					{
						bool hasCurNext = curEnumerator.MoveNext();
						bool hasTargetNext = targetEnumerator.MoveNext();
						if (!hasCurNext && !hasTargetNext)
						{
							hasTargetChanged = false;
							break;
						}
						if (!hasCurNext || !hasTargetNext)
						{
							hasTargetChanged = true;
							break;
						}
						if (curEnumerator.Current != targetEnumerator.Current)
						{
							hasTargetChanged = true;
							break;
						}
					}
				}
				else
				{
					hasTargetChanged = newTarget != curTarget;
				}
				if (!hasTargetChanged)
				{
					continue;
				}
				warmupRepaintCount = 0;
				GUIHelper.RequestRepaint();
				currentTargets[k] = newTarget;
				if (newTarget == null)
				{
					if (propertyTrees[k] != null)
					{
						propertyTrees[k].Dispose();
					}
					propertyTrees[k] = null;
					if ((bool)editors[k])
					{
						UnityEngine.Object.DestroyImmediate(editors[k]);
					}
					editors[k] = null;
					continue;
				}
				EditorWindow editorWindow = newTarget as EditorWindow;
				if (newTarget.GetType().InheritsFrom<UnityEngine.Object>() && !editorWindow)
				{
					UnityEngine.Object unityObject = newTarget as UnityEngine.Object;
					if ((bool)unityObject)
					{
						if (propertyTrees[k] != null)
						{
							propertyTrees[k].Dispose();
						}
						propertyTrees[k] = null;
						if ((bool)editors[k])
						{
							UnityEngine.Object.DestroyImmediate(editors[k]);
						}
						editors[k] = UnityEditor.Editor.CreateEditor(unityObject);
						MaterialEditor materialEditor = editors[k] as MaterialEditor;
						if (materialEditor != null && materialForceVisibleProperty != null)
						{
							materialForceVisibleProperty.SetValue(materialEditor, true, null);
						}
					}
					else
					{
						if (propertyTrees[k] != null)
						{
							propertyTrees[k].Dispose();
						}
						propertyTrees[k] = null;
						if ((bool)editors[k])
						{
							UnityEngine.Object.DestroyImmediate(editors[k]);
						}
						editors[k] = null;
					}
				}
				else
				{
					if (propertyTrees[k] != null)
					{
						propertyTrees[k].Dispose();
					}
					if ((bool)editors[k])
					{
						UnityEngine.Object.DestroyImmediate(editors[k]);
					}
					editors[k] = null;
					if (newTarget is IList)
					{
						propertyTrees[k] = PropertyTree.Create(newTarget as IList);
					}
					else
					{
						propertyTrees[k] = PropertyTree.Create(newTarget);
					}
				}
			}
		}

		private void InitializeIfNeeded()
		{
			if (!isInitialized)
			{
				isInitialized = true;
				if (base.titleContent != null && base.titleContent.text == GetType().FullName)
				{
					base.titleContent.text = GetType().GetNiceName().SplitPascalCase();
				}
				base.wantsMouseMove = true;
				Selection.selectionChanged = (Action)Delegate.Remove(Selection.selectionChanged, new Action(SelectionChanged));
				Selection.selectionChanged = (Action)Delegate.Combine(Selection.selectionChanged, new Action(SelectionChanged));
				Initialize();
			}
		}

		/// <summary>
		/// Initialize get called by OnEnable and by OnGUI after assembly reloads
		/// which often happens when you recompile or enter and exit play mode.
		/// </summary>
		protected virtual void Initialize()
		{
		}

		private void SelectionChanged()
		{
			Repaint();
		}

		/// <summary>
		/// Called when the window is enabled. Remember to call base.OnEnable();
		/// </summary>
		protected virtual void OnEnable()
		{
			InitializeIfNeeded();
			OdinEditorWindows.SetActive(this);
		}

		/// <summary>
		/// Draws the editor for the this.CurrentDrawingTargets[index].
		/// </summary>
		protected virtual void DrawEditor(int index)
		{
			if (!isInsideOnGUI)
			{
				EnsureEditorsAreReady();
			}
			PropertyTree tmpPropertyTree = propertyTrees[index];
			UnityEditor.Editor tmpEditor = editors[index];
			if (tmpPropertyTree != null || (tmpEditor != null && tmpEditor.target != null))
			{
				if (tmpPropertyTree != null)
				{
					bool withUndo = tmpPropertyTree.WeakTargets.FirstOrDefault() as UnityEngine.Object;
					tmpPropertyTree.Draw(withUndo);
				}
				else
				{
					OdinEditor.ForceHideMonoScriptInEditor = true;
					try
					{
						tmpEditor.OnInspectorGUI();
					}
					finally
					{
						OdinEditor.ForceHideMonoScriptInEditor = false;
					}
				}
			}
			if (DrawUnityEditorPreview)
			{
				DrawEditorPreview(index, defaultEditorPreviewHeight);
			}
		}

		/// <summary>
		/// Uses the <see cref="M:UnityEditor.Editor.DrawPreview(UnityEngine.Rect)" /> method to draw a preview for the this.CurrentDrawingTargets[index].
		/// </summary>
		protected virtual void DrawEditorPreview(int index, float height)
		{
			if (!isInsideOnGUI)
			{
				EnsureEditorsAreReady();
			}
			UnityEditor.Editor editor = editors[index];
			if (editor != null && editor.HasPreviewGUI())
			{
				Rect rect = EditorGUILayout.GetControlRect(false, height);
				editor.DrawPreview(rect);
			}
		}

		protected virtual void OnDisable()
		{
			OdinEditorWindows.SetInactive(this);
			Cleanup();
		}

		/// <summary>
		/// Called when the window is destroyed. Remember to call base.OnDestroy();
		/// </summary>
		protected virtual void OnDestroy()
		{
			Cleanup();
			if (this.OnClose != null)
			{
				this.OnClose();
			}
		}

		private void Cleanup()
		{
			if (editors != null)
			{
				for (int i = 0; i < editors.Length; i++)
				{
					if ((bool)editors[i])
					{
						UnityEngine.Object.DestroyImmediate(editors[i]);
						editors[i] = null;
					}
				}
				editors = null;
			}
			if (propertyTrees != null)
			{
				for (int j = 0; j < propertyTrees.Length; j++)
				{
					if (propertyTrees[j] != null)
					{
						propertyTrees[j].Dispose();
						propertyTrees[j] = null;
					}
				}
				propertyTrees = null;
			}
			Selection.selectionChanged = (Action)Delegate.Remove(Selection.selectionChanged, new Action(SelectionChanged));
			Selection.selectionChanged = (Action)Delegate.Remove(Selection.selectionChanged, new Action(SelectionChanged));
		}

		/// <summary>
		/// Called before starting to draw all editors for the <see cref="P:Sirenix.OdinInspector.Editor.OdinEditorWindow.CurrentDrawingTargets" />.
		/// </summary>
		protected virtual void OnEndDrawEditors()
		{
		}

		/// <summary>
		/// Called after all editors for the <see cref="P:Sirenix.OdinInspector.Editor.OdinEditorWindow.CurrentDrawingTargets" /> has been drawn.
		/// </summary>
		protected virtual void OnBeginDrawEditors()
		{
		}

		/// <summary>
		/// See ISerializationCallbackReceiver.OnBeforeSerialize for documentation on how to use this method.
		/// </summary>
		protected virtual void OnAfterDeserialize()
		{
		}

		/// <summary>
		/// Implement this method to receive a callback after unity serialized your object.
		/// </summary>
		protected virtual void OnBeforeSerialize()
		{
		}

		public void ShowToast(ToastPosition toastPosition, SdfIconType icon, string text, Color color, float expiryTime)
		{
			ShowToast(toastPosition, icon, text, null, color, expiryTime, null, null);
		}

		public void ShowToast(ToastPosition toastPosition, SdfIconType icon, string text, Color color, float expiryTime, string buttonText, Action buttonOnClick)
		{
			ShowToast(toastPosition, icon, text, null, color, expiryTime, buttonText, buttonOnClick);
		}

		public void ShowToast(ToastPosition toastPosition, SdfIconType icon, string header, string body, Color color, float expiryTime)
		{
			ShowToast(toastPosition, icon, header, body, color, expiryTime, null, null);
		}

		public void ShowToast(ToastPosition toastPosition, SdfIconType icon, string header, string body, Color color, float expiryTime, string buttonText, Action buttonOnClick)
		{
			toasts.Insert(0, new Toast(toastPosition, icon, header, body, color, expiryTime, buttonText, buttonOnClick));
		}

		private void HandleToastInput()
		{
			if (toasts == null || toasts.Count == 0)
			{
				return;
			}
			float toastOffsetTopLeft = 16f;
			float toastOffsetTopRight = 16f;
			float toastOffsetBottomLeft = 16f;
			float toastOffsetBottomRight = 16f;
			Rect area = ((ToastPopupArea == Rect.zero) ? base.position.SetPosition(Vector2.zero) : ToastPopupArea);
			Event e = Event.current;
			GUI.BeginClip(area);
			area.position = Vector2.zero;
			Vector2 start = default(Vector2);
			Vector2 target = default(Vector2);
			for (int i = 0; i < toasts.Count; i++)
			{
				Toast toast = toasts[i];
				bool hasButton = toast.ButtonOnClick != null;
				bool hasBody = !string.IsNullOrEmpty(toast.Body);
				Vector2 contentSize = Vector2.zero;
				contentSize.x = Toast.StyleHeader.CalcWidth(toast.Header);
				contentSize.y = Toast.StyleHeader.CalcHeight(toast.Header, contentSize.x);
				float buttonWidth = 0f;
				if (hasButton && !hasBody)
				{
					buttonWidth = Toast.Style.CalcWidth(toast.ButtonText);
					contentSize.x += 24f + buttonWidth;
				}
				float headerHeight = contentSize.y;
				float bodyHeight = 0f;
				if (hasBody)
				{
					contentSize.y += 4f;
					float bodyWidth = Toast.Style.CalcWidth(toast.Body);
					if (bodyWidth > 360f)
					{
						bodyWidth = 360f;
					}
					if (contentSize.x < bodyWidth)
					{
						contentSize.x = bodyWidth;
					}
					bodyHeight = Toast.Style.CalcHeight(toast.Body, bodyWidth);
					contentSize.y += bodyHeight;
					if (hasButton)
					{
						contentSize.y += 8f;
						buttonWidth = Toast.Style.CalcWidth(toast.ButtonText);
						if (buttonWidth > contentSize.x)
						{
							contentSize.x = buttonWidth;
						}
						contentSize.y += Toast.Style.CalcHeight(toast.ButtonText, buttonWidth);
					}
				}
				Vector2 toastSize = contentSize;
				toastSize.x += 16f;
				toastSize.y += 17.5f;
				if (toast.Icon != SdfIconType.None)
				{
					toastSize.x += Toast.StyleHeader.lineHeight + 8f + 8f;
				}
				switch (toast.ToastPosition)
				{
				default:
					start.x = area.x + 16f;
					start.y = area.y - toastSize.y;
					target.x = start.x;
					target.y = area.y + toastOffsetTopLeft;
					toastOffsetTopLeft += toastSize.y + 16f;
					break;
				case ToastPosition.TopRight:
					start.x = area.xMax - toastSize.x - 16f;
					start.y = area.y - toastSize.y;
					target.x = start.x;
					target.y = area.y + toastOffsetTopRight;
					toastOffsetTopRight += toastSize.y + 16f;
					break;
				case ToastPosition.BottomLeft:
					start.x = area.x + 16f;
					start.y = area.height;
					target.x = start.x;
					target.y = start.y - toastOffsetBottomLeft - toastSize.y;
					toastOffsetBottomLeft += toastSize.y + 16f;
					break;
				case ToastPosition.BottomRight:
					start.x = area.width - toastSize.x - 16f;
					start.y = area.height;
					target.x = start.x;
					target.y = start.y - toastOffsetBottomRight - toastSize.y;
					toastOffsetBottomRight += toastSize.y + 16f;
					break;
				}
				UnityShims.Rect.Ctor(out var targetRect, target, toastSize);
				if (toast.CurrentRect == Rect.zero)
				{
					toast.CurrentRect = UnityShims.Rect.Ctor(start, toastSize);
				}
				Rect rect = toast.CurrentRect;
				rect.y = targetRect.y;
				if (!area.Overlaps(rect))
				{
					continue;
				}
				Rect contentRect = rect.Padding(8f);
				if (toast.Icon != SdfIconType.None)
				{
					Rect iconRect = contentRect.TakeFromLeft(Toast.StyleHeader.lineHeight);
					contentRect.xMin += 8f;
					contentRect.xMax -= 8f;
				}
				Rect headerRect = contentRect.TakeFromTop(Toast.StyleHeader.CalcHeight(toast.Header, contentRect.width));
				contentRect.yMin += 4f;
				Rect buttonRect = Rect.zero;
				if (hasBody)
				{
					if (hasButton)
					{
						buttonRect = contentRect;
						buttonRect.yMin += Mathf.Max(0f, Toast.Style.CalcHeight(toast.Body, contentRect.width)) + 8f;
						buttonRect.width = buttonWidth;
					}
				}
				else if (hasButton)
				{
					buttonRect = headerRect.TakeFromRight(buttonWidth);
				}
				if (hasButton)
				{
					Rect hit = buttonRect.Expand(4f);
					EditorGUIUtility.AddCursorRect(hit, MouseCursor.Link);
					if (e.type == EventType.MouseDown && hit.Contains(e.mousePosition))
					{
						toast.ButtonOnClick();
						e.Use();
					}
				}
				if (e.type == EventType.MouseDown && rect.Contains(e.mousePosition))
				{
					toast.TimePassed = toast.ExpiryTime + 0.3f;
					e.Use();
				}
			}
			GUI.EndClip();
		}

		private void DrawToasts()
		{
			if (toasts == null || toasts.Count == 0)
			{
				return;
			}
			float toastOffsetTopLeft = 16f;
			float toastOffsetTopRight = 16f;
			float toastOffsetBottomLeft = 16f;
			float toastOffsetBottomRight = 16f;
			Rect area = ((ToastPopupArea == Rect.zero) ? base.position.SetPosition(Vector2.zero) : ToastPopupArea);
			Event e = Event.current;
			GUI.BeginClip(area);
			area.position = Vector2.zero;
			Vector2 start = default(Vector2);
			Vector2 target = default(Vector2);
			for (int i = 0; i < toasts.Count; i++)
			{
				Toast toast = toasts[i];
				bool hasButton = toast.ButtonOnClick != null;
				bool hasBody = !string.IsNullOrEmpty(toast.Body);
				Vector2 contentSize = Vector2.zero;
				contentSize.x = Toast.StyleHeader.CalcWidth(toast.Header);
				contentSize.y = Toast.StyleHeader.CalcHeight(toast.Header, contentSize.x);
				float buttonWidth = 0f;
				if (hasButton && !hasBody)
				{
					buttonWidth = Toast.Style.CalcWidth(toast.ButtonText);
					contentSize.x += 24f + buttonWidth;
				}
				float headerHeight = contentSize.y;
				float bodyHeight = 0f;
				if (hasBody)
				{
					contentSize.y += 4f;
					float bodyWidth = Toast.Style.CalcWidth(toast.Body);
					if (bodyWidth > 360f)
					{
						bodyWidth = 360f;
					}
					if (contentSize.x < bodyWidth)
					{
						contentSize.x = bodyWidth;
					}
					bodyHeight = Toast.Style.CalcHeight(toast.Body, bodyWidth);
					contentSize.y += bodyHeight;
					if (hasButton)
					{
						contentSize.y += 8f;
						buttonWidth = Toast.Style.CalcWidth(toast.ButtonText);
						if (buttonWidth > contentSize.x)
						{
							contentSize.x = buttonWidth;
						}
						contentSize.y += Toast.Style.CalcHeight(toast.ButtonText, buttonWidth);
					}
				}
				Vector2 toastSize = contentSize;
				toastSize.x += 16f;
				toastSize.y += 17.5f;
				if (toast.Icon != SdfIconType.None)
				{
					toastSize.x += Toast.StyleHeader.lineHeight + 8f + 8f;
				}
				switch (toast.ToastPosition)
				{
				default:
					start.x = area.x + 16f;
					start.y = area.y - toastSize.y;
					target.x = start.x;
					target.y = area.y + toastOffsetTopLeft;
					toastOffsetTopLeft += toastSize.y + 16f;
					break;
				case ToastPosition.TopRight:
					start.x = area.xMax - toastSize.x - 16f;
					start.y = area.y - toastSize.y;
					target.x = start.x;
					target.y = area.y + toastOffsetTopRight;
					toastOffsetTopRight += toastSize.y + 16f;
					break;
				case ToastPosition.BottomLeft:
					start.x = area.x + 16f;
					start.y = area.height;
					target.x = start.x;
					target.y = start.y - toastOffsetBottomLeft - toastSize.y;
					toastOffsetBottomLeft += toastSize.y + 16f;
					break;
				case ToastPosition.BottomRight:
					start.x = area.width - toastSize.x - 16f;
					start.y = area.height;
					target.x = start.x;
					target.y = start.y - toastOffsetBottomRight - toastSize.y;
					toastOffsetBottomRight += toastSize.y + 16f;
					break;
				}
				UnityShims.Rect.Ctor(out var targetRect, target, toastSize);
				if (toast.CurrentRect == Rect.zero)
				{
					toast.CurrentRect = UnityShims.Rect.Ctor(start, toastSize);
				}
				if (e.type == EventType.Layout)
				{
					toast.TimePassed += GUITimeHelper.LayoutDeltaTime;
				}
				float alpha = Mathf.Clamp(toast.TimePassed / 0.25f, 0f, 1f);
				if (toast.TimePassed > toast.ExpiryTime)
				{
					float timePassedSinceGoingOverDuration = toast.TimePassed - toast.ExpiryTime;
					float fadeOutAlpha = Mathf.Clamp01(1f - timePassedSinceGoingOverDuration / 0.5f);
					alpha = Mathf.Min(alpha, fadeOutAlpha);
				}
				toast.CurrentRect.y = Mathf.MoveTowards(toast.CurrentRect.y, targetRect.y, GUITimeHelper.LayoutDeltaTime * 175f);
				Rect rect = toast.CurrentRect;
				if (area.Overlaps(rect))
				{
					DesignerGUI.DrawRoundBlur6(rect, new Color(0f, 0f, 0f, 0.12f * alpha));
					Color bgColor;
					Color borderColor;
					Color fgColor;
					if (EditorGUIUtility.isProSkin)
					{
						bgColor = new Color(0.12f, 0.12f, 0.12f, alpha);
						borderColor = new Color(0.2f, 0.2f, 0.2f, alpha);
						fgColor = new Color(1f, 1f, 1f, alpha);
					}
					else
					{
						bgColor = new Color(0.9f, 0.9f, 0.9f, alpha);
						borderColor = new Color(0.98f, 0.98f, 0.98f, alpha);
						fgColor = new Color(0.12f, 0.12f, 0.12f, alpha);
					}
					Color lerpAccent = toast.Color;
					if (lerpAccent == Color.clear && EditorGUIUtility.isProSkin)
					{
						lerpAccent = Color.white;
					}
					Color accentColor = Color.Lerp(fgColor, lerpAccent, 0.2f);
					accentColor.a = alpha;
					SirenixEditorGUI.DrawRoundRect(rect, bgColor, 4f, borderColor, 1f);
					Rect contentRect = rect;
					Rect durationRect = contentRect.TakeFromBottom(1.5f);
					Color durationColor = Color.Lerp(bgColor, lerpAccent, 0.35f);
					durationColor.a = alpha;
					durationRect.width *= Mathf.Clamp01(toast.TimePassed / toast.ExpiryTime);
					GUI.BeginClip(durationRect);
					SirenixEditorGUI.DrawRoundRect(rect.SetPosition(new Vector2(0f, 0f - rect.height + 1.5f)), durationColor, 4f);
					GUI.EndClip();
					contentRect = contentRect.Padding(8f);
					if (toast.Icon != SdfIconType.None)
					{
						Rect iconRect = contentRect.TakeFromLeft(Toast.StyleHeader.lineHeight);
						SdfIcons.DrawIcon(iconRect.AlignTop(Toast.StyleHeader.lineHeight), toast.Icon, fgColor);
						contentRect.xMin += 8f;
						contentRect.xMax -= 8f;
					}
					Rect headerRect = contentRect.TakeFromTop(headerHeight);
					contentRect.yMin += 4f;
					Toast.StyleHeader.normal.textColor = fgColor;
					GUI.Label(headerRect, toast.Header, Toast.StyleHeader);
					Rect buttonRect = Rect.zero;
					if (hasBody)
					{
						Toast.Style.normal.textColor = fgColor;
						GUI.Label(contentRect, toast.Body, Toast.Style);
						if (hasButton)
						{
							buttonRect = contentRect;
							buttonRect.yMin += bodyHeight + 8f;
							buttonRect.width = buttonWidth;
							Toast.Style.normal.textColor = accentColor;
							GUI.Label(buttonRect, toast.ButtonText, Toast.Style);
							EditorGUI.DrawRect(buttonRect.AlignBottom(1f), accentColor);
						}
					}
					else if (hasButton)
					{
						buttonRect = headerRect.TakeFromRight(buttonWidth);
						Toast.Style.normal.textColor = accentColor;
						GUI.Label(buttonRect, toast.ButtonText, Toast.Style);
						EditorGUI.DrawRect(buttonRect.AlignBottom(1f), accentColor);
					}
					if (toast.Color != Color.clear)
					{
						Color alphaAdjusted = toast.Color;
						alphaAdjusted.a = 0.2f * alpha;
						SirenixEditorGUI.DrawRoundRect(rect, alphaAdjusted, 4f);
					}
				}
				if (alpha == 0f)
				{
					toasts.RemoveAt(i--);
				}
				GUIHelper.RequestRepaint();
			}
			GUI.EndClip();
		}

		public virtual void AddItemsToMenu(GenericMenu menu)
		{
			if (this is OdinMenuEditorWindow menuEditor)
			{
				object selection = menuEditor.MenuTree?.Selection?.SelectedValue;
				if (selection == null)
				{
					menu.AddDisabledItem(new GUIContent("Customize Selected Menu Item in Visual Designer", "No menu item is currently selected, so this option can't be picked."));
					return;
				}
				string selectionName = ((!(selection is Type selectionType)) ? selection.GetType().GetNiceName() : selectionType.GetNiceName());
				menu.AddItem(new GUIContent("Customize '" + selectionName + "' in Visual Designer"), on: false, delegate
				{
					OdinVisualDesigner.OpenForInstance(selection);
				});
			}
			else
			{
				menu.AddItem(new GUIContent("Customize in Visual Designer"), on: false, delegate
				{
					OdinVisualDesigner.OpenForInstance(this);
				});
			}
		}

		internal void RefreshPropertyTrees()
		{
			if (propertyTrees != null)
			{
				for (int i = 0; i < propertyTrees.Length; i++)
				{
					PropertyTree tree = propertyTrees[i];
					InspectorProperty root = tree?.RootProperty;
					if (root != null)
					{
						tree.hasSetupSearchFilter = false;
						root.RefreshSetup();
					}
				}
			}
			Repaint();
		}
	}
}
