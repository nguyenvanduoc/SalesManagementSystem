using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using NPOI.SS.UserModel;
using SalesManagementSystem.Repositories.Interfaces;

namespace SalesManagementSystem.Services
{
    public class ExcelExportService : Interfaces.IExcelExportService
    {
        private readonly IDMBieuMauRepository _bieuMauRepo;

        public ExcelExportService(IDMBieuMauRepository bieuMauRepo)
        {
            _bieuMauRepo = bieuMauRepo;
        }

        public byte[] Export<T>(string maBieuMau, IEnumerable<T> data, out string fileExtension, Dictionary<string, object> variables = null)
        {
            fileExtension = "xlsx"; // default
            var bieuMau = _bieuMauRepo.GetByMa(maBieuMau);
            if (bieuMau == null || bieuMau.NoiDung == null || bieuMau.NoiDung.Length == 0)
            {
                throw new Exception($"Không tìm thấy biểu mẫu '{maBieuMau}' hoặc biểu mẫu chưa có file đính kèm.");
            }

            if (!string.IsNullOrEmpty(bieuMau.DuoiFile))
            {
                fileExtension = bieuMau.DuoiFile.ToLower();
            }
            else if (!string.IsNullOrEmpty(bieuMau.TenFile))
            {
                fileExtension = Path.GetExtension(bieuMau.TenFile).Replace(".", "").ToLower();
            }

            using (var stream = new MemoryStream(bieuMau.NoiDung))
            {
                IWorkbook workbook;
                try
                {
                    workbook = WorkbookFactory.Create(stream);
                }
                catch (Exception)
                {
                    throw new Exception("File Excel biểu mẫu không hợp lệ. Vui lòng đảm bảo bạn đang upload file Excel đúng định dạng (.xls hoặc .xlsx).");
                }

                if (workbook.NumberOfSheets == 0) throw new Exception("File Excel biểu mẫu không có Sheet nào.");
                
                var worksheet = workbook.GetSheetAt(0);

                // 1. Thay thế biến đơn
                if (variables != null && variables.Count > 0)
                {
                    ReplaceSingleVariables(worksheet, variables);
                }

                // 2. Điền dữ liệu danh sách
                if (data != null)
                {
                    FillListData(worksheet, maBieuMau, data);
                }

                // Calculate formulas before saving (optional but recommended)
                // workbook.GetCreationHelper().CreateFormulaEvaluator().EvaluateAll();

                using (var outputStream = new MemoryStream())
                {
                    workbook.Write(outputStream);
                    return outputStream.ToArray();
                }
            }
        }

        private void ReplaceSingleVariables(ISheet worksheet, Dictionary<string, object> variables)
        {
            for (int rowIdx = worksheet.FirstRowNum; rowIdx <= worksheet.LastRowNum; rowIdx++)
            {
                var row = worksheet.GetRow(rowIdx);
                if (row == null) continue;

                for (int colIdx = row.FirstCellNum; colIdx < row.LastCellNum; colIdx++)
                {
                    var cell = row.GetCell(colIdx);
                    if (cell != null && cell.CellType == CellType.String)
                    {
                        string text = cell.StringCellValue;
                        if (!string.IsNullOrEmpty(text))
                        {
                            bool isChanged = false;
                            foreach (var kv in variables)
                            {
                                var placeholder = $"%{kv.Key}";
                                if (text.Contains(placeholder))
                                {
                                    text = text.Replace(placeholder, kv.Value?.ToString() ?? "");
                                    isChanged = true;
                                }
                            }

                            if (isChanged)
                            {
                                cell.SetCellValue(text);
                            }
                        }
                    }
                }
            }
        }

