using System;
using System.Linq;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Windows
{
	public class OdinFeedbackWindow : OdinEditorWindow
	{
		private enum State
		{
			Message,
			Waiting,
			Success
		}

		private const string MessageGroupName = "MESSAGE-GROUP";

		private const string WaitingGroupName = "WAITING-GROUP";

		private const string SuccessGroupName = "SUCCESS-GROUP";

		[NonSerialized]
		private State state;

		[NonSerialized]
		private string errorMessage;

		[NonSerialized]
		private DateTime validatorInstallDate;

		[NonSerialized]
		private DateTime inspectorInstallDate;

		[LabelWidth(80f)]
		[ShowInInspector]
		[InfoBox("@errorMessage", InfoMessageType.Error, "@string.IsNullOrEmpty(errorMessage) == false")]
		[ShowIfGroup("MESSAGE-GROUP", true, Condition = "@state == State.Message")]
		[BoxGroup("MESSAGE-GROUP/Author", true, false, 0f, LabelText = "Optional")]
		[LabelText("Name")]
		private static EditorPrefString authorName = new EditorPrefString("authorName", null);

		[HorizontalGroup("MESSAGE-GROUP/Author/Split", 0f, 0, 0, 0f)]
		[LabelWidth(80f)]
		[LabelText("Email")]
		[ShowInInspector]
		private static EditorPrefString authorEmail = new EditorPrefString("authorEmail", null);

		[LabelWidth(80f)]
		[HorizontalGroup("MESSAGE-GROUP/Author/Split", 0f, 0, 0, 0f)]
		[LabelText("Company")]
		[ShowInInspector]
		private static EditorPrefString authorCompany = new EditorPrefString("authorCompany", null);

		[LabelText("Title")]
		[LabelWidth(80f)]
		[SerializeField]
		[BoxGroup("MESSAGE-GROUP/Message", true, false, 0f, ShowLabel = false)]
		private static string messageTitle;

		[SerializeField]
		[CustomValueDrawer("DrawMessageBox")]
		[BoxGroup("MESSAGE-GROUP/Message", true, false, 0f)]
		private string messageText;

		[HideInInspector]
		[SerializeField]
		private string product;

		[HideInInspector]
		[SerializeField]
		private OdinFeedbackUtility.FeedbackMetaData[] metadata;

		[BoxGroup("MESSAGE-GROUP/Author", true, false, 0f)]
		[OnInspectorGUI]
		private static void Space()
		{
			GUILayout.Space(5f);
		}

		public static void Open(string product, params OdinFeedbackUtility.FeedbackMetaData[] metadata)
		{
			Vector2 size = new Vector2(475f, 430f);
			Rect rect = GUIHelper.GetEditorWindowRect().AlignCenter(size.x, size.y);
			OdinFeedbackWindow w = EditorWindow.GetWindowWithRect<OdinFeedbackWindow>(rect, utility: true, "Send Feedback");
			w.product = product;
			w.metadata = metadata?.Union(new OdinFeedbackUtility.FeedbackMetaData[5]
			{
				new OdinFeedbackUtility.FeedbackMetaData("Unity Version", Application.unityVersion.ToString()),
				new OdinFeedbackUtility.FeedbackMetaData("Odin Version", OdinInspectorVersion.Version.ToString()),
				new OdinFeedbackUtility.FeedbackMetaData("Odin Build Name", OdinInspectorVersion.BuildName.ToString()),
				new OdinFeedbackUtility.FeedbackMetaData("Validator Install Date", OdinInspectorVersion.Version.ToString()),
				new OdinFeedbackUtility.FeedbackMetaData("Inspector Install Date", OdinInspectorVersion.Version.ToString())
			}).ToArray();
			w.position = rect;
			w.minSize = size;
			w.maxSize = size;
			w.ShowUtility();
		}

		[BoxGroup("MESSAGE-GROUP/Message", true, false, 0f)]
		[Button(ButtonSizes.Large)]
		public void Send()
		{
			string odinVersion = OdinInspectorVersion.Version + " " + OdinInspectorVersion.BuildName;
			if (OdinInspectorVersion.IsEnterprise)
			{
				odinVersion += " enterprise";
			}
			OdinFeedbackUtility.FeedbackMessage message = new OdinFeedbackUtility.FeedbackMessage
			{
				AuthorName = authorName,
				AuthorEmail = authorEmail,
				AuthorCompany = authorCompany,
				Title = messageTitle,
				MessageText = messageText,
				Product = product,
				UnityVersion = Application.unityVersion,
				OdinVersion = odinVersion,
				MetaData = metadata
			};
			state = State.Waiting;
			OdinFeedbackUtility.SendFeedback(message, ReceiveReply);
		}

		private void ReceiveReply(OdinFeedbackUtility.FeedbackReply reply)
		{
			if (reply.Succeeded)
			{
				state = State.Success;
				errorMessage = null;
			}
			else
			{
				state = State.Message;
				errorMessage = reply.Message;
			}
		}

		private string DrawMessageBox(string value)
		{
			Rect rect = GUILayoutUtility.GetRect(0f, 0f, GUILayoutOptions.ExpandWidth().ExpandHeight());
			return EditorGUI.TextArea(rect, value);
		}

		[ShowIfGroup("WAITING-GROUP", true, Condition = "@state == State.Waiting")]
		[OnInspectorGUI]
		private void DrawWaitingForReply()
		{
			UnityShims.Rect.Ctor(out var rect, Vector2.zero, base.position.size);
			long t = (long)(EditorApplication.timeSinceStartup * 3.0);
			GUI.Label(rect.AlignCenterY(20f), "Hold on" + new string('.', (int)(t % 4)), SirenixGUIStyles.LabelCentered);
			GUIHelper.RequestRepaint();
		}

		[OnInspectorGUI]
		[ShowIfGroup("SUCCESS-GROUP", true, Condition = "@state == State.Success")]
		private void DrawSuccess()
		{
			UnityShims.Rect.Ctor(out var rect, Vector2.zero, base.position.size);
			rect = rect.AlignCenterY(40f);
			GUI.Label(rect, "Thank you for sharing your thoughts with us.", SirenixGUIStyles.LabelCentered);
			rect = rect.AlignCenterX(120f).AddY(40f);
			GUIHelper.PushColor(Color.green);
			if (SirenixEditorGUI.IconButton(rect, EditorIcons.Checkmark))
			{
				Close();
			}
			GUIHelper.PopColor();
		}
	}
}
