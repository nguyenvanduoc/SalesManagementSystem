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

        public List<PhieuXuatKhoListViewModel> GetList(int page, int pageSize, string tuNgay, string denNgay, string soChungTu, int? idKho, int? trangThai, int? idNhanSuNhan, int? idSanPham, int? idNhaCungCap, string tenNguoiGiao, int? idPhuongTien, string tenNguoiNhan, out int totalRecords)
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
                p.Add("@IDSanPham", idSanPham);
                p.Add("@IDNhaCungCap", idNhaCungCap);
                p.Add("@TenNguoiGiao", string.IsNullOrEmpty(tenNguoiGiao) ? null : tenNguoiGiao);
                p.Add("@IDPhuongTien", idPhuongTien);
                p.Add("@TenNguoiNhan", string.IsNullOrEmpty(tenNguoiNhan) ? null : tenNguoiNhan);
                p.Add("@TotalRecords", dbType: DbType.Int32, direction: ParameterDirection.Output);

                var list = conn.Query<PhieuXuatKhoListViewModel>("sp_KHO_PhieuXuat_GetList", p, commandType: CommandType.StoredProcedure).ToList();
                totalRecords = p.Get<int>("@TotalRecords");

                if (list.Any())
                {
                    var ids = list.Select(x => x.ID).ToList();
                    var sqlSoLuong = @"SELECT IDPhieuXuat, SUM(SoLuong) as TongSoLuong FROM KHO_PhieuXuat_ChiTiet WHERE IDPhieuXuat IN @Ids GROUP BY IDPhieuXuat";
                    var dapperResult = conn.Query(sqlSoLuong, new { Ids = ids });
                    var dict = dapperResult.ToDictionary(row => (int)row.IDPhieuXuat, row => (decimal)(row.TongSoLuong ?? 0m));
                    foreach (var item in list)
                    {
                        if (dict.TryGetValue(item.ID, out var sl))
                            item.TongSoLuong = sl;
                    }
                }

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

        public List<PhieuXuatKhoChiTietViewModel> GetChiTiet(int idPhieuXuat)
        {
            using (var conn = _db.CreateConnection())
            {
                var p = new DynamicParameters();
                p.Add("@IDPhieuXuat", idPhieuXuat);
                return conn.Query<PhieuXuatKhoChiTietViewModel>("sp_KHO_PhieuXuat_ChiTiet_GetList", p, commandType: CommandType.StoredProcedure).ToList();
            }
        }

        public string GenerateSoChungTu()
        {
            using (var conn = _db.CreateConnection())
            {
                string sql = "SELECT TOP 1 SoChungTu FROM KHO_PhieuXuat WHERE SoChungTu LIKE 'PX%' ORDER BY ID DESC";
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

        public int Save(PhieuXuatKhoViewModel model, int userId)
        {
            using (var conn = _db.CreateConnection())
            {
                conn.Open();
                using (var tr = conn.BeginTransaction())
                {
                    try
                    {
                        // Kiểm tra tồn kho trước khi lưu
                        if (model.ChiTiets != null && model.ChiTiets.Any())
                        {
                            var itemsCheck = model.ChiTiets.Select(x => new CheckTonKhoRequestItem 
                            { 
                                IDSanPham = x.IDSanPham, 
                                SoLuongCanXuat = x.SoLuong 
                            }).ToList();

                            var pTonKho = new DynamicParameters();
                            pTonKho.Add("@IDKho", model.IDKho);
                            pTonKho.Add("@ListSanPham", Newtonsoft.Json.JsonConvert.SerializeObject(itemsCheck));
                            pTonKho.Add("@ExcludeSoChungTu", string.IsNullOrEmpty(model.SoChungTu) ? null : model.SoChungTu);

                            var checkTon = conn.Query<CheckTonKhoResponseViewModel>("sp_KHO_TonKho_CheckByKho", pTonKho, transaction: tr, commandType: CommandType.StoredProcedure).ToList();
                            var missingItems = checkTon.Where(x => !x.IsDuTon).ToList();
                            if (missingItems.Any())
                            {
                                var msg = string.Join("<br/>", missingItems.Select(x => $"Sản phẩm <b>[{x.MaSanPham}] - {x.TenSanPham}</b> vượt quá tồn kho hiện tại! (Tồn hiện tại: <b>{x.SoLuongTon:N0}</b>, Yêu cầu xuất: <b>{x.SoLuongCanXuat:N0}</b>)"));
                                throw new Exception(msg);
                            }
                        }

                        var p = new DynamicParameters();
                        p.Add("@ID", model.ID);
                        p.Add("@SoChungTu", model.SoChungTu, dbType: DbType.String, direction: ParameterDirection.InputOutput, size: 50);
                        p.Add("@NgayXuat", model.NgayXuat);
                        p.Add("@IDKho", model.IDKho);
                        p.Add("@IDDonDatHang", model.IDDonDatHang);
                        p.Add("@IDChungTuBanHang", model.IDChungTuBanHang);
                        p.Add("@TenNguoiNhan", model.TenNguoiNhan);
                        p.Add("@SoDienThoaiNguoiNhan", model.SoDienThoaiNguoiNhan);
                        p.Add("@GhiChu", model.GhiChu);
                        p.Add("@TongTienHang", model.TongTienHang);
                        p.Add("@TongTienThue", model.TongTienThue);
                        p.Add("@TongCong", model.TongCong);
                        p.Add("@TrangThai", model.TrangThai);
                        p.Add("@UserId", userId);

                        int newId = conn.QuerySingle<int>("sp_KHO_PhieuXuat_Save", p, transaction: tr, commandType: CommandType.StoredProcedure);
                        model.SoChungTu = p.Get<string>("@SoChungTu");

                        conn.Execute("DELETE FROM KHO_PhieuXuat_ChiTiet WHERE IDPhieuXuat = @IDPhieuXuat", new { IDPhieuXuat = newId }, transaction: tr);

                        if (model.ChiTiets != null && model.ChiTiets.Any())
                        {
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
                        }

                        if (model.TrangThai == 2)
                        {
                            var pGhi = new DynamicParameters();
                            pGhi.Add("@ID", newId);
                            pGhi.Add("@UserId", userId);
                            conn.Execute("sp_KHO_PhieuXuat_GhiSo", pGhi, transaction: tr, commandType: CommandType.StoredProcedure);
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

        public void GhiSo(int id, int userId)
        {
            using (var conn = _db.CreateConnection())
            {
                var px = GetById(id);
                if (px == null) throw new Exception("Không tìm thấy phiếu xuất kho");

                var chiTiets = GetChiTiet(id);
                if (chiTiets != null && chiTiets.Any())
                {
                    var itemsCheck = chiTiets.Select(x => new CheckTonKhoRequestItem 
                    { 
                        IDSanPham = x.IDSanPham, 
                        SoLuongCanXuat = x.SoLuong 
                    }).ToList();

                    var pTonKho = new DynamicParameters();
                    pTonKho.Add("@IDKho", px.IDKho);
                    pTonKho.Add("@ListSanPham", Newtonsoft.Json.JsonConvert.SerializeObject(itemsCheck));
                    pTonKho.Add("@ExcludeSoChungTu", px.SoChungTu);

                    var checkTon = conn.Query<CheckTonKhoResponseViewModel>("sp_KHO_TonKho_CheckByKho", pTonKho, commandType: CommandType.StoredProcedure).ToList();
                    var missingItems = checkTon.Where(x => !x.IsDuTon).ToList();
                    if (missingItems.Any())
                    {
                        var msg = string.Join("<br/>", missingItems.Select(x => $"Sản phẩm <b>[{x.MaSanPham}] - {x.TenSanPham}</b> vượt quá tồn kho hiện tại! (Tồn hiện tại: <b>{x.SoLuongTon:N0}</b>, Yêu cầu xuất: <b>{x.SoLuongCanXuat:N0}</b>)"));
                        throw new Exception(msg);
                    }
                }

                var p = new DynamicParameters();
                p.Add("@ID", id);
                p.Add("@UserId", userId);
                conn.Execute("sp_KHO_PhieuXuat_GhiSo", p, commandType: CommandType.StoredProcedure);
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
                var px = GetById(id);
                if (px == null) throw new Exception("Không tìm thấy phiếu xuất kho");

                if (px.IDDonDatHang.HasValue && px.IDDonDatHang.Value > 0)
                {
                    throw new Exception("Không thể hủy phiếu xuất kho được tạo từ đơn đặt hàng.");
                }

                var p = new DynamicParameters();
                p.Add("@ID", id);
                p.Add("@NguoiHuy", userId);
                p.Add("@LyDoHuy", reason);
                conn.Execute("sp_KHO_PhieuXuat_Cancel", p, commandType: CommandType.StoredProcedure);

                if (!string.IsNullOrEmpty(px.SoChungTu))
                {
                    conn.Execute("DELETE FROM KHO_GiaoDichKho WHERE LoaiChungTu = 2 AND SoChungTu = @SoChungTu", new { SoChungTu = px.SoChungTu });
                }
            }
        }
    }
}
