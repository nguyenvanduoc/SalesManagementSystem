using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using SalesManagementSystem.Helpers;
using SalesManagementSystem.Models.ViewModels;
using SalesManagementSystem.Repositories.Interfaces;

namespace SalesManagementSystem.Controllers
{
    public class PhanQuyenController : BaseController
    {
        private readonly IAclPhanQuyenRepository _phanQuyenRepo;

        public PhanQuyenController(IAclPhanQuyenRepository phanQuyenRepo)
        {
            _phanQuyenRepo = phanQuyenRepo;
        }

        public ActionResult Index()
        {
            ViewBag.Title = "Phân quyền người dùng";
            var tree = _phanQuyenRepo.GetTreeLogin();
            return View(tree);
        }

        public ActionResult GetGrid(int idLogin)
        {
            var matrix = _phanQuyenRepo.GetMatrixQuyen(idLogin);
            ViewBag.IDLogin = idLogin;
            return PartialView("_MatrixGrid", matrix);
        }

        [HttpPost]
        public ActionResult Save(int idLogin, List<int> checkedActionIds)
        {
            var userSession = (UserLogin)Session[CommonConstants.USER_SESSION];
            int currentUser = userSession != null ? userSession.UserID : 0;

            // Nếu không có checked nào, nhận về null từ ajax, thì khởi tạo lại danh sách rỗng để thực thi Delete all
            if (checkedActionIds == null)
            {
                checkedActionIds = new List<int>();
            }

            var result = _phanQuyenRepo.SaveQuyen(idLogin, checkedActionIds, currentUser);

            if (result)
            {
                return Json(new { success = true, message = "Lưu phân quyền thành công!" });
            }
            return Json(new { success = false, message = "Có lỗi xảy ra khi lưu phân quyền." });
        }
    }
}
