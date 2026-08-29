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
            try
            {
                using (var conn = _db.CreateConnection())
                {
                    conn.Execute("IF COL_LENGTH('BAN_ChungTuBanHang', 'TongTienKhac') IS NULL ALTER TABLE BAN_ChungTuBanHang ADD TongTienKhac DECIMAL(18,2) NULL");
                    conn.Execute("IF COL_LENGTH('BAN_ChungTuBanHang_ChiTiet', 'DonGiaBocXep') IS NULL ALTER TABLE BAN_ChungTuBanHang_ChiTiet ADD DonGiaBocXep DECIMAL(18,2) NULL");
                    conn.Execute("IF COL_LENGTH('BAN_ChungTuBanHang_ChiTiet', 'ThanhTienBocXep') IS NULL ALTER TABLE BAN_ChungTuBanHang_ChiTiet ADD ThanhTienBocXep DECIMAL(18,2) NULL");
                    conn.Execute("IF COL_LENGTH('BAN_ChungTuBanHang_ChiTiet', 'SoTienKhac') IS NULL ALTER TABLE BAN_ChungTuBanHang_ChiTiet ADD SoTienKhac DECIMAL(18,2) NULL");
                    conn.Execute("IF COL_LENGTH('BAN_ChungTuBanHang_ChiTiet', 'ThanhTienHang') IS NULL ALTER TABLE BAN_ChungTuBanHang_ChiTiet ADD ThanhTienHang DECIMAL(18,2) NULL");
                }
            }
            catch { }
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

        public IEnumerable<DonHangChungTuViewModel> GetDonHangList(string tuNgay, string denNgay, string soDonHang, int? idKhachHang, int? trangThaiChungTu, int? idSanPham = null, int? idPhuongTien = null, string hoTenTaiXe = null)
        {
            using (var conn = _db.CreateConnection())
            {
                var p = new DynamicParameters();
                p.Add("@TuNgay", string.IsNullOrEmpty(tuNgay) ? null : tuNgay);
                p.Add("@DenNgay", string.IsNullOrEmpty(denNgay) ? null : denNgay);
                p.Add("@SoDonHang", string.IsNullOrEmpty(soDonHang) ? null : soDonHang);
                p.Add("@IDKhachHang", idKhachHang);
                p.Add("@TrangThaiChungTu", trangThaiChungTu);
                p.Add("@IDSanPham", idSanPham);
                p.Add("@IDPhuongTien", idPhuongTien);
                p.Add("@HoTenTaiXe", string.IsNullOrEmpty(hoTenTaiXe) ? null : hoTenTaiXe);

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

                string sqlChiTiet = @"
                    SELECT 
                        c.ID, c.IDChungTuBanHang, c.IDSanPham,
                        s.MaSanPham, s.TenSanPham, s.DVT,
                        c.STT, c.SoLuong, c.DonGia,
                        c.DonGiaBocXep, c.ThanhTienBocXep, c.SoTienKhac, c.SoTienChietKhau, c.ChuongTrinhTichLuySale, c.ThanhTienHang,
                        c.ThanhTien, c.ThueGTGT, c.TienThue, c.TongSauThue, c.GhiChu
                    FROM BAN_ChungTuBanHang_ChiTiet c
                    JOIN DM_SanPham s ON c.IDSanPham = s.ID
                    WHERE c.IDChungTuBanHang = @IDChungTuBanHang
                    ORDER BY c.STT;
                ";
                var details = conn.Query<ChungTuBanHangChiTietViewModel>(sqlChiTiet, new { IDChungTuBanHang = id }).ToList();

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
                    PhiBocXep = master.PhiBocXep,
                    TongTienKhac = master.TongTienKhac ?? 0m,
                    TongTienChietKhau = master.TongTienChietKhau,
                    TongChuongTrinhTichLuySale = master.TongChuongTrinhTichLuySale,
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

        public int Insert(ChungTuBanHangViewModel model, int nguoiTao, bool ghiSo = false, int trangThai = 1)
        {
            using (var conn = _db.CreateConnection())
            {
                conn.Open();
                using (var tr = conn.BeginTransaction())
                {
                    try
                    {
                        if (model.IDKho <= 0)
                        {
                            model.IDKho = 1;
                        }

                        if (ghiSo)
                        {
                            trangThai = 2;
                        }

                        bool isTruKho = (trangThai == 1 || trangThai == 2);

                        // 1. Kiểm tra tồn kho nếu là Đề nghị ghi (1) hoặc Đã ghi (2)
                        if (isTruKho)
                        {
                            var itemsCheck = model.ChiTiets.Select(x => new CheckTonKhoRequestItem { IDSanPham = x.IDSanPham, SoLuongCanXuat = x.SoLuong }).ToList();
                            var pTonKho = new DynamicParameters();
                            pTonKho.Add("@IDKho", model.IDKho);
                            pTonKho.Add("@ListSanPham", Newtonsoft.Json.JsonConvert.SerializeObject(itemsCheck));
                            var checkTon = conn.Query<CheckTonKhoResponseViewModel>("sp_KHO_TonKho_CheckByKho", pTonKho, transaction: tr, commandType: System.Data.CommandType.StoredProcedure).ToList();
                            var missingItems = checkTon.Where(x => !x.IsDuTon).ToList();
                            if (missingItems.Any())
                            {
                                var msg = string.Join("; ", missingItems.Select(x => $"Sản phẩm [{x.MaSanPham}] - {x.TenSanPham} vượt tồn kho. Tồn hiện tại: {x.SoLuongTon:N0}, cần xuất: {x.SoLuongCanXuat:N0}."));
                                throw new Exception(msg);
                            }
                        }

                        var p = new DynamicParameters();
                        p.Add("@SoChungTu", model.SoChungTu);
                        p.Add("@NgayChungTu", model.NgayChungTu);
                        p.Add("@IDDonDatHang", model.IDDonDatHang);
                        p.Add("@IDKhachHang", model.IDKhachHang);
                        p.Add("@IDKho", model.IDKho);
                        p.Add("@IDTaiKhoanThanhToan", model.IDTaiKhoanThanhToan);
                        p.Add("@TongTienHang", model.TongTienHang);
                        p.Add("@TongTienThue", model.TongTienThue);
                        p.Add("@PhiBocXep", model.PhiBocXep);
                        p.Add("@TongTienKhac", model.TongTienKhac);
                        p.Add("@TongTienChietKhau", model.TongTienChietKhau ?? 0m);
                        p.Add("@TongChuongTrinhTichLuySale", model.TongChuongTrinhTichLuySale ?? 0m);
                        p.Add("@TongCong", model.TongCong);
                        p.Add("@DaThanhToan", model.DaThanhToan);
                        p.Add("@ConLai", model.ConLai);
                        p.Add("@TrangThai", trangThai);
                        p.Add("@NguoiTao", nguoiTao);
                        p.Add("@NewID", dbType: System.Data.DbType.Int32, direction: System.Data.ParameterDirection.Output);

                        conn.Execute("sp_BAN_ChungTuBanHang_Insert", p, transaction: tr, commandType: System.Data.CommandType.StoredProcedure);
                        int newId = p.Get<int>("@NewID");
                        decimal tongGiaVonThucTe = 0;

                        foreach (var ct in model.ChiTiets)
                        {
                            decimal donGiaVon = 0;
                            if (trangThai == 2)
                            {
                                string sqlAvg = @"
                                    DECLARE @TongSoLuong DECIMAL(18,2) = 0;
                                    DECLARE @TongGiaTri DECIMAL(18,2) = 0;
                                    SELECT 
                                        @TongSoLuong = ISNULL(SUM(SoLuongNhap),0) - ISNULL(SUM(SoLuongXuat),0),
                                        @TongGiaTri = ISNULL(SUM(SoLuongNhap * DonGia),0) - ISNULL(SUM(SoLuongXuat * ISNULL(DonGiaVon, 0)),0)
                                    FROM KHO_GiaoDichKho
                                    WHERE IDSanPham = @IDSanPham AND NgayChungTu <= @NgayChungTu
                                      AND IDKho IN (SELECT ID FROM DM_KhoHang WHERE ISNULL(IsKhoChinh, 0) = 1); -- Chỉ tính giá vốn từ kho chính
                                    
                                    IF @TongSoLuong > 0
                                        SELECT CAST(@TongGiaTri / @TongSoLuong AS DECIMAL(18,2));
                                    ELSE
                                        SELECT CAST(0 AS DECIMAL(18,2));
                                ";
                                donGiaVon = conn.ExecuteScalar<decimal>(sqlAvg, new { IDSanPham = ct.IDSanPham, NgayChungTu = model.NgayChungTu }, transaction: tr);
                            }
                            decimal thanhTienVon = ct.SoLuong * donGiaVon;
                            if (trangThai == 2) tongGiaVonThucTe += thanhTienVon;

                            var pCt = new DynamicParameters();
                            pCt.Add("@IDChungTuBanHang", newId);
                            pCt.Add("@IDSanPham", ct.IDSanPham);
                            pCt.Add("@STT", ct.STT);
                            pCt.Add("@SoLuong", ct.SoLuong);
                            pCt.Add("@DonGia", ct.DonGia);
                            pCt.Add("@DonGiaBocXep", ct.DonGiaBocXep);
                            pCt.Add("@ThanhTienBocXep", ct.ThanhTienBocXep);
                            pCt.Add("@SoTienKhac", ct.SoTienKhac);
                            pCt.Add("@SoTienChietKhau", ct.SoTienChietKhau);
                            pCt.Add("@ChuongTrinhTichLuySale", ct.ChuongTrinhTichLuySale);
                            pCt.Add("@DonGiaVon", (trangThai == 2) ? (decimal?)donGiaVon : null);
                            pCt.Add("@ThanhTienVon", (trangThai == 2) ? (decimal?)thanhTienVon : null);
                            pCt.Add("@ThanhTienHang", ct.ThanhTienHang);
                            pCt.Add("@ThanhTien", ct.ThanhTien);
                            pCt.Add("@ThueGTGT", ct.ThueGTGT);
                            pCt.Add("@TienThue", ct.TienThue);
                            pCt.Add("@TongSauThue", ct.TongSauThue);
                            pCt.Add("@GhiChu", ct.GhiChu);

                            string sqlInsertCt = @"
                                INSERT INTO BAN_ChungTuBanHang_ChiTiet 
                                (IDChungTuBanHang, IDSanPham, STT, SoLuong, DonGia, DonGiaBocXep, ThanhTienBocXep, SoTienKhac, SoTienChietKhau, ChuongTrinhTichLuySale, DonGiaVon, ThanhTienVon, ThanhTienHang, ThanhTien, ThueGTGT, TienThue, TongSauThue, GhiChu)
                                VALUES 
                                (@IDChungTuBanHang, @IDSanPham, @STT, @SoLuong, @DonGia, @DonGiaBocXep, @ThanhTienBocXep, @SoTienKhac, @SoTienChietKhau, @ChuongTrinhTichLuySale, @DonGiaVon, @ThanhTienVon, @ThanhTienHang, @ThanhTien, @ThueGTGT, @TienThue, @TongSauThue, @GhiChu);
                            ";
                            conn.Execute(sqlInsertCt, pCt, transaction: tr);
                        }

                        // 3. KHO_PhieuXuat
                        var lastSo = conn.ExecuteScalar<string>("SELECT TOP 1 SoChungTu FROM KHO_PhieuXuat ORDER BY ID DESC", transaction: tr);
                        string soPx = "PX00001";
                        if (!string.IsNullOrEmpty(lastSo))
                        {
                            var numStr = lastSo.Replace("PX", "");
                            if (int.TryParse(numStr, out int num))
                                soPx = "PX" + (num + 1).ToString("D5");
                        }

                        string tenKhachHang = conn.ExecuteScalar<string>("SELECT TenKhachHang FROM NS_KhachHang WHERE ID = @ID", new { ID = model.IDKhachHang }, transaction: tr) ?? "";

                        var pPx = new DynamicParameters();
                        pPx.Add("@SoChungTu", soPx);
                        pPx.Add("@NgayXuat", model.NgayChungTu);
                        pPx.Add("@IDKho", model.IDKho);
                        pPx.Add("@IDNhanSuNhan", null, System.Data.DbType.Int32);
                        pPx.Add("@TenNguoiNhan", tenKhachHang);
                        pPx.Add("@IDChungTuBanHang", newId);
                        pPx.Add("@IDDonDatHang", model.IDDonDatHang);
                        pPx.Add("@GhiChu", "Xuất kho tự động từ CTBH " + model.SoChungTu);
                        pPx.Add("@TongTienHang", model.TongTienHang);
                        pPx.Add("@TongTienThue", model.TongTienThue);
                        pPx.Add("@TongCong", model.TongCong);
                        pPx.Add("@NguoiTao", nguoiTao);
                        pPx.Add("@TrangThai", trangThai);
                        pPx.Add("@NewID", dbType: System.Data.DbType.Int32, direction: System.Data.ParameterDirection.Output);

                        conn.Execute("INSERT INTO KHO_PhieuXuat (SoChungTu, NgayXuat, IDChungTuBanHang, IDDonDatHang, IDKho, IDNhanSuNhan, TenNguoiNhan, GhiChu, TongTienHang, TongTienThue, TongCong, NguoiTao, NgayTao, TrangThai) VALUES (@SoChungTu, @NgayXuat, @IDChungTuBanHang, @IDDonDatHang, @IDKho, @IDNhanSuNhan, @TenNguoiNhan, @GhiChu, @TongTienHang, @TongTienThue, @TongCong, @NguoiTao, GETDATE(), @TrangThai); SELECT @NewID = SCOPE_IDENTITY();", pPx, transaction: tr);
                        int idPhieuXuat = pPx.Get<int>("@NewID");

                        // 4. KHO_PhieuXuat_ChiTiet and KHO_GiaoDichKho
                        int sttPx = 1;
                        foreach (var ct in model.ChiTiets)
                        {
                            var pPxCt = new DynamicParameters();
                            pPxCt.Add("@IDPhieuXuat", idPhieuXuat);
                            pPxCt.Add("@IDSanPham", ct.IDSanPham);
                            pPxCt.Add("@STT", sttPx++);
                            pPxCt.Add("@SoLuong", ct.SoLuong);
                            pPxCt.Add("@DonGia", ct.DonGia);
                            pPxCt.Add("@ThanhTien", ct.ThanhTien);
                            decimal donGiaVonPx = 0;
                            if (trangThai == 2)
                            {
                                string sqlAvg = @"
                                    DECLARE @TongSoLuong DECIMAL(18,2) = 0;
                                    DECLARE @TongGiaTri DECIMAL(18,2) = 0;
                                    SELECT 
                                        @TongSoLuong = ISNULL(SUM(SoLuongNhap),0) - ISNULL(SUM(SoLuongXuat),0),
                                        @TongGiaTri = ISNULL(SUM(SoLuongNhap * DonGia),0) - ISNULL(SUM(SoLuongXuat * ISNULL(DonGiaVon, 0)),0)
                                    FROM KHO_GiaoDichKho
                                    WHERE IDSanPham = @IDSanPham AND NgayChungTu <= @NgayChungTu
                                      AND IDKho IN (SELECT ID FROM DM_KhoHang WHERE ISNULL(IsKhoChinh, 0) = 1); -- Chỉ tính giá vốn từ kho chính
                                    
                                    IF @TongSoLuong > 0
                                        SELECT CAST(@TongGiaTri / @TongSoLuong AS DECIMAL(18,2));
                                    ELSE
                                        SELECT CAST(0 AS DECIMAL(18,2));
                                ";
                                donGiaVonPx = conn.ExecuteScalar<decimal>(sqlAvg, new { IDSanPham = ct.IDSanPham, NgayChungTu = model.NgayChungTu }, transaction: tr);
                            }
                            decimal thanhTienVonPx = ct.SoLuong * donGiaVonPx;

                            pPxCt.Add("@DonGiaVon", (trangThai == 2) ? (decimal?)donGiaVonPx : null);
                            pPxCt.Add("@ThanhTienVon", (trangThai == 2) ? (decimal?)thanhTienVonPx : null);
                            pPxCt.Add("@ThueGTGT", ct.ThueGTGT);
                            pPxCt.Add("@TienThue", ct.TienThue);
                            pPxCt.Add("@TongSauThue", ct.TongSauThue);
                            pPxCt.Add("@NewID", dbType: System.Data.DbType.Int32, direction: System.Data.ParameterDirection.Output);

                            conn.Execute("INSERT INTO KHO_PhieuXuat_ChiTiet (IDPhieuXuat, IDSanPham, STT, SoLuong, DonGia, ThanhTien, DonGiaVon, ThanhTienVon, ThueGTGT, TienThue, TongSauThue) VALUES (@IDPhieuXuat, @IDSanPham, @STT, @SoLuong, @DonGia, @ThanhTien, @DonGiaVon, @ThanhTienVon, @ThueGTGT, @TienThue, @TongSauThue); SELECT @NewID = SCOPE_IDENTITY();", pPxCt, transaction: tr);
                            int idChiTietKho = pPxCt.Get<int>("@NewID");

                            // Trừ kho khi ở trạng thái Đề nghị ghi (1) hoặc Đã ghi (2)
                            if (isTruKho)
                            {
                                var pGd = new DynamicParameters();
                                pGd.Add("@NgayChungTu", model.NgayChungTu);
                                pGd.Add("@SoChungTu", soPx);
                                pGd.Add("@LoaiChungTu", 2);
                                pGd.Add("@IDChiTietKho", idChiTietKho);
                                pGd.Add("@IDKho", model.IDKho);
                                pGd.Add("@IDSanPham", ct.IDSanPham);
                                pGd.Add("@SoLuongNhap", 0);
                                pGd.Add("@SoLuongXuat", ct.SoLuong);
                                pGd.Add("@DonGia", ct.DonGia);
                                pGd.Add("@ThanhTien", ct.ThanhTien);
                                pGd.Add("@DonGiaVon", donGiaVonPx);
                                pGd.Add("@ThanhTienVon", thanhTienVonPx);
                                pGd.Add("@DienGiai", "Xuất kho tự động từ CTBH " + model.SoChungTu);
                                pGd.Add("@NguoiTao", nguoiTao);

                                conn.Execute("INSERT INTO KHO_GiaoDichKho (NgayChungTu, SoChungTu, LoaiChungTu, IDChiTietKho, IDKho, IDSanPham, SoLuongNhap, SoLuongXuat, DonGia, ThanhTien, DonGiaVon, ThanhTienVon, DienGiai, NgayTao, NguoiTao) VALUES (@NgayChungTu, @SoChungTu, @LoaiChungTu, @IDChiTietKho, @IDKho, @IDSanPham, @SoLuongNhap, @SoLuongXuat, @DonGia, @ThanhTien, @DonGiaVon, @ThanhTienVon, @DienGiai, GETDATE(), @NguoiTao)", pGd, transaction: tr);
                            }
                        }

                        // 5. KT_NhatKyChung (Chỉ sinh bút toán kế toán khi Đã ghi = 2)
                        if (trangThai == 2)
                        {
                            string taiKhoanNo = "131";
                            if (model.IDTaiKhoanThanhToan.HasValue)
                            {
                                taiKhoanNo = conn.ExecuteScalar<string>("SELECT SoTaiKhoan FROM KT_TaiKhoanKeToan WHERE ID = @ID", new { ID = model.IDTaiKhoanThanhToan.Value }, transaction: tr) ?? "131";
                            }

                            if (model.TongTienHang > 0)
                            {
                                conn.Execute("INSERT INTO KT_NhatKyChung (NgayChungTu, SoChungTu, LoaiChungTu, IDChungTu, TaiKhoanNo, TaiKhoanCo, SoTien, DienGiai, NgayTao, NguoiTao, IsHuy) VALUES (@NgayChungTu, @SoChungTu, @LoaiChungTu, @IDChungTu, @TaiKhoanNo, @TaiKhoanCo, @SoTien, @DienGiai, GETDATE(), @NguoiTao, 0)",
                                    new { NgayChungTu = model.NgayChungTu, SoChungTu = model.SoChungTu, LoaiChungTu = "BAN", IDChungTu = newId, TaiKhoanNo = taiKhoanNo, TaiKhoanCo = "5111", SoTien = model.TongTienHang, DienGiai = "Doanh thu bán hàng hóa CT " + model.SoChungTu, NguoiTao = nguoiTao }, transaction: tr);
                            }

                            if (model.TongTienThue > 0)
                            {
                                conn.Execute("INSERT INTO KT_NhatKyChung (NgayChungTu, SoChungTu, LoaiChungTu, IDChungTu, TaiKhoanNo, TaiKhoanCo, SoTien, DienGiai, NgayTao, NguoiTao, IsHuy) VALUES (@NgayChungTu, @SoChungTu, @LoaiChungTu, @IDChungTu, @TaiKhoanNo, @TaiKhoanCo, @SoTien, @DienGiai, GETDATE(), @NguoiTao, 0)",
                                    new { NgayChungTu = model.NgayChungTu, SoChungTu = model.SoChungTu, LoaiChungTu = "BAN", IDChungTu = newId, TaiKhoanNo = taiKhoanNo, TaiKhoanCo = "33311", SoTien = model.TongTienThue, DienGiai = "Thuế GTGT đầu ra CT " + model.SoChungTu, NguoiTao = nguoiTao }, transaction: tr);
                            }

                            if (tongGiaVonThucTe > 0)
                            {
                                conn.Execute("INSERT INTO KT_NhatKyChung (NgayChungTu, SoChungTu, LoaiChungTu, IDChungTu, TaiKhoanNo, TaiKhoanCo, SoTien, DienGiai, NgayTao, NguoiTao, IsHuy) VALUES (@NgayChungTu, @SoChungTu, @LoaiChungTu, @IDChungTu, @TaiKhoanNo, @TaiKhoanCo, @SoTien, @DienGiai, GETDATE(), @NguoiTao, 0)",
                                    new { NgayChungTu = model.NgayChungTu, SoChungTu = model.SoChungTu, LoaiChungTu = "BAN", IDChungTu = newId, TaiKhoanNo = "632", TaiKhoanCo = "156", SoTien = tongGiaVonThucTe, DienGiai = "Giá vốn hàng bán CT " + model.SoChungTu, NguoiTao = nguoiTao }, transaction: tr);
                            }
                        }

                        tr.Commit();
                        return newId;
                    }
                    catch (Exception ex)
                    {
                        tr.Rollback();
                        throw;
                    }
                }
            }
        }

        public void Update(ChungTuBanHangViewModel model, int nguoiCapNhat, bool ghiSo = false, int trangThai = 1)
        {
            using (var conn = _db.CreateConnection())
            {
                conn.Open();
                using (var tr = conn.BeginTransaction())
                {
                    try
                    {
                        if (model.IDKho <= 0)
                        {
                            model.IDKho = 1;
                        }

                        if (ghiSo)
                        {
                            trangThai = 2;
                        }

                        bool isTruKho = (trangThai == 1 || trangThai == 2);

                        // 1. Kiểm tra tồn kho nếu là Đề nghị ghi (1) hoặc Đã ghi (2)
                        if (isTruKho)
                        {
                            string existingSoPx = conn.ExecuteScalar<string>("SELECT SoChungTu FROM KHO_PhieuXuat WHERE IDChungTuBanHang = @ID", new { ID = model.ID }, transaction: tr);
                            var itemsCheck = model.ChiTiets.Select(x => new CheckTonKhoRequestItem { IDSanPham = x.IDSanPham, SoLuongCanXuat = x.SoLuong }).ToList();
                            var pTonKho = new DynamicParameters();
                            pTonKho.Add("@IDKho", model.IDKho);
                            pTonKho.Add("@ListSanPham", Newtonsoft.Json.JsonConvert.SerializeObject(itemsCheck));
                            pTonKho.Add("@ExcludeSoChungTu", existingSoPx);
                            var checkTon = conn.Query<CheckTonKhoResponseViewModel>("sp_KHO_TonKho_CheckByKho", pTonKho, transaction: tr, commandType: System.Data.CommandType.StoredProcedure).ToList();
                            var missingItems = checkTon.Where(x => !x.IsDuTon).ToList();
                            if (missingItems.Any())
                            {
                                var msg = string.Join("; ", missingItems.Select(x => $"Sản phẩm [{x.MaSanPham}] - {x.TenSanPham} vượt tồn kho. Tồn hiện tại: {x.SoLuongTon:N0}, cần xuất: {x.SoLuongCanXuat:N0}."));
                                throw new Exception(msg);
                            }
                        }

                        // 2. Cập nhật BAN_ChungTuBanHang
                        conn.Execute(@"
                            UPDATE BAN_ChungTuBanHang 
                            SET NgayChungTu = @NgayChungTu,
                                IDKho = @IDKho,
                                IDTaiKhoanThanhToan = @IDTaiKhoanThanhToan,
                                PhiBocXep = @PhiBocXep,
                                TongTienKhac = @TongTienKhac,
                                TongTienChietKhau = @TongTienChietKhau,
                                TongChuongTrinhTichLuySale = @TongChuongTrinhTichLuySale,
                                TongTienHang = @TongTienHang,
                                TongTienThue = @TongTienThue,
                                TongCong = @TongCong,
                                ConLai = @ConLai,
                                TrangThai = @TrangThai,
                                NguoiCapNhat = @NguoiCapNhat,
                                NgayCapNhat = GETDATE()
                            WHERE ID = @ID", 
                            new { 
                                NgayChungTu = model.NgayChungTu, 
                                IDKho = model.IDKho, 
                                IDTaiKhoanThanhToan = model.IDTaiKhoanThanhToan, 
                                PhiBocXep = model.PhiBocXep,
                                TongTienKhac = model.TongTienKhac,
                                TongTienChietKhau = model.TongTienChietKhau ?? 0m,
                                TongChuongTrinhTichLuySale = model.TongChuongTrinhTichLuySale ?? 0m,
                                TongTienHang = model.TongTienHang,
                                TongTienThue = model.TongTienThue,
                                TongCong = model.TongCong,
                                ConLai = model.ConLai,
                                TrangThai = trangThai, 
                                NguoiCapNhat = nguoiCapNhat, 
                                ID = model.ID 
                            }, transaction: tr);

                        // 3. Cập nhật hoặc tạo mới KHO_PhieuXuat
                        int idPhieuXuat = conn.ExecuteScalar<int>("SELECT ISNULL(ID, 0) FROM KHO_PhieuXuat WHERE IDChungTuBanHang = @ID", new { ID = model.ID }, transaction: tr);
                        
                        if (idPhieuXuat == 0)
                        {
                            var lastSo = conn.ExecuteScalar<string>("SELECT TOP 1 SoChungTu FROM KHO_PhieuXuat ORDER BY ID DESC", transaction: tr);
                            string newSoPx = "PX00001";
                            if (!string.IsNullOrEmpty(lastSo))
                            {
                                var numStr = lastSo.Replace("PX", "");
                                if (int.TryParse(numStr, out int num))
                                    newSoPx = "PX" + (num + 1).ToString("D5");
                            }

                            string tenKhachHang = conn.ExecuteScalar<string>("SELECT TenKhachHang FROM NS_KhachHang WHERE ID = @ID", new { ID = model.IDKhachHang }, transaction: tr) ?? "";

                            var pPx = new DynamicParameters();
                            pPx.Add("@SoChungTu", newSoPx);
                            pPx.Add("@NgayXuat", model.NgayChungTu);
                            pPx.Add("@IDKho", model.IDKho);
                            pPx.Add("@IDNhanSuNhan", null, System.Data.DbType.Int32);
                            pPx.Add("@TenNguoiNhan", tenKhachHang);
                            pPx.Add("@IDChungTuBanHang", model.ID);
                            pPx.Add("@IDDonDatHang", model.IDDonDatHang);
                            pPx.Add("@GhiChu", "Xuất kho tự động từ CTBH " + model.SoChungTu);
                            pPx.Add("@TongTienHang", model.TongTienHang);
                            pPx.Add("@TongTienThue", model.TongTienThue);
                            pPx.Add("@TongCong", model.TongCong);
                            pPx.Add("@NguoiTao", nguoiCapNhat);
                            pPx.Add("@TrangThai", trangThai);
                            pPx.Add("@NewID", dbType: System.Data.DbType.Int32, direction: System.Data.ParameterDirection.Output);

                            conn.Execute("INSERT INTO KHO_PhieuXuat (SoChungTu, NgayXuat, IDChungTuBanHang, IDDonDatHang, IDKho, IDNhanSuNhan, TenNguoiNhan, GhiChu, TongTienHang, TongTienThue, TongCong, NguoiTao, NgayTao, TrangThai) VALUES (@SoChungTu, @NgayXuat, @IDChungTuBanHang, @IDDonDatHang, @IDKho, @IDNhanSuNhan, @TenNguoiNhan, @GhiChu, @TongTienHang, @TongTienThue, @TongCong, @NguoiTao, GETDATE(), @TrangThai); SELECT @NewID = SCOPE_IDENTITY();", pPx, transaction: tr);
                            idPhieuXuat = pPx.Get<int>("@NewID");
                        }
                        else
                        {
                            conn.Execute(@"
                                UPDATE KHO_PhieuXuat 
                                SET NgayXuat = @NgayChungTu,
                                    IDKho = @IDKho,
                                    TrangThai = @TrangThai
                                WHERE IDChungTuBanHang = @IDChungTu",
                                new {
                                    NgayChungTu = model.NgayChungTu,
                                    IDKho = model.IDKho,
                                    TrangThai = trangThai,
                                    IDChungTu = model.ID
                                }, transaction: tr);
                        }

                        string soPx = conn.ExecuteScalar<string>("SELECT SoChungTu FROM KHO_PhieuXuat WHERE ID = @ID", new { ID = idPhieuXuat }, transaction: tr);

                        // Tái tạo chi tiết chứng từ bán hàng và chi tiết phiếu xuất để đảm bảo khớp dữ liệu sản phẩm
                        conn.Execute("DELETE FROM BAN_ChungTuBanHang_ChiTiet WHERE IDChungTuBanHang = @ID", new { ID = model.ID }, transaction: tr);
                        conn.Execute("DELETE FROM KHO_PhieuXuat_ChiTiet WHERE IDPhieuXuat = @IDPhieuXuat", new { IDPhieuXuat = idPhieuXuat }, transaction: tr);

                        // Xóa giao dịch kho cũ để tái tạo
                        conn.Execute("DELETE FROM KHO_GiaoDichKho WHERE LoaiChungTu = 2 AND SoChungTu = @SoPx", new { SoPx = soPx }, transaction: tr);
                        decimal tongGiaVonThucTe = 0;

                        int sttDetail = 1;
                        foreach (var ct in model.ChiTiets)
                        {
                            decimal donGiaVon = 0;
                            if (trangThai == 2)
                            {
                                string sqlAvg = @"
                                    DECLARE @TongSoLuong DECIMAL(18,2) = 0;
                                    DECLARE @TongGiaTri DECIMAL(18,2) = 0;
                                    SELECT 
                                        @TongSoLuong = ISNULL(SUM(SoLuongNhap),0) - ISNULL(SUM(SoLuongXuat),0),
                                        @TongGiaTri = ISNULL(SUM(SoLuongNhap * DonGia),0) - ISNULL(SUM(SoLuongXuat * ISNULL(DonGiaVon, 0)),0)
                                    FROM KHO_GiaoDichKho
                                    WHERE IDSanPham = @IDSanPham AND NgayChungTu <= @NgayChungTu
                                      AND IDKho IN (SELECT ID FROM DM_KhoHang WHERE ISNULL(IsKhoChinh, 0) = 1);
                                    
                                    IF @TongSoLuong > 0
                                        SELECT CAST(@TongGiaTri / @TongSoLuong AS DECIMAL(18,2));
                                    ELSE
                                        SELECT CAST(0 AS DECIMAL(18,2));
                                ";
                                donGiaVon = conn.ExecuteScalar<decimal>(sqlAvg, new { IDSanPham = ct.IDSanPham, NgayChungTu = model.NgayChungTu }, transaction: tr);
                            }
                            decimal thanhTienVon = ct.SoLuong * donGiaVon;
                            if (trangThai == 2) tongGiaVonThucTe += thanhTienVon;

                            // Chèn lại BAN_ChungTuBanHang_ChiTiet
                            var pCt = new DynamicParameters();
                            pCt.Add("@IDChungTuBanHang", model.ID);
                            pCt.Add("@IDSanPham", ct.IDSanPham);
                            pCt.Add("@STT", sttDetail);
                            pCt.Add("@SoLuong", ct.SoLuong);
                            pCt.Add("@DonGia", ct.DonGia);
                            pCt.Add("@DonGiaBocXep", ct.DonGiaBocXep);
                            pCt.Add("@ThanhTienBocXep", ct.ThanhTienBocXep);
                            pCt.Add("@SoTienKhac", ct.SoTienKhac);
                            pCt.Add("@SoTienChietKhau", ct.SoTienChietKhau);
                            pCt.Add("@ChuongTrinhTichLuySale", ct.ChuongTrinhTichLuySale);
                            pCt.Add("@DonGiaVon", (trangThai == 2) ? (decimal?)donGiaVon : null);
                            pCt.Add("@ThanhTienVon", (trangThai == 2) ? (decimal?)thanhTienVon : null);
                            pCt.Add("@ThanhTienHang", ct.ThanhTienHang);
                            pCt.Add("@ThanhTien", ct.ThanhTien);
                            pCt.Add("@ThueGTGT", ct.ThueGTGT);
                            pCt.Add("@TienThue", ct.TienThue);
                            pCt.Add("@TongSauThue", ct.TongSauThue);
                            pCt.Add("@GhiChu", ct.GhiChu);

                            string sqlInsertCt = @"
                                INSERT INTO BAN_ChungTuBanHang_ChiTiet 
                                (IDChungTuBanHang, IDSanPham, STT, SoLuong, DonGia, DonGiaBocXep, ThanhTienBocXep, SoTienKhac, SoTienChietKhau, ChuongTrinhTichLuySale, DonGiaVon, ThanhTienVon, ThanhTienHang, ThanhTien, ThueGTGT, TienThue, TongSauThue, GhiChu)
                                VALUES 
                                (@IDChungTuBanHang, @IDSanPham, @STT, @SoLuong, @DonGia, @DonGiaBocXep, @ThanhTienBocXep, @SoTienKhac, @SoTienChietKhau, @ChuongTrinhTichLuySale, @DonGiaVon, @ThanhTienVon, @ThanhTienHang, @ThanhTien, @ThueGTGT, @TienThue, @TongSauThue, @GhiChu);
                            ";
                            conn.Execute(sqlInsertCt, pCt, transaction: tr);

                            // Chèn lại KHO_PhieuXuat_ChiTiet
                            var pPxCt = new DynamicParameters();
                            pPxCt.Add("@IDPhieuXuat", idPhieuXuat);
                            pPxCt.Add("@IDSanPham", ct.IDSanPham);
                            pPxCt.Add("@STT", sttDetail++);
                            pPxCt.Add("@SoLuong", ct.SoLuong);
                            pPxCt.Add("@DonGia", ct.DonGia);
                            pPxCt.Add("@ThanhTien", ct.ThanhTien);
                            pPxCt.Add("@DonGiaVon", (trangThai == 2) ? (decimal?)donGiaVon : null);
                            pPxCt.Add("@ThanhTienVon", (trangThai == 2) ? (decimal?)thanhTienVon : null);
                            pPxCt.Add("@ThueGTGT", ct.ThueGTGT);
                            pPxCt.Add("@TienThue", ct.TienThue);
                            pPxCt.Add("@TongSauThue", ct.TongSauThue);
                            pPxCt.Add("@NewID", dbType: System.Data.DbType.Int32, direction: System.Data.ParameterDirection.Output);

                            conn.Execute("INSERT INTO KHO_PhieuXuat_ChiTiet (IDPhieuXuat, IDSanPham, STT, SoLuong, DonGia, ThanhTien, DonGiaVon, ThanhTienVon, ThueGTGT, TienThue, TongSauThue) VALUES (@IDPhieuXuat, @IDSanPham, @STT, @SoLuong, @DonGia, @ThanhTien, @DonGiaVon, @ThanhTienVon, @ThueGTGT, @TienThue, @TongSauThue); SELECT @NewID = SCOPE_IDENTITY();", pPxCt, transaction: tr);
                            int idChiTietKho = pPxCt.Get<int>("@NewID");

                            if (isTruKho)
                            {
                                var pGd = new DynamicParameters();
                                pGd.Add("@NgayChungTu", model.NgayChungTu);
                                pGd.Add("@SoChungTu", soPx);
                                pGd.Add("@LoaiChungTu", 2);
                                pGd.Add("@IDChiTietKho", idChiTietKho);
                                pGd.Add("@IDKho", model.IDKho);
                                pGd.Add("@IDSanPham", ct.IDSanPham);
                                pGd.Add("@SoLuongNhap", 0);
                                pGd.Add("@SoLuongXuat", ct.SoLuong);
                                pGd.Add("@DonGia", ct.DonGia);
                                pGd.Add("@ThanhTien", ct.ThanhTien);
                                pGd.Add("@DonGiaVon", (trangThai == 2) ? donGiaVon : 0);
                                pGd.Add("@ThanhTienVon", (trangThai == 2) ? thanhTienVon : 0);
                                pGd.Add("@DienGiai", "Xuất kho tự động từ CTBH " + model.SoChungTu);
                                pGd.Add("@NguoiTao", nguoiCapNhat);

                                conn.Execute("INSERT INTO KHO_GiaoDichKho (NgayChungTu, SoChungTu, LoaiChungTu, IDChiTietKho, IDKho, IDSanPham, SoLuongNhap, SoLuongXuat, DonGia, ThanhTien, DonGiaVon, ThanhTienVon, DienGiai, NgayTao, NguoiTao) VALUES (@NgayChungTu, @SoChungTu, @LoaiChungTu, @IDChiTietKho, @IDKho, @IDSanPham, @SoLuongNhap, @SoLuongXuat, @DonGia, @ThanhTien, @DonGiaVon, @ThanhTienVon, @DienGiai, GETDATE(), @NguoiTao)", pGd, transaction: tr);
                            }
                        }

                        // 5. KT_NhatKyChung (Xóa cũ nếu có)
                        conn.Execute("DELETE FROM KT_NhatKyChung WHERE LoaiChungTu = 'BAN' AND IDChungTu = @ID", new { ID = model.ID }, transaction: tr);

                        if (trangThai == 2)
                        {
                            string taiKhoanNo = "131";
                            if (model.IDTaiKhoanThanhToan.HasValue)
                            {
                                taiKhoanNo = conn.ExecuteScalar<string>("SELECT SoTaiKhoan FROM KT_TaiKhoanKeToan WHERE ID = @ID", new { ID = model.IDTaiKhoanThanhToan.Value }, transaction: tr) ?? "131";
                            }

                            if (model.TongTienHang > 0)
                            {
                                conn.Execute("INSERT INTO KT_NhatKyChung (NgayChungTu, SoChungTu, LoaiChungTu, IDChungTu, TaiKhoanNo, TaiKhoanCo, SoTien, DienGiai, NgayTao, NguoiTao, IsHuy) VALUES (@NgayChungTu, @SoChungTu, @LoaiChungTu, @IDChungTu, @TaiKhoanNo, @TaiKhoanCo, @SoTien, @DienGiai, GETDATE(), @NguoiTao, 0)",
                                    new { NgayChungTu = model.NgayChungTu, SoChungTu = model.SoChungTu, LoaiChungTu = "BAN", IDChungTu = model.ID, TaiKhoanNo = taiKhoanNo, TaiKhoanCo = "5111", SoTien = model.TongTienHang, DienGiai = "Doanh thu bán hàng hóa CT " + model.SoChungTu, NguoiTao = nguoiCapNhat }, transaction: tr);
                            }

                            if (model.TongTienThue > 0)
                            {
                                conn.Execute("INSERT INTO KT_NhatKyChung (NgayChungTu, SoChungTu, LoaiChungTu, IDChungTu, TaiKhoanNo, TaiKhoanCo, SoTien, DienGiai, NgayTao, NguoiTao, IsHuy) VALUES (@NgayChungTu, @SoChungTu, @LoaiChungTu, @IDChungTu, @TaiKhoanNo, @TaiKhoanCo, @SoTien, @DienGiai, GETDATE(), @NguoiTao, 0)",
                                    new { NgayChungTu = model.NgayChungTu, SoChungTu = model.SoChungTu, LoaiChungTu = "BAN", IDChungTu = model.ID, TaiKhoanNo = taiKhoanNo, TaiKhoanCo = "33311", SoTien = model.TongTienThue, DienGiai = "Thuế GTGT đầu ra CT " + model.SoChungTu, NguoiTao = nguoiCapNhat }, transaction: tr);
                            }

                            if (tongGiaVonThucTe > 0)
                            {
                                conn.Execute("INSERT INTO KT_NhatKyChung (NgayChungTu, SoChungTu, LoaiChungTu, IDChungTu, TaiKhoanNo, TaiKhoanCo, SoTien, DienGiai, NgayTao, NguoiTao, IsHuy) VALUES (@NgayChungTu, @SoChungTu, @LoaiChungTu, @IDChungTu, @TaiKhoanNo, @TaiKhoanCo, @SoTien, @DienGiai, GETDATE(), @NguoiTao, 0)",
                                    new { NgayChungTu = model.NgayChungTu, SoChungTu = model.SoChungTu, LoaiChungTu = "BAN", IDChungTu = model.ID, TaiKhoanNo = "632", TaiKhoanCo = "156", SoTien = tongGiaVonThucTe, DienGiai = "Giá vốn hàng bán CT " + model.SoChungTu, NguoiTao = nguoiCapNhat }, transaction: tr);
                            }

                            if (model.IDDonDatHang.HasValue && model.IDDonDatHang.Value > 0)
                            {
                                conn.Execute("UPDATE NS_DonDatHang SET TrangThaiDon = 3, NguoiCapNhat = @NguoiGhi, NgayCapNhat = GETDATE() WHERE ID = @ID", new { NguoiGhi = nguoiCapNhat, ID = model.IDDonDatHang.Value }, transaction: tr);
                            }
                        }
                        else
                        {
                            if (model.IDDonDatHang.HasValue && model.IDDonDatHang.Value > 0)
                            {
                                conn.Execute("UPDATE NS_DonDatHang SET TrangThaiDon = 2, NguoiCapNhat = @NguoiGhi, NgayCapNhat = GETDATE() WHERE ID = @ID", new { NguoiGhi = nguoiCapNhat, ID = model.IDDonDatHang.Value }, transaction: tr);
                            }
                        }

                        tr.Commit();
                    }
                    catch (Exception ex)
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

        public void GhiSo(int id, int nguoiGhi)
        {
            using (var conn = _db.CreateConnection())
            {
                conn.Open();
                using (var tr = conn.BeginTransaction())
                {
                    try
                    {
                        var p = new DynamicParameters();
                        p.Add("@ID", id);
                        var master = conn.QueryFirstOrDefault<BAN_ChungTuBanHang>("sp_BAN_ChungTuBanHang_GetById", p, transaction: tr, commandType: System.Data.CommandType.StoredProcedure);
                        if (master == null) throw new Exception("Chứng từ không tồn tại.");
                        if (master.TrangThai != 1 && master.TrangThai != 4) throw new Exception("Chứng từ đã ghi hoặc đã hủy.");

                        var details = conn.Query<ChungTuBanHangChiTietViewModel>("sp_BAN_ChungTuBanHang_ChiTiet_GetList", new { IDChungTuBanHang = id }, transaction: tr, commandType: System.Data.CommandType.StoredProcedure).ToList();

                        // 1. Kiểm tra tồn kho
                        var itemsCheck = details.Select(x => new CheckTonKhoRequestItem { IDSanPham = x.IDSanPham, SoLuongCanXuat = x.SoLuong }).ToList();
                        var pTonKho = new DynamicParameters();
                        pTonKho.Add("@IDKho", master.IDKho);
                        pTonKho.Add("@ListSanPham", Newtonsoft.Json.JsonConvert.SerializeObject(itemsCheck));
                        var checkTon = conn.Query<CheckTonKhoResponseViewModel>("sp_KHO_TonKho_CheckByKho", pTonKho, transaction: tr, commandType: System.Data.CommandType.StoredProcedure).ToList();
                        var missingItems = checkTon.Where(x => !x.IsDuTon).ToList();
                        if (missingItems.Any())
                        {
                            var msg = string.Join("; ", missingItems.Select(x => $"Sản phẩm [{x.MaSanPham}] - {x.TenSanPham} vượt tồn kho. Tồn hiện tại: {x.SoLuongTon:N0}, cần xuất: {x.SoLuongCanXuat:N0}."));
                            throw new Exception(msg);
                        }

                        // 2. Update Status BAN_ChungTuBanHang = 2
                        conn.Execute("UPDATE BAN_ChungTuBanHang SET TrangThai = 2, NguoiCapNhat = @NguoiGhi, NgayCapNhat = GETDATE() WHERE ID = @ID", new { NguoiGhi = nguoiGhi, ID = id }, transaction: tr);

                        // 3. Update Status KHO_PhieuXuat = 2
                        int? idPhieuXuat = conn.ExecuteScalar<int?>("SELECT ID FROM KHO_PhieuXuat WHERE IDChungTuBanHang = @ID", new { ID = id }, transaction: tr);
                        decimal tongGiaVon = 0;
                        if (idPhieuXuat.HasValue)
                        {
                            conn.Execute("UPDATE KHO_PhieuXuat SET TrangThai = 2, NguoiCapNhat = @NguoiGhi, NgayCapNhat = GETDATE() WHERE ID = @IDPhieuXuat", new { NguoiGhi = nguoiGhi, IDPhieuXuat = idPhieuXuat.Value }, transaction: tr);

                            // 4. Sinh / Cập nhật KHO_GiaoDichKho
                            string soPx = conn.ExecuteScalar<string>("SELECT SoChungTu FROM KHO_PhieuXuat WHERE ID = @ID", new { ID = idPhieuXuat.Value }, transaction: tr);
                            var pxChiTiets = conn.Query("SELECT ID, IDSanPham FROM KHO_PhieuXuat_ChiTiet WHERE IDPhieuXuat = @IDPhieuXuat", new { IDPhieuXuat = idPhieuXuat.Value }, transaction: tr);

                            // Xóa giao dịch cũ nếu có để sinh lại chuẩn xác
                            conn.Execute("DELETE FROM KHO_GiaoDichKho WHERE LoaiChungTu = 2 AND SoChungTu = @SoPx", new { SoPx = soPx }, transaction: tr);
                            
                            foreach (var ct in details)
                            {
                                var pxCt = pxChiTiets.FirstOrDefault(x => x.IDSanPham == ct.IDSanPham);
                                int idChiTietKho = pxCt != null ? (int)pxCt.ID : 0;

                                string sqlAvg = @"
                                    DECLARE @TongSoLuong DECIMAL(18,2) = 0;
                                    DECLARE @TongGiaTri DECIMAL(18,2) = 0;
                                    SELECT 
                                        @TongSoLuong = ISNULL(SUM(SoLuongNhap),0) - ISNULL(SUM(SoLuongXuat),0),
                                        @TongGiaTri = ISNULL(SUM(SoLuongNhap * DonGia),0) - ISNULL(SUM(SoLuongXuat * ISNULL(DonGiaVon, 0)),0)
                                    FROM KHO_GiaoDichKho
                                    WHERE IDSanPham = @IDSanPham AND NgayChungTu <= @NgayChungTu
                                      AND IDKho IN (SELECT ID FROM DM_KhoHang WHERE ISNULL(IsKhoChinh, 0) = 1); -- Chỉ tính giá vốn từ kho chính
                                    
                                    IF @TongSoLuong > 0
                                        SELECT CAST(@TongGiaTri / @TongSoLuong AS DECIMAL(18,2));
                                    ELSE
                                        SELECT CAST(0 AS DECIMAL(18,2));
                                ";
                                decimal donGiaVon = conn.ExecuteScalar<decimal>(sqlAvg, new { IDSanPham = ct.IDSanPham, NgayChungTu = master.NgayChungTu }, transaction: tr);
                                decimal thanhTienVon = ct.SoLuong * donGiaVon;
                                tongGiaVon += thanhTienVon;

                                var pGd = new DynamicParameters();
                                pGd.Add("@NgayChungTu", master.NgayChungTu);
                                pGd.Add("@SoChungTu", soPx);
                                pGd.Add("@LoaiChungTu", 2);
                                pGd.Add("@IDChiTietKho", idChiTietKho);
                                pGd.Add("@IDKho", master.IDKho);
                                pGd.Add("@IDSanPham", ct.IDSanPham);
                                pGd.Add("@SoLuongNhap", 0);
                                pGd.Add("@SoLuongXuat", ct.SoLuong);
                                pGd.Add("@DonGia", ct.DonGia);
                                pGd.Add("@ThanhTien", ct.ThanhTien);
                                pGd.Add("@DonGiaVon", donGiaVon);
                                pGd.Add("@ThanhTienVon", thanhTienVon);
                                pGd.Add("@DienGiai", "Xuất kho tự động từ CTBH " + master.SoChungTu);
                                pGd.Add("@NguoiTao", nguoiGhi);

                                conn.Execute("INSERT INTO KHO_GiaoDichKho (NgayChungTu, SoChungTu, LoaiChungTu, IDChiTietKho, IDKho, IDSanPham, SoLuongNhap, SoLuongXuat, DonGia, ThanhTien, DonGiaVon, ThanhTienVon, DienGiai, NgayTao, NguoiTao) VALUES (@NgayChungTu, @SoChungTu, @LoaiChungTu, @IDChiTietKho, @IDKho, @IDSanPham, @SoLuongNhap, @SoLuongXuat, @DonGia, @ThanhTien, @DonGiaVon, @ThanhTienVon, @DienGiai, GETDATE(), @NguoiTao)", pGd, transaction: tr);
                                
                                conn.Execute("UPDATE BAN_ChungTuBanHang_ChiTiet SET DonGiaVon = @DonGiaVon, ThanhTienVon = @ThanhTienVon WHERE IDChungTuBanHang = @IDChungTu AND IDSanPham = @IDSanPham", 
                                    new { DonGiaVon = donGiaVon, ThanhTienVon = thanhTienVon, IDChungTu = id, IDSanPham = ct.IDSanPham }, transaction: tr);

                                if (idChiTietKho > 0)
                                {
                                    conn.Execute("UPDATE KHO_PhieuXuat_ChiTiet SET DonGiaVon = @DonGiaVon, ThanhTienVon = @ThanhTienVon WHERE ID = @IDChiTietKho",
                                        new { DonGiaVon = donGiaVon, ThanhTienVon = thanhTienVon, IDChiTietKho = idChiTietKho }, transaction: tr);
                                }
                            }
                        }

                        // 5. KT_NhatKyChung
                        conn.Execute("DELETE FROM KT_NhatKyChung WHERE LoaiChungTu = 'BAN' AND IDChungTu = @ID", new { ID = id }, transaction: tr);

                        string taiKhoanNo = "131";
                        if (master.IDTaiKhoanThanhToan.HasValue)
                        {
                            taiKhoanNo = conn.ExecuteScalar<string>("SELECT SoTaiKhoan FROM KT_TaiKhoanKeToan WHERE ID = @ID", new { ID = master.IDTaiKhoanThanhToan.Value }, transaction: tr) ?? "131";
                        }

                        if (master.TongTienHang > 0)
                        {
                            conn.Execute("INSERT INTO KT_NhatKyChung (NgayChungTu, SoChungTu, LoaiChungTu, IDChungTu, TaiKhoanNo, TaiKhoanCo, SoTien, DienGiai, NgayTao, NguoiTao, IsHuy) VALUES (@NgayChungTu, @SoChungTu, @LoaiChungTu, @IDChungTu, @TaiKhoanNo, @TaiKhoanCo, @SoTien, @DienGiai, GETDATE(), @NguoiTao, 0)",
                                new { NgayChungTu = master.NgayChungTu, SoChungTu = master.SoChungTu, LoaiChungTu = "BAN", IDChungTu = id, TaiKhoanNo = taiKhoanNo, TaiKhoanCo = "5111", SoTien = master.TongTienHang, DienGiai = "Doanh thu bán hàng hóa CT " + master.SoChungTu, NguoiTao = nguoiGhi }, transaction: tr);
                        }

                        if (master.TongTienThue > 0)
                        {
                            conn.Execute("INSERT INTO KT_NhatKyChung (NgayChungTu, SoChungTu, LoaiChungTu, IDChungTu, TaiKhoanNo, TaiKhoanCo, SoTien, DienGiai, NgayTao, NguoiTao, IsHuy) VALUES (@NgayChungTu, @SoChungTu, @LoaiChungTu, @IDChungTu, @TaiKhoanNo, @TaiKhoanCo, @SoTien, @DienGiai, GETDATE(), @NguoiTao, 0)",
                                new { NgayChungTu = master.NgayChungTu, SoChungTu = master.SoChungTu, LoaiChungTu = "BAN", IDChungTu = id, TaiKhoanNo = taiKhoanNo, TaiKhoanCo = "33311", SoTien = master.TongTienThue, DienGiai = "Thuế GTGT đầu ra CT " + master.SoChungTu, NguoiTao = nguoiGhi }, transaction: tr);
                        }

                        if (idPhieuXuat.HasValue && tongGiaVon > 0)
                        {
                            conn.Execute("INSERT INTO KT_NhatKyChung (NgayChungTu, SoChungTu, LoaiChungTu, IDChungTu, TaiKhoanNo, TaiKhoanCo, SoTien, DienGiai, NgayTao, NguoiTao, IsHuy) VALUES (@NgayChungTu, @SoChungTu, @LoaiChungTu, @IDChungTu, @TaiKhoanNo, @TaiKhoanCo, @SoTien, @DienGiai, GETDATE(), @NguoiTao, 0)",
                                new { NgayChungTu = master.NgayChungTu, SoChungTu = master.SoChungTu, LoaiChungTu = "BAN", IDChungTu = id, TaiKhoanNo = "632", TaiKhoanCo = "156", SoTien = tongGiaVon, DienGiai = "Giá vốn hàng bán CT " + master.SoChungTu, NguoiTao = nguoiGhi }, transaction: tr);
                        }

                        // Cập nhật trạng thái đơn đặt hàng thành 3 (Đã lập chứng từ)
                        if (master.IDDonDatHang.HasValue && master.IDDonDatHang.Value > 0)
                        {
                            conn.Execute("UPDATE NS_DonDatHang SET TrangThaiDon = 3, NguoiCapNhat = @NguoiGhi, NgayCapNhat = GETDATE() WHERE ID = @ID", new { NguoiGhi = nguoiGhi, ID = master.IDDonDatHang.Value }, transaction: tr);
                        }

                        tr.Commit();
                    }
                    catch (Exception ex)
                    {
                        tr.Rollback();
                        throw;
                    }
                }
            }
        }

        public void Cancel(int id, int? idDonDatHang, int nguoiHuy, string lyDo)
        {
            using (var conn = _db.CreateConnection())
            {
                conn.Open();
                using (var tr = conn.BeginTransaction())
                {
                    try
                    {
                        if (id > 0)
                        {
                            var p = new DynamicParameters();
                            p.Add("@ID", id);
                            p.Add("@NguoiHuy", nguoiHuy);
                            p.Add("@LyDoHuy", lyDo);

                            conn.Execute("UPDATE BAN_ChungTuBanHang SET TrangThai = 3, NguoiCapNhat = @NguoiHuy, NgayCapNhat = GETDATE() WHERE ID = @ID", new { NguoiHuy = nguoiHuy, ID = id }, transaction: tr);

                            int? idPhieuXuat = conn.ExecuteScalar<int?>("SELECT ID FROM KHO_PhieuXuat WHERE IDChungTuBanHang = @ID", new { ID = id }, transaction: tr);
                            if (idPhieuXuat.HasValue)
                            {
                                conn.Execute("UPDATE KHO_PhieuXuat SET TrangThai = 3, NguoiCapNhat = @NguoiHuy, NgayCapNhat = GETDATE() WHERE ID = @IDPhieuXuat", new { NguoiHuy = nguoiHuy, IDPhieuXuat = idPhieuXuat.Value }, transaction: tr);
                                conn.Execute("DELETE FROM KHO_GiaoDichKho WHERE LoaiChungTu = 2 AND SoChungTu = (SELECT SoChungTu FROM KHO_PhieuXuat WHERE ID = @IDPhieuXuat)", new { IDPhieuXuat = idPhieuXuat.Value }, transaction: tr);
                            }

                            conn.Execute("UPDATE KT_NhatKyChung SET IsHuy = 1 WHERE LoaiChungTu = 'BAN' AND IDChungTu = @ID", new { ID = id }, transaction: tr);
                            
                            int? idDonDatHangDb = conn.ExecuteScalar<int?>("SELECT IDDonDatHang FROM BAN_ChungTuBanHang WHERE ID = @ID", new { ID = id }, transaction: tr);
                            if (idDonDatHangDb.HasValue && idDonDatHangDb.Value > 0)
                            {
                                conn.Execute("UPDATE NS_DonDatHang SET TrangThaiDon = 4, NguoiCapNhat = @NguoiHuy, NgayCapNhat = GETDATE() WHERE ID = @IDDonDatHang", new { NguoiHuy = nguoiHuy, IDDonDatHang = idDonDatHangDb.Value }, transaction: tr);
                            }
                        }
                        else if (idDonDatHang.HasValue && idDonDatHang.Value > 0)
                        {
                            // Hủy trực tiếp Đơn đặt hàng khi chưa có chứng từ
                            conn.Execute("UPDATE NS_DonDatHang SET TrangThaiDon = 4, NguoiCapNhat = @NguoiHuy, NgayCapNhat = GETDATE() WHERE ID = @ID", new { NguoiHuy = nguoiHuy, ID = idDonDatHang.Value }, transaction: tr);
                        }

                        tr.Commit();
                    }
                    catch (Exception ex)
                    {
                        tr.Rollback();
                        throw;
                    }
                }
            }
        }

        public void BoGhi(int id, int nguoiBoGhi)
        {
            using (var conn = _db.CreateConnection())
            {
                conn.Open();
                using (var tr = conn.BeginTransaction())
                {
                    try
                    {
                        var p = new DynamicParameters();
                        p.Add("@ID", id);
                        var master = conn.QueryFirstOrDefault<BAN_ChungTuBanHang>("sp_BAN_ChungTuBanHang_GetById", p, transaction: tr, commandType: System.Data.CommandType.StoredProcedure);
                        if (master == null) throw new Exception("Chứng từ không tồn tại.");
                        if (master.TrangThai != 1 && master.TrangThai != 2 && master.TrangThai != 3) throw new Exception("Chứng từ không hợp lệ để bỏ ghi.");

                        // 1. Cập nhật trạng thái chứng từ thành 4 (Lưu nháp) theo yêu cầu
                        conn.Execute("UPDATE BAN_ChungTuBanHang SET TrangThai = 4, NguoiCapNhat = @NguoiBoGhi, NgayCapNhat = GETDATE() WHERE ID = @ID", new { NguoiBoGhi = nguoiBoGhi, ID = id }, transaction: tr);

                        // 2. Cập nhật trạng thái phiếu xuất tương ứng thành 4 (Lưu nháp)
                        int? idPhieuXuat = conn.ExecuteScalar<int?>("SELECT ID FROM KHO_PhieuXuat WHERE IDChungTuBanHang = @ID", new { ID = id }, transaction: tr);
                        if (idPhieuXuat.HasValue)
                        {
                            conn.Execute("UPDATE KHO_PhieuXuat SET TrangThai = 4, NguoiCapNhat = @NguoiBoGhi, NgayCapNhat = GETDATE() WHERE ID = @IDPhieuXuat", new { NguoiBoGhi = nguoiBoGhi, IDPhieuXuat = idPhieuXuat.Value }, transaction: tr);

                            // 3. Xóa giao dịch kho (bỏ trừ kho khi về Lưu nháp)
                            conn.Execute(@"
                                DELETE FROM KHO_GiaoDichKho 
                                WHERE LoaiChungTu = 2 
                                  AND SoChungTu = (SELECT SoChungTu FROM KHO_PhieuXuat WHERE ID = @IDPhieuXuat)", 
                                new { IDPhieuXuat = idPhieuXuat.Value }, transaction: tr);
                        }

                        // 4. Xóa bút toán nhật ký chung (KT_NhatKyChung)
                        conn.Execute("DELETE FROM KT_NhatKyChung WHERE LoaiChungTu = 'BAN' AND IDChungTu = @ID", new { ID = id }, transaction: tr);

                        // 5. Cập nhật trạng thái đơn hàng gốc thành 2 (Đang lập chứng từ)
                        if (master.IDDonDatHang.HasValue && master.IDDonDatHang.Value > 0)
                        {
                            conn.Execute("UPDATE NS_DonDatHang SET TrangThaiDon = 2, NguoiCapNhat = @NguoiBoGhi, NgayCapNhat = GETDATE() WHERE ID = @ID", new { NguoiBoGhi = nguoiBoGhi, ID = master.IDDonDatHang.Value }, transaction: tr);
                        }

                        tr.Commit();
                    }
                    catch (Exception ex)
                    {
                        tr.Rollback();
                        throw;
                    }
                }
            }
        }

        public IEnumerable<CheckTonKhoResponseViewModel> CheckTonKhoByKho(int idKho, List<CheckTonKhoRequestItem> sanPhams)
        {
            using (var conn = _db.CreateConnection())
            {
                var p = new DynamicParameters();
                p.Add("@IDKho", idKho);
                p.Add("@ListSanPham", Newtonsoft.Json.JsonConvert.SerializeObject(sanPhams));

                return conn.Query<CheckTonKhoResponseViewModel>("sp_KHO_TonKho_CheckByKho", p, commandType: System.Data.CommandType.StoredProcedure);
            }
        }
        
        public IEnumerable<CheckTonKhoResponseViewModel> CheckTonKhoAllKho(List<CheckTonKhoRequestItem> sanPhams)
        {
            using (var conn = _db.CreateConnection())
            {
                var p = new DynamicParameters();
                p.Add("@ListSanPham", Newtonsoft.Json.JsonConvert.SerializeObject(sanPhams));

                return conn.Query<CheckTonKhoResponseViewModel>("sp_KHO_TonKho_CheckAllKho", p, commandType: System.Data.CommandType.StoredProcedure);
            }
        }
    }
}
