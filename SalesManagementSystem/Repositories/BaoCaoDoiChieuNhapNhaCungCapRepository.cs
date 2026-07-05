using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Dapper;
using SalesManagementSystem.Data;
using SalesManagementSystem.Models.ViewModels;
using SalesManagementSystem.Repositories.Interfaces;

namespace SalesManagementSystem.Repositories
{
    public class BaoCaoDoiChieuNhapNhaCungCapRepository : IBaoCaoDoiChieuNhapNhaCungCapRepository
    {
        private readonly DbConnectionFactory _db;

        public BaoCaoDoiChieuNhapNhaCungCapRepository(DbConnectionFactory db)
        {
            _db = db;
        }

        public IEnumerable<BaoCaoDoiChieuNhapNhaCungCapViewModel> GetList(int? idNhaCungCap, DateTime tuNgay, DateTime denNgay)
        {
            using (var conn = _db.CreateConnection())
            {
                // Tự động tạo SP nếu tồn tại file script (mô phỏng migration)
                try {
                    string sqlPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data", "sp_BaoCao_DoiChieuNhapNhaCungCap.sql");
                    if (System.IO.File.Exists(sqlPath)) {
                        string sql = System.IO.File.ReadAllText(sqlPath);
                        var parts = sql.Split(new[] { "\r\nGO", "\nGO", "GO\r\n", "GO\n" }, StringSplitOptions.RemoveEmptyEntries);
                        foreach(var part in parts) {
                            if (!string.IsNullOrWhiteSpace(part)) {
                                conn.Execute(part);
                            }
                        }
                        // Sau khi chạy xong đổi tên file hoặc xoá để tránh chạy lại lần sau
                        System.IO.File.Move(sqlPath, sqlPath + ".executed");
                    }
                    
                    // Script insert menu (ACL)
                    string aclPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data", "insert_acl_BaoCaoDoiChieuNhapNhaCungCap.sql");
                    if (System.IO.File.Exists(aclPath)) {
                        string sql = System.IO.File.ReadAllText(aclPath);
                        var parts = sql.Split(new[] { "\r\nGO", "\nGO", "GO\r\n", "GO\n" }, StringSplitOptions.RemoveEmptyEntries);
                        foreach(var part in parts) {
                            if (!string.IsNullOrWhiteSpace(part)) {
                                conn.Execute(part);
                            }
                        }
                        System.IO.File.Move(aclPath, aclPath + ".executed");
                    }
                } catch { }

                var p = new DynamicParameters();
                p.Add("@IDNhaCungCap", idNhaCungCap);
                p.Add("@TuNgay", tuNgay);
                p.Add("@DenNgay", denNgay);

                return conn.Query<BaoCaoDoiChieuNhapNhaCungCapViewModel>(
                    "sp_BaoCao_DoiChieuNhapNhaCungCap",
                    p,
                    commandType: CommandType.StoredProcedure
                ).ToList();
            }
        }

        public IEnumerable<dynamic> GetNhaCungCapDropdown()
        {
            using (var conn = _db.CreateConnection())
            {
                return conn.Query<dynamic>(
                    "SELECT ID, ISNULL(MaNhaCungCap, '') + ' - ' + ISNULL(TenNhaCungCap, '') AS TenHienThi FROM DM_NhaCungCap ORDER BY TenNhaCungCap"
                ).ToList();
            }
        }
    }
}
