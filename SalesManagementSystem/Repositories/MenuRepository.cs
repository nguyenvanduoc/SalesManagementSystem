using System.Collections.Generic;
using System.Linq;
using Dapper;
using SalesManagementSystem.Data;
using SalesManagementSystem.Models.ViewModels;

namespace SalesManagementSystem.Repositories
{
    /// <summary>
    /// Repository duy nhất chứa SQL đọc menu động từ ACL_ManHinh + ACL_Action.
    /// Không có logic nghiệp vụ — chỉ query và map dữ liệu.
    /// </summary>
    public class MenuRepository
    {
        private readonly DbConnectionFactory _db;

        public MenuRepository(DbConnectionFactory db)
        {
            _db = db;
        }

        /// <summary>
        /// Đọc toàn bộ menu sidebar:
        /// - Lấy các màn hình đang IsSuDung = 1 từ ACL_ManHinh.
        /// - JOIN với ACL_Action để lấy TenController và TenAction đầu tiên
        ///   (dùng làm link điều hướng khi click vào menu item).
        /// - Group theo NhomChaManHinh để render từng section sidebar.
        /// </summary>
        public List<MenuGroupVM> GetSidebarGroups()
        {
            // Lấy action đầu tiên (ID nhỏ nhất) của mỗi màn hình làm link chính
            const string sql = @"
                SELECT
                    m.ID            AS IDManHinh,
                    m.TenManHinh,
                    m.NhomChaManHinh,
                    ISNULL(a.TenController, '')  AS TenController,
                    ISNULL(a.TenAction, 'Index') AS TenAction
                FROM ACL_ManHinh m
                LEFT JOIN ACL_Action a
                    ON a.ID = (
                        SELECT TOP 1 ID
                        FROM ACL_Action
                        WHERE IDManHinh = m.ID
                        ORDER BY ID ASC
                    )
                WHERE m.IsSuDung = 1
                ORDER BY  m.STT,m.NhomChaManHinh, m.ID";

            IEnumerable<dynamic> rows;
            using (var conn = _db.CreateConnection())
                rows = conn.Query(sql).ToList();

            // Map sang ViewModel — group theo NhomChaManHinh
            var groups = rows
                .GroupBy(r => (string)r.NhomChaManHinh)
                .Select(g => new MenuGroupVM
                {
                    TenNhom = g.Key,
                    Items = g.Select(r => new MenuItemVM
                    {
                        IDManHinh    = (int)r.IDManHinh,
                        TenManHinh   = (string)r.TenManHinh,
                        TenController = (string)r.TenController,
                        TenAction    = (string)r.TenAction
                    }).ToList()
                })
                .ToList();

            return groups;
        }
    }
}
