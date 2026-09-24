using System;
using System.Linq;
using System.Reflection;
using System.Text;
using Sirenix.OdinInspector.Editor.Internal;
using Sirenix.OdinInspector.Editor.Validation;
using Sirenix.Serialization;
using Sirenix.Serialization.Utilities;
using UnityEngine;

namespace Sirenix.OdinValidator.Editor
{
	[Serializable]
	public class SerializedRule
	{
		public bool Enabled;

		public bool EnabledOverridden;

		[SerializeField]
		private string Type;

		[SerializeField]
		private string Override;

		[NonSerialized]
		public Type ValidatorType;

		[NonSerialized]
		public IValidator DataOverride;

		[NonSerialized]
		public RegisterValidationRuleAttribute Attr;

		private Type isConfigurableRuleType;

		private bool? isConfigurable;

		public bool IsConfigurable
		{
			get
			{
				if (!isConfigurable.HasValue || isConfigurableRuleType != ValidatorType)
				{
					isConfigurableRuleType = ValidatorType;
					if (ValidatorType == null)
					{
						isConfigurable = false;
					}
					else
					{
						isConfigurable = ValidatorType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).Any((FieldInfo x) => SerializationPolicies.Strict.ShouldSerializeMember(x));
					}
				}
				return isConfigurable.Value;
			}
		}

		private SerializedRule()
		{
		}

		public SerializedRule(Type validatorType, RegisterValidationRuleAttribute attr)
		{
			ValidatorType = validatorType;
			Attr = attr;
		}

		public void Save()
		{
			if (ValidatorType == null)
			{
				Type = "";
			}
			else
			{
				Type = TwoWaySerializationBinder.Default.BindToName(ValidatorType);
			}
			if (DataOverride == null)
			{
				Override = "";
				return;
			}
			using Cache<SerializationContext> ctx = Cache<SerializationContext>.Claim();
			ctx.Value.StringReferenceResolver = EditorOnlyObjectAddressExternalReferenceResolver.Instance;
			byte[] bytes = SerializationUtility.SerializeValue(DataOverride, DataFormat.JSON, ctx);
			Override = Encoding.UTF8.GetString(bytes);
		}

		public void Load()
		{
			if (string.IsNullOrWhiteSpace(Type))
			{
				ValidatorType = null;
			}
			else
			{
				ValidatorType = TwoWaySerializationBinder.Default.BindToType(Type);
			}
			if (string.IsNullOrWhiteSpace(Override))
			{
				DataOverride = null;
				return;
			}
			using Cache<DeserializationContext> ctx = Cache<DeserializationContext>.Claim();
			ctx.Value.StringReferenceResolver = EditorOnlyObjectAddressExternalReferenceResolver.Instance;
			byte[] bytes = Encoding.UTF8.GetBytes(Override);
			DataOverride = SerializationUtility.DeserializeValue<IValidator>(bytes, DataFormat.JSON, ctx);
		}

		public SerializedRule CreateCopy()
		{
			Save();
			SerializedRule copy = FastDeepCopier.DeepCopy(this);
			copy.Load();
			return copy;
		}
	}
}
