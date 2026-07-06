IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'KT_PhieuChiFile')
BEGIN
    CREATE TABLE KT_PhieuChiFile (
        ID INT IDENTITY(1,1) PRIMARY KEY,
        IDPhieuChi INT NOT NULL,
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

    CREATE INDEX IX_KT_PhieuChiFile_IDPhieuChi
    ON KT_PhieuChiFile(IDPhieuChi, IsDeleted);
END
