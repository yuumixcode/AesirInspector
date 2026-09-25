using Sirenix.OdinInspector;
using UnityEngine;

namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// TabGroup 特性的案例 SO。
    /// </summary>
    [AesirExample]
    public class TabGroupExampleSO : AttributeExampleSO<TabGroupExampleSO>
    {
        [FoldoutGroup("No Parameters")]
        [TabGroup("No Parameters/Tabs", "General")]
        public string playerName1;

        [FoldoutGroup("No Parameters")]
        [TabGroup("No Parameters/Tabs", "General")]
        public int playerLevel1;

        [FoldoutGroup("No Parameters")]
        [TabGroup("No Parameters/Tabs", "General")]
        public string playerClass1;

        [FoldoutGroup("No Parameters")]
        [TabGroup("No Parameters/Tabs", "Stats")]
        public int strength1;

        [FoldoutGroup("No Parameters")]
        [TabGroup("No Parameters/Tabs", "Stats")]
        public int dexterity1;

        [FoldoutGroup("No Parameters")]
        [TabGroup("No Parameters/Tabs", "Stats")]
        public int intelligence1;

        [FoldoutGroup("No Parameters")]
        [TabGroup("No Parameters/Tabs", "Quests")]
        public bool hasMainQuest1;

        [FoldoutGroup("No Parameters")]
        [TabGroup("No Parameters/Tabs", "Quests")]
        public int mainQuestProgress1;

        [FoldoutGroup("No Parameters")]
        [TabGroup("No Parameters/Tabs", "Quests")]
        public bool hasSideQuest1;

        [FoldoutGroup("No Parameters")]
        [TabGroup("No Parameters/Tabs", "Quests")]
        public int sideQuestProgress1;

        [FoldoutGroup("Parameter: UseFixedHeight")]
        [TabGroup("Parameter: UseFixedHeight/Adaptive", "Short")]
        public int shortTab;

        [FoldoutGroup("Parameter: UseFixedHeight")]
        [TabGroup("Parameter: UseFixedHeight/Adaptive", "Tall")]
        [DisplayAsString]
        public string tallTab = "\n\n\n\nTall Content";

        [FoldoutGroup("Parameter: UseFixedHeight")]
        [TabGroup("Parameter: UseFixedHeight/Fixed", "Short", UseFixedHeight = true)]
        public int fixedShortTab;

        [FoldoutGroup("Parameter: UseFixedHeight")]
        [TabGroup("Parameter: UseFixedHeight/Fixed", "Tall", UseFixedHeight = true)]
        [DisplayAsString]
        public string fixedTallTab = "\n\n\n\nTall Content";

        [FoldoutGroup("Parameter: SdfIcon")]
        [TabGroup("Parameter: SdfIcon/Tabs", "Player", SdfIconType.PersonFill)]
        public string playerName;

        [FoldoutGroup("Parameter: SdfIcon")]
        [TabGroup("Parameter: SdfIcon/Tabs", "Inventory", SdfIconType.BriefcaseFill)]
        public int inventorySize;

        [FoldoutGroup("Parameter: TextColor, TabName")]
        [TabGroup("Parameter: TextColor, TabName/Tabs", "General", SdfIconType.ImageAlt, TextColor = "green")]
        public string playerName2;

        [FoldoutGroup("Parameter: TextColor, TabName")]
        [TabGroup("Parameter: TextColor, TabName/Tabs", "General")]
        public int playerLevel2;

        [FoldoutGroup("Parameter: TextColor, TabName")]
        [TabGroup("Parameter: TextColor, TabName/Tabs", "General")]
        public string playerClass2;

        [FoldoutGroup("Parameter: TextColor, TabName")]
        [TabGroup("Parameter: TextColor, TabName/Tabs", "Stats", SdfIconType.BarChartLineFill,
            TextColor = "blue")]
        public int strength2;

        [FoldoutGroup("Parameter: TextColor, TabName")]
        [TabGroup("Parameter: TextColor, TabName/Tabs", "Stats")]
        public int dexterity2;

        [FoldoutGroup("Parameter: TextColor, TabName")]
        [TabGroup("Parameter: TextColor, TabName/Tabs", "Stats")]
        public int intelligence2;

        [FoldoutGroup("Parameter: TextColor, TabName")]
        [TabGroup("Parameter: TextColor, TabName/Tabs", "Quests", SdfIconType.Question,
            TextColor = "@questColor", TabName = "")]
        public bool hasMainQuest2;

        [FoldoutGroup("Parameter: TextColor, TabName")]
        [TabGroup("Parameter: TextColor, TabName/Tabs", "Quests")]
        public Color questColor = new Color(1f, 0.5f, 0f);

        [FoldoutGroup("Parameter: TabLayouting")]
        [TabGroup("Parameter: TabLayouting/Shrink Tabs", "Collection 1", TextColor = "blue")]
        [TabGroup("Parameter: TabLayouting/Shrink Tabs", "Collection 2", TextColor = "blue")]
        [TabGroup("Parameter: TabLayouting/Shrink Tabs", "Settings", SdfIconType.GearFill,
            TextColor = "grey")]
        [TabGroup("Parameter: TabLayouting/Shrink Tabs", "Abilities", TextColor = "green")]
        [TabGroup("Parameter: TabLayouting/Shrink Tabs", "Wand", SdfIconType.Magic, TextColor = "red")]
        [TabGroup("Parameter: TabLayouting/Shrink Tabs", "Character", SdfIconType.PersonFill,
            TextColor = "orange")]
        [TabGroup("Parameter: TabLayouting/Shrink Tabs", "World Map", SdfIconType.Map, TextColor = "orange",
            TabLayouting = TabLayouting.Shrink)]
        [TabGroup("Parameter: TabLayouting/Shrink Tabs", "Collection 4", TextColor = "blue")]
        [TabGroup("Parameter: TabLayouting/Shrink Tabs", "Collection 3", TextColor = "blue")]
        [TabGroup("Parameter: TabLayouting/Shrink Tabs", "Missions", SdfIconType.ExclamationSquareFill,
            TextColor = "yellow")]
        [TabGroup("Parameter: TabLayouting/Shrink Tabs", "Guide", SdfIconType.QuestionSquareFill,
            TextColor = "blue")]
        public float a;

        [FoldoutGroup("Parameter: TabLayouting")]
        [TabGroup("Parameter: TabLayouting/Shrink Tabs", "Abilities", TextColor = "green")]
        [TabGroup("Parameter: TabLayouting/Shrink Tabs", "Wand", SdfIconType.Magic, TextColor = "red")]
        [TabGroup("Parameter: TabLayouting/Shrink Tabs", "Guide", SdfIconType.QuestionSquareFill,
            TextColor = "blue")]
        [TabGroup("Parameter: TabLayouting/Shrink Tabs", "Settings", SdfIconType.GearFill,
            TextColor = "grey")]
        [TabGroup("Parameter: TabLayouting/Shrink Tabs", "Collection 4", TextColor = "blue")]
        [TabGroup("Parameter: TabLayouting/Shrink Tabs", "World Map", SdfIconType.Map, TextColor = "orange",
            TabLayouting = TabLayouting.Shrink)]
        [TabGroup("Parameter: TabLayouting/Shrink Tabs", "Collection 2", TextColor = "blue")]
        [TabGroup("Parameter: TabLayouting/Shrink Tabs", "Collection 1", TextColor = "blue")]
        [TabGroup("Parameter: TabLayouting/Shrink Tabs", "Missions", SdfIconType.ExclamationSquareFill,
            TextColor = "yellow")]
        [TabGroup("Parameter: TabLayouting/Shrink Tabs", "Collection 3", TextColor = "blue")]
        [TabGroup("Parameter: TabLayouting/Shrink Tabs", "Character", SdfIconType.PersonFill,
            TextColor = "orange")]
        public float b;

        [FoldoutGroup("Parameter: TabLayouting")]
        [TabGroup("Parameter: TabLayouting/Multi Row Tabs", "Settings", SdfIconType.GearFill,
            TextColor = "grey")]
        [TabGroup("Parameter: TabLayouting/Multi Row Tabs", "Collection 4", TextColor = "blue")]
        [TabGroup("Parameter: TabLayouting/Multi Row Tabs", "Collection 3", TextColor = "blue")]
        [TabGroup("Parameter: TabLayouting/Multi Row Tabs", "Collection 2", TextColor = "blue")]
        [TabGroup("Parameter: TabLayouting/Multi Row Tabs", "Character", SdfIconType.PersonFill,
            TextColor = "orange")]
        [TabGroup("Parameter: TabLayouting/Multi Row Tabs", "Missions", SdfIconType.ExclamationSquareFill,
            TextColor = "yellow")]
        [TabGroup("Parameter: TabLayouting/Multi Row Tabs", "Abilities", TextColor = "green")]
        [TabGroup("Parameter: TabLayouting/Multi Row Tabs", "Wand", SdfIconType.Magic, TextColor = "red")]
        [TabGroup("Parameter: TabLayouting/Multi Row Tabs", "World Map", SdfIconType.Map,
            TextColor = "orange", TabLayouting = TabLayouting.MultiRow)]
        [TabGroup("Parameter: TabLayouting/Multi Row Tabs", "Collection 1", TextColor = "blue")]
        [TabGroup("Parameter: TabLayouting/Multi Row Tabs", "Guide", SdfIconType.QuestionSquareFill,
            TextColor = "blue")]
        public float e;

        [FoldoutGroup("Parameter: TabLayouting")]
        [TabGroup("Parameter: TabLayouting/Multi Row Tabs", "Guide", SdfIconType.QuestionSquareFill,
            TextColor = "blue")]
        [TabGroup("Parameter: TabLayouting/Multi Row Tabs", "Settings", SdfIconType.GearFill,
            TextColor = "grey")]
        [TabGroup("Parameter: TabLayouting/Multi Row Tabs", "World Map", SdfIconType.Map,
            TextColor = "orange", TabLayouting = TabLayouting.MultiRow)]
        [TabGroup("Parameter: TabLayouting/Multi Row Tabs", "Character", SdfIconType.PersonFill,
            TextColor = "orange")]
        [TabGroup("Parameter: TabLayouting/Multi Row Tabs", "Wand", SdfIconType.Magic, TextColor = "red")]
        [TabGroup("Parameter: TabLayouting/Multi Row Tabs", "Abilities", TextColor = "green")]
        [TabGroup("Parameter: TabLayouting/Multi Row Tabs", "Missions", SdfIconType.ExclamationSquareFill,
            TextColor = "yellow")]
        [TabGroup("Parameter: TabLayouting/Multi Row Tabs", "Collection 1", TextColor = "blue")]
        [TabGroup("Parameter: TabLayouting/Multi Row Tabs", "Collection 2", TextColor = "blue")]
        [TabGroup("Parameter: TabLayouting/Multi Row Tabs", "Collection 3", TextColor = "blue")]
        [TabGroup("Parameter: TabLayouting/Multi Row Tabs", "Collection 4", TextColor = "blue")]
        public float f;

        [FoldoutGroup("Combining With Other Attributes")]
        [TitleGroup("Combining With Other Attributes/Tabs")]
        [HorizontalGroup("Combining With Other Attributes/Tabs/Split", 0.5f)]
        [TabGroup("Combining With Other Attributes/Tabs/Split/Parameters", "A")]
        public string NameA;

        [FoldoutGroup("Combining With Other Attributes")]
        [TitleGroup("Combining With Other Attributes/Tabs")]
        [HorizontalGroup("Combining With Other Attributes/Tabs/Split", 0.5f)]
        [TabGroup("Combining With Other Attributes/Tabs/Split/Parameters", "A")]
        public string NameB;

        [FoldoutGroup("Combining With Other Attributes")]
        [TitleGroup("Combining With Other Attributes/Tabs")]
        [HorizontalGroup("Combining With Other Attributes/Tabs/Split", 0.5f)]
        [TabGroup("Combining With Other Attributes/Tabs/Split/Parameters", "A")]
        public string NameC;

        [FoldoutGroup("Combining With Other Attributes")]
        [TabGroup("Combining With Other Attributes/Tabs/Split/Parameters", "B")]
        public int ValueA;

        [FoldoutGroup("Combining With Other Attributes")]
        [TabGroup("Combining With Other Attributes/Tabs/Split/Parameters", "B")]
        public int ValueB;

        [FoldoutGroup("Combining With Other Attributes")]
        [TabGroup("Combining With Other Attributes/Tabs/Split/Parameters", "B")]
        public int ValueC;

        [FoldoutGroup("Combining With Other Attributes")]
        [TabGroup("Combining With Other Attributes/Tabs/Split/Buttons", "Responsive")]
        [ResponsiveButtonGroup(
            "Combining With Other Attributes/Tabs/Split/Buttons/Responsive/ResponsiveButtons")]
        public void Hello() { }

        [FoldoutGroup("Combining With Other Attributes")]
        [ResponsiveButtonGroup(
            "Combining With Other Attributes/Tabs/Split/Buttons/Responsive/ResponsiveButtons")]
        public void World() { }

        [FoldoutGroup("Combining With Other Attributes")]
        [ResponsiveButtonGroup(
            "Combining With Other Attributes/Tabs/Split/Buttons/Responsive/ResponsiveButtons")]
        public void And() { }

        [FoldoutGroup("Combining With Other Attributes")]
        [ResponsiveButtonGroup(
            "Combining With Other Attributes/Tabs/Split/Buttons/Responsive/ResponsiveButtons")]
        public void Such() { }

        [FoldoutGroup("Combining With Other Attributes")]
        [TabGroup("Combining With Other Attributes/Tabs/Split/Buttons", "More Tabs")]
        [TabGroup("Combining With Other Attributes/Tabs/Split/Buttons/More Tabs/SubTabGroup", "A")]
        [Button]
        public void SubButtonA() { }

        [FoldoutGroup("Combining With Other Attributes")]
        [TabGroup("Combining With Other Attributes/Tabs/Split/Buttons/More Tabs/SubTabGroup", "A")]
        [Button]
        public void SubButtonB() { }

        [FoldoutGroup("Combining With Other Attributes")]
        [TabGroup("Combining With Other Attributes/Tabs/Split/Buttons/More Tabs/SubTabGroup", "B")]
        [Button(ButtonSizes.Gigantic)]
        public void SubButtonC() { }

        public override void AesirInspectorReset()
        {
            playerName1 = null;
            playerLevel1 = 0;
            playerClass1 = null;
            strength1 = 0;
            dexterity1 = 0;
            intelligence1 = 0;
            hasMainQuest1 = false;
            mainQuestProgress1 = 0;
            hasSideQuest1 = false;
            sideQuestProgress1 = 0;
            shortTab = 0;
            tallTab = "\n\n\n\nTall Content";
            fixedShortTab = 0;
            fixedTallTab = "\n\n\n\nTall Content";
            playerName = null;
            inventorySize = 0;
            playerName2 = null;
            playerLevel2 = 0;
            playerClass2 = null;
            strength2 = 0;
            dexterity2 = 0;
            intelligence2 = 0;
            hasMainQuest2 = false;
            questColor = new Color(1f, 0.5f, 0f);
            a = 0f;
            b = 0f;
            e = 0f;
            f = 0f;
            NameA = null;
            NameB = null;
            NameC = null;
            ValueA = 0;
            ValueB = 0;
            ValueC = 0;
        }
    }
}
