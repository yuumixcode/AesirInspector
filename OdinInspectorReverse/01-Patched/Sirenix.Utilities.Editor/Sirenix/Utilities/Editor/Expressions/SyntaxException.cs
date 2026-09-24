using System;
using UnityEditor;

namespace Sirenix.Utilities.Editor.Expressions
{
	internal class SyntaxException : Exception
	{
		public readonly Tokenizer Tokenizer;

		public readonly ASTNode ASTNode;

		public SyntaxException(Tokenizer tokenizer, string message)
			: base(message)
		{
			Tokenizer = tokenizer;
		}

		public SyntaxException(ASTNode node, string message)
			: base(message)
		{
			ASTNode = node;
		}

		public string GetNiceErrorMessage(string expression, bool richText)
		{
			return Message + "\n\n" + GetCodeErrorSnippet(expression, richText);
		}

		public string GetCodeErrorSnippet(string expression, bool richText)
		{
			if (string.IsNullOrEmpty(expression))
			{
				return expression;
			}
			int start;
			int length;
			if (Tokenizer != null)
			{
				start = Tokenizer.TokenStartedStringPosition;
				length = Tokenizer.ExpressionStringPosition - start;
			}
			else
			{
				start = ASTNode.NodeStartIndex;
				length = ASTNode.NodeEndIndex - start;
			}
			string color = (EditorGUIUtility.isProSkin ? "FF534A" : "B70000FF");
			return expression.Substring(0, start) + (richText ? ("<color=#" + color + ">") : ">>>>") + expression.Substring(start, length) + (richText ? "</color>" : "<<<<") + expression.Substring(start + length);
		}
	}
}
