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
            ViewBag.Title = "PhÃ¢n quyá»n ngÆ°á»i dÃ¹ng";
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
                    return Json(new { success = false, message = "NhÃ¢n sá»± nÃ y khÃ´ng cÃ³ cáº¥p trÃªn Ä‘á»ƒ káº¿ thá»«a quyá»n!" });
                }
                checkedActionIds = parentActionIds;
            }

            // Náº¿u khÃ´ng cÃ³ checked nÃ o, nháº­n vá» null tá»« ajax, thÃ¬ khá»Ÿi táº¡o láº¡i danh sÃ¡ch rá»—ng Ä‘á»ƒ thá»±c thi Delete all
            if (checkedActionIds == null)
            {
                checkedActionIds = new List<int>();
            }

            var result = _phanQuyenRepo.SaveQuyen(idLogin, checkedActionIds, currentUser);

            if (result)
            {
                AuditLog.AddUpdate("ACL_PhanQuyen", idLogin.ToString(), new { Roles = "Old Roles (Many)" }, new { Roles = string.Join(",", checkedActionIds) });
                return Json(new { success = true, message = "LÆ°u phÃ¢n quyá»n thÃ nh cÃ´ng!" });
            }
            return Json(new { success = false, message = "CÃ³ lá»—i xáº£y ra khi lÆ°u phÃ¢n quyá»n." });
        }
    }
}
