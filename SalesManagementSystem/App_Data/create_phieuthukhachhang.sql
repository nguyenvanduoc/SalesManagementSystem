-- =============================================
-- Author:      Antigravity
-- Create date: 2026-06-18
-- Description: Tạo bảng, stored procedures và phân quyền cho module Phiếu Thu Khách Hàng
-- =============================================

-- 1. Tạo bảng BAN_PhieuThuKhachHang
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[BAN_PhieuThuKhachHang]') AND type in (N'U'))
BEGIN
    CREATE TABLE BAN_PhieuThuKhachHang (
        ID INT IDENTITY(1,1) PRIMARY KEY,
        SoPhieuThu NVARCHAR(50) NOT NULL UNIQUE,
        NgayThu DATE NOT NULL,
        IDChungTuBanHang INT NOT NULL,
        IDKhachHang INT NOT NULL,
        IDTaiKhoanThanhToan INT NOT NULL,
        SoTienThu DECIMAL(18,2) NOT NULL DEFAULT 0,
        GhiChu NVARCHAR(1000) NULL,
        TrangThai INT NOT NULL DEFAULT 1, -- 1: Đề nghị ghi, 2: Đã ghi, 3: Đã hủy
        NgayTao DATETIME NOT NULL DEFAULT GETDATE(),
        NguoiTao INT NULL,
        NgayCapNhat DATETIME NULL,
        NguoiCapNhat INT NULL,
        NgayGhi DATETIME NULL,
        NguoiGhi INT NULL,
        NgayHuy DATETIME NULL,
        NguoiHuy INT NULL,
        LyDoHuy NVARCHAR(500) NULL,
        IsDeleted BIT NOT NULL DEFAULT 0
    );
END
GO

-- 2. Bổ sung các cột hủy vào bảng KT_NhatKyChung (nếu chưa tồn tại)
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'KT_NhatKyChung' AND COLUMN_NAME = 'IsHuy')
BEGIN
    ALTER TABLE KT_NhatKyChung ADD IsHuy BIT NOT NULL DEFAULT 0;
END
GO

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'KT_NhatKyChung' AND COLUMN_NAME = 'NgayHuy')
BEGIN
    ALTER TABLE KT_NhatKyChung ADD NgayHuy DATETIME NULL;
END
GO

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'KT_NhatKyChung' AND COLUMN_NAME = 'NguoiHuy')
BEGIN
    ALTER TABLE KT_NhatKyChung ADD NguoiHuy INT NULL;
END
GO

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'KT_NhatKyChung' AND COLUMN_NAME = 'LyDoHuy')
BEGIN
    ALTER TABLE KT_NhatKyChung ADD LyDoHuy NVARCHAR(500) NULL;
END
GO

-- 3. Đăng ký màn hình và phân quyền
DECLARE @ManHinhID INT;
IF NOT EXISTS (SELECT 1 FROM ACL_ManHinh WHERE TenManHinh = N'Phiếu thu khách hàng')
BEGIN
    INSERT INTO ACL_ManHinh (TenManHinh, NhomChaManHinh, IsSuDung, STT)
    VALUES (N'Phiếu thu khách hàng', N'BAN HANG', 1, 1026);
    SET @ManHinhID = SCOPE_IDENTITY();
END
ELSE
BEGIN
    SELECT @ManHinhID = ID FROM ACL_ManHinh WHERE TenManHinh = N'Phiếu thu khách hàng';
END

-- Đăng ký các action cho màn hình Phiếu thu khách hàng
IF NOT EXISTS (SELECT 1 FROM ACL_Action WHERE IDManHinh = @ManHinhID AND TenAction = 'Index')
    INSERT INTO ACL_Action (IDManHinh, TenAction, TenController, LoaiPhanQuyen, GhiChu)
    VALUES (@ManHinhID, 'Index', 'PhieuThuKhachHang', 1, N'Xem danh sách phiếu thu');

