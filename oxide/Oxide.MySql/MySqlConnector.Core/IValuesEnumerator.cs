using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace MySqlConnector.Core;

internal interface IValuesEnumerator
{
	int FieldCount { get; }

	ValueTask<bool> MoveNextAsync();

	bool MoveNext();

	[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(1)]
	void GetValues(object[] values);
}
