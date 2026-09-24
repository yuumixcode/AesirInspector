using System;
using System.Collections.Generic;
using System.Linq;

namespace Sirenix.Utilities.Editor.Expressions
{
	internal class ASTParser
	{
		private Tokenizer tokenizer;

		private Token current;

		private Stack<Token[]> expectedStops = new Stack<Token[]>();

		private static readonly HashSet<NodeType> DefaultToRightPrecedenceOperators = new HashSet<NodeType>
		{
			NodeType.LOGICAL_OR,
			NodeType.LOGICAL_AND
		};

		private static readonly HashSet<NodeType> ManualPrecedenceOperators = new HashSet<NodeType>
		{
			NodeType.LOGICAL_OR,
			NodeType.LOGICAL_AND,
			NodeType.ADD,
			NodeType.SUBTRACT,
			NodeType.MULTIPLY,
			NodeType.DIVIDE,
			NodeType.REMAINDER,
			NodeType.BITWISE_INCLUSIVE_OR,
			NodeType.BITWISE_EXCLUSIVE_OR,
			NodeType.BITWISE_AND,
			NodeType.EQUALS,
			NodeType.NOT_EQUALS,
			NodeType.GREATER_THAN,
			NodeType.GREATER_THAN_OR_EQUAL,
			NodeType.LESS_THAN,
			NodeType.LESS_THAN_OR_EQUAL,
			NodeType.LEFT_SHIFT,
			NodeType.RIGHT_SHIFT,
			NodeType.NULL_COALESCE,
			NodeType.TERNARY_CONDITIONAL
		};

		private static readonly Dictionary<NodeType, int> OperatorPrecedences = new Dictionary<NodeType, int>
		{
			{
				NodeType.MEMBER_ACCESS_NULL_CONDITIONAL,
				-1
			},
			{
				NodeType.ELEMENT_ACCESS_NULL_CONDITIONAL,
				-1
			},
			{
				NodeType.MEMBER_ACCESS,
				0
			},
			{
				NodeType.INVOCATION,
				0
			},
			{
				NodeType.ELEMENT_ACCESS,
				0
			},
			{
				NodeType.POST_INCREMENT,
				0
			},
			{
				NodeType.POST_DECREMENT,
				0
			},
			{
				NodeType.INSTANTIATE_TYPE,
				0
			},
			{
				NodeType.TYPEOF,
				0
			},
			{
				NodeType.CHECKED,
				0
			},
			{
				NodeType.UNCHECKED,
				0
			},
			{
				NodeType.DEFAULT_TYPED,
				0
			},
			{
				NodeType.DEFAULT_INFERRED,
				0
			},
			{
				NodeType.SIZE_OF,
				0
			},
			{
				NodeType.MEMBER_ACCESS_POINTER_DEREFERENCE,
				0
			},
			{
				NodeType.UNARY_MINUS,
				1
			},
			{
				NodeType.UNARY_COMPLEMENT,
				1
			},
			{
				NodeType.UNARY_NOT,
				1
			},
			{
				NodeType.PRE_INCREMENT,
				1
			},
			{
				NodeType.PRE_DECREMENT,
				1
			},
			{
				NodeType.TYPE_CAST,
				1
			},
			{
				NodeType.ADDRESS_OF,
				1
			},
			{
				NodeType.DEREFERENCE_POINTER,
				1
			},
			{
				NodeType.MULTIPLY,
				2
			},
			{
				NodeType.DIVIDE,
				2
			},
			{
				NodeType.REMAINDER,
				2
			},
			{
				NodeType.ADD,
				3
			},
			{
				NodeType.SUBTRACT,
				3
			},
			{
				NodeType.LEFT_SHIFT,
				4
			},
			{
				NodeType.RIGHT_SHIFT,
				4
			},
			{
				NodeType.LESS_THAN,
				5
			},
			{
				NodeType.GREATER_THAN,
				5
			},
			{
				NodeType.LESS_THAN_OR_EQUAL,
				5
			},
			{
				NodeType.GREATER_THAN_OR_EQUAL,
				5
			},
			{
				NodeType.RELATIONAL_IS,
				5
			},
			{
				NodeType.RELATIONAL_AS,
				5
			},
			{
				NodeType.EQUALS,
				6
			},
			{
				NodeType.NOT_EQUALS,
				6
			},
			{
				NodeType.BITWISE_AND,
				7
			},
			{
				NodeType.BITWISE_EXCLUSIVE_OR,
				8
			},
			{
				NodeType.BITWISE_INCLUSIVE_OR,
				9
			},
			{
				NodeType.LOGICAL_AND,
				10
			},
			{
				NodeType.LOGICAL_OR,
				11
			},
			{
				NodeType.NULL_COALESCE,
				12
			},
			{
				NodeType.TERNARY_CONDITIONAL,
				13
			}
		};

		public ASTParser(Tokenizer tokenizer)
		{
			this.tokenizer = tokenizer;
		}

		private Token GetNextToken()
		{
			current = tokenizer.GetNextToken();
			return current;
		}

		private bool Peek(Token token)
		{
			return current == token;
		}

		private bool Accept(Token token)
		{
			if (current == token)
			{
				current = tokenizer.GetNextToken();
				return true;
			}
			return false;
		}

		private void Expect(Token token)
		{
			if (current == token)
			{
				current = tokenizer.GetNextToken();
				return;
			}
			throw new SyntaxException(tokenizer, "Expected token " + token.ToTokenString());
		}

