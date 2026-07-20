namespace Mono.Cecil.Cil;

public struct InstructionSymbol(int offset, SequencePoint sequencePoint)
{
	public readonly int Offset = offset;

	public readonly SequencePoint SequencePoint = sequencePoint;
}
