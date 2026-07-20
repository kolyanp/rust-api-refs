using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using Carbon.InternalCallHookGeneration;
using Carbon.Pooling;
using HarmonyLib;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Mono.Cecil;

namespace Carbon.Generator;

public class InternalCallHook
{
	public static List<AssemblyDefinition> Assemblies = new List<AssemblyDefinition>();

	public static ConcurrentDictionary<string, int> InheritanceCache = new ConcurrentDictionary<string, int>();

	public static TypeDefinition FindTypeInAssemblies(string fullName)
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

	public static void GeneratePartial(CompilationUnitSyntax input, out CompilationUnitSyntax output, CSharpParseOptions options, string fileName, List<ClassDeclarationSyntax> classes = null, string debugOutputPath = null, List<string> usingsList = null, IEnumerable<MetadataReference> references = null)
	{
		BaseNamespaceDeclarationSyntax @namespace;
		if (classes == null)
		{
			classes = new List<ClassDeclarationSyntax>();
			FindPluginInfo(input, out @namespace, out var _, out var _, classes);
		}
		else
		{
			SyntaxNode parent = ((SyntaxNode)classes[0]).Parent;
			@namespace = (BaseNamespaceDeclarationSyntax)(object)((parent is BaseNamespaceDeclarationSyntax) ? parent : null);
		}
		if (classes.Count == 0)
		{
			output = null;
			return;
		}
		InternalCallHookTypeModel internalCallHookTypeModel = CreateModel(input, @namespace, classes, usingsList, references, options);
		if (internalCallHookTypeModel.Methods.Count == 0)
		{
			output = null;
			return;
		}
		string text = InternalCallHookEmitter.BuildSource(internalCallHookTypeModel);
		string text2 = fileName + "/Internal";
		output = CSharpExtensions.GetCompilationUnitRoot(CSharpSyntaxTree.ParseText(text, options, text2, Encoding.UTF8, default(CancellationToken)), default(CancellationToken));
	}

