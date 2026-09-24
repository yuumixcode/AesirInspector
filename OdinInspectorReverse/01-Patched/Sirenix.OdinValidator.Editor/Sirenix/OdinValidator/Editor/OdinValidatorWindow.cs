using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinValidator.Editor
{
	public class OdinValidatorWindow : EditorWindow, IHasCustomMenu
	{
		[SerializeField]
		private bool disposeSessionOnDestroy;

		[SerializeField]
		private ValidationItem[] include;

		[SerializeField]
		private ValidationItem[] exclude;

		[SerializeField]
		internal ValidationProfile profile;

		[NonSerialized]
		private ValidationSessionAssetHandle handle;

		[NonSerialized]
		private ValidationSessionEditor validationSessionEditor;

		public event Action OnClose;

		private void OnEnable()
		{
			base.titleContent = new GUIContent("Odin Validator");
			base.wantsMouseMove = true;
			Init();
		}

		private void OnGUI()
		{
			if (Event.current.type == EventType.Layout)
			{
				Init();
			}
			if (validationSessionEditor == null)
			{
				GUILayout.Label("Validation session was lost.");
				return;
			}
			Rect area = base.position.ResetPosition();
			validationSessionEditor.OnGUI(area);
			this.RepaintIfRequested();
		}

		private void Init()
		{
			if (validationSessionEditor == null)
			{
				if ((bool)profile)
				{
					handle?.Dispose();
					handle = profile.ClaimSessionHandle();
					validationSessionEditor = new ValidationSessionEditor(this, handle.Session);
				}
				else if (include != null && include.Length != 0)
				{
					include = include ?? new ValidationItem[0];
					exclude = exclude ?? new ValidationItem[0];
					CreateSession(base.name, include, exclude);
				}
			}
		}

		public static ValidationSessionEditor OpenWindow(ValidationProfile profile)
		{
			foreach (ValidationSessionEditor item in ValidationSessionEditor.ActiveEditors)
			{
				if ((bool)item.Window && item.Window.profile == profile)
				{
					item.Window.Show();
					item.Window.Focus();
					return item;
				}
			}
			OdinValidatorWindow wnd = ScriptableObject.CreateInstance<OdinValidatorWindow>();
			wnd.profile = profile;
			wnd.handle = profile.ClaimSessionHandle();
			wnd.handle.Session.StartSession(GlobalConfig<GlobalValidationConfig>.Instance.WatchForChanges, GlobalConfig<GlobalValidationConfig>.Instance.ValidateInBackground);
			wnd.validationSessionEditor = new ValidationSessionEditor(wnd, wnd.handle.Session);
			wnd.Show();
			return wnd.validationSessionEditor;
		}

		public static ValidationSessionEditor OpenWindow(ValidationProfile profile, ValidationSessionAssetHandle handle)
		{
			foreach (ValidationSessionEditor item in ValidationSessionEditor.ActiveEditors)
			{
				if ((bool)item.Window && item.Window.profile == profile)
				{
					item.Window.Show();
					item.Window.Focus();
					return item;
				}
			}
			OdinValidatorWindow wnd = ScriptableObject.CreateInstance<OdinValidatorWindow>();
			wnd.profile = profile;
			wnd.handle = handle;
			wnd.handle.Session.StartSession(GlobalConfig<GlobalValidationConfig>.Instance.WatchForChanges, GlobalConfig<GlobalValidationConfig>.Instance.ValidateInBackground);
			wnd.validationSessionEditor = new ValidationSessionEditor(wnd, wnd.handle.Session);
			wnd.Show();
			return wnd.validationSessionEditor;
		}

		public static ValidationSessionEditor OpenWindow()
		{
			return OpenWindow(ValidationProfile.MainValidationProfile);
		}

		public static ValidationSessionEditor OpenWindow(string name, IList<ValidationItem> include, IList<ValidationItem> exclude, bool startValidating = true)
		{
			OdinValidatorWindow wnd = ScriptableObject.CreateInstance<OdinValidatorWindow>();
			ValidationSession session = wnd.CreateSession(name, include, exclude);
			if (startValidating)
			{
				session.ValidateEverythingNow(openClosedScenes: true, showProgressBar: true);
			}
			Rect pos = wnd.position;
			Vector2 size = pos.size;
			bool changed = false;
			if (size.x < 1200f)
			{
				size.x = 1200f;
				changed = true;
			}
			if (size.y < 600f)
			{
				size.y = 600f;
				changed = true;
			}
			if (changed)
			{
				pos.size = size;
				wnd.position = pos;
			}
			wnd.Show();
			return wnd.validationSessionEditor;
		}

		private ValidationSession CreateSession(string name, IList<ValidationItem> include, IList<ValidationItem> exclude)
		{
			SessionConfig.SerializableSessionConfigData configData = new SessionConfig.SerializableSessionConfigData(include, exclude);
			SessionConfig.SerializableSessionConfigData serializableSessionConfigData = configData;
			serializableSessionConfigData.OnSaveChanges = (Action)Delegate.Combine(serializableSessionConfigData.OnSaveChanges, (Action)delegate
			{
				this.include = configData.Include.ToArray();
				this.exclude = configData.Exclude.ToArray();
				EditorUtility.SetDirty(this);
			});
			SessionConfig sessionConfig = new SessionConfig(configData);
			ValidationSession session = new ValidationSession(name, sessionConfig);
			validationSessionEditor = new ValidationSessionEditor(this, session);
			this.include = include?.ToArray() ?? new ValidationItem[0];
			this.exclude = exclude?.ToArray() ?? new ValidationItem[0];
			disposeSessionOnDestroy = true;
			base.name = name;
			base.titleContent = new GUIContent(name);
			session.StartSession(GlobalConfig<GlobalValidationConfig>.Instance.WatchForChanges, GlobalConfig<GlobalValidationConfig>.Instance.ValidateInBackground);
			return session;
		}

		public static ValidationSessionEditor OpenWindow(ValidationSession session, bool disposeSessionOnWindowDestroy)
		{
			OdinValidatorWindow wnd = ScriptableObject.CreateInstance<OdinValidatorWindow>();
			wnd.validationSessionEditor = new ValidationSessionEditor(wnd, session);
			wnd.disposeSessionOnDestroy = disposeSessionOnWindowDestroy;
			wnd.Show();
			return wnd.validationSessionEditor;
		}

		private void OnDisable()
		{
			if (validationSessionEditor != null)
			{
				validationSessionEditor.Dispose();
				if (disposeSessionOnDestroy && !validationSessionEditor.ValidationSession.IsDisposed)
				{
					validationSessionEditor.ValidationSession.Dispose();
				}
				validationSessionEditor = null;
			}
			if (handle != null)
			{
				if (!handle.IsDisposed)
				{
					handle.Dispose();
				}
				handle = null;
			}
		}

		private void OnDestroy()
		{
			this.OnClose?.Invoke();
			OnDisable();
		}

		public void AddItemsToMenu(GenericMenu menu)
		{
			ValidationSession session = validationSessionEditor?.ValidationSession;
			if (session != null)
			{
				menu.AddItem(new GUIContent("Export results/HTML"), on: false, delegate
				{
					string report = session.ToHtml();
					SaveReport(report, "html");
				});
				menu.AddItem(new GUIContent("Export results/JSON"), on: false, delegate
				{
					string report = session.ToJson();
					SaveReport(report, "json");
				});
				menu.AddItem(new GUIContent("Export results/JSON (Pretty)"), on: false, delegate
				{
					string report = session.ToJson(prettyPrint: true);
					SaveReport(report, "json");
				});
			}
			else
			{
				menu.AddDisabledItem(new GUIContent("Export results/HTML"));
				menu.AddDisabledItem(new GUIContent("Export results/JSON"));
				menu.AddDisabledItem(new GUIContent("Export results/JSON (Pretty)"));
			}
		}

		private void SaveReport(string report, string fileExtension)
		{
			string sessionName = validationSessionEditor.ValidationSession.Name;
			string fileName = sessionName.Replace(" ", "") + "Report";
			string path = EditorUtility.SaveFilePanel("Save Validation Report", "", fileName, fileExtension ?? "");
			if (path.Length != 0)
			{
				using (StreamWriter file = File.CreateText(path))
				{
					file.Write(report);
				}
			}
		}

		internal void SwitchProfile(ValidationProfile validationProfile)
		{
			validationSessionEditor?.Dispose();
			validationSessionEditor = null;
			profile = validationProfile;
			Init();
			Repaint();
			GUIHelper.ExitGUI(removeFocusControl: true);
		}
	}
}
