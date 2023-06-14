using System.Collections.Generic;
using MyObjects.Demo.Model.Products.Events;
using MyObjects.NHibernate;

namespace MyObjects.Demo.Model.Products;

public partial class ProductCategory : AggregateRoot
{
    public virtual string Name { get; protected internal set; }
    public virtual int? ExternalId { get; protected internal set; }
    public virtual ProductCategory Parent { get; protected set; }
    
    [FetchWithSubselect]
    public virtual IEnumerable<ProductCategory> Children => this.children;
    
    public ProductCategory(string name)
    {
        Name = name;
    }

    public ProductCategory(string name, ProductCategory parent) : this(name)
    {
        Parent = parent;
    }

    public virtual void MoveUnder(ProductCategory parent)
    {
        var previousParent = this.Parent;
        this.Parent = parent;
        this.Publish(new ProductCategoryMoved(this, previousParent));
    }

    public virtual void MoveToRoot()
    {
        var previousParent = this.Parent;
        this.Parent = null;
        this.Publish(new ProductCategoryMoved(this, previousParent));
    }
}