# UI_STANDARD.md

## Dự án Sài Gòn Cửu Long

> Tài liệu quy chuẩn giao diện bắt buộc áp dụng cho toàn bộ màn hình trong hệ thống.

---

# 1. NGUYÊN TẮC CHUNG

## 1.1 Mục tiêu

* Đảm bảo toàn bộ giao diện thống nhất.
* Hạn chế mỗi màn hình một kiểu thiết kế khác nhau.
* Tăng khả năng bảo trì và mở rộng.
* Tất cả màn hình mới phải tuân thủ tài liệu này.

## 1.2 Công nghệ

* ASP.NET MVC 4.8
* Bootstrap 5
* jQuery
* AJAX
* Dapper
* Unity DI

---

# 2. TIÊU CHUẨN FORM DANH SÁCH (INDEX)

## Bố cục bắt buộc

Thứ tự hiển thị:

1. Header
2. Khung tìm kiếm
3. Grid dữ liệu
4. Pagination

```text
+--------------------------------+
| [+ Thêm mới]    TIÊU ĐỀ TRANG |
+--------------------------------+

+--------------------------------+
| Bộ lọc tìm kiếm                |
+--------------------------------+

+--------------------------------+
| Grid dữ liệu                   |
+--------------------------------+

+--------------------------------+
| Phân trang                     |
+--------------------------------+
```

---

## 2.1 Header

### Tiêu đề

Bắt buộc sử dụng:

```html
<h4 class="mb-0 fw-bold text-uppercase"
    style="color:#0b5b84;font-size:1.25rem;">
    @ViewBag.Title
</h4>
```

### Quy định

* Không icon.
* In hoa.
* In đậm.
* Màu #0b5b84.
* Cùng hàng với nút Thêm mới.

---

## 2.2 Nút Thêm mới

Chỉ hiển thị khi có quyền:

```csharp
PermissionHelper.HasPermission(
    "Employee",
    LoaiPhanQuyen.Them
)
```

Không được disable.

Không có quyền => Ẩn hoàn toàn.

---

# 3. KHUNG TÌM KIẾM

## Bắt buộc

Mọi màn hình danh sách đều phải có vùng filter.

Cấu trúc:

```html
<div class="card shadow-sm mb-4">
    <div class="card-body">
        <form id="searchForm">
            ...
        </form>
    </div>
</div>
```

---

## Nút Tìm kiếm

```html
<button class="btn text-white"
        style="background:#2998e4">
    <i class="bi bi-search"></i>
    Tìm kiếm
</button>
```

### Quy định

* Màu #2998e4
* Chữ trắng
* Có icon kính lúp

---

## Nút Làm mới

```html
<a class="btn text-white"
   style="background:#f1c40f">
   Làm mới
</a>
```

### Quy định

* Màu vàng
* Không icon

---

# 4. GRID DỮ LIỆU

## Wrapper

```html
<div class="table-responsive"
     style="min-height:350px;">
```

---

## Table

```html
<table class="table-custom table-bordered">
```

---

## Header

### Quy định

* Nền: #f4f6f9
* Chữ đen
* In đậm
* Căn trái

```css
background:#f4f6f9;
font-weight:bold;
text-align:left;
```

---

## Màu dòng

### Dòng lẻ

```css
background:#ffffff;
```

### Dòng chẵn

```css
background:#dbe6ef;
```

### Hover

```css
background:#c9d9e8;
```

---

## Canh dữ liệu

### Center

* Mã
* STT
* Ngày sinh
* Giới tính
* Điện thoại

```html
class="text-center"
```

### Right

* Số lượng
* Đơn giá
* Thành tiền
* Tiền thuế
* Tổng tiền

```html
class="text-end"
```

### Left

* Tên
* Địa chỉ
* Ghi chú
* Email

---

# 5. CỘT THAO TÁC

## Vị trí

```text
Checkbox
↓
Thao tác
↓
STT
↓
Dữ liệu
```

---

## Nút menu

```html
<i class="bi bi-grid-3x3-gap-fill"></i>
```

---

## Dropdown

Bắt buộc:

```html
<div class="dropdown dropdown-hover">
```

### Hover tự mở

Không click.

### 4 dòng cuối

Tự động DropUp bằng CSS.

Không code JS.

---

## Chức năng chuẩn

### Sửa

```html
bi-pencil-square
text-primary
```

### Xóa

```html
bi-trash-fill
text-danger
```

---

# 6. POPUP XÓA

## Không sử dụng

```javascript
confirm()
```

---

## Bắt buộc

```javascript
confirmDelete(url)
```

Ví dụ:

```html
<button onclick="confirmDelete(url)">
```

---

# 7. MODAL CHUẨN

## Cấu hình

```html
data-bs-backdrop="static"
data-bs-keyboard="false"
```

---

## Modal Form

Tiêu đề:

```html
<h5 class="modal-title fw-bold text-uppercase"
    style="color:#0b5b84;">
    @ViewBag.Title
</h5>
```

### Quy định

* In hoa
* In đậm
* Không icon

---

## Nút

### Đồng ý

```text
Nền vàng
Chữ trắng
```

### Để sau

```text
Nền xám
Chữ đen
```

---

# 8. TOAST NOTIFICATION

## Không sử dụng

```javascript
alert()
```

---

## Bắt buộc

```javascript
showToast(type,message)
```

### Success

```javascript
showToast('success','Lưu thành công');
```

