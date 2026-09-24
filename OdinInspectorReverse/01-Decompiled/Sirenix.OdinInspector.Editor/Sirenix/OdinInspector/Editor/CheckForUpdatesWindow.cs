using System;
using System.IO;
using System.Net;
using System.Threading;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor
{
	public class CheckForUpdatesWindow : EditorWindow
	{
		[Serializable]
		private struct SirenixVersion
		{
			[Serializable]
			public struct BetaVersion
			{
				public bool available;

				public string version;

				public int releaseDateTicks;

				public string patchNotesUrl;
			}

			public string name;

			public string version;

			public string patchNotesUrl;

			public int releaseDateTicks;

			public BetaVersion beta;

			public string downloadUrl;
		}

		private enum FetchVersionResult : byte
		{
			None,
			Fetching,
			Failed,
			Success
		}

		private const string CheckForUpdatesDailyPref = "CheckForUpdates.CheckDaily";

		private const string CheckBetaVersionsEnablePref = "CheckForUpdates.EnableBeta";

		private const string LastCheckDatePref = "CheckForUpdates.LastCheckDate";

		private static bool? checkDaily;

		private static bool? checkForBeta;

		private static FetchVersionResult state;

		private static SirenixVersion latestVersion;

		internal static bool CheckDailyEnabled
		{
			get
			{
				if (!checkDaily.HasValue)
				{
					checkDaily = EditorPrefs.GetBool("CheckForUpdates.CheckDaily", defaultValue: false);
				}
				return checkDaily.Value;
			}
			set
			{
				if (!checkDaily.HasValue || checkDaily.Value != value)
				{
					checkDaily = value;
					EditorPrefs.SetBool("CheckForUpdates.CheckDaily", value);
				}
			}
		}

		internal static bool IncludeBetasEnabled
		{
			get
			{
				if (!checkForBeta.HasValue)
				{
					checkForBeta = EditorPrefs.GetBool("CheckForUpdates.EnableBeta", defaultValue: false);
				}
				return checkForBeta.Value;
			}
			set
			{
				if (!checkForBeta.HasValue || checkForBeta.Value != value)
				{
					checkForBeta = value;
					EditorPrefs.SetBool("CheckForUpdates.EnableBeta", value);
				}
			}
		}

		[InitializeOnLoadMethod]
		private static void DailyCheckForUpdates()
		{
			if (CheckDailyEnabled)
			{
				EditorApplication.update = (EditorApplication.CallbackFunction)Delegate.Remove(EditorApplication.update, new EditorApplication.CallbackFunction(WaitForVersionFetch));
				EditorApplication.update = (EditorApplication.CallbackFunction)Delegate.Combine(EditorApplication.update, new EditorApplication.CallbackFunction(WaitForVersionFetch));
				int lastDays = EditorPrefs.GetInt("CheckForUpdates.LastCheckDate", 0);
				int nowDays = (int)(DateTime.Now.Date.Ticks / 864000000000L);
				if (lastDays < nowDays)
				{
					EditorPrefs.SetInt("CheckForUpdates.LastCheckDate", nowDays);
					StartTryRefreshLatestOdinVersion();
				}
			}
		}

		private static void WaitForVersionFetch()
		{
			if (state == FetchVersionResult.Success || state == FetchVersionResult.Failed)
			{
				EditorApplication.update = (EditorApplication.CallbackFunction)Delegate.Remove(EditorApplication.update, new EditorApplication.CallbackFunction(WaitForVersionFetch));
				if (state == FetchVersionResult.Success && (IsVersionHigher(latestVersion.version, OdinInspectorVersion.Version) || (IncludeBetasEnabled && IsVersionHigher(latestVersion.beta.version, OdinInspectorVersion.Version))))
				{
					OpenWindow();
				}
			}
		}

		public static void OpenWindow()
		{
			CheckForUpdatesWindow w = EditorWindow.GetWindow<CheckForUpdatesWindow>("Odin Updates");
			w.position = GUIHelper.GetEditorWindowRect().AlignCenter(350f, 150f);
			w.minSize = new Vector2(350f, 150f);
			w.maxSize = new Vector2(350f, 150f);
		}

		private void OnEnable()
		{
			if (state == FetchVersionResult.None && state != FetchVersionResult.Fetching)
			{
				StartTryRefreshLatestOdinVersion();
			}
		}

		private void OnGUI()
		{
			Rect rect = new Rect(0f, 0f, base.position.width, base.position.height).Padding(4f);
			GUI.enabled = state != FetchVersionResult.Fetching;
			if (SirenixEditorGUI.IconButton(rect.AlignRight(20f).AlignTop(20f), EditorIcons.Refresh, "Refresh"))
			{
				StartTryRefreshLatestOdinVersion();
			}
			GUI.enabled = true;
			Rect content = rect.AlignTop(rect.height - 48f);
			GUI.DrawTexture(content.AlignLeft(64f), EditorIcons.OdinInspectorLogo, ScaleMode.ScaleToFit);
			content = content.AddXMin(64f);
			if (state == FetchVersionResult.Fetching)
			{
				Rect r = content.AlignCenterY(20f);
				GUI.DrawTexture(r.AlignLeft(20f).SubY(2f), EditorIcons.Refresh.Raw);
				GUI.Label(r.AddXMin(24f), GUIHelper.TempContent("Getting latest version..."));
				GUIHelper.RequestRepaint();
			}
			else if (state == FetchVersionResult.Failed)
			{
				GUI.Label(content, GUIHelper.TempContent("Failed to fetch latest Odin Inspector version.\nPlease try again later."), SirenixGUIStyles.MultiLineCenteredLabel);
			}
			else if (state == FetchVersionResult.Success)
			{
				Rect r2 = content.AlignCenterY(20f);
				if (latestVersion.beta.available && IsVersionHigher(latestVersion.beta.version, OdinInspectorVersion.Version))
				{
					r2.y += 15f;
					GUI.DrawTexture(r2.AlignLeft(20f).SubY(2f), EditorIcons.Bell.Raw);
					GUI.Label(r2.AddXMin(24f), "Beta " + latestVersion.beta.version + " is available for download!");
					r2.y -= 25f;
				}
				if (IsVersionHigher(latestVersion.version, OdinInspectorVersion.Version))
				{
					GUI.DrawTexture(r2.AlignLeft(20f).SubY(2f), EditorIcons.Bell.Raw);
					GUI.Label(r2.AddXMin(24f), "Patch " + latestVersion.version + " is available for download!");
				}
				else
				{
					GUI.color = Color.green;
					GUI.DrawTexture(r2.AlignLeft(20f).SubY(2f), EditorIcons.Checkmark.Raw);
					GUI.color = Color.white;
					GUI.Label(r2.AddXMin(24f), "Latest stable Odin version " + OdinInspectorVersion.Version + " installed");
				}
			}
			GUI.enabled = state == FetchVersionResult.Success;
			Rect buttons = rect.AlignBottom(44f);
			if (GUI.Button(buttons.AlignTop(20f).Split(0, 2), GUIHelper.TempContent("See patch notes")))
			{
				Application.OpenURL(latestVersion.patchNotesUrl);
			}
			if (GUI.Button(buttons.AlignTop(20f).Split(1, 2), GUIHelper.TempContent("Download here")))
			{
				Application.OpenURL(latestVersion.downloadUrl);
			}
			GUI.enabled = true;
			CheckDailyEnabled = EditorGUI.ToggleLeft(buttons.AlignBottom(20f).Split(0, 2), "Daily check for updates", CheckDailyEnabled);
			IncludeBetasEnabled = EditorGUI.ToggleLeft(buttons.AlignBottom(20f).Split(1, 2), "Include betas", IncludeBetasEnabled);
			this.RepaintIfRequested();
		}

		private static string GetUpdateAvailableMessage(SirenixVersion version)
		{
			if (version.beta.available)
			{
				if (IsVersionHigher(version.beta.version, OdinInspectorVersion.Version))
				{
					return "Beta version " + version.version + " is available for download at " + version.downloadUrl;
				}
			}
			else if (IsVersionHigher(version.version, OdinInspectorVersion.Version))
			{
				return "Update " + version.version + " is available for download at " + version.downloadUrl;
			}
			return "Looks like you're up to date with the latest version. You get a cookie for that.";
		}

		private static bool IsVersionHigher(string a, string b)
		{
			try
			{
				Version aVersion = new Version(a);
				Version bVersion = new Version(b);
				return Math.Max(aVersion.Major, 0) > Math.Max(bVersion.Major, 0) || Math.Max(aVersion.Minor, 0) > Math.Max(bVersion.Minor, 0) || Math.Max(aVersion.Build, 0) > Math.Max(bVersion.Build, 0) || Math.Max(aVersion.Revision, 0) > Math.Max(bVersion.Revision, 0);
			}
			catch
			{
				return false;
			}
		}

		private static void StartTryRefreshLatestOdinVersion()
		{
			if (state != FetchVersionResult.Fetching)
			{
				state = FetchVersionResult.Fetching;
				Thread thread = new Thread((ThreadStart)delegate
				{
					TryRefreshLatestOdinVersion();
				});
				thread.IsBackground = true;
				thread.Start();
			}
		}

		private static void TryRefreshLatestOdinVersion()
		{
			try
			{
				HttpWebRequest request = (HttpWebRequest)WebRequest.Create("http://odininspector.com/latest-version/odin-inspector");
				request.Method = "GET";
				using HttpWebResponse response = (HttpWebResponse)request.GetResponse();
				using Stream stream = response.GetResponseStream();
				using StreamReader reader = new StreamReader(stream);
				if (response.ContentType == "application/json; charset=utf-8")
				{
					latestVersion = JsonUtility.FromJson<SirenixVersion>(reader.ReadToEnd());
					state = FetchVersionResult.Success;
				}
				else
				{
					latestVersion = default(SirenixVersion);
					state = FetchVersionResult.Failed;
				}
			}
			catch
			{
				latestVersion = default(SirenixVersion);
				state = FetchVersionResult.Failed;
			}
		}
	}
}
