-- =============================================
-- Author:      Antigravity
-- Create date: 2026-06-19
-- Description: Module Phiếu Chi, Sổ Quỹ, Công Nợ Phải Trả NCC
-- =============================================

-- =============================================
-- 1. BẢNG DM_KhoanMucChi (Danh mục khoản mục chi)
-- =============================================
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'DM_KhoanMucChi')
BEGIN
    CREATE TABLE DM_KhoanMucChi (
        ID              INT IDENTITY(1,1) PRIMARY KEY,
        MaKhoanMuc      NVARCHAR(50)    NOT NULL,
        TenKhoanMuc     NVARCHAR(255)   NOT NULL,
        IsHoatDong      BIT             NOT NULL DEFAULT 1,
        NgayTao         DATETIME        NULL,
        NguoiTao        INT             NULL,
        NgayCapNhat     DATETIME        NULL,
        NguoiCapNhat    INT             NULL
    );
    PRINT N'Đã tạo bảng DM_KhoanMucChi';
END
ELSE
    PRINT N'Bảng DM_KhoanMucChi đã tồn tại, bỏ qua';
GO

-- Dữ liệu mẫu khoản mục chi
IF NOT EXISTS (SELECT 1 FROM DM_KhoanMucChi)
BEGIN
    INSERT INTO DM_KhoanMucChi (MaKhoanMuc, TenKhoanMuc, IsHoatDong, NgayTao)
    VALUES
        (N'XD',  N'Xăng dầu',            1, GETDATE()),
        (N'BX',  N'Bốc xếp',             1, GETDATE()),
        (N'VC',  N'Vận chuyển',           1, GETDATE()),
        (N'MVT', N'Mua vật tư',           1, GETDATE()),
        (N'TKH', N'Tiếp khách hàng',      1, GETDATE()),
        (N'LCB', N'Lương - Công nhân',    1, GETDATE()),
        (N'CC',  N'Chi phí văn phòng',    1, GETDATE()),
        (N'TT',  N'Thanh toán nhà cung cấp', 1, GETDATE()),
        (N'KH',  N'Khác',                 1, GETDATE());
    PRINT N'Đã chèn dữ liệu mẫu DM_KhoanMucChi';
END
GO

-- =============================================
-- 2. BẢNG KT_PhieuChi
-- =============================================
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'KT_PhieuChi')
BEGIN
    CREATE TABLE KT_PhieuChi (
        ID                      INT IDENTITY(1,1) PRIMARY KEY,
        SoPhieuChi              NVARCHAR(50)     NOT NULL,
        NgayChi                 DATE             NOT NULL,
        IDKhoanMucChi           INT              NULL,
        IDTaiKhoanThanhToan     INT              NOT NULL,
        IDNguoiNhan             INT              NULL,
        NguoiNhanTien           NVARCHAR(255)    NULL,
        SoDienThoaiNguoiNhan    NVARCHAR(50)     NULL,
        IDNhaCungCap            INT              NULL,
        IDPhieuNhap             INT              NULL,
        SoTienChi               DECIMAL(18,2)   NOT NULL DEFAULT 0,
        DienGiai                NVARCHAR(1000)  NULL,
        TrangThai               INT             NOT NULL DEFAULT 1,
        LyDoHuy                 NVARCHAR(500)   NULL,
        NgayTao                 DATETIME        NULL,
        NguoiTao                INT             NULL,
        NgayCapNhat             DATETIME        NULL,
        NguoiCapNhat            INT             NULL,
        NgayGhi                 DATETIME        NULL,
        NguoiGhi                INT             NULL,
        NgayHuy                 DATETIME        NULL,
        NguoiHuy                INT             NULL,
        IsDeleted               BIT             NOT NULL DEFAULT 0
    );
    PRINT N'Đã tạo bảng KT_PhieuChi';
END
ELSE
    PRINT N'Bảng KT_PhieuChi đã tồn tại, bỏ qua';
GO

-- =============================================
-- 3. STORED PROCEDURES – DM_KhoanMucChi
-- =============================================
GO
CREATE OR ALTER PROCEDURE sp_DM_KhoanMucChi_GetList
    @Keyword    NVARCHAR(255) = NULL,
    @IsHoatDong BIT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SELECT ID, MaKhoanMuc, TenKhoanMuc, IsHoatDong, NgayTao
    FROM DM_KhoanMucChi
    WHERE 1 = 1
      AND (@Keyword IS NULL OR MaKhoanMuc LIKE '%' + @Keyword + '%' OR TenKhoanMuc LIKE '%' + @Keyword + '%')
      AND (@IsHoatDong IS NULL OR IsHoatDong = @IsHoatDong)
    ORDER BY TenKhoanMuc;