		private bool ExpectButDontEat(Token token)
		{
			if (current == token)
			{
				return true;
			}
			throw new SyntaxException(tokenizer, "Expected token " + token.ToTokenString());
		}

		public ASTNode Parse()
		{
			expectedStops.Clear();
			GetNextToken();
			if (Peek(Token.EOF))
			{
				return new ASTNode
				{
					NodeType = NodeType.NOP
				};
			}
			ExpressionStartTermAndContinuations(out var astRoot);
			if (current != Token.EOF)
			{
				throw new SyntaxException(tokenizer, "Unexpected token " + current.ToTokenString());
			}
			if (astRoot == null)
			{
				throw new InvalidOperationException("Root ASTNode is null when parsing succeeded! What on earth just happened?!");
			}
			return astRoot;
		}

		private bool ExpressionStartTermAndContinuations(out ASTNode ast, bool allowFailure = false)
		{
			Token[] stops = null;
			if (expectedStops.Count > 0)
			{
				stops = expectedStops.Peek();
			}
			if (!ExpressionStartTerm(out ast))
			{
				if (allowFailure)
				{
					return false;
				}
				throw new SyntaxException(tokenizer, "Unexpected token " + current.ToTokenString());
			}
			do
			{
				if (stops != null && stops.Contains(current))
				{
					return true;
				}
			}
			while (Continuation(ref ast));
			if (stops == null)
			{
				return true;
			}
			if (allowFailure)
			{
				return false;
			}
			throw new SyntaxException(tokenizer, "Expected one of the following tokens: " + string.Join(", ", stops.Select((Token n) => n.ToTokenString()).ToArray()));
		}

		private void ExpressionStartTermAndContinuationsUntil(Token stopToken, out ASTNode ast)
		{
			if (!ExpressionStartTerm(out ast))
			{
				throw new SyntaxException(tokenizer, "Unexpected token " + current.ToTokenString());
			}
			do
			{
				if (current == stopToken)
				{
					return;
				}
			}
			while (Continuation(ref ast));
			throw new SyntaxException(tokenizer, "Expected token " + stopToken.ToTokenString());
		}

		private bool Continuation(ref ASTNode ast)
		{
			if (TypeCast(ref ast))
			{
				return true;
			}
			if (MemberAccess(ref ast, allowNullConditional: true))
			{
				return true;
			}
			if (Invocation(ref ast))
			{
				return true;
			}
			if (ElementAccess(ref ast))
			{
				return true;
			}
			if (Operator(ref ast))
			{
				return true;
			}
			if (SimpleAssignment(ref ast))
			{
				return true;
			}
			if (AssignmentOperator(ref ast))
			{
				return true;
			}
			return false;
		}

		private bool ExpressionStartTerm(out ASTNode ast)
		{
			if (Constant(out ast))
			{
				return true;
			}
			if (PropertyQuery(out ast))
			{
				return true;
			}
			if (Identifier(out ast))
			{
				return true;
			}
			if (TypeOf(out ast))
			{
				return true;
			}
			if (SizeOf(out ast))
			{
				return true;
			}
			if (PreOperator(out ast))
			{
				return true;
			}
			if (ParenthesisedExpression(out ast))
			{
				return true;
			}
			if (This(out ast))
			{
				return true;
			}
			if (Base(out ast))
			{
				return true;
			}
			if (Default(out ast))
			{
				return true;
			}
			if (InstantiateType(out ast))
			{
				return true;
			}
			if (ExpressionArgument(out ast))
			{
				return true;
			}
			return false;
		}

		private bool Constant(out ASTNode ast)
		{
			bool wasConstant = true;
			switch (current)
			{
			case Token.SIGNED_INT32:
				ast = new ASTNode
				{
					NodeStartIndex = tokenizer.TokenStartedStringPosition,
					NodeType = NodeType.CONSTANT_SIGNED_INT32,
					NodeValue = (int)tokenizer.SignedIntegerConstantValue
				};
				break;
			case Token.UNSIGNED_INT32:
				ast = new ASTNode
				{
					NodeStartIndex = tokenizer.TokenStartedStringPosition,
					NodeType = NodeType.CONSTANT_UNSIGNED_INT32,
					NodeValue = (uint)tokenizer.UnsignedIntegerConstantValue
				};
				break;
			case Token.SIGNED_INT64:
				ast = new ASTNode
				{
					NodeStartIndex = tokenizer.TokenStartedStringPosition,
					NodeType = NodeType.CONSTANT_SIGNED_INT64,
					NodeValue = tokenizer.SignedIntegerConstantValue
				};
				break;
			case Token.UNSIGNED_INT64:
				ast = new ASTNode
				{
					NodeType = NodeType.CONSTANT_UNSIGNED_INT64,
					NodeValue = tokenizer.UnsignedIntegerConstantValue
				};
				break;
			case Token.FLOAT32:
				ast = new ASTNode
				{
					NodeType = NodeType.CONSTANT_FLOAT32,
					NodeValue = tokenizer.Float32ConstantValue
				};
				break;
			case Token.FLOAT64:
				ast = new ASTNode
				{
					NodeType = NodeType.CONSTANT_FLOAT64,
					NodeValue = tokenizer.Float64ConstantValue
				};
				break;
			case Token.DECIMAL:
				ast = new ASTNode
				{
					NodeType = NodeType.CONSTANT_DECIMAL,
					NodeValue = tokenizer.DecimalConstantValue
				};
				break;
			case Token.CHAR_CONSTANT:
				ast = new ASTNode
				{
					NodeType = NodeType.CONSTANT_CHAR,
					NodeValue = tokenizer.CharacterConstantValue
				};
				break;
			case Token.STRING_CONSTANT:
				ast = new ASTNode
				{
					NodeType = NodeType.CONSTANT_STRING,
					NodeValue = tokenizer.StringConstantValue
				};
				break;
			case Token.TRUE:
				ast = new ASTNode
				{
					NodeType = NodeType.CONSTANT_BOOLEAN,
					NodeValue = true
				};
				break;
			case Token.FALSE:
				ast = new ASTNode
				{
					NodeType = NodeType.CONSTANT_BOOLEAN,
					NodeValue = false
				};
				break;
			case Token.NULL:
				ast = new ASTNode
				{
					NodeType = NodeType.CONSTANT_NULL
				};
				break;
			default:
				wasConstant = false;
				ast = null;
				break;
			}
			if (wasConstant)
			{
				ast.NodeStartIndex = tokenizer.TokenStartedStringPosition;
				ast.NodeEndIndex = tokenizer.ExpressionStringPosition;
				GetNextToken();
			}
			return wasConstant;
		}

