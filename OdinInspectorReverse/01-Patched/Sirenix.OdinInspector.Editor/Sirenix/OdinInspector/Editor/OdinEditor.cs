using System;
using System.ComponentModel;
using System.Reflection;
using Sirenix.OdinInspector.Internal;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor
{
	[InitializeOnLoad]
	[CanEditMultipleObjects]
	public class OdinEditor : UnityEditor.Editor
	{
		private static readonly Type inspectorWindowType = typeof(EditorWindow).Assembly.GetType("UnityEditor.InspectorWindow");

		private static readonly GUIContent networkChannelLabel = new GUIContent("Network Channel", "QoS channel used for updates. Use the [NetworkSettings] class attribute to change this.");

		private static readonly GUIContent networkSendIntervalLabel = new GUIContent("Network Send Interval", "Maximum update rate in seconds. Use the [NetworkSettings] class attribute to change this, or implement GetNetworkSendInterval");

		private static Type AudioFilterGUIType;

		private static Func<MonoBehaviour, int> AudioUtil_GetCustomFilterChannelCount;

		private static Func<MonoBehaviour, bool> AudioUtil_HaveAudioCallback;

		private static Action<object, MonoBehaviour> DrawAudioFilterGUI;

		private static bool HasReflectedAudioFilter;

		private static bool Initialized = false;

		private int warmupRepaintCount;

		private float labelWidth;

		private object audioFilterGUIInstance;

		[NonSerialized]
		private PropertyTree tree;

		public static bool ForceHideMonoScriptInEditor { get; set; }

		public PropertyTree Tree
		{
			get
			{
				if (tree == null)
				{
					try
					{
						tree = PropertyTree.Create(base.serializedObject);
					}
					catch (ArgumentException)
					{
					}
				}
				return tree;
			}
		}

		/// <summary>
		/// Draws the default Odin inspector.
		/// </summary>
		public new void DrawDefaultInspector()
		{
			DrawOdinInspector();
		}

		/// <summary>
		/// Draws the default Unity inspector.
		/// </summary>
		public void DrawUnityInspector()
		{
			base.DrawDefaultInspector();
		}

		/// <summary>
		/// Not yet documented.
		/// </summary>
		public override void OnInspectorGUI()
		{
			DrawOdinInspector();
		}

		/// <summary>
		/// Draws the property tree.
		/// </summary>
		protected virtual void DrawTree()
		{
			RectOffset margins = SirenixGUIStyles.PropertyMargin.margin;
			GUILayout.BeginHorizontal(SirenixGUIStyles.None);
			GUILayout.Space(-margins.left);
			GUILayout.BeginVertical(SirenixGUIStyles.None);
			GUILayout.Space(-margins.top + 2);
			Tree.Draw();
			GUILayout.EndVertical();
			GUILayout.EndHorizontal();
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("This method will be removed, use GUIHelper.CurrentWindow.")]
		protected EditorWindow GetInspectorWindow()
		{
			Type inspectorWindowType = typeof(EditorWindow).Assembly.GetType("UnityEditor.InspectorWindow");
			try
			{
				EditorWindow window = GUIHelper.CurrentWindow;
				if (window != null && window.GetType() != inspectorWindowType)
				{
					return null;
				}
				return window;
			}
			catch
			{
			}
			return null;
		}

		protected void MockUnityGenericInspector()
		{
			if (IsMissingMonoBehaviourTarget() && MissingMonoBehaviourGUI())
			{
				return;
			}
			base.OnInspectorGUI();
			if (HasReflectedAudioFilter && base.target is MonoBehaviour && AudioUtil_HaveAudioCallback(base.target as MonoBehaviour) && AudioUtil_GetCustomFilterChannelCount(base.target as MonoBehaviour) > 0)
			{
				if (audioFilterGUIInstance == null)
				{
					audioFilterGUIInstance = Activator.CreateInstance(AudioFilterGUIType);
				}
				DrawAudioFilterGUI(audioFilterGUIInstance, base.target as MonoBehaviour);
			}
		}

		/// <summary>        
		/// Called by Unity.
		/// </summary>
		protected virtual void OnDisable()
		{
			OdinEditors.SetInactive(this);
			EnsureInitialized();
			if (tree != null)
			{
				tree.Dispose();
			}
			tree = null;
		}

		/// <summary>
		/// Called by Unity.
		/// </summary>
		protected virtual void OnEnable()
		{
			if (tree != null)
			{
				tree.Dispose();
			}
			tree = null;
			warmupRepaintCount = 0;
			EnsureInitialized();
			EditorWindow window = GUIHelper.CurrentWindow;
			if (window != null && window.GetType() == inspectorWindowType)
			{
				window.wantsMouseMove = true;
			}
			OdinEditors.SetActive(this);
		}

		private static void EnsureInitialized()
		{
			if (!Initialized)
			{
				Initialized = true;
				try
				{
					string haveAudioCallbackName = (UnityVersion.IsVersionOrGreater(5, 6) ? "HasAudioCallback" : "HaveAudioCallback");
					AudioUtil_HaveAudioCallback = (Func<MonoBehaviour, bool>)Delegate.CreateDelegate(typeof(Func<MonoBehaviour, bool>), typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.AudioUtil").GetMethod(haveAudioCallbackName, BindingFlags.Static | BindingFlags.Public));
					AudioUtil_GetCustomFilterChannelCount = (Func<MonoBehaviour, int>)Delegate.CreateDelegate(typeof(Func<MonoBehaviour, int>), typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.AudioUtil").GetMethod("GetCustomFilterChannelCount", BindingFlags.Static | BindingFlags.Public));
					AudioFilterGUIType = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.AudioFilterGUI");
					DrawAudioFilterGUI = EmitUtilities.CreateWeakInstanceMethodCaller<MonoBehaviour>(AudioFilterGUIType.GetMethod("DrawAudioFilterGUI", BindingFlags.Instance | BindingFlags.Public));
					HasReflectedAudioFilter = true;
				}
				catch (Exception)
				{
					Debug.LogWarning("The internal Unity class AudioFilterGUI has been changed; cannot properly mock a generic Unity inspector. This probably won't be very noticeable.");
				}
			}
		}

		private void DrawOdinInspector()
		{
			EnsureInitialized();
			if (Tree == null)
			{
				base.OnInspectorGUI();
				return;
			}
			if (Tree.RootPropertyCount == 0)
			{
				AssemblyCategory assemblyTypeFlag = AssemblyUtilities.GetAssemblyCategory(base.target.GetType().Assembly);
				if (assemblyTypeFlag == AssemblyCategory.UnityEngine)
				{
					MockUnityGenericInspector();
					return;
				}
			}
			if (Event.current.type == EventType.Layout)
			{
				tree.DrawMonoScriptObjectField = !ForceHideMonoScriptInEditor && tree.GetUnitySerializedObjectNoUpdate() != null && tree.TargetType != null && GlobalConfig<GeneralDrawerConfig>.Instance.ShowMonoScriptInEditor && !tree.TargetType.IsDefined(typeof(HideMonoScriptAttribute), inherit: true);
			}
			DrawTree();
			if (UnityNetworkingUtility.NetworkBehaviourType != null && UnityNetworkingUtility.NetworkBehaviourType.IsAssignableFrom(base.target.GetType()) && !base.target.GetType().IsDefined<HideNetworkBehaviourFieldsAttribute>(inherit: true))
			{
				EditorGUILayout.LabelField(networkChannelLabel, GUIHelper.TempContent(UnityNetworkingUtility.GetNetworkChannel(base.target as MonoBehaviour).ToString()));
				EditorGUILayout.LabelField(networkSendIntervalLabel, GUIHelper.TempContent(UnityNetworkingUtility.GetNetworkingInterval(base.target as MonoBehaviour).ToString()));
			}
			RepaintWarmup();
			this.RepaintIfRequested();
		}

		private void RepaintWarmup()
		{
			if (warmupRepaintCount < 1)
			{
				Repaint();
				if (Event.current.type == EventType.Repaint)
				{
					warmupRepaintCount++;
				}
			}
		}

		private bool IsMissingMonoBehaviourTarget()
		{
			if (!(base.target.GetType() == typeof(MonoBehaviour)))
			{
				return base.target.GetType() == typeof(ScriptableObject);
			}
			return true;
		}

		private bool MissingMonoBehaviourGUI()
		{
			base.serializedObject.Update();
			SerializedProperty serializedProperty = base.serializedObject.FindProperty("m_Script");
			if (serializedProperty == null)
			{
				return false;
			}
			EditorGUILayout.PropertyField(serializedProperty);
			MonoScript monoScript = serializedProperty.objectReferenceValue as MonoScript;
			bool invalid = true;
			if (monoScript != null)
			{
				invalid = false;
			}
			if (invalid)
			{
				SirenixEditorGUI.WarningMessageBox("The associated script can not be loaded.\nPlease fix any compile errors\nand assign a valid script.");
			}
			if (base.serializedObject.ApplyModifiedProperties())
			{
				ActiveEditorTracker.sharedTracker.ForceRebuild();
			}
			return true;
		}
	}
}
