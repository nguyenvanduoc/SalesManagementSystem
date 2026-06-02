# Tiêu chuẩn thiết kế giao diện (UI Standard)

Tài liệu này ghi lại các tiêu chuẩn thiết kế giao diện cho dự án Quản Lý Bán Hàng, đảm bảo tính nhất quán trên toàn hệ thống.

## 1. Tiêu chuẩn Form Danh sách (Index)

Tất cả các trang danh sách (Index) cần tuân thủ cấu trúc sau:

1. **Header**: Tiêu đề trang và nút thêm mới nằm cùng một hàng.
2. **Khung tìm kiếm/lọc (Filter)**: Luôn luôn có một form chứa các điều kiện lọc ở phía trên bảng dữ liệu. Nó giúp người dùng dễ dàng tìm kiếm và thu hẹp dữ liệu hiển thị.
3. **Bảng dữ liệu (Table)**: Nằm bên dưới khung lọc, sử dụng thẻ bảng tiêu chuẩn với màu sắc nhẹ nhàng (ví dụ: `table-light` cho thead).

**Mẫu HTML tham khảo cho phần Filter:**
```html
<div class="card shadow-sm mb-4">
    <div class="card-body">
        <form method="get" action="@Url.Action("Index")">
            <div class="row g-3 align-items-center">
                <div class="col-md-4">
                    <input type="text" name="keyword" class="form-control" placeholder="Nhập từ khóa tìm kiếm..." value="@ViewBag.Keyword">
                </div>
                <div class="col-md-3">
                    <select name="status" class="form-select">
                        <option value="">-- Tất cả trạng thái --</option>
                        <option value="1">Đang hoạt động</option>
                        <option value="0">Ngừng hoạt động</option>
                    </select>
                </div>
                <div class="col-md-auto d-flex gap-2">
                    <button type="submit" class="btn text-white px-3" style="background-color: #2998e4; border: none;">
                        <i class="bi bi-search"></i> Tìm kiếm
                    </button>
                    <a href="@Url.Action("Index")" class="btn text-white px-3" style="background-color: #f1c40f; border: none;">
                        Làm mới
                    </a>
                </div>
            </div>
        </form>
    </div>
</div>
```

### Tiêu chuẩn Nút bấm trong Form lọc:
- **Tìm kiếm**: Nền màu xanh dương sáng (VD: `#2998e4`), chữ trắng, có icon tìm kiếm (kính lúp) bên trái chữ.
- **Làm mới**: Nền màu vàng (VD: `#f1c40f`), chữ trắng, không có icon, dùng để xóa các điều kiện lọc và tải lại trang mặc định.
- Cả hai nút đặt cạnh nhau (dùng `d-flex gap-2`).

## 2. Tiêu chuẩn Popup Xóa (Delete Confirmation)

Hệ thống không sử dụng hộp thoại `confirm()` mặc định của trình duyệt vì tính thẩm mỹ kém. Thay vào đó, sử dụng Global Modal của Bootstrap đã được thiết kế sẵn trong `_Layout.cshtml`.

#### 3.4 Phân trang (Pagination)
- **Giao diện (CSS)**: Sử dụng class `.custom-pagination` cho vùng footer chứa phân trang. Style này có nền màu xám xanh sáng (`#f1f5f9`), viền trên nhạt (`#e2e8f0`).
- **Nút bấm**: Nút bấm trang có dạng hình tròn (`border-radius: 50%`), nền trong suốt, màu chữ xanh xám đậm (`#0b5b84`), viền ngoài mảnh màu mờ (`#dce4ec`).
- **Nút đang chọn (Active)**: Nền xanh lơ (`#c0d6e4`), viền (`#b9d2e1`), chữ giữ màu xanh đậm (`#0b5b84`).
- **Icon điều hướng**: Dùng các icon filled đặc thay vì mũi tên đơn điệu:
  - Trang đầu tiên (First): `bi-skip-start-fill`
  - Trang trước (Prev): `bi-caret-left-fill`
  - Trang sau (Next): `bi-caret-right-fill`
  - Trang cuối cùng (Last): `bi-skip-end-fill`
