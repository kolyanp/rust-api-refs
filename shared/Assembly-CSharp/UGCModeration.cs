using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ConVar;
using Development.Attributes;
using Facepunch;
using Facepunch.Rust;
using Network;
using Newtonsoft.Json;
using UnityEngine;

[ResetStaticFields]
[ConsoleSystem.Factory("moderation")]
public static class UGCModeration
{
	private class Description
	{
		public DateTime Timestamp;

		public string ServerId;

		public string ServerIp;

		public int ServerPort;

		public string SteamId;

		public string PlayerName;

		public ulong EntityId;

		public string EntityPrefab;

		public string ContentType;

		public int Frame;
	}

	private class PendingUpload
	{
		public Description Description;

		public uint Crc;

		public FileStorage.Type FileType;

		public RealTimeUntil UntilUpload;

		public int Attempts;

		public Task<bool> UploadTask;

		public bool IsSameContent(PendingUpload other)
		{
			if (Description.EntityId == other.Description.EntityId)
			{
				return Description.Frame == other.Description.Frame;
			}
			return false;
		}
	}

	private static readonly MediaTypeHeaderValue BinaryContentType = new MediaTypeHeaderValue("application/octet-stream");

	private static readonly TimeSpan UploadTimeout = TimeSpan.FromSeconds(10.0);

	private static readonly List<PendingUpload> queue = new List<PendingUpload>();

	private static PendingUpload uploading;

	private static RealTimeUntil untilNextUpdate;

	[RconVar(Name = "ugc_upload_url")]
	public static string UploadUrl { get; set; } = "https://api.facepunch.com/ugc/rust/upload";

	[RconVar(Name = "ugc_upload", Saved = true, Help = "Toggle whether player created content (signs, photos, paintings...) are uploaded for moderation")]
	public static bool UploadEnabled { get; set; }

	[ServerVar(Name = "ugc_upload_debounce", Help = "Seconds content waits before it is uploaded for moderation, saving the same content again restarts the wait")]
	public static float UploadDebounce { get; set; } = 120f;

	[ServerVar(Name = "ugc_upload_frequency", Help = "Seconds between checks of the moderation upload queue, at most one upload starts per check")]
	public static float UpdateInterval { get; set; } = 4f;

	[ServerVar(Name = "ugc_upload_retry_cooldown", Help = "Seconds to wait before retrying a failed moderation upload")]
	public static float FailureCooldown { get; set; } = 60f;

	[ServerVar(Name = "ugc_upload_max_attempts", Help = "Number of attempts at uploading content for moderation before giving up")]
	public static int MaxAttempts { get; set; } = 5;

	[ServerVar(Name = "ugc_upload_max_queue", Help = "Maximum content waiting to be uploaded for moderation, the oldest is dropped when full")]
	public static int MaxQueueSize { get; set; } = 1000;

	private static bool Enabled
	{
		get
		{
			if (UploadEnabled)
			{
				return Analytics.Manager.ServerFPUploader.Enabled;
			}
			return false;
		}
	}

	public static void Send(BasePlayer player, BaseNetworkable entity, string contentType, uint crc, FileStorage.Type fileType, int frame = 0)
	{
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		if (Enabled && crc != 0)
		{
			PendingUpload pending = new PendingUpload
			{
				Description = new Description
				{
					Timestamp = DateTime.UtcNow,
					ServerId = ConVar.Server.server_id,
					ServerIp = Net.sv.ip,
					ServerPort = Net.sv.port,
					SteamId = player.UserIDString,
					PlayerName = player.displayName,
					EntityId = entity.net.ID.Value,
					EntityPrefab = entity.ShortPrefabName,
					ContentType = contentType,
					Frame = frame
				},
				Crc = crc,
				FileType = fileType,
				UntilUpload = RealTimeUntil.op_Implicit(UploadDebounce)
			};
			queue.RemoveAll((PendingUpload upload) => upload.IsSameContent(pending));
			queue.Add(pending);
			int num = Mathf.Max(MaxQueueSize, 0);
			if (queue.Count > num)
			{
				queue.RemoveRange(0, queue.Count - num);
			}
		}
	}

