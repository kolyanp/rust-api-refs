using System;
using System.Runtime.CompilerServices;

namespace MySqlConnector.Core;

[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(0)]
[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(1)]
internal abstract class SqlParser(StatementPreparer preparer)
{
	[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(0)]
	[Flags]
	protected enum FinalParseStates
	{
		None = 0,
		Complete = 1,
		NeedsNewline = 2,
		NeedsSemicolon = 4
	}

	[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(0)]
	private enum State
	{
		Beginning,
		Statement,
		SingleQuotedString,
		SingleQuotedStringBackslash,
		SingleQuotedStringSingleQuote,
		DoubleQuotedString,
		DoubleQuotedStringBackslash,
		DoubleQuotedStringDoubleQuote,
		BacktickQuotedString,
		BacktickQuotedStringBacktick,
		EndOfLineComment,
		Hyphen,
		SecondHyphen,
		ForwardSlash,
		CStyleComment,
		CStyleCommentAsterisk,
		QuestionMark,
		AtSign,
		NamedParameter
	}

	protected StatementPreparer Preparer { get; } = preparer;

	public void Parse(string sql)
	{
		if (sql == null)
		{
			throw new ArgumentNullException("sql");
		}
		OnBeforeParse(sql);
		int num = -1;
		bool flag = (Preparer.Options & StatementPreparerOptions.NoBackslashEscapes) == StatementPreparerOptions.NoBackslashEscapes;
		State state = State.Beginning;
		State state2 = State.Beginning;
		bool flag2 = false;
		for (int i = 0; i < sql.Length; i++)
		{
			char c = sql[i];
			switch (state)
			{
			case State.EndOfLineComment:
				if (c == '\n')
				{
					state = state2;
				}
				break;
			case State.CStyleComment:
				if (c == '*')
				{
					state = State.CStyleCommentAsterisk;
				}
				break;
			case State.CStyleCommentAsterisk:
				state = ((c == '/') ? state2 : State.CStyleComment);
				break;
			case State.SingleQuotedString:
				switch (c)
				{
				case '\'':
					state = State.SingleQuotedStringSingleQuote;
					break;
				case '\\':
					if (!flag)
					{
						state = State.SingleQuotedStringBackslash;
					}
					break;
				}
				break;
			case State.SingleQuotedStringBackslash:
				state = State.SingleQuotedString;
				break;
			case State.DoubleQuotedString:
				switch (c)
				{
				case '"':
					state = State.DoubleQuotedStringDoubleQuote;
					break;
				case '\\':
					if (!flag)
					{
						state = State.DoubleQuotedStringBackslash;
					}
					break;
				}
				break;
			case State.DoubleQuotedStringBackslash:
				state = State.DoubleQuotedString;
				break;
			case State.BacktickQuotedString:
				if (c == '`')
				{
					state = State.BacktickQuotedStringBacktick;
				}
				break;
			case State.SingleQuotedStringSingleQuote:
				if (c == '\'')
				{
					state = State.SingleQuotedString;
					break;
				}
				if (flag2)
				{
					OnNamedParameter(num, i - num);
				}
				if (c == ';')
				{
					OnStatementEnd(i);
					state = State.Beginning;
				}
				else
				{
					state = State.Statement;
				}
				break;
			case State.DoubleQuotedStringDoubleQuote:
				if (c == '"')
				{
					state = State.DoubleQuotedString;
					break;
				}
				if (flag2)
				{
					OnNamedParameter(num, i - num);
				}
				if (c == ';')
				{
					OnStatementEnd(i);
					state = State.Beginning;
				}
				else
				{
					state = State.Statement;
				}
				break;
			case State.BacktickQuotedStringBacktick:
				if (c == '`')
				{
					state = State.BacktickQuotedString;
					break;
				}
				if (flag2)
				{
					OnNamedParameter(num, i - num);
				}
				if (c == ';')
				{
					OnStatementEnd(i);
					state = State.Beginning;
				}
				else
				{
					state = State.Statement;
				}
				break;
			case State.SecondHyphen:
				state = ((c != ' ') ? State.Statement : State.EndOfLineComment);
				break;
			case State.Hyphen:
				state = ((c != '-') ? State.Statement : State.SecondHyphen);
				break;
			case State.ForwardSlash:
				state = ((c != '*') ? State.Statement : State.CStyleComment);
				break;
			case State.QuestionMark:
				if (IsVariableName(c))
				{
					state = State.NamedParameter;
					break;
				}
				OnPositionalParameter(num);
				if (c == ';')
				{
					OnStatementEnd(i);
					state = State.Beginning;
				}
				else
				{
					state = State.Statement;
				}
				break;
			case State.AtSign:
				if (IsVariableName(c))
				{
					state = State.NamedParameter;
					break;
				}
				switch (c)
				{
				case '`':
					state = State.BacktickQuotedString;
					flag2 = true;
					break;
				case '"':
					state = State.DoubleQuotedString;
					flag2 = true;
					break;
				case '\'':
					state = State.SingleQuotedString;
					flag2 = true;
					break;
				default:
					state = State.Statement;
					break;
				}
				break;
			case State.NamedParameter:
				if (!IsVariableName(c))
				{
					OnNamedParameter(num, i - num);
					if (c == ';')
					{
						OnStatementEnd(i);
						state = State.Beginning;
					}
					else
					{
						state = State.Statement;
					}
				}
				break;
			default:
				throw new InvalidOperationException($"Unexpected state: {state}");
			case State.Beginning:
			case State.Statement:
				if (c == '-' && i < sql.Length - 2 && sql[i + 1] == '-' && sql[i + 2] == ' ')
				{
					state2 = state;
					state = State.Hyphen;
					break;
				}
				if (c == '/' && i < sql.Length - 1 && sql[i + 1] == '*')
				{
					state2 = state;
					state = State.ForwardSlash;
					break;
				}
				switch (c)
				{
				case '\'':
					state = State.SingleQuotedString;
					break;
				case '"':
					state = State.DoubleQuotedString;
					break;
				case '`':
					state = State.BacktickQuotedString;
					break;
				case '?':
					state = State.QuestionMark;
					num = i;
					break;
				case '@':
					state = State.AtSign;
					num = i;
					break;
				case '#':
					state2 = state;
					state = State.EndOfLineComment;
					break;
				case ';':
					if (state != State.Beginning)
					{
						OnStatementEnd(i);
					}
					state = State.Beginning;
					break;
				default:
					if (!IsWhitespace(c) && state == State.Beginning)
					{
						state = State.Statement;
						OnStatementBegin(i);
					}
					break;
				}
				break;
			}
		}
		FinalParseStates finalParseStates = FinalParseStates.None;
		if (state == State.NamedParameter)
		{
			OnNamedParameter(num, sql.Length - num);
			state = State.Statement;
		}
		else if (state == State.QuestionMark)
		{
			OnPositionalParameter(num);
			state = State.Statement;
		}
		else if (state == State.EndOfLineComment)
		{
			finalParseStates |= FinalParseStates.NeedsNewline;
			state = state2;
		}
		else if ((state == State.SingleQuotedStringSingleQuote || state == State.DoubleQuotedStringDoubleQuote || state == State.BacktickQuotedStringBacktick) ? true : false)
		{
			state = State.Statement;
		}
		if (state == State.Statement)
		{
			OnStatementEnd(sql.Length);
			finalParseStates |= FinalParseStates.NeedsSemicolon;
			state = State.Beginning;
		}
		if (state == State.Beginning)
		{
			finalParseStates |= FinalParseStates.Complete;
		}
		OnParsed(finalParseStates);
	}

	protected virtual void OnBeforeParse(string sql)
	{
	}

	protected virtual void OnStatementBegin(int index)
	{
	}

	protected virtual void OnPositionalParameter(int index)
	{
	}

	protected virtual void OnNamedParameter(int index, int length)
	{
	}

	protected virtual void OnStatementEnd(int index)
	{
	}

	protected virtual void OnParsed(FinalParseStates states)
	{
	}

	private static bool IsWhitespace(char ch)
	{
		switch (ch)
		{
		case '\t':
		case '\n':
		case '\r':
		case ' ':
			return true;
		default:
			return false;
		}
	}

	private static bool IsVariableName(char ch)
	{
		if (ch >= 'a')
		{
			if (ch <= 'z' || ch >= '\u0080')
			{
				goto IL_003b;
			}
		}
		else if (ch >= 'A')
		{
			if (ch <= 'Z' || ch == '_')
			{
				goto IL_003b;
			}
		}
		else if (ch >= '0')
		{
			if (ch <= '9')
			{
				goto IL_003b;
			}
		}
		else if (ch == '$' || ch == '.')
		{
			goto IL_003b;
		}
		return false;
		IL_003b:
		return true;
	}
}
