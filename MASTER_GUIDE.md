# MASTER GUIDE — Antigravity Sales & Warehouse Management
> **Đọc file này trước khi viết BẤT KỲ dòng code nào.**  
> Mọi thành viên và AI đều phải tuân thủ 100% các quy tắc dưới đây.

---

## 1. Thông tin dự án

| Mục | Giá trị |
|---|---|
| **Tên project** | SalesManagementSystem |
| **Namespace gốc** | `SalesManagementSystem` |
| **Framework** | ASP.NET MVC 5 — **.NET Framework 4.8** |
| **Database** | SQL Server (LocalDB dev / SQL Server Express prod) |
| **ORM** | **Dapper** (KHÔNG dùng Entity Framework — tuyệt đối) |
| **DI Container** | **Unity 5.11.1 + Unity.Mvc5 1.4.0** (KHÔNG dùng Interface) |
| **Deploy** | IIS trên Windows PC nội bộ (LAN / Tailscale VPN) |

---

## 2. Triết lý Code "Antigravity" — Bắt buộc

```
✅ PHẢI làm                          ❌ KHÔNG được làm
─────────────────────────────────    ──────────────────────────────────
Dùng Dapper cho mọi truy vấn SQL     Dùng Entity Framework / LINQ to DB
Inject trực tiếp Concrete Class      Tạo Interface (IRepository, IService)
Giữ Controller cực mỏng (thin)       Đặt logic nghiệp vụ trong Controller
Mọi SQL nằm trong Repositories/      Viết SQL trong Service hoặc Controller
Dùng Transaction khi multi-step      Thực hiện nhiều bước mà không Transaction
Đăng ký DI trong UnityConfig.cs      Dùng new() trực tiếp trong Controller
```

---

## 3. Cấu trúc thư mục chuẩn

```
SalesManagementSystem/              ← Root của Web Project
│
├── 📁 App_Start/
│   ├── UnityConfig.cs              ← [QUAN TRỌNG] Đăng ký DI tất cả classes
│   ├── BundleConfig.cs
│   ├── FilterConfig.cs
│   └── RouteConfig.cs
│
├── 📁 Controllers/                 ← Thin Controllers: nhận request → gọi Service → trả View
│   ├── HomeController.cs
│   ├── ProductController.cs        ← [Tạo sau]
│   ├── OrderController.cs          ← [Tạo sau]
│   └── InventoryController.cs      ← [Tạo sau]
│
├── 📁 Data/
│   └── DbConnectionFactory.cs      ← Factory duy nhất tạo SqlConnection
│
├── 📁 Models/
│   ├── 📁 Entities/                ← POCO ánh xạ 1-1 với bảng SQL (không annotation EF)
│   │   ├── Category.cs
│   │   ├── Product.cs
│   │   ├── Inventory.cs
│   │   ├── InventoryTransaction.cs
│   │   ├── Order.cs
│   │   ├── OrderDetail.cs
│   │   └── User.cs
│   │
│   └── 📁 ViewModels/              ← Class gộp dữ liệu truyền ra View
│       ├── RevenueReportVM.cs
│       └── OrderFormVM.cs
│
├── 📁 Repositories/                ← NƠI DUY NHẤT chứa SQL và Dapper
│   ├── ProductRepository.cs
│   └── OrderRepository.cs
│
├── 📁 Services/                    ← Logic nghiệp vụ: validate, tính toán, gọi Repository
│   ├── InventoryService.cs
│   └── OrderService.cs
│
├── 📁 Views/                       ← Razor .cshtml, layout AdminLTE
│
├── Global.asax.cs                  ← Gọi UnityConfig.RegisterComponents() ĐẦU TIÊN
└── Web.config                      ← DefaultConnection string
```

---

## 4. Cơ sở dữ liệu — Schema SQL Server

