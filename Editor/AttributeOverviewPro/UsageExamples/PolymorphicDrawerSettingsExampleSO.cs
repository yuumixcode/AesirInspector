using System;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using Sirenix.Utilities;
using UnityEngine;

namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// PolymorphicDrawerSettings 特性的案例 SO。
    /// </summary>
    [AesirExample]
    public class PolymorphicDrawerSettingsExampleSO : OdinAttributeExampleSO<PolymorphicDrawerSettingsExampleSO>
    {
        [Title("No Parameters")]
        public IDemo<int> Default;

        [Title("Parameter: ShowBaseType")]
        [LabelText("On")]
        [PolymorphicDrawerSettings(ShowBaseType = true)]
        public IDemo<int> ShowBaseTypeOn;

        [Title("Parameter: ShowBaseType")]
        [LabelText("Off")]
        [PolymorphicDrawerSettings(ShowBaseType = false)]
        public IDemo<int> ShowBaseTypeOff;

        [Title("Parameter: ReadOnlyIfNotNullReference")]
        [LabelText("On")]
        [PolymorphicDrawerSettings(ReadOnlyIfNotNullReference = true)]
        public IDemo<int> ReadOnlyIfNotNullReferenceOn;

        [Title("Parameter: ReadOnlyIfNotNullReference")]
        [LabelText("Off")]
        [PolymorphicDrawerSettings(ReadOnlyIfNotNullReference = false)]
        public IDemo<int> ReadOnlyIfNotNullReferenceOff;

        [Title("Parameter: NonDefaultConstructorPreference")]
        [LabelText("Exclude")]
        [PolymorphicDrawerSettings(NonDefaultConstructorPreference = NonDefaultConstructorPreference.Exclude)]
        public IVector2<int> NonDefaultConstructorPreferenceExclude;

        [Title("Parameter: NonDefaultConstructorPreference")]
        [LabelText("Construct Ideal")]
        [PolymorphicDrawerSettings(NonDefaultConstructorPreference = NonDefaultConstructorPreference.ConstructIdeal)]
        public IVector2<int> NonDefaultConstructorPreferenceConstructIdeal;

        [Title("Parameter: NonDefaultConstructorPreference")]
        [LabelText("Prefer Uninitialized")]
        [PolymorphicDrawerSettings(NonDefaultConstructorPreference = NonDefaultConstructorPreference.PreferUninitialized)]
        public IVector2<int> NonDefaultConstructorPreferencePreferUninitialized;

        [Title("Parameter: NonDefaultConstructorPreference")]
        [LabelText("Log Warning")]
        [PolymorphicDrawerSettings(NonDefaultConstructorPreference = NonDefaultConstructorPreference.LogWarning)]
        public IVector2<int> NonDefaultConstructorPreferenceLogWarning;

        [Title("Parameter: CreateInstanceFunction (Type type)")]
        [PolymorphicDrawerSettings(CreateInstanceFunction = "CreatePolymorphicInstance")]
        public IVector2<int> CreateCustomInstance;

        IVector2<int> CreatePolymorphicInstance(Type type)
        {
            Debug.Log("Constructor called for " + type?.ToString() + ".");
            if (typeof(SomeNonDefaultCtorClass) == type)
            {
                return new SomeNonDefaultCtorClass(485);
            }

            return type.InstantiateDefault(preferUninitializedOverNonDefault: false) as IVector2<int>;
        }

        public interface IVector2<T>
        {
            T X { get; set; }

            T Y { get; set; }
        }

        [Serializable]
        public class SomeNonDefaultCtorClass : IVector2<int>
        {
            [OdinSerialize]
            public int X { get; set; }

            [OdinSerialize]
            public int Y { get; set; }

            public SomeNonDefaultCtorClass(int x)
            {
                X = x;
                Y = (x + 1) * 4;
            }
        }

        [Serializable]
        public class DemoInt32 : IDemo<int>
        {
            [OdinSerialize]
            public int Value { get; set; }
        }

        [Serializable]
        public class DemoString : IDemo<int>
        {
            public string ExtraInfo;

            [OdinSerialize]
            public int Value { get; set; }
        }

        public struct DemoStructInt32 : IDemo<int>
        {
            [OdinSerialize]
            public int Value { get; set; }
        }

        public override void AesirInspectorReset()
        {
            Default = null;
            ShowBaseTypeOn = null;
            ShowBaseTypeOff = null;
            ReadOnlyIfNotNullReferenceOn = null;
            ReadOnlyIfNotNullReferenceOff = null;
            NonDefaultConstructorPreferenceExclude = null;
            NonDefaultConstructorPreferenceConstructIdeal = null;
            NonDefaultConstructorPreferencePreferUninitialized = null;
            NonDefaultConstructorPreferenceLogWarning = null;
            CreateCustomInstance = null;
        }
    }

    public interface IDemo<T>
    {
        T Value { get; set; }
    }
}
