IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'BAN_PhieuThuKhachHangFile')
BEGIN
    CREATE TABLE BAN_PhieuThuKhachHangFile (
        ID INT IDENTITY(1,1) PRIMARY KEY,
        IDChungTuBanHang INT NOT NULL,
        TenFile NVARCHAR(255) NOT NULL,
        LoaiFile NVARCHAR(50) NULL,
        DungLuong BIGINT NULL,
        NoiDungFile VARBINARY(MAX) NOT NULL,
        GhiChu NVARCHAR(500) NULL,
        NgayTao DATETIME NULL DEFAULT GETDATE(),
        NguoiTao INT NULL,
        NgayCapNhat DATETIME NULL,
        NguoiCapNhat INT NULL,
        IsDeleted BIT NOT NULL DEFAULT 0
    );

    CREATE INDEX IX_BAN_PhieuThuKhachHangFile_IDChungTuBanHang
    ON BAN_PhieuThuKhachHangFile(IDChungTuBanHang, IsDeleted);
END
