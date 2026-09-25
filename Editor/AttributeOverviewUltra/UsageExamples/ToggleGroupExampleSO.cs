using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// ToggleGroup 特性的案例 SO。
    /// </summary>
    [AesirExample]
    public class ToggleGroupExampleSO : AttributeExampleSO<ToggleGroupExampleSO>
    {
        [Title("No Parameters")]
        [ToggleGroup("MyToggle")]
        public bool MyToggle;

        [ToggleGroup("MyToggle")]
        public float A;

        [HideLabel]
        [ToggleGroup("MyToggle")]
        [Multiline]
        public string B;

        [Title("Member Reference ($)")]
        [ToggleGroup("EnableGroupOne", "$GroupOneTitle")]
        public bool EnableGroupOne = true;

        [ToggleGroup("EnableGroupOne")]
        public string GroupOneTitle = "One";

        [ToggleGroup("EnableGroupOne")]
        public float GroupOneA;

        [ToggleGroup("EnableGroupOne")]
        public float GroupOneB;

        [Title("Parameter: ToggleGroupTitle")]
        [ToggleGroup(nameof(Toggle2), "Custom Title")]
        public bool Toggle2;

        [ToggleGroup(nameof(Toggle2))]
        public int field3;

        [Title("Parameter: ToggleGroupTitle")]
        [ToggleGroup(nameof(Toggle4), "Toggle 4")]
        public bool Toggle4;

        [ToggleGroup(nameof(Toggle4))]
        public int field5;

        [Title("Parameter: Order")]
        [ToggleGroup(nameof(Toggle3), 10)]
        public bool Toggle3;

        [ToggleGroup(nameof(Toggle3))]
        public int field4;

        [Title("Parameter: CollapseOthersOnExpand")]
        [ToggleGroup(nameof(Toggle5), CollapseOthersOnExpand = true)]
        public bool Toggle5;

        [ToggleGroup(nameof(Toggle5))]
        public int field6;

        [Title("Combining With Other Attributes")]
        [Toggle("Enabled")]
        public MyToggleObject Three = new MyToggleObject();

        [Title("Combining With Other Attributes")]
        [Toggle("Enabled")]
        public MyToggleA Four = new MyToggleA();

        [Title("Combining With Other Attributes")]
        [Toggle("Enabled")]
        public MyToggleB Five = new MyToggleB();

        [Title("Combining With Other Attributes")]
        public MyToggleC[] ToggleList = new MyToggleC[3]
        {
            new MyToggleC
            {
                Test = 2f,
                Enabled = true
            },
            new MyToggleC
            {
                Test = 5f
            },
            new MyToggleC
            {
                Test = 7f
            }
        };

        public override void AesirInspectorReset()
        {
            MyToggle = false;
            A = 0f;
            B = null;
            EnableGroupOne = true;
            GroupOneTitle = "One";
            GroupOneA = 0f;
            GroupOneB = 0f;
            Toggle2 = false;
            field3 = 0;
            Toggle4 = false;
            field5 = 0;
            Toggle3 = false;
            field4 = 0;
            Toggle5 = false;
            field6 = 0;
            Three = new MyToggleObject();
            Four = new MyToggleA();
            Five = new MyToggleB();
            ToggleList = new MyToggleC[3]
            {
                new MyToggleC
                {
                    Test = 2f,
                    Enabled = true
                },
                new MyToggleC
                {
                    Test = 5f
                },
                new MyToggleC
                {
                    Test = 7f
                }
            };
        }

        [Serializable]
        public class MyToggleObject
        {
            public bool Enabled;

            [HideInInspector]
            public string Title;

            public int A;

            public int B;
        }

        [Serializable]
        public class MyToggleA : MyToggleObject
        {
            public float C;

            public float D;

            public float F;
        }

        [Serializable]
        public class MyToggleB : MyToggleObject
        {
            public string Text;
        }

        [Serializable]
        public class MyToggleC
        {
            [ToggleGroup("Enabled", "$Label")]
            public bool Enabled;

            [ToggleGroup("Enabled")]
            public float Test;

            public string Label => Test.ToString();
        }
    }
}
