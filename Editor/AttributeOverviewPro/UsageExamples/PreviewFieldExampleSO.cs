using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using UnityEngine;

namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// PreviewField 特性的案例 SO。
    /// </summary>
    [AesirExample]
    public class PreviewFieldExampleSO : AttributeExampleSO<PreviewFieldExampleSO>
    {
        [FoldoutGroup("No Parameters")]
        [PreviewField]
        public Texture regularPreviewField;

        [FoldoutGroup("Parameter: Height")]
        [PreviewField(Height = 70)]
        public Texture2D texture2D;

        [FoldoutGroup("Parameter: FilterMode")]
        [PreviewField(FilterMode = FilterMode.Point)]
        public Texture2D texture2D2;

        [FoldoutGroup("Parameter: ObjectFieldAlignment")]
        [PreviewField(ObjectFieldAlignment.Center)]
        public Texture previewField2;

        [FoldoutGroup("Parameter: ObjectFieldAlignment")]
        [VerticalGroup("Parameter: ObjectFieldAlignment/row1/left")]
        public string A;

        [FoldoutGroup("Parameter: ObjectFieldAlignment")]
        [VerticalGroup("Parameter: ObjectFieldAlignment/row1/left")]
        public string B;

        [FoldoutGroup("Parameter: ObjectFieldAlignment")]
        [VerticalGroup("Parameter: ObjectFieldAlignment/row1/left")]
        public string C;

        [FoldoutGroup("Parameter: ObjectFieldAlignment")]
        [HideLabel]
        [PreviewField(50f, ObjectFieldAlignment.Right)]
        [HorizontalGroup("Parameter: ObjectFieldAlignment/row1", 50f)]
        [VerticalGroup("Parameter: ObjectFieldAlignment/row1/right")]
        public Object D;

        [FoldoutGroup("Parameter: ObjectFieldAlignment")]
        [HideLabel]
        [HorizontalGroup("Parameter: ObjectFieldAlignment/row2", 50f)]
        [PreviewField(50f, ObjectFieldAlignment.Left)]
        [VerticalGroup("Parameter: ObjectFieldAlignment/row2/left")]
        public Object E;

        [FoldoutGroup("Parameter: ObjectFieldAlignment")]
        [VerticalGroup("Parameter: ObjectFieldAlignment/row2/right")]
        [LabelWidth(-54f)]
        public string F;

        [FoldoutGroup("Parameter: ObjectFieldAlignment")]
        [VerticalGroup("Parameter: ObjectFieldAlignment/row2/right")]
        [LabelWidth(-54f)]
        public string G;

        [FoldoutGroup("Parameter: ObjectFieldAlignment")]
        [LabelWidth(-54f)]
        [VerticalGroup("Parameter: ObjectFieldAlignment/row2/right")]
        public string H;

        [FoldoutGroup("Parameter: PreviewGetter")]
        [PreviewField("preview")]
        public Object I;

        [FoldoutGroup("Parameter: PreviewGetter")]
#pragma warning disable CS0414
        Texture preview;
#pragma warning restore CS0414

        [FoldoutGroup("Global Configuration")]
        [InfoBox(
            "These object fields can also be selectively enabled and customized globally from the Odin preferences window.\n\n - Hold Ctrl + Click = Delete Instance\n - Drag and drop = Move / Swap.\n - Ctrl + Drag = Replace.\n - Ctrl + drag and drop = Move and override.")]
        [Button(ButtonSizes.Large)]
        void ConfigureGlobalPreviewFieldSettings()
        {
            GlobalConfig<GeneralDrawerConfig>.Instance.OpenInEditor();
        }

        public override void AesirInspectorReset()
        {
            regularPreviewField = null;
            texture2D = null;
            texture2D2 = null;
            previewField2 = null;
            A = null;
            B = null;
            C = null;
            D = null;
            E = null;
            F = null;
            G = null;
            H = null;
            I = null;
            preview = null;
        }
    }
}
