# Blazor Pivot Table – MySQL Database Connection using UrlAdaptor

## Project Overview

This repository demonstrates a production-ready pattern for binding a **MySQL** database to the **Syncfusion Blazor Pivot Table** using the **URL Adaptor**. The sample application fetches order records stored in a MySQL table, exposes them through a RESTful ASP.NET Core Web API controller, and binds the result to the Pivot Table on the client through `SfDataManager` configured with `Adaptors.UrlAdaptor`.

The implementation shows a clean remote data binding architecture where the Pivot Table never talks to MySQL directly. Instead, every data operation (read, insert, update, delete) flows through HTTP endpoints handled by the `OrderController`, which in turn executes queries against MySQL using the **MySql.Data** (Oracle's MySqlClient) ADO.NET data provider.

### Key Features

- **MySQL Integration**: Order records are persisted in a MySQL table (`orders`) and read/written through the MySql.Data ADO.NET provider.
- **Syncfusion Blazor Pivot Table**: Field list, drill-through, and configurable rows, columns, and values for multidimensional analysis.
- **URL Adaptor**: Full control over pivot data operations (read and write) through dedicated `Url`, `InsertUrl`, `UpdateUrl`, and `RemoveUrl` endpoints.
- **Complete CRUD Operations**: Add, edit, and delete records directly from the pivot view and the drill-through grid.
- **DataManagerRequest**: Structured request object for handling complex pivot data operations, including search, filter, sort, and paging parameters.
- **Primary Key Configuration**: The `BeginDrillThrough` event marks `OrderID` as the primary key column so that CRUD operations from the drill-through grid target the correct record.
- **Configurable Backend Endpoint**: Server port managed via `Properties/launchSettings.json`.

## Repository

**Sample Location / Source Code:** [https://github.com/SyncfusionExamples/syncfusion-blazor-pivot-table-mysql-database-binding-sample](https://github.com/SyncfusionExamples/syncfusion-blazor-pivot-table-mysql-database-binding-sample)

## Prerequisites

| Component | Version | Purpose |
|-----------|---------|---------|
| Visual Studio 2022 / VS Code | Latest | Development IDE with Blazor workload |
| .NET SDK | net10.0 or compatible | Runtime and build tools |
| MySQL Server / MariaDB | 8.0 or later | Relational database server |
| MySql.Data NuGet package | Latest | MySQL ADO.NET data provider (Oracle Connector/NET) |
| Syncfusion.Blazor.PivotTable | Latest | Pivot Table UI component |
| Syncfusion.Blazor.Themes | Latest | Styling for the Pivot Table component |
| Syncfusion.Blazor.Data | Latest | Data adaptors including URL Adaptor support |
| Newtonsoft.Json | Latest | JSON serialization for CRUD models |

> **Note:** Install the latest version of the required Syncfusion Blazor NuGet packages and the latest available version of `MySql.Data` from NuGet. Do not pin to a specific patch version.

## MySQL Setup

### 1. Start the MySQL Server

Ensure the MySQL service is running on your machine. The sample expects the server to be reachable at:

```text
Server=localhost
Port=3306
```

Verify the service status with:

```powershell
Get-Service mysql*
Start-Service mysql*
```

### 2. Create the Database

Connect to MySQL using the `mysql` CLI, MySQL Workbench, or any SQL client and create the database:

```sql
CREATE DATABASE Orders;
```

### 3. Create the Table

Switch to the `Orders` database and create the `orders` table used by the sample:

```sql
USE Orders;

CREATE TABLE orders
(
    orderid        INT AUTO_INCREMENT PRIMARY KEY,
    customername   VARCHAR(100),
    employeeid     INT,
    shipcity       VARCHAR(100),
    freight        DECIMAL(10, 2)
);
```

### 4. Insert Sample Data (Optional)

Seed the table with a few rows so the Pivot Table has data to render on first load:

```sql
INSERT INTO orders (customername, employeeid, shipcity, freight) VALUES
('Toms',   1, 'New York',     35.30),
('Ravi',   2, 'London',       80.20),
('Sven',   1, 'Berlin',       52.10),
('Sara',   3, 'Madrid',       18.40),
('Paul',   2, 'Tokyo',        64.75);
```

### 5. Configure the Connection String

The connection string is defined in `Controllers/OrderController.cs`:

```csharp
string ConnectionString =
    "Server=localhost;Port=3306;Database=Orders;Uid=root;Pwd=password@123;";
```

Update the following values to match your local MySQL installation:

| Property | Default | Description |
|----------|---------|-------------|
| `Server` | `localhost` | MySQL server host |
| `Port` | `3306` | MySQL server port |
| `Database` | `Orders` | Target database name |
| `Uid` | `root` | Database user with read/write privileges |
| `Pwd` | `password@123` | Password for the database user |

## Project Structure

| File/Folder | Purpose |
|-------------|---------|
| `/Controllers/OrderController.cs` | REST API controller exposing read and CRUD endpoints that execute MySQL queries through MySql.Data |
| `/Models` | Contains the `Order` data model (defined inside `OrderController.cs`) representing an order record |
| `/Data` | Data access logic (implemented inline in `OrderController.cs` via `MySqlConnection`) |
| `/Repository` | Repository methods for read/write operations (implemented as controller actions `GetOrderData`, `Insert`, `Update`, `Delete`) |
| `/Components/Pages/Home.razor` | Pivot Table page with URL Adaptor configuration, cell editing, and drill-through setup |
| `/Components/App.razor` | Root component that references Syncfusion stylesheet and `syncfusion-blazor.min.js` |
| `/Program.cs` | Service registration for ASP.NET Core, Syncfusion Blazor, and MVC controllers |
| `/Properties/launchSettings.json` | Port configuration for the local development server |
| `/PivotTableMySQL.csproj` | Project file with Syncfusion Blazor packages and MySql.Data NuGet references |

## Application Flow

The sample follows a layered remote data binding architecture:

```text
MySQL Database  (orders table)
        ↓
MySql.Data Provider  (ADO.NET connection)
        ↓
OrderController       (REST API endpoints)
        ↓
UrlAdaptor            (HTTP request/response handling)
        ↓
SfDataManager         (client-side data manager)
        ↓
Blazor Pivot Table    (renders aggregated pivot view)
```

1. The **Pivot Table** raises a data request through `SfDataManager`.
2. `SfDataManager` formats the request as a `DataManagerRequest` and POSTs it to the **Url Adaptor** endpoint.
3. The **OrderController** receives the request, opens a **MySqlConnection**, and executes the appropriate SQL query against the **MySQL** `orders` table.
4. The result is serialized into the `{ result, count }` format expected by the Url Adaptor and returned to the client.
5. The **Pivot Table** renders the aggregated pivot view from the returned data.

## Configuration Steps

### Database Configuration

Create the `Orders` database and the `orders` table as described in the [MySQL Setup](#mysql-setup) section, then verify connectivity with the `mysql` CLI or MySQL Workbench.

```bash
mysql -h localhost -P 3306 -u root -p
```

### Connection String Setup

Update the `ConnectionString` field at the top of `OrderController.cs` to match your MySQL credentials:

```csharp
string ConnectionString =
    "Server=localhost;Port=3306;Database=Orders;Uid=root;Pwd=<your-password>;";
```

### Dependency Injection Registration

`Program.cs` registers the services required by the sample:

```csharp
builder.Services.AddSyncfusionBlazor();
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddControllers();
```

`app.MapControllers()` is called in the middleware pipeline so that `OrderController` endpoints are reachable from the Pivot Table.

### Controller Configuration

The controller routes are defined in `Controllers/OrderController.cs`:

| Endpoint | HTTP Method | Purpose |
|----------|-------------|---------|
| `POST /api/Order` | POST | Read endpoint that returns the `orders` collection as `{ result, count }` |
| `POST /api/Order/Insert` | POST | Insert a new order record |
| `POST /api/Order/Update` | POST | Update an existing order record |
| `POST /api/Order/Delete` | POST | Delete an order record by `OrderID` |

### UrlAdaptor Configuration

The `SfDataManager` configuration in `Components/Pages/Home.razor` binds the Pivot Table to the REST backend:

```razor
<SfDataManager
    Url="http://localhost:5145/api/Order"
    InsertUrl="http://localhost:5145/api/Order/Insert"
    UpdateUrl="http://localhost:5145/api/Order/Update"
    RemoveUrl="http://localhost:5145/api/Order/Delete"
    Adaptor="Adaptors.UrlAdaptor">
</SfDataManager>
```

| Property | Description |
|----------|-------------|
| **Url** | REST endpoint for reading order records with data operation support (search, filter, sort, paging) |
| **InsertUrl** | Endpoint for creating new records (including row insertion) |
| **UpdateUrl** | Endpoint for updating existing records (including cell edits) |
| **RemoveUrl** | Endpoint for deleting records (including row removal) |
| **Adaptor** | Set to `Adaptors.UrlAdaptor` to enable URL-based data binding |

> **Important:** The port in the URLs above (`5145`) must match the `applicationUrl` defined in `Properties/launchSettings.json`. If you change the port, update all four URLs in `Home.razor` accordingly.

## Getting Started / Quick Start

1. **Clone the repository**

   ```bash
   git clone https://github.com/SyncfusionExamples/syncfusion-blazor-pivot-table-mysql-database-binding-sample
   cd "syncfusion-blazor-pivot-table-mysql-database-binding-sample"
   cd "PivotTableMySQL"
   ```

   **Repository:** [https://github.com/SyncfusionExamples/syncfusion-blazor-pivot-table-mysql-database-binding-sample](https://github.com/SyncfusionExamples/syncfusion-blazor-pivot-table-mysql-database-binding-sample)

2. **Restore packages and build**

   From the repository root, change into the Blazor project folder and restore the dependencies:

   ```bash
   cd PivotTableMySQL
   dotnet restore
   dotnet build
   ```

3. **Start MySQL and seed the database**

   Ensure MySQL is running and that the `Orders` database and `orders` table exist (see [MySQL Setup](#mysql-setup)).

4. **Run the application**

   From inside the `PivotTableMySQL` project folder, start the application with `dotnet run`:

   ```bash
   cd PivotTableMySQL
   dotnet run
   ```

5. **Open the application**

   Navigate to the local URL displayed in the terminal (typically `https://localhost:7169` or `http://localhost:5145`).

6. **Access the Web API (Optional)**

   Send a POST request to `http://localhost:5145/api/Order` to verify the read endpoint returns order data from MySQL.

## CRUD Operations

The sample supports full CRUD operations through the Url Adaptor. Each operation is routed to a dedicated controller endpoint that executes the corresponding SQL statement against MySQL.

### Read

- The Pivot Table POSTs a `DataManagerRequest` to `POST /api/Order`.
- `OrderController.Post` calls `GetOrderData()`, which runs `SELECT * FROM orders ORDER BY orderid` through `MySqlDataAdapter`.
- The result is returned as `{ result = DataSource, count = count }`, which is the format expected by the Url Adaptor.

### Insert

1. Double-click an empty cell in the pivot view value area to open the editor.
2. Enter a value for the new measure and click **Update**.
3. The Pivot Table POSTs the new record to `POST /api/Order/Insert`.
4. The controller executes an `INSERT INTO orders (...)` statement against MySQL.

### Update

1. Double-click any value cell in the pivot view.
2. Modify the value in the editor dialog and click **Update**.
3. The Pivot Table POSTs the updated record to `POST /api/Order/Update`.
4. The controller executes an `UPDATE orders SET ... WHERE orderid=...` statement against MySQL.

### Delete

1. Right-click the row or cell to be removed and select the delete option.
2. Confirm the deletion in the dialog.
3. The Pivot Table POSTs the record key to `POST /api/Order/Delete`.
4. The controller executes a `DELETE FROM orders WHERE orderid=...` statement against MySQL.

### Drill-Through to Source Records

1. Double-click an aggregated value cell in the pivot view.
2. A drill-through grid opens showing the underlying MySQL records.
3. The `BeginDrillThrough` event handler marks the `OrderID` column as `IsPrimaryKey = true` so that CRUD operations from the drill-through grid uniquely identify the correct record.
4. Close the drill-through window to return to the pivot view.

## API Endpoints

| Endpoint | HTTP Method | Request Body | Description |
|----------|-------------|--------------|-------------|
| `/api/Order` | POST | `DataManagerRequest` | Returns the full `orders` collection as `{ result, count }` for Pivot Table binding |
| `/api/Order/Insert` | POST | `CRUDModel<Order>` | Inserts a new order record into MySQL |
| `/api/Order/Update` | POST | `CRUDModel<Order>` | Updates an existing order record in MySQL |
| `/api/Order/Delete` | POST | `CRUDModel<Order>` | Deletes an order record from MySQL using the `Key` field |

## Data Model

The `Order` model represents a single row in the MySQL `orders` table:

```csharp
public class Order
{
    [Key]
    public int? OrderID { get; set; }
    public string? CustomerName { get; set; }
    public int? EmployeeID { get; set; }
    public decimal? Freight { get; set; }
    public string? ShipCity { get; set; }
}
```

| Field | Type | Pivot Role | Description |
|-------|------|-----------|-------------|
| `OrderID` | `int` | Primary Key | Unique identifier for the order |
| `CustomerName` | `string` | Row | Customer name used as a pivot row field |
| `EmployeeID` | `int` | Column | Employee ID used as a pivot column field |
| `Freight` | `decimal` | Value | Freight amount used as the pivot value (aggregated measure) |
| `ShipCity` | `string` | Dimension | Ship city used as an additional pivot field |

## Static Assets

`Components/App.razor` references the Syncfusion stylesheet and script bundle:

```html
<link href="_content/Syncfusion.Blazor.Themes/bootstrap5.css" rel="stylesheet" />
<script src="_content/Syncfusion.Blazor.Core/scripts/syncfusion-blazor.min.js" type="text/javascript"></script>
```

If CSS or scripts fail to load, check the browser developer tools (F12) for 404 errors on `_content/Syncfusion.*` paths.

## Troubleshooting

### Connection Error

- Verify `Server`, `Port`, `Database`, `Uid`, and `Pwd` in `OrderController.cs` match your MySQL installation.
- Confirm the MySQL service is running and reachable on `localhost:3306`:
  - **Windows:** `Get-Service mysql*` and `Start-Service mysql*`
  - **macOS (Homebrew):** `brew services start mysql`
  - **Linux:** `sudo systemctl start mysql` (Ubuntu) or `sudo systemctl start mysqld` (RHEL/CentOS)
- Test the connection from the `mysql` CLI using the same credentials:
  ```bash
  mysql -h localhost -P 3306 -u root -p
  ```
- Ensure the database user has read/write privileges on the `orders` table:
  ```sql
  GRANT ALL PRIVILEGES ON Orders.* TO 'root'@'localhost';
  FLUSH PRIVILEGES;
  ```

### Missing Controllers / Endpoint Not Found

- Confirm `app.MapControllers()` is present in `Program.cs`.
- Verify all routes (`api/Order`, `api/Order/Insert`, `api/Order/Update`, `api/Order/Delete`) are defined in `OrderController.cs`.
- Check that the URL port in `Home.razor` matches the running port from `launchSettings.json`.

### Port Already in Use

- Change the port numbers in `Properties/launchSettings.json` to available ports.
- Update the `Url`, `InsertUrl`, `UpdateUrl`, and `RemoveUrl` properties in `Home.razor` to match the new port.

## Full Documentation

For detailed, step-by-step directions including complete code examples, controller implementations, and advanced configurations, refer to the official Syncfusion Blazor Pivot Table documentation. *(Documentation link will be added once published.)*
