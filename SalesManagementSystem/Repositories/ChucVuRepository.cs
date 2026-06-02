using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using SalesManagementSystem.Data;
using SalesManagementSystem.Models.Entities;

namespace SalesManagementSystem.Repositories
{
    public class ChucVuRepository
    {
        private readonly DbConnectionFactory _db;

        public ChucVuRepository(DbConnectionFactory db)
        {
            _db = db;
        }

        public IEnumerable<ChucVu> GetPaged(int page, int pageSize, string keyword, out int totalRecords)
        {
            var conditions = new List<string> { "1 = 1" };
            var parameters = new DynamicParameters();
            
            if (!string.IsNullOrEmpty(keyword))
            {
                conditions.Add("(MaChucVu LIKE @Keyword OR TenChucVu LIKE @Keyword)");
                parameters.Add("Keyword", "%" + keyword.Trim() + "%");
            }
            
            var whereClause = "WHERE " + string.Join(" AND ", conditions);
            
            string countSql = $"SELECT COUNT(1) FROM DM_ChucVu {whereClause}";
            string sql = $@"
                SELECT * FROM DM_ChucVu 
                {whereClause}
                ORDER BY STT, MaChucVu
                OFFSET @Offset ROWS 
                FETCH NEXT @PageSize ROWS ONLY";
            
            parameters.Add("Offset", (page - 1) * pageSize);
            parameters.Add("PageSize", pageSize);
            
            using (var conn = _db.CreateConnection())
            {
                totalRecords = conn.ExecuteScalar<int>(countSql, parameters);
                return conn.Query<ChucVu>(sql, parameters);
            }
        }

        public IEnumerable<ChucVu> GetAll()
        {
            const string sql = "SELECT * FROM DM_ChucVu ORDER BY STT, MaChucVu";
            using (var conn = _db.CreateConnection())
                return conn.Query<ChucVu>(sql);
        }

        public ChucVu GetById(int id)
        {
            const string sql = "SELECT * FROM DM_ChucVu WHERE ID = @ID";
            using (var conn = _db.CreateConnection())
                return conn.QueryFirstOrDefault<ChucVu>(sql, new { ID = id });
        }

        public bool IsDuplicateCode(string code, int id = 0)
        {
            const string sql = "SELECT COUNT(1) FROM DM_ChucVu WHERE MaChucVu = @MaChucVu AND ID != @ID";
            using (var conn = _db.CreateConnection())
                return conn.ExecuteScalar<int>(sql, new { MaChucVu = code, ID = id }) > 0;
        }

        public int Insert(ChucVu entity)
        {
            entity.NgayTao = DateTime.Now;
            const string sql = @"
                INSERT INTO DM_ChucVu (MaChucVu, TenChucVu, STT, NgayTao, NguoiTao, NgayCapNhat, NguoiCapNhat)
                VALUES (@MaChucVu, @TenChucVu, @STT, @NgayTao, @NguoiTao, @NgayCapNhat, @NguoiCapNhat);
                SELECT CAST(SCOPE_IDENTITY() AS INT)";
            using (var conn = _db.CreateConnection())
                return conn.ExecuteScalar<int>(sql, entity);
        }

        public void Update(ChucVu entity)
        {
            entity.NgayCapNhat = DateTime.Now;
            const string sql = @"
                UPDATE DM_ChucVu
                SET MaChucVu = @MaChucVu, TenChucVu = @TenChucVu, STT = @STT,
                    NgayCapNhat = @NgayCapNhat, NguoiCapNhat = @NguoiCapNhat
                WHERE ID = @ID";
            using (var conn = _db.CreateConnection())
                conn.Execute(sql, entity);
        }

        public void Delete(int id)
        {
            const string sql = "DELETE FROM DM_ChucVu WHERE ID = @ID";
            using (var conn = _db.CreateConnection())
                conn.Execute(sql, new { ID = id });
        }
    }
}
