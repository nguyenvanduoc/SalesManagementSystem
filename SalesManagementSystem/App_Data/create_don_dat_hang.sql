-- ============================================================
-- Script tạo bảng Đơn Đặt Hàng
-- Chạy 1 lần trên SQL Server
-- ============================================================

-- ── Bảng NS_DonDatHang (Header) ─────────────────────────────
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'NS_DonDatHang')
BEGIN
    CREATE TABLE NS_DonDatHang (
        ID              INT IDENTITY(1,1) PRIMARY KEY NOT NULL,
        IDKhachHang     INT           NULL,
        NgayTaoDon      DATETIME      NULL,
        SoDonHang       NVARCHAR(100) NULL,
        IDNhanVien      INT           NULL,
        ThoiHanGiaoHang DATETIME      NULL,
        TrangThaiDon    INT           NOT NULL DEFAULT 1,
        TongTien        DECIMAL(18,0) NOT NULL DEFAULT 0,
        GhiChu          NVARCHAR(3000) NULL,
        NgayCapNhat     DATETIME      NULL,
        NguoiCapNhat    INT           NULL,
        NgayTao         DATETIME      NULL,
        NguoiTao        INT           NULL
    );
    PRINT 'Đã tạo bảng NS_DonDatHang';
END
ELSE
    PRINT 'Bảng NS_DonDatHang đã tồn tại';

-- ── Bảng NS_DonDatHangChiTiet (Detail) ──────────────────────
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'NS_DonDatHangChiTiet')
BEGIN
    CREATE TABLE NS_DonDatHangChiTiet (
        ID              INT IDENTITY(1,1) PRIMARY KEY NOT NULL,
        IDDonDatHang    INT           NOT NULL,   -- FK -> NS_DonDatHang.ID
        IDSanPham       INT           NULL,
        SoLuong         DECIMAL(18,2) NOT NULL DEFAULT 1,
        DonGia          DECIMAL(18,0) NOT NULL DEFAULT 0,
        ThanhTien       DECIMAL(18,0) NOT NULL DEFAULT 0,
        ThueGTGT        DECIMAL(8,2)  NOT NULL DEFAULT 0,
        IsHangKhuyenMai BIT           NOT NULL DEFAULT 0,
        GhiChu          NVARCHAR(3000) NULL,
        -- Denorm từ header (theo yêu cầu)
        NgayTaoDon      DATETIME      NULL,
        SoDonHang       NVARCHAR(100) NULL,
        IDNhanVien      INT           NULL,
        ThoiHanGiaoHang DATETIME      NULL,
        TrangThaiDon    INT           NULL,
        -- Audit
        NgayCapNhat     DATETIME      NULL,
        NguoiCapNhat    INT           NULL,
        NgayTao         DATETIME      NULL,
        NguoiTao        INT           NULL
    );
    PRINT 'Đã tạo bảng NS_DonDatHangChiTiet';
END
ELSE
    PRINT 'Bảng NS_DonDatHangChiTiet đã tồn tại';
