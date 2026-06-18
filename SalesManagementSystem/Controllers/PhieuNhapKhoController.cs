using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using SalesManagementSystem.Helpers;
using SalesManagementSystem.Models.ViewModels;
using SalesManagementSystem.Repositories.Interfaces;
using SalesManagementSystem.Services.Interfaces;

namespace SalesManagementSystem.Controllers
{
    public class PhieuNhapKhoController : BaseController
    {
        private readonly IPhieuNhapKhoRepository _repo;
        private readonly IExcelExportService _excelExportService;

        public PhieuNhapKhoController(IPhieuNhapKhoRepository repo, IExcelExportService excelExportService)
        {
            _repo = repo;
            _excelExportService = excelExportService;
        }

        private SelectList GetKhoList(int? selectedId = null)
        {
            var items = _repo.GetKhoForDropdown("").Select(x => new { ID = x.ID, Name = x.MaKhoHang + " - " + x.TenKhoHang }).ToList();
            return new SelectList(items, "ID", "Name", selectedId);
        }

        private SelectList GetNhaCungCapList(int? selectedId = null)
        {
            var items = _repo.GetNhaCungCapForDropdown("").Select(x => new { ID = x.ID, Name = x.MaNhaCungCap + " - " + x.TenNhaCungCap }).ToList();
            return new SelectList(items, "ID", "Name", selectedId);
        }