		private bool PropertyQuery(out ASTNode ast)
		{
			if (Peek(Token.PROPERTY_QUERY))
			{
				ast = new ASTNode
				{
					NodeType = NodeType.PROPERTY_QUERY,
					NodeValue = tokenizer.IdentifierValue,
					NodeStartIndex = tokenizer.TokenStartedStringPosition,
					NodeEndIndex = tokenizer.ExpressionStringPosition
				};
				GetNextToken();
				return true;
			}
			ast = null;
			return false;
		}

		private bool Identifier(out ASTNode ast)
		{
			if (Peek(Token.IDENTIFIER))
			{
				ast = new ASTNode
				{
					NodeType = NodeType.IDENTIFIER,
					NodeValue = tokenizer.IdentifierValue,
					NodeStartIndex = tokenizer.TokenStartedStringPosition,
					NodeEndIndex = tokenizer.ExpressionStringPosition
				};
				GetNextToken();
				TokenizerState state = tokenizer.GetState();
				Token currentBackup = current;
				if (Accept(Token.LESS_THAN))
				{
					if (!Identifier(out var genericArg))
					{
						tokenizer.SetState(state);
						current = currentBackup;
						ast.Children.Clear();
					}
					else
					{
						while (MemberAccess(ref genericArg, allowNullConditional: false))
						{
						}
						ast.Children[0] = genericArg;
						while (true)
						{
							if (Accept(Token.COMMA))
							{
								if (!Identifier(out genericArg))
								{
									tokenizer.SetState(state);
									current = currentBackup;
									ast.Children.Clear();
									break;
								}
								while (MemberAccess(ref genericArg, allowNullConditional: false))
								{
								}
								ast.Children[ast.Children.Count] = genericArg;
								continue;
							}
							if (!Accept(Token.GREATER_THAN))
							{
								tokenizer.SetState(state);
								current = currentBackup;
								ast.Children.Clear();
							}
							break;
						}
					}
				}
				ast.NodeEndIndex = tokenizer.ExpressionStringPosition;
				while (Peek(Token.LEFT_BRACKET))
				{
					TokenizerState state2 = tokenizer.GetState();
					Token currentBackup2 = current;
					Expect(Token.LEFT_BRACKET);
					if (!Peek(Token.COMMA) && !Peek(Token.RIGHT_BRACKET))
					{
						tokenizer.SetState(state2);
						current = currentBackup2;
						break;
					}
					int rank = 1;
					while (Accept(Token.COMMA))
					{
						rank++;
					}
					ASTNode temp = ast;
					ast = new ASTNode
					{
						NodeType = NodeType.ARRAY_OF,
						NodeValue = rank,
						NodeStartIndex = temp.NodeStartIndex,
						NodeEndIndex = tokenizer.ExpressionStringPosition
					};
					ast.Children[0] = temp;
					Expect(Token.RIGHT_BRACKET);
				}
				return true;
			}
			ast = null;
			return false;
		}

		private bool TypeOf(out ASTNode ast)
		{
			int start = tokenizer.TokenStartedStringPosition;
			if (Accept(Token.TYPEOF))
			{
				Expect(Token.LEFT_PARENTHESIS);
				if (Accept(Token.VOID))
				{
					ast = new ASTNode
					{
						NodeType = NodeType.TYPEOF_VOID,
						NodeStartIndex = start
					};
					Expect(Token.RIGHT_PARENTHESIS);
					ast.NodeEndIndex = tokenizer.ExpressionStringPosition;
					return true;
				}
				if (!TypeName(out var type))
				{
					throw new SyntaxException(tokenizer, "Expected type identifier after 'typeof('");
				}
				Expect(Token.RIGHT_PARENTHESIS);
				ast = new ASTNode
				{
					NodeType = NodeType.TYPEOF,
					NodeStartIndex = start,
					NodeEndIndex = tokenizer.ExpressionStringPosition
				};
				ast.Children[0] = type;
				return true;
			}
			ast = null;
			return false;
		}

