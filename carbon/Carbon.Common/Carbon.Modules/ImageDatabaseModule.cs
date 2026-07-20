using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using Carbon.Base;
using Carbon.Core;
using Carbon.Extensions;
using Carbon.Pooling;
using Facepunch;
using Oxide.Core;
using Oxide.Core.Libraries;
using Oxide.Plugins;
using ProtoBuf;
using QRCoder;
using UnityEngine;

namespace Carbon.Modules;

public class ImageDatabaseModule : CarbonModule<ImageDatabaseConfig, EmptyModuleData>
{
	public class ImageQueue : IPooled
	{
		public bool IsDone;

		public WebRequests.WebRequest.Client Client;

		private Timer _timeout;

		private int _index;

		private bool _poolInit;

		public List<string> ImageUrls { get; internal set; } = new List<string>();

		public List<ImageQueueResult> Result { get; internal set; } = new List<ImageQueueResult>();

		public Action<List<ImageQueueResult>> ResultAction { get; set; }

		public void Init()
		{
			CreateTimeout();
			MoveNext();
		}

		public void MoveNext()
		{
			if (_index >= ImageUrls.Count)
			{
				IsDone = true;
				return;
			}
			string text = ImageUrls[_index];
			Client.DownloadDataAsync(new Uri(text), text);
			_index++;
		}

		public void CreateTimeout()
		{
			_timeout = Community.Runtime.Core.timer.In(Singleton.ConfigInstance.TimeoutPerUrl * (float)ImageUrls.Count, delegate
			{
				if (IsDone)
				{
					return;
				}
				try
				{
					if (Result.Count > 0)
					{
						ResultAction?.Invoke(Result);
					}
				}
				catch (Exception ex)
				{
					Logger.Error("Failed timeout process", ex);
				}
			});
		}

		public void EnterPool()
		{
			IsDone = false;
			ImageUrls.Clear();
			Result.Clear();
			ResultAction = null;
			_timeout?.Reset();
			_index = 0;
		}

		public void LeavePool()
		{
			if (_poolInit)
			{
				return;
			}
			Client = new WebRequests.WebRequest.Client();
			Client.Headers.Add("User-Agent", Community.Runtime.Analytics.UserAgent);
			Client.Credentials = CredentialCache.DefaultCredentials;
			Client.Proxy = null;
			Client.DownloadDataCompleted += delegate(object _, DownloadDataCompletedEventArgs e)
			{
				if (e.Error == null)
				{
					Result.Add(new ImageQueueResult
					{
						Url = (string)e.UserState,
						Data = e.Result
					});
				}
				Community.Runtime.Core.NextFrame(MoveNext);
			};
			_poolInit = true;
		}
	}

	public struct ImageQueueResult
	{
		public string Url;

		public byte[] Data;

		public uint CRC;

		public bool Success;
	}

	public static ImageDatabaseModule Singleton;

	internal Dictionary<string, string> _defaultImages = new Dictionary<string, string>
	{
		["carbonb"] = "https://cdn.carbonmod.gg/carbonlogo_b.png",
		["carbonw"] = "https://cdn.carbonmod.gg/carbonlogo_w.png",
		["carbonbs"] = "https://cdn.carbonmod.gg/carbonlogo_bs.png",
		["carbonws"] = "https://cdn.carbonmod.gg/carbonlogo_ws.png",
		["cflogo"] = "https://cdn.carbonmod.gg/content/codefling-logo.png",
		["checkmark"] = "https://cdn.carbonmod.gg/content/checkmark.png",
		["umodlogo"] = "https://cdn.carbonmod.gg/content/umod-logo.png",
		["clouddl"] = "https://cdn.carbonmod.gg/content/cloud-dl.png",
		["trashcan"] = "https://cdn.carbonmod.gg/content/trash-can.png",
		["shopping"] = "https://cdn.carbonmod.gg/content/shopping-cart.png",
		["installed"] = "https://cdn.carbonmod.gg/content/installed.png",
		["reload"] = "https://cdn.carbonmod.gg/content/reload.png",
		["update-pending"] = "https://cdn.carbonmod.gg/content/update-pending.png",
		["magnifying-glass"] = "https://cdn.carbonmod.gg/content/magnifying-glass.png",
		["filter"] = "https://cdn.carbonmod.gg/content/filter.png",
		["star"] = "https://cdn.carbonmod.gg/content/star.png",
		["glow"] = "https://cdn.carbonmod.gg/content/glow.png",
		["gear"] = "https://cdn.carbonmod.gg/content/gear.png",
		["sort"] = "https://cdn.carbonmod.gg/content/sort.png",
		["close"] = "https://cdn.carbonmod.gg/content/close.png",
		["fade"] = "https://cdn.carbonmod.gg/content/fade.png",
		["graph"] = "https://cdn.carbonmod.gg/content/graph.png",
		["maximize"] = "https://cdn.carbonmod.gg/content/maximize.png",
		["minimize"] = "https://cdn.carbonmod.gg/content/minimize.png",
		["folder"] = "https://cdn.carbonmod.gg/content/folder.png",
		["file"] = "https://cdn.carbonmod.gg/content/file.png",
		["translate"] = "https://cdn.carbonmod.gg/content/translate.png",
		["cf_hero"] = "https://cdn.carbonmod.gg/content/cf_hero.png",
		["umod_hero"] = "https://cdn.carbonmod.gg/content/umod_hero.png",
		["installed_hero"] = "https://cdn.carbonmod.gg/content/installed_hero.png",
		["hero_fade"] = "https://cdn.carbonmod.gg/content/hero_fade.png",
		["fade_flip"] = "https://cdn.carbonmod.gg/content/fade_flip.png",
		["empty_star"] = "https://cdn.carbonmod.gg/content/empty_star.png",
		["half_star"] = "https://cdn.carbonmod.gg/content/half_star.png",
		["full_star"] = "https://cdn.carbonmod.gg/content/full_star.png",
		["top_left"] = "https://cdn.carbonmod.gg/content/top_left.png",
		["default_profile"] = "https://cdn.carbonmod.gg/content/default_profile.jpg",
		["bsod"] = "https://cdn.carbonmod.gg/content/bsod.png"
	};