        public ActionResult Index(
            int page = 1, int pageSize = 20,
            string tuNgay = "", string denNgay = "",
            string soChungTu = "", int? idKho = null,
            int? idNhaCungCap = null, int? trangThai = null,
            string tenNguoiNhan = "")
        {
            if (!PermissionHelper.HasPermission("PhieuNhapKho", LoaiPhanQuyen.Xem)) return View("AccessDenied");

            int totalRecords;
            var list = _repo.GetPaged(page, pageSize, tuNgay, denNgay, soChungTu, idKho, idNhaCungCap, trangThai, tenNguoiNhan, out totalRecords);

            var model = new PagedListViewModel<PhieuNhapKhoListViewModel>
            {
                Items = list,
                CurrentPage = page,
                PageSize = pageSize,
                TotalRecords = totalRecords,
                ActionName = "GetList"
            };

            ViewBag.TuNgay = tuNgay;
            ViewBag.DenNgay = denNgay;
            ViewBag.SoChungTu = soChungTu;
            ViewBag.Khos = GetKhoList(idKho);
            ViewBag.NhaCungCaps = GetNhaCungCapList(idNhaCungCap);
            ViewBag.TrangThai = trangThai;

            if (Request.IsAjaxRequest() || Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return PartialView("_PhieuNhapKhoList", model);

            return View("Index", model);
        }

        [HttpGet]
        public ActionResult GetList(int page = 1, int pageSize = 20, string tuNgay = "", string denNgay = "", string soChungTu = "", int? idKho = null, int? idNhaCungCap = null, int? trangThai = null, string tenNguoiNhan = "")
        {
            if (!PermissionHelper.HasPermission("PhieuNhapKho", LoaiPhanQuyen.Xem)) return Content("<div class='alert alert-danger'>Không có quyền truy cập</div>");

            try 
            {
                var list = _repo.GetPaged(page, pageSize, tuNgay, denNgay, soChungTu, idKho, idNhaCungCap, trangThai, tenNguoiNhan, out int totalRecords);

                var model = new PagedListViewModel<PhieuNhapKhoListViewModel>
                {
                    Items = list,
                    CurrentPage = page,
                    PageSize = pageSize,
                    TotalRecords = totalRecords,
                    ActionName = "GetList"
                };

                return PartialView("_PhieuNhapKhoList", model);
            }
            catch(Exception ex)
            {
                return Content($"<div class='alert alert-danger'>Lỗi Server: {ex.Message} <br/> {ex.StackTrace}</div>");
            }
        }

        [HttpGet]
        public ActionResult ExportExcel(string tuNgay = "", string denNgay = "", string soChungTu = "", int? idKho = null, int? idNhaCungCap = null, int? trangThai = null, string tenNguoiNhan = "")
        {
            if (!PermissionHelper.HasPermission("PhieuNhapKho", LoaiPhanQuyen.Xem)) 
                return View("AccessDenied");

            try
            {
                var list = _repo.GetPaged(1, 100000, tuNgay, denNgay, soChungTu, idKho, idNhaCungCap, trangThai, tenNguoiNhan, out int totalRecords);

                var session = (UserLoginViewModel)Session[CommonConstants.USER_SESSION];
                string nguoiLapBieu = session != null ? (session.HoDem + " " + session.Ten).Trim() : "Hệ thống";
                if (string.IsNullOrEmpty(nguoiLapBieu)) nguoiLapBieu = session?.UserName ?? "Hệ thống";

                var variables = new Dictionary<string, object>
                {
                    { "Ngay", DateTime.Now.ToString("dd") },
                    { "Thang", DateTime.Now.ToString("MM") },
                    { "Nam", DateTime.Now.ToString("yyyy") },
                    { "NguoiLapBieu", nguoiLapBieu }
                };

                int stt = 1;
                var exportData = list.Select(item => new {
                    STT = stt++,
                    SoChungTu = item.SoChungTu,
                    NgayNhap = item.NgayNhap,
                    TenKho = item.TenKho,
                    TenNhaCungCap = item.TenNhaCungCap,
                    SoHoaDon = item.SoHoaDon,
                    NgayHoaDon = item.NgayHoaDon,
                    TenNguoiGiao = item.TenNguoiGiao,
                    SoDienThoaiNguoiGiao = item.SoDienThoaiNguoiGiao,
                    TenNguoiNhan = item.TenNguoiNhan,
                    TongTienHang = item.TongTienHang,
                    TongTienThue = item.TongTienThue,
                    TongCong = item.TongCong,
                    TrangThai = item.TrangThai == 1 ? "Đề nghị ghi" : (item.TrangThai == 2 ? "Đã ghi" : (item.TrangThai == 3 ? "Đã hủy" : "")),
                    NguoiTao = item.NguoiTaoText,
                    NgayTao = item.NgayTao
                }).ToList();

                string fileExtension;
                var fileBytes = _excelExportService.Export("PN01", exportData, out fileExtension, variables);

                string contentType = fileExtension == "xls" 
                    ? "application/vnd.ms-excel" 
                    : "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

                return File(fileBytes, contentType, $"DanhSachPhieuNhapKho_{DateTime.Now:yyyyMMddHHmmss}.{fileExtension}");
            }
            catch (Exception ex)
            {
                TempData["ToastType"] = "error";
                TempData["ToastMessage"] = $"Lỗi xuất Excel: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

        public ActionResult Create()
        {
            if (!PermissionHelper.HasPermission("PhieuNhapKho", LoaiPhanQuyen.Them)) return View("AccessDenied");

            var model = new PhieuNhapKhoViewModel();
            model.SoChungTu = _repo.GenerateSoChungTu();
            
            return View("Edit", model);
        }

        public ActionResult Edit(int id, bool isView = false)
        {
            if (!PermissionHelper.HasPermission("PhieuNhapKho", LoaiPhanQuyen.Xem)) return View("AccessDenied");

            var entity = _repo.GetByID(id);
            if (entity == null) return HttpNotFound();

            var model = new PhieuNhapKhoViewModel
            {
                ID = entity.ID,
                SoChungTu = entity.SoChungTu,
                NgayNhap = entity.NgayNhap,
                IDKho = entity.IDKho,
                IDNhaCungCap = entity.IDNhaCungCap,
                SoHoaDon = entity.SoHoaDon,
                NgayHoaDon = entity.NgayHoaDon,
                TenNguoiGiao = entity.TenNguoiGiao,
                SoDienThoaiNguoiGiao = entity.SoDienThoaiNguoiGiao,
                TenNguoiNhan = entity.TenNguoiNhan,
                GhiChu = entity.GhiChu,
                TrangThai = entity.TrangThai,
                IsReadOnly = isView || entity.TrangThai == 2 || entity.TrangThai == 3
            };

            int total;
            var list = _repo.GetPaged(1, 1, null, null, entity.SoChungTu, null, null, null, null, out total);
            var item = list.FirstOrDefault();
            if (item != null)
            {
                model.TenKho = item.TenKho;
                model.TenNhaCungCap = item.TenNhaCungCap;
            }

            model.ChiTiets = _repo.GetChiTiet(id);
            ViewBag.IsView = isView;
            return View("Edit", model);
        }

        [HttpPost]
        public ActionResult Save(PhieuNhapKhoViewModel model)
        {
            if (model.ID == 0 && !PermissionHelper.HasPermission("PhieuNhapKho", LoaiPhanQuyen.Them)) return Json(new { success = false, message = "Không có quyền thêm mới" });
            if (model.ID > 0 && !PermissionHelper.HasPermission("PhieuNhapKho", LoaiPhanQuyen.CapNhat)) return Json(new { success = false, message = "Không có quyền sửa" });

            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Dữ liệu không hợp lệ" });
            }

            try
            {
                var user = GetCurrentUser();
                int userId = user?.IDNhanSu ?? 0;

                if (model.ID > 0)
                {
                    _repo.Save(model, userId);
                    return Json(new { success = true, message = "Cập nhật phiếu nhập kho thành công" });
                }
                else
                {
                    int newId = _repo.Save(model, userId);
                    return Json(new { success = true, id = newId, soChungTu = model.SoChungTu, message = "Đề nghị ghi thành công" });
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
            if (!PermissionHelper.HasPermission("PhieuNhapKho", LoaiPhanQuyen.TuyChon)) return Json(new { success = false, message = "Không có quyền ghi sổ" });

            try
            {
                var user = GetCurrentUser();
                int userId = user?.IDNhanSu ?? 0;

                _repo.GhiSo(id, userId);
                return Json(new { success = true, message = "Ghi sổ thành công" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public ActionResult HuyPhieu(int id, string lyDoHuy)
        {
            if (!PermissionHelper.HasPermission("PhieuNhapKho", LoaiPhanQuyen.TuyChon)) return Json(new { success = false, message = "Không có quyền hủy phiếu" });

            if (string.IsNullOrWhiteSpace(lyDoHuy))
            {
                return Json(new { success = false, message = "Vui lòng nhập lý do hủy" });
            }

            try
            {
                var user = GetCurrentUser();
                int userId = user?.IDNhanSu ?? 0;

                _repo.HuyPhieu(id, lyDoHuy, userId);
                return Json(new { success = true, message = "Hủy phiếu thành công" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public ActionResult Delete(int id)
        {
            if (!PermissionHelper.HasPermission("PhieuNhapKho", LoaiPhanQuyen.Xoa)) return Json(new { success = false, message = "Không có quyền xóa" });

            try
            {
                var user = GetCurrentUser();
                int userId = user?.IDNhanSu ?? 0;

                _repo.Delete(id, userId);
                return Json(new { success = true, message = "Xóa phiếu thành công" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // Dropdowns endpoints
        [HttpGet]
        public ActionResult SearchKhoHang(string q)
        {
            var data = _repo.GetKhoForDropdown(q);
            return Json(data.Select(x => new { id = (int)x.ID, text = (string)x.MaKhoHang + " - " + (string)x.TenKhoHang }), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult SearchNhaCungCap(string q)
        {
            var data = _repo.GetNhaCungCapForDropdown(q);
            return Json(data.Select(x => new { id = (int)x.ID, text = (string)x.MaNhaCungCap + " - " + (string)x.TenNhaCungCap }), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult SearchNhanSu(string q)
        {
            var data = _repo.GetNhanSuForDropdown(q);
            return Json(data.Select(x => new { id = (int)x.ID, text = (string)x.MaNhanSu + " - " + (string)x.HoTen }), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult SearchSanPham(string q)
        {
            var data = _repo.GetSanPhamForDropdown(q);
            return Json(data.Select(x => new { id = (int)x.ID, text = (string)x.MaSanPham + " - " + (string)x.TenSanPham, dvt = (string)x.DVT }), JsonRequestBehavior.AllowGet);
        }
    }
}
