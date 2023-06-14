using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using MyObjects.Demo.Functions.Model;
using MyObjects.Demo.Model.Products;
using MyObjects.Demo.Model.Products.Commands;
using MyObjects.Functions;
using NHibernate.Linq;

namespace MyObjects.Demo.Functions.Api;

[Route("api")][OpenApi("Product")]
public partial class ProductFunctions : HttpFunctionsBase
{
    private readonly Mapper mapper;
    private readonly ProjectedQuery<Product, ProductProjectionDto> productQuery;
    
    public ProductFunctions(IDependencies dependencies, ProjectedQuery<Product, ProductProjectionDto> productQuery, Mapper mapper) : base(dependencies)
    {
        this.productQuery = productQuery;
        this.mapper = mapper;
    }
    
    /// <summary>
    /// Returns all products
    /// </summary>
    /// <remarks>
    /// Remarks Foo
    /// </remarks>
    /// <param name="skip">Number of products to skip (optional)</param>
    /// <param name="take">Number of products to return (optional, defaults to 1000)</param>
    /// <returns>List of products Bar</returns>
    [HttpGet][Route("products")]
    public async Task<HttpOk<IEnumerable<ProductProjectionDto>>> GetProducts(int? skip, int? take)
    {
        var products = await this.productQuery.Run(skip, take).ToListAsync();
        return new HttpOk<IEnumerable<ProductProjectionDto>>(products);
    }

    [HttpPost][Route("products")]
    public async Task<HttpCreated> CreateProduct(CreateProductDto body)
    {
        var productRef = await this.Mediator.Send(this.mapper.Map<CreateProduct>(body));
        return new HttpCreated($"api/products/{productRef.Id}");
    }
    
    [HttpGet][Route("products/{id}")]
    public async Task<HttpOk<ProductDto>> GetProduct(Reference<Product> id)
    {
        return HttpOk.WithValue(this.mapper.Map<ProductDto>(await this.Session.Resolve(id)));
    }

    [HttpPut][Route("products/{id},{version}")]
    public async Task<OneOf<HttpConflict, HttpOk<ProductDto>>> UpdateProduct(Reference<Product> id, int version, UpdateProductDto body)
    {
        var command = this.mapper.Map(body, new UpdateProduct(id.WithVersion(version)));
        await this.Mediator.Send(command);
        return HttpOk.WithValue(this.mapper.Map<ProductDto>(await this.Session.Resolve(id)));
    }
    
    [HttpPatch][Route("products/{id}")]
    public async Task<OneOf<HttpOk<ProductDto>, HttpConflict>> PatchProduct(Reference<Product> id, UpdateProductDto body)
    {
        var product = await this.Session.Resolve(id);
        
        var command = this.mapper.Map(body, new UpdateProduct(product.GetVersionedReference()));
        await this.Mediator.Send(command);
        
        this.Session.Clear();
        
        return HttpOk.WithValue(this.mapper.Map<ProductDto>(await this.Session.Resolve(id)));
    }
}