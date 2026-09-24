using System;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Sirenix.Utilities
{
	/// <summary>
	/// <para>
	/// A GlobalConfig singleton, automatically created and saved as a ScriptableObject in the project at the specified path.
	/// This only happens if the UnityEditor is present. If it's not, a non-persistent ScriptableObject is created at run-time.
	/// </para>
	/// <para>
	/// Remember to locate the path within a resources folder if you want the config file to be loaded at runtime without the Unity editor being present.
	/// </para>
	/// <para>
	/// The asset path is specified by defining a <see cref="T:Sirenix.Utilities.GlobalConfigAttribute" />. If no attribute is defined it will be saved in the root assets folder.
	/// </para>
	/// </summary>
	/// <example>
	/// <code>
	/// [GlobalConfig("Assets/Resources/MyConfigFiles/")]
	/// public class MyGlobalConfig : GlobalConfig&lt;MyGlobalConfig&gt;
	/// {
	///     public int MyGlobalVariable;
	/// }
	///
	/// void SomeMethod()
	/// {
	///     int value = MyGlobalConfig.Instance.MyGlobalVariable;
	/// }
	/// </code>
	/// </example>
	public abstract class GlobalConfig<T> : ScriptableObject, IGlobalConfigEvents where T : GlobalConfig<T>, new()
	{
		private static GlobalConfigAttribute configAttribute;

		private static T instance;

		public static GlobalConfigAttribute ConfigAttribute
		{
			get
			{
				if (configAttribute == null)
				{
					configAttribute = typeof(T).GetCustomAttribute<GlobalConfigAttribute>();
					if (configAttribute == null)
					{
						configAttribute = new GlobalConfigAttribute(typeof(T).GetNiceName());
					}
				}
				return configAttribute;
			}
		}

		/// <summary>
		/// Gets a value indicating whether this instance has instance loaded.
		/// </summary>
		public static bool HasInstanceLoaded => GlobalConfigUtility<T>.HasInstanceLoaded;

		/// <summary>
		/// Gets the singleton instance.
		/// </summary>
		public static T Instance => GlobalConfigUtility<T>.GetInstance(ConfigAttribute.AssetPath);

		/// <summary>
		/// Tries to load the singleton instance.
		/// </summary>
		public static void LoadInstanceIfAssetExists()
		{
			GlobalConfigUtility<T>.LoadInstanceIfAssetExists(ConfigAttribute.AssetPath);
		}

		/// <summary>
		/// Opens the config in a editor window. This is currently only used internally by the Sirenix.OdinInspector.Editor assembly.
		/// </summary>
		public void OpenInEditor()
		{
			Type windowType = null;
			try
			{
				Assembly editorAssembly = null;
				Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
				foreach (Assembly assembly in assemblies)
				{
					if (assembly.GetName().Name == "Sirenix.OdinInspector.Editor")
					{
						editorAssembly = assembly;
						break;
					}
				}
				if (editorAssembly != null)
				{
					windowType = editorAssembly.GetType("Sirenix.OdinInspector.Editor.SirenixPreferencesWindow");
				}
			}
			catch
			{
			}
			if (windowType != null)
			{
				(from x in windowType.GetMethods()
					where x.Name == "OpenWindow" && x.GetParameters().Length == 1
					select x).First().Invoke(null, new object[1] { this });
			}
			else
			{
				Debug.LogError("Failed to open window, could not find Sirenix.OdinInspector.Editor.SirenixPreferencesWindow");
			}
		}

		protected virtual void OnConfigInstanceFirstAccessed()
		{
		}

		protected virtual void OnConfigAutoCreated()
		{
		}

		void IGlobalConfigEvents.OnConfigAutoCreated()
		{
			OnConfigAutoCreated();
		}

		void IGlobalConfigEvents.OnConfigInstanceFirstAccessed()
		{
			OnConfigInstanceFirstAccessed();
		}
	}
}
