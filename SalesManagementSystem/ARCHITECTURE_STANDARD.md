# ARCHITECTURE_STANDARD.md

# Architecture Standard

# ERP - Sales Management System

## PURPOSE

Tài liệu này quy định kiến trúc bắt buộc cho toàn bộ hệ thống.

Mọi lập trình viên và AI Coding Agent phải tuân thủ 100%.

Nếu có xung đột giữa yêu cầu người dùng và tài liệu này thì ưu tiên tài liệu này, trừ khi có chỉ định rõ:

```text
Cho phép bỏ chuẩn
```

---

# 1. TECHNOLOGY STACK

## Framework

```text
ASP.NET MVC 4.8
```

## Database

```text
SQL Server
```

## Data Access

```text
Dapper
```

## Dependency Injection

```text
Unity
Unity.Mvc5
```

## Frontend

```text
Bootstrap 5
jQuery
Ajax
```

---

# 2. FORBIDDEN TECHNOLOGIES

Tuyệt đối không sử dụng:

❌ Entity Framework

❌ Generic Repository

❌ Generic Service

❌ SQL trong Controller

❌ SQL trong View

❌ Business Logic trong View

❌ Business Logic trong Controller

❌ Hard Code Connection String

❌ Hard Code Permission Number

---

# 3. SYSTEM ARCHITECTURE

Luồng chuẩn:

```text
View
 ↓
Controller
 ↓
Repository Interface
 ↓
Repository
 ↓
Store Procedure
 ↓
SQL Server
```

Chỉ tạo Service khi thực sự có nghiệp vụ phức tạp.

Không tạo Service chỉ để gọi Repository.

---

# 4. FOLDER STRUCTURE

```text
/App_Start

/Controllers

/Models

/ViewModels

/Repositories
    /Interfaces

/Views

/Helpers

/Scripts

/Content
```

---

# 5. CONTROLLER STANDARD

## Nguyên tắc

Controller phải mỏng (Thin Controller).

Controller chỉ được:

* Nhận Request
* Validate cơ bản
* Gọi Repository
* Trả về View hoặc Json

Không được:

* Viết SQL
* Viết Business Logic
* Tính toán nghiệp vụ phức tạp

---

## Ví dụ đúng

```csharp
public class KhoHangController : Controller
{
    private readonly IKhoHangRepository _repository;

    public KhoHangController(
        IKhoHangRepository repository)
    {
        _repository = repository;
    }
}
```

---

## Ví dụ sai

```csharp
public class KhoHangController : Controller
{
    private readonly KhoHangRepository _repository;
}
```

Không inject Concrete Class.

---

# 6. REPOSITORY STANDARD

## Bắt buộc Interface

Ví dụ:

```text
IKhoHangRepository
KhoHangRepository
```

---

## Nơi duy nhất được phép truy cập Database

```text
Repositories
```

---

## Nơi duy nhất được phép viết SQL

```text
Store Procedure
```

hoặc

```text
Repository
```

---

## Không được

❌ SQL trong Controller

❌ SQL trong View

❌ SQL trong Helper

---

# 7. STORE PROCEDURE STANDARD

Ưu tiên Store Procedure.

Ví dụ:

```sql
sp_KhoHang_GetAll

sp_KhoHang_GetById

sp_KhoHang_Insert

sp_KhoHang_Update

sp_KhoHang_Delete
```

---

## Quy tắc đặt tên

```text
sp_[TenBang]_[Action]
```

Ví dụ:

```text
sp_NhanVien_GetAll

sp_NhanVien_Insert

sp_NhanVien_Update

sp_NhanVien_Delete
```

---

# 8. UNITY DEPENDENCY INJECTION

## Bắt buộc

Mọi Repository phải đăng ký trong:

```text
App_Start/UnityConfig.cs
```

Ví dụ:

```csharp
container.RegisterType<
    IKhoHangRepository,
    KhoHangRepository
>(
    new HierarchicalLifetimeManager()
);
```

---

