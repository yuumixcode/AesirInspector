using System;

namespace Sirenix.Utilities.Editor.Expressions
{
	internal abstract class ASTVisitor
	{
		public void Visit(ASTNode ast)
		{
			switch (ast.NodeType)
			{
			case NodeType.TERNARY_CONDITIONAL:
				TernaryConditional(ast);
				break;
			case NodeType.NULL_COALESCE:
				NullCoalesce(ast);
				break;
			case NodeType.LOGICAL_OR:
				LogicalOr(ast);
				break;
			case NodeType.LOGICAL_AND:
				LogicalAnd(ast);
				break;
			case NodeType.BITWISE_INCLUSIVE_OR:
				BitwiseInclusiveOr(ast);
				break;
			case NodeType.BITWISE_EXCLUSIVE_OR:
				BitwiseExclusiveOr(ast);
				break;
			case NodeType.BITWISE_AND:
				BitwiseAnd(ast);
				break;
			case NodeType.EQUALS:
				Equals(ast);
				break;
			case NodeType.NOT_EQUALS:
				NotEquals(ast);
				break;
			case NodeType.LESS_THAN:
				LessThan(ast);
				break;
			case NodeType.GREATER_THAN:
				GreaterThan(ast);
				break;
			case NodeType.GREATER_THAN_OR_EQUAL:
				GreaterThanOrEqual(ast);
				break;
			case NodeType.LESS_THAN_OR_EQUAL:
				LessThanOrEqual(ast);
				break;
			case NodeType.RELATIONAL_IS:
				RelationalIs(ast);
				break;
			case NodeType.RELATIONAL_AS:
				RelationalAs(ast);
				break;
			case NodeType.LEFT_SHIFT:
				LeftShift(ast);
				break;
			case NodeType.RIGHT_SHIFT:
				RightShift(ast);
				break;
			case NodeType.ADD:
				Add(ast);
				break;
			case NodeType.SUBTRACT:
				Subtract(ast);
				break;
			case NodeType.MULTIPLY:
				Multiply(ast);
				break;
			case NodeType.DIVIDE:
				Divide(ast);
				break;
			case NodeType.REMAINDER:
				Remainder(ast);
				break;
			case NodeType.UNARY_MINUS:
				UnaryMinus(ast);
				break;
			case NodeType.UNARY_NOT:
				UnaryNot(ast);
				break;
			case NodeType.UNARY_COMPLEMENT:
				UnaryComplement(ast);
				break;
			case NodeType.PRE_INCREMENT:
				PreIncrement(ast);
				break;
			case NodeType.PRE_DECREMENT:
				PreDecrement(ast);
				break;
			case NodeType.POST_INCREMENT:
				PostIncrement(ast);
				break;
			case NodeType.POST_DECREMENT:
				PostDecrement(ast);
				break;
			case NodeType.CONSTANT_SIGNED_INT32:
				ConstantSignedInt32(ast);
				break;
			case NodeType.CONSTANT_UNSIGNED_INT32:
				ConstantUnsignedInt32(ast);
				break;
			case NodeType.CONSTANT_SIGNED_INT64:
				ConstantSignedInt64(ast);
				break;
			case NodeType.CONSTANT_UNSIGNED_INT64:
				ConstantUnsignedInt64(ast);
				break;
			case NodeType.CONSTANT_FLOAT32:
				ConstantFloat32(ast);
				break;
			case NodeType.CONSTANT_FLOAT64:
				ConstantFloat64(ast);
				break;
			case NodeType.CONSTANT_DECIMAL:
				ConstantDecimal(ast);
				break;
			case NodeType.CONSTANT_STRING:
				ConstantString(ast);
				break;
			case NodeType.CONSTANT_CHAR:
				ConstantChar(ast);
				break;
			case NodeType.CONSTANT_BOOLEAN:
				ConstantBoolean(ast);
				break;
			case NodeType.CONSTANT_NULL:
				ConstantNull(ast);
				break;
			case NodeType.MEMBER_ACCESS:
				MemberAccess(ast);
				break;
			case NodeType.MEMBER_ACCESS_NULL_CONDITIONAL:
				MemberAccessNullConditional(ast);
				break;
			case NodeType.MEMBER_ACCESS_POINTER_DEREFERENCE:
				MemberAccessPointerDereference(ast);
				break;
			case NodeType.ELEMENT_ACCESS:
				ElementAccess(ast);
				break;
			case NodeType.ELEMENT_ACCESS_NULL_CONDITIONAL:
				ElementAccessNullConditional(ast);
				break;
			case NodeType.THIS_ACCESS:
				ThisAccess(ast);
				break;
			case NodeType.BASE_ACCESS:
				BaseAccess(ast);
				break;
			case NodeType.TYPEOF:
				TypeOf(ast);
				break;
			case NodeType.TYPEOF_VOID:
				TypeOfVoid(ast);
				break;
			case NodeType.SIZE_OF:
				SizeOf(ast);
				break;
			case NodeType.DEFAULT_TYPED:
				DefaultTyped(ast);
				break;
			case NodeType.DEFAULT_INFERRED:
				DefaultInferred(ast);
				break;
			case NodeType.INVOCATION:
				Invocation(ast);
				break;
			case NodeType.IDENTIFIER:
				Identifier(ast);
				break;
			case NodeType.NUMBERED_EXPRESSION_ARGUMENT:
				NumberedExpressionArgument(ast);
				break;
			case NodeType.NAMED_EXPRESSION_ARGUMENT:
				NamedExpressionArgument(ast);
				break;
			case NodeType.INSTANTIATE_TYPE:
				InstantiateType(ast);
				break;
			case NodeType.CHECKED:
				Checked(ast);
				break;
			case NodeType.UNCHECKED:
				Unchecked(ast);
				break;
			case NodeType.TYPE_CAST:
				TypeCast(ast);
				break;
			case NodeType.ADDRESS_OF:
				AddressOf(ast);
				break;
			case NodeType.DEREFERENCE_POINTER:
				DereferencePointer(ast);
				break;
			case NodeType.PARENTHESIZED_EXPRESSION:
				ParenthesizedExpression(ast);
				break;
			case NodeType.ARRAY_OF:
				ArrayOf(ast);
				break;
			case NodeType.PROPERTY_QUERY:
				PropertyQuery(ast);
				break;
			case NodeType.SIMPLE_ASSIGNMENT:
				SimpleAssignment(ast);
				break;
			case NodeType.NOP:
				Nop(ast);
				break;
			default:
				throw new NotImplementedException(ast.NodeType.ToString());
			}
		}

