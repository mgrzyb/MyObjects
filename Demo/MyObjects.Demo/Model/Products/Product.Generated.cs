using System.Collections.Generic;

namespace MyObjects.Demo.Model.Products;

public partial class Product
{
    private readonly ICollection<ProductCategory> categories = new List<ProductCategory>();
    
    protected Product()
    {
    }
}