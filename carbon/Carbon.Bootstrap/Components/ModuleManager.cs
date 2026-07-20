using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using API.Assembly;
using API.Events;
using API.Logger;
using Carbon;
using Carbon.Components;
using Carbon.Extensions;
using Carbon.Profiler;
using Facepunch;
using HarmonyLib;
using Mono.Cecil;
using Mono.Collections.Generic;
using Utility;

namespace Components;

internal sealed class ModuleManager : AddonManager
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

	private static readonly string[] _references = new string[5]
	{
		Context.CarbonModules,
		Context.CarbonExtensions,
		Context.CarbonManaged,
		Context.CarbonLib,
		Context.GameManaged
	};

	public static Dictionary<string, Assembly> ModuleAssemblyCache = new Dictionary<string, Assembly>();

	public static Resolver ResolverInstance;

	public static ReaderParameters ReadingParameters = new ReaderParameters
	{
		AssemblyResolver = (IAssemblyResolver)(object)(ResolverInstance = new Resolver())
	};

	internal void Awake()
	{
		FileWatcherManager watcher = Bootstrap.Watcher;
		WatchFolder obj = new WatchFolder
		{
			Filter = "*.dll",
			IncludeSubFolders = false,
			Directory = Context.CarbonModules,
			OnEvent = delegate(WatchFileEvent e)
			{
				if (e.IsInitial && e.Type == WatcherChangeTypes.Created)
				{
					Load(e.Path, "ModuleManager.Created");
				}
			}
		};
		WatchFolder item = obj;
		base.Watcher = obj;
		watcher.Watch(item);
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public override Assembly Load(string file, string requester = null)
	{
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		Item item = base._loaded.FirstOrDefault((Item x) => x.File == file);
		AssemblyDefinition val = null;
		MemoryStream stream = null;
		IModulePackage modulePackage = null;
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
			byte[] array = memoryStream.ToArray();
			assembly = Assembly.Load(array);
			ModuleAssemblyCache[assembly.FullName] = assembly;
			bool isProfiledAssembly = MonoProfiler.TryStartProfileFor(MonoProfilerConfig.ProfileTypes.Module, assembly, Path.GetFileNameWithoutExtension(file));
			Assemblies.Modules.Update(Path.GetFileNameWithoutExtension(file), assembly, file, isProfiledAssembly);
			if (base.AssemblyManager.IsType<IModulePackage>(assembly, out var output))
			{
				string file2 = Path.Combine(Context.CarbonModules, text + ".dll");
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
				item.PostProcessedRaw = array;
				item.Shared = assembly.GetTypes();
				List<Type> list = new List<Type>();
				if (output != null)
				{
					foreach (Type item2 in output)
					{
						if (Enumerable.Contains<Type>(item2.GetInterfaces(), typeof(IModulePackage)))
						{
							modulePackage = Activator.CreateInstance(item2) as IModulePackage;
							Hydrate(assembly, modulePackage);
							list.Add(item2);
							item.Addon = modulePackage;
						}
					}
				}
				item.Types = list;
			}
			if (modulePackage == null)
			{
				Utility.Logger.Error("Failed loading module '" + file + "'");
				Dispose();
				return null;
			}
			try
			{
				CarbonEventArgs e = Pool.Get<CarbonEventArgs>();
				e.Init(file);
				modulePackage.Awake(e);
				modulePackage.OnLoaded(e);
				Pool.Free<CarbonEventArgs>(ref e);
				ModuleEventArgs e2 = Pool.Get<ModuleEventArgs>();
				e2.Init(file, modulePackage, item.Shared);
				Bootstrap.Events.Trigger(CarbonEvent.ModuleLoaded, e2);
				Pool.Free<ModuleEventArgs>(ref e2);
			}
			catch (Exception ex)
			{
				Utility.Logger.Error("Failed to instantiate module from type '" + text + "' [" + file + "]", ex);
				ModuleEventArgs e3 = Pool.Get<ModuleEventArgs>();
				e3.Init(file, modulePackage, item.Shared);
				Bootstrap.Events.Trigger(CarbonEvent.ModuleLoadFailed, e3);
				Pool.Free<ModuleEventArgs>(ref e3);
			}
			Dispose();
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
		if (item == null)
		{
			Utility.Logger.Log("Couldn't find module '" + file + "' (requested by " + requester + ")");
			return;
		}
		try
		{
			ModuleEventArgs e = Pool.Get<ModuleEventArgs>();
			e.Init(file, (IModulePackage)item.Addon, null);
			Bootstrap.Events.Trigger(CarbonEvent.ModuleUnloaded, e);
			Pool.Free<ModuleEventArgs>(ref e);
			item.Addon.OnUnloaded(EventArgs.Empty);
		}
		catch (Exception ex)
		{
			Utility.Logger.Error("Failed unloading module '" + file + "' (requested by " + requester + ")", ex);
			ModuleEventArgs e2 = Pool.Get<ModuleEventArgs>();
			e2.Init(file, (IModulePackage)item.Addon, null);
			Bootstrap.Events.Trigger(CarbonEvent.ModuleUnloadFailed, e2);
			Pool.Free<ModuleEventArgs>(ref e2);
		}
		base._loaded.Remove(item);
	}

	internal override void Hydrate(Assembly assembly, ICarbonAddon addon)
	{
		base.Hydrate(assembly, addon);
		Type logger = typeof(ILogger) ?? throw new Exception();
		Type events = typeof(IEventManager) ?? throw new Exception();
		Type[] types = assembly.GetTypes();
		foreach (Type type in types)
		{
			foreach (FieldInfo item in from x in type.GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
				where logger.IsAssignableFrom(x.FieldType)
				select x)
			{
				item.SetValue(assembly, Activator.CreateInstance(AccessTools.TypeByName("Carbon.Logger") ?? null));
			}
			foreach (FieldInfo item2 in from x in type.GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
				where events.IsAssignableFrom(x.FieldType)
				select x)
			{
				item2.SetValue(assembly, Bootstrap.Events);
			}
		}
	}
}
