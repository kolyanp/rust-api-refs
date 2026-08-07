using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Carbon.Base;
using Carbon.Contracts;
using Carbon.Core;
using UnityEngine;

namespace Carbon.Managers;

public class ScriptProcessor : BaseProcessor, IScriptProcessor, IBaseProcessor, IDisposable
{
	public class Script : Process, IScriptProcessor.IScript, IBaseProcessor.IProcess, IDisposable
	{
		public IScriptLoader Loader { get; set; }

		public override IBaseProcessor.IParser Parser => new ScriptParser();

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
				ModLoader.GetCompilationResult(base.File).Clear();
				Loader = new ScriptLoader
				{
					Parser = Parser,
					Mod = Community.Runtime.Plugins,
					Process = this
				};
				Loader.Sources.Add(new BaseSource
				{
					FilePath = base.File,
					FileName = Path.GetFileName(base.File),
					ContextFilePath = base.File,
					ContextFileName = Path.GetFileName(base.File)
				});
				Loader.Load();
			}
			catch (Exception arg)
			{
				Logger.Warn($"Failed processing {Path.GetFileNameWithoutExtension(base.File)}:\n{arg}");
			}
		}
	}

	public class ScriptParser : Parser, IBaseProcessor.IParser
	{
	}

	public override string Name => "Script Processor";

	public override bool EnableWatcher
	{
		get
		{
			if (Community.IsConfigReady)
			{
				return Community.Runtime.Config.Watchers.ScriptWatchers;
			}
			return true;
		}
	}

	public override string Folder => Defines.GetScriptsFolder();

	public override string Extension => ".cs";

	public override float Rate => Community.Runtime.Config.Processors.ScriptProcessingRate;

	public override Type IndexedType => typeof(Script);

	public override void Start()
	{
		base.BlacklistPattern = new string[3] { "backups", "debug", "cszip_dev" };
		base.IncludeSubdirectories = Community.Runtime.Config.Watchers.ScriptWatcherOption == SearchOption.AllDirectories;
		base.Start();
	}

	public bool AllPendingScriptsComplete()
	{
		foreach (KeyValuePair<string, IBaseProcessor.IProcess> item in base.InstanceBuffer)
		{
			if (item.Value is Script { Loader: not null } script && !script.Loader.HasFinished)
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
			if (item.Value is Script { Loader: not null } script && !script.Loader.HasRequires && !script.Loader.HasFinished)
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
			if (item.Value is Script { Loader: not null } script && !script.Loader.IsExtension && !script.Loader.HasFinished)
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
