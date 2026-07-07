IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[KT_PhieuThu]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[KT_PhieuThu](
        [ID] [int] IDENTITY(1,1) NOT NULL,
        [SoPhieuThu] [varchar](50) NOT NULL,
        [NgayThu] [date] NOT NULL,
        [IDTaiKhoanThanhToan] [int] NOT NULL,
        [IDKhachHang] [int] NOT NULL,
        [NguoiNopTien] [nvarchar](250) NULL,
        [SoDienThoaiNguoiNop] [varchar](50) NULL,
        [DienGiai] [nvarchar](500) NULL,
        [SoTienThu] [decimal](18, 0) NOT NULL DEFAULT (0),
        [TrangThai] [int] NOT NULL DEFAULT (1), -- 1: Mới, 2: Đã ghi sổ, 3: Hủy
        [NguoiTao] [int] NULL,
        [NgayTao] [datetime] NULL DEFAULT (getdate()),
        [NguoiCapNhat] [int] NULL,
        [NgayCapNhat] [datetime] NULL,
        CONSTRAINT [PK_KT_PhieuThu] PRIMARY KEY CLUSTERED ([ID] ASC),
        CONSTRAINT [UQ_KT_PhieuThu_SoPhieuThu] UNIQUE NONCLUSTERED ([SoPhieuThu] ASC)
    ) ON [PRIMARY]
END
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[KT_PhieuThuChiTiet]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[KT_PhieuThuChiTiet](
        [ID] [int] IDENTITY(1,1) NOT NULL,
        [IDPhieuThu] [int] NOT NULL,
        [IDChungTuBanHang] [int] NULL,
        [LoaiThu] [int] NOT NULL, -- 1: Thu nợ hóa đơn, 2: Dư trả trước, 3: Dùng trả trước
        [SoTienPhanBo] [decimal](18, 0) NOT NULL DEFAULT (0),
        [DienGiai] [nvarchar](500) NULL,
        CONSTRAINT [PK_KT_PhieuThuChiTiet] PRIMARY KEY CLUSTERED ([ID] ASC)
    ) ON [PRIMARY]
END
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[KT_PhieuThuFile]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[KT_PhieuThuFile](
        [ID] [int] IDENTITY(1,1) NOT NULL,
        [IDPhieuThu] [int] NOT NULL,
        [TenFile] [nvarchar](255) NOT NULL,
        [LoaiFile] [varchar](50) NULL,
        [DungLuong] [int] NULL,
        [NoiDungFile] [varbinary](max) NULL,
        [NgayTao] [datetime] NULL DEFAULT (getdate()),
        [NguoiTao] [int] NULL,
        CONSTRAINT [PK_KT_PhieuThuFile] PRIMARY KEY CLUSTERED ([ID] ASC)
    ) ON [PRIMARY]
END
GO
