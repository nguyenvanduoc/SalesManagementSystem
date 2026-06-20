using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace SalesManagementSystem
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            // Trang chủ
            routes.MapRoute(name: "Root", url: "", defaults: new { controller = "Home", action = "Index" });
            routes.MapRoute(name: "Home_Index", url: "trang-chu/tong-quan", defaults: new { controller = "Home", action = "Index" });
            routes.MapRoute(name: "Home_About", url: "trang-chu/gioi-thieu", defaults: new { controller = "Home", action = "About" });
            routes.MapRoute(name: "Home_Contact", url: "trang-chu/lien-he", defaults: new { controller = "Home", action = "Contact" });
            routes.MapRoute(name: "Home_ClearCache", url: "trang-chu/xoa-bo-nho-dem", defaults: new { controller = "Home", action = "ClearCache" });

            // Đăng nhập
            routes.MapRoute(name: "Login_Index", url: "Login", defaults: new { controller = "Login", action = "Index" });
            routes.MapRoute(name: "Login_Logout", url: "Logout", defaults: new { controller = "Login", action = "Logout" });

            // Phòng ban
            routes.MapRoute(name: "PhongBan_List", url: "phong-ban", defaults: new { controller = "PhongBan", action = "GetPhongBan" });
            routes.MapRoute(name: "PhongBan_Create", url: "phong-ban/them-moi", defaults: new { controller = "PhongBan", action = "CreatePhongBan" });
            routes.MapRoute(name: "PhongBan_Update", url: "phong-ban/cap-nhat", defaults: new { controller = "PhongBan", action = "UpdatePhongBan" });
            routes.MapRoute(name: "PhongBan_Delete", url: "phong-ban/xoa", defaults: new { controller = "PhongBan", action = "DeletePhongBan" });

            // Chức vụ
            routes.MapRoute(name: "ChucVu_List", url: "chuc-vu", defaults: new { controller = "ChucVu", action = "GetChucVu" });
            routes.MapRoute(name: "ChucVu_Create", url: "chuc-vu/them-moi", defaults: new { controller = "ChucVu", action = "CreateChucVu" });
            routes.MapRoute(name: "ChucVu_Update", url: "chuc-vu/cap-nhat", defaults: new { controller = "ChucVu", action = "UpdateChucVu" });
            routes.MapRoute(name: "ChucVu_Delete", url: "chuc-vu/xoa", defaults: new { controller = "ChucVu", action = "DeleteChucVu" });

            // nhân sự
            routes.MapRoute(name: "NhanSu_List", url: "nhan-vien", defaults: new { controller = "NhanSu", action = "Index" });
            routes.MapRoute(name: "NhanSu_Create", url: "nhan-vien/them-moi", defaults: new { controller = "NhanSu", action = "Create" });
            routes.MapRoute(name: "NhanSu_Update", url: "nhan-vien/cap-nhat", defaults: new { controller = "NhanSu", action = "Update" });
            routes.MapRoute(name: "NhanSu_Delete", url: "nhan-vien/xoa", defaults: new { controller = "NhanSu", action = "Delete" });
            routes.MapRoute(name: "NhanSu_BatchDelete", url: "nhan-vien/xoa-nhieu", defaults: new { controller = "NhanSu", action = "BatchDelete" });

            // Người dùng
            routes.MapRoute(name: "NguoiDung_List", url: "nguoi-dung", defaults: new { controller = "NguoiDung", action = "GetNguoiDung" });
            routes.MapRoute(name: "NguoiDung_Create", url: "nguoi-dung/them-moi", defaults: new { controller = "NguoiDung", action = "CreateNguoiDung" });
            routes.MapRoute(name: "NguoiDung_Update", url: "nguoi-dung/cap-nhat", defaults: new { controller = "NguoiDung", action = "EditNguoiDung" });
            routes.MapRoute(name: "NguoiDung_Delete", url: "nguoi-dung/xoa", defaults: new { controller = "NguoiDung", action = "DeleteNguoiDung" });
            routes.MapRoute(name: "NguoiDung_ChangePassword", url: "nguoi-dung/doi-mat-khau", defaults: new { controller = "NguoiDung", action = "ChangePassword" });

            // AclManHinh
            routes.MapRoute(name: "AclManHinh_List", url: "AclManHinh", defaults: new { controller = "AclManHinh", action = "GetManHinh" });
            
            // AclAction
            routes.MapRoute(name: "AclAction_List", url: "AclAction", defaults: new { controller = "AclAction", action = "GetAction" });

            // Phân quyền
            routes.MapRoute(name: "PhanQuyen_Index", url: "phan-quyen", defaults: new { controller = "PhanQuyen", action = "Index" });
            routes.MapRoute(name: "PhanQuyen_GetGrid", url: "phan-quyen/lay-luoi-quyen", defaults: new { controller = "PhanQuyen", action = "GetGrid" });
            routes.MapRoute(name: "PhanQuyen_Save", url: "phan-quyen/luu", defaults: new { controller = "", action = "Save" });

            // Biểu mẫu
            routes.MapRoute(name: "DMBieuMau_Index", url: "danh-muc-bieu-mau", defaults: new { controller = "DMBieuMau", action = "Index" });
            routes.MapRoute(name: "DMBieuMau_CreateEdit", url: "danh-muc-bieu-mau/cap-nhat", defaults: new { controller = "DMBieuMau", action = "CreateEdit" });
            routes.MapRoute(name: "DMBieuMau_Delete", url: "danh-muc-bieu-mau/xoa", defaults: new { controller = "DMBieuMau", action = "Delete" });
            routes.MapRoute(name: "DMBieuMau_Download", url: "danh-muc-bieu-mau/tai-ve", defaults: new { controller = "DMBieuMau", action = "Download" });

            // Phiếu thu khách hàng
            routes.MapRoute(name: "PhieuThuKhachHang_List", url: "phieu-thu", defaults: new { controller = "PhieuThuKhachHang", action = "Index" });
            routes.MapRoute(name: "PhieuThuKhachHang_GetData", url: "phieu-thu/danh-sach", defaults: new { controller = "PhieuThuKhachHang", action = "GetList" });
            routes.MapRoute(name: "PhieuThuKhachHang_Create", url: "phieu-thu/them-moi", defaults: new { controller = "PhieuThuKhachHang", action = "Create" });
            routes.MapRoute(name: "PhieuThuKhachHang_Update", url: "phieu-thu/cap-nhat", defaults: new { controller = "PhieuThuKhachHang", action = "Edit" });
            routes.MapRoute(name: "PhieuThuKhachHang_Save", url: "phieu-thu/save", defaults: new { controller = "PhieuThuKhachHang", action = "Save" });
            routes.MapRoute(name: "PhieuThuKhachHang_GhiSo", url: "phieu-thu/ghi-so", defaults: new { controller = "PhieuThuKhachHang", action = "GhiSo" });
            routes.MapRoute(name: "PhieuThuKhachHang_Huy", url: "phieu-thu/huy", defaults: new { controller = "PhieuThuKhachHang", action = "Huy" });
            routes.MapRoute(name: "PhieuThuKhachHang_Delete", url: "phieu-thu/xoa", defaults: new { controller = "PhieuThuKhachHang", action = "Delete" });
            routes.MapRoute(name: "PhieuThuKhachHang_GetCongNo", url: "phieu-thu/get-cong-no", defaults: new { controller = "PhieuThuKhachHang", action = "GetCongNoChungTu" });

            // Tài khoản thanh toán
            routes.MapRoute(name: "TaiKhoanThanhToan_List", url: "tai-khoan-thanh-toan", defaults: new { controller = "TaiKhoanThanhToan", action = "Index" });
            routes.MapRoute(name: "TaiKhoanThanhToan_GetList", url: "tai-khoan-thanh-toan/danh-sach", defaults: new { controller = "TaiKhoanThanhToan", action = "GetList" });
            routes.MapRoute(name: "TaiKhoanThanhToan_Create", url: "tai-khoan-thanh-toan/them-moi", defaults: new { controller = "TaiKhoanThanhToan", action = "Create" });
            routes.MapRoute(name: "TaiKhoanThanhToan_Update", url: "tai-khoan-thanh-toan/cap-nhat", defaults: new { controller = "TaiKhoanThanhToan", action = "Edit" });
            routes.MapRoute(name: "TaiKhoanThanhToan_Save", url: "tai-khoan-thanh-toan/save", defaults: new { controller = "TaiKhoanThanhToan", action = "Save" });
            routes.MapRoute(name: "TaiKhoanThanhToan_Delete", url: "tai-khoan-thanh-toan/xoa", defaults: new { controller = "TaiKhoanThanhToan", action = "Delete" });

            // Phiếu Chi
            routes.MapRoute(name: "PhieuChi_List",    url: "phieu-chi",             defaults: new { controller = "PhieuChi", action = "Index" });
            routes.MapRoute(name: "PhieuChi_GetList", url: "phieu-chi/danh-sach",   defaults: new { controller = "PhieuChi", action = "GetList" });
            routes.MapRoute(name: "PhieuChi_Create",  url: "phieu-chi/them-moi",    defaults: new { controller = "PhieuChi", action = "Create" });
            routes.MapRoute(name: "PhieuChi_Update",  url: "phieu-chi/cap-nhat",    defaults: new { controller = "PhieuChi", action = "Edit" });
            routes.MapRoute(name: "PhieuChi_Details", url: "phieu-chi/chi-tiet",    defaults: new { controller = "PhieuChi", action = "Details" });
            routes.MapRoute(name: "PhieuChi_Save",    url: "phieu-chi/save",        defaults: new { controller = "PhieuChi", action = "Save" });
            routes.MapRoute(name: "PhieuChi_GhiSo",  url: "phieu-chi/ghi-so",      defaults: new { controller = "PhieuChi", action = "GhiSo" });
            routes.MapRoute(name: "PhieuChi_Huy",    url: "phieu-chi/huy",         defaults: new { controller = "PhieuChi", action = "Huy" });
            routes.MapRoute(name: "PhieuChi_Delete", url: "phieu-chi/xoa",         defaults: new { controller = "PhieuChi", action = "Delete" });
            routes.MapRoute(name: "PhieuChi_GetPhieuNhap", url: "phieu-chi/get-phieu-nhap", defaults: new { controller = "PhieuChi", action = "GetPhieuNhapByNCC" });
            routes.MapRoute(name: "PhieuChi_GetPhieuNhapDetail", url: "phieu-chi/get-phieu-nhap-detail", defaults: new { controller = "PhieuChi", action = "GetPhieuNhapDetail" });

            // Sổ Quỹ
            routes.MapRoute(name: "SoQuy_List",    url: "so-quy",           defaults: new { controller = "SoQuy", action = "Index" });
            routes.MapRoute(name: "SoQuy_GetList", url: "so-quy/danh-sach", defaults: new { controller = "SoQuy", action = "GetList" });
            routes.MapRoute(name: "SoQuy_Details", url: "so-quy/chi-tiet",  defaults: new { controller = "SoQuy", action = "Details" });
            routes.MapRoute(name: "SoQuy_ExportTongHop", url: "so-quy/xuat-excel-tong-hop", defaults: new { controller = "SoQuy", action = "ExportExcelTongHop" });
            routes.MapRoute(name: "SoQuy_ExportChiTiet", url: "so-quy/xuat-excel-chi-tiet", defaults: new { controller = "SoQuy", action = "ExportExcelChiTiet" });

            // Công Nợ Phải Trả NCC
            routes.MapRoute(name: "CongNoNCC_List",    url: "cong-no-ncc",           defaults: new { controller = "CongNoNCC", action = "Index" });
            routes.MapRoute(name: "CongNoNCC_GetList", url: "cong-no-ncc/danh-sach", defaults: new { controller = "CongNoNCC", action = "GetList" });

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}
