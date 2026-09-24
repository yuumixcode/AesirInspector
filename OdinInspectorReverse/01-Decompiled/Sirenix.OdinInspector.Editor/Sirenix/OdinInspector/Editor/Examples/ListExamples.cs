using System;
using System.Collections.Generic;
using Sirenix.OdinInspector.Editor.Examples.Internal;
using Sirenix.Utilities.Editor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Examples
{
	[AttributeExample(typeof(ListDrawerSettingsAttribute))]
	[ExampleAsComponentData(Namespaces = new string[] { "System", "System.Collections.Generic", "Sirenix.Utilities.Editor" })]
	internal class ListExamples
	{
		[Serializable]
		public struct SomeStruct
		{
			public string SomeString;

			public int One;

			public int Two;

			public int Three;
		}

		[Serializable]
		public struct SomeOtherStruct
		{
			[HideLabel]
			[PreviewField(50f, ObjectFieldAlignment.Left)]
			[PropertyOrder(-1f)]
			[HorizontalGroup("Split", 55f, 0, 0, 0f)]
			public MonoBehaviour SomeObject;

			[FoldoutGroup("Split/$Name", false, 0f)]
			public int A;

			[FoldoutGroup("Split/$Name", false, 0f)]
			public int B;

			[FoldoutGroup("Split/$Name", false, 0f)]
			public int C;

			[FoldoutGroup("Split/$Name", false, 0f)]
			public int Two;

			[FoldoutGroup("Split/$Name", false, 0f)]
			public int Three;

			private string Name
			{
				get
				{
					if (!SomeObject)
					{
						return "Null";
					}
					return SomeObject.name;
				}
			}
		}

		[Title("List Basics", null, TitleAlignments.Left, true, true)]
		[InfoBox("List elements can now be dragged around to reorder them and deleted individually, and lists have paging (try adding a lot of elements!). You can still drag many assets at once into lists from the project view - just drag them into the list itself and insert them where you want to add them.", InfoMessageType.Info, null)]
		public List<float> FloatList;

		[Range(0f, 1f)]
		[InfoBox("Applying a [Range] attribute to this list instead applies it to all of its float entries.", InfoMessageType.Info, null)]
		public float[] FloatRangeArray;

		[InfoBox("Lists can be made read-only in different ways.", InfoMessageType.Info, null)]
		[ListDrawerSettings(IsReadOnly = true)]
		public int[] ReadOnlyArray1 = new int[3] { 1, 2, 3 };

		[ReadOnly]
		public int[] ReadOnlyArray2 = new int[3] { 1, 2, 3 };

		public SomeOtherStruct[] SomeStructList;

		[Title("Advanced List Customization", null, TitleAlignments.Left, true, true)]
		[ListDrawerSettings(NumberOfItemsPerPage = 5)]
		[InfoBox("Using [ListDrawerSettings], lists can be customized in a wide variety of ways.", InfoMessageType.Info, null)]
		public int[] FiveItemsPerPage;

		[ListDrawerSettings(ShowIndexLabels = true, ListElementLabelName = "SomeString")]
		public SomeStruct[] IndexLabels;

		[ListDrawerSettings(DraggableItems = false, ShowFoldout = false, ShowIndexLabels = true, ShowPaging = false, ShowItemCount = false, HideRemoveButton = true)]
		public int[] MoreListSettings = new int[3] { 1, 2, 3 };

		[ListDrawerSettings(OnBeginListElementGUI = "BeginDrawListElement", OnEndListElementGUI = "EndDrawListElement")]
		public SomeStruct[] InjectListElementGUI;

		[ListDrawerSettings(OnTitleBarGUI = "DrawRefreshButton")]
		public List<int> CustomButtons;

		[ListDrawerSettings(CustomAddFunction = "CustomAddFunction")]
		public List<int> CustomAddBehaviour;

		[OnInspectorGUI]
		[PropertyOrder(-2.1474836E+09f)]
		private void DrawIntroInfoBox()
		{
			SirenixEditorGUI.InfoMessageBox("Out of the box, Odin significantly upgrades the drawing of lists and arrays in the inspector - across the board, without you ever lifting a finger.");
		}

		private void BeginDrawListElement(int index)
		{
			SirenixEditorGUI.BeginBox(InjectListElementGUI[index].SomeString, false);
		}

		private void EndDrawListElement(int index)
		{
			SirenixEditorGUI.EndBox();
		}

		private void DrawRefreshButton()
		{
			if (SirenixEditorGUI.ToolbarButton(EditorIcons.Refresh))
			{
				Debug.Log(CustomButtons.Count.ToString());
			}
		}

		private int CustomAddFunction()
		{
			return CustomAddBehaviour.Count;
		}
	}
}
