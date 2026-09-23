using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;

namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// TypeSelectorSettings 特性的案例 SO。
    /// </summary>
    [AesirExample]
    public class TypeSelectorSettingsExampleSO : AttributeExampleSO<TypeSelectorSettingsExampleSO>
    {
        [Title("No Parameters")]
        [ShowInInspector]
        public Type Default;

        [Title("Parameter: ShowCategories")]
        [LabelText("On")]
        [ShowInInspector]
        [TypeSelectorSettings(ShowCategories = true)]
        public Type ShowCategories_On;

        [Title("Parameter: ShowCategories")]
        [LabelText("Off")]
        [ShowInInspector]
        [TypeSelectorSettings(ShowCategories = false)]
        public Type ShowCategories_Off;

        [Title("Parameter: PreferNamespaces")]
        [LabelText("On")]
        [ShowInInspector]
        [TypeSelectorSettings(PreferNamespaces = true, ShowCategories = true)]
        public Type PreferNamespaces_On;

        [Title("Parameter: PreferNamespaces")]
        [LabelText("Off")]
        [ShowInInspector]
        [TypeSelectorSettings(PreferNamespaces = false, ShowCategories = true)]
        public Type PreferNamespaces_Off;

        [Title("Parameter: ShowNoneItem")]
        [LabelText("On")]
        [ShowInInspector]
        [TypeSelectorSettings(ShowNoneItem = true)]
        public Type ShowNoneItem_On;

        [Title("Parameter: ShowNoneItem")]
        [LabelText("Off")]
        [ShowInInspector]
        [TypeSelectorSettings(ShowNoneItem = false)]
        public Type ShowNoneItem_Off;

        [Title("Parameter: FilterTypesFunction (Type type)")]
        [ShowInInspector]
        [TypeSelectorSettings(FilterTypesFunction = "TypeFilter", ShowCategories = false)]
        public Type CustomTypeFilterExample;

        private bool TypeFilter(Type type)
        {
            return type.GetInterfaces().Any((Type i) => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>));
        }

        public override void AesirInspectorReset()
        {
            Default = null;
            ShowCategories_On = null;
            ShowCategories_Off = null;
            PreferNamespaces_On = null;
            PreferNamespaces_Off = null;
            ShowNoneItem_On = null;
            ShowNoneItem_Off = null;
            CustomTypeFilterExample = null;
        }
    }
}
