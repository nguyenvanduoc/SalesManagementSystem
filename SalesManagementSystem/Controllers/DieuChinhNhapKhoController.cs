using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using SalesManagementSystem.Models.ViewModels;
using SalesManagementSystem.Repositories.Interfaces;
using SalesManagementSystem.Helpers;

namespace SalesManagementSystem.Controllers
{
    [CustomAuthorize(AuthorizeTypes.AuthorizedUsers)]
    public class DieuChinhNhapKhoController : BaseController
    {
        private readonly IDieuChinhNhapKhoRepository _dieuChinhRepo;
        private readonly IPhieuNhapKhoRepository _phieuNhapRepo;

        public DieuChinhNhapKhoController(
            IDieuChinhNhapKhoRepository dieuChinhRepo,
            IPhieuNhapKhoRepository phieuNhapRepo)
        {
            _dieuChinhRepo = dieuChinhRepo;
            _phieuNhapRepo = phieuNhapRepo;
        }

        public ActionResult Index(
            int page = 1, int pageSize = 20,
            string tuNgay = "", string denNgay = "",
            int? idLoaiNhap = null, int? idKho = null,
            int? idNhaCungCap = null, int? idKhachHang = null,
            string soChungTu = "", bool chiDonDieuChinh = false)
        {
            if (!PermissionHelper.HasPermission("DieuChinhNhapKho", LoaiPhanQuyen.Xem)) return View("AccessDenied");

            if (string.IsNullOrEmpty(tuNgay)) tuNgay = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).ToString("yyyy-MM-dd");
            if (string.IsNullOrEmpty(denNgay)) denNgay = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month)).ToString("yyyy-MM-dd");

            ViewBag.TuNgay = tuNgay;
            ViewBag.DenNgay = denNgay;
            ViewBag.IDLoaiNhap = idLoaiNhap;
            ViewBag.IDKho = idKho;
            ViewBag.IDNhaCungCap = idNhaCungCap;
            ViewBag.IDKhachHang = idKhachHang;
            ViewBag.SoChungTu = soChungTu;
            ViewBag.ChiDonDieuChinh = chiDonDieuChinh;

            if (Request.IsAjaxRequest())
            {
                int totalRecords = 0;
                var data = _dieuChinhRepo.GetPaged(page, pageSize, tuNgay, denNgay, idLoaiNhap, idKho, idNhaCungCap, idKhachHang, soChungTu, chiDonDieuChinh, out totalRecords);
                
                var model = new PagedListViewModel<DieuChinhNhapKhoListViewModel>
                {
                    Items = data,
                    CurrentPage = page,
                    PageSize = pageSize,
                    TotalRecords = totalRecords,
                    ActionName = "Index"
                };

                return PartialView("_AdjustList", model);
            }
            
            var initialData = _dieuChinhRepo.GetPaged(page, pageSize, tuNgay, denNgay, idLoaiNhap, idKho, idNhaCungCap, idKhachHang, soChungTu, chiDonDieuChinh, out int initialTotal);
            var initModel = new PagedListViewModel<DieuChinhNhapKhoListViewModel>
            {
                Items = initialData,
                CurrentPage = page,
                PageSize = pageSize,
                TotalRecords = initialTotal,
                ActionName = "Index"
            };

            return View(initModel);
        }

        [HttpGet]
        public ActionResult Adjust(int id)
        {
            if (!PermissionHelper.HasPermission("DieuChinhNhapKho", LoaiPhanQuyen.CapNhat)) return View("AccessDenied");

            var entity = _phieuNhapRepo.GetByID(id);
            if (entity == null || entity.TrangThai != 2) // Chỉ phiếu đã ghi mới được điều chỉnh
            {
                return HttpNotFound("Phiếu nhập không tồn tại hoặc không thể điều chỉnh (chưa ghi sổ).");
            }

            var model = new PhieuNhapKhoViewModel
            {
                ID = entity.ID,
                SoChungTu = entity.SoChungTu,
                NgayNhap = entity.NgayNhap,
                IDKho = entity.IDKho,
                IDNhaCungCap = entity.IDNhaCungCap,
                SoHoaDon = entity.SoHoaDon,
                NgayHoaDon = entity.NgayHoaDon,
                TenNguoiGiao = entity.TenNguoiGiao,
                SoDienThoaiNguoiGiao = entity.SoDienThoaiNguoiGiao,
                TenNguoiNhan = entity.TenNguoiNhan,
                GhiChu = entity.GhiChu,
                TrangThai = entity.TrangThai,
                IsReadOnly = false, // Cho phép sửa trong giao diện điều chỉnh
                IDLoaiNhapKho = entity.IDLoaiNhapKho,
                IDKhoNguon = entity.IDKhoNguon,
                IDKhachHang = entity.IDKhachHang,
                IDPhuongTien = entity.IDPhuongTien,
                NgayGiaoHang = entity.NgayGiaoHang,
                HoTenTaiXe = entity.HoTenTaiXe,
                SoDienThoaiTaiXe = entity.SoDienThoaiTaiXe,
                TienVanChuyen = entity.TienVanChuyen
            };

            int total;
            var list = _phieuNhapRepo.GetPaged(1, 1, null, null, entity.SoChungTu, null, null, null, null, null, null, out total);
            var item = list.FirstOrDefault();
            if (item != null)
            {
                model.TenKho = item.TenKho;
                model.TenNhaCungCap = item.TenNhaCungCap;
                model.TenLoaiNhap = item.TenLoaiNhap;
                model.MaLoaiNhap = item.MaLoaiNhap;
                model.TenKhoNguon = item.TenKhoNguon;
                model.TenKhachHang = item.TenKhachHang;
            }

            if (model.IDPhuongTien.HasValue && model.IDPhuongTien > 0)
            {
                var phuongTiens = _phieuNhapRepo.GetPhuongTienForDropdown("");
                var pt = phuongTiens.FirstOrDefault(x => (int)x.ID == model.IDPhuongTien);
                if (pt != null)
                {
                    model.TenPhuongTien = (string)pt.MaPhuongTien + " - " + (string)pt.TenPhuongTien;
                }
            }

            model.ChiTiets = _phieuNhapRepo.GetChiTiet(id);
            
            ViewBag.IsAdjust = true;
            return View(model);
        }

        [HttpPost]
        public ActionResult SaveAdjust(DieuChinhNhapKhoPostModel model)
        {
            if (!PermissionHelper.HasPermission("DieuChinhNhapKho", LoaiPhanQuyen.CapNhat)) return Json(new { success = false, message = "Không có quyền thực hiện" });

            try
            {
                if (!ModelState.IsValid)
                {
                    return Json(new { success = false, message = "Dữ liệu không hợp lệ." });
                }

                var chiTiets = Newtonsoft.Json.JsonConvert.DeserializeObject<List<PhieuNhapKhoChiTietViewModel>>(model.ChiTietsJson) ?? new List<PhieuNhapKhoChiTietViewModel>();
                foreach (var ct in chiTiets)
                {
                    if (ct.DonGiaVanChuyen < 0)
                    {
                        return Json(new { success = false, message = "Đơn giá vận chuyển không được âm" });
                    }

                    ct.TienVanChuyen = ct.DonGiaVanChuyen * ct.SoLuong;
                }
                model.ChiTietsJson = Newtonsoft.Json.JsonConvert.SerializeObject(chiTiets);

                if (model.IDLoaiNhapKho > 0 && model.IDKhoNguon.HasValue)
                {
                    var loaiNhapList = _phieuNhapRepo.GetLoaiNhapKhoForDropdown();
                    dynamic loaiNhap = null;
                    foreach (var item in loaiNhapList)
                    {
                        if ((int)item.ID == model.IDLoaiNhapKho)
                        {
                            loaiNhap = item;
                            break;
                        }
                    }

                    if (loaiNhap != null)
                    {
                        var loaiNhapDict = (IDictionary<string, object>)loaiNhap;
                        if (loaiNhapDict.ContainsKey("MaLoaiNhap") && (string)loaiNhapDict["MaLoaiNhap"] == "CHUYEN_KHO")
                        {
                            var invalidItemsDynamic = _phieuNhapRepo.CheckTonKhoChuyenKho(model.IDKhoNguon.Value, model.ChiTietsJson).ToList();
                            if (invalidItemsDynamic.Any())
                            {
                                var msg = "Kho nguồn không đủ số lượng cho các sản phẩm:\n";
                                foreach (var itemDynamic in invalidItemsDynamic)
                                {
                                    var item = (IDictionary<string, object>)itemDynamic;
                                    var slYeuCau = Convert.ToDecimal(item["SoLuongYeuCau"]).ToString("0.##");
                                    var slTon = Convert.ToDecimal(item["SoLuongTon"]).ToString("0.##");
                                    msg += $"- {item["MaSanPham"]} - {item["TenSanPham"]} (Yêu cầu: {slYeuCau}, Tồn kho: {slTon})\n";
                                }
                                return Json(new { success = false, message = msg });
                            }
                        }
                    }
                }

                var user = GetCurrentUser();
                int userId = user?.IDNhanSu ?? 0;
                
                _dieuChinhRepo.SaveAdjustment(model, userId);

                return Json(new { success = true, message = "Điều chỉnh phiếu nhập kho thành công." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public ActionResult History(int id)
        {
            if (!PermissionHelper.HasPermission("DieuChinhNhapKho", LoaiPhanQuyen.Xem)) return Content("<div class='alert alert-danger'>Không có quyền truy cập</div>");

            var history = _dieuChinhRepo.GetAdjustHistory(id);
            return PartialView("_HistoryModal", history);
        }
    }
}
