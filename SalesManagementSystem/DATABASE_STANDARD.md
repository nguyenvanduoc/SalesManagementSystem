Hãy tạo file DATABASE_STANDARD.md cho dự án ERP ASP.NET MVC 4.8.

Mục tiêu:
Tài liệu này là tiêu chuẩn Database bắt buộc để AI Coding Agent, Developer và DBA tuân thủ khi tạo mới hoặc chỉnh sửa Database.

Yêu cầu:

# 1. Công nghệ

* SQL Server
* Dapper
* ASP.NET MVC 4.8
* Unity DI

# 2. Quy chuẩn Primary Key

Tất cả bảng bắt buộc sử dụng:

```sql
ID INT IDENTITY(1,1) NOT NULL PRIMARY KEY
```

Không sử dụng:

* UNIQUEIDENTIFIER
* Composite Primary Key
* BIGINT làm khóa chính

trừ khi có yêu cầu đặc biệt.

# 3. Quy chuẩn Audit Column

Tất cả bảng nghiệp vụ bắt buộc có:

```sql
NgayTao DATETIME NULL,
NguoiTao INT NULL,
NgayCapNhat DATETIME NULL,
NguoiCapNhat INT NULL
```

AI phải tự động thêm 4 cột này khi tạo bảng mới.

# 4. Quy chuẩn Foreign Key

Hệ thống KHÔNG sử dụng Foreign Key vật lý.

Không tạo:

```sql
CONSTRAINT FK_...
FOREIGN KEY(...)
```

Không tạo:

```sql
ALTER TABLE
ADD CONSTRAINT FK_...
```

Không tạo Cascade Delete.

Quan hệ dữ liệu được quản lý bằng:

* ID tham chiếu
* Repository
* Business Logic
* Validation tầng ứng dụng

Ví dụ:

```sql
IDKhoHang INT NULL,
IDPhongBan INT NULL,
IDNhanVien INT NULL
```

Chỉ lưu ID tham chiếu.

Không tạo Foreign Key.

# 5. Quy chuẩn đặt tên bảng

Sử dụng PascalCase.

Ví dụ:

```text
DM_KhoHang
DM_NhanVien
DM_PhongBan
DM_KhachHang
DM_HangHoa
```

Không dùng:

```text
tblKhoHang
tbl_NhanVien
tbNhanVien
```

# 6. Quy chuẩn trường Mã

Mọi bảng danh mục phải có trường Mã.

Ví dụ:

```sql
MaKhoHang NVARCHAR(100)
MaNhanVien NVARCHAR(100)
MaKhachHang NVARCHAR(100)
MaHangHoa NVARCHAR(100)
```

Backend bắt buộc kiểm tra trùng mã khi Insert và Update.

Không phụ thuộc hoàn toàn vào Database.

# 7. Quy chuẩn kiểu dữ liệu

Chuỗi:

```sql
NVARCHAR
```

Tiền:

```sql
DECIMAL(18,2)
```

Ngày:

```sql
DATETIME
```

Không dùng:

```sql
FLOAT
VARCHAR lưu tiếng Việt
VARCHAR lưu ngày tháng
```

# 8. Quy chuẩn Store Procedure

Ưu tiên sử dụng Store Procedure cho CRUD.

Quy tắc đặt tên:

```text
sp_[TenBang]_[Action]
```

Ví dụ:

```text
sp_DM_KhoHang_GetAll
sp_DM_KhoHang_GetById
sp_DM_KhoHang_Insert
sp_DM_KhoHang_Update
sp_DM_KhoHang_Delete
```

# 9. Quy chuẩn Transaction

Các nghiệp vụ:

* Nhập kho
* Xuất kho
* Đơn hàng
* Phiếu thu
* Phiếu chi
* Duyệt chứng từ
* Hủy chứng từ

Bắt buộc sử dụng Transaction.

# 10. Quy chuẩn Soft Delete

Ưu tiên dùng:

```sql
DaXoa BIT NULL DEFAULT(0)
```

Không xóa vật lý dữ liệu nếu không cần thiết.

# 11. Quy chuẩn ACL

Hệ thống sử dụng:

```text
ACL_Login
ACL_ManHinh
ACL_Action
ACL_PhanQuyen
```

Không tự tạo hệ thống phân quyền mới.

# 12. Quy chuẩn số chứng từ

Định dạng:

```text
Prefix + YY + Running Number
```

Ví dụ:

```text
DH26000001
PN26000001
PX26000001
PT26000001
PC26000001
```

# 13. Quy chuẩn Log

Tạo bảng:

```text
SYS_AuditLog
```

Ghi nhận:

* Insert
* Update
* Delete
* Login
* Approve
* Cancel

# 14. Quy chuẩn tạo bảng mới

Khi tạo bảng mới AI phải tự động thêm:

```sql
ID INT IDENTITY(1,1) PRIMARY KEY

NgayTao DATETIME NULL
NguoiTao INT NULL
NgayCapNhat DATETIME NULL
NguoiCapNhat INT NULL
```

Không tạo Foreign Key.

Không tạo Composite Key.

Không đổi tên cột ID.

# 15. Checklist

Trước khi hoàn thành thay đổi Database phải kiểm tra:

□ Có ID Identity

□ Có Audit Columns

□ Có trường Mã nếu là danh mục

□ Có Duplicate Code Check

□ Có Store Procedure

□ Có Transaction nếu là nghiệp vụ

□ Có Audit Log

□ Không dùng Foreign Key

□ Không dùng Composite Key

□ Không dùng FLOAT cho tiền

□ Không dùng VARCHAR cho dữ liệu tiếng Việt

Nếu còn bất kỳ mục nào chưa đạt thì tiếp tục chỉnh sửa cho đến khi đạt chuẩn.

Viết thành tài liệu DATABASE_STANDARD.md chuyên nghiệp để AI Coding Agent đọc và tuân thủ.
