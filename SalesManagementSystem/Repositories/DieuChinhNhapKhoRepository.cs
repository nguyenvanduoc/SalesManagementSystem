using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using Dapper;
using SalesManagementSystem.Data;
using SalesManagementSystem.Models.ViewModels;
using SalesManagementSystem.Repositories.Interfaces;

namespace SalesManagementSystem.Repositories
{
    public class DieuChinhNhapKhoRepository : IDieuChinhNhapKhoRepository
    {
        private readonly DbConnectionFactory _db;

        public DieuChinhNhapKhoRepository(DbConnectionFactory db)
        {
            _db = db;
            try
            {
                // Run script to ensure tables and SPs are created
                string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data", "create_dieu_chinh_phieu_nhap.sql");
                if (File.Exists(path))
                {
                    string script = File.ReadAllText(path);
                    using (var conn = _db.CreateConnection())
                    {
                        // Split script by GO
                        var commandTexts = script.Split(new[] { "\r\nGO", "\nGO", "\r\nGO\r\n", "\nGO\n" }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (var commandText in commandTexts)
                        {
                            if (!string.IsNullOrWhiteSpace(commandText))
                            {
                                conn.Execute(commandText);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Log exception if needed
            }
        }

        public IEnumerable<DieuChinhNhapKhoListViewModel> GetPaged(
            int page, int pageSize,
            string tuNgay, string denNgay,
            int? idLoaiNhap, int? idKho,
            int? idNhaCungCap, int? idKhachHang,
            string soChungTu, bool chiDonDieuChinh,
            out int totalRecords)
        {
            using (var conn = _db.CreateConnection())
            {
                var p = new DynamicParameters();
                p.Add("@TuNgay", string.IsNullOrWhiteSpace(tuNgay) ? (DateTime?)null : DateTime.Parse(tuNgay));
                p.Add("@DenNgay", string.IsNullOrWhiteSpace(denNgay) ? (DateTime?)null : DateTime.Parse(denNgay).AddDays(1).AddSeconds(-1));
                p.Add("@IDLoaiNhapKho", idLoaiNhap);
                p.Add("@IDKho", idKho);
                p.Add("@IDNhaCungCap", idNhaCungCap);
                p.Add("@IDKhachHang", idKhachHang);
                p.Add("@SoChungTu", string.IsNullOrWhiteSpace(soChungTu) ? null : soChungTu.Trim());
                p.Add("@ChiDonDieuChinh", chiDonDieuChinh ? 1 : 0);
                p.Add("@Offset", (page - 1) * pageSize);
                p.Add("@PageSize", pageSize);
                p.Add("@TotalRecords", dbType: DbType.Int32, direction: ParameterDirection.Output);

                var items = conn.Query<DieuChinhNhapKhoListViewModel>("sp_KHO_DieuChinhPhieuNhap_GetPaged", p, commandType: CommandType.StoredProcedure).ToList();
                totalRecords = p.Get<int>("@TotalRecords");

                // Calculate CongNo
                foreach (var item in items)
                {
                    item.CongNo = item.TongTien - item.DaThanhToan;
                }

                return items;
            }
        }

        public IEnumerable<DieuChinhNhapKhoHistoryViewModel> GetAdjustHistory(int idPhieuNhap)
        {
            using (var conn = _db.CreateConnection())
            {
                var histories = conn.Query<DieuChinhNhapKhoHistoryViewModel>(
                    "sp_KHO_DieuChinhPhieuNhap_GetHistory", 
                    new { IDPhieuNhap = idPhieuNhap }, 
                    commandType: CommandType.StoredProcedure).ToList();

                foreach (var h in histories)
                {
                    h.ChiTiets = conn.Query<DieuChinhNhapKhoHistoryDetailViewModel>(
                        "sp_KHO_DieuChinhPhieuNhap_GetHistoryDetail", 
                        new { IDDieuChinh = h.ID }, 
                        commandType: CommandType.StoredProcedure).ToList();
                }

                return histories;
            }
        }

        public void SaveAdjustment(DieuChinhNhapKhoPostModel model, int userId)
        {
            using (var conn = _db.CreateConnection())
            {
                var p = new DynamicParameters();
                p.Add("@IDPhieuNhap", model.IDPhieuNhap);
                p.Add("@LyDoDieuChinh", model.LyDoDieuChinh);
                p.Add("@ChiTietsJson", model.ChiTietsJson);
                p.Add("@IDLoaiNhapKho", model.IDLoaiNhapKho);
                p.Add("@IDKho", model.IDKho);
                p.Add("@IDKhoNguon", model.IDKhoNguon);
                p.Add("@IDNhaCungCap", model.IDNhaCungCap);
                p.Add("@IDKhachHang", model.IDKhachHang);
                p.Add("@IDPhuongTien", model.IDPhuongTien);
                p.Add("@NgayNhap", model.NgayNhap);
                p.Add("@NgayGiaoHang", model.NgayGiaoHang);
                p.Add("@HoTenTaiXe", model.HoTenTaiXe);
                p.Add("@SoDienThoaiTaiXe", model.SoDienThoaiTaiXe);
                p.Add("@SoHoaDon", model.SoHoaDon);
                p.Add("@NgayHoaDon", model.NgayHoaDon);
                p.Add("@GhiChu", model.GhiChu);
                p.Add("@NguoiTao", userId);

                conn.Execute("sp_KHO_DieuChinhPhieuNhap_Save", p, commandType: CommandType.StoredProcedure);

                conn.Execute(@"
                    ;WITH ChiTiet AS (
                        SELECT IDSanPham,
                               CASE WHEN ISNULL(DonGiaVanChuyen, 0) >= 0 THEN ISNULL(DonGiaVanChuyen, 0) ELSE 0 END AS DonGiaVanChuyen
                        FROM OPENJSON(@ChiTietsJson)
                        WITH (
                            IDSanPham INT '$.IDSanPham',
                            DonGiaVanChuyen DECIMAL(18,2) '$.DonGiaVanChuyen'
                        )
                    )
                    UPDATE ct
                    SET DonGiaVanChuyen = src.DonGiaVanChuyen,
                        TienVanChuyen = src.DonGiaVanChuyen * ct.SoLuong
                    FROM KHO_PhieuNhap_ChiTiet ct
                    INNER JOIN ChiTiet src ON ct.IDSanPham = src.IDSanPham
                    WHERE ct.IDPhieuNhap = @IDPhieuNhap;

                    UPDATE KHO_PhieuNhap
                    SET TienVanChuyen = ISNULL((
                        SELECT SUM(ISNULL(TienVanChuyen, 0))
                        FROM KHO_PhieuNhap_ChiTiet
                        WHERE IDPhieuNhap = @IDPhieuNhap
                    ), 0)
                    WHERE ID = @IDPhieuNhap;",
                    new { model.IDPhieuNhap, model.ChiTietsJson });
            }
        }
    }
}
