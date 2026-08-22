using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using Carbon.Publicizer;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Collections.Generic;
using Newtonsoft.Json;
using ProtoBuf;
using ProtoBuf.Meta;

namespace Carbon.Components;

public class Modifier
{
	public class Field
	{
		public string Name;

		public string Type;

		public object DefaultValue;

		public bool IsStatic;

		public bool ShouldSave;

		public Field WithName(string value)
		{
			Name = value;
			return this;
		}

		public Field WithType(string value)
		{
			Type = value;
			return this;
		}

		public Field WithDefaultValue(object value)
		{
			DefaultValue = value;
			return this;
		}

		public Field WithStatic(bool wants)
		{
			IsStatic = wants;
			return this;
		}

		public Field WithSave(bool wants)
		{
			ShouldSave = wants;
			return this;
		}

		public bool Validate()
		{
			if (!string.IsNullOrEmpty(Name))
			{
				return !string.IsNullOrEmpty(Type);
			}
			return false;
		}
	}

	public static readonly string DataType = "CarbonData";

	[JsonIgnore]
	public string Path;

	public string Assembly;

	public string Name;

	public List<Field> Fields = new List<Field>();

	public static ModifierBank Active = new ModifierBank();

	public static ModifierBank Read(string path)
	{
		ModifierBank modifierBank = ((!File.Exists(path)) ? null : JsonConvert.DeserializeObject<ModifierBank>(File.ReadAllText(path)));
		if (modifierBank != null)
		{
			for (int i = 0; i < modifierBank.Count; i++)
			{
				modifierBank[i].Path = path;
			}
		}
		return modifierBank;
	}

	public Modifier WithAssembly(string value)
	{
		Assembly = value;
		return this;
	}

	public Modifier WithName(string value)
	{
		Name = value;
		return this;
	}

	public Modifier WithField(Field value)
	{
		Fields.Add(value);
		return this;
	}

	public Modifier WithPath(string value)
	{
		Path = value;
		return this;
	}

	public bool Validate()
	{
		if (!string.IsNullOrEmpty(Assembly))
		{
			return !string.IsNullOrEmpty(Name);
		}
		return false;
	}

	public bool HasSavedFields()
	{
		for (int i = 0; i < Fields.Count; i++)
		{
			if (Fields[i].ShouldSave)
			{
				return true;
			}
		}
		return false;
	}

	public int GetInvalidMembers()
	{
		int num = 0;
		for (int i = 0; i < Fields.Count; i++)
		{
			if (!Fields[i].Validate())
			{
				num++;
			}
		}
		return num;
	}

	public void ClearInvalidMembers()
	{
		for (int i = 0; i < Fields.Count; i++)
		{
			if (!Fields[i].Validate())
			{
				Fields.RemoveAt(i);
				i--;
			}
		}
	}

	internal static void CollectAll(string directory)
	{
		if (string.IsNullOrEmpty(directory) || !Directory.Exists(directory))
		{
			return;
		}
		Active.Clear();
		string[] files = Directory.GetFiles(directory);
		foreach (string path in files)
		{
			try
			{
				Active.AddRange(Read(path));
			}
			catch (Exception ex)
			{
				Console.WriteLine("Failed reading modifier file " + System.IO.Path.GetFileName(path) + " (" + ex.Message + ")\n" + ex.StackTrace);
			}
		}
		if (Active.Count <= 0)
		{
			return;
		}
		int num = 0;
		int num2 = 0;
		for (int j = 0; j < Active.Count; j++)
		{
			Modifier modifier = Active[j];
			if (!modifier.Validate())
			{
				num++;
				Active.RemoveAt(j);
				j--;
			}
			else
			{
				num2 += modifier.GetInvalidMembers();
				modifier.ClearInvalidMembers();
			}
		}
	}

	internal static void ApplyModifiers(string assemblyFileName, AssemblyDefinition assembly, ref int modifiers, ref int members)
	{
		string fileNameWithoutExtension = System.IO.Path.GetFileNameWithoutExtension(assemblyFileName);
		for (int i = 0; i < Active.Count; i++)
		{
			Modifier modifier = Active[i];
			if (modifier.Assembly.Equals(fileNameWithoutExtension, StringComparison.CurrentCultureIgnoreCase))
			{
				ApplySavedModifiersImpl(assembly, modifier, ref modifiers, ref members);
				ApplyModifiersImpl(assembly, modifier, null, ref modifiers, ref members);
			}
		}
	}

