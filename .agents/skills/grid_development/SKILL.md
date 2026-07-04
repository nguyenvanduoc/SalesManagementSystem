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
