
    create table [Product] (
        Id INT IDENTITY NOT NULL,
       Version INT not null,
       Name NVARCHAR(255) null,
       ExternalId INT null,
       CreatedBy NVARCHAR(255) null,
       UpdatedBy NVARCHAR(255) null,
       CreatedOn DATETIMEOFFSET null,
       UpdatedOn DATETIMEOFFSET null,
       primary key CLUSTERED (Id)
    )

    create table Product_Categories (
        ProductId INT not null,
       CategorieId INT not null,
       primary key CLUSTERED (ProductId, CategorieId)
    )

    create table [ProductCategory] (
        Id INT IDENTITY NOT NULL,
       Version INT not null,
       Name NVARCHAR(255) null,
       ExternalId INT null,
       ParentId INT null,
       CreatedBy NVARCHAR(255) null,
       UpdatedBy NVARCHAR(255) null,
       CreatedOn DATETIMEOFFSET null,
       UpdatedOn DATETIMEOFFSET null,
       primary key CLUSTERED (Id)
    )

    create table [SalesOrder] (
        Id INT IDENTITY NOT NULL,
       Version INT not null,
       Number NVARCHAR(255) null,
       Status INT null,
       Total DECIMAL(19,5) null,
       CreatedBy NVARCHAR(255) null,
       UpdatedBy NVARCHAR(255) null,
       CreatedOn DATETIMEOFFSET null,
       UpdatedOn DATETIMEOFFSET null,
       primary key CLUSTERED (Id)
    )

    create table [SalesOrderLine] (
        Id INT IDENTITY NOT NULL,
       Version INT not null,
       OrderId INT null,
       ProductId INT null,
       Quantity DECIMAL(19,5) null,
       Price DECIMAL(19,5) null,
       Value DECIMAL(19,5) null,
       CreatedBy NVARCHAR(255) null,
       UpdatedBy NVARCHAR(255) null,
       CreatedOn DATETIMEOFFSET null,
       UpdatedOn DATETIMEOFFSET null,
       primary key CLUSTERED (Id)
    )

    alter table Product_Categories 
        add constraint FK_PRODUCT_CATEGORIES_CategorieId 
        foreign key (CategorieId) 
        references [ProductCategory]

    alter table Product_Categories 
        add constraint FK_PRODUCT_CATEGORIES_ProductId 
        foreign key (ProductId) 
        references [Product]

    alter table [ProductCategory] 
        add constraint FK_PRODUCTCATEGORY_PRODUCTCATEGORY 
        foreign key (ParentId) 
        references [ProductCategory]

    create index IDX_PRODUCTCATEGORY_PARENT on [ProductCategory] (ParentId)

    alter table [SalesOrderLine] 
        add constraint FK_SALESORDERLINE_SALESORDER 
        foreign key (OrderId) 
        references [SalesOrder]

    alter table [SalesOrderLine] 
        add constraint FK_SALESORDERLINE_PRODUCT 
        foreign key (ProductId) 
        references [Product]

    create index IDX_SALESORDERLINE_ORDER on [SalesOrderLine] (OrderId)

    create index IDX_SALESORDERLINE_PRODUCT on [SalesOrderLine] (ProductId)
