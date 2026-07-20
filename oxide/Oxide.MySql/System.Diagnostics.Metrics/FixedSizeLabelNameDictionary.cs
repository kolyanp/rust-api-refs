using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace System.Diagnostics.Metrics;

internal sealed class FixedSizeLabelNameDictionary<TStringSequence, TObjectSequence, TAggregator> : ConcurrentDictionary<TStringSequence, ConcurrentDictionary<TObjectSequence, TAggregator>> where TStringSequence : IStringSequence, IEquatable<TStringSequence> where TObjectSequence : IObjectSequence, IEquatable<TObjectSequence> where TAggregator : Aggregator
{
	public void Collect(Action<LabeledAggregationStatistics> visitFunc)
	{
		using IEnumerator<KeyValuePair<TStringSequence, ConcurrentDictionary<TObjectSequence, TAggregator>>> enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			KeyValuePair<TStringSequence, ConcurrentDictionary<TObjectSequence, TAggregator>> current = enumerator.Current;
			TStringSequence key = current.Key;
			foreach (KeyValuePair<TObjectSequence, TAggregator> item in current.Value)
			{
				TObjectSequence key2 = item.Key;
				KeyValuePair<string, string>[] array = new KeyValuePair<string, string>[key.Length];
				for (int i = 0; i < array.Length; i++)
				{
					array[i] = new KeyValuePair<string, string>(key[i], key2[i]?.ToString() ?? "");
				}
				IAggregationStatistics stats = item.Value.Collect();
				visitFunc(new LabeledAggregationStatistics(stats, array));
			}
		}
	}

	public ConcurrentDictionary<TObjectSequence, TAggregator> GetValuesDictionary([In][_003Cb470df79_002Da5b8_002D442e_002D9a19_002Ddc7ef3b9a2cb_003EIsReadOnly] ref TStringSequence names)
	{
		return GetOrAdd(names, (TStringSequence _) => new ConcurrentDictionary<TObjectSequence, TAggregator>());
	}
}
