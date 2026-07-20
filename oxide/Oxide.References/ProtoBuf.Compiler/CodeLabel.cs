using System.Reflection.Emit;

namespace ProtoBuf.Compiler;

internal struct CodeLabel(Label value, int index)
{
	public readonly Label Value = value;

	public readonly int Index = index;
}
