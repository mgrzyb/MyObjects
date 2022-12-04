using System.Collections.Generic;
using System.Linq;
using MyObjects.Demo.Model.Products;

namespace MyObjects.Demo.Model.Orders
{
    public class SalesOrder : AggregateRoot
    {
        public virtual string Number { get; protected set; }
        public virtual OrderStatus Status { get; protected set; } = OrderStatus.New;
        
        private readonly ICollection<SalesOrderLine> lines = new List<SalesOrderLine>();
        public virtual IEnumerable<SalesOrderLine> Lines => this.lines;

        public virtual decimal Total
        {
            get => this.Lines.Sum(l => l.Value);
            protected set { }
        }

        protected SalesOrder()
        {
        }

        public SalesOrder(string number)
        {
            Number = number;
        }

        protected internal virtual SalesOrderLine AddLine(Product product, decimal price, decimal quantity = 1)
        {
            var line = new SalesOrderLine(this, product)
            {
                Price = price,
                Quantity = quantity
            };
            
            this.lines.Add(line);
            
            return line;
        }

        protected internal virtual void Cancel()
        {
            this.Status = OrderStatus.Canceled;
            this.Publish(new SalesOrderCanceled(this));
        }
    }
}