using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using SalesManagementSystem.Models.ViewModels;
using SalesManagementSystem.Repositories.Interfaces;
using SalesManagementSystem.Services.Interfaces;
using Xceed.Document.NET;
using Xceed.Words.NET;

namespace SalesManagementSystem.Services
{
    /// <summary>
    /// Service xử lý Base Export Word sử dụng DocX (Xceed.Words.NET)
    /// </summary>
    public class WordExportService : IWordExportService
    {
        private readonly IDMBieuMauRepository _bieuMauRepo;

        public WordExportService(IDMBieuMauRepository bieuMauRepo)
        {
            _bieuMauRepo = bieuMauRepo;
        }

        public ExportResult ExportWord(string maBieuMau, object data, Dictionary<string, object> tables = null, bool isPdf = false)
        {
            var result = new ExportResult { Success = false };

            try
            {
                // 1. Lấy biểu mẫu từ DB
                var bieuMau = _bieuMauRepo.GetByMa(maBieuMau);
                if (bieuMau == null || bieuMau.NoiDung == null || bieuMau.NoiDung.Length == 0)
                {
                    result.Message = "Không tìm thấy biểu mẫu hoặc nội dung biểu mẫu trống.";
                    return result;
                }

                // 2. Load Word Document từ MemoryStream
                using (var ms = new MemoryStream())
                {
                    ms.Write(bieuMau.NoiDung, 0, bieuMau.NoiDung.Length);
                    ms.Position = 0;

                    using (var document = DocX.Load(ms))
                    {
                        // 3. Thay thế biến đơn (Single variables)
                        ReplaceSingleVariables(document, data);

                        // 4. Xử lý các bảng dữ liệu (Data tables)
                        if (tables != null && tables.Count > 0)
                        {
                            ReplaceTables(document, tables);
                        }

                        // 5. Lưu kết quả
                        using (var outMs = new MemoryStream())
                        {
                            document.SaveAs(outMs);
                            
                            if (isPdf)
                            {
                                // TODO: Hiện tại DocX open source không hỗ trợ lưu trực tiếp PDF.
                                // Tính năng này sẽ được triển khai bằng thư viện thứ 3 (như Spire.Doc hoặc Interop.Word)
                                throw new NotImplementedException("Xuất PDF chưa được cài đặt vì đang chờ quyết định thư viện PDF miễn phí.");
                            }
                            else
                            {
                                result.FileBytes = outMs.ToArray();
                                result.FileName = $"{bieuMau.TenBieuMau ?? "Export"}_{DateTime.Now:yyyyMMddHHmmss}.docx";
                                result.ContentType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
                                result.Success = true;
                                result.Message = "Export Word thành công.";
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                result.Message = $"Lỗi khi export Word: {ex.Message}";
                // Thêm cơ chế log error ở đây nếu hệ thống có logger
            }

            return result;
        }

        private void ReplaceSingleVariables(DocX document, object data)
        {
            if (data == null) return;

            if (data is Dictionary<string, object> dict)
            {
                foreach (var kvp in dict)
                {
                    string replaceText = FormatValue(kvp.Value);
                    document.ReplaceText("@" + kvp.Key, replaceText);
                    document.ReplaceText("«" + kvp.Key + "»", replaceText);
                }
            }
            else
            {
                PropertyInfo[] properties = data.GetType().GetProperties();
                foreach (var prop in properties)
                {
                    string replaceText = FormatValue(prop.GetValue(data));
                    document.ReplaceText("@" + prop.Name, replaceText);
                    document.ReplaceText("«" + prop.Name + "»", replaceText);
                }
            }

            // Replace missing/unmatched variables with empty string (Optional: có thể tắt nếu không muốn xóa biến dư)
            // document.ReplaceText(new System.Text.RegularExpressions.Regex(@"@[a-zA-Z0-9_]+"), ""); 
        }

        private void ReplaceTables(DocX document, Dictionary<string, object> tables)
        {
            foreach (var kvp in tables)
            {
                string tableName = kvp.Key;
                object tableData = kvp.Value;

                string startTag = $"#{tableName}";
                string endTag = $"#End{tableName}";

                // Tìm Table chứa startTag
                var targetTable = document.Tables.FirstOrDefault(t => t.Rows.Any(r => r.Cells.Any(c => c.Paragraphs.Any(p => p.Text.Contains(startTag)))));
                if (targetTable == null) continue;

                // Xóa dòng chứa startTag và endTag
                var startRow = targetTable.Rows.FirstOrDefault(r => r.Cells.Any(c => c.Paragraphs.Any(p => p.Text.Contains(startTag))));
                var endRow = targetTable.Rows.FirstOrDefault(r => r.Cells.Any(c => c.Paragraphs.Any(p => p.Text.Contains(endTag))));

                // Dòng template là dòng chứa các ký tự @
                var templateRow = targetTable.Rows.FirstOrDefault(r => r.Cells.Any(c => c.Paragraphs.Any(p => p.Text.Contains("@"))));
                
                if (templateRow == null) continue;

                int templateIndex = targetTable.Rows.IndexOf(templateRow);

                // Clone template row và điền dữ liệu
                if (tableData is DataTable dt)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        var newRow = targetTable.InsertRow(templateRow, templateIndex + i + 1);
                        foreach (DataColumn col in dt.Columns)
                        {
                            newRow.ReplaceText("@" + col.ColumnName, FormatValue(dt.Rows[i][col]));
                        }
                    }
                }
                else if (tableData is IEnumerable list)
                {
                    int i = 0;
                    foreach (var item in list)
                    {
                        var newRow = targetTable.InsertRow(templateRow, templateIndex + i + 1);
                        PropertyInfo[] props = item.GetType().GetProperties();
                        foreach (var prop in props)
                        {
                            newRow.ReplaceText("@" + prop.Name, FormatValue(prop.GetValue(item)));
                        }
                        i++;
                    }
                }

                // Xóa dòng template ban đầu
                templateRow.Remove();
                
                // Xóa dòng chứa tag nếu có trong bảng
                if (startRow != null && startRow != templateRow) startRow.Remove();
                if (endRow != null && endRow != templateRow) endRow.Remove();

                // Ngoài ra, thay thế #TableName và #EndTableName ở ngoài bảng nếu user đặt text ngoài bảng
                document.ReplaceText(startTag, "");
                document.ReplaceText(endTag, "");
            }
        }

        private string FormatValue(object value)
        {
            if (value == null) return "";

            if (value is DateTime dt)
            {
                return dt.ToString("dd/MM/yyyy");
            }
            if (value is bool b)
            {
                return b ? "Có" : "Không";
            }
            if (value is decimal || value is double || value is float || value is int || value is long)
            {
                // Format số, tiền tệ tùy theo yêu cầu, ở đây ví dụ format phân cách ngàn
                return string.Format("{0:#,##0.##}", value);
            }

            return value.ToString();
        }
    }
}
