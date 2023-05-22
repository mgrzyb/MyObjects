using System.Collections.Generic;

namespace MyObjects.Demo.Model.Products
{
    public partial class Product : AggregateRoot
    {
        public virtual string Name { get; protected set; }

        public virtual int ExternalId { get; protected internal set; }

        public virtual ICollection<ProductCategory> Categories => this.categories;

        public Product(string name)
        {
            Name = name;
        }
    }
}