	internal const int MaximumBytes = 4104304;

	public override string Name => "ImageDatabase";

	public override Type Type => typeof(ImageDatabaseModule);

	public override VersionNumber Version => new VersionNumber(1, 0, 0);

	public override bool EnabledByDefault => true;

	public override bool ForceEnabled => true;

	internal ImageDatabaseDataProto _protoData { get; set; }

	internal string _getProtoDataPath()
	{
		return Path.Combine(Defines.GetModulesFolder(), Name, "data.db");
	}

	[ConsoleCommand("imagedb.loaddefaults")]
	[AuthLevel(2)]
	private void LoadDefaults(Arg arg)
	{
		LoadDefaultImages(forced: true);
		arg.ReplyWith("Loading all default images.");
	}

	[ConsoleCommand("imagedb.deleteimage")]
	[AuthLevel(2)]
	private void DeleteImg(Arg arg)
	{
		arg.ReplyWith(DeleteImage(arg.GetString(0, "")) ? "Deleted image" : "Couldn't delete image. Probably because it doesn't exist");
	}

	[ConsoleCommand("imagedb.clearinvalid")]
	[AuthLevel(2)]
	private void ClearInvalid(Arg arg)
	{
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		Dictionary<uint, CacheData> dictionary = Pool.Get<Dictionary<uint, CacheData>>();
		foreach (KeyValuePair<uint, CacheData> item in FileStorage.server._cache)
		{
			if (item.Value.data.Length >= 4104304)
			{
				dictionary.Add(item.Key, item.Value);
			}
		}
		foreach (KeyValuePair<uint, CacheData> item2 in dictionary)
		{
			FileStorage.server.Remove(item2.Key, (Type)0, item2.Value.entityID);
		}
		arg.ReplyWith($"Removed {dictionary.Count:n0} invalid stored files from FileStorage (above the maximum size of {4104304.Format().ToUpper()}).");
		Pool.FreeUnmanaged<uint, CacheData>(ref dictionary);
	}

	public override void Init()
	{
		base.Init();
		Singleton = this;
	}

	public override void OnServerInit(bool initial)
	{
		base.OnServerInit(initial);
		if (initial)
		{
			if (Validate())
			{
				Save();
			}
			if (base.ConfigInstance.LoadDefaultImagesOnStartup)
			{
				LoadDefaultImages();
			}
		}
	}

	public override void OnServerSaved()
	{
		base.OnServerSaved();
		SaveDatabase();
	}

	public override void Load()
	{
		string file = _getProtoDataPath();
		if (OsEx.File.Exists(file))
		{
			using MemoryStream memoryStream = new MemoryStream(OsEx.File.ReadBytes(file));
			try
			{
				_protoData = Serializer.Deserialize<ImageDatabaseDataProto>((Stream)memoryStream);
			}
			catch
			{
				_protoData = new ImageDatabaseDataProto();
				Save();
			}
		}
		else
		{
			_protoData = new ImageDatabaseDataProto();
		}
		base.Load();
	}

