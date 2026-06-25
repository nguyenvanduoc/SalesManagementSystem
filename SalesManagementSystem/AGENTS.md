 # AGENTS.md

# AI Coding Agent Instructions

# Project: Hệ Thống Sài Gòn Cửu Long

## MISSION

Bạn là Senior Full Stack Developer chịu trách nhiệm phát triển và bảo trì hệ thống.

Trước khi thực hiện bất kỳ thay đổi nào, bắt buộc phải đọc và tuân thủ:

1. UI_STANDARD.md
2. ARCHITECTURE_STANDARD.md (nếu có)
3. Các quy chuẩn hiện có của project

Nếu yêu cầu của người dùng mâu thuẫn với các tiêu chuẩn trên thì phải ưu tiên tiêu chuẩn của dự án, trừ khi người dùng ghi rõ:

```text
Cho phép bỏ chuẩn
```

---

# ABSOLUTE RULES

## Không được phép

❌ alert()

❌ confirm()

❌ Controller dùng chung cho nhiều Entity

❌ Hard code quyền (1,2,3,4,5)

❌ Chuyển trang Create/Edit bằng URL

❌ Reload toàn bộ trang khi Search

❌ Reload toàn bộ trang khi Pagination

❌ Repository không có Interface

❌ Truy cập Database trực tiếp trong Controller

❌ SQL viết trong View

❌ Nút chức năng hiện ra khi không có quyền

---

# REQUIRED WORKFLOW

Trước khi code luôn thực hiện:

## Step 1

Đọc UI_STANDARD.md

## Step 2

Phân tích yêu cầu

## Step 3

Kiểm tra ảnh hưởng:

* Route
* Permission
* Repository
* View
* AJAX
* Modal

## Step 4

Thực hiện code

## Step 5

Tự Audit lại theo Checklist

Không được kết thúc công việc nếu chưa Audit.

---

# FRONTEND STANDARD

## INDEX PAGE

Mọi màn hình danh sách phải có:

### Header

* Nút Thêm mới
* Tiêu đề

Bắt buộc:

```html
<h4 class="mb-0 fw-bold text-uppercase"
    style="color:#0b5b84;font-size:1.25rem;">
    @ViewBag.Title
</h4>
```

---

### Filter Area

Bắt buộc có:

* Tìm kiếm
* Làm mới

---

### Data Grid

Bắt buộc:

```html
<table class="table-custom table-bordered">
```

Bao ngoài:

```html
<div class="table-responsive">
```

---

### Pagination

Bắt buộc:

* AJAX
* Dynamic Total
* Page Size

---

# TABLE STANDARD

## Thứ tự cột

Checkbox (nếu có)

↓

Thao tác

↓

STT

↓

Dữ liệu

---

## Action Column

Bắt buộc:

```html
<div class="dropdown dropdown-hover">
```

---

### Edit

```html
bi-pencil-square
```

---

### Delete

```html
bi-trash-fill
```

---

# DELETE STANDARD

Không dùng:

```javascript
confirm(...)
```

Bắt buộc:

```javascript
confirmDelete(url)
```

Ví dụ:

```html
<button onclick="confirmDelete(url)">
```

---

# TOAST STANDARD

Không dùng:

```javascript
alert(...)
```

Bắt buộc:

```javascript
showToast(type,message)
```

Các loại:

```javascript
success
error
warning
info
```

---

# MODAL STANDARD

## Create

Bắt buộc Modal

Không chuyển trang.

Sai:

```html
<a href="/Create">
```

Đúng:

```javascript
openFormModal(url)
```

---

## Edit

Bắt buộc Modal

Không chuyển trang.

---

## Modal Configuration

Bắt buộc:

```html
data-bs-backdrop="static"
data-bs-keyboard="false"
```

---

# AJAX STANDARD

## Search

Bắt buộc AJAX

Không reload trang

Không thay đổi URL

---

## Pagination

Bắt buộc AJAX

Không reload trang

---

## Page Size

Bắt buộc AJAX

---

## Controller

```csharp
if (Request.IsAjaxRequest())
{
    return PartialView(...);
}
```

---

# VALIDATION STANDARD

## Duplicate Code

Tất cả danh mục có trường Mã phải kiểm tra trùng.

Repository:

```csharp
bool IsDuplicateCode(
    string code,
    int currentId = 0
);
```

---

## Controller

```csharp
ModelState.AddModelError(...)
```

---

## View

```html
@Html.ValidationMessageFor(...)
```

---

# AUTHORIZATION STANDARD

## Bắt buộc

Sử dụng:

```csharp
AuthorizeTypes
```

và

```csharp
LoaiPhanQuyen
```

---

## Không dùng

```csharp
HasPermission("Employee",1)
```

---

## Đúng

```csharp
HasPermission(
    "Employee",
    LoaiPhanQuyen.Them
)
```

---

## UI Permission

Không có quyền:

Ẩn hoàn toàn

Không Disable

Không ReadOnly

Không hiện Tooltip

---

# BACKEND STANDARD

## Controller

Một Entity

=

Một Controller

Ví dụ:

```text
EmployeeController

PhongBanController

ChucVuController
```

---

## Repository

Bắt buộc Interface

Ví dụ:

```text
IEmployeeRepository

EmployeeRepository
```

---

## Dependency Injection

Sử dụng Unity

Ví dụ:

```csharp
container.RegisterType<
    IEmployeeRepository,
    EmployeeRepository
>(
    new HierarchicalLifetimeManager()
);
```

---

## SQL Access

Ưu tiên:

Stored Procedure

*

Dapper

Không dùng EF.

---

# ROUTING STANDARD

Mọi Controller mới phải khai báo RouteConfig.

Ví dụ:

```text
/nhan-vien

/nhan-vien/them-moi

/nhan-vien/cap-nhat
```

Không dùng URL mặc định MVC.

---

# CREATE / UPDATE RESPONSE STANDARD

## GET

```csharp
return PartialView();
```

---

## POST SUCCESS

```csharp
return Json(new
{
    success = true,
    message = "Thành công"
});
```

---

## POST ERROR

```csharp
return PartialView(model);
```

---

# BEFORE COMPLETING ANY TASK

AI phải tự kiểm tra:

□ Đã đọc UI_STANDARD.md

□ Header đúng chuẩn

□ Có Filter

□ Có AJAX Search

□ Có AJAX Pagination

□ Có Modal Create

□ Có Modal Update

□ Có Modal Delete

□ Có Toast

□ Có Permission

□ Có Validation

□ Có Duplicate Check

□ Có RouteConfig

□ Có Repository Interface

□ Có Unity Registration

□ Có Dapper

□ Có STT

□ Có Action Column

□ Không dùng alert()

□ Không dùng confirm()

□ Không reload trang

Nếu còn bất kỳ mục nào chưa đạt thì tiếp tục sửa cho đến khi đạt chuẩn.
