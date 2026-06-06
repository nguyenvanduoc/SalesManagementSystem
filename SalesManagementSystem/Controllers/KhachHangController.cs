using System;
using System.Linq;
using System.Web.Mvc;
using Dapper;
using SalesManagementSystem.Models.Entities;
using SalesManagementSystem.Repositories.Interfaces;
using SalesManagementSystem.Helpers;
using SalesManagementSystem.Models.ViewModels;
using SalesManagementSystem.Services.Interfaces;

namespace SalesManagementSystem.Controllers
{
    public class KhachHangController : BaseController
    {
        private readonly IKhachHangRepository _khachHangRepo;
        private readonly IExcelExportService _excelExportService;
        private readonly SalesManagementSystem.Data.DbConnectionFactory _db;

        public KhachHangController(IKhachHangRepository khachHangRepo, IExcelExportService excelExportService, SalesManagementSystem.Data.DbConnectionFactory db)
        {
            _khachHangRepo = khachHangRepo;
            _excelExportService = excelExportService;
            _db = db;
        }

        private class DropdownItem
        {
            public int ID { get; set; }
            public string Name { get; set; }
        }

        private void PopulateDropdowns()
        {
            using (var conn = _db.CreateConnection())
            {
                ViewBag.NhomKhachHangs = new SelectList(conn.Query<DropdownItem>("SELECT ID, TenNhomKhachHang as Name FROM NS_NhomKhachHang"), "ID", "Name");
                ViewBag.NhanViens = new SelectList(conn.Query<DropdownItem>("SELECT ID, HoTen as Name FROM NS_NhanVien"), "ID", "Name");
                ViewBag.TinhThanhs = new SelectList(conn.Query<DropdownItem>("SELECT ID, TenTinhThanh as Name FROM DM_TinhThanh WHERE DaXoa = 0 ORDER BY ThuTu"), "ID", "Name");
            }
        }

        // ==========================================
        // QUẢN LÝ KHÁCH HÀNG
        // ==========================================

        // GET: KhachHang/Index
        public ActionResult Index(int page = 1, int pageSize = 10, string keyword = "")
        {
            int totalRecords;
            var khachHangs = _khachHangRepo.GetPaged(page, pageSize, keyword, out totalRecords);

            var model = new PagedListViewModel<KhachHangViewModel>
            {
                Items = khachHangs,
                CurrentPage = page,
                PageSize = pageSize,
                TotalRecords = totalRecords,
                Keyword = keyword,
                ActionName = "GetList"
            };

            ViewBag.Keyword = keyword;

            return View(model);
        }

        // GET: KhachHang/GetList
        public ActionResult GetList(int page = 1, int pageSize = 10, string keyword = "")
        {
            int totalRecords;
            var khachHangs = _khachHangRepo.GetPaged(page, pageSize, keyword, out totalRecords);

            var model = new PagedListViewModel<KhachHangViewModel>
            {
                Items = khachHangs,
                CurrentPage = page,
                PageSize = pageSize,
                TotalRecords = totalRecords,
                Keyword = keyword,
                ActionName = "GetList"
            };

            ViewBag.Keyword = keyword;

            return PartialView("_KhachHangList", model);
        }

        // GET: KhachHang/Create
        [CustomAuthorize(AuthorizeTypes.MustHavePermission)]
        public ActionResult Create()
        {
            PopulateDropdowns();
            return PartialView("Create", new NS_KhachHang());
        }

        // POST: KhachHang/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [CustomAuthorize(AuthorizeTypes.MustHavePermission)]
        public ActionResult Create(NS_KhachHang khachHang)
        {
            if (ModelState.IsValid)
            {
                if (!string.IsNullOrEmpty(khachHang.MaKhachHang) && _khachHangRepo.IsDuplicateCode(khachHang.MaKhachHang))
                {
                    ModelState.AddModelError("MaKhachHang", "Mã khách hàng đã tồn tại trong hệ thống.");
                    PopulateDropdowns();
                    return PartialView("Create", khachHang);
                }

                var session = (SalesManagementSystem.Models.ViewModels.UserLoginViewModel)Session[SalesManagementSystem.Helpers.CommonConstants.USER_SESSION];
                khachHang.NguoiTao = session?.IDNhanSu ?? 0;
                _khachHangRepo.Insert(khachHang);

                AuditLog.AddInsert("NS_KhachHang", khachHang.ID.ToString(), khachHang);

                return Json(new { success = true, message = "Thêm mới khách hàng thành công!" });
            }
            PopulateDropdowns();
            return PartialView("Create", khachHang);
        }

        // GET: KhachHang/Edit/5
        [CustomAuthorize(AuthorizeTypes.MustHavePermission)]
        public ActionResult Edit(int id)
        {
            var khachHang = _khachHangRepo.GetById(id);
            if (khachHang == null)
            {
                return HttpNotFound();
            }
            PopulateDropdowns();
            return PartialView("Edit", khachHang);
        }