END
GO

-- =============================================
-- 4. STORED PROCEDURES – KT_PhieuChi
-- =============================================
GO
CREATE OR ALTER PROCEDURE sp_KT_PhieuChi_GenerateSo
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @Nam   CHAR(2)  = RIGHT(CAST(YEAR(GETDATE()) AS VARCHAR(4)), 2);
    DECLARE @MaxSo INT;

    SELECT @MaxSo = ISNULL(MAX(CAST(RIGHT(SoPhieuChi, 6) AS INT)), 0)
    FROM KT_PhieuChi
    WHERE SoPhieuChi LIKE 'PC' + @Nam + '%'
      AND ISNUMERIC(RIGHT(SoPhieuChi, 6)) = 1;

    SELECT 'PC' + @Nam + RIGHT('000000' + CAST(@MaxSo + 1 AS VARCHAR(6)), 6) AS SoPhieuChi;
END
GO

CREATE OR ALTER PROCEDURE sp_KT_PhieuChi_GetList
    @TuNgay         DATETIME        = NULL,
    @DenNgay        DATETIME        = NULL,
    @SoPhieuChi     NVARCHAR(50)    = NULL,
    @IDNhaCungCap   INT             = NULL,
    @IDKhoanMucChi  INT             = NULL,
    @TrangThai      INT             = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        pc.ID,
        pc.SoPhieuChi,
        pc.NgayChi,
        pc.IDKhoanMucChi,
        km.TenKhoanMuc,
        pc.IDTaiKhoanThanhToan,
        tk.TenTaiKhoan  AS TenTaiKhoanThanhToan,
        tk.SoTaiKhoan,
        pc.IDNguoiNhan,
        ISNULL(ns.HoDem, '') + ' ' + ISNULL(ns.Ten, '') AS TenNguoiNhan,
        pc.NguoiNhanTien,
        pc.SoDienThoaiNguoiNhan,
        pc.IDNhaCungCap,
        ncc.TenNhaCungCap,
        pc.IDPhieuNhap,
        pn.SoChungTu AS SoPhieuNhap,
        pc.SoTienChi,
        pc.DienGiai,
        pc.TrangThai,
        pc.NgayTao,
        pc.NguoiTao,
        pc.NgayGhi,
        pc.LyDoHuy
    FROM KT_PhieuChi pc
    LEFT JOIN DM_KhoanMucChi        km  ON pc.IDKhoanMucChi       = km.ID
    LEFT JOIN DM_TaiKhoanThanhToan  tk  ON pc.IDTaiKhoanThanhToan = tk.ID
    LEFT JOIN NS_NhanSu             ns  ON pc.IDNguoiNhan          = ns.ID
    LEFT JOIN DM_NhaCungCap         ncc ON pc.IDNhaCungCap         = ncc.ID
    LEFT JOIN KHO_PhieuNhap         pn  ON pc.IDPhieuNhap          = pn.ID
    WHERE pc.IsDeleted = 0
      AND (@TuNgay        IS NULL OR CAST(pc.NgayChi AS DATE) >= CAST(@TuNgay AS DATE))
      AND (@DenNgay       IS NULL OR CAST(pc.NgayChi AS DATE) <= CAST(@DenNgay AS DATE))
      AND (@SoPhieuChi    IS NULL OR pc.SoPhieuChi LIKE '%' + @SoPhieuChi + '%')
      AND (@IDNhaCungCap  IS NULL OR pc.IDNhaCungCap = @IDNhaCungCap)
      AND (@IDKhoanMucChi IS NULL OR pc.IDKhoanMucChi = @IDKhoanMucChi)
      AND (@TrangThai     IS NULL OR pc.TrangThai = @TrangThai)
    ORDER BY pc.NgayChi DESC, pc.ID DESC;
END
GO

