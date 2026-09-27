using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// TypeInfoBox 特性的案例 SO。
    /// </summary>
    [AesirExample]
    public class TypeInfoBoxExampleSO : AttributeExampleSO<TypeInfoBoxExampleSO>
    {
        [Title("On Serializable Class")]
        public MyType MyObject = new MyType();

        [Title("On ScriptableObject Type")]
        [InfoBox("Click the pen icon to open a new inspector for the Scripty object.")]
        [InlineEditor()]
        public MyScriptyType Scripty;

        [OnInspectorInit]
        void CreateData()
        {
            Scripty = CreateInstance<MyScriptyType>();
            Scripty.name = "Scripty";
        }

        [OnInspectorDispose]
        void CleanupData()
        {
            if (Scripty != null)
            {
                DestroyImmediate(Scripty);
            }
        }

        public override void AesirInspectorReset()
        {
            MyObject = new MyType();
            Scripty = null;
        }

        [Serializable]
        [TypeInfoBox(
            "The TypeInfoBox attribute can be put on type definitions and will result in an InfoBox being drawn at the top of a property.")]
        public class MyType
        {
            public int Value;
        }

        [TypeInfoBox(
            "The TypeInfoBox attribute can also be used to display a text at the top of, for example, MonoBehaviours or ScriptableObjects.")]
        public class MyScriptyType : ScriptableObject
        {
            public string MyText = "Hello, I am a Scripty object.";

            [TextArea(10, 15)]
            public string Box;
        }
    }
}
