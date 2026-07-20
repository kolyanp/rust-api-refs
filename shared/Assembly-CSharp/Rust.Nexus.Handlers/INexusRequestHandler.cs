using Facepunch;
using ProtoBuf.Nexus;

namespace Rust.Nexus.Handlers;

public interface INexusRequestHandler : IPooled
{
	Response Response { get; }

	void Execute();
}
