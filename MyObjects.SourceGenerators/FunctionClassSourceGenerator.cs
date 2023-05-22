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
            Debugger.Launch(); */
        
        context.RegisterForSyntaxNotifications(() => new PartialFunctionClassReceiver());
    }

    public void Execute(GeneratorExecutionContext context)
    {
        if (context.SyntaxReceiver is not PartialFunctionClassReceiver receiver)
            return;

        try
        {
            foreach (var declarationSyntax in receiver.ClassDeclarationSyntax)
            {
                Generate(context, declarationSyntax);
            }
        }
        catch (Exception e)
        {
            throw new Exception(e.ToString());
        }
    }

    private void Generate(GeneratorExecutionContext context, ClassDeclarationSyntax declarationSyntax)
    {
        var userClassModel = context.Compilation.GetSemanticModel(declarationSyntax.SyntaxTree);
        var userClassSymbol = ModelExtensions.GetDeclaredSymbol(userClassModel, declarationSyntax) as ITypeSymbol;

        var routePrefix = userClassSymbol.GetAttributes().Where(a => a.AttributeClass?.Name == "RouteAttribute").Select(a => a.ConstructorArguments[0].Value.ToString())
            .FirstOrDefault();
        
        var endpoints = userClassSymbol.GetMembers().OfType<IMethodSymbol>()
            .Select(m => new
            {
                Handler = m,
                RouteAttribute = m.GetAttributes().FirstOrDefault(a => a.AttributeClass?.Name == "RouteAttribute"),
                MethodAttributess = m.GetAttributes().Where(a => a.AttributeClass?.BaseType?.Name == "HttpMethodAttribute")
            })
            .Where(e => e.RouteAttribute is not null);

        var functions = endpoints.Select(e =>
        {
            var r = e.RouteAttribute.ConstructorArguments[0].Value.ToString();
            var route = string.IsNullOrEmpty(routePrefix) ? r : $"{routePrefix}/{r}";
            
            var methods = string.Join(", ", e.MethodAttributess.Select(m => $@"""{Regex.Match(m.AttributeClass.Name, "(?:Http)(.+)(?:Attribute)").Groups[1].Value.ToLower()}"""));

            var handlerParameters = e.Handler.Parameters
                .Where(p => p.Name != "body" && Regex.IsMatch(route, $"{{{p.Name}(:\\w+)?}}"))
                .Select(p => $"{GetFunctionParameterType(p.Type)} {p.Name}");
            
            var functionParameters = string.Join(", ", new[] {"HttpRequest req"}.Concat(handlerParameters));
            
            return $@"
        [FunctionName(""{e.Handler.Name}"")]    // Generated
        public async Task<IActionResult> {e.Handler.Name}_(
            [HttpTrigger(AuthorizationLevel.Function, {methods}, Route = ""{route}"")]
            {functionParameters})
        {{
            {(e.Handler.Parameters.SingleOrDefault(p => p.Name == "body") is IParameterSymbol body
                ? $"var body = JsonConvert.DeserializeObject<{body.Type.GetFullName()}>(await new StreamReader(req.Body).ReadToEndAsync(), JsonSerializerSettings);" : "")}

            return await this.Pipeline.Run(() => this.{e.Handler.Name}(
                {string.Join(", ", e.Handler.Parameters.Select(p => GetParameterValue(route, p)))}
            ));
        }}

";
        });

        var text = $@"
using System;
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
        context.AddSource(declarationSyntax.Identifier + ".Generated.cs", sourceText);
    }

    private string GetParameterValue(string route, IParameterSymbol p)
    {
        if (Regex.IsMatch(route, $"{{{p.Name}(:\\w+)?}}") || p.Name == "body" || p.Name == "req")
        {
            if (p.Type.Name.StartsWith("Reference"))
                return $"new Reference<{((INamedTypeSymbol) p.Type).TypeArguments[0].GetFullName()}>({p.Name})";

            return p.Name;
        }
        else
        {
            if (p.Type.Name.StartsWith("Reference"))
                return $"new Reference<{((INamedTypeSymbol) p.Type).TypeArguments[0].GetFullName()}>(req.Query[\"{p.Name}\"])";

            return $"req.Query[\"{p.Name}\"]";
        }
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
    public IList<ClassDeclarationSyntax> ClassDeclarationSyntax = new List<ClassDeclarationSyntax>();
    
    public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
    {
        if (syntaxNode is ClassDeclarationSyntax classDeclarationSyntax)
        {
            if (classDeclarationSyntax.Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword)))
            {
                if (classDeclarationSyntax.BaseList?.Types.Any(t => t.ToString().StartsWith("FunctionsBase<")) == true)
                {
                    if (classDeclarationSyntax.Members.Any(m => m.IsKind(SyntaxKind.MethodDeclaration)
                                                                && m.AttributeLists.SelectMany(l => l.Attributes)
                                                                    .Any(a => a.Name.ToString() == "Route")))
                    {
                        this.ClassDeclarationSyntax.Add(classDeclarationSyntax);
                    }
                }
            }
        }
    }
}
