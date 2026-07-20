using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Carbon.Components;
using Mono.Cecil;
using Mono.Collections.Generic;

namespace Carbon.Publicizer;

public class Patch : IDisposable
{
	public class CarbonAssemblyResolver : BaseAssemblyResolver
	{
		private readonly IDictionary<string, AssemblyDefinition> _cache = new Dictionary<string, AssemblyDefinition>(StringComparer.Ordinal);

		public override AssemblyDefinition Resolve(AssemblyNameReference name)
		{
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Expected O, but got Unknown
			return ((BaseAssemblyResolver)this).Resolve(name, new ReaderParameters());
		}

		public override AssemblyDefinition Resolve(AssemblyNameReference name, ReaderParameters parameters)
		{
			//IL_001a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0021: Expected O, but got Unknown
			if (_cache.TryGetValue(name.FullName, out var value))
			{
				return value;
			}
			if (parameters == null)
			{
				parameters = new ReaderParameters();
			}
			parameters.AssemblyResolver = (IAssemblyResolver)(object)this;
			parameters.InMemory = true;
			string[] searchDirectories = ((BaseAssemblyResolver)this).GetSearchDirectories();
			string[] array = searchDirectories;
			foreach (string path in array)
			{
				if (!Directory.Exists(path))
				{
					continue;
				}
				string[] files = Directory.GetFiles(path, "*.dll", SearchOption.AllDirectories);
				string[] array2 = files;
				foreach (string text in array2)
				{
					string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(text);
					if (fileNameWithoutExtension.Equals(name.Name, StringComparison.OrdinalIgnoreCase))
					{
						try
						{
							value = AssemblyDefinition.ReadAssembly(text, parameters);
						}
						catch
						{
						}
						break;
					}
				}
				if (value != null)
				{
					break;
				}
			}
			_cache[name.FullName] = value;
			return value;
		}

		public void RegisterAssembly(AssemblyDefinition assembly)
		{
			if (assembly == null)
			{
				throw new ArgumentNullException("assembly");
			}
			string fullName = ((AssemblyNameReference)assembly.Name).FullName;
			if (!_cache.ContainsKey(fullName))
			{
				_cache[fullName] = assembly;
			}
		}

		protected override void Dispose(bool disposing)
		{
			foreach (AssemblyDefinition value in _cache.Values)
			{
				value.Dispose();
			}
			_cache.Clear();
			((BaseAssemblyResolver)this).Dispose(disposing);
		}
	}

	public static CarbonAssemblyResolver AssemblyResolver;

	public static List<Patch> Publicized = new List<Patch>();

	public static Dictionary<Assembly, string> PatchMapping = new Dictionary<Assembly, string>();

	public static Action<(string path, byte[] buffer)> onBufferUpdate;

	public static Action<(string path, byte[] buffer)> onPatchUpdate;

	public static string CarbonModifierDirectory;

	public static string CarbonManagedDirectory;

	public static string RustManagedDirectory;

	public static AssemblyDefinition bootstrap;

	public static AssemblyDefinition common;

	public static AssemblyDefinition facepunchSystem;

	public AssemblyDefinition assembly;

	public ReaderParameters readerParameters;

	public byte[] processed;

	public string filePath;

	public string fileName;

	public int modifiers;

	public int members;

	public bool ShouldPublicize => Config.Singleton.Publicizer.PublicizedAssemblies.Any((string x) => fileName.StartsWith(x, StringComparison.OrdinalIgnoreCase));

	public static void Init(string carbonModifierDir, string carbonManagedDir, string rustManagedDir)
	{
		CarbonModifierDirectory = carbonModifierDir;
		CarbonManagedDirectory = carbonManagedDir;
		RustManagedDirectory = rustManagedDir;
		if (!string.IsNullOrEmpty(carbonManagedDir))
		{
			TryLoadCommonAssembly(ref common, Path.Combine(carbonManagedDir, "Carbon.Common.dll"));
			TryLoadCommonAssembly(ref bootstrap, Path.Combine(carbonManagedDir, "Carbon.Bootstrap.dll"));
		}
		if (!string.IsNullOrEmpty(rustManagedDir))
		{
			TryLoadCommonAssembly(ref facepunchSystem, Path.Combine(rustManagedDir, "Facepunch.System.dll"));
		}
		BuiltInPatches.Current = new Patch[2]
		{
			new AssemblyCSharp(),
			new RustHarmony()
		};
		Modifier.CollectAll(carbonModifierDir);
		static void TryLoadCommonAssembly(ref AssemblyDefinition definition, string path)
		{
			if (File.Exists(path))
			{
				definition = AssemblyDefinition.ReadAssembly((Stream)new MemoryStream(File.ReadAllBytes(path)));
			}
		}
	}

	public static void Uninit()
	{
		AssemblyDefinition obj = bootstrap;
		if (obj != null)
		{
			obj.Dispose();
		}
		bootstrap = null;
	}

	public string GetFullPath()
	{
		return Path.Combine(filePath, fileName);
	}

	public Patch(string path, string name)
	{
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Expected O, but got Unknown
		filePath = path;
		fileName = name;
		if (AssemblyResolver == null)
		{
			AssemblyResolver = new CarbonAssemblyResolver();
			((BaseAssemblyResolver)AssemblyResolver).AddSearchDirectory(RustManagedDirectory);
		}
		readerParameters = new ReaderParameters
		{
			AssemblyResolver = (IAssemblyResolver)(object)AssemblyResolver
		};
	}