IF NOT EXISTS (SELECT 1 FROM ACL_Action WHERE IDManHinh = @ManHinhID AND TenAction = 'Create')
    INSERT INTO ACL_Action (IDManHinh, TenAction, TenController, LoaiPhanQuyen, GhiChu)
    VALUES (@ManHinhID, 'Create', 'PhieuThuKhachHang', 2, N'Lập phiếu thu');

IF NOT EXISTS (SELECT 1 FROM ACL_Action WHERE IDManHinh = @ManHinhID AND TenAction = 'Save')
    INSERT INTO ACL_Action (IDManHinh, TenAction, TenController, LoaiPhanQuyen, GhiChu)
    VALUES (@ManHinhID, 'Save', 'PhieuThuKhachHang', 2, N'Lưu phiếu thu');

IF NOT EXISTS (SELECT 1 FROM ACL_Action WHERE IDManHinh = @ManHinhID AND TenAction = 'Edit')
    INSERT INTO ACL_Action (IDManHinh, TenAction, TenController, LoaiPhanQuyen, GhiChu)
    VALUES (@ManHinhID, 'Edit', 'PhieuThuKhachHang', 3, N'Sửa phiếu thu');

IF NOT EXISTS (SELECT 1 FROM ACL_Action WHERE IDManHinh = @ManHinhID AND TenAction = 'Delete')
    INSERT INTO ACL_Action (IDManHinh, TenAction, TenController, LoaiPhanQuyen, GhiChu)
    VALUES (@ManHinhID, 'Delete', 'PhieuThuKhachHang', 4, N'Xóa phiếu thu');

IF NOT EXISTS (SELECT 1 FROM ACL_Action WHERE IDManHinh = @ManHinhID AND TenAction = 'GhiSo')
    INSERT INTO ACL_Action (IDManHinh, TenAction, TenController, LoaiPhanQuyen, GhiChu)
    VALUES (@ManHinhID, 'GhiSo', 'PhieuThuKhachHang', 5, N'Ghi sổ phiếu thu');

IF NOT EXISTS (SELECT 1 FROM ACL_Action WHERE IDManHinh = @ManHinhID AND TenAction = 'Huy')
    INSERT INTO ACL_Action (IDManHinh, TenAction, TenController, LoaiPhanQuyen, GhiChu)
    VALUES (@ManHinhID, 'Huy', 'PhieuThuKhachHang', 5, N'Hủy phiếu thu');

-- Cấp quyền cho toàn bộ các tài khoản hiện có
INSERT INTO ACL_PhanQuyen (IDLogin, IDAction, IsChoPhep, NgayTao)
SELECT l.ID, act.ID, 1, GETDATE()
FROM ACL_Login l
CROSS JOIN ACL_Action act
WHERE act.IDManHinh = @ManHinhID
  AND NOT EXISTS (
      SELECT 1 FROM ACL_PhanQuyen pq 
      WHERE pq.IDLogin = l.ID AND pq.IDAction = act.ID
  );
GO

-- 4. Stored Procedures cho Phiếu Thu Khách Hàng

