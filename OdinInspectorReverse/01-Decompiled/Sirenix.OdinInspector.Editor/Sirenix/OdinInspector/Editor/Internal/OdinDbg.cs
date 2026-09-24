using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Assertions;

namespace Sirenix.OdinInspector.Editor.Internal
{
	internal static class OdinDbg
	{
		[Conditional("SIRENIX_INTERNAL")]
		public static void LogInternal(string message)
		{
			UnityEngine.Debug.Log("[Odin Internal]: " + message);
		}

		[Conditional("SIRENIX_INTERNAL")]
		public static void AssertInternal(bool condition, [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)
		{
			if (condition)
			{
				return;
			}
			string codeLine = null;
			using (StreamReader reader = new StreamReader(callerFilePath))
			{
				string line = null;
				int i;
				for (i = 0; i < callerLineNumber; i++)
				{
					line = reader.ReadLine();
					if (line == null)
					{
						break;
					}
				}
				if (line != null && i == callerLineNumber)
				{
					codeLine = line;
				}
			}
			if (codeLine != null)
			{
				int expressionStart = codeLine.IndexOf("OdinDbg", StringComparison.Ordinal);
				expressionStart = codeLine.IndexOf("AssertInternal", expressionStart, StringComparison.Ordinal);
				expressionStart = codeLine.IndexOf('(', expressionStart);
				if (expressionStart != -1)
				{
					expressionStart++;
				}
				int expressionEnd = codeLine.IndexOf(')', expressionStart);
				if (expressionStart != -1 && expressionEnd != -1 && expressionEnd > expressionStart)
				{
					codeLine = codeLine.Substring(expressionStart, expressionEnd - expressionStart);
				}
			}
			string fileName = Path.GetFileName(callerFilePath);
			string linkText = $"{fileName}:{callerLineNumber}";
			string msg = (string.IsNullOrEmpty(codeLine) ? $"<a href=\"{callerFilePath}\" line=\"{callerLineNumber}\">{linkText}</a>" : $"{codeLine} (at <a href=\"{callerFilePath}\" line=\"{callerLineNumber}\">{linkText}</a>)");
			throw new AssertionException(msg, string.Empty);
		}
	}
}
