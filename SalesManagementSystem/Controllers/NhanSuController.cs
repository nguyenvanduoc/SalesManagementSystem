using System;
using System.Collections.Generic;
using System.Web.Mvc;
using SalesManagementSystem.Models.Entities;
using SalesManagementSystem.Repositories;
using SalesManagementSystem.Repositories.Interfaces;
using SalesManagementSystem.Helpers;
using SalesManagementSystem.Models.ViewModels;

namespace SalesManagementSystem.Controllers
{
    public class NhanSuController : BaseController
    {
        private readonly INhanSuRepository _employeeRepo;
        private readonly SalesManagementSystem.Services.Interfaces.IExcelExportService _excelExportService;
        private readonly SalesManagementSystem.Repositories.Interfaces.IChucVuRepository _chucVuRepo;
        private readonly SalesManagementSystem.Repositories.Interfaces.IPhongBanRepository _phongBanRepo;
        private readonly SalesManagementSystem.Services.Interfaces.IWordExportService _wordExportService;

        public NhanSuController(INhanSuRepository employeeRepo, 
            SalesManagementSystem.Services.Interfaces.IExcelExportService excelExportService,
            SalesManagementSystem.Repositories.Interfaces.IChucVuRepository chucVuRepo,
            SalesManagementSystem.Repositories.Interfaces.IPhongBanRepository phongBanRepo,
            SalesManagementSystem.Services.Interfaces.IWordExportService wordExportService)
        {
            _employeeRepo = employeeRepo;
            _excelExportService = excelExportService;
            _chucVuRepo = chucVuRepo;
            _phongBanRepo = phongBanRepo;
            _wordExportService = wordExportService;
        }

        private void SetViewBags()
        {
            ViewBag.ChucVus = new System.Web.Mvc.SelectList(_chucVuRepo.GetAll(), "ID", "TenChucVu");
            ViewBag.PhongBans = new System.Web.Mvc.SelectList(_phongBanRepo.GetAll(), "ID", "TenPhongBan");
        }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult GetAvatar(int id)
        {
            var emp = _employeeRepo.GetById(id);
            if (emp != null && emp.HinhAnh != null && emp.HinhAnh.Length > 0)
            {
                return File(emp.HinhAnh, "image/jpeg");
            }
            return File("~/Content/IMG/default-avatar.svg", "image/svg+xml");
        }

        // GET: Employee
        public ActionResult Index(int page = 1, int pageSize = 10, string keyword = "", bool? gender = null)
        {
            int totalRecords;
            var NhanSus = _employeeRepo.GetPaged(page, pageSize, keyword, gender, out totalRecords);

            var model = new PagedListViewModel<NhanSu>
            {
                Items = NhanSus,
                CurrentPage = page,
                PageSize = pageSize,
                TotalRecords = totalRecords,
                Keyword = keyword,
                ActionName = "Index"
            };

            ViewBag.Keyword = keyword;

            if (Request.IsAjaxRequest())
            {
                return PartialView("_NhanSuList", model);
            }

            return View(model);
        }

        // GET: Employee/Create
        [CustomAuthorize(AuthorizeTypes.MustHavePermission)]
        public ActionResult Create()
        {
            SetViewBags();
            return PartialView(new NhanSu());
        }

        // POST: Employee/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [CustomAuthorize(AuthorizeTypes.MustHavePermission)]
        public ActionResult Create([Bind(Exclude = "ID,NgayTao,NguoiTao,NgayCapNhat,NguoiCapNhat")] NhanSu employee, System.Web.HttpPostedFileBase avatarFile)
        {
            if (ModelState.IsValid)
            {
                if (_employeeRepo.IsDuplicateCode(employee.MaNhanSu))
                {
                    ModelState.AddModelError("MaNhanSu", "MÃ£ nhÃ¢n sá»± Ä‘Ã£ tá»“n táº¡i trong há»‡ thá»‘ng.");
                    SetViewBags();
                    return PartialView(employee);
                }

                var session = (SalesManagementSystem.Models.ViewModels.UserLoginViewModel)Session[SalesManagementSystem.Helpers.CommonConstants.USER_SESSION];
                employee.NguoiTao = session?.IDNhanSu ?? 0;

                if (avatarFile != null && avatarFile.ContentLength > 0)
                {
                    using (var ms = new System.IO.MemoryStream())
                    {
                        avatarFile.InputStream.CopyTo(ms);
                        employee.HinhAnh = ms.ToArray();
                    }
                }

                _employeeRepo.Insert(employee);

                // AUDIT LOG
                AuditLog.AddInsert("NS_NhanSu", employee.ID.ToString(), employee);

                return Json(new { success = true, message = "ThÃªm má»›i nhÃ¢n sá»± thÃ nh cÃ´ng!" });
            }
            SetViewBags();
            return PartialView(employee);
        }