-- sp_BAN_PhieuThuKhachHang_GetList
CREATE OR ALTER PROCEDURE sp_BAN_PhieuThuKhachHang_GetList
    @TuNgay DATE = NULL,
    @DenNgay DATE = NULL,
    @SoChungTu NVARCHAR(50) = NULL,
    @IDKhachHang INT = NULL,
    @TrangThaiCongNo INT = NULL -- 1: Chưa thanh toán, 2: Thanh toán một phần, 3: Đã thanh toán
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        c.ID,
        ISNULL(d.SoDonHang, c.SoChungTu) AS SoChungTu,
        c.NgayChungTu,
        c.IDKhachHang,
        kh.TenKhachHang,
        c.TongCong,
        ISNULL(pt.DaThanhToan, 0) AS DaThanhToan,
        (c.TongCong - ISNULL(pt.DaThanhToan, 0)) AS ConLai,
        CASE 
            WHEN ISNULL(pt.DaThanhToan, 0) = 0 THEN 1 -- Chưa thanh toán
            WHEN c.TongCong - ISNULL(pt.DaThanhToan, 0) <= 0 THEN 3 -- Đã thanh toán
            ELSE 2 -- Thanh toán một phần
        END AS TrangThaiCongNo,
        ns.HoDem + ' ' + ns.Ten AS TenNguoiTao,
        c.NgayTao
    FROM BAN_ChungTuBanHang c
    JOIN NS_KhachHang kh ON c.IDKhachHang = kh.ID
    LEFT JOIN NS_NhanSu ns ON c.NguoiTao = ns.ID
    LEFT JOIN NS_DonDatHang d ON c.IDDonDatHang = d.ID
    OUTER APPLY (
        SELECT SUM(p.SoTienThu) AS DaThanhToan
        FROM BAN_PhieuThuKhachHang p
        WHERE p.IDChungTuBanHang = c.ID 
          AND p.TrangThai = 2 -- Đã ghi
          AND p.IsDeleted = 0
    ) pt
    WHERE c.IsDeleted = 0
      AND c.TrangThai = 2 -- Chỉ hiển thị chứng từ bán hàng đã ghi sổ
      AND (@TuNgay IS NULL OR c.NgayChungTu >= @TuNgay)
      AND (@DenNgay IS NULL OR c.NgayChungTu <= @DenNgay)
      AND (@SoChungTu IS NULL OR c.SoChungTu LIKE '%' + @SoChungTu + '%' OR d.SoDonHang LIKE '%' + @SoChungTu + '%')
      AND (@IDKhachHang IS NULL OR c.IDKhachHang = @IDKhachHang)
      AND (@TrangThaiCongNo IS NULL OR @TrangThaiCongNo = 0 OR (
            CASE 
                WHEN ISNULL(pt.DaThanhToan, 0) = 0 THEN 1
                WHEN c.TongCong - ISNULL(pt.DaThanhToan, 0) <= 0 THEN 3
                ELSE 2
            END = @TrangThaiCongNo
      ))
    ORDER BY c.NgayChungTu DESC, c.ID DESC;
END
GO

-- sp_BAN_PhieuThuKhachHang_GetByID
CREATE OR ALTER PROCEDURE sp_BAN_PhieuThuKhachHang_GetByID
    @ID INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        pt.ID,
        pt.SoPhieuThu,
        pt.NgayThu,
        pt.IDChungTuBanHang,
        ct.SoChungTu AS SoChungTuBanHang,
        pt.IDKhachHang,
        kh.TenKhachHang,
        pt.IDTaiKhoanThanhToan,
        tk.SoTaiKhoan,
        tk.TenTaiKhoan,
        pt.SoTienThu,
        pt.GhiChu,
        pt.TrangThai,
        pt.NgayTao,
        pt.NguoiTao,
        pt.NgayCapNhat,
        pt.NguoiCapNhat,
        pt.NgayGhi,
        pt.NguoiGhi,
        pt.NgayHuy,
        pt.NguoiHuy,
        pt.LyDoHuy,
        ct.TongCong AS TongChungTu,
        -- Đã thanh toán trước đó
        ISNULL((
            SELECT SUM(p.SoTienThu) 
            FROM BAN_PhieuThuKhachHang p 
            WHERE p.IDChungTuBanHang = pt.IDChungTuBanHang 
              AND p.TrangThai = 2 
              AND p.IsDeleted = 0 
              AND p.ID <> pt.ID
        ), 0) AS DaThanhToanTruoc,
        -- Còn lại sau thu
        (ct.TongCong - ISNULL((
            SELECT SUM(p.SoTienThu) 
            FROM BAN_PhieuThuKhachHang p 
            WHERE p.IDChungTuBanHang = pt.IDChungTuBanHang 
              AND p.TrangThai = 2 
              AND p.IsDeleted = 0 
              AND p.ID <> pt.ID
        ), 0) - pt.SoTienThu) AS ConLaiSauThu,
        ns.HoDem + ' ' + ns.Ten AS TenNguoiTao
    FROM BAN_PhieuThuKhachHang pt
    JOIN BAN_ChungTuBanHang ct ON pt.IDChungTuBanHang = ct.ID
    JOIN NS_KhachHang kh ON pt.IDKhachHang = kh.ID
    JOIN KT_TaiKhoanKeToan tk ON pt.IDTaiKhoanThanhToan = tk.ID
    LEFT JOIN NhanSu ns ON pt.NguoiTao = ns.ID
    WHERE pt.ID = @ID AND pt.IsDeleted = 0;
