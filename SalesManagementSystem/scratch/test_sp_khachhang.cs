using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using SalesManagementSystem.Data;
using Dapper;

class Program
{
    static void Main()
    {
        try
        {
            ConfigurationManager.AppSettings["ConfigFile"] = @"c:\Users\duoc0\OneDrive\Desktop\WEB_QLBH\QuanLyBanHang\SalesManagementSystem\SalesManagementSystem\App_Config\systemPublic.dat";
            ConfigurationManager.AppSettings["KeyPart1"] = "VanDuoc@123123!";
            AppDomain.CurrentDomain.SetData("DataDirectory", @"c:\Users\duoc0\OneDrive\Desktop\WEB_QLBH\QuanLyBanHang\SalesManagementSystem\SalesManagementSystem\App_Data");

            var db = new DbConnectionFactory();
            using (var conn = db.CreateConnection())
            {
                conn.Open();

                string sqlSp = @"
IF OBJECT_ID('sp_BaoCao_DoiChieuCongNoKhachHang', 'P') IS NOT NULL
    DROP PROCEDURE sp_BaoCao_DoiChieuCongNoKhachHang;
";
                conn.Execute(sqlSp);

                string createSp = @"
CREATE PROCEDURE sp_BaoCao_DoiChieuCongNoKhachHang
    @IDKhachHang INT = NULL,
    @TuNgay DATETIME,
    @DenNgay DATETIME,
    @SoChungTu NVARCHAR(50) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    -- 1. Tính nợ đầu kỳ
    DECLARE @NoDauKy DECIMAL(18,2) = 0;
    
    -- Tổng bán trước TuNgay
    DECLARE @TongBanDauKy DECIMAL(18,2) = 0;
    SELECT @TongBanDauKy = ISNULL(SUM(ct.TongSauThue), 0)
    FROM BAN_ChungTuBanHang bh
    INNER JOIN BAN_ChungTuBanHang_ChiTiet ct ON bh.ID = ct.IDChungTuBanHang
    WHERE bh.IsDeleted = 0 
      AND bh.TrangThai IN (1, 2)
      AND (@IDKhachHang IS NULL OR bh.IDKhachHang = @IDKhachHang)
      AND CAST(bh.NgayChungTu AS DATE) < CAST(@TuNgay AS DATE);

    -- Tổng trả hàng trước TuNgay
    DECLARE @TongTraDauKy DECIMAL(18,2) = 0;
    SELECT @TongTraDauKy = ISNULL(SUM(ct.ThanhTien), 0)
    FROM BAN_TraHangBan th
    INNER JOIN BAN_TraHangBanChiTiet ct ON th.ID = ct.IDTraHang
    WHERE th.TrangThai = 2
      AND (@IDKhachHang IS NULL OR th.IDKhachHang = @IDKhachHang)
      AND CAST(th.NgayChungTu AS DATE) < CAST(@TuNgay AS DATE);

    -- Tổng thu tiền trước TuNgay (dùng KT_PhieuThu)
    DECLARE @TongThuDauKy DECIMAL(18,2) = 0;
    SELECT @TongThuDauKy = ISNULL(SUM(SoTienThu), 0)
    FROM KT_PhieuThu
    WHERE TrangThai = 2
      AND (@IDKhachHang IS NULL OR IDKhachHang = @IDKhachHang)
      AND CAST(NgayThu AS DATE) < CAST(@TuNgay AS DATE);
      
    SET @NoDauKy = @TongBanDauKy - @TongTraDauKy - @TongThuDauKy;

    -- 2. Gom dữ liệu phát sinh trong kỳ
    CREATE TABLE #PhatSinh (
        STT INT IDENTITY(1,1),
        NgayPhatSinh DATETIME,
        SoChungTu NVARCHAR(50),
        TenNhanVien NVARCHAR(MAX),
        TenKhuVuc NVARCHAR(MAX),
        TenTinh NVARCHAR(MAX),
        TenKhachHang NVARCHAR(MAX),
        LoaiPhatSinh NVARCHAR(100),
        MaSanPham NVARCHAR(50),
        TenSanPham NVARCHAR(MAX),
        DienGiai NVARCHAR(MAX),
        SoLuongBan DECIMAL(18,2),
        DonGiaBan DECIMAL(18,2),
        PhaiThu DECIMAL(18,2),
        DaThanhToan DECIMAL(18,2),
        ConNoLuyKe DECIMAL(18,2),
        GhiChu NVARCHAR(MAX),
        LoaiDong INT, -- 0: Nợ đầu kỳ, 1: Bán hàng, 2: Trả hàng, 3: Thu tiền
        ThuTuSapXep INT,
        IDPhatSinh INT
    );

    -- Insert Nợ đầu kỳ
    INSERT INTO #PhatSinh (
        NgayPhatSinh, SoChungTu, TenNhanVien, TenKhuVuc, TenTinh, TenKhachHang, LoaiPhatSinh, MaSanPham, TenSanPham, DienGiai, 
        SoLuongBan, DonGiaBan, PhaiThu, DaThanhToan, ConNoLuyKe, GhiChu, 
        LoaiDong, ThuTuSapXep, IDPhatSinh
    )
    VALUES (
        DATEADD(day, -1, @TuNgay), '', '', '', '', '', N'Nợ đầu kỳ', '', N'Nợ đầu kỳ', '', 
        0, 0, 0, 0, @NoDauKy, '', 
        0, 0, 0
    );

    -- Insert Phiếu Bán hàng
    INSERT INTO #PhatSinh (
        NgayPhatSinh, SoChungTu, TenNhanVien, TenKhuVuc, TenTinh, TenKhachHang, LoaiPhatSinh, MaSanPham, TenSanPham, DienGiai, 
        SoLuongBan, DonGiaBan, PhaiThu, DaThanhToan, ConNoLuyKe, GhiChu, 
        LoaiDong, ThuTuSapXep, IDPhatSinh
    )
    SELECT 
        bh.NgayChungTu,
        bh.SoChungTu,
        ISNULL(nv.HoTen, ISNULL(LTRIM(RTRIM(ISNULL(ns.HoDem, '') + ' ' + ISNULL(ns.Ten, ''))), '')),
        ISNULL(kh.TenKhuVuc, ''),
        ISNULL(tt.TenTinhThanh, kh.DiaChi),
        kh.TenKhachHang,
        CASE WHEN bh.TrangThai = 1 THEN N'Bán hàng (Đề nghị ghi)' ELSE N'Bán hàng' END,
        sp.MaSanPham,
        sp.TenSanPham,
        ct.GhiChu,
        ct.SoLuong,
        ct.DonGia,
        ct.TongSauThue,
        0,
        0,
        ct.GhiChu,
        1,
        1,
        bh.ID
    FROM BAN_ChungTuBanHang bh
    INNER JOIN BAN_ChungTuBanHang_ChiTiet ct ON bh.ID = ct.IDChungTuBanHang
    LEFT JOIN NS_KhachHang kh ON bh.IDKhachHang = kh.ID
    LEFT JOIN NS_NhanVien nv ON kh.IDNhanVien = nv.ID
    LEFT JOIN NS_NhanSu ns ON kh.IDNhanVien = ns.ID
    LEFT JOIN DM_TinhThanh tt ON kh.IDTinhThanh = tt.ID
    LEFT JOIN DM_SanPham sp ON ct.IDSanPham = sp.ID
    WHERE bh.IsDeleted = 0 
      AND bh.TrangThai IN (1, 2)
      AND (@IDKhachHang IS NULL OR bh.IDKhachHang = @IDKhachHang)
      AND (@SoChungTu IS NULL OR @SoChungTu = '' OR bh.SoChungTu LIKE '%' + @SoChungTu + '%')
      AND CAST(bh.NgayChungTu AS DATE) >= CAST(@TuNgay AS DATE)
      AND CAST(bh.NgayChungTu AS DATE) <= CAST(@DenNgay AS DATE);

