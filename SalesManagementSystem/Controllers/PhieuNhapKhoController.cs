using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using SalesManagementSystem.Helpers;
using SalesManagementSystem.Models.ViewModels;
using SalesManagementSystem.Repositories.Interfaces;
using Dapper;
using SalesManagementSystem.Data;
using SalesManagementSystem.Services.Interfaces;

namespace SalesManagementSystem.Controllers
{
    public class PhieuNhapKhoController : BaseController
    {
        private readonly IPhieuNhapKhoRepository _repo;
        private readonly IExcelExportService _excelExportService;

        public PhieuNhapKhoController(IPhieuNhapKhoRepository repo, IExcelExportService excelExportService)
        {
            _repo = repo;
            _excelExportService = excelExportService;
        }

        [HttpGet]
        public ActionResult GetSpDefinition(string spName)
        {
            using (var conn = new System.Data.SqlClient.SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["DefaultConnection"]?.ConnectionString ?? "Data Source=.;Initial Catalog=QuanLyBanHang;Integrated Security=True"))
            {
                // try to use DbConnectionFactory if it's injected, but we can just use Dapper on a generic connection if we had it.
                // since we don't have DbConnectionFactory injected here, let's just query sys.sql_modules
            }
            return Json("Please implement properly", JsonRequestBehavior.AllowGet);
        }

        private SelectList GetKhoList(int? selectedId = null)
        {
            var items = _repo.GetKhoForDropdown("").Select(x => new { ID = x.ID, Name = x.MaKhoHang + " - " + x.TenKhoHang }).ToList();
            return new SelectList(items, "ID", "Name", selectedId);
        }

        private SelectList GetNhaCungCapList(int? selectedId = null)
        {
            var items = _repo.GetNhaCungCapForDropdown("").Select(x => new { ID = x.ID, Name = x.MaNhaCungCap + " - " + x.TenNhaCungCap }).ToList();
            return new SelectList(items, "ID", "Name", selectedId);
        }

        private SelectList GetPhuongTienList(int? selectedId = null)
        {
            var items = _repo.GetPhuongTienForDropdown("").Select(x => new { ID = x.ID, Name = x.MaPhuongTien + " - " + x.TenPhuongTien }).ToList();
            return new SelectList(items, "ID", "Name", selectedId);
        }

        private SelectList GetSanPhamList(int? selectedId = null)
        {
            var items = _repo.GetSanPhamForDropdown("").Select(x => new { ID = x.ID, Name = x.MaSanPham + " - " + x.TenSanPham }).ToList();
            return new SelectList(items, "ID", "Name", selectedId);
        }

