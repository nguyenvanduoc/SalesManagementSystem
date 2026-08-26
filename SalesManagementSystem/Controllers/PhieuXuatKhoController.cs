using SalesManagementSystem.Helpers;
using SalesManagementSystem.Helpers.Security;
using SalesManagementSystem.Models.Entities;
using SalesManagementSystem.Models.ViewModels;
using SalesManagementSystem.Repositories.Interfaces;
using SalesManagementSystem.Services.Interfaces;
using System;
using System.Collections.Generic;
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
        private readonly IExcelExportService _excelExportService;
        private readonly DbConnectionFactory _db;

        public PhieuXuatKhoController(
            IPhieuXuatKhoRepository repo,
            IDonDatHangRepository donDatHangRepo,
            INhatKyChungRepository nhatKyRepo,
            IDmKhoHangRepository khoHangRepo,
            IExcelExportService excelExportService,
            DbConnectionFactory db)
        {
            _repo = repo;
            _donDatHangRepo = donDatHangRepo;
            _nhatKyRepo = nhatKyRepo;
            _khoHangRepo = khoHangRepo;
            _excelExportService = excelExportService;
            _db = db;
        }

        public ActionResult Index(int page = 1, int pageSize = 20, string tuNgay = "", string denNgay = "", string soChungTu = "", int? idKho = null, int? idSanPham = null, int? idNhaCungCap = null, string tenNguoiGiao = "", int? idPhuongTien = null, string tenNguoiNhan = "", int? trangThai = null, int? idNhanSuNhan = null)
        {
            if (!PermissionHelper.HasPermission("PhieuXuatKho", LoaiPhanQuyen.Xem)) return View("AccessDenied");

            int totalRecords;
            var list = _repo.GetList(page, pageSize, tuNgay, denNgay, soChungTu, idKho, trangThai, idNhanSuNhan, idSanPham, idNhaCungCap, tenNguoiGiao, idPhuongTien, tenNguoiNhan, out totalRecords);

            var model = new PagedListViewModel<PhieuXuatKhoListViewModel>
            {
                Items = list,
                CurrentPage = page,
                PageSize = pageSize,
                TotalRecords = totalRecords,
                ActionName = "GetList",
                Keyword = soChungTu
            };

            using (var conn = _db.CreateConnection())
            {
                var khos = conn.Query("SELECT ID, MaKhoHang + ' - ' + TenKhoHang AS Name FROM DM_KhoHang ORDER BY TenKhoHang")
                               .Select(x => new { ID = (int)x.ID, Name = (string)x.Name }).ToList();
                ViewBag.Khos = new SelectList(khos, "ID", "Name", idKho);

                var sps = conn.Query("SELECT ID, MaSanPham + ' - ' + TenSanPham AS Name FROM DM_SanPham ORDER BY TenSanPham")
                              .Select(x => new { ID = (int)x.ID, Name = (string)x.Name }).ToList();
                ViewBag.SanPhams = new SelectList(sps, "ID", "Name", idSanPham);

                var nccs = conn.Query("SELECT ID, MaKhachHang + ' - ' + TenKhachHang AS Name FROM NS_KhachHang ORDER BY TenKhachHang")
                               .Select(x => new { ID = (int)x.ID, Name = (string)x.Name }).ToList();
                ViewBag.NhaCungCaps = new SelectList(nccs, "ID", "Name", idNhaCungCap);

                var pts = conn.Query("SELECT ID, MaPhuongTien + ' - ' + TenPhuongTien AS Name FROM DM_PhuongTien ORDER BY STT, TenPhuongTien")
                              .Select(x => new { ID = (int)x.ID, Name = (string)x.Name }).ToList();
                ViewBag.PhuongTiens = new SelectList(pts, "ID", "Name", idPhuongTien);
            }

            ViewBag.TuNgay = tuNgay;
            ViewBag.DenNgay = denNgay;
            ViewBag.SoChungTu = soChungTu;
            ViewBag.IDKho = idKho;
            ViewBag.IDSanPham = idSanPham;
            ViewBag.IDNhaCungCap = idNhaCungCap;
            ViewBag.TenNguoiGiao = tenNguoiGiao;
            ViewBag.IDPhuongTien = idPhuongTien;
            ViewBag.TenNguoiNhan = tenNguoiNhan;
            ViewBag.TrangThai = trangThai;

            if ((Request.IsAjaxRequest() || Request.Headers["X-Requested-With"] == "XMLHttpRequest") && Request.Headers["X-SPA-Load"] != "true")
                return PartialView("_PhieuXuatKhoList", model);

            return View("Index", model);
        }

        public ActionResult GetList(int page = 1, int pageSize = 20, string tuNgay = "", string denNgay = "", string soChungTu = "", int? idKho = null, int? idSanPham = null, int? idNhaCungCap = null, string tenNguoiGiao = "", int? idPhuongTien = null, string tenNguoiNhan = "", int? trangThai = null, int? idNhanSuNhan = null)
        {
            if (!PermissionHelper.HasPermission("PhieuXuatKho", LoaiPhanQuyen.Xem)) return Content("<div class='alert alert-danger'>Không có quyền truy cập</div>");

            try
            {
                int totalRecords;
                var list = _repo.GetList(page, pageSize, tuNgay, denNgay, soChungTu, idKho, trangThai, idNhanSuNhan, idSanPham, idNhaCungCap, tenNguoiGiao, idPhuongTien, tenNguoiNhan, out totalRecords);

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
            var paged = _donDatHangRepo.GetPaged(1, 1000, "", "", null, null, 2, "", null, null, out totalRecords); // 2 = Đã duyệt
            return Json(new { data = paged }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetModalChonDon()
        {
            return PartialView("_ChonDonDatHangModal");
        }

        public ActionResult Create(int idDonDatHang)
        {
            return Content("<div class='alert alert-danger'>Màn hình Phiếu xuất kho chỉ để xem. Vui lòng lập chứng từ bán hàng.</div>");
        }

        [HttpPost]
        public ActionResult Save(PhieuXuatKhoViewModel model)
        {
            return Json(new { success = false, message = "Màn hình Phiếu xuất kho chỉ hỗ trợ xem dữ liệu." });
        }

        [HttpPost]
        public ActionResult GhiSo(int id)
        {
            return Json(new { success = false, message = "Thao tác ghi được thực hiện ở màn hình Chứng từ bán hàng." });
        }

        [HttpPost]
        public ActionResult Huy(int id, string lyDo)
        {
            return Json(new { success = false, message = "Thao tác Hủy được thực hiện ở màn hình Chứng từ bán hàng." });
        }

        public ActionResult Details(int id)
        {
            if (!PermissionHelper.HasPermission("PhieuXuatKho", LoaiPhanQuyen.Xem)) return View("AccessDenied");

            var model = _repo.GetById(id);
            if (model == null) return HttpNotFound("Không tìm thấy phiếu xuất kho");

            int totalKhos;
            var khos = _khoHangRepo.GetPaged(1, 1000, "", out totalKhos).ToList();
            ViewBag.KhoList = new SelectList(khos, "ID", "TenKhoHang", model.IDKho);

            ViewBag.IsReadOnly = true;
            return View(model);
        }

        public ActionResult GetDetailInline(int id)
        {
            if (!PermissionHelper.HasPermission("PhieuXuatKho", LoaiPhanQuyen.Xem)) return Content("<div class='text-danger p-3'>Không có quyền truy cập</div>");

            try
            {
                var model = _repo.GetById(id);
                if (model == null) return Content("<div class='text-danger p-3'>Không tìm thấy dữ liệu phiếu xuất kho</div>");

                return PartialView("_DetailInline", model);
            }
            catch (Exception ex)
            {
                return Content($"<div class='text-danger p-3'>Lỗi: {ex.Message}</div>");
            }
        }

        public ActionResult ExportExcel(string tuNgay = "", string denNgay = "", string soChungTu = "", int? idKho = null, int? idSanPham = null, int? idNhaCungCap = null, string tenNguoiGiao = "", int? idPhuongTien = null, string tenNguoiNhan = "", int? trangThai = null, int? idNhanSuNhan = null)
        {
            if (!PermissionHelper.HasPermission("PhieuXuatKho", LoaiPhanQuyen.Xem)) return Content("Không có quyền truy cập");

            try
            {
                int totalRecords;
                var list = _repo.GetList(1, int.MaxValue, tuNgay, denNgay, soChungTu, idKho, trangThai, idNhanSuNhan, idSanPham, idNhaCungCap, tenNguoiGiao, idPhuongTien, tenNguoiNhan, out totalRecords);

                var ids = list.Select(x => x.ID).ToList();
                List<PhieuXuatKhoChiTietViewModel> allDetails = new List<PhieuXuatKhoChiTietViewModel>();

                if (ids.Count > 0)
                {
                    using (var conn = _db.CreateConnection())
                    {
                        string sql = @"
                            SELECT
                                ct.ID, ct.IDPhieuXuat, ct.IDSanPham,
                                sp.MaSanPham, sp.TenSanPham, sp.DVT,
                                ct.SoLuong, ct.DonGia, ct.ThanhTien
                            FROM KHO_PhieuXuat_ChiTiet ct
                            LEFT JOIN DM_SanPham sp ON ct.IDSanPham = sp.ID
                            WHERE ct.IDPhieuXuat IN @IDs
                            ORDER BY ct.ID";
                        allDetails = conn.Query<PhieuXuatKhoChiTietViewModel>(sql, new { IDs = ids }).ToList();
                    }
                }

                var flatList = new List<XuatKho01ExportModel>();
                int stt = 1;
                foreach (var px in list)
                {
                    var details = allDetails.Where(d => d.IDPhieuXuat == px.ID).ToList();

                    if (details.Count == 0)
                    {
                        flatList.Add(new XuatKho01ExportModel
                        {
                            STT = stt++,
                            SoChungTu = px.SoChungTu ?? "",
                            NgayXuat = px.NgayXuat.ToString("dd/MM/yyyy"),
                            SoDonHang = px.SoDonHang ?? "",
                            TenKhachHang = px.TenKhachHang ?? "",
                            TenKho = px.TenKhoHang ?? "",
                            TenKhoHang = px.TenKhoHang ?? "",
                            MaSanPham = "",
                            TenSanPham = "",
                            DVT = "",
                            SoLuong = 0m
                        });
                    }
                    else
                    {
                        foreach (var d in details)
                        {
                            flatList.Add(new XuatKho01ExportModel
                            {
                                STT = stt++,
                                SoChungTu = px.SoChungTu ?? "",
                                NgayXuat = px.NgayXuat.ToString("dd/MM/yyyy"),
                                SoDonHang = px.SoDonHang ?? "",
                                TenKhachHang = px.TenKhachHang ?? "",
                                TenKho = px.TenKhoHang ?? "",
                                TenKhoHang = px.TenKhoHang ?? "",
                                MaSanPham = d.MaSanPham ?? "",
                                TenSanPham = d.TenSanPham ?? "",
                                DVT = d.DVT ?? "",
                                SoLuong = d.SoLuong
                            });
                        }
                    }
                }

                var variables = new Dictionary<string, object>
                {
                    { "TuNgay", tuNgay },
                    { "DenNgay", denNgay },
                    { "KhachHang", "Tất cả" },
                    { "Ngay", DateTime.Now.ToString("dd") },
                    { "Thang", DateTime.Now.ToString("MM") },
                    { "Nam", DateTime.Now.ToString("yyyy") }
                };

                string ext;
                var fileBytes = _excelExportService.Export("PhieuXuat01", flatList, out ext, variables);

                string contentType = ext == "xls" 
                    ? "application/vnd.ms-excel" 
                    : "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

                return File(fileBytes, contentType, $"PhieuXuatKho_{DateTime.Now:yyyyMMddHHmmss}.{ext}");
            }
            catch (Exception ex)
            {
                return Content($"Lỗi xuất Excel: {ex.Message}");
            }
        }

        public class XuatKho01ExportModel
        {
            public int STT { get; set; }
            public string SoChungTu { get; set; }
            public string NgayXuat { get; set; }
            public string SoDonHang { get; set; }
            public string TenKhachHang { get; set; }
            public string TenKho { get; set; }
            public string TenKhoHang { get; set; }
            public string MaSanPham { get; set; }
            public string TenSanPham { get; set; }
            public string DVT { get; set; }
            public decimal SoLuong { get; set; }
        }
    }
}
