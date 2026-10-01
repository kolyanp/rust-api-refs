using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using Carbon.Base;
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
				Logger.Error("Error clearing " + File, ex);
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
				Logger.Error("Error disposing " + File, ex);
			}
		}

		public override void Execute(IBaseProcessor processor)
		{
			try
			{
				ModLoader.GetCompilationResult(File, clear: true);
				if (!OsEx.File.Exists(File))
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
				using (ZipArchive zipArchive = ZipFile.OpenRead(File))
				{
					foreach (ZipArchiveEntry entry in zipArchive.Entries)
					{
						using StreamReader streamReader = new StreamReader(entry.Open());
						Loader.Sources.Add(new BaseSource
						{
							ContextFilePath = File,
							ContextFileName = Path.GetFileName(File),
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
				Logger.Warn($"Failed processing {Path.GetFileNameWithoutExtension(File)}:\n{arg}");
			}
		}
	}

	public class ZipScriptParser : Parser, IBaseProcessor.IParser
	{
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
		BlacklistPattern = new string[2] { "backups", "debug" };
		IncludeSubdirectories = Community.Runtime.Config.Watchers.ScriptWatcherOption == SearchOption.AllDirectories;
		base.Start();
	}

	public bool AllPendingScriptsComplete()
	{
		foreach (KeyValuePair<string, IBaseProcessor.IProcess> item in InstanceBuffer)
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
		foreach (KeyValuePair<string, IBaseProcessor.IProcess> item in InstanceBuffer)
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
		foreach (KeyValuePair<string, IBaseProcessor.IProcess> item in InstanceBuffer)
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
