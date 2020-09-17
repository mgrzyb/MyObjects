using System.Collections.Generic;
using System.Linq;
using MyObjects.Demo.Model.Products;

namespace MyObjects.Demo.Model.Orders
{
    public class SalesOrder : AggregateRoot
    {
        private readonly ICollection<SalesOrderLine> lines = new List<SalesOrderLine>();
        public virtual IEnumerable<SalesOrderLine> Lines => this.lines;

        public virtual decimal Total => this.Lines.Sum(l => l.Value);

        public virtual SalesOrderLine AddLine(Product product, decimal price, decimal quantity = 1)
        {
            var line = new SalesOrderLine(this, product)
            {
                Price = price,
                Quantity = quantity
            };
            
            this.lines.Add(line);
            
            return line;
        }
    }
}