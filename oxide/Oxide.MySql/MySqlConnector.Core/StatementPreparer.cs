using System.Collections.Generic;
using System.Data;
using System.Runtime.CompilerServices;
using MySqlConnector.Protocol.Serialization;
using MySqlConnector.Utilities;

namespace MySqlConnector.Core;

[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(1)]
[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(0)]
internal sealed class StatementPreparer(string commandText, [_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(2)][field: _003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(2)] MySqlParameterCollection parameters, StatementPreparerOptions options)
{
	[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(0)]
	private sealed class ParameterSqlParser(StatementPreparer preparer, ByteBufferWriter writer) : SqlParser(preparer)
	{
		private int m_currentParameterIndex;

		private int m_lastIndex;

		public bool IsComplete { get; private set; }

		private ByteBufferWriter Writer { get; } = writer;

		protected override void OnNamedParameter(int index, int length)
		{
			int parameterIndex = base.Preparer.GetParameterIndex(base.Preparer.CommandText.Substring(index, length));
			if (parameterIndex != -1)
			{
				DoAppendParameter(parameterIndex, index, length);
			}
		}

		protected override void OnPositionalParameter(int index)
		{
			DoAppendParameter(m_currentParameterIndex, index, 1);
			m_currentParameterIndex++;
		}

		private void DoAppendParameter(int parameterIndex, int textIndex, int textLength)
		{
			Writer.Write(base.Preparer.CommandText, m_lastIndex, textIndex - m_lastIndex);
			base.Preparer.GetInputParameter(parameterIndex).AppendSqlString(Writer, base.Preparer.Options);
			m_lastIndex = textIndex + textLength;
		}

		protected override void OnParsed(FinalParseStates states)
		{
			Writer.Write(base.Preparer.CommandText, m_lastIndex, base.Preparer.CommandText.Length - m_lastIndex);
			if ((states & FinalParseStates.NeedsNewline) == FinalParseStates.NeedsNewline)
			{
				Writer.Write((byte)10);
			}
			if ((states & FinalParseStates.NeedsSemicolon) == FinalParseStates.NeedsSemicolon && (base.Preparer.Options & StatementPreparerOptions.AppendSemicolon) == StatementPreparerOptions.AppendSemicolon)
			{
				Writer.Write((byte)59);
			}
			IsComplete = (states & FinalParseStates.Complete) == FinalParseStates.Complete;
		}
	}

	[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(0)]
	private sealed class PreparedCommandSqlParser(StatementPreparer preparer, List<ParsedStatement> statements, List<int> statementStartEndIndexes, ByteBufferWriter writer) : SqlParser(preparer)
	{
		private int m_currentParameterIndex;

		private int m_lastIndex;

		private List<ParsedStatement> Statements { get; } = statements;

		private List<int> StatementStartEndIndexes { get; } = statementStartEndIndexes;

		private ByteBufferWriter Writer { get; } = writer;

		protected override void OnStatementBegin(int index)
		{
			Statements.Add(new ParsedStatement());
			StatementStartEndIndexes.Add(Writer.Position);
			Writer.Write((byte)22);
			m_lastIndex = index;
		}

		protected override void OnNamedParameter(int index, int length)
		{
			string parameterName = base.Preparer.CommandText.Substring(index, length);
			DoAppendParameter(parameterName, -1, index, length);
		}

		protected override void OnPositionalParameter(int index)
		{
			DoAppendParameter(null, m_currentParameterIndex, index, 1);
			m_currentParameterIndex++;
		}

		[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(2)]
		private void DoAppendParameter(string parameterName, int parameterIndex, int textIndex, int textLength)
		{
			Writer.Write(base.Preparer.CommandText, m_lastIndex, textIndex - m_lastIndex);
			m_lastIndex = textIndex + textLength;
			Writer.Write((byte)63);
			List<ParsedStatement> statements = Statements;
			statements[statements.Count - 1].ParameterNames.Add(parameterName);
			List<ParsedStatement> statements2 = Statements;
			statements2[statements2.Count - 1].NormalizedParameterNames.Add((parameterName == null) ? null : MySqlParameter.NormalizeParameterName(parameterName));
			List<ParsedStatement> statements3 = Statements;
			statements3[statements3.Count - 1].ParameterIndexes.Add(parameterIndex);
		}

		protected override void OnStatementEnd(int index)
		{
			Writer.Write(base.Preparer.CommandText, m_lastIndex, index - m_lastIndex);
			m_lastIndex = index;
			StatementStartEndIndexes.Add(Writer.Position);
		}
	}

	public StatementPreparerOptions Options { get; } = options;

	private string CommandText { get; } = commandText;

	public ParsedStatements SplitStatements()
	{
		List<ParsedStatement> list = new List<ParsedStatement>();
		List<int> list2 = new List<int>();
		ByteBufferWriter byteBufferWriter = new ByteBufferWriter(CommandText.Length + 1);
		new PreparedCommandSqlParser(this, list, list2, byteBufferWriter).Parse(CommandText);
		for (int i = 0; i < list.Count; i++)
		{
			list[i].StatementBytes = Utility.Slice(byteBufferWriter.ArraySegment, list2[i * 2], list2[i * 2 + 1] - list2[i * 2]);
		}
		return new ParsedStatements(list, byteBufferWriter.ToPayloadData());
	}

	public bool ParseAndBindParameters(ByteBufferWriter writer)
	{
		if (!string.IsNullOrWhiteSpace(CommandText))
		{
			ParameterSqlParser parameterSqlParser = new ParameterSqlParser(this, writer);
			parameterSqlParser.Parse(CommandText);
			return parameterSqlParser.IsComplete;
		}
		return true;
	}

	private int GetParameterIndex(string name)
	{
		int num = parameters?.NormalizedIndexOf(name) ?? (-1);
		if (num == -1 && (Options & StatementPreparerOptions.AllowUserVariables) == 0)
		{
			throw new MySqlException("Parameter '" + name + "' must be defined. To use this as a variable, set 'Allow User Variables=true' in the connection string.");
		}
		return num;
	}

	private MySqlParameter GetInputParameter(int index)
	{
		if (index >= (parameters?.Count ?? 0))
		{
			object arg = index;
			object arg2 = parameters?.Count ?? 0;
			MySqlParameterCollection mySqlParameterCollection = parameters;
			throw new MySqlException(string.Format("Parameter index {0} is invalid when only {1} parameter{2} defined.", arg, arg2, (mySqlParameterCollection != null && mySqlParameterCollection.Count == 1) ? " is" : "s are"));
		}
		MySqlParameter mySqlParameter = parameters[index];
		if (mySqlParameter.Direction != ParameterDirection.Input && (Options & StatementPreparerOptions.AllowOutputParameters) == 0)
		{
			throw new MySqlException("Only ParameterDirection.Input is supported when CommandType is Text (parameter name: " + mySqlParameter.ParameterName + ")");
		}
		return mySqlParameter;
	}
}