CREATE OR ALTER PROCEDURE sp_KT_PhieuChi_GetByID
    @ID INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        pc.*,
        km.TenKhoanMuc,
        tk.TenTaiKhoan  AS TenTaiKhoanThanhToan,
        tk.SoTaiKhoan,
        ISNULL(ns.HoDem, '') + ' ' + ISNULL(ns.Ten, '') AS TenNguoiNhan,
        ncc.TenNhaCungCap,
        pn.SoChungTu AS SoPhieuNhap
    FROM KT_PhieuChi pc
    LEFT JOIN DM_KhoanMucChi        km  ON pc.IDKhoanMucChi       = km.ID
    LEFT JOIN DM_TaiKhoanThanhToan  tk  ON pc.IDTaiKhoanThanhToan = tk.ID
    LEFT JOIN NS_NhanSu             ns  ON pc.IDNguoiNhan          = ns.ID
    LEFT JOIN DM_NhaCungCap         ncc ON pc.IDNhaCungCap         = ncc.ID
    LEFT JOIN KHO_PhieuNhap         pn  ON pc.IDPhieuNhap          = pn.ID
    WHERE pc.ID = @ID AND pc.IsDeleted = 0;
END
GO

CREATE OR ALTER PROCEDURE sp_KT_PhieuChi_Save
    @ID                     INT             = 0,
    @SoPhieuChi             NVARCHAR(50),
    @NgayChi                DATE,
    @IDKhoanMucChi          INT             = NULL,
    @IDTaiKhoanThanhToan    INT,
    @IDNguoiNhan            INT             = NULL,
    @NguoiNhanTien          NVARCHAR(255)   = NULL,
    @SoDienThoaiNguoiNhan   NVARCHAR(50)    = NULL,
    @IDNhaCungCap           INT             = NULL,
    @IDPhieuNhap            INT             = NULL,
    @SoTienChi              DECIMAL(18,2),
    @DienGiai               NVARCHAR(1000)  = NULL,
    @NguoiTao               INT             = NULL,
    @NewID                  INT             OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    IF @ID = 0 OR @ID IS NULL
    BEGIN
        INSERT INTO KT_PhieuChi
            (SoPhieuChi, NgayChi, IDKhoanMucChi, IDTaiKhoanThanhToan, IDNguoiNhan,
             NguoiNhanTien, SoDienThoaiNguoiNhan,
             IDNhaCungCap, IDPhieuNhap, SoTienChi, DienGiai, TrangThai,
             NgayTao, NguoiTao, IsDeleted)
        VALUES
            (@SoPhieuChi, @NgayChi, @IDKhoanMucChi, @IDTaiKhoanThanhToan, @IDNguoiNhan,
             @NguoiNhanTien, @SoDienThoaiNguoiNhan,
             @IDNhaCungCap, @IDPhieuNhap, @SoTienChi, @DienGiai, 1,
             GETDATE(), @NguoiTao, 0);
        SET @NewID = SCOPE_IDENTITY();
    END
    ELSE
    BEGIN
        UPDATE KT_PhieuChi SET
            NgayChi                 = @NgayChi,
            IDKhoanMucChi           = @IDKhoanMucChi,
            IDTaiKhoanThanhToan     = @IDTaiKhoanThanhToan,
            IDNguoiNhan             = @IDNguoiNhan,
            NguoiNhanTien           = @NguoiNhanTien,
            SoDienThoaiNguoiNhan    = @SoDienThoaiNguoiNhan,
            IDNhaCungCap            = @IDNhaCungCap,
            IDPhieuNhap             = @IDPhieuNhap,
            SoTienChi               = @SoTienChi,
            DienGiai                = @DienGiai,
            NgayCapNhat             = GETDATE(),
            NguoiCapNhat            = @NguoiTao
        WHERE ID = @ID AND IsDeleted = 0;
        SET @NewID = @ID;
    END
END
GO

