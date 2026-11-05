using System;

public interface IAITarget
{
	Nullable Position { get; }

	bool IsValid(BoatAI self);

	bool IsReached(BoatAI self);
}
