using System;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Carbon.CompilerPolyfills;

[Generator]
public sealed class CompilerPolyfillGenerator : ISourceGenerator
{
	public void Initialize(GeneratorInitializationContext context)
	{
	}

	public void Execute(GeneratorExecutionContext context)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		string value = default(string);
		bool result = default(bool);
		bool forceDesignTimePolyfills = ((GeneratorExecutionContext)(ref context)).AnalyzerConfigOptions.GlobalOptions.TryGetValue("build_property.DesignTimeBuild", ref value) && bool.TryParse(value, out result) && result;
		string a = default(string);
		string accessibility = ((((GeneratorExecutionContext)(ref context)).AnalyzerConfigOptions.GlobalOptions.TryGetValue("build_property.CarbonCompilerPolyfillsAccessibility", ref a) && string.Equals(a, "internal", StringComparison.OrdinalIgnoreCase)) ? "internal" : "public");
		string text = CompilerPolyfillCatalog.BuildSource((string metadataName) => IsAvailable(((GeneratorExecutionContext)(ref context)).Compilation.GetTypeByMetadataName(metadataName), accessibility), forceDesignTimePolyfills, accessibility);
		if (text.Length != 0)
		{
			((GeneratorExecutionContext)(ref context)).AddSource("Carbon.CompilerPolyfills.g.cs", SourceText.From(text, Encoding.UTF8, (SourceHashAlgorithm)1));
		}
	}

	private static bool IsAvailable(INamedTypeSymbol? symbol, string accessibility)
	{
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Invalid comparison between Unknown and I4
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Invalid comparison between Unknown and I4
		if (symbol == null || (int)((ISymbol)symbol).DeclaredAccessibility != 6)
		{
			if (accessibility == "internal")
			{
				if (symbol == null)
				{
					return false;
				}
				return (int)((ISymbol)symbol).DeclaredAccessibility == 4;
			}
			return false;
		}
		return true;
	}
}
