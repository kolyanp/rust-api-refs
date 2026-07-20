using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Oxide.Core.Configuration;

namespace Oxide.Core;

public class DataFileSystem
{
	private readonly Dictionary<string, DynamicConfigFile> _datafiles;

	public string Directory { get; private set; }

	public DataFileSystem(string directory)
	{
		Directory = directory;
		_datafiles = new Dictionary<string, DynamicConfigFile>();
		KeyValuesConverter item = new KeyValuesConverter();
		new JsonSerializerSettings().Converters.Add(item);
	}

	public DynamicConfigFile GetFile(string name)
	{
		name = DynamicConfigFile.SanitizeName(name);
		if (_datafiles.TryGetValue(name, out var value))
		{
			return value;
		}
		value = new DynamicConfigFile(Path.Combine(Directory, name + ".json"));
		_datafiles.Add(name, value);
		return value;
	}

	public bool ExistsDatafile(string name)
	{
		return GetFile(name).Exists();
	}

	public DynamicConfigFile GetDatafile(string name)
	{
		DynamicConfigFile file = GetFile(name);
		if (file.Exists())
		{
			file.Load();
		}
		else
		{
			file.Save();
		}
		return file;
	}

	public string[] GetFiles(string path = "", string searchPattern = "*")
	{
		return System.IO.Directory.GetFiles(Path.Combine(Directory, path), searchPattern);
	}

	public void SaveDatafile(string name)
	{
		GetFile(name).Save();
	}

	public T ReadObject<T>(string name)
	{
		T val = default(T);
		if (ExistsDatafile(name))
		{
			val = GetFile(name).ReadObject<T>();
		}
		if (val == null)
		{
			val = Activator.CreateInstance<T>();
			WriteObject(name, val);
		}
		return val;
	}

	public void WriteObject<T>(string name, T Object, bool sync = false)
	{
		GetFile(name).WriteObject(Object, sync);
	}

	public void DeleteDataFile(string name)
	{
		GetFile(name).Delete();
	}

	public void ForEachObject<T>(string name, Action<T> callback)
	{
		string folder = DynamicConfigFile.SanitizeName(name);
		foreach (DynamicConfigFile item in from a in _datafiles
			where a.Key.StartsWith(folder)
			select a.Value)
		{
			callback?.Invoke(item.ReadObject<T>());
		}
	}
}
