using UnityEngine.SceneManagement;

namespace Sirenix.OdinInspector.Editor.Validation
{
	public static class SceneExtensions
	{
		public static SceneReference ToSceneReference(this Scene scene)
		{
			return new SceneReference(scene);
		}
	}
}
