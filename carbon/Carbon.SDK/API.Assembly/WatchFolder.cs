using System;

namespace API.Assembly;

public sealed class WatchFolder
{
	public string Directory { get; set; }

	public string Filter { get; set; }

	public bool IncludeSubFolders { get; set; }

	public Action<WatchFileEvent> OnEvent { get; set; }
}