END
GO

-- sp_BAN_PhieuThuKhachHang_Save
CREATE OR ALTER PROCEDURE sp_BAN_PhieuThuKhachHang_Save
    @ID INT = NULL,
    @SoPhieuThu NVARCHAR(50),
    @NgayThu DATE,
    @IDChungTuBanHang INT,
    @IDKhachHang INT,
    @IDTaiKhoanThanhToan INT,
    @SoTienThu DECIMAL(18,2),
    @GhiChu NVARCHAR(1000) = NULL,
    @TrangThai INT,
    @UserId INT,
    @NewID INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    IF @ID IS NULL OR @ID = 0
    BEGIN
        INSERT INTO BAN_PhieuThuKhachHang (
            SoPhieuThu, NgayThu, IDChungTuBanHang, IDKhachHang, IDTaiKhoanThanhToan, 
            SoTienThu, GhiChu, TrangThai, NgayTao, NguoiTao, IsDeleted
        )
        VALUES (
            @SoPhieuThu, @NgayThu, @IDChungTuBanHang, @IDKhachHang, @IDTaiKhoanThanhToan, 
            @SoTienThu, @GhiChu, @TrangThai, GETDATE(), @UserId, 0
        );
        SET @NewID = SCOPE_IDENTITY();
    END
    ELSE
    BEGIN
        UPDATE BAN_PhieuThuKhachHang
        SET 
            NgayThu = @NgayThu,
            IDChungTuBanHang = @IDChungTuBanHang,
            IDKhachHang = @IDKhachHang,
            IDTaiKhoanThanhToan = @IDTaiKhoanThanhToan,
            SoTienThu = @SoTienThu,
            GhiChu = @GhiChu,
            TrangThai = @TrangThai,
            NgayCapNhat = GETDATE(),
            NguoiCapNhat = @UserId
        WHERE ID = @ID AND IsDeleted = 0;
        
        SET @NewID = @ID;
    END
END
GO

