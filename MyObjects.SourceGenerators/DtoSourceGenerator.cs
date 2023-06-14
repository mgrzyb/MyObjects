using System.Diagnostics;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace MyObjects.SourceGenerators;

[Generator]
public class DtoSourceGenerator : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext context)
    {
        /*if (!Debugger.IsAttached)
            Debugger.Launch();*/

        context.RegisterForSyntaxNotifications(() => new DtoClassReceiver());
    }

    public void Execute(GeneratorExecutionContext context)
    {
        var syntaxReceiver = context.SyntaxReceiver as DtoClassReceiver;

        foreach (var syntax in syntaxReceiver.DtoClassSyntax)
        {
            this.Generate(context, syntax);
        }
    }
    private void Generate(GeneratorExecutionContext context, ClassDeclarationSyntax dtoClassSyntax)
    {
        var dtoClassModel = context.Compilation.GetSemanticModel(dtoClassSyntax.SyntaxTree);
        var dtoClassSymbol = ModelExtensions.GetDeclaredSymbol(dtoClassModel, dtoClassSyntax) as ITypeSymbol;

        var entityTypes = dtoClassSymbol.AllInterfaces
            .Where(i => i.Name.StartsWith("IDtoFor"))
            .SelectMany(i => i.TypeArguments);
        
        //context.ReportDiagnostic(Diagnostic.Create("MG001", "DtoSourceGenerator", String.Join(", ", entityTypes.Select(t => t.Name)), DiagnosticSeverity.Info, DiagnosticSeverity.Info, true, 1));

        var implementedMembers = dtoClassSymbol.GetMembers();
        
        var implementation = entityTypes.SelectMany(t => 
            t.GetMembers()
            .Where(m => m.Kind == SymbolKind.Property).OfType<IPropertySymbol>()
            .Where(p => p.Type.IsValueType || p.Type.Name.Equals("string", StringComparison.OrdinalIgnoreCase))
            .Where(p => implementedMembers.Any(m => m.Name == p.Name) == false)
            .Select(p => $"public {p.Type} {p.Name} {{ get; set; }}"));
        // add the generated implementation to the compilation
        SourceText sourceText = SourceText.From($@"
namespace {string.Join(".", dtoClassSymbol.ContainingNamespace.ConstituentNamespaces)};
public partial class {dtoClassSymbol.GetName()}
{{
    {String.Join("\n", implementation)}
}}", Encoding.UTF8);
        context.AddSource(dtoClassSyntax.Identifier + ".Generated.cs", sourceText);
    }

    public class DtoClassReceiver : ISyntaxReceiver
    {
        public List<ClassDeclarationSyntax> DtoClassSyntax { get; private set; } = new List<ClassDeclarationSyntax>();

        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            if (syntaxNode is ClassDeclarationSyntax classDeclarationSyntax)
            {
                if (classDeclarationSyntax.Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword)))
                {
                    if (classDeclarationSyntax.BaseList?.Types.Any(t => t.Type is GenericNameSyntax gs && gs.Identifier.Text == "IDtoFor") == true)
                    {
                        this.DtoClassSyntax.Add(classDeclarationSyntax);
                    }
                }
            }

        }
    }
}