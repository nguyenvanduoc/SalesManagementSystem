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
            container.RegisterType<SalesManagementSystem.Repositories.Interfaces.INhanVienRepository, NhanVienRepository>(new HierarchicalLifetimeManager());
            container.RegisterType<SalesManagementSystem.Repositories.Interfaces.IChucVuRepository, ChucVuRepository>(new HierarchicalLifetimeManager());
            container.RegisterType<SalesManagementSystem.Repositories.Interfaces.IPhongBanRepository, PhongBanRepository>(new HierarchicalLifetimeManager());

            // ── Services (logic nghiệp vụ) ────────────────────────────────────
            container.RegisterType<InventoryService>(new HierarchicalLifetimeManager());
            //container.RegisterType<OrderService>(new HierarchicalLifetimeManager());

            DependencyResolver.SetResolver(new UnityDependencyResolver(container));
        }
    }
}