-- sp_BAN_PhieuThuKhachHang_Ghi
CREATE OR ALTER PROCEDURE sp_BAN_PhieuThuKhachHang_Ghi
    @ID INT,
    @UserId INT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRANSACTION;
    BEGIN TRY
        -- 1. Lấy thông tin phiếu thu
        DECLARE @IDChungTuBanHang INT, @IDKhachHang INT, @IDTaiKhoanThanhToan INT, @SoTienThu DECIMAL(18,2), @SoPhieuThu NVARCHAR(50), @NgayThu DATE;
        DECLARE @TrangThai INT;

        SELECT 
            @IDChungTuBanHang = IDChungTuBanHang,
            @IDKhachHang = IDKhachHang,
            @IDTaiKhoanThanhToan = IDTaiKhoanThanhToan,
            @SoTienThu = SoTienThu,
            @SoPhieuThu = SoPhieuThu,
            @NgayThu = NgayThu,
            @TrangThai = TrangThai
        FROM BAN_PhieuThuKhachHang
        WHERE ID = @ID AND IsDeleted = 0;

        IF @IDChungTuBanHang IS NULL
        BEGIN
            THROW 50001, N'Phiếu thu không tồn tại.', 1;
        END

        IF @TrangThai <> 1
        BEGIN
            THROW 50002, N'Trạng thái phiếu thu không hợp lệ (phải là Đề nghị ghi).', 1;
        END

        -- Lấy số tài khoản thanh toán
        DECLARE @TaiKhoanNo NVARCHAR(20);
        SELECT @TaiKhoanNo = SoTaiKhoan FROM KT_TaiKhoanKeToan WHERE ID = @IDTaiKhoanThanhToan;
        IF @TaiKhoanNo IS NULL
        BEGIN
            THROW 50003, N'Tài khoản thanh toán không tồn tại.', 1;
        END

        -- 2. Kiểm tra chứng từ bán hàng
        DECLARE @TongCong DECIMAL(18,2), @SoChungTuBanHang NVARCHAR(50), @TrangThaiCTBH INT;
        SELECT 
            @TongCong = TongCong, 
            @SoChungTuBanHang = SoChungTu,
            @TrangThaiCTBH = TrangThai
        FROM BAN_ChungTuBanHang 
        WHERE ID = @IDChungTuBanHang AND IsDeleted = 0;

        IF @TongCong IS NULL
        BEGIN
            THROW 50004, N'Chứng từ bán hàng không tồn tại.', 1;
        END

        IF @TrangThaiCTBH <> 2
        BEGIN
            THROW 50005, N'Chứng từ bán hàng chưa được ghi sổ.', 1;
        END

        -- 3. Tính toán công nợ hiện tại
        DECLARE @DaThanhToanTruoc DECIMAL(18,2);
        SELECT @DaThanhToanTruoc = ISNULL(SUM(SoTienThu), 0)
        FROM BAN_PhieuThuKhachHang
        WHERE IDChungTuBanHang = @IDChungTuBanHang 
          AND TrangThai = 2 
          AND IsDeleted = 0 
          AND ID <> @ID;

        IF @DaThanhToanTruoc + @SoTienThu > @TongCong
        BEGIN
            DECLARE @ErrStr NVARCHAR(500) = N'Số tiền thu vượt quá số tiền còn phải thu. Tổng cộng: ' + CAST(@TongCong AS NVARCHAR(50)) + N', Đã thu trước đó: ' + CAST(@DaThanhToanTruoc AS NVARCHAR(50)) + N', Đang thu lần này: ' + CAST(@SoTienThu AS NVARCHAR(50));
            THROW 50006, @ErrStr, 1;
        END

        -- 4. Cập nhật trạng thái phiếu thu
        UPDATE BAN_PhieuThuKhachHang
        SET TrangThai = 2,
            NgayGhi = GETDATE(),
            NguoiGhi = @UserId,
            NgayCapNhat = GETDATE(),
            NguoiCapNhat = @UserId
        WHERE ID = @ID;

        -- 5. Cập nhật lại công nợ chứng từ bán hàng
        DECLARE @NewDaThanhToan DECIMAL(18,2) = @DaThanhToanTruoc + @SoTienThu;
        DECLARE @NewConLai DECIMAL(18,2) = @TongCong - @NewDaThanhToan;

        UPDATE BAN_ChungTuBanHang
        SET DaThanhToan = @NewDaThanhToan,
            ConLai = @NewConLai
        WHERE ID = @IDChungTuBanHang;

        -- 6. Ghi sổ kế toán KT_NhatKyChung (Nợ 1111/1121/113, Có 131)
        IF NOT EXISTS (SELECT 1 FROM KT_NhatKyChung WHERE LoaiChungTu = 'PHIEU_THU' AND IDChungTu = @ID AND IsHuy = 0)
        BEGIN
            INSERT INTO KT_NhatKyChung (
                NgayChungTu, SoChungTu, LoaiChungTu, IDChungTu, 
                TaiKhoanNo, TaiKhoanCo, SoTien, DienGiai, NgayTao, NguoiTao, IsHuy
            )
            VALUES (
                @NgayThu, @SoPhieuThu, 'PHIEU_THU', @ID,
                @TaiKhoanNo, '131', @SoTienThu, N'Thu tiền khách hàng theo chứng từ ' + @SoChungTuBanHang, 
                GETDATE(), @UserId, 0
            );
        END

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

