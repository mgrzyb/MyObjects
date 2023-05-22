namespace MyObjects.Demo.Model.Products;

using System.Collections.Generic;

public partial class ProductCategory
{
    private readonly ICollection<ProductCategory> children = new List<ProductCategory>();
    
    protected ProductCategory()
    {
    }
}