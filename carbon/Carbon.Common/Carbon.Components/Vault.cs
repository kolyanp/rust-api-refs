using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using Carbon.Core;
using Carbon.Extensions;
using Facepunch;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;

namespace Carbon.Components;

public class Vault
{
	public class Factory : List<Item>, IPooled
	{
		public uint id;

		public void EnterPool()
		{
			id = 0u;
			using (Enumerator enumerator = GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					Item current = enumerator.Current;
					Item item = current;
					Pool.Free<Item>(ref item);
				}
			}
			Clear();
		}

		public void LeavePool()
		{
		}

		public bool HasItem(uint item)
		{
			for (int i = 0; i < base.Count; i++)
			{
				Item item2 = base[i];
				if (item2.id == item)
				{
					return true;
				}
			}
			return false;
		}

		public bool HasItem(uint item, out Item value)
		{
			for (int i = 0; i < base.Count; i++)
			{
				value = base[i];
				if (value.id == item)
				{
					return true;
				}
			}
			value = null;
			return false;
		}

		public Item GetItem(uint item)
		{
			for (int i = 0; i < base.Count; i++)
			{
				Item item2 = base[i];
				if (item2.id == item)
				{
					return item2;
				}
			}
			return null;
		}

		public void AddItem(Item item)
		{
			if (GetItem(item.id) == null)
			{
				Add(item);
			}
		}

