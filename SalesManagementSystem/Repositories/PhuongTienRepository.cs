using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Dapper;
using SalesManagementSystem.Data;
using SalesManagementSystem.Models.Entities;
using SalesManagementSystem.Repositories.Interfaces;

namespace SalesManagementSystem.Repositories
{
    public class PhuongTienRepository : IPhuongTienRepository
    {
        private readonly DbConnectionFactory _db;

        public PhuongTienRepository(DbConnectionFactory db)
        {
            _db = db;
        }

        public IEnumerable<PhuongTien> GetAll()
        {
            const string sql = "SELECT ID, MaPhuongTien, TenPhuongTien, STT, NgayTao, NguoiTao, NgayCapNhat, NguoiCapNhat FROM DM_PhuongTien ORDER BY TenPhuongTien";
            using (var conn = _db.CreateConnection())
            {
                return conn.Query<PhuongTien>(sql);
            }
        }

        public IEnumerable<PhuongTien> GetPaged(int page, int pageSize, string keyword, out int totalRecords)
        {
            using (var conn = _db.CreateConnection())
            {
                var parameters = new DynamicParameters();
                parameters.Add("@Keyword", string.IsNullOrEmpty(keyword) ? "" : keyword.ToLower());
                parameters.Add("@Offset", (page - 1) * pageSize);
                parameters.Add("@PageSize", pageSize);

                var countSql = @"
                    SELECT COUNT(*) 
                    FROM DM_PhuongTien
                    WHERE (@Keyword = '' OR LOWER(TenPhuongTien) LIKE '%' + @Keyword + '%' OR LOWER(MaPhuongTien) LIKE '%' + @Keyword + '%')";

                totalRecords = conn.ExecuteScalar<int>(countSql, parameters);

                var sql = @"
                    SELECT ID, MaPhuongTien, TenPhuongTien, STT, NgayTao, NguoiTao, NgayCapNhat, NguoiCapNhat
                    FROM DM_PhuongTien
                    WHERE (@Keyword = '' OR LOWER(TenPhuongTien) LIKE '%' + @Keyword + '%' OR LOWER(MaPhuongTien) LIKE '%' + @Keyword + '%')
                    ORDER BY ISNULL(STT, 9999), NgayTao DESC
                    OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

                return conn.Query<PhuongTien>(sql, parameters);
            }
        }

        public PhuongTien GetById(int id)
        {
            using (var conn = _db.CreateConnection())
            {
                return conn.QueryFirstOrDefault<PhuongTien>("SELECT ID, MaPhuongTien, TenPhuongTien, STT, NgayTao, NguoiTao, NgayCapNhat, NguoiCapNhat FROM DM_PhuongTien WHERE ID = @ID", new { ID = id });
            }
        }

        public bool IsDuplicateCode(string maPhuongTien, int currentId = 0)
        {
            using (var conn = _db.CreateConnection())
            {
                var sql = "SELECT COUNT(1) FROM DM_PhuongTien WHERE MaPhuongTien = @MaPhuongTien AND ID != @CurrentId";
                return conn.ExecuteScalar<int>(sql, new { MaPhuongTien = maPhuongTien, CurrentId = currentId }) > 0;
            }
        }

        public int Insert(PhuongTien entity)
        {
            using (var conn = _db.CreateConnection())
            {
                var sql = @"
                    INSERT INTO DM_PhuongTien (MaPhuongTien, TenPhuongTien, STT, NgayTao, NguoiTao)
                    VALUES (@MaPhuongTien, @TenPhuongTien, @STT, @NgayTao, @NguoiTao);
                    SELECT CAST(SCOPE_IDENTITY() as int);";

                entity.NgayTao = DateTime.Now;
                return conn.ExecuteScalar<int>(sql, entity);
            }
        }

        public int Update(PhuongTien entity)
        {
            using (var conn = _db.CreateConnection())
            {
                var sql = @"
                    UPDATE DM_PhuongTien 
                    SET MaPhuongTien = @MaPhuongTien, 
                        TenPhuongTien = @TenPhuongTien, 
                        STT = @STT, 
                        NgayCapNhat = @NgayCapNhat, 
                        NguoiCapNhat = @NguoiCapNhat
                    WHERE ID = @ID";

                entity.NgayCapNhat = DateTime.Now;
                return conn.Execute(sql, entity);
            }
        }

        public int Delete(int id)
        {
            using (var conn = _db.CreateConnection())
            {
                return conn.Execute("DELETE FROM DM_PhuongTien WHERE ID = @ID", new { ID = id });
            }
        }
    }
}