CREATE OR ALTER PROCEDURE sp_KT_PhieuChi_GhiSo
    @ID         INT,
    @NguoiGhi   INT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE KT_PhieuChi
    SET TrangThai    = 2,
        NgayGhi      = GETDATE(),
        NguoiGhi     = @NguoiGhi,
        NgayCapNhat  = GETDATE(),
        NguoiCapNhat = @NguoiGhi
    WHERE ID = @ID AND IsDeleted = 0 AND TrangThai = 1;

    -- Ghi vào Nhật Ký Chung (khi ghi)
    DECLARE @SoPhieuChi         NVARCHAR(50);
    DECLARE @NgayChi            DATE;
    DECLARE @SoTienChi          DECIMAL(18,2);
    DECLARE @DienGiai           NVARCHAR(1000);
    DECLARE @SoTKThanhToan      NVARCHAR(20);  -- Số TK tiền mặt/ngân hàng (Có)
    DECLARE @IDTKKeToan         INT;

    SELECT
        @SoPhieuChi     = pc.SoPhieuChi,
        @NgayChi        = pc.NgayChi,
        @SoTienChi      = pc.SoTienChi,
        @DienGiai       = pc.DienGiai,
        @IDTKKeToan     = tk.IDTaiKhoanKeToan
    FROM KT_PhieuChi pc
    LEFT JOIN DM_TaiKhoanThanhToan tk ON pc.IDTaiKhoanThanhToan = tk.ID
    WHERE pc.ID = @ID;

    -- Lấy số tài khoản kế toán của tài khoản thanh toán
    SELECT @SoTKThanhToan = SoTaiKhoan
    FROM KT_TaiKhoanKeToan
    WHERE ID = @IDTKKeToan;

    -- Ghi bút toán: Nợ 6xx (chi phí), Có TK tiền
    -- Chỉ ghi nếu chưa tồn tại bút toán này
    IF NOT EXISTS (
        SELECT 1 FROM KT_NhatKyChung
        WHERE LoaiChungTu = N'PHIEUCHI' AND IDChungTu = @ID AND IsHuy = 0
    )
    BEGIN
        INSERT INTO KT_NhatKyChung
            (NgayChungTu, SoChungTu, LoaiChungTu, IDChungTu,
             TaiKhoanNo, TaiKhoanCo, SoTien, DienGiai, NgayTao, NguoiTao)
        VALUES
            (@NgayChi, @SoPhieuChi, N'PHIEUCHI', @ID,
             N'6418',
             ISNULL(@SoTKThanhToan, N'1111'),
             @SoTienChi,
             ISNULL(@DienGiai, N'Phiếu chi ' + @SoPhieuChi),
             GETDATE(), @NguoiGhi);
    END
END
GO

CREATE OR ALTER PROCEDURE sp_KT_PhieuChi_Huy
    @ID         INT,
    @NguoiHuy   INT,
    @LyDoHuy    NVARCHAR(500)
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE KT_PhieuChi
    SET TrangThai    = 3,
        LyDoHuy      = @LyDoHuy,
        NgayHuy      = GETDATE(),
        NguoiHuy     = @NguoiHuy,
        NgayCapNhat  = GETDATE(),
        NguoiCapNhat = @NguoiHuy
    WHERE ID = @ID AND IsDeleted = 0;

    -- Đánh dấu hủy bút toán NKC nếu đã ghi
    UPDATE KT_NhatKyChung
    SET IsHuy = 1
    WHERE LoaiChungTu = N'PHIEUCHI' AND IDChungTu = @ID;
END
GO

CREATE OR ALTER PROCEDURE sp_KT_PhieuChi_Delete
    @ID         INT,
    @NguoiXoa   INT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE KT_PhieuChi
    SET IsDeleted    = 1,
        NgayCapNhat  = GETDATE(),
        NguoiCapNhat = @NguoiXoa
    WHERE ID = @ID AND TrangThai <> 2; -- Không xóa phiếu đã ghi
END
GO

-- =============================================
-- 5. STORED PROCEDURE – Sổ Quỹ
-- =============================================
GO
CREATE OR ALTER PROCEDURE sp_KT_SoQuy_GetList
    @TuNgay             DATETIME        = NULL,
    @DenNgay            DATETIME        = NULL,
    @IDTaiKhoanThanhToan INT            = NULL
