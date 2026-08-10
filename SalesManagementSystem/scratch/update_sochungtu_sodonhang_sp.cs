using System;
using System.Configuration;
using System.Data;
using Dapper;

class Program
{
    static void Main()
    {
        ConfigurationManager.AppSettings["ConfigFile"] = @"c:\Users\duoc0\OneDrive\Desktop\WEB_QLBH\QuanLyBanHang\SalesManagementSystem\SalesManagementSystem\App_Config\systemPublic.dat";
        ConfigurationManager.AppSettings["KeyPart1"] = "VanDuoc@123123!";

        var factory = new SalesManagementSystem.Data.DbConnectionFactory();
        using (var conn = factory.CreateConnection())
        {
            conn.Open();

            Console.WriteLine("=== UPDATING SP_KT_PHIEUTHU_GETLIST ===");
            string sql1 = @"
ALTER PROCEDURE sp_KT_PhieuThu_GetList
    @TuNgay         DATETIME        = NULL,
    @DenNgay        DATETIME        = NULL,
    @SoPhieuThu     NVARCHAR(50)    = NULL,
    @IDKhachHang    INT             = NULL,
    @TrangThai      INT             = NULL,
    @NguoiNopTien   NVARCHAR(250)   = NULL,
    @IDTaiKhoanThanhToan INT        = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT
        pt.ID,
        pt.SoPhieuThu,
        pt.NgayThu,
        pt.IDTaiKhoanThanhToan,
        tk.TenTaiKhoan AS TenTaiKhoanThanhToan,
        pt.NguoiNopTien,
        pt.SoDienThoaiNguoiNop,
        pt.IDKhachHang,
        kh.TenKhachHang,
        
        STUFF((
            SELECT ', ' + ct2.SoChungTu
            FROM KT_PhieuThuChiTiet ct
            INNER JOIN BAN_ChungTuBanHang ct2 ON ct.IDChungTuBanHang = ct2.ID
            WHERE ct.IDPhieuThu = pt.ID
            FOR XML PATH('')
        ), 1, 2, '') AS SoChungTuBanHang,

        pt.SoTienThu,
        pt.DienGiai,
        pt.TrangThai,
        pt.NgayTao,
        pt.NguoiTao,
        LTRIM(RTRIM(ISNULL(ns.HoDem, '') + ' ' + ISNULL(ns.Ten, ''))) AS TenNguoiTao,
        pt.NgayCapNhat,
        
        (SELECT ISNULL(SUM(SoTienPhanBo), 0) FROM KT_PhieuThuChiTiet WHERE IDPhieuThu = pt.ID AND LoaiThu = 1) AS SoTienPhanBo
        
    FROM KT_PhieuThu pt
    LEFT JOIN DM_TaiKhoanThanhToan  tk  ON pt.IDTaiKhoanThanhToan = tk.ID
    LEFT JOIN NS_KhachHang          kh  ON pt.IDKhachHang         = kh.ID
    LEFT JOIN NS_NhanSu             ns  ON pt.NguoiTao            = ns.ID
    WHERE 1=1
      AND (@TuNgay IS NULL OR CAST(pt.NgayThu AS DATE) >= CAST(@TuNgay AS DATE))
      AND (@DenNgay IS NULL OR CAST(pt.NgayThu AS DATE) <= CAST(@DenNgay AS DATE))
      AND (@SoPhieuThu IS NULL OR @SoPhieuThu = '' OR pt.SoPhieuThu LIKE '%' + @SoPhieuThu + '%' OR EXISTS (
          SELECT 1 
          FROM KT_PhieuThuChiTiet ptct 
          INNER JOIN BAN_ChungTuBanHang ctbh ON ptct.IDChungTuBanHang = ctbh.ID
          LEFT JOIN NS_DonDatHang ddh ON ctbh.IDDonDatHang = ddh.ID
          WHERE ptct.IDPhieuThu = pt.ID 
            AND (ctbh.SoChungTu LIKE '%' + @SoPhieuThu + '%' OR ddh.SoDonHang LIKE '%' + @SoPhieuThu + '%')
      ))
      AND (@IDKhachHang IS NULL OR pt.IDKhachHang = @IDKhachHang)
      AND (@TrangThai IS NULL OR pt.TrangThai = @TrangThai)
      AND (@NguoiNopTien IS NULL OR pt.NguoiNopTien LIKE '%' + @NguoiNopTien + '%')
      AND (@IDTaiKhoanThanhToan IS NULL OR pt.IDTaiKhoanThanhToan = @IDTaiKhoanThanhToan)
    ORDER BY pt.NgayThu DESC, pt.ID DESC
END
";
            conn.Execute(sql1);
            Console.WriteLine("sp_KT_PhieuThu_GetList updated!");

            Console.WriteLine("\n=== UPDATING SP_KHO_PHIEUXUAT_GETLIST ===");
            string sql2 = @"
ALTER PROCEDURE sp_KHO_PhieuXuat_GetList
    @Page INT = 1,
    @PageSize INT = 20,
    @TuNgay NVARCHAR(10) = NULL,
    @DenNgay NVARCHAR(10) = NULL,
    @SoChungTu NVARCHAR(50) = NULL,
    @IDKho INT = NULL,
    @TrangThai INT = NULL,
    @IDNhanSuNhan INT = NULL,
    @TotalRecords INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @Offset INT = (@Page - 1) * @PageSize;

    SELECT 
        px.ID,
        px.SoChungTu,
        px.NgayXuat,
        px.IDDonDatHang,
        px.IDChungTuBanHang,
        px.IDKho,
        k.TenKhoHang AS TenKhoHang,
        px.IDNhanSuNhan,
        px.TenNguoiNhan,
        px.GhiChu,
        px.TongTienHang,
        px.TongTienThue,
        px.TongCong,
        px.TrangThai,
        dh.SoDonHang,
        dh.NgayTaoDon AS NgayDonHang,
        dh.TrangThaiDon AS TrangThaiDonHang,
        kh.TenKhachHang
    INTO #TempList
    FROM KHO_PhieuXuat px
    INNER JOIN BAN_ChungTuBanHang ctbh ON px.IDChungTuBanHang = ctbh.ID
    LEFT JOIN DM_KhoHang k ON px.IDKho = k.ID
    LEFT JOIN NS_DonDatHang dh ON px.IDDonDatHang = dh.ID
    LEFT JOIN NS_KhachHang kh ON dh.IDKhachHang = kh.ID
    WHERE px.IsDeleted = 0
      AND ctbh.IsDeleted = 0
      AND ctbh.TrangThai IN (1, 2)
      AND (@TuNgay IS NULL OR px.NgayXuat >= @TuNgay)
      AND (@DenNgay IS NULL OR px.NgayXuat <= @DenNgay)
      AND (@SoChungTu IS NULL OR @SoChungTu = '' OR px.SoChungTu LIKE '%' + @SoChungTu + '%' OR dh.SoDonHang LIKE '%' + @SoChungTu + '%' OR ctbh.SoChungTu LIKE '%' + @SoChungTu + '%')
      AND (@IDKho IS NULL OR px.IDKho = @IDKho)
      AND (@TrangThai IS NULL OR px.TrangThai = @TrangThai)
      AND (@IDNhanSuNhan IS NULL OR px.IDNhanSuNhan = @IDNhanSuNhan);

    SELECT @TotalRecords = COUNT(*) FROM #TempList;

    SELECT * 
    FROM #TempList
    ORDER BY NgayXuat DESC, ID DESC
    OFFSET @Offset ROWS
    FETCH NEXT @PageSize ROWS ONLY;

    DROP TABLE #TempList;
END
";
            conn.Execute(sql2);
            Console.WriteLine("sp_KHO_PhieuXuat_GetList updated!");

            Console.WriteLine("\n=== UPDATING SP_BAOCAO_DOICHIEUCONGNOKHACHHANG ===");
            string sql3 = @"
ALTER PROCEDURE sp_BaoCao_DoiChieuCongNoKhachHang
    @IDKhachHang INT = NULL,
    @TuNgay DATETIME,
    @DenNgay DATETIME,
    @SoChungTu NVARCHAR(50) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @NoDauKy DECIMAL(18,2) = 0;
    
    DECLARE @TongBanDauKy DECIMAL(18,2) = 0;
    SELECT @TongBanDauKy = ISNULL(SUM(ct.TongSauThue), 0)
    FROM BAN_ChungTuBanHang bh
    INNER JOIN BAN_ChungTuBanHang_ChiTiet ct ON bh.ID = ct.IDChungTuBanHang
    WHERE bh.IsDeleted = 0 
      AND bh.TrangThai IN (1, 2)
      AND (@IDKhachHang IS NULL OR bh.IDKhachHang = @IDKhachHang)
      AND CAST(bh.NgayChungTu AS DATE) < CAST(@TuNgay AS DATE);

    DECLARE @TongTraDauKy DECIMAL(18,2) = 0;
    SELECT @TongTraDauKy = ISNULL(SUM(ct.ThanhTien), 0)
    FROM BAN_TraHangBan th
    INNER JOIN BAN_TraHangBanChiTiet ct ON th.ID = ct.IDTraHang
    WHERE th.TrangThai = 2
      AND (@IDKhachHang IS NULL OR th.IDKhachHang = @IDKhachHang)
      AND CAST(th.NgayChungTu AS DATE) < CAST(@TuNgay AS DATE);

    DECLARE @TongThuDauKy DECIMAL(18,2) = 0;
    SELECT @TongThuDauKy = ISNULL(SUM(SoTienThu), 0)
    FROM KT_PhieuThu
    WHERE TrangThai = 2
      AND (@IDKhachHang IS NULL OR IDKhachHang = @IDKhachHang)
      AND CAST(NgayThu AS DATE) < CAST(@TuNgay AS DATE);
      
    SET @NoDauKy = @TongBanDauKy - @TongTraDauKy - @TongThuDauKy;

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
        LoaiDong INT,
        ThuTuSapXep INT,
        IDPhatSinh INT
    );

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
      AND (@SoChungTu IS NULL OR @SoChungTu = '' OR bh.SoChungTu LIKE '%' + @SoChungTu + '%' OR EXISTS (SELECT 1 FROM NS_DonDatHang ddh WHERE ddh.ID = bh.IDDonDatHang AND ddh.SoDonHang LIKE '%' + @SoChungTu + '%'))
      AND CAST(bh.NgayChungTu AS DATE) >= CAST(@TuNgay AS DATE)
      AND CAST(bh.NgayChungTu AS DATE) <= CAST(@DenNgay AS DATE);

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
      AND (@SoChungTu IS NULL OR @SoChungTu = '' OR pt.SoPhieuThu LIKE '%' + @SoChungTu + '%' OR EXISTS (
          SELECT 1 
          FROM KT_PhieuThuChiTiet ptct 
          INNER JOIN BAN_ChungTuBanHang ctbh ON ptct.IDChungTuBanHang = ctbh.ID
          LEFT JOIN NS_DonDatHang ddh ON ctbh.IDDonDatHang = ddh.ID
          WHERE ptct.IDPhieuThu = pt.ID 
            AND (ctbh.SoChungTu LIKE '%' + @SoChungTu + '%' OR ddh.SoDonHang LIKE '%' + @SoChungTu + '%')
      ))
      AND CAST(pt.NgayThu AS DATE) >= CAST(@TuNgay AS DATE)
      AND CAST(pt.NgayThu AS DATE) <= CAST(@DenNgay AS DATE);

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
            conn.Execute(sql3);
            Console.WriteLine("sp_BaoCao_DoiChieuCongNoKhachHang updated!");
        }
    }
}
