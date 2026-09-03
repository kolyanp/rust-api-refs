using System.Threading.Tasks;
using Facepunch;
using ProtoBuf;

namespace CompanionServer.Handlers;

public interface IHandler : IPooled
{
	AppRequest Request { get; }

	ValidationResult Validate();

	ValueTask Execute();

	void SendError(string code);
}
