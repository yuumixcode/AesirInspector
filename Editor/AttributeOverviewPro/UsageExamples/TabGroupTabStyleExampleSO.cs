using Sirenix.OdinInspector;
using UnityEngine;

namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// TabGroup 特性的案例 SO：SdfIcon、TextColor、TabName 参数。
    /// </summary>
    [AesirExample]
    public class TabGroupTabStyleExampleSO : AttributeExampleSO<TabGroupTabStyleExampleSO>
    {
        [TabGroup("Icon Tabs", "Player", SdfIconType.PersonFill)]
        public string playerName;

        [TabGroup("Icon Tabs", "Inventory", SdfIconType.BriefcaseFill)]
        public int inventorySize;

        [TabGroup("Styled Tabs", "General", SdfIconType.ImageAlt, TextColor = "green")]
        public string generalName;

        [TabGroup("Styled Tabs", "General")]
        public int generalLevel;

        [TabGroup("Styled Tabs", "Stats", SdfIconType.BarChartLineFill, TextColor = "blue")]
        public int strength;

        [TabGroup("Styled Tabs", "Stats")]
        public int dexterity;

        [TabGroup("Styled Tabs", "Quests", SdfIconType.Question, TextColor = "@questColor", TabName = "")]
        public bool hasMainQuest;

        [TabGroup("Styled Tabs", "Quests")]
        public Color questColor = new Color(1f, 0.5f, 0f);

        public override void AesirInspectorReset()
        {
            playerName = null;
            inventorySize = 0;
            generalName = null;
            generalLevel = 0;
            strength = 0;
            dexterity = 0;
            hasMainQuest = false;
            questColor = new Color(1f, 0.5f, 0f);
        }
    }
}
