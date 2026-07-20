using System;

namespace Carbon.Contracts;

public interface IZipScriptProcessor : IScriptProcessor, IBaseProcessor, IDisposable
{
	public interface IZipScript : IProcess, IDisposable
	{
		IScriptLoader Loader { get; set; }
	}
}