	private static void ApplyModifiersImpl(AssemblyDefinition assembly, Modifier modifier, TypeDefinition type, ref int modifiers, ref int members)
	{
		//IL_01b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e2: Expected O, but got Unknown
		try
		{
			bool flag = type != null;
			if (type == null)
			{
				type = GetTypeDefinition(assembly, modifier.Name);
			}
			if (type == null)
			{
				Console.WriteLine(" Couldn't find type for modifier: " + modifier.Name + " [" + modifier.Assembly + "]");
				return;
			}
			modifiers++;
			for (int i = 0; i < modifier.Fields.Count; i++)
			{
				Field field = modifier.Fields[i];
				if ((!flag || field.ShouldSave) && (flag || !field.ShouldSave))
				{
					TypeReference typeReference = GetTypeReference(assembly, field.Type);
					if (typeReference == null)
					{
						Console.WriteLine(" Couldn't find field type for modifier: " + field.Name + " in " + modifier.Name + " [" + modifier.Assembly + "]");
					}
					else if (((IEnumerable<FieldDefinition>)type.Fields).FirstOrDefault((FieldDefinition x) => ((MemberReference)x).Name.Equals(field.Name)) != null || ((IEnumerable<PropertyDefinition>)type.Properties).FirstOrDefault((PropertyDefinition x) => ((MemberReference)x).Name.Equals(field.Name)) != null)
					{
						Console.WriteLine(" Couldn't create field for modifier: " + field.Name + " in " + modifier.Name + " [" + modifier.Assembly + "] as a member with the same name already exists");
					}
					else
					{
						FieldDefinition val = new FieldDefinition(field.Name, (FieldAttributes)(flag ? 6 : 128), assembly.MainModule.ImportReference(typeReference))
						{
							IsStatic = (!flag && field.IsStatic),
							Constant = field.DefaultValue
						};
						type.Fields.Add(val);
						members++;
					}
				}
			}
		}
		catch (Exception ex)
		{
			Console.WriteLine(" Failed applying modifier: " + modifier.Name + " [" + modifier.Assembly + "] (" + ex.Message + ")\n" + ex.StackTrace);
		}
	}

	private static string PascalToCamel(string text)
	{
		if (string.IsNullOrEmpty(text) || char.IsLower(text[0]))
		{
			return text;
		}
		return char.ToLowerInvariant(text[0]) + text.Substring(1);
	}

	private static uint ManifestHash(string str)
	{
		if (!string.IsNullOrEmpty(str))
		{
			return BitConverter.ToUInt32(new MD5CryptoServiceProvider().ComputeHash(Encoding.UTF8.GetBytes(str)), 0);
		}
		return 0u;
	}

