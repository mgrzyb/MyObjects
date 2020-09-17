using MyObjects.Demo.Model.Products;

namespace MyObjects.Demo.Model.Orders
{
    public class SalesOrderLine : Entity
    {
        public virtual SalesOrder Order { get; protected set; }
        public virtual Product Product { get; protected set; }
        
        public virtual decimal Quantity { get; set; }
        public virtual decimal Price { get; set; }
        
        public virtual decimal Value
        {
            get => this.Price * this.Quantity;
            protected set { }
        }

        protected SalesOrderLine()
        {
        }

        public SalesOrderLine(SalesOrder order, Product product)
        {
            this.Order = order;
            this.Product = product;
        }
    }
}