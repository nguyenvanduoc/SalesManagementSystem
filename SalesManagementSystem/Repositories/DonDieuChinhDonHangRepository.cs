    using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Dapper;
using Newtonsoft.Json;
using SalesManagementSystem.Data;
using SalesManagementSystem.Models.Entities;
using SalesManagementSystem.Models.ViewModels;
using SalesManagementSystem.Repositories.Interfaces;

namespace SalesManagementSystem.Repositories
{
    public class DonDieuChinhDonHangRepository : IDonDieuChinhDonHangRepository
    {
        private readonly DbConnectionFactory _db;

        public DonDieuChinhDonHangRepository(DbConnectionFactory db)
        {
            _db = db;
            try
            {
                using (var conn = _db.CreateConnection())
                {
                    string sql = @"
                        IF OBJECT_ID('DON_DieuChinhDonHang') IS NULL
                        BEGIN
                            CREATE TABLE DON_DieuChinhDonHang
                            (
                                ID INT IDENTITY PRIMARY KEY,
                                IDDonHang INT NOT NULL,
                                SoDieuChinh NVARCHAR(50) NOT NULL,
                                NgayDieuChinh DATETIME NOT NULL,
                                LyDoDieuChinh NVARCHAR(1000) NULL,
                                TongTienCu DECIMAL(18,2) NOT NULL DEFAULT 0,
                                TongTienMoi DECIMAL(18,2) NOT NULL DEFAULT 0,
                                NguoiTao INT NULL,
                                NgayTao DATETIME NULL
                            );
                        END

                        IF OBJECT_ID('DON_DieuChinhDonHang_ChiTiet') IS NULL
                        BEGIN
                            CREATE TABLE DON_DieuChinhDonHang_ChiTiet
                            (
                                ID INT IDENTITY PRIMARY KEY,
                                IDDieuChinh INT NOT NULL,
                                IDSanPham INT NOT NULL,
                                SoLuongCu DECIMAL(18,2) NULL,
                                SoLuongMoi DECIMAL(18,2) NULL,
                                DonGiaCu DECIMAL(18,2) NULL,
                                DonGiaMoi DECIMAL(18,2) NULL,
                                ThanhTienCu DECIMAL(18,2) NULL,
                                ThanhTienMoi DECIMAL(18,2) NULL,
                                GhiChu NVARCHAR(500) NULL
                            );
                        END

                        DECLARE @ManHinhID INT;
                        IF NOT EXISTS (SELECT 1 FROM ACL_ManHinh WHERE TenManHinh = N'Điều chỉnh đơn hàng')
                        BEGIN
                            INSERT INTO ACL_ManHinh (TenManHinh, NhomChaManHinh, IsSuDung, STT)
                            VALUES (N'Điều chỉnh đơn hàng', N'BAN HANG', 1, 1028);
                            SET @ManHinhID = SCOPE_IDENTITY();
                        END
                        ELSE
                        BEGIN
                            SELECT @ManHinhID = ID FROM ACL_ManHinh WHERE TenManHinh = N'Điều chỉnh đơn hàng';
                        END

                        IF NOT EXISTS (SELECT 1 FROM ACL_Action WHERE IDManHinh = @ManHinhID AND TenAction = 'Index')
                            INSERT INTO ACL_Action (IDManHinh, TenAction, TenController, LoaiPhanQuyen, GhiChu)
                            VALUES (@ManHinhID, 'Index', 'DonDieuChinhDonHang', 1, N'Xem danh sách điều chỉnh đơn hàng');

                        IF NOT EXISTS (SELECT 1 FROM ACL_Action WHERE IDManHinh = @ManHinhID AND TenAction = 'Adjust')
                            INSERT INTO ACL_Action (IDManHinh, TenAction, TenController, LoaiPhanQuyen, GhiChu)
                            VALUES (@ManHinhID, 'Adjust', 'DonDieuChinhDonHang', 3, N'Thực hiện điều chỉnh đơn hàng');

                        IF NOT EXISTS (SELECT 1 FROM ACL_Action WHERE IDManHinh = @ManHinhID AND TenAction = 'History')
                            INSERT INTO ACL_Action (IDManHinh, TenAction, TenController, LoaiPhanQuyen, GhiChu)
                            VALUES (@ManHinhID, 'History', 'DonDieuChinhDonHang', 1, N'Xem lịch sử điều chỉnh');

                        INSERT INTO ACL_PhanQuyen (IDLogin, IDAction, IsChoPhep, NgayTao)
                        SELECT l.ID, act.ID, 1, GETDATE()
                        FROM ACL_Login l
                        CROSS JOIN ACL_Action act
                        WHERE act.IDManHinh = @ManHinhID
                          AND NOT EXISTS (
                              SELECT 1 FROM ACL_PhanQuyen pq 
                              WHERE pq.IDLogin = l.ID AND pq.IDAction = act.ID
                          );
                    ";
                    conn.Execute(sql);

                    // Tạo Stored Procedure
                    string spSql = @"
CREATE OR ALTER PROCEDURE sp_DON_DieuChinhDonHang_Save
    @IDDonHang INT,
    @LyDoDieuChinh NVARCHAR(1000),
    @ChiTietsJson NVARCHAR(MAX),
    @PhiBocXep DECIMAL(18,2),
    @IDKho INT,
    @NguoiTao INT,
    @IDKhachHang INT = NULL,
    @IDNhanVien INT = NULL,
    @NgayTaoDon DATETIME = NULL,
    @NgayGiaoHang DATETIME = NULL,
    @ThoiHanGiaoHang DATETIME = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    -- 1. Lấy thông tin đơn hàng hiện tại
    DECLARE @SoDonHang NVARCHAR(50);
    DECLARE @TrangThaiDon INT;
    DECLARE @TongTienCu DECIMAL(18,2);

    DECLARE @currentNgayTaoDon DATETIME;
    DECLARE @currentIDNhanVien INT;
    DECLARE @currentThoiHanGiaoHang DATETIME;
    DECLARE @currentIDKhachHang INT;

    SELECT 
        @SoDonHang = SoDonHang,
        @currentNgayTaoDon = NgayTaoDon,
        @currentIDNhanVien = IDNhanVien,
        @currentThoiHanGiaoHang = ThoiHanGiaoHang,
        @currentIDKhachHang = IDKhachHang,
        @TrangThaiDon = TrangThaiDon,
        @TongTienCu = TongTien
    FROM NS_DonDatHang
    WHERE ID = @IDDonHang;

    IF @SoDonHang IS NULL
    BEGIN
        THROW 50001, N'Không tìm thấy đơn hàng gốc.', 1;
    END

    -- Fallback to current values if inputs are null
    SET @IDKhachHang = ISNULL(@IDKhachHang, @currentIDKhachHang);
    SET @IDNhanVien = ISNULL(@IDNhanVien, @currentIDNhanVien);
    SET @NgayTaoDon = ISNULL(@NgayTaoDon, @currentNgayTaoDon);
    SET @ThoiHanGiaoHang = ISNULL(@ThoiHanGiaoHang, @currentThoiHanGiaoHang);

    -- 2. Parse chi tiết mới từ JSON
    DECLARE @ChiTietMoi TABLE (
        IDSanPham INT,
        SoLuong DECIMAL(18,2),
        DonGia DECIMAL(18,2),
        ThueGTGT DECIMAL(18,2),
        ThanhTien DECIMAL(18,2),
        ThanhTienThue DECIMAL(18,2),
        ThanhTienSauThue DECIMAL(18,2),
        IsHangKhuyenMai BIT,
        GhiChu NVARCHAR(500)
    );

    INSERT INTO @ChiTietMoi (IDSanPham, SoLuong, DonGia, ThueGTGT, ThanhTien, ThanhTienThue, ThanhTienSauThue, IsHangKhuyenMai, GhiChu)
    SELECT 
        ISNULL(IDSanPham, 0),
        ROUND(CASE WHEN SoLuong >= 0 THEN SoLuong ELSE 1 END, 2),
        ROUND(CASE WHEN DonGia >= 0 THEN DonGia ELSE 0 END, 2),
        ROUND(CASE WHEN ThueGTGT >= 0 THEN ThueGTGT ELSE 0 END, 2),
        ROUND(CASE WHEN SoLuong >= 0 THEN SoLuong ELSE 1 END * CASE WHEN DonGia >= 0 THEN DonGia ELSE 0 END, 0) AS ThanhTien,
        ROUND(ROUND(CASE WHEN SoLuong >= 0 THEN SoLuong ELSE 1 END * CASE WHEN DonGia >= 0 THEN DonGia ELSE 0 END, 0) * CASE WHEN ThueGTGT >= 0 THEN ThueGTGT ELSE 0 END / 100, 0) AS ThanhTienThue,
        ROUND(CASE WHEN SoLuong >= 0 THEN SoLuong ELSE 1 END * CASE WHEN DonGia >= 0 THEN DonGia ELSE 0 END, 0) + 
        ROUND(ROUND(CASE WHEN SoLuong >= 0 THEN SoLuong ELSE 1 END * CASE WHEN DonGia >= 0 THEN DonGia ELSE 0 END, 0) * CASE WHEN ThueGTGT >= 0 THEN ThueGTGT ELSE 0 END / 100, 0) AS ThanhTienSauThue,
        ISNULL(IsHangKhuyenMai, 0),
        GhiChu
    FROM OPENJSON(@ChiTietsJson)
    WITH (
        IDSanPham INT '$.idSanPham',
        SoLuong DECIMAL(18,2) '$.soLuong',
        DonGia DECIMAL(18,2) '$.donGia',
        ThueGTGT DECIMAL(18,2) '$.thueGTGT',
        IsHangKhuyenMai BIT '$.isHangKhuyenMai',
        GhiChu NVARCHAR(500) '$.ghiChu'
    );

    -- Tính toán tổng tiền mới
    DECLARE @newThanhTienHang DECIMAL(18,2);
    DECLARE @newThanhTienThue DECIMAL(18,2);
    DECLARE @newTongTien DECIMAL(18,2);

    SELECT 
        @newThanhTienHang = SUM(ThanhTien),
        @newThanhTienThue = SUM(ThanhTienThue)
    FROM @ChiTietMoi;

    SET @newThanhTienHang = ISNULL(@newThanhTienHang, 0);
    SET @newThanhTienThue = ISNULL(@newThanhTienThue, 0);
    SET @newTongTien = @newThanhTienHang + @newThanhTienThue - @PhiBocXep;

    -- 3. Sinh số điều chỉnh
    DECLARE @adjCount INT;
    SELECT @adjCount = COUNT(1) FROM DON_DieuChinhDonHang WHERE IDDonHang = @IDDonHang;
    DECLARE @soDieuChinh NVARCHAR(50) = N'DC-' + @SoDonHang + N'-' + RIGHT('00' + CAST(@adjCount + 1 AS NVARCHAR(10)), 2);

    BEGIN TRANSACTION;
    BEGIN TRY
        -- 4. Lưu header Điều chỉnh
        DECLARE @idDieuChinh INT;
        INSERT INTO DON_DieuChinhDonHang 
            (IDDonHang, SoDieuChinh, NgayDieuChinh, LyDoDieuChinh, TongTienCu, TongTienMoi, NguoiTao, NgayTao, TrangThaiDon)
        VALUES 
            (@IDDonHang, @soDieuChinh, GETDATE(), @LyDoDieuChinh, @TongTienCu, @newTongTien, @NguoiTao, GETDATE(), @TrangThaiDon);
        SET @idDieuChinh = SCOPE_IDENTITY();

        -- 5. Lấy tập hợp tất cả các ID sản phẩm tham gia (trước và sau)
        DECLARE @allSpIds TABLE (IDSanPham INT PRIMARY KEY);
        INSERT INTO @allSpIds (IDSanPham)
        SELECT DISTINCT IDSanPham FROM NS_DonDatHangChiTiet WHERE IDDonDatHang = @IDDonHang AND IDSanPham IS NOT NULL
        UNION
        SELECT DISTINCT IDSanPham FROM @ChiTietMoi WHERE IDSanPham IS NOT NULL;

        -- Kiểm tra xem đơn hàng đã xuất kho chưa
        DECLARE @isDaXuatKho BIT = 0;
        IF EXISTS (SELECT 1 FROM KHO_PhieuXuat WHERE IDDonDatHang = @IDDonHang AND TrangThai = 2 AND IsDeleted = 0)
        BEGIN
            SET @isDaXuatKho = 1;
        END

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
            DECLARE @itemGhiChu NVARCHAR(500) = N'';

            -- Lấy thông tin cũ
            SELECT 
                @slCu = SoLuong,
                @dgCu = DonGia,
                @ttCu = CASE WHEN ThanhTienSauThue <> 0 THEN ThanhTienSauThue ELSE (ThanhTien + ISNULL(ThanhTienThue, 0)) END
            FROM NS_DonDatHangChiTiet
            WHERE IDDonDatHang = @IDDonHang AND IDSanPham = @spId;

            -- Lấy thông tin mới
            SELECT 
                @slMoi = SoLuong,
                @dgMoi = DonGia,
                @ttMoi = ThanhTienSauThue,
                @itemGhiChu = GhiChu
            FROM @ChiTietMoi
            WHERE IDSanPham = @spId;

            -- Chỉ ghi nhận dòng có thay đổi
            IF ISNULL(@slCu, 0) <> ISNULL(@slMoi, 0) OR ISNULL(@dgCu, 0) <> ISNULL(@dgMoi, 0) OR ISNULL(@ttCu, 0) <> ISNULL(@ttMoi, 0)
            BEGIN
                INSERT INTO DON_DieuChinhDonHang_ChiTiet
                    (IDDieuChinh, IDSanPham, SoLuongCu, SoLuongMoi, DonGiaCu, DonGiaMoi, ThanhTienCu, ThanhTienMoi, GhiChu)
                VALUES
                    (@idDieuChinh, @spId, @slCu, @slMoi, @dgCu, @dgMoi, @ttCu, @ttMoi, @itemGhiChu);

                -- Xử lý chênh lệch tồn kho nếu đã xuất kho
                IF @isDaXuatKho = 1 AND @IDKho IS NOT NULL AND @IDKho > 0
                BEGIN
                    DECLARE @qCu DECIMAL(18,2) = ISNULL(@slCu, 0);
                    DECLARE @qMoi DECIMAL(18,2) = ISNULL(@slMoi, 0);
                    DECLARE @delta DECIMAL(18,2) = @qMoi - @qCu;

                    IF @delta > 0
                    BEGIN
                        -- Xuất thêm (LoaiChungTu = 2)
                        DECLARE @dgMoiOrCu DECIMAL(18,2) = COALESCE(@dgMoi, @dgCu, 0);
                        INSERT INTO KHO_GiaoDichKho 
                            (NgayChungTu, SoChungTu, LoaiChungTu, IDChiTietKho, IDKho, IDSanPham, SoLuongNhap, SoLuongXuat, DonGia, ThanhTien, DienGiai, NgayTao, NguoiTao) 
                        VALUES 
                            (CAST(GETDATE() AS DATE), @soDieuChinh, 2, 0, @IDKho, @spId, 0, @delta, @dgMoiOrCu, @dgMoiOrCu * @delta, N'Xuất điều chỉnh tăng bán hàng theo phiếu ' + @soDieuChinh, GETDATE(), @NguoiTao);
                    END
                    ELSE IF @delta < 0
                    BEGIN
                        -- Nhập lại (LoaiChungTu = 1)
                        DECLARE @actualDelta DECIMAL(18,2) = ABS(@delta);
                        DECLARE @dgCuOrMoi DECIMAL(18,2) = COALESCE(@dgCu, @dgMoi, 0);
                        INSERT INTO KHO_GiaoDichKho 
                            (NgayChungTu, SoChungTu, LoaiChungTu, IDChiTietKho, IDKho, IDSanPham, SoLuongNhap, SoLuongXuat, DonGia, ThanhTien, DienGiai, NgayTao, NguoiTao) 
                        VALUES 
                            (CAST(GETDATE() AS DATE), @soDieuChinh, 1, 0, @IDKho, @spId, @actualDelta, 0, @dgCuOrMoi, @dgCuOrMoi * @actualDelta, N'Nhập điều chỉnh giảm bán hàng theo phiếu ' + @soDieuChinh, GETDATE(), @NguoiTao);
                    END
                END
            END

            FETCH NEXT FROM db_cursor INTO @spId;
        END

        CLOSE db_cursor;
        DEALLOCATE db_cursor;

        -- 6. Cập nhật bảng gốc NS_DonDatHang & NS_DonDatHangChiTiet
        UPDATE NS_DonDatHang SET
            TongTien = @newTongTien,
            PhiBocXep = @PhiBocXep,
            ThanhTienHang = @newThanhTienHang,
            ThanhTienThue = @newThanhTienThue,
            NgayCapNhat = GETDATE(),
            NguoiCapNhat = @NguoiTao,
            IDKhachHang = @IDKhachHang,
            IDNhanVien = @IDNhanVien,
            NgayTaoDon = @NgayTaoDon,
            ThoiHanGiaoHang = @ThoiHanGiaoHang
        WHERE ID = @IDDonHang;

        DELETE FROM NS_DonDatHangChiTiet WHERE IDDonDatHang = @IDDonHang;

        INSERT INTO NS_DonDatHangChiTiet
            (IDDonDatHang, IDSanPham, SoLuong, DonGia, ThanhTien, ThanhTienSauThue, ThanhTienThue,
             ThueGTGT, IsHangKhuyenMai, GhiChu,
             NgayTaoDon, SoDonHang, IDNhanVien, ThoiHanGiaoHang, TrangThaiDon, NgayTao, NguoiTao)
        SELECT 
            @IDDonHang, IDSanPham, SoLuong, DonGia, ThanhTien, ThanhTienSauThue, ThanhTienThue,
            ThueGTGT, IsHangKhuyenMai, GhiChu,
            @NgayTaoDon, @SoDonHang, @IDNhanVien, @ThoiHanGiaoHang, @TrangThaiDon, GETDATE(), @NguoiTao
        FROM @ChiTietMoi;

        -- 7. Cập nhật bảng liên đới BAN_ChungTuBanHang & BAN_ChungTuBanHang_ChiTiet (nếu có)
        DECLARE @invoiceId INT;
        DECLARE @currentDaThanhToan DECIMAL(18,2);
        
        SELECT @invoiceId = ID, @currentDaThanhToan = DaThanhToan
        FROM BAN_ChungTuBanHang
        WHERE IDDonDatHang = @IDDonHang AND IsDeleted = 0;

        IF @invoiceId IS NOT NULL
        BEGIN
            DECLARE @newConLai DECIMAL(18,2) = @newTongTien - @currentDaThanhToan;
            
            UPDATE BAN_ChungTuBanHang SET
                IDKho = @IDKho,
                IDKhachHang = @IDKhachHang,
                NgayChungTu = ISNULL(@NgayGiaoHang, NgayChungTu),
                TongTienHang = @newThanhTienHang,
                TongTienThue = @newThanhTienThue,
                PhiBocXep = @PhiBocXep,
                TongCong = @newTongTien,
                ConLai = @newConLai,
                NgayCapNhat = GETDATE(),
                NguoiCapNhat = @NguoiTao
            WHERE ID = @invoiceId;

            DELETE FROM BAN_ChungTuBanHang_ChiTiet WHERE IDChungTuBanHang = @invoiceId;

            INSERT INTO BAN_ChungTuBanHang_ChiTiet
                (IDChungTuBanHang, IDSanPham, STT, SoLuong, DonGia, ThanhTien, ThueGTGT, TienThue, TongSauThue, GhiChu)
            SELECT
                @invoiceId, IDSanPham, ROW_NUMBER() OVER(ORDER BY IDSanPham), SoLuong, DonGia, ThanhTien, ThueGTGT, ThanhTienThue, ThanhTienSauThue, GhiChu
            FROM @ChiTietMoi;

            -- Cập nhật ngày chứng từ trong KT_NhatKyChung nếu có
            IF @NgayGiaoHang IS NOT NULL
            BEGIN
                UPDATE KT_NhatKyChung
                SET NgayChungTu = CAST(@NgayGiaoHang AS DATE)
                WHERE LoaiChungTu = 'BAN' AND IDChungTu = @invoiceId;
            END
        END

        -- 8. Cập nhật bảng KHO_PhieuXuat & KHO_PhieuXuat_ChiTiet (nếu có)
        DECLARE @shipId INT;
        SELECT @shipId = ID FROM KHO_PhieuXuat WHERE IDDonDatHang = @IDDonHang AND IsDeleted = 0;

        IF @shipId IS NOT NULL
        BEGIN
            UPDATE KHO_PhieuXuat SET
                IDKho = @IDKho,
                NgayXuat = ISNULL(@NgayGiaoHang, NgayXuat),
                TongTienHang = @newThanhTienHang,
                TongTienThue = @newThanhTienThue,
                TongCong = @newTongTien,
                NgayCapNhat = GETDATE(),
                NguoiCapNhat = @NguoiTao
            WHERE ID = @shipId;

            DELETE FROM KHO_PhieuXuat_ChiTiet WHERE IDPhieuXuat = @shipId;

            INSERT INTO KHO_PhieuXuat_ChiTiet
                (IDPhieuXuat, IDSanPham, STT, SoLuong, DonGia, ThanhTien, ThueGTGT, TienThue, TongSauThue)
            SELECT
                @shipId, IDSanPham, ROW_NUMBER() OVER(ORDER BY IDSanPham), SoLuong, DonGia, ThanhTien, ThueGTGT, ThanhTienThue, ThanhTienSauThue
            FROM @ChiTietMoi;

            -- Cập nhật IDKho và NgayChungTu của các giao dịch kho cũ
            UPDATE KHO_GiaoDichKho
            SET IDKho = @IDKho,
                NgayChungTu = ISNULL(CAST(@NgayGiaoHang AS DATE), NgayChungTu)
            WHERE IDChiTietKho IN (
                SELECT pxct.ID
                FROM KHO_PhieuXuat_ChiTiet pxct
                WHERE pxct.IDPhieuXuat = @shipId
            ) AND LoaiChungTu = 2; -- Xuất kho
        END

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END;
";
                    conn.Execute(spSql);
                }
            }
            catch { }
        }