	private static void ApplySavedModifiersImpl(AssemblyDefinition assembly, Modifier modifier, ref int modifiers, ref int members)
	{
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ca: Expected O, but got Unknown
		//IL_020c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0213: Expected O, but got Unknown
		//IL_023a: Unknown result type (might be due to invalid IL or missing references)
		//IL_023f: Unknown result type (might be due to invalid IL or missing references)
		if (!modifier.HasSavedFields())
		{
			return;
		}
		try
		{
			TypeDefinition type = GetTypeDefinition(assembly, modifier.Name);
			ModuleDefinition module;
			TypeDefinition storeModifiers;
			TypeDefinition baseDataType;
			TypeDefinition dataType;
			TypeDefinition saveInfoType;
			TypeDefinition loadInfoType;
			FieldDefinition carbonDataField;
			if (type != null)
			{
				module = assembly.MainModule;
				storeModifiers = Patch.common.MainModule.GetType("Carbon.Components", "StoredModifiers");
				baseDataType = ((IEnumerable<TypeDefinition>)storeModifiers.NestedTypes).First((TypeDefinition t) => ((MemberReference)t).Name.Equals("Data", StringComparison.Ordinal));
				dataType = (TypeDefinition)(((object)((IEnumerable<TypeDefinition>)module.Types).FirstOrDefault((TypeDefinition x) => ((MemberReference)x).Name.Equals(((MemberReference)type).Name + DataType, StringComparison.CurrentCulture))) ?? ((object)new TypeDefinition(((TypeReference)type).Namespace, ((MemberReference)type).Name + DataType, (TypeAttributes)2, module.ImportReference((TypeReference)(object)baseDataType))));
				TypeDefinition type2 = assembly.MainModule.GetType("BaseNetworkable");
				saveInfoType = ((IEnumerable<TypeDefinition>)type2.NestedTypes).First((TypeDefinition t) => ((MemberReference)t).Name.Equals("SaveInfo", StringComparison.CurrentCulture));
				loadInfoType = ((IEnumerable<TypeDefinition>)type2.NestedTypes).First((TypeDefinition t) => ((MemberReference)t).Name.Equals("LoadInfo", StringComparison.CurrentCulture));
				if (!module.Types.Contains(dataType))
				{
					module.Types.Add(dataType);
					carbonDataField = new FieldDefinition(PascalToCamel(((MemberReference)type).Name + DataType), (FieldAttributes)128, assembly.MainModule.ImportReference((TypeReference)(object)dataType));
					type.Fields.Add(carbonDataField);
					MethodReference val = module.ImportReference((MethodBase)typeof(ProtoContractAttribute).GetConstructor(Type.EmptyTypes));
					CustomAttribute val2 = new CustomAttribute(module.ImportReference(val));
					val2.Properties.Add(new CustomAttributeNamedArgument("ImplicitFields", new CustomAttributeArgument(module.ImportReference(typeof(ImplicitFields)), (object)(ImplicitFields)1)));
					dataType.CustomAttributes.Add(val2);
					HandleRustSave();
					HandleRustLoad();
					HandleInitializer();
				}
				ApplyModifiersImpl(assembly, modifier, dataType, ref modifiers, ref members);
			}
			void HandleInitializer()
			{
				//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
				//IL_00a7: Expected O, but got Unknown
				//IL_0106: Unknown result type (might be due to invalid IL or missing references)
				//IL_011a: Unknown result type (might be due to invalid IL or missing references)
				//IL_012f: Unknown result type (might be due to invalid IL or missing references)
				//IL_0144: Unknown result type (might be due to invalid IL or missing references)
				//IL_0170: Unknown result type (might be due to invalid IL or missing references)
				//IL_0185: Unknown result type (might be due to invalid IL or missing references)
				//IL_019a: Unknown result type (might be due to invalid IL or missing references)
				//IL_01af: Unknown result type (might be due to invalid IL or missing references)
				//IL_01c3: Unknown result type (might be due to invalid IL or missing references)
				//IL_01d6: Unknown result type (might be due to invalid IL or missing references)
				//IL_0210: Unknown result type (might be due to invalid IL or missing references)
				//IL_0217: Expected O, but got Unknown
				//IL_0229: Unknown result type (might be due to invalid IL or missing references)
				//IL_023c: Unknown result type (might be due to invalid IL or missing references)
				//IL_026e: Unknown result type (might be due to invalid IL or missing references)
				MethodReference val3 = module.ImportReference((MethodBase)typeof(RuntimeTypeModel).GetProperty("Default").GetMethod);
				MethodReference val4 = module.ImportReference((MethodBase)typeof(RuntimeTypeModel).GetProperty("Item").GetMethod);
				MethodReference val5 = module.ImportReference((MethodBase)typeof(MetaType).GetMethod("AddSubType", new Type[2]
				{
					typeof(int),
					typeof(Type)
				}));
				MethodDefinition val6 = new MethodDefinition("Initialize", (MethodAttributes)17, module.TypeSystem.Void);
				val6.IsPublic = false;
				ILProcessor iLProcessor = val6.Body.GetILProcessor();
				MethodReference val7 = module.ImportReference((MethodBase)typeof(Type).GetMethod("GetTypeFromHandle"));
				TypeReference val8 = module.ImportReference((TypeReference)(object)baseDataType);
				TypeReference val9 = module.ImportReference((TypeReference)(object)dataType);
				iLProcessor.Append(iLProcessor.Create(OpCodes.Call, val3));
				iLProcessor.Append(iLProcessor.Create(OpCodes.Ldtoken, val8));
				iLProcessor.Append(iLProcessor.Create(OpCodes.Call, val7));
				iLProcessor.Append(iLProcessor.Create(OpCodes.Callvirt, val4));
				int num = (int)(ManifestHash(((MemberReference)dataType).FullName) & 0xFFFFFFF);
				iLProcessor.Append(iLProcessor.Create(OpCodes.Ldc_I4, num));
				iLProcessor.Append(iLProcessor.Create(OpCodes.Ldtoken, val9));
				iLProcessor.Append(iLProcessor.Create(OpCodes.Call, val7));
				iLProcessor.Append(iLProcessor.Create(OpCodes.Callvirt, val5));
				iLProcessor.Append(iLProcessor.Create(OpCodes.Pop));
				iLProcessor.Append(iLProcessor.Create(OpCodes.Ret));
				dataType.Methods.Add(val6);
				MethodDefinition val10 = new MethodDefinition(".ctor", (MethodAttributes)6278, module.TypeSystem.Void);
				ILProcessor iLProcessor2 = val10.Body.GetILProcessor();
				iLProcessor2.Append(iLProcessor2.Create(OpCodes.Ldarg_0));
				iLProcessor2.Append(iLProcessor2.Create(OpCodes.Call, module.ImportReference((MethodBase)typeof(object).GetConstructor(Type.EmptyTypes))));
				iLProcessor2.Append(iLProcessor2.Create(OpCodes.Ret));
				dataType.Methods.Add(val10);
			}
			void HandleRustLoad()
			{
				//IL_0053: Unknown result type (might be due to invalid IL or missing references)
				//IL_019b: Unknown result type (might be due to invalid IL or missing references)
				//IL_01ba: Unknown result type (might be due to invalid IL or missing references)
				//IL_01d9: Unknown result type (might be due to invalid IL or missing references)
				//IL_01fe: Unknown result type (might be due to invalid IL or missing references)
				//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
				//IL_0102: Expected O, but got Unknown
				//IL_0104: Unknown result type (might be due to invalid IL or missing references)
				//IL_0115: Unknown result type (might be due to invalid IL or missing references)
				//IL_0126: Unknown result type (might be due to invalid IL or missing references)
				//IL_013d: Unknown result type (might be due to invalid IL or missing references)
				//IL_0241: Unknown result type (might be due to invalid IL or missing references)
				//IL_0248: Expected O, but got Unknown
				//IL_026a: Unknown result type (might be due to invalid IL or missing references)
				MethodDefinition val3 = (MethodDefinition)(((object)((IEnumerable<MethodDefinition>)type.Methods).FirstOrDefault((MethodDefinition x) => ((MemberReference)x).Name.Equals("Load", StringComparison.CurrentCulture))) ?? ((object)new MethodDefinition("Load", (MethodAttributes)198, module.TypeSystem.Void)));
				ILProcessor iLProcessor = val3.Body.GetILProcessor();
				TypeDefinition val4 = type.BaseType.Resolve();
				MethodDefinition loadBaseMethod = null;
				while (loadBaseMethod == null)
				{
					loadBaseMethod = ((IEnumerable<MethodDefinition>)val4.Methods).FirstOrDefault((MethodDefinition m) => ((MemberReference)m).Name.Equals("Load", StringComparison.CurrentCulture) && ((MethodReference)m).Parameters.Count == 1);
					if (loadBaseMethod != null || val4.BaseType == null)
					{
						break;
					}
					val4 = val4.BaseType.Resolve();
				}
				if (!type.Methods.Contains(val3))
				{
					((MethodReference)val3).Parameters.Add(new ParameterDefinition("info", (ParameterAttributes)0, (TypeReference)(object)loadInfoType));
					iLProcessor.Append(iLProcessor.Create(OpCodes.Ldarg_0));
					iLProcessor.Append(iLProcessor.Create(OpCodes.Ldarg_1));
					iLProcessor.Append(iLProcessor.Create(OpCodes.Call, (MethodReference)(object)loadBaseMethod));
					iLProcessor.Append(iLProcessor.Create(OpCodes.Ret));
					type.Methods.Add(val3);
				}
				int num = val3.Body.Instructions.IndexOf(((IEnumerable<Instruction>)val3.Body.Instructions).FirstOrDefault(delegate(Instruction x)
				{
					//IL_0001: Unknown result type (might be due to invalid IL or missing references)
					//IL_0006: Unknown result type (might be due to invalid IL or missing references)
					return x.OpCode == OpCodes.Call && x.Operand == loadBaseMethod;
				})) + 1;
				iLProcessor.Body.Instructions.Insert(num, iLProcessor.Create(OpCodes.Ldarg_0));
				iLProcessor.Body.Instructions.Insert(num + 1, iLProcessor.Create(OpCodes.Ldarg_0));
				iLProcessor.Body.Instructions.Insert(num + 2, iLProcessor.Create(OpCodes.Ldflda, (FieldReference)(object)carbonDataField));
				iLProcessor.Body.Instructions.Insert(num + 3, iLProcessor.Create(OpCodes.Ldarg_1));
				GenericInstanceMethod val5 = new GenericInstanceMethod((MethodReference)(object)((MethodReference)((IEnumerable<MethodDefinition>)storeModifiers.Methods).FirstOrDefault((MethodDefinition m) => ((MemberReference)m).Name.Equals("TryGetData", StringComparison.CurrentCulture))).Resolve());
				val5.GenericArguments.Add((TypeReference)(object)dataType);
				iLProcessor.Body.Instructions.Insert(num + 4, iLProcessor.Create(OpCodes.Call, module.ImportReference((MethodReference)(object)val5)));
			}
			void HandleRustSave()
			{
				//IL_0053: Unknown result type (might be due to invalid IL or missing references)
				//IL_019e: Unknown result type (might be due to invalid IL or missing references)
				//IL_01bd: Unknown result type (might be due to invalid IL or missing references)
				//IL_01dc: Unknown result type (might be due to invalid IL or missing references)
				//IL_020c: Unknown result type (might be due to invalid IL or missing references)
				//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
				//IL_0105: Expected O, but got Unknown
				//IL_0107: Unknown result type (might be due to invalid IL or missing references)
				//IL_0118: Unknown result type (might be due to invalid IL or missing references)
				//IL_0129: Unknown result type (might be due to invalid IL or missing references)
				//IL_0140: Unknown result type (might be due to invalid IL or missing references)
				//IL_024f: Unknown result type (might be due to invalid IL or missing references)
				//IL_0256: Expected O, but got Unknown
				//IL_0278: Unknown result type (might be due to invalid IL or missing references)
				MethodDefinition val3 = (MethodDefinition)(((object)((IEnumerable<MethodDefinition>)type.Methods).FirstOrDefault((MethodDefinition x) => ((MemberReference)x).Name.Equals("Save", StringComparison.CurrentCulture))) ?? ((object)new MethodDefinition("Save", (MethodAttributes)198, module.TypeSystem.Void)));
				ILProcessor iLProcessor = val3.Body.GetILProcessor();
				TypeDefinition val4 = type.BaseType.Resolve();
				MethodDefinition val5 = null;
				while (val5 == null)
				{
					val5 = ((IEnumerable<MethodDefinition>)val4.Methods).FirstOrDefault((MethodDefinition m) => ((MemberReference)m).Name.Equals("Save", StringComparison.CurrentCulture) && ((MethodReference)m).Parameters.Count == 1);
					if (val5 != null || val4.BaseType == null)
					{
						break;
					}
					val4 = val4.BaseType.Resolve();
				}
				MethodReference save = module.ImportReference((MethodReference)(object)val5);
				if (!type.Methods.Contains(val3))
				{
					((MethodReference)val3).Parameters.Add(new ParameterDefinition("info", (ParameterAttributes)0, (TypeReference)(object)saveInfoType));
					iLProcessor.Append(iLProcessor.Create(OpCodes.Ldarg_0));
					iLProcessor.Append(iLProcessor.Create(OpCodes.Ldarg_1));
					iLProcessor.Append(iLProcessor.Create(OpCodes.Call, save));
					iLProcessor.Append(iLProcessor.Create(OpCodes.Ret));
					type.Methods.Add(val3);
				}
				int num = val3.Body.Instructions.IndexOf(((IEnumerable<Instruction>)val3.Body.Instructions).FirstOrDefault(delegate(Instruction x)
				{
					//IL_0001: Unknown result type (might be due to invalid IL or missing references)
					//IL_0006: Unknown result type (might be due to invalid IL or missing references)
					return x.OpCode == OpCodes.Call && x.Operand == save;
				})) + 1;
				iLProcessor.Body.Instructions.Insert(num, iLProcessor.Create(OpCodes.Ldarg_0));
				iLProcessor.Body.Instructions.Insert(num + 1, iLProcessor.Create(OpCodes.Ldarg_0));
				iLProcessor.Body.Instructions.Insert(num + 2, iLProcessor.Create(OpCodes.Ldfld, module.ImportReference((FieldReference)(object)carbonDataField)));
				iLProcessor.Body.Instructions.Insert(num + 3, iLProcessor.Create(OpCodes.Ldarg_1));
				GenericInstanceMethod val6 = new GenericInstanceMethod((MethodReference)(object)((MethodReference)((IEnumerable<MethodDefinition>)storeModifiers.Methods).FirstOrDefault((MethodDefinition m) => ((MemberReference)m).Name.Equals("TryUpdateData", StringComparison.CurrentCulture))).Resolve());
				val6.GenericArguments.Add((TypeReference)(object)dataType);
				iLProcessor.Body.Instructions.Insert(num + 4, iLProcessor.Create(OpCodes.Call, module.ImportReference((MethodReference)(object)val6)));
			}
		}
		catch (Exception ex)
		{
			Console.WriteLine(" Failed applying saved modifier: " + modifier.Name + " [" + modifier.Assembly + "] (" + ex.Message + ")\n" + ex.StackTrace);
		}
	}

