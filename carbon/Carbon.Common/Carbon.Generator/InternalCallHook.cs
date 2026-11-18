using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using Carbon.InternalCallHookGeneration;
using Carbon.Pooling;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Mono.Cecil;

namespace Carbon.Generator;

public class InternalCallHook
{
	public static List<AssemblyDefinition> Assemblies = new List<AssemblyDefinition>();

	public static ConcurrentDictionary<string, int> InheritanceCache = new ConcurrentDictionary<string, int>();

	public static ConcurrentDictionary<string, TypeDefinition> TypeCache = new ConcurrentDictionary<string, TypeDefinition>();

	private static readonly ConditionalWeakTable<MetadataReference, string[]> RefLikeTypeNameCache = new ConditionalWeakTable<MetadataReference, string[]>();

	public static TypeDefinition FindTypeInAssemblies(string fullName)
	{
		return TypeCache.GetOrAdd(fullName, (Func<string, TypeDefinition>)FindTypeInAssembliesUncached);
	}

	private static TypeDefinition FindTypeInAssembliesUncached(string fullName)
	{
		for (int i = 0; i < Assemblies.Count; i++)
		{
			AssemblyDefinition val = Assemblies[i];
			TypeDefinition type = val.MainModule.GetType(fullName);
			if (type != null)
			{
				return type;
			}
			for (int j = 0; j < val.MainModule.Types.Count; j++)
			{
				type = val.MainModule.Types[j];
				if (((MemberReference)type).FullName == fullName || ((MemberReference)type).Name == fullName)
				{
					return type;
				}
			}
		}
		return null;
	}

	public static int GetInheritanceDepth(TypeDefinition type)
	{
		if (InheritanceCache.TryGetValue(((MemberReference)type).FullName, out var value))
		{
			return value;
		}
		TypeReference baseType = type.BaseType;
		while (baseType != null)
		{
			TypeDefinition val = baseType.Resolve();
			if (val == null)
			{
				break;
			}
			value++;
			baseType = val.BaseType;
		}
		InheritanceCache[((MemberReference)type).FullName] = value;
		return value;
	}

	public static int GetMethodParameterDepthScore(MethodDeclarationSyntax method)
	{
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		int num = 0;
		for (int i = 0; i < ((BaseParameterListSyntax)((BaseMethodDeclarationSyntax)method).ParameterList).Parameters.Count; i++)
		{
			ParameterSyntax val = ((BaseParameterListSyntax)((BaseMethodDeclarationSyntax)method).ParameterList).Parameters[i];
			if (((BaseParameterSyntax)val).Type != null)
			{
				TypeDefinition val2 = FindTypeInAssemblies(((object)((BaseParameterSyntax)val).Type).ToString());
				if (val2 != null)
				{
					num += GetInheritanceDepth(val2);
				}
			}
		}
		return num;
	}

	public static void GeneratePartial(CompilationUnitSyntax input, out CompilationUnitSyntax output, CSharpParseOptions options, string fileName, List<ClassDeclarationSyntax> classes = null, string debugOutputPath = null, List<string> usingsList = null, IEnumerable<MetadataReference> references = null, IReadOnlyList<SyntaxTree> sourceTrees = null)
	{
		BaseNamespaceDeclarationSyntax @namespace = null;
		if (classes == null)
		{
			classes = new List<ClassDeclarationSyntax>();
			FindPluginInfo(input, out @namespace, out var _, out var _, classes);
		}
		if (classes.Count == 0)
		{
			output = null;
			return;
		}
		if (@namespace == null)
		{
			SyntaxNode parent = ((SyntaxNode)classes[0]).Parent;
			@namespace = (BaseNamespaceDeclarationSyntax)(object)((parent is BaseNamespaceDeclarationSyntax) ? parent : null);
		}
		InternalCallHookTypeModel internalCallHookTypeModel = CreateModel(input, @namespace, classes, usingsList, references, options, sourceTrees);
		if (internalCallHookTypeModel.Methods.Count == 0)
		{
			output = null;
			return;
		}
		string text = InternalCallHookEmitter.BuildSource(internalCallHookTypeModel);
		string text2 = fileName + "/Internal";
		output = CSharpExtensions.GetCompilationUnitRoot(CSharpSyntaxTree.ParseText(text, options, text2, Encoding.UTF8, default(CancellationToken)), default(CancellationToken));
	}

