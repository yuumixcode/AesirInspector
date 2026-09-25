using Sirenix.OdinInspector;
using UnityEngine;

namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// InlineEditor 特性的案例 SO。
    /// </summary>
    [AesirExample]
    public class InlineEditorExampleSO : AttributeExampleSO<InlineEditorExampleSO>
    {
        [FoldoutGroup("No Parameters")]
        [InlineEditor]
        public Material material;

        [FoldoutGroup("Parameter: InlineEditorModes")]
        [InlineEditor()]
        public Material guiOnly;

        [FoldoutGroup("Parameter: InlineEditorModes")]
        [InlineEditor(InlineEditorModes.GUIAndHeader)]
        public Material guiAndHeader;

        [FoldoutGroup("Parameter: InlineEditorModes")]
        [InlineEditor(InlineEditorModes.FullEditor)]
        public Material fullEditor;

        [FoldoutGroup("Parameter: InlineEditorModes")]
        [InlineEditor(InlineEditorModes.SmallPreview)]
        public Material[] smallPreviewList = new Material[3];

        [FoldoutGroup("Parameter: InlineEditorModes")]
        [InlineEditor(InlineEditorModes.LargePreview)]
        public Mesh mesh;

        [FoldoutGroup("Parameter: InlineEditorObjectFieldModes")]
        [InlineEditor(InlineEditorObjectFieldModes.Boxed)]
        public Material boxedMode;

        [FoldoutGroup("Parameter: InlineEditorObjectFieldModes")]
        [InlineEditor(InlineEditorObjectFieldModes.Foldout)]
        public Material foldoutMode;

        [FoldoutGroup("Parameter: InlineEditorObjectFieldModes")]
        [InlineEditor(InlineEditorObjectFieldModes.CompletelyHidden)]
        public Material completelyHiddenMode;

        [FoldoutGroup("Parameter: InlineEditorObjectFieldModes")]
        [InlineEditor(InlineEditorObjectFieldModes.Hidden)]
        public Material hiddenMode;

        public override void AesirInspectorReset()
        {
            material = null;
            guiOnly = null;
            guiAndHeader = null;
            fullEditor = null;
            smallPreviewList = new Material[3];
            mesh = null;
            boxedMode = null;
            foldoutMode = null;
            completelyHiddenMode = null;
            hiddenMode = null;
        }
    }
}
