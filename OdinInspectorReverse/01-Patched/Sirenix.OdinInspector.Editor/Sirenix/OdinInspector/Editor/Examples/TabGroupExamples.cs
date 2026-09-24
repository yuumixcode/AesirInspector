using System;
using Sirenix.OdinInspector.Editor.Examples.Internal;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Examples
{
	[AttributeExample(typeof(TabGroupAttribute))]
	[ExampleAsComponentData(Namespaces = new string[] { "System" })]
	internal class TabGroupExamples
	{
		[Serializable]
		[HideLabel]
		public class MyTabObject
		{
			public int A;

			public int B;

			public int C;
		}

		[TabGroup("General", false, 0f)]
		public string playerName1;

		[TabGroup("General", false, 0f)]
		public int playerLevel1;

		[TabGroup("General", false, 0f)]
		public string playerClass1;

		[TabGroup("Stats", false, 0f)]
		public int strength1;

		[TabGroup("Stats", false, 0f)]
		public int dexterity1;

		[TabGroup("Stats", false, 0f)]
		public int intelligence1;

		[TabGroup("Quests", false, 0f)]
		public bool hasMainQuest1;

		[TabGroup("Quests", false, 0f)]
		public int mainQuestProgress1;

		[TabGroup("Quests", false, 0f)]
		public bool hasSideQuest1;

		[TabGroup("Quests", false, 0f)]
		public int sideQuestProgress1;

		[TabGroup("tab2", "General", SdfIconType.ImageAlt, false, 0f, TextColor = "green")]
		public string playerName2;

		[TabGroup("tab2", "General", false, 0f)]
		public int playerLevel2;

		[TabGroup("tab2", "General", false, 0f)]
		public string playerClass2;

		[TabGroup("tab2", "Stats", SdfIconType.BarChartLineFill, false, 0f, TextColor = "blue")]
		public int strength2;

		[TabGroup("tab2", "Stats", false, 0f)]
		public int dexterity2;

		[TabGroup("tab2", "Stats", false, 0f)]
		public int intelligence2;

		[TabGroup("tab2", "Quests", SdfIconType.Question, false, 0f, TextColor = "@questColor", TabName = "")]
		public bool hasMainQuest2;

		[TabGroup("tab2", "Quests", false, 0f)]
		public Color questColor = new Color(1f, 0.5f, 0f);

		[TabGroup("shrink tabs", "Collection 1", false, 0f, TextColor = "blue")]
		[TabGroup("shrink tabs", "Collection 2", false, 0f, TextColor = "blue")]
		[TabGroup("shrink tabs", "Settings", SdfIconType.GearFill, false, 0f, TextColor = "grey")]
		[TabGroup("shrink tabs", "Abilities", false, 0f, TextColor = "green")]
		[TabGroup("shrink tabs", "Wand", SdfIconType.Magic, false, 0f, TextColor = "red")]
		[TabGroup("shrink tabs", "Character", SdfIconType.PersonFill, false, 0f, TextColor = "orange")]
		[TabGroup("shrink tabs", "World Map", SdfIconType.Map, false, 0f, TextColor = "orange", TabLayouting = TabLayouting.Shrink)]
		[TabGroup("shrink tabs", "Collection 4", false, 0f, TextColor = "blue")]
		[TabGroup("shrink tabs", "Collection 3", false, 0f, TextColor = "blue")]
		[TabGroup("shrink tabs", "Missions", SdfIconType.ExclamationSquareFill, false, 0f, TextColor = "yellow")]
		[TabGroup("shrink tabs", "Guide", SdfIconType.QuestionSquareFill, false, 0f, TextColor = "blue")]
		public float a;

		[TabGroup("shrink tabs", "Abilities", false, 0f, TextColor = "green")]
		[TabGroup("shrink tabs", "Wand", SdfIconType.Magic, false, 0f, TextColor = "red")]
		[TabGroup("shrink tabs", "Guide", SdfIconType.QuestionSquareFill, false, 0f, TextColor = "blue")]
		[TabGroup("shrink tabs", "Settings", SdfIconType.GearFill, false, 0f, TextColor = "grey")]
		[TabGroup("shrink tabs", "Collection 4", false, 0f, TextColor = "blue")]
		[TabGroup("shrink tabs", "World Map", SdfIconType.Map, false, 0f, TextColor = "orange", TabLayouting = TabLayouting.Shrink)]
		[TabGroup("shrink tabs", "Collection 2", false, 0f, TextColor = "blue")]
		[TabGroup("shrink tabs", "Collection 1", false, 0f, TextColor = "blue")]
		[TabGroup("shrink tabs", "Missions", SdfIconType.ExclamationSquareFill, false, 0f, TextColor = "yellow")]
		[TabGroup("shrink tabs", "Collection 3", false, 0f, TextColor = "blue")]
		[TabGroup("shrink tabs", "Character", SdfIconType.PersonFill, false, 0f, TextColor = "orange")]
		public float b;

		[TabGroup("shrink tabs", "World Map", SdfIconType.Map, false, 0f, TextColor = "orange", TabLayouting = TabLayouting.Shrink)]
		[TabGroup("shrink tabs", "Character", SdfIconType.PersonFill, false, 0f, TextColor = "orange")]
		[TabGroup("shrink tabs", "Guide", SdfIconType.QuestionSquareFill, false, 0f, TextColor = "blue")]
		[TabGroup("shrink tabs", "Wand", SdfIconType.Magic, false, 0f, TextColor = "red")]
		[TabGroup("shrink tabs", "Abilities", false, 0f, TextColor = "green")]
		[TabGroup("shrink tabs", "Missions", SdfIconType.ExclamationSquareFill, false, 0f, TextColor = "yellow")]
		[TabGroup("shrink tabs", "Collection 1", false, 0f, TextColor = "blue")]
		[TabGroup("shrink tabs", "Collection 2", false, 0f, TextColor = "blue")]
		[TabGroup("shrink tabs", "Collection 3", false, 0f, TextColor = "blue")]
		[TabGroup("shrink tabs", "Collection 4", false, 0f, TextColor = "blue")]
		[TabGroup("shrink tabs", "Settings", SdfIconType.GearFill, false, 0f, TextColor = "grey")]
		public float c;

		[TabGroup("shrink tabs", "Character", SdfIconType.PersonFill, false, 0f, TextColor = "orange")]
		[TabGroup("shrink tabs", "Wand", SdfIconType.Magic, false, 0f, TextColor = "red")]
		[TabGroup("shrink tabs", "Abilities", false, 0f, TextColor = "green")]
		[TabGroup("shrink tabs", "Missions", SdfIconType.ExclamationSquareFill, false, 0f, TextColor = "yellow")]
		[TabGroup("shrink tabs", "Collection 1", false, 0f, TextColor = "blue")]
		[TabGroup("shrink tabs", "Collection 2", false, 0f, TextColor = "blue")]
		[TabGroup("shrink tabs", "Collection 3", false, 0f, TextColor = "blue")]
		[TabGroup("shrink tabs", "Collection 4", false, 0f, TextColor = "blue")]
		[TabGroup("shrink tabs", "Settings", SdfIconType.GearFill, false, 0f, TextColor = "grey")]
		[TabGroup("shrink tabs", "Guide", SdfIconType.QuestionSquareFill, false, 0f, TextColor = "blue")]
		[TabGroup("shrink tabs", "World Map", SdfIconType.Map, false, 0f, TextColor = "orange", TabLayouting = TabLayouting.Shrink)]
		public float d;

		[TabGroup("multi row", "Settings", SdfIconType.GearFill, false, 0f, TextColor = "grey")]
		[TabGroup("multi row", "Collection 4", false, 0f, TextColor = "blue")]
		[TabGroup("multi row", "Collection 3", false, 0f, TextColor = "blue")]
		[TabGroup("multi row", "Collection 2", false, 0f, TextColor = "blue")]
		[TabGroup("multi row", "Character", SdfIconType.PersonFill, false, 0f, TextColor = "orange")]
		[TabGroup("multi row", "Missions", SdfIconType.ExclamationSquareFill, false, 0f, TextColor = "yellow")]
		[TabGroup("multi row", "Abilities", false, 0f, TextColor = "green")]
		[TabGroup("multi row", "Wand", SdfIconType.Magic, false, 0f, TextColor = "red")]
		[TabGroup("multi row", "World Map", SdfIconType.Map, false, 0f, TextColor = "orange", TabLayouting = TabLayouting.MultiRow)]
		[TabGroup("multi row", "Collection 1", false, 0f, TextColor = "blue")]
		[TabGroup("multi row", "Guide", SdfIconType.QuestionSquareFill, false, 0f, TextColor = "blue")]
		public float e;

		[TabGroup("multi row", "Guide", SdfIconType.QuestionSquareFill, false, 0f, TextColor = "blue")]
		[TabGroup("multi row", "Settings", SdfIconType.GearFill, false, 0f, TextColor = "grey")]
		[TabGroup("multi row", "World Map", SdfIconType.Map, false, 0f, TextColor = "orange", TabLayouting = TabLayouting.MultiRow)]
		[TabGroup("multi row", "Character", SdfIconType.PersonFill, false, 0f, TextColor = "orange")]
		[TabGroup("multi row", "Wand", SdfIconType.Magic, false, 0f, TextColor = "red")]
		[TabGroup("multi row", "Abilities", false, 0f, TextColor = "green")]
		[TabGroup("multi row", "Missions", SdfIconType.ExclamationSquareFill, false, 0f, TextColor = "yellow")]
		[TabGroup("multi row", "Collection 1", false, 0f, TextColor = "blue")]
		[TabGroup("multi row", "Collection 2", false, 0f, TextColor = "blue")]
		[TabGroup("multi row", "Collection 3", false, 0f, TextColor = "blue")]
		[TabGroup("multi row", "Collection 4", false, 0f, TextColor = "blue")]
		public float f;

		[TabGroup("multi row", "Collection 4", false, 0f, TextColor = "blue")]
		[TabGroup("multi row", "Collection 3", false, 0f, TextColor = "blue")]
		[TabGroup("multi row", "Collection 1", false, 0f, TextColor = "blue")]
		[TabGroup("multi row", "Missions", SdfIconType.ExclamationSquareFill, false, 0f, TextColor = "yellow")]
		[TabGroup("multi row", "Abilities", false, 0f, TextColor = "green")]
		[TabGroup("multi row", "Wand", SdfIconType.Magic, false, 0f, TextColor = "red")]
		[TabGroup("multi row", "Collection 2", false, 0f, TextColor = "blue")]
		[TabGroup("multi row", "World Map", SdfIconType.Map, false, 0f, TextColor = "orange", TabLayouting = TabLayouting.MultiRow)]
		[TabGroup("multi row", "Settings", SdfIconType.GearFill, false, 0f, TextColor = "grey")]
		[TabGroup("multi row", "Character", SdfIconType.PersonFill, false, 0f, TextColor = "orange")]
		[TabGroup("multi row", "Guide", SdfIconType.QuestionSquareFill, false, 0f, TextColor = "blue")]
		public float g;

		[TabGroup("multi row", "Settings", SdfIconType.GearFill, false, 0f, TextColor = "grey")]
		[TabGroup("multi row", "Collection 4", false, 0f, TextColor = "blue")]
		[TabGroup("multi row", "Collection 3", false, 0f, TextColor = "blue")]
		[TabGroup("multi row", "Guide", SdfIconType.QuestionSquareFill, false, 0f, TextColor = "blue")]
		[TabGroup("multi row", "Wand", SdfIconType.Magic, false, 0f, TextColor = "red")]
		[TabGroup("multi row", "Character", SdfIconType.PersonFill, false, 0f, TextColor = "orange")]
		[TabGroup("multi row", "World Map", SdfIconType.Map, false, 0f, TextColor = "orange", TabLayouting = TabLayouting.MultiRow)]
		[TabGroup("multi row", "Collection 2", false, 0f, TextColor = "blue")]
		[TabGroup("multi row", "Collection 1", false, 0f, TextColor = "blue")]
		[TabGroup("multi row", "Abilities", false, 0f, TextColor = "green")]
		[TabGroup("multi row", "Missions", SdfIconType.ExclamationSquareFill, false, 0f, TextColor = "yellow")]
		public float h;
	}
}
