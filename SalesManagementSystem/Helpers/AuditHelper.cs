using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Dapper;
using SalesManagementSystem.Data;
using SalesManagementSystem.Models.ViewModels;

namespace SalesManagementSystem.Helpers
{
    [AttributeUsage(AttributeTargets.Property)]
    public class AuditIgnoreAttribute : Attribute
    {
    }

    public class AuditHelper
    {
        private AuditLogRoot _logData = new AuditLogRoot();

        private static readonly string[] _ignoredColumns = new[]
        {
            "CreatedDate", "CreatedBy", "ModifiedDate", "ModifiedBy", "RowVersion", "Timestamp", "NgayTao", "NguoiTao", "NgayCapNhat", "NguoiCapNhat"
        };

        public bool HasChanges()
        {
            return _logData.Tables.Any();
        }

        public void AddInsert(string tableName, string primaryKey, object newObj)
        {
            if (newObj == null) return;
            var tableLog = new AuditLogTable
            {
                TableName = tableName,
                PrimaryKey = primaryKey,
                Action = "Insert"
            };

            var props = newObj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (var prop in props)
            {
                if (_ignoredColumns.Contains(prop.Name, StringComparer.OrdinalIgnoreCase) || 
                    prop.GetCustomAttribute<AuditIgnoreAttribute>() != null)
                {
                    continue;
                }

                var val = prop.GetValue(newObj);
                tableLog.Changes.Add(new AuditLogChange
                {
                    Column = prop.Name,
                    OldValue = null,
                    NewValue = val
                });
            }

            _logData.Tables.Add(tableLog);
        }

        public void AddDelete(string tableName, string primaryKey, object oldObj)
        {
            if (oldObj == null) return;
            var tableLog = new AuditLogTable
            {
                TableName = tableName,
                PrimaryKey = primaryKey,
                Action = "Delete"
            };

            var props = oldObj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (var prop in props)
            {
                if (_ignoredColumns.Contains(prop.Name, StringComparer.OrdinalIgnoreCase) || 
                    prop.GetCustomAttribute<AuditIgnoreAttribute>() != null)
                {
                    continue;
                }

                var val = prop.GetValue(oldObj);
                tableLog.Changes.Add(new AuditLogChange
                {
                    Column = prop.Name,
                    OldValue = val,
                    NewValue = null
                });
            }

            _logData.Tables.Add(tableLog);
        }

        public void AddUpdate(string tableName, string primaryKey, object oldObj, object newObj)
        {
            if (oldObj == null || newObj == null) return;

            var tableLog = new AuditLogTable
            {
                TableName = tableName,
                PrimaryKey = primaryKey,
                Action = "Update"
            };

            var props = newObj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (var prop in props)
            {
                if (_ignoredColumns.Contains(prop.Name, StringComparer.OrdinalIgnoreCase) || 
                    prop.GetCustomAttribute<AuditIgnoreAttribute>() != null)
                {
                    continue;
                }

                var oldValProp = oldObj.GetType().GetProperty(prop.Name);
                if (oldValProp == null) continue;

                var oldVal = oldValProp.GetValue(oldObj);
                var newVal = prop.GetValue(newObj);

                if (!object.Equals(oldVal, newVal))
                {
                    tableLog.Changes.Add(new AuditLogChange
                    {
                        Column = prop.Name,
                        OldValue = oldVal,
                        NewValue = newVal
                    });
                }
            }

            if (tableLog.Changes.Any())
            {
                _logData.Tables.Add(tableLog);
            }
        }

        public void SaveAudit(int idLogin, string tenManHinh, string tenController, string tenAction)
        {
            if (!HasChanges()) return;

            string jsonContent = JsonConvert.SerializeObject(_logData, Formatting.None);

            string sql = @"
                INSERT INTO NK_TongHop (ID, IDLogin, TenManHinh, TenController, TenAction, NgayThucThi, NoiDung)
                SELECT ISNULL(MAX(ID), 0) + 1, @IDLogin, @TenManHinh, @TenController, @TenAction, @NgayThucThi, @NoiDung
                FROM NK_TongHop";

            var dbFactory = System.Web.Mvc.DependencyResolver.Current.GetService(typeof(DbConnectionFactory)) as DbConnectionFactory;
            if (dbFactory != null)
            {
                using (var conn = dbFactory.CreateConnection())
                {
                    conn.Execute(sql, new
                    {
                        IDLogin = idLogin,
                        TenManHinh = tenManHinh ?? tenController,
                        TenController = tenController,
                        TenAction = tenAction,
                        NgayThucThi = DateTime.Now,
                        NoiDung = jsonContent
                    });
                }
            }
        }
    }
}
