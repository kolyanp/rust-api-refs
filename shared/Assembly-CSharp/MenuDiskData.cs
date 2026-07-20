using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Facepunch.Math;
using Newtonsoft.Json;
using UnityEngine;

public static class MenuDiskData
{
	[JsonModel]
	public struct ServerSaveData
	{
		public string Name { get; set; }

		public string Address { get; set; }

		public int QueryPort { get; set; }

		public uint LastConnected { get; set; }
	}

	[JsonModel]
	public class MenuData
	{
		public List<int> ReadNotifications { get; set; } = new List<int>();

		public List<ServerSaveData> SavedServers { get; set; } = new List<ServerSaveData>();
	}

	private static MenuData _menuData;

	private static readonly string _menuDataPath = Path.Combine(Application.persistentDataPath, "menu_data.json");

	public static MenuData Data
	{
		get
		{
			if (_menuData == null)
			{
				Load();
			}
			return _menuData;
		}
	}

	public static void Load()
	{
		try
		{
			if (File.Exists(_menuDataPath))
			{
				_menuData = JsonConvert.DeserializeObject<MenuData>(File.ReadAllText(_menuDataPath));
				if (_menuData == null)
				{
					_menuData = new MenuData();
				}
			}
			else
			{
				_menuData = new MenuData();
			}
		}
		catch (NullReferenceException ex)
		{
			Debug.LogWarning((object)("Menu data file corrupted (NRE): " + ex.Message + ". Deleting file and creating a new one."));
			try
			{
				if (File.Exists(_menuDataPath))
				{
					File.Delete(_menuDataPath);
				}
			}
			catch (Exception ex2)
			{
				Debug.LogException(ex2);
			}
			finally
			{
				_menuData = new MenuData();
			}
		}
		catch (Exception ex3)
		{
			Debug.LogException(ex3);
			_menuData = new MenuData();
		}
	}

	public static async Task Save()
	{
		try
		{
			string directoryName = Path.GetDirectoryName(_menuDataPath);
			if (!string.IsNullOrEmpty(directoryName) && !Directory.Exists(directoryName))
			{
				Directory.CreateDirectory(directoryName);
			}
			string contents = JsonConvert.SerializeObject((object)_menuData, (Formatting)1);
			await File.WriteAllTextAsync(_menuDataPath, contents);
		}
		catch (Exception ex)
		{
			Debug.LogException(ex);
		}
	}

	public static void AddRecentServer(string name, string address, int queryPort)
	{
		ServerSaveData item = new ServerSaveData
		{
			Name = name,
			Address = address,
			QueryPort = queryPort,
			LastConnected = (uint)Epoch.Current
		};
		Data.SavedServers.RemoveAll((ServerSaveData s) => s.Address.Equals(address, StringComparison.OrdinalIgnoreCase) && s.QueryPort == queryPort);
		Data.SavedServers.Insert(0, item);
		if (Data.SavedServers.Count > 3)
		{
			Data.SavedServers.RemoveRange(3, Data.SavedServers.Count - 3);
		}
	}
}
