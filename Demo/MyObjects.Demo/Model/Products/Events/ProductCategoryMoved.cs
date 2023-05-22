namespace MyObjects.Demo.Model.Products.Events;

public class ProductCategoryMoved : IDomainEvent
{
    public ProductCategory Category { get; }
    public ProductCategory? PreviousParent { get; }

    public ProductCategoryMoved(ProductCategory category, ProductCategory? previousParent)
    {
        this.Category = category;
        this.PreviousParent = previousParent;
    }
}