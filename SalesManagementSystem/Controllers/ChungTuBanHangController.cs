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
        private readonly SalesManagementSystem.Services.Interfaces.IExcelExportService _excelExportService;

        public ChungTuBanHangController(
            IChungTuBanHangRepository repo,
            IDonDatHangRepository donDatHangRepo,
            ITaiKhoanKeToanRepository taiKhoanRepo,
            INhatKyChungRepository nhatKyRepo,
            IDmKhoHangRepository khoHangRepo,
            SalesManagementSystem.Services.Interfaces.IExcelExportService excelExportService)
        {
            _repo = repo;
            _donDatHangRepo = donDatHangRepo;
            _taiKhoanRepo = taiKhoanRepo;
            _nhatKyRepo = nhatKyRepo;
            _khoHangRepo = khoHangRepo;
            _excelExportService = excelExportService;
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

            var model = new ChungTuBanHangViewModel();
            model.SoChungTu = _repo.GenerateSoChungTu();
            model.IDDonDatHang = idDonDatHang;
            model.SoDonHang = donHang.SoDonHang;
            model.IDKhachHang = donHang.IDKhachHang ?? 0;
            model.TenKhachHang = khachHang?.TenKhachHang ?? "";
            model.IDKho = 0;
            model.TenKhoHang = "";
            
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
            model.PhiBocXep = donHang.PhiBocXep;
            model.TongCong = model.ChiTiets.Sum(x => x.TongSauThue) + model.PhiBocXep;
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
                    model.PhiBocXep = donHang.PhiBocXep;
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

        public ActionResult Detail(int id)
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
                    model.PhiBocXep = donHang.PhiBocXep;
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
            
            return View(model);
        }

        [HttpPost]
        public ActionResult Save(ChungTuBanHangViewModel model, bool ghiSo = false)
        {
            if (!PermissionHelper.HasPermission("ChungTuBanHang", LoaiPhanQuyen.Them)) return Json(new { success = false, message = "Không có quyền thêm mới" });

            try
            {
                var user = GetCurrentUser();
                int userId = user?.IDNhanSu ?? 0;

                if (model.ID > 0)
                {
                    _repo.Update(model, userId, ghiSo && PermissionHelper.HasPermission("ChungTuBanHang", LoaiPhanQuyen.TuyChon));
                    return Json(new { success = true, id = model.ID });
                }
                else
                {
                    if (string.IsNullOrEmpty(model.SoChungTu))
                    {
                        model.SoChungTu = _repo.GenerateSoChungTu();
                    }

                    int newId = _repo.Insert(model, userId, ghiSo && PermissionHelper.HasPermission("ChungTuBanHang", LoaiPhanQuyen.TuyChon));

                    // Cập nhật trạng thái đơn đặt hàng: 2 (Đang lập chứng từ), 3 (Đã lập chứng từ)
                    int trangThaiDonHang = ghiSo ? 3 : 2;
                    if (model.IDDonDatHang.HasValue && model.IDDonDatHang.Value > 0)
                    {
                        _donDatHangRepo.UpdateStatus(model.IDDonDatHang.Value, trangThaiDonHang, userId);
                    }

                    return Json(new { success = true, id = newId });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
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

                _repo.GhiSo(id, userId);

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public ActionResult Huy(int id, int? idDonDatHang, string lyDo)
        {
            if (!PermissionHelper.HasPermission("ChungTuBanHang", LoaiPhanQuyen.TuyChon)) return Json(new { success = false, message = "Không có quyền hủy" });

            try
            {
                var user = GetCurrentUser();
                int userId = user?.IDNhanSu ?? 0;

                _repo.Cancel(id, idDonDatHang, userId, lyDo);

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

        public ActionResult ExportExcel(int id)
        {
            try
            {
                var model = _repo.GetById(id);
                if (model == null) return HttpNotFound("Không tìm thấy chứng từ");

                var session = (SalesManagementSystem.Models.ViewModels.UserLoginViewModel)Session[SalesManagementSystem.Helpers.CommonConstants.USER_SESSION];
                string nguoiLapBieu = session != null ? (session.HoDem + " " + session.Ten).Trim() : "";
                if (string.IsNullOrEmpty(nguoiLapBieu)) nguoiLapBieu = session?.UserName ?? "";

                // Get NhanSu and Kho info
                string tenNhanSu = "";
                string sdtNhanSu = "";
                string diaChiKho = "";
                
                using (var conn = (new Data.DbConnectionFactory()).CreateConnection())
                {
                    if (model.IDNhanVien.HasValue && model.IDNhanVien.Value > 0)
                    {
                        var ns = conn.QueryFirstOrDefault("SELECT ISNULL(HoDem, '') + ' ' + ISNULL(Ten, '') AS TenNhanSu, DienThoai FROM NS_NhanSu WHERE ID = @ID", new { ID = model.IDNhanVien.Value });
                        if (ns != null)
                        {
                            tenNhanSu = ((string)ns.TenNhanSu).Trim();
                            sdtNhanSu = (string)ns.DienThoai ?? "";
                        }
                    }
                    if (model.IDKho > 0)
                    {
                        diaChiKho = conn.QueryFirstOrDefault<string>("SELECT DiaChi FROM DM_KhoHang WHERE ID = @ID", new { ID = model.IDKho }) ?? "";
                    }
                }

                var variables = new System.Collections.Generic.Dictionary<string, object>
                {
                    { "SoChungTu", model.SoChungTu },
                    { "TenKhachHang", model.TenKhachHang },
                    { "DiaChi", model.DiaChi },
                    { "SoDienThoaiKhachHang", model.SoDienThoai },
                    { "TenNhanSu", tenNhanSu },
                    { "SoDienThoaiNhanSu", sdtNhanSu },
                    { "DiaChiKho", diaChiKho },
                    { "PhiBocXep", model.PhiBocXep.ToString("N0") },
                    { "TongCongBangChu", SalesManagementSystem.Helpers.NumberToTextHelper.DocTienBangChu(model.TongCong) },
                    { "ngaythangnam", $"Ngày {DateTime.Now.Day:D2} tháng {DateTime.Now.Month:D2} năm {DateTime.Now.Year}" },
                    { "NguoiLapBieu", nguoiLapBieu }
                };

                int stt = 1;
                var exportData = model.ChiTiets.Select(x => new {
                    STT = stt++,
                    MaSanPham = x.MaSanPham,
                    TenSanPham = x.TenSanPham,
                    DVT = x.DVT,
                    HanSuDung = "", // Blank for now unless there's specific data
                    SoLuong = x.SoLuong,
                    DonGia = x.DonGia,
                    TongSauThue = x.TongSauThue
                }).ToList();

                string fileExtension;
                var fileBytes = _excelExportService.Export("CTBH01", exportData, out fileExtension, variables);

                string contentType = fileExtension == "xls" 
                    ? "application/vnd.ms-excel" 
                    : "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

                return File(fileBytes, contentType, $"ChungTuBanHang_{model.SoChungTu}_{DateTime.Now:yyyyMMddHHmmss}.{fileExtension}");
            }
            catch (Exception ex)
            {
                TempData["ToastMessage"] = "Lỗi xuất Excel: " + ex.Message;
                TempData["ToastType"] = "error";
                return RedirectToAction("Detail", new { id = id });
            }
        }
    }
}