		protected abstract void TernaryConditional(ASTNode ast);

		protected abstract void NullCoalesce(ASTNode ast);

		protected abstract void LogicalOr(ASTNode ast);

		protected abstract void LogicalAnd(ASTNode ast);

		protected abstract void BitwiseInclusiveOr(ASTNode ast);

		protected abstract void BitwiseExclusiveOr(ASTNode ast);

		protected abstract void BitwiseAnd(ASTNode ast);

		protected abstract void Equals(ASTNode ast);

		protected abstract void NotEquals(ASTNode ast);

		protected abstract void LessThan(ASTNode ast);

		protected abstract void GreaterThan(ASTNode ast);

		protected abstract void GreaterThanOrEqual(ASTNode ast);

		protected abstract void LessThanOrEqual(ASTNode ast);

		protected abstract void RelationalIs(ASTNode ast);

		protected abstract void RelationalAs(ASTNode ast);

		protected abstract void LeftShift(ASTNode ast);

		protected abstract void RightShift(ASTNode ast);

		protected abstract void Add(ASTNode ast);

		protected abstract void Subtract(ASTNode ast);

		protected abstract void Multiply(ASTNode ast);

		protected abstract void Divide(ASTNode ast);

		protected abstract void Remainder(ASTNode ast);

		protected abstract void UnaryMinus(ASTNode ast);

		protected abstract void UnaryNot(ASTNode ast);

		protected abstract void UnaryComplement(ASTNode ast);

		protected abstract void PreIncrement(ASTNode ast);

		protected abstract void PreDecrement(ASTNode ast);

		protected abstract void PostIncrement(ASTNode ast);

		protected abstract void PostDecrement(ASTNode ast);

		protected abstract void ConstantSignedInt32(ASTNode ast);

		protected abstract void ConstantUnsignedInt32(ASTNode ast);

		protected abstract void ConstantSignedInt64(ASTNode ast);

		protected abstract void ConstantUnsignedInt64(ASTNode ast);

		protected abstract void ConstantFloat32(ASTNode ast);

		protected abstract void ConstantFloat64(ASTNode ast);

		protected abstract void ConstantDecimal(ASTNode ast);

		protected abstract void ConstantString(ASTNode ast);

		protected abstract void ConstantChar(ASTNode ast);

		protected abstract void ConstantBoolean(ASTNode ast);

		protected abstract void ConstantNull(ASTNode ast);

		protected abstract void MemberAccess(ASTNode ast);

		protected abstract void MemberAccessNullConditional(ASTNode ast);

		protected abstract void MemberAccessPointerDereference(ASTNode ast);

		protected abstract void ElementAccess(ASTNode ast);

		protected abstract void ElementAccessNullConditional(ASTNode ast);

		protected abstract void ThisAccess(ASTNode ast);

		protected abstract void BaseAccess(ASTNode ast);

		protected abstract void TypeOf(ASTNode ast);

		protected abstract void TypeOfVoid(ASTNode ast);

		protected abstract void SizeOf(ASTNode ast);

		protected abstract void DefaultTyped(ASTNode ast);

		protected abstract void DefaultInferred(ASTNode ast);

		protected abstract void Invocation(ASTNode ast);

		protected abstract void Identifier(ASTNode ast);

		protected abstract void InstantiateType(ASTNode ast);

		protected abstract void NumberedExpressionArgument(ASTNode ast);

		protected abstract void NamedExpressionArgument(ASTNode ast);

		protected abstract void Checked(ASTNode ast);

		protected abstract void Unchecked(ASTNode ast);

		protected abstract void TypeCast(ASTNode ast);

		protected abstract void AddressOf(ASTNode ast);

		protected abstract void DereferencePointer(ASTNode ast);

		protected abstract void ParenthesizedExpression(ASTNode ast);

		protected abstract void ArrayOf(ASTNode ast);

		protected abstract void PropertyQuery(ASTNode ast);

		protected abstract void SimpleAssignment(ASTNode ast);

		protected abstract void Nop(ASTNode ast);
	}
}