	private static InternalCallHookTypeModel CreateModel(CompilationUnitSyntax input, BaseNamespaceDeclarationSyntax @namespace, List<ClassDeclarationSyntax> classes, List<string> usingsList, IEnumerable<MetadataReference> references, CSharpParseOptions options)
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0190: Unknown result type (might be due to invalid IL or missing references)
		//IL_0195: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0204: Unknown result type (might be due to invalid IL or missing references)
		//IL_0222: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_0309: Unknown result type (might be due to invalid IL or missing references)
		InternalCallHookTypeModel obj = new InternalCallHookTypeModel
		{
			NamespaceName = (((@namespace != null) ? ((object)@namespace.Name).ToString() : null) ?? string.Empty)
		};
		SyntaxToken identifier = ((BaseTypeDeclarationSyntax)classes[0]).Identifier;
		obj.TypeName = ((SyntaxToken)(ref identifier)).ValueText;
		obj.BaseKind = "plugin";
		obj.VersionOwnerExpression = "base";
		InternalCallHookTypeModel internalCallHookTypeModel = obj;
		internalCallHookTypeModel.GlobalUsings.AddRange(((IEnumerable<UsingDirectiveSyntax>)(object)input.Usings).Select((UsingDirectiveSyntax x) => ((object)x).ToString()));
		if (usingsList != null)
		{
			internalCallHookTypeModel.GlobalUsings.AddRange(usingsList);
		}
		if (@namespace != null)
		{
			internalCallHookTypeModel.NamespaceUsings.AddRange(((IEnumerable<UsingDirectiveSyntax>)(object)@namespace.Usings).Select((UsingDirectiveSyntax x) => ((object)x).ToString()));
		}
		MethodDeclarationSyntax[] array = classes.SelectMany((ClassDeclarationSyntax x) => ((SyntaxNode)x).ChildNodes().OfType<MethodDeclarationSyntax>()).Where(IsHookableMethod).OrderBy(delegate(MethodDeclarationSyntax x)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			SyntaxToken identifier2 = x.Identifier;
			return ((SyntaxToken)(ref identifier2)).ValueText;
		})
			.ToArray();
		HashSet<string> refLikeMethodKeys = GetRefLikeMethodKeys(input, references, options, array);
		MethodDeclarationSyntax[] array2 = array;
		foreach (MethodDeclarationSyntax val in array2)
		{
			if (refLikeMethodKeys != null && refLikeMethodKeys.Contains(GetMethodKey(val)))
			{
				continue;
			}
			string text = ResolveHookName(val, classes);
			InternalCallHookMethodModel internalCallHookMethodModel = new InternalCallHookMethodModel();
			identifier = val.Identifier;
			internalCallHookMethodModel.MethodName = ((SyntaxToken)(ref identifier)).ValueText;
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
					bool flag = ((IEnumerable<SyntaxToken>)(object)((BaseParameterSyntax)current).Modifiers).Any((SyntaxToken x) => CSharpExtensions.IsKind(x, (SyntaxKind)8361));
					bool flag2 = current.Default != null || ((BaseParameterSyntax)current).Type is NullableTypeSyntax;
					List<InternalCallHookParameterModel> parameters = internalCallHookMethodModel2.Parameters;
					InternalCallHookParameterModel internalCallHookParameterModel = new InternalCallHookParameterModel();
					TypeSyntax type = ((BaseParameterSyntax)current).Type;
					TupleTypeSyntax val2 = (TupleTypeSyntax)(object)((type is TupleTypeSyntax) ? type : null);
					internalCallHookParameterModel.TypeName = ((val2 != null) ? ("(" + string.Join(", ", ((IEnumerable<TupleElementSyntax>)(object)val2.Elements).Select((TupleElementSyntax e) => ((object)e.Type).ToString())) + ")") : ((object)((BaseParameterSyntax)current).Type).ToString()).Replace("global::", string.Empty);
					internalCallHookParameterModel.IsOut = flag;
					internalCallHookParameterModel.IsRef = ((IEnumerable<SyntaxToken>)(object)((BaseParameterSyntax)current).Modifiers).Any((SyntaxToken x) => CSharpExtensions.IsKind(x, (SyntaxKind)8360));
					internalCallHookParameterModel.UseInlineDefaultExpression = flag2;
					internalCallHookParameterModel.RequiresGuard = !flag2 && !flag;
					parameters.Add(internalCallHookParameterModel);
				}
			}
			internalCallHookTypeModel.Methods.Add(internalCallHookMethodModel2);
		}
		return internalCallHookTypeModel;
	}

	private static bool IsHookableMethod(MethodDeclarationSyntax method)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		SyntaxTokenList modifiers = ((MemberDeclarationSyntax)method).Modifiers;
		if (((SyntaxTokenList)(ref modifiers)).Count == 0 || ((IEnumerable<SyntaxToken>)(object)((MemberDeclarationSyntax)method).Modifiers).All((SyntaxToken modifier) => !CSharpExtensions.IsKind(modifier, (SyntaxKind)8343) && !CSharpExtensions.IsKind(modifier, (SyntaxKind)8347)) || ((IEnumerable<AttributeListSyntax>)(object)((MemberDeclarationSyntax)method).AttributeLists).Any((AttributeListSyntax x) => ((IEnumerable<AttributeSyntax>)(object)x.Attributes).Any((AttributeSyntax y) => ((object)y.Name).ToString() == "HookMethod")))
		{
			return method.TypeParameterList == null;
		}
		return false;
	}

	private static HashSet<string> GetRefLikeMethodKeys(CompilationUnitSyntax input, IEnumerable<MetadataReference> references, CSharpParseOptions options, IReadOnlyCollection<MethodDeclarationSyntax> methods)
	{
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Expected O, but got Unknown
		if (methods.Count == 0 || !methods.Any(CanHaveRefLikeSignature) || references == null || (ParseOptions)(object)options == (ParseOptions)null)
		{
			return null;
		}
		HashSet<string> hashSet = new HashSet<string>(methods.Select(GetMethodKey));
		SyntaxTree val = CSharpSyntaxTree.Create((CSharpSyntaxNode)(object)input, options, "", (Encoding)null);
		CSharpCompilation val2 = CSharpCompilation.Create("Carbon.InternalCallHook.Analysis", (IEnumerable<SyntaxTree>)(object)new SyntaxTree[1] { val }, references, new CSharpCompilationOptions((OutputKind)2, false, (string)null, (string)null, (string)null, (IEnumerable<string>)null, (OptimizationLevel)0, false, true, (string)null, (string)null, default(ImmutableArray<byte>), (bool?)null, (Platform)0, (ReportDiagnostic)0, 4, (IEnumerable<KeyValuePair<string, ReportDiagnostic>>)null, true, false, (XmlReferenceResolver)null, (SourceReferenceResolver)null, (MetadataReferenceResolver)null, (AssemblyIdentityComparer)null, (StrongNameProvider)null, false, (MetadataImportOptions)0, (NullableContextOptions)0));
		SemanticModel semanticModel = val2.GetSemanticModel(val, true);
		HashSet<string> hashSet2 = null;
		foreach (MethodDeclarationSyntax item in ((SyntaxNode)CSharpExtensions.GetCompilationUnitRoot(val, default(CancellationToken))).DescendantNodes((Func<SyntaxNode, bool>)null, false).OfType<MethodDeclarationSyntax>())
		{
			if (!hashSet.Contains(GetMethodKey(item)))
			{
				continue;
			}
			IMethodSymbol declaredSymbol = CSharpExtensions.GetDeclaredSymbol(semanticModel, (BaseMethodDeclarationSyntax)(object)item, default(CancellationToken));
			if (declaredSymbol != null && HasRefLikeSignature(declaredSymbol))
			{
				if (hashSet2 == null)
				{
					hashSet2 = new HashSet<string>();
				}
				hashSet2.Add(GetMethodKey(item));
			}
		}
		return hashSet2;
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
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		if (!method.ReturnType.IsRefLikeType)
		{
			return ImmutableArrayExtensions.Any<IParameterSymbol>(method.Parameters, (Func<IParameterSymbol, bool>)((IParameterSymbol x) => x.Type.IsRefLikeType));
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
		object[] obj = new object[4]
		{
			GetContainingTypeKey(method),
			null,
			null,
			null
		};
		SyntaxToken identifier = method.Identifier;
		obj[1] = ((SyntaxToken)(ref identifier)).ValueText;
		obj[2] = method.ReturnType;
		obj[3] = string.Join(",", ((IEnumerable<ParameterSyntax>)(object)((BaseParameterListSyntax)((BaseMethodDeclarationSyntax)method).ParameterList).Parameters).Select(GetParameterKey));
		return string.Format("{0}|{1}|{2}|{3}", obj);
	}

	private static string GetContainingTypeKey(MethodDeclarationSyntax method)
	{
		return string.Join(".", ((SyntaxNode)method).Ancestors(true).OfType<TypeDeclarationSyntax>().Reverse()
			.Select(delegate(TypeDeclarationSyntax x)
			{
				//IL_0001: Unknown result type (might be due to invalid IL or missing references)
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				SyntaxToken identifier = ((BaseTypeDeclarationSyntax)x).Identifier;
				return ((SyntaxToken)(ref identifier)).ValueText;
			}));
	}

	private static string GetParameterKey(ParameterSyntax parameter)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		return string.Format("{0}:{1}", string.Join(" ", ((IEnumerable<SyntaxToken>)(object)((BaseParameterSyntax)parameter).Modifiers).Select((SyntaxToken x) => ((SyntaxToken)(ref x)).Text)), ((BaseParameterSyntax)parameter).Type);
	}

	private static string ResolveHookName(MethodDeclarationSyntax method, List<ClassDeclarationSyntax> classes)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c6: Unknown result type (might be due to invalid IL or missing references)
		AttributeSyntax val = ((IEnumerable<AttributeListSyntax>)(object)((MemberDeclarationSyntax)method).AttributeLists).Select((AttributeListSyntax x) => ((IEnumerable<AttributeSyntax>)(object)x.Attributes).FirstOrDefault((AttributeSyntax val4) => ((object)val4.Name).ToString() == "HookMethod")).FirstOrDefault();
		string text;
		if (val != null)
		{
			AttributeArgumentListSyntax argumentList = val.ArgumentList;
			if (argumentList != null && argumentList.Arguments.Count > 0)
			{
				text = ((object)val.ArgumentList.Arguments[0]).ToString().Replace("\"", string.Empty);
				goto IL_0092;
			}
		}
		SyntaxToken identifier = method.Identifier;
		text = ((SyntaxToken)(ref identifier)).ValueText;
		goto IL_0092;
		IL_0092:
		string result = text;
		if (val != null)
		{
			AttributeArgumentListSyntax argumentList2 = val.ArgumentList;
			if (argumentList2 == null || argumentList2.Arguments.Count != 0)
			{
				AttributeArgumentSyntax val2 = val.ArgumentList.Arguments[0];
				string text2 = ((object)val2).ToString();
				if (text2.Contains("nameof"))
				{
					result = text2.Replace("nameof", string.Empty).Replace("(", string.Empty).Replace(")", string.Empty);
					if (result.Contains("."))
					{
						result = result.Split('.')[^1];
					}
					return result;
				}
				if (text2.Contains("."))
				{
					ExpressionSyntax expression = val2.Expression;
					MemberAccessExpressionSyntax val3 = (MemberAccessExpressionSyntax)(object)((expression is MemberAccessExpressionSyntax) ? expression : null);
					string text3 = ((val3 != null) ? ((object)val3.Expression).ToString() : null);
					string text4 = ((val3 != null) ? ((object)val3.Name).ToString() : null);
					string text5 = AccessTools.Field(AccessTools.TypeByName(text3), text4)?.GetValue(null)?.ToString();
					if (!string.IsNullOrEmpty(text5))
					{
						return text5;
					}
				}
				if (text2.Contains("\""))
				{
					identifier = ((BaseTypeDeclarationSyntax)classes[0]).Identifier;
					string text6 = AccessTools.Field(AccessTools.TypeByName(((SyntaxToken)(ref identifier)).Text), text2.Replace("\"", string.Empty))?.GetValue(null)?.ToString();
					if (!string.IsNullOrEmpty(text6))
					{
						return text6;
					}
				}
				return result;
			}
		}
		return result;
	}

	private static string GetConditionalSymbol(MethodDeclarationSyntax method)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		AttributeSyntax? obj = ((IEnumerable<AttributeListSyntax>)(object)((MemberDeclarationSyntax)method).AttributeLists).SelectMany((AttributeListSyntax x) => (IEnumerable<AttributeSyntax>)(object)x.Attributes).FirstOrDefault((AttributeSyntax x) => ((object)x.Name).ToString() == "Conditional");
		object obj2;
		if (obj == null)
		{
			obj2 = null;
		}
		else
		{
			AttributeArgumentListSyntax argumentList = obj.ArgumentList;
			obj2 = ((argumentList == null) ? null : ((object)argumentList.Arguments.FirstOrDefault())?.ToString());
		}
		return ((string)obj2)?.Replace("\"", string.Empty) ?? string.Empty;
	}

	public static bool FindPluginInfo(CompilationUnitSyntax input, out BaseNamespaceDeclarationSyntax @namespace, out int namespaceIndex, out int classIndex, List<ClassDeclarationSyntax> classes)
	{
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		//IL_013a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
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
				if (((MemberDeclarationSyntax)val5).AttributeLists.Count > 0)
				{
					Enumerator<AttributeListSyntax> enumerator = ((MemberDeclarationSyntax)val5).AttributeLists.GetEnumerator();
					while (enumerator.MoveNext())
					{
						AttributeListSyntax current = enumerator.Current;
						NameSyntax name = current.Attributes[0].Name;
						IdentifierNameSyntax val6 = (IdentifierNameSyntax)(object)((name is IdentifierNameSyntax) ? name : null);
						if (val6 != null)
						{
							SyntaxToken identifier = ((SimpleNameSyntax)val6).Identifier;
							if (((SyntaxToken)(ref identifier)).Text.Equals("Info"))
							{
								namespaceIndex = i;
								@namespace = val3;
								classIndex = j;
								val = val5;
								classes?.Insert(0, val);
							}
						}
					}
				}
				else if (((IEnumerable<SyntaxToken>)(object)((MemberDeclarationSyntax)val5).Modifiers).Any((SyntaxToken x) => CSharpExtensions.IsKind(x, (SyntaxKind)8406)))
				{
					classes?.Add(val5);
				}
			}
		}
		return val != null;
	}
}
