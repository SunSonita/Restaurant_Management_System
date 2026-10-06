/* =========================================================
   Restaurant Management System Database Schema & Views
   Target Database: RestaurantDB
   Engine: Microsoft SQL Server
========================================================= */

IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'RestaurantDB')
BEGIN
    CREATE DATABASE RestaurantDB;
END
GO

USE RestaurantDB;
GO

/* ================= 1. SYSTEM & SECURITY ================= */
IF OBJECT_ID('dbo.COMPANY_PROFILE', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.COMPANY_PROFILE (
        [CompanyID] int IDENTITY(1,1) NOT NULL,
        [CompanyName] nvarchar(150) NOT NULL,
        [LogoPath] varchar(260) NULL,
        [Phone] varchar(30) NULL,
        [Address] nvarchar(300) NULL,
        CONSTRAINT PK_COMPANY_PROFILE PRIMARY KEY ([CompanyID])
    );
END
GO

IF OBJECT_ID('dbo.APP_USER', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.APP_USER (
        [UserID] int IDENTITY(1,1) NOT NULL,
        [Username] varchar(50) NOT NULL,
        [PasswordHash] varchar(255) NOT NULL,
        [FullName] nvarchar(100) NOT NULL,
        [Role] varchar(20) NOT NULL CONSTRAINT DF_APP_USER_Role DEFAULT 'Cashier',
        [ProfileImagePath] varchar(260) NULL,
        [IsActive] bit NOT NULL CONSTRAINT DF_APP_USER_IsActive DEFAULT 1,
        [CreatedAt] datetime2 NOT NULL CONSTRAINT DF_APP_USER_CreatedAt DEFAULT SYSDATETIME(),
        CONSTRAINT PK_APP_USER PRIMARY KEY ([UserID]),
        CONSTRAINT UQ_APP_USER_Username UNIQUE ([Username]),
        CONSTRAINT CK_APP_USER_Role CHECK (Role IN ('Admin','Manager','Cashier'))
    );
END
GO

/* ================= 2. TABLE MANAGEMENT ================= */
IF OBJECT_ID('dbo.TABLE_GROUP', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.TABLE_GROUP (
        [TableGroupID] int IDENTITY(1,1) NOT NULL,
        [GroupCode] varchar(20) NOT NULL,
        [GroupName] nvarchar(100) NOT NULL,
        [GroupType] varchar(20) NOT NULL,
        [ImagePath] varchar(260) NULL,
        CONSTRAINT PK_TABLE_GROUP PRIMARY KEY ([TableGroupID]),
        CONSTRAINT UQ_TABLE_GROUP_GroupCode UNIQUE ([GroupCode]),
        CONSTRAINT CK_TABLE_GROUP_GroupType CHECK (GroupType IN ('MainTable','Delivery','TakeOut'))
    );
END
GO

IF OBJECT_ID('dbo.DINING_TABLE', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.DINING_TABLE (
        [TableID] int IDENTITY(1,1) NOT NULL,
        [TableGroupID] int NOT NULL,
        [TableCode] varchar(20) NOT NULL,
        [TableName] nvarchar(100) NOT NULL,
        [Capacity] int NOT NULL CONSTRAINT DF_DINING_TABLE_Capacity DEFAULT 4,
        [Status] varchar(20) NOT NULL CONSTRAINT DF_DINING_TABLE_Status DEFAULT 'Available',
        [ImagePath] varchar(260) NULL,
        [IsActive] bit NOT NULL CONSTRAINT DF_DINING_TABLE_IsActive DEFAULT 1,
        CONSTRAINT PK_DINING_TABLE PRIMARY KEY ([TableID]),
        CONSTRAINT FK_DINING_TABLE_TableGroupID FOREIGN KEY ([TableGroupID]) REFERENCES dbo.TABLE_GROUP([TableGroupID]),
        CONSTRAINT UQ_DINING_TABLE_TableCode UNIQUE ([TableCode])
    );
END
GO

IF COL_LENGTH('dbo.DINING_TABLE', 'Capacity') IS NULL
    ALTER TABLE dbo.DINING_TABLE ADD [Capacity] int NOT NULL CONSTRAINT DF_DINING_TABLE_Capacity DEFAULT 4;
IF COL_LENGTH('dbo.DINING_TABLE', 'Status') IS NULL
    ALTER TABLE dbo.DINING_TABLE ADD [Status] varchar(20) NOT NULL CONSTRAINT DF_DINING_TABLE_Status DEFAULT 'Available';
GO

/* ================= 3. MENU & ITEMS ================= */
IF OBJECT_ID('dbo.UOM', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.UOM (
        [UomID] int IDENTITY(1,1) NOT NULL,
        [UomName] nvarchar(30) NOT NULL,
        CONSTRAINT PK_UOM PRIMARY KEY ([UomID]),
        CONSTRAINT UQ_UOM_UomName UNIQUE ([UomName])
    );
END
GO

IF OBJECT_ID('dbo.PRINTER', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.PRINTER (
        [PrinterID] int IDENTITY(1,1) NOT NULL,
        [PrinterName] nvarchar(100) NOT NULL,
        CONSTRAINT PK_PRINTER PRIMARY KEY ([PrinterID]),
        CONSTRAINT UQ_PRINTER_PrinterName UNIQUE ([PrinterName])
    );
END
GO

IF OBJECT_ID('dbo.CURRENCY', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.CURRENCY (
        [CurrencyCode] char(3) NOT NULL,
        [CurrencyName] nvarchar(50) NOT NULL,
        [Symbol] nvarchar(5) NOT NULL,
        [IsBase] bit NOT NULL CONSTRAINT DF_CURRENCY_IsBase DEFAULT 0,
        CONSTRAINT PK_CURRENCY PRIMARY KEY ([CurrencyCode])
    );
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'UX_CURRENCY_OneBase' AND object_id = OBJECT_ID('dbo.CURRENCY'))
BEGIN
    CREATE UNIQUE INDEX UX_CURRENCY_OneBase ON dbo.CURRENCY([IsBase]) WHERE [IsBase] = 1;
END
GO

IF OBJECT_ID('dbo.ITEM_GROUP', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.ITEM_GROUP (
        [GroupID] int IDENTITY(1,1) NOT NULL,
        [ParentGroupID] int NULL,
        [GroupCode] varchar(20) NOT NULL,
        [GroupName] nvarchar(100) NOT NULL,
        [ImagePath] varchar(260) NULL,
        [IsVisible] bit NOT NULL CONSTRAINT DF_ITEM_GROUP_IsVisible DEFAULT 1,
        CONSTRAINT PK_ITEM_GROUP PRIMARY KEY ([GroupID]),
        CONSTRAINT FK_ITEM_GROUP_ParentGroupID FOREIGN KEY ([ParentGroupID]) REFERENCES dbo.ITEM_GROUP([GroupID]),
        CONSTRAINT UQ_ITEM_GROUP_GroupCode UNIQUE ([GroupCode])
    );
END
GO

IF OBJECT_ID('dbo.ITEM', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.ITEM (
        [ItemID] int IDENTITY(1,1) NOT NULL,
        [ItemCode] varchar(30) NOT NULL,
        [ItemName] nvarchar(150) NOT NULL,
        [ItemName2] nvarchar(150) NULL,
        [Description] nvarchar(500) NULL,
        [GroupID] int NOT NULL,
        [UomID] int NOT NULL,
        [ImagePath] varchar(260) NULL,
        [IsStockItem] bit NOT NULL CONSTRAINT DF_ITEM_IsStockItem DEFAULT 0,
        [ValuationMethod] varchar(20) NULL,
        [PrinterID] int NULL,
        [Printer2ID] int NULL,
        [IsInactive] bit NOT NULL CONSTRAINT DF_ITEM_IsInactive DEFAULT 0,
        [CreatedAt] datetime2 NOT NULL CONSTRAINT DF_ITEM_CreatedAt DEFAULT SYSDATETIME(),
        CONSTRAINT PK_ITEM PRIMARY KEY ([ItemID]),
        CONSTRAINT UQ_ITEM_ItemCode UNIQUE ([ItemCode]),
        CONSTRAINT FK_ITEM_GroupID FOREIGN KEY ([GroupID]) REFERENCES dbo.ITEM_GROUP([GroupID]),
        CONSTRAINT FK_ITEM_UomID FOREIGN KEY ([UomID]) REFERENCES dbo.UOM([UomID]),
        CONSTRAINT CK_ITEM_ValuationMethod CHECK (ValuationMethod IN ('Standard','FIFO','MovingAverage')),
        CONSTRAINT FK_ITEM_PrinterID FOREIGN KEY ([PrinterID]) REFERENCES dbo.PRINTER([PrinterID]),
        CONSTRAINT FK_ITEM_Printer2ID FOREIGN KEY ([Printer2ID]) REFERENCES dbo.PRINTER([PrinterID])
    );
END
GO

IF OBJECT_ID('dbo.ITEM_PRICE', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.ITEM_PRICE (
        [ItemID] int NOT NULL,
        [CurrencyCode] char(3) NOT NULL,
        [Price] decimal(18,2) NOT NULL CONSTRAINT DF_ITEM_PRICE_Price DEFAULT 0,
        CONSTRAINT PK_ITEM_PRICE PRIMARY KEY ([ItemID], [CurrencyCode]),
        CONSTRAINT FK_ITEM_PRICE_ItemID FOREIGN KEY ([ItemID]) REFERENCES dbo.ITEM([ItemID]) ON DELETE CASCADE,
        CONSTRAINT FK_ITEM_PRICE_CurrencyCode FOREIGN KEY ([CurrencyCode]) REFERENCES dbo.CURRENCY([CurrencyCode]),
        CONSTRAINT CK_ITEM_PRICE_Price CHECK (Price >= 0)
    );
END
GO

/* ================= 4. POS ================= */
IF OBJECT_ID('dbo.CUSTOMER', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.CUSTOMER (
        [CustomerID] int IDENTITY(1,1) NOT NULL,
        [CustomerCode] varchar(20) NOT NULL,
        [CustomerName] nvarchar(100) NOT NULL,
        [Phone] varchar(30) NULL,
        [IsDefault] bit NOT NULL CONSTRAINT DF_CUSTOMER_IsDefault DEFAULT 0,
        CONSTRAINT PK_CUSTOMER PRIMARY KEY ([CustomerID]),
        CONSTRAINT UQ_CUSTOMER_CustomerCode UNIQUE ([CustomerCode])
    );
END
GO

IF OBJECT_ID('dbo.DISCOUNT_REASON', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.DISCOUNT_REASON (
        [ReasonID] int IDENTITY(1,1) NOT NULL,
        [ReasonName] nvarchar(50) NOT NULL,
        [IsActive] bit NOT NULL CONSTRAINT DF_DISCOUNT_REASON_IsActive DEFAULT 1,
        CONSTRAINT PK_DISCOUNT_REASON PRIMARY KEY ([ReasonID]),
        CONSTRAINT UQ_DISCOUNT_REASON_ReasonName UNIQUE ([ReasonName])
    );
END
GO

IF OBJECT_ID('dbo.SALE_ORDER', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.SALE_ORDER (
        [OrderID] bigint IDENTITY(1,1) NOT NULL,
        [OrderNo] varchar(30) NOT NULL,
        [InvoiceNo] varchar(50) NULL,
        [TableID] int NULL,
        [CustomerID] int NOT NULL,
        [CreatedBy] int NOT NULL,
        [PostingDate] datetime2 NOT NULL CONSTRAINT DF_SALE_ORDER_PostingDate DEFAULT SYSDATETIME(),
        [Status] varchar(20) NOT NULL CONSTRAINT DF_SALE_ORDER_Status DEFAULT 'Open',
        [Note] nvarchar(500) NULL,
        [SubTotal] decimal(18,2) NOT NULL CONSTRAINT DF_SALE_ORDER_SubTotal DEFAULT 0,
        [ItemDiscountTotal] decimal(18,2) NOT NULL CONSTRAINT DF_SALE_ORDER_ItemDiscountTotal DEFAULT 0,
        [DocDiscountPercent] decimal(9,4) NOT NULL CONSTRAINT DF_SALE_ORDER_DocDiscountPercent DEFAULT 0,
        [DocDiscountAmount] decimal(18,2) NOT NULL CONSTRAINT DF_SALE_ORDER_DocDiscountAmount DEFAULT 0,
        [GrandTotal] decimal(18,2) NOT NULL CONSTRAINT DF_SALE_ORDER_GrandTotal DEFAULT 0,
        [CurrencyCode] char(3) NOT NULL CONSTRAINT DF_SALE_ORDER_CurrencyCode DEFAULT 'KHR',
        [ExchangeRate] decimal(18,4) NULL,
        CONSTRAINT PK_SALE_ORDER PRIMARY KEY ([OrderID]),
        CONSTRAINT UQ_SALE_ORDER_OrderNo UNIQUE ([OrderNo]),
        CONSTRAINT FK_SALE_ORDER_TableID FOREIGN KEY ([TableID]) REFERENCES dbo.DINING_TABLE([TableID]),
        CONSTRAINT FK_SALE_ORDER_CustomerID FOREIGN KEY ([CustomerID]) REFERENCES dbo.CUSTOMER([CustomerID]),
        CONSTRAINT FK_SALE_ORDER_CurrencyCode FOREIGN KEY ([CurrencyCode]) REFERENCES dbo.CURRENCY([CurrencyCode]),
        CONSTRAINT FK_SALE_ORDER_CreatedBy FOREIGN KEY ([CreatedBy]) REFERENCES dbo.APP_USER([UserID]),
        CONSTRAINT CK_SALE_ORDER_Status CHECK (Status IN ('Open','Sent','Billed','Paid','Void'))
    );
END
GO

IF COL_LENGTH('dbo.SALE_ORDER', 'InvoiceNo') IS NULL
    ALTER TABLE dbo.SALE_ORDER ADD [InvoiceNo] varchar(50) NULL;
GO

IF OBJECT_ID('dbo.SALE_ORDER_ITEM', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.SALE_ORDER_ITEM (
        [OrderItemID] bigint IDENTITY(1,1) NOT NULL,
        [OrderID] bigint NOT NULL,
        [LineNo] int NOT NULL,
        [ItemID] int NOT NULL,
        [ItemName] nvarchar(150) NOT NULL,
        [UomName] nvarchar(30) NOT NULL,
        [Qty] decimal(18,3) NOT NULL,
        [UnitPrice] decimal(18,2) NOT NULL,
        [TotalBeforeDis] decimal(18,2) NOT NULL,
        [DiscountPercent] decimal(9,4) NOT NULL CONSTRAINT DF_SALE_ORDER_ITEM_DiscountPercent DEFAULT 0,
        [DiscountAmount] decimal(18,2) NOT NULL CONSTRAINT DF_SALE_ORDER_ITEM_DiscountAmount DEFAULT 0,
        [ReasonID] int NULL,
        [TotalAfterDis] decimal(18,2) NOT NULL,
        [Note] nvarchar(500) NULL,
        [SentToKitchenAt] datetime2 NULL,
        CONSTRAINT PK_SALE_ORDER_ITEM PRIMARY KEY ([OrderItemID]),
        CONSTRAINT FK_SALE_ORDER_ITEM_OrderID FOREIGN KEY ([OrderID]) REFERENCES dbo.SALE_ORDER([OrderID]),
        CONSTRAINT FK_SALE_ORDER_ITEM_ItemID FOREIGN KEY ([ItemID]) REFERENCES dbo.ITEM([ItemID]),
        CONSTRAINT CK_SALE_ORDER_ITEM_Qty CHECK (Qty > 0),
        CONSTRAINT CK_SALE_ORDER_ITEM_DiscountPercent CHECK (DiscountPercent BETWEEN 0 AND 100),
        CONSTRAINT FK_SALE_ORDER_ITEM_ReasonID FOREIGN KEY ([ReasonID]) REFERENCES dbo.DISCOUNT_REASON([ReasonID]),
        CONSTRAINT UQ_OrderItem_Line UNIQUE ([OrderID], [LineNo])
    );
END
GO

/* ================= 5. PAYMENT ================= */
IF OBJECT_ID('dbo.EXCHANGE_RATE', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.EXCHANGE_RATE (
        [RateID] int IDENTITY(1,1) NOT NULL,
        [CurrencyCode] char(3) NOT NULL,
        [Rate] decimal(18,4) NOT NULL,
        [EffectiveDate] date NOT NULL,
        CONSTRAINT PK_EXCHANGE_RATE PRIMARY KEY ([RateID]),
        CONSTRAINT FK_EXCHANGE_RATE_CurrencyCode FOREIGN KEY ([CurrencyCode]) REFERENCES dbo.CURRENCY([CurrencyCode]),
        CONSTRAINT CK_EXCHANGE_RATE_Rate CHECK (Rate > 0),
        CONSTRAINT UQ_ExchangeRate UNIQUE ([CurrencyCode], [EffectiveDate])
    );
END
GO

IF OBJECT_ID('dbo.PAYMENT_METHOD', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.PAYMENT_METHOD (
        [MethodID] tinyint IDENTITY(1,1) NOT NULL,
        [MethodName] nvarchar(50) NOT NULL,
        CONSTRAINT PK_PAYMENT_METHOD PRIMARY KEY ([MethodID]),
        CONSTRAINT UQ_PAYMENT_METHOD_Name UNIQUE ([MethodName])
    );
END
GO

IF OBJECT_ID('dbo.PAYMENT', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.PAYMENT (
        [PaymentID] bigint IDENTITY(1,1) NOT NULL,
        [OrderID] bigint NOT NULL,
        [InvoiceNo] varchar(50) NULL,
        [PaymentDate] datetime2 NOT NULL CONSTRAINT DF_PAYMENT_PaymentDate DEFAULT SYSDATETIME(),
        [ReceivedBy] int NOT NULL,
        [TotalDue] decimal(18,2) NULL,
        [TotalReceived] decimal(18,2) NOT NULL,
        [ChangeGiven] decimal(18,2) NOT NULL CONSTRAINT DF_PAYMENT_ChangeGiven DEFAULT 0,
        [ChangeAmount] decimal(18,2) NULL,
        [ExchangeRate] decimal(18,4) NOT NULL CONSTRAINT DF_PAYMENT_ExchangeRate DEFAULT 4000,
        CONSTRAINT PK_PAYMENT PRIMARY KEY ([PaymentID]),
        CONSTRAINT FK_PAYMENT_OrderID FOREIGN KEY ([OrderID]) REFERENCES dbo.SALE_ORDER([OrderID]),
        CONSTRAINT FK_PAYMENT_ReceivedBy FOREIGN KEY ([ReceivedBy]) REFERENCES dbo.APP_USER([UserID]),
        CONSTRAINT CK_PAYMENT_TotalReceived CHECK (TotalReceived >= 0),
        CONSTRAINT CK_PAYMENT_ChangeGiven CHECK (ChangeGiven >= 0)
    );
END
GO

IF COL_LENGTH('dbo.PAYMENT', 'InvoiceNo') IS NULL
    ALTER TABLE dbo.PAYMENT ADD [InvoiceNo] varchar(50) NULL;
IF COL_LENGTH('dbo.PAYMENT', 'TotalDue') IS NULL
    ALTER TABLE dbo.PAYMENT ADD [TotalDue] decimal(18,2) NULL;
IF COL_LENGTH('dbo.PAYMENT', 'ChangeAmount') IS NULL
    ALTER TABLE dbo.PAYMENT ADD [ChangeAmount] decimal(18,2) NULL;
IF COL_LENGTH('dbo.PAYMENT', 'ExchangeRate') IS NULL
    ALTER TABLE dbo.PAYMENT ADD [ExchangeRate] decimal(18,4) NOT NULL CONSTRAINT DF_PAYMENT_ExchangeRate DEFAULT 4000;
GO

IF OBJECT_ID('dbo.PAYMENT_DETAIL', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.PAYMENT_DETAIL (
        [DetailID] bigint IDENTITY(1,1) NOT NULL,
        [PaymentID] bigint NOT NULL,
        [MethodID] tinyint NOT NULL,
        [CurrencyCode] char(3) NOT NULL,
        [Amount] decimal(18,2) NOT NULL,
        [ExchangeRate] decimal(18,4) NOT NULL CONSTRAINT DF_PAYMENT_DETAIL_ExchangeRate DEFAULT 1,
        [AmountInBase] AS ([Amount] * [ExchangeRate]) PERSISTED,
        CONSTRAINT PK_PAYMENT_DETAIL PRIMARY KEY ([DetailID]),
        CONSTRAINT FK_PAYMENT_DETAIL_PaymentID FOREIGN KEY ([PaymentID]) REFERENCES dbo.PAYMENT([PaymentID]) ON DELETE CASCADE,
        CONSTRAINT FK_PAYMENT_DETAIL_MethodID FOREIGN KEY ([MethodID]) REFERENCES dbo.PAYMENT_METHOD([MethodID]),
        CONSTRAINT FK_PAYMENT_DETAIL_CurrencyCode FOREIGN KEY ([CurrencyCode]) REFERENCES dbo.CURRENCY([CurrencyCode]),
        CONSTRAINT CK_PAYMENT_DETAIL_Amount CHECK (Amount > 0)
    );
END
GO

/* ================= 6. SHIFT & CASH ================= */
IF OBJECT_ID('dbo.SHIFT', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.SHIFT (
        [ShiftID] bigint IDENTITY(1,1) NOT NULL,
        [CashierID] int NOT NULL,
        [StartTime] datetime2 NOT NULL CONSTRAINT DF_SHIFT_StartTime DEFAULT SYSDATETIME(),
        [EndTime] datetime2 NULL,
        [OpeningCash] decimal(18,2) NOT NULL,
        [ClosingCash] decimal(18,2) NULL,
        [ActualCash] decimal(18,2) NULL,
        [CashDifference] AS ([ActualCash] - [ClosingCash]) PERSISTED,
        CONSTRAINT PK_SHIFT PRIMARY KEY ([ShiftID]),
        CONSTRAINT FK_SHIFT_CashierID FOREIGN KEY ([CashierID]) REFERENCES dbo.APP_USER([UserID]),
        CONSTRAINT CK_SHIFT_OpeningCash CHECK (OpeningCash >= 0)
    );
END
GO

IF OBJECT_ID('dbo.CASH_TRANSACTION', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.CASH_TRANSACTION (
        [TransID] bigint IDENTITY(1,1) NOT NULL,
        [ShiftID] bigint NOT NULL,
        [TransType] varchar(10) NOT NULL,
        [Amount] decimal(18,2) NOT NULL,
        [Reason] nvarchar(200) NOT NULL,
        [CreatedBy] int NOT NULL,
        [CreatedAt] datetime2 NOT NULL CONSTRAINT DF_CASH_TRANSACTION_CreatedAt DEFAULT SYSDATETIME(),
        CONSTRAINT PK_CASH_TRANSACTION PRIMARY KEY ([TransID]),
        CONSTRAINT FK_CASH_TRANSACTION_ShiftID FOREIGN KEY ([ShiftID]) REFERENCES dbo.SHIFT([ShiftID]),
        CONSTRAINT FK_CASH_TRANSACTION_CreatedBy FOREIGN KEY ([CreatedBy]) REFERENCES dbo.APP_USER([UserID]),
        CONSTRAINT CK_CASH_TRANSACTION_Type CHECK (TransType IN ('PayIn','PayOut')),
        CONSTRAINT CK_CASH_TRANSACTION_Amount CHECK (Amount > 0)
    );
END
GO

/* ================= 7. SYSTEM & AUDIT ================= */
IF OBJECT_ID('dbo.AUDIT_LOG', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.AUDIT_LOG (
        [LogID] bigint IDENTITY(1,1) NOT NULL,
        [TableName] varchar(50) NOT NULL,
        [RecordID] bigint NOT NULL,
        [Action] varchar(10) NOT NULL,
        [OldValues] nvarchar(max) NULL,
        [NewValues] nvarchar(max) NULL,
        [ChangedBy] int NOT NULL,
        [ChangedAt] datetime2 NOT NULL CONSTRAINT DF_AUDIT_LOG_ChangedAt DEFAULT SYSDATETIME(),
        CONSTRAINT PK_AUDIT_LOG PRIMARY KEY ([LogID]),
        CONSTRAINT FK_AUDIT_LOG_ChangedBy FOREIGN KEY ([ChangedBy]) REFERENCES dbo.APP_USER([UserID]),
        CONSTRAINT CK_AUDIT_LOG_Action CHECK (Action IN ('INSERT','UPDATE','DELETE'))
    );
END
GO

IF OBJECT_ID('dbo.SYSTEM_SETTING', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.SYSTEM_SETTING (
        [SettingKey] varchar(50) NOT NULL,
        [SettingValue] nvarchar(500) NOT NULL,
        [Description] nvarchar(200) NULL,
        CONSTRAINT PK_SYSTEM_SETTING PRIMARY KEY ([SettingKey])
    );
END
GO

/* ================= 7.1 INVENTORY & STOCK MANAGEMENT ================= */
IF OBJECT_ID('dbo.SUPPLIER', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.SUPPLIER (
        [SupplierID] int IDENTITY(1,1) NOT NULL,
        [SupplierName] nvarchar(150) NOT NULL,
        [ContactName] nvarchar(100) NULL,
        [Phone] varchar(50) NULL,
        [Email] varchar(100) NULL,
        [Address] nvarchar(250) NULL,
        [IsActive] bit NOT NULL CONSTRAINT DF_SUPPLIER_IsActive DEFAULT 1,
        [CreatedAt] datetime2 NOT NULL CONSTRAINT DF_SUPPLIER_CreatedAt DEFAULT SYSDATETIME(),
        CONSTRAINT PK_SUPPLIER PRIMARY KEY ([SupplierID])
    );
END
GO

IF OBJECT_ID('dbo.PURCHASE_ORDER', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.PURCHASE_ORDER (
        [PurchaseID] bigint IDENTITY(1,1) NOT NULL,
        [PurchaseNo] varchar(50) NOT NULL,
        [SupplierID] int NULL,
        [PurchaseDate] datetime2 NOT NULL CONSTRAINT DF_PURCHASE_ORDER_Date DEFAULT SYSDATETIME(),
        [TotalAmount] decimal(18,2) NOT NULL CONSTRAINT DF_PURCHASE_ORDER_Total DEFAULT 0,
        [Status] varchar(30) NOT NULL CONSTRAINT DF_PURCHASE_ORDER_Status DEFAULT 'Received',
        [Note] nvarchar(500) NULL,
        [CreatedBy] int NULL,
        CONSTRAINT PK_PURCHASE_ORDER PRIMARY KEY ([PurchaseID]),
        CONSTRAINT FK_PURCHASE_ORDER_Supplier FOREIGN KEY ([SupplierID]) REFERENCES dbo.SUPPLIER([SupplierID])
    );
END
GO

IF OBJECT_ID('dbo.PURCHASE_ORDER_ITEM', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.PURCHASE_ORDER_ITEM (
        [DetailID] bigint IDENTITY(1,1) NOT NULL,
        [PurchaseID] bigint NOT NULL,
        [ItemID] int NOT NULL,
        [Qty] decimal(18,2) NOT NULL,
        [UnitCost] decimal(18,2) NOT NULL,
        [TotalCost] decimal(18,2) NOT NULL,
        CONSTRAINT PK_PURCHASE_ORDER_ITEM PRIMARY KEY ([DetailID]),
        CONSTRAINT FK_PURCHASE_ORDER_ITEM_PO FOREIGN KEY ([PurchaseID]) REFERENCES dbo.PURCHASE_ORDER([PurchaseID]) ON DELETE CASCADE,
        CONSTRAINT FK_PURCHASE_ORDER_ITEM_Item FOREIGN KEY ([ItemID]) REFERENCES dbo.ITEM([ItemID])
    );
END
GO

IF OBJECT_ID('dbo.STOCK_MOVEMENT', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.STOCK_MOVEMENT (
        [MovementID] bigint IDENTITY(1,1) NOT NULL,
        [ItemID] int NOT NULL,
        [MovementType] varchar(30) NOT NULL,
        [Qty] decimal(18,2) NOT NULL,
        [UnitCost] decimal(18,2) NULL,
        [ReferenceType] varchar(50) NULL,
        [ReferenceID] varchar(50) NULL,
        [Reason] nvarchar(250) NULL,
        [CreatedAt] datetime2 NOT NULL CONSTRAINT DF_STOCK_MOVEMENT_CreatedAt DEFAULT SYSDATETIME(),
        [CreatedBy] int NULL,
        CONSTRAINT PK_STOCK_MOVEMENT PRIMARY KEY ([MovementID]),
        CONSTRAINT FK_STOCK_MOVEMENT_Item FOREIGN KEY ([ItemID]) REFERENCES dbo.ITEM([ItemID])
    );
END
GO

/* ================= 8. HELPER FUNCTIONS & TRIGGERS ================= */
IF OBJECT_ID('dbo.fn_UsdRate', 'FN') IS NOT NULL
    DROP FUNCTION dbo.fn_UsdRate;
GO

CREATE FUNCTION dbo.fn_UsdRate()
RETURNS decimal(18,4)
AS
BEGIN
    DECLARE @Rate decimal(18,4);
    SELECT TOP 1 @Rate = Rate 
    FROM dbo.EXCHANGE_RATE 
    WHERE CurrencyCode = 'USD' 
    ORDER BY EffectiveDate DESC, RateID DESC;

    RETURN ISNULL(@Rate, 4100.0000);
END;
GO

IF OBJECT_ID('dbo.TR_ITEM_PRICE_AutoConvert', 'TR') IS NOT NULL
    DROP TRIGGER dbo.TR_ITEM_PRICE_AutoConvert;
GO

CREATE TRIGGER dbo.TR_ITEM_PRICE_AutoConvert
ON dbo.ITEM_PRICE
AFTER INSERT, UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    IF TRIGGER_NESTLEVEL() > 1
        RETURN;

    DECLARE @UsdRate decimal(18,4) = dbo.fn_UsdRate();

    -- If KHR inserted/updated, update or insert USD
    IF EXISTS (SELECT 1 FROM inserted WHERE CurrencyCode = 'KHR')
    BEGIN
        MERGE dbo.ITEM_PRICE AS target
        USING (
            SELECT ItemID, ROUND(Price / @UsdRate, 2) AS ConvertedPrice
            FROM inserted
            WHERE CurrencyCode = 'KHR'
        ) AS src
        ON (target.ItemID = src.ItemID AND target.CurrencyCode = 'USD')
        WHEN MATCHED THEN
            UPDATE SET Price = src.ConvertedPrice
        WHEN NOT MATCHED THEN
            INSERT (ItemID, CurrencyCode, Price)
            VALUES (src.ItemID, 'USD', src.ConvertedPrice);
    END;

    -- If USD inserted/updated, update or insert KHR
    IF EXISTS (SELECT 1 FROM inserted WHERE CurrencyCode = 'USD')
    BEGIN
        MERGE dbo.ITEM_PRICE AS target
        USING (
            SELECT ItemID, ROUND(Price * @UsdRate, 0) AS ConvertedPrice
            FROM inserted
            WHERE CurrencyCode = 'USD'
        ) AS src
        ON (target.ItemID = src.ItemID AND target.CurrencyCode = 'KHR')
        WHEN MATCHED THEN
            UPDATE SET Price = src.ConvertedPrice
        WHEN NOT MATCHED THEN
            INSERT (ItemID, CurrencyCode, Price)
            VALUES (src.ItemID, 'KHR', src.ConvertedPrice);
    END;
END;
GO

/* ================= 9. REPORTING VIEWS ================= */
IF OBJECT_ID('dbo.vw_ItemList', 'V') IS NOT NULL
    DROP VIEW dbo.vw_ItemList;
GO

CREATE VIEW dbo.vw_ItemList
AS
SELECT 
    i.ItemID,
    i.ItemCode,
    i.ItemName,
    i.ItemName2,
    i.GroupID,
    g.GroupName,
    i.UomID,
    u.UomName,
    ISNULL(p_khr.Price, 0) AS PriceKHR,
    ISNULL(p_usd.Price, 0) AS PriceUSD,
    i.ImagePath,
    i.IsStockItem,
    i.ValuationMethod,
    i.PrinterID,
    i.Printer2ID,
    i.IsInactive,
    i.CreatedAt
FROM dbo.ITEM i
JOIN dbo.ITEM_GROUP g ON i.GroupID = g.GroupID
JOIN dbo.UOM u ON i.UomID = u.UomID
LEFT JOIN dbo.ITEM_PRICE p_khr ON i.ItemID = p_khr.ItemID AND p_khr.CurrencyCode = 'KHR'
LEFT JOIN dbo.ITEM_PRICE p_usd ON i.ItemID = p_usd.ItemID AND p_usd.CurrencyCode = 'USD';
GO

IF OBJECT_ID('dbo.vw_DailySaleSummary', 'V') IS NOT NULL
    DROP VIEW dbo.vw_DailySaleSummary;
GO

CREATE VIEW dbo.vw_DailySaleSummary
AS
SELECT 
    CAST(PostingDate AS date) AS SaleDate,
    COUNT(OrderID) AS TotalOrders,
    SUM(SubTotal) AS TotalSubTotal,
    SUM(ItemDiscountTotal + DocDiscountAmount) AS TotalDiscount,
    SUM(GrandTotal) AS TotalRevenueKHR,
    ROUND(SUM(GrandTotal) / dbo.fn_UsdRate(), 2) AS TotalRevenueUSD
FROM dbo.SALE_ORDER
WHERE Status IN ('Sent', 'Billed', 'Paid')
GROUP BY CAST(PostingDate AS date);
GO

IF OBJECT_ID('dbo.vw_SaleByTable', 'V') IS NOT NULL
    DROP VIEW dbo.vw_SaleByTable;
GO

CREATE VIEW dbo.vw_SaleByTable
AS
SELECT 
    t.TableID,
    t.TableCode,
    t.TableName,
    tg.GroupName AS TableGroupName,
    COUNT(o.OrderID) AS OrderCount,
    ISNULL(SUM(o.GrandTotal), 0) AS TotalAmountKHR,
    ISNULL(ROUND(SUM(o.GrandTotal) / dbo.fn_UsdRate(), 2), 0) AS TotalAmountUSD
FROM dbo.DINING_TABLE t
LEFT JOIN dbo.TABLE_GROUP tg ON t.TableGroupID = tg.TableGroupID
LEFT JOIN dbo.SALE_ORDER o ON t.TableID = o.TableID AND o.Status IN ('Sent', 'Billed', 'Paid')
GROUP BY t.TableID, t.TableCode, t.TableName, tg.GroupName;
GO

IF OBJECT_ID('dbo.vw_DailyPaymentSummary', 'V') IS NOT NULL
    DROP VIEW dbo.vw_DailyPaymentSummary;
GO

CREATE VIEW dbo.vw_DailyPaymentSummary
AS
SELECT 
    CAST(p.PaymentDate AS date) AS PaymentDate,
    pm.MethodName,
    pd.CurrencyCode,
    SUM(pd.Amount) AS TotalAmount,
    SUM(pd.AmountInBase) AS TotalAmountInBaseKHR
FROM dbo.PAYMENT p
JOIN dbo.PAYMENT_DETAIL pd ON p.PaymentID = pd.PaymentID
JOIN dbo.PAYMENT_METHOD pm ON pd.MethodID = pm.MethodID
GROUP BY CAST(p.PaymentDate AS date), pm.MethodName, pd.CurrencyCode;
GO

IF OBJECT_ID('dbo.vw_ItemSaleSummary', 'V') IS NOT NULL
    DROP VIEW dbo.vw_ItemSaleSummary;
GO

CREATE VIEW dbo.vw_ItemSaleSummary
AS
SELECT 
    soi.ItemID,
    soi.ItemName,
    soi.UomName,
    SUM(soi.Qty) AS TotalQuantitySold,
    SUM(soi.TotalAfterDis) AS TotalSalesKHR,
    ROUND(SUM(soi.TotalAfterDis) / dbo.fn_UsdRate(), 2) AS TotalSalesUSD
FROM dbo.SALE_ORDER_ITEM soi
JOIN dbo.SALE_ORDER so ON soi.OrderID = so.OrderID
WHERE so.Status IN ('Sent', 'Billed', 'Paid')
GROUP BY soi.ItemID, soi.ItemName, soi.UomName;
GO

IF OBJECT_ID('dbo.vw_ItemPrice', 'V') IS NOT NULL
    DROP VIEW dbo.vw_ItemPrice;
GO

CREATE VIEW dbo.vw_ItemPrice
AS
SELECT 
    ItemID,
    MAX(CASE WHEN CurrencyCode = 'KHR' THEN Price ELSE NULL END) AS PriceKHR,
    MAX(CASE WHEN CurrencyCode = 'USD' THEN Price ELSE NULL END) AS PriceUSD
FROM dbo.ITEM_PRICE
GROUP BY ItemID;
GO

IF OBJECT_ID('dbo.vw_ItemStock', 'V') IS NOT NULL
    DROP VIEW dbo.vw_ItemStock;
GO

CREATE VIEW dbo.vw_ItemStock
AS
SELECT 
    i.ItemID,
    ISNULL(SUM(CASE WHEN sm.MovementType IN ('Purchase', 'StockIn', 'AdjustmentIn') THEN sm.Qty 
                    WHEN sm.MovementType IN ('Sale', 'StockOut', 'AdjustmentOut', 'Waste') THEN -sm.Qty 
                    ELSE 0 END), 0) AS QtyOnHand,
    ISNULL(SUM(CASE WHEN sm.MovementType IN ('Purchase', 'StockIn', 'AdjustmentIn') THEN sm.Qty ELSE 0 END), 0) AS TotalIn,
    ISNULL(SUM(CASE WHEN sm.MovementType IN ('Sale', 'StockOut', 'AdjustmentOut', 'Waste') THEN sm.Qty ELSE 0 END), 0) AS TotalOut
FROM dbo.ITEM i
LEFT JOIN dbo.STOCK_MOVEMENT sm ON i.ItemID = sm.ItemID
GROUP BY i.ItemID;
GO

IF OBJECT_ID('dbo.vw_SaleSummary', 'V') IS NOT NULL
    DROP VIEW dbo.vw_SaleSummary;
GO

CREATE VIEW dbo.vw_SaleSummary
AS
SELECT 
    o.OrderID,
    o.PostingDate,
    ISNULL(o.InvoiceNo, o.OrderNo) AS InvoiceNo,
    o.OrderNo,
    ISNULL(u.FullName, 'System') AS Creator,
    o.SubTotal AS TotalBeforeDiscount,
    o.SubTotal AS TotalBeforeDis,
    o.ItemDiscountTotal AS DiscountItem,
    o.DocDiscountAmount AS DiscountOrder,
    (o.SubTotal - o.ItemDiscountTotal - o.DocDiscountAmount) AS TotalAfterDiscount,
    (o.SubTotal - o.ItemDiscountTotal - o.DocDiscountAmount) AS TotalAfterDis,
    ISNULL(o.TaxAmount, 0) AS Tax,
    ISNULL(o.ServiceChargeAmount, 0) AS ServiceCharge,
    o.GrandTotal,
    ISNULL(p.TotalReceived, o.GrandTotal) AS PaidAmount,
    ISNULL(p.TotalReceived, o.GrandTotal) AS Paid,
    ISNULL(p.ChangeGiven, 0) AS Change,
    ISNULL(p.ChangeGiven, 0) AS ChangeAmount,
    ISNULL(pm.MethodName, 'Cash') AS PaymentMethod,
    o.Status AS PaymentStatus,
    t.TableName,
    c.CustomerName
FROM dbo.SALE_ORDER o
LEFT JOIN dbo.APP_USER u ON o.CreatedBy = u.UserID
LEFT JOIN dbo.DINING_TABLE t ON o.TableID = t.TableID
LEFT JOIN dbo.CUSTOMER c ON o.CustomerID = c.CustomerID
OUTER APPLY (
    SELECT TOP 1 PaymentID, TotalReceived, ChangeGiven
    FROM dbo.PAYMENT WHERE OrderID = o.OrderID
    ORDER BY PaymentID DESC
) p
OUTER APPLY (
    SELECT TOP 1 m.MethodName
    FROM dbo.PAYMENT_DETAIL pd
    JOIN dbo.PAYMENT_METHOD m ON pd.MethodID = m.MethodID
    WHERE pd.PaymentID = p.PaymentID
) pm;
GO

IF OBJECT_ID('dbo.vw_SaleByTable', 'V') IS NOT NULL
    DROP VIEW dbo.vw_SaleByTable;
GO

CREATE VIEW dbo.vw_SaleByTable
AS
SELECT 
    o.OrderID,
    ISNULL(o.InvoiceNo, o.OrderNo) AS InvoiceNo,
    o.OrderNo,
    o.PostingDate,
    o.TableID,
    CASE 
        WHEN o.TableID IS NULL THEN 'Takeaway / Delivery' 
        ELSE ISNULL(t.TableName, 'Table ' + CAST(o.TableID AS varchar(10))) 
    END AS TableName,
    ISNULL(tg.GroupName, 'Main Dining Hall') AS GroupTable,
    ISNULL(u.FullName, 'System') AS Creator,
    o.SubTotal AS TotalBeforeDis,
    o.ItemDiscountTotal AS DiscountItem,
    o.DocDiscountAmount AS DiscountDoc,
    (o.SubTotal - o.ItemDiscountTotal - o.DocDiscountAmount) AS TotalAfterDis,
    ISNULL(p.TotalReceived, o.GrandTotal) AS Paid,
    o.Status
FROM dbo.SALE_ORDER o
LEFT JOIN dbo.DINING_TABLE t ON o.TableID = t.TableID
LEFT JOIN dbo.TABLE_GROUP tg ON t.TableGroupID = tg.TableGroupID
LEFT JOIN dbo.APP_USER u ON o.CreatedBy = u.UserID
OUTER APPLY (
    SELECT TOP 1 TotalReceived 
    FROM dbo.PAYMENT WHERE OrderID = o.OrderID
    ORDER BY PaymentID DESC
) p;
GO

IF OBJECT_ID('dbo.vw_SaleByGroup', 'V') IS NOT NULL
    DROP VIEW dbo.vw_SaleByGroup;
GO

CREATE VIEW dbo.vw_SaleByGroup
AS
SELECT 
    ISNULL(g.GroupName, 'Other') AS GroupName,
    ISNULL(SUM(oi.TotalAfterDis), 0) AS Amount
FROM dbo.SALE_ORDER_ITEM oi
JOIN dbo.ITEM i ON oi.ItemID = i.ItemID
LEFT JOIN dbo.ITEM_GROUP g ON i.GroupID = g.GroupID
JOIN dbo.SALE_ORDER o ON oi.OrderID = o.OrderID
WHERE o.Status = 'Paid'
GROUP BY g.GroupName;
GO

IF OBJECT_ID('dbo.PAYMENT_LINE', 'V') IS NOT NULL
    DROP VIEW dbo.PAYMENT_LINE;
GO

CREATE VIEW dbo.PAYMENT_LINE
AS
SELECT DetailID AS PaymentLineID, PaymentID, MethodID, Amount, CurrencyCode, ExchangeRate, AmountInBase
FROM dbo.PAYMENT_DETAIL;
GO

/* ================= 10. SEED INITIAL DATA ================= */
IF NOT EXISTS (SELECT 1 FROM dbo.APP_USER WHERE Username = 'admin')
BEGIN
    INSERT INTO dbo.APP_USER (Username, PasswordHash, FullName, Role, IsActive, ProfileImagePath)
    VALUES ('admin', 'admin123', 'System Administrator', 'Admin', 1, 'Resources\admin_profile.png');
END

IF NOT EXISTS (SELECT 1 FROM dbo.APP_USER WHERE Username = 'manager')
BEGIN
    INSERT INTO dbo.APP_USER (Username, PasswordHash, FullName, Role, IsActive)
    VALUES ('manager', 'manager123', 'Restaurant Manager', 'Manager', 1);
END

IF NOT EXISTS (SELECT 1 FROM dbo.APP_USER WHERE Username = 'cashier')
BEGIN
    INSERT INTO dbo.APP_USER (Username, PasswordHash, FullName, Role, IsActive)
    VALUES ('cashier', 'cashier123', 'Main Cashier', 'Cashier', 1);
END

IF NOT EXISTS (SELECT 1 FROM dbo.TABLE_GROUP)
BEGIN
    INSERT INTO dbo.TABLE_GROUP (GroupCode, GroupName, GroupType)
    VALUES 
    ('GRP-MAIN', 'Main Dining Hall', 'MainTable'),
    ('GRP-VIP', 'VIP Private Room', 'MainTable'),
    ('GRP-OUT', 'Outdoor Terrace', 'MainTable'),
    ('GRP-BAR', 'Bar Counter', 'MainTable');
END

IF NOT EXISTS (SELECT 1 FROM dbo.DINING_TABLE)
BEGIN
    INSERT INTO dbo.DINING_TABLE (TableGroupID, TableCode, TableName, Capacity, Status, IsActive)
    VALUES 
    (1, 'T-01', 'Table 1', 4, 'Available', 1),
    (1, 'T-02', 'Table 2', 4, 'Available', 1),
    (1, 'T-03', 'Table 3', 6, 'Available', 1),
    (2, 'VIP-1', 'VIP Room 1', 10, 'Available', 1);
END
GO
