using System;
using System.Collections.Generic;
using System.Configuration.Assemblies;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Threading;
using Sirenix.Utilities;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor
{
	/// <summary>
	/// Provides utilities for emitting ScriptableObject and MonoBehaviour-derived types with specific property names and types, and providing instances of <see cref="T:UnityEditor.SerializedProperty" /> with those names and types.
	/// </summary>
	public static class UnityPropertyEmitter
	{
		/// <summary>
		/// A handle for a set of emitted Unity objects. When disposed (or collected by the GC) this handle will queue the emitted object instances for destruction.
		/// </summary>
		public class Handle : IDisposable
		{
			/// <summary>
			/// The unity property to represent.
			/// </summary>
			public readonly SerializedProperty UnityProperty;

			/// <summary>
			/// The Unity objects to represent.
			/// </summary>
			public readonly UnityEngine.Object[] Objects;

			private int disposed;

			/// <summary>
			/// Initializes a new instance of the <see cref="T:Sirenix.OdinInspector.Editor.UnityPropertyEmitter.Handle" /> class.
			/// </summary>
			/// <param name="unityProperty">The unity property to represent.</param>
			/// <param name="objects">The objects to represent.</param>
			public Handle(SerializedProperty unityProperty, UnityEngine.Object[] objects)
			{
				UnityProperty = unityProperty;
				Objects = objects;
			}

			/// <summary>
			/// Finalizes an instance of the <see cref="T:Sirenix.OdinInspector.Editor.UnityPropertyEmitter.Handle" /> class.
			/// </summary>
			~Handle()
			{
				Dispose();
			}

			public void Dispose()
			{
				if (Interlocked.Increment(ref disposed) == 1)
				{
					lock (MarkedForDestruction_LOCK)
					{
						MarkedForDestruction.AddRange(Objects);
					}
				}
			}
		}

		public const string EMIT_ASSEMBLY_NAME = "Sirenix.OdinInspector.EmittedUnityProperties";

		public const string HOST_GO_NAME = "ODIN_EMIT_HOST_GO_ac922281-4f8a-4e1b-8a45-65af1a8350b3";

		public const HideFlags HOST_GO_HIDE_FLAGS = HideFlags.HideInHierarchy | HideFlags.DontSaveInEditor | HideFlags.NotEditable | HideFlags.DontSaveInBuild;

		private static AssemblyBuilder emittedAssembly;

		private static ModuleBuilder emittedModule;

		private static readonly Dictionary<Type, Type> PreCreatedScriptableObjectTypes;

		private static readonly DoubleLookupDictionary<string, Type, Type> MonoBehaviourTypeCache;

		private static readonly DoubleLookupDictionary<string, Type, Type> ScriptableObjectTypeCache;

		private static GameObject hostGO;

		private static readonly object MarkedForDestruction_LOCK;

		private static readonly List<UnityEngine.Object> MarkedForDestruction;

		private static GameObject HostGO
		{
			get
			{
				if (hostGO == null)
				{
					hostGO = GameObject.Find("ODIN_EMIT_HOST_GO_ac922281-4f8a-4e1b-8a45-65af1a8350b3");
					if (hostGO == null)
					{
						hostGO = new GameObject("ODIN_EMIT_HOST_GO_ac922281-4f8a-4e1b-8a45-65af1a8350b3");
						hostGO.hideFlags = HideFlags.HideInHierarchy | HideFlags.DontSaveInEditor | HideFlags.NotEditable | HideFlags.DontSaveInBuild;
					}
				}
				return hostGO;
			}
		}

		static UnityPropertyEmitter()
		{
			PreCreatedScriptableObjectTypes = new Dictionary<Type, Type>
			{
				{
					typeof(AnimationCurve),
					typeof(EmittedAnimationCurveContainer)
				},
				{
					typeof(Gradient),
					typeof(EmittedGradientContainer)
				}
			};
			MonoBehaviourTypeCache = new DoubleLookupDictionary<string, Type, Type>();
			ScriptableObjectTypeCache = new DoubleLookupDictionary<string, Type, Type>();
			MarkedForDestruction_LOCK = new object();
			MarkedForDestruction = new List<UnityEngine.Object>();
			EditorApplication.update = (EditorApplication.CallbackFunction)Delegate.Combine(EditorApplication.update, new EditorApplication.CallbackFunction(DestroyMarkedObjects));
		}

		private static void DestroyMarkedObjects()
		{
			lock (MarkedForDestruction_LOCK)
			{
				for (int i = 0; i < MarkedForDestruction.Count; i++)
				{
					UnityEngine.Object obj = MarkedForDestruction[i];
					if (obj != null)
					{
						UnityEngine.Object.DestroyImmediate(obj);
					}
				}
				MarkedForDestruction.Clear();
			}
		}

		/// <summary>
		/// Creates an emitted MonoBehaviour-based <see cref="T:UnityEditor.SerializedProperty" />.
		/// </summary>
		/// <param name="fieldName">Name of the field to emit.</param>
		/// <param name="valueType">Type of the value to create a property for.</param>
		/// <param name="targetCount">The target count of the tree to create a property for.</param>
		/// <param name="gameObject">The game object that the MonoBehaviour of the property is located on.</param>
		/// <exception cref="T:System.ArgumentNullException">
		/// fieldName is null
		/// or
		/// valueType is null
		/// </exception>
		/// <exception cref="T:System.ArgumentException">Target count must be equal to or higher than 1.</exception>
		public static Handle CreateEmittedMonoBehaviourProperty(string fieldName, Type valueType, int targetCount, ref GameObject gameObject)
		{
			DestroyMarkedObjects();
			if (fieldName == null)
			{
				throw new ArgumentNullException("fieldName");
			}
			if (valueType == null)
			{
				throw new ArgumentNullException("valueType");
			}
			if (targetCount < 1)
			{
				throw new ArgumentException("Target count must be equal to or higher than 1.");
			}
			if (gameObject == null)
			{
				gameObject = HostGO;
			}
			if (!MonoBehaviourTypeCache.TryGetInnerValue(fieldName, valueType, out var resultType))
			{
				resultType = EmitMonoBehaviourType(fieldName, valueType);
				MonoBehaviourTypeCache.AddInner(fieldName, valueType, resultType);
			}
			MonoBehaviour[] targets = new MonoBehaviour[targetCount];
			for (int i = 0; i < targetCount; i++)
			{
				targets[i] = (MonoBehaviour)gameObject.AddComponent(resultType);
				targets[i].hideFlags = gameObject.hideFlags;
			}
			UnityEngine.Object[] objs = targets;
			SerializedObject serializedObject = new SerializedObject(objs);
			SerializedProperty unityProperty = serializedObject.FindProperty(fieldName);
			objs = targets;
			return new Handle(unityProperty, objs);
		}

		/// <summary>
		/// Creates an emitted ScriptableObject-based <see cref="T:UnityEditor.SerializedProperty" />.
		/// </summary>
		/// <param name="fieldName">Name of the field to emit.</param>
		/// <param name="valueType">Type of the value to create a property for.</param>
		/// <param name="targetCount">The target count of the tree to create a property for.</param>
		/// <exception cref="T:System.ArgumentNullException">
		/// fieldName is null
		/// or
		/// valueType is null
		/// </exception>
		/// <exception cref="T:System.ArgumentException">Target count must be equal to or higher than 1.</exception>
		public static SerializedProperty CreateEmittedScriptableObjectProperty(string fieldName, Type valueType, int targetCount)
		{
			DestroyMarkedObjects();
			if (fieldName == null)
			{
				throw new ArgumentNullException("fieldName");
			}
			if (valueType == null)
			{
				throw new ArgumentNullException("valueType");
			}
			if (targetCount < 1)
			{
				throw new ArgumentException("Target count must be equal to or higher than 1.");
			}
			if (PreCreatedScriptableObjectTypes.TryGetValue(valueType, out var resultType))
			{
				fieldName = "value";
			}
			else if (!ScriptableObjectTypeCache.TryGetInnerValue(fieldName, valueType, out resultType))
			{
				resultType = EmitScriptableObjectType(fieldName, valueType);
				ScriptableObjectTypeCache.AddInner(fieldName, valueType, resultType);
			}
			ScriptableObject[] targets = new ScriptableObject[targetCount];
			for (int i = 0; i < targetCount; i++)
			{
				targets[i] = ScriptableObject.CreateInstance(resultType);
			}
			UnityEngine.Object[] objs = targets;
			SerializedObject serializedObject = new SerializedObject(objs);
			return serializedObject.FindProperty(fieldName);
		}

		private static void EnsureEmitModule()
		{
			if (emittedAssembly == null)
			{
				FixUnityAboutWindowBeforeEmit.Fix();
				AssemblyName assemblyName = new AssemblyName("Sirenix.OdinInspector.EmittedUnityProperties");
				assemblyName.CultureInfo = CultureInfo.InvariantCulture;
				assemblyName.Flags = AssemblyNameFlags.None;
				assemblyName.ProcessorArchitecture = ProcessorArchitecture.MSIL;
				assemblyName.VersionCompatibility = AssemblyVersionCompatibility.SameDomain;
				emittedAssembly = AppDomain.CurrentDomain.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
				emittedModule = emittedAssembly.DefineDynamicModule("Sirenix.OdinInspector.EmittedUnityProperties", true);
			}
		}

		private static Type EmitMonoBehaviourType(string memberName, Type valueType)
		{
			string typeName = "Sirenix.OdinInspector.EmittedUnityProperties.EmittedMBProperty_" + memberName + "_" + valueType.GetCompilableNiceFullName();
			Type inheritedType = typeof(EmittedMonoBehaviour<>).MakeGenericType(valueType);
			return EmitType(memberName, typeName, inheritedType, valueType);
		}

		private static Type EmitScriptableObjectType(string memberName, Type valueType)
		{
			string typeName = "Sirenix.OdinInspector.EmittedUnityProperties.EmittedSOProperty_" + memberName + "_" + valueType.GetCompilableNiceFullName();
			Type inheritedType = typeof(EmittedScriptableObject<>).MakeGenericType(valueType);
			return EmitType(memberName, typeName, inheritedType, valueType);
		}

		private static Type EmitType(string memberName, string typeName, Type inheritedType, Type valueType)
		{
			EnsureEmitModule();
			MethodInfo abstractSetValueMethod = inheritedType.GetMethod("SetValue");
			MethodInfo abstractGetValueMethod = inheritedType.GetMethod("GetValue");
			MethodInfo abstractPropBackingFieldGet = inheritedType.GetProperty("BackingFieldInfo").GetGetMethod();
			TypeBuilder type = emittedModule.DefineType(typeName, TypeAttributes.Sealed, inheritedType);
			type.SetCustomAttribute(new CustomAttributeBuilder(typeof(CompilerGeneratedAttribute).GetConstructor(Type.EmptyTypes), new object[0]));
			FieldBuilder valueField = type.DefineField(memberName, valueType, FieldAttributes.Public);
			valueField.SetCustomAttribute(new CustomAttributeBuilder(typeof(SerializeField).GetConstructor(Type.EmptyTypes), new object[0]));
			FieldBuilder backingFieldInfoField = type.DefineField("backingFieldInfo", typeof(FieldInfo), FieldAttributes.Private | FieldAttributes.Static);
			MethodBuilder setValueMethod = type.DefineMethod(abstractSetValueMethod.Name, MethodAttributes.Public | MethodAttributes.Virtual, abstractSetValueMethod.ReturnType, (from n in abstractSetValueMethod.GetParameters()
				select n.ParameterType).ToArray());
			ILGenerator gen1 = setValueMethod.GetILGenerator();
			gen1.Emit(OpCodes.Ldarg_0);
			gen1.Emit(OpCodes.Ldarg_1);
			gen1.Emit(OpCodes.Stfld, valueField);
			gen1.Emit(OpCodes.Ret);
			type.DefineMethodOverride(setValueMethod, abstractSetValueMethod);
			MethodBuilder getValueMethod = type.DefineMethod(abstractGetValueMethod.Name, MethodAttributes.Public | MethodAttributes.Virtual, abstractGetValueMethod.ReturnType, (from n in abstractGetValueMethod.GetParameters()
				select n.ParameterType).ToArray());
			ILGenerator gen2 = getValueMethod.GetILGenerator();
			gen2.Emit(OpCodes.Ldarg_0);
			gen2.Emit(OpCodes.Ldfld, valueField);
			gen2.Emit(OpCodes.Ret);
			type.DefineMethodOverride(getValueMethod, abstractGetValueMethod);
			MethodBuilder propBackingFieldGet = type.DefineMethod(abstractPropBackingFieldGet.Name, MethodAttributes.Public | MethodAttributes.Virtual, abstractPropBackingFieldGet.ReturnType, (from n in abstractPropBackingFieldGet.GetParameters()
				select n.ParameterType).ToArray());
			ILGenerator gen3 = propBackingFieldGet.GetILGenerator();
			gen3.Emit(OpCodes.Ldsfld, backingFieldInfoField);
			gen3.Emit(OpCodes.Ret);
			type.DefineMethodOverride(propBackingFieldGet, abstractPropBackingFieldGet);
			Type result = type.CreateType();
			FieldInfo backingValueField = result.GetField(memberName, BindingFlags.Instance | BindingFlags.Public);
			FieldInfo backingFieldInfo = result.GetField("backingFieldInfo", BindingFlags.Static | BindingFlags.NonPublic);
			backingFieldInfo.SetValue(null, backingValueField);
			return result;
		}
	}
}
