using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using API.Assembly;
using API.Events;
using Carbon;
using Carbon.Components;
using Carbon.Extensions;
using Carbon.Profiler;
using Facepunch;
using Mono.Cecil;
using Mono.Collections.Generic;
using Utility;

namespace Components;

internal sealed class ExtensionManager : AddonManager, IExtensionManager, IAddonManager
{
	public class Resolver : IAssemblyResolver, IDisposable
	{
		internal Dictionary<string, AssemblyDefinition> Cache = new Dictionary<string, AssemblyDefinition>();

		public void Dispose()
		{
			Cache.Clear();
			Cache = null;
		}

		public AssemblyDefinition Resolve(AssemblyNameReference name)
		{
			if (Cache.TryGetValue(name.Name, out var value))
			{
				return value;
			}
			bool flag = false;
			string[] references = _references;
			foreach (string path in references)
			{
				string[] files = Directory.GetFiles(path);
				foreach (string text in files)
				{
					if (PathEx.HasExtension(text, ".dll") && Path.GetFileNameWithoutExtension(text) == name.Name)
					{
						Cache.Add(name.Name, value = AssemblyDefinition.ReadAssembly(text, ReadingParameters));
						flag = true;
					}
					if (flag)
					{
						break;
					}
				}
				if (flag)
				{
					break;
				}
			}
			return value;
		}

		public AssemblyDefinition Resolve(AssemblyNameReference name, ReaderParameters parameters)
		{
			return Resolve(name);
		}
	}

	private readonly string[] _directories = new string[1] { Context.CarbonExtensions };

	private static readonly string[] _references;

	public static Dictionary<string, Assembly> ExtensionAssemblyCache;

	public static Resolver ResolverInstance;

	public static ReaderParameters ReadingParameters;

	internal List<string> _created = new List<string>();

	internal List<string> _changed = new List<string>();

	internal List<string> _deleted = new List<string>();

	internal void Awake()
	{
		FileWatcherManager watcher = Bootstrap.Watcher;
		WatchFolder obj = new WatchFolder
		{
			Filter = "*.dll",
			IncludeSubFolders = false,
			Directory = Context.CarbonExtensions,
			OnEvent = delegate(WatchFileEvent e)
			{
				if (e.IsInitial && e.Type == WatcherChangeTypes.Created && !_created.Contains(e.Path) && !_changed.Contains(e.Path) && !_deleted.Contains(e.Path))
				{
					_created.Add(e.Path);
				}
			}
		};
		WatchFolder item = obj;
		base.Watcher = obj;
		watcher.Watch(item);
	}