        private void FillListData<T>(ISheet worksheet, string maBieuMau, IEnumerable<T> dataList)
        {
            var data = dataList.ToList();
            var type = typeof(T);
            var properties = type.GetProperties();

            int templateRowIndex = -1;
            string prefix = $"%{maBieuMau}.";

            // Tìm dòng chứa placeholder danh sách (ví dụ: %NS01.)
            for (int rowIdx = worksheet.FirstRowNum; rowIdx <= worksheet.LastRowNum; rowIdx++)
            {
                var row = worksheet.GetRow(rowIdx);
                if (row == null) continue;

                for (int colIdx = row.FirstCellNum; colIdx < row.LastCellNum; colIdx++)
                {
                    var cell = row.GetCell(colIdx);
                    if (cell != null && cell.CellType == CellType.String)
                    {
                        var cellText = cell.StringCellValue;
                        if (!string.IsNullOrEmpty(cellText) && cellText.Contains(prefix))
                        {
                            templateRowIndex = rowIdx;
                            break;
                        }
                    }
                }
                if (templateRowIndex != -1) break;
            }

            if (templateRowIndex == -1) return; // Không tìm thấy template row

            if (data.Count == 0)
            {
                // Nếu không có dữ liệu, xóa dòng template
                var rowToRemove = worksheet.GetRow(templateRowIndex);
                if (rowToRemove != null)
                {
                    worksheet.RemoveRow(rowToRemove);
                    if (templateRowIndex < worksheet.LastRowNum)
                    {
                        worksheet.ShiftRows(templateRowIndex + 1, worksheet.LastRowNum, -1);
                    }
                }
                return;
            }

            // Copy template row cho các dòng dữ liệu tiếp theo
            if (data.Count > 1)
            {
                worksheet.ShiftRows(templateRowIndex + 1, worksheet.LastRowNum, data.Count - 1);
                var templateRow = worksheet.GetRow(templateRowIndex);
                for (int i = 1; i < data.Count; i++)
                {
                    var newRow = worksheet.CreateRow(templateRowIndex + i);
                    CopyRow(templateRow, newRow);
                }
            }

            for (int i = 0; i < data.Count; i++)
            {
                var item = data[i];
                int currentRowIdx = templateRowIndex + i;
                var currentRow = worksheet.GetRow(currentRowIdx);
                if (currentRow == null) continue;

                for (int colIdx = currentRow.FirstCellNum; colIdx < currentRow.LastCellNum; colIdx++)
                {
                    var cell = currentRow.GetCell(colIdx);
                    if (cell == null || cell.CellType != CellType.String) continue;

                    var text = cell.StringCellValue;

                    if (string.IsNullOrEmpty(text) || !text.Contains(prefix)) continue;

                    if (text.Contains($"{prefix}STT") || text.Contains($"{prefix}P_STT"))
                    {
                        cell.SetCellValue(i + 1);
                        continue;
                    }

                    bool replaced = false;
                    foreach (var prop in properties)
                    {
                        var propPlaceholder = $"{prefix}{prop.Name}";
                        if (text.Contains(propPlaceholder))
                        {
                            var value = prop.GetValue(item);
                            ApplyFormatter(cell, value, text, propPlaceholder);
                            replaced = true;
                            break;
                        }
                    }

                    if (!replaced)
                    {
                        // Clear placeholder nếu không tìm thấy property mapping
                        if (text.Trim() == prefix)
                        {
                            cell.SetBlank();
                        }
                        else
                        {
                            cell.SetCellValue(text.Replace(text, ""));
                        }
                    }
                }
            }
        }

        private void CopyRow(IRow sourceRow, IRow destinationRow)
        {
            destinationRow.Height = sourceRow.Height;
            for (int i = sourceRow.FirstCellNum; i < sourceRow.LastCellNum; i++)
            {
                var sourceCell = sourceRow.GetCell(i);
                if (sourceCell != null)
                {
                    var newCell = destinationRow.CreateCell(i);
                    newCell.CellStyle = sourceCell.CellStyle;
                    switch (sourceCell.CellType)
                    {
                        case CellType.String:
                            newCell.SetCellValue(sourceCell.StringCellValue);
                            break;
                        case CellType.Numeric:
                            newCell.SetCellValue(sourceCell.NumericCellValue);
                            break;
                        case CellType.Boolean:
                            newCell.SetCellValue(sourceCell.BooleanCellValue);
                            break;
                        case CellType.Formula:
                            newCell.SetCellFormula(sourceCell.CellFormula);
                            break;
                        case CellType.Error:
                            newCell.SetCellErrorValue(sourceCell.ErrorCellValue);
                            break;
                        case CellType.Blank:
                            newCell.SetBlank();
                            break;
                    }
                }
            }
        }

