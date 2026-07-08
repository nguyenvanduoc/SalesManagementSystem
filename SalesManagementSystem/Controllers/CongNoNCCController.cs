using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using SalesManagementSystem.Helpers;
using SalesManagementSystem.Models.ViewModels;
using SalesManagementSystem.Repositories.Interfaces;

namespace SalesManagementSystem.Controllers
{
    public class CongNoNCCController : BaseController
    {
        private readonly ICongNoNCCRepository _repo;
        private readonly INhaCungCapRepository _nccRepo;
        private readonly IPhieuChiRepository _phieuChiRepo;

        public CongNoNCCController(ICongNoNCCRepository repo, INhaCungCapRepository nccRepo, IPhieuChiRepository phieuChiRepo)
        {
            _repo    = repo;
            _nccRepo = nccRepo;
            _phieuChiRepo = phieuChiRepo;  
        }

        // GET: /cong-no-ncc
        public ActionResult Index(
            string tuNgay = "",
            string denNgay = "",
            int? idNhaCungCap = null,
            int? trangThaiCongNo = null)
        {
            if (!PermissionHelper.HasPermission("CongNoNCC", LoaiPhanQuyen.Xem))
                return View("AccessDenied");

            var list = _repo.GetList(tuNgay, denNgay, idNhaCungCap, trangThaiCongNo).ToList();
            int totalRecords = list.Count;
            int page         = 1;
            int pageSize     = 20;

            var pagedItems = list.Skip((page - 1) * pageSize).Take(pageSize);

            var model = new PagedListViewModel<CongNoNCCViewModel>
            {
                Items        = pagedItems,
                CurrentPage  = page,
                PageSize     = pageSize,
                TotalRecords = totalRecords,
                ActionName   = "GetList"
            };

            ViewBag.Title           = "Công Nợ Phải Trả Nhà Cung Cấp";
            ViewBag.TuNgay          = tuNgay;
            ViewBag.DenNgay         = denNgay;
            ViewBag.IDNhaCungCap    = idNhaCungCap;
            ViewBag.TrangThaiCongNo = trangThaiCongNo;
            ViewBag.TongPhaiTra     = list.Sum(x => x.TongTienHang);
            ViewBag.TongDaTra       = list.Sum(x => x.DaThanhToan);
            ViewBag.TongConLai      = list.Sum(x => x.ConLai);
            
            decimal tongTienTraTruoc = 0;
            if (idNhaCungCap.HasValue && idNhaCungCap.Value > 0)
            {
                tongTienTraTruoc = _phieuChiRepo.GetTienTraTruocNhaCungCap(idNhaCungCap.Value);
            }
            ViewBag.TongTienTraTruoc = tongTienTraTruoc;

            PopulateNhaCungCapDropdown(idNhaCungCap);

            if (Request.IsAjaxRequest() || Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return PartialView("_CongNoNCCList", model);

            return View("Index", model);
        }

        // GET: /cong-no-ncc/danh-sach
        public ActionResult GetList(
            int page = 1, int pageSize = 20,
            string tuNgay = "",
            string denNgay = "",
            int? idNhaCungCap = null,
            int? trangThaiCongNo = null)
        {
            if (!PermissionHelper.HasPermission("CongNoNCC", LoaiPhanQuyen.Xem))
                return Content("<div class='alert alert-danger'>Không có quyền truy cập</div>");

            try
            {
                var list = _repo.GetList(tuNgay, denNgay, idNhaCungCap, trangThaiCongNo).ToList();
                int totalRecords = list.Count;
                var pagedItems   = list.Skip((page - 1) * pageSize).Take(pageSize);

                var model = new PagedListViewModel<CongNoNCCViewModel>
                {
                    Items        = pagedItems,
                    CurrentPage  = page,
                    PageSize     = pageSize,
                    TotalRecords = totalRecords,
                    ActionName   = "GetList"
                };

                ViewBag.TongPhaiTra = list.Sum(x => x.TongTienHang);
                ViewBag.TongDaTra   = list.Sum(x => x.DaThanhToan);
                ViewBag.TongConLai  = list.Sum(x => x.ConLai);
                
                decimal tongTienTraTruoc = 0;
                if (idNhaCungCap.HasValue && idNhaCungCap.Value > 0)
                {
                    tongTienTraTruoc = _phieuChiRepo.GetTienTraTruocNhaCungCap(idNhaCungCap.Value);
                }
                ViewBag.TongTienTraTruoc = tongTienTraTruoc;

                return PartialView("_CongNoNCCList", model);
            }
            catch (Exception ex)
            {
                return Content($"<div class='alert alert-danger'>Lỗi: {ex.Message}</div>");
            }
        }

        private void PopulateNhaCungCapDropdown(int? selectedId = null)
        {
            var list = _nccRepo.GetForDropdown("")
                .Select(x => new SelectListItem
                {
                    Value    = ((int)x.ID).ToString(),
                    Text     = (string)x.TenNhaCungCap,
                    Selected = selectedId.HasValue && (int)x.ID == selectedId.Value
                }).ToList();
            ViewBag.NhaCungCapList = list;
        }
    }
}
