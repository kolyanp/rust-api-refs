using System.Runtime.CompilerServices;

namespace System.Diagnostics.Metrics;

internal struct LabelInstruction(int sourceIndex, string labelName)
{
	public int SourceIndex
	{
		[_003Cb470df79_002Da5b8_002D442e_002D9a19_002Ddc7ef3b9a2cb_003EIsReadOnly]
		get;
	} = sourceIndex;

	public string LabelName
	{
		[_003Cb470df79_002Da5b8_002D442e_002D9a19_002Ddc7ef3b9a2cb_003EIsReadOnly]
		get;
	} = labelName;
}
