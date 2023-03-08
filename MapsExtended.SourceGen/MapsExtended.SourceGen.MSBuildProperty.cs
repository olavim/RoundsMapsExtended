using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;

namespace MapsExtended.SourceGen
{
	[Generator]
	public class MSBuildPropertySourceGenerator : ISourceGenerator
	{
		private const string AttributeSource = @"
namespace MapsExt.SourceGen
{
	[System.AttributeUsage(System.AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
	internal class MSBuildPropertyAttribute : System.Attribute {
		public readonly string property;
		public MSBuildPropertyAttribute(string property)
		{
			this.property = property;
		}
	}
}
";

		public void Initialize(GeneratorInitializationContext ctx)
		{
			ctx.RegisterForPostInitialization(i => i.AddSource("MSBuildPropertyAttribute.g.cs", AttributeSource));
			ctx.RegisterForSyntaxNotifications(() => new MSBuildPropertyClassesCollector());
		}

		public void Execute(GeneratorExecutionContext ctx)
		{
			if (!(ctx.SyntaxContextReceiver is MSBuildPropertyClassesCollector collector))
			{
				return;
			}

			foreach (var type in collector.Classes)
			{
				var attributes = type.GetAttributes().Where(att => att.AttributeClass.ToDisplayString() == "MapsExt.SourceGen.MSBuildPropertyAttribute");
				var properties = new Dictionary<string, string>();

				foreach (var att in attributes)
				{
					string name = att.ConstructorArguments[0].Value as string;
					ctx.AnalyzerConfigOptions.GlobalOptions.TryGetValue($"build_property.{name}", out string value);
					properties[name] = value;
				}

				var source = $@"
namespace {type.ContainingNamespace.ToDisplayString()}
{{
	public partial class {type.Name}
	{{
		{string.Join("\n", properties.Keys.Select(key => $"internal const string MSBuild_{key} = \"{properties[key]}\";"))}
	}}
}}
";
				ctx.AddSource($"{type.ToDisplayString()}.g.MSBuildProperty.cs", source);
			}
		}
	}

	internal class MSBuildPropertyClassesCollector : ISyntaxContextReceiver
	{
		public List<ITypeSymbol> Classes { get; } = new List<ITypeSymbol>();

		public void OnVisitSyntaxNode(GeneratorSyntaxContext ctx)
		{
			if (ctx.Node is ClassDeclarationSyntax cls && cls.AttributeLists.Count > 0)
			{
				var type = ctx.SemanticModel.GetDeclaredSymbol(cls) as ITypeSymbol;
				if (type.GetAttributes().Any(att => att.AttributeClass.ToDisplayString() == "MapsExt.SourceGen.MSBuildPropertyAttribute"))
				{
					Classes.Add(type);
				}
			}
		}
	}
}