        // GET: Employee/Update/5
        [CustomAuthorize(AuthorizeTypes.MustHavePermission)]
        public ActionResult Update(int id)
        {
            var employee = _employeeRepo.GetById(id);
            if (employee == null)
            {
                return HttpNotFound();
            }
            SetViewBags();
            return PartialView(employee);
        }

        // POST: Employee/Update/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [CustomAuthorize(AuthorizeTypes.MustHavePermission)]
        public ActionResult Update([Bind(Exclude = "NgayTao,NguoiTao,NgayCapNhat,NguoiCapNhat")] NhanSu employee, System.Web.HttpPostedFileBase avatarFile)
        {
            if (ModelState.IsValid)
            {
                if (_employeeRepo.IsDuplicateCode(employee.MaNhanSu, employee.ID))
                {
                    ModelState.AddModelError("MaNhanSu", "MÃ£ nhÃ¢n sá»± Ä‘Ã£ tá»“n táº¡i trong há»‡ thá»‘ng.");
                    SetViewBags();
                    return PartialView(employee);
                }

                // FETCH OLD OBJ FOR AUDIT
                var oldEmployee = _employeeRepo.GetById(employee.ID);

                if (avatarFile != null && avatarFile.ContentLength > 0)
                {
                    using (var ms = new System.IO.MemoryStream())
                    {
                        avatarFile.InputStream.CopyTo(ms);
                        employee.HinhAnh = ms.ToArray();
                    }
                }
                else
                {
                    employee.HinhAnh = oldEmployee.HinhAnh;
                }

                var session = (SalesManagementSystem.Models.ViewModels.UserLoginViewModel)Session[SalesManagementSystem.Helpers.CommonConstants.USER_SESSION];
                employee.NguoiCapNhat = session?.IDNhanSu ?? 0;
                _employeeRepo.Update(employee);

                // AUDIT LOG
                AuditLog.AddUpdate("NS_NhanSu", employee.ID.ToString(), oldEmployee, employee);

                return Json(new { success = true, message = "Cáº­p nháº­t nhÃ¢n sá»± thÃ nh cÃ´ng!" });
            }
            SetViewBags();
            return PartialView(employee);
        }

        // POST: Employee/Delete/5
        [HttpPost]
        [CustomAuthorize(AuthorizeTypes.MustHavePermission)]
        public ActionResult Delete(int id)
        {
            var oldEmployee = _employeeRepo.GetById(id);
            if (oldEmployee != null)
                AuditLog.AddDelete("NS_NhanSu", id.ToString(), oldEmployee);
            
            ForceSaveAudit();
            _employeeRepo.Delete(id);

            return Json(new { success = true, message = "XÃ³a dá»¯ liá»‡u thÃ nh cÃ´ng" });
        }

        // POST: Employee/BatchDelete
        [HttpPost]
        [CustomAuthorize(AuthorizeTypes.MustHavePermission)]
        public ActionResult BatchDelete(int[] ids)
        {
            if (ids != null && ids.Length > 0)
            {
                foreach (var id in ids)
                {
                    var oldEmployee = _employeeRepo.GetById(id);
                    if (oldEmployee != null)
                        AuditLog.AddDelete("NS_NhanSu", id.ToString(), oldEmployee);
                    
                    ForceSaveAudit();
                    _employeeRepo.Delete(id);
                }
            }
            return Json(new { success = true, message = "XÃ³a dá»¯ liá»‡u thÃ nh cÃ´ng" });
        }
        // GET: Employee/ExportExcel
        public ActionResult ExportExcel()
        {
            try
            {
                // 1. Láº¥y dá»¯ liá»‡u (khÃ´ng phÃ¢n trang hoáº·c láº¥y táº¥t cáº£ tuá»³ nghiá»‡p vá»¥)
                int total;
                var data = _employeeRepo.GetPaged(1, 10000, "", null, out total);

                var session = (SalesManagementSystem.Models.ViewModels.UserLoginViewModel)Session[SalesManagementSystem.Helpers.CommonConstants.USER_SESSION];
                string nguoiLapBieu = session != null ? (session.HoDem + " " + session.Ten).Trim() : "";
                if (string.IsNullOrEmpty(nguoiLapBieu)) nguoiLapBieu = session?.UserName ?? "";

                // 2. Chuáº©n bá»‹ biáº¿n Ä‘Æ¡n
                var variables = new System.Collections.Generic.Dictionary<string, object>
                {
                    { "Ngay", DateTime.Now.ToString("dd") },
                    { "Thang", DateTime.Now.ToString("MM") },
                    { "Nam", DateTime.Now.ToString("yyyy") },
                    { "NguoiLapBieu", nguoiLapBieu }
                };

                // Chuáº©n bá»‹ dá»¯ liá»‡u danh sÃ¡ch khá»›p vá»›i cÃ¡c cá»™t trong máº«u Excel (HoTen, P_NgaySinh, P_SoDienThoai)
                var exportData = System.Linq.Enumerable.Select(data, x => new {
                    MaNhanSu = x.MaNhanSu,
                    HoTen = (x.HoDem + " " + x.Ten).Trim(),
                    P_NgaySinh = x.NgaySinh,
                    P_SoDienThoai = x.SoDienThoai,
                    Email = x.Email
                });

                // 3. Xuáº¥t file báº±ng Service chung
                string fileExtension;
                var fileBytes = _excelExportService.Export(BieuMauConstants.DS_NHAN_SU, exportData, out fileExtension, variables);

                string contentType = fileExtension == "xls" 
                    ? "application/vnd.ms-excel" 
                    : "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

                return File(fileBytes, contentType, $"DanhSachNhanSu_{DateTime.Now:yyyyMMddHHmmss}.{fileExtension}");
            }
            catch (Exception ex)
            {
                // Xá»­ lÃ½ lá»—i náº¿u khÃ´ng tÃ¬m tháº¥y máº«u hoáº·c lá»—i xuáº¥t file
                TempData["ToastMessage"] = "Lá»—i xuáº¥t Excel: " + ex.Message;
                TempData["ToastType"] = "error";
                return RedirectToAction("Index");
            }
        }

