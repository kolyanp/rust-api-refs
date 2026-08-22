using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Carbon;
using Oxide.Core.Plugins;

namespace Oxide.Core.Libraries;

public class WebRequests : Library
{
	public class WebRequest : IDisposable
	{
		public class Client : WebClient
		{
			public int StatusCode { get; private set; }

			public int Timeout { get; set; }

			public DecompressionMethods AutomaticDecompression { get; set; } = DecompressionMethods.GZip;

			public Client()
			{
				base.Encoding = System.Text.Encoding.UTF8;
			}

			protected override WebResponse GetWebResponse(System.Net.WebRequest request, IAsyncResult result)
			{
				WebResponse webResponse = null;
				try
				{
					webResponse = base.GetWebResponse(request, result);
					if (webResponse is HttpWebResponse httpWebResponse)
					{
						StatusCode = (int)httpWebResponse.StatusCode;
					}
				}
				catch (WebException ex)
				{
					if (ex.Response == null)
					{
						throw;
					}
					webResponse = ex.Response;
					if (webResponse is HttpWebResponse httpWebResponse2)
					{
						StatusCode = (int)httpWebResponse2.StatusCode;
					}
				}
				return webResponse;
			}

			protected override WebResponse GetWebResponse(System.Net.WebRequest request)
			{
				WebResponse webResponse = null;
				try
				{
					webResponse = base.GetWebResponse(request);
					if (webResponse is HttpWebResponse httpWebResponse)
					{
						StatusCode = (int)httpWebResponse.StatusCode;
					}
				}
				catch (WebException ex)
				{
					if (ex.Response == null)
					{
						throw;
					}
					webResponse = ex.Response;
					if (webResponse is HttpWebResponse httpWebResponse2)
					{
						StatusCode = (int)httpWebResponse2.StatusCode;
					}
				}
				return webResponse;
			}

			protected override System.Net.WebRequest GetWebRequest(Uri address)
			{
				HttpWebRequest httpWebRequest = base.GetWebRequest(address) as HttpWebRequest;
				if (string.IsNullOrEmpty(httpWebRequest.UserAgent))
				{
					httpWebRequest.UserAgent = Community.Runtime.Analytics.UserAgent;
				}
				httpWebRequest.AutomaticDecompression = AutomaticDecompression;
				if (Timeout > 0)
				{
					httpWebRequest.Timeout = Timeout;
				}
				if (!address.IsLoopback && Community.IsConfigReady && IPAddress.TryParse(Community.Runtime.Config.WebRequestIp, out IPAddress address2))
				{
					IPEndPoint bindEndPoint = new IPEndPoint(address2, 0);
					httpWebRequest.ServicePoint.BindIPEndPointDelegate = (ServicePoint _, IPEndPoint _, int _) => bindEndPoint;
				}
				return httpWebRequest;
			}

			public new void Dispose()
			{
				base.Dispose();
			}
		}

		internal DateTime _time;

		internal bool _data;

		internal Uri _uri;

		internal Client _client;

		public Action<int, string> Callback { get; set; }

		public Action<int, byte[]> DataCallback { get; set; }

		public float Timeout { get; set; }

		public string Method { get; set; }

		public string Url { get; }

		public string Body { get; set; }

		public DecompressionMethods DecompressionMethod { get; set; } = DecompressionMethods.GZip;

		public TimeSpan ResponseDuration { get; protected set; }

		public int ResponseCode { get; protected set; }

		public object ResponseObject { get; protected set; } = string.Empty;

		public Exception ResponseError { get; protected set; }

		public Plugin Owner { get; protected set; }

		public Dictionary<string, string> RequestHeaders { get; set; }

		public WebRequest(string url, Action<int, string> callback, Plugin owner)
		{
			Url = url;
			Callback = callback;
			Owner = owner;
			_uri = new Uri(url);
			_data = false;
		}

		public WebRequest(string url, Action<int, byte[]> callback, Plugin owner)
		{
			Url = url;
			DataCallback = callback;
			Owner = owner;
			_uri = new Uri(url);
			_data = true;
		}

