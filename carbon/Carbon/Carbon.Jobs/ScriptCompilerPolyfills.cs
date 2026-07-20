using System.Collections.Generic;
using System.Text;
using System.Threading;
using Carbon.CompilerPolyfills;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Carbon.Jobs;

internal static class ScriptCompilerPolyfills
{
	public static void InjectMissingPolyfills(List<SyntaxTree> trees, IReadOnlyCollection<MetadataReference> references, CSharpParseOptions parseOptions)
	{
		CSharpCompilation probeCompilation = CSharpCompilation.Create("__CarbonPolyfillProbe", (IEnumerable<SyntaxTree>)null, (IEnumerable<MetadataReference>)references, (CSharpCompilationOptions)null);
		string text = CompilerPolyfillCatalog.BuildSource((string metadataName) => IsPublic(((Compilation)probeCompilation).GetTypeByMetadataName(metadataName)), forceDesignTimePolyfills: false);
		if (text.Length != 0)
		{
			trees.Insert(0, CSharpSyntaxTree.ParseText(text, parseOptions, "__Carbon.CompilerPolyfills.g.cs", Encoding.UTF8, default(CancellationToken)));
		}
	}

	private static bool IsPublic(INamedTypeSymbol symbol)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Invalid comparison between Unknown and I4
		if (symbol == null)
		{
			return false;
		}
		return (int)((ISymbol)symbol).DeclaredAccessibility == 6;
	}
}
