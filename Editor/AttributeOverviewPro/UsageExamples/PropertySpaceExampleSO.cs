using Sirenix.OdinInspector;

namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// PropertySpace 特性的案例 SO。
    /// </summary>
    [AesirExample]
    internal class PropertySpaceExampleSO : AttributeExampleSO<PropertySpaceExampleSO>
    {
        [Title("No Parameters")]
        public int noSpace;

        [Title("No Parameters")]
        [PropertySpace]
        [ShowInInspector]
        public string Property { get; set; }

        [Title("Parameter: SpaceBefore")]
        [PropertySpace(20)]
        public int spaceBefore;

        [Title("Parameter: SpaceBefore, SpaceAfter")]
        [PropertySpace(20, 20)]
        public int spaceBeforeAndAfter;

        [Title("Parameter: SpaceBefore, SpaceAfter")]
        [PropertySpace(SpaceBefore = 30, SpaceAfter = 60)]
        public int BeforeAndAfter;

        public override void AesirInspectorReset()
        {
            noSpace = 0;
            Property = null;
            spaceBefore = 0;
            spaceBeforeAndAfter = 0;
            BeforeAndAfter = 0;
        }
    }
}