```sql
-- Danh mục sản phẩm
Categories      (Id, Name, Description)

-- Sản phẩm
Products        (Id, CategoryId, Name, Sku, CostPrice, SellingPrice, Unit)

-- Tồn kho hiện tại
Inventory       (ProductId, Quantity, LastUpdated)

-- Lịch sử nhập/xuất kho
InventoryTransactions (Id, ProductId, TransactionType, Quantity, Date, UserId)
                       -- TransactionType: 'IN' | 'OUT'

-- Đơn hàng
Orders          (Id, OrderDate, TotalAmount, UserId, Status)
                 -- Status: 'Pending' | 'Completed' | 'Cancelled'

-- Chi tiết đơn hàng
OrderDetails    (Id, OrderId, ProductId, Quantity, UnitPrice, SubTotal)

-- Người dùng
Users           (Id, Username, FullName, Role)
                 -- Role: 'Admin' | 'Warehouse' | 'Sale'
```

---

## 5. Quy tắc code cho từng layer

### 5.1 DbConnectionFactory — KHÔNG THAY ĐỔI

```csharp
// Data/DbConnectionFactory.cs
// Luôn lấy connection string từ Web.config key "DefaultConnection"
public class DbConnectionFactory
{
    private readonly string _connectionString;
    public DbConnectionFactory()
    {
        _connectionString = ConfigurationManager
            .ConnectionStrings["DefaultConnection"].ConnectionString;
    }
    public SqlConnection CreateConnection() => new SqlConnection(_connectionString);
}
```

### 5.2 Repository — Template chuẩn

```csharp
// Repositories/XxxRepository.cs
namespace SalesManagementSystem.Repositories
{
    public class XxxRepository                         // ← Không có IXxxRepository
    {
        private readonly DbConnectionFactory _db;

        public XxxRepository(DbConnectionFactory db)  // ← Constructor injection
        {
            _db = db;
        }

        public IEnumerable<Xxx> GetAll()
        {
            const string sql = "SELECT ... FROM ...";
            using (var conn = _db.CreateConnection())  // ← Luôn dùng using
                return conn.Query<Xxx>(sql);
        }

        // Multi-step → PHẢI dùng Transaction
        public void MultiStepOperation(...)
        {
            using (var conn = _db.CreateConnection())
            {
                conn.Open();
                using (var tx = conn.BeginTransaction())
                {
                    try   { /* ... */ tx.Commit(); }
                    catch { tx.Rollback(); throw; }
                }
            }
        }
    }
}
```

### 5.3 Service — Template chuẩn

```csharp
// Services/XxxService.cs
namespace SalesManagementSystem.Services
{
    public class XxxService                            // ← Không có IXxxService
    {
        private readonly XxxRepository _repo;
        private readonly DbConnectionFactory _db;      // ← Chỉ inject nếu cần query phụ

        public XxxService(XxxRepository repo, DbConnectionFactory db)
        {
            _repo = repo;
            _db = db;
        }

        // Đặt logic nghiệp vụ, validate tại đây
        // KHÔNG đặt SQL trực tiếp (trừ query nhỏ tính toán)
    }
}
```

### 5.4 Controller — Thin Controller chuẩn

```csharp
// Controllers/XxxController.cs
namespace SalesManagementSystem.Controllers
{
    public class XxxController : Controller
    {
        private readonly XxxService _service;          // ← Inject Service, không inject Repo

        public XxxController(XxxService service)
        {
            _service = service;
        }

        public ActionResult Index()
        {
            var data = _service.GetAll();              // ← Chỉ gọi Service, không logic
            return View(data);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(XxxViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);
            _service.Create(vm);
            return RedirectToAction("Index");
        }
    }
}
```

### 5.5 UnityConfig — Quy tắc đăng ký DI

```csharp
// App_Start/UnityConfig.cs
// MỖI KHI thêm Repository hoặc Service mới → PHẢI đăng ký tại đây

container.RegisterType<DbConnectionFactory>(new HierarchicalLifetimeManager());

// Repositories
container.RegisterType<ProductRepository>(new HierarchicalLifetimeManager());
container.RegisterType<OrderRepository>(new HierarchicalLifetimeManager());
// ← Thêm Repository mới vào đây

// Services
container.RegisterType<InventoryService>(new HierarchicalLifetimeManager());
container.RegisterType<OrderService>(new HierarchicalLifetimeManager());
// ← Thêm Service mới vào đây
```

> **`HierarchicalLifetimeManager`** = Scoped per HTTP Request (tương đương `AddScoped` trong .NET Core)

---

## 6. NuGet Packages — Danh sách chính thức