- **Cấu trúc Footer**: Nằm ở dưới cùng của Grid, chứa điều hướng trang bên trái, dropdown chọn số dòng bên cạnh điều hướng, và tổng số lượng dòng ở bên phải.
- **Số liệu động**: Text hiển thị số dòng (ví dụ: `1 - 10 của 20`) phải được tính toán động dựa vào biến `Model.Count()` (hoặc tổng record từ DB) chứ không dùng text mặc định (hardcode).
- **Màu chữ phụ**: Các đoạn text như "mẫu tin/trang" hay tổng số dòng cần dùng màu xanh đậm chuẩn thông qua class `.text-info-custom`.

## 4. Quy chuẩn dữ liệu và Validation (Data Validation)

### 4.1 Kiểm tra trùng mã (Unique Code)
- **Bắt buộc đối với tất cả các đối tượng có Mã**: Khi Thêm mới (Create) hoặc Cập nhật (Update), bắt buộc phải kiểm tra trùng lặp "Mã" (VD: `MaNhanVien`, `MaKhachHang`, `MaHangHoa`...).
- **Cơ chế**:
  - Dưới backend (Repository), viết hàm kiểm tra dạng `IsDuplicateCode(string code, int currentId = 0)`.
  - Trong Controller, gọi hàm check trước khi Insert/Update. Nếu bị trùng, dùng `ModelState.AddModelError("Ma...", "Mã ... đã tồn tại trong hệ thống.");` và trả lại View.
  - Trên màn hình View, luôn sử dụng `@Html.ValidationMessageFor(m => m.Ma..., "", new { @class = "text-danger small" })` để hiển thị lỗi màu đỏ cho người dùng dễ nhận diện.

**Ví dụ:**
```html
<div class="card-footer custom-pagination d-flex flex-wrap align-items-center justify-content-between" style="padding: 12px 16px;">
    <div class="d-flex align-items-center gap-3">
        <nav aria-label="Page navigation">
            <ul class="pagination pagination-sm mb-0">
                <li class="page-item disabled"><a class="page-link" href="#"><i class="bi bi-chevron-left"></i></a></li>
                <li class="page-item active"><a class="page-link" href="#">1</a></li>
                <li class="page-item"><a class="page-link" href="#"><i class="bi bi-chevron-right"></i></a></li>
            </ul>
        </nav>
        <div class="d-flex align-items-center">
            <select class="form-select form-select-sm" style="width: 70px;">
                <option value="10" selected>10</option>
                <option value="20">20</option>
            </select>
            <span class="ms-2 text-info-custom" style="font-size: 0.85rem;">mẫu tin/trang</span>
        </div>
    </div>
    <div class="text-info-custom" style="font-size: 0.85rem;">
        @{
            var total = Model != null ? Model.Count() : 0;
            var start = total > 0 ? 1 : 0;
            var end = total > 10 ? 10 : total;
        }
        @start - @end của @total
    </div>
</div>
```

## 4. Modal (Popup):
- Giao diện sáng (Light mode) sạch sẽ.
- Cạnh trên (top-border) có đường viền màu vàng dày để nhấn mạnh cảnh báo.
- Tiêu đề: **Thông báo!** (chữ xám đen) cùng icon cảnh báo màu vàng.
- Nút bấm:
  - **ĐỒNG Ý**: Nền vàng, chữ trắng đậm (bên trái).
  - **ĐỂ SAU**: Nền xám nhạt, chữ đen đậm (bên phải).

### Cách sử dụng trong code:
Thay vì dùng:
```html
<form action="..." method="post" onsubmit="return confirm('Bạn có chắc chắn muốn xóa?');">
```
Hãy sử dụng hàm JS `confirmDelete(url)` đã được định nghĩa trong `_Layout.cshtml`:
```html
<button type="button" class="btn btn-sm btn-outline-danger" title="Xóa" onclick="confirmDelete('@Url.Action("Delete", "ControllerName", new { id = item.ID })')">
    <i class="bi bi-trash"></i>
</button>
```

Hệ thống sẽ tự động gọi Popup Xóa chuẩn và thực thi phương thức `POST` đến `url` nếu người dùng chọn "ĐỒNG Ý".

## 3. Tiêu chuẩn Bảng dữ liệu (Table Grid) & Cột Thao tác

