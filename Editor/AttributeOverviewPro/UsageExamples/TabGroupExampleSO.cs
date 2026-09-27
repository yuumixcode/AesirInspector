using Sirenix.OdinInspector;
using UnityEngine;

namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// TabGroup 特性的案例 SO：基础用法（多标签分组）。
    /// </summary>
    [AesirExample]
    public class TabGroupExampleSO : AttributeExampleSO<TabGroupExampleSO>
    {
        [TabGroup("Tabs", "General")]
        public string playerName;

        [TabGroup("Tabs", "General")]
        public int playerLevel;

        [TabGroup("Tabs", "General")]
        public string playerClass;

        [TabGroup("Tabs", "Stats")]
        public int strength;

        [TabGroup("Tabs", "Stats")]
        public int dexterity;

        [TabGroup("Tabs", "Stats")]
        public int intelligence;

        [TabGroup("Tabs", "Quests")]
        public bool hasMainQuest;

        [TabGroup("Tabs", "Quests")]
        public int mainQuestProgress;

        [TabGroup("Tabs", "Quests")]
        public bool hasSideQuest;

        [TabGroup("Tabs", "Quests")]
        public int sideQuestProgress;

        public override void AesirInspectorReset()
        {
            playerName = null;
            playerLevel = 0;
            playerClass = null;
            strength = 0;
            dexterity = 0;
            intelligence = 0;
            hasMainQuest = false;
            mainQuestProgress = 0;
            hasSideQuest = false;
            sideQuestProgress = 0;
        }
    }
}