		public bool RemoveItem(uint item)
		{
			if (HasItem(item, out var value))
			{
				return Remove(value);
			}
			return false;
		}
	}

	public class Item : IPooled
	{
		public uint id;

		public bool encrypted;

		internal string runtimeId;

		internal byte[] salt;

		internal byte[] hash;

		[CompilerGenerated]
		private string _003CCache_003Ek__BackingField;

		internal string Cache
		{
			get
			{
				if (string.IsNullOrEmpty(_003CCache_003Ek__BackingField))
				{
					_003CCache_003Ek__BackingField = Encoding.UTF8.GetString(encrypted ? DecryptData(hash, CARBON_ID, salt) : hash);
				}
				return _003CCache_003Ek__BackingField;
			}
			[CompilerGenerated]
			set
			{
				_003CCache_003Ek__BackingField = value;
			}
		}

		public void EnterPool()
		{
			id = 0u;
			hash = null;
			salt = null;
			encrypted = false;
			Cache = null;
		}

		public void LeavePool()
		{
		}
	}

	public static class Pool
	{
		private static Dictionary<string, uint> NamePoolString = new Dictionary<string, uint>();

		private static Dictionary<uint, string> NamePoolInt = new Dictionary<uint, string>();

		public static void Save(BinaryWriter writer)
		{
			writer.Write(NamePoolString.Count);
			foreach (KeyValuePair<string, uint> item in NamePoolString)
			{
				writer.Write(item.Key);
				writer.Write(item.Value);
			}
		}

		public static void Load(BinaryReader reader)
		{
			int num = reader.ReadInt32();
			for (int i = 0; i < num; i++)
			{
				string text = reader.ReadString();
				uint num2 = reader.ReadUInt32();
				NamePoolString[text] = num2;
				NamePoolInt[num2] = text;
			}
		}

		public static uint Get(string name)
		{
			if (NamePoolString.TryGetValue(name, out var value))
			{
				return value;
			}
			value = ManifestHash(name);
			NamePoolString[name] = value;
			NamePoolInt[value] = name;
			return value;
		}

		public static string Get(uint hash)
		{
			if (!NamePoolInt.TryGetValue(hash, out var value))
			{
				return null;
			}
			return value;
		}

		private static uint ManifestHash(string str)
		{
			if (!string.IsNullOrEmpty(str))
			{
				return BitConverter.ToUInt32(new MD5CryptoServiceProvider().ComputeHash(Encoding.UTF8.GetBytes(str)), 0);
			}
			return 0u;
		}
	}

	public class Protected : JsonConverter
	{
		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			if (!(value is string text))
			{
				writer.WriteNull();
			}
			else
			{
				writer.WriteValue(ReverseReplacement(text) ?? text);
			}
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			string text = reader.Value?.ToString();
			if (string.IsNullOrEmpty(text))
			{
				return null;
			}
			return ApplyReplacement(text);
		}

		public override bool CanConvert(Type objectType)
		{
			return objectType == typeof(string);
		}
	}

	public static readonly string Global = "global";

	private const int DEFAULT_KEY_BIT_SIZE = 256;

	private const int DEFAULT_MAC_BIT_SIZE = 128;

	private const int DEFAULT_NONCE_BIT_SIZE = 128;

	private const int SALT_SIZE = 16;

	private static readonly SecureRandom RANDOM = new SecureRandom();

	private static readonly List<Factory> FACTORIES = new List<Factory>();

	private static string CARBON_ID_CACHE;

	private static string CARBON_ID => CARBON_ID_CACHE ?? (CARBON_ID_CACHE = JObject.Parse(OsEx.File.ReadText(Path.Combine(Defines.GetRustIdentityFolder(), "carbon.id")))["UID"].ToObject<string>());

	private static byte[] EncryptData(byte[] buffer, string password, out byte[] salt)
	{
		using Rfc2898DeriveBytes rfc2898DeriveBytes = new Rfc2898DeriveBytes(password, salt = RANDOM.GenerateSeed(16));
		return EncryptImpl(buffer, rfc2898DeriveBytes.GetBytes(32));
	}

	private static byte[] DecryptData(byte[] buffer, string password, byte[] salt)
	{
		using Rfc2898DeriveBytes rfc2898DeriveBytes = new Rfc2898DeriveBytes(password, salt);
		return DecryptImpl(buffer, rfc2898DeriveBytes.GetBytes(32));
	}

	private static byte[] DecryptImpl(byte[] encryptedMessage, byte[] key, int nonSecretPayloadLength = 0)
	{
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Expected O, but got Unknown
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Expected O, but got Unknown
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Expected O, but got Unknown
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Expected O, but got Unknown
		ValidateKeyImpl(key);
		if (encryptedMessage == null || encryptedMessage.Length == 0)
		{
			throw new ArgumentException("Encrypted Message Required!", "encryptedMessage");
		}
		using MemoryStream input = new MemoryStream(encryptedMessage);
		using BinaryReader binaryReader = new BinaryReader(input);
		byte[] array = binaryReader.ReadBytes(nonSecretPayloadLength);
		byte[] array2 = binaryReader.ReadBytes(16);
		GcmBlockCipher val = new GcmBlockCipher((IBlockCipher)new AesEngine());
		AeadParameters val2 = new AeadParameters(new KeyParameter(key), 128, array2, array);
		val.Init(false, (ICipherParameters)(object)val2);
		byte[] array3 = binaryReader.ReadBytes(encryptedMessage.Length - nonSecretPayloadLength - array2.Length);
		byte[] array4 = new byte[val.GetOutputSize(array3.Length)];
		int num = val.ProcessBytes(array3, 0, array3.Length, array4, 0);
		val.DoFinal(array4, num);
		return array4;
	}

	private static byte[] EncryptImpl(byte[] messageToEncrypt, byte[] key, byte[] nonSecretPayload = null)
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Expected O, but got Unknown
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Expected O, but got Unknown
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Expected O, but got Unknown
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Expected O, but got Unknown
		ValidateKeyImpl(key);
		if (nonSecretPayload == null)
		{
			nonSecretPayload = Array.Empty<byte>();
		}
		byte[] array = new byte[16];
		RANDOM.NextBytes(array, 0, array.Length);
		GcmBlockCipher val = new GcmBlockCipher((IBlockCipher)new AesEngine());
		AeadParameters val2 = new AeadParameters(new KeyParameter(key), 128, array, nonSecretPayload);
		val.Init(true, (ICipherParameters)(object)val2);
		byte[] array2 = new byte[val.GetOutputSize(messageToEncrypt.Length)];
		int num = val.ProcessBytes(messageToEncrypt, 0, messageToEncrypt.Length, array2, 0);
		val.DoFinal(array2, num);
		using MemoryStream memoryStream = new MemoryStream();
		using BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
		binaryWriter.Write(nonSecretPayload);
		binaryWriter.Write(array);
		binaryWriter.Write(array2);
		return memoryStream.ToArray();
	}

	private static void ValidateKeyImpl(byte[] key)
	{
		if (key == null || key.Length != 32)
		{
			throw new ArgumentException($"Key needs to be {256} bit! actual:{((key != null) ? new int?(key.Length * 8) : ((int?)null))}", "key");
		}
	}

	private static void ClearToPool()
	{
		for (int i = 0; i < FACTORIES.Count; i++)
		{
			Factory factory = FACTORIES[i];
			Pool.Free<Factory>(ref factory);
		}
		FACTORIES.Clear();
	}

	private static Factory CreateFactory(uint id)
	{
		if (id == 0)
		{
			return null;
		}
		Factory factory = GetFactory(id) ?? Pool.Get<Factory>();
		factory.id = id;
		if (!FACTORIES.Contains(factory))
		{
			FACTORIES.Add(factory);
		}
		return factory;
	}

	public static Factory GetFactory(uint id)
	{
		for (int i = 0; i < FACTORIES.Count; i++)
		{
			Factory factory = FACTORIES[i];
			if (factory.id.Equals(id))
			{
				return factory;
			}
		}
		return null;
	}

	public static List<Factory> GetFactories()
	{
		return FACTORIES;
	}

	public static bool Add(string factory, string name, string value, bool encrypted = true)
	{
		if (string.IsNullOrEmpty(factory) || string.IsNullOrEmpty(name) || string.IsNullOrEmpty(value))
		{
			Logger.Warn("Attempted to add a null factory, name or value into the Carbon.Vault");
			return false;
		}
		Factory factory2 = CreateFactory(Pool.Get(factory));
		uint num = Pool.Get(name);
		byte[] salt = null;
		byte[] bytes = Encoding.UTF8.GetBytes(value);
		Item value2;
		bool flag = factory2.HasItem(num, out value2);
		if (value2 == null)
		{
			value2 = Pool.Get<Item>();
		}
		value2.id = num;
		value2.encrypted = encrypted;
		value2.hash = (encrypted ? EncryptData(bytes, CARBON_ID, out salt) : bytes);
		value2.salt = salt;
		value2.Cache = value;
		value2.runtimeId = "{" + factory + ":" + name + "}";
		if (!flag)
		{
			factory2.AddItem(value2);
			Save(silent: true);
		}
		return true;
	}

	public static bool Remove(string factory, string name)
	{
		if (string.IsNullOrEmpty(factory) || string.IsNullOrEmpty(name))
		{
			Logger.Warn("Attempted to remove a non-existent factory or factory item from the Carbon.Vault");
			return false;
		}
		if (CreateFactory(Pool.Get(factory)).RemoveItem(Pool.Get(name)))
		{
			Save(silent: true);
			return true;
		}
		return false;
	}

	public static string Get(string factory, string name)
	{
		if (string.IsNullOrEmpty(factory) || string.IsNullOrEmpty(name))
		{
			Logger.Warn("Provided a null factory or name for retrieving a value from the Carbon.Vault");
			return null;
		}
		Factory factory2 = CreateFactory(Pool.Get(factory));
		uint item = Pool.Get(name);
		if (!factory2.HasItem(item, out var value))
		{
			Logger.Warn("Identifier with '" + name + "' for factory '" + factory + "' does not exist in the Carbon.Vault");
			return null;
		}
		return value.Cache;
	}

	public static void Save(bool silent = false)
	{
		int num = 0;
		int num2 = 0;
		using MemoryStream memoryStream = new MemoryStream();
		using (GZipStream output = new GZipStream(memoryStream, CompressionMode.Compress))
		{
			using BinaryWriter binaryWriter = new BinaryWriter(output);
			Pool.Save(binaryWriter);
			binaryWriter.Write(FACTORIES.Count);
			foreach (Factory fACTORy in FACTORIES)
			{
				num++;
				binaryWriter.Write(fACTORy.id);
				binaryWriter.Write(fACTORy.Count);
				foreach (Item item in fACTORy)
				{
					num2++;
					binaryWriter.Write(item.id);
					binaryWriter.Write(item.hash.Length);
					binaryWriter.Write(item.hash);
					binaryWriter.Write(item.salt != null);
					if (item.salt != null)
					{
						binaryWriter.Write(item.salt.Length);
						binaryWriter.Write(item.salt);
					}
					binaryWriter.Write(item.encrypted);
				}
			}
		}
		OsEx.File.Create(Defines.GetVaultFile(), memoryStream.ToArray());
		if (!silent)
		{
			Logger.Log(string.Format("Saved Carbon.Vault with {0:n0} {1} and {2:n0} {3}", new object[4]
			{
				num,
				num.Plural("factory", "factories"),
				num2,
				num2.Plural("item", "items")
			}));
		}
	}

	public static void Load(bool silent = false)
	{
		if (!OsEx.File.Exists(Defines.GetVaultFile()))
		{
			return;
		}
		ClearToPool();
		using MemoryStream stream = new MemoryStream(OsEx.File.ReadBytes(Defines.GetVaultFile()));
		using GZipStream input = new GZipStream(stream, CompressionMode.Decompress);
		using BinaryReader binaryReader = new BinaryReader(input);
		try
		{
			Pool.Load(binaryReader);
			int num = 0;
			int num2 = binaryReader.ReadInt32();
			for (int i = 0; i < num2; i++)
			{
				Factory factory = Pool.Get<Factory>();
				factory.id = binaryReader.ReadUInt32();
				int num3 = binaryReader.ReadInt32();
				for (int j = 0; j < num3; j++)
				{
					num++;
					Item item = Pool.Get<Item>();
					item.id = binaryReader.ReadUInt32();
					item.hash = binaryReader.ReadBytes(binaryReader.ReadInt32());
					if (binaryReader.ReadBoolean())
					{
						item.salt = binaryReader.ReadBytes(binaryReader.ReadInt32());
					}
					item.encrypted = binaryReader.ReadBoolean();
					item.runtimeId = "{" + Pool.Get(factory.id) + ":" + Pool.Get(item.id) + "}";
					factory.AddItem(item);
				}
				FACTORIES.Add(factory);
			}
			if (!silent)
			{
				Logger.Log(string.Format("Loaded Carbon.Vault with {0:n0} {1} and {2:n0} {3}", new object[4]
				{
					num2,
					num2.Plural("factory", "factories"),
					num,
					num.Plural("item", "items")
				}));
			}
		}
		catch (Exception ex)
		{
			Logger.Error("Failed loading Carbon.Vault", ex);
		}
	}

	public static string ReverseReplacement(string source, bool encrypted = true)
	{
		if (string.IsNullOrEmpty(source))
		{
			return null;
		}
		for (int i = 0; i < FACTORIES.Count; i++)
		{
			Factory factory = FACTORIES[i];
			for (int j = 0; j < factory.Count; j++)
			{
				Item item = factory[j];
				if ((encrypted || !item.encrypted) && item.Cache.Equals(source))
				{
					return item.runtimeId;
				}
			}
		}
		return null;
	}

	public static string ApplyReplacement(string source, bool encrypted = true)
	{
		if (string.IsNullOrEmpty(source))
		{
			return null;
		}
		for (int i = 0; i < FACTORIES.Count; i++)
		{
			Factory factory = FACTORIES[i];
			for (int j = 0; j < factory.Count; j++)
			{
				Item item = factory[j];
				if ((encrypted || !item.encrypted) && source.Equals(item.runtimeId))
				{
					return item.Cache;
				}
			}
		}
		return null;
	}
}
