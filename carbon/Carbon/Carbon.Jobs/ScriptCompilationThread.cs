using System;
using System.CodeDom.Compiler;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using API.Assembly;
using Carbon.Base;
using Carbon.Components;
using Carbon.Contracts;
using Carbon.Core;
using Carbon.Extensions;
using Carbon.Generator;
using Carbon.Pooling;
using Carbon.Profiler;
using Facepunch;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.Text;
using Mono.Cecil;

namespace Carbon.Jobs;

public class ScriptCompilationThread : BaseThreadedJob
{
	public class CarbonAssemblyResolver : BaseAssemblyResolver
	{
		private readonly IDictionary<string, AssemblyDefinition> cache = new Dictionary<string, AssemblyDefinition>(StringComparer.Ordinal);

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
			if (cache.TryGetValue(name.FullName, out var value))
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
						value = AssemblyDefinition.ReadAssembly(text, parameters);
						break;
					}
				}
				if (value != null)
				{
					break;
				}
			}
			cache[name.FullName] = value;
			return value;
		}

		public void RegisterAssembly(AssemblyDefinition assembly)
		{
			if (assembly == null)
			{
				throw new ArgumentNullException("assembly");
			}
			string fullName = ((AssemblyNameReference)assembly.Name).FullName;
			if (!cache.ContainsKey(fullName))
			{
				cache[fullName] = assembly;
			}
		}

		protected override void Dispose(bool disposing)
		{
			foreach (AssemblyDefinition value in cache.Values)
			{
				value.Dispose();
			}
			cache.Clear();
			((BaseAssemblyResolver)this).Dispose(disposing);
		}
	}

	public class CompilerException : Exception
	{
		public string FilePath;

		public CompilerError Error;

		public CompilerException(string filePath, CompilerError error)
		{
			FilePath = filePath;
			Error = error;
		}

		public override string ToString()
		{
			return string.Format("{0}\n ({1} {2} line {3})", new object[4] { Error.ErrorText, FilePath, Error.Column, Error.Line });
		}
	}

	public List<ISource> Sources;

	public string[] References;

	public string[] Requires;

	public string InternalCallHookSource;

	public bool IsExtension;

	public bool IsCompileTestMode;

	public bool IsCompileSuccess;

	public List<string> Usings = new List<string>();

	public Dictionary<Type, List<uint>> Hooks = new Dictionary<Type, List<uint>>();

	public Dictionary<Type, List<HookMethodAttribute>> HookMethods = new Dictionary<Type, List<HookMethodAttribute>>();

	public Dictionary<Type, List<PluginReferenceAttribute>> PluginReferences = new Dictionary<Type, List<PluginReferenceAttribute>>();

	public TimeSpan CompileTime;

	public TimeSpan InternalCallHookGenTime;

	public Assembly Assembly;

	public List<CompilerException> Exceptions = new List<CompilerException>();

	public List<CompilerException> Warnings = new List<CompilerException>();

	private const string _internalCallHookPattern = "override object InternalCallHook";

	private const string _partialPattern = " partial ";

	private Stopwatch _stopwatch;

	private List<ClassDeclarationSyntax> ClassList = new List<ClassDeclarationSyntax>();

	private static EmitOptions _emitOptions = new EmitOptions(false, (DebugInformationFormat)3, (string)null, (string)null, 0, 0uL, false, default(SubsystemVersion), (string)null, false, true, default(ImmutableArray<InstrumentationKind>), (HashAlgorithmName?)null, (Encoding)null, (Encoding)null);

	private static ConcurrentDictionary<string, byte[]> _compilationCache = new ConcurrentDictionary<string, byte[]>();

	private static ConcurrentDictionary<string, byte[]> _extensionCompilationCache = new ConcurrentDictionary<string, byte[]>();

	private static Dictionary<string, PortableExecutableReference> _referenceCache = new Dictionary<string, PortableExecutableReference>();

	private static Dictionary<string, PortableExecutableReference> _extensionReferenceCache = new Dictionary<string, PortableExecutableReference>();

	private static readonly string[] _libraryDirectories = new string[5]
	{
		Defines.GetLibFolder(),
		Defines.GetManagedFolder(),
		Defines.GetRustManagedFolder(),
		Defines.GetManagedModulesFolder(),
		Defines.GetExtensionsFolder()
	};

	private static bool hasLoaded;

	private List<MetadataReference> references;

	public ISource InitialSource
	{
		get
		{
			if (Sources == null || Sources.Count <= 0)
			{
				return null;
			}
			return Sources[0];
		}
	}

	private static byte[] _getPlugin(string name)
	{
		name = name.Replace(" ", string.Empty);
		foreach (KeyValuePair<string, byte[]> item in _compilationCache)
		{
			if (item.Key == name)
			{
				return item.Value;
			}
		}
		return null;
	}

	private static byte[] _getExtensionPlugin(string name)
	{
		foreach (KeyValuePair<string, byte[]> item in _extensionCompilationCache)
		{
			if (item.Key == name)
			{
				return item.Value;
			}
		}
		return null;
	}

	private static void _overridePlugin(string name, byte[] pluginAssembly)
	{
		name = name.Replace(" ", "");
		if (pluginAssembly == null)
		{
			return;
		}
		byte[] array = _getPlugin(name);
		if (array == null)
		{
			try
			{
				_compilationCache.AddOrUpdate(name, pluginAssembly, (string a, byte[] v) => pluginAssembly);
				return;
			}
			catch
			{
				return;
			}
		}
		Array.Clear(array, 0, array.Length);
		try
		{
			_compilationCache[name] = pluginAssembly;
		}
		catch
		{
		}
	}

	private static void _overrideExtensionPlugin(string name, byte[] pluginAssembly)
	{
		if (pluginAssembly == null)
		{
			return;
		}
		byte[] array = _getExtensionPlugin(name);
		if (array == null)
		{
			try
			{
				_extensionCompilationCache.AddOrUpdate(name, pluginAssembly, (string a, byte[] v) => pluginAssembly);
				return;
			}
			catch
			{
				return;
			}
		}
		Array.Clear(array, 0, array.Length);
		try
		{
			_extensionCompilationCache[name] = pluginAssembly;
		}
		catch
		{
		}
	}

	internal static void _clearExtensionPlugin(string name)
	{
		if (_extensionCompilationCache.ContainsKey(name))
		{
			_extensionCompilationCache.TryRemove(name, out var _);
		}
		if (_extensionReferenceCache.ContainsKey(name))
		{
			_extensionReferenceCache.Remove(name);
		}
	}

	internal static void _injectPatchedReferences()
	{
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		foreach (string publicizedAssembly in Community.Runtime.Config.Publicizer.PublicizedAssemblies)
		{
			string key = publicizedAssembly.Replace(".dll", string.Empty);
			using MemoryStream memoryStream = new MemoryStream(PatchedAssemblies.AssemblyCache[key]);
			_referenceCache[key] = MetadataReference.CreateFromStream((Stream)memoryStream, default(MetadataReferenceProperties), (DocumentationProvider)null, (string)null);
		}
	}

	private void _injectReference(string id, string name, List<MetadataReference> references, string[] directories, bool direct = false, bool allowCache = true)
	{
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		if (allowCache && _referenceCache.TryGetValue(name, out var value))
		{
			references.Add((MetadataReference)(object)value);
			return;
		}
		byte[] array = null;
		if (direct)
		{
			bool flag = false;
			foreach (string folder in directories)
			{
				string[] filesWithExtension = OsEx.Folder.GetFilesWithExtension(folder, "dll");
				foreach (string text in filesWithExtension)
				{
					if (text.Contains(name))
					{
						array = OsEx.File.ReadBytes(text);
						flag = true;
						break;
					}
				}
				if (flag)
				{
					break;
				}
			}
		}
		else
		{
			array = Community.Runtime.AssemblyEx.Read(name, directories);
		}
		if (array == null)
		{
			return;
		}
		using MemoryStream memoryStream = new MemoryStream(array);
		PortableExecutableReference val = MetadataReference.CreateFromStream((Stream)memoryStream, default(MetadataReferenceProperties), (DocumentationProvider)null, (string)null);
		references.Add((MetadataReference)(object)val);
		_referenceCache[name] = val;
	}

	private void _injectExtensionReference(string name, List<MetadataReference> references)
	{
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		if (_extensionReferenceCache.TryGetValue(name, out var value))
		{
			references.Add((MetadataReference)(object)value);
			return;
		}
		byte[] array = Community.Runtime.AssemblyEx.Read(name, _libraryDirectories);
		if (array == null)
		{
			return;
		}
		using MemoryStream memoryStream = new MemoryStream(array);
		PortableExecutableReference val = MetadataReference.CreateFromStream((Stream)memoryStream, default(MetadataReferenceProperties), (DocumentationProvider)null, (string)null);
		references.Add((MetadataReference)(object)val);
		_extensionReferenceCache.Add(name, val);
	}

	private List<MetadataReference> _addReferences()
	{
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		List<MetadataReference> list = new List<MetadataReference>();
		string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(InitialSource.FilePath);
		_injectReference(fileNameWithoutExtension, "0Harmony", list, _libraryDirectories, direct: true);
		foreach (string item in Community.Runtime.AssemblyEx.RefWhitelist)
		{
			try
			{
				_injectReference(fileNameWithoutExtension, item, list, _libraryDirectories);
			}
			catch (Exception arg)
			{
				Logger.Debug(fileNameWithoutExtension, $"Error loading common reference '{item}': {arg}", 4);
			}
		}
		foreach (KeyValuePair<Type, KeyValuePair<string, byte[]>> item2 in Community.Runtime.AssemblyEx.Modules.Loaded)
		{
			try
			{
				string fileName = Path.GetFileName(item2.Value.Key);
				using MemoryStream memoryStream = new MemoryStream(item2.Value.Value);
				PortableExecutableReference val = MetadataReference.CreateFromStream((Stream)memoryStream, default(MetadataReferenceProperties), (DocumentationProvider)null, (string)null);
				list.Add((MetadataReference)(object)val);
				_referenceCache[fileName] = val;
			}
			catch (Exception arg2)
			{
				Logger.Debug(fileNameWithoutExtension, $"Error loading module reference '{item2}': {arg2}", 4);
			}
		}
		foreach (KeyValuePair<Type, KeyValuePair<string, byte[]>> item3 in Community.Runtime.AssemblyEx.Extensions.Loaded)
		{
			try
			{
				_injectExtensionReference(Path.GetFileName(item3.Value.Key), list);
			}
			catch (Exception arg3)
			{
				Logger.Debug(fileNameWithoutExtension, $"Error loading extension reference '{item3}': {arg3}", 4);
			}
		}
		return list;
	}

	public static void PrewarmInternalHookGenerator()
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Expected O, but got Unknown
		if (hasLoaded)
		{
			return;
		}
		hasLoaded = true;
		CarbonAssemblyResolver carbonAssemblyResolver = new CarbonAssemblyResolver();
		ReaderParameters val = new ReaderParameters
		{
			AssemblyResolver = (IAssemblyResolver)(object)carbonAssemblyResolver,
			InMemory = true
		};
		((BaseAssemblyResolver)carbonAssemblyResolver).AddSearchDirectory(Defines.GetRustManagedFolder());
		string[] files = Directory.GetFiles(Defines.GetRustManagedFolder(), "*.dll");
		foreach (string text in files)
		{
			try
			{
				AssemblyDefinition val2 = AssemblyDefinition.ReadAssembly(text, val);
				InternalCallHook.Assemblies.Add(val2);
				carbonAssemblyResolver.RegisterAssembly(val2);
			}
			catch (Exception ex)
			{
				Console.WriteLine("Failed " + text + " (" + ex.Message + ")\n" + ex.StackTrace);
			}
		}
	}

	public override void Start()
	{
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		IsCompileTestMode = Community.Runtime?.Config?.Compiler?.CompileTestMode == true;
		PrewarmInternalHookGenerator();
		references = _addReferences();
		string[] requires = Requires;
		foreach (string text in requires)
		{
			try
			{
				byte[] buffer = _getPlugin(text);
				using MemoryStream memoryStream = new MemoryStream(buffer);
				references.Add((MetadataReference)(object)MetadataReference.CreateFromStream((Stream)memoryStream, default(MetadataReferenceProperties), (DocumentationProvider)null, (string)null));
			}
			catch (Exception ex)
			{
				Logger.Error("Failed loading required plugin for '" + InitialSource.ContextFileName + "': " + text, ex);
			}
		}
		List<string> list = Pool.Get<List<string>>();
		string[] array = References;
		foreach (string text2 in array)
		{
			try
			{
				if (_referenceCache.ContainsKey(text2))
				{
					continue;
				}
				string text3 = Path.Combine(Defines.GetExtensionsFolder(), text2 + ".dll");
				if (OsEx.File.Exists(text3))
				{
					_injectExtensionReference(text3, references);
					continue;
				}
				string text4 = Path.Combine(Defines.GetLibFolder(), text2 + ".dll");
				if (OsEx.File.Exists(text4))
				{
					_injectReference(text2, text4, references, _libraryDirectories);
					continue;
				}
				string text5 = Path.Combine(Defines.GetRustManagedFolder(), text2 + ".dll");
				if (OsEx.File.Exists(text5))
				{
					_injectReference(text2, text5, references, _libraryDirectories);
				}
				else
				{
					list.Add(text2);
				}
			}
			catch (Exception ex2)
			{
				Logger.Error("Failed loading reference for '" + InitialSource.ContextFileName + "': " + text2, ex2);
			}
		}
		if (list.Count > 0)
		{
			foreach (string item in list)
			{
				Logger.Warn(" Couldn't find reference '" + item + "' for '" + ((!string.IsNullOrEmpty(InitialSource.ContextFilePath)) ? Path.GetFileNameWithoutExtension(InitialSource.ContextFilePath) : "<unknown>") + "'");
			}
		}
		Pool.FreeUnmanaged<string>(ref list);
		base.Start();
	}

	public override void ThreadFunction()
	{
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_027e: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_03bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_03dd: Expected O, but got Unknown
		//IL_01fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0205: Unknown result type (might be due to invalid IL or missing references)
		//IL_020a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0214: Unknown result type (might be due to invalid IL or missing references)
		//IL_0219: Unknown result type (might be due to invalid IL or missing references)
		//IL_021f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0224: Unknown result type (might be due to invalid IL or missing references)
		//IL_022c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0236: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01db: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_0463: Unknown result type (might be due to invalid IL or missing references)
		//IL_0468: Unknown result type (might be due to invalid IL or missing references)
		//IL_046c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0471: Unknown result type (might be due to invalid IL or missing references)
		//IL_04ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_04b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_04b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_04bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_04f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_04f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_04f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_04fb: Invalid comparison between Unknown and I4
		//IL_04fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0500: Invalid comparison between Unknown and I4
		//IL_0597: Unknown result type (might be due to invalid IL or missing references)
		//IL_059c: Unknown result type (might be due to invalid IL or missing references)
		//IL_05a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_05ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_05cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_05d6: Expected O, but got Unknown
		//IL_051f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0524: Unknown result type (might be due to invalid IL or missing references)
		//IL_0531: Unknown result type (might be due to invalid IL or missing references)
		//IL_0536: Unknown result type (might be due to invalid IL or missing references)
		//IL_0554: Unknown result type (might be due to invalid IL or missing references)
		//IL_055e: Expected O, but got Unknown
		if (Sources.TrueForAll((ISource x) => string.IsNullOrEmpty(x.Content)))
		{
			Dispose();
			return;
		}
		try
		{
			Exceptions.Clear();
			Warnings.Clear();
			List<SyntaxTree> list = Pool.Get<List<SyntaxTree>>();
			List<string> list2 = Pool.Get<List<string>>();
			_stopwatch = Pool.Get<Stopwatch>();
			try
			{
				list2.AddRange(Community.Runtime.Config.Compiler.ConditionalCompilationSymbols);
			}
			catch (Exception ex)
			{
				Logger.Error("Failed referencing conditional compilation symbols", ex);
			}
			list2.Add("WIN");
			if (Modifier.Active.HasPlugin(Path.GetFileNameWithoutExtension(InitialSource.ContextFilePath)))
			{
				list2.Add("MODIFIERS_PATCHED");
			}
			string contextFileName = InitialSource.ContextFileName;
			CSharpParseOptions val = new CSharpParseOptions((LanguageVersion)2147483646, (DocumentationMode)1, (SourceCodeKind)0, (IEnumerable<string>)null).WithPreprocessorSymbols((IEnumerable<string>)list2);
			bool flag = Sources.Any((ISource x) => !string.IsNullOrEmpty(x.Content) && x.Content.Contains("override object InternalCallHook"));
			foreach (ISource source in Sources)
			{
				SyntaxTree val2 = CSharpSyntaxTree.ParseText(source.Content, val, source.FilePath, Encoding.UTF8, default(CancellationToken));
				CompilationUnitSyntax val3 = CSharpExtensions.GetCompilationUnitRoot(val2, default(CancellationToken));
				HookCaller.HandleVersionConditionals(val3, list2);
				val = val.WithPreprocessorSymbols((IEnumerable<string>)list2);
				val2 = val2.WithRootAndOptions((SyntaxNode)(object)val3, (ParseOptions)(object)val);
				if (InternalCallHook.FindPluginInfo(val3, out var @namespace, out var namespaceIndex, out var classIndex, ClassList))
				{
					ClassDeclarationSyntax val4 = ClassList[0];
					if (!((IEnumerable<SyntaxToken>)(object)((MemberDeclarationSyntax)val4).Modifiers).Any((SyntaxToken x) => CSharpExtensions.IsKind(x, (SyntaxKind)8406)))
					{
						ClassDeclarationSyntax obj = val4;
						SyntaxTokenList modifiers = ((MemberDeclarationSyntax)val4).Modifiers;
						val4 = obj.WithModifiers(((SyntaxTokenList)(ref modifiers)).Add(SyntaxFactory.ParseToken(" partial ", 0)));
					}
					val3 = val3.WithMembers(val3.Members.RemoveAt(namespaceIndex).Insert(namespaceIndex, (MemberDeclarationSyntax)(object)@namespace.WithMembers(@namespace.Members.RemoveAt(classIndex).Insert(classIndex, (MemberDeclarationSyntax)(object)val4))));
					list.Insert(0, CSharpSyntaxTree.ParseText(((SyntaxNode)val3).ToFullString(), val, source.FilePath, Encoding.UTF8, default(CancellationToken)));
				}
				else
				{
					list.Add(val2);
				}
				Usings.AddRange(((IEnumerable<UsingDirectiveSyntax>)(object)val3.Usings).Select((UsingDirectiveSyntax x) => ((object)x).ToString()));
			}
			if (!flag)
			{
				_stopwatch.Start();
				SyntaxTree val5 = CSharpSyntaxTree.ParseText(Sources.Select((ISource x) => x.Content).ToString("\n"), val, contextFileName, Encoding.UTF8, default(CancellationToken));
				InternalCallHook.GeneratePartial(CSharpExtensions.GetCompilationUnitRoot(val5, default(CancellationToken)), out var output, val, contextFileName, ClassList, Defines.GetScriptDebugFolder(), Usings, references);
				InternalCallHookGenTime = _stopwatch.Elapsed;
				if (output != null)
				{
					InternalCallHookSource = ((SyntaxNode)SyntaxNodeExtensions.NormalizeWhitespace<CompilationUnitSyntax>(output, "    ", "\r\n", false)).ToFullString();
					list.Add(((SyntaxNode)output).SyntaxTree);
				}
			}
			ScriptCompilerPolyfills.InjectMissingPolyfills(list, references, val);
			CSharpCompilationOptions val6 = new CSharpCompilationOptions((OutputKind)2, false, (string)null, (string)null, (string)null, (IEnumerable<string>)null, (OptimizationLevel)1, false, true, (string)null, (string)null, default(ImmutableArray<byte>), (bool?)null, (Platform)0, (ReportDiagnostic)0, 4, (IEnumerable<KeyValuePair<string, ReportDiagnostic>>)null, true, true, (XmlReferenceResolver)null, (SourceReferenceResolver)null, (MetadataReferenceResolver)null, (AssemblyIdentityComparer)null, (StrongNameProvider)null, false, (MetadataImportOptions)0, (NullableContextOptions)0);
			_stopwatch.Restart();
			if (InitialSource == null)
			{
				Dispose();
				return;
			}
			CSharpCompilation val7 = CSharpCompilation.Create($"Script.{InitialSource.FileName}.{Guid.NewGuid():N}", (IEnumerable<SyntaxTree>)list, (IEnumerable<MetadataReference>)references, val6);
			using (MemoryStream memoryStream = new MemoryStream())
			{
				EmitResult val8 = ((Compilation)val7).Emit((Stream)memoryStream, (Stream)null, (Stream)null, (Stream)null, (IEnumerable<ResourceDescription>)null, _emitOptions, (IMethodSymbol)null, (Stream)null, (IEnumerable<EmbeddedText>)null, (Stream)null, default(CancellationToken));
				List<string> list3 = Pool.Get<List<string>>();
				List<string> list4 = Pool.Get<List<string>>();
				Enumerator<Diagnostic> enumerator2 = val8.Diagnostics.GetEnumerator();
				while (enumerator2.MoveNext())
				{
					Diagnostic current2 = enumerator2.Current;
					if (list3.Contains(current2.Id) || list4.Contains(current2.Id))
					{
						continue;
					}
					FileLinePositionSpan mappedLineSpan = current2.Location.GetMappedLineSpan();
					LinePositionSpan span = ((FileLinePositionSpan)(ref mappedLineSpan)).Span;
					object obj2;
					if (current2 == null)
					{
						obj2 = null;
					}
					else
					{
						Location location = current2.Location;
						if (location == null)
						{
							obj2 = null;
						}
						else
						{
							SyntaxTree sourceTree = location.SourceTree;
							obj2 = ((sourceTree != null) ? sourceTree.FilePath : null);
						}
					}
					string text = (string)obj2;
					string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(text);
					DiagnosticSeverity severity = current2.Severity;
					LinePosition start;
					if ((int)severity != 2)
					{
						if ((int)severity == 3)
						{
							list3.Add(current2.Id);
							List<CompilerException> exceptions = Exceptions;
							start = ((LinePositionSpan)(ref span)).Start;
							int num = ((LinePosition)(ref start)).Line + 1;
							start = ((LinePositionSpan)(ref span)).Start;
							exceptions.Add(new CompilerException(text, new CompilerError(fileNameWithoutExtension, num, ((LinePosition)(ref start)).Character + 1, current2.Id, current2.GetMessage((IFormatProvider)CultureInfo.InvariantCulture))));
						}
					}
					else if (!current2.GetMessage((IFormatProvider)CultureInfo.InvariantCulture).Contains("Assuming assembly reference"))
					{
						list3.Add(current2.Id);
						List<CompilerException> warnings = Warnings;
						start = ((LinePositionSpan)(ref span)).Start;
						int num2 = ((LinePosition)(ref start)).Line + 1;
						start = ((LinePositionSpan)(ref span)).Start;
						warnings.Add(new CompilerException(text, new CompilerError(fileNameWithoutExtension, num2, ((LinePosition)(ref start)).Character + 1, current2.Id, current2.GetMessage((IFormatProvider)CultureInfo.InvariantCulture))));
					}
				}
				Pool.FreeUnmanaged<string>(ref list3);
				Pool.FreeUnmanaged<string>(ref list4);
				if (val8.Success)
				{
					IsCompileSuccess = true;
					byte[] array = memoryStream.ToArray();
					if (array != null)
					{
						if (IsExtension)
						{
							_overrideExtensionPlugin(InitialSource.ContextFilePath, array);
						}
						_overridePlugin(Path.GetFileNameWithoutExtension(InitialSource.ContextFilePath), array);
						if (IsCompileTestMode)
						{
							Assembly = null;
						}
						else
						{
							Assembly = Assembly.Load(array);
							try
							{
								string fileNameWithoutExtension2 = Path.GetFileNameWithoutExtension(string.IsNullOrEmpty(InitialSource.ContextFileName) ? InitialSource.FileName : InitialSource.ContextFileName);
								bool isProfiledAssembly = MonoProfiler.TryStartProfileFor(MonoProfilerConfig.ProfileTypes.Plugin, Assembly, fileNameWithoutExtension2, incremental: true);
								Assemblies.Plugins.Update(fileNameWithoutExtension2, Assembly, string.IsNullOrEmpty(InitialSource.ContextFilePath) ? InitialSource.FilePath : InitialSource.ContextFilePath, isProfiledAssembly);
							}
							catch (Exception ex2)
							{
								Logger.Error("Couldn't cache assembly in Carbon's global database", ex2);
							}
						}
					}
				}
			}
			references.Clear();
			references = null;
			Pool.FreeUnmanaged<string>(ref list2);
			Pool.FreeUnmanaged<SyntaxTree>(ref list);
			CompileTime = _stopwatch.Elapsed;
			_stopwatch.Reset();
			Pool.FreeUnsafe<Stopwatch>(ref _stopwatch);
			if (IsCompileTestMode || Assembly == null)
			{
				return;
			}
			Type[] types = Assembly.GetTypes();
			foreach (Type type in types)
			{
				List<uint> list5 = new List<uint>();
				List<HookMethodAttribute> list6 = new List<HookMethodAttribute>();
				List<PluginReferenceAttribute> list7 = new List<PluginReferenceAttribute>();
				Hooks.Add(type, list5);
				HookMethods.Add(type, list6);
				PluginReferences.Add(type, list7);
				MethodInfo[] methods = type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
				foreach (MethodInfo methodInfo in methods)
				{
					if (InternalCallHook.HasRefLikeSignature(methodInfo))
					{
						continue;
					}
					if (Community.Runtime.HookManager.IsHook(methodInfo.Name))
					{
						uint orAdd = HookStringPool.GetOrAdd(methodInfo.Name);
						if (!list5.Contains(orAdd))
						{
							list5.Add(orAdd);
						}
						continue;
					}
					HookMethodAttribute customAttribute = methodInfo.GetCustomAttribute<HookMethodAttribute>();
					if (customAttribute != null)
					{
						customAttribute.Method = methodInfo;
						list6.Add(customAttribute);
					}
				}
				FieldInfo[] fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
				foreach (FieldInfo fieldInfo in fields)
				{
					PluginReferenceAttribute customAttribute2 = fieldInfo.GetCustomAttribute<PluginReferenceAttribute>();
					if (customAttribute2 != null)
					{
						customAttribute2.Field = fieldInfo;
						list7.Add(customAttribute2);
					}
				}
			}
			if (Exceptions.Count <= 0)
			{
				return;
			}
			throw null;
		}
		catch (Exception ex3)
		{
			Logger.Error("Threading compilation failed for '" + InitialSource?.ContextFilePath + "'", ex3);
			Analytics.plugin_native_compile_fail(InitialSource, ex3);
		}
	}

	public override void Dispose()
	{
		ClassList?.Clear();
		Exceptions?.Clear();
		Warnings?.Clear();
		Hooks?.Clear();
		HookMethods?.Clear();
		PluginReferences?.Clear();
		ClassList = null;
		Hooks = null;
		HookMethods = null;
		PluginReferences = null;
		Exceptions = null;
		Warnings = null;
		InternalCallHookSource = null;
	}
}
