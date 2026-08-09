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
        [CustomAuthorize(AuthorizeTypes.MustHavePermission)]
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
        [CustomAuthorize(AuthorizeTypes.MustHavePermission)]
        public ActionResult Save(int idLogin, List<int> checkedActionIds, bool isInherit = false)
        {
            var userSession = (UserLoginViewModel)Session[CommonConstants.USER_SESSION];
            int currentUser = userSession != null ? userSession.IDNhanSu : 0;

            if (isInherit)
            {
                var parentActionIds = _phanQuyenRepo.GetParentActionIds(idLogin);
                if (parentActionIds == null)
                {
                    return Json(new { success = false, message = "Nhân sự này không có cấp trên để kế thừa quyền!" });
                }
                checkedActionIds = parentActionIds;
            }

            // Nếu không có checked nào, nhận về null từ ajax, thì khởi tạo lại danh sách rỗng để thực thi Delete all
            if (checkedActionIds == null)
            {
                checkedActionIds = new List<int>();
            }

            var result = _phanQuyenRepo.SaveQuyen(idLogin, checkedActionIds, currentUser);

            if (result)
            {
                PermissionHelper.ClearUserPermissionsCache(idLogin);
                AuditLog.AddUpdate("ACL_PhanQuyen", idLogin.ToString(), new { Roles = "Old Roles (Many)" }, new { Roles = string.Join(",", checkedActionIds) });
                return Json(new { success = true, message = "Lưu phân quyền thành công!" });
            }
            return Json(new { success = false, message = "Có lỗi xảy ra khi lưu phân quyền." });
        }
    }
}