	public override void Save()
	{
		base.Save();
		SaveDatabase();
	}

	public void SaveDatabase()
	{
		string text = _getProtoDataPath();
		OsEx.Folder.Create(Path.GetDirectoryName(text));
		using MemoryStream memoryStream = new MemoryStream();
		Serializer.Serialize<ImageDatabaseDataProto>((Stream)memoryStream, _protoData ?? (_protoData = new ImageDatabaseDataProto()));
		byte[] array = memoryStream.ToArray();
		OsEx.File.Create(text, array);
		Array.Clear(array, 0, array.Length);
		array = null;
	}

	private void LoadDefaultImages(bool forced = false)
	{
		Queue(forced, _defaultImages);
	}

	public override bool PreLoadShouldSave(bool newConfig, bool newData)
	{
		bool result = false;
		if (_protoData.Map == null)
		{
			_protoData.Map = new Dictionary<string, uint>();
			result = true;
		}
		if (_protoData.CustomMap == null)
		{
			_protoData.CustomMap = new Dictionary<string, string>();
			result = true;
		}
		return result;
	}

	public bool Validate()
	{
		if (_protoData.Identifier != ((BaseNetworkable)CommunityEntity.ServerInstance).net.ID.Value)
		{
			PutsWarn($"The server identifier has changed. Wiping old image database. [old {_protoData.Identifier}, new {((BaseNetworkable)CommunityEntity.ServerInstance).net.ID.Value}]");
			_protoData.CustomMap.Clear();
			_protoData.Map.Clear();
			_protoData.Identifier = ((BaseNetworkable)CommunityEntity.ServerInstance).net.ID.Value;
			return true;
		}
		if (!HasImage("checkmark"))
		{
			_protoData.CustomMap.Clear();
			_protoData.Map.Clear();
			return true;
		}
		return false;
	}

	public void QueueBatch(bool @override, IEnumerable<string> urls)
	{
		if (urls == null || !urls.Any())
		{
			return;
		}
		int urlCount = urls.Count();
		QueueBatch(@override, delegate(List<ImageQueueResult> results)
		{
			//IL_009b: Unknown result type (might be due to invalid IL or missing references)
			foreach (ImageQueueResult item in results.Where((ImageQueueResult result) => result.Data != null && result.Data.Length != 0))
			{
				if (item.Data.Length >= 4104304)
				{
					Puts($"Failed storing {urlCount:n0} jobs: {item.Data.Length} more or equal than {4104304}");
				}
				else
				{
					uint num = FileStorage.server.Store(item.Data, (Type)0, new NetworkableId(_protoData.Identifier), 0u);
					if (num != 0)
					{
						_protoData.Map[GetId(item.Url)] = num;
					}
				}
			}
		}, urls);
	}

	public void QueueBatch(bool @override, Action<List<ImageQueueResult>> onComplete, IEnumerable<string> urls)
	{
		if (urls == null || !urls.Any())
		{
			return;
		}
		ImageQueue imageQueue = Pool.Get<ImageQueue>();
		List<ImageQueueResult> existent = Pool.Get<List<ImageQueueResult>>();
		int urlCount = urls.Count();
		try
		{
			imageQueue.ImageUrls.AddRange(urls);
			if (!@override)
			{
				foreach (string url in urls)
				{
					uint image = GetImage(url);
					if (image != 0)
					{
						existent.Add(new ImageQueueResult
						{
							CRC = image,
							Url = url,
							Success = true
						});
						imageQueue.ImageUrls.Remove(url);
					}
				}
			}
			else
			{
				foreach (string imageUrl in imageQueue.ImageUrls)
				{
					DeleteAllImages(imageUrl);
				}
			}
		}
		catch (Exception ex)
		{
			Logger.Error("Failed processing queue batch", ex);
		}
		((MonoBehaviour)Community.Runtime.Core.persistence).StartCoroutine(RunQueue(imageQueue, delegate(List<ImageQueueResult> results)
		{
			//IL_0079: Unknown result type (might be due to invalid IL or missing references)
			try
			{
				if (results != null)
				{
					for (int i = 0; i < results.Count; i++)
					{
						ImageQueueResult value = results[i];
						if (value.Data.Length >= 4104304)
						{
							Puts($"Failed storing {urlCount:n0} jobs: {value.Data.Length} more or equal than {4104304}");
						}
						else
						{
							uint num = FileStorage.server.Store(value.Data, (Type)0, new NetworkableId(_protoData.Identifier), 0u);
							if (num != 0)
							{
								_protoData.Map[GetId(value.Url)] = num;
								value.Success = true;
								value.CRC = num;
								results[i] = value;
							}
						}
					}
					results.InsertRange(0, existent);
					onComplete?.Invoke(results);
				}
			}
			catch (Exception ex2)
			{
				PutsError($"Failed QueueBatch of {urls.Count():n0}", ex2);
			}
			Pool.FreeUnmanaged<ImageQueueResult>(ref existent);
		}));
	}

