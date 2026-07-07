using SalesManagementSystem.Helpers;
using SalesManagementSystem.Helpers.Security;
using SalesManagementSystem.Models.Entities;
using SalesManagementSystem.Models.ViewModels;
using SalesManagementSystem.Repositories.Interfaces;
using System;
using System.Linq;
using System.Web.Mvc;
using Dapper;
using SalesManagementSystem.Data;

namespace SalesManagementSystem.Controllers
{
    public class PhieuXuatKhoController : BaseController
    {
        private readonly IPhieuXuatKhoRepository _repo;
        private readonly IDonDatHangRepository _donDatHangRepo;
        private readonly INhatKyChungRepository _nhatKyRepo;
        private readonly IDmKhoHangRepository _khoHangRepo;

        public PhieuXuatKhoController(
            IPhieuXuatKhoRepository repo,
            IDonDatHangRepository donDatHangRepo,
            INhatKyChungRepository nhatKyRepo,
            IDmKhoHangRepository khoHangRepo)
        {
            _repo = repo;
            _donDatHangRepo = donDatHangRepo;
            _nhatKyRepo = nhatKyRepo;
            _khoHangRepo = khoHangRepo;
        }

        public ActionResult Index(int page = 1, int pageSize = 20, string tuNgay = "", string denNgay = "", string soChungTu = "", int? idKho = null, int? trangThai = null, int? idNhanSuNhan = null)
        {
            if (!PermissionHelper.HasPermission("PhieuXuatKho", LoaiPhanQuyen.Xem)) return View("AccessDenied");

            int totalRecords;
            var list = _repo.GetList(page, pageSize, tuNgay, denNgay, soChungTu, idKho, trangThai, idNhanSuNhan, out totalRecords);

            var model = new PagedListViewModel<PhieuXuatKhoListViewModel>
            {
                Items = list,
                CurrentPage = page,
                PageSize = pageSize,
                TotalRecords = totalRecords,
                ActionName = "GetList",
                Keyword = soChungTu
            };

            int totalKhos;
            var khos = _khoHangRepo.GetPaged(1, 1000, "", out totalKhos).ToList();
            ViewBag.Khos = new SelectList(khos, "ID", "TenKhoHang", idKho);

            ViewBag.TuNgay = tuNgay;
            ViewBag.DenNgay = denNgay;
            ViewBag.SoChungTu = soChungTu;
            ViewBag.IDKho = idKho;
            ViewBag.TrangThai = trangThai;

            if (Request.IsAjaxRequest() || Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return PartialView("_PhieuXuatKhoList", model);

            return View("Index", model);
        }

        public ActionResult GetList(int page = 1, int pageSize = 20, string tuNgay = "", string denNgay = "", string soChungTu = "", int? idKho = null, int? trangThai = null, int? idNhanSuNhan = null)
        {
            if (!PermissionHelper.HasPermission("PhieuXuatKho", LoaiPhanQuyen.Xem)) return Content("<div class='alert alert-danger'>Không có quyền truy cập</div>");

            try
            {
                int totalRecords;
                var list = _repo.GetList(page, pageSize, tuNgay, denNgay, soChungTu, idKho, trangThai, idNhanSuNhan, out totalRecords);

                var model = new PagedListViewModel<PhieuXuatKhoListViewModel>
                {
                    Items = list,
                    CurrentPage = page,
                    PageSize = pageSize,
                    TotalRecords = totalRecords,
                    ActionName = "GetList",
                    Keyword = soChungTu
                };

                return PartialView("_PhieuXuatKhoList", model);
            }
            catch (Exception ex)
            {
                return Content("<div class='alert alert-danger'>Lỗi: " + ex.Message + "</div>");
            }
        }

        public ActionResult GetDonDatHangDaDuyet()
        {
            int totalRecords;
            var paged = _donDatHangRepo.GetPaged(1, 1000, "", "", null, null, 2, "", null, null, out totalRecords); // 2 = Đã duyệt
            return Json(new { data = paged }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetModalChonDon()
        {
            return PartialView("_ChonDonDatHangModal");
        }

        public ActionResult Create(int idDonDatHang)
        {
            return Content("<div class='alert alert-danger'>Màn hình Phiếu xuất kho chỉ để xem. Vui lòng lập chứng từ bán hàng.</div>");
        }

        [HttpPost]
        public ActionResult Save(PhieuXuatKhoViewModel model)
        {
            return Json(new { success = false, message = "Màn hình Phiếu xuất kho chỉ hỗ trợ xem dữ liệu." });
        }

        [HttpPost]
        public ActionResult GhiSo(int id)
        {
            return Json(new { success = false, message = "Thao tác ghi được thực hiện ở màn hình Chứng từ bán hàng." });
        }

        [HttpPost]
        public ActionResult Huy(int id, string lyDo)
        {
            return Json(new { success = false, message = "Thao tác Hủy được thực hiện ở màn hình Chứng từ bán hàng." });
        }

        public ActionResult Details(int id)
        {
            if (!PermissionHelper.HasPermission("PhieuXuatKho", LoaiPhanQuyen.Xem)) return View("AccessDenied");

            var model = _repo.GetById(id);
            if (model == null) return HttpNotFound("Không tìm thấy phiếu xuất kho");

            int totalKhos;
            var khos = _khoHangRepo.GetPaged(1, 1000, "", out totalKhos).ToList();
            ViewBag.KhoList = new SelectList(khos, "ID", "TenKhoHang", model.IDKho);

            ViewBag.IsReadOnly = true;
            return View(model);
        }

        public ActionResult GetDetailInline(int id)
        {
            if (!PermissionHelper.HasPermission("PhieuXuatKho", LoaiPhanQuyen.Xem)) return Content("<div class='text-danger p-3'>Không có quyền truy cập</div>");

            try
            {
                var model = _repo.GetById(id);
                if (model == null) return Content("<div class='text-danger p-3'>Không tìm thấy dữ liệu phiếu xuất kho</div>");

                return PartialView("_DetailInline", model);
            }
            catch (Exception ex)
            {
                return Content($"<div class='text-danger p-3'>Lỗi: {ex.Message}</div>");
            }
        }
    }
}
