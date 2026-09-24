using System;
using System.Reflection;
using Sirenix.Serialization;
using Sirenix.Utilities;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Internal
{
	[Serializable]
	internal struct AttributePatch : ISerializationCallbackReceiver
	{
		[SerializeField]
		private string serializedAttributeType;

		[NonSerialized]
		public Type AttributeType;

		public AttributePatchType PatchType;

		public RefList<MemberDelta> MemberDeltas;

		public AttributePatch DeepCopy()
		{
			AttributePatch result = new AttributePatch
			{
				AttributeType = AttributeType,
				PatchType = PatchType
			};
			if (MemberDeltas == null || MemberDeltas.Length == 0)
			{
				result.MemberDeltas = RefList<MemberDelta>.Empty;
			}
			else
			{
				result.MemberDeltas = new RefList<MemberDelta>(MemberDeltas.Length);
				for (int i = 0; i < MemberDeltas.Length; i++)
				{
					result.MemberDeltas.Add(ref MemberDeltas[i]);
				}
			}
			return result;
		}

		public void OnBeforeSerialize()
		{
			if (AttributeType != null)
			{
				serializedAttributeType = TwoWaySerializationBinder.Default.BindToName(AttributeType);
			}
			if (MemberDeltas != null)
			{
				for (int i = 0; i < MemberDeltas.Length; i++)
				{
					MemberDeltas[i].SerializedMember = MemberDeltas[i].Member?.Name;
				}
			}
		}

		public void OnAfterDeserialize()
		{
			if (!string.IsNullOrEmpty(serializedAttributeType))
			{
				AttributeType = TwoWaySerializationBinder.Default.BindToType(serializedAttributeType);
			}
			if (MemberDeltas == null || AttributeType == null)
			{
				return;
			}
			for (int i = MemberDeltas.Length - 1; i >= 0; i--)
			{
				string serializedMember = MemberDeltas[i].SerializedMember;
				MemberInfo member;
				if (serializedMember == null)
				{
					member = null;
				}
				else
				{
					FieldInfo field = AttributeType.GetField(serializedMember, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
					member = ((!(field != null)) ? ((MemberInfo)AttributeType.GetProperty(serializedMember, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)) : ((MemberInfo)field));
				}
				if (member == null)
				{
					MemberDeltas.RemoveAt(i);
				}
				else
				{
					MemberDeltas[i].Member = member;
				}
			}
		}

		public void AddDeltaChange(InspectorProperty property, object instance)
		{
			if (MemberDeltas.Capacity == 0)
			{
				MemberDeltas = new RefList<MemberDelta>();
			}
			OdinDesignerBindingAttribute binding = property.GetAttribute<OdinDesignerBindingAttribute>();
			if (binding != null)
			{
				for (int i = 0; i < binding.MemberNames.Length; i++)
				{
					MemberInfo info = binding.GetBindingMemberInfo(AttributeType, i);
					bool exists = false;
					for (int j = 0; j < MemberDeltas.Length; j++)
					{
						ref MemberDelta delta = ref MemberDeltas[j];
						if (!(delta.Member.Name != info.Name))
						{
							delta.Value = info.GetMemberValue(instance);
							exists = true;
							break;
						}
					}
					if (!exists)
					{
						MemberDelta newDelta = new MemberDelta
						{
							Member = info,
							Value = info.GetMemberValue(instance)
						};
						MemberDeltas.Add(ref newDelta);
					}
				}
				return;
			}
			for (int k = 0; k < MemberDeltas.Length; k++)
			{
				ref MemberDelta delta2 = ref MemberDeltas[k];
				if (!(delta2.Member.Name != property.Info.PropertyName))
				{
					delta2.Value = property.ValueEntry.WeakSmartValue;
					return;
				}
			}
			MemberDelta newDelta2 = new MemberDelta
			{
				Member = property.Info.GetMemberInfo(),
				Value = property.ValueEntry.WeakSmartValue
			};
			MemberDeltas.Add(ref newDelta2);
		}

		public void AddDeltaChange(FieldInfo field, object value)
		{
			if (MemberDeltas.Capacity == 0)
			{
				MemberDeltas = new RefList<MemberDelta>();
			}
			for (int i = 0; i < MemberDeltas.Length; i++)
			{
				ref MemberDelta delta = ref MemberDeltas[i];
				if (!(delta.Member.Name != field.Name))
				{
					delta.Value = value;
					return;
				}
			}
			MemberDelta newDelta = new MemberDelta
			{
				Member = field,
				Value = value
			};
			MemberDeltas.Add(ref newDelta);
		}

		public void RemoveDeltas(InspectorProperty property)
		{
			OdinDesignerBindingAttribute bindings = property.GetAttribute<OdinDesignerBindingAttribute>();
			if (bindings == null)
			{
				for (int i = 0; i < MemberDeltas.Length; i++)
				{
					if (MemberDeltas[i].Member.Name == property.Name)
					{
						MemberDeltas.RemoveAt(i);
						break;
					}
				}
				return;
			}
			for (int j = 0; j < bindings.MemberNames.Length; j++)
			{
				for (int k = 0; k < MemberDeltas.Length; k++)
				{
					if (MemberDeltas[k].Member.Name == bindings.MemberNames[j])
					{
						MemberDeltas.RemoveAt(k);
						break;
					}
				}
			}
		}
	}
}
