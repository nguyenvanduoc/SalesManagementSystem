-- =============================================
-- Author:      Antigravity
-- Create date: 2026-06-26
-- Description: Tạo bảng lưu lịch sử điều chỉnh phiếu nhập kho
-- =============================================

-- 1. Tạo bảng KHO_DieuChinhPhieuNhap
IF OBJECT_ID('KHO_DieuChinhPhieuNhap') IS NULL
BEGIN
    CREATE TABLE KHO_DieuChinhPhieuNhap
    (
        ID INT IDENTITY PRIMARY KEY,
        IDPhieuNhap INT NOT NULL,
        SoDieuChinh NVARCHAR(50) NOT NULL,
        NgayDieuChinh DATETIME NOT NULL,
        LyDoDieuChinh NVARCHAR(1000) NOT NULL,
        TongTienCu DECIMAL(18,2) NOT NULL DEFAULT 0,
        TongTienMoi DECIMAL(18,2) NOT NULL DEFAULT 0,
        ChenhLech DECIMAL(18,2) NOT NULL DEFAULT 0,
        NgayTao DATETIME NULL,
        NguoiTao INT NULL
    );
END
GO

-- 2. Tạo bảng KHO_DieuChinhPhieuNhapChiTiet
IF OBJECT_ID('KHO_DieuChinhPhieuNhapChiTiet') IS NULL
BEGIN
    CREATE TABLE KHO_DieuChinhPhieuNhapChiTiet
    (
        ID INT IDENTITY PRIMARY KEY,
        IDDieuChinh INT NOT NULL,
        IDPhieuNhapChiTiet INT NOT NULL,
        IDSanPhamCu INT NULL,
        IDSanPhamMoi INT NULL,
        SoLuongCu DECIMAL(18,2) NULL,
        SoLuongMoi DECIMAL(18,2) NULL,
        DonGiaCu DECIMAL(18,2) NULL,
        DonGiaMoi DECIMAL(18,2) NULL,
        ThanhTienCu DECIMAL(18,2) NULL,
        ThanhTienMoi DECIMAL(18,2) NULL,
        LoaiThayDoi NVARCHAR(20) NULL,
        NgayTao DATETIME NULL,
        NguoiTao INT NULL
    );
END
GO

-- 3. Đăng ký màn hình trong ACL_ManHinh
DECLARE @ManHinhID INT;
IF NOT EXISTS (SELECT 1 FROM ACL_ManHinh WHERE TenManHinh = N'Điều chỉnh nhập kho')
BEGIN
    -- Nhóm KHO HÀNG
    INSERT INTO ACL_ManHinh (TenManHinh, NhomChaManHinh, IsSuDung, STT)
    VALUES (N'Điều chỉnh nhập kho', N'KHO HANG', 1, 1029);
    SET @ManHinhID = SCOPE_IDENTITY();
END
ELSE
BEGIN
    SELECT @ManHinhID = ID FROM ACL_ManHinh WHERE TenManHinh = N'Điều chỉnh nhập kho';
END

-- Đăng ký các action cho màn hình
IF NOT EXISTS (SELECT 1 FROM ACL_Action WHERE IDManHinh = @ManHinhID AND TenAction = 'Index')
    INSERT INTO ACL_Action (IDManHinh, TenAction, TenController, LoaiPhanQuyen, GhiChu)
    VALUES (@ManHinhID, 'Index', 'DieuChinhNhapKho', 1, N'Xem danh sách phiếu nhập kho');

IF NOT EXISTS (SELECT 1 FROM ACL_Action WHERE IDManHinh = @ManHinhID AND TenAction = 'Adjust')
    INSERT INTO ACL_Action (IDManHinh, TenAction, TenController, LoaiPhanQuyen, GhiChu)
    VALUES (@ManHinhID, 'Adjust', 'DieuChinhNhapKho', 3, N'Thực hiện điều chỉnh phiếu nhập kho');

IF NOT EXISTS (SELECT 1 FROM ACL_Action WHERE IDManHinh = @ManHinhID AND TenAction = 'History')
    INSERT INTO ACL_Action (IDManHinh, TenAction, TenController, LoaiPhanQuyen, GhiChu)
    VALUES (@ManHinhID, 'History', 'DieuChinhNhapKho', 1, N'Xem lịch sử điều chỉnh');

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