        private void ApplyFormatter(ICell cell, object value, string originalText, string placeholder)
        {
            if (value == null)
            {
                if (originalText == placeholder)
                {
                    cell.SetBlank();
                }
                else
                {
                    cell.SetCellValue(originalText.Replace(placeholder, ""));
                }
                return;
            }

            // Nếu ô chỉ chứa duy nhất placeholder, ta gán value trực tiếp để giữ đúng type (số, ngày)
            if (originalText.Trim() == placeholder)
            {
                if (value is DateTime dt)
                {
                    cell.SetCellValue(dt);
                    var style = cell.Sheet.Workbook.CreateCellStyle();
                    style.CloneStyleFrom(cell.CellStyle);
                    style.DataFormat = cell.Sheet.Workbook.CreateDataFormat().GetFormat("dd/MM/yyyy");
                    cell.CellStyle = style;
                }
                else if (value is string str)
                {
                    // Tránh mất số 0 ở đầu (ví dụ: "00125")
                    if (str.StartsWith("0") && str.Length > 1 && str.All(char.IsDigit))
                    {
                        cell.SetCellValue(str);
                    }
                    else if (double.TryParse(str, out double numVal))
                    {
                        cell.SetCellValue(numVal);
                    }
                    else
                    {
                        cell.SetCellValue(str);
                    }
                }
                else if (value is int i)
                {
                    cell.SetCellValue(i);
                }
                else if (value is double d)
                {
                    cell.SetCellValue(d);
                }
                else if (value is decimal dec)
                {
                    cell.SetCellValue(Convert.ToDouble(dec));
                }
                else
                {
                    cell.SetCellValue(value.ToString());
                }
            }
            else
            {
                // Nếu ô có chứa chữ khác kết hợp (ví dụ: "Mã NV: %NS01.MaNV")
                string formattedValue = value is DateTime dt ? dt.ToString("dd/MM/yyyy") : value.ToString();
                cell.SetCellValue(originalText.Replace(placeholder, formattedValue));
            }
        }

        public byte[] ExportGrouped<TKey, TItem>(string maBieuMau, IEnumerable<IGrouping<TKey, TItem>> groupedData, out string fileExtension, Dictionary<string, object> variables = null)
        {
            fileExtension = "xlsx"; // default
            var bieuMau = _bieuMauRepo.GetByMa(maBieuMau);
            if (bieuMau == null || bieuMau.NoiDung == null || bieuMau.NoiDung.Length == 0)
            {
                throw new Exception($"Không tìm thấy biểu mẫu '{maBieuMau}' hoặc biểu mẫu chưa có file đính kèm.");
            }

            if (!string.IsNullOrEmpty(bieuMau.DuoiFile))
            {
                fileExtension = bieuMau.DuoiFile.ToLower();
            }
            else if (!string.IsNullOrEmpty(bieuMau.TenFile))
            {
                fileExtension = Path.GetExtension(bieuMau.TenFile).Replace(".", "").ToLower();
            }

            using (var stream = new MemoryStream(bieuMau.NoiDung))
            {
                IWorkbook workbook;
                try
                {
                    workbook = WorkbookFactory.Create(stream);
                }
                catch (Exception)
                {
                    throw new Exception("File Excel biểu mẫu không hợp lệ. Vui lòng đảm bảo bạn đang upload file Excel đúng định dạng (.xls hoặc .xlsx).");
                }

                if (workbook.NumberOfSheets == 0) throw new Exception("File Excel biểu mẫu không có Sheet nào.");
                
                var worksheet = workbook.GetSheetAt(0);

                if (variables != null && variables.Count > 0)
                {
                    ReplaceSingleVariables(worksheet, variables);
                }

                if (groupedData != null)
                {
                    FillGroupedData(worksheet, maBieuMau, groupedData);
                }

                using (var outputStream = new MemoryStream())
                {
                    workbook.Write(outputStream);
                    return outputStream.ToArray();
                }
            }
        }