        public ActionResult Index(int page = 1, int pageSize = 20, 
            string tuNgay = null, string denNgay = null,
            string soChungTu = null, int? idKho = null, int? idNhaCungCap = null, 
            int? trangThai = null, string tenNguoiNhan = null,
            string tenNguoiGiao = null, int? idPhuongTien = null, string hoTenTaiXe = null, int? idSanPham = null)
        {
            if (!PermissionHelper.HasPermission("PhieuNhapKho", LoaiPhanQuyen.Xem)) return View("AccessDenied");

            int totalRecords;
            var list = _repo.GetPaged(page, pageSize, tuNgay, denNgay, soChungTu, idKho, idNhaCungCap, trangThai, tenNguoiNhan, tenNguoiGiao, idPhuongTien, hoTenTaiXe, idSanPham, out totalRecords);

            var model = new PagedListViewModel<PhieuNhapKhoListViewModel>
            {
                Items = list,
                CurrentPage = page,
                PageSize = pageSize,
                TotalRecords = totalRecords,
                ActionName = "GetList"
            };

            ViewBag.TuNgay = tuNgay;
            ViewBag.DenNgay = denNgay;
            ViewBag.SoChungTu = soChungTu;
            ViewBag.Khos = GetKhoList(idKho);
            ViewBag.NhaCungCaps = GetNhaCungCapList(idNhaCungCap);
            ViewBag.PhuongTiens = GetPhuongTienList(idPhuongTien);
            ViewBag.SanPhams = GetSanPhamList(idSanPham);
            ViewBag.TrangThai = trangThai;
            ViewBag.TenNguoiGiao = tenNguoiGiao;
            ViewBag.TenNguoiNhan = tenNguoiNhan;

            if (Request.IsAjaxRequest() || Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return PartialView("_PhieuNhapKhoList", model);

            return View("Index", model);
        }

        [HttpGet]
        public ActionResult GetList(int page = 1, int pageSize = 20, string tuNgay = "", string denNgay = "", string soChungTu = "", int? idKho = null, int? idNhaCungCap = null, int? trangThai = null, string tenNguoiNhan = "", string tenNguoiGiao = "", int? idPhuongTien = null, string hoTenTaiXe = null, int? idSanPham = null)
        {
            if (!PermissionHelper.HasPermission("PhieuNhapKho", LoaiPhanQuyen.Xem)) return Content("<div class='alert alert-danger'>Không có quyền truy cập</div>");

            try 
            {
                var list = _repo.GetPaged(page, pageSize, tuNgay, denNgay, soChungTu, idKho, idNhaCungCap, trangThai, tenNguoiNhan, tenNguoiGiao, idPhuongTien, hoTenTaiXe, idSanPham, out int totalRecords);

                var model = new PagedListViewModel<PhieuNhapKhoListViewModel> 
                {
                    Items = list,
                    CurrentPage = page,
                    PageSize = pageSize,
                    TotalRecords = totalRecords,
                    ActionName = "GetList"
                };

                return PartialView("_PhieuNhapKhoList", model);
            }
            catch(Exception ex)
            {
                return Content($"<div class='alert alert-danger'>Lỗi Server: {ex.Message} <br/> {ex.StackTrace}</div>");
            }
        }

        [HttpGet]
        public ActionResult ExportExcel(string tuNgay = "", string denNgay = "", string soChungTu = "", int? idKho = null, int? idNhaCungCap = null, int? trangThai = null, string tenNguoiNhan = "", string tenNguoiGiao = "", int? idPhuongTien = null, string hoTenTaiXe = null, int? idSanPham = null)
        {
            if (!PermissionHelper.HasPermission("PhieuNhapKho", LoaiPhanQuyen.Xem)) 
                return View("AccessDenied");

            try
            {
                var list = _repo.GetPaged(1, 100000, tuNgay, denNgay, soChungTu, idKho, idNhaCungCap, trangThai, tenNguoiNhan, tenNguoiGiao, idPhuongTien, hoTenTaiXe, idSanPham, out int totalRecords);

                var phieuIds = list.Select(x => x.ID).ToList();
                List<dynamic> details = new List<dynamic>();
                if (phieuIds.Any())
                {
                    using (var conn = new DbConnectionFactory().CreateConnection())
                    {
                        details = conn.Query(@"
                            SELECT c.IDPhieuNhap, s.MaSanPham, s.TenSanPham, s.DVT, c.SoLuong, c.DonGia, c.ThanhTien, c.DonGiaVanChuyen, c.TienVanChuyen 
                            FROM KHO_PhieuNhap_ChiTiet c
                            LEFT JOIN DM_SanPham s ON c.IDSanPham = s.ID
                            WHERE c.IDPhieuNhap IN @IDs",
                            new { IDs = phieuIds }).ToList();
                    }
                }

                string nhaCungCapName = "Tất cả";
                if (idNhaCungCap.HasValue)
                {
                    using (var conn = new DbConnectionFactory().CreateConnection())
                    {
                        nhaCungCapName = conn.ExecuteScalar<string>(
                            "SELECT TenNhaCungCap FROM DM_NhaCungCap WHERE ID = @ID",
                            new { ID = idNhaCungCap.Value }
                        ) ?? "Tất cả";
                    }
                }

                string strTuNgay = "";
                string strDenNgay = "";
                if (DateTime.TryParse(tuNgay, out DateTime dTu)) strTuNgay = dTu.ToString("dd/MM/yyyy");
                if (DateTime.TryParse(denNgay, out DateTime dDen)) strDenNgay = dDen.ToString("dd/MM/yyyy");

                var session = (UserLoginViewModel)Session[CommonConstants.USER_SESSION];
                string nguoiLapBieu = session != null ? (session.HoDem + " " + session.Ten).Trim() : "Hệ thống";
                if (string.IsNullOrEmpty(nguoiLapBieu)) nguoiLapBieu = session?.UserName ?? "Hệ thống";

                var variables = new Dictionary<string, object>
                {
                    { "TuNgay", strTuNgay },
                    { "DenNgay", strDenNgay },
                    { "KhachHang", nhaCungCapName },
                    { "Ngay", DateTime.Now.ToString("dd") },
                    { "Thang", DateTime.Now.ToString("MM") },
                    { "Nam", DateTime.Now.ToString("yyyy") },
                    { "NguoiLapBieu", nguoiLapBieu }
                };

                int stt = 1;
                var exportData = new List<PhieuNhapExcelModel>();

                foreach (var item in list)
                {
                    var itemDetails = details.Where(d => (int)d.IDPhieuNhap == item.ID).ToList();
                    if (!itemDetails.Any())
                    {
                        exportData.Add(new PhieuNhapExcelModel {
                            STT = stt++,
                            SoChungTu = item.SoChungTu,
                            NgayNhap = item.NgayNhap != null ? ((DateTime)item.NgayNhap).ToString("dd/MM/yyyy") : "",
                            NgayGiao = item.NgayGiaoHang != null ? ((DateTime)item.NgayGiaoHang).ToString("dd/MM/yyyy") : "",
                            TenNguoiNhanHang = item.TenNguoiNhan ?? "",
                            TenKho = item.TenKho,
                            TenPhuongTien = item.TenPhuongTien,
                            TenNguoiGiaoHang = item.TenNguoiGiao ?? "",
                            TenNhaCungCap = item.TenNhaCungCap,
                            MaSanPham = "",
                            TenSanPham = "",
                            DVT = "",
                            SoLuong = 0M,
                            DonGia = 0M,
                            TongTien = 0M,
                            DonGiaVanChuyen = 0M,
                            ThanhTienVanChuyen = 0M
                        });
                    }
                    else
                    {
                        foreach (var d in itemDetails)
                        {
                            exportData.Add(new PhieuNhapExcelModel {
                                STT = stt++,
                                SoChungTu = item.SoChungTu,
                                NgayNhap = item.NgayNhap != null ? ((DateTime)item.NgayNhap).ToString("dd/MM/yyyy") : "",
                                NgayGiao = item.NgayGiaoHang != null ? ((DateTime)item.NgayGiaoHang).ToString("dd/MM/yyyy") : "",
                                TenNguoiNhanHang = item.TenNguoiNhan ?? "",
                                TenKho = item.TenKho,
                                TenPhuongTien = item.TenPhuongTien,
                                TenNguoiGiaoHang = item.TenNguoiGiao ?? "",
                                TenNhaCungCap = item.TenNhaCungCap,
                                MaSanPham = (string)(d.MaSanPham ?? ""),
                                TenSanPham = (string)(d.TenSanPham ?? ""),
                                DVT = (string)(d.DVT ?? ""),
                                SoLuong = Convert.ToDecimal(d.SoLuong ?? 0m),
                                DonGia = Convert.ToDecimal(d.DonGia ?? 0m),
                                TongTien = Convert.ToDecimal(d.ThanhTien ?? 0m),
                                DonGiaVanChuyen = Convert.ToDecimal(d.DonGiaVanChuyen),
                                ThanhTienVanChuyen = Convert.ToDecimal(d.TienVanChuyen)
                            });
                        }
                    }
                }

                string fileExtension;
                var fileBytes = _excelExportService.Export("PN01", exportData, out fileExtension, variables);

                string contentType = fileExtension == "xls" 
                    ? "application/vnd.ms-excel" 
                    : "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

                return File(fileBytes, contentType, $"DanhSachPhieuNhapKho_{DateTime.Now:yyyyMMddHHmmss}.{fileExtension}");
            }
            catch (Exception ex)
            {
                TempData["ToastType"] = "error";
                TempData["ToastMessage"] = $"Lỗi xuất Excel: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

        public ActionResult Create()
        {
            if (!PermissionHelper.HasPermission("PhieuNhapKho", LoaiPhanQuyen.Them)) return View("AccessDenied");

            var model = new PhieuNhapKhoViewModel();
            model.SoChungTu = _repo.GenerateSoChungTu();
            
            var loaiNhapList = _repo.GetLoaiNhapKhoForDropdown();
            foreach (var itemDynamic in loaiNhapList)
            {
                var item = (IDictionary<string, object>)itemDynamic;
                if (item.ContainsKey("MaLoaiNhap") && (string)item["MaLoaiNhap"] == "NHAP_MUA")
                {
                    model.IDLoaiNhapKho = (int)item["ID"];
                    model.MaLoaiNhap = (string)item["MaLoaiNhap"];
                    model.TenLoaiNhap = (string)item["TenLoaiNhap"];
                    break;
                }
            }

            return View("Edit", model);
        }

        public ActionResult Copy(int id)
        {
            if (!PermissionHelper.HasPermission("PhieuNhapKho", LoaiPhanQuyen.Them)) return View("AccessDenied");

            var entity = _repo.GetByID(id);
            if (entity == null) return HttpNotFound();

            var model = new PhieuNhapKhoViewModel
            {
                ID = 0,
                SoChungTu = _repo.GenerateSoChungTu(),
                NgayNhap = DateTime.Now,
                IDKho = entity.IDKho,
                IDNhaCungCap = entity.IDNhaCungCap,
                SoHoaDon = entity.SoHoaDon,
                NgayHoaDon = entity.NgayHoaDon,
                TenNguoiGiao = entity.TenNguoiGiao,
                SoDienThoaiNguoiGiao = entity.SoDienThoaiNguoiGiao,
                TenNguoiNhan = entity.TenNguoiNhan,
                GhiChu = entity.GhiChu,
                TrangThai = 1, // Mặc định là Nháp
                IsReadOnly = false,
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
            var list = _repo.GetPaged(1, 1, null, null, entity.SoChungTu, null, null, null, null, null, null, null, null, out total);
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
                var phuongTiens = _repo.GetPhuongTienForDropdown("");
                var pt = phuongTiens.FirstOrDefault(x => (int)x.ID == model.IDPhuongTien);
                if (pt != null)
                {
                    model.TenPhuongTien = (string)pt.MaPhuongTien + " - " + (string)pt.TenPhuongTien;
                }
            }

            var chiTiets = _repo.GetChiTiet(id);
            if (chiTiets != null)
            {
                model.ChiTiets = chiTiets.Select(x => new PhieuNhapKhoChiTietViewModel
                {
                    ID = 0,
                    IDPhieuNhap = 0,
                    IDSanPham = x.IDSanPham,
                    MaSanPham = x.MaSanPham,
                    TenSanPham = x.TenSanPham,
                    DVT = x.DVT,
                    SoLuong = x.SoLuong,
                    DonGia = x.DonGia,
                    ThanhTien = x.ThanhTien,
                    ThueGTGT = x.ThueGTGT,
                    TienThue = x.TienThue,
                    TongSauThue = x.TongSauThue,
                    DonGiaVanChuyen = x.DonGiaVanChuyen,
                    TienVanChuyen = x.TienVanChuyen,
                    GhiChu = x.GhiChu,
                    NgaySanXuat = x.NgaySanXuat,
                    HanSuDung = x.HanSuDung
                }).ToList();
            }

            ViewBag.IsView = false;
            return View("Edit", model);
        }

        public ActionResult Edit(int id, bool isView = false)
        {
            if (!PermissionHelper.HasPermission("PhieuNhapKho", LoaiPhanQuyen.Xem)) return View("AccessDenied");

            var entity = _repo.GetByID(id);
            if (entity == null) return HttpNotFound();

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
                TrangThaiThanhToan = entity.TrangThaiThanhToan,
                IsReadOnly = isView || entity.TrangThai == 2 || entity.TrangThai == 3,
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
            var list = _repo.GetPaged(1, 1, null, null, entity.SoChungTu, null, null, null, null, null, null, null, null, out total);
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
                var phuongTiens = _repo.GetPhuongTienForDropdown("");
                var pt = phuongTiens.FirstOrDefault(x => (int)x.ID == model.IDPhuongTien);
                if (pt != null)
                {
                    model.TenPhuongTien = (string)pt.MaPhuongTien + " - " + (string)pt.TenPhuongTien;
                }
            }

            model.ChiTiets = _repo.GetChiTiet(id);
            ViewBag.IsView = isView;
            return View("Edit", model);
        }

        [HttpPost]
        public ActionResult Save(PhieuNhapKhoViewModel model)
        {
            if (model.ID == 0 && !PermissionHelper.HasPermission("PhieuNhapKho", LoaiPhanQuyen.Them)) return Json(new { success = false, message = "Không có quyền thêm mới" });
            if (model.ID > 0 && !PermissionHelper.HasPermission("PhieuNhapKho", LoaiPhanQuyen.CapNhat)) return Json(new { success = false, message = "Không có quyền sửa" });

            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Dữ liệu không hợp lệ" });
            }

            try
            {
                if (model.ChiTiets != null && model.ChiTiets.Any(x => x.DonGiaVanChuyen < 0))
                {
                    return Json(new { success = false, message = "Tiá»n váº­n chuyá»ƒn khÃ´ng Ä‘Æ°á»£c Ã¢m." });
                }

                if (model.IDLoaiNhapKho.HasValue && model.IDKhoNguon.HasValue)
                {
                    var loaiNhapList = _repo.GetLoaiNhapKhoForDropdown();
                    dynamic loaiNhap = null;
                    foreach (var item in loaiNhapList)
                    {
                        if ((int)item.ID == model.IDLoaiNhapKho.Value)
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
                            var chiTietsJson = Newtonsoft.Json.JsonConvert.SerializeObject(model.ChiTiets);
                            var invalidItemsDynamic = _repo.CheckTonKhoChuyenKho(model.IDKhoNguon.Value, chiTietsJson).ToList();
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

                int newId = _repo.Save(model, userId);
                return Json(new { success = true, id = newId, soChungTu = model.SoChungTu, message = model.TrangThai == 0 ? (model.ID > 0 ? "Lưu nháp thành công" : "Lưu nháp thành công") : (model.ID > 0 ? "Cập nhật phiếu nhập kho thành công" : "Đề nghị ghi thành công") });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public ActionResult GhiSo(int id)
        {
            if (!PermissionHelper.HasPermission("PhieuNhapKho", LoaiPhanQuyen.TuyChon)) return Json(new { success = false, message = "Không có quyền ghi" });

            try
            {
                var user = GetCurrentUser();
                int userId = user?.IDNhanSu ?? 0;

                _repo.GhiSo(id, userId);
                return Json(new { success = true, message = "ghi thành công" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public ActionResult BoGhiSo(int id)
        {
            if (!PermissionHelper.HasPermission("PhieuNhapKho", LoaiPhanQuyen.TuyChon)) return Json(new { success = false, message = "Không có quyền bỏ ghi" });

            try
            {
                var user = GetCurrentUser();
                int userId = user?.IDNhanSu ?? 0;

                _repo.BoGhiSo(id, userId);
                return Json(new { success = true, message = "Bỏ ghi thành công" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public ActionResult HuyPhieu(int id, string lyDoHuy)
        {
            if (!PermissionHelper.HasPermission("PhieuNhapKho", LoaiPhanQuyen.TuyChon)) return Json(new { success = false, message = "Không có quyền hủy phiếu" });

            if (string.IsNullOrWhiteSpace(lyDoHuy))
            {
                return Json(new { success = false, message = "Vui lòng nhập lý do hủy" });
            }

            try
            {
                var user = GetCurrentUser();
                int userId = user?.IDNhanSu ?? 0;

                _repo.HuyPhieu(id, lyDoHuy, userId);
                return Json(new { success = true, message = "Hủy phiếu thành công" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public ActionResult Delete(int id)
        {
            if (!PermissionHelper.HasPermission("PhieuNhapKho", LoaiPhanQuyen.Xoa)) return Json(new { success = false, message = "Không có quyền xóa" });

            try
            {
                var user = GetCurrentUser();
                int userId = user?.IDNhanSu ?? 0;

                _repo.Delete(id, userId);
                return Json(new { success = true, message = "Xóa phiếu thành công" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public ActionResult DeletePhanQuyenPhu(int id)
        {
            if (!PermissionHelper.HasPermission("PhieuNhapKho", LoaiPhanQuyen.TuyChon)) return Json(new { success = false, message = "Không có quyền xóa (phân quyền phụ)" });

            try
            {
                var user = GetCurrentUser();
                int userId = user?.IDNhanSu ?? 0;

                _repo.DeletePhanQuyenPhu(id, userId);
                return Json(new { success = true, message = "Xóa phiếu và tính lại tồn kho thành công" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public ActionResult GetDetailInline(int id)
        {
            if (!PermissionHelper.HasPermission("PhieuNhapKho", LoaiPhanQuyen.Xem)) 
                return Content("<div class='alert alert-danger'>Không có quyền xem chi tiết</div>");

            var entity = _repo.GetByID(id);
            if (entity == null) return HttpNotFound();

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
                IsReadOnly = true,
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
            var list = _repo.GetPaged(1, 1, null, null, entity.SoChungTu, null, null, null, null, null, null, null, null, out total);
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
                var phuongTiens = _repo.GetPhuongTienForDropdown("");
                var pt = phuongTiens.FirstOrDefault(x => (int)x.ID == model.IDPhuongTien);
                if (pt != null)
                {
                    model.TenPhuongTien = (string)pt.MaPhuongTien + " - " + (string)pt.TenPhuongTien;
                }
            }

            model.ChiTiets = _repo.GetChiTiet(id);
            ViewBag.IsView = true;
            ViewBag.IsInlineDetail = true;
            
            return PartialView("_DetailInline", model);
        }

        // Dropdowns endpoints
        [HttpGet]
        public ActionResult SearchKhoHang(string q)
        {
            var data = _repo.GetKhoForDropdown(q);
            return Json(data.Select(x => new { id = (int)x.ID, text = (string)x.MaKhoHang + " - " + (string)x.TenKhoHang }), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult SearchNhaCungCap(string q)
        {
            var data = _repo.GetNhaCungCapForDropdown(q);
            return Json(data.Select(x => new { id = (int)x.ID, text = (string)x.MaNhaCungCap + " - " + (string)x.TenNhaCungCap }), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult SearchNhanSu(string q)
        {
            var data = _repo.GetNhanSuForDropdown(q);
            return Json(data.Select(x => new { id = (int)x.ID, text = (string)x.MaNhanSu + " - " + (string)x.HoTen }), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult SearchSanPham(string q)
        {
            var data = _repo.GetSanPhamForDropdown(q);
            return Json(data.Select(x => new { id = (int)x.ID, text = (string)x.MaSanPham + " - " + (string)x.TenSanPham, dvt = (string)x.DVT }), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult SearchPhuongTien(string q)
        {
            var data = _repo.GetPhuongTienForDropdown(q);
            return Json(data.Select(x => new { id = (int)x.ID, text = (string)x.MaPhuongTien + " - " + (string)x.TenPhuongTien }), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult SearchLoaiNhapKho()
        {
            var data = _repo.GetLoaiNhapKhoForDropdown();
            return Json(data.Select(x => new { id = (int)x.ID, ma = (string)x.MaLoaiNhap, text = (string)x.TenLoaiNhap }), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult SearchKhachHang(string q)
        {
            var data = _repo.GetKhachHangForDropdown(q);
            return Json(data.Select(x => new { id = (int)x.ID, text = (string)x.MaKhachHang + " - " + (string)x.TenKhachHang }), JsonRequestBehavior.AllowGet);
        }

        private class PhieuNhapExcelModel
        {
            public int STT { get; set; }
            public string SoChungTu { get; set; }
            public string NgayNhap { get; set; }
            public string NgayGiao { get; set; }
            public string TenNguoiNhanHang { get; set; }
            public string TenKho { get; set; }
            public string TenPhuongTien { get; set; }
            public string TenNguoiGiaoHang { get; set; }
            public string TenNhaCungCap { get; set; }
            public string MaSanPham { get; set; }
            public string TenSanPham { get; set; }
            public string DVT { get; set; }
            public decimal SoLuong { get; set; }
            public decimal DonGia { get; set; }
            public decimal TongTien { get; set; }
            public decimal DonGiaVanChuyen { get; set; }
            public decimal ThanhTienVanChuyen { get; set; }
        }
    }
}
