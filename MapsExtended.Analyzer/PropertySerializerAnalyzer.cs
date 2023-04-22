using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace MapsExt.Analyzers
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public class MyAnalyzer : DiagnosticAnalyzer
	{
		private const string DiagnosticId = "PropertySerializerAnalyzer";
		private const string Title = "Class does not implement interface";
		private const string Category = "Naming";

		private static readonly (string, string)[] RequiredImplementations = new[]
		{
			("MapsExt.Properties.PropertySerializerAttribute", "MapsExt.Properties.IPropertyWriter"),
			("MapsExt.Editor.Properties.EditorPropertySerializerAttribute", "MapsExt.Properties.IPropertyWriter"),
			("MapsExt.Editor.Properties.EditorPropertySerializerAttribute", "MapsExt.Editor.Properties.IPropertyReader")
		};

		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
		{
			get
			{
				var list = new List<DiagnosticDescriptor>();
				for (int i = 0; i < RequiredImplementations.Length; i++)
				{
					string message = $"Class '{{0}}' does not implement interface '{RequiredImplementations[i].Item2}<>'";
					list.Add(GetDescriptor(message));
				}
				return ImmutableArray.Create(list.ToArray());
			}
		}

		public override void Initialize(AnalysisContext context)
		{
			context.EnableConcurrentExecution();
			context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
			context.RegisterSyntaxNodeAction(AnalyzeSyntaxNode, SyntaxKind.ClassDeclaration);
		}

		private static void AnalyzeSyntaxNode(SyntaxNodeAnalysisContext context)
		{
			var symbolDisplayFormat = new SymbolDisplayFormat(typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces);
			var classDeclarationSyntax = (ClassDeclarationSyntax) context.Node;
			var declaredSymbol = context.SemanticModel.GetDeclaredSymbol(classDeclarationSyntax);

			for (int i = 0; i < RequiredImplementations.Length; i++)
			{
				string attributeTypeName = RequiredImplementations[i].Item1;
				string interfaceTypeName = RequiredImplementations[i].Item2;

				var attribute = declaredSymbol.GetAttributes().FirstOrDefault(x => x.AttributeClass.ToDisplayString(symbolDisplayFormat) == attributeTypeName);

				if (attribute is null)
				{
					continue;
				}

				var propertyType = (INamedTypeSymbol) attribute.ConstructorArguments[0].Value;
				var baseType = declaredSymbol.AllInterfaces.ToList().Find(x => x.ToDisplayString(symbolDisplayFormat) == interfaceTypeName);

				if (baseType is null || baseType.TypeArguments[0].ToDisplayString(symbolDisplayFormat) != propertyType.ToDisplayString(symbolDisplayFormat))
				{
					string message = $"Class '{{0}}' does not implement interface '{interfaceTypeName}<{propertyType.Name}>'";
					var diagnostic = Diagnostic.Create(GetDescriptor(message), classDeclarationSyntax.Identifier.GetLocation(), classDeclarationSyntax.Identifier.ToString());
					context.ReportDiagnostic(diagnostic);
				}
			}
		}

		private static DiagnosticDescriptor GetDescriptor(string message)
		{
			return new DiagnosticDescriptor(DiagnosticId, Title, message, Category, DiagnosticSeverity.Error, isEnabledByDefault: true);
		}
	}
}
