using System;
using System.Collections.Generic;
using System.Reflection;
using Sirenix.Utilities;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Validation
{
	public abstract class SceneValidator : IValidator
	{
		private static Func<Type, bool, IEnumerable<UnityEngine.Object>> FindComponentsOfType;

		private bool initialized;

		private List<GameObject> sceneRoots;

		public SceneReference ValidatedScene { get; private set; }

		RevalidationCriteria IValidator.RevalidationCriteria => RevalidationCriteria.OnValueChange;

		static SceneValidator()
		{
			MethodInfo findObjectsOfType = null;
			if (UnityVersion.Major > 2019)
			{
				MethodInfo[] methods = typeof(UnityEngine.Object).GetMethods();
				MethodInfo[] array = methods;
				foreach (MethodInfo methodInfo in array)
				{
					if (methodInfo.IsPublic && !methodInfo.IsGenericMethod && methodInfo.IsStatic && methodInfo.Name == "FindObjectsOfType")
					{
						ParameterInfo[] parms = methodInfo.GetParameters();
						if (parms.Length == 2 && parms[0].ParameterType == typeof(Type) && parms[1].ParameterType == typeof(bool))
						{
							findObjectsOfType = methodInfo;
							break;
						}
					}
				}
			}
			if (findObjectsOfType == null)
			{
				FindComponentsOfType = (Type t, bool b) => FindComponentsOfTypeFallback(t, b);
				return;
			}
			Func<Type, bool, UnityEngine.Object[]> methodDel = (Func<Type, bool, UnityEngine.Object[]>)Delegate.CreateDelegate(typeof(Func<Type, bool, UnityEngine.Object[]>), findObjectsOfType);
			FindComponentsOfType = (Type t, bool b) => methodDel(t, b);
		}

		private static IEnumerable<UnityEngine.Object> FindComponentsOfTypeFallback(Type t, bool includeInactive)
		{
			UnityEngine.Object[] objects = Resources.FindObjectsOfTypeAll(t);
			UnityEngine.Object[] array = objects;
			foreach (UnityEngine.Object o in array)
			{
				if (o is Component cmp && (o.hideFlags & HideFlags.DontSave) == 0 && cmp.gameObject.scene.IsValid())
				{
					yield return o;
				}
			}
		}

		public void Initialize(SceneReference scene)
		{
			if (initialized)
			{
				throw new Exception("Can't initialize a scene validator twice!");
			}
			initialized = true;
			ValidatedScene = scene;
			Initialize();
		}

		public List<GameObject> GetSceneRoots()
		{
			LoadSceneIfNotLoaded();
			if (!ValidatedScene.IsValid || !ValidatedScene.IsLoaded)
			{
				return new List<GameObject>();
			}
			if (sceneRoots == null)
			{
				sceneRoots = new List<GameObject>();
			}
			else
			{
				sceneRoots.Clear();
			}
			if (ValidatedScene.TryGetScene(out var scene))
			{
				sceneRoots.AddRange(SceneUtilities.GetSceneRoots(scene));
			}
			return sceneRoots;
		}

		public GameObject GetSceneRoot(string name)
		{
			foreach (GameObject root in GetSceneRoots())
			{
				if (root.name == name)
				{
					return root;
				}
			}
			return null;
		}

		public IEnumerable<T> FindAllComponentsInSceneOfType<T>(bool includeInactive = true) where T : Component
		{
			IEnumerable<UnityEngine.Object> objects = FindComponentsOfType(typeof(T), includeInactive);
			foreach (UnityEngine.Object o in objects)
			{
				if (o is T cmp && cmp.gameObject.scene.path == ValidatedScene.Path)
				{
					yield return cmp;
				}
			}
		}

		public T FindComponentInSceneOfType<T>(bool includeInactive = true) where T : Component
		{
			using (IEnumerator<T> enumerator = FindAllComponentsInSceneOfType<T>(includeInactive).GetEnumerator())
			{
				if (enumerator.MoveNext())
				{
					return enumerator.Current;
				}
			}
			return null;
		}

		public IEnumerable<GameObject> GetAllGameObjectsInScene()
		{
			foreach (GameObject root in GetSceneRoots())
			{
				yield return root;
				foreach (Transform child in GetChildren(root.transform))
				{
					yield return child.gameObject;
				}
			}
		}

		private static IEnumerable<Transform> GetChildren(Transform transform)
		{
			int childCount = transform.childCount;
			for (int i = 0; i < childCount; i++)
			{
				Transform child = transform.GetChild(i);
				yield return child;
				foreach (Transform child2 in GetChildren(child))
				{
					yield return child2;
				}
			}
		}

		public GameObject GetGameObjectAtPath(string path, char pathSeparator = '/')
		{
			string[] steps = path.Split(new char[1] { pathSeparator });
			return GetGameObjectAtPath(steps);
		}

		public GameObject GetGameObjectAtPath(IList<string> path)
		{
			if (path.Count == 0)
			{
				return null;
			}
			GameObject root = GetSceneRoot(path[0]);
			if (root == null)
			{
				return null;
			}
			Transform current = root.transform;
			for (int i = 1; i < path.Count; i++)
			{
				int childCount = current.childCount;
				Transform next = null;
				for (int j = 0; j < childCount; j++)
				{
					Transform child = current.GetChild(j);
					if (child.name == path[i])
					{
						next = child;
						break;
					}
				}
				if (next == null)
				{
					return null;
				}
				current = next;
			}
			return current.gameObject;
		}

		public T GetComponentAtPath<T>(string path, char pathSeparator = '/') where T : Component
		{
			GameObject go = GetGameObjectAtPath(path, pathSeparator);
			if (go != null)
			{
				return go.GetComponent<T>();
			}
			return null;
		}

		public T GetComponentAtPath<T>(IList<string> path) where T : Component
		{
			GameObject go = GetGameObjectAtPath(path);
			if (go != null)
			{
				return go.GetComponent<T>();
			}
			return null;
		}

		public void LoadSceneIfNotLoaded(bool askToSave = true)
		{
			if (!ValidatedScene.IsLoaded && ValidatedScene.IsValid && ValidatedScene.TryGetScene(out var scene))
			{
				if (askToSave)
				{
					EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
				}
				EditorSceneManager.OpenScene(scene.path, OpenSceneMode.Single);
			}
		}

		protected virtual void Initialize()
		{
		}

		public virtual bool CanValidateScene(SceneReference scene)
		{
			if (scene.TryGetScene(out var unityScene))
			{
				return unityScene.IsValid();
			}
			return false;
		}

		public void RunValidation(ref ValidationResult result)
		{
			if (result == null)
			{
				result = new ValidationResult();
			}
			result.Setup = new ValidationSetup
			{
				Validator = this,
				Root = ValidatedScene
			};
			result.Path = ValidatedScene.Path;
			result.ResultType = ValidationResultType.Valid;
			result.Message = "";
			try
			{
				Validate(result);
			}
			catch (Exception innerException)
			{
				while (innerException is TargetInvocationException)
				{
					innerException = innerException.InnerException;
				}
				result.ResultType = ValidationResultType.Error;
				result.Message = "An exception was thrown during validation: " + innerException.ToString();
			}
		}

		protected abstract void Validate(ValidationResult result);
	}
}
