using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Sirenix.Reflection.Editor;
using Sirenix.Serialization;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Sirenix.OdinInspector.Editor.Internal.UIToolkitIntegration
{
	public static class ImguiElementUtils
	{
		private class CallbackInvokerContext
		{
			public Action<SerializedProperty> Action;
		}

		public static class ImGuiVisualElementLayoutEntry
		{
			public static GUILayoutEntry_Internal<OdinImGuiElement> Create(OdinImGuiElement element)
			{
				return GUILayoutEntry_Internal<OdinImGuiElement>.CreateCustom(element, SetVertical, SetHorizontal, CalcWidth, CalcHeight);
			}

			private static void CalcWidth(ref GUILayoutEntry_Internal<OdinImGuiElement> entry)
			{
			}

			private static void CalcHeight(ref GUILayoutEntry_Internal<OdinImGuiElement> entry)
			{
			}

			private static void SetVertical(ref GUILayoutEntry_Internal<OdinImGuiElement> entry, float y, float height)
			{
				entry.rect.y = y;
				Update(ref entry);
			}

			private static void SetHorizontal(ref GUILayoutEntry_Internal<OdinImGuiElement> entry, float x, float width)
			{
				entry.rect.x = x;
				entry.minWidth = width;
				entry.maxWidth = width;
				Update(ref entry);
			}

			private static void Update(ref GUILayoutEntry_Internal<OdinImGuiElement> entry)
			{
				Vector2 size = entry.Value.GetSize(entry.minWidth);
				entry.maxHeight = size.y;
				entry.minHeight = size.y;
				entry.rect.width = entry.minWidth;
				entry.rect.height = entry.minHeight;
				entry.Value.RectAtPostLayoutTime = entry.rect;
			}
		}

		private class ImGUIElementDrawer
		{
			private bool toDrawListChanged;

			public int CurrentIndex;

			public Panel_Internal Panel;

			public IMGUIContainer ImGuiContainer;

			public VisualElement ElementContainer;

			public List<OdinImGuiElement> ToDraw = new List<OdinImGuiElement>();

			public HashSet<OdinImGuiElement> CurrentElements = new HashSet<OdinImGuiElement>();

			public ImGUIElementDrawer(IMGUIContainer container)
			{
				if (!IsSupported)
				{
					throw new NotSupportedException("UIToolkit is not supported by Odin in this version of Unity; it requires version 2020.2 or above.");
				}
				ImGuiContainer = container;
				ImGuiContainer.RegisterCallback<GeometryChangedEvent>(delegate
				{
					Hook();
					ProcessElements();
				});
				Panel = new Panel_Internal(container.panel);
			}

			public void BeginContainerCallback()
			{
				CurrentIndex = 0;
			}

			public void ProcessElements()
			{
				if (ElementContainer != null && ElementContainer.hierarchy.childCount != CurrentIndex)
				{
					toDrawListChanged = true;
				}
				if (ImGuiContainer != null)
				{
					VisualElement parent = ImGuiContainer.parent;
					if (parent != null && ElementContainer != null && ElementContainer.parent == parent)
					{
						int next = parent.IndexOf(ImGuiContainer) + 1;
						if ((next < parent.hierarchy.childCount && parent.hierarchy[next] != ElementContainer) || next == parent.hierarchy.childCount)
						{
							UnityEditorEventUtility.DelayAction(delegate
							{
								ElementContainer.PlaceInFront(ImGuiContainer);
							});
						}
					}
				}
				if (toDrawListChanged && (Event.current == null || Event.current.type != EventType.Layout))
				{
					Hook();
					ToDraw.SetLength(CurrentIndex);
					int k = 0;
					if (k < 3)
					{
						bool changed = false;
						for (int i = 0; i < ElementContainer.hierarchy.childCount && i < ToDraw.Count; i++)
						{
							if (ElementContainer.hierarchy[i] == ToDraw[i])
							{
								continue;
							}
							if (ToDraw[i].hierarchy.parent != ElementContainer)
							{
								ElementContainer.Insert(i, ToDraw[i]);
								ToDraw[i].OnAdd();
								changed = true;
								continue;
							}
							ElementContainer.RemoveAt(i);
							if (i >= ElementContainer.hierarchy.childCount || ElementContainer.hierarchy[i] != ToDraw[i])
							{
								ElementContainer.Insert(i, ToDraw[i]);
								ToDraw[i].OnAdd();
							}
							else
							{
								i--;
							}
							changed = true;
						}
					}
					for (int i2 = ToDraw.Count; i2 < ElementContainer.hierarchy.childCount; i2++)
					{
						ElementContainer.RemoveAt(i2);
					}
					for (int i3 = ElementContainer.hierarchy.childCount; i3 < ToDraw.Count; i3++)
					{
						ElementContainer.Add(ToDraw[i3]);
						ToDraw[i3].OnAdd();
					}
					toDrawListChanged = false;
				}
				for (int i4 = 0; i4 < CurrentIndex; i4++)
				{
					OdinImGuiElement e = ToDraw[i4];
					e.AbsoluteRect = e.RectAtPostLayoutTime;
					Vector2 layoutOffset = Vector2.zero;
					Vector2 viewOffset = Vector2.zero;
					Vector2 clipOffset = Vector2.zero;
					if (e.ClipRect.HasValue)
					{
						clipOffset += e.ClipRect.Value.position;
					}
					for (int j = 0; j < e.ParentLayoutGroups.Count; j++)
					{
						GUILayoutGroup_Internal g = e.ParentLayoutGroups[j];
						if (g.resetCoords)
						{
							layoutOffset += g.rect.position;
						}
					}
					for (int j2 = 0; j2 < e.ScrollViewStates.Count; j2++)
					{
						viewOffset += e.ScrollViewStates[j2].scrollPosition;
					}
					ref Rect absoluteRect = ref e.AbsoluteRect;
					absoluteRect.position += layoutOffset - viewOffset - clipOffset;
					float left = e.style.left.value.value;
					float top = e.style.top.value.value;
					float width = e.style.width.value.value;
					float height = e.style.height.value.value;
					if (e.ClipRect.HasValue && (left != e.ClipRect.Value.x || top != e.ClipRect.Value.y || width != e.ClipRect.Value.width || height != e.ClipRect.Value.height))
					{
						e.style.left = e.ClipRect.Value.x;
						e.style.top = e.ClipRect.Value.y;
						e.style.width = e.ClipRect.Value.width;
						e.style.height = e.ClipRect.Value.height;
						e.CurrClip = e.ClipRect.Value;
					}
					StyleLength left2 = e.PositioningContainer.style.left;
					StyleLength top2 = e.PositioningContainer.style.top;
					StyleLength width2 = e.PositioningContainer.style.width;
					if (left2 != e.AbsoluteRect.x || top2 != e.AbsoluteRect.y || width2 != e.AbsoluteRect.width)
					{
						e.PositioningContainer.style.left = e.AbsoluteRect.x;
						e.PositioningContainer.style.top = e.AbsoluteRect.y;
						e.PositioningContainer.style.width = e.AbsoluteRect.width;
					}
				}
			}

			public void ToDrawListChanged()
			{
				toDrawListChanged = true;
			}

			private void Hook()
			{
				if (ElementContainer == null)
				{
					ElementContainer = new VisualElement();
					ElementContainer.style.position = Position.Absolute;
					ElementContainer.style.left = 0f;
					ElementContainer.style.top = 0f;
					ElementContainer.style.width = Length.Percent(100f);
					ElementContainer.style.height = Length.Percent(100f);
					ElementContainer.style.overflow = Overflow.Hidden;
					ElementContainer.pickingMode = PickingMode.Ignore;
					ImGuiContainer.parent.Add(ElementContainer);
					ImGuiContainer.parent.RemoveFromClassList("unity-inspector-element");
				}
			}
		}

		private const string NotSupportedMessage = "UIToolkit is not supported by Odin in this version of Unity; it requires version 2020.2 or above.";

		public static readonly bool IsSupported;

		private static readonly Type SerializedPropertyChangeEvent_Type;

		private static readonly PropertyInfo SerializedPropertyChangeEvent_changedProperty_Prop;

		private static readonly MethodInfo CallbackEventHandler_RegisterCallback_MethodDefinition;

		private static readonly MethodInfo CallbackEventHandler_RegisterCallback_SerializedPropertyChangeEvent_Method;

		private static readonly Type EventCallback_SerializedPropertyChangeEvent_Type;

		private static readonly DynamicMethod EmittedCallbackInvoker;

		private static Dictionary<IMGUIContainer, ImGUIElementDrawer> imGUIElementDrawerLookup;

		private static ScriptableObject owner;

		private static Panel_Internal panel;

		private static VisualElement visualTree;

		static ImguiElementUtils()
		{
			IsSupported = true;
			imGUIElementDrawerLookup = new Dictionary<IMGUIContainer, ImGUIElementDrawer>();
			SerializedPropertyChangeEvent_Type = TwoWaySerializationBinder.Default.BindToType("UnityEditor.UIElements.SerializedPropertyChangeEvent");
			SerializedPropertyChangeEvent_changedProperty_Prop = SerializedPropertyChangeEvent_Type?.GetProperty("changedProperty", BindingFlags.Instance | BindingFlags.Public);
			MethodInfo[] methods = typeof(CallbackEventHandler).GetMethods(BindingFlags.Instance | BindingFlags.Public);
			foreach (MethodInfo method in methods)
			{
				if (method.Name == "RegisterCallback" && method.IsGenericMethodDefinition)
				{
					ParameterInfo[] parameters = method.GetParameters();
					if (parameters.Length == 2 && parameters[0].ParameterType.IsGenericType && parameters[0].ParameterType.GetGenericTypeDefinition() == typeof(EventCallback<>) && parameters[1].ParameterType == typeof(TrickleDown))
					{
						CallbackEventHandler_RegisterCallback_MethodDefinition = method;
						break;
					}
				}
			}
			if (SerializedPropertyChangeEvent_Type == null || SerializedPropertyChangeEvent_changedProperty_Prop == null || CallbackEventHandler_RegisterCallback_MethodDefinition == null)
			{
				IsSupported = false;
				return;
			}
			EventCallback_SerializedPropertyChangeEvent_Type = typeof(EventCallback<>).MakeGenericType(SerializedPropertyChangeEvent_Type);
			CallbackEventHandler_RegisterCallback_SerializedPropertyChangeEvent_Method = CallbackEventHandler_RegisterCallback_MethodDefinition.MakeGenericMethod(SerializedPropertyChangeEvent_Type);
			DynamicMethod method2 = new DynamicMethod("ImguiElementUtils.EmittedCallbackInvoker", typeof(void), new Type[2]
			{
				typeof(CallbackInvokerContext),
				SerializedPropertyChangeEvent_Type
			}, restrictedSkipVisibility: true);
			ILGenerator il = method2.GetILGenerator();
			il.Emit(OpCodes.Ldarg_0);
			il.Emit(OpCodes.Ldfld, typeof(CallbackInvokerContext).GetField("Action", BindingFlags.Instance | BindingFlags.Public));
			il.Emit(OpCodes.Ldarg_1);
			il.Emit(OpCodes.Callvirt, SerializedPropertyChangeEvent_changedProperty_Prop.GetGetMethod(nonPublic: true));
			il.Emit(OpCodes.Call, typeof(Action<SerializedProperty>).GetMethod("Invoke", BindingFlags.Instance | BindingFlags.Public));
			il.Emit(OpCodes.Ret);
			EmittedCallbackInvoker = method2;
		}

		[InitializeOnLoadMethod]
		private static void Init()
		{
			if (!IsSupported)
			{
				return;
			}
			UIElementsUtility_Internals.BeginContainerCallback += delegate(IMGUIContainer container)
			{
				if (imGUIElementDrawerLookup.TryGetValue(container, out var value))
				{
					value.BeginContainerCallback();
				}
			};
			UIElementsUtility_Internals.EndContainerCallback += delegate(IMGUIContainer container)
			{
				if (imGUIElementDrawerLookup.TryGetValue(container, out var value))
				{
					value.ProcessElements();
				}
			};
		}

		public static void RegisterSerializedPropertyChangeEventCallback(PropertyField propertyField, Action<SerializedProperty> callback)
		{
			if (!IsSupported)
			{
				throw new NotSupportedException("UIToolkit is not supported by Odin in this version of Unity; it requires version 2020.2 or above.");
			}
			CallbackInvokerContext ctx = new CallbackInvokerContext
			{
				Action = callback
			};
			Delegate del = EmittedCallbackInvoker.CreateDelegate(EventCallback_SerializedPropertyChangeEvent_Type, ctx);
			CallbackEventHandler_RegisterCallback_SerializedPropertyChangeEvent_Method.Invoke(propertyField, new object[2]
			{
				del,
				TrickleDown.NoTrickleDown
			});
		}

		public static OdinImGuiElement CreatePropertyFieldElement(SerializedProperty unityProperty, Action<SerializedProperty> callback = null)
		{
			if (!IsSupported)
			{
				throw new NotSupportedException("UIToolkit is not supported by Odin in this version of Unity; it requires version 2020.2 or above.");
			}
			if (unityProperty == null)
			{
				throw new ArgumentNullException("unityProperty");
			}
			PropertyField propField = new PropertyField(unityProperty);
			if (callback != null)
			{
				RegisterSerializedPropertyChangeEventCallback(propField, callback);
			}
			OdinImGuiElement element = new OdinImGuiElement(propField, unityProperty, delegate
			{
				PropertyField element2 = new PropertyField(unityProperty);
				OdinImGuiElement odinImGuiElement = new OdinImGuiElement(element2, unityProperty);
				odinImGuiElement.Bind(unityProperty.serializedObject);
				return odinImGuiElement;
			});
			element.Bind(unityProperty.serializedObject);
			return element;
		}

		public static Rect EmbedVisualElementAndDrawItHere(OdinImGuiElement element, GUIContent label)
		{
			if (!IsSupported)
			{
				throw new NotSupportedException("UIToolkit is not supported by Odin in this version of Unity; it requires version 2020.2 or above.");
			}
			Rect rect = EmbedVisualElementAndDrawItHere(element);
			if (Event.current.type == EventType.Layout && element.LabelElement != null)
			{
				DisplayStyle display = ((label == null || label.text == "") ? DisplayStyle.None : DisplayStyle.Flex);
				if (element.CachedDisplay != display)
				{
					element.LabelElement.style.display = display;
					element.CachedDisplay = display;
				}
				if (display == DisplayStyle.Flex)
				{
					string text = label?.text ?? "";
					string tooltip = label?.tooltip ?? "";
					if (element.CachedText != text)
					{
						element.LabelElement.text = text;
						element.CachedText = text;
					}
					if (element.CachedTooltip != tooltip)
					{
						element.LabelElement.tooltip = tooltip;
						element.CachedTooltip = tooltip;
					}
				}
				SetImGuiLabelWidths(element);
			}
			return rect;
		}

		public static Rect EmbedVisualElementAndDrawItHere(OdinImGuiElement element)
		{
			if (!IsSupported)
			{
				throw new NotSupportedException("UIToolkit is not supported by Odin in this version of Unity; it requires version 2020.2 or above.");
			}
			IMGUIContainer imGuiContainer = UIElementsUtility_Internals.GetCurrentIMGUIContainer();
			if (imGuiContainer == null)
			{
				Debug.LogError("You are not allowed to embed visual elements into IMGUI outside of an IMGUI container draw call.");
				return default(Rect);
			}
			if (!imGUIElementDrawerLookup.TryGetValue(imGuiContainer, out var drawer))
			{
				if (Event.current.type != EventType.Layout)
				{
					return default(Rect);
				}
				drawer = new ImGUIElementDrawer(imGuiContainer);
				imGUIElementDrawerLookup[imGuiContainer] = drawer;
			}
			element.LastImGUIContainer = imGuiContainer;
			if (drawer.CurrentIndex == drawer.ToDraw.Count)
			{
				drawer.ToDraw.Add(element);
				drawer.ToDrawListChanged();
			}
			else if (drawer.ToDraw[drawer.CurrentIndex] != element)
			{
				drawer.ToDraw[drawer.CurrentIndex] = element;
				drawer.ToDrawListChanged();
			}
			drawer.CurrentIndex++;
			Rect rect = GetImGUILayoutRectForElement(element);
			if (element.HasElementsThatWantsFocus)
			{
				int controlId = GUIUtility.GetControlID(FocusType.Keyboard);
				if (Event.current.type == EventType.Layout && GUIUtility.keyboardControl == controlId && imGuiContainer.focusController.focusedElement == imGuiContainer)
				{
					((!element.ShiftWasHeldPriorToShiftTabCommand) ? element.Query().ToList().OfType<Focusable>()
						.FirstOrDefault((Focusable x) => !(x is OdinImGuiFocusRelay) && x.focusable) : element.Query().ToList().OfType<Focusable>()
						.LastOrDefault((Focusable x) => !(x is OdinImGuiFocusRelay) && x.focusable))?.Focus();
				}
				if (element.SetImGuiFocusTo != 0)
				{
					GUIUtility.keyboardControl = controlId + element.SetImGuiFocusTo;
					element.SetImGuiFocusTo = 0;
				}
			}
			element.ShiftWasHeldPriorToShiftTabCommand = ((UnityShims.EventModifiers)UnityShims.Misc.GetEventModifiers(Event.current)).HasFlag(UnityShims.EventModifiers.Shift);
			if (Event.current.type == EventType.Repaint)
			{
				float alpha = GUI.color.a;
				bool enabled = GUI.enabled;
				if (element.CachedOpacity != alpha)
				{
					element.PositioningContainer.style.opacity = alpha;
				}
				if (element.CachedEnabled != enabled)
				{
					element.CachedEnabled = enabled;
					element.SetEnabled(enabled);
				}
				element.ClipRect = GUIClipInfo.TopMostRect;
			}
			else if (Event.current.type == EventType.Layout)
			{
				SetImGuiLabelWidths(element);
			}
			return rect;
		}

		private static void SetImGuiLabelWidths(OdinImGuiElement element)
		{
			if (!element.LabelElementIsPropertyFieldLabel)
			{
				return;
			}
			float indent = GUIHelper.CurrentIndentAmount;
			float width = GUIHelper.BetterLabelWidth - indent;
			if (element.LabelElement != null)
			{
				if (element.LabelElement.style.width != width)
				{
					element.LabelElement.style.width = width;
				}
				if (element.LabelElement.style.minWidth != 0f)
				{
					element.LabelElement.style.minWidth = 0f;
				}
				if (element.LabelElement.style.marginLeft != indent)
				{
					element.LabelElement.style.marginLeft = indent;
				}
			}
		}

		private static void EnsureInit()
		{
			if (panel.IPanel == null)
			{
				owner = ScriptableObject.CreateInstance<ScriptableObject>();
				panel = Panel_Internals.CreateEditorPanel(owner);
				visualTree = panel.IPanel.visualTree;
				visualTree.style.left = 0f;
				visualTree.style.top = 0f;
				visualTree.style.width = 100f;
				visualTree.style.height = 100f;
				visualTree.style.minHeight = 100f;
				visualTree.style.position = Position.Absolute;
				visualTree.style.top = 0f;
				visualTree.style.left = 0f;
				visualTree.style.right = 0f;
				visualTree.style.bottom = 0f;
				visualTree.style.backgroundColor = Color.clear;
			}
		}

		public static IEnumerable<StyleSheet> GetAllStyleSheets(VisualElement e)
		{
			if (!IsSupported)
			{
				throw new NotSupportedException("UIToolkit is not supported by Odin in this version of Unity; it requires version 2020.2 or above.");
			}
			while (e != null)
			{
				for (int i = 0; i < e.styleSheets.count; i++)
				{
					yield return e.styleSheets[i];
				}
				e = e.parent;
			}
		}

		public static Rect MeasureSizeOfVisualElement(VisualElement targetContainer, VisualElement element, float width)
		{
			if (!IsSupported)
			{
				throw new NotSupportedException("UIToolkit is not supported by Odin in this version of Unity; it requires version 2020.2 or above.");
			}
			EnsureInit();
			if (panel.duringLayoutPhase)
			{
				Debug.LogError("TODO: Support recursive MeasureNow() (or don't) which could happen in UpdateVisualTreePhaseLayout()");
				return new Rect(0f, 0f, width, 30f);
			}
			visualTree.styleSheets.Clear();
			foreach (StyleSheet item in GetAllStyleSheets(targetContainer))
			{
				visualTree.styleSheets.Add(item);
			}
			panel.ApplyStyles();
			visualTree.Add(element);
			visualTree.style.width = width;
			panel.VisualTreeSetSize(new Vector2(width, 1000f));
			panel.UpdateVisualTreePhaseViewData();
			panel.UpdateVisualTreePhaseBindings();
			panel.UpdateVisualTreePhaseAnimation();
			panel.UpdateVisualTreePhaseStyles();
			panel.UpdateVisualTreePhaseLayout();
			Rect rect = element.contentRect;
			visualTree.Clear();
			return rect;
		}

		public static Rect GetImGUILayoutRectForElement(OdinImGuiElement element)
		{
			if (!IsSupported)
			{
				throw new NotSupportedException("UIToolkit is not supported by Odin in this version of Unity; it requires version 2020.2 or above.");
			}
			switch (Event.current.type)
			{
			case EventType.Layout:
			{
				GUILayoutEntry_Internal<OdinImGuiElement> entry = ImGuiVisualElementLayoutEntry.Create(element);
				GUILayoutUtility_Internals.TopLevel.Add(entry);
				element.ParentLayoutGroups.Clear();
				foreach (GUILayoutGroup_Internal item in GUILayoutUtility_Internals.Current.LayoutGroups)
				{
					element.ParentLayoutGroups.Add(item);
				}
				element.ScrollViewStates.Clear();
				foreach (ScrollViewState_Internal item2 in GUI_Internals.ScrollViewStates)
				{
					element.ScrollViewStates.Add(item2);
				}
				return default(Rect);
			}
			case EventType.Used:
				return default(Rect);
			default:
				return GUILayoutUtility_Internals.TopLevel.GetNext().rect;
			}
		}
	}
}
