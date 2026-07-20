using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace MySqlConnector.Core;

[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(0)]
[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(1)]
internal sealed class RandomLoadBalancer : ILoadBalancer
{
	private readonly Random m_random;

	public static ILoadBalancer Instance { get; } = new RandomLoadBalancer();

	public IReadOnlyList<string> LoadBalance(IReadOnlyList<string> hosts)
	{
		List<string> list = new List<string>(hosts);
		for (int num = hosts.Count - 1; num >= 1; num--)
		{
			int num2;
			lock (m_random)
			{
				num2 = m_random.Next(num + 1);
			}
			if (num != num2)
			{
				List<string> list2 = list;
				int index = num2;
				List<string> list3 = list;
				int index2 = num;
				string value = list[num];
				string value2 = list[num2];
				list2[index] = value;
				list3[index2] = value2;
			}
		}
		return list;
	}

	private RandomLoadBalancer()
	{
		m_random = new Random();
	}
}
