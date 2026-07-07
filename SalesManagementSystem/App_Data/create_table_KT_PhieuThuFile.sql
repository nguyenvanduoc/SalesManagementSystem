-- Tạo bảng KT_PhieuThuFile nếu chưa tồn tại
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[KT_PhieuThuFile]') AND type = 'U')
BEGIN
    CREATE TABLE [dbo].[KT_PhieuThuFile](
        [ID]           INT             IDENTITY(1,1) NOT NULL,
        [IDPhieuThu]   INT             NOT NULL,
        [TenFile]      NVARCHAR(255)   NOT NULL,
        [LoaiFile]     VARCHAR(50)     NULL,
        [DungLuong]    BIGINT          NULL,
        [NoiDungFile]  VARBINARY(MAX)  NULL,
        [NgayTao]      DATETIME        NULL DEFAULT (GETDATE()),
        [NguoiTao]     INT             NULL,
        CONSTRAINT [PK_KT_PhieuThuFile] PRIMARY KEY CLUSTERED ([ID] ASC),
        CONSTRAINT [FK_KT_PhieuThuFile_PhieuThu] FOREIGN KEY ([IDPhieuThu]) REFERENCES [KT_PhieuThu]([ID])
    );

    CREATE INDEX IX_KT_PhieuThuFile_IDPhieuThu ON KT_PhieuThuFile(IDPhieuThu);
    
    PRINT N'Đã tạo bảng KT_PhieuThuFile thành công.';
END
ELSE
BEGIN
    PRINT N'Bảng KT_PhieuThuFile đã tồn tại.';
END
GO