        [HttpGet]
        public ActionResult ExportExcelNS02()
        {
            try
            {
                var data = _employeeRepo.GetAllWithChucVu();

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

                var exportData = System.Linq.Enumerable.Select(data, x => new {
                    TenChucVu = string.IsNullOrEmpty(x.TenChucVu) ? "ChÆ°a cÃ³ chá»©c vá»¥" : x.TenChucVu,
                    MaNhanSu = x.MaNhanSu,
                    HoTen = (x.HoDem + " " + x.Ten).Trim(),
                    P_NgaySinh = x.NgaySinh,
                    P_SoDienThoai = x.SoDienThoai,
                    Email = x.Email,
                    LuongCoBan = x.LuongCoBan
                });

                var groupedData = System.Linq.Enumerable.GroupBy(exportData, x => x.TenChucVu);

                string fileExtension;
                var fileBytes = _excelExportService.ExportGrouped(BieuMauConstants.DS_NHAN_SU_THEO_CHUC_VU, groupedData, out fileExtension, variables);

                string contentType = fileExtension == "xls" 
                    ? "application/vnd.ms-excel" 
                    : "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

                return File(fileBytes, contentType, $"DanhSachNhanSuTheoChucVu_{DateTime.Now:yyyyMMddHHmmss}.{fileExtension}");
            }
            catch (Exception ex)
            {
                TempData["ToastType"] = "error";
                TempData["ToastMessage"] = "Lá»—i xuáº¥t file: " + ex.Message;
                return RedirectToAction("Index");
            }
        }

        [HttpGet]
        public ActionResult ExportQuyetDinhNhanSu(int id)
        {
            var emp = _employeeRepo.GetById(id);
            if (emp == null) return HttpNotFound();

            // Láº¥y thÃªm thÃ´ng tin phÃ²ng ban, chá»©c vá»¥ náº¿u cáº§n (tÃ¹y theo schema)
            // Giáº£ láº­p biáº¿n dá»¯ liá»‡u cho máº«u Quyáº¿t Äá»‹nh
            var exportData = new
            {
                HoTen = emp.HoDem + " " + emp.Ten,
                NgaySinh = emp.NgaySinh?.ToString("dd/MM/yyyy") ?? "",
                GioiTinh = (emp.GioiTinh == true) ? "Nam" : "Ná»¯",
                SoCMND = emp.SoCMND ?? "",
                NgayCap = emp.NgayCap?.ToString("dd/MM/yyyy") ?? "",
                DiaChi = emp.DiaChi ?? "",
                SoDienThoai = emp.SoDienThoai ?? "",
                // ThÃªm cÃ¡c biáº¿n khÃ¡c tÃ¹y vÃ o máº«u word @TenChucVu, @TenPhongBan...
            };

            // Giáº£ láº­p danh sÃ¡ch Ä‘á»ƒ láº·p báº£ng trong Word #DanhSachPhuCap
            var tables = new Dictionary<string, object>
            {
                {
                    "DanhSachPhuCap", new List<object>
                    {
                        new { STT = 1, TenPhuCap = "Phá»¥ cáº¥p Äƒn trÆ°a", SoTien = 500000 },
                        new { STT = 2, TenPhuCap = "Phá»¥ cáº¥p xÄƒng xe", SoTien = 300000 }
                    }
                }
            };

            // Gá»i service xuáº¥t file Word. Táº¡m thá»i isPdf = false vÃ¬ thÆ° viá»‡n OpenSource ko xuáº¥t trá»±c tiáº¿p Ä‘Æ°á»£c PDF
            var result = _wordExportService.ExportWord(BieuMauConstants.QUYET_DINH_NHAN_SU, exportData, tables, isPdf: false);

            if (result.Success)
            {
                return File(result.FileBytes, result.ContentType, result.FileName);
            }

            TempData["ErrorMessage"] = result.Message;
            return RedirectToAction("Index");
        }
    }
}