	public void Queue(bool @override, Dictionary<string, string> mappedUrls)
	{
		if (mappedUrls == null || mappedUrls.Count == 0)
		{
			return;
		}
		List<string> list = new List<string>();
		foreach (KeyValuePair<string, string> mappedUrl in mappedUrls)
		{
			list.Add(mappedUrl.Value);
			AddMap(mappedUrl.Key, mappedUrl.Value);
		}
		QueueBatch(@override, list);
	}

	public void Queue(bool @override, Action<List<ImageQueueResult>> onComplete, Dictionary<string, string> mappedUrls)
	{
		if (mappedUrls == null || mappedUrls.Count == 0)
		{
			return;
		}
		List<string> list = new List<string>();
		foreach (KeyValuePair<string, string> mappedUrl in mappedUrls)
		{
			list.Add(mappedUrl.Value);
			AddMap(mappedUrl.Key, mappedUrl.Value);
		}
		QueueBatch(@override, onComplete, list);
	}

	public void Queue(Dictionary<string, string> mappedUrls)
	{
		if (mappedUrls != null && mappedUrls.Count != 0)
		{
			Queue(@override: false, mappedUrls);
		}
	}

	public void Queue(Action<List<ImageQueueResult>> onComplete, Dictionary<string, string> mappedUrls)
	{
		if (mappedUrls != null && mappedUrls.Count != 0)
		{
			Queue(@override: false, onComplete, mappedUrls);
		}
	}

	public void Queue(bool @override, string key, string url)
	{
		if (!string.IsNullOrEmpty(key) && !string.IsNullOrEmpty(url))
		{
			AddMap(key, url);
			QueueBatch(@override, new _003C_003Ez__ReadOnlySingleElementList<string>(url));
		}
	}

	public void Queue(bool @override, string url)
	{
		Queue(@override, url, url);
	}

	public void Queue(string key, string url)
	{
		Queue(@override: false, key, url);
	}

	public void Queue(string url)
	{
		Queue(@override: false, url);
	}