		public WebRequest Start()
		{
			_client = new Client();
			_client.Headers["User-Agent"] = Community.Runtime.Analytics.UserAgent;
			if (Method != "GET")
			{
				_client.Headers["Content-Type"] = "application/x-www-form-urlencoded";
			}
			_client.Credentials = CredentialCache.DefaultCredentials;
			_client.Proxy = null;
			_client.Encoding = Encoding.UTF8;
			_client.AutomaticDecompression = DecompressionMethod;
			float num = ((Timeout <= 0f) ? WebRequests.Timeout : Timeout) * 1000f;
			_client.Timeout = ((num >= 2.1474836E+09f) ? int.MaxValue : ((int)num));
			if (RequestHeaders != null && RequestHeaders.Count > 0)
			{
				foreach (KeyValuePair<string, string> requestHeader in RequestHeaders)
				{
					_client.Headers[requestHeader.Key] = requestHeader.Value;
				}
			}
			switch (Method)
			{
			case "GET":
				_time = DateTime.Now;
				try
				{
					if (_data)
					{
						_client.DownloadDataCompleted += delegate(object _, DownloadDataCompletedEventArgs e)
						{
							ResponseDuration = DateTime.Now - _time;
							ResponseCode = _client.StatusCode;
							try
							{
								if (e == null)
								{
									OnComplete();
								}
								else if (e.Error != null)
								{
									ResponseError = e.Error;
									Logger.Error("Failed executing '" + Method + "' webrequest [response] (" + Url + ")", e.Error);
									OnComplete();
								}
								else
								{
									ResponseObject = e.Result;
									OnComplete();
								}
							}
							catch (Exception ex3)
							{
								Logger.Error("Failed executing '" + Method + "' webrequest [internal] (" + Url + ")", ex3);
								OnComplete();
							}
						};
						_client.DownloadDataAsync(_uri);
						break;
					}
					_client.DownloadStringCompleted += delegate(object _, DownloadStringCompletedEventArgs e)
					{
						ResponseDuration = DateTime.Now - _time;
						ResponseCode = _client.StatusCode;
						try
						{
							if (e == null)
							{
								OnComplete();
							}
							else if (e.Error != null)
							{
								ResponseError = e.Error;
								Logger.Error("Failed executing '" + Method + "' webrequest [response] (" + Url + ")", e.Error);
								OnComplete();
							}
							else
							{
								ResponseObject = e.Result;
								OnComplete();
							}
						}
						catch (Exception ex3)
						{
							Logger.Error("Failed executing '" + Method + "' webrequest [internal] (" + Url + ")", ex3);
							OnComplete();
						}
					};
					_client.DownloadStringAsync(_uri);
				}
				catch (Exception ex2)
				{
					Logger.Error("Failed executing '" + Method + "' webrequest [internal] (" + Url + ")", ex2);
					ResponseCode = _client.StatusCode;
					ResponseError = ex2;
					OnComplete();
				}
				break;
			case "PUT":
			case "PATCH":
			case "POST":
			case "DELETE":
				_time = DateTime.Now;
				try
				{
					if (_data)
					{
						_client.UploadDataCompleted += delegate(object _, UploadDataCompletedEventArgs e)
						{
							ResponseDuration = DateTime.Now - _time;
							ResponseCode = _client.StatusCode;
							try
							{
								if (e == null)
								{
									OnComplete();
								}
								else if (e.Error != null)
								{
									ResponseError = e.Error;
									Logger.Error("Failed executing '" + Method + "' webrequest [response] (" + Url + ")", e.Error);
									OnComplete();
								}
								else
								{
									ResponseObject = e.Result;
									OnComplete();
								}
							}
							catch (Exception ex3)
							{
								Logger.Error("Failed executing '" + Method + "' webrequest [internal] (" + Url + ")", ex3);
								OnComplete();
							}
						};
						_client.UploadDataAsync(_uri, Method, Encoding.Default.GetBytes(Body));
						break;
					}
					_client.UploadStringCompleted += delegate(object _, UploadStringCompletedEventArgs e)
					{
						ResponseDuration = DateTime.Now - _time;
						ResponseCode = _client.StatusCode;
						try
						{
							if (e == null)
							{
								OnComplete();
							}
							else if (e.Error != null)
							{
								ResponseError = e.Error;
								Logger.Error("Failed executing '" + Method + "' webrequest [response] (" + Url + ")", e.Error);
								OnComplete();
							}
							else
							{
								ResponseObject = e.Result;
								OnComplete();
							}
						}
						catch (Exception ex3)
						{
							Logger.Error("Failed executing '" + Method + "' webrequest [internal] (" + Url + ")", ex3);
							OnComplete();
						}
					};
					_client.UploadStringAsync(_uri, Method, string.IsNullOrEmpty(Body) ? string.Empty : Body);
				}
				catch (Exception ex)
				{
					Logger.Error("Failed executing '" + Method + "' webrequest [internal] (" + Url + ")", ex);
					ResponseCode = _client.StatusCode;
					ResponseError = ex;
					OnComplete();
				}
				break;
			}
			return this;
		}