AS
BEGIN
    SET NOCOUNT ON;

    -- THU: từ BAN_PhieuThuKhachHang
    SELECT
        pth.NgayThu                     AS NgayChungTu,
        pth.SoPhieuThu                  AS SoChungTu,
        N'THU'                          AS LoaiChungTu,
        pth.IDTaiKhoanThanhToan,
        tk.TenTaiKhoan                  AS TenTaiKhoanThanhToan,
        ISNULL(pth.DienGiai, N'Thu tiền khách hàng') AS DienGiai,
        pth.SoTienThu                   AS SoTienThu,
        0                               AS SoTienChi,
        pth.TrangThai
    FROM BAN_PhieuThuKhachHang pth
    LEFT JOIN DM_TaiKhoanThanhToan tk ON pth.IDTaiKhoanThanhToan = tk.ID
    WHERE pth.IsDeleted = 0
      AND pth.TrangThai = 2  -- Chỉ lấy đã ghi
      AND (@TuNgay IS NULL OR CAST(pth.NgayThu AS DATE) >= CAST(@TuNgay AS DATE))
      AND (@DenNgay IS NULL OR CAST(pth.NgayThu AS DATE) <= CAST(@DenNgay AS DATE))
      AND (@IDTaiKhoanThanhToan IS NULL OR pth.IDTaiKhoanThanhToan = @IDTaiKhoanThanhToan)

    UNION ALL

    -- CHI: từ KT_PhieuChi
    SELECT
        pc.NgayChi                      AS NgayChungTu,
        pc.SoPhieuChi                   AS SoChungTu,
        N'CHI'                          AS LoaiChungTu,
        pc.IDTaiKhoanThanhToan,
        tk2.TenTaiKhoan                 AS TenTaiKhoanThanhToan,
        ISNULL(pc.DienGiai, N'Phiếu chi') AS DienGiai,
        0                               AS SoTienThu,
        pc.SoTienChi                    AS SoTienChi,
        pc.TrangThai
    FROM KT_PhieuChi pc
    LEFT JOIN DM_TaiKhoanThanhToan tk2 ON pc.IDTaiKhoanThanhToan = tk2.ID
    WHERE pc.IsDeleted = 0
      AND pc.TrangThai = 2  -- Chỉ lấy đã ghi
      AND (@TuNgay IS NULL OR CAST(pc.NgayChi AS DATE) >= CAST(@TuNgay AS DATE))
      AND (@DenNgay IS NULL OR CAST(pc.NgayChi AS DATE) <= CAST(@DenNgay AS DATE))
      AND (@IDTaiKhoanThanhToan IS NULL OR pc.IDTaiKhoanThanhToan = @IDTaiKhoanThanhToan)

    ORDER BY NgayChungTu ASC, SoChungTu ASC;
END
GO

-- =============================================
-- 6. STORED PROCEDURE – Công Nợ Phải Trả NCC
-- =============================================
GO
CREATE OR ALTER PROCEDURE sp_CongNo_PhaseTra_NCC_GetList
    @TuNgay         DATETIME        = NULL,
    @DenNgay        DATETIME        = NULL,
    @IDNhaCungCap   INT             = NULL,
    @TrangThaiCongNo INT            = NULL
    -- TrangThaiCongNo: NULL=tất cả, 1=còn nợ, 2=đã thanh toán hết
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        pn.ID               AS IDPhieuNhap,
        pn.SoChungTu        AS SoPhieuNhap,
        pn.NgayNhap,
        pn.IDNhaCungCap,
        ncc.TenNhaCungCap,
        ncc.DienThoai       AS DienThoaiNCC,
        pn.TongTienHang,
        ISNULL(
            (SELECT SUM(pc2.SoTienChi)
             FROM KT_PhieuChi pc2
             WHERE pc2.IDPhieuNhap = pn.ID
               AND pc2.TrangThai = 2
               AND pc2.IsDeleted = 0),
            0
        )                   AS DaThanhToan,
        pn.TongTienHang - ISNULL(
            (SELECT SUM(pc2.SoTienChi)
             FROM KT_PhieuChi pc2
             WHERE pc2.IDPhieuNhap = pn.ID
               AND pc2.TrangThai = 2
               AND pc2.IsDeleted = 0),
            0
        )                   AS ConLai
    FROM KHO_PhieuNhap pn
    LEFT JOIN DM_NhaCungCap ncc ON pn.IDNhaCungCap = ncc.ID
    WHERE pn.IsDeleted = 0
      AND (@TuNgay IS NULL OR CAST(pn.NgayNhap AS DATE) >= CAST(@TuNgay AS DATE))
      AND (@DenNgay IS NULL OR CAST(pn.NgayNhap AS DATE) <= CAST(@DenNgay AS DATE))
      AND (@IDNhaCungCap IS NULL OR pn.IDNhaCungCap = @IDNhaCungCap)
      AND (
          @TrangThaiCongNo IS NULL
          OR (@TrangThaiCongNo = 1 AND -- Còn nợ
              pn.TongTienHang - ISNULL(
                  (SELECT SUM(pc2.SoTienChi) FROM KT_PhieuChi pc2
                   WHERE pc2.IDPhieuNhap = pn.ID AND pc2.TrangThai = 2 AND pc2.IsDeleted = 0), 0
              ) > 0
          )
          OR (@TrangThaiCongNo = 2 AND -- Đã thanh toán hết
              pn.TongTienHang - ISNULL(
                  (SELECT SUM(pc2.SoTienChi) FROM KT_PhieuChi pc2
                   WHERE pc2.IDPhieuNhap = pn.ID AND pc2.TrangThai = 2 AND pc2.IsDeleted = 0), 0
              ) <= 0
          )
      )
    ORDER BY pn.NgayNhap DESC, pn.ID DESC;
