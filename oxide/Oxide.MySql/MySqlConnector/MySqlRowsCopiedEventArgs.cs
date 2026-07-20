using System;

namespace MySqlConnector;

public sealed class MySqlRowsCopiedEventArgs : EventArgs
{
	public bool Abort { get; set; }

	public long RowsCopied { get; internal set; }

	internal MySqlRowsCopiedEventArgs()
	{
	}
}
