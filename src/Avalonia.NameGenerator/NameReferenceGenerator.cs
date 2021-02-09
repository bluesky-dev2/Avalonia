﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System.Text;
using Avalonia.NameGenerator.Resolver;
using Microsoft.CodeAnalysis.CSharp;

[assembly: InternalsVisibleTo("Avalonia.NameGenerator.Tests")]

namespace Avalonia.NameGenerator
{
    [Generator]
    public class NameReferenceGenerator : ISourceGenerator
    {
        private const string INamedType = "Avalonia.INamed";
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

            var compilation = (CSharpCompilation)context.Compilation;
            var nameResolver = new XamlXNameResolver(compilation);
            var nameGenerator = new FindControlNameGenerator();
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
                                $"Unable to discover the relevant Avalonia XAML file for {typeSymbol.Name}.",
                                "Unable to discover the relevant Avalonia XAML file " +
                                $"neither at {xamlFileName} nor at {aXamlFileName}",
                                "Usage",
                                DiagnosticSeverity.Warning,
                                true),
                            Location.None));
                    continue;
                }

                try
                {
                    var nameSpace = typeSymbol.ContainingNamespace.ToDisplayString(SymbolDisplayFormat);
                    var xaml = relevantXamlFile.GetText()!.ToString();
                    var names = nameResolver.ResolveNames(xaml);
                    var sourceCode = nameGenerator.GenerateNames(typeSymbol.Name, nameSpace, names);
                    context.AddSource($"{typeSymbol.Name}.g.cs", SourceText.From(sourceCode, Encoding.UTF8));
                }
                catch (Exception exception)
                {
                    context.ReportDiagnostic(
                        Diagnostic.Create(
                            new DiagnosticDescriptor(
                                "AXN0002",
                                $"Unhandled exception occured while generating typed Name references for {typeSymbol.Name}.",
                                $"Unhandled exception occured while generating typed Name references: {exception}",
                                "Usage",
                                DiagnosticSeverity.Warning,
                                true),
                            Location.None));
                }
            }
        }

        private static IReadOnlyList<INamedTypeSymbol> UnpackAnnotatedTypes(
            GeneratorExecutionContext context,
            CSharpCompilation existingCompilation,
            NameReferenceSyntaxReceiver nameReferenceSyntaxReceiver)
        {
            var allowedNameGenerator = context
                .GetMSBuildProperty("AvaloniaNameGenerator", "false")
                .Equals("true", StringComparison.OrdinalIgnoreCase);

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
                var typeSymbol = (INamedTypeSymbol)model.GetDeclaredSymbol(candidateClass);
                if (InheritsFrom(typeSymbol, INamedType) == false)
                {
                    continue;
                }
                if (allowedNameGenerator == false)
                {
                    var relevantAttribute = typeSymbol!
                        .GetAttributes()
                        .FirstOrDefault(attr => attr.AttributeClass!.Equals(attributeSymbol, SymbolEqualityComparer.Default));

                    if (relevantAttribute == null)
                    {
                        continue;
                    }
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
                        $"The type {typeSymbol?.Name} should be declared with the 'partial' keyword " +
                        "as it is either annotated with the [GenerateTypedNameReferences] attribute, " +
                        "or the <AvaloniaNameGenerator> property is set to 'true' in the C# project file (it is set " +
                        "to 'true' by default). In order to skip the processing of irrelevant files, put " +
                        "<AvaloniaNameGenerator>false</AvaloniaNameGenerator> into your .csproj file as " +
                        "<PropertyGroup> descendant and decorate only relevant view classes with the " + 
                        "[GenerateTypedNameReferences] attribute.";

                    context.ReportDiagnostic(
                        Diagnostic.Create(
                            new DiagnosticDescriptor(
                                "AXN0003",
                                missingPartialKeywordMessage,
                                missingPartialKeywordMessage,
                                "Usage",
                                DiagnosticSeverity.Warning,
                                true),
                            Location.None));
                }
            }

            return symbols;
        }

        static bool InheritsFrom(INamedTypeSymbol symbol, string typeName)
        {            
            while (true)
            {
                if (symbol.ToString() == typeName)
                {
                    return true;
                }
                if (symbol.BaseType != null)
                {
                    var intefaces = symbol.AllInterfaces;
                    foreach (var @interface in intefaces)
                    {
                        if (@interface.ToString() == typeName)
                        {
                            return true;
                        }
                    }
                    symbol = symbol.BaseType;
                    continue;
                }
                break;
            }
            return false;
        }
    }
}
