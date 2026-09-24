using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinValidator.Editor.Validators
{
	internal class OdinShaderUtil
	{
		public static readonly bool ShaderHasErrorIsSupported;

		public static readonly Func<Shader, bool> ShaderHasError;

		static OdinShaderUtil()
		{
			MethodInfo shaderHasErrorMethod = typeof(ShaderUtil).GetMethod("ShaderHasError", new Type[1] { typeof(Shader) });
			if (shaderHasErrorMethod != null)
			{
				ShaderHasErrorIsSupported = true;
				ShaderHasError = (Func<Shader, bool>)shaderHasErrorMethod.CreateDelegate(typeof(Func<Shader, bool>));
			}
		}
	}
}
