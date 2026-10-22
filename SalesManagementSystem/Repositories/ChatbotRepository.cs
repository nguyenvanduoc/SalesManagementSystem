using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using SalesManagementSystem.Data;
using SalesManagementSystem.Models.ViewModels;
using SalesManagementSystem.Repositories.Interfaces;

namespace SalesManagementSystem.Repositories
{
    /// <summary>
    /// Truy vấn phục vụ Trợ lý ảo. Các số liệu công nợ KH / tồn kho / doanh thu
    /// dùng lại repository sẵn có; ở đây chỉ chứa các truy vấn chưa có nơi nào cung cấp.
    /// </summary>
    public class ChatbotRepository : IChatbotRepository
    {
        private readonly DbConnectionFactory _db;

        public ChatbotRepository(DbConnectionFactory db)
        {
            _db = db;
        }

        public IEnumerable<ChatbotDanhMucItem> GetDanhMucKhachHang()
        {
            const string sql = "SELECT ID, ISNULL(MaKhachHang, '') AS Ma, ISNULL(TenKhachHang, '') AS Ten FROM NS_KhachHang";
            using (var conn = _db.CreateConnection())
                return conn.Query<ChatbotDanhMucItem>(sql).ToList();
        }

        public IEnumerable<ChatbotDanhMucItem> GetDanhMucSanPham()
        {
            const string sql = "SELECT ID, ISNULL(MaSanPham, '') AS Ma, ISNULL(TenSanPham, '') AS Ten FROM DM_SanPham";
            using (var conn = _db.CreateConnection())
                return conn.Query<ChatbotDanhMucItem>(sql).ToList();
        }

        public IEnumerable<ChatbotDanhMucItem> GetDanhMucNhaCungCap()
        {
            const string sql = "SELECT ID, ISNULL(MaNhaCungCap, '') AS Ma, ISNULL(TenNhaCungCap, '') AS Ten FROM DM_NhaCungCap";
            using (var conn = _db.CreateConnection())
                return conn.Query<ChatbotDanhMucItem>(sql).ToList();
        }

        public IEnumerable<ChatbotDanhMucItem> GetDanhMucKho()
        {
            const string sql = "SELECT ID, ISNULL(MaKhoHang, '') AS Ma, ISNULL(TenKhoHang, '') AS Ten FROM DM_KhoHang";
            using (var conn = _db.CreateConnection())
                return conn.Query<ChatbotDanhMucItem>(sql).ToList();
        }

        public IEnumerable<ChatbotCongNoNccItem> GetCongNoNhaCungCap(int? idNhaCungCap)
        {
            // Cùng cách tính "Đã thanh toán" với sp_Dashboard_GetData (phân bổ phiếu chi + phiếu chi gắn trực tiếp),
            // chỉ tính phiếu nhập đã ghi sổ (TrangThai = 2). Quá hạn: sau 30 ngày kể từ ngày nhập.
            const string sql = @"
                ;WITH Paid AS (
                    SELECT
                        pn.IDNhaCungCap,
                        pn.NgayNhap,
                        ISNULL(pn.TongCong, 0) AS TongCong,
                        ISNULL((SELECT SUM(ct.SoTienPhanBo)
                                FROM KT_PhieuChiChiTiet ct
                                INNER JOIN KT_PhieuChi pc ON ct.IDPhieuChi = pc.ID
                                WHERE ct.IDPhieuNhap = pn.ID AND ct.LoaiChi = 1
                                  AND pc.TrangThai = 2 AND pc.IsDeleted = 0), 0)
                        + ISNULL((SELECT SUM(pc2.SoTienChi)
                                  FROM KT_PhieuChi pc2
                                  WHERE pc2.IDPhieuNhap = pn.ID
                                    AND pc2.TrangThai = 2 AND pc2.IsDeleted = 0
                                    AND NOT EXISTS (SELECT 1 FROM KT_PhieuChiChiTiet ct WHERE ct.IDPhieuChi = pc2.ID)), 0) AS DaThanhToan
                    FROM KHO_PhieuNhap pn
                    WHERE pn.IsDeleted = 0 AND pn.TrangThai = 2
                      AND (@IDNhaCungCap IS NULL OR pn.IDNhaCungCap = @IDNhaCungCap)
                )
                SELECT
                    p.IDNhaCungCap AS ID,
                    ISNULL(ncc.TenNhaCungCap, N'(Chưa xác định)') AS Ten,
                    SUM(p.TongCong) AS TongPhatSinh,
                    SUM(p.DaThanhToan) AS DaThanhToan,
                    SUM(p.TongCong - p.DaThanhToan) AS ConNo,
                    SUM(CASE WHEN p.TongCong > p.DaThanhToan AND DATEADD(day, 30, p.NgayNhap) < CAST(GETDATE() AS DATE)
                             THEN p.TongCong - p.DaThanhToan ELSE 0 END) AS QuaHan,
                    SUM(CASE WHEN p.TongCong - p.DaThanhToan > 1000 THEN 1 ELSE 0 END) AS SoPhieuConNo
                FROM Paid p
                LEFT JOIN DM_NhaCungCap ncc ON p.IDNhaCungCap = ncc.ID
                GROUP BY p.IDNhaCungCap, ncc.TenNhaCungCap
                ORDER BY ConNo DESC";

            using (var conn = _db.CreateConnection())
                return conn.Query<ChatbotCongNoNccItem>(sql, new { IDNhaCungCap = idNhaCungCap }).ToList();
        }