-- =============================================
-- 4. STORED PROCEDURE: sp_KHO_DieuChinhPhieuNhap_Save
-- =============================================
IF OBJECT_ID('sp_KHO_DieuChinhPhieuNhap_Save', 'P') IS NOT NULL
    DROP PROCEDURE sp_KHO_DieuChinhPhieuNhap_Save;
GO
CREATE PROCEDURE sp_KHO_DieuChinhPhieuNhap_Save
    @IDPhieuNhap INT,
    @LyDoDieuChinh NVARCHAR(1000),
    @ChiTietsJson NVARCHAR(MAX),
    @IDLoaiNhapKho INT,
    @IDKho INT,
    @IDKhoNguon INT = NULL,
    @IDNhaCungCap INT = NULL,
    @IDKhachHang INT = NULL,
    @IDPhuongTien INT = NULL,
    @NgayNhap DATETIME,
    @NgayGiaoHang DATETIME = NULL,
    @HoTenTaiXe NVARCHAR(255) = NULL,
    @SoDienThoaiTaiXe NVARCHAR(50) = NULL,
    @SoHoaDon NVARCHAR(50) = NULL,
    @NgayHoaDon DATETIME = NULL,
    @GhiChu NVARCHAR(500) = NULL,
    @NguoiTao INT,
    @TenNguoiNhan NVARCHAR(200) = NULL,
    @TenNguoiGiao NVARCHAR(200) = NULL,
    @SoDienThoaiNguoiGiao NVARCHAR(50) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    -- 1. Lấy thông tin phiếu nhập hiện tại
    DECLARE @SoChungTu NVARCHAR(50);
    DECLARE @TrangThai INT;
    DECLARE @TongTienCu DECIMAL(18,2);
    DECLARE @OldIDLoaiNhapKho INT;
    DECLARE @OldIDKho INT;
    DECLARE @OldIDKhoNguon INT;

    SELECT 
        @SoChungTu = SoChungTu,
        @TrangThai = TrangThai,
        @OldIDLoaiNhapKho = IDLoaiNhapKho,
        @OldIDKho = IDKho,
        @OldIDKhoNguon = IDKhoNguon
    FROM KHO_PhieuNhap
    WHERE ID = @IDPhieuNhap AND IsDeleted = 0;

    IF @SoChungTu IS NULL
    BEGIN
        THROW 50001, N'Không tìm thấy phiếu nhập gốc hoặc phiếu đã bị xóa.', 1;
    END

    IF @TrangThai NOT IN (2) -- Chỉ phiếu đã ghi (trạng thái = 2) mới được phép
    BEGIN
        THROW 50002, N'Chỉ phiếu nhập đã ghi sổ mới được điều chỉnh qua module này.', 1;
    END

    -- Get Old Total
    SELECT @TongTienCu = ISNULL(SUM(TongSauThue), 0)
    FROM KHO_PhieuNhap_ChiTiet
    WHERE IDPhieuNhap = @IDPhieuNhap;

    -- Lấy mã loại nhập kho
    DECLARE @MaLoaiNhap NVARCHAR(50);
    SELECT @MaLoaiNhap = MaLoaiNhap FROM DM_LoaiNhapKho WHERE ID = @IDLoaiNhapKho;

    -- 2. Parse chi tiết mới từ JSON
    DECLARE @ChiTietMoi TABLE (
        IDSanPham INT,
        SoLuong DECIMAL(18,2),
        DonGia DECIMAL(18,2),
        ThueGTGT DECIMAL(18,2),
        ThanhTien DECIMAL(18,2),
        TienThue DECIMAL(18,2),
        TongSauThue DECIMAL(18,2),
        DonGiaVanChuyen DECIMAL(18,2) DEFAULT 0,
        TienVanChuyen DECIMAL(18,2) DEFAULT 0,
        GhiChu NVARCHAR(500),
        NgaySanXuat DATETIME NULL,
        HanSuDung DATETIME NULL
    );

    INSERT INTO @ChiTietMoi (IDSanPham, SoLuong, DonGia, ThueGTGT, ThanhTien, TienThue, TongSauThue, DonGiaVanChuyen, TienVanChuyen, GhiChu, NgaySanXuat, HanSuDung)
    SELECT 
        ISNULL(IDSanPham, 0),
        ROUND(CASE WHEN SoLuong >= 0 THEN SoLuong ELSE 0 END, 2),
        ROUND(CASE WHEN DonGia >= 0 THEN DonGia ELSE 0 END, 2),
        ROUND(CASE WHEN ThueGTGT >= 0 THEN ThueGTGT ELSE 0 END, 2),
        ROUND(CASE WHEN SoLuong >= 0 THEN SoLuong ELSE 0 END * CASE WHEN DonGia >= 0 THEN DonGia ELSE 0 END, 0) AS ThanhTien,
        ROUND(ROUND(CASE WHEN SoLuong >= 0 THEN SoLuong ELSE 0 END * CASE WHEN DonGia >= 0 THEN DonGia ELSE 0 END, 0) * CASE WHEN ThueGTGT >= 0 THEN ThueGTGT ELSE 0 END / 100, 0) AS TienThue,
        ROUND(CASE WHEN SoLuong >= 0 THEN SoLuong ELSE 0 END * CASE WHEN DonGia >= 0 THEN DonGia ELSE 0 END, 0) + 
        ROUND(ROUND(CASE WHEN SoLuong >= 0 THEN SoLuong ELSE 0 END * CASE WHEN DonGia >= 0 THEN DonGia ELSE 0 END, 0) * CASE WHEN ThueGTGT >= 0 THEN ThueGTGT ELSE 0 END / 100, 0) AS TongSauThue,
        ISNULL(DonGiaVanChuyen, 0),
        ISNULL(DonGiaVanChuyen, 0) * ROUND(CASE WHEN SoLuong >= 0 THEN SoLuong ELSE 0 END, 2) AS TienVanChuyen,
        GhiChu,
        NgaySanXuat,
        HanSuDung
    FROM OPENJSON(@ChiTietsJson)
    WITH (
        IDSanPham INT '$.IDSanPham',
        SoLuong DECIMAL(18,2) '$.SoLuong',
        DonGia DECIMAL(18,2) '$.DonGia',
        ThueGTGT DECIMAL(18,2) '$.ThueGTGT',
        DonGiaVanChuyen DECIMAL(18,2) '$.DonGiaVanChuyen',
        GhiChu NVARCHAR(500) '$.GhiChu',
        NgaySanXuat DATETIME '$.NgaySanXuat',
        HanSuDung DATETIME '$.HanSuDung'
    );

    -- Tính toán tổng tiền mới
    DECLARE @newTongTien DECIMAL(18,2);
    SELECT @newTongTien = ISNULL(SUM(TongSauThue), 0) FROM @ChiTietMoi;

    DECLARE @newTienHang DECIMAL(18,2);
    SELECT @newTienHang = ISNULL(SUM(ThanhTien), 0) FROM @ChiTietMoi;

    DECLARE @newTienThue DECIMAL(18,2);
    SELECT @newTienThue = ISNULL(SUM(TienThue), 0) FROM @ChiTietMoi;

    DECLARE @ChenhLech DECIMAL(18,2) = @newTongTien - @TongTienCu;

    -- 3. Sinh số điều chỉnh
    DECLARE @adjCount INT;
    SELECT @adjCount = COUNT(1) FROM KHO_DieuChinhPhieuNhap WHERE IDPhieuNhap = @IDPhieuNhap;
    DECLARE @soDieuChinh NVARCHAR(50) = N'DC-' + @SoChungTu + N'-' + RIGHT('00' + CAST(@adjCount + 1 AS NVARCHAR(10)), 2);

    BEGIN TRANSACTION;
    BEGIN TRY
        -- 4. Lưu header Điều chỉnh
        DECLARE @idDieuChinh INT;
        INSERT INTO KHO_DieuChinhPhieuNhap 
            (IDPhieuNhap, SoDieuChinh, NgayDieuChinh, LyDoDieuChinh, TongTienCu, TongTienMoi, ChenhLech, NgayTao, NguoiTao)
        VALUES 
            (@IDPhieuNhap, @soDieuChinh, GETDATE(), @LyDoDieuChinh, @TongTienCu, @newTongTien, @ChenhLech, GETDATE(), @NguoiTao);
        SET @idDieuChinh = SCOPE_IDENTITY();

        -- 5. Lấy tập hợp tất cả các ID sản phẩm tham gia (trước và sau)
        DECLARE @allSpIds TABLE (IDSanPham INT PRIMARY KEY);
        INSERT INTO @allSpIds (IDSanPham)
        SELECT DISTINCT IDSanPham FROM KHO_PhieuNhap_ChiTiet WHERE IDPhieuNhap = @IDPhieuNhap AND IDSanPham IS NOT NULL
        UNION
        SELECT DISTINCT IDSanPham FROM @ChiTietMoi WHERE IDSanPham IS NOT NULL;

        -- Bảng tạm để lưu trữ chi tiết mới đã được xử lý ở trên

        -- Duyệt qua từng sản phẩm để so sánh và ghi nhận điều chỉnh
        DECLARE @spId INT;
        DECLARE db_cursor CURSOR LOCAL FOR SELECT IDSanPham FROM @allSpIds;
        OPEN db_cursor;
        FETCH NEXT FROM db_cursor INTO @spId;

        WHILE @@FETCH_STATUS = 0
        BEGIN
            DECLARE @slCu DECIMAL(18,2) = NULL, @slMoi DECIMAL(18,2) = NULL;
            DECLARE @dgCu DECIMAL(18,2) = NULL, @dgMoi DECIMAL(18,2) = NULL;
            DECLARE @ttCu DECIMAL(18,2) = NULL, @ttMoi DECIMAL(18,2) = NULL;
            DECLARE @itemLoaiThayDoi NVARCHAR(20) = N'KhongDoi';

            -- Lấy thông tin cũ
            SELECT 
                @slCu = SUM(SoLuong),
                @dgCu = MAX(DonGia),
                @ttCu = SUM(TongSauThue)
            FROM KHO_PhieuNhap_ChiTiet
            WHERE IDPhieuNhap = @IDPhieuNhap AND IDSanPham = @spId;

            -- Lấy thông tin mới
            SELECT 
                @slMoi = SUM(SoLuong),
                @dgMoi = MAX(DonGia),
                @ttMoi = SUM(TongSauThue)
            FROM @ChiTietMoi
            WHERE IDSanPham = @spId;

            IF @slCu IS NOT NULL AND @slMoi IS NULL SET @itemLoaiThayDoi = N'Xoa';
            ELSE IF @slCu IS NULL AND @slMoi IS NOT NULL SET @itemLoaiThayDoi = N'ThemMoi';
            ELSE IF ISNULL(@slCu, 0) <> ISNULL(@slMoi, 0) OR ISNULL(@dgCu, 0) <> ISNULL(@dgMoi, 0) OR ISNULL(@ttCu, 0) <> ISNULL(@ttMoi, 0)
                SET @itemLoaiThayDoi = N'CapNhat';

            -- Kiểm tra xem kho có bị thay đổi không
            DECLARE @isKhoChanged BIT = 0;
            IF ISNULL(@OldIDKho, 0) <> ISNULL(@IDKho, 0) OR ISNULL(@OldIDKhoNguon, 0) <> ISNULL(@IDKhoNguon, 0)
                SET @isKhoChanged = 1;

            -- Chỉ ghi nhận dòng có thay đổi hoặc nếu kho bị thay đổi (để lưu lịch sử)
            IF @itemLoaiThayDoi <> N'KhongDoi' OR @isKhoChanged = 1
            BEGIN
                IF @isKhoChanged = 1 AND @itemLoaiThayDoi = N'KhongDoi'
                    SET @itemLoaiThayDoi = N'DoiKho';

                INSERT INTO KHO_DieuChinhPhieuNhapChiTiet
                    (IDDieuChinh, IDPhieuNhapChiTiet, IDSanPhamCu, IDSanPhamMoi, SoLuongCu, SoLuongMoi, DonGiaCu, DonGiaMoi, ThanhTienCu, ThanhTienMoi, LoaiThayDoi, NgayTao, NguoiTao)
                VALUES
                    (@idDieuChinh, 0, CASE WHEN @slCu IS NOT NULL THEN @spId ELSE NULL END, CASE WHEN @slMoi IS NOT NULL THEN @spId ELSE NULL END, @slCu, @slMoi, @dgCu, @dgMoi, @ttCu, @ttMoi, @itemLoaiThayDoi, GETDATE(), @NguoiTao);
            END

            FETCH NEXT FROM db_cursor INTO @spId;
        END

        CLOSE db_cursor;
        DEALLOCATE db_cursor;

        -- 6. Cập nhật bảng gốc KHO_PhieuNhap & KHO_PhieuNhap_ChiTiet
        UPDATE KHO_PhieuNhap SET
            IDLoaiNhapKho = @IDLoaiNhapKho,
            IDKho = @IDKho,
            IDKhoNguon = @IDKhoNguon,
            IDNhaCungCap = @IDNhaCungCap,
            IDKhachHang = @IDKhachHang,
            IDPhuongTien = @IDPhuongTien,
            NgayNhap = @NgayNhap,
            NgayGiaoHang = @NgayGiaoHang,
            HoTenTaiXe = @HoTenTaiXe,
            SoDienThoaiTaiXe = @SoDienThoaiTaiXe,
            SoHoaDon = @SoHoaDon,
            NgayHoaDon = @NgayHoaDon,
            GhiChu = @GhiChu,
            TenNguoiNhan = @TenNguoiNhan,
            TenNguoiGiao = @TenNguoiGiao,
            SoDienThoaiNguoiGiao = @SoDienThoaiNguoiGiao,
            NgayCapNhat = GETDATE(),
            NguoiCapNhat = @NguoiTao,
            TongTienHang = @newTienHang,
            TongTienThue = @newTienThue,
            TongCong = @newTongTien,
            TienVanChuyen = ISNULL((SELECT SUM(ISNULL(TienVanChuyen, 0)) FROM @ChiTietMoi), 0),
            ConLai = ISNULL(ConLai, 0) + @ChenhLech
        WHERE ID = @IDPhieuNhap;

        DELETE FROM KHO_PhieuNhap_ChiTiet WHERE IDPhieuNhap = @IDPhieuNhap;

        INSERT INTO KHO_PhieuNhap_ChiTiet
            (IDPhieuNhap, IDSanPham, SoLuong, DonGia, ThanhTien, ThueGTGT, TienThue, TongSauThue, DonGiaVanChuyen, TienVanChuyen, GhiChu, NgaySanXuat, HanSuDung)
        SELECT 
            @IDPhieuNhap, IDSanPham, SoLuong, DonGia, ThanhTien, ThueGTGT, TienThue, TongSauThue, ISNULL(DonGiaVanChuyen, 0), ISNULL(TienVanChuyen, 0), GhiChu, NgaySanXuat, HanSuDung
        FROM @ChiTietMoi;

        -- 7. Cập nhật lại sổ kho KHO_GiaoDichKho theo đúng dữ liệu cuối cùng (Clean Ledger)
        DECLARE @SoChungTuGoc NVARCHAR(50);
        SELECT @SoChungTuGoc = SoChungTu FROM KHO_PhieuNhap WHERE ID = @IDPhieuNhap;

        DECLARE @LoaiChungTuGoc INT;
        SELECT TOP 1 @LoaiChungTuGoc = LoaiChungTu FROM KHO_GiaoDichKho WHERE SoChungTu = @SoChungTuGoc;
        IF @LoaiChungTuGoc IS NULL SET @LoaiChungTuGoc = 2;

        DELETE FROM KHO_GiaoDichKho WHERE SoChungTu = @SoChungTuGoc;

        -- Thêm lại Nhập cho Kho nhận
        INSERT INTO KHO_GiaoDichKho 
            (NgayChungTu, SoChungTu, LoaiChungTu, IDChiTietKho, IDKho, IDSanPham, SoLuongNhap, SoLuongXuat, DonGia, ThanhTien, DienGiai, NgayTao, NguoiTao)
        SELECT 
            @NgayNhap, @SoChungTuGoc, @LoaiChungTuGoc, 0, @IDKho, IDSanPham, SoLuong, 0, DonGia, TongSauThue, N'Điều chỉnh phiếu ' + @SoChungTuGoc, GETDATE(), @NguoiTao
        FROM @ChiTietMoi WHERE SoLuong > 0;

        -- Thêm lại Xuất cho Kho nguồn (nếu là chuyển kho)
        IF @MaLoaiNhap = 'CHUYEN_KHO' AND @IDKhoNguon IS NOT NULL
        BEGIN
            INSERT INTO KHO_GiaoDichKho 
                (NgayChungTu, SoChungTu, LoaiChungTu, IDChiTietKho, IDKho, IDSanPham, SoLuongNhap, SoLuongXuat, DonGia, ThanhTien, DienGiai, NgayTao, NguoiTao)
            SELECT 
                @NgayNhap, @SoChungTuGoc, @LoaiChungTuGoc, 0, @IDKhoNguon, IDSanPham, 0, SoLuong, DonGia, TongSauThue, N'Điều chỉnh phiếu ' + @SoChungTuGoc, GETDATE(), @NguoiTao
            FROM @ChiTietMoi WHERE SoLuong > 0;
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
IF OBJECT_ID('sp_KHO_DieuChinhPhieuNhap_GetPaged', 'P') IS NOT NULL
    DROP PROCEDURE sp_KHO_DieuChinhPhieuNhap_GetPaged;
