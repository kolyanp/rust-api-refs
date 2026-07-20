using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
		if (!ExistsDatafile(name))
		{
			T val = Activator.CreateInstance<T>();
			WriteObject(name, val);
			return val;
		}
		return GetFile(name).ReadObject<T>();
	}

	public void WriteObject<T>(string name, T Object, bool sync = false)
	{
		GetFile(name).WriteObject(Object, sync);
	}

	public void ForEachObject<T>(string name, Action<T> callback)
	{
		string folder = DynamicConfigFile.SanitizeName(name);
		foreach (DynamicConfigFile item in _datafiles.Where(delegate(KeyValuePair<string, DynamicConfigFile> d)
		{
			KeyValuePair<string, DynamicConfigFile> keyValuePair = d;
			return keyValuePair.Key.StartsWith(folder);
		}).Select(delegate(KeyValuePair<string, DynamicConfigFile> a)
		{
			KeyValuePair<string, DynamicConfigFile> keyValuePair = a;
			return keyValuePair.Value;
		}))
		{
			callback?.Invoke(item.ReadObject<T>());
		}
	}

	public void DeleteDataFile(string name)
	{
		GetFile(name).Delete();
	}
}
