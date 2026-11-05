using System;
using System.Collections.Generic;
using Development.Attributes;
using Facepunch;
using ProtoBuf;
using UnityEngine;

[ResetStaticFields]
public static class TelephoneManager
{
	public const int MaxPhoneNumber = 99990000;

	public const int MinPhoneNumber = 10000000;

	[ServerVar(Help = "(Generated) Maximum number of simultaneous active telephone calls allowed on the server at any time")]
	public static int MaxConcurrentCalls = 10;

	[ServerVar(Help = "(Generated) Maximum duration in seconds a telephone call can remain active before it is automatically terminated")]
	public static int MaxCallLength = 120;

	public static Dictionary<int, PhoneController> allTelephones = new Dictionary<int, PhoneController>();

	private static Dictionary<int, NPCPhoneNumber> npcPhones = new Dictionary<int, NPCPhoneNumber>();

	public static int maxAssignedPhoneNumber = 99990000;

	public static IReadOnlyCollection<PhoneController> AllTelephones => allTelephones.Values;

	public static IReadOnlyCollection<NPCPhoneNumber> AllNPCPhones => npcPhones.Values;

	public static bool IsNumberInUse(int number)
	{
		if (!allTelephones.ContainsKey(number))
		{
			return npcPhones.ContainsKey(number);
		}
		return true;
	}

	public static int GetUnusedTelephoneNumber()
	{
		int num = Random.Range(10000000, 99990000);
		int num2 = 0;
		int num3 = 1000;
		while (IsNumberInUse(num) && num2 < num3)
		{
			num2++;
			num = Random.Range(10000000, 99990000);
		}
		if (num2 == num3)
		{
			num = maxAssignedPhoneNumber + 1;
		}
		maxAssignedPhoneNumber = Mathf.Max(maxAssignedPhoneNumber, num);
		return num;
	}

	public static void RegisterTelephone(PhoneController t, bool checkPhoneNumber = false)
	{
		if (checkPhoneNumber && allTelephones.ContainsKey(t.PhoneNumber) && (Object)(object)allTelephones[t.PhoneNumber] != (Object)(object)t)
		{
			t.PhoneNumber = GetUnusedTelephoneNumber();
		}
		if (!allTelephones.ContainsKey(t.PhoneNumber) && t.PhoneNumber != 0)
		{
			allTelephones.Add(t.PhoneNumber, t);
			maxAssignedPhoneNumber = Mathf.Max(maxAssignedPhoneNumber, t.PhoneNumber);
		}
	}

	public static void DeregisterTelephone(PhoneController t)
	{
		allTelephones.Remove(t.PhoneNumber);
	}

	public static PhoneController GetTelephone(int number)
	{
		return allTelephones.GetValueOrDefault(number);
	}

	public static void RegisterNPCPhone(NPCPhoneNumber phoneNumber)
	{
		DeregisterNPCPhone(phoneNumber);
		if (phoneNumber.Number == 0 || IsNumberInUse(phoneNumber.Number))
		{
			phoneNumber.Number = GetUnusedTelephoneNumber();
		}
		npcPhones.Add(phoneNumber.Number, phoneNumber);
		maxAssignedPhoneNumber = Mathf.Max(maxAssignedPhoneNumber, phoneNumber.Number);
	}

	public static void DeregisterNPCPhone(NPCPhoneNumber phoneNumber)
	{
		if (npcPhones.TryGetValue(phoneNumber.Number, out var value) && value == phoneNumber)
		{
			npcPhones.Remove(phoneNumber.Number);
			return;
		}
		int num = 0;
		foreach (KeyValuePair<int, NPCPhoneNumber> npcPhone in npcPhones)
		{
			if (npcPhone.Value == phoneNumber)
			{
				num = npcPhone.Key;
				break;
			}
		}
		if (num != 0)
		{
			npcPhones.Remove(num);
		}
	}

	public static NPCPhoneNumber GetNPCPhone(int number)
	{
		return npcPhones.GetValueOrDefault(number);
	}

