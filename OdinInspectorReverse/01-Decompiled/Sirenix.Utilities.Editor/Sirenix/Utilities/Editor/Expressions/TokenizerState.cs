namespace Sirenix.Utilities.Editor.Expressions
{
	public struct TokenizerState
	{
		public Token NextToken;

		public int TokenStringPosition;

		public int TokenStartedStringPosition;

		public string IdentifierValue;

		public int ExpressionArgumentNumber;

		public string TokenString;

		public char CharacterConstantValue;

		public float Float32ConstantValue;

		public double Float64ConstantValue;

		public long IntegerConstantValue;

		public ulong UnsignedIntegerConstantValue;

		public decimal DecimalConstantValue;

		public string StringConstantValue;
	}
}
