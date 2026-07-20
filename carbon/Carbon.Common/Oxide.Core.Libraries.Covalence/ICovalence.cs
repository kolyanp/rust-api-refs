namespace Oxide.Core.Libraries.Covalence;

public interface ICovalence
{
	IPlayerManager Players { get; }

	IServer Server { get; }
}
