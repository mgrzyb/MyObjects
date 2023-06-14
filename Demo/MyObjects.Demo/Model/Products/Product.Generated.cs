using System.Collections.Generic;

namespace MyObjects.Demo.Model.Products;

public partial class Product
{
    private readonly ISet<ProductCategory> categories = new HashSet<ProductCategory>();
    
    protected Product()
    {
    }
}