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

            // Phân quyền
            routes.MapRoute(name: "PhanQuyen_Index", url: "phan-quyen", defaults: new { controller = "PhanQuyen", action = "Index" });
            routes.MapRoute(name: "PhanQuyen_GetGrid", url: "phan-quyen/lay-luoi-quyen", defaults: new { controller = "PhanQuyen", action = "GetGrid" });
            routes.MapRoute(name: "PhanQuyen_Save", url: "phan-quyen/luu", defaults: new { controller = "", action = "Save" });

            // Biểu mẫu
            routes.MapRoute(name: "DMBieuMau_Index", url: "danh-muc-bieu-mau", defaults: new { controller = "DMBieuMau", action = "Index" });
            routes.MapRoute(name: "DMBieuMau_CreateEdit", url: "danh-muc-bieu-mau/cap-nhat", defaults: new { controller = "DMBieuMau", action = "CreateEdit" });
            routes.MapRoute(name: "DMBieuMau_Delete", url: "danh-muc-bieu-mau/xoa", defaults: new { controller = "DMBieuMau", action = "Delete" });
            routes.MapRoute(name: "DMBieuMau_Download", url: "danh-muc-bieu-mau/tai-ve", defaults: new { controller = "DMBieuMau", action = "Download" });

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}
