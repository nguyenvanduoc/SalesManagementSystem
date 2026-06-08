using System.Collections.Generic;
using System.Linq;
using Dapper;
using SalesManagementSystem.Data;
using SalesManagementSystem.Models.ViewModels;
using SalesManagementSystem.Repositories.Interfaces;
using System.Text;
using System.Text.RegularExpressions;

namespace SalesManagementSystem.Repositories
{
    /// <summary>
    /// Repository duy nhất chứa SQL đọc menu động từ ACL_ManHinh + ACL_Action.
    /// Không có logic nghiệp vụ — chỉ query và map dữ liệu.
    /// </summary>
    public class MenuRepository : IMenuRepository
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

        public List<MenuSearchResultVM> SearchMenu(string keyword)
        {
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
                ORDER BY m.NhomChaManHinh, m.STT";

            IEnumerable<dynamic> rows;
            using (var conn = _db.CreateConnection())
                rows = conn.Query(sql).ToList();

            string normalizedKeyword = RemoveDiacritics(keyword ?? "").ToLower();

            var results = new List<MenuSearchResultVM>();

            foreach (var r in rows)
            {
                string tenManHinh = r.TenManHinh;
                string nhomCha = r.NhomChaManHinh;
                string controller = r.TenController;
                string action = r.TenAction;

                string normalizedTen = RemoveDiacritics(tenManHinh ?? "").ToLower();
                string normalizedNhom = RemoveDiacritics(nhomCha ?? "").ToLower();

                if (normalizedTen.Contains(normalizedKeyword) || normalizedNhom.Contains(normalizedKeyword))
                {
                    results.Add(new MenuSearchResultVM
                    {
                        IDManHinh = (int)r.IDManHinh,
                        TenManHinh = tenManHinh,
                        Breadcrumb = $"{nhomCha} > {tenManHinh}",
                        DuongDan = string.IsNullOrEmpty(controller) ? "#" : $"/{controller}/{action}",
                        TenController = controller
                    });
                }
            }

            return results;
        }

        private string RemoveDiacritics(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return text;
            
            // Normalize Unicode
            text = text.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder();

            foreach (var c in text)
            {
                if (System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c) != System.Globalization.UnicodeCategory.NonSpacingMark)
                {
                    sb.Append(c);
                }
            }

            // Remove D with stroke (Đ/đ)
            string result = sb.ToString().Normalize(NormalizationForm.FormC);
            result = result.Replace("Đ", "D").Replace("đ", "d");
            
            return result;
        }
    }
}