		private bool SizeOf(out ASTNode ast)
		{
			int start = tokenizer.TokenStartedStringPosition;
			if (Accept(Token.SIZEOF))
			{
				Expect(Token.LEFT_PARENTHESIS);
				if (!TypeName(out var type))
				{
					throw new SyntaxException(tokenizer, "Expected type identifier after 'sizeof('");
				}
				Expect(Token.RIGHT_PARENTHESIS);
				ast = new ASTNode
				{
					NodeType = NodeType.SIZE_OF,
					NodeStartIndex = start,
					NodeEndIndex = tokenizer.ExpressionStringPosition
				};
				ast.Children[0] = type;
				return true;
			}
			ast = null;
			return false;
		}

		private bool TypeName(out ASTNode ast)
		{
			if (Identifier(out ast))
			{
				while (MemberAccess(ref ast, allowNullConditional: false))
				{
				}
				return true;
			}
			return false;
		}

		private bool PreOperator(out ASTNode ast)
		{
			int start = tokenizer.TokenStartedStringPosition;
			ASTNode operand;
			ASTNode @operator;
			switch (current)
			{
			case Token.PLUS:
				ExpressionStartTermAndContinuations(out ast);
				return true;
			case Token.MINUS:
				GetNextToken();
				ExpressionStartTermAndContinuations(out operand);
				@operator = new ASTNode
				{
					NodeType = NodeType.UNARY_MINUS
				};
				break;
			case Token.NOT:
				GetNextToken();
				ExpressionStartTermAndContinuations(out operand);
				@operator = new ASTNode
				{
					NodeType = NodeType.UNARY_NOT
				};
				break;
			case Token.COMPLEMENT:
				GetNextToken();
				ExpressionStartTermAndContinuations(out operand);
				@operator = new ASTNode
				{
					NodeType = NodeType.UNARY_COMPLEMENT
				};
				break;
			case Token.INCREMENT:
				GetNextToken();
				ExpressionStartTermAndContinuations(out operand);
				@operator = new ASTNode
				{
					NodeType = NodeType.PRE_INCREMENT
				};
				break;
			case Token.DECREMENT:
				GetNextToken();
				ExpressionStartTermAndContinuations(out operand);
				@operator = new ASTNode
				{
					NodeType = NodeType.PRE_DECREMENT
				};
				break;
			case Token.BITWISE_AND:
				GetNextToken();
				ExpressionStartTermAndContinuations(out operand);
				@operator = new ASTNode
				{
					NodeType = NodeType.ADDRESS_OF
				};
				break;
			default:
				ast = null;
				return false;
			}
			@operator.NodeStartIndex = start;
			@operator.NodeEndIndex = tokenizer.ExpressionStringPosition;
			ast = GetUnaryOperatorPrecedence(@operator, operand);
			return true;
		}

		private bool ParenthesisedExpression(out ASTNode ast)
		{
			int start = tokenizer.TokenStartedStringPosition;
			if (Accept(Token.LEFT_PARENTHESIS))
			{
				expectedStops.Push(new Token[1] { Token.RIGHT_PARENTHESIS });
				ExpressionStartTermAndContinuations(out ast);
				expectedStops.Pop();
				ASTNode expr = ast;
				ast = new ASTNode
				{
					NodeType = NodeType.PARENTHESIZED_EXPRESSION,
					NodeStartIndex = start,
					NodeEndIndex = tokenizer.ExpressionStringPosition
				};
				ast.Children[0] = expr;
				Expect(Token.RIGHT_PARENTHESIS);
				return true;
			}
			ast = null;
			return false;
		}

		private bool This(out ASTNode ast)
		{
			int start = tokenizer.TokenStartedStringPosition;
			if (Accept(Token.THIS))
			{
				ast = new ASTNode
				{
					NodeType = NodeType.THIS_ACCESS,
					NodeStartIndex = start,
					NodeEndIndex = tokenizer.ExpressionStringPosition
				};
				return true;
			}
			ast = null;
			return false;
		}

		private bool Base(out ASTNode ast)
		{
			int start = tokenizer.TokenStartedStringPosition;
			if (Accept(Token.BASE))
			{
				ast = new ASTNode
				{
					NodeType = NodeType.BASE_ACCESS,
					NodeStartIndex = start,
					NodeEndIndex = tokenizer.ExpressionStringPosition
				};
				return true;
			}
			ast = null;
			return false;
		}

		private bool Default(out ASTNode ast)
		{
			int start = tokenizer.TokenStartedStringPosition;
			if (Accept(Token.DEFAULT))
			{
				if (Accept(Token.LEFT_PARENTHESIS))
				{
					if (!TypeName(out var type))
					{
						throw new SyntaxException(tokenizer, "Expected type identifier after 'default('");
					}
					Expect(Token.RIGHT_PARENTHESIS);
					ast = new ASTNode
					{
						NodeType = NodeType.DEFAULT_TYPED
					};
					ast.Children[0] = type;
				}
				else
				{
					ast = new ASTNode
					{
						NodeType = NodeType.DEFAULT_INFERRED
					};
				}
				ast.NodeStartIndex = start;
				ast.NodeEndIndex = tokenizer.ExpressionStringPosition;
				return true;
			}
			ast = null;
			return false;
		}

