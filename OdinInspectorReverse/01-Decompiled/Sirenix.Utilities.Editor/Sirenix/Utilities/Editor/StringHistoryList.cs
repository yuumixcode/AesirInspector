using System;
using UnityEditor;
using UnityEngine;

namespace Sirenix.Utilities.Editor
{
	internal class StringHistoryList
	{
		[Serializable]
		private class State
		{
			public string[] History;

			public int MaxLength;

			public int HistoryCount;
		}

		private readonly string editorPrefKey;

		private readonly int defaultMaxLength;

		private State state;

		private int currentControlId;

		private int currentHistoryIndex;

		private string currentBuffer;

		public int MaxLength => state.MaxLength;

		public StringHistoryList(string editorPrefKey, int defaultMaxLength)
		{
			if (defaultMaxLength < 1)
			{
				throw new ArgumentException("DefaultMaxLength must be atleast 1.", "defaultMaxLength");
			}
			this.editorPrefKey = editorPrefKey;
			this.defaultMaxLength = defaultMaxLength;
			Load();
		}

		public void Apply(int controlId, string text)
		{
			if (state.MaxLength == 0)
			{
				return;
			}
			if (string.IsNullOrEmpty(text))
			{
				currentControlId = -1;
				return;
			}
			bool textAlreadyInHistory = false;
			int existingHistoryIndex = 0;
			if (controlId == currentControlId && currentHistoryIndex != -1 && string.Equals(text, state.History[currentHistoryIndex]))
			{
				textAlreadyInHistory = true;
				existingHistoryIndex = currentHistoryIndex;
			}
			else
			{
				for (int i = 0; i < state.HistoryCount; i++)
				{
					if (string.Equals(text, state.History[i], StringComparison.Ordinal))
					{
						textAlreadyInHistory = true;
						existingHistoryIndex = i;
						break;
					}
				}
			}
			int s = existingHistoryIndex;
			if (!textAlreadyInHistory)
			{
				s = Mathf.Min(state.History.Length - 1, state.HistoryCount);
			}
			for (int i2 = s; i2 > 0; i2--)
			{
				state.History[i2] = state.History[i2 - 1];
			}
			state.History[0] = text;
			state.HistoryCount = Mathf.Min(state.HistoryCount + 1, state.History.Length);
			Save();
			currentControlId = -1;
			Save();
		}

		public void ReleaseControlId(int controlId)
		{
			if (currentControlId == controlId)
			{
				currentControlId = -1;
			}
		}

		public string GetPrevious(int controlId, string buffer)
		{
			if (state.MaxLength == 0)
			{
				return buffer;
			}
			if (currentControlId != controlId)
			{
				currentControlId = controlId;
				currentHistoryIndex = -1;
			}
			if (currentHistoryIndex == -1)
			{
				currentBuffer = buffer;
			}
			currentHistoryIndex = Mathf.Min(currentHistoryIndex + 1, state.HistoryCount - 1);
			return state.History[currentHistoryIndex];
		}

		public string GetNext(int controlId, string buffer)
		{
			if (state.MaxLength == 0)
			{
				return buffer;
			}
			if (currentControlId != controlId)
			{
				currentControlId = controlId;
				currentHistoryIndex = -1;
			}
			if (currentHistoryIndex == -1)
			{
				currentBuffer = buffer;
			}
			currentHistoryIndex = Mathf.Max(currentHistoryIndex - 1, -1);
			if (currentHistoryIndex == -1)
			{
				return currentBuffer;
			}
			return state.History[currentHistoryIndex];
		}

		public void Reset()
		{
			EditorPrefs.DeleteKey(editorPrefKey);
			state = new State
			{
				History = new string[defaultMaxLength],
				MaxLength = defaultMaxLength,
				HistoryCount = 0
			};
			Save();
		}

		public void Clear()
		{
			EditorPrefs.DeleteKey(editorPrefKey);
			state.History = new string[state.MaxLength];
			state.HistoryCount = 0;
			Save();
		}

		public void SetMaxLength(int maxLength)
		{
			if (maxLength < 0)
			{
				throw new ArgumentException("MaxLength must be atleast 0.", "maxLength");
			}
			state.MaxLength = maxLength;
			Array.Resize(ref state.History, maxLength);
			state.HistoryCount = Mathf.Min(state.HistoryCount, maxLength);
			Save();
		}

		private void Save()
		{
			string json = JsonUtility.ToJson(state);
			EditorPrefs.SetString(editorPrefKey, json);
		}

		private void Load()
		{
			try
			{
				string json = EditorPrefs.GetString(editorPrefKey, string.Empty);
				if (!string.IsNullOrEmpty(json))
				{
					State state = JsonUtility.FromJson<State>(json);
					if (state.MaxLength <= 0 || state.History == null)
					{
						Reset();
					}
					else if (state.History.Length != state.MaxLength)
					{
						Array.Resize(ref state.History, state.MaxLength);
					}
					this.state = state;
				}
				else
				{
					Reset();
				}
			}
			catch
			{
				Debug.LogError("Something went wrong loading string history list. Resetting.");
				Reset();
			}
		}
	}
}
