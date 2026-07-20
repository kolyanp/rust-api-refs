using System;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace MySqlConnector.Core;

[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(1)]
[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(0)]
internal sealed class NormalizedSchema
{
	private const string ReQuoted = "`((?:[^`]|``)+)`";

	private const string ReUnQuoted = "([^\\.`]+)";

	private const string ReEither = "(?:`((?:[^`]|``)+)`|([^\\.`]+))";

	private const string ReName = "^\\s*(?:`((?:[^`]|``)+)`|([^\\.`]+))\\s*(?:\\.\\s*(?:`((?:[^`]|``)+)`|([^\\.`]+))\\s*)?$";

	private static readonly Regex s_nameRegex = new Regex("^\\s*(?:`((?:[^`]|``)+)`|([^\\.`]+))\\s*(?:\\.\\s*(?:`((?:[^`]|``)+)`|([^\\.`]+))\\s*)?$", RegexOptions.Compiled);

	[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(2)]
	[field: _003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(2)]
	public string Schema
	{
		[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(2)]
		get;
	}

	[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(2)]
	[field: _003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(2)]
	public string Component
	{
		[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(2)]
		get;
	}

	public string FullyQualified => "`" + Schema + "`.`" + Component + "`";

	private static Regex NameRegex()
	{
		return s_nameRegex;
	}

	public static NormalizedSchema MustNormalize(string name, [_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(2)] string defaultSchema = null)
	{
		NormalizedSchema normalizedSchema = new NormalizedSchema(name, defaultSchema);
		if (normalizedSchema != null)
		{
			if (normalizedSchema.Component != null)
			{
				if (normalizedSchema.Schema == null)
				{
					throw new ArgumentException("Could not determine schema", "defaultSchema");
				}
				return normalizedSchema;
			}
			throw new ArgumentException("Could not determine function/procedure name", "name");
		}
		_003C54935a9f_002D04ec_002D42f0_002Db2db_002Dde3406f234de_003E_003CPrivateImplementationDetails_003E.ThrowInvalidOperationException();
		NormalizedSchema result = default(NormalizedSchema);
		return result;
	}

	public NormalizedSchema(string name, [_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(2)] string defaultSchema = null)
	{
		Match match = NameRegex().Match(name);
		if (match.Success)
		{
			if (match.Groups[3].Success)
			{
				Component = match.Groups[3].Value.Replace("``", "`").Trim();
			}
			else if (match.Groups[4].Success)
			{
				Component = match.Groups[4].Value.Trim();
			}
			string text = "";
			if (match.Groups[1].Success)
			{
				text = match.Groups[1].Value.Replace("``", "`").Trim();
			}
			else if (match.Groups[2].Success)
			{
				text = match.Groups[2].Value.Trim();
			}
			if (Component == null)
			{
				Component = text.Trim();
			}
			else
			{
				Schema = text.Trim();
			}
			if (Schema == null)
			{
				Schema = defaultSchema;
			}
		}
	}
}
