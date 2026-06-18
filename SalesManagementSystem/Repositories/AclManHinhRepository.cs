using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using SalesManagementSystem.Data;
using SalesManagementSystem.Models.Entities;
using SalesManagementSystem.Repositories.Interfaces;

namespace SalesManagementSystem.Repositories
{
    public class AclManHinhRepository : IAclManHinhRepository
    {
        private readonly DbConnectionFactory _db;

        public AclManHinhRepository(DbConnectionFactory db)
        {
            _db = db;
        }

        public IEnumerable<AclManHinh> GetPaged(int page, int pageSize, string keyword, out int totalRecords)
        {
            var conditions = new List<string> { "1 = 1" };
            var parameters = new DynamicParameters();
            
            if (!string.IsNullOrEmpty(keyword))
            {
                conditions.Add("(TenManHinh LIKE @Keyword OR NhomChaManHinh LIKE @Keyword)");
                parameters.Add("Keyword", "%" + keyword.Trim() + "%");
            }
            
            var whereClause = "WHERE " + string.Join(" AND ", conditions);
            
            string countSql = $"SELECT COUNT(1) FROM ACL_ManHinh {whereClause}";
            string sql = $@"
                SELECT * FROM ACL_ManHinh 
                {whereClause}
                ORDER BY NhomChaManHinh, STT, TenManHinh
                OFFSET @Offset ROWS 
                FETCH NEXT @PageSize ROWS ONLY";
            
            parameters.Add("Offset", (page - 1) * pageSize);
            parameters.Add("PageSize", pageSize);
            
            using (var conn = _db.CreateConnection())
            {
                totalRecords = conn.ExecuteScalar<int>(countSql, parameters);
                return conn.Query<AclManHinh>(sql, parameters);
            }
        }

        public IEnumerable<AclManHinh> GetAll()
        {
            const string sql = "SELECT * FROM ACL_ManHinh ORDER BY NhomChaManHinh, STT, TenManHinh";
            using (var conn = _db.CreateConnection())
                return conn.Query<AclManHinh>(sql);
        }

        public AclManHinh GetById(int id)
        {
            const string sql = "SELECT * FROM ACL_ManHinh WHERE ID = @ID";
            using (var conn = _db.CreateConnection())
                return conn.QueryFirstOrDefault<AclManHinh>(sql, new { ID = id });
        }

        public int Insert(AclManHinh entity)
        {
            const string sql = @"
                INSERT INTO ACL_ManHinh (TenManHinh, NhomChaManHinh, IsSuDung, IDThamChieu, STT)
                VALUES (@TenManHinh, @NhomChaManHinh, @IsSuDung, @IDThamChieu, @STT);
                SELECT CAST(SCOPE_IDENTITY() AS INT)";
            using (var conn = _db.CreateConnection())
                return conn.ExecuteScalar<int>(sql, entity);
        }

        public void Update(AclManHinh entity)
        {
            const string sql = @"
                UPDATE ACL_ManHinh
                SET TenManHinh = @TenManHinh, NhomChaManHinh = @NhomChaManHinh, 
                    IsSuDung = @IsSuDung, IDThamChieu = @IDThamChieu, STT = @STT
                WHERE ID = @ID";
            using (var conn = _db.CreateConnection())
                conn.Execute(sql, entity);
        }

        public void Delete(int id)
        {
            const string sql = "DELETE FROM ACL_ManHinh WHERE ID = @ID";
            using (var conn = _db.CreateConnection())
                conn.Execute(sql, new { ID = id });
        }
    }
}