        public IEnumerable<DonDieuChinhListViewModel> GetPaged(
            int page, int pageSize,
            string tuNgay, string denNgay,
            int? idKhachHang, string soDonHang,
            bool chiDonDieuChinh,
            out int totalRecords)
        {
            using (var conn = _db.CreateConnection())
            {
                var p = new DynamicParameters();
                p.Add("@TuNgay", string.IsNullOrWhiteSpace(tuNgay) ? (DateTime?)null : DateTime.Parse(tuNgay));
                p.Add("@DenNgay", string.IsNullOrWhiteSpace(denNgay) ? (DateTime?)null : DateTime.Parse(denNgay).AddDays(1).AddSeconds(-1));
                p.Add("@IDKhachHang", idKhachHang);
                p.Add("@SoDonHang", string.IsNullOrWhiteSpace(soDonHang) ? null : soDonHang.Trim());
                p.Add("@ChiDonDieuChinh", chiDonDieuChinh ? 1 : 0);
                p.Add("@Offset", (page - 1) * pageSize);
                p.Add("@PageSize", pageSize);

                // Chỉ hiển thị các đơn: Đã lập chứng từ OR Đã xuất kho OR Đã phát sinh phiếu thu
                string filterSql = @"
                    d.TrangThaiDon != 4
                    AND (
                        EXISTS (SELECT 1 FROM BAN_ChungTuBanHang c WHERE c.IDDonDatHang = d.ID AND c.IsDeleted = 0)
                        OR EXISTS (SELECT 1 FROM KHO_PhieuXuat px WHERE px.IDDonDatHang = d.ID AND px.TrangThai = 2 AND px.IsDeleted = 0)
                        OR EXISTS (
                            SELECT 1 
                            FROM BAN_PhieuThuKhachHang pt 
                            INNER JOIN BAN_ChungTuBanHang c2 ON pt.IDChungTuBanHang = c2.ID 
                            WHERE c2.IDDonDatHang = d.ID AND pt.TrangThai = 2 AND pt.IsDeleted = 0 AND c2.IsDeleted = 0
                        )
                    )
                    AND (@TuNgay IS NULL OR d.NgayTaoDon >= @TuNgay)
                    AND (@DenNgay IS NULL OR d.NgayTaoDon <= @DenNgay)
                    AND (@IDKhachHang IS NULL OR d.IDKhachHang = @IDKhachHang)
                    AND (@SoDonHang IS NULL OR d.SoDonHang LIKE '%' + @SoDonHang + '%')
                    AND (@ChiDonDieuChinh = 0 OR EXISTS (SELECT 1 FROM DON_DieuChinhDonHang dc WHERE dc.IDDonHang = d.ID))";

                string countSql = "SELECT COUNT(1) FROM NS_DonDatHang d WHERE " + filterSql;
                totalRecords = conn.ExecuteScalar<int>(countSql, p);

                string sql = $@"
                    SELECT
                        d.ID, d.SoDonHang, d.NgayTaoDon, d.TrangThaiDon,
                        k.TenKhachHang,
                        d.TongTien,
                        ISNULL((
                            SELECT SUM(pt.SoTienThu) 
                            FROM BAN_PhieuThuKhachHang pt 
                            INNER JOIN BAN_ChungTuBanHang c ON pt.IDChungTuBanHang = c.ID 
                            WHERE c.IDDonDatHang = d.ID AND pt.TrangThai = 2 AND pt.IsDeleted = 0 AND c.IsDeleted = 0
                        ), 0) AS DaThanhToan,
                        ISNULL(tt.TenTrangThai, N'Không xác định') AS TenTrangThai,
                        CAST(CASE WHEN EXISTS (SELECT 1 FROM DON_DieuChinhDonHang dc WHERE dc.IDDonHang = d.ID) THEN 1 ELSE 0 END AS BIT) AS DaDieuChinh,
                        ISNULL((SELECT COUNT(1) FROM DON_DieuChinhDonHang dc WHERE dc.IDDonHang = d.ID), 0) AS SoLanDieuChinh,
                        (SELECT MAX(dc.NgayDieuChinh) FROM DON_DieuChinhDonHang dc WHERE dc.IDDonHang = d.ID) AS NgayDieuChinh,
                        (
                            SELECT TOP 1 ns.HoDem + ' ' + ns.Ten 
                            FROM DON_DieuChinhDonHang dc 
                            LEFT JOIN NS_NhanSu ns ON dc.NguoiTao = ns.ID 
                            WHERE dc.IDDonHang = d.ID 
                            ORDER BY dc.NgayDieuChinh DESC, dc.ID DESC
                        ) AS NguoiDieuChinh
                    FROM NS_DonDatHang d
                    LEFT JOIN NS_KhachHang k ON d.IDKhachHang = k.ID
                    LEFT JOIN DM_TrangThaiDonHang tt ON d.TrangThaiDon = tt.ID
                    WHERE {filterSql}
                    ORDER BY d.ID DESC
                    OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

                return conn.Query<DonDieuChinhListViewModel>(sql, p).ToList();
            }
        }

