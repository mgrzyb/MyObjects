using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace MyObjects.SourceGenerators;

[Generator]
public class FunctionClassSourceGenerator : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext context)
    {
        
        /*if (!Debugger.IsAttached)
            Debugger.Launch();*/
        
        context.RegisterForSyntaxNotifications(() => new PartialFunctionClassReceiver());
    }

    public void Execute(GeneratorExecutionContext context)
    {
        if (context.SyntaxReceiver is not PartialFunctionClassReceiver receiver)
            return;
        if (receiver.ClassDeclarationSyntax is null)
            return;
        
        var userClassModel = context.Compilation.GetSemanticModel(receiver.ClassDeclarationSyntax.SyntaxTree);
        var userClassSymbol = ModelExtensions.GetDeclaredSymbol(userClassModel, receiver.ClassDeclarationSyntax) as ITypeSymbol;

        var endpoints = userClassSymbol.GetMembers().OfType<IMethodSymbol>()
            .Select(m => new
            {
                Handler = m,
                Route = m.GetAttributes().FirstOrDefault(a => a.AttributeClass.Name == "RouteAttribute"),
                Methods = m.GetAttributes().Where(a => a.AttributeClass.BaseType.Name == "HttpMethodAttribute")
            })
            .Where(e => e.Route is not null);
        
        var functions = endpoints.Select(e => $@"
    [FunctionName(""{e.Handler.Name}"")]    // Generated
        public async Task<IActionResult> {e.Handler.Name}_(
            [HttpTrigger(AuthorizationLevel.Function, {string.Join(", ", e.Methods.Select(m => $@"""{Regex.Match(m.AttributeClass.Name, "(?:Http)(.+)(?:Attribute)").Groups[1].Value.ToLower()}"""))}, Route = ""{e.Route.ConstructorArguments[0].Value}"")]
            {string.Join(", ", new [] { "HttpRequest req" }
                .Concat(e.Handler.Parameters.Where(p=>p.Name != "body").Select(p => $"{GetFunctionParameterType(p.Type)} {p.Name}")))})
        {{
            {(e.Handler.Parameters.SingleOrDefault(p=>p.Name == "body") is IParameterSymbol body 
                ? $"var body = JsonConvert.DeserializeObject<{body.Type.GetFullName()}>(await new StreamReader(req.Body).ReadToEndAsync());" : "")}
            return await this.HttpPipeline.Run(() => this.{e.Handler.Name}(
                {string.Join(", ", e.Handler.Parameters.Select(GetParameterValue))}
            ));
        }}

");
        
        var text = $@"
using System.IO;
using Newtonsoft.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
namespace {string.Join(".", userClassSymbol.ContainingNamespace.ConstituentNamespaces)};
public partial class {userClassSymbol.GetName()}
{{
    {string.Join("\n\t", functions)}
}}";
        var sourceText = SourceText.From(text, Encoding.UTF8);
        context.AddSource(receiver.ClassDeclarationSyntax.Identifier + ".Generated.cs", sourceText);
    }

    private string GetParameterValue(IParameterSymbol p)
    {
        if (p.Type.Name.StartsWith("Reference"))
            return $"new Reference<{((INamedTypeSymbol)p.Type).TypeArguments[0].GetFullName()}>({p.Name})";
        return p.Name;
    }

    private string GetFunctionParameterType(ITypeSymbol type)
    {
        if (type.Name.StartsWith("Reference"))
            return "int";
        return type.Name;
    }
}

public class PartialFunctionClassReceiver : ISyntaxReceiver
{
    public ClassDeclarationSyntax? ClassDeclarationSyntax { get; private set; }
    
    public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
    {
        if (syntaxNode is ClassDeclarationSyntax classDeclarationSyntax)
        {
            if (classDeclarationSyntax.Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword)))
            {
                if (classDeclarationSyntax.BaseList.Types.Any(t => t.ToString() == "FunctionsBase"))
                {
                    if (classDeclarationSyntax.Members.Any(m => m.IsKind(SyntaxKind.MethodDeclaration)
                                                                && m.AttributeLists.SelectMany(l => l.Attributes)
                                                                    .Any(a => a.Name.ToString() == "Route")))
                    {
                        this.ClassDeclarationSyntax = classDeclarationSyntax;
                    }
                }
            }
        }
    }
}
