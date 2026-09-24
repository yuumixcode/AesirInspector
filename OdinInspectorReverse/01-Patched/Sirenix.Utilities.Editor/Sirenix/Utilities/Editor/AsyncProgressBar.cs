using System;
using System.Reflection;
using UnityEditor;

namespace Sirenix.Utilities.Editor
{
	/// <summary>
	/// Not yet documented.
	/// </summary>
	public static class AsyncProgressBar
	{
		private static readonly Type AsyncProgressBarType = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.AsyncProgressBar");

		private static readonly Func<float> ProgressGetter = DeepReflection.CreateValueGetter<float>(AsyncProgressBarType, "progress");

		private static readonly Func<string> ProgressInfoGetter = DeepReflection.CreateValueGetter<string>(AsyncProgressBarType, "progressInfo");

		private static readonly Func<bool> IsShowingGetter = DeepReflection.CreateValueGetter<bool>(AsyncProgressBarType, "isShowing");

		private static readonly Action<string, float> DisplayCaller = (Action<string, float>)Delegate.CreateDelegate(typeof(Action<string, float>), AsyncProgressBarType.GetMethod("Display", BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy));

		private static readonly Action ClearCaller = (Action)Delegate.CreateDelegate(typeof(Action), AsyncProgressBarType.GetMethod("Clear", BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy));

		/// <summary>
		/// Not yet documented.
		/// </summary>
		public static float Progress => ProgressGetter();

		/// <summary>
		/// Not yet documented.
		/// </summary>
		public static string ProgressInfo => ProgressInfoGetter();

		/// <summary>
		/// Not yet documented.
		/// </summary>
		public static bool IsShowing => IsShowingGetter();

		/// <summary>
		/// Not yet documented.
		/// </summary>
		public static void Display(string progressInfo, float progress)
		{
			DisplayCaller(progressInfo, progress);
		}

		/// <summary>
		/// Not yet documented.
		/// </summary>
		public static void Clear()
		{
			ClearCaller();
		}
	}
}
