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
            var paged = _donDatHangRepo.GetPaged(1, 1000, "", "", null, null, 2, "", out totalRecords); // 2 = Đã duyệt
            return Json(new { data = paged }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetModalChonDon()
        {
            return PartialView("_ChonDonDatHangModal");
        }

        public ActionResult Create(int idDonDatHang)
        {
            if (!PermissionHelper.HasPermission("PhieuXuatKho", LoaiPhanQuyen.Them)) return View("AccessDenied");

            var donHang = _donDatHangRepo.GetById(idDonDatHang);
            if (donHang == null || donHang.TrangThaiDon != 2) return HttpNotFound("Đơn hàng không tồn tại hoặc chưa được duyệt.");

            var khachHang = donHang.IDKhachHang.HasValue ? (new SalesManagementSystem.Repositories.KhachHangRepository(new Data.DbConnectionFactory())).GetById(donHang.IDKhachHang.Value) : null;

            int totalKhos;
            var khos = _khoHangRepo.GetPaged(1, 1000, "", out totalKhos).ToList();
            var firstKho = khos.FirstOrDefault();

            var model = new PhieuXuatKhoViewModel();
            model.SoChungTu = _repo.GenerateSoChungTu();
            model.IDDonDatHang = idDonDatHang;
            model.SoDonHang = donHang.SoDonHang;
            model.TenKhachHang = khachHang?.TenKhachHang ?? "";
            model.IDKho = firstKho?.ID ?? 0;
            model.TenKhoHang = firstKho?.TenKhoHang ?? "";
            model.NgayXuat = DateTime.Now.Date;

            var chiTietsDon = _donDatHangRepo.GetChiTietByDonId(idDonDatHang);

            int stt = 1;
            foreach (var ct in chiTietsDon)
            {
                model.ChiTiets.Add(new PhieuXuatKhoChiTietViewModel
                {
                    IDSanPham = ct.IDSanPham ?? 0,
                    MaSanPham = ct.MaSanPham,
                    TenSanPham = ct.TenSanPham,
                    DVT = ct.DVT,
                    STT = stt++,
                    SoLuong = ct.SoLuong,
                    DonGia = ct.DonGia,
                    ThanhTien = ct.ThanhTien,
                    ThueGTGT = ct.ThueGTGT,
                    TienThue = ct.ThanhTienThue,
                    TongSauThue = ct.ThanhTienSauThue
                });
            }

            model.TongTienHang = model.ChiTiets.Sum(x => x.ThanhTien);
            model.TongTienThue = model.ChiTiets.Sum(x => x.TienThue);
            model.TongCong = model.ChiTiets.Sum(x => x.TongSauThue);

            ViewBag.KhoList = new SelectList(khos, "ID", "TenKhoHang", model.IDKho);

            return View(model);
        }

        [HttpPost]
        public ActionResult Save(PhieuXuatKhoViewModel model)
        {
            if (!PermissionHelper.HasPermission("PhieuXuatKho", LoaiPhanQuyen.Them)) return Json(new { success = false, message = "Không có quyền thêm mới" });

            try
            {
                var user = GetCurrentUser();
                int userId = user?.IDNhanSu ?? 0;

                int newId = _repo.Insert(model, userId);

                // Cập nhật trạng thái đơn hàng sang Đã Xuất Kho (3)
                if (model.IDDonDatHang.HasValue)
                {
                    var p = new DynamicParameters();
                    p.Add("@ID", model.IDDonDatHang.Value);
                    p.Add("@TrangThaiDon", 3);
                    p.Add("@IDNguoiCapNhat", userId);
                    new DbConnectionFactory().CreateConnection().Execute("UPDATE NS_DonDatHang SET TrangThaiDon = @TrangThaiDon WHERE ID = @ID", p);
                }

                return Json(new { success = true, id = newId });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public ActionResult GhiSo(int id)
        {
            if (!PermissionHelper.HasPermission("PhieuXuatKho", LoaiPhanQuyen.TuyChon)) return Json(new { success = false, message = "Không có quyền ghi sổ" });

            try
            {
                var user = GetCurrentUser();
                int userId = user?.IDNhanSu ?? 0;

                var model = _repo.GetById(id);
                if (model == null) return Json(new { success = false, message = "Phiếu xuất không tồn tại." });
                if (model.TrangThai != 1) return Json(new { success = false, message = "Phiếu xuất đã ghi hoặc đã hủy." });

                // 1. Cập nhật Status = 2
                _repo.UpdateStatus(id, 2, userId);

                // 2. Ghi Tồn kho (KHO_GiaoDichKho)
                using (var conn = new DbConnectionFactory().CreateConnection())
                {
                    foreach (var ct in model.ChiTiets)
                    {
                        var p = new DynamicParameters();
                        p.Add("@IDKho", model.IDKho);
                        p.Add("@IDSanPham", ct.IDSanPham);
                        p.Add("@NgayGiaoDich", model.NgayXuat);
                        p.Add("@LoaiChungTu", 2); // 2 = Xuất
                        p.Add("@IDChungTu", model.ID);
                        p.Add("@SoChungTu", model.SoChungTu);
                        p.Add("@SoLuong", ct.SoLuong);
                        p.Add("@DonGia", ct.DonGia);
                        p.Add("@ThanhTien", ct.ThanhTien);
                        p.Add("@NguoiTao", userId);
                        p.Add("@NgayTao", DateTime.Now);
                        p.Add("@IsHuy", 0);

                        conn.Execute(@"
                            INSERT INTO KHO_GiaoDichKho (IDKho, IDSanPham, NgayGiaoDich, LoaiChungTu, IDChungTu, SoChungTu, SoLuong, DonGia, ThanhTien, NguoiTao, NgayTao, IsHuy) 
                            VALUES (@IDKho, @IDSanPham, @NgayGiaoDich, @LoaiChungTu, @IDChungTu, @SoChungTu, @SoLuong, @DonGia, @ThanhTien, @NguoiTao, @NgayTao, @IsHuy)", p);
                    }
                }

                // 3. Ghi Nhật ký chung (Giá vốn hàng bán: Nợ 632 / Có 156)
                decimal tongGiaVon = model.ChiTiets.Sum(x => x.ThanhTien); // Ở đây tạm dùng ThanhTien làm giá vốn
                if (tongGiaVon > 0)
                {
                    _nhatKyRepo.Insert(new KT_NhatKyChung
                    {
                        NgayChungTu = model.NgayXuat,
                        SoChungTu = model.SoChungTu,
                        LoaiChungTu = "PX",
                        IDChungTu = model.ID,
                        TaiKhoanNo = "632",
                        TaiKhoanCo = "156",
                        SoTien = tongGiaVon,
                        DienGiai = "Giá vốn hàng bán xuất kho theo phiếu " + model.SoChungTu,
                        NguoiTao = userId
                    });
                }

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public ActionResult Huy(int id, string lyDo)
        {
            if (!PermissionHelper.HasPermission("PhieuXuatKho", LoaiPhanQuyen.TuyChon)) return Json(new { success = false, message = "Không có quyền hủy" });

            try
            {
                var user = GetCurrentUser();
                int userId = user?.IDNhanSu ?? 0;

                // 1. Cập nhật Status = 3
                _repo.Cancel(id, userId, lyDo);

                // 2. Hủy tồn kho
                using (var conn = new DbConnectionFactory().CreateConnection())
                {
                    conn.Execute("UPDATE KHO_GiaoDichKho SET IsHuy = 1 WHERE LoaiChungTu = 2 AND IDChungTu = @ID", new { ID = id });
                }

                // 3. Hủy Nhật ký chung
                _nhatKyRepo.Cancel("PX", id, userId);

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}
