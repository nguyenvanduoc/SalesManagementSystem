CREATE TABLE DM_LoaiNhapKho
(
    ID INT IDENTITY(1,1) PRIMARY KEY,
    MaLoaiNhap NVARCHAR(50) NOT NULL,
    TenLoaiNhap NVARCHAR(255) NOT NULL,
    MoTa NVARCHAR(500) NULL,
    IsHoatDong BIT NOT NULL DEFAULT 1,
    STT INT NULL,
    NgayTao DATETIME NULL,
    NguoiTao INT NULL,
    NgayCapNhat DATETIME NULL,
    NguoiCapNhat INT NULL
);
GO

INSERT INTO DM_LoaiNhapKho (MaLoaiNhap, TenLoaiNhap, MoTa, STT, NgayTao) VALUES 
('NHAP_MUA', N'Nhập mua hàng', N'Nhập hàng mua từ nhà cung cấp', 1, GETDATE()),
('CHUYEN_KHO', N'Chuyển kho nội bộ', N'Nhận hàng từ kho nội bộ khác', 2, GETDATE()),
('TRA_HANG', N'Khách hàng trả hàng', N'Nhận hàng do khách hàng trả lại', 3, GETDATE()),
('DIEU_CHINH', N'Điều chỉnh tồn kho', N'Điều chỉnh số lượng tồn kho (tăng/giảm)', 4, GETDATE()),
('NHAP_KHAC', N'Nhập khác', N'Nhập hàng vì lý do khác', 5, GETDATE());
GO

CREATE OR ALTER PROCEDURE sp_DM_LoaiNhapKho_GetDropdown
AS
BEGIN
    SET NOCOUNT ON;
    SELECT 
        ID,
        MaLoaiNhap,
        TenLoaiNhap
    FROM DM_LoaiNhapKho
    WHERE IsHoatDong = 1
    ORDER BY STT ASC, TenLoaiNhap ASC;
END
GO
