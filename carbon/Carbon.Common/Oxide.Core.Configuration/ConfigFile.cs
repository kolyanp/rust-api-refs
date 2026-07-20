using System;
using System.IO;
using Newtonsoft.Json;

namespace Oxide.Core.Configuration;

public abstract class ConfigFile
{
	[JsonIgnore]
	public string Filename { get; private set; }

	protected ConfigFile(string filename)
	{
		Filename = filename;
	}

	public static T Load<T>(string filename) where T : ConfigFile
	{
		T val = (T)Activator.CreateInstance(typeof(T), filename);
		val.Load();
		return val;
	}

	public virtual void Load(string filename = null)
	{
		JsonConvert.PopulateObject(File.ReadAllText(filename ?? Filename), (object)this);
	}

	public virtual void Save(string filename = null)
	{
		string contents = JsonConvert.SerializeObject((object)this, (Formatting)1);
		File.WriteAllText(filename ?? Filename, contents);
	}
}
