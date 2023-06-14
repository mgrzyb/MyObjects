using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace MyObjects.SourceGenerators;

internal class HttpFunctionModel
{
    public string Name;
    public string Summary;
    public string Description;
    public string ReturnValueDescription;
    
    public string Route;
    public IEnumerable<string> Methods;
    public IEnumerable<HttpFunctionParameter> Parameters;
    public ITypeSymbol TargetReturnType;
    public IEnumerable<HttpFunctionReturnValue> ReturnValues;
}

internal class HttpFunctionReturnValue
{
    public int Code;
    public ITypeSymbol Type;
}

internal class HttpFunctionParameter
{
    public string Name;
    public ITypeSymbol TargetType;
    public FunctionParameterBindingKind BindingKind;
    public string Type;
    public bool Required;
    public string Description;
}

internal enum FunctionParameterBindingKind
{
    Url,
    Body,
    Query,
    PassThrough
}

[Generator]
public class HttpFunctionClassSourceGenerator : ISourceGenerator
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

        try
        {
            foreach (var declarationSyntax in receiver.ClassDeclarationSyntax)
            {
                var classModel = context.Compilation.GetSemanticModel(declarationSyntax.SyntaxTree);
                var classSymbol = classModel.GetDeclaredSymbol(declarationSyntax) as ITypeSymbol;

                var functions = this.DiscoverHttpFunctions(context, classSymbol);

                context.AddSource(
                    declarationSyntax.Identifier + ".Generated.cs", 
                    this.GenerateHttpFunctions(context, classSymbol, functions));
            }
        }
        catch (Exception e)
        {
            throw new Exception(e.ToString());
        }
    }


    private IEnumerable<HttpFunctionModel> DiscoverHttpFunctions(GeneratorExecutionContext context, ITypeSymbol classSymbol)
    {
        var routePrefix = classSymbol.GetAttributes()
            .Where(a => a.AttributeClass?.Name == "RouteAttribute")
            .Select(a => a.ConstructorArguments[0].Value.ToString())
            .FirstOrDefault();

        var endpoints = classSymbol.GetMembers().OfType<IMethodSymbol>()
            .Select(m => new
            {
                Handler = m,
                RouteAttribute = m.GetAttributes().FirstOrDefault(a => a.AttributeClass?.Name == "RouteAttribute"),
                MethodAttributess = m.GetAttributes().Where(a => a.AttributeClass?.BaseType?.Name == "HttpMethodAttribute")
            })
            .Where(e => e.RouteAttribute is not null);

        return endpoints
            .Select(e =>
            {
                var route = e.RouteAttribute.ConstructorArguments[0].Value.ToString();

                var summary = "";
                var remarks = "";
                var parameterSummary = new Dictionary<string, string>();
                var returnValueSummary = "";
                
                var doc = ExtractMethodDocumentation(e.Handler);

                if (doc != null)
                {
                    summary = doc.Root.Element("summary")?.Value ?? "";
                    remarks = doc.Root.Element("remarks")?.Value ?? "";
                    foreach (var p in doc.Root.Elements("param"))
                    {
                        var name = p.Attribute("name")?.Value;
                        if (name != null) 
                            parameterSummary.Add(name, p.Value);
                    }

                    returnValueSummary = doc.Root.Element("returns")?.Value ?? "";
                }

                var returnTypes = UnwrapReturnType((INamedTypeSymbol)e.Handler.ReturnType);

                var rv = GetHttpFunctionReturnValues(returnTypes);
                
                return new HttpFunctionModel
                {
                    Name = e.Handler.Name,
                    Summary = summary,
                    Description = remarks,
                    ReturnValueDescription = returnValueSummary,
                    Route = string.IsNullOrEmpty(routePrefix) ? route : $"{routePrefix}/{route}",
                    Methods = e.MethodAttributess.Select(a => Regex.Match(a.AttributeClass.Name, "(?:Http)(.+)(?:Attribute)").Groups[1].Value.ToLower()),
                    Parameters = e.Handler.Parameters.Select(p => new HttpFunctionParameter
                    {
                        Name = p.Name,
                        BindingKind = GetParameterBindingKind(p, route),
                        Type = GetParameterType(p),
                        TargetType = p.Type,
                        Required = p.Type.Name != "Nullable",
                        Description = parameterSummary.GetValueOrDefault(p.Name) ?? ""
                    }),
                    ReturnValues = rv,
                    TargetReturnType = e.Handler.ReturnType
                };

                IEnumerable<ITypeSymbol> UnwrapReturnType(INamedTypeSymbol returnType)
                {
                    switch (returnType.Name)
                    {
                        case "Task":
                            return UnwrapReturnType((INamedTypeSymbol) returnType.TypeArguments[0]);
                        case "OneOf":
                            return returnType.TypeArguments;
                        default:
                            return new[] {returnType};     
                    }
                }
            });

        FunctionParameterBindingKind GetParameterBindingKind(IParameterSymbol p, string route)
        {
            if (p.Name == "body" || p.GetAttributes().Any(a => a.AttributeClass.Name == "FromBodyAttribute")) 
                return FunctionParameterBindingKind.Body;
            if (p.Type.Name == "HttpRequest")
                return FunctionParameterBindingKind.PassThrough;
            if (route.Contains($"{{{p.Name}}}"))
                return FunctionParameterBindingKind.Url;
            return FunctionParameterBindingKind.Query;
        }
        
        string GetParameterType(IParameterSymbol p)
        {
            if (p.Type.Name == "Reference")
                return "int";
                
            return p.Type.ToString();
        }
        
    }
    private SourceText GenerateHttpFunctions(GeneratorExecutionContext context, ITypeSymbol classSymbol, IEnumerable<HttpFunctionModel> functions)
    {
        var openApiAttribute = classSymbol.GetAttributes().FirstOrDefault(a => a.AttributeClass.Name == "OpenApiAttribute");
        var generateOpenApi = openApiAttribute != null;

        var usingStatements = new List<string>();
        usingStatements.AddRange(new []
        {
            "using System;",
            "using System.IO;",
            "using Newtonsoft.Json;",
            "using System.Threading.Tasks;",
            "using Microsoft.AspNetCore.Http;",
            "using Microsoft.AspNetCore.Mvc;",
            "using Microsoft.Azure.WebJobs;",
            "using Microsoft.Azure.WebJobs.Extensions.Http;"
        });

        if (generateOpenApi)
        {
            usingStatements.AddRange(new []
            {
                "using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;",
                "using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;",
                "using Microsoft.OpenApi.Models;",
                "using System.Net;"
            });
        }

        var classNamespace = string.Join(".", classSymbol.ContainingNamespace.ConstituentNamespaces);

        var classDeclaration = $@"public partial class {classSymbol.GetName()}
        {{
            {string.Join("\n\t", functions.Select(f => {
                var parameters = f.Parameters
                    .Where(p => p.BindingKind == FunctionParameterBindingKind.Url)
                    .Select(p => $"{p.Type} {p.Name}");

                var functionStatements = new List<string>();
                var bodyParameter = f.Parameters.FirstOrDefault(p => p.BindingKind == FunctionParameterBindingKind.Body);
                if (bodyParameter != null)
                    functionStatements.Add($"var {bodyParameter.Name} = JsonConvert.DeserializeObject<{bodyParameter.Type}>(await new StreamReader(req.Body).ReadToEndAsync(), JsonSerializerSettings);");
                
                functionStatements.Add($@"
                    return await this.Run(async () => {{
                        var result = await this.{f.Name}(
                            {string.Join(", ", f.Parameters.Select(GetConvertedParameterValue))}
                        );
                        return this.CreateActionResult(result);
                    }});
                ");

                var functionAttributes = new List<string>();
                if (generateOpenApi) {
                    
                    var openApiTag = openApiAttribute.ConstructorArguments[0].Value;

                    functionAttributes.AddRange(new[]
                    {
                        $"[OpenApiOperation(operationId: \"{f.Name}\", tags: new[] {{ \"{openApiTag}\" }}, Summary = \"{f.Summary}\", Description = \"{f.Description}\", Visibility = OpenApiVisibilityType.Important)]",
                        $"[OpenApiSecurity(\"function_key\", SecuritySchemeType.ApiKey, Name = \"code\", In = OpenApiSecurityLocationType.Query)]"
                    });
                    
                    foreach (var p in f.Parameters)
                    {
                        if (p.BindingKind == FunctionParameterBindingKind.Body) 
                        {
                            functionAttributes.Add($"[OpenApiRequestBody(\"application/json\", typeof({p.TargetType}))]");
                        } else
                        {
                            var parameterLocation = p.BindingKind == FunctionParameterBindingKind.Query ?
                                "ParameterLocation.Query" : 
                                "ParameterLocation.Path";

                            functionAttributes.Add($"[OpenApiParameter(name: \"{p.Name}\", In = {parameterLocation}, Required = {(p.Required).ToString().ToLower()}, Type = typeof({p.Type}), Summary = \"{p.Name}\", Description = \"{p.Description}\", Visibility = OpenApiVisibilityType.Important)]");
                        }
                    }

                    var mainReturnValue = f.ReturnValues.FirstOrDefault();
                    if (mainReturnValue != null) {
                        if (mainReturnValue.Type == null)
                            functionAttributes.Add($"[OpenApiResponseWithoutBody((HttpStatusCode){mainReturnValue.Code}, Summary = \"{f.ReturnValueDescription}\", Description = \"{f.ReturnValueDescription}\")]");
                        else
                            functionAttributes.Add($"[OpenApiResponseWithBody(statusCode: (HttpStatusCode){mainReturnValue.Code}, contentType: \"application/json\", bodyType: typeof({mainReturnValue.Type}), Summary = \"{f.ReturnValueDescription}\", Description = \"{f.ReturnValueDescription}\")]");

                        foreach (var returnValue in f.ReturnValues.Skip(1))
                        {
                            if (returnValue.Type == null)
                                functionAttributes.Add($"[OpenApiResponseWithoutBody((HttpStatusCode){returnValue.Code})]");
                            else
                                functionAttributes.Add($"[OpenApiResponseWithBody(statusCode: (HttpStatusCode){returnValue.Code}, contentType: \"application/json\", bodyType: typeof({returnValue.Type}))]");
                        } 
                    }
                }
                functionAttributes.Add($"[FunctionName(\"{f.Name}\")]");

                return $@"
                    {string.Join("\n\t", functionAttributes)}
                    public async Task<IActionResult> {f.Name}_(
                        [HttpTrigger(AuthorizationLevel.Function, {string.Join(", ", f.Methods.Select(m => $"\"{m}\""))}, Route = ""{f.Route}"")]
                        {string.Join(", ", new [] { "HttpRequest req"}.Concat(parameters))})
                    {{
                        {string.Join("\n\t", functionStatements)}
                    }}
                ";
            }))}
        }}";
        
        return SourceText.From(@$"
    {string.Join("\n\t", usingStatements)}
    namespace {classNamespace};
    {classDeclaration}
", Encoding.UTF8);

        string GetConvertedParameterValue(HttpFunctionParameter p)
        {
            var value = p.BindingKind == FunctionParameterBindingKind.Query ? 
                $"req.Query[\"{p.Name}\"]" : 
                p.Name;

            var targetType = (INamedTypeSymbol) p.TargetType;

            if (p.BindingKind == FunctionParameterBindingKind.Query && p.TargetType.Name.ToLower() != "string")
            {
                switch (targetType.Name)
                {
                    case "Nullable":
                        switch (targetType.TypeArguments[0].Name)
                        {
                            case "Int32":
                                return $"string.IsNullOrEmpty({value}.ToString()) ? null : int.Parse({value})";
                            case "Reference":
                                return $"string.IsNullOrEmpty({value}.ToString()) ? null : {GetReference(targetType, $"int.Parse({value})")}";
                            default:
                                return value;
                        }
                    case "int32":
                        return $"string.IsNullOrEmpty({value}.ToString()) ? throw new ArgumentException() : int.Parse({value})";
                    case "Reference":
                        return $"string.IsNullOrEmpty({value}.ToString()) ? throw new ArgumentException() : {GetReference(targetType, $"int.Parse({value})")}";
                }
            }

            if (p.TargetType.Name.StartsWith("Reference"))
            {
                return GetReference(targetType, value);
            }

            return value;
        }
    }
    private static IEnumerable<HttpFunctionReturnValue> GetHttpFunctionReturnValues(IEnumerable<ITypeSymbol> returnTypes)
    {

        return returnTypes.Select(t =>
        {
            switch (t)
            {
                case INamedTypeSymbol {Name: "HttpOk"} httpOk:
                    return new HttpFunctionReturnValue
                    {
                        Code = 200,
                        Type = httpOk.TypeArguments[0]
                    };

                case INamedTypeSymbol {Name: "HttpCreated"} httpCreated:
                    return new HttpFunctionReturnValue
                    {
                        Code = 201
                    };
                case INamedTypeSymbol {Name: "HttpConflict"} httpConflict:
                    return new HttpFunctionReturnValue
                    {
                        Code = 409,
                    };
                default:
                    return new HttpFunctionReturnValue
                    {
                        Code = 200,
                        Type = t
                    };
            }
        });
    }
    private static XDocument? ExtractMethodDocumentation(IMethodSymbol method)
    {
        var syntax = method.DeclaringSyntaxReferences.FirstOrDefault().GetSyntax();
        if (!syntax.HasLeadingTrivia)
            return null;
        var documentationCommentLines = syntax.GetLeadingTrivia().ToString().Split(new[] {'\n', '\r'}, StringSplitOptions.RemoveEmptyEntries);
        var documentation = new StringBuilder();
        foreach (var triviaSyntax in documentationCommentLines)
        {
            documentation.Append(triviaSyntax.TrimStart(new[] {' ', '\t', '/'}));
        }

        try
        {
            return XDocument.Parse("<doc>" + documentation + "</doc>");
        }
        catch (Exception)
        {
            return null;
        }
    }

    private static string GetReference(INamedTypeSymbol targetType, string value)
    {
        return $"new Reference<{targetType.TypeArguments[0].GetFullName()}>({value})";
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
                if (classDeclarationSyntax.BaseList?.Types.Any(t => t.ToString() == "HttpFunctionsBase") == true)
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
