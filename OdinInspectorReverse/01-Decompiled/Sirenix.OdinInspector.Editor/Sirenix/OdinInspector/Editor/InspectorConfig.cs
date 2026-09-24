using System;
using System.IO;
using System.Reflection;
using Sirenix.Serialization;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Rendering;

namespace Sirenix.OdinInspector.Editor
{
	/// <summary>
	/// <para>
	/// Tell Odin which types should be drawn or should not be drawn by Odin.
	/// </para>
	/// <para>
	/// You can modify which types should be drawn by Odin in the Preferences window found in 'Tools -&gt; Odin Inspector -&gt; Preferences -&gt; Editor Types',
	/// or by locating the configuration file stored as a serialized object in the Sirenix folder under 'Odin Inspector/Config/Editor/InspectorConfig'.
	/// </para>
	/// </summary>
	[SirenixEditorConfig]
	public class InspectorConfig : GlobalConfig<InspectorConfig>, ISerializationCallbackReceiver
	{
		[Space(5f)]
		[SerializeField]
		[HorizontalGroup(0f, 0, 0, 0f)]
		[OnValueChanged("UpdateAndRefreshInspector", false)]
		[ToggleLeft]
		[LabelText(" Enable Odin In Inspector")]
		[Tooltip("Whether Odin is enabled in the inspector or not.")]
		private bool enableOdinInInspector = true;

		[SerializeField]
		[HideInInspector]
		private InspectorDefaultEditors defaultEditorBehaviour = InspectorDefaultEditors.UserTypes | InspectorDefaultEditors.PluginTypes | InspectorDefaultEditors.OtherTypes;

		[HideInInspector]
		[SerializeField]
		private bool processMouseMoveInInspector = true;

		[DisableContextMenu(true, true)]
		[SerializeField]
		private InspectorTypeDrawingConfig drawingConfig = new InspectorTypeDrawingConfig();

		private static bool IsHeadlessMode => SystemInfo.graphicsDeviceType == GraphicsDeviceType.Null;

		/// <summary>
		/// Whether Odin is enabled in the inspector or not.
		/// </summary>
		public bool EnableOdinInInspector
		{
			get
			{
				return enableOdinInInspector;
			}
			set
			{
				if (value != enableOdinInInspector)
				{
					enableOdinInInspector = value;
					UpdateAndRefreshInspector();
				}
			}
		}

		/// <summary>
		/// InspectorDefaultEditors is a bitmask used to tell which types should have an Odin Editor generated.
		/// </summary>
		public InspectorDefaultEditors DefaultEditorBehaviour
		{
			get
			{
				return defaultEditorBehaviour;
			}
			set
			{
				defaultEditorBehaviour = value;
			}
		}

		/// <summary>
		/// The config which contains configuration data for which types Odin should draw in the inspector.
		/// </summary>
		public InspectorTypeDrawingConfig DrawingConfig => drawingConfig;

		internal bool ProcessMouseMoveInInspector
		{
			get
			{
				return processMouseMoveInInspector;
			}
			set
			{
				processMouseMoveInInspector = value;
			}
		}

		[InitializeOnLoadMethod]
		private static void RemoveObsoleteGeneratedOdinEditorsDLL()
		{
			UnityEditorEventUtility.EditorApplication_delayCall += delegate
			{
				UnityEditorEventUtility.EditorApplication_delayCall += delegate
				{
					if (!EditorPrefs.HasKey("PREVENT_SIRENIX_FILE_GENERATION"))
					{
						string text = SirenixAssetPaths.SirenixAssembliesPath + "Editor";
						string text2 = text + "/GeneratedOdinEditors.dll";
						if (File.Exists(text2))
						{
							AssetDatabase.DeleteAsset(text2);
							if (File.Exists(text2 + ".mdb"))
							{
								AssetDatabase.DeleteAsset(text2 + ".mdb");
							}
							AssetDatabase.Refresh();
						}
					}
				};
			};
		}

		private void SuppressMissingEditorTypeErrorsMessage()
		{
			if (UnityVersion.Major == 2017 && UnityVersion.Minor == 1)
			{
				SirenixEditorGUI.ErrorMessageBox("Suppressing these error messages may cause crashes on Unity 2017.1 (see Unity issue 920772). A fix is being backported from 2017.2 - meanwhile, you may want to disable this option, and live with the constant error messages about missing editor types.");
			}
		}

		void ISerializationCallbackReceiver.OnBeforeSerialize()
		{
		}

		void ISerializationCallbackReceiver.OnAfterDeserialize()
		{
			drawingConfig.UpdateCaches();
		}

		/// <summary>
		/// Updates Unity with the current Odin editor configuration.
		/// </summary>
		public void UpdateOdinEditors()
		{
			if (IsHeadlessMode || InternalEditorUtility.inBatchMode)
			{
				return;
			}
			CustomEditorUtility.ResetCustomEditors();
			if (enableOdinInInspector)
			{
				TypeDrawerPair[] editors = InspectorTypeDrawingConfigDrawer.GetEditors();
				for (int i = 0; i < editors.Length; i++)
				{
					TypeDrawerPair typeDrawerPair = editors[i];
					Type drawnType = TwoWaySerializationBinder.Default.BindToType(typeDrawerPair.DrawnTypeName);
					Type editorType = TwoWaySerializationBinder.Default.BindToType(typeDrawerPair.EditorTypeName);
					if (!(drawnType == null) && !(editorType == null))
					{
						CustomEditorUtility.SetCustomEditor(drawnType, editorType, isFallbackEditor: false, isEditorForChildClasses: false);
					}
				}
			}
			EditorApplication.delayCall = (EditorApplication.CallbackFunction)Delegate.Combine(EditorApplication.delayCall, (EditorApplication.CallbackFunction)delegate
			{
				Type type = typeof(EditorWindow).Assembly.GetType("UnityEditor.InspectorWindow");
				Type type2 = typeof(EditorWindow).Assembly.GetType("UnityEditor.ActiveEditorTracker");
				if (type != null && type2 != null)
				{
					MethodInfo method = type.GetMethod("CreateTracker", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
					FieldInfo field = type.GetField("m_Tracker", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
					MethodInfo method2 = type2.GetMethod("ForceRebuild", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
					if (method != null && field != null && method2 != null)
					{
						UnityEngine.Object[] array = Resources.FindObjectsOfTypeAll(type);
						UnityEngine.Object[] array2 = array;
						foreach (UnityEngine.Object obj in array2)
						{
							try
							{
								method.Invoke(obj, null);
								object value = field.GetValue(obj);
								method2.Invoke(value, null);
							}
							catch (Exception exception)
							{
								Debug.LogException(exception);
							}
						}
					}
				}
			});
		}

		[HorizontalGroup(100f, 0, 0, 0f)]
		[Button("Update Editors", 22, DisplayParameters = false)]
		public void UpdateAndRefreshInspector()
		{
			UpdateOdinEditors();
			UnityEngine.Object[] objects = Selection.objects;
			Selection.objects = null;
			EditorApplication.delayCall = (EditorApplication.CallbackFunction)Delegate.Combine(EditorApplication.delayCall, (EditorApplication.CallbackFunction)delegate
			{
				Selection.objects = objects;
			});
		}

		[OnInspectorGUI]
		private void BottomSpace()
		{
			GUILayout.Space(10f);
		}
	}
}
