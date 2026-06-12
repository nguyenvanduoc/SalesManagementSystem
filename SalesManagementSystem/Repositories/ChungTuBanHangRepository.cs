using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using SalesManagementSystem.Data;
using SalesManagementSystem.Models.Entities;
using SalesManagementSystem.Models.ViewModels;
using SalesManagementSystem.Repositories.Interfaces;

namespace SalesManagementSystem.Repositories
{
    public class ChungTuBanHangRepository : IChungTuBanHangRepository
    {
        private readonly DbConnectionFactory _db;

        public ChungTuBanHangRepository(DbConnectionFactory db)
        {
            _db = db;
        }

        public string GenerateSoChungTu()
        {
            using (var conn = _db.CreateConnection())
            {
                var query = "SELECT TOP 1 SoChungTu FROM BAN_ChungTuBanHang ORDER BY ID DESC";
                var lastSo = conn.ExecuteScalar<string>(query);
                if (string.IsNullOrEmpty(lastSo)) return "BH00001";
                var numStr = lastSo.Replace("BH", "");
                if (int.TryParse(numStr, out int num))
                {
                    return "BH" + (num + 1).ToString("D5");
                }
                return "BH" + DateTime.Now.ToString("yyyyMMddHHmmss");
            }
        }

        public IEnumerable<ChungTuBanHangListViewModel> GetList(string tuNgay, string denNgay, string soChungTu, int? idKhachHang, int? idKho, int? trangThai)
        {
            using (var conn = _db.CreateConnection())
            {
                var p = new DynamicParameters();
                p.Add("@TuNgay", string.IsNullOrEmpty(tuNgay) ? null : tuNgay);
                p.Add("@DenNgay", string.IsNullOrEmpty(denNgay) ? null : denNgay);
                p.Add("@SoChungTu", string.IsNullOrEmpty(soChungTu) ? null : soChungTu);
                p.Add("@IDKhachHang", idKhachHang);
                p.Add("@IDKho", idKho);
                p.Add("@TrangThai", trangThai);

                return conn.Query<ChungTuBanHangListViewModel>("sp_BAN_ChungTuBanHang_GetList", p, commandType: System.Data.CommandType.StoredProcedure);
            }
        }

        public IEnumerable<DonHangChungTuViewModel> GetDonHangList(string tuNgay, string denNgay, string soDonHang, int? idKhachHang, int? trangThaiChungTu)
        {
            using (var conn = _db.CreateConnection())
            {
                var p = new DynamicParameters();
                p.Add("@TuNgay", string.IsNullOrEmpty(tuNgay) ? null : tuNgay);
                p.Add("@DenNgay", string.IsNullOrEmpty(denNgay) ? null : denNgay);
                p.Add("@SoDonHang", string.IsNullOrEmpty(soDonHang) ? null : soDonHang);
                p.Add("@IDKhachHang", idKhachHang);
                p.Add("@TrangThaiChungTu", trangThaiChungTu);

                return conn.Query<DonHangChungTuViewModel>("sp_BAN_ChungTuBanHang_GetDonHangList", p, commandType: System.Data.CommandType.StoredProcedure);
            }
        }

        public ChungTuBanHangViewModel GetById(int id)
        {
            using (var conn = _db.CreateConnection())
            {
                var p = new DynamicParameters();
                p.Add("@ID", id);
                var master = conn.QueryFirstOrDefault<BAN_ChungTuBanHang>("sp_BAN_ChungTuBanHang_GetById", p, commandType: System.Data.CommandType.StoredProcedure);
                if (master == null) return null;

                var details = conn.Query<ChungTuBanHangChiTietViewModel>("sp_BAN_ChungTuBanHang_ChiTiet_GetList", new { IDChungTuBanHang = id }, commandType: System.Data.CommandType.StoredProcedure).ToList();

                var vm = new ChungTuBanHangViewModel
                {
                    ID = master.ID,
                    SoChungTu = master.SoChungTu,
                    NgayChungTu = master.NgayChungTu,
                    IDDonDatHang = master.IDDonDatHang,
                    IDKhachHang = master.IDKhachHang,
                    IDKho = master.IDKho,
                    IDTaiKhoanThanhToan = master.IDTaiKhoanThanhToan,
                    TongTienHang = master.TongTienHang,
                    TongTienThue = master.TongTienThue,
                    TongCong = master.TongCong,
                    DaThanhToan = master.DaThanhToan,
                    ConLai = master.ConLai,
                    TrangThai = master.TrangThai,
                    ChiTiets = details
                };
                
                // Get display names
                vm.SoDonHang = conn.ExecuteScalar<string>("SELECT SoDonHang FROM NS_DonDatHang WHERE ID = @ID", new { ID = master.IDDonDatHang });
                vm.TenKhachHang = conn.ExecuteScalar<string>("SELECT TenKhachHang FROM NS_KhachHang WHERE ID = @ID", new { ID = master.IDKhachHang });
                vm.TenKhoHang = conn.ExecuteScalar<string>("SELECT TenKhoHang FROM DM_KhoHang WHERE ID = @ID", new { ID = master.IDKho });
                vm.SoTaiKhoanThanhToan = conn.ExecuteScalar<string>("SELECT SoTaiKhoan + ' - ' + TenTaiKhoan FROM KT_TaiKhoanKeToan WHERE ID = @ID", new { ID = master.IDTaiKhoanThanhToan });

                return vm;
            }
        }