	private static TypeDefinition GetTypeDefinition(AssemblyDefinition assembly, string name)
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		TypeDefinition type = assembly.MainModule.GetType(name);
		if (type != null)
		{
			return type;
		}
		Enumerator<AssemblyNameReference> enumerator = assembly.MainModule.AssemblyReferences.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				AssemblyNameReference current = enumerator.Current;
				try
				{
					AssemblyDefinition val = assembly.MainModule.AssemblyResolver.Resolve(current);
					TypeDefinition type2 = val.MainModule.GetType(name);
					if (type2 != null)
					{
						return type2;
					}
				}
				catch
				{
				}
			}
		}
		finally
		{
			((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
		}
		return null;
	}

	private static TypeReference GetTypeReference(AssemblyDefinition assembly, string fullName)
	{
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Expected O, but got Unknown
		if (!fullName.Contains('`') || !fullName.Contains('['))
		{
			return TryResolveSimple(assembly, fullName);
		}
		int num = fullName.IndexOf('[');
		string name = fullName.Substring(0, num);
		string input = fullName.Substring(num + 1, fullName.Length - num - 2);
		List<string> list = SplitGenericArgs(input);
		TypeDefinition typeDefinition = GetTypeDefinition(assembly, name);
		GenericInstanceType val = new GenericInstanceType((TypeReference)(object)typeDefinition);
		foreach (string item in list)
		{
			val.GenericArguments.Add(GetTypeReference(assembly, item));
		}
		return (TypeReference)(object)val;
	}

	private static List<string> SplitGenericArgs(string input)
	{
		List<string> list = new List<string>();
		int num = 0;
		int num2 = 0;
		for (int i = 0; i < input.Length; i++)
		{
			switch (input[i])
			{
			case '[':
				num++;
				break;
			case ']':
				num--;
				break;
			case ',':
				if (num == 0)
				{
					list.Add(input.Substring(num2, i - num2).Trim());
					num2 = i + 1;
				}
				break;
			}
		}
		list.Add(input.Substring(num2).Trim());
		return list;
	}

	private static TypeReference TryResolveSimple(AssemblyDefinition assembly, string name)
	{
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		TypeReference type = assembly.MainModule.GetType(name, true);
		if (type != null)
		{
			return type;
		}
		Enumerator<AssemblyNameReference> enumerator = assembly.MainModule.AssemblyReferences.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				AssemblyNameReference current = enumerator.Current;
				try
				{
					AssemblyDefinition val = assembly.MainModule.AssemblyResolver.Resolve(current);
					TypeDefinition val2 = ((IEnumerable<TypeDefinition>)val.MainModule.Types).FirstOrDefault((TypeDefinition t) => ((MemberReference)t).FullName.Equals(name));
					if (val2 != null)
					{
						return assembly.MainModule.ImportReference((TypeReference)(object)val2);
					}
				}
				catch
				{
				}
			}
		}
		finally
		{
			((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
		}
		return null;
	}
}
