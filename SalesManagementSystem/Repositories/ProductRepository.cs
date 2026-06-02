using System.Collections.Generic;
using System.Linq;
using Dapper;
using SalesManagementSystem.Data;
using SalesManagementSystem.Models.Entities;

namespace SalesManagementSystem.Repositories
{
    /// <summary>
    /// Nơi DUY NHẤT chứa SQL liên quan đến Products và Categories.
    /// Không có logic nghiệp vụ ở đây — chỉ CRUD + query.
    /// </summary>
    public class ProductRepository
    {
        private readonly DbConnectionFactory _db;

        public ProductRepository(DbConnectionFactory db)
        {
            _db = db;
        }

        public IEnumerable<Product> GetAll()
        {
            const string sql = @"
                SELECT p.*, c.Name AS CategoryName
                FROM Products p
                INNER JOIN Categories c ON c.Id = p.CategoryId
                ORDER BY p.Name";
            using (var conn = _db.CreateConnection())
                return conn.Query<Product>(sql);
        }

        public Product GetById(int id)
        {
            const string sql = @"
                SELECT p.*, c.Name AS CategoryName
                FROM Products p
                INNER JOIN Categories c ON c.Id = p.CategoryId
                WHERE p.Id = @Id";
            using (var conn = _db.CreateConnection())
                return conn.QueryFirstOrDefault<Product>(sql, new { Id = id });
        }

        public IEnumerable<Product> GetByCategory(int categoryId)
        {
            const string sql = @"
                SELECT p.*, c.Name AS CategoryName
                FROM Products p
                INNER JOIN Categories c ON c.Id = p.CategoryId
                WHERE p.CategoryId = @CategoryId
                ORDER BY p.Name";
            using (var conn = _db.CreateConnection())
                return conn.Query<Product>(sql, new { CategoryId = categoryId });
        }

        public int Insert(Product product)
        {
            const string sql = @"
                INSERT INTO Products (CategoryId, Name, Sku, CostPrice, SellingPrice, Unit)
                VALUES (@CategoryId, @Name, @Sku, @CostPrice, @SellingPrice, @Unit);
                SELECT CAST(SCOPE_IDENTITY() AS INT)";
            using (var conn = _db.CreateConnection())
                return conn.ExecuteScalar<int>(sql, product);
        }

        public void Update(Product product)
        {
            const string sql = @"
                UPDATE Products
                SET CategoryId = @CategoryId, Name = @Name, Sku = @Sku,
                    CostPrice = @CostPrice, SellingPrice = @SellingPrice, Unit = @Unit
                WHERE Id = @Id";
            using (var conn = _db.CreateConnection())
                conn.Execute(sql, product);
        }

        public void Delete(int id)
        {
            const string sql = "DELETE FROM Products WHERE Id = @Id";
            using (var conn = _db.CreateConnection())
                conn.Execute(sql, new { Id = id });
        }

        public IEnumerable<Category> GetAllCategories()
        {
            const string sql = "SELECT * FROM Categories ORDER BY Name";
            using (var conn = _db.CreateConnection())
                return conn.Query<Category>(sql);
        }
    }
}
