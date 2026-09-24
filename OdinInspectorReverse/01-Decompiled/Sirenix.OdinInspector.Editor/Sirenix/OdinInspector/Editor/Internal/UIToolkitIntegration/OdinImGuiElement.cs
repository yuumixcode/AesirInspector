using System;
using System.Collections.Generic;
using Sirenix.Reflection.Editor;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Sirenix.OdinInspector.Editor.Internal.UIToolkitIntegration
{
	public class OdinImGuiElement : VisualElement
	{
		private static int idCounter;

		private Vector2 measuredSize;

		private string expectedLabel;

		private bool isMeasuredBeforeAdd;

		private readonly Func<OdinImGuiElement> measurementCloneFactory;

		internal float CachedOpacity = -1f;

		internal bool CachedEnabled = true;

		internal DisplayStyle CachedDisplay;

		internal string CachedText;

		internal string CachedTooltip;

		internal Rect CurrClip;

		internal Rect RectAtPostLayoutTime;

		internal Rect? ClipRect;

		internal Rect AbsoluteRect;

		internal Rect PrevAbsoluteRect;

		internal List<GUILayoutGroup_Internal> ParentLayoutGroups = new List<GUILayoutGroup_Internal>();

		internal List<ScrollViewState_Internal> ScrollViewStates = new List<ScrollViewState_Internal>();

		internal IMGUIContainer LastImGUIContainer;

		public VisualElement PositioningContainer;

		public Label LabelElement;

		public bool LabelElementIsPropertyFieldLabel;

		public bool HasElementsThatWantsFocus = true;

		public int SetImGuiFocusTo;

		public bool ShiftWasHeldPriorToShiftTabCommand;

		private static string GetName(VisualElement element)
		{
			if (element is PropertyField p)
			{
				return p.bindingPath;
			}
			return element.name;
		}

		public OdinImGuiElement(VisualElement element, SerializedProperty unityProperty, Func<OdinImGuiElement> measurementCloneFactory = null)
		{
			base.name = $"({idCounter++}) : " + unityProperty.propertyPath;
			this.measurementCloneFactory = measurementCloneFactory;
			PositioningContainer = new VisualElement();
			PositioningContainer.name = "Odin Positioning Container (Value)";
			Add(PositioningContainer);
			FindLabel(element);
			PositioningContainer.Add(element);
			expectedLabel = unityProperty.displayName;
			SetupFocus();
		}

		private void FindLabel(VisualElement element)
		{
			EventCallback<GeometryChangedEvent> onGeometryChanged = null;
			onGeometryChanged = delegate
			{
				if (!string.IsNullOrWhiteSpace(expectedLabel))
				{
					Label label = FastRecursiveLabelFinder(element);
					if (label != null && string.Equals(label.text, expectedLabel, StringComparison.InvariantCultureIgnoreCase) && (label.ClassListContains("unity-property-field__label") || label.ClassListContains("unity-base-field__label")))
					{
						element.UnregisterCallback(onGeometryChanged);
						LabelElement = label;
						LabelElementIsPropertyFieldLabel = true;
						CachedDisplay = LabelElement.style.display.value;
						CachedText = LabelElement.text;
					}
				}
			};
			element.RegisterCallback(onGeometryChanged);
		}

		public OdinImGuiElement(SerializedProperty unityProperty)
		{
			base.name = $"Odin Clip Container ({idCounter++}) : " + unityProperty.propertyPath;
			measurementCloneFactory = delegate
			{
				SerializedProperty serializedProperty = unityProperty.Copy();
				OdinImGuiElement odinImGuiElement = new OdinImGuiElement(serializedProperty);
				odinImGuiElement.Bind(serializedProperty.serializedObject);
				return odinImGuiElement;
			};
			PositioningContainer = new VisualElement();
			PositioningContainer.name = "Odin Positioning Container (Value)";
			Add(PositioningContainer);
			PropertyField propField = new PropertyField(unityProperty);
			FindLabel(propField);
			PositioningContainer.Add(propField);
			expectedLabel = unityProperty.displayName;
			SetupFocus();
		}

		public OdinImGuiElement(VisualElement element)
		{
			base.name = $"Odin Clip ({idCounter++}) : " + GetName(element);
			PositioningContainer = new VisualElement();
			PositioningContainer.name = "Odin Element (Value)";
			Add(PositioningContainer);
			FindLabel(element);
			PositioningContainer.Add(element);
			SetupFocus();
		}

		private void SetupFocus()
		{
			Insert(0, new OdinImGuiFocusRelay(this, -1));
			Add(new OdinImGuiFocusRelay(this, 1));
		}

		internal Vector2 GetSize(float width)
		{
			if (!isMeasuredBeforeAdd)
			{
				isMeasuredBeforeAdd = true;
				if (LastImGUIContainer != null)
				{
					OdinImGuiElement elementToMeasure = measurementCloneFactory?.Invoke() ?? this;
					try
					{
						measuredSize = ImguiElementUtils.MeasureSizeOfVisualElement(LastImGUIContainer, elementToMeasure, width).size;
					}
					finally
					{
						if (elementToMeasure != this)
						{
							elementToMeasure.Unbind();
							elementToMeasure.RemoveFromHierarchy();
						}
					}
					return measuredSize;
				}
			}
			Vector2 size = PositioningContainer.contentRect.size;
			if (size.x == 0f || size.y == 0f || float.IsNaN(size.x) || float.IsNaN(size.y))
			{
				return measuredSize;
			}
			return size;
		}

		internal void OnAdd()
		{
			base.style.position = Position.Absolute;
			base.style.left = 0f;
			base.style.top = 0f;
			base.style.width = Length.Percent(100f);
			base.style.height = Length.Percent(100f);
			base.style.overflow = Overflow.Hidden;
			base.pickingMode = PickingMode.Ignore;
			PositioningContainer.style.position = Position.Absolute;
			PositioningContainer.pickingMode = PickingMode.Ignore;
		}

		private static Label FastRecursiveLabelFinder(VisualElement element, int depth = 0)
		{
			if (element is Label lbl)
			{
				return lbl;
			}
			if (depth >= 6)
			{
				return null;
			}
			int childCount = Math.Min(element.childCount, 10);
			for (int i = 0; i < childCount; i++)
			{
				Label label = FastRecursiveLabelFinder(element.hierarchy[i]);
				if (label != null)
				{
					return label;
				}
			}
			return null;
		}
	}
}
