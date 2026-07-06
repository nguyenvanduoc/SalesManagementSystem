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
    public class PhieuChiController : BaseController
    {
        private readonly IPhieuChiRepository _repo;
        private readonly IExcelExportService _excelExportService;

        public PhieuChiController(IPhieuChiRepository repo, IExcelExportService excelExportService)
        {
            _repo = repo;
            _excelExportService = excelExportService;
        }

        // GET: /phieu-chi
        public ActionResult Index(
            int page = 1, int pageSize = 20,
            string tuNgay = "", string denNgay = "",
            string soPhieuChi = "",
            int? idNhaCungCap = null,
            int? idKhoanMucChi = null,
            int? trangThai = null,
            string nguoiNhanTien = "",
            int? idTaiKhoanThanhToan = null)
        {
            if (!PermissionHelper.HasPermission("PhieuChi", LoaiPhanQuyen.Xem))
                return View("AccessDenied");

            var list = _repo.GetList(tuNgay, denNgay, soPhieuChi, idNhaCungCap, idKhoanMucChi, trangThai, nguoiNhanTien, idTaiKhoanThanhToan).ToList();
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

            var dashboard = _repo.GetDashboardData(tuNgay, denNgay, soPhieuChi, idNhaCungCap, idKhoanMucChi, trangThai, nguoiNhanTien, idTaiKhoanThanhToan);
            ViewBag.Dashboard = dashboard;

            PopulateFilterDropdowns();
            ViewBag.TuNgay               = tuNgay;
            ViewBag.DenNgay              = denNgay;
            ViewBag.SoPhieuChi           = soPhieuChi;
            ViewBag.IDNhaCungCap         = idNhaCungCap;
            ViewBag.IDKhoanMucChi        = idKhoanMucChi;
            ViewBag.TrangThai            = trangThai;
            ViewBag.NguoiNhanTien        = nguoiNhanTien;
            ViewBag.IDTaiKhoanThanhToan = idTaiKhoanThanhToan;
            ViewBag.Title                = "Phiếu Chi";

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
            int? trangThai = null,
            string nguoiNhanTien = "",
            int? idTaiKhoanThanhToan = null)
        {
            if (!PermissionHelper.HasPermission("PhieuChi", LoaiPhanQuyen.Xem))
                return Content("<div class='alert alert-danger'>Không có quyền truy cập</div>");

            try
            {
                var list = _repo.GetList(tuNgay, denNgay, soPhieuChi, idNhaCungCap, idKhoanMucChi, trangThai, nguoiNhanTien, idTaiKhoanThanhToan).ToList();
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

                var dashboard = _repo.GetDashboardData(tuNgay, denNgay, soPhieuChi, idNhaCungCap, idKhoanMucChi, trangThai, nguoiNhanTien, idTaiKhoanThanhToan);
                ViewBag.Dashboard = dashboard;

                return PartialView("_PhieuChiList", model);
            }
            catch (Exception ex)
            {
                return Content($"<div class='alert alert-danger'>Lỗi: {ex.Message}</div>");
            }
        }

        // GET: /phieu-chi/export-excel
        [HttpGet]
        public ActionResult ExportExcel(
            string tuNgay = "", string denNgay = "",
            string soPhieuChi = "",
            int? idNhaCungCap = null,
            int? idKhoanMucChi = null,
            int? trangThai = null,
            string nguoiNhanTien = "",
            int? idTaiKhoanThanhToan = null)
        {
            if (!PermissionHelper.HasPermission("PhieuChi", LoaiPhanQuyen.Xem))
                return Content("Không có quyền xuất Excel");

            try
            {
                var list = _repo.GetList(tuNgay, denNgay, soPhieuChi, idNhaCungCap, idKhoanMucChi, trangThai, nguoiNhanTien, idTaiKhoanThanhToan).ToList();

                var variables = new Dictionary<string, object>
                {
                    { "TuNgay", string.IsNullOrEmpty(tuNgay) ? "" : $"Từ ngày: {DateTime.Parse(tuNgay):dd/MM/yyyy}" },
                    { "DenNgay", string.IsNullOrEmpty(denNgay) ? "" : $"Đến ngày: {DateTime.Parse(denNgay):dd/MM/yyyy}" }
                };

                int stt = 1;
                var exportData = list.Select(item => new {
                    STT = stt++,
                    SoPhieuChi = item.SoPhieuChi,
                    NgayChi = item.NgayChi,
                    KhoanMucChi = item.TenKhoanMuc,
                    TaiKhoanTT = item.TenTaiKhoanThanhToan,
                    NhaCungCap = item.TenNhaCungCap,
                    NguoiNhan = !string.IsNullOrEmpty(item.NguoiNhanTien) 
                        ? item.NguoiNhanTien + (!string.IsNullOrEmpty(item.SoDienThoaiNguoiNhan) ? $" ({item.SoDienThoaiNguoiNhan})" : "") 
                        : item.TenNguoiNhan,
                    SoTienChi = item.SoTienChi,
                    TrangThai = item.TenTrangThai
                }).ToList();

                string fileExtension;
                var fileBytes = _excelExportService.Export("PC01", exportData, out fileExtension, variables);

                string contentType = fileExtension == "xls" 
                    ? "application/vnd.ms-excel" 
                    : "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

                return File(fileBytes, contentType, $"DanhSachPhieuChi_{DateTime.Now:yyyyMMddHHmmss}.{fileExtension}");
            }
            catch (Exception ex)
            {
                return Content($"Lỗi xuất Excel: {ex.Message}");
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

        [HttpGet]
        public ActionResult GetDetailInline(int id)
        {
            var model = _repo.GetByID(id);
            if (model == null) return Content("<div class='alert alert-danger'>Không tìm thấy dữ liệu</div>");
            return PartialView("_DetailInline", model);
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
                return Content("<div class='alert alert-warning'>Phiếu đã ghi, không thể chỉnh sửa.</div>");
            if (model.TrangThai == 3)
                return Content("<div class='alert alert-warning'>Phiếu đã hủy, không thể chỉnh sửa.</div>");

            ViewBag.Title = "Cập nhật Phiếu Chi";
            PopulateFormDropdowns(model.IDNhaCungCap, model.IDPhieuNhap);
            return PartialView("_Form", model);
        }

        // GET: /phieu-chi/chi-tiet?id=x
        [HttpGet]
        public ActionResult Details(int id)
        {
            if (!PermissionHelper.HasPermission("PhieuChi", LoaiPhanQuyen.Xem))
                return Content("<div class='alert alert-danger'>Không có quyền xem chi tiết</div>");

            var model = _repo.GetByID(id);
            if (model == null) return HttpNotFound();

            ViewBag.Title = "Chi tiết Phiếu Chi";
            ViewBag.IsView = true;
            PopulateFormDropdowns(model.IDNhaCungCap, model.IDPhieuNhap);
            return PartialView("_Form", model);
        }

        // GET: /phieu-chi/dieu-chinh-phan-bo?id=x
        [HttpGet]
        public ActionResult AdjustAllocation(int id)
        {
            if (!PermissionHelper.HasPermission("PhieuChi", LoaiPhanQuyen.CapNhat))
                return Content("<div class='alert alert-danger'>Không có quyền cập nhật (điều chỉnh phân bổ)</div>");

            var model = _repo.GetByID(id);
            if (model == null) return HttpNotFound();

            if (model.TrangThai == 3)
                return Content("<div class='alert alert-danger'>Phiếu đã hủy, không thể điều chỉnh.</div>");

            if (model.TrangThai != 2)
                return Content("<div class='alert alert-warning'>Chỉ có thể điều chỉnh phiếu đã ghi sổ.</div>");

            ViewBag.Title = "Cập nhật";
            ViewBag.IsAdjustAllocation = true;
            PopulateFormDropdowns(model.IDNhaCungCap, model.IDPhieuNhap);
            return PartialView("_Form", model);
        }

        [HttpPost]
        public ActionResult AdjustAllocation(PhieuChiViewModel model, string chiTietJson)
        {
            try
            {
                if (!PermissionHelper.HasPermission("PhieuChi", LoaiPhanQuyen.CapNhat))
                    return Json(new { success = false, message = "Không có quyền thực hiện" });

                var oldModel = _repo.GetByID(model.ID);
                if (oldModel == null)
                    return Json(new { success = false, message = "Phiếu chi không tồn tại" });

                if (oldModel.TrangThai == 3)
                    return Json(new { success = false, message = "Phiếu chi đã hủy, không thể điều chỉnh." });

                var user = GetCurrentUser();
                int userId = user?.IDNhanSu ?? 0;

                var newChiTiets = new List<PhieuChiChiTietViewModel>();
                if (!string.IsNullOrEmpty(chiTietJson))
                {
                    newChiTiets = Newtonsoft.Json.JsonConvert.DeserializeObject<List<PhieuChiChiTietViewModel>>(chiTietJson);
                }

                _repo.DieuChinhPhanBo(model, newChiTiets, userId);

                AuditLog.AddUpdate("KT_PhieuChi", model.ID.ToString(), null, model);

                return Json(new { success = true, message = "Cập nhật thành công." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // POST: /phieu-chi/save
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Save(PhieuChiViewModel model, string chiTietJson, bool ghiSo = false)
        {
            bool hasThem = PermissionHelper.HasPermission("PhieuChi", LoaiPhanQuyen.Them);
            bool hasSua  = PermissionHelper.HasPermission("PhieuChi", LoaiPhanQuyen.CapNhat);

            if (model.ID == 0 && !hasThem)
                return Json(new { success = false, message = "Không có quyền thêm mới" });
            if (model.ID > 0 && !hasSua)
                return Json(new { success = false, message = "Không có quyền cập nhật" });

            if (!ModelState.IsValid)
            {
                PopulateFormDropdowns(model.IDNhaCungCap, model.IDPhieuNhap);
                return PartialView("_Form", model);
            }

            if (!string.IsNullOrEmpty(chiTietJson))
            {
                try {
                    model.ChiTiets = Newtonsoft.Json.JsonConvert.DeserializeObject<List<PhieuChiChiTietViewModel>>(chiTietJson);
                } catch {
                    model.ChiTiets = new List<PhieuChiChiTietViewModel>();
                }
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

                string msg = ghiSo ? "Lưu và ghi thành công" : "Lưu thành công";
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
                    return Json(new { success = false, message = "Không thể xóa phiếu đã ghi." });

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

        // GET AJAX: /phieu-chi/get-phieu-nhap-cong-no?idNhaCungCap=x
        [HttpGet]
        public ActionResult GetPhieuNhapCongNo(int idNhaCungCap)
        {
            try
            {
                var list = _repo.GetPhieuNhapCongNo(idNhaCungCap)
                    .Select(x => new {
                        id = (int)x.ID,
                        soPhieuNhap = (string)x.SoPhieuNhap,
                        ngayNhap = ((DateTime)x.NgayNhap).ToString("dd/MM/yyyy"),
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

        // GET AJAX: /phieu-chi/get-tien-tra-truoc?idNhaCungCap=x
        [HttpGet]
        public ActionResult GetTienTraTruocNhaCungCap(int idNhaCungCap)
        {
            try
            {
                var tien = _repo.GetTienTraTruocNhaCungCap(idNhaCungCap);
                return Json(new { success = true, data = tien }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public ActionResult GetFiles(int idPhieuChi)
        {
            if (!PermissionHelper.HasPermission("PhieuChi", LoaiPhanQuyen.Xem))
                return Content("<div class='alert alert-danger'>KhÃ´ng cÃ³ quyá»n xem file Ä‘Ã­nh kÃ¨m</div>");

            try
            {
                ViewBag.CanDeleteFile = PermissionHelper.HasPermission("PhieuChi", LoaiPhanQuyen.Xoa);
                var files = _repo.File_GetList(idPhieuChi);
                return PartialView("_PhieuChiFileList", files);
            }
            catch (Exception ex)
            {
                return Content("Lá»—i táº£i danh sÃ¡ch file: " + ex.Message);
            }
        }

        [HttpPost]
        public ActionResult UploadFile(int idPhieuChi, HttpPostedFileBase file)
        {
            if (!PermissionHelper.HasPermission("PhieuChi", LoaiPhanQuyen.CapNhat))
                return Json(new { success = false, message = "KhÃ´ng cÃ³ quyá»n Ä‘Ã­nh kÃ¨m file" });

            try
            {
                if (file == null || file.ContentLength == 0)
                    return Json(new { success = false, message = "Vui lÃ²ng chá»n file há»£p lá»‡." });

                var phieuChi = _repo.GetByID(idPhieuChi);
                if (phieuChi == null)
                    return Json(new { success = false, message = "Phiáº¿u chi khÃ´ng tá»“n táº¡i." });

                string ext = Path.GetExtension(file.FileName).ToLower();
                var allowedExts = new[] { ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".jpg", ".jpeg", ".png" };
                if (!allowedExts.Contains(ext))
                    return Json(new { success = false, message = "Äá»‹nh dáº¡ng file khÃ´ng Ä‘Æ°á»£c há»— trá»£." });

                byte[] fileData;
                using (var binaryReader = new BinaryReader(file.InputStream))
                {
                    fileData = binaryReader.ReadBytes(file.ContentLength);
                }

                var model = new PhieuChiFile
                {
                    IDPhieuChi = idPhieuChi,
                    TenFile = Path.GetFileName(file.FileName),
                    LoaiFile = ext.Replace(".", ""),
                    DungLuong = file.ContentLength,
                    NoiDungFile = fileData
                };

                var user = GetCurrentUser();
                _repo.File_Save(model, user?.IDNhanSu ?? 0);

                return Json(new { success = true, message = "Táº£i file lÃªn thÃ nh cÃ´ng." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lá»—i táº£i file: " + ex.Message });
            }
        }

        [HttpPost]
        public ActionResult DeleteFile(int id)
        {
            if (!PermissionHelper.HasPermission("PhieuChi", LoaiPhanQuyen.Xoa))
                return Json(new { success = false, message = "KhÃ´ng cÃ³ quyá»n xÃ³a file" });

            try
            {
                var user = GetCurrentUser();
                _repo.File_Delete(id, user?.IDNhanSu ?? 0);
                return Json(new { success = true, message = "XÃ³a file thÃ nh cÃ´ng." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lá»—i khi xÃ³a file: " + ex.Message });
            }
        }

        [HttpGet]
        public ActionResult DownloadFile(int id)
        {
            if (!PermissionHelper.HasPermission("PhieuChi", LoaiPhanQuyen.Xem))
                return HttpNotFound("KhÃ´ng cÃ³ quyá»n táº£i file.");

            try
            {
                var file = _repo.File_GetByID(id);
                if (file == null) return HttpNotFound("File khÃ´ng tá»“n táº¡i.");

                return File(file.NoiDungFile, GetMimeType(file.LoaiFile), file.TenFile);
            }
            catch (Exception)
            {
                return HttpNotFound("CÃ³ lá»—i xáº£y ra khi láº¥y file.");
            }
        }

        [HttpGet]
        public ActionResult ViewFile(int id)
        {
            if (!PermissionHelper.HasPermission("PhieuChi", LoaiPhanQuyen.Xem))
                return HttpNotFound("KhÃ´ng cÃ³ quyá»n xem file.");

            try
            {
                var file = _repo.File_GetByID(id);
                if (file == null) return HttpNotFound("File khÃ´ng tá»“n táº¡i.");

                var ext = (file.LoaiFile ?? "").ToLower();
                if (ext != "pdf" && ext != "jpg" && ext != "jpeg" && ext != "png")
                    return File(file.NoiDungFile, "application/octet-stream", file.TenFile);

                return File(file.NoiDungFile, GetMimeType(file.LoaiFile));
            }
            catch (Exception)
            {
                return HttpNotFound("CÃ³ lá»—i xáº£y ra khi láº¥y file.");
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

        private void PopulateFilterDropdowns()
        {
            var khoanMucs = _repo.GetKhoanMucDropdown()
                .Select(x => new SelectListItem { Value = ((int)x.ID).ToString(), Text = (string)x.TenHienThi });
            ViewBag.KhoanMucList = new SelectList(khoanMucs.ToList(), "Value", "Text");

            var nccs = _repo.GetNhaCungCapDropdown()
                .Select(x => new SelectListItem { Value = ((int)x.ID).ToString(), Text = (string)x.TenHienThi });
            ViewBag.NhaCungCapList = new SelectList(nccs.ToList(), "Value", "Text");

            var taiKhoans = _repo.GetTaiKhoanDropdown()
                .Select(x => new SelectListItem { Value = ((int)x.ID).ToString(), Text = (string)x.TenHienThi });
            ViewBag.TaiKhoanList = new SelectList(taiKhoans.ToList(), "Value", "Text");
        }

        private void PopulateFormDropdowns(int? idNhaCungCap = null, int? currentPhieuNhapId = null)
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

            var phieuNhaps = _repo.GetPhieuNhapDropdown(idNhaCungCap, currentPhieuNhapId)
                .Select(x => new SelectListItem { Value = ((int)x.ID).ToString(), Text = (string)x.TenHienThi });
            ViewBag.PhieuNhapList = new SelectList(phieuNhaps.ToList(), "Value", "Text");
        }
        [HttpGet]
        public ActionResult DebugSchema()
        {
            var connStr = SalesManagementSystem.Helpers.Security.ConfigManager.GetConnectionString("DefaultConnection");
            var result = new List<string>();
            try 
            {
                using (var conn = new System.Data.SqlClient.SqlConnection(connStr))
                {
                    conn.Open();
                    using (var cmd = new System.Data.SqlClient.SqlCommand("SELECT COLUMN_NAME, DATA_TYPE FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'KT_PhieuChiChiTiet'", conn))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            result.Add(reader["COLUMN_NAME"] + " : " + reader["DATA_TYPE"]);
                        }
                    }
                    
                    // also select top 5 rows
                    result.Add("--- TOP 5 ROWS ---");
                    using (var cmd = new System.Data.SqlClient.SqlCommand("SELECT TOP 5 * FROM KT_PhieuChiChiTiet ORDER BY ID DESC", conn))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var row = "";
                            for(int i = 0; i < reader.FieldCount; i++) row += reader.GetName(i) + "=" + reader[i] + ", ";
                            result.Add(row);
                        }
                    }
                }
            } 
            catch(Exception ex)
            {
                result.Add("ERROR: " + ex.Message);
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }
    }
}
