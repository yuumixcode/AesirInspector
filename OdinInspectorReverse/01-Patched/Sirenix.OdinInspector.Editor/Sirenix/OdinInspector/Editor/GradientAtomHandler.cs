using System;
using System.Reflection;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor
{
	[AtomHandler]
	public sealed class GradientAtomHandler : BaseAtomHandler<Gradient>
	{
		private static readonly PropertyInfo ModeProperty = typeof(Gradient).GetProperty("mode", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

		public override Gradient CreateInstance()
		{
			return new Gradient();
		}

		protected override bool CompareImplementation(Gradient a, Gradient b)
		{
			if (ModeProperty != null)
			{
				Enum aMode = (Enum)ModeProperty.GetValue(a, null);
				Enum bMode = (Enum)ModeProperty.GetValue(b, null);
				if (!aMode.Equals(bMode))
				{
					return false;
				}
			}
			if (a.alphaKeys.Length != b.alphaKeys.Length || a.colorKeys.Length != b.colorKeys.Length)
			{
				return false;
			}
			for (int i = 0; i < a.alphaKeys.Length; i++)
			{
				GradientAlphaKey aKey = a.alphaKeys[i];
				GradientAlphaKey bKey = b.alphaKeys[i];
				if (aKey.alpha != bKey.alpha || aKey.time != bKey.time)
				{
					return false;
				}
			}
			for (int j = 0; j < a.colorKeys.Length; j++)
			{
				GradientColorKey aKey2 = a.colorKeys[j];
				GradientColorKey bKey2 = b.colorKeys[j];
				if (aKey2.color != bKey2.color || aKey2.time != bKey2.time)
				{
					return false;
				}
			}
			return true;
		}

		protected override void CopyImplementation(ref Gradient from, ref Gradient to)
		{
			if (ModeProperty != null)
			{
				ModeProperty.SetValue(to, ModeProperty.GetValue(from, null), null);
			}
			to.SetKeys(from.colorKeys, from.alphaKeys);
		}
	}
}
