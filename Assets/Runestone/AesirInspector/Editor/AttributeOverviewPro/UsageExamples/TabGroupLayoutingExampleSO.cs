using Sirenix.OdinInspector;

namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// TabGroup 特性的案例 SO：TabLayouting 参数（Shrink 收缩标签、MultiRow 多行标签）。
    /// </summary>
    [AesirExample]
    public class TabGroupLayoutingExampleSO : AttributeExampleSO<TabGroupLayoutingExampleSO>
    {
        [TabGroup("Shrink Tabs", "Collection 1", TextColor = "blue")]
        [TabGroup("Shrink Tabs", "Collection 2", TextColor = "blue")]
        [TabGroup("Shrink Tabs", "Settings", SdfIconType.GearFill, TextColor = "grey")]
        [TabGroup("Shrink Tabs", "Abilities", TextColor = "green")]
        [TabGroup("Shrink Tabs", "Wand", SdfIconType.Magic, TextColor = "red")]
        [TabGroup("Shrink Tabs", "Character", SdfIconType.PersonFill, TextColor = "orange")]
        [TabGroup("Shrink Tabs", "World Map", SdfIconType.Map, TextColor = "orange",
            TabLayouting = TabLayouting.Shrink)]
        [TabGroup("Shrink Tabs", "Collection 4", TextColor = "blue")]
        [TabGroup("Shrink Tabs", "Collection 3", TextColor = "blue")]
        [TabGroup("Shrink Tabs", "Missions", SdfIconType.ExclamationSquareFill, TextColor = "yellow")]
        [TabGroup("Shrink Tabs", "Guide", SdfIconType.QuestionSquareFill, TextColor = "blue")]
        public float shrinkFirst;

        [TabGroup("Shrink Tabs", "Abilities", TextColor = "green")]
        [TabGroup("Shrink Tabs", "Wand", SdfIconType.Magic, TextColor = "red")]
        [TabGroup("Shrink Tabs", "Guide", SdfIconType.QuestionSquareFill, TextColor = "blue")]
        [TabGroup("Shrink Tabs", "Settings", SdfIconType.GearFill, TextColor = "grey")]
        [TabGroup("Shrink Tabs", "Collection 4", TextColor = "blue")]
        [TabGroup("Shrink Tabs", "World Map", SdfIconType.Map, TextColor = "orange",
            TabLayouting = TabLayouting.Shrink)]
        [TabGroup("Shrink Tabs", "Collection 2", TextColor = "blue")]
        [TabGroup("Shrink Tabs", "Collection 1", TextColor = "blue")]
        [TabGroup("Shrink Tabs", "Missions", SdfIconType.ExclamationSquareFill, TextColor = "yellow")]
        [TabGroup("Shrink Tabs", "Collection 3", TextColor = "blue")]
        [TabGroup("Shrink Tabs", "Character", SdfIconType.PersonFill, TextColor = "orange")]
        public float shrinkSecond;

        [TabGroup("Multi Row Tabs", "Settings", SdfIconType.GearFill, TextColor = "grey")]
        [TabGroup("Multi Row Tabs", "Collection 4", TextColor = "blue")]
        [TabGroup("Multi Row Tabs", "Collection 3", TextColor = "blue")]
        [TabGroup("Multi Row Tabs", "Collection 2", TextColor = "blue")]
        [TabGroup("Multi Row Tabs", "Character", SdfIconType.PersonFill, TextColor = "orange")]
        [TabGroup("Multi Row Tabs", "Missions", SdfIconType.ExclamationSquareFill, TextColor = "yellow")]
        [TabGroup("Multi Row Tabs", "Abilities", TextColor = "green")]
        [TabGroup("Multi Row Tabs", "Wand", SdfIconType.Magic, TextColor = "red")]
        [TabGroup("Multi Row Tabs", "World Map", SdfIconType.Map, TextColor = "orange",
            TabLayouting = TabLayouting.MultiRow)]
        [TabGroup("Multi Row Tabs", "Collection 1", TextColor = "blue")]
        [TabGroup("Multi Row Tabs", "Guide", SdfIconType.QuestionSquareFill, TextColor = "blue")]
        public float multiRowFirst;

        [TabGroup("Multi Row Tabs", "Guide", SdfIconType.QuestionSquareFill, TextColor = "blue")]
        [TabGroup("Multi Row Tabs", "Settings", SdfIconType.GearFill, TextColor = "grey")]
        [TabGroup("Multi Row Tabs", "World Map", SdfIconType.Map, TextColor = "orange",
            TabLayouting = TabLayouting.MultiRow)]
        [TabGroup("Multi Row Tabs", "Character", SdfIconType.PersonFill, TextColor = "orange")]
        [TabGroup("Multi Row Tabs", "Wand", SdfIconType.Magic, TextColor = "red")]
        [TabGroup("Multi Row Tabs", "Abilities", TextColor = "green")]
        [TabGroup("Multi Row Tabs", "Missions", SdfIconType.ExclamationSquareFill, TextColor = "yellow")]
        [TabGroup("Multi Row Tabs", "Collection 1", TextColor = "blue")]
        [TabGroup("Multi Row Tabs", "Collection 2", TextColor = "blue")]
        [TabGroup("Multi Row Tabs", "Collection 3", TextColor = "blue")]
        [TabGroup("Multi Row Tabs", "Collection 4", TextColor = "blue")]
        public float multiRowSecond;

        public override void AesirInspectorReset()
        {
            shrinkFirst = 0f;
            shrinkSecond = 0f;
            multiRowFirst = 0f;
            multiRowSecond = 0f;
        }
    }
}
