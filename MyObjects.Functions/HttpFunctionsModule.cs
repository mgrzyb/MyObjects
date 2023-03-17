using System.Reflection;
using Autofac;
using Autofac.Extensions.DependencyInjection.AzureFunctions;
using Autofac.Extras.AggregateService;
using Microsoft.AspNetCore.Mvc;
using Module = Autofac.Module;

namespace MyObjects.Functions;

public class HttpFunctionsModule : Module
{
    private readonly Assembly functionsAssembly;

    public HttpFunctionsModule(Assembly functionsAssembly)
    {
        this.functionsAssembly = functionsAssembly;
    }

    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterAggregateService<FunctionsBase<IActionResult>.IDependencies>();

        builder
            .RegisterAssemblyTypes(this.functionsAssembly)
            .Where(t => t.Name.EndsWith("Functions"))
            .AsSelf() // Azure Functions core code resolves a function class by itself.
            .InstancePerTriggerRequest(); // This will scope nested dependencies to each function execution

        builder.RegisterType<FunctionPipeline<IActionResult>>().AsImplementedInterfaces();
        builder.RegisterDecorator<ConvertInvalidReferenceToNotFoundDecorator, IFunctionPipeline<IActionResult>>();
    }
}