	private static InternalCallHookTypeModel CreateModel(CompilationUnitSyntax input, BaseNamespaceDeclarationSyntax @namespace, List<ClassDeclarationSyntax> classes, List<string> usingsList, IEnumerable<MetadataReference> references, CSharpParseOptions options, IReadOnlyList<SyntaxTree> sourceTrees)
	{
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0201: Unknown result type (might be due to invalid IL or missing references)
		//IL_0206: Unknown result type (might be due to invalid IL or missing references)
		//IL_0268: Unknown result type (might be due to invalid IL or missing references)
		//IL_026d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0271: Unknown result type (might be due to invalid IL or missing references)
		//IL_0276: Unknown result type (might be due to invalid IL or missing references)
		//IL_0294: Unknown result type (might be due to invalid IL or missing references)
		//IL_031d: Unknown result type (might be due to invalid IL or missing references)
		//IL_037b: Unknown result type (might be due to invalid IL or missing references)
		InternalCallHookTypeModel internalCallHookTypeModel = new InternalCallHookTypeModel
		{
			NamespaceName = (((@namespace != null) ? ((object)@namespace.Name).ToString() : null) ?? string.Empty)
		};
		SyntaxToken identifier = ((BaseTypeDeclarationSyntax)classes[0]).Identifier;
		internalCallHookTypeModel.TypeName = identifier.ValueText;
		internalCallHookTypeModel.BaseKind = "plugin";
		internalCallHookTypeModel.VersionOwnerExpression = "base";
		InternalCallHookTypeModel model = internalCallHookTypeModel;
		model.GlobalUsings.AddRange(((IEnumerable<UsingDirectiveSyntax>)(object)input.Usings).Select((UsingDirectiveSyntax x) => ((object)x).ToString()));
		if (usingsList != null)
		{
			model.GlobalUsings.AddRange(usingsList);
		}
		if (@namespace != null)
		{
			model.NamespaceUsings.AddRange(((IEnumerable<UsingDirectiveSyntax>)(object)@namespace.Usings).Select((UsingDirectiveSyntax x) => ((object)x).ToString()));
		}
		MethodDeclarationSyntax[] array = classes.Where((ClassDeclarationSyntax x) =>
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			SyntaxToken identifier2 = ((BaseTypeDeclarationSyntax)x).Identifier;
			if (identifier2.ValueText == model.TypeName)
			{
				SyntaxNode parent = ((SyntaxNode)x).Parent;
				SyntaxNode obj = ((parent is BaseNamespaceDeclarationSyntax) ? parent : null);
				return (((obj != null) ? ((object)((BaseNamespaceDeclarationSyntax)obj).Name).ToString() : null) ?? string.Empty) == model.NamespaceName;
			}
			return false;
		}).SelectMany((ClassDeclarationSyntax x) => ((SyntaxNode)x).ChildNodes().OfType<MethodDeclarationSyntax>()).Where(IsHookableMethod)
			.OrderBy((MethodDeclarationSyntax x) =>
			{
				//IL_0001: Unknown result type (might be due to invalid IL or missing references)
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				SyntaxToken identifier2 = x.Identifier;
				return identifier2.ValueText;
			})
			.ToArray();
		Lazy<CSharpCompilation> analysis = new Lazy<CSharpCompilation>(() => CreateAnalysisCompilation(input, references, options, sourceTrees), isThreadSafe: false);
		HashSet<string> refLikeMethodKeys = GetRefLikeMethodKeys(input, references, options, array, analysis);
		MethodDeclarationSyntax[] array2 = array;
		foreach (MethodDeclarationSyntax val in array2)
		{
			if (refLikeMethodKeys != null && refLikeMethodKeys.Contains(GetMethodKey(val)))
			{
				continue;
			}
			string text = ResolveHookName(val, analysis);
			InternalCallHookMethodModel internalCallHookMethodModel = new InternalCallHookMethodModel();
			identifier = val.Identifier;
			internalCallHookMethodModel.MethodName = identifier.ValueText;
			internalCallHookMethodModel.HookName = text;
			internalCallHookMethodModel.HookId = HookStringPool.GetOrAdd(text);
			internalCallHookMethodModel.ReturnsVoid = ((object)val.ReturnType).ToString() == "void";
			internalCallHookMethodModel.Score = GetMethodParameterDepthScore(val);
			internalCallHookMethodModel.ConditionalSymbol = GetConditionalSymbol(val);
			InternalCallHookMethodModel internalCallHookMethodModel2 = internalCallHookMethodModel;
			Enumerator<ParameterSyntax> enumerator = ((BaseParameterListSyntax)((BaseMethodDeclarationSyntax)val).ParameterList).Parameters.GetEnumerator();
			while (enumerator.MoveNext())
			{
				ParameterSyntax current = enumerator.Current;
				if (((BaseParameterSyntax)current).Type != null)
				{
					bool flag = ((IEnumerable<SyntaxToken>)(object)((BaseParameterSyntax)current).Modifiers).Any((SyntaxToken x) =>
					{
						//IL_0000: Unknown result type (might be due to invalid IL or missing references)
						return CSharpExtensions.IsKind(x, (SyntaxKind)8361);
					});
					bool flag2 = current.Default != null || ((BaseParameterSyntax)current).Type is NullableTypeSyntax;
					List<InternalCallHookParameterModel> parameters = internalCallHookMethodModel2.Parameters;
					InternalCallHookParameterModel internalCallHookParameterModel = new InternalCallHookParameterModel();
					TypeSyntax type = ((BaseParameterSyntax)current).Type;
					TupleTypeSyntax val2 = (TupleTypeSyntax)(object)((type is TupleTypeSyntax) ? type : null);
					internalCallHookParameterModel.TypeName = ((val2 != null) ? ("(" + string.Join(", ", ((IEnumerable<TupleElementSyntax>)(object)val2.Elements).Select((TupleElementSyntax e) => ((object)e.Type).ToString())) + ")") : ((object)((BaseParameterSyntax)current).Type).ToString()).Replace("global::", string.Empty);
					internalCallHookParameterModel.IsOut = flag;
					internalCallHookParameterModel.IsRef = ((IEnumerable<SyntaxToken>)(object)((BaseParameterSyntax)current).Modifiers).Any((SyntaxToken x) =>
					{
						//IL_0000: Unknown result type (might be due to invalid IL or missing references)
						return CSharpExtensions.IsKind(x, (SyntaxKind)8360);
					});
					internalCallHookParameterModel.UseInlineDefaultExpression = flag2;
					internalCallHookParameterModel.RequiresGuard = !flag2 && !flag;
					parameters.Add(internalCallHookParameterModel);
				}
			}
			model.Methods.Add(internalCallHookMethodModel2);
		}
		return model;
	}

	private static bool IsHookableMethod(MethodDeclarationSyntax method)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		SyntaxTokenList modifiers = ((MemberDeclarationSyntax)method).Modifiers;
		if (modifiers.Count == 0 || ((IEnumerable<SyntaxToken>)(object)((MemberDeclarationSyntax)method).Modifiers).All((SyntaxToken modifier) =>
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_000d: Unknown result type (might be due to invalid IL or missing references)
			return !CSharpExtensions.IsKind(modifier, (SyntaxKind)8343) && !CSharpExtensions.IsKind(modifier, (SyntaxKind)8347);
		}) || ((IEnumerable<AttributeListSyntax>)(object)((MemberDeclarationSyntax)method).AttributeLists).Any((AttributeListSyntax x) =>
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return ((IEnumerable<AttributeSyntax>)(object)x.Attributes).Any((AttributeSyntax y) => IsHookMethodAttribute(y.Name));
		}))
		{
			return method.TypeParameterList == null;
		}
		return false;
	}

	private static CSharpCompilation CreateAnalysisCompilation(CompilationUnitSyntax input, IEnumerable<MetadataReference> references, CSharpParseOptions options, IReadOnlyList<SyntaxTree> sourceTrees)
	{
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Expected Obj, but got Unknown
		if (references == null || (ParseOptions)(object)options == (ParseOptions)null)
		{
			return null;
		}
		return CSharpCompilation.Create("Carbon.InternalCallHook.Analysis", (IEnumerable<SyntaxTree>)(sourceTrees ?? new SyntaxTree[1] { ((SyntaxNode)input).SyntaxTree }), references, new CSharpCompilationOptions((OutputKind)2, false, (string)null, (string)null, (string)null, (IEnumerable<string>)null, (OptimizationLevel)0, false, true, (string)null, (string)null, default(ImmutableArray<byte>), (bool?)null, (Platform)0, (ReportDiagnostic)0, 4, (IEnumerable<KeyValuePair<string, ReportDiagnostic>>)null, true, false, (XmlReferenceResolver)null, (SourceReferenceResolver)null, (MetadataReferenceResolver)null, (AssemblyIdentityComparer)null, (StrongNameProvider)null, false, (MetadataImportOptions)0, (NullableContextOptions)0));
	}

	private static HashSet<string> GetRefLikeMethodKeys(CompilationUnitSyntax input, IEnumerable<MetadataReference> references, CSharpParseOptions options, IReadOnlyCollection<MethodDeclarationSyntax> methods, Lazy<CSharpCompilation> analysis)
	{
		if (methods.Count == 0 || !methods.Any(CanHaveRefLikeSignature) || references == null || (ParseOptions)(object)options == (ParseOptions)null)
		{
			return null;
		}
		HashSet<string> refLikeTypeNames = GetRefLikeTypeNames(input, references);
		if (refLikeTypeNames != null && !AnySignatureMentions(methods, refLikeTypeNames))
		{
			return null;
		}
		CSharpCompilation value = analysis.Value;
		HashSet<string> hashSet = null;
		foreach (MethodDeclarationSyntax method in methods)
		{
			IMethodSymbol declaredSymbol = CSharpExtensions.GetDeclaredSymbol(value.GetSemanticModel(((SyntaxNode)method).SyntaxTree, true), (BaseMethodDeclarationSyntax)(object)method, default(CancellationToken));
			if (declaredSymbol != null && HasRefLikeSignature(declaredSymbol))
			{
				if (hashSet == null)
				{
					hashSet = new HashSet<string>();
				}
				hashSet.Add(GetMethodKey(method));
			}
		}
		return hashSet;
	}

	private static HashSet<string> GetRefLikeTypeNames(CompilationUnitSyntax input, IEnumerable<MetadataReference> references)
	{
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_016c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0171: Unknown result type (might be due to invalid IL or missing references)
		HashSet<string> hashSet = new HashSet<string>(StringComparer.Ordinal);
		foreach (MetadataReference reference in references)
		{
			string[] array = ((reference is PortableExecutableReference) ? RefLikeTypeNameCache.GetValue(reference, ReadRefLikeTypeNames) : null);
			if (array == null)
			{
				return null;
			}
			for (int i = 0; i < array.Length; i++)
			{
				hashSet.Add(array[i]);
			}
		}
		List<UsingDirectiveSyntax> list = null;
		SyntaxToken identifier;
		foreach (SyntaxNode item in ((SyntaxNode)input).DescendantNodes((Func<SyntaxNode, bool>)IsDeclarationContainer, false))
		{
			StructDeclarationSyntax val = (StructDeclarationSyntax)(object)((item is StructDeclarationSyntax) ? item : null);
			if (val != null && CSharpExtensions.Any(((MemberDeclarationSyntax)val).Modifiers, (SyntaxKind)8360))
			{
				identifier = ((BaseTypeDeclarationSyntax)val).Identifier;
				hashSet.Add(identifier.ValueText);
				continue;
			}
			UsingDirectiveSyntax val2 = (UsingDirectiveSyntax)(object)((item is UsingDirectiveSyntax) ? item : null);
			if (val2 != null && val2.Alias != null)
			{
				(list ?? (list = new List<UsingDirectiveSyntax>())).Add(val2);
			}
		}
		if (list != null)
		{
			for (int j = 0; j < list.Count; j++)
			{
				if (MentionsAny((SyntaxNode)(object)list[j].NamespaceOrType, hashSet))
				{
					identifier = ((SimpleNameSyntax)list[j].Alias.Name).Identifier;
					hashSet.Add(identifier.ValueText);
				}
			}
		}
		return hashSet;
	}

	private static bool IsDeclarationContainer(SyntaxNode node)
	{
		if (!(node is CompilationUnitSyntax) && !(node is BaseNamespaceDeclarationSyntax))
		{
			return node is TypeDeclarationSyntax;
		}
		return true;
	}

	private static string[] ReadRefLikeTypeNames(MetadataReference reference)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			Metadata metadata = ((PortableExecutableReference)reference).GetMetadata();
			List<string> list = new List<string>();
			AssemblyMetadata val = (AssemblyMetadata)(object)((metadata is AssemblyMetadata) ? metadata : null);
			if (val != null)
			{
				foreach (ModuleMetadata module in val.GetModules())
				{
					AddRefLikeTypeNames(module.GetMetadataReader(), list);
				}
			}
			else
			{
				AddRefLikeTypeNames(((ModuleMetadata)metadata).GetMetadataReader(), list);
			}
			return list.ToArray();
		}
		catch
		{
			return null;
		}
	}

	private static void AddRefLikeTypeNames(MetadataReader reader, List<string> names)
	{
		foreach (TypeDefinitionHandle typeDefinition2 in reader.TypeDefinitions)
		{
			TypeDefinition typeDefinition = reader.GetTypeDefinition(typeDefinition2);
			if (IsByRefLike(reader, typeDefinition))
			{
				string text = reader.GetString(typeDefinition.Name);
				int num = text.IndexOf('`');
				names.Add((num < 0) ? text : text.Substring(0, num));
			}
		}
	}

	private static bool IsByRefLike(MetadataReader reader, TypeDefinition type)
	{
		foreach (CustomAttributeHandle customAttribute in type.GetCustomAttributes())
		{
			EntityHandle constructor = reader.GetCustomAttribute(customAttribute).Constructor;
			StringHandle handle = default;
			if (constructor.Kind == HandleKind.MemberReference)
			{
				EntityHandle parent = reader.GetMemberReference((MemberReferenceHandle)constructor).Parent;
				if (parent.Kind == HandleKind.TypeReference)
				{
					handle = reader.GetTypeReference((TypeReferenceHandle)parent).Name;
				}
			}
			else if (constructor.Kind == HandleKind.MethodDefinition)
			{
				handle = reader.GetTypeDefinition(reader.GetMethodDefinition((MethodDefinitionHandle)constructor).GetDeclaringType()).Name;
			}
			if (!handle.IsNil && reader.StringComparer.Equals(handle, "IsByRefLikeAttribute"))
			{
				return true;
			}
		}
		return false;
	}

	private static bool AnySignatureMentions(IReadOnlyCollection<MethodDeclarationSyntax> methods, HashSet<string> names)
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		foreach (MethodDeclarationSyntax method in methods)
		{
			if (MentionsAny((SyntaxNode)(object)method.ReturnType, names))
			{
				return true;
			}
			SeparatedSyntaxList<ParameterSyntax> parameters = ((BaseParameterListSyntax)((BaseMethodDeclarationSyntax)method).ParameterList).Parameters;
			for (int i = 0; i < parameters.Count; i++)
			{
				if (MentionsAny((SyntaxNode)(object)((BaseParameterSyntax)parameters[i]).Type, names))
				{
					return true;
				}
			}
		}
		return false;
	}

	private static bool MentionsAny(SyntaxNode node, HashSet<string> names)
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		if (node == null)
		{
			return false;
		}
		foreach (SyntaxNode item in node.DescendantNodesAndSelf((Func<SyntaxNode, bool>)null, false))
		{
			SimpleNameSyntax val = (SimpleNameSyntax)(object)((item is SimpleNameSyntax) ? item : null);
			if (val != null)
			{
				SyntaxToken identifier = val.Identifier;
				if (names.Contains(identifier.ValueText))
				{
					return true;
				}
			}
		}
		return false;
	}

	private static bool CanHaveRefLikeSignature(MethodDeclarationSyntax method)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		if (!CanBeRefLikeType(method.ReturnType))
		{
			return ((IEnumerable<ParameterSyntax>)(object)((BaseParameterListSyntax)((BaseMethodDeclarationSyntax)method).ParameterList).Parameters).Any((ParameterSyntax x) => CanBeRefLikeType(((BaseParameterSyntax)x).Type));
		}
		return true;
	}

	private static bool CanBeRefLikeType(TypeSyntax type)
	{
		if (type != null)
		{
			return !(type is PredefinedTypeSyntax);
		}
		return false;
	}

	private static bool HasRefLikeSignature(IMethodSymbol method)
	{
		if (!method.ReturnType.IsRefLikeType)
		{
			return method.Parameters.Any((IParameterSymbol x) => x.Type.IsRefLikeType);
		}
		return true;
	}

	public static bool HasRefLikeSignature(MethodInfo method)
	{
		if (!IsRefLikeType(method.ReturnType))
		{
			return method.GetParameters().Any((ParameterInfo x) => IsRefLikeType(x.ParameterType));
		}
		return true;
	}

	private static bool IsRefLikeType(Type type)
	{
		while ((object)type != null && type.HasElementType)
		{
			type = type.GetElementType();
		}
		if (type == null)
		{
			return false;
		}
		try
		{
			return type.GetCustomAttributesData().Any((CustomAttributeData x) => x.AttributeType.FullName == "System.Runtime.CompilerServices.IsByRefLikeAttribute");
		}
		catch
		{
			return false;
		}
	}

	private static string GetMethodKey(MethodDeclarationSyntax method)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		object[] array = new object[4]
		{
			GetContainingTypeKey(method),
			null,
			null,
			null
		};
		SyntaxToken identifier = method.Identifier;
		array[1] = identifier.ValueText;
		array[2] = method.ReturnType;
		array[3] = string.Join(",", ((IEnumerable<ParameterSyntax>)(object)((BaseParameterListSyntax)((BaseMethodDeclarationSyntax)method).ParameterList).Parameters).Select(GetParameterKey));
		return string.Format("{0}|{1}|{2}|{3}", array);
	}

	private static string GetContainingTypeKey(MethodDeclarationSyntax method)
	{
		return string.Join(".", ((SyntaxNode)method).Ancestors(true).OfType<TypeDeclarationSyntax>().Reverse()
			.Select((TypeDeclarationSyntax x) =>
			{
				//IL_0001: Unknown result type (might be due to invalid IL or missing references)
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				SyntaxToken identifier = ((BaseTypeDeclarationSyntax)x).Identifier;
				return identifier.ValueText;
			}));
	}

	private static string GetParameterKey(ParameterSyntax parameter)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		return string.Format("{0}:{1}", string.Join(" ", ((IEnumerable<SyntaxToken>)(object)((BaseParameterSyntax)parameter).Modifiers).Select((SyntaxToken x) => x.Text)), ((BaseParameterSyntax)parameter).Type);
	}

	private static string ResolveHookName(MethodDeclarationSyntax method, Lazy<CSharpCompilation> analysis)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_01af: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0130: Unknown result type (might be due to invalid IL or missing references)
		//IL_0171: Unknown result type (might be due to invalid IL or missing references)
		//IL_0176: Unknown result type (might be due to invalid IL or missing references)
		AttributeSyntax hookMethod = ((IEnumerable<AttributeListSyntax>)(object)((MemberDeclarationSyntax)method).AttributeLists).SelectMany((AttributeListSyntax x) =>
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return (IEnumerable<AttributeSyntax>)(object)x.Attributes;
		}).FirstOrDefault((AttributeSyntax x) => IsHookMethodAttribute(x.Name));
		AttributeArgumentSyntax hookNameArgument = GetHookNameArgument(hookMethod);
		SyntaxToken val;
		if (hookNameArgument == null)
		{
			val = method.Identifier;
			return val.ValueText;
		}
		string result = ((object)hookNameArgument).ToString().Replace("\"", string.Empty);
		ExpressionSyntax expression = hookNameArgument.Expression;
		LiteralExpressionSyntax val2 = (LiteralExpressionSyntax)(object)((expression is LiteralExpressionSyntax) ? expression : null);
		if (val2 != null && CSharpExtensions.IsKind((SyntaxNode)(object)val2, (SyntaxKind)8750))
		{
			val = val2.Token;
			if (val.ValueText.Length <= 0)
			{
				val = method.Identifier;
				return val.ValueText;
			}
			val = val2.Token;
			return val.ValueText;
		}
		ExpressionSyntax expression2 = hookNameArgument.Expression;
		InvocationExpressionSyntax val3 = (InvocationExpressionSyntax)(object)((expression2 is InvocationExpressionSyntax) ? expression2 : null);
		if (val3 != null)
		{
			ExpressionSyntax expression3 = val3.Expression;
			IdentifierNameSyntax val4 = (IdentifierNameSyntax)(object)((expression3 is IdentifierNameSyntax) ? expression3 : null);
			if (val4 != null)
			{
				val = ((SimpleNameSyntax)val4).Identifier;
				if (val.ValueText == "nameof")
				{
					ArgumentListSyntax argumentList = val3.ArgumentList;
					if (argumentList != null)
					{
						SeparatedSyntaxList<ArgumentSyntax> arguments = ((BaseArgumentListSyntax)argumentList).Arguments;
						if (arguments.Count == 1)
						{
							ExpressionSyntax expression4 = arguments[0].Expression;
							ExpressionSyntax obj = ((expression4 is MemberAccessExpressionSyntax) ? expression4 : null);
							SimpleNameSyntax val5 = (SimpleNameSyntax)(((object)((obj != null) ? ((MemberAccessExpressionSyntax)obj).Name : null)) ?? ((object)((expression4 is SimpleNameSyntax) ? expression4 : null)));
							if (val5 != null)
							{
								val = val5.Identifier;
								return val.ValueText;
							}
						}
					}
				}
			}
		}
		CSharpCompilation value = analysis.Value;
		if (value != null)
		{
			Optional<object> constantValue = value.GetSemanticModel(((SyntaxNode)hookNameArgument).SyntaxTree, true).GetConstantValue((SyntaxNode)(object)hookNameArgument.Expression, default(CancellationToken));
			if (constantValue.HasValue)
			{
				if (!(constantValue.Value is string { Length: >0 } text))
				{
					val = method.Identifier;
					return val.ValueText;
				}
				return text;
			}
		}
		return result;
	}

	private static AttributeArgumentSyntax GetHookNameArgument(AttributeSyntax hookMethod)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		if (((hookMethod != null) ? hookMethod.ArgumentList : null) == null)
		{
			return null;
		}
		SeparatedSyntaxList<AttributeArgumentSyntax> arguments = hookMethod.ArgumentList.Arguments;
		AttributeArgumentSyntax val = null;
		AttributeArgumentSyntax val2 = null;
		for (int i = 0; i < arguments.Count; i++)
		{
			AttributeArgumentSyntax val3 = arguments[i];
			if (val3.NameEquals == null)
			{
				if (val == null)
				{
					val = val3;
				}
				continue;
			}
			SyntaxToken identifier = ((SimpleNameSyntax)val3.NameEquals.Name).Identifier;
			if (identifier.ValueText == "Name")
			{
				val2 = val3;
			}
		}
		return val2 ?? val;
	}

	private static string GetConditionalSymbol(MethodDeclarationSyntax method)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		AttributeSyntax? val = ((IEnumerable<AttributeListSyntax>)(object)((MemberDeclarationSyntax)method).AttributeLists).SelectMany((AttributeListSyntax x) =>
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return (IEnumerable<AttributeSyntax>)(object)x.Attributes;
		}).FirstOrDefault((AttributeSyntax x) => ((object)x.Name).ToString() == "Conditional");
		object obj;
		if (val == null)
		{
			obj = null;
		}
		else
		{
			AttributeArgumentListSyntax argumentList = val.ArgumentList;
			obj = ((argumentList == null) ? null : ((object)argumentList.Arguments.FirstOrDefault())?.ToString());
		}
		return ((string)obj)?.Replace("\"", string.Empty) ?? string.Empty;
	}

	private static bool IsHookMethodAttribute(NameSyntax name)
	{
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		QualifiedNameSyntax val = (QualifiedNameSyntax)(object)((name is QualifiedNameSyntax) ? name : null);
		if (val == null)
		{
			AliasQualifiedNameSyntax val2 = (AliasQualifiedNameSyntax)(object)((name is AliasQualifiedNameSyntax) ? name : null);
			if (val2 == null)
			{
				SimpleNameSyntax val3 = (SimpleNameSyntax)(object)((name is SimpleNameSyntax) ? name : null);
				if (val3 != null)
				{
					SyntaxToken identifier = val3.Identifier;
					string valueText = identifier.ValueText;
					return valueText == "HookMethod" || valueText == "HookMethodAttribute";
				}
				return false;
			}
			return IsHookMethodAttribute((NameSyntax)(object)val2.Name);
		}
		return IsHookMethodAttribute((NameSyntax)(object)val.Right);
	}

	private static bool IsInfoAttribute(NameSyntax name)
	{
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		QualifiedNameSyntax val = (QualifiedNameSyntax)(object)((name is QualifiedNameSyntax) ? name : null);
		if (val == null)
		{
			AliasQualifiedNameSyntax val2 = (AliasQualifiedNameSyntax)(object)((name is AliasQualifiedNameSyntax) ? name : null);
			if (val2 == null)
			{
				SimpleNameSyntax val3 = (SimpleNameSyntax)(object)((name is SimpleNameSyntax) ? name : null);
				if (val3 != null)
				{
					SyntaxToken identifier = val3.Identifier;
					string valueText = identifier.ValueText;
					return valueText == "Info" || valueText == "InfoAttribute";
				}
				return false;
			}
			return IsInfoAttribute((NameSyntax)(object)val2.Name);
		}
		return IsInfoAttribute((NameSyntax)(object)val.Right);
	}

	public static bool FindPluginInfo(CompilationUnitSyntax input, out BaseNamespaceDeclarationSyntax @namespace, out int namespaceIndex, out int classIndex, List<ClassDeclarationSyntax> classes)
	{
		//IL_0142: Unknown result type (might be due to invalid IL or missing references)
		//IL_0147: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_012e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		ClassDeclarationSyntax val = null;
		@namespace = null;
		namespaceIndex = 0;
		classIndex = 0;
		for (int i = 0; i < input.Members.Count; i++)
		{
			MemberDeclarationSyntax val2 = input.Members[i];
			BaseNamespaceDeclarationSyntax val3 = (BaseNamespaceDeclarationSyntax)(object)((val2 is BaseNamespaceDeclarationSyntax) ? val2 : null);
			if (val3 == null)
			{
				continue;
			}
			for (int j = 0; j < val3.Members.Count; j++)
			{
				MemberDeclarationSyntax val4 = val3.Members[j];
				ClassDeclarationSyntax val5 = (ClassDeclarationSyntax)(object)((val4 is ClassDeclarationSyntax) ? val4 : null);
				if (val5 == null)
				{
					continue;
				}
				bool flag = false;
				Enumerator<AttributeListSyntax> enumerator = ((MemberDeclarationSyntax)val5).AttributeLists.GetEnumerator();
				while (enumerator.MoveNext())
				{
					AttributeListSyntax current = enumerator.Current;
					Enumerator<AttributeSyntax> enumerator2 = current.Attributes.GetEnumerator();
					while (enumerator2.MoveNext())
					{
						AttributeSyntax current2 = enumerator2.Current;
						if (IsInfoAttribute(current2.Name))
						{
							namespaceIndex = i;
							@namespace = val3;
							classIndex = j;
							val = val5;
							flag = true;
							break;
						}
					}
					if (flag)
					{
						break;
					}
				}
				if (flag)
				{
					classes?.Insert(0, val);
				}
				else if (((IEnumerable<SyntaxToken>)(object)((MemberDeclarationSyntax)val5).Modifiers).Any((SyntaxToken x) =>
				{
					//IL_0000: Unknown result type (might be due to invalid IL or missing references)
					return CSharpExtensions.IsKind(x, (SyntaxKind)8406);
				}))
				{
					classes?.Add(val5);
				}
			}
		}
		return val != null;
	}
}
