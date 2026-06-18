using Dapper;
using SalesManagementSystem.Data;
using SalesManagementSystem.Models.ViewModels;
using SalesManagementSystem.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace SalesManagementSystem.Repositories
{
    public class PhieuXuatKhoRepository : IPhieuXuatKhoRepository
    {
        private readonly DbConnectionFactory _db;

        public PhieuXuatKhoRepository(DbConnectionFactory db)
        {
            _db = db;
        }

        public List<PhieuXuatKhoListViewModel> GetList(int page, int pageSize, string tuNgay, string denNgay, string soChungTu, int? idKho, int? trangThai, int? idNhanSuNhan, out int totalRecords)
        {
            using (var conn = _db.CreateConnection())
            {
                var p = new DynamicParameters();
                p.Add("@Page", page);
                p.Add("@PageSize", pageSize);
                p.Add("@TuNgay", string.IsNullOrEmpty(tuNgay) ? null : tuNgay);
                p.Add("@DenNgay", string.IsNullOrEmpty(denNgay) ? null : denNgay);
                p.Add("@SoChungTu", string.IsNullOrEmpty(soChungTu) ? null : soChungTu);
                p.Add("@IDKho", idKho);
                p.Add("@TrangThai", trangThai);
                p.Add("@IDNhanSuNhan", idNhanSuNhan);
                p.Add("@TotalRecords", dbType: DbType.Int32, direction: ParameterDirection.Output);

                var list = conn.Query<PhieuXuatKhoListViewModel>("sp_KHO_PhieuXuat_GetList", p, commandType: CommandType.StoredProcedure).ToList();
                totalRecords = p.Get<int>("@TotalRecords");
                return list;
            }
        }

        public PhieuXuatKhoViewModel GetById(int id)
        {
            using (var conn = _db.CreateConnection())
            {
                var p = new DynamicParameters();
                p.Add("@ID", id);

                var model = conn.QueryFirstOrDefault<PhieuXuatKhoViewModel>("sp_KHO_PhieuXuat_GetById", p, commandType: CommandType.StoredProcedure);
                
                if (model != null)
                {
                    var pDetails = new DynamicParameters();
                    pDetails.Add("@IDPhieuXuat", id);
                    model.ChiTiets = conn.Query<PhieuXuatKhoChiTietViewModel>("sp_KHO_PhieuXuat_ChiTiet_GetList", pDetails, commandType: CommandType.StoredProcedure).ToList();

                    if (model.IDDonDatHang.HasValue)
                    {
                        var khQuery = @"
                            SELECT kh.MaKhachHang, kh.DiaChi, kh.SoDienThoai, kh.MaSoThue
                            FROM NS_DonDatHang dh
                            JOIN NS_KhachHang kh ON dh.IDKhachHang = kh.ID
                            WHERE dh.ID = @IDDonDatHang";
                        var khInfo = conn.QueryFirstOrDefault(khQuery, new { IDDonDatHang = model.IDDonDatHang });
                        if (khInfo != null)
                        {
                            model.MaKhachHang = khInfo.MaKhachHang;
                            model.DiaChiKhachHang = khInfo.DiaChi;
                            model.SoDienThoaiKhachHang = khInfo.SoDienThoai;
                            model.MaSoThueKhachHang = khInfo.MaSoThue;
                        }
                    }
                }

                return model;
            }
        }

        public string GenerateSoChungTu()
        {
            using (var conn = _db.CreateConnection())
            {
                string sql = "SELECT TOP 1 SoChungTu FROM KHO_PhieuXuat ORDER BY ID DESC";
                var lastSo = conn.QueryFirstOrDefault<string>(sql);

                if (string.IsNullOrEmpty(lastSo)) return "PX00001";

                string numPart = lastSo.Replace("PX", "");
                if (int.TryParse(numPart, out int num))
                {
                    return "PX" + (num + 1).ToString("D5");
                }
                return "PX00001";
            }
        }

        public int Insert(PhieuXuatKhoViewModel model, int userId)
        {
            using (var conn = _db.CreateConnection())
            {
                conn.Open();
                using (var tr = conn.BeginTransaction())
                {
                    try
                    {
                        var p = new DynamicParameters();
                        p.Add("@SoChungTu", model.SoChungTu);
                        p.Add("@NgayXuat", model.NgayXuat);
                        p.Add("@IDDonDatHang", model.IDDonDatHang);
                        p.Add("@IDKho", model.IDKho);
                        p.Add("@IDNhanSuNhan", model.IDNhanSuNhan);
                        p.Add("@TenNguoiNhan", model.TenNguoiNhan);
                        p.Add("@SoDienThoaiNguoiNhan", model.SoDienThoaiNguoiNhan);
                        p.Add("@GhiChu", model.GhiChu);
                        p.Add("@TongTienHang", model.TongTienHang);
                        p.Add("@TongTienThue", model.TongTienThue);
                        p.Add("@TongCong", model.TongCong);
                        p.Add("@NguoiTao", userId);

                        int newId = conn.QuerySingle<int>("sp_KHO_PhieuXuat_Insert", p, transaction: tr, commandType: CommandType.StoredProcedure);

                        foreach (var ct in model.ChiTiets)
                        {
                            var pCt = new DynamicParameters();
                            pCt.Add("@IDPhieuXuat", newId);
                            pCt.Add("@IDSanPham", ct.IDSanPham);
                            pCt.Add("@SoLuong", ct.SoLuong);
                            pCt.Add("@DonGia", ct.DonGia);
                            pCt.Add("@ThanhTien", ct.ThanhTien);
                            pCt.Add("@ThueGTGT", ct.ThueGTGT);
                            pCt.Add("@TienThue", ct.TienThue);
                            pCt.Add("@TongSauThue", ct.TongSauThue);
                            pCt.Add("@GhiChu", ct.GhiChu);

                            conn.Execute("sp_KHO_PhieuXuat_ChiTiet_Insert", pCt, transaction: tr, commandType: CommandType.StoredProcedure);
                        }

                        tr.Commit();
                        return newId;
                    }
                    catch
                    {
                        tr.Rollback();
                        throw;
                    }
                }
            }
        }

        public void UpdateStatus(int id, int status, int userId)
        {
            using (var conn = _db.CreateConnection())
            {
                var p = new DynamicParameters();
                p.Add("@ID", id);
                p.Add("@TrangThai", status);
                p.Add("@NguoiGhi", userId);
                conn.Execute("sp_KHO_PhieuXuat_UpdateStatus", p, commandType: CommandType.StoredProcedure);
            }
        }

        public void Cancel(int id, int userId, string reason)
        {
            using (var conn = _db.CreateConnection())
            {
                var p = new DynamicParameters();
                p.Add("@ID", id);
                p.Add("@NguoiHuy", userId);
                p.Add("@LyDoHuy", reason);
                conn.Execute("sp_KHO_PhieuXuat_Cancel", p, commandType: CommandType.StoredProcedure);
            }
        }
    }
}