		private bool InstantiateType(out ASTNode ast)
		{
			int start = tokenizer.TokenStartedStringPosition;
			if (Accept(Token.NEW))
			{
				if (!TypeName(out var type))
				{
					throw new SyntaxException(tokenizer, "Expected type name after 'new'");
				}
				ast = new ASTNode
				{
					NodeType = NodeType.INSTANTIATE_TYPE,
					NodeStartIndex = start
				};
				ast.Children[0] = type;
				bool readParameters = true;
				bool isArray = false;
				Token expectAtEnd;
				if (Accept(Token.LEFT_BRACKET))
				{
					isArray = true;
					expectAtEnd = Token.RIGHT_BRACKET;
				}
				else if (Accept(Token.LEFT_PARENTHESIS))
				{
					expectAtEnd = Token.RIGHT_PARENTHESIS;
					if (Accept(Token.RIGHT_PARENTHESIS))
					{
						ast.NodeEndIndex = tokenizer.ExpressionStringPosition;
						readParameters = false;
					}
				}
				else
				{
					if (!Peek(Token.SCOPE_BEGIN))
					{
						throw new SyntaxException(tokenizer, "Expected '(' or '['");
					}
					if (type.NodeType != NodeType.ARRAY_OF)
					{
						throw new SyntaxException(type, "Expected array type identifier");
					}
					expectAtEnd = Token.UNKNOWN;
					readParameters = false;
				}
				int args = 0;
				if (readParameters)
				{
					if (Accept(Token.REF) || Accept(Token.OUT) || Accept(Token.IN))
					{
						throw new SyntaxException(tokenizer, "ref, out and in keywords cannot be used for constructor arguments");
					}
					expectedStops.Push(new Token[2]
					{
						Token.COMMA,
						expectAtEnd
					});
					ExpressionStartTermAndContinuations(out var parameter);
					ast.Children[1] = parameter;
					args = 1;
					while (Accept(Token.COMMA))
					{
						if (Accept(Token.REF) || Accept(Token.OUT) || Accept(Token.IN))
						{
							throw new SyntaxException(tokenizer, "ref, out and in keywords cannot be used for constructor arguments");
						}
						args++;
						ExpressionStartTermAndContinuations(out parameter);
						ast.Children[ast.Children.Count] = parameter;
					}
					expectedStops.Pop();
					Expect(expectAtEnd);
				}
				if (isArray)
				{
					ASTNode temp = type;
					type = new ASTNode
					{
						NodeType = NodeType.ARRAY_OF,
						NodeValue = args,
						NodeStartIndex = temp.NodeStartIndex,
						NodeEndIndex = temp.NodeEndIndex
					};
					type.Children[0] = temp;
					ast.Children[0] = type;
				}
				if (Accept(Token.SCOPE_BEGIN))
				{
					throw new SyntaxException(tokenizer, "Scoped type/array initialization values are not supported yet");
				}
				ast.NodeEndIndex = tokenizer.ExpressionStringPosition;
				return true;
			}
			ast = null;
			return false;
		}

		private bool ExpressionArgument(out ASTNode ast)
		{
			if (Accept(Token.NUMBERED_EXPRESSION_ARGUMENT))
			{
				ast = new ASTNode
				{
					NodeType = NodeType.NUMBERED_EXPRESSION_ARGUMENT,
					NodeStartIndex = tokenizer.TokenStartedStringPosition,
					NodeEndIndex = tokenizer.ExpressionStringPosition,
					NodeValue = tokenizer.ExpressionArgumentNumber
				};
				return true;
			}
			if (Accept(Token.NAMED_EXPRESSION_ARGUMENT))
			{
				ast = new ASTNode
				{
					NodeType = NodeType.NAMED_EXPRESSION_ARGUMENT,
					NodeStartIndex = tokenizer.TokenStartedStringPosition,
					NodeEndIndex = tokenizer.ExpressionStringPosition,
					NodeValue = tokenizer.IdentifierValue
				};
				return true;
			}
			if (Accept(Token.UNNUMBERED_EXPRESSION_ARGUMENT))
			{
				throw new SyntaxException(tokenizer, "Unnumbered expression arguments are not supported.");
			}
			ast = null;
			return false;
		}

		private bool TypeCast(ref ASTNode ast)
		{
			if (ast.NodeType == NodeType.PARENTHESIZED_EXPRESSION && !Peek(Token.EOF) && !Peek(Token.POINT) && !Peek(Token.QUESTION_MARK) && !Peek(Token.LEFT_BRACKET))
			{
				ASTNode castType = ast.Children[0];
				if (castType.NodeType != NodeType.IDENTIFIER && castType.NodeType != NodeType.MEMBER_ACCESS)
				{
					return false;
				}
				TokenizerState state = tokenizer.GetState();
				Token currentBackup = current;
				ASTNode exprToCast;
				if (Peek(Token.LEFT_PARENTHESIS))
				{
					Expect(Token.LEFT_PARENTHESIS);
					ExpressionStartTermAndContinuations(out exprToCast);
					if (Accept(Token.COMMA))
					{
						tokenizer.SetState(state);
						current = currentBackup;
						return Invocation(ref ast);
					}
					tokenizer.SetState(state);
					current = currentBackup;
				}
				if (!ExpressionStartTermAndContinuations(out exprToCast, allowFailure: true))
				{
					tokenizer.SetState(state);
					current = currentBackup;
					return false;
				}
				ast = new ASTNode
				{
					NodeType = NodeType.TYPE_CAST,
					NodeStartIndex = ast.NodeStartIndex,
					NodeEndIndex = exprToCast.NodeEndIndex
				};
				ast.Children[0] = castType;
				ast.Children[1] = exprToCast;
				return true;
			}
			return false;
		}

