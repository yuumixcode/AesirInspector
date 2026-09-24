using System.Collections.Generic;
using Sirenix.OdinInspector.Editor.Examples.Internal;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Examples
{
	[AttributeExample(typeof(SceneObjectsOnlyAttribute))]
	[ExampleAsComponentData(Namespaces = new string[] { "System.Collections.Generic" })]
	[AttributeExample(typeof(AssetsOnlyAttribute))]
	internal class SceneAndAssetsOnlyExamples
	{
		[Title("Assets only", null, TitleAlignments.Left, true, true)]
		[AssetsOnly]
		public List<GameObject> OnlyPrefabs;

		[AssetsOnly]
		public GameObject SomePrefab;

		[AssetsOnly]
		public Material MaterialAsset;

		[AssetsOnly]
		public MeshRenderer SomeMeshRendererOnPrefab;

		[SceneObjectsOnly]
		[Title("Scene Objects only", null, TitleAlignments.Left, true, true)]
		public List<GameObject> OnlySceneObjects;

		[SceneObjectsOnly]
		public GameObject SomeSceneObject;

		[SceneObjectsOnly]
		public MeshRenderer SomeMeshRenderer;
	}
}
