using System.Collections.Generic;

namespace MyObjects.Demo.Model.Products
{
    public partial class Product : AggregateRoot
    {
        public virtual string Name { get; protected internal set; }
        public virtual string Description { get; protected internal set; }

        public virtual int? ExternalId { get; protected internal set; }
        public virtual int Foo { get; protected internal set; }

        public virtual ICollection<ProductCategory> Categories => this.categories;

        public Product(string name)
        {
            Name = name;
        }
    }
}