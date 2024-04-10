using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using MyObjects.Demo.Functions.Model;
using MyObjects.Demo.Model.Products;
using MyObjects.Functions;
using NHibernate.Linq;


namespace MyObjects.Demo.Functions.Api;

[Route("api")]
public partial class ProductCategoryFunctions : HttpFunctionsBase
{
    private readonly Mapper mapper;
    private readonly ProjectedQuery<ProductCategory, ProductCategoryDto> categoriesQuery;

    public ProductCategoryFunctions(IDependencies dependencies, ProjectedQuery<ProductCategory, ProductCategoryDto> categoriesQuery, Mapper mapper) : base(dependencies)
    {
        this.mapper = mapper;
        this.categoriesQuery = categoriesQuery;
    }
    
    [HttpGet][Route("product-categories")]
    public async Task<IActionResult> GetProductCategories()
    {
        var c = await this.Session.Query<ProductCategory>().Where(c => c.Parent == null).ToListAsync();
        return new OkObjectResult(mapper.Map<IEnumerable<ProductCategoryDto>>(c));
    }
    
}