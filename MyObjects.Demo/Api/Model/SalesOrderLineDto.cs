using MyObjects.Demo.Model.Products;

namespace MyObjects.Demo.Api.Model
{
    public class SalesOrderLineDto
    {
        public Reference<Product> ProductId { get; set; }
        public decimal Price { get; set; }
        public decimal Quantity { get; set; }
    }
}