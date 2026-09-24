using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.OdinInspector.Editor.Internal;
using Sirenix.OdinInspector.Editor.Validation;
using Sirenix.Reflection.Editor;
using Sirenix.Utilities;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinValidator.Editor
{
	public sealed class OdinValidationRunner : IDisposable
	{
		public class AssetUnloadedWhileValidatingException : Exception
		{
			public AssetUnloadedWhileValidatingException(string message)
				: base(message)
			{
			}
		}

		private class DebugLogCollector : ILogHandler, IDisposable
		{
			public List<ResultItem> Logs = new List<ResultItem>();

			private ILogHandler prevLogger;

			private bool supressErrorsAndWarningsFromUnityLogger;

			public DebugLogCollector(bool supressErrorsAndWarningsFromUnityLogger)
			{
				prevLogger = UnityEngine.Debug.unityLogger.logHandler;
				UnityEngine.Debug.unityLogger.logHandler = this;
				this.supressErrorsAndWarningsFromUnityLogger = supressErrorsAndWarningsFromUnityLogger;
				Application.logMessageReceivedThreaded += OnLog;
			}

			private void OnLog(string condition, string stackTrace, LogType type)
			{
				if (type == LogType.Warning || type == LogType.Error || type == LogType.Exception || type == LogType.Assert)
				{
					lock (Logs)
					{
						Logs.Add(new ResultItem(type.ToString() + ":" + condition + "\n\n" + stackTrace, (type == LogType.Warning) ? ValidationResultType.Warning : ValidationResultType.Error));
					}
				}
			}

			public void LogFormat(LogType logType, UnityEngine.Object context, string format, params object[] args)
			{
				if (logType == LogType.Error || logType == LogType.Warning || logType == LogType.Exception || logType == LogType.Assert)
				{
					if (!supressErrorsAndWarningsFromUnityLogger)
					{
						prevLogger.LogFormat(logType, context, format, args);
					}
					Logs.Add(new ResultItem(string.Format(format, args), (logType == LogType.Warning) ? ValidationResultType.Warning : ValidationResultType.Error));
				}
				else
				{
					prevLogger.LogFormat(logType, context, format, args);
				}
			}

			public void LogException(Exception exception, UnityEngine.Object context)
			{
				if (!supressErrorsAndWarningsFromUnityLogger)
				{
					prevLogger.LogException(exception, context);
				}
				Logs.Add(new ResultItem(exception.ToString(), ValidationResultType.Error));
			}

			public ResultItem[] GetLoadIssuesOrNull(bool distinct = false)
			{
				if (Logs.Count == 0)
				{
					return null;
				}
				if (distinct)
				{
					return Logs.Distinct().ToArray();
				}
				return Logs.ToArray();
			}

			public void Dispose()
			{
				Application.logMessageReceivedThreaded -= OnLog;
				UnityEngine.Debug.unityLogger.logHandler = prevLogger;
			}
		}

		private struct LoadedAssetInfo
		{
			public enum AssetType
			{
				BrokenAsset,
				Normal,
				BrokenComponent
			}

			public AssetType Type;

			public UnityEngine.Object Asset;

			public GameObject OwningGO;

			public UnityEngine.Component BrokenComponent;

			public int ComponentIndex;

			public string AssetPath;

			public ObjectAddress Address;

			public bool IsValid;

			public bool IsMainAsset;

			public int SubAssetIndex;

			public ResultItem[] LoadIssues;

			public LoadedAssetInfo(AssetType type, UnityEngine.Object asset, string assetPath, ObjectAddress address, bool isMainAsset, int subAssetIndex, ResultItem[] loadIssues)
			{
				Type = type;
				Asset = asset;
				OwningGO = null;
				BrokenComponent = null;
				ComponentIndex = -1;
				AssetPath = assetPath;
				Address = address;
				IsValid = true;
				IsMainAsset = isMainAsset;
				SubAssetIndex = subAssetIndex;
				LoadIssues = loadIssues;
			}

			public LoadedAssetInfo(GameObject owningGo, UnityEngine.Component brokenComponent, int componentIndex, string assetPath, ObjectAddress data, ResultItem[] loadIssues)
			{
				Type = AssetType.BrokenComponent;
				Asset = null;
				OwningGO = owningGo;
				BrokenComponent = brokenComponent;
				ComponentIndex = componentIndex;
				AssetPath = assetPath;
				Address = data;
				IsValid = true;
				IsMainAsset = false;
				SubAssetIndex = 0;
				LoadIssues = loadIssues;
			}
		}

		private static WeakReferenceEventListener<OdinValidationRunner> unloadEventListener;

		public static readonly EditorPrefBool EnableLeakDetection;

		private bool isDisposed;

		private Dictionary<Type, PropertyTree> cachedPropertyTrees;

		private StackTrace allocationTrace;

		public OdinValidationPolicy Policy;

		public ValidationRunnerConfig Config;

		private MiniPool<List<ValidationResult>> validationResultListPool = new MiniPool<List<ValidationResult>>(() => new List<ValidationResult>());

		private Func<Type, bool> customValidationFilter;

		static OdinValidationRunner()
		{
			EnableLeakDetection = new EditorPrefBool("Odin_Validator_ValidationRunner_EnableLeakDetection", defaultValue: true);
			unloadEventListener = new WeakReferenceEventListener<OdinValidationRunner>(delegate(OdinValidationRunner runner, object[] args)
			{
				runner.OnUnload();
			});
			AssemblyReloadEvents.beforeAssemblyReload += delegate
			{
				unloadEventListener.InvokeEvent(null);
			};
		}

		public OdinValidationRunner(OdinValidationPolicy policy = null, ValidationRunnerConfig config = null)
		{
			Policy = policy ?? OdinValidationPolicy.Default;
			Config = config ?? ValidationRunnerConfig.Default;
			unloadEventListener.SubscribeListener(this);
			if ((bool)EnableLeakDetection)
			{
				allocationTrace = new StackTrace();
			}
		}

		~OdinValidationRunner()
		{
			if (!isDisposed)
			{
				Dispose(isLeaking: true);
			}
		}

		private void OnUnload()
		{
			if (!isDisposed)
			{
				Dispose(isLeaking: false);
			}
		}

		public void Dispose()
		{
			Dispose(isLeaking: false);
		}

		private void Dispose(bool isLeaking)
		{
			if (isDisposed)
			{
				return;
			}
			if (isLeaking)
			{
				if (allocationTrace != null)
				{
					UnityEngine.Debug.LogWarning("An Odin ValidationRunner instance is being garbage collected without first having been disposed. ValidationRunner instances must be disposed once they are no longer needed. This instance was allocated at the following location: \n\n" + allocationTrace.ToString());
				}
				UnityEditorEventUtility.DelayActionThreadSafe(ActuallyDispose);
			}
			else
			{
				ActuallyDispose();
			}
		}

		private void ActuallyDispose()
		{
			if (cachedPropertyTrees != null)
			{
				foreach (PropertyTree tree in cachedPropertyTrees.Values)
				{
					tree.Dispose();
				}
				cachedPropertyTrees = null;
			}
			isDisposed = true;
			unloadEventListener.DesubscribeListener(this);
		}

		public IEnumerable<PersistentValidationResultBatch> ValidateAllAssetsAtPath(string path)
		{
			IEnumerable<LoadedAssetInfo> assetsAtPath = LoadAllAssetsAtPathProperly(path);
			int i = 0;
			foreach (LoadedAssetInfo loadInfo in assetsAtPath)
			{
				i++;
				int j = 0;
				bool hadResults = false;
				foreach (PersistentValidationResultBatch r in ValidateAsset(loadInfo))
				{
					hadResults = true;
					j++;
					yield return r;
				}
				if (!hadResults)
				{
					yield return PersistentValidationResultBatch.Ignore;
				}
			}
		}

		private IEnumerable<LoadedAssetInfo> LoadAllAssetsAtPathProperly(string path)
		{
			if (string.IsNullOrWhiteSpace(path))
			{
				return Enumerable.Empty<LoadedAssetInfo>();
			}
			string guid = AssetDatabase.AssetPathToGUID(path);
			if (string.IsNullOrWhiteSpace(guid))
			{
				return Enumerable.Empty<LoadedAssetInfo>();
			}
			if (path.FastEndsWith(".prefab"))
			{
				GameObject mainAsset;
				ResultItem[] loadIssues;
				using (DebugLogCollector logHandler = new DebugLogCollector(GlobalConfig<GlobalValidationConfig>.Instance.SupressAssetLoadErrorsFromUnityLogger.Value))
				{
					using (AssetLoadTimings.Time(path))
					{
						mainAsset = AssetDatabase.LoadMainAssetAtPath(path) as GameObject;
					}
					loadIssues = logHandler.GetLoadIssuesOrNull();
				}
				if (mainAsset == null)
				{
					Type objectType = typeof(GameObject);
					string niceObjectName = path.GetObjectNameFromAssetPath();
					return new LoadedAssetInfo[1]
					{
						new LoadedAssetInfo(LoadedAssetInfo.AssetType.BrokenAsset, mainAsset, path, new ObjectAddress(ObjectAddress.AddressType.PrefabGameObject, guid, path, default(ObjectAddress.SubAssetAddress), default(ObjectAddress.HierarchyAddress), default(ObjectAddress.ComponentAddress), niceObjectName, objectType, isBroken: true), isMainAsset: true, 0, loadIssues)
					};
				}
				return RecurseThroughPrefab(mainAsset, guid, path, -1, null, null, isMainAsset: true, loadIssues);
			}
			if (path.FastEndsWith(".unity") || path.FastEndsWith(".scene"))
			{
				SceneAsset sceneAsset;
				ResultItem[] loadIssues2;
				using (DebugLogCollector logHandler2 = new DebugLogCollector(GlobalConfig<GlobalValidationConfig>.Instance.SupressAssetLoadErrorsFromUnityLogger.Value))
				{
					using (AssetLoadTimings.Time(path))
					{
						sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(path);
					}
					loadIssues2 = logHandler2.GetLoadIssuesOrNull();
				}
				bool isBroken = sceneAsset == null;
				LoadedAssetInfo.AssetType infoType = ((!isBroken) ? LoadedAssetInfo.AssetType.Normal : LoadedAssetInfo.AssetType.BrokenAsset);
				string niceObjectName2 = path.GetObjectNameFromAssetPath();
				Type objectType2 = typeof(SceneAsset);
				ObjectAddress recoveryData = new ObjectAddress(ObjectAddress.AddressType.Asset, guid, path, default(ObjectAddress.SubAssetAddress), default(ObjectAddress.HierarchyAddress), default(ObjectAddress.ComponentAddress), niceObjectName2, objectType2, isBroken);
				return new LoadedAssetInfo[1]
				{
					new LoadedAssetInfo(infoType, sceneAsset, path, recoveryData, isMainAsset: true, 0, loadIssues2)
				};
			}
			UnityEngine.Object[] assets = null;
			UnityEngine.Object mainAsset2 = null;
			ResultItem[] loadIssues3;
			using (DebugLogCollector logHandler3 = new DebugLogCollector(GlobalConfig<GlobalValidationConfig>.Instance.SupressAssetLoadErrorsFromUnityLogger.Value))
			{
				using (AssetLoadTimings.Time(path))
				{
					assets = AssetDatabase.LoadAllAssetsAtPath(path);
					mainAsset2 = AssetDatabase.LoadMainAssetAtPath(path);
				}
				loadIssues3 = logHandler3.GetLoadIssuesOrNull();
			}
			bool assetsContainMainAsset = false;
			for (int i = 0; i < assets.Length; i++)
			{
				if (mainAsset2 == assets[i])
				{
					assetsContainMainAsset = true;
					break;
				}
			}
			if (!assetsContainMainAsset)
			{
				assets = ArrayUtilities.CreateNewArrayWithInsertedElement(assets, 0, mainAsset2);
			}
			LoadedAssetInfo[] resultArr = new LoadedAssetInfo[assets.Length];
			for (int j = 0; j < assets.Length; j++)
			{
				bool isMainAsset = ((!(mainAsset2 == null)) ? (mainAsset2 == assets[j]) : (j == 0));
				bool isBroken2 = assets[j] == null;
				ObjectAddress.AddressType type = ObjectAddress.AddressType.Asset;
				Type objectType3;
				string niceObjectName3;
				if (isBroken2)
				{
					objectType3 = typeof(UnityEngine.Object);
					niceObjectName3 = path.GetObjectNameFromAssetPath();
					if (!isMainAsset)
					{
						niceObjectName3 += $" [{j}]";
					}
				}
				else
				{
					objectType3 = assets[j].GetType();
					niceObjectName3 = assets[j].name;
					if (!isMainAsset)
					{
						niceObjectName3 += $" [{j}]";
					}
				}
				object obj = assets[j];
				Type objType = null;
				OdinEntityId entityId = OdinEntityId.None;
				if (obj != null)
				{
					objType = obj.GetType();
					entityId = OdinEntityId.FromObject(assets[j]);
				}
				ObjectAddress.SubAssetAddress subAddress = (isMainAsset ? default(ObjectAddress.SubAssetAddress) : new ObjectAddress.SubAssetAddress(objType, entityId, j));
				ObjectAddress objectAddress = new ObjectAddress(type, guid, path, subAddress, default(ObjectAddress.HierarchyAddress), default(ObjectAddress.ComponentAddress), niceObjectName3, objectType3, isBroken2);
				LoadedAssetInfo.AssetType infoType2 = ((!isBroken2) ? LoadedAssetInfo.AssetType.Normal : LoadedAssetInfo.AssetType.BrokenAsset);
				resultArr[j] = new LoadedAssetInfo(infoType2, assets[j], path, objectAddress, isMainAsset, j, isMainAsset ? loadIssues3 : null);
			}
			return resultArr;
		}

		private IEnumerable<LoadedAssetInfo> RecurseThroughPrefab(GameObject go, string guid, string path, int gameObjectIndex, string[] names, int[] indices, bool isMainAsset, ResultItem[] loadIssues)
		{
			bool isBroken = go == null;
			ObjectAddress.AddressType type = ObjectAddress.AddressType.PrefabGameObject;
			ObjectAddress.HierarchyAddress hierarchyAddress = default(ObjectAddress.HierarchyAddress);
			if (!isBroken && !isMainAsset)
			{
				hierarchyAddress = new ObjectAddress.HierarchyAddress(ArrayUtilities.CreateNewArrayWithAddedElement(names ?? new string[0], go.name), ArrayUtilities.CreateNewArrayWithAddedElement(indices ?? new int[0], gameObjectIndex));
			}
			else if (isBroken)
			{
				hierarchyAddress = new ObjectAddress.HierarchyAddress(ArrayUtilities.CreateNewArrayWithAddedElement(names ?? new string[0], "|BROKEN|"), ArrayUtilities.CreateNewArrayWithAddedElement(indices ?? new int[0], gameObjectIndex));
			}
			Type objectType;
			string niceObjectName;
			if (isBroken)
			{
				objectType = typeof(GameObject);
				niceObjectName = "";
			}
			else
			{
				objectType = typeof(GameObject);
				niceObjectName = go.name;
			}
			yield return new LoadedAssetInfo((!isBroken) ? LoadedAssetInfo.AssetType.Normal : LoadedAssetInfo.AssetType.BrokenAsset, go, path, new ObjectAddress(type, guid, path, default(ObjectAddress.SubAssetAddress), hierarchyAddress, default(ObjectAddress.ComponentAddress), niceObjectName, objectType, isBroken), isMainAsset, gameObjectIndex, loadIssues);
			if (go == null || isBroken)
			{
				yield break;
			}
			UnityEngine.Component[] components = go.GetComponents(typeof(UnityEngine.Component));
			string goName = go.name;
			int i = 0;
			while (true)
			{
				if (i < components.Length)
				{
					UnityEngine.Component component = components[i];
					bool isCmpBroken = component == null;
					ObjectAddress componentAddress = new ObjectAddress(objectType: (!isCmpBroken) ? component.GetType() : typeof(UnityEngine.Component), type: ObjectAddress.AddressType.PrefabComponent, assetGUID: guid, assetPath: path, subAsset: default(ObjectAddress.SubAssetAddress), hierarchyAddress: hierarchyAddress, component: new ObjectAddress.ComponentAddress(i, component?.GetType()), niceObjectName: goName, isBroken: isCmpBroken);
					componentAddress.IsBroken = component == null;
					if (component != null)
					{
						yield return new LoadedAssetInfo(LoadedAssetInfo.AssetType.Normal, component, path, componentAddress, isMainAsset: false, componentAddress.Component.ComponentIndex, null);
					}
					else
					{
						yield return new LoadedAssetInfo(go, component, i, path, componentAddress, null);
					}
					if (!(go == null))
					{
						i++;
						continue;
					}
					break;
				}
				Transform transform = go.transform;
				for (i = 0; i < transform.childCount; i++)
				{
					foreach (LoadedAssetInfo item in RecurseThroughPrefab(transform.GetChild(i).gameObject, guid, path, i, hierarchyAddress.SubNames, hierarchyAddress.SubIndices, isMainAsset: false, null))
					{
						yield return item;
						if (go == null)
						{
							yield break;
						}
					}
				}
				break;
			}
		}

		private IEnumerable<PersistentValidationResultBatch> ValidateAsset(LoadedAssetInfo loadInfo)
		{
			if (loadInfo.LoadIssues != null)
			{
				if (loadInfo.Asset == null)
				{
					yield return new PersistentValidationResultBatch(DynamicObjectAddress.CreateBroken(loadInfo.Address), null, 0, loadInfo.LoadIssues);
				}
				else
				{
					yield return new PersistentValidationResultBatch(DynamicObjectAddress.GetOrCreate(loadInfo.Asset, loadInfo.Address), null, 0, loadInfo.LoadIssues);
				}
			}
			if (loadInfo.Type == LoadedAssetInfo.AssetType.Normal)
			{
				if (loadInfo.Asset == null)
				{
					yield break;
				}
				foreach (PersistentValidationResultBatch item in ValidateObject(loadInfo.Asset))
				{
					yield return item;
				}
				yield break;
			}
			if (loadInfo.Type == LoadedAssetInfo.AssetType.BrokenAsset)
			{
				string message;
				if (loadInfo.AssetPath.EndsWith(".prefab"))
				{
					message = "Broken Prefab: a prefab at path '" + loadInfo.AssetPath + "' was null when loaded.";
				}
				else
				{
					string path = loadInfo.AssetPath;
					if (!loadInfo.IsMainAsset)
					{
						path += $" [{loadInfo.SubAssetIndex}]";
					}
					message = (((object)loadInfo.Asset == null) ? ("Broken Asset: " + (loadInfo.IsMainAsset ? "an asset" : "a subasset") + " of unknown type at path '" + path + "' was null when loaded.") : string.Format("Broken Asset: {0} of type '{1}' at path '{2}' was null when loaded.", loadInfo.IsMainAsset ? "an asset" : "a subasset", loadInfo.Asset.GetType(), path));
				}
				if (loadInfo.IsMainAsset)
				{
					yield return new PersistentValidationResultBatch(loadInfo.AssetPath, message, null, 0, ValidationResultType.Error, DynamicObjectAddress.CreateBroken(loadInfo.Address));
				}
				yield break;
			}
			if (loadInfo.Type == LoadedAssetInfo.AssetType.BrokenComponent)
			{
				if (!(loadInfo.OwningGO == null))
				{
					string message2 = (((object)loadInfo.BrokenComponent == null) ? ("Broken Component: a component at index '" + loadInfo.ComponentIndex + "' is null on the GameObject '" + loadInfo.OwningGO.name + "'! A script reference is likely broken.") : ("Broken Component: a component of type '" + loadInfo.BrokenComponent.GetType().GetNiceName() + "' at index '" + loadInfo.ComponentIndex + "' is null on the GameObject '" + loadInfo.OwningGO.name + "'! A script reference is likely broken."));
					yield return new PersistentValidationResultBatch(loadInfo.AssetPath, message2, null, 0, ValidationResultType.Error, DynamicObjectAddress.CreateBroken(loadInfo.Address));
				}
				yield break;
			}
			throw new NotImplementedException(loadInfo.Type.ToString());
		}

		internal IEnumerable<ValidationResult> ValidateGlobalValidator(Type globalValidator)
		{
			GlobalValidator validator = (GlobalValidator)Activator.CreateInstance(globalValidator);
			return ValidateGlobalValidator(validator);
		}

		internal IEnumerable<ValidationResult> ValidateGlobalValidator(GlobalValidator validator)
		{
			foreach (ValidationResult item in validator.RunValidation())
			{
				yield return item;
			}
		}

		public IEnumerable<PersistentValidationResultBatch> ValidateSceneValidators(SceneReference scene)
		{
			if (!scene.IsValid)
			{
				throw new Exception("Scene not valid: " + scene);
			}
			if (!scene.IsLoaded)
			{
				throw new Exception("Scene not loaded: " + scene);
			}
			IList<SceneValidator> validators = DefaultValidatorLocator.Instance.GetSceneValidators(scene);
			foreach (SceneValidator validator in validators)
			{
				ValidationResult result = new ValidationResult();
				validator.RunValidation(ref result);
				if (result.ResultType != ValidationResultType.IgnoreResult)
				{
					yield return new PersistentValidationResultBatch(result, null);
				}
			}
		}

		public IEnumerable<PersistentValidationResultBatch> ValidateSceneContent(SceneReference scene)
		{
			if (!scene.IsValid)
			{
				throw new Exception("Scene not valid: " + scene);
			}
			if (!scene.IsLoaded)
			{
				throw new Exception("Scene not loaded: " + scene);
			}
			if (!scene.TryGetScene(out var unityScene))
			{
				throw new Exception("Scene not loaded");
			}
			foreach (GameObject root in SceneUtilities.GetSceneRoots(unityScene))
			{
				if (!root)
				{
					continue;
				}
				bool foundResult1 = false;
				foreach (Transform t in IterateHierarchy(root.transform, returnSelf: true))
				{
					foundResult1 = true;
					bool foundResult2 = false;
					if (!t)
					{
						continue;
					}
					GameObject go = t.gameObject;
					if (go == null)
					{
						continue;
					}
					foreach (PersistentValidationResultBatch result in ValidateObject(go))
					{
						foundResult2 = true;
						yield return result;
					}
					if (go == null)
					{
						continue;
					}
					UnityEngine.Component[] components = go.GetComponents<UnityEngine.Component>();
					foreach (UnityEngine.Component component in components)
					{
						bool foundResult3 = false;
						if (go == null)
						{
							break;
						}
						if ((bool)component)
						{
							foreach (PersistentValidationResultBatch result2 in ValidateObject(component, go))
							{
								foundResult2 = true;
								foundResult3 = true;
								yield return result2;
							}
						}
						if (!foundResult3)
						{
							yield return PersistentValidationResultBatch.Ignore;
						}
					}
					if (!foundResult2)
					{
						yield return PersistentValidationResultBatch.Ignore;
					}
				}
				if (!foundResult1)
				{
					yield return PersistentValidationResultBatch.Ignore;
				}
			}
		}

		public IEnumerable<PersistentValidationResultBatch> ValidateScene(SceneReference scene)
		{
			if (!scene.IsValid)
			{
				throw new Exception("Scene not valid: " + scene);
			}
			if (!scene.IsLoaded)
			{
				throw new Exception("Scene not loaded: " + scene);
			}
			foreach (PersistentValidationResultBatch item in ValidateSceneValidators(scene))
			{
				yield return item;
			}
			foreach (PersistentValidationResultBatch item2 in ValidateSceneContent(scene))
			{
				yield return item2;
			}
		}

		private IEnumerable<Transform> IterateHierarchy(Transform transform, bool returnSelf)
		{
			if (returnSelf)
			{
				yield return transform;
			}
			if (!transform)
			{
				yield break;
			}
			for (int i = 0; i < ((!(transform == null)) ? transform.childCount : 0); i++)
			{
				if (!transform)
				{
					break;
				}
				Transform child = transform.GetChild(i);
				if (!child)
				{
					continue;
				}
				yield return child;
				foreach (Transform item in IterateHierarchy(child, returnSelf: false))
				{
					yield return item;
				}
			}
		}

		public IEnumerable<PersistentValidationResultBatch> ValidateObject(object value)
		{
			return ValidateObject(value, null);
		}

		public IEnumerable<PersistentValidationResultBatch> ValidateObject(object value, object parentContext)
		{
			return ValidateObject(value, parentContext, null);
		}

		public IEnumerable<PersistentValidationResultBatch> ValidateObject(object value, ObjectAddress objectAddress)
		{
			return ValidateObject(value, null, objectAddress);
		}

		public IEnumerable<PersistentValidationResultBatch> ValidateObject(object value, object parentContext, ObjectAddress objectAddress)
		{
			if (value == null)
			{
				if (parentContext is GameObject)
				{
					GameObject go = parentContext as GameObject;
					DynamicObjectAddress.TryGet(OdinEntityId.FromObject(go), out var dynAddress);
					yield return new PersistentValidationResultBatch(null, "Missing Component, the associated script can not be loaded.", null, 0, ValidationResultType.Error, dynAddress);
				}
				else
				{
					yield return new PersistentValidationResultBatch(null, "Root object to scan is null; this could indicate a corrupted asset or a component/asset with a missing script file", null, 0, ValidationResultType.Error);
				}
			}
			else
			{
				if (!Policy.ShouldValidate(this, value))
				{
					yield break;
				}
				bool isUnityObj = value is UnityEngine.Object;
				UnityEngine.Object unityObj = (isUnityObj ? (value as UnityEngine.Object) : null);
				if (!isUnityObj || !(unityObj == null))
				{
					if (cachedPropertyTrees == null)
					{
						cachedPropertyTrees = new Dictionary<Type, PropertyTree>(FastTypeComparer.Instance);
					}
					if (!cachedPropertyTrees.TryGetValue(value.GetType(), out var tree))
					{
						PersistentValidationResultBatch exceptionResult = null;
						try
						{
							tree = PropertyTree.Create(value).SetUpForValidation();
						}
						catch (Exception ex)
						{
							tree?.Dispose();
							if (isUnityObj)
							{
								exceptionResult = new PersistentValidationResultBatch(objectAddress: (!(objectAddress == null) && !(unityObj == null)) ? DynamicObjectAddress.GetOrCreate(unityObj, objectAddress) : DynamicObjectAddress.Unknown, path: null, message: ex.UnwrapException().ToString(), validatorType: null, validatorIndex: 0, resultType: ValidationResultType.Error);
							}
							else
							{
								DynamicObjectAddress dynObjectAddress = ((!(objectAddress == null) && !(unityObj == null)) ? DynamicObjectAddress.GetOrCreate(unityObj, objectAddress) : DynamicObjectAddress.Unknown);
								ex.GetBaseException();
								exceptionResult = new PersistentValidationResultBatch(null, ex.UnwrapException().ToString(), null, 0, ValidationResultType.Error, dynObjectAddress);
							}
						}
						if (exceptionResult != null)
						{
							yield return exceptionResult;
							yield break;
						}
					}
					else
					{
						cachedPropertyTrees.Remove(value.GetType());
						tree.SetTargets(value);
						tree.SetUpForValidation();
					}
					ValidationComponentProvider validationComponentProvider = null;
					foreach (ComponentProvider item in tree.ComponentProviders)
					{
						if (item is ValidationComponentProvider provider)
						{
							validationComponentProvider = provider;
							break;
						}
					}
					if (validationComponentProvider == null)
					{
						throw new Exception("Property had no ValidationComponentProvider");
					}
					customValidationFilter = customValidationFilter ?? ((Func<Type, bool>)((Type type) => Policy.CustomValidatorFilter(this, type)));
					validationComponentProvider.ValidatorLocator.CustomValidatorFilter = customValidationFilter;
					List<ValidationResult> resultBuffer = validationResultListPool.Get();
					resultBuffer.Clear();
					try
					{
						try
						{
							DynamicObjectAddress dynamicObjectAddress = null;
							if (isUnityObj)
							{
								if (objectAddress == null)
								{
									ObjectAddress.TryCreateObjectAddress(unityObj, out objectAddress, out var _);
								}
								if (objectAddress != null)
								{
									dynamicObjectAddress = DynamicObjectAddress.GetOrCreate(unityObj, objectAddress);
								}
							}
							if (dynamicObjectAddress == null)
							{
								dynamicObjectAddress = DynamicObjectAddress.Unknown;
							}
							InspectorProperty root = tree.RootProperty;
							ValidationComponent validationComponent = root.GetComponent<ValidationComponent>();
							if (validationComponent != null && root.GetAttribute<DontValidateAttribute>() == null && validationComponent.ValidatorLocator.PotentiallyHasValidatorsFor(root))
							{
								int from = resultBuffer.Count;
								validationComponent.ValidateProperty(ref resultBuffer);
								if (from == resultBuffer.Count)
								{
									yield return PersistentValidationResultBatch.Ignore;
									if (isUnityObj && unityObj == null)
									{
										yield break;
									}
								}
								else
								{
									for (int i = from; i < resultBuffer.Count; i++)
									{
										yield return new PersistentValidationResultBatch(resultBuffer[i], dynamicObjectAddress);
										if (isUnityObj && unityObj == null)
										{
											yield break;
										}
									}
								}
							}
							if (!Policy.ShouldDeeplyValidate(this, value))
							{
								yield break;
							}
							InspectorProperty current = tree.RootProperty;
							while (true)
							{
								tree.UpdateTree();
								while (current != null && !current.IsReachableFromRoot())
								{
									current = current.Parent;
								}
								if (current == null)
								{
									break;
								}
								bool isSameSerializationContext = current.SerializationRoot == tree.RootProperty;
								bool includeChildren = isSameSerializationContext && current.Info.PropertyType != PropertyType.Method && current.GetAttribute<DontValidateAttribute>() == null;
								current = current.NextProperty(includeChildren, visibleOnly: true);
								if (current == null)
								{
									break;
								}
								if (!isSameSerializationContext)
								{
									continue;
								}
								ValidationComponent validationComponent2 = current.GetComponent<ValidationComponent>();
								if (validationComponent2 == null || current.GetAttribute<DontValidateAttribute>() != null || !validationComponent2.ValidatorLocator.PotentiallyHasValidatorsFor(current))
								{
									continue;
								}
								int from2 = resultBuffer.Count;
								validationComponent2.ValidateProperty(ref resultBuffer);
								if (from2 == resultBuffer.Count)
								{
									yield return PersistentValidationResultBatch.Ignore;
									if (isUnityObj && unityObj == null)
									{
										break;
									}
									continue;
								}
								for (int i = from2; i < resultBuffer.Count; i++)
								{
									yield return new PersistentValidationResultBatch(resultBuffer[i], dynamicObjectAddress);
									if (isUnityObj && unityObj == null)
									{
										yield break;
									}
								}
							}
							yield break;
						}
						finally
						{
							OdinValidationRunner odinValidationRunner = this;
							if (odinValidationRunner.cachedPropertyTrees.ContainsKey(value.GetType()))
							{
								tree.Dispose();
							}
							else
							{
								tree.CleanForCachedReuse();
								odinValidationRunner.cachedPropertyTrees.Add(value.GetType(), tree);
							}
						}
					}
					finally
					{
						OdinValidationRunner odinValidationRunner = this;
						resultBuffer.Clear();
						odinValidationRunner.validationResultListPool.Return(resultBuffer);
					}
				}
				MonoScript script;
				string typeOfThing;
				if (unityObj is MonoBehaviour)
				{
					script = MonoScript.FromMonoBehaviour(unityObj as MonoBehaviour);
					typeOfThing = "MonoBehaviour";
				}
				else if (unityObj is ScriptableObject)
				{
					script = MonoScript.FromScriptableObject(unityObj as ScriptableObject);
					typeOfThing = "ScriptableObject";
				}
				else if (unityObj is UnityEngine.Component)
				{
					typeOfThing = "Component";
					script = null;
				}
				else
				{
					typeOfThing = "UnityEngine.Object";
					script = null;
				}
				if (script == null)
				{
					yield return new PersistentValidationResultBatch(null, typeOfThing + " of type '" + unityObj.GetType()?.ToString() + "' appears to have a missing script", null, 0, ValidationResultType.Error);
				}
				else
				{
					yield return new PersistentValidationResultBatch(null, typeOfThing + " of type '" + unityObj.GetType()?.ToString() + "' was in a destroyed state while being scanned", null, 0, ValidationResultType.Error);
				}
			}
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("Use ValidateObjectRecursively instead", false)]
		public IEnumerable<PersistentValidationResultBatch> ValidateUnityObjectRecursively(UnityEngine.Object value)
		{
			return ValidateObject(value);
		}
	}
}
