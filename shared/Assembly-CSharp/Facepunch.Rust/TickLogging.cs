using System;
using UnityEngine;

namespace Facepunch.Rust;

public class TickLogging
{
	private static int tickUploaderInterval = 60;

	public static AnalyticsTable TickTable = new AnalyticsTable(GetTickTableName(), TimeSpan.FromSeconds((double)tick_uploader_lifetime), AnalyticsDocumentMode.CSV);

	[ServerVar]
	[Help("time (in seconds) before the tick uploader is disposed and recreated")]
	public static int tick_uploader_lifetime
	{
		get
		{
			return tickUploaderInterval;
		}
		set
		{
			tickUploaderInterval = value;
			TimeSpan uploadInterval = TimeSpan.FromSeconds((double)tickUploaderInterval);
			TickTable.UploadInterval = uploadInterval;
		}
	}

	private static string GetTickTableName()
	{
		BuildInfo current = BuildInfo.Current;
		bool num = (current.Scm.Branch != null && current.Scm.Branch == "experimental/release") || current.Scm.Branch == "release";
		bool isEditor = Application.isEditor;
		string text = ((num && !isEditor) ? "release" : (isEditor ? "editor" : "staging"));
		return "player_ticks_" + text;
	}

	public static void RegisterForAnalytics(AnalyticsManager manager)
	{
		manager.AddTable(TickTable, manager.AzureBulkUploader);
	}
}
