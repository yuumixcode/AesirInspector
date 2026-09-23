using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Sirenix.OdinInspector;

namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// TypeDrawerSettings 特性的案例 SO。
    /// </summary>
    [AesirExample]
    public class TypeDrawerSettingsExampleSO : AttributeExampleSO<TypeDrawerSettingsExampleSO>
    {
        [Title("No Parameters")]
        [ShowInInspector]
        public Type Default;

        [Title("Parameter: BaseType")]
        [TypeDrawerSettings(BaseType = typeof(IEnumerable<>))]
        [ShowInInspector]
        [LabelText("Set")]
        public Type BaseType_Set;

        [Title("Parameter: BaseType")]
        [ShowInInspector]
        [LabelText("Not Set")]
        [TypeDrawerSettings(BaseType = null)]
        public Type BaseType_NotSet;

        [Title("Parameter: Filter")]
        [LabelText("Concrete Types")]
        [ShowInInspector]
        [TypeDrawerSettings(BaseType = typeof(IBaseGeneric<>), Filter = TypeInclusionFilter.IncludeConcreteTypes)]
        public Type Filter_Default;

        [Title("Parameter: Filter")]
        [TypeDrawerSettings(BaseType = typeof(IBaseGeneric<>), Filter = (TypeInclusionFilter.IncludeConcreteTypes | TypeInclusionFilter.IncludeGenerics))]
        [LabelText("Concrete- && Generic Types")]
        [ShowInInspector]
        public Type Filter_Generics;

        [Title("Parameter: Filter")]
        [LabelText("Concrete- && Interface Types")]
        [ShowInInspector]
        [TypeDrawerSettings(BaseType = typeof(IBaseGeneric<>), Filter = (TypeInclusionFilter.IncludeConcreteTypes | TypeInclusionFilter.IncludeInterfaces))]
        public Type Filter_Interfaces;

        [Title("Parameter: Filter")]
        [TypeDrawerSettings(BaseType = typeof(IBaseGeneric<>), Filter = (TypeInclusionFilter.IncludeConcreteTypes | TypeInclusionFilter.IncludeAbstracts))]
        [LabelText("Concrete- && Abstract Types")]
        [ShowInInspector]
        public Type Filter_Abstracts;

        [Title("Parameter: Filter")]
        [TypeDrawerSettings(BaseType = typeof(IBaseGeneric<>), Filter = (TypeInclusionFilter.IncludeConcreteTypes | TypeInclusionFilter.IncludeGenerics | TypeInclusionFilter.IncludeAbstracts))]
        [LabelText("Concrete-, Abstract- && Generic Types")]
        [ShowInInspector]
        public Type Filter_Abstracts_Generics;

        [Title("Parameter: Filter")]
        [LabelText("Concrete-, Interface- && Generic Types")]
        [ShowInInspector]
        [TypeDrawerSettings(BaseType = typeof(IBaseGeneric<>), Filter = (TypeInclusionFilter.IncludeConcreteTypes | TypeInclusionFilter.IncludeGenerics | TypeInclusionFilter.IncludeInterfaces))]
        public Type Filter_Interfaces_Generics;

        [Title("Parameter: Filter")]
        [ShowInInspector]
        [LabelText("Concrete-, Interface- && Abstract Types")]
        [TypeDrawerSettings(BaseType = typeof(IBaseGeneric<>), Filter = (TypeInclusionFilter.IncludeConcreteTypes | TypeInclusionFilter.IncludeAbstracts | TypeInclusionFilter.IncludeInterfaces))]
        public Type Filter_Interfaces_Abstracts;

        [Title("Parameter: Filter")]
        [ShowInInspector]
        [LabelText("All")]
        [TypeDrawerSettings(BaseType = typeof(IBaseGeneric<>), Filter = TypeInclusionFilter.IncludeAll)]
        public Type Filter_All;

        public interface IBaseGeneric<T> { }

        public interface IBase : IBaseGeneric<int> { }

        public abstract class Base : IBase, IBaseGeneric<int> { }

        public class Concrete : Base { }

        public class ConcreteGeneric<T> : Base { }

        public abstract class BaseGeneric<T> : IBase, IBaseGeneric<int> { }

        [CompilerGenerated]
        public class ConcreteGenerated : Base { }

        public override void AesirInspectorReset()
        {
            Default = null;
            BaseType_Set = null;
            BaseType_NotSet = null;
            Filter_Default = null;
            Filter_Generics = null;
            Filter_Interfaces = null;
            Filter_Abstracts = null;
            Filter_Abstracts_Generics = null;
            Filter_Interfaces_Generics = null;
            Filter_Interfaces_Abstracts = null;
            Filter_All = null;
        }
    }
}
