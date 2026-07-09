---
name: "Table Grid Development Convention"
description: "Quy tắc khi tạo mới các màn hình dạng lưới (grid/table) để có chức năng kéo thả độ rộng cột."
---

# Quy tắc tạo màn hình danh sách (Grid/Table)

Khi tạo mới bất kỳ màn hình nào có chứa bảng dữ liệu (grid/table), BẮT BUỘC phải tuân thủ quy tắc sau để hỗ trợ tính năng kéo thả thay đổi kích thước độ rộng cột (resize column).

## 1. Cấu trúc Table
- Thẻ `<table>` phải có class `table-custom`.
- BẮT BUỘC phải cung cấp một `id` duy nhất cho table, ví dụ `id="tblTenManHinh"`.
- Nên cung cấp style `min-width` phù hợp (ví dụ: `style="width:100%; min-width:1000px;"`).

Ví dụ:
```html
<table id="tblDanhMuc" class="table table-custom table-bordered table-hover mb-0" style="width:100%; min-width:1000px;">
```

## 2. Bổ sung Style và Script Resizer
Ở cuối file view (thường là `_List.cshtml`), BẮT BUỘC chèn đoạn style và script dưới đây để kích hoạt tính năng resize cột. Cần thay thế `#tblTenManHinh` và `tblTenManHinh` bằng `id` thực tế của bảng.

```html
<style>
    #tblTenManHinh th {
        position: relative;
        background-clip: padding-box;
    }
    #tblTenManHinh th .resizer {
        position: absolute;
        top: 0;
        right: -3px;
        width: 6px;
        height: 100%;
        cursor: col-resize;
        user-select: none;
        z-index: 10;
        background-color: transparent;
    }
    #tblTenManHinh th .resizer:hover, #tblTenManHinh th .resizer.resizing {
        background-color: rgba(11, 91, 132, 0.5);
    }
</style>
<script>
    (function() {
        var table = document.getElementById('tblTenManHinh');
        if (!table) return;
        
        var ths = table.querySelectorAll('th');
        ths.forEach(function(th) {
            // Bỏ qua các cột chứa icon, checkbox (chiều rộng nhỏ hơn 50px)
            if(th.innerText.trim() === '' && (th.style.width === '35px' || th.style.width === '40px' || th.style.width === '50px')) return;
            // Bỏ qua cột Thao tác và STT
            if(th.innerText.trim() === 'Thao tác' || th.innerText.trim() === 'STT') return;
            
            // Tránh add trùng resizer nếu đã có
            if(th.querySelector('.resizer')) return;

            var resizer = document.createElement('div');
            resizer.classList.add('resizer');
            th.appendChild(resizer);

            var startX, startWidth;
            
            resizer.addEventListener('mousedown', function(e) {
                startX = e.pageX;
                startWidth = th.offsetWidth;
                resizer.classList.add('resizing');

                var doDrag = function(e) {
                    var newWidth = startWidth + (e.pageX - startX);
                    if(newWidth > 30) {
                        th.style.width = newWidth + 'px';
                        th.style.minWidth = newWidth + 'px';
                        th.style.maxWidth = newWidth + 'px';
                    }
                };

                var stopDrag = function(e) {
                    resizer.classList.remove('resizing');
                    document.removeEventListener('mousemove', doDrag);
                    document.removeEventListener('mouseup', stopDrag);
                };

                document.addEventListener('mousemove', doDrag);
                document.addEventListener('mouseup', stopDrag);
                
                e.preventDefault();
            });
        });
    })();
</script>
```

**Lưu ý:** Chắc chắn rằng biến chuỗi cho ID truyền vào trong đoạn script trùng khớp với thẻ HTML `table` id.

## 3. Chức năng Làm mới (Reset) và Tìm kiếm

Đối với các màn hình danh sách có form lọc/tìm kiếm dữ liệu:
- Khi người dùng click vào nút **Làm mới** (Clear/Reset), hệ thống không chỉ phải xóa các điều kiện lọc (clear form) mà còn phải **TỰ ĐỘNG** thực hiện lại lệnh tìm kiếm để tải lại danh sách dữ liệu ban đầu (dữ liệu không có bộ lọc).
- Tránh tình trạng nút Làm mới chỉ xóa trắng các trường nhập liệu nhưng bảng dữ liệu bên dưới vẫn giữ nguyên kết quả tìm kiếm cũ hoặc bị biến mất.

