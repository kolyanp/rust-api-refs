using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace MySqlConnector.Core;

[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(1)]
internal interface ILoadBalancer
{
	IReadOnlyList<string> LoadBalance(IReadOnlyList<string> hosts);
}
