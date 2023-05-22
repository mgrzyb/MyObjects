using System;
using System.Collections.Generic;
using System.Linq;
using MyObjects.Demo.Model.Products;

namespace MyObjects.Demo.Model.Orders
{
    public partial class SalesOrder : AggregateRoot
    {
        public virtual string Number { get; }
        public virtual OrderStatus Status { get; protected set; } = OrderStatus.New;
        
        public virtual IEnumerable<SalesOrderLine> Lines => this.lines;

        public virtual decimal Total => this.Lines.Sum(l => l.Value);

        public SalesOrder(string number)
        {
            this.Number = number;
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