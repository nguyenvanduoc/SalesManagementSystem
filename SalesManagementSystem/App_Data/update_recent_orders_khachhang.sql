-- Cập nhật stored procedure sp_Dashboard_GetData
-- Thêm cột TenKhachHang vào kết quả DonHangGanDay (result set #16)
-- Chạy script này trên database QLBH để áp dụng thay đổi

-- Tìm và thay thế phần query gần đây trong stored procedure:
-- Cách 1: Chạy lại toàn bộ file create_dashboard_stored_procedure.sql (DROP + CREATE)
-- Cách 2: Chỉ ALTER phần nhỏ (bên dưới)

-- Kiểm tra tên bảng KhachHang chính xác:
-- SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME LIKE '%KhachHang%'

-- Test query trước khi ALTER:
SELECT TOP 5
    d.ID,
    d.SoDonHang,
    d.NgayTaoDon,
    d.TrangThaiDon,
    CASE d.TrangThaiDon
        WHEN 1 THEN N'Chưa giao'
        WHEN 2 THEN N'Đang giao'
        WHEN 3 THEN N'Đã giao'
        WHEN 4 THEN N'Đã hủy'
        ELSE N'Khác'
    END AS TenTrangThai,
    d.TongTien,
    ISNULL(l.HoDem + ' ' + l.Ten, N'Hệ thống') AS TenNguoiTao,
    kh.TenKhachHang
FROM NS_DonDatHang d
LEFT JOIN ACL_Login l ON d.NguoiTao = l.ID
LEFT JOIN DM_KhachHang kh ON d.IDKhachHang = kh.ID
ORDER BY d.NgayTaoDon DESC, d.ID DESC;
