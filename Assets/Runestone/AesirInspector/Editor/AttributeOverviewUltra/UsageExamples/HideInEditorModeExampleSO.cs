using Sirenix.OdinInspector;

namespace Runestone.AesirInspector.Editor
{
    [AesirExample]
    public class HideInEditorModeExampleSO : AttributeExampleSO<HideInEditorModeExampleSO>
    {
        [Title("No Parameters")]
        public int alwaysVisible;

        [HideInEditorMode]
        public string hiddenInEditor = "This is visible in play mode but hidden in editor mode";

        [HideInEditorMode]
        public int hiddenInEditorMode;

        public override void AesirInspectorReset()
        {
            alwaysVisible = 0;
            hiddenInEditor = "This is visible in play mode but hidden in editor mode";
            hiddenInEditorMode = 0;
        }
    }
}
