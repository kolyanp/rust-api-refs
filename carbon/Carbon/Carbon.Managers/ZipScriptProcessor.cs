using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using Carbon.Base;
using Carbon.Components;
using Carbon.Contracts;
using Carbon.Core;
using Carbon.Extensions;
using UnityEngine;

namespace Carbon.Managers;

public class ZipScriptProcessor : BaseProcessor, IZipScriptProcessor, IScriptProcessor, IBaseProcessor, IDisposable
{
	public class ZipScript : Process, IScriptProcessor.IScript, IBaseProcessor.IProcess, IDisposable
	{
		public IScriptLoader Loader { get; set; }

		public override IBaseProcessor.IParser Parser => new ZipScriptParser();

		public override void Clear()
		{
			try
			{
				Loader?.Clear();
			}
			catch (Exception ex)
			{
				Logger.Error("Error clearing " + base.File, ex);
			}
		}

		public override void Dispose()
		{
			try
			{
				Loader?.Dispose();
			}
			catch (Exception ex)
			{
				Logger.Error("Error disposing " + base.File, ex);
			}
		}

		public override void Execute(IBaseProcessor processor)
		{
			try
			{
				ModLoader.GetCompilationResult(base.File, clear: true);
				if (!OsEx.File.Exists(base.File))
				{
					Dispose();
					return;
				}
				Loader = new ScriptLoader
				{
					Parser = Parser,
					Mod = Community.Runtime.ZipPlugins,
					Process = this,
					BypassFileNameChecks = true
				};
				using (ZipArchive zipArchive = ZipFile.OpenRead(base.File))
				{
					foreach (ZipArchiveEntry entry in zipArchive.Entries)
					{
						using StreamReader streamReader = new StreamReader(entry.Open());
						Loader.Sources.Add(new BaseSource
						{
							ContextFilePath = base.File,
							ContextFileName = Path.GetFileName(base.File),
							FilePath = entry.FullName,
							FileName = entry.Name,
							Content = streamReader.ReadToEnd()
						});
					}
				}
				Loader.Load();
			}
			catch (Exception arg)
			{
				Logger.Warn($"Failed processing {Path.GetFileNameWithoutExtension(base.File)}:\n{arg}");
			}
		}
	}

	public class ZipScriptParser : Parser, IBaseProcessor.IParser
	{
		internal const string FOOT = "FindObjectsOfType";

		public override void Process(string file, string input, out string output)
		{
			using (TimeMeasure.New("ScriptParser.Process"))
			{
				try
				{
					if (input.Contains("FindObjectsOfType"))
					{
						Logger.Warn(" Warning! '" + Path.GetFileNameWithoutExtension(file) + "' uses UnityEngine.GameObject.FindObjectsOfType. That may cause significant performance drops, and/or server stalls. Report to the developer or use at your own discretion!");
					}
					output = input.Replace("PluginTimers", "Timers");
				}
				catch
				{
					output = input;
				}
			}
		}
	}

	public override string Name => "ZipScript Processor";

	public override bool EnableWatcher
	{
		get
		{
			if (Community.IsConfigReady)
			{
				return Community.Runtime.Config.Watchers.ZipScriptWatchers;
			}
			return true;
		}
	}

	public override string Folder => Defines.GetScriptsFolder();

	public override string Extension => ".cszip";

	public override float Rate => Community.Runtime.Config.Processors.ZipScriptProcessingRate;

	public override Type IndexedType => typeof(ZipScript);

	public override void Start()
	{
		base.BlacklistPattern = new string[2] { "backups", "debug" };
		base.IncludeSubdirectories = Community.Runtime.Config.Watchers.ScriptWatcherOption == SearchOption.AllDirectories;
		base.Start();
	}

	public bool AllPendingScriptsComplete()
	{
		foreach (KeyValuePair<string, IBaseProcessor.IProcess> item in base.InstanceBuffer)
		{
			if (item.Value is ZipScript { Loader: not null } zipScript && !zipScript.Loader.HasFinished)
			{
				return false;
			}
		}
		return true;
	}

	public bool AllNonRequiresScriptsComplete()
	{
		foreach (KeyValuePair<string, IBaseProcessor.IProcess> item in base.InstanceBuffer)
		{
			if (item.Value is ZipScript { Loader: not null } zipScript && !zipScript.Loader.HasRequires && !zipScript.Loader.HasFinished)
			{
				return false;
			}
		}
		return true;
	}

	public bool AllExtensionsComplete()
	{
		foreach (KeyValuePair<string, IBaseProcessor.IProcess> item in base.InstanceBuffer)
		{
			if (item.Value is ZipScript { Loader: not null } zipScript && !zipScript.Loader.IsExtension && !zipScript.Loader.HasFinished)
			{
				return false;
			}
		}
		return true;
	}

	void IScriptProcessor.StartCoroutine(IEnumerator coroutine)
	{
		((MonoBehaviour)this).StartCoroutine(coroutine);
	}

	void IScriptProcessor.StopCoroutine(IEnumerator coroutine)
	{
		((MonoBehaviour)this).StopCoroutine(coroutine);
	}

	void IScriptProcessor.InvokeRepeating(Action action, float delay, float repeat)
	{
		((FacepunchBehaviour)this).InvokeRepeating(action, delay, repeat);
	}
}