### 3.1 Giao diện Bảng (Grid)
- Sử dụng class `.table-custom table-bordered` đã được định nghĩa toàn cục trong `_Layout.cshtml`.
- Dòng chẵn (even) sẽ có nền màu xanh nhạt (`#dbe6ef`), dòng lẻ (odd) nền trắng.
- Khi rê chuột (hover) vào một dòng, nền dòng sẽ chuyển sang màu xanh đậm hơn (`#c9d9e8`).
- **Tiêu đề cột (Header)**: Chữ màu đen (`#212529`) và được tô đậm (`font-weight: bold`), viền dưới dày 2px. **Tiêu đề (title) của các cột luôn luôn canh trái**.
- Các ô dữ liệu tự động canh giữa theo chiều dọc (`vertical-align: middle`), đường lưới mờ (`#e9ecef`).
- Sử dụng class `table-responsive` bao bọc bên ngoài table với `min-height: 350px` để chống vỡ khung và hiển thị tốt dropdown.
- **Canh lề dữ liệu (Alignment)**:
  - **Canh giữa (`text-center`)**: Các cột Mã, Ngày sinh, Giới tính, Số điện thoại.
  - **Canh phải (`text-end`)**: Các cột Số tiền, Số lượng, Đơn giá, Thành tiền (dữ liệu số/tiền tệ).
  - **Canh trái (mặc định)**: Cột Họ đệm, Tên nhân viên và các cột văn bản dài (Địa chỉ, Email, Ghi chú, Nội dung...).

### 3.2 Cột Thao tác (Action Column)
- **Vị trí**: Đặt ở bên trái, ngay sau cột Checkbox (nếu có) và trước các cột dữ liệu.
- **Hiệu ứng Hover**: Menu tự động hiển thị (Drop-down) khi rê chuột vào mà không cần click. Cần thêm class `.dropdown-hover` vào thẻ bao ngoài cùng của nút.
- **Xử lý các dòng cuối (Chống bị che khuất)**: Đối với 4 dòng cuối cùng của bảng, menu dropdown sẽ tự động mở ngược hất lên trên (Drop-up, đè ngược lên grid) để không bị cắt hoặc che khuất bởi footer/scroll nhờ CSS `.table-custom tbody tr:nth-last-child(-n+4) .dropdown-menu`. Không cần code thêm logic JS. Bóng đổ (shadow) cũng được lật ngược lên trên cho thẩm mỹ.
- **Icon chuẩn**: 
  - Nút hiển thị Menu: Dùng icon dạng hình vuông có chấm (`bi-grid-3x3-gap-fill`) với nền nhạt.
  - Sửa: Icon `bi-pencil-square`, màu `text-primary`.
  - Xóa: Icon `bi-trash-fill`, màu `text-danger`.
- **Dropdown Menu**: 
  - Có tiêu đề "THAO TÁC" in hoa, in đậm (`dropdown-header text-uppercase fw-bold text-dark`).
  - Phân cách bởi đường kẻ ngang (`dropdown-divider`).
  - Các chức năng được liệt kê kèm icon. Ví dụ: Chỉnh sửa (`bi-pencil-square`), Xóa (`bi-trash-fill` màu đỏ).

**Mẫu HTML tham khảo (Trích đoạn Row):**
```html
<td class="text-center align-middle">
    <div class="dropdown dropdown-hover">
        <button class="btn btn-sm btn-light border dropdown-toggle-hide-arrow shadow-sm" type="button" data-bs-toggle="dropdown" aria-expanded="false" style="padding: 2px 6px;">
            <i class="bi bi-grid-3x3-gap-fill text-dark"></i>
        </button>
        <ul class="dropdown-menu shadow">
            <li><h6 class="dropdown-header text-uppercase fw-bold text-dark">Thao tác</h6></li>
            <li><hr class="dropdown-divider"></li>
            <li>
                <a class="dropdown-item" href="...">
                    <i class="bi bi-pencil-square me-2 text-primary"></i> Chỉnh sửa
                </a>
            </li>
            <li>
                <button type="button" class="dropdown-item text-danger" onclick="confirmDelete('...')">
                    <i class="bi bi-trash-fill me-2 text-danger"></i> Xóa
                </button>
            </li>
        </ul>
    </div>
</td>
```

### 3.3 Tiêu chuẩn Phân trang (Pagination)
- **Vị trí**: Luôn đặt ở dưới cùng của bảng dữ liệu, nằm trong thẻ `.card-footer` của component bao bọc bảng.
- **Cấu trúc**: 
  - Bên trái: Các nút điều hướng trang (Trang đầu, Trước, 1, 2, 3..., Tiếp, Trang cuối). Dùng component `.pagination` của Bootstrap (với icon `bi-chevron-left/right` và `bi-chevron-double-left/right`).
  - Ở giữa (cạnh điều hướng): Dropdown chọn số lượng mẫu tin hiển thị trên 1 trang (mặc định các mốc `10`, `20`, `50`, `100`), kèm nhãn "mẫu tin/trang".
  - Bên phải: Dòng text hiển thị thông tin hiển thị hiện tại (Ví dụ: `1 - 10 của 1720`).
