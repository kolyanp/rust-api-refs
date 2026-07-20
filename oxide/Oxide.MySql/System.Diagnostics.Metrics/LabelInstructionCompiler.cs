using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Security;

namespace System.Diagnostics.Metrics;

[SecurityCritical]
internal static class LabelInstructionCompiler
{
	public static AggregatorLookupFunc<TAggregator> Create<TAggregator>(ref AggregatorStore<TAggregator> aggregatorStore, Func<TAggregator> createAggregatorFunc, ReadOnlySpan<KeyValuePair<string, object>> labels) where TAggregator : Aggregator
	{
		LabelInstruction[] array = Compile(labels);
		Array.Sort(array, (LabelInstruction a, LabelInstruction b) => string.CompareOrdinal(a.LabelName, b.LabelName));
		int expectedLabels = labels.Length;
		switch (array.Length)
		{
		case 0:
		{
			TAggregator defaultAggregator = aggregatorStore.GetAggregator();
			return delegate(ReadOnlySpan<KeyValuePair<string, object>> l, out TAggregator aggregator)
			{
				if (l.Length != expectedLabels)
				{
					aggregator = null;
					return false;
				}
				aggregator = defaultAggregator;
				return true;
			};
		}
		case 1:
		{
			StringSequence1 names2 = new StringSequence1(array[0].LabelName);
			ConcurrentDictionary<ObjectSequence1, TAggregator> labelValuesDictionary2 = aggregatorStore.GetLabelValuesDictionary<StringSequence1, ObjectSequence1>(ref names2);
			LabelInstructionInterpreter<ObjectSequence1, TAggregator> labelInstructionInterpreter2 = new LabelInstructionInterpreter<ObjectSequence1, TAggregator>(expectedLabels, array, labelValuesDictionary2, createAggregatorFunc);
			return labelInstructionInterpreter2.GetAggregator;
		}
		case 2:
		{
			StringSequence2 names4 = new StringSequence2(array[0].LabelName, array[1].LabelName);
			ConcurrentDictionary<ObjectSequence2, TAggregator> labelValuesDictionary4 = aggregatorStore.GetLabelValuesDictionary<StringSequence2, ObjectSequence2>(ref names4);
			LabelInstructionInterpreter<ObjectSequence2, TAggregator> labelInstructionInterpreter4 = new LabelInstructionInterpreter<ObjectSequence2, TAggregator>(expectedLabels, array, labelValuesDictionary4, createAggregatorFunc);
			return labelInstructionInterpreter4.GetAggregator;
		}
		case 3:
		{
			StringSequence3 names3 = new StringSequence3(array[0].LabelName, array[1].LabelName, array[2].LabelName);
			ConcurrentDictionary<ObjectSequence3, TAggregator> labelValuesDictionary3 = aggregatorStore.GetLabelValuesDictionary<StringSequence3, ObjectSequence3>(ref names3);
			LabelInstructionInterpreter<ObjectSequence3, TAggregator> labelInstructionInterpreter3 = new LabelInstructionInterpreter<ObjectSequence3, TAggregator>(expectedLabels, array, labelValuesDictionary3, createAggregatorFunc);
			return labelInstructionInterpreter3.GetAggregator;
		}
		default:
		{
			string[] array2 = new string[array.Length];
			for (int num = 0; num < array.Length; num++)
			{
				array2[num] = array[num].LabelName;
			}
			StringSequenceMany names = new StringSequenceMany(array2);
			ConcurrentDictionary<ObjectSequenceMany, TAggregator> labelValuesDictionary = aggregatorStore.GetLabelValuesDictionary<StringSequenceMany, ObjectSequenceMany>(ref names);
			LabelInstructionInterpreter<ObjectSequenceMany, TAggregator> labelInstructionInterpreter = new LabelInstructionInterpreter<ObjectSequenceMany, TAggregator>(expectedLabels, array, labelValuesDictionary, createAggregatorFunc);
			return labelInstructionInterpreter.GetAggregator;
		}
		}
	}

	private static LabelInstruction[] Compile(ReadOnlySpan<KeyValuePair<string, object>> labels)
	{
		LabelInstruction[] array = new LabelInstruction[labels.Length];
		for (int i = 0; i < labels.Length; i++)
		{
			int num = i;
			int sourceIndex = i;
			KeyValuePair<string, object> keyValuePair = labels[i];
			array[num] = new LabelInstruction(sourceIndex, keyValuePair.Key);
		}
		return array;
	}
}
