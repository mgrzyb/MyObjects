using System.Collections.Immutable;
using System.Diagnostics;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace MyObjects.SourceGenerators;

[Generator]
public class DecoratorSourceGenerator : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext context)
    {
        /*if (!Debugger.IsAttached)
            Debugger.Launch();*/
        
        context.RegisterForSyntaxNotifications(() => new PartialDecoratorReceiver());
    }

    public void Execute(GeneratorExecutionContext context)
    {
        var syntaxReceiver = context.SyntaxReceiver as PartialDecoratorReceiver;

        var userClass = syntaxReceiver?.ClassDeclarationSyntax;
        if (userClass is null)
            return;

        var userClassModel = context.Compilation.GetSemanticModel(userClass.SyntaxTree);
        var userClassSymbol = ModelExtensions.GetDeclaredSymbol(userClassModel, userClass) as ITypeSymbol;

        var fields = userClassSymbol.GetMembers().OfType<IFieldSymbol>();
        var fieldTypes = fields.Select(f => f.Type);

        var decoratedField = fields.FirstOrDefault(f => userClassSymbol.Interfaces.Contains(f.Type, SymbolEqualityComparer.Default));
        if (decoratedField is null)
            return;

        var implementedMethodNames = userClassSymbol
            .GetMembers()
            .OfType<IMethodSymbol>()
            .Select(m => m.Name);

        var methodsToImplement = decoratedField.Type
            .GetInheritedMembers()
            .OfType<IMethodSymbol>()
            .Where(m => implementedMethodNames.Contains(m.Name) == false);

        var implementation = methodsToImplement
            .Select(m =>
                @$"public {(m.ReturnsVoid ? "void" : m.ReturnType.GetFullName())} {GetMethodName(m)}({string.Join(", ", m.Parameters.Select(p => $"{p.Type.GetFullName()} {p.Name}"))}) {GetMethodConstraints(m)} 
{{ 
    {(m.ReturnsVoid ? "" : "return ")}this.{decoratedField.Name}.{GetMethodName(m)}({string.Join(", ", m.Parameters.Select(p => p.Name))}); 
}}");

        // add the generated implementation to the compilation
        SourceText sourceText = SourceText.From($@"
{GetUsings(methodsToImplement)}
namespace {string.Join(".", userClassSymbol.ContainingNamespace.ConstituentNamespaces)};
public partial class {userClassSymbol.GetName()}
{{
    {string.Join("\n", implementation)}
}}", Encoding.UTF8);
        context.AddSource(userClass.Identifier + ".Generated.cs", sourceText);
    }

    private string GetUsings(IEnumerable<IMethodSymbol> methods)
    {
        return string.Join("\n", methods
            .SelectMany(m => new[] {m.ReturnType}.Concat(m.Parameters.Select(p => p.Type)))
            .Select(t => t.ContainingNamespace)
            .Distinct(SymbolEqualityComparer.Default)
            .OfType<INamespaceSymbol>()
            .Select(n => $"using {string.Join(".", n.ConstituentNamespaces)};"));
    }

    private static string GetMethodConstraints(IMethodSymbol m)
    {
        if (m.IsGenericMethod == false)
            return string.Empty;

        return string.Join(" ", m.TypeParameters
            .Where(t => t.ConstraintTypes.Any())
            .Select(tp => $" where {tp.Name} : {string.Join(", ", tp.ConstraintTypes.Select(t => t.GetFullName()))}"));
    }

    private static string GetMethodName(IMethodSymbol m)
    {
        if (m.IsGenericMethod)
            return $"{m.Name}<{string.Join(", ", m.TypeParameters.Select(p => p.GetFullName()))}>";
        return m.Name;
    }
}

public static class ITypeSymbolExtensions
{
    public static string GetFullName(this ITypeSymbol type)
    {
        if (type is ITypeParameterSymbol)
            return type.Name;

        return $"{type.ContainingNamespace}.{type.GetName()}";
    }

    public static string GetName(this ITypeSymbol type)
    {
        if (type is ITypeParameterSymbol)
            return type.Name;
        
        if (type is INamedTypeSymbol namedType)
        {
            if (namedType.IsGenericType)
                return $"{type.Name}<{string.Join(", ", namedType.TypeArguments.Select(GetFullName))}>";
        }

        return type.Name;
    }
    
    private static IEnumerable<ITypeSymbol> GetBaseTypeAndInterfaces(ITypeSymbol type)
    {
        yield return type;

        if (type.BaseType is not null)
        {
            foreach (var t in GetBaseTypeAndInterfaces(type.BaseType))
            {
                yield return t;
            }
        }

        foreach (var i in type.Interfaces)
        {
            foreach (var t in GetBaseTypeAndInterfaces(i))
            {
                yield return t;
            }
        }
    }

    public static IEnumerable<ISymbol> GetInheritedMembers(this ITypeSymbol type)
    {
        var types = GetBaseTypeAndInterfaces(type).Distinct(SymbolEqualityComparer.Default).OfType<ITypeSymbol>();
        foreach (var t in types)
        {
            foreach (var member in t.GetMembers())
            {
                yield return member;
            }
        }
    }
}

public class PartialDecoratorReceiver : ISyntaxReceiver
{
    public ClassDeclarationSyntax? ClassDeclarationSyntax { get; private set; }
    
    public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
    {
        if (syntaxNode is ClassDeclarationSyntax classDeclarationSyntax)
        {
            if (classDeclarationSyntax.Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword)))
            {
                if (classDeclarationSyntax.Identifier.Text.EndsWith("Decorator"))
                {
                    this.ClassDeclarationSyntax = classDeclarationSyntax;
                }
            }
        }
    }
}