        public IEnumerable<ChatbotHangDiDuongItem> GetHangDangVeKho(int? idSanPham)
        {
            // Phiếu nhập kho chưa ghi sổ (TrangThai = 1) = hàng đang đi đường về kho (giống khối "Đang đi đường" trên Dashboard)
            const string sql = @"
                SELECT
                    pn.ID AS IDChungTu,
                    pn.SoChungTu,
                    pn.NgayNhap AS Ngay,
                    ISNULL(ncc.TenNhaCungCap, '') AS DoiTac,
                    ISNULL(pt.TenPhuongTien, '') AS PhuongTien,
                    ISNULL(pn.HoTenTaiXe, '') AS TaiXe,
                    ct.IDSanPham,
                    ISNULL(sp.MaSanPham, '') AS MaSanPham,
                    ISNULL(sp.TenSanPham, '') AS TenSanPham,
                    ISNULL(sp.DVT, '') AS DVT,
                    ISNULL(ct.SoLuong, 0) AS SoLuong,
                    CASE WHEN ct.ThanhTien > 0 THEN ct.ThanhTien
                         WHEN ct.TongSauThue > 0 THEN ct.TongSauThue
                         ELSE ISNULL(ct.SoLuong * ct.DonGia, 0) END AS ThanhTien
                FROM KHO_PhieuNhap pn
                INNER JOIN KHO_PhieuNhap_ChiTiet ct ON ct.IDPhieuNhap = pn.ID
                LEFT JOIN DM_SanPham sp ON ct.IDSanPham = sp.ID
                LEFT JOIN DM_NhaCungCap ncc ON pn.IDNhaCungCap = ncc.ID
                LEFT JOIN DM_PhuongTien pt ON pn.IDPhuongTien = pt.ID
                WHERE pn.IsDeleted = 0 AND pn.TrangThai = 1
                  AND (@IDSanPham IS NULL OR ct.IDSanPham = @IDSanPham)
                ORDER BY pn.NgayNhap DESC, pn.ID DESC";

            using (var conn = _db.CreateConnection())
                return conn.Query<ChatbotHangDiDuongItem>(sql, new { IDSanPham = idSanPham }).ToList();
        }

        public IEnumerable<ChatbotHangDiDuongItem> GetHangDangGiao(int? idSanPham)
        {
            // Đơn đặt hàng trạng thái 2 = Đang giao cho khách
            const string sql = @"
                SELECT
                    d.ID AS IDChungTu,
                    d.SoDonHang AS SoChungTu,
                    d.NgayTaoDon AS Ngay,
                    ISNULL(kh.TenKhachHang, '') AS DoiTac,
                    ISNULL(pt.TenPhuongTien, '') AS PhuongTien,
                    ISNULL(d.HoTenTaiXe, '') AS TaiXe,
                    ct.IDSanPham,
                    ISNULL(sp.MaSanPham, '') AS MaSanPham,
                    ISNULL(sp.TenSanPham, '') AS TenSanPham,
                    ISNULL(sp.DVT, '') AS DVT,
                    ISNULL(ct.SoLuong, 0) AS SoLuong,
                    ISNULL(ct.ThanhTien, 0) AS ThanhTien
                FROM NS_DonDatHang d
                INNER JOIN NS_DonDatHangChiTiet ct ON ct.IDDonDatHang = d.ID
                LEFT JOIN DM_SanPham sp ON ct.IDSanPham = sp.ID
                LEFT JOIN NS_KhachHang kh ON d.IDKhachHang = kh.ID
                LEFT JOIN DM_PhuongTien pt ON d.IDPhuongTien = pt.ID
                WHERE d.TrangThaiDon = 2
                  AND (@IDSanPham IS NULL OR ct.IDSanPham = @IDSanPham)
                ORDER BY d.NgayTaoDon DESC, d.ID DESC";

            using (var conn = _db.CreateConnection())
                return conn.Query<ChatbotHangDiDuongItem>(sql, new { IDSanPham = idSanPham }).ToList();
        }

