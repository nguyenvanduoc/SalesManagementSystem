using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SalesManagementSystem.Models.ViewModels;
using SalesManagementSystem.Repositories;
using SalesManagementSystem.Repositories.Interfaces;

using SalesManagementSystem.Helpers;

namespace SalesManagementSystem.Controllers
{
    [CustomAuthorize(AuthorizeTypes.AuthorizedUsers)]
    public class KHO_HaoHutController : BaseController
    {
        private readonly IKhoHaoHutRepository _haoHutRepo;
        private readonly IDmKhoHangRepository _khoHangRepo;

        // Constructor Injection
        public KHO_HaoHutController(IKhoHaoHutRepository haoHutRepo, IDmKhoHangRepository khoHangRepo)
        {
            _haoHutRepo = haoHutRepo;
            _khoHangRepo = khoHangRepo;
        }

        // GET: KHO_HaoHut
        [CustomAuthorize(AuthorizeTypes.MustHavePermission)]
        public ActionResult Index()
        {
            ViewBag.KhoHangs = _khoHangRepo.GetAll();
            return View();
        }

        [HttpPost]
        public ActionResult GetList(HaoHutHangHoaFilter filter)
        {
            try
            {
                var data = _haoHutRepo.GetList(filter);
                int totalRecords = data.Count > 0 ? data[0].TotalRecords : 0;
                
                var mappedData = data.Select(x => new {
                    ID = x.ID,
                    SoChungTu = x.SoChungTu,
                    NgayHaoHut = x.NgayHaoHut,
                    LoaiHaoHut = x.LoaiHaoHut,
                    IDKho = x.IDKho,
                    TenKho = x.TenKho,
                    IDDonHang = x.IDDonHang,
                    SoDonHang = x.SoDonHang,
                    IDKhachHang = x.IDKhachHang,
                    TenKhachHang = x.TenKhachHang,
                    TongSoLuong = x.TongSoLuong,
                    TongTienHaoHut = x.TongTienHaoHut,
                    TrangThai = x.TrangThai
                }).ToList();

                return Json(new { success = true, data = mappedData, total = totalRecords });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        public ActionResult _List()
        {
            return PartialView();
        }

        [CustomAuthorize(AuthorizeTypes.MustHavePermission)]
        public ActionResult _Create()
        {
            ViewBag.KhoHangs = _khoHangRepo.GetAll();
            return PartialView();
        }

        [CustomAuthorize(AuthorizeTypes.MustHavePermission)]
        public ActionResult _Edit(int id)
        {
            ViewBag.KhoHangs = _khoHangRepo.GetAll();
            var model = _haoHutRepo.GetByID(id);
            return PartialView(model);
        }
        
        public ActionResult _Detail(int id)
        {
            var model = _haoHutRepo.GetByID(id);
            return PartialView(model);
        }

        [HttpPost]
        [CustomAuthorize(AuthorizeTypes.MustHavePermission)]
        public ActionResult Save(HaoHutHangHoaViewModel model)
        {
            try
            {
                int currentUserId = 1; // Fallback TODO: Get actual UserID from session/auth

                int haoHutId = model.ID;
                if (model.ID == 0)
                {
                    haoHutId = _haoHutRepo.Insert(model, currentUserId);
                }
                else
                {
                    _haoHutRepo.Update(model, currentUserId);
                }

                // Xóa chi tiết cũ và lưu chi tiết mới
                _haoHutRepo.DeleteDetails(haoHutId);

                if (model.Details != null && model.Details.Any())
                {
                    foreach (var detail in model.Details)
                    {
                        detail.IDHaoHut = haoHutId;
                        _haoHutRepo.InsertDetail(detail, currentUserId);
                    }
                }

                return Json(new { success = true, id = haoHutId, message = "Lưu thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [CustomAuthorize(AuthorizeTypes.MustHavePermission)]
        public ActionResult GhiNhan(int id)
        {
            try
            {
                int currentUserId = 1; // Fallback
                _haoHutRepo.GhiNhan(id, currentUserId);
                return Json(new { success = true, message = "Ghi nhận thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [CustomAuthorize(AuthorizeTypes.MustHavePermission)]
        public ActionResult Huy(int id)
        {
            try
            {
                int currentUserId = 1; // Fallback
                _haoHutRepo.Huy(id, currentUserId);
                return Json(new { success = true, message = "Hủy thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
        
        [HttpPost]
        [CustomAuthorize(AuthorizeTypes.MustHavePermission)]
        public ActionResult Delete(int id)
        {
            try
            {
                int currentUserId = 1; // Fallback
                _haoHutRepo.Delete(id, currentUserId);
                return Json(new { success = true, message = "Xóa thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public ActionResult GetDonHang(string keyword)
        {
            try
            {
                var data = _haoHutRepo.GetDonHang(keyword).Select(x => new {
                    ID = x.ID,
                    SoDonHang = x.SoDonHang,
                    NgayTaoDon = x.NgayTaoDon,
                    IDKhachHang = x.IDKhachHang,
                    TenKhachHang = x.TenKhachHang,
                    IDChungTuBanHang = x.IDChungTuBanHang,
                    SoChungTuBanHang = x.SoChungTuBanHang
                }).ToList();
                return Json(new { success = true, data = data }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public ActionResult GetChiTietDonHang(int id)
        {
            try
            {
                var data = _haoHutRepo.GetChiTietDonHang(id).Select(x => new {
                    IDSanPham = x.IDSanPham,
                    MaSanPham = x.MaSanPham,
                    TenSanPham = x.TenSanPham,
                    SoLuongHienTai = x.SoLuongHienTai,
                    DonGiaBan = x.DonGiaBan,
                    DonGiaHaoHut = x.DonGiaHaoHut
                }).ToList();
                return Json(new { success = true, data = data }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public ActionResult GetTonKho(int idKho, int idSanPham)
        {
            try
            {
                var tonKho = _haoHutRepo.GetTonKho(idKho, idSanPham);
                var giaNhap = _haoHutRepo.GetGiaNhapGanNhat(idSanPham);
                return Json(new { success = true, tonKho = tonKho, giaNhap = giaNhap }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}