        public int Insert(ChungTuBanHangViewModel model, int nguoiTao)
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
                        p.Add("@NgayChungTu", model.NgayChungTu);
                        p.Add("@IDDonDatHang", model.IDDonDatHang);
                        p.Add("@IDKhachHang", model.IDKhachHang);
                        p.Add("@IDKho", model.IDKho);
                        p.Add("@IDTaiKhoanThanhToan", model.IDTaiKhoanThanhToan);
                        p.Add("@TongTienHang", model.TongTienHang);
                        p.Add("@TongTienThue", model.TongTienThue);
                        p.Add("@TongCong", model.TongCong);
                        p.Add("@DaThanhToan", model.DaThanhToan);
                        p.Add("@ConLai", model.ConLai);
                        p.Add("@TrangThai", model.TrangThai > 0 ? model.TrangThai : 2); // Sử dụng trạng thái truyền vào, mặc định 2
                        p.Add("@NguoiTao", nguoiTao);
                        p.Add("@NewID", dbType: System.Data.DbType.Int32, direction: System.Data.ParameterDirection.Output);

                        conn.Execute("sp_BAN_ChungTuBanHang_Insert", p, transaction: tr, commandType: System.Data.CommandType.StoredProcedure);
                        int newId = p.Get<int>("@NewID");

                        foreach (var ct in model.ChiTiets)
                        {
                            var pCt = new DynamicParameters();
                            pCt.Add("@IDChungTuBanHang", newId);
                            pCt.Add("@IDSanPham", ct.IDSanPham);
                            pCt.Add("@STT", ct.STT);
                            pCt.Add("@SoLuong", ct.SoLuong);
                            pCt.Add("@DonGia", ct.DonGia);
                            pCt.Add("@ThanhTien", ct.ThanhTien);
                            pCt.Add("@ThueGTGT", ct.ThueGTGT);
                            pCt.Add("@TienThue", ct.TienThue);
                            pCt.Add("@TongSauThue", ct.TongSauThue);
                            pCt.Add("@GhiChu", ct.GhiChu);

                            conn.Execute("sp_BAN_ChungTuBanHang_ChiTiet_Insert", pCt, transaction: tr, commandType: System.Data.CommandType.StoredProcedure);
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

        public void UpdateStatus(int id, int trangThai, int nguoiCapNhat)
        {
            using (var conn = _db.CreateConnection())
            {
                var p = new DynamicParameters();
                p.Add("@ID", id);
                p.Add("@TrangThai", trangThai);
                p.Add("@NguoiCapNhat", nguoiCapNhat);

                conn.Execute("sp_BAN_ChungTuBanHang_UpdateStatus", p, commandType: System.Data.CommandType.StoredProcedure);
            }
        }

        public void Cancel(int id, int nguoiHuy, string lyDo)
        {
            using (var conn = _db.CreateConnection())
            {
                var p = new DynamicParameters();
                p.Add("@ID", id);
                p.Add("@NguoiHuy", nguoiHuy);
                p.Add("@LyDoHuy", lyDo);

                conn.Execute("sp_BAN_ChungTuBanHang_Cancel", p, commandType: System.Data.CommandType.StoredProcedure);
            }
        }
    }
}
