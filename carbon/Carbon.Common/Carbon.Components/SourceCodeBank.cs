using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using ICSharpCode.Decompiler;
using ICSharpCode.Decompiler.CSharp;
using ICSharpCode.Decompiler.Metadata;
using ICSharpCode.Decompiler.TypeSystem;
using Unity.Collections.LowLevel.Unsafe;

namespace Carbon.Components;

public class SourceCodeBank
{
	public class CachedAssemblyBank : Dictionary<string, SourceCode>
	{
	}

	public struct SourceCode
	{
		public Dictionary<string, string> Types;

		public Dictionary<string, string> Methods;

		public CSharpDecompiler Decompiler;

		public DecompilerSettings Settings;

		public static SourceCode Get(string assemblyPath)
		{
			//IL_0022: Unknown result type (might be due to invalid IL or missing references)
			//IL_0027: Unknown result type (might be due to invalid IL or missing references)
			//IL_0033: Expected O, but got Unknown
			//IL_003c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0046: Expected O, but got Unknown
			SourceCode result = default(SourceCode);
			result.Types = new Dictionary<string, string>();
			result.Methods = new Dictionary<string, string>();
			result.Settings = new DecompilerSettings
			{
				ThrowOnAssemblyResolveErrors = false
			};
			result.Decompiler = new CSharpDecompiler(assemblyPath, result.Settings);
			return result;
		}

		public static SourceCode Get(PEFile file)
		{
			//IL_0022: Unknown result type (might be due to invalid IL or missing references)
			//IL_0027: Unknown result type (might be due to invalid IL or missing references)
			//IL_0033: Expected O, but got Unknown
			//IL_0066: Unknown result type (might be due to invalid IL or missing references)
			//IL_0076: Expected O, but got Unknown
			//IL_0071: Unknown result type (might be due to invalid IL or missing references)
			//IL_007b: Expected O, but got Unknown
			SourceCode result = default(SourceCode);
			result.Types = new Dictionary<string, string>();
			result.Methods = new Dictionary<string, string>();
			result.Settings = new DecompilerSettings
			{
				ThrowOnAssemblyResolveErrors = false
			};
			result.Decompiler = new CSharpDecompiler((MetadataFile)(object)file, (IAssemblyResolver)new UniversalAssemblyResolver((string)null, false, DotNetCorePathFinderExtensions.DetectTargetFrameworkId((MetadataFile)(object)file), DotNetCorePathFinderExtensions.DetectRuntimePack((MetadataFile)(object)file), (PEStreamOptions)(result.Settings.LoadInMemory ? 2 : 0), (MetadataReaderOptions)(result.Settings.ApplyWindowsRuntimeProjections ? 1 : 0)), result.Settings);
			return result;
		}

		public string ParseType(string type)
		{
			//IL_0073: Unknown result type (might be due to invalid IL or missing references)
			if (Types.TryGetValue(type, out var value))
			{
				return value;
			}
			ITypeDefinition val = Decompiler.TypeSystem.MainModule.TypeDefinitions.FirstOrDefault((ITypeDefinition x) => ((INamedElement)x).FullName.Equals(type, StringComparison.OrdinalIgnoreCase));
			if (val == null)
			{
				return string.Empty;
			}
			Settings.UsingDeclarations = true;
			return Types[type] = Decompiler.DecompileTypeAsString(((ITypeDefinitionOrUnknown)val).FullTypeName);
		}

		public string ParseMethod(string type, string method)
		{
			//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
			string key = type + ":" + method;
			if (Methods.TryGetValue(key, out var value))
			{
				return value;
			}
			ITypeDefinition val = Decompiler.TypeSystem.MainModule.TypeDefinitions.FirstOrDefault((ITypeDefinition x) => ((INamedElement)x).FullName.Equals(type, StringComparison.OrdinalIgnoreCase));
			if (val == null)
			{
				return string.Empty;
			}
			IMethod val2 = val.Methods.FirstOrDefault((IMethod x) => ((IEntity)x).Name.Equals(method, StringComparison.OrdinalIgnoreCase));
			if (val2 == null)
			{
				return string.Empty;
			}
			Settings.UsingDeclarations = false;
			return Methods[key] = Decompiler.DecompileAsString((EntityHandle[])(object)new EntityHandle[1] { ((IEntity)val2).MetadataToken });
		}

		public string ParseMethod(uint token)
		{
			//IL_0052: Unknown result type (might be due to invalid IL or missing references)
			//IL_005c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0061: Unknown result type (might be due to invalid IL or missing references)
			string key = token.ToString();
			if (Methods.TryGetValue(key, out var value))
			{
				return value;
			}
			Settings.UsingDeclarations = false;
			return Methods[key] = Decompiler.DecompileAsString((EntityHandle[])(object)new EntityHandle[1] { ((IEntity)Decompiler.TypeSystem.MainModule.GetDefinition(UnsafeUtility.As<uint, MethodDefinitionHandle>(ref token))).MetadataToken });
		}

		public unsafe string ParseMethod(MonoProfiler.MonoMethod* methodInfo, out string type, out string method)
		{
			//IL_004a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0089: Unknown result type (might be due to invalid IL or missing references)
			//IL_008e: Unknown result type (might be due to invalid IL or missing references)
			uint token = methodInfo->token;
			string key = token.ToString();
			type = null;
			method = null;
			if (Methods.TryGetValue(key, out var value))
			{
				return value;
			}
			Settings.UsingDeclarations = false;
			IMethod definition = Decompiler.TypeSystem.MainModule.GetDefinition(UnsafeUtility.As<uint, MethodDefinitionHandle>(ref token));
			if (definition == null)
			{
				return "No method body available";
			}
			type = ((INamedElement)((IMember)definition).DeclaringType).FullName;
			method = ((IEntity)definition).Name;
			return Methods[key] = Decompiler.DecompileAsString((EntityHandle[])(object)new EntityHandle[1] { ((IEntity)definition).MetadataToken });
		}
	}

	public static CachedAssemblyBank AssemblyBank { get; } = new CachedAssemblyBank();

	public static SourceCode Parse(string assemblyPath)
	{
		if (!AssemblyBank.TryGetValue(assemblyPath, out var value))
		{
			value = (AssemblyBank[assemblyPath] = SourceCode.Get(assemblyPath));
		}
		return value;
	}

	public static SourceCode Parse(string name, PEFile file)
	{
		if (!AssemblyBank.TryGetValue(name, out var value))
		{
			value = (AssemblyBank[name] = SourceCode.Get(file));
		}
		return value;
	}

	public unsafe static SourceCode Parse(string name, ModuleHandle handle)
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Expected O, but got Unknown
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Expected O, but got Unknown
		if (!AssemblyBank.TryGetValue(name, out var value))
		{
			MonoProfiler.MonoImage* ptr = MonoProfiler.MonoImage.handle_to_image(handle);
			PEReader val = new PEReader(ptr->raw_data, (int)ptr->raw_data_len);
			PEFile file = new PEFile(name, val, (MetadataReaderOptions)1);
			value = (AssemblyBank[name] = SourceCode.Get(file));
		}
		return value;
	}
}