	public virtual bool Execute()
	{
		assembly = AssemblyDefinition.ReadAssembly((Stream)new MemoryStream(File.ReadAllBytes(GetFullPath())), readerParameters);
		if (!ShouldPublicize)
		{
			return false;
		}
		Modifier.ApplyModifiers(fileName, assembly, ref modifiers, ref members);
		Publicize();
		Publicized.Add(this);
		return true;
	}

	public void UpdateBuffer()
	{
		if (processed != null)
		{
			return;
		}
		using MemoryStream memoryStream = new MemoryStream();
		assembly.Write((Stream)memoryStream);
		processed = memoryStream.ToArray();
		onBufferUpdate?.Invoke((fileName, processed));
	}

	public void Write(string path)
	{
		UpdateBuffer();
		try
		{
			File.WriteAllBytes(path, processed);
			Dispose();
		}
		catch (Exception value)
		{
			Console.WriteLine(value);
		}
	}

	public void Load()
	{
		UpdateBuffer();
		Assembly key = Assembly.Load(processed);
		PatchMapping[key] = Path.Combine(filePath, fileName);
		Console.WriteLine(" Loading patched assembly '" + fileName + "' " + ((modifiers != 0 || members != 0) ? string.Format("[{0:n0} {1}, {2:n0} {3}]", new object[4]
		{
			modifiers,
			(members == 1) ? "mod" : "mods",
			members,
			(members == 1) ? "fld" : "flds"
		}) : string.Empty));
	}

	public void Dispose()
	{
		readerParameters = null;
		AssemblyDefinition obj = assembly;
		if (obj != null)
		{
			obj.Dispose();
		}
		assembly = null;
	}

	public void Publicize()
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		if (assembly == null)
		{
			throw new Exception("Loaded assembly is null: " + GetFullPath());
		}
		Enumerator<TypeDefinition> enumerator = assembly.MainModule.Types.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				TypeDefinition current = enumerator.Current;
				Publicize(current);
			}
		}
		finally
		{
			((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
		}
	}

	private static void Publicize(TypeDefinition type)
	{
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_012e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0133: Unknown result type (might be due to invalid IL or missing references)
		//IL_0222: Unknown result type (might be due to invalid IL or missing references)
		//IL_0227: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			if (Config.Singleton.Publicizer.IsMemberIgnored(((MemberReference)type).Name))
			{
				return;
			}
			if (((TypeReference)type).IsNested)
			{
				type.IsNestedPublic = true;
			}
			else
			{
				type.IsPublic = true;
			}
			Enumerator<MethodDefinition> enumerator = type.Methods.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					MethodDefinition current = enumerator.Current;
					if (!Config.Singleton.Publicizer.IsMemberIgnored(((MemberReference)type).Name + "." + ((MemberReference)current).Name))
					{
						current.IsPublic = true;
					}
				}
			}
			finally
			{
				((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
			}
			Enumerator<FieldDefinition> enumerator2 = type.Fields.GetEnumerator();
			try
			{
				while (enumerator2.MoveNext())
				{
					FieldDefinition current2 = enumerator2.Current;
					if (Config.Singleton.Publicizer.IsMemberIgnored(((MemberReference)type).Name + "." + ((MemberReference)current2).Name))
					{
						continue;
					}
					bool flag = false;
					Enumerator<EventDefinition> enumerator3 = type.Events.GetEnumerator();
					try
					{
						while (enumerator3.MoveNext())
						{
							EventDefinition current3 = enumerator3.Current;
							if (!(((MemberReference)current3).Name != ((MemberReference)current2).Name))
							{
								flag = true;
								break;
							}
						}
					}
					finally
					{
						((IDisposable)enumerator3/*cast due to constrained. prefix*/).Dispose();
					}
					if (flag)
					{
						continue;
					}
					bool flag2 = false;
					Enumerator<CustomAttribute> enumerator4 = current2.CustomAttributes.GetEnumerator();
					try
					{
						while (enumerator4.MoveNext())
						{
							CustomAttribute current4 = enumerator4.Current;
							if (!(((MemberReference)current4.AttributeType).FullName != "UnityEngine.SerializeField"))
							{
								flag2 = true;
								break;
							}
						}
					}
					finally
					{
						((IDisposable)enumerator4/*cast due to constrained. prefix*/).Dispose();
					}
					if (!current2.IsPublic && !flag2)
					{
						current2.IsNotSerialized = true;
					}
					current2.IsPublic = true;
				}
			}
			finally
			{
				((IDisposable)enumerator2/*cast due to constrained. prefix*/).Dispose();
			}
			Enumerator<PropertyDefinition> enumerator5 = type.Properties.GetEnumerator();
			try
			{
				while (enumerator5.MoveNext())
				{
					PropertyDefinition current5 = enumerator5.Current;
					if (current5.GetMethod != null)
					{
						current5.GetMethod.IsPublic = true;
					}
					if (current5.SetMethod != null)
					{
						current5.SetMethod.IsPublic = true;
					}
				}
			}
			finally
			{
				((IDisposable)enumerator5/*cast due to constrained. prefix*/).Dispose();
			}
		}
		catch (Exception ex)
		{
			Console.WriteLine(ex.Message);
			throw ex;
		}
		Enumerator<TypeDefinition> enumerator6 = type.NestedTypes.GetEnumerator();
		try
		{
			while (enumerator6.MoveNext())
			{
				TypeDefinition current6 = enumerator6.Current;
				Publicize(current6);
			}
		}
		finally
		{
			((IDisposable)enumerator6/*cast due to constrained. prefix*/).Dispose();
		}
	}
}
