using MyObjects.Demo.Model.Products;

namespace MyObjects.Demo.Model.Orders
{
    public partial class SalesOrderLine : Entity
    {
        public virtual SalesOrder Order { get; }
        public virtual Product Product { get; }

        public virtual decimal Quantity { get; protected internal set; }
        public virtual decimal Price { get; protected internal set; }

        public virtual decimal Value => this.Price * this.Quantity;
        
        public SalesOrderLine(SalesOrder order, Product product)
        {
            this.Order = order;
            this.Product = product;
        }
    }
}