-- sp_BAN_PhieuThuKhachHang_Huy
CREATE OR ALTER PROCEDURE sp_BAN_PhieuThuKhachHang_Huy
    @ID INT,
    @UserId INT,
    @LyDoHuy NVARCHAR(500)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRANSACTION;
    BEGIN TRY
        -- 1. Lấy thông tin phiếu thu
        DECLARE @IDChungTuBanHang INT, @SoTienThu DECIMAL(18,2), @TrangThai INT;
        SELECT 
            @IDChungTuBanHang = IDChungTuBanHang,
            @SoTienThu = SoTienThu,
            @TrangThai = TrangThai
        FROM BAN_PhieuThuKhachHang
        WHERE ID = @ID AND IsDeleted = 0;

        IF @TrangThai IS NULL
        BEGIN
            THROW 50001, N'Phiếu thu không tồn tại.', 1;
        END

        IF @TrangThai = 3
        BEGIN
            THROW 50002, N'Phiếu thu đã được hủy trước đó.', 1;
        END

        -- 2. Cập nhật trạng thái phiếu thu sang 3 (Đã hủy)
        UPDATE BAN_PhieuThuKhachHang
        SET TrangThai = 3,
            NgayHuy = GETDATE(),
            NguoiHuy = @UserId,
            LyDoHuy = @LyDoHuy,
            NgayCapNhat = GETDATE(),
            NguoiCapNhat = @UserId
        WHERE ID = @ID;

        IF @TrangThai = 2 -- Nếu đã ghi sổ thì hoàn sổ
        BEGIN
            -- 3. Đánh dấu hủy sổ nhật ký chung
            UPDATE KT_NhatKyChung
            SET IsHuy = 1,
                NgayHuy = GETDATE(),
                NguoiHuy = @UserId,
                LyDoHuy = @LyDoHuy
            WHERE LoaiChungTu = 'PHIEU_THU' AND IDChungTu = @ID;

            -- 4. Tính toán lại công nợ chứng từ bán hàng
            DECLARE @TongCong DECIMAL(18,2);
            SELECT @TongCong = TongCong FROM BAN_ChungTuBanHang WHERE ID = @IDChungTuBanHang;

            DECLARE @DaThanhToanMoi DECIMAL(18,2);
            SELECT @DaThanhToanMoi = ISNULL(SUM(SoTienThu), 0)
            FROM BAN_PhieuThuKhachHang
            WHERE IDChungTuBanHang = @IDChungTuBanHang 
              AND TrangThai = 2 
              AND IsDeleted = 0;

            DECLARE @ConLaiMoi DECIMAL(18,2) = @TongCong - @DaThanhToanMoi;

            UPDATE BAN_ChungTuBanHang
            SET DaThanhToan = @DaThanhToanMoi,
                ConLai = @ConLaiMoi
            WHERE ID = @IDChungTuBanHang;
        END

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

-- sp_BAN_PhieuThuKhachHang_Delete
CREATE OR ALTER PROCEDURE sp_BAN_PhieuThuKhachHang_Delete
    @ID INT,
    @UserId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @TrangThai INT;
    SELECT @TrangThai = TrangThai FROM BAN_PhieuThuKhachHang WHERE ID = @ID AND IsDeleted = 0;

    IF @TrangThai IS NULL
    BEGIN
        THROW 50001, N'Phiếu thu không tồn tại.', 1;
    END

    IF @TrangThai = 2
    BEGIN
        THROW 50002, N'Không thể xóa phiếu thu đã ghi sổ. Vui lòng hủy phiếu thu trước.', 1;
    END

    UPDATE BAN_PhieuThuKhachHang
    SET IsDeleted = 1,
        NgayCapNhat = GETDATE(),
        NguoiCapNhat = @UserId
    WHERE ID = @ID;