END
GO

-- =============================================
-- 7. PHÂN QUYỀN ACL
-- =============================================

-- Kiểm tra ACL_ManHinh có cột IDParent không
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='ACL_ManHinh' AND COLUMN_NAME='IDParent')
BEGIN
    -- Bỏ qua, insert bình thường
    PRINT N'Cột IDParent không tồn tại, bỏ qua IDParent trong insert'
END
GO

-- Đăng ký màn hình PhieuChi
IF NOT EXISTS (SELECT 1 FROM ACL_ManHinh WHERE TenManHinh = N'PhieuChi')
BEGIN
    INSERT INTO ACL_ManHinh (TenManHinh, TieuDe, ControllerName, ActionName, Icon, SapXep, IsHoatDong)
    VALUES (N'PhieuChi', N'Phiếu Chi', N'PhieuChi', N'Index', N'bi-cash-stack', 40, 1);
    PRINT N'Đã đăng ký màn hình PhieuChi';
END
GO

IF NOT EXISTS (SELECT 1 FROM ACL_ManHinh WHERE TenManHinh = N'SoQuy')
BEGIN
    INSERT INTO ACL_ManHinh (TenManHinh, TieuDe, ControllerName, ActionName, Icon, SapXep, IsHoatDong)
    VALUES (N'SoQuy', N'Sổ Quỹ', N'SoQuy', N'Index', N'bi-journal-text', 41, 1);
    PRINT N'Đã đăng ký màn hình SoQuy';
END
GO

IF NOT EXISTS (SELECT 1 FROM ACL_ManHinh WHERE TenManHinh = N'CongNoNCC')
BEGIN
    INSERT INTO ACL_ManHinh (TenManHinh, TieuDe, ControllerName, ActionName, Icon, SapXep, IsHoatDong)
    VALUES (N'CongNoNCC', N'Công Nợ Phải Trả NCC', N'CongNoNCC', N'Index', N'bi-receipt', 42, 1);
    PRINT N'Đã đăng ký màn hình CongNoNCC';
END
GO

-- Đăng ký các Action (Xem=1, Thêm=2, CapNhat=3, Xoa=4, TuyChon=5)
-- PhieuChi
IF NOT EXISTS (SELECT 1 FROM ACL_Action WHERE TenManHinh = N'PhieuChi' AND LoaiPhanQuyen = 1)
BEGIN
    INSERT INTO ACL_Action (TenManHinh, LoaiPhanQuyen) VALUES (N'PhieuChi', 1); -- Xem
    INSERT INTO ACL_Action (TenManHinh, LoaiPhanQuyen) VALUES (N'PhieuChi', 2); -- Thêm
    INSERT INTO ACL_Action (TenManHinh, LoaiPhanQuyen) VALUES (N'PhieuChi', 3); -- CapNhat
    INSERT INTO ACL_Action (TenManHinh, LoaiPhanQuyen) VALUES (N'PhieuChi', 4); -- Xoa
    INSERT INTO ACL_Action (TenManHinh, LoaiPhanQuyen) VALUES (N'PhieuChi', 5); -- TuyChon (GhiSo/Huy)
    PRINT N'Đã đăng ký ACL_Action cho PhieuChi';
END
GO

-- SoQuy
IF NOT EXISTS (SELECT 1 FROM ACL_Action WHERE TenManHinh = N'SoQuy' AND LoaiPhanQuyen = 1)
BEGIN
    INSERT INTO ACL_Action (TenManHinh, LoaiPhanQuyen) VALUES (N'SoQuy', 1);
    PRINT N'Đã đăng ký ACL_Action cho SoQuy';
END
GO

-- CongNoNCC
IF NOT EXISTS (SELECT 1 FROM ACL_Action WHERE TenManHinh = N'CongNoNCC' AND LoaiPhanQuyen = 1)
BEGIN
    INSERT INTO ACL_Action (TenManHinh, LoaiPhanQuyen) VALUES (N'CongNoNCC', 1);
    PRINT N'Đã đăng ký ACL_Action cho CongNoNCC';
END
GO

PRINT N'Script hoàn thành!';
GO