        public IEnumerable<DonDieuChinhHistoryViewModel> GetAdjustHistory(int idDonHang)
        {
            using (var conn = _db.CreateConnection())
            {
                string sql = @"
                    SELECT 
                        dc.ID,
                        dc.SoDieuChinh,
                        dc.NgayDieuChinh,
                        dc.LyDoDieuChinh,
                        dc.TongTienCu,
                        dc.TongTienMoi,
                        dc.TrangThaiDon,
                        tt.TenTrangThai,
                        ns.HoDem + ' ' + ns.Ten AS TenNguoiTao
                    FROM DON_DieuChinhDonHang dc
                    LEFT JOIN NS_NhanSu ns ON dc.NguoiTao = ns.ID
                    LEFT JOIN DM_TrangThaiDonHang tt ON dc.TrangThaiDon = tt.ID
                    WHERE dc.IDDonHang = @IDDonHang
                    ORDER BY dc.NgayDieuChinh DESC, dc.ID DESC";

                var histories = conn.Query<DonDieuChinhHistoryViewModel>(sql, new { IDDonHang = idDonHang }).ToList();

                string detailSql = @"
                    SELECT 
                        sp.TenSanPham,
                        sp.MaSanPham,
                        sp.DVT,
                        ct.SoLuongCu,
                        ct.SoLuongMoi,
                        ct.DonGiaCu,
                        ct.DonGiaMoi,
                        ct.ThanhTienCu,
                        ct.ThanhTienMoi,
                        ct.GhiChu
                    FROM DON_DieuChinhDonHang_ChiTiet ct
                    LEFT JOIN DM_SanPham sp ON ct.IDSanPham = sp.ID
                    WHERE ct.IDDieuChinh = @IDDieuChinh
                    ORDER BY ct.ID";

                foreach (var h in histories)
                {
                    h.ChiTiets = conn.Query<DonDieuChinhHistoryDetailViewModel>(detailSql, new { IDDieuChinh = h.ID }).ToList();
                }

                return histories;
            }
        }

        public void SaveAdjustment(DonDieuChinhPostModel model, int userId)
        {
            using (var conn = _db.CreateConnection())
            {
                var p = new DynamicParameters();
                p.Add("@IDDonHang", model.IDDonHang);
                p.Add("@LyDoDieuChinh", model.LyDoDieuChinh);
                p.Add("@ChiTietsJson", model.ChiTietsJson);
                p.Add("@PhiBocXep", model.PhiBocXep);
                p.Add("@IDKho", model.IDKho);
                p.Add("@NguoiTao", userId);
                p.Add("@IDKhachHang", model.IDKhachHang);
                p.Add("@IDNhanVien", model.IDNhanVien);
                p.Add("@NgayTaoDon", model.NgayTaoDon);
                p.Add("@NgayGiaoHang", model.NgayGiaoHang);
                p.Add("@ThoiHanGiaoHang", model.ThoiHanGiaoHang);

                conn.Execute("sp_DON_DieuChinhDonHang_Save", p, commandType: CommandType.StoredProcedure);
            }
        }
    }
}