GO
CREATE PROCEDURE sp_KHO_DieuChinhPhieuNhap_GetPaged
    @TuNgay DATETIME = NULL,
    @DenNgay DATETIME = NULL,
    @IDLoaiNhapKho INT = NULL,
    @IDKho INT = NULL,
    @IDNhaCungCap INT = NULL,
    @IDKhachHang INT = NULL,
    @SoChungTu NVARCHAR(50) = NULL,
    @ChiDonDieuChinh BIT = 0,
    @Offset INT = 0,
    @PageSize INT = 10,
    @TotalRecords INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    -- Loc du lieu vao bang tam
    SELECT pn.ID INTO #FilteredPhieuNhap
    FROM KHO_PhieuNhap pn
    WHERE pn.TrangThai = 2 AND pn.IsDeleted = 0
        AND (@TuNgay IS NULL OR pn.NgayNhap >= @TuNgay)
        AND (@DenNgay IS NULL OR pn.NgayNhap <= @DenNgay)
        AND (@IDLoaiNhapKho IS NULL OR pn.IDLoaiNhapKho = @IDLoaiNhapKho)
        AND (@IDKho IS NULL OR pn.IDKho = @IDKho OR pn.IDKhoNguon = @IDKho)
        AND (@IDNhaCungCap IS NULL OR pn.IDNhaCungCap = @IDNhaCungCap)
        AND (@IDKhachHang IS NULL OR pn.IDKhachHang = @IDKhachHang)
        AND (@SoChungTu IS NULL OR pn.SoChungTu LIKE '%' + @SoChungTu + '%')
        AND (@ChiDonDieuChinh = 0 OR EXISTS (SELECT 1 FROM KHO_DieuChinhPhieuNhap dc WHERE dc.IDPhieuNhap = pn.ID));

    -- Dem tong so ban ghi
    SELECT @TotalRecords = COUNT(1) FROM #FilteredPhieuNhap;

    -- Lay du lieu phan trang
    SELECT
        pn.ID, pn.SoChungTu, pn.NgayNhap, pn.TrangThai,
        pn.IDLoaiNhapKho,
        ln.TenLoaiNhap,
        pn.IDKho,
        k.TenKhoHang AS TenKhoNhap,
        pn.IDKhoNguon,
        kng.TenKhoHang AS TenKhoNguon,
        CASE 
            WHEN ln.MaLoaiNhap = 'NHAP_MUA' THEN ncc.TenNhaCungCap
            WHEN ln.MaLoaiNhap = 'TRA_HANG' THEN kh.TenKhachHang
            ELSE ''
        END AS DoiTuong,
        ISNULL((SELECT SUM(TongSauThue) FROM KHO_PhieuNhap_ChiTiet ct WHERE ct.IDPhieuNhap = pn.ID), 0) AS TongTien,
        ISNULL((
            SELECT SUM(pc.SoTienChi) 
            FROM KT_PhieuChi pc 
            WHERE pc.IDPhieuNhap = pn.ID AND pc.TrangThai = 2 AND pc.IsDeleted = 0
        ), 0) AS DaThanhToan,
        CAST(CASE WHEN EXISTS (SELECT 1 FROM KHO_DieuChinhPhieuNhap dc WHERE dc.IDPhieuNhap = pn.ID) THEN 1 ELSE 0 END AS BIT) AS DaDieuChinh,
        ISNULL((SELECT COUNT(1) FROM KHO_DieuChinhPhieuNhap dc WHERE dc.IDPhieuNhap = pn.ID), 0) AS SoLanDieuChinh,
        (SELECT MAX(dc.NgayDieuChinh) FROM KHO_DieuChinhPhieuNhap dc WHERE dc.IDPhieuNhap = pn.ID) AS NgayDieuChinhCuoi,
        CASE pn.TrangThai WHEN 1 THEN N'Mới tạo' WHEN 2 THEN N'Đã ghi sổ' WHEN 3 THEN N'Đã thanh toán' ELSE N'' END AS TenTrangThai
    FROM KHO_PhieuNhap pn
    INNER JOIN #FilteredPhieuNhap f ON pn.ID = f.ID
    LEFT JOIN DM_LoaiNhapKho ln ON pn.IDLoaiNhapKho = ln.ID
    LEFT JOIN DM_KhoHang k ON pn.IDKho = k.ID
    LEFT JOIN DM_KhoHang kng ON pn.IDKhoNguon = kng.ID
    LEFT JOIN DM_NhaCungCap ncc ON pn.IDNhaCungCap = ncc.ID
    LEFT JOIN NS_KhachHang kh ON pn.IDKhachHang = kh.ID
    ORDER BY pn.ID DESC
    OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;

    DROP TABLE #FilteredPhieuNhap;
