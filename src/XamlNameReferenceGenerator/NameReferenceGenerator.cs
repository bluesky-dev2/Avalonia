﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System.Text;
using Microsoft.CodeAnalysis.CSharp;
using XamlNameReferenceGenerator.Infrastructure;

[assembly: InternalsVisibleTo("XamlNameReferenceGenerator.Tests")]

namespace XamlNameReferenceGenerator
{
    [Generator]
    public class NameReferenceGenerator : ISourceGenerator
    {
        private const string AttributeName = "GenerateTypedNameReferencesAttribute";
        private const string AttributeFile = "GenerateTypedNameReferencesAttribute";
        private const string AttributeCode = @"// <auto-generated />

using System;

[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
internal sealed class GenerateTypedNameReferencesAttribute : Attribute { }
";
        private static readonly SymbolDisplayFormat SymbolDisplayFormat = new SymbolDisplayFormat(
            typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
            genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters |
                             SymbolDisplayGenericsOptions.IncludeTypeConstraints |
                             SymbolDisplayGenericsOptions.IncludeVariance);

        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForSyntaxNotifications(() => new NameReferenceSyntaxReceiver());
        }

        public void Execute(GeneratorExecutionContext context)
        {
            context.AddSource(AttributeFile, SourceText.From(AttributeCode, Encoding.UTF8));
            if (!(context.SyntaxReceiver is NameReferenceSyntaxReceiver receiver))
            {
                return;
            }

            var compilation = (CSharpCompilation) context.Compilation;
            var xamlParser = new NameResolver(compilation);
            var symbols = UnpackAnnotatedTypes(context, compilation, receiver);
            if (symbols == null)
            {
                return;
            }

            foreach (var typeSymbol in symbols)
            {
                var xamlFileName = $"{typeSymbol.Name}.xaml";
                var aXamlFileName = $"{typeSymbol.Name}.axaml";
                var relevantXamlFile = context
                    .AdditionalFiles
                    .FirstOrDefault(text =>
                        text.Path.EndsWith(xamlFileName) ||
                        text.Path.EndsWith(aXamlFileName));

                if (relevantXamlFile is null)
                {
                    context.ReportDiagnostic(
                        Diagnostic.Create(
                            new DiagnosticDescriptor(
                                "AXN0001",
                                "Unable to discover the relevant Avalonia XAML file.",
                                "Unable to discover the relevant Avalonia XAML file " +
                                $"neither at {xamlFileName} nor at {aXamlFileName}",
                                "Usage",
                                DiagnosticSeverity.Error,
                                true),
                            Location.None));
                    return;
                }

                try
                {
                    var sourceCode = GenerateSourceCode(xamlParser, typeSymbol, relevantXamlFile);
                    context.AddSource($"{typeSymbol.Name}.g.cs", SourceText.From(sourceCode, Encoding.UTF8));
                }
                catch (Exception exception)
                {
                    context.ReportDiagnostic(
                        Diagnostic.Create(
                            new DiagnosticDescriptor(
                                "AXN0002",
                                "Unhandled exception occured while generating typed Name references.",
                                $"Unhandled exception occured while generating typed Name references: {exception}",
                                "Usage",
                                DiagnosticSeverity.Error,
                                true),
                            Location.None));
                    return;
                }
            }
        }

        private static string GenerateSourceCode(
            INameResolver nameResolver,
            INamedTypeSymbol classSymbol,
            AdditionalText xamlFile)
        {
            var className = classSymbol.Name;
            var nameSpace = classSymbol.ContainingNamespace.ToDisplayString(SymbolDisplayFormat);
            var namedControls = nameResolver
                .ResolveNames(xamlFile.GetText()!.ToString())
                .Select(info => "        " +
                                $"internal {info.TypeName} {info.Name} => " +
                                $"this.FindControl<{info.TypeName}>(\"{info.Name}\");");
            return $@"// <auto-generated />

using Avalonia.Controls;

namespace {nameSpace}
{{
    partial class {className}
    {{
{string.Join("\n", namedControls)}   
    }}
}}
";
        }

        private static IReadOnlyList<INamedTypeSymbol> UnpackAnnotatedTypes(
            GeneratorExecutionContext context,
            CSharpCompilation existingCompilation,
            NameReferenceSyntaxReceiver nameReferenceSyntaxReceiver)
        {
            var options = (CSharpParseOptions)existingCompilation.SyntaxTrees[0].Options;
            var compilation = existingCompilation.AddSyntaxTrees(
                CSharpSyntaxTree.ParseText(
                    SourceText.From(AttributeCode, Encoding.UTF8),
                    options));

            var symbols = new List<INamedTypeSymbol>();
            var attributeSymbol = compilation.GetTypeByMetadataName(AttributeName);
            foreach (var candidateClass in nameReferenceSyntaxReceiver.CandidateClasses)
            {
                var model = compilation.GetSemanticModel(candidateClass.SyntaxTree);
                var typeSymbol = (INamedTypeSymbol) model.GetDeclaredSymbol(candidateClass);
                var relevantAttribute = typeSymbol!
                    .GetAttributes()
                    .FirstOrDefault(attr => attr.AttributeClass!.Equals(attributeSymbol, SymbolEqualityComparer.Default));

                if (relevantAttribute == null)
                {
                    continue;
                }

                var isPartial = candidateClass
                    .Modifiers
                    .Any(modifier => modifier.IsKind(SyntaxKind.PartialKeyword));

                if (isPartial)
                {
                    symbols.Add(typeSymbol);
                }
                else
                {
                    var missingPartialKeywordMessage =
                        $"The type {typeSymbol.Name} should be declared with the 'partial' keyword " +
                        "as it is annotated with the [GenerateTypedNameReferences] attribute.";

                    context.ReportDiagnostic(
                        Diagnostic.Create(
                            new DiagnosticDescriptor(
                                "AXN0003",
                                missingPartialKeywordMessage,
                                missingPartialKeywordMessage,
                                "Usage",
                                DiagnosticSeverity.Error,
                                true),
                            Location.None));
                    return null;
                }
            }

            return symbols;
        }
    }
}