| Package | Version | Lý do |
|---|---|---|
| `Dapper` | **2.1.35** | Micro-ORM thay EF |
| `Unity` | **5.11.1** | IoC Container |
| `Unity.Mvc5` | **1.4.0** | Tích hợp Unity vào MVC |
| `Microsoft.AspNet.Mvc` | 5.2.9 | ASP.NET MVC 5 |
| `bootstrap` | 5.2.3 | UI framework |
| `jQuery` | 3.7.0 | JavaScript |

> **KHÔNG cài thêm** `EntityFramework`, `AutoMapper`, hay bất kỳ thư viện DI nào khác.

---

## 7. Web.config — Connection String

```xml
<connectionStrings>
  <add name="DefaultConnection"
       connectionString="Data Source=.;Initial Catalog=SalesWarehouseDB;Integrated Security=True;"
       providerName="System.Data.SqlClient" />
</connectionStrings>
```

- **Development:** `Data Source=.` (LocalDB / SQL Server Express local)
- **Production (IIS):** Đổi thành `Data Source=TEN_MAY_CHU` hoặc IP nội bộ
- **Key name luôn là `DefaultConnection`** — không đổi tên

---

## 8. Checklist khi thêm tính năng mới

Khi thêm module mới (ví dụ: **Supplier** — Nhà cung cấp):

```
[ ] 1. Tạo bảng SQL:      CREATE TABLE Suppliers (...)
[ ] 2. Tạo Entity:        Models/Entities/Supplier.cs
[ ] 3. Tạo ViewModel:     Models/ViewModels/SupplierFormVM.cs  (nếu cần)
[ ] 4. Tạo Repository:    Repositories/SupplierRepository.cs
[ ] 5. Tạo Service:       Services/SupplierService.cs          (nếu có nghiệp vụ)
[ ] 6. Đăng ký DI:        App_Start/UnityConfig.cs             ← KHÔNG ĐƯỢC BỎ QUÊN
[ ] 7. Tạo Controller:    Controllers/SupplierController.cs
[ ] 8. Tạo Views:         Views/Supplier/Index.cshtml, Create.cshtml, Edit.cshtml
[ ] 9. Cập nhật .csproj:  Thêm <Compile Include="..."> cho file mới
```

---

## 9. Luồng dữ liệu (Data Flow)

```
[View / HTTP Request]
        │
        ▼
[Controller]  ←── Unity inject Service
        │ gọi
        ▼
[Service]     ←── Unity inject Repository + DbConnectionFactory
        │ gọi
        ▼
[Repository]  ←── Unity inject DbConnectionFactory
        │ dùng
        ▼
[DbConnectionFactory] → SqlConnection → SQL Server (SalesWarehouseDB)
```

---

## 10. Quy ước đặt tên

| Loại | Quy ước | Ví dụ |
|---|---|---|
| Entity | `PascalCase`, số ít | `Product`, `OrderDetail` |
| Repository | `{Entity}Repository` | `ProductRepository` |
| Service | `{Domain}Service` | `OrderService`, `InventoryService` |
| ViewModel | `{Purpose}VM` | `RevenueReportVM`, `OrderFormVM` |
| Controller | `{Entity}Controller` | `ProductController` |
| View folder | Trùng tên Controller | `Views/Product/` |
| SQL params | `@CamelCase` | `@ProductId`, `@FromDate` |

---

## 11. Mẫu SQL Dapper thường dùng

```csharp
// Query nhiều bản ghi
conn.Query<Product>("SELECT * FROM Products WHERE CategoryId = @Id", new { Id = id });

// Query 1 bản ghi
conn.QueryFirstOrDefault<Product>("SELECT * FROM Products WHERE Id = @Id", new { Id = id });

// Insert lấy ID vừa tạo
int newId = conn.ExecuteScalar<int>(
    "INSERT INTO Products (...) VALUES (...); SELECT CAST(SCOPE_IDENTITY() AS INT)", obj);

// Update / Delete
conn.Execute("UPDATE Products SET Name = @Name WHERE Id = @Id", new { Name = "...", Id = 1 });

// JOIN → bind vào property phụ (CategoryName, UserFullName)
conn.Query<Product>("SELECT p.*, c.Name AS CategoryName FROM Products p JOIN Categories c ON ...");
```

---

*Cập nhật lần cuối: 2026-06-02*
