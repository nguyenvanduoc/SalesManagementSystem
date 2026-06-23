using System;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Collections.Generic;
using SalesManagementSystem.Models.Entities;
using SalesManagementSystem.Models.ViewModels;
using SalesManagementSystem.Repositories.Interfaces;
using SalesManagementSystem.Helpers;

namespace SalesManagementSystem.Controllers
{
    [CustomAuthorize(AuthorizeTypes.AuthorizedUsers)]
    public class HopDongKhachHangController : BaseController
    {
        private readonly IHopDongKhachHangRepository _repo;
        private readonly IKhachHangRepository _khachHangRepo; // Assuming this exists to get customers for dropdowns

        public HopDongKhachHangController(
            IHopDongKhachHangRepository repo,
            IKhachHangRepository khachHangRepo)
        {
            _repo = repo;
            _khachHangRepo = khachHangRepo;
        }

        // GET: HopDongKhachHang
        public ActionResult Index()
        {
            ViewBag.KhachHangList = _khachHangRepo.GetAll();
            return View();
        }

        [HttpPost]
        public ActionResult GetList(DateTime? tuNgay, DateTime? denNgay, string soHopDong, string tenHopDong, int? idKhachHang, int? trangThai, bool chiHienThiSapHetHan = false, int pageNumber = 1, int pageSize = 20)
        {
            try
            {
                int totalRecords, tongHopDong, dangHieuLuc, sapHetHan, daThanhLy;
                var data = _repo.GetList(
                    tuNgay, denNgay, soHopDong, tenHopDong, idKhachHang, trangThai, chiHienThiSapHetHan, 
                    pageNumber, pageSize, 
                    out totalRecords, out tongHopDong, out dangHieuLuc, out sapHetHan, out daThanhLy);

                var vm = new HopDongKhachHangListVM
                {
                    DanhSachHopDong = data,
                    TongSoBanGhi = totalRecords,
                    TongSoTrang = (int)Math.Ceiling((double)totalRecords / pageSize),
                    TrangHienTai = pageNumber
                };

                ViewBag.Dashboard = new HopDongDashboardVM
                {
                    TongHopDong = tongHopDong,
                    DangHieuLuc = dangHieuLuc,
                    SapHetHan = sapHetHan,
                    DaThanhLy = daThanhLy
                };
                ViewBag.PageSize = pageSize;

                return PartialView("_HopDongList", vm);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi khi tải dữ liệu: " + ex.Message });
            }
        }

        [CustomAuthorize(AuthorizeTypes.AuthorizedUsers)]
        public ActionResult Form(int id = 0)
        {
            ViewBag.KhachHangList = _khachHangRepo.GetAll();
            
            HopDongKhachHang model = new HopDongKhachHang();
            if (id > 0)
            {
                model = _repo.GetByID(id);
                if (model == null) return HttpNotFound();
            }
            else
            {
                model.NgayKy = DateTime.Today;
                model.TuNgay = DateTime.Today;
                model.DenNgay = DateTime.Today.AddYears(1); 
            }

            return PartialView("Form", model);
        }

        [HttpPost]
        [ValidateInput(false)]
        [CustomAuthorize(AuthorizeTypes.MustHavePermission)]
        public ActionResult Save(HopDongKhachHang model, IEnumerable<HttpPostedFileBase> uploadedFiles)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(model.SoHopDong))
                    return Json(new { success = false, message = "Số hợp đồng không được để trống." });

                if (model.IDKhachHang <= 0)
                    return Json(new { success = false, message = "Vui lòng chọn khách hàng." });

                if (!model.NgayKy.HasValue)
                    return Json(new { success = false, message = "Ngày ký không được để trống." });

                if (model.TuNgay.HasValue && model.DenNgay.HasValue && model.TuNgay > model.DenNgay)
                    return Json(new { success = false, message = "Từ ngày không được lớn hơn Đến ngày." });

                if (model.GiaTriHopDong < 0)
                    return Json(new { success = false, message = "Giá trị hợp đồng không hợp lệ." });

                // Check duplicate
                if (_repo.CheckDuplicate(model.ID, model.SoHopDong))
                    return Json(new { success = false, message = "Số hợp đồng đã tồn tại trong hệ thống." });

                int currentUserId = GetCurrentUser()?.UserID ?? 0;
                int savedId = _repo.Save(model, currentUserId);

                if (uploadedFiles != null)
                {
                    var allowedExts = new[] { ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".jpg", ".jpeg", ".png" };
                    foreach (var file in uploadedFiles)
                    {
                        if (file != null && file.ContentLength > 0)
                        {
                            string ext = System.IO.Path.GetExtension(file.FileName).ToLower();
                            if (allowedExts.Contains(ext))
                            {
                                byte[] fileData = null;
                                using (var binaryReader = new System.IO.BinaryReader(file.InputStream))
                                {
                                    fileData = binaryReader.ReadBytes(file.ContentLength);
                                }

                                var fileModel = new HopDongKhachHangFile
                                {
                                    IDHopDong = savedId,
                                    TenFile = System.IO.Path.GetFileName(file.FileName),
                                    LoaiFile = ext.Replace(".", ""),
                                    DungLuong = file.ContentLength,
                                    NoiDungFile = fileData
                                };

                                _repo.File_Save(fileModel, currentUserId);
                            }
                        }
                    }
                }

