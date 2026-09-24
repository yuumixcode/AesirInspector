using System;
using System.Reflection;
using Sirenix.Utilities;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	/// <summary>
	/// Animation curve property drawer.
	/// </summary>
	public sealed class AnimationCurveDrawer : DrawWithUnityBaseDrawer<AnimationCurve>
	{
		private AnimationCurve[] curvesLastFrame;

		private static Action clearCache;

		private static IAtomHandler<AnimationCurve> atomHandler;

		static AnimationCurveDrawer()
		{
			atomHandler = AtomHandlerLocator.GetAtomHandler<AnimationCurve>();
			MethodInfo mi = null;
			Type type = AssemblyUtilities.GetTypeByCachedFullName("UnityEditorInternal.AnimationCurvePreviewCache");
			if (type != null)
			{
				MethodInfo method = type.GetMethod("ClearCache", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
				ParameterInfo[] pars = method.GetParameters();
				if (pars != null && pars.Length == 0)
				{
					mi = method;
				}
			}
			if (mi != null)
			{
				clearCache = EmitUtilities.CreateStaticMethodCaller(mi);
			}
		}

		protected override void Initialize()
		{
			base.Initialize();
			if (clearCache != null)
			{
				clearCache();
				curvesLastFrame = new AnimationCurve[base.ValueEntry.ValueCount];
				for (int i = 0; i < base.ValueEntry.ValueCount; i++)
				{
					AnimationCurve value = base.ValueEntry.Values[i];
					curvesLastFrame[i] = atomHandler.CreateInstance();
					atomHandler.Copy(ref value, ref curvesLastFrame[i]);
				}
			}
		}

		protected override void DrawPropertyLayout(GUIContent label)
		{
			if (clearCache != null)
			{
				for (int i = 0; i < base.ValueEntry.ValueCount; i++)
				{
					if (!atomHandler.Compare(curvesLastFrame[i], base.ValueEntry.Values[i]))
					{
						clearCache();
						break;
					}
				}
			}
			base.DrawPropertyLayout(label);
			if (clearCache != null)
			{
				for (int j = 0; j < base.ValueEntry.ValueCount; j++)
				{
					AnimationCurve value = base.ValueEntry.Values[j];
					atomHandler.Copy(ref value, ref curvesLastFrame[j]);
				}
			}
		}
	}
}
