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
    /// Quy tắc Antigravity: KHÔNG dùng Interface — đăng ký trực tiếp Concrete Class.
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
            container.RegisterType<ProductRepository>(new HierarchicalLifetimeManager());
            container.RegisterType<OrderRepository>(new HierarchicalLifetimeManager());
            container.RegisterType<MenuRepository>(new HierarchicalLifetimeManager());

            // ── Services (logic nghiệp vụ) ────────────────────────────────────
            container.RegisterType<InventoryService>(new HierarchicalLifetimeManager());
            container.RegisterType<OrderService>(new HierarchicalLifetimeManager());

            DependencyResolver.SetResolver(new UnityDependencyResolver(container));
        }
    }
}
