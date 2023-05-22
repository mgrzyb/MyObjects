using System;
using System.Threading.Tasks;
using HotChocolate.AzureFunctions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyObjects.Functions;

namespace MyObjects.Demo.Functions.Api;

public partial class GraphQLFunctions : FunctionsBase<IActionResult>
{
    private readonly IServiceProvider serviceProvider;
    private readonly IGraphQLRequestExecutor executor;

    public GraphQLFunctions(IDependencies dependencies, IServiceProvider serviceProvider, IGraphQLRequestExecutor executor) : base(dependencies)
    {
        this.executor = executor;
        this.serviceProvider = serviceProvider;
    }

    [HttpGet][HttpPost][Route("graphql/{*rest}")]
    public Task<IActionResult> GraphQL(HttpRequest req)
    {
        req.HttpContext.RequestServices = this.serviceProvider;
        return this.executor.ExecuteAsync(req);
    }
}