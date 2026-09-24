using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Sirenix.OdinInspector.Editor.Examples
{
	[AttributeExample(typeof(TypeDrawerSettingsAttribute))]
	internal class TypeDrawerSettingsAttributeExample
	{
		public interface IBaseGeneric<T>
		{
		}

		public interface IBase : IBaseGeneric<int>
		{
		}

		public abstract class Base : IBase, IBaseGeneric<int>
		{
		}

		public class Concrete : Base
		{
		}

		public class ConcreteGeneric<T> : Base
		{
		}

		public abstract class BaseGeneric<T> : IBase, IBaseGeneric<int>
		{
		}

		[CompilerGenerated]
		public class ConcreteGenerated : Base
		{
		}

		[ShowInInspector]
		public Type Default;

		[TypeDrawerSettings(BaseType = typeof(IEnumerable<>))]
		[Title("Base Type", null, TitleAlignments.Left, true, true)]
		[ShowInInspector]
		[LabelText("Set")]
		public Type BaseType_Set;

		[ShowInInspector]
		[LabelText("Not Set")]
		[TypeDrawerSettings(BaseType = null)]
		public Type BaseType_NotSet;

		[LabelText("Concrete Types")]
		[Title("Filter", null, TitleAlignments.Left, true, true)]
		[ShowInInspector]
		[TypeDrawerSettings(BaseType = typeof(IBaseGeneric<>), Filter = TypeInclusionFilter.IncludeConcreteTypes)]
		public Type Filter_Default;

		[TypeDrawerSettings(BaseType = typeof(IBaseGeneric<>), Filter = (TypeInclusionFilter.IncludeConcreteTypes | TypeInclusionFilter.IncludeGenerics))]
		[LabelText("Concrete- && Generic Types")]
		[ShowInInspector]
		public Type Filter_Generics;

		[LabelText("Concrete- && Interface Types")]
		[ShowInInspector]
		[TypeDrawerSettings(BaseType = typeof(IBaseGeneric<>), Filter = (TypeInclusionFilter.IncludeConcreteTypes | TypeInclusionFilter.IncludeInterfaces))]
		public Type Filter_Interfaces;

		[TypeDrawerSettings(BaseType = typeof(IBaseGeneric<>), Filter = (TypeInclusionFilter.IncludeConcreteTypes | TypeInclusionFilter.IncludeAbstracts))]
		[LabelText("Concrete- && Abstract Types")]
		[ShowInInspector]
		public Type Filter_Abstracts;

		[TypeDrawerSettings(BaseType = typeof(IBaseGeneric<>), Filter = (TypeInclusionFilter.IncludeConcreteTypes | TypeInclusionFilter.IncludeGenerics | TypeInclusionFilter.IncludeAbstracts))]
		[LabelText("Concrete-, Abstract- && Generic Types")]
		[ShowInInspector]
		public Type Filter_Abstracts_Generics;

		[LabelText("Concrete-, Interface- && Generic Types")]
		[ShowInInspector]
		[TypeDrawerSettings(BaseType = typeof(IBaseGeneric<>), Filter = (TypeInclusionFilter.IncludeConcreteTypes | TypeInclusionFilter.IncludeGenerics | TypeInclusionFilter.IncludeInterfaces))]
		public Type Filter_Interfaces_Generics;

		[ShowInInspector]
		[LabelText("Concrete-, Interface- && Abstract Types")]
		[TypeDrawerSettings(BaseType = typeof(IBaseGeneric<>), Filter = (TypeInclusionFilter.IncludeConcreteTypes | TypeInclusionFilter.IncludeAbstracts | TypeInclusionFilter.IncludeInterfaces))]
		public Type Filter_Interfaces_Abstracts;

		[ShowInInspector]
		[LabelText("All")]
		[TypeDrawerSettings(BaseType = typeof(IBaseGeneric<>), Filter = TypeInclusionFilter.IncludeAll)]
		public Type Filter_All;
	}
}
