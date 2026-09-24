using System;
using Sirenix.OdinInspector.Editor.Internal;
using Sirenix.OdinInspector.Editor.Validation;
using Sirenix.Utilities;

namespace Sirenix.OdinValidator.Editor
{
	public class CombinedRuleInstance
	{
		internal enum RuleType
		{
			None,
			Value,
			RootObject,
			Scene,
			Global,
			Unknown
		}

		public SerializedRule Project;

		public SerializedRule Local;

		public SerializedRule Default;

		private RuleType? type;

		public string Description => Default.Attr.Description;

		public string Name => Default.Attr.Name;

		public string Category => Default.Attr.Description;

		internal RuleType Type
		{
			get
			{
				if (!type.HasValue)
				{
					if (Default == null)
					{
						type = RuleType.None;
					}
					else if (typeof(SceneValidator).IsAssignableFrom(Default.ValidatorType))
					{
						type = RuleType.Scene;
					}
					else if (typeof(GlobalValidator).IsAssignableFrom(Default.ValidatorType))
					{
						type = RuleType.Global;
					}
					else if (Default.ValidatorType.InheritsFrom(typeof(RootObjectValidator<>)))
					{
						type = RuleType.RootObject;
					}
					else if (Default.ValidatorType.InheritsFrom(typeof(ValueValidator<>)))
					{
						type = RuleType.Value;
					}
					else
					{
						type = RuleType.Unknown;
					}
				}
				return type.Value;
			}
		}

		public bool Enabled
		{
			get
			{
				if (Local != null && Local.EnabledOverridden)
				{
					return Local.Enabled;
				}
				if (Project != null && Project.EnabledOverridden)
				{
					return Project.Enabled;
				}
				return Default.Enabled;
			}
		}

		public ConfigSourceType EnabledOverrideState
		{
			get
			{
				if (Local != null && Local.EnabledOverridden)
				{
					return ConfigSourceType.Local;
				}
				if (Project != null && Project.EnabledOverridden)
				{
					return ConfigSourceType.Project;
				}
				return ConfigSourceType.Default;
			}
		}

		public ConfigSourceType DataOverrideState
		{
			get
			{
				if (Local != null && Local.DataOverride != null)
				{
					return ConfigSourceType.Local;
				}
				if (Project != null && Project.DataOverride != null)
				{
					return ConfigSourceType.Project;
				}
				return ConfigSourceType.Default;
			}
		}

		public IValidator ValidatorData => Local?.DataOverride ?? Project?.DataOverride ?? Default.DataOverride;

		public void SetEnabledOverrideState(ConfigSourceType type, bool value)
		{
			switch (type)
			{
			case ConfigSourceType.Local:
				Local = Local ?? new SerializedRule(Default.ValidatorType, Default.Attr);
				Local.EnabledOverridden = true;
				Local.Enabled = value;
				break;
			case ConfigSourceType.Project:
				Project = Project ?? new SerializedRule(Default.ValidatorType, Default.Attr);
				Project.EnabledOverridden = true;
				Project.Enabled = value;
				break;
			default:
				throw new Exception("Default is immutable.");
			}
		}

		public IValidator GetOverrideDataOrDefaultCopy(ConfigSourceType type)
		{
			switch (type)
			{
			case ConfigSourceType.Local:
				if (Local == null || Local.DataOverride == null)
				{
					return FastDeepCopier.DeepCopy(Project?.DataOverride ?? Default.DataOverride);
				}
				return Local.DataOverride;
			case ConfigSourceType.Project:
				if (Project == null || Project.DataOverride == null)
				{
					return FastDeepCopier.DeepCopy(Default.DataOverride);
				}
				return Project.DataOverride;
			default:
				throw new Exception("Default is immutable.");
			}
		}

		public void SetOverrideData(ConfigSourceType type, IValidator data)
		{
			switch (type)
			{
			case ConfigSourceType.Local:
				if (data == null)
				{
					if (Local != null)
					{
						Local.DataOverride = null;
					}
					break;
				}
				if (Local == null)
				{
					Local = (Project ?? Default).CreateCopy();
				}
				Local.EnabledOverridden = true;
				Local.DataOverride = data;
				break;
			case ConfigSourceType.Project:
				if (data == null)
				{
					if (Project != null)
					{
						Project.DataOverride = null;
					}
					break;
				}
				if (Project == null)
				{
					Project = Default.CreateCopy();
				}
				Project.EnabledOverridden = true;
				Project.DataOverride = data;
				break;
			default:
				throw new Exception("Default is immutable.");
			}
		}

		public bool IsEnabledIn(ConfigSourceType configSourceType)
		{
			if (configSourceType.HasFlag(ConfigSourceType.Local) && Local != null && Local.EnabledOverridden)
			{
				return Local.Enabled;
			}
			if (configSourceType.HasFlag(ConfigSourceType.Project) && Project != null && Project.EnabledOverridden)
			{
				return Project.Enabled;
			}
			if (configSourceType.HasFlag(ConfigSourceType.Default))
			{
				return Default.Enabled;
			}
			return false;
		}

		public bool IsEnabledOverriddenIn(ConfigSourceType configSourceType)
		{
			if (configSourceType.HasFlag(ConfigSourceType.Local))
			{
				if (Local != null)
				{
					return Local.EnabledOverridden;
				}
				return false;
			}
			if (configSourceType.HasFlag(ConfigSourceType.Project))
			{
				if (Project != null)
				{
					return Project.EnabledOverridden;
				}
				return false;
			}
			throw new Exception();
		}

		public bool IsDataOverriddenIn(ConfigSourceType configSourceType)
		{
			if (configSourceType.HasFlag(ConfigSourceType.Local))
			{
				if (Local != null)
				{
					return Local.DataOverride != null;
				}
				return false;
			}
			if (configSourceType.HasFlag(ConfigSourceType.Project))
			{
				if (Project != null)
				{
					return Project.DataOverride != null;
				}
				return false;
			}
			throw new Exception();
		}

		public SerializedRule GetSerializedRule(ConfigSourceType value)
		{
			return value switch
			{
				ConfigSourceType.Local => Local, 
				ConfigSourceType.Project => Project, 
				ConfigSourceType.Default => Default, 
				_ => throw new Exception(), 
			};
		}
	}
}
