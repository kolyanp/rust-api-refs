using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;

namespace Carbon;

public class Assemblies
{
	public class RuntimeAssembly
	{
		public Assembly CurrentAssembly { get; internal set; }

		public bool IsProfiledAssembly { get; internal set; }

		public string Location { get; internal set; }

		public List<RuntimeAssembly> History { get; internal set; }
	}

	public class RuntimeAssemblyBank : ConcurrentDictionary<string, RuntimeAssembly>
	{
		public RuntimeAssembly Get(string key)
		{
			TryGetValue(key, out var value);
			return value;
		}

		public KeyValuePair<string, RuntimeAssembly> Find(Assembly assembly)
		{
			using (IEnumerator<KeyValuePair<string, RuntimeAssembly>> enumerator = GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					KeyValuePair<string, RuntimeAssembly> current = enumerator.Current;
					if (current.Value.CurrentAssembly == assembly)
					{
						return current;
					}
					foreach (RuntimeAssembly item in current.Value.History)
					{
						if (item.CurrentAssembly == assembly)
						{
							return new KeyValuePair<string, RuntimeAssembly>(current.Key, item);
						}
					}
				}
			}
			return default(KeyValuePair<string, RuntimeAssembly>);
		}

		public void Update(string key, Assembly assembly, string location, bool isProfiledAssembly = false)
		{
			if (string.IsNullOrEmpty(key))
			{
				Logger.Warn("RuntimeAssemblyBank.Update key == null");
				return;
			}
			if (assembly == null)
			{
				Logger.Warn("RuntimeAssemblyBank.Update assembly == null");
				return;
			}
			AddOrUpdate(key, (string _) => new RuntimeAssembly
			{
				CurrentAssembly = assembly,
				IsProfiledAssembly = isProfiledAssembly,
				Location = location,
				History = new List<RuntimeAssembly>()
			}, delegate(string _, RuntimeAssembly existent)
			{
				if (existent.CurrentAssembly != null)
				{
					existent.History.Insert(0, new RuntimeAssembly
					{
						CurrentAssembly = existent.CurrentAssembly,
						IsProfiledAssembly = existent.IsProfiledAssembly,
						Location = existent.Location
					});
				}
				existent.CurrentAssembly = assembly;
				existent.Location = location;
				existent.IsProfiledAssembly = isProfiledAssembly;
				return existent;
			});
		}
	}

	public static RuntimeAssemblyBank Plugins { get; } = new RuntimeAssemblyBank();

	public static RuntimeAssemblyBank Modules { get; } = new RuntimeAssemblyBank();

	public static RuntimeAssemblyBank Extensions { get; } = new RuntimeAssemblyBank();

	public static RuntimeAssemblyBank Harmony { get; } = new RuntimeAssemblyBank();
}