### Error

```javascript
showToast('error','Lỗi hệ thống');
```

### Warning

```javascript
showToast('warning','Vui lòng chọn dữ liệu');
```

### Info

```javascript
showToast('info','Thông báo');
```

---

# 9. AJAX STANDARD

## Danh sách

Không reload toàn bộ trang.

Bắt buộc AJAX:

* Tìm kiếm
* Phân trang
* Đổi page size

---

## Hiệu ứng Loading khi tìm kiếm (Search Loading Overlay)

* Bắt buộc hiển thị spinner xoay tròn đa sắc (`custom-multi-spinner`) dạng overlay làm mờ trên vùng lưới dữ liệu trong suốt quá trình tải AJAX tìm kiếm.
* **Quy chuẩn thực hiện:**
  * Tránh việc tự viết bắt sự kiện `$form.on('submit')` cục bộ ở từng View rồi gọi hàm `$.ajax` thủ công kèm theo `e.preventDefault()`.
  * Thay vào đó, hãy để sự kiện submit form tự động nổi bọt (bubble) lên `document` để tập trung xử lý bởi [tab-manager.js](file:///c:/Users/duoc0/OneDrive/Desktop/WEB_QLBH/QuanLyBanHang/SalesManagementSystem/SalesManagementSystem/Scripts/tab-manager.js). Hệ thống quản lý tab sẽ tự động áp dụng hàm `ajaxLoadGrid` cùng hiệu ứng loading nháy mượt mà chuẩn tương tự màn hình Đơn đặt hàng.

---

## Controller

```csharp
if(Request.IsAjaxRequest())
{
    return PartialView("_List");
}
```

---

## View

Tách riêng:

```text
_EmployeeList.cshtml
```

---

# 10. FORM THÊM/SỬA

## Không điều hướng sang trang mới

Không dùng:

```html
<a href="/employee/create">
```

---

## Bắt buộc Modal

```javascript
openFormModal(url)
```

---

## GET

```csharp
return PartialView();
```

---

## POST Thành công

```csharp
return Json(new
{
    success = true,
    message = "Lưu thành công"
});
```

---

## POST Lỗi

```csharp
return PartialView(model);
```

---

# 11. VALIDATION

## Kiểm tra trùng mã

Áp dụng cho mọi danh mục.

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
ModelState.AddModelError(
    "MaNhanVien",
    "Mã đã tồn tại"
);
```

---

## View

```html
@Html.ValidationMessageFor(...)
```

---

# 12. PAGINATION

## Thành phần

### Trái

* First
* Prev
* Page Number
* Next
* Last

### Giữa

Page Size:

```text
10
20
50
100
```

### Phải

```text
1 - 10 của 200
```

Tính động.

Không hardcode.

---

## Icon

### First

```html
bi-skip-start-fill
```

### Prev

```html
bi-caret-left-fill
```

### Next

```html
bi-caret-right-fill
```

### Last

```html
bi-skip-end-fill
```

---

# 13. PHÂN QUYỀN GIAO DIỆN

## Không dùng số cứng

Sai:

```csharp
HasPermission("Employee",1)
```

Đúng:

```csharp
HasPermission(
    "Employee",
    LoaiPhanQuyen.Xem
)
```

---

## Nút phải kiểm tra quyền

* Xem
* Thêm
* Cập nhật
* Xóa
* Tùy chọn

Không có quyền => Ẩn hoàn toàn.

---

# 14. ROUTING

## Bắt buộc RouteConfig

Khai báo trước route Default.

Ví dụ:

```text
/nhan-vien
/nhan-vien/them-moi
/nhan-vien/cap-nhat
```

Không dùng:

```text
/Employee/Create
/Employee/Index
```

---

# 15. KIẾN TRÚC BACKEND

## Controller

Mỗi Entity một Controller riêng.

Ví dụ:

```text
EmployeeController
PhongBanController
ChucVuController
```

---

## Repository

Bắt buộc Interface.

```csharp
IEmployeeRepository
EmployeeRepository
```

---

## Dependency Injection

Unity Container.

```csharp
container.RegisterType<
    IEmployeeRepository,
    EmployeeRepository
>(
    new HierarchicalLifetimeManager()
);
```

---

# 16. NÚT XUẤT EXCEL

## Bắt buộc

Sử dụng định dạng thống nhất cho nút Xuất Excel:

```html
<button class="btn text-white"
        style="background:#2ecc71">
    <i class="bi bi-file-earmark-excel"></i>
    Xuất Excel
</button>
```

### Quy định

* Màu nền: #2ecc71
* Chữ trắng
* Có icon biểu tượng file Excel

---

# 17. CHECKLIST REVIEW UI

Trước khi hoàn thành màn hình phải kiểm tra:

□ Header đúng chuẩn

□ Có Filter

□ Có AJAX Search

□ Có AJAX Pagination

□ Có Modal Create

□ Có Modal Update

□ Có Modal Delete

□ Có Toast

□ Có phân quyền

□ Có kiểm tra trùng mã

□ Có RouteConfig

□ Có Repository Interface

□ Có DI Unity

□ Có ValidationMessageFor

□ Có Pagination chuẩn

□ Có cột STT

□ Có cột Thao tác

□ Có nút Xuất Excel đúng chuẩn

□ Không dùng alert()

□ Không dùng confirm()

□ Không reload trang khi tìm kiếm
