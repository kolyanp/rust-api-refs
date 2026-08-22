using System.Collections.Generic;
using System.IO;
using Carbon;
using Carbon.Base;
using Carbon.Core;
using Carbon.Extensions;
using Facepunch;
using Newtonsoft.Json;
using Oxide.Core.Plugins;
using Oxide.Plugins;

namespace Oxide.Core.Libraries;

public class Lang : Library
{
	public Dictionary<string, Dictionary<string, string>> Phrases { get; set; } = new Dictionary<string, Dictionary<string, string>>();

	public Lang(BaseHookable plugin)
	{
		foreach (string item in Directory.EnumerateDirectories(Defines.GetLangFolder()))
		{
			string fileName = Path.GetFileName(item);
			Dictionary<string, string> messageFile = GetMessageFile(plugin.Name, fileName);
			if (messageFile != null)
			{
				Phrases[fileName] = messageFile;
			}
		}
	}

	public string GetLanguage(string userId)
	{
		if (!string.IsNullOrEmpty(userId) && Interface.Oxide.Permission.UserExists(userId, out var data))
		{
			return data.Language;
		}
		return Community.Runtime.Config.Language;
	}

	public string[] GetLanguages(Plugin plugin = null)
	{
		List<string> list = Pool.Get<List<string>>();
		string[] directories = Directory.GetDirectories(Interface.Oxide.LangDirectory);
		foreach (string text in directories)
		{
			if (Directory.GetFiles(text).Length != 0 && (plugin == null || (plugin != null && OsEx.File.Exists(Path.Combine(text, plugin.Name + ".json")))))
			{
				list.Add(text.Substring(Interface.Oxide.LangDirectory.Length + 1));
			}
		}
		string[] result = list.ToArray();
		Pool.FreeUnmanaged<string>(ref list);
		return result;
	}

	public void SetLanguage(string lang, string userId)
	{
		if (!string.IsNullOrEmpty(lang) && !string.IsNullOrEmpty(userId))
		{
			UserData userData = Interface.Oxide.Permission.GetUserData(userId, addIfNotExisting: true);
			if (string.IsNullOrEmpty(userData.Language) || !userData.Language.Equals(lang))
			{
				userData.Language = lang;
			}
		}
	}

	public void SetServerLanguage(string lang)
	{
		if (!string.IsNullOrEmpty(lang) && !(lang == Community.Runtime.Config.Language))
		{
			Community.Runtime.Config.Language = lang;
			Community.Runtime.SaveConfig();
		}
	}

	public string GetServerLanguage()
	{
		return Community.Runtime.Config.Language;
	}

	private Dictionary<string, string> GetMessageFile(string plugin, string lang = "en")
	{
		if (string.IsNullOrEmpty(plugin))
		{
			return null;
		}
		char[] invalidFileNameChars = Path.GetInvalidFileNameChars();
		char[] array = invalidFileNameChars;
		foreach (char oldChar in array)
		{
			lang = lang.Replace(oldChar, '_');
		}
		string file = Path.Combine(Defines.GetLangFolder(), lang, plugin + ".json");
		if (!OsEx.File.Exists(file))
		{
			return null;
		}
		return JsonConvert.DeserializeObject<Dictionary<string, string>>(OsEx.File.ReadText(file));
	}

	private void SaveMessageFile(string plugin, string lang = "en")
	{
		if (Phrases.TryGetValue(lang, out var value))
		{
			string text = Path.Combine(Defines.GetLangFolder(), lang);
			OsEx.Folder.Create(text);
			OsEx.File.Create(Path.Combine(text, plugin + ".json"), JsonConvert.SerializeObject((object)value, (Formatting)1));
		}
	}

	public void RegisterMessages(Dictionary<string, string> newPhrases, BaseHookable plugin, string lang = "en")
	{
		if (!Phrases.TryGetValue(lang, out var value))
		{
			Phrases.Add(lang, value = newPhrases);
		}
		bool flag = false;
		foreach (KeyValuePair<string, string> newPhrase in newPhrases)
		{
			if (!value.TryGetValue(newPhrase.Key, out var _))
			{
				value.Add(newPhrase.Key, newPhrase.Value);
				flag = true;
			}
		}
		if ((newPhrases == value) | flag)
		{
			SaveMessageFile(plugin.Name, lang);
		}
	}

	public string GetMessageByLanguage(string key, Plugin plugin, string lang = "en")
	{
		if (string.IsNullOrEmpty(key) || plugin == null)
		{
			return key;
		}
		return GetMessage(key, plugin, null, lang);
	}

	public string GetMessage(string key, BaseHookable hookable, string player = null, string lang = null)
	{
		if (string.IsNullOrEmpty(lang))
		{
			lang = GetLanguage(player);
		}
		if (Phrases.TryGetValue(lang, out var value) && value.TryGetValue(key, out var value2))
		{
			return value2;
		}
		try
		{
			if (hookable is RustPlugin rustPlugin)
			{
				rustPlugin.ILoadDefaultMessages();
			}
			value = GetMessageFile(hookable.Name, lang);
			if (value.TryGetValue(key, out value2))
			{
				return value2;
			}
		}
		catch
		{
		}
		if (!(lang == "en"))
		{
			return GetMessage(key, hookable, player, "en");
		}
		return key;
	}

	public Dictionary<string, string> GetMessages(string lang, BaseHookable plugin)
	{
		return GetMessageFile(plugin.Name, lang);
	}
}