	public static PhoneController GetRandomTelephone(int ignoreNumber)
	{
		foreach (KeyValuePair<int, PhoneController> allTelephone in allTelephones)
		{
			if (allTelephone.Value.PhoneNumber != ignoreNumber)
			{
				return allTelephone.Value;
			}
		}
		return null;
	}

	public static int GetCurrentActiveCalls()
	{
		int num = 0;
		foreach (KeyValuePair<int, PhoneController> allTelephone in allTelephones)
		{
			if (allTelephone.Value.serverState != Telephone.CallState.Idle)
			{
				num++;
			}
		}
		if (num == 0)
		{
			return 0;
		}
		return num / 2;
	}

	public static void GetPhoneDirectory(BasePlayer player, int ignoreNumber, int page, int perPage, PhoneDirectory directory)
	{
		directory.entries = Pool.Get<List<DirectoryEntry>>();
		int startIndex = page * perPage;
		int count = 0;
		if (!AddPhonesToDirectory(playerOnly: true) && !AddPhonesToDirectory(playerOnly: false) && !AddNPCPhonesToDirectory())
		{
			directory.atEnd = true;
		}
		bool AddNPCPhonesToDirectory()
		{
			foreach (KeyValuePair<int, NPCPhoneNumber> npcPhone in npcPhones)
			{
				if (npcPhone.Key != ignoreNumber && !((Object)(object)npcPhone.Value.GetConversationFor(player) == (Object)null))
				{
					count++;
					if (count >= startIndex)
					{
						DirectoryEntry val = Pool.Get<DirectoryEntry>();
						Phrase name = npcPhone.Value.Name;
						val.phoneName = name.english;
						val.phoneNameToken = name.token;
						val.phoneNumber = npcPhone.Key;
						directory.entries.Add(val);
						if (directory.entries.Count >= perPage)
						{
							directory.atEnd = false;
							return true;
						}
					}
				}
			}
			return false;
		}
		bool AddPhonesToDirectory(bool playerOnly)
		{
			foreach (KeyValuePair<int, PhoneController> allTelephone in allTelephones)
			{
				if (allTelephone.Key != ignoreNumber && !string.IsNullOrEmpty(allTelephone.Value.PhoneName) && (!playerOnly || allTelephone.Value.CanModifyPhoneName) && (playerOnly || !allTelephone.Value.CanModifyPhoneName))
				{
					count++;
					if (count >= startIndex)
					{
						DirectoryEntry val = Pool.Get<DirectoryEntry>();
						val.phoneName = allTelephone.Value.GetDirectoryName();
						val.phoneNumber = allTelephone.Value.PhoneNumber;
						directory.entries.Add(val);
						if (directory.entries.Count >= perPage)
						{
							directory.atEnd = false;
							return true;
						}
					}
				}
			}
			return false;
		}
	}

	[ServerVar(Help = "(Generated) Prints a table of all registered telephone entities showing their number, directory name, and world position")]
	public static void PrintAllPhones(ConsoleSystem.Arg arg)
	{
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_014d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0159: Unknown result type (might be due to invalid IL or missing references)
		//IL_0165: Unknown result type (might be due to invalid IL or missing references)
		TextTable val = Pool.Get<TextTable>();
		try
		{
			val.AddColumns(new string[4] { "Number", "Name", "Type", "Position" });
			foreach (KeyValuePair<int, PhoneController> allTelephone in allTelephones)
			{
				Vector3 position = ((Component)allTelephone.Value).transform.position;
				val.AddRow(new string[4]
				{
					allTelephone.Key.ToString(),
					allTelephone.Value.GetDirectoryName(),
					"PhoneController",
					$"{position.x} {position.y} {position.z}"
				});
			}
			foreach (KeyValuePair<int, NPCPhoneNumber> npcPhone in npcPhones)
			{
				Vector3 position2 = ((Component)npcPhone.Value.Provider.GetEntity()).transform.position;
				val.AddRow(new string[4]
				{
					npcPhone.Key.ToString(),
					npcPhone.Value.Name.english,
					"NPC",
					$"{position2.x} {position2.y} {position2.z}"
				});
			}
			arg.ReplyWith(((object)val).ToString());
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}
}