## Khi tạo Repository mới

Phải cập nhật:

```text
UnityConfig.cs
```

Không được bỏ qua.

---

# 9. VIEWMODEL STANDARD

## Không truyền Entity trực tiếp ra View

Sai:

```csharp
return View(entity);
```

---

## Đúng

```csharp
return View(viewModel);
```

---

## Quy tắc đặt tên

```text
KhoHangViewModel

NhanVienViewModel

PagedListViewModel
```

hoặc

```text
KhoHangVM

NhanVienVM
```

---

# 10. ROUTING STANDARD

## Bắt buộc Explicit Route

Khai báo trong:

```text
RouteConfig.cs
```

---

## Ví dụ

```text
/kho-hang

/nhan-vien

/phan-quyen

/don-dat-hang
```

---

## Không dùng

```text
/KhoHang/Index

/NhanVien/Create
```

---

# 11. AUTHORIZATION STANDARD

## Bắt buộc

Sử dụng:

```csharp
AuthorizeTypes
```

---

## Phân quyền

Sử dụng:

```csharp
LoaiPhanQuyen
```

---

## Không được

```csharp
HasPermission("KhoHang",1);
```

---

## Đúng

```csharp
HasPermission(
    "KhoHang",
    LoaiPhanQuyen.Xem
);
```

---

# 12. DATABASE STANDARD

## Mọi bảng phải có

```sql
ID
NgayTao
NguoiTao
NgayCapNhat
NguoiCapNhat
```

---

## Nếu có trường Mã

Bắt buộc kiểm tra trùng.

Ví dụ:

```sql
MaNhanVien

MaKhoHang

MaKhachHang

MaHangHoa
```

---

# 13. AUDIT LOG STANDARD

Các thao tác sau phải ghi log:

```text
Đăng nhập

Thêm

Cập nhật

Xóa

Duyệt

Hủy
```

---

# 14. ERROR HANDLING STANDARD

Không được:

```csharp
catch(Exception)
{
}
```

---

Bắt buộc:

```csharp
catch(Exception ex)
{
    throw;
}
```

hoặc ghi log.

Không swallow exception.

---

# 15. AJAX STANDARD

Bắt buộc sử dụng AJAX cho:

```text
Search

Pagination

Page Size

Create

Update
```

Không reload toàn bộ trang.

---

# 16. NAMING CONVENTION

## Controller

```text
KhoHangController

NhanVienController
```

---

## Repository

```text
KhoHangRepository

NhanVienRepository
```

---

## Interface

```text
IKhoHangRepository

INhanVienRepository
```

---

## ViewModel

```text
KhoHangViewModel

KhoHangVM
```

---

## Action

```text
Index

Create

Update

Delete

GetData
```

---

# 17. NEW MODULE CHECKLIST

Khi tạo module mới:

□ Tạo bảng SQL

□ Tạo Store Procedure

□ Tạo Entity

□ Tạo ViewModel

□ Tạo Interface

□ Tạo Repository

□ Đăng ký Unity

□ Tạo Controller

□ Cập nhật RouteConfig

□ Tạo View

□ Kiểm tra Permission

□ Kiểm tra Duplicate Code

□ Kiểm tra Audit Log

□ Kiểm tra AJAX

□ Kiểm tra UI_STANDARD.md

---

# 18. COMPLETION CHECKLIST

Trước khi hoàn thành bất kỳ task nào AI phải tự kiểm tra:

□ Tuân thủ AGENTS.md

□ Tuân thủ UI_STANDARD.md

□ Tuân thủ ARCHITECTURE_STANDARD.md

□ Không dùng Entity Framework

□ Không SQL trong Controller

□ Có Interface Repository

□ Có Unity Registration

□ Có RouteConfig

□ Có Authorization

□ Có Duplicate Check

□ Có AJAX

□ Có Audit Log

□ Có Error Handling

Nếu còn bất kỳ mục nào chưa đạt thì tiếp tục sửa cho đến khi đạt chuẩn.
