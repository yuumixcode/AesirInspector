using Sirenix.OdinInspector;

namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// Unit 特性的案例 SO。
    /// </summary>
    [AesirExample]
    public class UnitExampleSO : AttributeExampleSO<UnitExampleSO>
    {
        [Title("Parameter: unit")]
        [Unit(Units.Kilogram)]
        public float Weight;

        [Title("Parameter: base, display")]
        [Unit(Units.MetersPerSecond, Units.KilometersPerHour)]
        public float Speed;

        [Unit(Units.Meter, Units.Centimeter)]
        public float Distance;

        [Title("Parameter: DisplayAsString, ForceDisplayUnit")]
        [ShowInInspector]
        [Unit(Units.MetersPerSecond, Units.MilesPerHour, DisplayAsString = true, ForceDisplayUnit = true)]
        public float SpeedMilesPerHour => Speed;

        public override void AesirInspectorReset()
        {
            Weight = 0f;
            Speed = 0f;
            Distance = 0f;
        }
    }
}
