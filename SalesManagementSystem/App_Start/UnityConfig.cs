using System.Web.Mvc;
using Unity;
using Unity.Mvc5;
using SalesManagementSystem.Data;
using SalesManagementSystem.Repositories;
using SalesManagementSystem.Services;
using Unity.Lifetime;

namespace SalesManagementSystem.App_Start
{
    /// <summary>
    /// Cấu hình Unity DI Container cho ASP.NET MVC 5.
    /// Sử dụng Interface cho các Repository để đảm bảo tính tường minh.
    /// </summary>
    public static class UnityConfig
    {
        public static void RegisterComponents()
        {
            var container = new UnityContainer();

            // ── Infrastructure ────────────────────────────────────────────────
            // HierarchicalLifetimeManager ≈ Scoped (mỗi HTTP request)
            container.RegisterType<DbConnectionFactory>(new HierarchicalLifetimeManager());

            // ── Repositories (nơi duy nhất chứa SQL + Dapper) ────────────────
            //container.RegisterType<ProductRepository>(new HierarchicalLifetimeManager());
            //container.RegisterType<OrderRepository>(new HierarchicalLifetimeManager());
            container.RegisterType<SalesManagementSystem.Repositories.Interfaces.IMenuRepository, MenuRepository>(new HierarchicalLifetimeManager());
            container.RegisterType<SalesManagementSystem.Repositories.Interfaces.INhanSuRepository, NhanSuRepository>(new HierarchicalLifetimeManager());
            container.RegisterType<SalesManagementSystem.Repositories.Interfaces.IChucVuRepository, ChucVuRepository>(new HierarchicalLifetimeManager());
            container.RegisterType<SalesManagementSystem.Repositories.Interfaces.IPhongBanRepository, PhongBanRepository>(new HierarchicalLifetimeManager());
            container.RegisterType<SalesManagementSystem.Repositories.Interfaces.IAclLoginRepository, AclLoginRepository>(new HierarchicalLifetimeManager());
            container.RegisterType<SalesManagementSystem.Repositories.Interfaces.IAclPhanQuyenRepository, AclPhanQuyenRepository>(new HierarchicalLifetimeManager());
            container.RegisterType<SalesManagementSystem.Repositories.Interfaces.IAclManHinhRepository, AclManHinhRepository>(new HierarchicalLifetimeManager());
            container.RegisterType<SalesManagementSystem.Repositories.Interfaces.IAclActionRepository, AclActionRepository>(new HierarchicalLifetimeManager());
            container.RegisterType<SalesManagementSystem.Repositories.Interfaces.INKTongHopRepository, NKTongHopRepository>(new HierarchicalLifetimeManager());
            container.RegisterType<SalesManagementSystem.Repositories.Interfaces.IAclLoginSessionRepository, AclLoginSessionRepository>(new HierarchicalLifetimeManager());
            container.RegisterType<SalesManagementSystem.Repositories.Interfaces.IDMBieuMauRepository, DMBieuMauRepository>(new HierarchicalLifetimeManager());
            container.RegisterType<SalesManagementSystem.Repositories.Interfaces.IKhachHangRepository, KhachHangRepository>(new HierarchicalLifetimeManager());
            container.RegisterType<SalesManagementSystem.Repositories.Interfaces.IDmSanPhamRepository, DmSanPhamRepository>(new HierarchicalLifetimeManager());
            container.RegisterType<SalesManagementSystem.Repositories.Interfaces.IDmKhoHangRepository, DmKhoHangRepository>(new HierarchicalLifetimeManager());
            container.RegisterType<SalesManagementSystem.Repositories.Interfaces.IDonDatHangRepository, DonDatHangRepository>(new HierarchicalLifetimeManager());
            container.RegisterType<SalesManagementSystem.Repositories.Interfaces.IDonDieuChinhDonHangRepository, DonDieuChinhDonHangRepository>(new HierarchicalLifetimeManager());
            container.RegisterType<SalesManagementSystem.Repositories.Interfaces.IDieuChinhNhapKhoRepository, DieuChinhNhapKhoRepository>(new HierarchicalLifetimeManager());
            container.RegisterType<SalesManagementSystem.Repositories.Interfaces.IPhieuNhapKhoRepository, SalesManagementSystem.Repositories.PhieuNhapKhoRepository>(new HierarchicalLifetimeManager());
            container.RegisterType<SalesManagementSystem.Repositories.Interfaces.INhaCungCapRepository, NhaCungCapRepository>(new HierarchicalLifetimeManager());
            container.RegisterType<SalesManagementSystem.Repositories.Interfaces.ITonKhoRepository, TonKhoRepository>(new HierarchicalLifetimeManager());
            container.RegisterType<SalesManagementSystem.Repositories.Interfaces.IChungTuBanHangRepository, ChungTuBanHangRepository>(new HierarchicalLifetimeManager());
            container.RegisterType<SalesManagementSystem.Repositories.Interfaces.INhatKyChungRepository, NhatKyChungRepository>(new HierarchicalLifetimeManager());
            container.RegisterType<SalesManagementSystem.Repositories.Interfaces.ITaiKhoanKeToanRepository, TaiKhoanKeToanRepository>(new HierarchicalLifetimeManager());
            container.RegisterType<SalesManagementSystem.Repositories.Interfaces.IPhieuXuatKhoRepository, PhieuXuatKhoRepository>(new HierarchicalLifetimeManager());
            container.RegisterType<SalesManagementSystem.Repositories.Interfaces.IPhieuThuKhachHangRepository, PhieuThuKhachHangRepository>(new HierarchicalLifetimeManager());
            container.RegisterType<SalesManagementSystem.Repositories.Interfaces.ITaiKhoanThanhToanRepository, TaiKhoanThanhToanRepository>(new HierarchicalLifetimeManager());
            container.RegisterType<SalesManagementSystem.Repositories.Interfaces.IPhieuChiRepository, PhieuChiRepository>(new HierarchicalLifetimeManager());
            container.RegisterType<SalesManagementSystem.Repositories.Interfaces.ISoQuyRepository, SoQuyRepository>(new HierarchicalLifetimeManager());
            container.RegisterType<SalesManagementSystem.Repositories.Interfaces.ICongNoNCCRepository, CongNoNCCRepository>(new HierarchicalLifetimeManager());
            container.RegisterType<SalesManagementSystem.Repositories.Interfaces.IDashboardRepository, DashboardRepository>(new HierarchicalLifetimeManager());
            container.RegisterType<SalesManagementSystem.Repositories.Interfaces.IPhuongTienRepository, PhuongTienRepository>(new HierarchicalLifetimeManager());
            container.RegisterType<SalesManagementSystem.Repositories.Interfaces.IHopDongKhachHangRepository, HopDongKhachHangRepository>(new HierarchicalLifetimeManager());
            container.RegisterType<SalesManagementSystem.Repositories.Interfaces.ITraHangBanRepository, TraHangBanRepository>(new HierarchicalLifetimeManager());
            // ── Services (logic nghiệp vụ) ────────────────────────────────────
            container.RegisterType<InventoryService>(new HierarchicalLifetimeManager());
            container.RegisterType<SalesManagementSystem.Services.Interfaces.IExcelExportService, ExcelExportService>(new HierarchicalLifetimeManager());
            container.RegisterType<SalesManagementSystem.Services.Interfaces.IWordExportService, WordExportService>(new HierarchicalLifetimeManager());
            //container.RegisterType<OrderService>(new HierarchicalLifetimeManager());

            DependencyResolver.SetResolver(new UnityDependencyResolver(container));
        }
    }
}
