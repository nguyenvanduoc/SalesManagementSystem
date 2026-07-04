using System;
using System.Linq;
using System.Web.Mvc;
using SalesManagementSystem.Helpers;
using SalesManagementSystem.Repositories.Interfaces;

namespace SalesManagementSystem.Controllers
{
    public class BaoCaoDoiChieuNhapNhaCungCapController : BaseController
    {
        private readonly IBaoCaoDoiChieuNhapNhaCungCapRepository _repo;

        public BaoCaoDoiChieuNhapNhaCungCapController(IBaoCaoDoiChieuNhapNhaCungCapRepository repo)
        {
            _repo = repo;
        }

        public ActionResult Index()
        {
            // Tự do tuỳ chỉnh quyền nếu cần, hiện tại không ràng buộc quyền cụ thể hoặc bạn có thể tự thêm
            // if (!PermissionHelper.HasPermission("BaoCao", LoaiPhanQuyen.Xem)) return View("AccessDenied");

            ViewBag.Title = "BÁO CÁO ĐỐI CHIẾU NHẬP NCC";
            
            var nccs = _repo.GetNhaCungCapDropdown()
                .Select(x => new SelectListItem { Value = ((int)x.ID).ToString(), Text = (string)x.TenHienThi });
            ViewBag.NhaCungCapList = new SelectList(nccs.ToList(), "Value", "Text");

            // Khởi tạo ngày mặc định (đầu tháng đến hiện tại)
            ViewBag.TuNgay = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).ToString("yyyy-MM-dd");
            ViewBag.DenNgay = DateTime.Now.ToString("yyyy-MM-dd");

            return View();
        }

        [HttpGet]
        public ActionResult GetList(int? idNhaCungCap, DateTime? tuNgay, DateTime? denNgay)
        {
            if (!idNhaCungCap.HasValue)
                return Content("<div class='alert alert-warning text-center mt-3'>Vui lòng chọn nhà cung cấp</div>");
            
            if (!tuNgay.HasValue)
                return Content("<div class='alert alert-warning text-center mt-3'>Vui lòng chọn từ ngày</div>");

            if (!denNgay.HasValue)
                return Content("<div class='alert alert-warning text-center mt-3'>Vui lòng chọn đến ngày</div>");

            if (tuNgay.Value > denNgay.Value)
                return Content("<div class='alert alert-danger text-center mt-3'>Từ ngày không được lớn hơn đến ngày</div>");

            try
            {
                var data = _repo.GetList(idNhaCungCap.Value, tuNgay.Value, denNgay.Value);
                return PartialView("_DanhSach", data.ToList());
            }
            catch (Exception ex)
            {
                return Content($"<div class='alert alert-danger text-center mt-3'>Lỗi tải dữ liệu: {ex.Message}</div>");
            }
        }
    }
}
