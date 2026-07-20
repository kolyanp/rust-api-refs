using System;
using System.Collections.Generic;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using API.Abstracts;
using API.Contracts;
using Carbon;
using Utility;

namespace Components;

internal sealed class DownloadManager : CarbonBehaviour, IDownloadManager
{
	private sealed class DownloadItem
	{
		public string URL;

		public DateTime Start;

		public string Identifier;

		public Action<string, byte[]> Callback;

		public CancellationToken Token;

		public bool SuppressErrors;

		public CancellationTokenRegistration Registration;

		public WebClient Client;

		public bool Started;

		public bool Completed;
	}

	private const int Concurrency = 4;

	private Queue<DownloadItem> _downloadQueue;

	private int _currentDownloads;

	private string _userAgent;

	private readonly object _downloadSync = new object();

	private void Awake()
	{
		_downloadQueue = new Queue<DownloadItem>();
		try
		{
			_userAgent = $"Carbon v{Assembly.GetExecutingAssembly().GetName().Version}";
		}
		catch
		{
		}
		ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;
		if (Enum.TryParse<SecurityProtocolType>("Tls13", out var result))
		{
			ServicePointManager.SecurityProtocol |= result;
		}
	}

	private void Update()
	{
		while (_downloadQueue.Count > 0 && _currentDownloads < 4)
		{
			DownloadItem job = _downloadQueue.Dequeue();
			if (job.Token.IsCancellationRequested)
			{
				continue;
			}
			WebClient webClient = new WebClient();
			webClient.DownloadDataCompleted += OnDownloadDataCompleted;
			webClient.Headers.Add(HttpRequestHeader.UserAgent, _userAgent);
			webClient.Headers.Add(HttpRequestHeader.CacheControl, "no-store, no-cache, must-revalidate, max-age=0");
			webClient.Headers.Add(HttpRequestHeader.Pragma, "no-cache");
			job.Client = webClient;
			job.Registration = job.Token.Register(delegate
			{
				CancelJob(job);
			});
			if (job.Token.IsCancellationRequested)
			{
				CancelJob(job);
				continue;
			}
			lock (_downloadSync)
			{
				job.Started = true;
				_currentDownloads++;
				job.Start = DateTime.UtcNow;
			}
			try
			{
				webClient.DownloadDataAsync(new Uri(job.URL), job);
			}
			catch (Exception error)
			{
				CompleteJob(job, webClient, null, error, cancelled: false);
			}
		}
	}

	private void OnDownloadDataCompleted(object sender, DownloadDataCompletedEventArgs e)
	{
		WebClient webClient = (WebClient)sender;
		DownloadItem downloadItem = (DownloadItem)e.UserState;
		if (e.Cancelled || downloadItem.Token.IsCancellationRequested)
		{
			CompleteJob(downloadItem, webClient, null, null, cancelled: true);
		}
		else if (e.Error != null)
		{
			CompleteJob(downloadItem, webClient, null, e.Error, cancelled: false);
		}
		else
		{
			CompleteJob(downloadItem, webClient, e.Result, null, cancelled: false);
		}
	}

	private void CancelJob(DownloadItem job)
	{
		WebClient client = job.Client;
		try
		{
			client?.CancelAsync();
		}
		catch (Exception)
		{
		}
		CompleteJob(job, client, null, null, cancelled: true);
	}

	private void CompleteJob(DownloadItem job, WebClient webClient, byte[] result, Exception error, bool cancelled)
	{
		lock (_downloadSync)
		{
			if (job.Completed)
			{
				return;
			}
			job.Completed = true;
			if (job.Started)
			{
				_currentDownloads--;
			}
		}
		try
		{
			if (cancelled)
			{
				return;
			}
			if (error != null)
			{
				if (!job.SuppressErrors)
				{
					Utility.Logger.Error("Download job '" + job.URL + "' failed", error);
				}
				if (job.Callback != null)
				{
					job.Callback(job.Identifier, Array.Empty<byte>());
				}
				else
				{
					Utility.Logger.Error("Download callback is null");
				}
				return;
			}
			if (!Community.Runtime.Config.Logging.ReducedLogging && result != null)
			{
				TimeSpan timeSpan = DateTime.UtcNow - job.Start;
				Utility.Logger.Log("Download job '" + job.URL + "' finished [" + FormatBytes((double)result.LongLength / timeSpan.TotalSeconds) + "/sec]");
			}
			if (job.Callback != null)
			{
				job.Callback(job.Identifier, result);
			}
			else
			{
				Utility.Logger.Error("Download callback is null");
			}
		}
		finally
		{
			job.Registration.Dispose();
			webClient?.Dispose();
		}
	}

	public Task<byte[]> Download(string url, CancellationToken token)
	{
		return Download(url, token, suppressErrors: false);
	}

	public async Task<byte[]> Download(string url, CancellationToken token, bool suppressErrors)
	{
		TaskCompletionSource<byte[]> tcs = new TaskCompletionSource<byte[]>();
		using (token.Register(delegate
		{
			tcs.TrySetCanceled();
		}))
		{
			DownloadItem item = new DownloadItem
			{
				URL = url,
				Callback = delegate(string _, byte[] bytes)
				{
					tcs.TrySetResult(bytes);
				},
				Identifier = $"{Guid.NewGuid():N}",
				Token = token,
				SuppressErrors = suppressErrors
			};
			_downloadQueue.Enqueue(item);
			try
			{
				return await tcs.Task;
			}
			catch (TaskCanceledException)
			{
				return null;
			}
		}
	}

	public Task<byte[]> Download(string url)
	{
		return Download(url, CancellationToken.None);
	}

	public void DownloadAsync(string url, Action<string, byte[]> callback)
	{
		DownloadItem item = new DownloadItem
		{
			URL = url,
			Callback = callback,
			Identifier = $"{Guid.NewGuid():N}",
			Token = CancellationToken.None
		};
		_downloadQueue.Enqueue(item);
	}

	private static string FormatBytes(double bytes)
	{
		string arg;
		if (bytes > 1048576.0)
		{
			arg = "mb";
			bytes /= 1048576.0;
		}
		else if (bytes > 1024.0)
		{
			arg = "kb";
			bytes /= 1024.0;
		}
		else
		{
			arg = "b";
		}
		return $"{bytes:0}{arg}";
	}
}