- **Màu sắc**: Màu xám nhạt tiêu chuẩn của text (`text-muted`), card footer nền trắng.

**Mẫu HTML tham khảo (Trích đoạn Footer):**
```html
<div class="card-footer bg-white border-top d-flex flex-wrap align-items-center justify-content-between" style="padding: 12px 16px;">
    <div class="d-flex align-items-center gap-3">
        <nav aria-label="Page navigation">
            <ul class="pagination pagination-sm mb-0">
                <li class="page-item disabled"><a class="page-link" href="#"><i class="bi bi-chevron-double-left"></i></a></li>
                <li class="page-item disabled"><a class="page-link" href="#"><i class="bi bi-chevron-left"></i></a></li>
                <li class="page-item active"><a class="page-link" href="#">1</a></li>
                <li class="page-item"><a class="page-link" href="#">2</a></li>
                <li class="page-item disabled"><a class="page-link" href="#">...</a></li>
                <li class="page-item"><a class="page-link" href="#"><i class="bi bi-chevron-right"></i></a></li>
                <li class="page-item"><a class="page-link" href="#"><i class="bi bi-chevron-double-right"></i></a></li>
            </ul>
        </nav>
        
        <div class="d-flex align-items-center">
            <select class="form-select form-select-sm" style="width: 70px; cursor: pointer;">
                <option value="10" selected>10</option>
                <option value="20">20</option>
                <option value="50">50</option>
                <option value="100">100</option>
            </select>
            <span class="ms-2 text-muted" style="font-size: 0.85rem;">mẫu tin/trang</span>
        </div>
    </div>
 
</div>
```

## 5. Thông báo Toast (Toast Notifications)
- Thay vì dùng `alert()` mặc định của trình duyệt gây trải nghiệm không tốt, hệ thống cung cấp chuẩn **Global Toast**.
- **Vị trí**: Nằm ở góc trên cùng bên phải (`top: 25px; right: 25px;`), không che khuất thao tác người dùng.
- **Phân loại**:
  - `success` (Xanh lá): Dùng khi thao tác thành công (Thêm, sửa, xóa...).
  - `error` (Đỏ): Dùng khi gặp lỗi hệ thống, lưu dữ liệu thất bại.
  - `warning` (Vàng): Dùng để cảnh báo (Ví dụ: "Vui lòng chọn ít nhất một nhân viên").
  - `info` (Đen/Xám): Thông tin thông thường.

### Cách sử dụng trong Javascript:
Hàm `showToast(type, message)` đã được tích hợp sẵn ở `_Layout.cshtml`, có thể gọi trực tiếp từ bất kỳ màn hình nào.

```javascript
// Hiển thị cảnh báo (Warning)
showToast('warning', 'Vui lòng chọn ít nhất một dữ liệu để thao tác.');

// Hiển thị thành công (Success)
showToast('success', 'Đã lưu thông tin nhân viên thành công.');

// Hiển thị lỗi (Error)
showToast('error', 'Có lỗi xảy ra trong quá trình xóa dữ liệu.');
```

## 6. Popup Modal (Cửa sổ thông báo / xác nhận)
- Tất cả các popup cảnh báo, xác nhận (như xác nhận xóa) bắt buộc phải cấu hình không được đóng khi click ra bên ngoài (static backdrop).
- Thay vì mất đi, khi người dùng click ra ngoài khoảng tối, Modal sẽ nháy nhẹ để báo hiệu người dùng cần phải tương tác bằng cách bấm nút.
- **Cấu hình HTML (Bootstrap)**: Thêm 2 thuộc tính `data-bs-backdrop="static"` và `data-bs-keyboard="false"` vào thẻ bao ngoài của Modal.
```html
<div class="modal fade" id="myModal" tabindex="-1" aria-hidden="true" data-bs-backdrop="static" data-bs-keyboard="false">
   ...
</div>
```