	public static void Update()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		if (RealTimeUntil.op_Implicit(untilNextUpdate) > 0f)
		{
			return;
		}
		untilNextUpdate = RealTimeUntil.op_Implicit(UpdateInterval);
		if (uploading != null)
		{
			if (!uploading.UploadTask.IsCompleted)
			{
				return;
			}
			FinishUpload();
		}
		if (queue.Count == 0)
		{
			return;
		}
		if (!Enabled)
		{
			queue.Clear();
			return;
		}
		for (int i = 0; i < queue.Count; i++)
		{
			PendingUpload upload = queue[i];
			if (RealTimeUntil.op_Implicit(upload.UntilUpload) > 0f)
			{
				continue;
			}
			try
			{
				byte[] content = FileStorage.server.GetByCrc(upload.Crc, upload.FileType);
				if (content == null)
				{
					queue.RemoveAt(i);
					i--;
					continue;
				}
				upload.UploadTask = Task.Run(() => TryUpload(upload.Description, content));
				uploading = upload;
				break;
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
				queue.RemoveAt(i);
				i--;
			}
		}
	}

	private static void FinishUpload()
	{
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		Task<bool> uploadTask = uploading.UploadTask;
		if (uploadTask.IsCompletedSuccessfully && uploadTask.Result)
		{
			queue.Remove(uploading);
		}
		else
		{
			if (uploadTask.IsFaulted)
			{
				Debug.LogException((Exception)uploadTask.Exception);
			}
			uploading.Attempts++;
			if (uploading.Attempts >= MaxAttempts)
			{
				Debug.LogWarning((object)$"Gave up uploading {uploading.Description.ContentType} for moderation after {MaxAttempts} attempts");
				queue.Remove(uploading);
			}
			else
			{
				uploading.UntilUpload = RealTimeUntil.op_Implicit(FailureCooldown);
			}
		}
		uploading.UploadTask = null;
		uploading = null;
	}

	private static async Task<bool> TryUpload(Description description, byte[] content)
	{
		string text = JsonConvert.SerializeObject((object)description);
		using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, UploadUrl);
		request.Headers.Add(Analytics.AnalyticsHeader, string.IsNullOrEmpty(Analytics.AnalyticsSecret) ? Analytics.AnalyticsPublicKey : Analytics.AnalyticsSecret);
		ByteArrayContent byteArrayContent = new ByteArrayContent(content);
		byteArrayContent.Headers.ContentType = BinaryContentType;
		request.Content = new MultipartFormDataContent
		{
			{
				new StringContent(text, Encoding.UTF8, "application/json"),
				"description"
			},
			{ byteArrayContent, "content", "content" }
		};
		if (Analytics.Log || Analytics.DryRun)
		{
			Debug.Log((object)$"UGC POST {UploadUrl}: {content.Length} bytes\n{text}");
		}
		if (Analytics.DryRun)
		{
			return true;
		}
		try
		{
			using CancellationTokenSource timeout = new CancellationTokenSource(UploadTimeout);
			using HttpResponseMessage httpResponseMessage = await WebUtil.HttpClient.SendAsync(request, timeout.Token);
			if (!httpResponseMessage.IsSuccessStatusCode && Analytics.Log)
			{
				Debug.Log((object)$"UGC upload failed: {(int)httpResponseMessage.StatusCode} {httpResponseMessage.ReasonPhrase}");
			}
			return httpResponseMessage.IsSuccessStatusCode;
		}
		catch (Exception ex) when (ex is HttpRequestException || ex is OperationCanceledException)
		{
			if (Analytics.Log)
			{
				Debug.Log((object)("HTTP Error when uploading UGC: " + ex.Message));
			}
			return false;
		}
	}
}
