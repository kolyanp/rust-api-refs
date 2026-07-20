using System.IO;

namespace API.Assembly;

public readonly struct WatchFileEvent(WatcherChangeTypes type, string path, string oldPath, bool isInitial)
{
	public readonly WatcherChangeTypes Type = type;

	public readonly string Path = path;

	public readonly string OldPath = oldPath;

	public readonly bool IsInitial = isInitial;
}
