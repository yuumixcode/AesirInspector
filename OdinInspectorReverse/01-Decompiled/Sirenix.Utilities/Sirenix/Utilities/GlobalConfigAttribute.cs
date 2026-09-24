using System;
using System.ComponentModel;
using UnityEngine;

namespace Sirenix.Utilities
{
	/// <summary>
	/// <para>This attribute is used by classes deriving from GlobalConfig and specifies the asset path for the generated config file.</para>
	/// </summary>
	/// <seealso cref="!:EditorGlobalConfigAttribute" />
	/// <seealso cref="T:Sirenix.Utilities.GlobalConfig`1" />
	[AttributeUsage(AttributeTargets.Class)]
	public class GlobalConfigAttribute : Attribute
	{
		private string assetPath;

		/// <summary>
		/// Gets the full asset path including Application.dataPath. Only relevant if IsInResourcesFolder is false.
		/// </summary>
		[Obsolete("It's a bit more complicated than that as it's not always possible to know the full path, so try and make due without it if you can, only using the AssetDatabase.")]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public string FullPath => Application.dataPath + "/" + AssetPath;

		/// <summary>
		/// Gets the relative asset path. Only relevant if IsInResourcesFolder is false.
		/// </summary>
		public string AssetPath => assetPath;

		internal string AssetPathWithAssetsPrefix
		{
			get
			{
				string p = AssetPath;
				if (p.StartsWith("Assets/"))
				{
					return p;
				}
				return "Assets/" + p;
			}
		}

		internal string AssetPathWithoutAssetsPrefix
		{
			get
			{
				string p = AssetPath;
				if (p.StartsWith("Assets/"))
				{
					return p.Substring("Assets/".Length);
				}
				return p;
			}
		}

		/// <summary>
		/// Gets the resources path. Only relevant if IsInResourcesFolder is true.
		/// </summary>
		public string ResourcesPath
		{
			get
			{
				if (IsInResourcesFolder)
				{
					string resourcesPath = AssetPath;
					int i = resourcesPath.LastIndexOf("/resources/", StringComparison.InvariantCultureIgnoreCase);
					if (i >= 0)
					{
						return resourcesPath.Substring(i + "/resources/".Length);
					}
				}
				return "";
			}
		}

		/// <summary>
		/// Whether the config should be associated with an asset in the project. If false, no config asset will be generated or loaded, and a new "temporary" config instance will be created for every reload. This is true by default.
		/// </summary>
		[Obsolete("This option is obsolete and will have no effect - a GlobalConfig will always have an asset generated now; use a POCO singleton or a ScriptableSingleton<T> instead. Asset-less config objects that are recreated every reload cause UnityEngine.Object leaks.", true)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool UseAsset { get; set; }

		/// <summary>
		/// Gets a value indicating whether this asset is located within a resource folder.
		/// </summary>
		public bool IsInResourcesFolder => StringExtensions.Contains(AssetPath, "/resources/", StringComparison.OrdinalIgnoreCase);

		/// <summary>
		/// Initializes a new instance of the <see cref="T:Sirenix.Utilities.GlobalConfigAttribute" /> class.
		/// </summary>
		public GlobalConfigAttribute()
			: this("Assets/Resources/Global Settings")
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:Sirenix.Utilities.GlobalConfigAttribute" /> class.
		/// </summary>
		/// <param name="assetPath">The relative asset. Remember to locate the path within a resources folder if you want the config file to be loaded at runtime without the Unity Editor.</param>
		public GlobalConfigAttribute(string assetPath)
		{
			this.assetPath = assetPath.Trim().TrimEnd('/', '\\').TrimStart('/', '\\')
				.Replace('\\', '/') + "/";
		}
	}
}