		private void OnComplete()
		{
			Owner?.TrackStart();
			string text = "Web request callback raised an exception";
			if ((bool)Owner && Owner != null)
			{
				text = text + " in '" + Owner.ToPrettyString() + "' plugin";
			}
			try
			{
				if (_data)
				{
					DataCallback?.Invoke(ResponseCode, ResponseObject as byte[]);
				}
				else
				{
					Callback?.Invoke(ResponseCode, ResponseObject?.ToString());
				}
			}
			catch (Exception ex)
			{
				Logger.Error($"{text} [{ResponseCode}]", ex);
			}
			Owner?.TrackEnd();
			Dispose();
		}

		public void Dispose()
		{
			Owner = null;
			_uri = null;
			_client?.Dispose();
			_client = null;
		}
	}

	public static float Timeout = 30f;

	public WebRequests()
	{
		ServicePointManager.Expect100Continue = false;
		ServicePointManager.ServerCertificateValidationCallback = (object sender, X509Certificate cert, X509Chain chain, SslPolicyErrors error) => true;
		ServicePointManager.DefaultConnectionLimit = 200;
	}

	public WebRequest Enqueue(string url, string body, Action<int, string> callback, Plugin owner, RequestMethod method = RequestMethod.GET, Dictionary<string, string> headers = null, float timeout = 0f, DecompressionMethods decompressionMethod = DecompressionMethods.None)
	{
		return new WebRequest(url, callback, owner)
		{
			Method = method.ToString(),
			RequestHeaders = headers,
			Timeout = timeout,
			Body = body,
			DecompressionMethod = decompressionMethod
		}.Start();
	}

	public WebRequest EnqueueData(string url, string body, Action<int, byte[]> callback, Plugin owner, RequestMethod method = RequestMethod.GET, Dictionary<string, string> headers = null, float timeout = 0f, DecompressionMethods decompressionMethod = DecompressionMethods.None)
	{
		return new WebRequest(url, callback, owner)
		{
			Method = method.ToString(),
			RequestHeaders = headers,
			Timeout = timeout,
			Body = body,
			DecompressionMethod = decompressionMethod
		}.Start();
	}

	public async Task<WebRequest> EnqueueAsync(string url, string body, Action<int, string> callback, Plugin owner, RequestMethod method = RequestMethod.GET, Dictionary<string, string> headers = null, float timeout = 0f, DecompressionMethods decompressionMethod = DecompressionMethods.None)
	{
		TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
		WebRequest request = null;
		request = new WebRequest(url, delegate(int code, string data)
		{
			try
			{
				callback?.Invoke(code, data);
			}
			catch (Exception ex)
			{
				Logger.Error("Failed executing '" + request.Method + "' async webrequest [callback] (" + request.Url + ")", ex);
			}
			tcs.SetResult(result: true);
		}, owner)
		{
			Method = method.ToString(),
			RequestHeaders = headers,
			Timeout = timeout,
			Body = body,
			DecompressionMethod = decompressionMethod
		}.Start();
		await tcs.Task;
		return request;
	}

	public async Task<WebRequest> EnqueueDataAsync(string url, string body, Action<int, byte[]> callback, Plugin owner, RequestMethod method = RequestMethod.GET, Dictionary<string, string> headers = null, float timeout = 0f, DecompressionMethods decompressionMethod = DecompressionMethods.None)
	{
		TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
		WebRequest request = null;
		request = new WebRequest(url, delegate(int code, byte[] data)
		{
			try
			{
				callback?.Invoke(code, data);
			}
			catch (Exception ex)
			{
				Logger.Error("Failed executing '" + request.Method + "' async webrequest [callback] (" + request.Url + ")", ex);
			}
			tcs.SetResult(result: true);
		}, owner)
		{
			Method = method.ToString(),
			RequestHeaders = headers,
			Timeout = timeout,
			Body = body,
			DecompressionMethod = decompressionMethod
		}.Start();
		await tcs.Task;
		return request;
	}

	[Obsolete("EnqueueGet is deprecated, use Enqueue instead")]
	public void EnqueueGet(string url, Action<int, string> callback, Plugin owner, Dictionary<string, string> headers = null, float timeout = 0f)
	{
		Enqueue(url, null, callback, owner, RequestMethod.GET, headers, timeout);
	}

	[Obsolete("EnqueuePost is deprecated, use Enqueue instead")]
	public void EnqueuePost(string url, string body, Action<int, string> callback, Plugin owner, Dictionary<string, string> headers = null, float timeout = 0f)
	{
		Enqueue(url, body, callback, owner, RequestMethod.POST, headers, timeout);
	}

	[Obsolete("EnqueuePut is deprecated, use Enqueue instead")]
	public void EnqueuePut(string url, string body, Action<int, string> callback, Plugin owner, Dictionary<string, string> headers = null, float timeout = 0f)
	{
		Enqueue(url, body, callback, owner, RequestMethod.PUT, headers, timeout);
	}
}
