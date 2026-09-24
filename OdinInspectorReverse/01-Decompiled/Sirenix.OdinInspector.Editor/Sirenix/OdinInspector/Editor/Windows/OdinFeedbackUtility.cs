using System;
using System.Text;
using Sirenix.OdinInspector.Editor.Internal;
using UnityEngine;
using UnityEngine.Networking;

namespace Sirenix.OdinInspector.Editor.Windows
{
	public static class OdinFeedbackUtility
	{
		public struct FeedbackReply
		{
			public bool Succeeded;

			public string Message;

			public static FeedbackReply Success()
			{
				return new FeedbackReply
				{
					Succeeded = true,
					Message = null
				};
			}

			public static FeedbackReply Fail(string message)
			{
				return new FeedbackReply
				{
					Succeeded = false,
					Message = message
				};
			}
		}

		public struct FeedbackMessage
		{
			public string AuthorName;

			public string AuthorEmail;

			public string AuthorCompany;

			public string Title;

			public string MessageText;

			public string UnityVersion;

			public string OdinVersion;

			public string Product;

			public FeedbackMetaData[] MetaData;
		}

		[Serializable]
		public struct FeedbackMetaData
		{
			public string Key;

			public string Value;

			public FeedbackMetaData(string key, string value)
			{
				Key = key;
				Value = value;
			}
		}

		private const string Uri = "https://odininspector.com/api/feedback/send";

		private const int StatusCode_Offline = 0;

		private const int StatusCode_Success = 200;

		private const int StatusCode_PleaseWait = 400;

		private const int StatusCode_NotAccepted = 406;

		private const int StatusCode_FeatureDisabled = -1;

		private const int MinMessageLength = 10;

		private const int MaxMessageLength = 100000;

		public static void SendFeedback(FeedbackMessage message, Action<FeedbackReply> onCompleted)
		{
			if (string.IsNullOrEmpty(message.MessageText) || message.MessageText.Length < 10)
			{
				onCompleted(FeedbackReply.Fail("Please enter a message atleast " + 10 + " characters long."));
				return;
			}
			if (message.MessageText.Length > 100000)
			{
				onCompleted(FeedbackReply.Fail("Please reduce the message to at most " + 100000 + " characters."));
				return;
			}
			string json = JsonUtility.ToJson(message);
			UnityWebRequest request = new UnityWebRequest("https://odininspector.com/api/feedback/send", "POST", new DownloadHandlerBuffer(), new UploadHandlerRaw(Encoding.UTF8.GetBytes(json))
			{
				contentType = "application/json"
			});
			try
			{
				request.SetRequestHeader("User-Agent", "odin-inspector-unity-editor/1.0");
			}
			catch
			{
			}
			AsyncOperation op = OdinEditorWebUtility.SendWebRequest(request);
			OdinEditorWebUtility.SubscribeOnCompleted(op, delegate
			{
				try
				{
					long responseCode = request.responseCode;
					string text = request.downloadHandler.text;
					FeedbackReply obj2 = responseCode switch
					{
						0L => FeedbackReply.Fail("Server offline. Please try again later."), 
						200L => FeedbackReply.Success(), 
						400L => FeedbackReply.Fail(text), 
						-1L => FeedbackReply.Fail("We currently do not accept any more feedback."), 
						406L => FeedbackReply.Fail("Not accepted: " + text), 
						_ => FeedbackReply.Fail("We ran into an unexpected error. Please try again later."), 
					};
					onCompleted(obj2);
				}
				finally
				{
					request.Dispose();
				}
			});
		}
	}
}
