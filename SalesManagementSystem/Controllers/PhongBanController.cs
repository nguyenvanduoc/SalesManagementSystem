using System;
using System.Web.Mvc;
using SalesManagementSystem.Models.Entities;
using SalesManagementSystem.Repositories;

namespace SalesManagementSystem.Controllers
{
    public class PhongBanController : Controller
    {
        private readonly PhongBanRepository _phongBanRepo;

        public PhongBanController(PhongBanRepository phongBanRepo)
        {
            _phongBanRepo = phongBanRepo;
        }

        // ==========================================
        // QUẢN LÝ PHÒNG BAN
        // ==========================================

        // GET: PhongBan/GetPhongBan
        public ActionResult GetPhongBan(int page = 1, int pageSize = 10, string keyword = "")
        {
            int totalRecords;
            var phongBans = _phongBanRepo.GetPaged(page, pageSize, keyword, out totalRecords);

            ViewBag.Total = totalRecords;
            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalPages = totalRecords > 0 ? (int)Math.Ceiling((double)totalRecords / pageSize) : 1;
            ViewBag.Keyword = keyword;

            if (Request.IsAjaxRequest())
            {
                return PartialView("_PhongBanList", phongBans);
            }

            return View("GetPhongBan", phongBans);
        }

        // GET: PhongBan/CreatePhongBan
        public ActionResult CreatePhongBan()
        {
            return PartialView("CreatePhongBan", new PhongBan());
        }

        // POST: PhongBan/CreatePhongBan
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreatePhongBan(PhongBan phongBan)
        {
            if (ModelState.IsValid)
            {
                if (_phongBanRepo.IsDuplicateCode(phongBan.MaPhongBan))
                {
                    ModelState.AddModelError("MaPhongBan", "Mã phòng ban đã tồn tại trong hệ thống.");
                    return PartialView("CreatePhongBan", phongBan);
                }

                phongBan.NguoiTao = 0; // Default save as 0
                _phongBanRepo.Insert(phongBan);
                return Json(new { success = true, message = "Thêm mới phòng ban thành công!" });
            }
            return PartialView("CreatePhongBan", phongBan);
        }

        // GET: PhongBan/UpdatePhongBan/5
        public ActionResult UpdatePhongBan(int id)
        {
            var phongBan = _phongBanRepo.GetById(id);
            if (phongBan == null)
            {
                return HttpNotFound();
            }
            return PartialView("UpdatePhongBan", phongBan);
        }

        // POST: PhongBan/UpdatePhongBan/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdatePhongBan(PhongBan phongBan)
        {
            if (ModelState.IsValid)
            {
                if (_phongBanRepo.IsDuplicateCode(phongBan.MaPhongBan, phongBan.ID))
                {
                    ModelState.AddModelError("MaPhongBan", "Mã phòng ban đã tồn tại trong hệ thống.");
                    return PartialView("UpdatePhongBan", phongBan);
                }

                phongBan.NguoiCapNhat = 0; // Default save as 0
                _phongBanRepo.Update(phongBan);
                return Json(new { success = true, message = "Cập nhật phòng ban thành công!" });
            }
            return PartialView("UpdatePhongBan", phongBan);
        }

        // POST: PhongBan/DeletePhongBan
        [HttpPost]
        public ActionResult DeletePhongBan(int? id, int[] ids)
        {
            if (id.HasValue)
            {
                _phongBanRepo.Delete(id.Value);
            }
            else if (ids != null && ids.Length > 0)
            {
                foreach (var item in ids)
                {
                    _phongBanRepo.Delete(item);
                }
            }
            return RedirectToAction("GetPhongBan");
        }
    }
}