		private bool MemberAccess(ref ASTNode ast, bool allowNullConditional)
		{
			int start = ast.NodeStartIndex;
			Token token = current;
			if (token == Token.MEMBER_ACCESS_POINTER_DEREFERENCE)
			{
				throw new SyntaxException(tokenizer, "Member access by pointer dereference operator '->' is not supported");
			}
			if (!allowNullConditional && token == Token.MEMBER_ACCESS_NULL_CONDITIONAL)
			{
				throw new SyntaxException(tokenizer, "Unexpected null conditional");
			}
			if (Accept(Token.POINT) || Accept(Token.MEMBER_ACCESS_NULL_CONDITIONAL))
			{
				if (!Identifier(out var identifier))
				{
					throw new SyntaxException(ast, "Expected identifier");
				}
				ASTNode child = ast;
				ast = new ASTNode
				{
					NodeType = ((token == Token.MEMBER_ACCESS_NULL_CONDITIONAL) ? NodeType.MEMBER_ACCESS_NULL_CONDITIONAL : NodeType.MEMBER_ACCESS),
					NodeStartIndex = start,
					NodeEndIndex = tokenizer.ExpressionStringPosition
				};
				ast.Children[0] = child;
				ast.Children[1] = identifier;
				return true;
			}
			return false;
		}

		private bool Invocation(ref ASTNode ast)
		{
			int start = ast.NodeStartIndex;
			if (Accept(Token.LEFT_PARENTHESIS))
			{
				ASTNode method = ast;
				ast = new ASTNode
				{
					NodeType = NodeType.INVOCATION,
					NodeStartIndex = start
				};
				ast.Children[0] = method;
				if (Accept(Token.RIGHT_PARENTHESIS))
				{
					ast.NodeEndIndex = tokenizer.ExpressionStringPosition;
					return true;
				}
				if (Accept(Token.REF) || Accept(Token.OUT) || Accept(Token.IN))
				{
					throw new SyntaxException(tokenizer, "ref, out and in keywords are not currently supported");
				}
				expectedStops.Push(new Token[2]
				{
					Token.COMMA,
					Token.RIGHT_PARENTHESIS
				});
				ExpressionStartTermAndContinuations(out var parameter);
				ast.Children[1] = parameter;
				int args = 1;
				while (Accept(Token.COMMA))
				{
					if (Accept(Token.REF) || Accept(Token.OUT) || Accept(Token.IN))
					{
						throw new SyntaxException(tokenizer, "ref, out and in keywords are not currently supported");
					}
					args++;
					ExpressionStartTermAndContinuations(out parameter);
					ast.Children[ast.Children.Count] = parameter;
				}
				expectedStops.Pop();
				Expect(Token.RIGHT_PARENTHESIS);
				ast.NodeEndIndex = tokenizer.ExpressionStringPosition;
				return true;
			}
			return false;
		}

		private bool ElementAccess(ref ASTNode ast)
		{
			int start = ast.NodeStartIndex;
			Token token = current;
			if (Accept(Token.LEFT_BRACKET) || Accept(Token.ELEMENT_ACCESS_NULL_CONDITIONAL))
			{
				ASTNode container = ast;
				ast = new ASTNode
				{
					NodeType = ((token == Token.ELEMENT_ACCESS_NULL_CONDITIONAL) ? NodeType.ELEMENT_ACCESS_NULL_CONDITIONAL : NodeType.ELEMENT_ACCESS),
					NodeStartIndex = start
				};
				expectedStops.Push(new Token[2]
				{
					Token.COMMA,
					Token.RIGHT_BRACKET
				});
				ExpressionStartTermAndContinuations(out var parameter);
				int args = 1;
				ast.Children[0] = container;
				ast.Children[1] = parameter;
				while (Accept(Token.COMMA))
				{
					args++;
					ExpressionStartTermAndContinuations(out parameter);
					ast.Children[ast.Children.Count] = parameter;
				}
				expectedStops.Pop();
				Expect(Token.RIGHT_BRACKET);
				ast.NodeEndIndex = tokenizer.ExpressionStringPosition;
				return true;
			}
			return false;
		}