**Mã Javascript chuẩn (Ví dụ):**
```javascript
$container.find('.btn-reset').on('click', function() {
    // 1. Reset toàn bộ form
    $form[0].reset();
    
    // Nếu có sử dụng thư viện Select2, cần reset riêng giao diện Select2
    $form.find('.select2').val('').trigger('change');
    
    // 2. Tự động trigger sự kiện submit form hoặc gọi lại hàm loadData() 
    // để tìm kiếm và tải lại dữ liệu ban đầu.
    $form.trigger('submit'); 
});
```

## 4. Quy tắc đồng bộ cơ chế Loading trên lưới (Grid/Table Loader)

Để đảm bảo hiệu ứng loading tự động hiển thị mượt mà ở trung tâm lưới dữ liệu mà không làm mờ/khóa các bộ lọc tìm kiếm ở trên, tất cả các màn hình BẮT BUỘC phải tuân thủ cấu trúc HTML và thiết kế sau:

### A. Cấu trúc HTML chuẩn của một Tab Danh sách
Vùng chứa bảng/lưới dữ liệu BẮT BUỘC phải được đặt trong một thẻ `div` bao ngoài có một trong các ID hoặc Class chuẩn:
- ID chuẩn: `id="listContainer"` hoặc `id="table-container"` hoặc `id="gridData"`.
- Class chuẩn: `class="grid-container"` hoặc `class="table-container"` hoặc `class="table-responsive"`.

**Ví dụ cấu trúc chuẩn:**
```html
<div id="tenManHinh-container">
    <!-- 1. Bộ lọc tìm kiếm đầu trang -->
    <div class="card shadow-sm mb-3">
        <form id="searchForm" action="...">
            ...
        </form>
    </div>

    <!-- 2. Vùng chứa lưới dữ liệu (Sẽ tự động phủ Loading khi tải AJAX) -->
    <div id="table-container" class="grid-container table-responsive">
        @Html.Partial("_ListPartial", Model)
    </div>
</div>
```

### B. Cơ chế vận hành toàn cục (Global AJAX Loader)
Hệ thống sử dụng sự kiện AJAX toàn cục của jQuery (`ajaxSend` và `ajaxComplete` đếm request tại `_Layout.cshtml`) để tự động tìm vùng lưới của Tab active và phủ vòng xoay đa sắc lên đó:
- **Tự động bắt**: Bất kỳ request AJAX nào được gửi đi (tìm kiếm, reload, phân trang, sắp xếp) sẽ tăng biến đếm `loadingRequestCount` và hiển thị vòng xoay đa sắc ở chính giữa lưới.
- **Không nhấp nháy**: Đảm bảo hiển thị tối thiểu **500ms** để tránh mỏi mắt cho người dùng.
- **Không làm mờ hết màn hình**: Chỉ làm mờ đúng vùng dữ liệu bảng, người dùng vẫn có thể thao tác ở sidebar, topbar hoặc chuyển tab khác bình thường.

### C. Trường hợp tự viết AJAX riêng (Local AJAX)
Nếu màn hình danh sách có logic tự gọi AJAX riêng không qua TabManager (ví dụ: màn hình Nhập kho có hàm submit tự viết), BẮT BUỘC phải:
- Để mặc định `global: true` trong cấu hình `$.ajax` để kích hoạt Loading tự động.
- Nếu cần can thiệp hoặc hiển thị thủ công, hãy gọi cặp hàm toàn cục:
  - `showLoading()`: Gọi trước khi gửi request AJAX.
  - `hideLoading()`: Gọi bên trong cả hai callback `success` và `error` để tắt vòng xoay.

**Mã mẫu tự viết AJAX chuẩn:**
```javascript
$form.on('submit', function (e) {
    e.preventDefault();
    var url = $(this).attr('action') + '?' + $(this).serialize();
    
    // Gọi hiển thị loading trước khi tải
    if (typeof showLoading === 'function') showLoading();
    
    $.ajax({
        url: url,
        type: 'GET',
        success: function (res) {
            // Tắt loading và cập nhật HTML
            if (typeof hideLoading === 'function') hideLoading();
            $('#table-container').html(res);
        },
        error: function () {
            // Tắt loading kể cả khi lỗi
            if (typeof hideLoading === 'function') hideLoading();
            showToast('error', 'Lỗi tải dữ liệu');
        }
    });
});
```
