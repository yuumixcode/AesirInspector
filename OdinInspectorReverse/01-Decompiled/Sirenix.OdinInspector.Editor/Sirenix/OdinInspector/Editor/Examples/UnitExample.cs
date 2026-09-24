using Sirenix.OdinInspector.Editor.Examples.Internal;

namespace Sirenix.OdinInspector.Editor.Examples
{
	[ExampleAsComponentData(Namespaces = new string[] { "Sirenix.Utilities.Editor" })]
	[AttributeExample(typeof(UnitAttribute))]
	internal class UnitExample
	{
		[Unit(Units.Kilogram)]
		public float Weight;

		[Unit(Units.MetersPerSecond, Units.KilometersPerHour)]
		public float Speed;

		[Unit(Units.Meter, Units.Centimeter)]
		public float Distance;

		[ShowInInspector]
		[Unit(Units.MetersPerSecond, Units.MilesPerHour, DisplayAsString = true, ForceDisplayUnit = true)]
		public float SpeedMilesPerHour => Speed;
	}
}
