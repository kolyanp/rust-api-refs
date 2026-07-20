using System;

namespace Carbon.Contracts;

public interface IZipDevScriptProcessor : IScriptProcessor, IBaseProcessor, IDisposable
{
	public interface IZipDebugScript : IProcess, IDisposable
	{
		IScriptLoader Loader { get; set; }
	}
}