END
GO

-- sp_BAN_PhieuThuKhachHang_GenerateSoPhieuThu
CREATE OR ALTER PROCEDURE sp_BAN_PhieuThuKhachHang_GenerateSoPhieuThu
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Prefix NVARCHAR(10) = 'PT' + RIGHT(CAST(YEAR(GETDATE()) AS NVARCHAR(4)), 2);
    DECLARE @LastSoPhieu NVARCHAR(50);
    
    SELECT TOP 1 @LastSoPhieu = SoPhieuThu 
    FROM BAN_PhieuThuKhachHang 
    WHERE SoPhieuThu LIKE @Prefix + '%'
    ORDER BY ID DESC;

    IF @LastSoPhieu IS NULL
    BEGIN
        SELECT @Prefix + '000001' AS SoPhieuThu;
    END
    ELSE
    BEGIN
        DECLARE @LastNum INT;
        SET @LastNum = CAST(RIGHT(@LastSoPhieu, 6) AS INT);
        SELECT @Prefix + RIGHT('000000' + CAST(@LastNum + 1 AS NVARCHAR(6)), 6) AS SoPhieuThu;
    END
END
GO

-- sp_BAN_ChungTuBanHang_GetCongNoForDropdown
CREATE OR ALTER PROCEDURE sp_BAN_ChungTuBanHang_GetCongNoForDropdown
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        c.ID, 
        ISNULL(d.SoDonHang, c.SoChungTu) AS SoChungTu, 
        c.NgayChungTu, 
        c.IDKhachHang, 
        kh.TenKhachHang, 
        c.TongCong, 
        c.DaThanhToan, 
        c.ConLai
    FROM BAN_ChungTuBanHang c
    JOIN NS_KhachHang kh ON c.IDKhachHang = kh.ID
    LEFT JOIN NS_DonDatHang d ON c.IDDonDatHang = d.ID
    WHERE c.TrangThai = 2 
      AND c.IsDeleted = 0
      AND c.ConLai > 0
    ORDER BY c.NgayChungTu DESC, c.ID DESC;
END
GO

-- sp_BAN_ChungTuBanHang_GetCongNoByID
CREATE OR ALTER PROCEDURE sp_BAN_ChungTuBanHang_GetCongNoByID
    @ID INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        c.ID, 
        ISNULL(d.SoDonHang, c.SoChungTu) AS SoChungTu, 
        c.NgayChungTu, 
        c.IDKhachHang, 
        kh.TenKhachHang, 
        c.TongCong, 
        c.DaThanhToan, 
        c.ConLai
    FROM BAN_ChungTuBanHang c
    JOIN NS_KhachHang kh ON c.IDKhachHang = kh.ID
    LEFT JOIN NS_DonDatHang d ON c.IDDonDatHang = d.ID
    WHERE c.ID = @ID 
      AND c.IsDeleted = 0;
END
GO

-- sp_KT_TaiKhoanKeToan_GetThanhToanDropdown
CREATE OR ALTER PROCEDURE sp_KT_TaiKhoanKeToan_GetThanhToanDropdown
AS
BEGIN
    SET NOCOUNT ON;

    SELECT ID, SoTaiKhoan, TenTaiKhoan, SoTaiKhoan + ' - ' + TenTaiKhoan AS TenHienThi
    FROM KT_TaiKhoanKeToan
    WHERE IsChiTiet = 1 
      AND (SoTaiKhoan LIKE '111%' OR SoTaiKhoan LIKE '112%' OR SoTaiKhoan LIKE '113%')
    ORDER BY SoTaiKhoan;
END
GO