	public void AddImage(string keyOrUrl, byte[] imageData, Type type = (Type)0)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		_protoData.Map[keyOrUrl] = FileStorage.server.Store(imageData, type, new NetworkableId(_protoData.Identifier), 0u);
	}

	public void AddMap(string key, string url)
	{
		_protoData.CustomMap[key] = url;
	}

	public void RemoveMap(string key)
	{
		if (_protoData.CustomMap.ContainsKey(key))
		{
			_protoData.CustomMap.Remove(key);
		}
	}

	public string GetKeyImage(string key)
	{
		if (_protoData.CustomMap.TryGetValue(key, out var value))
		{
			return value;
		}
		return null;
	}

	public uint GetImage(string keyOrUrl)
	{
		if (string.IsNullOrEmpty(keyOrUrl))
		{
			return 0u;
		}
		if (_protoData.CustomMap.TryGetValue(keyOrUrl, out var value))
		{
			keyOrUrl = value;
		}
		string id = GetId(keyOrUrl);
		if (_protoData.Map.TryGetValue(id, out var value2))
		{
			return value2;
		}
		return 0u;
	}

	public string GetImageString(string keyOrUrl)
	{
		return GetImage(keyOrUrl).ToString();
	}

	public void SendImage(BasePlayer player, string name)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		uint image = GetImage(name);
		if (image != 0)
		{
			byte[] array = FileStorage.server.Get(image, (Type)0, new NetworkableId(_protoData.Identifier), 0u);
			if (array != null)
			{
				((BaseEntity)CommunityEntity.ServerInstance).ClientRPC(RpcTarget.Player("CL_ReceiveFilePng", player), image, (uint)array.Length, (ReadOnlySpan<byte>)array, 0u, (byte)0);
			}
		}
	}

	public bool HasImage(string keyOrUrl)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		return FileStorage.server.Get(GetImage(keyOrUrl), (Type)0, new NetworkableId(_protoData.Identifier), 0u) != null;
	}

	public bool DeleteImage(string url)
	{
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		string id = GetId(url);
		if (!_protoData.Map.TryGetValue(id, out var value))
		{
			return false;
		}
		FileStorage.server.Remove(value, (Type)0, new NetworkableId(_protoData.Identifier));
		_protoData.Map.Remove(id);
		return true;
	}

	public void DeleteAllImages(string url)
	{
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		Dictionary<string, uint> dictionary = Pool.Get<Dictionary<string, uint>>();
		foreach (KeyValuePair<string, uint> item in _protoData.Map)
		{
			dictionary.Add(item.Key, item.Value);
		}
		foreach (KeyValuePair<string, uint> item2 in dictionary.Where((KeyValuePair<string, uint> x) => x.Key.StartsWith(url)))
		{
			FileStorage.server.Remove(item2.Value, (Type)0, new NetworkableId(_protoData.Identifier));
			_protoData.Map.Remove(item2.Key);
		}
		Pool.FreeUnmanaged<string, uint>(ref dictionary);
	}

	public uint GetQRCode(string text, int pixels = 20, bool transparent = false, bool quietZones = true, bool whiteMode = false)
	{
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Expected O, but got Unknown
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Expected O, but got Unknown
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Expected O, but got Unknown
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		if (_protoData.Map.TryGetValue($"qr_{Community.Protect(text)}_{pixels}_0", out var value))
		{
			return value;
		}
		if (text.StartsWith("http"))
		{
			Url val = new Url(text);
			text = ((object)val).ToString();
		}
		QRCodeGenerator val2 = new QRCodeGenerator();
		try
		{
			QRCodeData val3 = val2.CreateQrCode(text, (ECCLevel)2, false, false, (EciMode)0, -1);
			try
			{
				QRCode val4 = new QRCode(val3);
				try
				{
					Bitmap graphic = val4.GetGraphic(pixels, whiteMode ? Color.White : Color.Black, transparent ? Color.Transparent : (whiteMode ? Color.Black : Color.White), quietZones);
					using MemoryStream memoryStream = new MemoryStream();
					((Image)graphic).Save((Stream)memoryStream, ImageFormat.Png);
					((Image)graphic).Dispose();
					byte[] array = memoryStream.ToArray();
					value = FileStorage.server.Store(array, (Type)0, new NetworkableId(_protoData.Identifier), 0u);
					_protoData.Map.Add($"qr_{Community.Protect(text)}_{pixels}_0", value);
					return value;
				}
				finally
				{
					((IDisposable)val4)?.Dispose();
				}
			}
			finally
			{
				((IDisposable)val3)?.Dispose();
			}
		}
		finally
		{
			((IDisposable)val2)?.Dispose();
		}
	}

	public static string GetId(string url)
	{
		return url;
	}

	public static IEnumerator RunQueue(ImageQueue imageQueue, Action<List<ImageQueueResult>> callback)
	{
		imageQueue.Init();
		while (!imageQueue.IsDone)
		{
			yield return null;
		}
		callback?.Invoke(imageQueue.Result);
		Pool.FreeUnsafe<ImageQueue>(ref imageQueue);
	}

	public override object InternalCallHook(uint hook, object[] args)
	{
		//IL_0141: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		object obj = ((args?.Length > 0) ? args[0] : null);
		try
		{
			switch (hook)
			{
			case 3338439234u:
			{
				bool flag = ((obj is Arg || obj == null) ? true : false);
				bool flag4 = flag;
				Arg arg3 = ((!flag4) ? ((Arg)null) : ((Arg)(obj ?? null)));
				if (flag4)
				{
					ClearInvalid(arg3);
					return null;
				}
				break;
			}
			case 2641566827u:
			{
				bool flag = ((obj is Arg || obj == null) ? true : false);
				bool flag3 = flag;
				Arg arg2 = ((!flag3) ? ((Arg)null) : ((Arg)(obj ?? null)));
				if (flag3)
				{
					DeleteImg(arg2);
					return null;
				}
				break;
			}
			case 986171947u:
				LoadDefaultImages(obj is bool flag5 && flag5);
				return null;
			case 2550838889u:
			{
				bool flag = ((obj is Arg || obj == null) ? true : false);
				bool flag2 = flag;
				Arg arg = ((!flag2) ? ((Arg)null) : ((Arg)(obj ?? null)));
				if (flag2)
				{
					LoadDefaults(arg);
					return null;
				}
				break;
			}
			case 948911233u:
				return _getProtoDataPath();
			}
		}
		catch (Exception ex)
		{
			Logger.Error(string.Format("Failed to call internal hook '{0}' on module '{1} v{2}' [{3}]", new object[4]
			{
				HookStringPool.GetOrAdd(hook),
				Name,
				Version,
				hook
			}), ex);
			OnException(hook);
		}
		return null;
	}
}
