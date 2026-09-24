using System;
using Sirenix.Serialization;
using Sirenix.Utilities;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Examples
{
	[AttributeExample(typeof(PolymorphicDrawerSettingsAttribute))]
	internal class PolymorphicDrawerSettingsExample
	{
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

		public interface IDemo<T>
		{
			T Value { get; set; }
		}

		[Serializable]
		public class DemoSOInt32 : SerializedScriptableObject, IDemo<int>
		{
			[OdinSerialize]
			public int Value { get; set; }
		}

		[Serializable]
		public class DemoSOInt32Target : SerializedScriptableObject, IDemo<int>
		{
			public int target;

			[OdinSerialize]
			public int Value { get; set; }
		}

		[Serializable]
		public class DemoSOFloat32 : SerializedScriptableObject, IDemo<float>
		{
			[OdinSerialize]
			public float Value { get; set; }
		}

		[Serializable]
		public class Demo<T> : IDemo<T>
		{
			[OdinSerialize]
			public T Value { get; set; }
		}

		[Serializable]
		public class DemoInt32Interface : IDemo<int>
		{
			[OdinSerialize]
			public int Value { get; set; }
		}

		public class DemoInt32 : Demo<int>
		{
		}

		public struct DemoStructInt32 : IDemo<int>
		{
			[OdinSerialize]
			public int Value { get; set; }
		}

		[ShowInInspector]
		public IDemo<int> Default;

		[ShowInInspector]
		[LabelText("On")]
		[PolymorphicDrawerSettings(ShowBaseType = true)]
		[Title("Show Base Type", null, TitleAlignments.Left, true, true)]
		public IDemo<int> ShowBaseType_On;

		[ShowInInspector]
		[LabelText("Off")]
		[PolymorphicDrawerSettings(ShowBaseType = false)]
		public IDemo<int> ShowBaseType_Off;

		[Title("Read Only If Not Null Reference", null, TitleAlignments.Left, true, true)]
		[ShowInInspector]
		[LabelText("On")]
		[PolymorphicDrawerSettings(ReadOnlyIfNotNullReference = true)]
		public IDemo<int> ReadOnlyIfNotNullReference_On;

		[ShowInInspector]
		[PolymorphicDrawerSettings(ReadOnlyIfNotNullReference = false)]
		[LabelText("Off")]
		public IDemo<int> ReadOnlyIfNotNullReference_Off;

		[LabelText("Exclude")]
		[Title("Non Default Constructor Preference", null, TitleAlignments.Left, true, true)]
		[ShowInInspector]
		[PolymorphicDrawerSettings(NonDefaultConstructorPreference = NonDefaultConstructorPreference.Exclude)]
		public IVector2<int> NonDefaultConstructorPreference_Ignore;

		[PolymorphicDrawerSettings(NonDefaultConstructorPreference = NonDefaultConstructorPreference.ConstructIdeal)]
		[LabelText("Construct Ideal")]
		[ShowInInspector]
		public IVector2<int> NonDefaultConstructorPreference_ConstructIdeal;

		[PolymorphicDrawerSettings(NonDefaultConstructorPreference = NonDefaultConstructorPreference.PreferUninitialized)]
		[LabelText("Prefer Uninitialized")]
		[ShowInInspector]
		public IVector2<int> NonDefaultConstructorPreference_PreferUninit;

		[LabelText("Log Warning")]
		[PolymorphicDrawerSettings(NonDefaultConstructorPreference = NonDefaultConstructorPreference.LogWarning)]
		[ShowInInspector]
		public IVector2<int> NonDefaultConstructorPreference_LogWarning;

		[PolymorphicDrawerSettings(CreateInstanceFunction = "CreateInstance")]
		[ShowInInspector]
		[Title("Create Custom Instance", null, TitleAlignments.Left, true, true)]
		public IVector2<int> CreateCustomInstance;

		private IVector2<int> CreateInstance(Type type)
		{
			Debug.Log("Constructor called for " + type?.ToString() + ".");
			if (typeof(SomeNonDefaultCtorClass) == type)
			{
				return new SomeNonDefaultCtorClass(485);
			}
			return type.InstantiateDefault(preferUninitializedOverNonDefault: false) as IVector2<int>;
		}
	}
}
