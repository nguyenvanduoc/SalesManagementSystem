using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Dapper;
using SalesManagementSystem.Data;
using SalesManagementSystem.Models.Entities;

namespace SalesManagementSystem.Repositories
{
    public class PhongBanRepository
    {
        private readonly DbConnectionFactory _db;

        public PhongBanRepository(DbConnectionFactory db)
        {
            _db = db;
        }

        public IEnumerable<PhongBan> GetPaged(int page, int pageSize, string keyword, out int totalRecords)
        {
            using (var conn = _db.CreateConnection())
            {
                var parameters = new DynamicParameters();
                parameters.Add("@Keyword", string.IsNullOrEmpty(keyword) ? "" : keyword.ToLower());
                parameters.Add("@Offset", (page - 1) * pageSize);
                parameters.Add("@PageSize", pageSize);

                var countSql = @"
                    SELECT COUNT(*) 
                    FROM DM_PhongBan
                    WHERE (@Keyword = '' OR LOWER(TenPhongBan) LIKE '%' + @Keyword + '%' OR LOWER(MaPhongBan) LIKE '%' + @Keyword + '%')";

                totalRecords = conn.ExecuteScalar<int>(countSql, parameters);

                var sql = @"
                    SELECT * 
                    FROM DM_PhongBan
                    WHERE (@Keyword = '' OR LOWER(TenPhongBan) LIKE '%' + @Keyword + '%' OR LOWER(MaPhongBan) LIKE '%' + @Keyword + '%')
                    ORDER BY STT ASC, TenPhongBan ASC
                    OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

                return conn.Query<PhongBan>(sql, parameters);
            }
        }

        public PhongBan GetById(int id)
        {
            using (var conn = _db.CreateConnection())
            {
                return conn.QueryFirstOrDefault<PhongBan>("SELECT * FROM DM_PhongBan WHERE ID = @ID", new { ID = id });
            }
        }

        public bool IsDuplicateCode(string maPhongBan, int currentId = 0)
        {
            using (var conn = _db.CreateConnection())
            {
                var sql = "SELECT COUNT(1) FROM DM_PhongBan WHERE MaPhongBan = @MaPhongBan AND ID != @CurrentId";
                return conn.ExecuteScalar<int>(sql, new { MaPhongBan = maPhongBan, CurrentId = currentId }) > 0;
            }
        }

        public int Insert(PhongBan entity)
        {
            using (var conn = _db.CreateConnection())
            {
                var sql = @"
                    INSERT INTO DM_PhongBan (MaPhongBan, TenPhongBan, STT, NgayTao, NguoiTao)
                    VALUES (@MaPhongBan, @TenPhongBan, @STT, @NgayTao, @NguoiTao);
                    SELECT CAST(SCOPE_IDENTITY() as int);";

                entity.NgayTao = DateTime.Now;
                return conn.ExecuteScalar<int>(sql, entity);
            }
        }

        public int Update(PhongBan entity)
        {
            using (var conn = _db.CreateConnection())
            {
                var sql = @"
                    UPDATE DM_PhongBan 
                    SET MaPhongBan = @MaPhongBan, 
                        TenPhongBan = @TenPhongBan, 
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
                return conn.Execute("DELETE FROM DM_PhongBan WHERE ID = @ID", new { ID = id });
            }
        }
    }
}
