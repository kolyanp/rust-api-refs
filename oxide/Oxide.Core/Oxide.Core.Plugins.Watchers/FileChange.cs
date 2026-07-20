using System.IO;

namespace Oxide.Core.Plugins.Watchers;

public sealed class FileChange
{
	public string Name { get; private set; }

	public WatcherChangeTypes ChangeType { get; private set; }

	public FileChange(string name, WatcherChangeTypes changeType)
	{
		Name = name;
		ChangeType = changeType;
	}
}
