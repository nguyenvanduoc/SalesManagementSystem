using System.Collections.Generic;

namespace SalesManagementSystem.Models.ViewModels
{
    /// <summary>Một mục đơn lẻ trong sidebar (1 màn hình + link điều hướng).</summary>
    public class MenuItemVM
    {
        public int IDManHinh { get; set; }
        public string TenManHinh { get; set; }

        /// <summary>Controller của action Index (dùng để tạo link và active state).</summary>
        public string TenController { get; set; }

        /// <summary>Action mặc định khi click vào menu item (thường là "Index").</summary>
        public string TenAction { get; set; }
    }

    /// <summary>Một nhóm menu cha (VD: BÁN HÀNG, KHO BÃI, BÁO CÁO).</summary>
    public class MenuGroupVM
    {
        public string TenNhom { get; set; }
        public List<MenuItemVM> Items { get; set; } = new List<MenuItemVM>();
    }

    /// <summary>ViewModel toàn bộ sidebar — truyền vào _Menu.cshtml partial.</summary>
    public class SidebarVM
    {
        public List<MenuGroupVM> Groups { get; set; } = new List<MenuGroupVM>();

        /// <summary>Controller đang active để highlight menu item.</summary>
        public string ActiveController { get; set; }

        /// <summary>Action đang active để highlight chi tiết hơn nếu cần.</summary>
        public string ActiveAction { get; set; }
    }
}
