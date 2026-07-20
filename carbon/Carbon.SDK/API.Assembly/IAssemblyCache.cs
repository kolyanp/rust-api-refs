using System.Reflection;

namespace API.Assembly;

public interface IAssemblyCache
{
	byte[] Raw { get; }

	string Name { get; }

	System.Reflection.Assembly Assembly { get; }
}