		private bool Operator(ref ASTNode ast)
		{
			ASTNode left = ast;
			int start = ast.NodeStartIndex;
			NodeType operatorType;
			ASTNode right;
			switch (current)
			{
			case Token.LOGICAL_OR:
				GetNextToken();
				operatorType = NodeType.LOGICAL_OR;
				ExpressionStartTermAndContinuations(out right);
				break;
			case Token.LOGICAL_AND:
				GetNextToken();
				operatorType = NodeType.LOGICAL_AND;
				ExpressionStartTermAndContinuations(out right);
				break;
			case Token.PLUS:
				GetNextToken();
				operatorType = NodeType.ADD;
				ExpressionStartTermAndContinuations(out right);
				break;
			case Token.MINUS:
				GetNextToken();
				operatorType = NodeType.SUBTRACT;
				ExpressionStartTermAndContinuations(out right);
				break;
			case Token.MULTIPLY:
				GetNextToken();
				operatorType = NodeType.MULTIPLY;
				ExpressionStartTermAndContinuations(out right);
				break;
			case Token.DIVIDE:
				GetNextToken();
				operatorType = NodeType.DIVIDE;
				ExpressionStartTermAndContinuations(out right);
				break;
			case Token.REMAINDER:
				GetNextToken();
				operatorType = NodeType.REMAINDER;
				ExpressionStartTermAndContinuations(out right);
				break;
			case Token.BITWISE_INCLUSIVE_OR:
				GetNextToken();
				operatorType = NodeType.BITWISE_INCLUSIVE_OR;
				ExpressionStartTermAndContinuations(out right);
				break;
			case Token.BITWISE_EXCLUSIVE_OR:
				GetNextToken();
				operatorType = NodeType.BITWISE_EXCLUSIVE_OR;
				ExpressionStartTermAndContinuations(out right);
				break;
			case Token.BITWISE_AND:
				GetNextToken();
				operatorType = NodeType.BITWISE_AND;
				ExpressionStartTermAndContinuations(out right);
				break;
			case Token.EQUALS:
				GetNextToken();
				operatorType = NodeType.EQUALS;
				ExpressionStartTermAndContinuations(out right);
				break;
			case Token.NOT_EQUALS:
				GetNextToken();
				operatorType = NodeType.NOT_EQUALS;
				ExpressionStartTermAndContinuations(out right);
				break;
			case Token.GREATER_THAN:
				GetNextToken();
				if (Peek(Token.GREATER_THAN))
				{
					GetNextToken();
					operatorType = NodeType.RIGHT_SHIFT;
					ExpressionStartTermAndContinuations(out right);
				}
				else
				{
					operatorType = NodeType.GREATER_THAN;
					ExpressionStartTermAndContinuations(out right);
				}
				break;
			case Token.GREATER_THAN_OR_EQUAL:
				GetNextToken();
				operatorType = NodeType.GREATER_THAN_OR_EQUAL;
				ExpressionStartTermAndContinuations(out right);
				break;
			case Token.LESS_THAN:
				GetNextToken();
				operatorType = NodeType.LESS_THAN;
				ExpressionStartTermAndContinuations(out right);
				break;
			case Token.LESS_THAN_OR_EQUAL:
				GetNextToken();
				operatorType = NodeType.LESS_THAN_OR_EQUAL;
				ExpressionStartTermAndContinuations(out right);
				break;
			case Token.LEFT_SHIFT:
				GetNextToken();
				operatorType = NodeType.LEFT_SHIFT;
				ExpressionStartTermAndContinuations(out right);
				break;
			case Token.NULL_COALESCE:
				GetNextToken();
				operatorType = NodeType.NULL_COALESCE;
				ExpressionStartTermAndContinuations(out right);
				break;
			case Token.INCREMENT:
				GetNextToken();
				ast = new ASTNode
				{
					NodeType = NodeType.POST_INCREMENT,
					NodeStartIndex = start,
					NodeEndIndex = tokenizer.ExpressionStringPosition
				};
				ast.Children[0] = left;
				return true;
			case Token.DECREMENT:
				GetNextToken();
				ast = new ASTNode
				{
					NodeType = NodeType.POST_DECREMENT,
					NodeStartIndex = start,
					NodeEndIndex = tokenizer.ExpressionStringPosition
				};
				ast.Children[0] = left;
				return true;
			case Token.RELATIONAL_IS:
			{
				GetNextToken();
				if (!TypeName(out var type2))
				{
					throw new SyntaxException(tokenizer, "Expected type identifier after 'is'");
				}
				ast = new ASTNode
				{
					NodeType = NodeType.RELATIONAL_IS,
					NodeStartIndex = start,
					NodeEndIndex = tokenizer.ExpressionStringPosition
				};
				ast.Children[0] = left;
				ast.Children[1] = type2;
				return true;
			}
			case Token.RELATIONAL_AS:
			{
				GetNextToken();
				if (!TypeName(out var type))
				{
					throw new SyntaxException(tokenizer, "Expected type identifier after 'as'");
				}
				ast = new ASTNode
				{
					NodeType = NodeType.RELATIONAL_AS,
					NodeStartIndex = start,
					NodeEndIndex = tokenizer.ExpressionStringPosition
				};
				ast.Children[0] = left;
				ast.Children[1] = type;
				return true;
			}
			case Token.QUESTION_MARK:
			{
				ast = new ASTNode
				{
					NodeType = NodeType.TERNARY_CONDITIONAL,
					NodeStartIndex = start
				};
				GetNextToken();
				ExpressionStartTermAndContinuationsUntil(Token.COLON, out var @true);
				Expect(Token.COLON);
				ExpressionStartTermAndContinuations(out var @false);
				ast.NodeEndIndex = tokenizer.ExpressionStringPosition;
				ast.Children[0] = left;
				ast.Children[1] = @true;
				ast.Children[2] = @false;
				return true;
			}
			default:
				return false;
			}
			ASTNode @operator = new ASTNode
			{
				NodeType = operatorType,
				NodeStartIndex = start,
				NodeEndIndex = tokenizer.ExpressionStringPosition
			};
			ast = GetTwoParamOperatorPrecedence(left, @operator, right);
			return true;
		}

		private bool SimpleAssignment(ref ASTNode ast)
		{
			if (Peek(Token.SIMPLE_ASSIGNMENT))
			{
				ASTNode newAst = new ASTNode
				{
					NodeType = NodeType.SIMPLE_ASSIGNMENT,
					NodeStartIndex = tokenizer.TokenStartedStringPosition,
					NodeEndIndex = tokenizer.ExpressionStringPosition
				};
				GetNextToken();
				if (!ExpressionStartTermAndContinuations(out var valueToAssign))
				{
					throw new SyntaxException(newAst, "Expected value to assign after '=' operator.");
				}
				newAst.Children[0] = ast;
				newAst.Children[1] = valueToAssign;
				ast = newAst;
				return true;
			}
			return false;
		}

