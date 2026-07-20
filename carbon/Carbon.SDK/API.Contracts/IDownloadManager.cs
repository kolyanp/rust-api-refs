using System;
using System.Threading;
using System.Threading.Tasks;

namespace API.Contracts;

public interface IDownloadManager
{
	Task<byte[]> Download(string url);

	Task<byte[]> Download(string url, CancellationToken token);

	Task<byte[]> Download(string url, CancellationToken token, bool suppressErrors);

	void DownloadAsync(string url, Action<string, byte[]> callback);
}
