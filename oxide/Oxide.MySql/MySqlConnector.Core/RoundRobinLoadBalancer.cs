using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace MySqlConnector.Core;

internal sealed class RoundRobinLoadBalancer : ILoadBalancer
{
	[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(1)]
	private readonly object m_lock;

	private uint m_counter;

	public RoundRobinLoadBalancer()
	{
		m_lock = new object();
	}

	[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(1)]
	public IReadOnlyList<string> LoadBalance(IReadOnlyList<string> hosts)
	{
		int num;
		lock (m_lock)
		{
			num = (int)(m_counter++ % hosts.Count);
		}
		List<string> list = new List<string>(hosts.Count);
		for (int i = num; i < hosts.Count; i++)
		{
			list.Add(hosts[i]);
		}
		for (int j = 0; j < num; j++)
		{
			list.Add(hosts[j]);
		}
		return list;
	}
}