## 7. Tìm kiếm & Phân trang bằng AJAX (AJAX Search & Pagination)
- Khi thực hiện tìm kiếm hoặc phân trang, **không được load lại toàn bộ trang** (URL trên trình duyệt không thay đổi). Thay vào đó, phải gọi ngầm tới Controller (hiển thị trong tab Network) thông qua AJAX.
- **Backend (Controller)**: Cần kiểm tra request là dạng AJAX (`Request.IsAjaxRequest()`) thì trả về `PartialView` chứa riêng dữ liệu phần bảng `<table>`. Nếu không thì trả về toàn bộ trang `View`.
- **Frontend (View)**: Tách phần bảng dữ liệu ra thành 1 Partial View riêng (ví dụ: `_EmployeeList.cshtml`). Sử dụng jQuery AJAX chặn các sự kiện submit của `<form>` tìm kiếm, các nút bấm phân trang (`.ajax-link`), và select số lượng bản ghi/trang (`#pageSizeSelect`) để nạp lại dữ liệu và thay thế `html` của `#table-container`.

## 8. Form Thêm/Sửa bằng Modal (AJAX Form Modal)
- Toàn bộ các thao tác Thêm mới (Create) và Cập nhật (Update) **phải được hiển thị trong Popup Modal** thay vì chuyển hướng người dùng sang trang mới, nhằm không để lộ đường dẫn nội bộ (URL) và mang lại trải nghiệm liền mạch.
- **Backend (Controller)**:
  - Hàm `GET` (trước khi hiển thị form): Chỉ trả về nội dung HTML của form bằng `return PartialView()`.
  - Hàm `POST` (xử lý lưu dữ liệu): Nếu validate bị lỗi, trả về `return PartialView()` chứa các thông báo lỗi màu đỏ. Nếu lưu thành công, trả về JSON dạng: `return Json(new { success = true, message = "Thành công" });`.
- **Frontend (View)**:
  - Trong trang `Create.cshtml` và `Update.cshtml`, phải gỡ bỏ giao diện chung bằng `Layout = null;`.
  - Cấu trúc file phải tuân thủ chuẩn Bootstrap Modal: gồm `<div class="modal-header">`, `<div class="modal-body">`, và `<div class="modal-footer">`.
  - Thay vì dùng thẻ `<a href="...">` để chuyển trang, sử dụng thẻ `<button onclick="openFormModal('url_controller')">` để lấy HTML của form rồi chèn vào `globalFormModal` có sẵn ở file `_Layout.cshtml`. Hệ thống đã tự động lắng nghe và nạp lại form (nếu lỗi) hoặc đóng Modal và nạp lại danh sách (nếu thành công).

## 9. Tiêu chuẩn Kiến trúc mã nguồn Backend (Architecture Standard)

### 9.1 Controllers
- **Không sử dụng Controller dùng chung** cho nhiều danh mục (Ví dụ: không dùng `DanhMucController` chung cho Chức vụ và Phòng ban).
- Mỗi bảng/đối tượng (Entity) phải có một Controller quản lý riêng biệt (Ví dụ: `ChucVuController`, `PhongBanController`, `EmployeeController`) để đảm bảo tính Single Responsibility và dễ mở rộng phân quyền sau này.

### 9.2 Tầng Interface & Repository (Dependency Inversion)
- Hệ thống áp dụng thiết kế theo Interface-based để giảm sự phụ thuộc (Loose Coupling).
- **Repository**: Chứa toàn bộ logic truy vấn SQL (sử dụng Dapper). Các class Repository (VD: `ChucVuRepository`) **bắt buộc** phải kế thừa từ một Interface tương ứng (VD: `IChucVuRepository`). Các Interface được đặt trong thư mục `Repositories/Interfaces/`.
- **Controller**: Khi khai báo Dependency Injection qua Constructor, Controller **chỉ được phép nhận Interface** thay vì nhận class trực tiếp.
  - Đúng: `public ChucVuController(IChucVuRepository repo)`
  - Sai: `public ChucVuController(ChucVuRepository repo)`

### 9.3 Dependency Injection (DI Container)
- Sử dụng **Unity Container** để quản lý Dependency Injection.
- Toàn bộ việc đăng ký mapping giữa Interface và Class thực thi phải được khai báo trong file `App_Start/UnityConfig.cs`.
- Cú pháp đăng ký chuẩn: `container.RegisterType<IChucVuRepository, ChucVuRepository>(new HierarchicalLifetimeManager());`
