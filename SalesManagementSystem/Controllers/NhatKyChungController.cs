using SalesManagementSystem.Helpers;
using SalesManagementSystem.Helpers.Security;
using SalesManagementSystem.Models.ViewModels;
using SalesManagementSystem.Repositories.Interfaces;
using System.Linq;
using System.Collections.Generic;
using System.Web.Mvc;

namespace SalesManagementSystem.Controllers
{
    public class NhatKyChungController : BaseController
    {
        private readonly INhatKyChungRepository _repo;
        private readonly ITaiKhoanKeToanRepository _taiKhoanRepo;

        public NhatKyChungController(INhatKyChungRepository repo, ITaiKhoanKeToanRepository taiKhoanRepo)
        {
            _repo = repo;
            _taiKhoanRepo = taiKhoanRepo;
        }

        public ActionResult Index(int page = 1, int pageSize = 20, string tuNgay = "", string denNgay = "", string soChungTu = "", string taiKhoanNo = "", string taiKhoanCo = "", string loaiChungTu = "")
        {
            if (!PermissionHelper.HasPermission("NhatKyChung", LoaiPhanQuyen.Xem)) return View("AccessDenied");

            var list = _repo.GetList(tuNgay, denNgay, soChungTu, taiKhoanNo, taiKhoanCo, loaiChungTu).ToList();
            int totalRecords = list.Count;
            var pagedItems = list.Skip((page - 1) * pageSize).Take(pageSize);

            var model = new PagedListViewModel<NhatKyChungListViewModel>
            {
                Items = pagedItems,
                CurrentPage = page,
                PageSize = pageSize,
                TotalRecords = totalRecords,
                ActionName = "GetList",
                Keyword = soChungTu
            };

            ViewBag.TaiKhoanKeToanList = _taiKhoanRepo.GetActive();
            ViewBag.TotalSum = list.Sum(x => x.SoTien);

            ViewBag.TuNgay = tuNgay;
            ViewBag.DenNgay = denNgay;
            ViewBag.SoChungTu = soChungTu;
            ViewBag.TaiKhoanNo = taiKhoanNo;
            ViewBag.TaiKhoanCo = taiKhoanCo;
            ViewBag.LoaiChungTu = loaiChungTu;

            if (Request.IsAjaxRequest() || Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return PartialView("_NhatKyChungList", model);

            return View("Index", model);
        }

        public ActionResult GetList(int page = 1, int pageSize = 20, string tuNgay = "", string denNgay = "", string soChungTu = "", string taiKhoanNo = "", string taiKhoanCo = "", string loaiChungTu = "")
        {
            if (!PermissionHelper.HasPermission("NhatKyChung", LoaiPhanQuyen.Xem)) return Content("<div class='alert alert-danger'>Không có quyền truy cập</div>");

            try
            {
                var list = _repo.GetList(tuNgay, denNgay, soChungTu, taiKhoanNo, taiKhoanCo, loaiChungTu).ToList();
                int totalRecords = list.Count;
                var pagedItems = list.Skip((page - 1) * pageSize).Take(pageSize);

                var model = new PagedListViewModel<NhatKyChungListViewModel>
                {
                    Items = pagedItems,
                    CurrentPage = page,
                    PageSize = pageSize,
                    TotalRecords = totalRecords,
                    ActionName = "GetList",
                    Keyword = soChungTu
                };

                ViewBag.TotalSum = list.Sum(x => x.SoTien);

                return PartialView("_NhatKyChungList", model);
            }
            catch (System.Exception ex)
            {
                return Content("<div class='alert alert-danger'>Lỗi: " + ex.Message + "</div>");
            }
        }
    }
}
