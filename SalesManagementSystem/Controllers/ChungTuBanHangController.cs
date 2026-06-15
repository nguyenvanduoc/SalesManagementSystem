using SalesManagementSystem.Helpers;
using SalesManagementSystem.Helpers.Security;
using SalesManagementSystem.Models.Entities;
using SalesManagementSystem.Models.ViewModels;
using SalesManagementSystem.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Dapper;
using SalesManagementSystem.Data;

namespace SalesManagementSystem.Controllers
{
    public class ChungTuBanHangController : BaseController
    {
        private readonly IChungTuBanHangRepository _repo;
        private readonly IDonDatHangRepository _donDatHangRepo;
        private readonly ITaiKhoanKeToanRepository _taiKhoanRepo;
        private readonly INhatKyChungRepository _nhatKyRepo;
        private readonly IDmKhoHangRepository _khoHangRepo;

        public ChungTuBanHangController(
            IChungTuBanHangRepository repo,
            IDonDatHangRepository donDatHangRepo,
            ITaiKhoanKeToanRepository taiKhoanRepo,
            INhatKyChungRepository nhatKyRepo,
            IDmKhoHangRepository khoHangRepo)
        {
            _repo = repo;
            _donDatHangRepo = donDatHangRepo;
            _taiKhoanRepo = taiKhoanRepo;
            _nhatKyRepo = nhatKyRepo;
            _khoHangRepo = khoHangRepo;
        }

