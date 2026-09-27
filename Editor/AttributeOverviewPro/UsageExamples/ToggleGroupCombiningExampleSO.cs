using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// ToggleGroup 特性的案例 SO：与 Toggle 属性、集合、类继承组合。
    /// </summary>
    [AesirExample]
    public class ToggleGroupCombiningExampleSO : AttributeExampleSO<ToggleGroupCombiningExampleSO>
    {
        [Toggle("Enabled")]
        public MyToggleObject Three = new MyToggleObject();

        [Toggle("Enabled")]
        public MyToggleA Four = new MyToggleA();

        [Toggle("Enabled")]
        public MyToggleB Five = new MyToggleB();

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
