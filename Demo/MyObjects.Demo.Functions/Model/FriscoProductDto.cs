using System;

namespace MyObjects.Demo.Functions.Model;

public partial class FriscoProductWrapperDto
    {
        public int ProductId { get; set; }
        public int PrimaryVariantId { get; set; }
        public FriscoProductDto Product { get; set; }
    }

    public partial class FriscoProductDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public long Ean { get; set; }
        public long PackSize { get; set; }
        public string UnitOfMeasure { get; set; }
        public double Grammage { get; set; }
        public string Producer { get; set; }
        public string Brand { get; set; }
        public string Supplier { get; set; }
        public Name Name { get; set; }
        public ProductContentData ContentData { get; set; }
        public Category PrimaryCategory { get; set; }
        public Category[] Categories { get; set; }
        public int PrimaryVariantId { get; set; }
        public object[] Variants { get; set; }
        public bool IsAvailable { get; set; }
        public bool IsStocked { get; set; }
        public long Stock { get; set; }
        public long UnrestrictedStock { get; set; }
        public long Vat { get; set; }
        public Price Price { get; set; }
        public string[] DeliveryMethods { get; set; }
        public DateTimeOffset ValidityPeriod { get; set; }
        public string CountryOfOrigin { get; set; }
        public string[] Tags { get; set; }
        public object[] Promotions { get; set; }
        public Restriction[] Restrictions { get; set; }
        public object[] Rations { get; set; }
        public object[] Containers { get; set; }
        public object[] Contests { get; set; }
        public Rating Rating { get; set; }
        public Uri ImageUrl { get; set; }
        public string Merchant { get; set; }
    }

    public partial class Category
    {
        public int CategoryId { get; set; }
        public string ParentPath { get; set; }
        public int Depth { get; set; }
        public Name Name { get; set; }
        public int? ParentId { get; set; }
    }

    public partial class Name
    {
        public string Pl { get; set; }
        public string En { get; set; }
    }

    public partial class ProductContentData
    {
    }

    public partial class Price
    {
        public bool IsPriceBeforeFirstDecrease { get; set; }
        public double PriceBeforeDiscount { get; set; }
        public double PricePrice { get; set; }
        public long DiscountPercent { get; set; }
        public DateTimeOffset ValidTo { get; set; }
        public string CampaignName { get; set; }
        public double PriceBeforeFirstDecrease { get; set; }
        public Guid DiscountId { get; set; }
    }

    public partial class Rating
    {
        public long Votes { get; set; }
        public double RatingRating { get; set; }
    }

    public partial class Restriction
    {
        public Guid RestrictionId { get; set; }
        public Delivery[] Deliveries { get; set; }
        public bool IsUnrestrictedStockDisallowed { get; set; }
        public long Priority { get; set; }
        public RestrictionContentData ContentData { get; set; }
    }

    public partial class RestrictionContentData
    {
        public string BadgeImg { get; set; }
        public string MessageSpecific { get; set; }
        public string Text { get; set; }
        public string Container { get; set; }
    }

    public partial class Delivery
    {
        public string[] DeliveryMethods { get; set; }
        public DeliveryDate[] DeliveryDates { get; set; }
    }

    public partial class DeliveryDate
    {
        public DateTimeOffset DateFrom { get; set; }
        public DateTimeOffset DateTo { get; set; }
        public long HourFrom { get; set; }
        public long HourTo { get; set; }
    }