        // POST: KhachHang/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [CustomAuthorize(AuthorizeTypes.MustHavePermission)]
        public ActionResult Edit(NS_KhachHang khachHang)
        {
            if (ModelState.IsValid)
            {
                if (!string.IsNullOrEmpty(khachHang.MaKhachHang) && _khachHangRepo.IsDuplicateCode(khachHang.MaKhachHang, khachHang.ID))
                {
                    ModelState.AddModelError("MaKhachHang", "Mã khách hàng đã tồn tại trong hệ thống.");
                    PopulateDropdowns();
                    return PartialView("Edit", khachHang);
                }

                var oldObj = _khachHangRepo.GetById(khachHang.ID);

                var session = (SalesManagementSystem.Models.ViewModels.UserLoginViewModel)Session[SalesManagementSystem.Helpers.CommonConstants.USER_SESSION];
                khachHang.NguoiCapNhat = session?.IDNhanSu ?? 0;
                _khachHangRepo.Update(khachHang);

                AuditLog.AddUpdate("NS_KhachHang", khachHang.ID.ToString(), oldObj, khachHang);

                return Json(new { success = true, message = "Cập nhật khách hàng thành công!" });
            }
            PopulateDropdowns();
            return PartialView("Edit", khachHang);
        }

        // POST: KhachHang/Delete
        [HttpPost]
        [CustomAuthorize(AuthorizeTypes.MustHavePermission)]
        public ActionResult Delete(int? id, int[] ids)
        {
            if (id.HasValue)
            {
                var oldObj = _khachHangRepo.GetById(id.Value);
                if (oldObj != null)
                    AuditLog.AddDelete("NS_KhachHang", id.Value.ToString(), oldObj);
                
                ForceSaveAudit();
                _khachHangRepo.Delete(id.Value);
            }
            else if (ids != null && ids.Length > 0)
            {
                foreach (var item in ids)
                {
                    var oldObj = _khachHangRepo.GetById(item);
                    if (oldObj != null)
                        AuditLog.AddDelete("NS_KhachHang", item.ToString(), oldObj);
                    
                    ForceSaveAudit();
                    _khachHangRepo.Delete(item);
                }
            }
            return Json(new { success = true, message = "Xóa dữ liệu thành công" });
        }

        // GET: KhachHang/ExportExcel
        public ActionResult ExportExcel()
        {
            try
            {
                int total;
                var data = _khachHangRepo.GetPaged(1, 10000, "", out total);

                var session = (SalesManagementSystem.Models.ViewModels.UserLoginViewModel)Session[SalesManagementSystem.Helpers.CommonConstants.USER_SESSION];
                string nguoiLapBieu = session != null ? (session.HoDem + " " + session.Ten).Trim() : "";
                if (string.IsNullOrEmpty(nguoiLapBieu)) nguoiLapBieu = session?.UserName ?? "";

                var variables = new System.Collections.Generic.Dictionary<string, object>
                {
                    { "Ngay", DateTime.Now.ToString("dd") },
                    { "Thang", DateTime.Now.ToString("MM") },
                    { "Nam", DateTime.Now.ToString("yyyy") },
                    { "NguoiLapBieu", nguoiLapBieu }
                };

                // The place holders are: 
                // %KH01.STT", "%KH01.MaKhachHang", "%KH01.HoTenKhachHang", "%KH01.MaSoThue", "%KH01.TenNhomKhachHang", "%KH01.SoDienThoai", "%KH01.Email", "%KH01.TenTinhThanh", "%KH01.TenNhanVien
                // However, NPOI ExportExcelService automatically handles the prefix in mapping, we just provide the properties:
                int stt = 1;
                var exportData = data.Select(x => new {
                    STT = stt++,
                    MaKhachHang = x.MaKhachHang,
                    HoTenKhachHang = x.HoTenKhachHang,
                    MaSoThue = x.MaSoThue,
                    TenNhomKhachHang = x.TenNhomKhachHang,
                    SoDienThoai = x.SoDienThoai,
                    Email = x.Email,
                    TenTinhThanh = x.TenTinhThanh,
                    TenNhanVien = x.TenNhanVien
                });

                string fileExtension;
                var fileBytes = _excelExportService.Export("KH01", exportData, out fileExtension, variables);

                string contentType = fileExtension == "xls" 
                    ? "application/vnd.ms-excel" 
                    : "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

                return File(fileBytes, contentType, $"DanhSachKhachHang_{DateTime.Now:yyyyMMddHHmmss}.{fileExtension}");
            }
            catch (Exception ex)
            {
                TempData["ToastMessage"] = "Lỗi xuất Excel: " + ex.Message;
                TempData["ToastType"] = "error";
                return RedirectToAction("Index");
            }
        }
    }
}