        private void FillGroupedData<TKey, TItem>(ISheet worksheet, string maBieuMau, IEnumerable<IGrouping<TKey, TItem>> groupedData)
        {
            var groups = groupedData.ToList();
            var type = typeof(TItem);
            var properties = type.GetProperties();

            int groupTemplateRowIndex = -1;
            int itemTemplateRowIndex = -1;
            int tmpTemplateRowIndex = -1;
            string groupPrefix = "%P_Group";
            string itemPrefix = $"%{maBieuMau}.";
            string tmpMarker = "[TMP]";

            for (int rowIdx = worksheet.FirstRowNum; rowIdx <= worksheet.LastRowNum; rowIdx++)
            {
                var row = worksheet.GetRow(rowIdx);
                if (row == null) continue;

                for (int colIdx = row.FirstCellNum; colIdx < row.LastCellNum; colIdx++)
                {
                    var cell = row.GetCell(colIdx);
                    if (cell != null && cell.CellType == CellType.String)
                    {
                        var cellText = cell.StringCellValue;
                        if (!string.IsNullOrEmpty(cellText))
                        {
                            if (cellText.Contains(groupPrefix)) groupTemplateRowIndex = rowIdx;
                            else if (cellText.Contains(itemPrefix)) itemTemplateRowIndex = rowIdx;
                            else if (cellText.Contains(tmpMarker)) tmpTemplateRowIndex = rowIdx;
                        }
                    }
                }
            }

            if (groupTemplateRowIndex == -1 || itemTemplateRowIndex == -1) return;

            bool hasTmp = tmpTemplateRowIndex != -1;
            int startTemplateRow = groupTemplateRowIndex;
            int endTemplateRow = hasTmp ? Math.Max(itemTemplateRowIndex, tmpTemplateRowIndex) : itemTemplateRowIndex;
            int templateBlockSize = endTemplateRow - startTemplateRow + 1;

            if (groups.Count == 0)
            {
                for (int i = endTemplateRow; i >= startTemplateRow; i--)
                {
                    worksheet.RemoveRow(worksheet.GetRow(i));
                }
                return;
            }

            // Create enough blocks for all groups at the bottom
            int totalNewBlockRows = groups.Count * templateBlockSize;
            int insertIndex = endTemplateRow + 1;

            if (insertIndex <= worksheet.LastRowNum)
            {
                worksheet.ShiftRows(insertIndex, worksheet.LastRowNum, totalNewBlockRows);
            }

            int currentRowIdx = insertIndex;

            // Copy blocks and update formulas
            for (int g = 0; g < groups.Count; g++)
            {
                int offset = currentRowIdx - startTemplateRow;

                for (int i = 0; i < templateBlockSize; i++)
                {
                    var sourceRow = worksheet.GetRow(startTemplateRow + i);
                    var newRow = worksheet.CreateRow(currentRowIdx + i);
                    if (sourceRow != null)
                    {
                        CopyRow(sourceRow, newRow);

                        // Update formulas
                        for (int col = newRow.FirstCellNum; col < newRow.LastCellNum; col++)
                        {
                            var cell = newRow.GetCell(col);
                            if (cell != null && cell.CellType == CellType.Formula)
                            {
                                string oldFormula = cell.CellFormula;
                                cell.SetCellFormula(UpdateFormulaReferences(oldFormula, offset, startTemplateRow + 1, endTemplateRow + 1));
                            }
                        }
                    }
                }
                currentRowIdx += templateBlockSize;
            }

            // Populate data and expand item rows
            currentRowIdx = insertIndex;
            List<int> tmpRowsToDelete = new List<int>();

            for (int g = 0; g < groups.Count; g++)
            {
                var group = groups[g];
                var items = group.ToList();

                var groupRow = worksheet.GetRow(currentRowIdx + (groupTemplateRowIndex - startTemplateRow));
                var itemRow = worksheet.GetRow(currentRowIdx + (itemTemplateRowIndex - startTemplateRow));
                var tmpRow = hasTmp ? worksheet.GetRow(currentRowIdx + (tmpTemplateRowIndex - startTemplateRow)) : null;

                // Set group name
                if (groupRow != null)
                {
                    for (int col = groupRow.FirstCellNum; col < groupRow.LastCellNum; col++)
                    {
                        var cell = groupRow.GetCell(col);
                        if (cell != null && cell.CellType == CellType.String)
                        {
                            var text = cell.StringCellValue;
                            if (text.Contains(groupPrefix))
                                cell.SetCellValue(text.Replace(groupPrefix, group.Key?.ToString() ?? ""));
                        }
                    }
                }

                // If tmpRow exists, clear its marker
                if (tmpRow != null)
                {
                    for (int col = tmpRow.FirstCellNum; col < tmpRow.LastCellNum; col++)
                    {
                        var cell = tmpRow.GetCell(col);
                        if (cell != null && cell.CellType == CellType.String && cell.StringCellValue.Contains(tmpMarker))
                        {
                            cell.SetCellValue(cell.StringCellValue.Replace(tmpMarker, ""));
                        }
                    }
                }

                // Insert needed item rows (if more than 1 item)
                int itemsToAdd = items.Count - 1;
                int itemRowIdx = currentRowIdx + (itemTemplateRowIndex - startTemplateRow);

                if (itemsToAdd > 0)
                {
                    worksheet.ShiftRows(itemRowIdx + 1, worksheet.LastRowNum, itemsToAdd);
                    
                    for (int i = 1; i <= itemsToAdd; i++)
                    {
                        var newItemRow = worksheet.CreateRow(itemRowIdx + i);
                        CopyRow(itemRow, newItemRow);
                    }
                }

                // Populate items
                for (int i = 0; i < items.Count; i++)
                {
                    var item = items[i];
                    var rowToFill = worksheet.GetRow(itemRowIdx + i);
                    if (rowToFill == null) continue;

                    for (int col = rowToFill.FirstCellNum; col < rowToFill.LastCellNum; col++)
                    {
                        var cell = rowToFill.GetCell(col);
                        if (cell == null || cell.CellType != CellType.String) continue;

                        var text = cell.StringCellValue;
                        if (string.IsNullOrEmpty(text) || !text.Contains(itemPrefix)) continue;

                        if (text.Contains($"{itemPrefix}STT") || text.Contains($"{itemPrefix}P_STT"))
                        {
                            cell.SetCellValue(i + 1);
                            continue;
                        }

                        bool replaced = false;
                        foreach (var prop in properties)
                        {
                            var propPlaceholder = $"{itemPrefix}{prop.Name}";
                            if (text.Contains(propPlaceholder))
                            {
                                var value = prop.GetValue(item);
                                ApplyFormatter(cell, value, text, propPlaceholder);
                                replaced = true;
                                break;
                            }
                        }

                        if (!replaced)
                        {
                            if (text.Trim() == itemPrefix) cell.SetBlank();
                            else cell.SetCellValue(text.Replace(text, ""));
                        }
                    }
                }

                if (hasTmp)
                {
                    int currentTmpRowIdx = currentRowIdx + (tmpTemplateRowIndex - startTemplateRow) + itemsToAdd;
                    tmpRowsToDelete.Add(currentTmpRowIdx - templateBlockSize);
                }

                // Move current row index to next block
                // Original block size + expanded rows
                currentRowIdx += templateBlockSize + itemsToAdd;
            }

            // Remove original template rows
            for (int i = endTemplateRow; i >= startTemplateRow; i--)
            {
                worksheet.RemoveRow(worksheet.GetRow(i));
            }

            // Shift everything up to remove template gap
            if (endTemplateRow + 1 <= worksheet.LastRowNum)
            {
                worksheet.ShiftRows(endTemplateRow + 1, worksheet.LastRowNum, -templateBlockSize);
            }

            // Remove TMP rows
            if (hasTmp && tmpRowsToDelete.Count > 0)
            {
                tmpRowsToDelete.Reverse();
                foreach (var idx in tmpRowsToDelete)
                {
                    worksheet.RemoveRow(worksheet.GetRow(idx));
                    if (idx < worksheet.LastRowNum)
                    {
                        worksheet.ShiftRows(idx + 1, worksheet.LastRowNum, -1);
                    }
                }
            }
        }

        private string UpdateFormulaReferences(string formula, int offset, int templateStartRow, int templateEndRow)
        {
            if (string.IsNullOrEmpty(formula)) return formula;

            return System.Text.RegularExpressions.Regex.Replace(formula, @"([A-Z]+)(\d+)", match =>
            {
                string col = match.Groups[1].Value;
                if (int.TryParse(match.Groups[2].Value, out int row))
                {
                    if (row >= templateStartRow && row <= templateEndRow)
                    {
                        return col + (row + offset).ToString();
                    }
                }
                return match.Value;
            });
        }
    }
}
