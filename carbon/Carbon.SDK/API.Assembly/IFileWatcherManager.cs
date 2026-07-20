namespace API.Assembly;

public interface IFileWatcherManager
{
	void Watch(WatchFolder item);

	void Unwatch(WatchFolder item);

	void Unwatch(string directory);
}