END
GO
IF OBJECT_ID('sp_KHO_DieuChinhPhieuNhap_GetHistory', 'P') IS NOT NULL
    DROP PROCEDURE sp_KHO_DieuChinhPhieuNhap_GetHistory;
GO
CREATE PROCEDURE sp_KHO_DieuChinhPhieuNhap_GetHistory
    @IDPhieuNhap INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        dc.ID,
        dc.SoDieuChinh,
        dc.NgayDieuChinh,
        dc.LyDoDieuChinh,
        dc.TongTienCu,
        dc.TongTienMoi,
        dc.ChenhLech,
        ns.HoDem + ' ' + ns.Ten AS TenNguoiTao
    FROM KHO_DieuChinhPhieuNhap dc
    LEFT JOIN NS_NhanSu ns ON dc.NguoiTao = ns.ID
    WHERE dc.IDPhieuNhap = @IDPhieuNhap
    ORDER BY dc.NgayDieuChinh DESC, dc.ID DESC;
END
GO

IF OBJECT_ID('sp_KHO_DieuChinhPhieuNhap_GetHistoryDetail', 'P') IS NOT NULL
    DROP PROCEDURE sp_KHO_DieuChinhPhieuNhap_GetHistoryDetail;
GO
CREATE PROCEDURE sp_KHO_DieuChinhPhieuNhap_GetHistoryDetail
    @IDDieuChinh INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        ISNULL(spMoi.TenSanPham, spCu.TenSanPham) AS TenSanPham,
        ISNULL(spMoi.MaSanPham, spCu.MaSanPham) AS MaSanPham,
        ISNULL(spMoi.DVT, spCu.DVT) AS DVT,
        ct.SoLuongCu,
        ct.SoLuongMoi,
        ct.DonGiaCu,
        ct.DonGiaMoi,
        ct.ThanhTienCu,
        ct.ThanhTienMoi,
        ct.LoaiThayDoi
    FROM KHO_DieuChinhPhieuNhapChiTiet ct
    LEFT JOIN DM_SanPham spCu ON ct.IDSanPhamCu = spCu.ID
    LEFT JOIN DM_SanPham spMoi ON ct.IDSanPhamMoi = spMoi.ID
    WHERE ct.IDDieuChinh = @IDDieuChinh
    ORDER BY ct.ID;
END
GO