        public ActionResult Index(int page = 1, int pageSize = 20, string tuNgay = "", string denNgay = "", string soDonHang = "", int? idKhachHang = null, int? trangThai = null)
        {
            if (!PermissionHelper.HasPermission("ChungTuBanHang", LoaiPhanQuyen.Xem)) return View("AccessDenied");

            var list = _repo.GetDonHangList(tuNgay, denNgay, soDonHang, idKhachHang, trangThai).ToList();
            int totalRecords = list.Count;
            var pagedItems = list.Skip((page - 1) * pageSize).Take(pageSize);

            var model = new PagedListViewModel<DonHangChungTuViewModel>
            {
                Items = pagedItems,
                CurrentPage = page,
                PageSize = pageSize,
                TotalRecords = totalRecords,
                ActionName = "GetList",
                Keyword = soDonHang
            };

            int totalKhs;
            var khs = (new SalesManagementSystem.Repositories.KhachHangRepository(new Data.DbConnectionFactory())).GetPaged(1, 1000, "", out totalKhs).ToList();
            ViewBag.KhachHangs = new SelectList(khs, "ID", "TenKhachHang", idKhachHang);

            ViewBag.TuNgay = tuNgay;
            ViewBag.DenNgay = denNgay;
            ViewBag.SoDonHang = soDonHang;
            ViewBag.IDKhachHang = idKhachHang;
            ViewBag.TrangThai = trangThai;

            if (Request.IsAjaxRequest() || Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return PartialView("_ChungTuBanHangList", model);

            return View("Index", model);
        }

        public ActionResult GetList(int page = 1, int pageSize = 20, string tuNgay = "", string denNgay = "", string soDonHang = "", int? idKhachHang = null, int? trangThai = null)
        {
            if (!PermissionHelper.HasPermission("ChungTuBanHang", LoaiPhanQuyen.Xem)) return Content("<div class='alert alert-danger'>Không có quyền truy cập</div>");

            try
            {
                var list = _repo.GetDonHangList(tuNgay, denNgay, soDonHang, idKhachHang, trangThai).ToList();
                int totalRecords = list.Count;
                var pagedItems = list.Skip((page - 1) * pageSize).Take(pageSize);

                var model = new PagedListViewModel<DonHangChungTuViewModel>
                {
                    Items = pagedItems,
                    CurrentPage = page,
                    PageSize = pageSize,
                    TotalRecords = totalRecords,
                    ActionName = "GetList",
                    Keyword = soDonHang
                };

                return PartialView("_ChungTuBanHangList", model);
            }
            catch (Exception ex)
            {
                return Content("<div class='alert alert-danger'>Lỗi: " + ex.Message + "</div>");
            }
        }



        public ActionResult Create(int idDonDatHang)
        {
            if (!PermissionHelper.HasPermission("ChungTuBanHang", LoaiPhanQuyen.Them)) return View("AccessDenied");

            var donHang = _donDatHangRepo.GetById(idDonDatHang);
            if (donHang == null || donHang.TrangThaiDon == 4) return HttpNotFound("Không tìm thấy đơn hàng hợp lệ");

            var khachHang = donHang.IDKhachHang.HasValue ? (new SalesManagementSystem.Repositories.KhachHangRepository(new Data.DbConnectionFactory())).GetById(donHang.IDKhachHang.Value) : null;

            int totalKhos;
            var khos = _khoHangRepo.GetPaged(1, 1000, "", out totalKhos).ToList();
            var firstKho = khos.FirstOrDefault();

            var model = new ChungTuBanHangViewModel();
            model.SoChungTu = _repo.GenerateSoChungTu();
            model.IDDonDatHang = idDonDatHang;
            model.SoDonHang = donHang.SoDonHang;
            model.IDKhachHang = donHang.IDKhachHang ?? 0;
            model.TenKhachHang = khachHang?.TenKhachHang ?? "";
            model.IDKho = firstKho?.ID ?? 0;
            model.TenKhoHang = firstKho?.TenKhoHang ?? "";
            
            var chiTietsDon = _donDatHangRepo.GetChiTietByDonId(idDonDatHang);

            int stt = 1;
            foreach (var ct in chiTietsDon)
            {
                model.ChiTiets.Add(new ChungTuBanHangChiTietViewModel
                {
                    IDSanPham = ct.IDSanPham ?? 0,
                    MaSanPham = ct.MaSanPham,
                    TenSanPham = ct.TenSanPham,
                    DVT = ct.DVT,
                    STT = stt++,
                    SoLuong = ct.SoLuong,
                    DonGia = ct.DonGia,
                    ThanhTien = ct.ThanhTien,
                    ThueGTGT = ct.ThueGTGT,
                    TienThue = ct.ThanhTienThue,
                    TongSauThue = ct.ThanhTienSauThue
                });
            }

            model.TongTienHang = model.ChiTiets.Sum(x => x.ThanhTien);
            model.TongTienThue = model.ChiTiets.Sum(x => x.TienThue);
            model.TongCong = model.ChiTiets.Sum(x => x.TongSauThue);
            model.ConLai = model.TongCong;
            model.DaThanhToan = 0;

            // Extra fields for Don Dat Hang info
            model.NgayTaoDon = donHang.NgayTaoDon;
            model.ThoiHanGiaoHang = donHang.ThoiHanGiaoHang;
            model.IDNhanVien = donHang.IDNhanVien;
            model.TenNhanVien = "";
            model.TrangThaiDon = donHang.TrangThaiDon;

            model.MaKhachHang = khachHang?.MaKhachHang ?? "";
            model.MaSoThue = khachHang?.MaSoThue ?? "";
            model.DiaChi = khachHang?.DiaChi ?? "";
            model.SoDienThoai = khachHang?.SoDienThoai ?? "";

            using (var conn = (new Data.DbConnectionFactory()).CreateConnection())
            {
                var nvItems = conn.Query("SELECT ID, ISNULL(MaNhanSu, '') + ' - ' + LTRIM(RTRIM(ISNULL(HoDem, '') + ' ' + ISNULL(Ten, ''))) AS TenNhanVien FROM NS_NhanSu ORDER BY Ten")
                    .Select(x => new { ID = (int)x.ID, TenNhanVien = (string)x.TenNhanVien })
                    .ToList();
                ViewBag.NhanVienList = new SelectList(nvItems, "ID", "TenNhanVien", model.IDNhanVien);
            }
            ViewBag.TrangThaiList = new SelectList(_donDatHangRepo.GetTrangThaiList(), "ID", "TenTrangThai", model.TrangThaiDon);

            ViewBag.TaiKhoanThanhToanList = _taiKhoanRepo.GetActive().ToList();
            ViewBag.KhoList = new SelectList(khos, "ID", "TenKhoHang", model.IDKho);
            
            return View(model);
        }

        public ActionResult Edit(int id)
        {
            if (!PermissionHelper.HasPermission("ChungTuBanHang", LoaiPhanQuyen.Xem)) return View("AccessDenied");

            var model = _repo.GetById(id);
            if (model == null) return HttpNotFound("Không tìm thấy chứng từ");

            int totalKhos;
            var khos = _khoHangRepo.GetPaged(1, 1000, "", out totalKhos).ToList();

            if (model.IDDonDatHang.HasValue)
            {
                var donHang = _donDatHangRepo.GetById(model.IDDonDatHang.Value);
                if (donHang != null)
                {
                    model.NgayTaoDon = donHang.NgayTaoDon;
                    model.ThoiHanGiaoHang = donHang.ThoiHanGiaoHang;
                    model.IDNhanVien = donHang.IDNhanVien;
                    model.TrangThaiDon = donHang.TrangThaiDon;
                }
            }
            
            var khachHang = (new SalesManagementSystem.Repositories.KhachHangRepository(new Data.DbConnectionFactory())).GetById(model.IDKhachHang);
            if (khachHang != null)
            {
                model.MaKhachHang = khachHang.MaKhachHang ?? "";
                model.MaSoThue = khachHang.MaSoThue ?? "";
                model.DiaChi = khachHang.DiaChi ?? "";
                model.SoDienThoai = khachHang.SoDienThoai ?? "";
            }

            using (var conn = (new Data.DbConnectionFactory()).CreateConnection())
            {
                var nvItems = conn.Query("SELECT ID, ISNULL(MaNhanSu, '') + ' - ' + LTRIM(RTRIM(ISNULL(HoDem, '') + ' ' + ISNULL(Ten, ''))) AS TenNhanVien FROM NS_NhanSu ORDER BY Ten")
                    .Select(x => new { ID = (int)x.ID, TenNhanVien = (string)x.TenNhanVien })
                    .ToList();
                ViewBag.NhanVienList = new SelectList(nvItems, "ID", "TenNhanVien", model.IDNhanVien);
            }
            ViewBag.TrangThaiList = new SelectList(_donDatHangRepo.GetTrangThaiList(), "ID", "TenTrangThai", model.TrangThaiDon);

            ViewBag.TaiKhoanThanhToanList = _taiKhoanRepo.GetActive().ToList();
            ViewBag.KhoList = new SelectList(khos, "ID", "TenKhoHang", model.IDKho);
            
            // Re-use Create view but maybe pass a flag or just let Create handle both
            ViewBag.IsEdit = true;
            return View("Create", model);
        }

        [HttpPost]
        public ActionResult Save(ChungTuBanHangViewModel model, bool ghiSo = false)
        {
            if (!PermissionHelper.HasPermission("ChungTuBanHang", LoaiPhanQuyen.Them)) return Json(new { success = false, message = "Không có quyền thêm mới" });

            try
            {
                // Kiểm tra tồn kho backend
                var itemsCheck = model.ChiTiets.Select(x => new CheckTonKhoRequestItem { IDSanPham = x.IDSanPham, SoLuongCanXuat = x.SoLuong }).ToList();
                var checkTon = _repo.CheckTonKhoByKho(model.IDKho, itemsCheck).ToList();
                var missingItems = checkTon.Where(x => !x.IsDuTon).ToList();
                if (missingItems.Any())
                {
                    var msg = string.Join("; ", missingItems.Select(x => $"Sản phẩm [{x.MaSanPham}] - {x.TenSanPham} vượt số lượng tồn. Tồn hiện tại: {x.SoLuongTon:N0}, số lượng cần xuất: {x.SoLuongCanXuat:N0}."));
                    return Json(new { success = false, message = msg });
                }
                var user = GetCurrentUser();
                int userId = user?.IDNhanSu ?? 0;

                if (string.IsNullOrEmpty(model.SoChungTu))
                {
                    model.SoChungTu = _repo.GenerateSoChungTu();
                }

                // Bất kể có ghi sổ hay không, chứng từ khởi tạo trạng thái = 1 (Chờ ghi)
                model.TrangThai = 1;

                int newId = _repo.Insert(model, userId);

                // Cập nhật trạng thái đơn đặt hàng: 2 (Đang lập chứng từ), 3 (Đã lập chứng từ)
                int trangThaiDonHang = ghiSo ? 3 : 2;
                if (model.IDDonDatHang.HasValue && model.IDDonDatHang.Value > 0)
                {
                    _donDatHangRepo.UpdateStatus(model.IDDonDatHang.Value, trangThaiDonHang, userId);
                }

                if (ghiSo && PermissionHelper.HasPermission("ChungTuBanHang", LoaiPhanQuyen.TuyChon))
                {
                    ProcessGhiSo(newId, userId);
                }

                return Json(new { success = true, id = newId });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        private void CreateAutoPhieuXuatKho(ChungTuBanHangViewModel model, int userId)
        {
            var dbFactory = new DbConnectionFactory();
            using (var conn = dbFactory.CreateConnection())
            {
                conn.Open();
                using (var tr = conn.BeginTransaction())
                {
                    try
                    {
                        var lastSo = conn.ExecuteScalar<string>("SELECT TOP 1 SoChungTu FROM KHO_PhieuXuat ORDER BY ID DESC", transaction: tr);
                        string soPx = "PX00001";
                        if (!string.IsNullOrEmpty(lastSo))
                        {
                            var numStr = lastSo.Replace("PX", "");
                            if (int.TryParse(numStr, out int num))
                                soPx = "PX" + (num + 1).ToString("D5");
                        }

                        var p = new DynamicParameters();
                        p.Add("@SoChungTu", soPx);
                        p.Add("@NgayXuat", model.NgayChungTu);
                        p.Add("@IDKho", model.IDKho);
                        p.Add("@IDNhanSuNhan", null, System.Data.DbType.Int32);
                        p.Add("@TenNguoiNhan", model.TenKhachHang);
                        p.Add("@IDChungTuBanHang", model.ID);
                        p.Add("@IDDonDatHang", model.IDDonDatHang);
                        p.Add("@GhiChu", "Xuất kho tự động từ CTBH " + model.SoChungTu);
                        p.Add("@TongTienHang", model.TongTienHang);
                        p.Add("@TongTienThue", model.TongTienThue);
                        p.Add("@TongCong", model.TongCong);
                        p.Add("@NguoiTao", userId);
                        p.Add("@TrangThai", 1); // Đề nghị ghi
                        p.Add("@NewID", dbType: System.Data.DbType.Int32, direction: System.Data.ParameterDirection.Output);

                        conn.Execute("INSERT INTO KHO_PhieuXuat (SoChungTu, NgayXuat, IDChungTuBanHang, IDDonDatHang, IDKho, IDNhanSuNhan, TenNguoiNhan, GhiChu, TongTienHang, TongTienThue, TongCong, NguoiTao, NgayTao, TrangThai) VALUES (@SoChungTu, @NgayXuat, @IDChungTuBanHang, @IDDonDatHang, @IDKho, @IDNhanSuNhan, @TenNguoiNhan, @GhiChu, @TongTienHang, @TongTienThue, @TongCong, @NguoiTao, GETDATE(), @TrangThai); SELECT @NewID = SCOPE_IDENTITY();", p, transaction: tr);
                        int idPhieu = p.Get<int>("@NewID");

                        int stt = 1;
                        foreach (var ct in model.ChiTiets)
                        {
                            conn.Execute("INSERT INTO KHO_PhieuXuat_ChiTiet (IDPhieuXuat, IDSanPham, STT, SoLuong, DonGia, ThanhTien, ThueGTGT, TienThue, TongSauThue) VALUES (@IDPhieuXuat, @IDSanPham, @STT, @SoLuong, @DonGia, @ThanhTien, @ThueGTGT, @TienThue, @TongSauThue)",
                                new { IDPhieuXuat = idPhieu, IDSanPham = ct.IDSanPham, STT = stt++, SoLuong = ct.SoLuong, DonGia = ct.DonGia, ThanhTien = ct.ThanhTien, ThueGTGT = ct.ThueGTGT, TienThue = ct.TienThue, TongSauThue = ct.TongSauThue }, transaction: tr);
                        }

                        tr.Commit();
                    }
                    catch
                    {
                        tr.Rollback();
                        throw;
                    }
                }
            }
        }

        [HttpPost]
        public ActionResult GhiSo(int id)
        {
            if (!PermissionHelper.HasPermission("ChungTuBanHang", LoaiPhanQuyen.TuyChon)) return Json(new { success = false, message = "Không có quyền ghi sổ" });

            try
            {
                var user = GetCurrentUser();
                int userId = user?.IDNhanSu ?? 0;

                ProcessGhiSo(id, userId);

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        private void ProcessGhiSo(int id, int userId)
        {
            var model = _repo.GetById(id);
            if (model == null) throw new Exception("Chứng từ không tồn tại.");
            if (model.TrangThai != 1) throw new Exception("Chứng từ đã ghi hoặc đã hủy.");

            // Kiểm tra tồn kho backend trước khi ghi sổ
            var itemsCheck = model.ChiTiets.Select(x => new CheckTonKhoRequestItem { IDSanPham = x.IDSanPham, SoLuongCanXuat = x.SoLuong }).ToList();
            var checkTon = _repo.CheckTonKhoByKho(model.IDKho, itemsCheck).ToList();
            var missingItems = checkTon.Where(x => !x.IsDuTon).ToList();
            if (missingItems.Any())
            {
                var msg = string.Join("; ", missingItems.Select(x => $"Sản phẩm [{x.MaSanPham}] - {x.TenSanPham} vượt số lượng tồn. Tồn hiện tại: {x.SoLuongTon:N0}, số lượng cần xuất: {x.SoLuongCanXuat:N0}."));
                throw new Exception("Lỗi tồn kho: " + msg);
            }

            // Update Status to 2 (Đã ghi)
            _repo.UpdateStatus(id, 2, userId);

            // Auto-create PhieuXuatKho when status changes to Đã ghi
            CreateAutoPhieuXuatKho(model, userId);

            // Cập nhật trạng thái đơn đặt hàng thành 3 (Đã lập chứng từ)
            if (model.IDDonDatHang.HasValue && model.IDDonDatHang.Value > 0)
            {
                _donDatHangRepo.UpdateStatus(model.IDDonDatHang.Value, 3, userId);
            }

            // Ghi Nhat Ky Chung
            if (model.IDTaiKhoanThanhToan.HasValue)
            {
                var taiKhoanNo = _taiKhoanRepo.GetActive().FirstOrDefault(x => x.ID == model.IDTaiKhoanThanhToan.Value)?.SoTaiKhoan ?? "131";
                
                // Doanh thu
                _nhatKyRepo.Insert(new KT_NhatKyChung
                {
                    NgayChungTu = model.NgayChungTu,
                    SoChungTu = model.SoChungTu,
                    LoaiChungTu = "BAN",
                    IDChungTu = model.ID,
                    TaiKhoanNo = taiKhoanNo,
                    TaiKhoanCo = "5111", // Doanh thu
                    SoTien = model.TongTienHang,
                    DienGiai = "Doanh thu bán hàng hóa theo CT " + model.SoChungTu,
                    NguoiTao = userId
                });

                // VAT
                if (model.TongTienThue > 0)
                {
                    _nhatKyRepo.Insert(new KT_NhatKyChung
                    {
                        NgayChungTu = model.NgayChungTu,
                        SoChungTu = model.SoChungTu,
                        LoaiChungTu = "BAN",
                        IDChungTu = model.ID,
                        TaiKhoanNo = taiKhoanNo,
                        TaiKhoanCo = "33311", // Thuế GTGT
                        SoTien = model.TongTienThue,
                        DienGiai = "Thuế GTGT đầu ra theo CT " + model.SoChungTu,
                        NguoiTao = userId
                    });
                }
            }
        }

        [HttpPost]
        public ActionResult Huy(int id, string lyDo)
        {
            if (!PermissionHelper.HasPermission("ChungTuBanHang", LoaiPhanQuyen.TuyChon)) return Json(new { success = false, message = "Không có quyền hủy" });

            try
            {
                var user = GetCurrentUser();
                int userId = user?.IDNhanSu ?? 0;

                _repo.Cancel(id, userId, lyDo);
                _nhatKyRepo.Cancel("BAN", id, userId);

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public ActionResult CheckTonKho(int idKho, List<CheckTonKhoRequestItem> sanPhams)
        {
            try
            {
                if (sanPhams == null || !sanPhams.Any())
                    return Json(new { success = false, hasError = false, message = "Không có sản phẩm nào để kiểm tra" });

                var result = _repo.CheckTonKhoByKho(idKho, sanPhams).ToList();
                bool hasError = result.Any(x => !x.IsDuTon);

                return Json(new { success = true, data = result, hasError = hasError });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, hasError = true, message = ex.Message });
            }
        }
        [HttpPost]
        public ActionResult CheckTonKhoAllKho(List<CheckTonKhoRequestItem> sanPhams)
        {
            try
            {
                if (sanPhams == null || !sanPhams.Any())
                    return Json(new { success = false, message = "Không có sản phẩm nào để kiểm tra" });

                var result = _repo.CheckTonKhoAllKho(sanPhams).ToList();

                // Nhóm theo Kho
                var groupedByKho = result.GroupBy(x => new { x.IDKho, x.TenKhoHang })
                    .Select(g => new
                    {
                        IDKho = g.Key.IDKho,
                        TenKhoHang = g.Key.TenKhoHang,
                        IsDuTonAll = g.All(x => x.IsDuTon),
                        ChiTiets = g.Select(x => new
                        {
                            x.IDSanPham,
                            x.MaSanPham,
                            x.TenSanPham,
                            x.SoLuongCanXuat,
                            x.SoLuongTon,
                            x.ChenhLech,
                            x.IsDuTon
                        }).ToList()
                    }).ToList();

                return Json(new { success = true, data = groupedByKho });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}
