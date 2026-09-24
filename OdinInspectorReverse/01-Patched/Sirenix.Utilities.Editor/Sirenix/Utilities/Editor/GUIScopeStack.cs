using System.Collections.Generic;
using UnityEngine;

namespace Sirenix.Utilities.Editor
{
	public class GUIScopeStack<T>
	{
		public Stack<T> InnerStack = new Stack<T>();

		private GUIFrameCounter guiState = new GUIFrameCounter();

		public int Count
		{
			get
			{
				if (guiState.Update().IsNewFrame)
				{
					InnerStack.Clear();
				}
				return InnerStack.Count;
			}
		}

		public void Push(T t)
		{
			if (guiState.Update().IsNewFrame)
			{
				InnerStack.Clear();
			}
			InnerStack.Push(t);
		}

		public T Pop()
		{
			if (Count == 0)
			{
				Debug.LogError("Pop call mismatch; no corresponding push call! Each call to Pop must always correspond to one - and only one - call to Push.");
				return default(T);
			}
			if (guiState.Update().IsNewFrame)
			{
				Debug.LogError("Pop call mismatch; no corresponding push call! Each call to Pop must always correspond to one - and only one - call to Push.");
				InnerStack.Clear();
				return default(T);
			}
			return InnerStack.Pop();
		}

		public T Peek()
		{
			if (guiState.Update().IsNewFrame)
			{
				InnerStack.Clear();
			}
			return InnerStack.Peek();
		}
	}
}
