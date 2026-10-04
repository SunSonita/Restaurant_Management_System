# Restaurant Management System

A comprehensive Restaurant Management System desktop application built with **C# (.NET 8 Windows Forms)** and **Microsoft SQL Server**.

---

## 🌟 Key Features

### 1. Point of Sale (POS)
- **Visual Order Management**: Real-time dining table selection, product catalog with item search, and category filtering.
- **Dynamic Table Status**: Visual status updates for Open, Sent to Kitchen, Billed, and Paid orders.
- **Flexible Bill Actions**: 
  - **Send to Kitchen**: Dispatches orders to kitchen printers and records timestamp.
  - **Bill**: Generates and prints professional 80mm thermal receipts with subtotal, tax, discounts, and dual-currency conversion.
  - **Pay**: Seamless integration with payment processing dialog supporting cash, ABA/QR, and credit cards.
- **Discounts**: Item-level and bill-level discounts with percentage or USD amount input, validated against reasons and order totals.

### 2. Multi-Currency Support (USD / KHR)
- Native dual-currency pricing with automated rate conversion (default base rate: 1 USD = 4,100 KHR).
- Price updates automatically synchronize both currencies via SQL triggers.
- Payment accepts USD and KHR with automatic change calculation in either currency.

### 3. Thermal Receipt Printing (80mm)
- High-resolution, cleanly aligned receipt rendering for standard 80mm (`80x80`) POS thermal printers.
- Displays Restaurant Header, Bill / Invoice number, Table Code, Cashier, itemized quantities and prices, dual currency totals (KHR and USD), and greeting footer.

### 4. Inventory & Item Master
- Categorized item groups with hierarchical parent-child relationships.
- Unit of Measure (UOM) management.
- Dual-printer assignment for beverage and kitchen preparation stations.
- Image attachment and instant search filters.

### 5. Table & Floor Management
- Multi-section / zone support (Main Dining, Delivery, Takeout).
- Visual table cards displaying table code, status, and active order totals.

### 6. Reports & Analytics
- **Daily Sales Summary**: Tracks orders, gross revenue, discounts, and net revenue in both KHR and USD.
- **Sales by Table**: Performance analysis per table and dining area.
- Fully responsive report layouts with automatic column stretching and aligned totals.

---

## 🛠️ Technology Stack

- **Platform**: .NET 8 Windows Forms (WinForms)
- **Language**: C# 12
- **Database**: Microsoft SQL Server / LocalDB (`(localdb)\MSSQLLocalDB` or SQL Server 2019/2022)
- **Data Access**: ADO.NET with SQL parameters and `DbHelper` connection pooling
- **Printing**: System.Drawing GDI+ PrintDocument optimized for 80mm thermal receipt width

---

## 🗄️ Database Setup

1. Open SQL Server Management Studio (SSMS) or SQLCMD.
2. Run the complete schema script located at:
   ```
   Database/RestaurantDB_Schema.sql
   ```
3. Update connection string in `Resturant_Management/App.config` if your server instance differs:
   ```xml
   <connectionStrings>
     <add name="DefaultConnection" 
          connectionString="Server=(localdb)\MSSQLLocalDB;Database=RestaurantDB;Integrated Security=True;TrustServerCertificate=True;" 
          providerName="System.Data.SqlClient" />
   </connectionStrings>
   ```

---

## 🚀 Getting Started

1. Clone repository:
   ```bash
   git clone https://github.com/SunSonita/Restaurant_Management_System.git
   cd Restaurant_Management_System
   ```
2. Open `Resturant_Management.sln` in **Visual Studio 2022** or build via .NET CLI:
   ```bash
   dotnet build
   dotnet run --project Resturant_Management
   ```
3. Default Cashier / Admin credentials:
   - **Username**: `admin`
   - **Password**: `123`