        public IEnumerable<ChatbotChungTuKhoItem> GetNhapKho(DateTime tuNgay, DateTime denNgay, int? idSanPham, int? idKho)
        {
            // Phiếu nhập kho đã ghi sổ trong khoảng ngày (mọi loại nhập: mua hàng, chuyển kho, trả hàng...)
            const string sql = @"
                SELECT
                    pn.ID AS IDChungTu,
                    pn.SoChungTu,
                    pn.NgayNhap AS Ngay,
                    pn.IDKho,
                    ISNULL(kho.TenKhoHang, '') AS TenKho,
                    ISNULL(ncc.TenNhaCungCap, '') AS DoiTac,
                    ISNULL(ln.TenLoaiNhap, '') AS Loai,
                    ct.IDSanPham,
                    ISNULL(sp.MaSanPham, '') AS MaSanPham,
                    ISNULL(sp.TenSanPham, '') AS TenSanPham,
                    ISNULL(sp.DVT, '') AS DVT,
                    ISNULL(ct.SoLuong, 0) AS SoLuong,
                    CASE WHEN ct.ThanhTien > 0 THEN ct.ThanhTien
                         WHEN ct.TongSauThue > 0 THEN ct.TongSauThue
                         ELSE ISNULL(ct.SoLuong * ct.DonGia, 0) END AS ThanhTien
                FROM KHO_PhieuNhap pn
                INNER JOIN KHO_PhieuNhap_ChiTiet ct ON ct.IDPhieuNhap = pn.ID
                LEFT JOIN DM_SanPham sp ON ct.IDSanPham = sp.ID
                LEFT JOIN DM_KhoHang kho ON pn.IDKho = kho.ID
                LEFT JOIN DM_NhaCungCap ncc ON pn.IDNhaCungCap = ncc.ID
                LEFT JOIN DM_LoaiNhapKho ln ON pn.IDLoaiNhapKho = ln.ID
                WHERE pn.IsDeleted = 0 AND pn.TrangThai = 2
                  AND pn.NgayNhap >= @TuNgay AND pn.NgayNhap < @DenNgay
                  AND (@IDSanPham IS NULL OR ct.IDSanPham = @IDSanPham)
                  AND (@IDKho IS NULL OR pn.IDKho = @IDKho)
                ORDER BY pn.NgayNhap, pn.ID";

            using (var conn = _db.CreateConnection())
                return conn.Query<ChatbotChungTuKhoItem>(sql, new
                {
                    TuNgay = tuNgay.Date,
                    DenNgay = denNgay.Date.AddDays(1),
                    IDSanPham = idSanPham,
                    IDKho = idKho
                }).ToList();
        }

        public IEnumerable<ChatbotChungTuKhoItem> GetXuatKho(DateTime tuNgay, DateTime denNgay, int? idSanPham, int? idKho)
        {
            // Phiếu xuất kho đã ghi sổ trong khoảng ngày; khách hàng lấy từ chứng từ bán hàng / đơn đặt hàng, không có thì lấy người nhận
            const string sql = @"
                SELECT
                    px.ID AS IDChungTu,
                    px.SoChungTu,
                    px.NgayXuat AS Ngay,
                    px.IDKho,
                    ISNULL(kho.TenKhoHang, '') AS TenKho,
                    COALESCE(kh1.TenKhachHang, kh2.TenKhachHang, px.TenNguoiNhan, '') AS DoiTac,
                    CASE WHEN px.IDChungTuBanHang IS NOT NULL OR px.IDDonDatHang IS NOT NULL THEN N'Xuất bán' ELSE N'Xuất khác' END AS Loai,
                    ct.IDSanPham,
                    ISNULL(sp.MaSanPham, '') AS MaSanPham,
                    ISNULL(sp.TenSanPham, '') AS TenSanPham,
                    ISNULL(sp.DVT, '') AS DVT,
                    ISNULL(ct.SoLuong, 0) AS SoLuong,
                    ISNULL(ct.ThanhTien, 0) AS ThanhTien
                FROM KHO_PhieuXuat px
                INNER JOIN KHO_PhieuXuat_ChiTiet ct ON ct.IDPhieuXuat = px.ID
                LEFT JOIN DM_SanPham sp ON ct.IDSanPham = sp.ID
                LEFT JOIN DM_KhoHang kho ON px.IDKho = kho.ID
                LEFT JOIN BAN_ChungTuBanHang ctbh ON px.IDChungTuBanHang = ctbh.ID
                LEFT JOIN NS_DonDatHang dh ON px.IDDonDatHang = dh.ID
                LEFT JOIN NS_KhachHang kh1 ON ctbh.IDKhachHang = kh1.ID
                LEFT JOIN NS_KhachHang kh2 ON dh.IDKhachHang = kh2.ID
                WHERE px.IsDeleted = 0 AND px.TrangThai = 2
                  AND px.NgayXuat >= @TuNgay AND px.NgayXuat < @DenNgay
                  AND (@IDSanPham IS NULL OR ct.IDSanPham = @IDSanPham)
                  AND (@IDKho IS NULL OR px.IDKho = @IDKho)
                ORDER BY px.NgayXuat, px.ID";

            using (var conn = _db.CreateConnection())
                return conn.Query<ChatbotChungTuKhoItem>(sql, new
                {
                    TuNgay = tuNgay.Date,
                    DenNgay = denNgay.Date.AddDays(1),
                    IDSanPham = idSanPham,
                    IDKho = idKho
                }).ToList();
        }
    }
}
