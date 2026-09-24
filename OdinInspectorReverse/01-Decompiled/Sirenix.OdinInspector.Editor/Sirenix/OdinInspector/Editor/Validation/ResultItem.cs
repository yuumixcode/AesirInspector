using System;
using System.Collections;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Validation
{
	public struct ResultItem : IEnumerable
	{
		public string Message;

		public ValidationResultType ResultType;

		public ResultItemMetaData[] MetaData;

		public Fix Fix;

		public Action<GenericMenu> OnContextClick;

		public Action OnSceneGUI;

		public UnityEngine.Object SelectionObject;

		public bool RichText;

		public ResultItem(string message, ValidationResultType type)
		{
			Message = message;
			ResultType = type;
			MetaData = null;
			Fix = null;
			OnContextClick = null;
			OnSceneGUI = null;
			SelectionObject = null;
			RichText = false;
		}

		public IEnumerator GetEnumerator()
		{
			return MetaData?.GetEnumerator();
		}

		public static ResultItem Error(string errorMessage)
		{
			return new ResultItem(errorMessage, ValidationResultType.Error);
		}

		public static ResultItem Warning(string errorMessage)
		{
			return new ResultItem(errorMessage, ValidationResultType.Warning);
		}
	}
}
