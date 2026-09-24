using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Sirenix.OdinInspector.Editor;
using Sirenix.OdinInspector.Editor.Internal;
using Sirenix.OdinInspector.Editor.Validation;
using Sirenix.Serialization;
using Sirenix.Utilities;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinValidator.Editor
{
	[OdinValidatorConfig]
	public class RuleConfig : GlobalConfig<RuleConfig>, ISerializedRulesContainer
	{
		private class InstantiationData
		{
			public bool Enabled;

			public IValidator Prototype;
		}

		[SerializeField]
		private List<SerializedRule> projectRules;

		[NonSerialized]
		private List<SerializedRule> localRules;

		[NonSerialized]
		private bool hasLoadedRules;

		private Dictionary<Type, InstantiationData> instantiationLookup;

		private static EditorPrefString localRulePath;

		private static List<SerializedRule> defaultRules;

		[NonSerialized]
		private RuleDataWrapper wrapperInstance;

		public static EditorPrefString LocalRulePath
		{
			get
			{
				if (localRulePath == null)
				{
					localRulePath = new EditorPrefString("SIRENIX_ODINVALIDATOR_LOCALRULEPATH", DefaultLocalRulePath);
				}
				if (string.IsNullOrEmpty(localRulePath.Value))
				{
					localRulePath.Value = DefaultLocalRulePath;
				}
				return localRulePath;
			}
		}

		public static string DefaultLocalRulePath => Application.persistentDataPath.TrimEnd('/', '\\') + "/Odin Validator/localrules.data";

		public void SaveRules()
		{
			if (!hasLoadedRules)
			{
				return;
			}
			if (projectRules != null)
			{
				foreach (SerializedRule rule in projectRules)
				{
					rule.Save();
				}
				for (int i = projectRules.Count - 1; i >= 0; i--)
				{
					if (projectRules[i].ValidatorType == null)
					{
						projectRules.RemoveAt(i);
					}
				}
			}
			EditorUtility.SetDirty(this);
			if (localRules == null || localRules.Count == 0)
			{
				if (File.Exists(LocalRulePath))
				{
					File.Delete(LocalRulePath);
				}
			}
			else
			{
				foreach (SerializedRule rule2 in localRules)
				{
					rule2.Save();
				}
				for (int i2 = localRules.Count - 1; i2 >= 0; i2--)
				{
					if (localRules[i2].ValidatorType == null)
					{
						localRules.RemoveAt(i2);
					}
				}
				byte[] bytes = Sirenix.Serialization.SerializationUtility.SerializeValue(localRules, DataFormat.Binary);
				string dir = Path.GetDirectoryName(LocalRulePath);
				Directory.CreateDirectory(dir);
				File.WriteAllBytes(LocalRulePath, bytes);
			}
			if (instantiationLookup != null)
			{
				RebuildInstantiationLookup();
			}
		}

		public void LoadRules()
		{
			if (projectRules != null)
			{
				foreach (SerializedRule rule in projectRules)
				{
					rule.Load();
				}
				for (int i = projectRules.Count - 1; i >= 0; i--)
				{
					if (projectRules[i].ValidatorType == null)
					{
						projectRules.RemoveAt(i);
					}
				}
			}
			if (File.Exists(LocalRulePath))
			{
				byte[] bytes = File.ReadAllBytes(LocalRulePath);
				try
				{
					localRules = Sirenix.Serialization.SerializationUtility.DeserializeValue<List<SerializedRule>>(bytes, DataFormat.Binary);
					if (localRules != null)
					{
						foreach (SerializedRule rule2 in localRules)
						{
							rule2.Load();
						}
						for (int i2 = localRules.Count - 1; i2 >= 0; i2--)
						{
							if (localRules[i2].ValidatorType == null)
							{
								localRules.RemoveAt(i2);
							}
						}
					}
				}
				catch (Exception ex)
				{
					Debug.LogError($"Error while deserializing local validation rules from file '{LocalRulePath}'. Local validation rules have been lost. The exception thrown was: {ex}");
					Debug.LogException(ex);
				}
			}
			else
			{
				localRules = null;
			}
			hasLoadedRules = true;
		}

		public RuleDataWrapper GetRuleDataWrapper()
		{
			if (!hasLoadedRules)
			{
				LoadRules();
			}
			if (wrapperInstance == null)
			{
				wrapperInstance = new RuleDataWrapper(this, projectRules, localRules, GetDefaultRules());
			}
			return wrapperInstance;
		}

		internal static bool IsValidatorEnabled(Type validatorType)
		{
			RuleConfig instance = GlobalConfig<RuleConfig>.Instance;
			if (instance.instantiationLookup == null)
			{
				instance.RebuildInstantiationLookup();
			}
			Dictionary<Type, InstantiationData> lookup = instance.instantiationLookup;
			if (!lookup.TryGetValue(validatorType, out var data))
			{
				Debug.LogError("Tried to instantiate rule '" + validatorType.GetNiceName() + "' which does not exist in the RuleConfig.");
				return false;
			}
			return data.Enabled;
		}

		public static IValidator InstantiateValidator(Type validatorType)
		{
			RuleConfig instance = GlobalConfig<RuleConfig>.Instance;
			if (instance.instantiationLookup == null)
			{
				instance.RebuildInstantiationLookup();
			}
			Dictionary<Type, InstantiationData> lookup = instance.instantiationLookup;
			if (!lookup.TryGetValue(validatorType, out var data))
			{
				Debug.LogError("Tried to instantiate rule '" + validatorType.GetNiceName() + "' which does not exist in the RuleConfig.");
				return null;
			}
			if (!data.Enabled)
			{
				return null;
			}
			return FastDeepCopier.DeepCopy(data.Prototype);
		}

		private void RebuildInstantiationLookup()
		{
			if (!hasLoadedRules)
			{
				LoadRules();
			}
			if (instantiationLookup == null)
			{
				instantiationLookup = new Dictionary<Type, InstantiationData>();
			}
			else
			{
				instantiationLookup.Clear();
			}
			List<SerializedRule> localRules = this.localRules;
			List<SerializedRule> projectRules = this.projectRules;
			List<SerializedRule> defaultRules = GetDefaultRules();
			foreach (SerializedRule rule in defaultRules)
			{
				instantiationLookup[rule.ValidatorType] = new InstantiationData
				{
					Enabled = rule.Enabled,
					Prototype = rule.DataOverride
				};
			}
			if (projectRules != null)
			{
				foreach (SerializedRule rule2 in projectRules)
				{
					if (!(rule2.ValidatorType == null) && instantiationLookup.TryGetValue(rule2.ValidatorType, out var data))
					{
						if (rule2.EnabledOverridden)
						{
							data.Enabled = rule2.Enabled;
						}
						if (rule2.DataOverride != null)
						{
							data.Prototype = rule2.DataOverride;
						}
					}
				}
			}
			if (localRules == null)
			{
				return;
			}
			foreach (SerializedRule rule3 in localRules)
			{
				if (!(rule3.ValidatorType == null) && instantiationLookup.TryGetValue(rule3.ValidatorType, out var data2))
				{
					if (rule3.EnabledOverridden)
					{
						data2.Enabled = rule3.Enabled;
					}
					if (rule3.DataOverride != null)
					{
						data2.Prototype = rule3.DataOverride;
					}
				}
			}
		}

		public static List<SerializedRule> GetDefaultRules()
		{
			if (defaultRules == null)
			{
				defaultRules = InstantiateDefaultRules();
			}
			return defaultRules;
		}

		public static List<SerializedRule> InstantiateDefaultRules()
		{
			List<SerializedRule> defaults = new List<SerializedRule>();
			Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
			foreach (Assembly assembly in assemblies)
			{
				foreach (RegisterValidationRuleAttribute attr in from x in assembly.GetAttributes<RegisterValidationRuleAttribute>()
					orderby x.Name descending
					select x)
				{
					if (!(attr.ValidatorType == null))
					{
						if (!typeof(IValidator).IsAssignableFrom(attr.ValidatorType))
						{
							Debug.LogError("Registered validator rule type '" + attr.ValidatorType.GetNiceFullName() + "' does not implement IRuleValidator; did you inherit from the wrong type?");
							continue;
						}
						if (attr.ValidatorType.IsGenericType)
						{
							Debug.LogError("Registered validator rule type '" + attr.ValidatorType.GetNiceFullName() + "' is generic! Generic rules are not allowed, as they cannot be configured for all variants.");
							continue;
						}
						if (attr.ValidatorType.GetConstructor(Type.EmptyTypes) == null)
						{
							Debug.LogError("Registered validator rule type '" + attr.ValidatorType.GetNiceFullName() + "' must have a public parameterless constructor.");
							continue;
						}
						IValidator instance = Activator.CreateInstance(attr.ValidatorType) as IValidator;
						SerializedRule rule = new SerializedRule(attr.ValidatorType, attr)
						{
							Enabled = attr.EnabledByDefault,
							ValidatorType = attr.ValidatorType,
							Attr = attr,
							DataOverride = instance
						};
						defaults.Add(rule);
					}
				}
			}
			return defaults;
		}

		public void SetProjectRules(List<SerializedRule> rules)
		{
			projectRules = rules;
		}

		public void SetLocalRules(List<SerializedRule> rules)
		{
			localRules = rules;
		}

		public T GetRuleData<T>(ConfigSourceType source) where T : class, IValidator
		{
			if (source != ConfigSourceType.Local && source != ConfigSourceType.Project)
			{
				throw new ArgumentException($"Source must be either Local or Project. '{source}' is invalid.");
			}
			RuleDataWrapper rulesData = GetRuleDataWrapper();
			CombinedRuleInstance ruleInstance = null;
			for (int i = 0; i < rulesData.Rules.Length; i++)
			{
				if (rulesData.Rules[i].Default.ValidatorType == typeof(T))
				{
					ruleInstance = rulesData.Rules[i];
					break;
				}
			}
			if (ruleInstance == null)
			{
				throw new Exception("Failed to find rule type '" + typeof(T).GetNiceName() + "'. Validator must be registered as a rule with the RegisterValidationRule attribute.");
			}
			return ruleInstance.GetOverrideDataOrDefaultCopy(source) as T;
		}

		public void SetAndSaveRuleData<T>(T data, ConfigSourceType source) where T : IValidator
		{
			if (source != ConfigSourceType.Local && source != ConfigSourceType.Project)
			{
				throw new ArgumentException($"Source must be either Local or Project. '{source}' is invalid.");
			}
			RuleDataWrapper rulesData = GetRuleDataWrapper();
			CombinedRuleInstance ruleInstance = null;
			for (int i = 0; i < rulesData.Rules.Length; i++)
			{
				if (rulesData.Rules[i].Default.ValidatorType == typeof(T))
				{
					ruleInstance = rulesData.Rules[i];
					break;
				}
			}
			if (ruleInstance == null)
			{
				throw new Exception("Failed to find rule type '" + typeof(T).GetNiceName() + "'. Validator must be registered as a rule with the RegisterValidationRule attribute.");
			}
			ruleInstance.SetOverrideData(source, data);
			rulesData.SaveChanges();
		}
	}
}