	internal void FixedUpdate()
	{
		foreach (string item in _created)
		{
			try
			{
				Load(item, "ExtensionManager.Created");
			}
			catch (Exception message)
			{
				Utility.Logger.Error(message);
			}
		}
		foreach (string item2 in _changed)
		{
			try
			{
				Unload(item2, "ExtensionManager.Changed");
				Load(item2, "ExtensionManager.Changed");
			}
			catch (Exception message2)
			{
				Utility.Logger.Error(message2);
			}
		}
		foreach (string item3 in _deleted)
		{
			try
			{
				Unload(item3, "ExtensionManager.Deleted");
			}
			catch (Exception message3)
			{
				Utility.Logger.Error(message3);
			}
		}
		_created.Clear();
		_changed.Clear();
		_deleted.Clear();
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public override Assembly Load(string file, string requester = null)
	{
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		if (requester == null)
		{
			MethodBase method = new StackFrame(1).GetMethod();
			requester = $"{method.DeclaringType}.{method.Name}";
		}
		Item item = base._loaded.FirstOrDefault((Item x) => x.File == file);
		AssemblyDefinition val = null;
		MemoryStream stream = null;
		ICarbonExtension carbonExtension = null;
		string text = string.Empty;
		Assembly assembly = null;
		if (File.Exists(file) && PathEx.HasExtension(file, ".dll"))
		{
			stream = new MemoryStream(File.ReadAllBytes(file));
			AssemblyDefinition val2 = AssemblyDefinition.ReadAssembly((Stream)stream, ReadingParameters);
			text = ((AssemblyNameReference)val2.Name).Name;
			((AssemblyNameReference)val2.Name).Name = $"{((AssemblyNameReference)val2.Name).Name}_{Guid.NewGuid()}";
			Enumerator<AssemblyNameReference> enumerator = val2.MainModule.AssemblyReferences.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					AssemblyNameReference current = enumerator.Current;
					if (ResolverInstance.Cache.TryGetValue(current.Name, out var value))
					{
						current.Name = ((AssemblyNameReference)value.Name).Name;
					}
				}
			}
			finally
			{
				((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
			}
			ResolverInstance.Cache[text] = val2;
			val = val2;
		}
		if (val == null || string.IsNullOrEmpty(text))
		{
			Dispose();
			return null;
		}
		using (MemoryStream memoryStream = new MemoryStream())
		{
			val.Write((Stream)memoryStream);
			memoryStream.Position = 0L;
			val.Dispose();
			byte[] postProcessedRaw = memoryStream.ToArray();
			assembly = _loader.Load(file, requester, _directories, IExtensionManager.ExtensionTypes.Extension)?.Assembly;
			ExtensionAssemblyCache[assembly.FullName] = assembly;
			bool isProfiledAssembly = MonoProfiler.TryStartProfileFor(MonoProfilerConfig.ProfileTypes.Extension, assembly, Path.GetFileNameWithoutExtension(file));
			Assemblies.Extensions.Update(Path.GetFileNameWithoutExtension(file), assembly, file, isProfiledAssembly);
			if (base.AssemblyManager.IsType<ICarbonExtension>(assembly, out var output))
			{
				string file2 = Path.Combine(Context.CarbonExtensions, text + ".dll");
				if (item == null)
				{
					List<Item> loaded = base._loaded;
					Item obj = new Item
					{
						File = file2
					};
					item = obj;
					loaded.Add(obj);
				}
				item.PostProcessedRaw = postProcessedRaw;
				item.Shared = assembly.GetTypes();
				List<Type> list = new List<Type>();
				if (output != null)
				{
					foreach (Type item2 in output)
					{
						if (Enumerable.Contains<Type>(item2.GetInterfaces(), typeof(ICarbonExtension)))
						{
							carbonExtension = Activator.CreateInstance(item2) as ICarbonExtension;
							Hydrate(assembly, carbonExtension);
							list.Add(item2);
							item.Addon = carbonExtension;
						}
					}
				}
				item.Types = list;
			}
			if (carbonExtension == null)
			{
				Utility.Logger.Error("Failed loading extension '" + file + "'");
				Dispose();
				return null;
			}
			CarbonEventArgs e = Pool.Get<CarbonEventArgs>();
			e.Init(file);
			try
			{
				carbonExtension.Awake(e);
				carbonExtension.OnLoaded(e);
				Bootstrap.Events.Trigger(CarbonEvent.ExtensionLoaded, e);
			}
			catch (Exception ex)
			{
				Utility.Logger.Error("Failed to instantiate module from type '" + text + "' [" + file + "]", ex);
				Bootstrap.Events.Trigger(CarbonEvent.ExtensionLoadFailed, e);
			}
			Pool.Free<CarbonEventArgs>(ref e);
			return assembly;
		}
		void Dispose()
		{
			stream?.Dispose();
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public override void Unload(string file, string requester)
	{
		Item item = base._loaded.FirstOrDefault((Item x) => x.File == file);
		CarbonEventArgs e = Pool.Get<CarbonEventArgs>();
		e.Init(file);
		try
		{
			Bootstrap.Events.Trigger(CarbonEvent.ExtensionUnloaded, e);
			item.Addon.OnUnloaded(EventArgs.Empty);
		}
		catch (Exception ex)
		{
			Utility.Logger.Error("Failed unloading extension '" + file + "' (requested by " + requester + ")", ex);
			Bootstrap.Events.Trigger(CarbonEvent.ExtensionUnloadFailed, e);
		}
		Pool.Free<CarbonEventArgs>(ref e);
		base._loaded.Remove(item);
	}

	static ExtensionManager()
	{
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Expected O, but got Unknown
		_references = new string[4]
		{
			Context.CarbonExtensions,
			Context.CarbonManaged,
			Context.CarbonLib,
			Context.GameManaged
		};
		ExtensionAssemblyCache = new Dictionary<string, Assembly>();
		ReadingParameters = new ReaderParameters
		{
			AssemblyResolver = (IAssemblyResolver)(object)(ResolverInstance = new Resolver())
		};
	}
}
