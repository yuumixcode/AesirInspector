using System;
using System.Collections.Generic;
using System.Reflection;

namespace Sirenix.OdinInspector.Editor.Internal
{
	internal struct BakedAttributePatch
	{
		public Type AttributeType;

		public AttributePatchType PatchType;

		public RefList<BakedMemberDelta> BakedMemberDeltas;

		private static readonly Dictionary<MemberInfo, RefList<BakedMemberDelta>.IndexRef> MemberDeltaHandles = new Dictionary<MemberInfo, RefList<BakedMemberDelta>.IndexRef>(16);

		public BakedAttributePatch(ref AttributePatch patch)
		{
			AttributeType = patch.AttributeType;
			PatchType = patch.PatchType;
			if (patch.MemberDeltas == RefList<MemberDelta>.Empty)
			{
				BakedMemberDeltas = RefList<BakedMemberDelta>.Empty;
				return;
			}
			BakedMemberDeltas = new RefList<BakedMemberDelta>(patch.MemberDeltas.Length);
			for (int i = 0; i < patch.MemberDeltas.Length; i++)
			{
				BakedMemberDelta bakedDelta = new BakedMemberDelta(ref patch.MemberDeltas[i]);
				BakedMemberDeltas.Add(ref bakedDelta);
			}
		}

		public void Merge(ref AttributePatch patch)
		{
			AttributePatchType patchType = patch.PatchType;
			if ((uint)(patchType - 1) <= 1u)
			{
				PatchType = patch.PatchType;
			}
			MemberDeltaHandles.Clear();
			for (int i = 0; i < BakedMemberDeltas.Length; i++)
			{
				ref BakedMemberDelta delta = ref BakedMemberDeltas[i];
				MemberDeltaHandles[delta.Member] = BakedMemberDeltas.GetIndexRef(i);
			}
			if (BakedMemberDeltas == RefList<BakedMemberDelta>.Empty)
			{
				BakedMemberDeltas = new RefList<BakedMemberDelta>(patch.MemberDeltas.Length);
			}
			for (int j = 0; j < patch.MemberDeltas.Length; j++)
			{
				ref MemberDelta delta2 = ref patch.MemberDeltas[j];
				if (!MemberDeltaHandles.TryGetValue(delta2.Member, out var deltaHandle))
				{
					BakedMemberDelta newBakedDelta = new BakedMemberDelta(ref delta2);
					BakedMemberDeltas.Add(ref newBakedDelta);
				}
				else
				{
					deltaHandle.Ref.Value = delta2.Value;
				}
			}
		}
	}
}
