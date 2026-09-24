using System.Collections.Generic;
using Sirenix.OdinInspector.Editor.Examples.Internal;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Examples
{
	[ExampleAsComponentData(Namespaces = new string[] { "System.Collections.Generic" })]
	[AttributeExample(typeof(AssetListAttribute), "The AssetList attribute works on both lists of UnityEngine.Object types, and directly on UnityEngine.Object types, but has different behaviour for each case.")]
	internal class AssetListExamples
	{
		[AssetList]
		[PreviewField(70f, ObjectFieldAlignment.Center)]
		public Texture2D SingleObject;

		[AssetList(Path = "/Plugins/Sirenix/")]
		public List<ScriptableObject> AssetList;

		[AssetList(Path = "Plugins/Sirenix/")]
		[FoldoutGroup("Filtered Odin ScriptableObjects", false, 0f)]
		public ScriptableObject Object;

		[AssetList(AutoPopulate = true, Path = "Plugins/Sirenix/")]
		[FoldoutGroup("Filtered Odin ScriptableObjects", false, 0f)]
		public List<ScriptableObject> AutoPopulatedWhenInspected;

		[FoldoutGroup("Filtered AssetLists examples", 0f)]
		[AssetList(LayerNames = "MyLayerName")]
		public GameObject[] AllPrefabsWithLayerName;

		[FoldoutGroup("Filtered AssetLists examples", 0f)]
		[AssetList(AssetNamePrefix = "Rock")]
		public List<GameObject> PrefabsStartingWithRock;

		[FoldoutGroup("Filtered AssetLists examples", 0f)]
		[AssetList(Tags = "MyTagA, MyTabB", Path = "/Plugins/Sirenix/")]
		public List<GameObject> GameObjectsWithTag;

		[AssetList(CustomFilterMethod = "HasRigidbodyComponent")]
		[FoldoutGroup("Filtered AssetLists examples", 0f)]
		public List<GameObject> MyRigidbodyPrefabs;

		private bool HasRigidbodyComponent(GameObject obj)
		{
			return obj.GetComponent<Rigidbody>() != null;
		}
	}
}
