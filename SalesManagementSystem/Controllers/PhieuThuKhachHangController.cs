using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using SalesManagementSystem.Helpers;
using SalesManagementSystem.Models.ViewModels;
using SalesManagementSystem.Repositories.Interfaces;
using Dapper;

namespace SalesManagementSystem.Controllers
{
    public class PhieuThuKhachHangController : BaseController
    {
        private readonly IPhieuThuKhachHangRepository _repo;
        private readonly ITaiKhoanKeToanRepository _taiKhoanRepo;
        private readonly IChungTuBanHangRepository _ctbhRepo;

        public PhieuThuKhachHangController(
            IPhieuThuKhachHangRepository repo,
            ITaiKhoanKeToanRepository taiKhoanRepo,
            IChungTuBanHangRepository ctbhRepo)
        {
            _repo = repo;
            _taiKhoanRepo = taiKhoanRepo;
            _ctbhRepo = ctbhRepo;
        }

        // GET: PhieuThuKhachHang
        public ActionResult Index(int page = 1, int pageSize = 20, string tuNgay = "", string denNgay = "", string soChungTu = "", int? idKhachHang = null, int? trangThaiCongNo = null)
        {
            if (!PermissionHelper.HasPermission("PhieuThuKhachHang", LoaiPhanQuyen.Xem)) 
                return View("AccessDenied");

            var list = _repo.GetList(tuNgay, denNgay, soChungTu, idKhachHang, trangThaiCongNo).ToList();
            int totalRecords = list.Count;
            var pagedItems = list.Skip((page - 1) * pageSize).Take(pageSize);

            var model = new PagedListViewModel<PhieuThuKhachHangListViewModel>
            {
                Items = pagedItems,
                CurrentPage = page,
                PageSize = pageSize,
                TotalRecords = totalRecords,
                ActionName = "GetList",
                Keyword = soChungTu
            };

            // Load view bags for filters
            ViewBag.KhachHangList = new SelectList(GetKhachHangs(), "ID", "TenKhachHang");

            ViewBag.TuNgay = tuNgay;
            ViewBag.DenNgay = denNgay;
            ViewBag.SoChungTu = soChungTu;
            ViewBag.IDKhachHang = idKhachHang;
            ViewBag.TrangThaiCongNo = trangThaiCongNo;

            if (Request.IsAjaxRequest() || Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return PartialView("_PhieuThuKhachHangList", model);

            return View("Index", model);
        }

        // GET: PhieuThuKhachHang/GetList (for AJAX paging/filtering)
        public ActionResult GetList(int page = 1, int pageSize = 20, string tuNgay = "", string denNgay = "", string soChungTu = "", int? idKhachHang = null, int? trangThaiCongNo = null)
        {
            if (!PermissionHelper.HasPermission("PhieuThuKhachHang", LoaiPhanQuyen.Xem)) 
                return Content("<div class='alert alert-danger'>Không có quyền truy cập</div>");

            try
            {
                var list = _repo.GetList(tuNgay, denNgay, soChungTu, idKhachHang, trangThaiCongNo).ToList();
                int totalRecords = list.Count;
                var pagedItems = list.Skip((page - 1) * pageSize).Take(pageSize);

                var model = new PagedListViewModel<PhieuThuKhachHangListViewModel>
                {
                    Items = pagedItems,
                    CurrentPage = page,
                    PageSize = pageSize,
                    TotalRecords = totalRecords,
                    ActionName = "GetList",
                    Keyword = soChungTu
                };

                return PartialView("_PhieuThuKhachHangList", model);
            }
            catch (Exception ex)
            {
                return Content("<div class='alert alert-danger'>Lỗi: " + ex.Message + "</div>");
            }
        }

        // GET: PhieuThuKhachHang/Edit/5 (Where ID is IDChungTuBanHang)
        [HttpGet]
        public ActionResult Edit(int id)
        {
            if (!PermissionHelper.HasPermission("PhieuThuKhachHang", LoaiPhanQuyen.Xem))
                return Content("<div class='alert alert-danger'>Không có quyền thực hiện chức năng này</div>");

            var congNo = _repo.GetCongNoChungTuByID(id);
            if (congNo == null)
                return HttpNotFound();

            var model = new PhieuThuKhachHangViewModel
            {
                IDChungTuBanHang = congNo.ID,
                SoChungTuBanHang = congNo.SoChungTu,
                IDKhachHang = congNo.IDKhachHang,
                TenKhachHang = congNo.TenKhachHang,
                TongChungTu = (decimal)congNo.TongCong,
                DaThanhToanTruoc = (decimal)congNo.DaThanhToan,
                ConLaiSauThu = (decimal)congNo.ConLai,
                SoPhieuThu = _repo.GenerateSoPhieuThu(),
                NgayThu = DateTime.Today,
                SoTienThu = 0
            };

            // Load view bags
            var taiKhoans = _repo.GetTaiKhoanThanhToanDropdown()
                .Select(x => new TaiKhoanDropdownItem {
                    ID = (int)x.ID,
                    TenHienThi = (string)x.TenHienThi
                }).ToList();
            ViewBag.TaiKhoanThanhToans = new SelectList(taiKhoans, "ID", "TenHienThi");

            var nhanSus = _repo.GetNhanSuDropdown()
                .Select(x => new NhanSuDropdownItem {
                    ID = (int)x.ID,
                    HoTen = (string)x.HoTen
                }).ToList();
            ViewBag.NhanSus = new SelectList(nhanSus, "ID", "HoTen");

            ViewBag.PaymentHistory = _repo.GetHistoryByChungTuID(id);
            
            decimal totalDebt = _repo.GetCreditInfo(congNo.IDKhachHang);
            ViewBag.TotalDebt = totalDebt;
            ViewBag.CreditLimitRemaining = 500000000 - totalDebt;

            ViewBag.RecentActivities = _repo.GetRecentActivities(id);

            return PartialView("Collect", model);
        }

        // POST: PhieuThuKhachHang/Save
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Save(PhieuThuKhachHangViewModel model, bool ghiSo = false)
        {
            bool hasThem = PermissionHelper.HasPermission("PhieuThuKhachHang", LoaiPhanQuyen.Them);
            bool hasSua = PermissionHelper.HasPermission("PhieuThuKhachHang", LoaiPhanQuyen.CapNhat);

            if (model.ID == 0 && !hasThem)
                return Json(new { success = false, message = "Không có quyền thêm mới" });
            if (model.ID > 0 && !hasSua)
                return Json(new { success = false, message = "Không có quyền cập nhật" });

            // Validate backend
            if (model.NgayThu == DateTime.MinValue)
            {
                ModelState.AddModelError("NgayThu", "Ngày thu không được rỗng.");
            }
            if (model.IDChungTuBanHang <= 0)
            {
                ModelState.AddModelError("IDChungTuBanHang", "Chưa chọn chứng từ bán hàng.");
            }
            if (model.IDTaiKhoanThanhToan <= 0)
            {
                ModelState.AddModelError("IDTaiKhoanThanhToan", "Chưa chọn tài khoản nhận tiền.");
            }
            if (!model.IDNguoiThu.HasValue || model.IDNguoiThu.Value <= 0)
            {
                ModelState.AddModelError("IDNguoiThu", "Chưa chọn người thu tiền.");
            }
            if (model.SoTienThu <= 0)
            {
                ModelState.AddModelError("SoTienThu", "Số tiền thu phải lớn hơn 0.");
            }

            if (model.IDChungTuBanHang > 0)
            {
                var congNo = _repo.GetCongNoChungTuByID(model.IDChungTuBanHang);
                if (congNo == null)
                {
                    ModelState.AddModelError("IDChungTuBanHang", "Chứng từ bán hàng không tồn tại.");
                }
                else
                {
                    decimal tongCong = (decimal)congNo.TongCong;
                    decimal daThuKhac = 0;
                    using (var conn = new SalesManagementSystem.Data.DbConnectionFactory().CreateConnection())
                    {
                        string sql = "SELECT ISNULL(SUM(SoTienThu), 0) FROM BAN_PhieuThuKhachHang WHERE IDChungTuBanHang = @IDCT AND TrangThai = 2 AND IsDeleted = 0 AND ID <> @ID";
                        daThuKhac = conn.ExecuteScalar<decimal>(sql, new { IDCT = model.IDChungTuBanHang, ID = model.ID });
                    }

                    decimal conLaiSauCacPhieuKhac = tongCong - daThuKhac;

                    if (model.SoTienThu > conLaiSauCacPhieuKhac)
                    {
                        ModelState.AddModelError("SoTienThu", $"Số tiền thu không được vượt quá số tiền còn phải thu ({conLaiSauCacPhieuKhac:N0}).");
                    }
                }
            }

            if (!ModelState.IsValid)
            {
                var congNo = _repo.GetCongNoChungTuByID(model.IDChungTuBanHang);
                if (congNo != null)
                {
                    model.SoChungTuBanHang = congNo.SoChungTu;
                    model.TenKhachHang = congNo.TenKhachHang;
                    model.TongChungTu = (decimal)congNo.TongCong;
                    model.DaThanhToanTruoc = (decimal)congNo.DaThanhToan;
                    model.ConLaiSauThu = (decimal)congNo.ConLai;
                }

                var taiKhoans = _repo.GetTaiKhoanThanhToanDropdown()
                    .Select(x => new TaiKhoanDropdownItem {
                        ID = (int)x.ID,
                        TenHienThi = (string)x.TenHienThi
                    }).ToList();
                ViewBag.TaiKhoanThanhToans = new SelectList(taiKhoans, "ID", "TenHienThi", model.IDTaiKhoanThanhToan);

                var nhanSus = _repo.GetNhanSuDropdown()
                    .Select(x => new NhanSuDropdownItem {
                        ID = (int)x.ID,
                        HoTen = (string)x.HoTen
                    }).ToList();
                ViewBag.NhanSus = new SelectList(nhanSus, "ID", "HoTen", model.IDNguoiThu);

                ViewBag.PaymentHistory = _repo.GetHistoryByChungTuID(model.IDChungTuBanHang);
                
                decimal totalDebt = _repo.GetCreditInfo(model.IDKhachHang);
                ViewBag.TotalDebt = totalDebt;
                ViewBag.CreditLimitRemaining = 500000000 - totalDebt;

                ViewBag.RecentActivities = _repo.GetRecentActivities(model.IDChungTuBanHang);

                return PartialView("Collect", model);
            }

            try
            {
                var user = GetCurrentUser();
                int userId = user?.IDNhanSu ?? 0;

                if (model.ID == 0)
                {
                    model.SoPhieuThu = _repo.GenerateSoPhieuThu();
                }
                else if (string.IsNullOrEmpty(model.SoPhieuThu))
                {
                    model.SoPhieuThu = _repo.GenerateSoPhieuThu();
                }

                model.TrangThai = 1; // Luôn lưu ở trạng thái Đề nghị ghi trước
                int savedId = _repo.Save(model, userId);

                // Ghi nhận Audit Log
                if (model.ID == 0)
                {
                    AuditLog.AddInsert("BAN_PhieuThuKhachHang", savedId.ToString(), model);
                }
                else
                {
                    var oldModel = _repo.GetByID(model.ID);
                    AuditLog.AddUpdate("BAN_PhieuThuKhachHang", model.ID.ToString(), oldModel, model);
                }

                if (ghiSo)
                {
                    var oldModelGhi = _repo.GetByID(savedId);
                    _repo.GhiSo(savedId, userId);
                    var newModelGhi = _repo.GetByID(savedId);
                    AuditLog.AddUpdate("BAN_PhieuThuKhachHang", savedId.ToString(), oldModelGhi, newModelGhi);
                }

                return Json(new { success = true, id = savedId, message = ghiSo ? "Lưu và ghi sổ thành công" : "Lưu đề nghị ghi thành công", redirectUrl = Url.Action("Edit", "PhieuThuKhachHang", new { id = model.IDChungTuBanHang }) });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        // POST: PhieuThuKhachHang/GhiSo
        [HttpPost]
        public ActionResult GhiSo(int id)
        {
            if (!PermissionHelper.HasPermission("PhieuThuKhachHang", LoaiPhanQuyen.TuyChon))
                return Json(new { success = false, message = "Không có quyền thực hiện chức năng ghi sổ" });

            try
            {
                var user = GetCurrentUser();
                int userId = user?.IDNhanSu ?? 0;

                var model = _repo.GetByID(id);
                if (model == null) return Json(new { success = false, message = "Phiếu thu không tồn tại." });

                var oldModel = _repo.GetByID(id);
                _repo.GhiSo(id, userId);
                var newModel = _repo.GetByID(id);
                AuditLog.AddUpdate("BAN_PhieuThuKhachHang", id.ToString(), oldModel, newModel);

                return Json(new { success = true, message = "Ghi sổ thành công" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Ghi sổ thất bại: " + ex.Message });
            }
        }

        // POST: PhieuThuKhachHang/Huy
        [HttpPost]
        public ActionResult Huy(int id, string lyDo)
        {
            if (!PermissionHelper.HasPermission("PhieuThuKhachHang", LoaiPhanQuyen.TuyChon))
                return Json(new { success = false, message = "Không có quyền thực hiện chức năng hủy phiếu" });

            if (string.IsNullOrEmpty(lyDo))
                return Json(new { success = false, message = "Vui lòng nhập lý do hủy." });

            try
            {
                var user = GetCurrentUser();
                int userId = user?.IDNhanSu ?? 0;

                var model = _repo.GetByID(id);
                if (model == null) return Json(new { success = false, message = "Phiếu thu không tồn tại." });

                var oldModelHuy = _repo.GetByID(id);
                _repo.Huy(id, userId, lyDo);
                var newModelHuy = _repo.GetByID(id);
                AuditLog.AddUpdate("BAN_PhieuThuKhachHang", id.ToString(), oldModelHuy, newModelHuy);

                return Json(new { success = true, message = "Hủy phiếu thu thành công" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Hủy thất bại: " + ex.Message });
            }
        }

        // POST: PhieuThuKhachHang/Delete
        [HttpPost]
        public ActionResult Delete(int id)
        {
            if (!PermissionHelper.HasPermission("PhieuThuKhachHang", LoaiPhanQuyen.Xoa))
                return Json(new { success = false, message = "Không có quyền thực hiện chức năng xóa" });

            try
            {
                var user = GetCurrentUser();
                int userId = user?.IDNhanSu ?? 0;

                var model = _repo.GetByID(id);
                if (model == null) return Json(new { success = false, message = "Phiếu thu không tồn tại." });

                _repo.Delete(id, userId);
                AuditLog.AddDelete("BAN_PhieuThuKhachHang", id.ToString(), model);

                return Json(new { success = true, message = "Xóa phiếu thu thành công" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Xóa thất bại: " + ex.Message });
            }
        }

        private IEnumerable<SalesManagementSystem.Models.ViewModels.KhachHangViewModel> GetKhachHangs()
        {
            var repo = new SalesManagementSystem.Repositories.KhachHangRepository(new SalesManagementSystem.Data.DbConnectionFactory());
            return repo.GetPaged(1, 10000, "", out int totalKhs).ToList();
        }

        [HttpGet]
        public ActionResult TestKhachHang()
        {
            try
            { 
                var list = GetKhachHangs().ToList();
                return Json(new { count = list.Count, first = list.FirstOrDefault()?.TenKhachHang }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        private class TaiKhoanDropdownItem
        {
            public int ID { get; set; }
            public string TenHienThi { get; set; }
        }

        private class NhanSuDropdownItem
        {
            public int ID { get; set; }
            public string HoTen { get; set; }
        }
    }
}
