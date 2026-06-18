using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using SalesManagementSystem.Data;
using SalesManagementSystem.Models.Entities;
using SalesManagementSystem.Repositories.Interfaces;

namespace SalesManagementSystem.Repositories
{
    public class AclActionRepository : IAclActionRepository
    {
        private readonly DbConnectionFactory _db;

        public AclActionRepository(DbConnectionFactory db)
        {
            _db = db;
        }

        public IEnumerable<AclAction> GetPaged(int page, int pageSize, string keyword, out int totalRecords)
        {
            var conditions = new List<string> { "1 = 1" };
            var parameters = new DynamicParameters();
            
            if (!string.IsNullOrEmpty(keyword))
            {
                conditions.Add("(a.TenAction LIKE @Keyword OR a.TenController LIKE @Keyword OR m.TenManHinh LIKE @Keyword OR a.GhiChu LIKE @Keyword)");
                parameters.Add("Keyword", "%" + keyword.Trim() + "%");
            }
            
            var whereClause = "WHERE " + string.Join(" AND ", conditions);
            
            string countSql = $@"
                SELECT COUNT(1) 
                FROM ACL_Action a
                LEFT JOIN ACL_ManHinh m ON a.IDManHinh = m.ID
                {whereClause}";

            string sql = $@"
                SELECT a.*, m.TenManHinh 
                FROM ACL_Action a
                LEFT JOIN ACL_ManHinh m ON a.IDManHinh = m.ID
                {whereClause}
                ORDER BY m.TenManHinh, a.TenController, a.TenAction
                OFFSET @Offset ROWS 
                FETCH NEXT @PageSize ROWS ONLY";
            
            parameters.Add("Offset", (page - 1) * pageSize);
            parameters.Add("PageSize", pageSize);
            
            using (var conn = _db.CreateConnection())
            {
                totalRecords = conn.ExecuteScalar<int>(countSql, parameters);
                return conn.Query<AclAction>(sql, parameters);
            }
        }

        public IEnumerable<AclAction> GetAll()
        {
            const string sql = "SELECT * FROM ACL_Action ORDER BY TenController, TenAction";
            using (var conn = _db.CreateConnection())
                return conn.Query<AclAction>(sql);
        }

        public AclAction GetById(int id)
        {
            const string sql = "SELECT * FROM ACL_Action WHERE ID = @ID";
            using (var conn = _db.CreateConnection())
                return conn.QueryFirstOrDefault<AclAction>(sql, new { ID = id });
        }

        public int Insert(AclAction entity)
        {
            const string sql = @"
                INSERT INTO ACL_Action (IDManHinh, TenAction, TenController, GhiChu, LoaiPhanQuyen)
                VALUES (@IDManHinh, @TenAction, @TenController, @GhiChu, @LoaiPhanQuyen);
                SELECT CAST(SCOPE_IDENTITY() AS INT)";
            using (var conn = _db.CreateConnection())
                return conn.ExecuteScalar<int>(sql, entity);
        }

        public void Update(AclAction entity)
        {
            const string sql = @"
                UPDATE ACL_Action
                SET IDManHinh = @IDManHinh, TenAction = @TenAction, TenController = @TenController, 
                    GhiChu = @GhiChu, LoaiPhanQuyen = @LoaiPhanQuyen
                WHERE ID = @ID";
            using (var conn = _db.CreateConnection())
                conn.Execute(sql, entity);
        }

        public void Delete(int id)
        {
            const string sql = "DELETE FROM ACL_Action WHERE ID = @ID";
            using (var conn = _db.CreateConnection())
                conn.Execute(sql, new { ID = id });
        }
    }
}
