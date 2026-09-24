using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector.Editor.Examples.Internal;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Examples
{
	[AttributeExample(typeof(ValueDropdownAttribute))]
	[ExampleAsComponentData(Namespaces = new string[] { "System", "System.Collections", "System.Collections.Generic", "System.Linq" })]
	internal class ValueDropdownExamples
	{
		[ValueDropdown("TextureSizes")]
		public int SomeSize1;

		[ValueDropdown("FriendlyTextureSizes")]
		public int SomeSize2;

		[ValueDropdown("FriendlyTextureSizes", AppendNextDrawer = true, DisableGUIInAppendedDrawer = true)]
		public int SomeSize3;

		[ValueDropdown("GetListOfMonoBehaviours", AppendNextDrawer = true)]
		public MonoBehaviour SomeMonoBehaviour;

		[ValueDropdown("KeyCodes")]
		public KeyCode FilteredEnum;

		[ValueDropdown("TreeViewOfInts", ExpandAllMenuItems = true)]
		public List<int> IntTreview = new List<int> { 1, 2, 7 };

		[ValueDropdown("GetAllSceneObjects", IsUniqueList = true)]
		public List<GameObject> UniqueGameobjectList;

		[ValueDropdown("GetAllSceneObjects", IsUniqueList = true, DropdownTitle = "Select Scene Object", DrawDropdownForListElements = false, ExcludeExistingValuesInList = true)]
		public List<GameObject> UniqueGameobjectListMode2;

		private IEnumerable TreeViewOfInts = new ValueDropdownList<int>
		{
			{ "Node 1/Node 1.1", 1 },
			{ "Node 1/Node 1.2", 2 },
			{ "Node 2/Node 2.1", 3 },
			{ "Node 3/Node 3.1", 4 },
			{ "Node 3/Node 3.2", 5 },
			{ "Node 1/Node 3.1/Node 3.1.1", 6 },
			{ "Node 1/Node 3.1/Node 3.1.2", 7 }
		};

		private static IEnumerable<KeyCode> KeyCodes = Enumerable.Range(48, 10).Cast<KeyCode>();

		private static IEnumerable FriendlyTextureSizes = new ValueDropdownList<int>
		{
			{ "Small", 256 },
			{ "Medium", 512 },
			{ "Large", 1024 }
		};

		private static int[] TextureSizes = new int[3] { 256, 512, 1024 };

		private IEnumerable<MonoBehaviour> GetListOfMonoBehaviours()
		{
			return UnityEngine.Object.FindObjectsOfType<MonoBehaviour>();
		}

		private static IEnumerable GetAllSceneObjects()
		{
			Func<Transform, string> getPath = null;
			getPath = (Transform x) => (!x) ? "" : (getPath(x.parent) + "/" + x.gameObject.name);
			return from x in UnityEngine.Object.FindObjectsOfType<GameObject>()
				select new ValueDropdownItem(getPath(x.transform), x);
		}

		private static IEnumerable GetAllScriptableObjects()
		{
			return from x in AssetDatabase.FindAssets("t:ScriptableObject")
				select AssetDatabase.GUIDToAssetPath(x) into x
				select new ValueDropdownItem(x, AssetDatabase.LoadAssetAtPath<ScriptableObject>(x));
		}

		private static IEnumerable GetAllSirenixAssets()
		{
			string root = "Assets/Plugins/Sirenix/";
			return from x in AssetDatabase.GetAllAssetPaths()
				where x.StartsWith(root)
				select x.Substring(root.Length) into x
				select new ValueDropdownItem(x, AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(root + x));
		}
	}
}