		private bool AssignmentOperator(ref ASTNode ast)
		{
			NodeType operatorNodeType;
			switch (current)
			{
			case Token.ADDITION_ASSIGNMENT:
				operatorNodeType = NodeType.ADD;
				break;
			case Token.SUBTRACTION_ASSIGNMENT:
				operatorNodeType = NodeType.SUBTRACT;
				break;
			case Token.MULTIPLICATION_ASSIGNMENT:
				operatorNodeType = NodeType.MULTIPLY;
				break;
			case Token.DIVISION_ASSIGNMENT:
				operatorNodeType = NodeType.DIVIDE;
				break;
			case Token.REMAINDER_ASSIGNMENT:
				operatorNodeType = NodeType.REMAINDER;
				break;
			case Token.LEFT_SHIFT_ASSIGNMENT:
				operatorNodeType = NodeType.LEFT_SHIFT;
				break;
			case Token.RIGHT_SHIFT_ASSIGNMENT:
				operatorNodeType = NodeType.RIGHT_SHIFT;
				break;
			case Token.BITWISE_AND_ASSIGNMENT:
				operatorNodeType = NodeType.BITWISE_AND;
				break;
			case Token.BITWISE_OR_ASSIGNMENT:
				operatorNodeType = NodeType.BITWISE_INCLUSIVE_OR;
				break;
			case Token.BITWISE_XOR_ASSIGNMENT:
				operatorNodeType = NodeType.BITWISE_EXCLUSIVE_OR;
				break;
			default:
				return false;
			}
			ASTNode newAst = new ASTNode
			{
				NodeType = NodeType.SIMPLE_ASSIGNMENT,
				NodeStartIndex = tokenizer.TokenStartedStringPosition,
				NodeEndIndex = tokenizer.ExpressionStringPosition
			};
			GetNextToken();
			if (!ExpressionStartTermAndContinuations(out var valueToAssign))
			{
				throw new SyntaxException(newAst, "Expected value to assign after '=' operator.");
			}
			ASTNode assignToAstCopy = ast.DeepCopy();
			ASTNode operatorAst = new ASTNode
			{
				NodeType = operatorNodeType,
				NodeStartIndex = newAst.NodeStartIndex,
				NodeEndIndex = newAst.NodeEndIndex
			};
			operatorAst.Children[0] = assignToAstCopy;
			operatorAst.Children[1] = valueToAssign;
			newAst.Children[0] = ast;
			newAst.Children[1] = operatorAst;
			ast = newAst;
			return true;
		}

		private ASTNode GetUnaryOperatorPrecedence(ASTNode @operator, ASTNode operand)
		{
			if (!ManualPrecedenceOperators.Contains(operand.NodeType))
			{
				@operator.Children[0] = operand;
				return @operator;
			}
			int operatorPriority = OperatorPrecedences[@operator.NodeType];
			int rightPriority = OperatorPrecedences[operand.NodeType];
			if (operatorPriority >= rightPriority)
			{
				@operator.Children[0] = operand;
				return @operator;
			}
			if (operand.Children.Count != 2 && operand.NodeType != NodeType.TERNARY_CONDITIONAL)
			{
				throw new InvalidOperationException("Manual precedence operator AST Node did not have two children during precedence resolution - how'd this happen?");
			}
			@operator.Children[0] = operand.Children[0];
			operand.Children[0] = @operator;
			return operand;
		}

		private ASTNode GetTwoParamOperatorPrecedence(ASTNode left, ASTNode @operator, ASTNode right)
		{
			if (!ManualPrecedenceOperators.Contains(@operator.NodeType) || !ManualPrecedenceOperators.Contains(right.NodeType))
			{
				@operator.Children[0] = left;
				@operator.Children[1] = right;
				return @operator;
			}
			int operatorPriority = OperatorPrecedences[@operator.NodeType];
			int rightPriority = OperatorPrecedences[right.NodeType];
			bool forceRightPrecedence = false;
			if (operatorPriority == rightPriority && DefaultToRightPrecedenceOperators.Contains(@operator.NodeType) && DefaultToRightPrecedenceOperators.Contains(right.NodeType))
			{
				forceRightPrecedence = true;
			}
			if (forceRightPrecedence || operatorPriority > rightPriority)
			{
				@operator.Children[0] = left;
				@operator.Children[1] = right;
				return @operator;
			}
			if (right.Children.Count == 0)
			{
				throw new InvalidOperationException("Manual precedence operator AST Node did not have at least one child during precedence resolution - how'd this happen?");
			}
			@operator.Children[0] = left;
			@operator.Children[1] = right.Children[0];
			right.Children[0] = @operator;
			return right;
		}

		private string ReadTypeName__()
		{
			ExpectButDontEat(Token.IDENTIFIER);
			string name = tokenizer.IdentifierValue;
			GetNextToken();
			while (Accept(Token.POINT))
			{
				ExpectButDontEat(Token.IDENTIFIER);
				name += tokenizer.IdentifierValue;
				GetNextToken();
			}
			if (Accept(Token.LESS_THAN))
			{
				ExpectButDontEat(Token.IDENTIFIER);
				name = name + "<" + tokenizer.IdentifierValue;
				while (Accept(Token.COMMA))
				{
					ExpectButDontEat(Token.IDENTIFIER);
					name = name + ", " + tokenizer.IdentifierValue;
				}
				Expect(Token.GREATER_THAN);
				name += ">";
			}
			return name;
		}
	}
}
