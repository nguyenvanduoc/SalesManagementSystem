using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SalesManagementSystem.Helpers;
using SalesManagementSystem.Models.Entities;
using SalesManagementSystem.Models.ViewModels;
using SalesManagementSystem.Repositories.Interfaces;
using SalesManagementSystem.Services.Interfaces;

namespace SalesManagementSystem.Controllers
{
    public class PhieuThuKhachHangController : BaseController
    {
        private readonly IPhieuThuKhachHangRepository _repo;
        private readonly IExcelExportService _excelExportService;

        public PhieuThuKhachHangController(IPhieuThuKhachHangRepository repo, IExcelExportService excelExportService)
        {
            _repo = repo;
            _excelExportService = excelExportService;
        }

        // GET: /phieu-thu-khach-hang
        public ActionResult Index(
            int page = 1, int pageSize = 20,
            string tuNgay = "", string denNgay = "",
            string soPhieuThu = "",
            int? idKhachHang = null,
            int? trangThai = null,
            string nguoiNopTien = "",
            int? idTaiKhoanThanhToan = null)
        {
            if (!PermissionHelper.HasPermission("PhieuThuKhachHang", LoaiPhanQuyen.Xem))
                return View("AccessDenied");

            var list = _repo.GetList(tuNgay, denNgay, soPhieuThu, idKhachHang, trangThai, nguoiNopTien, idTaiKhoanThanhToan).ToList();
            int totalRecords = list.Count;
            var pagedItems = list.Skip((page - 1) * pageSize).Take(pageSize);

            var model = new PagedListViewModel<PhieuThuKhachHangListViewModel>
            {
                Items        = pagedItems,
                CurrentPage  = page,
                PageSize     = pageSize,
                TotalRecords = totalRecords,
                ActionName   = "GetList",
                Keyword      = soPhieuThu
            };

            var dashboard = _repo.GetDashboardData(tuNgay, denNgay, soPhieuThu, idKhachHang, trangThai, nguoiNopTien, idTaiKhoanThanhToan);
            ViewBag.Dashboard = dashboard;

            PopulateFilterDropdowns();
            ViewBag.TuNgay               = tuNgay;
            ViewBag.DenNgay              = denNgay;
            ViewBag.SoPhieuThu           = soPhieuThu;
            ViewBag.IDKhachHang          = idKhachHang;
            ViewBag.TrangThai            = trangThai;
            ViewBag.NguoiNopTien         = nguoiNopTien;
            ViewBag.IDTaiKhoanThanhToan  = idTaiKhoanThanhToan;
            ViewBag.Title                = "Phiếu Thu Khách Hàng";

            if (Request.IsAjaxRequest() || Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return PartialView("_PhieuThuKhachHangList", model);

            return View("Index", model);
        }

        // GET: /phieu-thu-khach-hang/danh-sach
        public ActionResult GetList(
            int page = 1, int pageSize = 20,
            string tuNgay = "", string denNgay = "",
            string soPhieuThu = "",
            int? idKhachHang = null,
            int? trangThai = null,
            string nguoiNopTien = "",
            int? idTaiKhoanThanhToan = null)
        {
            if (!PermissionHelper.HasPermission("PhieuThuKhachHang", LoaiPhanQuyen.Xem))
                return Content("<div class='alert alert-danger'>Không có quyền truy cập</div>");

            try
            {
                var list = _repo.GetList(tuNgay, denNgay, soPhieuThu, idKhachHang, trangThai, nguoiNopTien, idTaiKhoanThanhToan).ToList();
                int totalRecords = list.Count;
                var pagedItems   = list.Skip((page - 1) * pageSize).Take(pageSize);

                var model = new PagedListViewModel<PhieuThuKhachHangListViewModel>
                {
                    Items        = pagedItems,
                    CurrentPage  = page,
                    PageSize     = pageSize,
                    TotalRecords = totalRecords,
                    ActionName   = "GetList",
                    Keyword      = soPhieuThu
                };

                var dashboard = _repo.GetDashboardData(tuNgay, denNgay, soPhieuThu, idKhachHang, trangThai, nguoiNopTien, idTaiKhoanThanhToan);
                ViewBag.Dashboard = dashboard;

                return PartialView("_PhieuThuKhachHangList", model);
            }
            catch (Exception ex)
            {
                return Content($"<div class='alert alert-danger'>Lỗi: {ex.Message}</div>");
            }
        }

        // GET: /phieu-thu-khach-hang/them-moi
        [HttpGet]
        public ActionResult Create(int? idKhachHang = null, int? idChungTuBanHang = null, decimal? soTien = null)
        {
            if (!PermissionHelper.HasPermission("PhieuThuKhachHang", LoaiPhanQuyen.Them))
                return Content("<div class='alert alert-danger'>Không có quyền thêm mới</div>");

            ViewBag.Title = "Thêm mới Phiếu Thu Khách Hàng";
            PopulateFormDropdowns(idKhachHang);
            var model = new PhieuThuKhachHangViewModel 
            { 
                NgayThu = DateTime.Today,
                IDKhachHang = idKhachHang,
                SoTienThu = soTien ?? 0M
            };
            model.SoPhieuThu = _repo.GenerateSoPhieuThu();
            ViewBag.PreSelectedChungTu = idChungTuBanHang;
            return PartialView("_Form", model);
        }

        [HttpGet]
        public ActionResult GetDetailInline(int id)
        {
            var model = _repo.GetByID(id);
            if (model == null) return Content("<div class='alert alert-danger'>Không tìm thấy dữ liệu</div>");
            return PartialView("_DetailInline", model);
        }

        // GET: /phieu-thu-khach-hang/cap-nhat?id=x
        [HttpGet]
        public ActionResult Edit(int id)
        {
            if (!PermissionHelper.HasPermission("PhieuThuKhachHang", LoaiPhanQuyen.CapNhat))
                return Content("<div class='alert alert-danger'>Không có quyền cập nhật</div>");

            var model = _repo.GetByID(id);
            if (model == null) return HttpNotFound();

            if (model.TrangThai == 2)
                return Content("<div class='alert alert-warning'>Phiếu đã ghi, không thể chỉnh sửa.</div>");
            if (model.TrangThai == 3)
                return Content("<div class='alert alert-warning'>Phiếu đã hủy, không thể chỉnh sửa.</div>");

            ViewBag.Title = "Cập nhật Phiếu Thu Khách Hàng";
            PopulateFormDropdowns(model.IDKhachHang);
            return PartialView("_Form", model);
        }

        // GET: /phieu-thu-khach-hang/chi-tiet?id=x
        [HttpGet]
        public ActionResult Details(int id)
        {
            if (!PermissionHelper.HasPermission("PhieuThuKhachHang", LoaiPhanQuyen.Xem))
                return Content("<div class='alert alert-danger'>Không có quyền xem chi tiết</div>");

            var model = _repo.GetByID(id);
            if (model == null) return HttpNotFound();

            ViewBag.Title = "Chi tiết Phiếu Thu Khách Hàng";
            ViewBag.IsView = true;
            PopulateFormDropdowns(model.IDKhachHang);
            return PartialView("_Form", model);
        }

        // GET: /phieu-thu-khach-hang/dieu-chinh-phan-bo?id=x
        [HttpGet]
        public ActionResult AdjustAllocation(int id)
        {
            if (!PermissionHelper.HasPermission("PhieuThuKhachHang", LoaiPhanQuyen.CapNhat))
                return Content("<div class='alert alert-danger'>Không có quyền cập nhật (điều chỉnh phân bổ)</div>");

            var model = _repo.GetByID(id);
            if (model == null) return HttpNotFound();

            if (model.TrangThai == 3)
                return Content("<div class='alert alert-danger'>Phiếu đã hủy, không thể điều chỉnh.</div>");

            if (model.TrangThai != 2)
                return Content("<div class='alert alert-warning'>Chỉ có thể điều chỉnh phiếu đã ghi sổ.</div>");

            ViewBag.Title = "Cập nhật";
            ViewBag.IsAdjustAllocation = true;
            PopulateFormDropdowns(model.IDKhachHang);
            return PartialView("_Form", model);
        }

        [HttpPost]
        public ActionResult AdjustAllocation(PhieuThuKhachHangViewModel model, string chiTietJson)
        {
            try
            {
                if (!PermissionHelper.HasPermission("PhieuThuKhachHang", LoaiPhanQuyen.CapNhat))
                    return Json(new { success = false, message = "Không có quyền thực hiện" });

                var oldModel = _repo.GetByID(model.ID);
                if (oldModel == null)
                    return Json(new { success = false, message = "Phiếu thu không tồn tại" });

                if (oldModel.TrangThai == 3)
                    return Json(new { success = false, message = "Phiếu thu đã hủy, không thể điều chỉnh." });

                var user = GetCurrentUser();
                int userId = user?.IDNhanSu ?? 0;

                var newChiTiets = new List<PhieuThuKhachHangChiTietViewModel>();
                if (!string.IsNullOrEmpty(chiTietJson))
                {
                    newChiTiets = Newtonsoft.Json.JsonConvert.DeserializeObject<List<PhieuThuKhachHangChiTietViewModel>>(chiTietJson);
                }

                _repo.DieuChinhPhanBo(model, newChiTiets, userId);

                AuditLog.AddUpdate("KT_PhieuThu", model.ID.ToString(), null, model);

                return Json(new { success = true, message = "Cập nhật thành công." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // POST: /phieu-thu-khach-hang/save
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Save(PhieuThuKhachHangViewModel model, string chiTietJson, bool ghiSo = false)
        {
            bool hasThem = PermissionHelper.HasPermission("PhieuThuKhachHang", LoaiPhanQuyen.Them);
            bool hasSua  = PermissionHelper.HasPermission("PhieuThuKhachHang", LoaiPhanQuyen.CapNhat);

            if (model.ID == 0 && !hasThem)
                return Json(new { success = false, message = "Không có quyền thêm mới" });
            if (model.ID > 0 && !hasSua)
                return Json(new { success = false, message = "Không có quyền cập nhật" });

            if (!ModelState.IsValid)
            {
                PopulateFormDropdowns(model.IDKhachHang);
                return PartialView("_Form", model);
            }

            if (!string.IsNullOrEmpty(chiTietJson))
            {
                try {
                    model.ChiTiets = Newtonsoft.Json.JsonConvert.DeserializeObject<List<PhieuThuKhachHangChiTietViewModel>>(chiTietJson);
                } catch {
                    model.ChiTiets = new List<PhieuThuKhachHangChiTietViewModel>();
                }
            }

            try
            {
                var user   = GetCurrentUser();
                int userId = user?.IDNhanSu ?? 0;

                if (model.ID == 0)
                    model.SoPhieuThu = _repo.GenerateSoPhieuThu();
                else if (string.IsNullOrEmpty(model.SoPhieuThu))
                    model.SoPhieuThu = _repo.GenerateSoPhieuThu();

                int savedId = _repo.Save(model, userId);

                if (model.ID == 0)
                    AuditLog.AddInsert("KT_PhieuThu", savedId.ToString(), model);
                else
                    AuditLog.AddUpdate("KT_PhieuThu", model.ID.ToString(), null, model);

                if (ghiSo)
                {
                    _repo.GhiSo(savedId, userId);
                    AuditLog.AddUpdate("KT_PhieuThu", savedId.ToString(), null, new { TrangThai = 2 });
                }

                string msg = ghiSo ? "Lưu và ghi thành công" : "Lưu thành công";
                return Json(new { success = true, id = savedId, message = msg });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        // POST: /phieu-thu-khach-hang/ghi-so
        [HttpPost]
        public ActionResult GhiSo(int id)
        {
            if (!PermissionHelper.HasPermission("PhieuThuKhachHang", LoaiPhanQuyen.TuyChon))
                return Json(new { success = false, message = "Không có quyền ghi" });

            try
            {
                var user = GetCurrentUser();
                _repo.GhiSo(id, user?.IDNhanSu ?? 0);
                return Json(new { success = true, message = "ghi thành công" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "ghi thất bại: " + ex.Message });
            }
        }

        // POST: /phieu-thu-khach-hang/huy
        [HttpPost]
        public ActionResult Huy(int id, string lyDo)
        {
            if (!PermissionHelper.HasPermission("PhieuThuKhachHang", LoaiPhanQuyen.TuyChon))
                return Json(new { success = false, message = "Không có quyền hủy phiếu" });

            if (string.IsNullOrWhiteSpace(lyDo))
                return Json(new { success = false, message = "Vui lòng nhập lý do hủy." });

            try
            {
                var user = GetCurrentUser();
                _repo.Huy(id, user?.IDNhanSu ?? 0, lyDo);
                return Json(new { success = true, message = "Hủy phiếu thu thành công" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Hủy thất bại: " + ex.Message });
            }
        }

        // POST: /phieu-thu-khach-hang/xoa
        [HttpPost]
        public ActionResult Delete(int id)
        {
            if (!PermissionHelper.HasPermission("PhieuThuKhachHang", LoaiPhanQuyen.Xoa))
                return Json(new { success = false, message = "Không có quyền xóa" });

            try
            {
                var user  = GetCurrentUser();
                var model = _repo.GetByID(id);
                if (model == null)
                    return Json(new { success = false, message = "Phiếu thu không tồn tại." });
                if (model.TrangThai == 2)
                    return Json(new { success = false, message = "Không thể xóa phiếu đã ghi." });

                _repo.Delete(id, user?.IDNhanSu ?? 0);
                AuditLog.AddDelete("KT_PhieuThu", id.ToString(), model);
                return Json(new { success = true, message = "Xóa thành công" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Xóa thất bại: " + ex.Message });
            }
        }

        // GET AJAX: /phieu-thu-khach-hang/get-chung-tu?idKhachHang=x
        [HttpGet]
        public ActionResult GetChungTuByKhachHang(int? idKhachHang)
        {
            var list = _repo.GetChungTuBanHangDropdown(idKhachHang)
                .Select(x => new { id = (int)x.ID, text = (string)x.TenHienThi });
            return Json(list, JsonRequestBehavior.AllowGet);
        }

        // GET AJAX: /phieu-thu-khach-hang/get-chung-tu-cong-no?idKhachHang=x
        [HttpGet]
        public ActionResult GetChungTuCongNo(int idKhachHang)
        {
            try
            {
                var list = _repo.GetChungTuBanHangCongNo(idKhachHang)
                    .Select(x => new {
                        id = (int)x.ID,
                        soChungTu = (string)x.SoChungTu,
                        ngayChungTu = ((DateTime)x.NgayNhap).ToString("dd/MM/yyyy"), // NgayNhap aliased in SP
                        tongTien = (decimal)x.TongTien,
                        daThanhToan = (decimal)x.DaThanhToan,
                        conLai = (decimal)x.ConLai
                    });
                return Json(new { success = true, data = list }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // GET AJAX: /phieu-thu-khach-hang/get-tien-tra-truoc?idKhachHang=x
        [HttpGet]
        public ActionResult GetTienTraTruocKhachHang(int idKhachHang, int? excludeId = null)
        {
            try
            {
                var tien = _repo.GetTienTraTruocKhachHang(idKhachHang);
                if (excludeId.HasValue && excludeId.Value > 0)
                {
                    var phieuThu = _repo.GetByID(excludeId.Value);
                    if (phieuThu != null && phieuThu.ChiTiets != null)
                    {
                        var excessCreated = phieuThu.ChiTiets.Where(x => x.LoaiThu == 2).Sum(x => x.SoTienPhanBo);
                        var prepaymentUsed = phieuThu.ChiTiets.Where(x => x.LoaiThu == 3).Sum(x => x.SoTienPhanBo);
                        tien = tien - excessCreated + prepaymentUsed;
                    }
                }
                return Json(new { success = true, data = tien }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        private void PopulateFilterDropdowns()
        {
            var khs = _repo.GetKhachHangDropdown()
                .Select(x => new SelectListItem { Value = ((int)x.ID).ToString(), Text = (string)x.TenHienThi });
            ViewBag.KhachHangList = new SelectList(khs.ToList(), "Value", "Text");

            var taiKhoans = _repo.GetTaiKhoanThanhToanDropdown()
                .Select(x => new SelectListItem { Value = ((int)x.ID).ToString(), Text = (string)x.TenHienThi });
            ViewBag.TaiKhoanList = new SelectList(taiKhoans.ToList(), "Value", "Text");
        }

        private void PopulateFormDropdowns(int? idKhachHang = null)
        {
            var taiKhoans = _repo.GetTaiKhoanThanhToanDropdown()
                .Select(x => new SelectListItem { Value = ((int)x.ID).ToString(), Text = (string)x.TenHienThi });
            ViewBag.TaiKhoanList = new SelectList(taiKhoans.ToList(), "Value", "Text");

            var khs = _repo.GetKhachHangDropdown()
                .Select(x => new SelectListItem { Value = ((int)x.ID).ToString(), Text = (string)x.TenHienThi });
            ViewBag.KhachHangList = new SelectList(khs.ToList(), "Value", "Text");

            var chungTus = _repo.GetChungTuBanHangDropdown(idKhachHang)
                .Select(x => new SelectListItem { Value = ((int)x.ID).ToString(), Text = (string)x.TenHienThi });
            ViewBag.ChungTuList = new SelectList(chungTus.ToList(), "Value", "Text");
        }

        [HttpGet]
        public ActionResult GetFiles(int idPhieuThu)
        {
            if (!PermissionHelper.HasPermission("PhieuThuKhachHang", LoaiPhanQuyen.Xem))
                return Content("<div class='alert alert-danger'>Không có quyền xem file đính kèm</div>");

            try
            {
                ViewBag.CanDeleteFile = PermissionHelper.HasPermission("PhieuThuKhachHang", LoaiPhanQuyen.Xoa);
                var files = _repo.File_GetList(idPhieuThu);
                return PartialView("_PhieuThuFileList", files);
            }
            catch (Exception ex)
            {
                return Content("Lỗi tải danh sách file: " + ex.Message);
            }
        }

        [HttpPost]
        public ActionResult UploadFile(int idPhieuThu, HttpPostedFileBase file)
        {
            if (!PermissionHelper.HasPermission("PhieuThuKhachHang", LoaiPhanQuyen.CapNhat))
                return Json(new { success = false, message = "Không có quyền đính kèm file" });

            try
            {
                if (file == null || file.ContentLength == 0)
                    return Json(new { success = false, message = "Vui lòng chọn file hợp lệ." });

                var phieuThu = _repo.GetByID(idPhieuThu);
                if (phieuThu == null)
                    return Json(new { success = false, message = "Phiếu thu không tồn tại." });

                string ext = Path.GetExtension(file.FileName).ToLower();
                var allowedExts = new[] { ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".jpg", ".jpeg", ".png" };
                if (!allowedExts.Contains(ext))
                    return Json(new { success = false, message = "Định dạng file không được hỗ trợ." });

                byte[] fileData;
                using (var binaryReader = new BinaryReader(file.InputStream))
                {
                    fileData = binaryReader.ReadBytes(file.ContentLength);
                }

                var model = new PhieuThuKhachHangFile
                {
                    IDPhieuThu = idPhieuThu,
                    TenFile = Path.GetFileName(file.FileName),
                    LoaiFile = ext.Replace(".", ""),
                    DungLuong = file.ContentLength,
                    NoiDungFile = fileData
                };

                var user = GetCurrentUser();
                _repo.File_Save(model, user?.IDNhanSu ?? 0);

                return Json(new { success = true, message = "Tải file lên thành công." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi tải file: " + ex.Message });
            }
        }

        [HttpPost]
        public ActionResult DeleteFile(int id)
        {
            if (!PermissionHelper.HasPermission("PhieuThuKhachHang", LoaiPhanQuyen.Xoa))
                return Json(new { success = false, message = "Không có quyền xóa file" });

            try
            {
                var user = GetCurrentUser();
                _repo.File_Delete(id, user?.IDNhanSu ?? 0);
                return Json(new { success = true, message = "Xóa file thành công." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi khi xóa file: " + ex.Message });
            }
        }

        [HttpGet]
        public ActionResult DownloadFile(int id)
        {
            if (!PermissionHelper.HasPermission("PhieuThuKhachHang", LoaiPhanQuyen.Xem))
                return HttpNotFound("Không có quyền tải file.");

            try
            {
                var file = _repo.File_GetByID(id);
                if (file == null) return HttpNotFound("File không tồn tại.");

                return File(file.NoiDungFile, GetMimeType(file.LoaiFile), file.TenFile);
            }
            catch (Exception)
            {
                return HttpNotFound("Có lỗi xảy ra khi lấy file.");
            }
        }

        [HttpGet]
        public ActionResult ViewFile(int id)
        {
            if (!PermissionHelper.HasPermission("PhieuThuKhachHang", LoaiPhanQuyen.Xem))
                return HttpNotFound("Không có quyền xem file.");

            try
            {
                var file = _repo.File_GetByID(id);
                if (file == null) return HttpNotFound("File không tồn tại.");

                var ext = (file.LoaiFile ?? "").ToLower();
                if (ext != "pdf" && ext != "jpg" && ext != "jpeg" && ext != "png")
                    return File(file.NoiDungFile, "application/octet-stream", file.TenFile);

                return File(file.NoiDungFile, GetMimeType(file.LoaiFile));
            }
            catch (Exception)
            {
                return HttpNotFound("Có lỗi xảy ra khi lấy file.");
            }
        }

        private string GetMimeType(string loaiFile)
        {
            switch ((loaiFile ?? "").ToLower())
            {
                case "pdf": return "application/pdf";
                case "doc": return "application/msword";
                case "docx": return "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
                case "xls": return "application/vnd.ms-excel";
                case "xlsx": return "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                case "jpg":
                case "jpeg": return "image/jpeg";
                case "png": return "image/png";
                default: return "application/octet-stream";
            }
        }
    }
}
