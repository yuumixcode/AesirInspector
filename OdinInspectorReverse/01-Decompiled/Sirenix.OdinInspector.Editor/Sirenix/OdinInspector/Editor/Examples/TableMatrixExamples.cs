using Sirenix.OdinInspector.Editor.Examples.Internal;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Examples
{
	[ShowOdinSerializedPropertiesInInspector]
	[AttributeExample(typeof(TableMatrixAttribute), "Right-click columns to use context menus, or drag the column and row labels in order to modify the tables.")]
	[ExampleAsComponentData(Namespaces = new string[] { "Sirenix.OdinInspector.Editor.Examples" })]
	internal class TableMatrixExamples
	{
		[TableMatrix(HorizontalTitle = "Square Celled Matrix", SquareCells = true)]
		public Texture2D[,] SquareCelledMatrix;

		[TableMatrix(SquareCells = true)]
		public Mesh[,] PrefabMatrix;

		[OnInspectorInit]
		private void CreateData()
		{
			SquareCelledMatrix = new Texture2D[8, 4]
			{
				{
					ExampleHelper.GetTexture(0),
					null,
					null,
					null
				},
				{
					null,
					ExampleHelper.GetTexture(1),
					null,
					null
				},
				{
					null,
					null,
					ExampleHelper.GetTexture(2),
					null
				},
				{
					null,
					null,
					null,
					ExampleHelper.GetTexture(3)
				},
				{
					ExampleHelper.GetTexture(4),
					null,
					null,
					null
				},
				{
					null,
					ExampleHelper.GetTexture(5),
					null,
					null
				},
				{
					null,
					null,
					ExampleHelper.GetTexture(6),
					null
				},
				{
					null,
					null,
					null,
					ExampleHelper.GetTexture(7)
				}
			};
			PrefabMatrix = new Mesh[8, 4]
			{
				{
					ExampleHelper.GetMesh(),
					null,
					null,
					null
				},
				{
					null,
					ExampleHelper.GetMesh(),
					null,
					null
				},
				{
					null,
					null,
					ExampleHelper.GetMesh(),
					null
				},
				{
					null,
					null,
					null,
					ExampleHelper.GetMesh()
				},
				{
					null,
					null,
					null,
					ExampleHelper.GetMesh()
				},
				{
					null,
					null,
					ExampleHelper.GetMesh(),
					null
				},
				{
					null,
					ExampleHelper.GetMesh(),
					null,
					null
				},
				{
					ExampleHelper.GetMesh(),
					null,
					null,
					null
				}
			};
		}
	}
}