    -- Insert Trả hàng bán
    INSERT INTO #PhatSinh (
        NgayPhatSinh, SoChungTu, TenNhanVien, TenKhuVuc, TenTinh, TenKhachHang, LoaiPhatSinh, MaSanPham, TenSanPham, DienGiai, 
        SoLuongBan, DonGiaBan, PhaiThu, DaThanhToan, ConNoLuyKe, GhiChu, 
        LoaiDong, ThuTuSapXep, IDPhatSinh
    )
    SELECT 
        th.NgayChungTu,
        th.SoChungTu,
        ISNULL(nv.HoTen, ISNULL(LTRIM(RTRIM(ISNULL(ns.HoDem, '') + ' ' + ISNULL(ns.Ten, ''))), '')),
        ISNULL(kh.TenKhuVuc, ''),
        ISNULL(tt.TenTinhThanh, kh.DiaChi),
        kh.TenKhachHang,
        N'Trả hàng bán',
        sp.MaSanPham,
        sp.TenSanPham,
        ct.GhiChu,
        -ct.SoLuongTra,
        ct.DonGia,
        -ct.ThanhTien,
        0,
        0,
        ct.GhiChu,
        2,
        1,
        th.ID
    FROM BAN_TraHangBan th
    INNER JOIN BAN_TraHangBanChiTiet ct ON th.ID = ct.IDTraHang
    LEFT JOIN NS_KhachHang kh ON th.IDKhachHang = kh.ID
    LEFT JOIN NS_NhanVien nv ON kh.IDNhanVien = nv.ID
    LEFT JOIN NS_NhanSu ns ON kh.IDNhanVien = ns.ID
    LEFT JOIN DM_TinhThanh tt ON kh.IDTinhThanh = tt.ID
    LEFT JOIN DM_SanPham sp ON ct.IDSanPham = sp.ID
    WHERE th.TrangThai = 2
      AND (@IDKhachHang IS NULL OR th.IDKhachHang = @IDKhachHang)
      AND (@SoChungTu IS NULL OR @SoChungTu = '' OR th.SoChungTu LIKE '%' + @SoChungTu + '%')
      AND CAST(th.NgayChungTu AS DATE) >= CAST(@TuNgay AS DATE)
      AND CAST(th.NgayChungTu AS DATE) <= CAST(@DenNgay AS DATE);

    -- Insert Phiếu Thu tiền khách hàng (KT_PhieuThu)
    INSERT INTO #PhatSinh (
        NgayPhatSinh, SoChungTu, TenNhanVien, TenKhuVuc, TenTinh, TenKhachHang, LoaiPhatSinh, MaSanPham, TenSanPham, DienGiai, 
        SoLuongBan, DonGiaBan, PhaiThu, DaThanhToan, ConNoLuyKe, GhiChu, 
        LoaiDong, ThuTuSapXep, IDPhatSinh
    )
    SELECT 
        pt.NgayThu,
        pt.SoPhieuThu,
        ISNULL(nv.HoTen, ISNULL(LTRIM(RTRIM(ISNULL(ns.HoDem, '') + ' ' + ISNULL(ns.Ten, ''))), '')),
        ISNULL(kh.TenKhuVuc, ''),
        ISNULL(tt.TenTinhThanh, kh.DiaChi),
        kh.TenKhachHang,
        N'Thu tiền khách hàng',
        '',
        ISNULL(pt.DienGiai, N'Thu tiền khách hàng'),
        pt.DienGiai,
        0,
        0,
        0,
        pt.SoTienThu,
        0,
        pt.DienGiai,
        3,
        1,
        pt.ID
    FROM KT_PhieuThu pt
    LEFT JOIN NS_KhachHang kh ON pt.IDKhachHang = kh.ID
    LEFT JOIN NS_NhanVien nv ON kh.IDNhanVien = nv.ID
    LEFT JOIN NS_NhanSu ns ON kh.IDNhanVien = ns.ID
    LEFT JOIN DM_TinhThanh tt ON kh.IDTinhThanh = tt.ID
    WHERE pt.TrangThai = 2
      AND (@IDKhachHang IS NULL OR pt.IDKhachHang = @IDKhachHang)
      AND (@SoChungTu IS NULL OR @SoChungTu = '' OR pt.SoPhieuThu LIKE '%' + @SoChungTu + '%')
      AND CAST(pt.NgayThu AS DATE) >= CAST(@TuNgay AS DATE)
      AND CAST(pt.NgayThu AS DATE) <= CAST(@DenNgay AS DATE);

    -- 3. Tính lũy kế và trả về
    SELECT 
        ROW_NUMBER() OVER(ORDER BY ThuTuSapXep ASC, CAST(NgayPhatSinh AS DATE) ASC, LoaiDong ASC, SoChungTu ASC, IDPhatSinh ASC, STT ASC) AS STT,
        NgayPhatSinh,
        SoChungTu,
        TenNhanVien,
        TenKhuVuc,
        TenTinh,
        TenKhachHang,
        LoaiPhatSinh,
        MaSanPham,
        TenSanPham,
        DienGiai,
        SoLuongBan,
        DonGiaBan,
        PhaiThu,
        DaThanhToan,
        @NoDauKy + SUM(PhaiThu - DaThanhToan) OVER(
            ORDER BY ThuTuSapXep ASC, CAST(NgayPhatSinh AS DATE) ASC, LoaiDong ASC, SoChungTu ASC, IDPhatSinh ASC, STT ASC
            ROWS UNBOUNDED PRECEDING
        ) AS ConNoLuyKe,
        GhiChu,
        LoaiDong,
        ThuTuSapXep,
        IDPhatSinh
    FROM #PhatSinh
    ORDER BY STT ASC;

    DROP TABLE #PhatSinh;
END
";
                conn.Execute(createSp);
                Console.WriteLine("UPDATED sp_BaoCao_DoiChieuCongNoKhachHang PROCEDURE SUCCESSFULLY.");

                // Test report execution for customer ID 8 (KHTongthauvu)
                var result = conn.Query("EXEC sp_BaoCao_DoiChieuCongNoKhachHang @IDKhachHang = 8, @TuNgay = '2026-01-01', @DenNgay = '2026-08-09'");
                Console.WriteLine("\n--- TEST REPORT RESULT FOR KHTongthauvu (ID=8) ---");
                foreach (var r in result)
                {
                    Console.WriteLine($"{r.STT,2} | {r.NgayPhatSinh:dd/MM/yyyy} | {r.SoChungTu,-10} | {r.LoaiPhatSinh,-22} | PhaiThu: {r.PhaiThu,13:#,##0} | DaThanhToan: {r.DaThanhToan,13:#,##0} | LuyKe: {r.ConNoLuyKe,13:#,##0}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error: " + ex.Message);
        }
    }
}