                return Json(new { success = true, message = "Lưu hợp đồng thành công.", id = savedId });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi khi lưu dữ liệu: " + ex.Message });
            }
        }

        [HttpPost]
        [CustomAuthorize(AuthorizeTypes.MustHavePermission)]
        public ActionResult ThanhLy(int id)
        {
            try
            {
                int currentUserId = GetCurrentUser()?.UserID ?? 0;
                _repo.ThanhLy(id, currentUserId);
                return Json(new { success = true, message = "Thanh lý hợp đồng thành công." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi khi thanh lý: " + ex.Message });
            }
        }

        [HttpPost]
        [CustomAuthorize(AuthorizeTypes.MustHavePermission)]
        public ActionResult Huy(int id)
        {
            try
            {
                int currentUserId = GetCurrentUser()?.UserID ?? 0;
                _repo.Huy(id, currentUserId);
                return Json(new { success = true, message = "Hủy hợp đồng thành công." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi khi hủy: " + ex.Message });
            }
        }

        [HttpPost]
        [CustomAuthorize(AuthorizeTypes.MustHavePermission)]
        public ActionResult Delete(int id)
        {
            try
            {
                int currentUserId = GetCurrentUser()?.UserID ?? 0;
                _repo.Delete(id, currentUserId);
                return Json(new { success = true, message = "Xóa hợp đồng thành công." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi khi xóa: " + ex.Message });
            }
        }

        // --- FILE METHODS ---

        public ActionResult GetFiles(int idHopDong)
        {
            try
            {
                var files = _repo.File_GetList(idHopDong);
                return PartialView("_FileList", files);
            }
            catch (Exception ex)
            {
                return Content("Lỗi tải danh sách file: " + ex.Message);
            }
        }

        [HttpPost]
        [CustomAuthorize(AuthorizeTypes.AuthorizedUsers)]
        public ActionResult UploadFile(int idHopDong, HttpPostedFileBase file)
        {
            try
            {
                if (file == null || file.ContentLength == 0)
                    return Json(new { success = false, message = "Vui lòng chọn file hợp lệ." });

                string ext = Path.GetExtension(file.FileName).ToLower();
                var allowedExts = new[] { ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".jpg", ".png" };
                if (!allowedExts.Contains(ext))
                    return Json(new { success = false, message = "Định dạng file không được hỗ trợ." });

                byte[] fileData = null;
                using (var binaryReader = new BinaryReader(file.InputStream))
                {
                    fileData = binaryReader.ReadBytes(file.ContentLength);
                }

                var model = new HopDongKhachHangFile
                {
                    IDHopDong = idHopDong,
                    TenFile = Path.GetFileName(file.FileName),
                    LoaiFile = ext.Replace(".", ""),
                    DungLuong = file.ContentLength,
                    NoiDungFile = fileData
                };

                int currentUserId = GetCurrentUser()?.UserID ?? 0;
                _repo.File_Save(model, currentUserId);

                return Json(new { success = true, message = "Tải file lên thành công." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi tải file: " + ex.Message });
            }
        }

        [HttpPost]
        [CustomAuthorize(AuthorizeTypes.AuthorizedUsers)]
        public ActionResult DeleteFile(int id)
        {
            try
            {
                int currentUserId = GetCurrentUser()?.UserID ?? 0;
                _repo.File_Delete(id, currentUserId);
                return Json(new { success = true, message = "Xóa file thành công." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi khi xóa file: " + ex.Message });
            }
        }

        public ActionResult DownloadFile(int id)
        {
            try
            {
                var file = _repo.File_GetByID(id);
                if (file == null) return HttpNotFound("File không tồn tại.");

                string mimeType = "application/octet-stream";
                switch (file.LoaiFile.ToLower())
                {
                    case "pdf": mimeType = "application/pdf"; break;
                    case "doc": mimeType = "application/msword"; break;
                    case "docx": mimeType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document"; break;
                    case "xls": mimeType = "application/vnd.ms-excel"; break;
                    case "xlsx": mimeType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"; break;
                    case "jpg":
                    case "jpeg": mimeType = "image/jpeg"; break;
                    case "png": mimeType = "image/png"; break;
                }

                return File(file.NoiDungFile, mimeType, file.TenFile);
            }
            catch (Exception)
            {
                return HttpNotFound("Có lỗi xảy ra khi lấy file.");
            }
        }

        public ActionResult ViewFile(int id)
        {
            try
            {
                var file = _repo.File_GetByID(id);
                if (file == null) return HttpNotFound("File không tồn tại.");

                string mimeType = "application/octet-stream";
                switch (file.LoaiFile.ToLower())
                {
                    case "pdf": mimeType = "application/pdf"; break;
                    case "jpg":
                    case "jpeg": mimeType = "image/jpeg"; break;
                    case "png": mimeType = "image/png"; break;
                    default: return File(file.NoiDungFile, "application/octet-stream", file.TenFile); // force download if not viewable
                }

                // Return without filename to display inline in browser instead of downloading
                return File(file.NoiDungFile, mimeType);
            }
            catch (Exception)
            {
                return HttpNotFound("Có lỗi xảy ra khi lấy file.");
            }
        }
    }
}
