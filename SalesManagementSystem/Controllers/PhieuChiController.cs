using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using SalesManagementSystem.Helpers;
using SalesManagementSystem.Models.ViewModels;
using SalesManagementSystem.Repositories.Interfaces;

namespace SalesManagementSystem.Controllers
{
    public class PhieuChiController : BaseController
    {
        private readonly IPhieuChiRepository _repo;

        public PhieuChiController(IPhieuChiRepository repo)
        {
            _repo = repo;
        }

        // GET: /phieu-chi
        public ActionResult Index(
            int page = 1, int pageSize = 20,
            string tuNgay = "", string denNgay = "",
            string soPhieuChi = "",
            int? idNhaCungCap = null,
            int? idKhoanMucChi = null,
            int? trangThai = null)
        {
            if (!PermissionHelper.HasPermission("PhieuChi", LoaiPhanQuyen.Xem))
                return View("AccessDenied");

            var list = _repo.GetList(tuNgay, denNgay, soPhieuChi, idNhaCungCap, idKhoanMucChi, trangThai).ToList();
            int totalRecords = list.Count;
            var pagedItems = list.Skip((page - 1) * pageSize).Take(pageSize);

            var model = new PagedListViewModel<PhieuChiListViewModel>
            {
                Items        = pagedItems,
                CurrentPage  = page,
                PageSize     = pageSize,
                TotalRecords = totalRecords,
                ActionName   = "GetList",
                Keyword      = soPhieuChi
            };

            PopulateFilterDropdowns();
            ViewBag.TuNgay        = tuNgay;
            ViewBag.DenNgay       = denNgay;
            ViewBag.SoPhieuChi    = soPhieuChi;
            ViewBag.IDNhaCungCap  = idNhaCungCap;
            ViewBag.IDKhoanMucChi = idKhoanMucChi;
            ViewBag.TrangThai     = trangThai;
            ViewBag.Title         = "Phiếu Chi";

            if (Request.IsAjaxRequest() || Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return PartialView("_PhieuChiList", model);

            return View("Index", model);
        }

        // GET: /phieu-chi/danh-sach
        public ActionResult GetList(
            int page = 1, int pageSize = 20,
            string tuNgay = "", string denNgay = "",
            string soPhieuChi = "",
            int? idNhaCungCap = null,
            int? idKhoanMucChi = null,
            int? trangThai = null)
        {
            if (!PermissionHelper.HasPermission("PhieuChi", LoaiPhanQuyen.Xem))
                return Content("<div class='alert alert-danger'>Không có quyền truy cập</div>");

            try
            {
                var list = _repo.GetList(tuNgay, denNgay, soPhieuChi, idNhaCungCap, idKhoanMucChi, trangThai).ToList();
                int totalRecords = list.Count;
                var pagedItems   = list.Skip((page - 1) * pageSize).Take(pageSize);

                var model = new PagedListViewModel<PhieuChiListViewModel>
                {
                    Items        = pagedItems,
                    CurrentPage  = page,
                    PageSize     = pageSize,
                    TotalRecords = totalRecords,
                    ActionName   = "GetList",
                    Keyword      = soPhieuChi
                };

                return PartialView("_PhieuChiList", model);
            }
            catch (Exception ex)
            {
                return Content($"<div class='alert alert-danger'>Lỗi: {ex.Message}</div>");
            }
        }

        // GET: /phieu-chi/them-moi
        [HttpGet]
        public ActionResult Create()
        {
            if (!PermissionHelper.HasPermission("PhieuChi", LoaiPhanQuyen.Them))
                return Content("<div class='alert alert-danger'>Không có quyền thêm mới</div>");

            ViewBag.Title = "Thêm mới Phiếu Chi";
            PopulateFormDropdowns();
            var model = new PhieuChiViewModel { NgayChi = DateTime.Today };
            model.SoPhieuChi = _repo.GenerateSoPhieuChi();
            return PartialView("_Form", model);
        }

        // GET: /phieu-chi/cap-nhat?id=x
        [HttpGet]
        public ActionResult Edit(int id)
        {
            if (!PermissionHelper.HasPermission("PhieuChi", LoaiPhanQuyen.CapNhat))
                return Content("<div class='alert alert-danger'>Không có quyền cập nhật</div>");

            var model = _repo.GetByID(id);
            if (model == null) return HttpNotFound();

            if (model.TrangThai == 2)
                return Content("<div class='alert alert-warning'>Phiếu đã ghi sổ, không thể chỉnh sửa.</div>");
            if (model.TrangThai == 3)
                return Content("<div class='alert alert-warning'>Phiếu đã hủy, không thể chỉnh sửa.</div>");

            ViewBag.Title = "Cập nhật Phiếu Chi";
            PopulateFormDropdowns(model.IDNhaCungCap);
            return PartialView("_Form", model);
        }

        // POST: /phieu-chi/save
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Save(PhieuChiViewModel model, bool ghiSo = false)
        {
            bool hasThem = PermissionHelper.HasPermission("PhieuChi", LoaiPhanQuyen.Them);
            bool hasSua  = PermissionHelper.HasPermission("PhieuChi", LoaiPhanQuyen.CapNhat);

            if (model.ID == 0 && !hasThem)
                return Json(new { success = false, message = "Không có quyền thêm mới" });
            if (model.ID > 0 && !hasSua)
                return Json(new { success = false, message = "Không có quyền cập nhật" });

            if (!ModelState.IsValid)
            {
                PopulateFormDropdowns(model.IDNhaCungCap);
                return PartialView("_Form", model);
            }

            try
            {
                var user   = GetCurrentUser();
                int userId = user?.IDNhanSu ?? 0;

                if (model.ID == 0)
                    model.SoPhieuChi = _repo.GenerateSoPhieuChi();
                else if (string.IsNullOrEmpty(model.SoPhieuChi))
                    model.SoPhieuChi = _repo.GenerateSoPhieuChi();

                int savedId = _repo.Save(model, userId);

                if (model.ID == 0)
                    AuditLog.AddInsert("KT_PhieuChi", savedId.ToString(), model);
                else
                    AuditLog.AddUpdate("KT_PhieuChi", model.ID.ToString(), null, model);

                if (ghiSo)
                {
                    _repo.GhiSo(savedId, userId);
                    AuditLog.AddUpdate("KT_PhieuChi", savedId.ToString(), null, new { TrangThai = 2 });
                }

                string msg = ghiSo ? "Lưu và ghi sổ thành công" : "Lưu thành công";
                return Json(new { success = true, id = savedId, message = msg });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        // POST: /phieu-chi/ghi-so
        [HttpPost]
        public ActionResult GhiSo(int id)
        {
            if (!PermissionHelper.HasPermission("PhieuChi", LoaiPhanQuyen.TuyChon))
                return Json(new { success = false, message = "Không có quyền ghi sổ" });

            try
            {
                var user = GetCurrentUser();
                _repo.GhiSo(id, user?.IDNhanSu ?? 0);
                return Json(new { success = true, message = "Ghi sổ thành công" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Ghi sổ thất bại: " + ex.Message });
            }
        }

        // POST: /phieu-chi/huy
        [HttpPost]
        public ActionResult Huy(int id, string lyDo)
        {
            if (!PermissionHelper.HasPermission("PhieuChi", LoaiPhanQuyen.TuyChon))
                return Json(new { success = false, message = "Không có quyền hủy phiếu" });

            if (string.IsNullOrWhiteSpace(lyDo))
                return Json(new { success = false, message = "Vui lòng nhập lý do hủy." });

            try
            {
                var user = GetCurrentUser();
                _repo.Huy(id, user?.IDNhanSu ?? 0, lyDo);
                return Json(new { success = true, message = "Hủy phiếu chi thành công" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Hủy thất bại: " + ex.Message });
            }
        }

        // POST: /phieu-chi/xoa
        [HttpPost]
        public ActionResult Delete(int id)
        {
            if (!PermissionHelper.HasPermission("PhieuChi", LoaiPhanQuyen.Xoa))
                return Json(new { success = false, message = "Không có quyền xóa" });

            try
            {
                var user  = GetCurrentUser();
                var model = _repo.GetByID(id);
                if (model == null)
                    return Json(new { success = false, message = "Phiếu chi không tồn tại." });
                if (model.TrangThai == 2)
                    return Json(new { success = false, message = "Không thể xóa phiếu đã ghi sổ." });

                _repo.Delete(id, user?.IDNhanSu ?? 0);
                AuditLog.AddDelete("KT_PhieuChi", id.ToString(), model);
                return Json(new { success = true, message = "Xóa thành công" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Xóa thất bại: " + ex.Message });
            }
        }

        // GET AJAX: /phieu-chi/get-phieu-nhap?idNhaCungCap=x
        [HttpGet]
        public ActionResult GetPhieuNhapByNCC(int? idNhaCungCap)
        {
            var list = _repo.GetPhieuNhapDropdown(idNhaCungCap)
                .Select(x => new { id = (int)x.ID, text = (string)x.TenHienThi });
            return Json(list, JsonRequestBehavior.AllowGet);
        }

        // GET AJAX: /phieu-chi/get-phieu-nhap-detail?idPhieuNhap=x
        [HttpGet]
        public ActionResult GetPhieuNhapDetail(int idPhieuNhap)
        {
            try
            {
                var phieuNhap = _repo.GetPhieuNhapDetail(idPhieuNhap);
                if (phieuNhap == null)
                    return Json(new { success = false }, JsonRequestBehavior.AllowGet);

                var cultureVi = new System.Globalization.CultureInfo("vi-VN");
                
                // Lấy danh sách lịch sử chi tiền
                var lichSu = _repo.GetLichSuChiTienPhieuNhap(idPhieuNhap)
                    .Select(x => new {
                        soPhieuChi = (string)x.SoPhieuChi,
                        ngayChi = ((DateTime)x.NgayChi).ToString("dd/MM/yyyy"),
                        soTienChi = ((decimal)x.SoTienChi).ToString("N0", cultureVi),
                        trangThai = (int)x.TrangThai
                    });

                return Json(new {
                    success = true,
                    tongCong = ((decimal)phieuNhap.TongCong).ToString("N0", cultureVi),
                    daThanhToan = ((decimal)phieuNhap.DaThanhToan).ToString("N0", cultureVi),
                    conLai = ((decimal)phieuNhap.ConLai).ToString("N0", cultureVi),
                    rawConLai = (decimal)phieuNhap.ConLai,
                    lichSu = lichSu
                }, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                return Json(new { success = false }, JsonRequestBehavior.AllowGet);
            }
        }

        private void PopulateFilterDropdowns()
        {
            var khoanMucs = _repo.GetKhoanMucDropdown()
                .Select(x => new SelectListItem { Value = ((int)x.ID).ToString(), Text = (string)x.TenHienThi });
            ViewBag.KhoanMucList = new SelectList(khoanMucs.ToList(), "Value", "Text");

            var nccs = _repo.GetNhaCungCapDropdown()
                .Select(x => new SelectListItem { Value = ((int)x.ID).ToString(), Text = (string)x.TenHienThi });
            ViewBag.NhaCungCapList = new SelectList(nccs.ToList(), "Value", "Text");
        }

        private void PopulateFormDropdowns(int? idNhaCungCap = null)
        {
            var khoanMucs = _repo.GetKhoanMucDropdown()
                .Select(x => new SelectListItem { Value = ((int)x.ID).ToString(), Text = (string)x.TenHienThi });
            ViewBag.KhoanMucList = new SelectList(khoanMucs.ToList(), "Value", "Text");

            var taiKhoans = _repo.GetTaiKhoanDropdown()
                .Select(x => new SelectListItem { Value = ((int)x.ID).ToString(), Text = (string)x.TenHienThi });
            ViewBag.TaiKhoanList = new SelectList(taiKhoans.ToList(), "Value", "Text");

            var nccs = _repo.GetNhaCungCapDropdown()
                .Select(x => new SelectListItem { Value = ((int)x.ID).ToString(), Text = (string)x.TenHienThi });
            ViewBag.NhaCungCapList = new SelectList(nccs.ToList(), "Value", "Text");

            var nhanSus = _repo.GetNhanSuDropdown()
                .Select(x => new SelectListItem { Value = ((int)x.ID).ToString(), Text = (string)x.TenHienThi });
            ViewBag.NhanSuList = new SelectList(nhanSus.ToList(), "Value", "Text");

            var phieuNhaps = _repo.GetPhieuNhapDropdown(idNhaCungCap)
                .Select(x => new SelectListItem { Value = ((int)x.ID).ToString(), Text = (string)x.TenHienThi });
            ViewBag.PhieuNhapList = new SelectList(phieuNhaps.ToList(), "Value", "Text");
        }
    }
}
