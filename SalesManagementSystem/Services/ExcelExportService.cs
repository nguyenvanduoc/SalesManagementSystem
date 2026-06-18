using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
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
            fileExtension = "xlsx";
            var bieuMau = _bieuMauRepo.GetByMa(maBieuMau);
            if (bieuMau == null || bieuMau.NoiDung == null || bieuMau.NoiDung.Length == 0)
                throw new Exception($"Không tìm thấy biểu mẫu '{maBieuMau}' hoặc biểu mẫu chưa có file đính kèm.");

            if (!string.IsNullOrEmpty(bieuMau.DuoiFile))
                fileExtension = bieuMau.DuoiFile.ToLower();
            else if (!string.IsNullOrEmpty(bieuMau.TenFile))
                fileExtension = Path.GetExtension(bieuMau.TenFile).Replace(".", "").ToLower();

            using (var stream = new MemoryStream(bieuMau.NoiDung))
            {
                IWorkbook workbook = WorkbookFactory.Create(stream);
                if (workbook.NumberOfSheets == 0) throw new Exception("File Excel biểu mẫu không có Sheet nào.");
                
                var worksheet = workbook.GetSheetAt(0);

                if (variables != null && variables.Count > 0)
                    ReplaceSingleVariables(worksheet, variables);

                if (data != null)
                    FillListData(worksheet, maBieuMau, data);

                ValidateAndCleanWorkbook(workbook);

                using (var outputStream = new MemoryStream())
                {
                    workbook.Write(outputStream);
                    return outputStream.ToArray();
                }
            }
        }

        public byte[] ExportGrouped<TKey, TItem>(string maBieuMau, IEnumerable<IGrouping<TKey, TItem>> groupedData, out string fileExtension, Dictionary<string, object> variables = null)
        {
            fileExtension = "xlsx";
            var bieuMau = _bieuMauRepo.GetByMa(maBieuMau);
            if (bieuMau == null || bieuMau.NoiDung == null || bieuMau.NoiDung.Length == 0)
                throw new Exception($"Không tìm thấy biểu mẫu '{maBieuMau}'.");

            using (var stream = new MemoryStream(bieuMau.NoiDung))
            {
                IWorkbook workbook = WorkbookFactory.Create(stream);
                var worksheet = workbook.GetSheetAt(0);

                if (variables != null && variables.Count > 0)
                    ReplaceSingleVariables(worksheet, variables);

                if (groupedData != null)
                    FillGroupedData(worksheet, maBieuMau, groupedData);

                ValidateAndCleanWorkbook(workbook);

                using (var outputStream = new MemoryStream())
                {
                    workbook.Write(outputStream);
                    return outputStream.ToArray();
                }
            }
        }

        // 3. Hàm dùng chung để tìm cột theo header
        public int FindColumnIndexByHeader(ISheet worksheet, string headerText, int maxRow = 100)
        {
            string target = NormalizeToPropertyName(headerText);
            for (int r = 0; r <= Math.Min(worksheet.LastRowNum, maxRow); r++)
            {
                var row = worksheet.GetRow(r);
                if (row == null) continue;
                for (int c = row.FirstCellNum; c < row.LastCellNum; c++)
                {
                    if (c < 0) continue;
                    var cell = row.GetCell(c);
                    if (cell != null && cell.CellType == CellType.String)
                    {
                        if (NormalizeToPropertyName(cell.StringCellValue) == target)
                            return c;
                    }
                }
            }
            return -1;
        }

        private string NormalizeToPropertyName(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return "";
            string normalized = input.Normalize(System.Text.NormalizationForm.FormD);
            var sb = new System.Text.StringBuilder();
            foreach (var c in normalized)
            {
                if (System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c) != System.Globalization.UnicodeCategory.NonSpacingMark)
                    sb.Append(c);
            }
            string result = sb.ToString().Normalize(System.Text.NormalizationForm.FormC).ToLower();
            result = result.Replace("đ", "d");
            result = Regex.Replace(result, "[^a-z0-9]", "");
            return result;
        }

        private void ReplaceSingleVariables(ISheet worksheet, Dictionary<string, object> variables)
        {
            var sortedKeys = variables.Keys.OrderByDescending(k => k.Length).ToList();
            var dateStyleCache = new Dictionary<short, ICellStyle>();

            // 7. Dò toàn bộ UsedRange
            for (int rowIdx = worksheet.FirstRowNum; rowIdx <= worksheet.LastRowNum; rowIdx++)
            {
                var row = worksheet.GetRow(rowIdx);
                if (row == null) continue;

                for (int colIdx = row.FirstCellNum; colIdx < row.LastCellNum; colIdx++)
                {
                    if (colIdx < 0) continue;
                    var cell = row.GetCell(colIdx);
                    if (cell != null && cell.CellType == CellType.String)
                    {
                        string text = cell.StringCellValue;
                        if (string.IsNullOrEmpty(text)) continue;

                        bool isFullMatch = false;
                        foreach (var key in sortedKeys)
                        {
                            string ph1 = $"%{key}";
                            string ph2 = $"%{key}%";

                            // Replace đúng full match để giữ format
                            if (text.Trim() == ph1 || text.Trim() == ph2)
                            {
                                ApplyFormatterDirect(cell, variables[key], dateStyleCache);
                                isFullMatch = true;
                                break;
                            }
                        }

                        if (!isFullMatch)
                        {
                            bool changed = false;
                            foreach (var key in sortedKeys)
                            {
                                string ph2 = $"%{key}%";
                                if (text.Contains(ph2))
                                {
                                    text = text.Replace(ph2, FormatForString(variables[key]));
                                    changed = true;
                                }
                                else 
                                {
                                    string pattern1 = $@"%{Regex.Escape(key)}(?![a-zA-Z0-9_])";
                                    if (Regex.IsMatch(text, pattern1))
                                    {
                                        text = Regex.Replace(text, pattern1, FormatForString(variables[key]));
                                        changed = true;
                                    }
                                }
                            }
                            if (changed) cell.SetCellValue(text);
                        }
                    }
                }
            }
        }

        private void FillListData<T>(ISheet worksheet, string maBieuMau, IEnumerable<T> dataList)
        {
            var data = dataList.ToList();
            if (data.Count == 0) return;

            var properties = typeof(T).GetProperties();
            var dateStyleCache = new Dictionary<short, ICellStyle>();

            var colMap = new Dictionary<string, int>(); 
            int headerRowIndex = -1;
            int firstColIndex = int.MaxValue;
            int lastColIndex = -1;

            // 2. Xác định cột theo header text động
            for (int r = 0; r <= worksheet.LastRowNum; r++)
            {
                var row = worksheet.GetRow(r);
                if (row == null) continue;
                
                int matchCount = 0;
                var tempMap = new Dictionary<string, int>();
                int tempFirst = int.MaxValue;
                int tempLast = -1;

                for (int c = row.FirstCellNum; c < row.LastCellNum; c++)
                {
                    if (c < 0) continue;
                    var cell = row.GetCell(c);
                    if (cell != null && cell.CellType == CellType.String)
                    {
                        string cellNorm = NormalizeToPropertyName(cell.StringCellValue);
                        var prop = properties.FirstOrDefault(p => NormalizeToPropertyName(p.Name) == cellNorm);
                        if (prop != null)
                        {
                            tempMap[prop.Name] = c;
                            matchCount++;
                            if (c < tempFirst) tempFirst = c;
                            if (c > tempLast) tempLast = c;
                        }
                    }
                }
                
                if (matchCount >= 2 && matchCount > colMap.Count)
                {
                    colMap = tempMap;
                    headerRowIndex = r;
                    firstColIndex = tempFirst;
                    lastColIndex = tempLast;
                }
            }

            if (headerRowIndex == -1) return; 

            int templateRowIndex = headerRowIndex + 1;
            var templateRow = worksheet.GetRow(templateRowIndex);
            if (templateRow == null) return;

            // Ensure we copy the entire template row bounds
            firstColIndex = Math.Min(firstColIndex, templateRow.FirstCellNum);
            lastColIndex = Math.Max(lastColIndex, templateRow.LastCellNum);

            int tmpRowIndex = -1;
            for (int r = templateRowIndex + 1; r <= worksheet.LastRowNum; r++)
            {
                var row = worksheet.GetRow(r);
                if (row != null)
                {
                    for (int c = row.FirstCellNum; c < row.LastCellNum; c++)
                    {
                        if (c < 0) continue;
                        var cell = row.GetCell(c);
                        if (cell != null && cell.CellType == CellType.String && cell.StringCellValue.Contains("[TMP]"))
                        {
                            tmpRowIndex = r;
                            break;
                        }
                    }
                    if (tmpRowIndex != -1) break;
                }
            }

            if (tmpRowIndex != -1)
            {
                worksheet.RemoveRow(worksheet.GetRow(tmpRowIndex));
                if (tmpRowIndex < worksheet.LastRowNum)
                    ShiftRowsSafely(worksheet, tmpRowIndex + 1, worksheet.LastRowNum, -1);
            }

            int rowsToInsert = data.Count - 1;
            if (rowsToInsert > 0)
            {
                ShiftRowsSafely(worksheet, templateRowIndex + 1, worksheet.LastRowNum, rowsToInsert);
                ExpandPrintArea(worksheet.Workbook, worksheet, templateRowIndex + 1, rowsToInsert);
            }

            // 4. Gán dữ liệu (duyệt ngược để giữ nguyên templateRow cho đến khi copy xong)
            for (int i = data.Count - 1; i >= 0; i--)
            {
                var item = data[i];
                var targetRow = worksheet.GetRow(templateRowIndex + i) ?? worksheet.CreateRow(templateRowIndex + i);
                
                // 5. Clone style từ template (Row đầu tiên) dựa theo vùng dữ liệu thực tế
                if (i > 0) 
                {
                    CopyRowDataExact(templateRow, targetRow, firstColIndex, lastColIndex);
                }
                
                // Đảm bảo row tự động giãn chiều cao theo nội dung (AutoFit)
                targetRow.Height = -1;

                // Extract and map by placeholders FIRST (so data doesn't get wiped if it contains %)
                for (int c = targetRow.FirstCellNum; c < targetRow.LastCellNum; c++)
                {
                    if (c < 0) continue;
                    var cell = targetRow.GetCell(c);
                    if (cell != null && cell.CellType == CellType.String)
                    {
                        string text = cell.StringCellValue;
                        if (text.Contains("%"))
                        {
                            if (text.Contains("STT")) 
                            {
                                cell.SetCellValue(i + 1);
                                continue;
                            }
                            
                            // Try to find property by placeholder name (e.g., %TK01.GiaTriTon%)
                            bool replaced = false;
                            foreach (var prop in properties)
                            {
                                if (text.Contains("." + prop.Name) || text.Contains("%" + prop.Name + "%"))
                                {
                                    var value = prop.GetValue(item);
                                    ApplyFormatterDirect(cell, value, dateStyleCache);
                                    replaced = true;
                                    break;
                                }
                            }
                            
                            // Only clear if it looks like a real unmapped placeholder, not just data with a % sign
                            if (!replaced && System.Text.RegularExpressions.Regex.IsMatch(text, @"%[a-zA-Z0-9_\.]+%?")) 
                            {
                                cell.SetCellValue(System.Text.RegularExpressions.Regex.Replace(text, @"%[a-zA-Z0-9_\.]+%?", ""));
                            }
                        }
                    }
                }

                // Map by Headers (colMap) OVERWRITING the placeholders
                foreach (var kvp in colMap)
                {
                    string propName = kvp.Key;
                    int colIdx = kvp.Value;

                    var cell = targetRow.GetCell(colIdx) ?? targetRow.CreateCell(colIdx);
                    var prop = properties.First(p => p.Name == propName);
                    var value = prop.GetValue(item);

                    ApplyFormatterDirect(cell, value, dateStyleCache);
                }

                // Handle STT if present in header
                int sttColIdx = -1;
                var headerRow = worksheet.GetRow(headerRowIndex);
                if (headerRow != null)
                {
                    for (int c = headerRow.FirstCellNum; c < headerRow.LastCellNum; c++)
                    {
                        var cell = headerRow.GetCell(c);
                        if (cell != null && cell.CellType == CellType.String && NormalizeToPropertyName(cell.StringCellValue) == "stt")
                        {
                            sttColIdx = c;
                            break;
                        }
                    }
                }
                if (sttColIdx >= 0)
                {
                    var cell = targetRow.GetCell(sttColIdx) ?? targetRow.CreateCell(sttColIdx);
                    cell.SetCellValue(i + 1);
                }
            }
        }

        private void FillGroupedData<TKey, TItem>(ISheet worksheet, string maBieuMau, IEnumerable<IGrouping<TKey, TItem>> groupedData)
        {
            var groups = groupedData.ToList();
            var type = typeof(TItem);
            var properties = type.GetProperties();
            var dateStyleCache = new Dictionary<short, ICellStyle>();

            int groupTemplateRowIndex = -1;
            int itemTemplateRowIndex = -1;
            int tmpTemplateRowIndex = -1;
            string groupPrefix = "%P_Group%";
            string itemPrefix = $"%{maBieuMau}.";
            string tmpMarker = "[TMP]";

            for (int rowIdx = worksheet.FirstRowNum; rowIdx <= worksheet.LastRowNum; rowIdx++)
            {
                var row = worksheet.GetRow(rowIdx);
                if (row == null) continue;

                for (int colIdx = row.FirstCellNum; colIdx < row.LastCellNum; colIdx++)
                {
                    if (colIdx < 0) continue;
                    var cell = row.GetCell(colIdx);
                    if (cell != null && cell.CellType == CellType.String)
                    {
                        var cellText = cell.StringCellValue;
                        if (!string.IsNullOrEmpty(cellText))
                        {
                            if (cellText.Contains("%P_Group")) groupTemplateRowIndex = rowIdx;
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
                    worksheet.RemoveRow(worksheet.GetRow(i));
                return;
            }

            int totalNewBlockRows = groups.Count * templateBlockSize;
            int insertIndex = endTemplateRow + 1;

            if (insertIndex <= worksheet.LastRowNum)
                ShiftRowsSafely(worksheet, insertIndex, worksheet.LastRowNum, totalNewBlockRows);

            int currentRowIdx = insertIndex;

            for (int g = 0; g < groups.Count; g++)
            {
                int offset = currentRowIdx - startTemplateRow;
                for (int i = 0; i < templateBlockSize; i++)
                {
                    var sourceRow = worksheet.GetRow(startTemplateRow + i);
                    var newRow = worksheet.CreateRow(currentRowIdx + i);
                    if (sourceRow != null)
                    {
                        CopyRowDataExact(sourceRow, newRow, 0, sourceRow.LastCellNum);
                    }
                }
                currentRowIdx += templateBlockSize;
            }

            currentRowIdx = insertIndex;
            List<int> tmpRowsToDelete = new List<int>();

            for (int g = 0; g < groups.Count; g++)
            {
                var group = groups[g];
                var items = group.ToList();

                var groupRow = worksheet.GetRow(currentRowIdx + (groupTemplateRowIndex - startTemplateRow));
                var itemRow = worksheet.GetRow(currentRowIdx + (itemTemplateRowIndex - startTemplateRow));
                var tmpRow = hasTmp ? worksheet.GetRow(currentRowIdx + (tmpTemplateRowIndex - startTemplateRow)) : null;

                if (groupRow != null)
                {
                    for (int col = groupRow.FirstCellNum; col < groupRow.LastCellNum; col++)
                    {
                        if (col < 0) continue;
                        var cell = groupRow.GetCell(col);
                        if (cell != null && cell.CellType == CellType.String)
                        {
                            var text = cell.StringCellValue;
                            if (text.Contains("%P_Group"))
                                cell.SetCellValue(Regex.Replace(text, @"%P_Group.*?%", group.Key?.ToString() ?? ""));
                        }
                    }
                }

                if (tmpRow != null)
                {
                    for (int col = tmpRow.FirstCellNum; col < tmpRow.LastCellNum; col++)
                    {
                        if (col < 0) continue;
                        var cell = tmpRow.GetCell(col);
                        if (cell != null && cell.CellType == CellType.String && cell.StringCellValue.Contains(tmpMarker))
                            cell.SetCellValue(cell.StringCellValue.Replace(tmpMarker, ""));
                    }
                }

                int itemsToAdd = items.Count - 1;
                int itemRowIdx = currentRowIdx + (itemTemplateRowIndex - startTemplateRow);

                if (itemsToAdd > 0)
                {
                    ShiftRowsSafely(worksheet, itemRowIdx + 1, worksheet.LastRowNum, itemsToAdd);
                    for (int i = 1; i <= itemsToAdd; i++)
                    {
                        var newItemRow = worksheet.CreateRow(itemRowIdx + i);
                        CopyRowDataExact(itemRow, newItemRow, 0, itemRow.LastCellNum);
                    }
                }

                for (int i = 0; i < items.Count; i++)
                {
                    var item = items[i];
                    var rowToFill = worksheet.GetRow(itemRowIdx + i);
                    if (rowToFill == null) continue;
                    
                    rowToFill.Height = -1; // Auto fit

                    for (int col = rowToFill.FirstCellNum; col < rowToFill.LastCellNum; col++)
                    {
                        if (col < 0) continue;
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
                            if (Regex.IsMatch(text, Regex.Escape(propPlaceholder) + @"%?(?![a-zA-Z0-9_])"))
                            {
                                var value = prop.GetValue(item);
                                ApplyFormatterDirect(cell, value, dateStyleCache);
                                replaced = true;
                                break;
                            }
                        }

                        if (!replaced)
                        {
                            if (text.Trim() == itemPrefix) cell.SetBlank();
                            else cell.SetCellValue(Regex.Replace(text, Regex.Escape(itemPrefix) + @"[a-zA-Z0-9_]*", ""));
                        }
                    }
                }

                if (hasTmp)
                {
                    int currentTmpRowIdx = currentRowIdx + (tmpTemplateRowIndex - startTemplateRow) + itemsToAdd;
                    tmpRowsToDelete.Add(currentTmpRowIdx - templateBlockSize);
                }

                currentRowIdx += templateBlockSize + itemsToAdd;
            }

            for (int i = endTemplateRow; i >= startTemplateRow; i--)
                worksheet.RemoveRow(worksheet.GetRow(i));

            if (endTemplateRow + 1 <= worksheet.LastRowNum)
                ShiftRowsSafely(worksheet, endTemplateRow + 1, worksheet.LastRowNum, -templateBlockSize);

            if (hasTmp && tmpRowsToDelete.Count > 0)
            {
                tmpRowsToDelete.Reverse();
                foreach (var idx in tmpRowsToDelete)
                {
                    var r = worksheet.GetRow(idx);
                    if (r != null)
                    {
                        worksheet.RemoveRow(r);
                        if (idx < worksheet.LastRowNum)
                            ShiftRowsSafely(worksheet, idx + 1, worksheet.LastRowNum, -1);
                    }
                }
            }
        }

        private void CopyRowDataExact(IRow sourceRow, IRow targetRow, int firstCol, int lastCol)
        {
            if (sourceRow == null || targetRow == null) return;
            targetRow.Height = sourceRow.Height;
            if (sourceRow.RowStyle != null) targetRow.RowStyle = sourceRow.RowStyle;

            for (int i = firstCol; i <= lastCol; i++)
            {
                var sCell = sourceRow.GetCell(i);
                if (sCell != null)
                {
                    var tCell = targetRow.GetCell(i) ?? targetRow.CreateCell(i);
                    tCell.CellStyle = sCell.CellStyle;

                    switch (sCell.CellType)
                    {
                        case CellType.String: tCell.SetCellValue(sCell.StringCellValue); break;
                        case CellType.Numeric: tCell.SetCellValue(sCell.NumericCellValue); break;
                        case CellType.Boolean: tCell.SetCellValue(sCell.BooleanCellValue); break;
                        case CellType.Formula: 
                            tCell.SetCellFormula(UpdateFormulaRow(sCell.CellFormula, sourceRow.RowNum, targetRow.RowNum));
                            break;
                    }
                }
            }
            CopyMergedRegionsInRow(sourceRow, targetRow, firstCol, lastCol);
        }

        private void CopyMergedRegionsInRow(IRow sourceRow, IRow destinationRow, int firstCol, int lastCol)
        {
            var worksheet = sourceRow.Sheet;
            var regionsToAdd = new List<CellRangeAddress>();
            for (int i = 0; i < worksheet.NumMergedRegions; i++)
            {
                var region = worksheet.GetMergedRegion(i);
                if (region.FirstRow == sourceRow.RowNum && region.LastRow == sourceRow.RowNum && region.FirstColumn >= firstCol && region.LastColumn <= lastCol)
                {
                    regionsToAdd.Add(new CellRangeAddress(
                        destinationRow.RowNum, destinationRow.RowNum,
                        region.FirstColumn, region.LastColumn
                    ));
                }
            }
            foreach (var r in regionsToAdd) worksheet.AddMergedRegion(r);
        }

        private string UpdateFormulaRow(string formula, int oldRow, int newRow)
        {
            if (string.IsNullOrEmpty(formula)) return formula;
            int offset = newRow - oldRow;
            return Regex.Replace(formula, @"([A-Z]+)(\d+)", match =>
            {
                string col = match.Groups[1].Value;
                if (int.TryParse(match.Groups[2].Value, out int row))
                {
                    if (row == oldRow + 1) return col + (row + offset).ToString();
                }
                return match.Value;
            });
        }

        private void ShiftRowsSafely(ISheet sheet, int startRow, int endRow, int n)
        {
            if (n == 0 || startRow > endRow || startRow < 0) return;

            var regionsToShift = new List<CellRangeAddress>();
            var regionsToRemove = new List<int>();
            for (int i = 0; i < sheet.NumMergedRegions; i++)
            {
                var region = sheet.GetMergedRegion(i);
                if (region.FirstRow >= startRow && region.LastRow <= endRow)
                {
                    regionsToRemove.Add(i);
                    regionsToShift.Add(new CellRangeAddress(
                        region.FirstRow + n, region.LastRow + n,
                        region.FirstColumn, region.LastColumn));
                }
            }

            regionsToRemove.Reverse();
            foreach (var idx in regionsToRemove) sheet.RemoveMergedRegion(idx);

            if (n > 0)
            {
                for (int i = endRow; i >= startRow; i--)
                {
                    var sourceRow = sheet.GetRow(i);
                    if (sourceRow != null)
                    {
                        var targetRow = sheet.GetRow(i + n) ?? sheet.CreateRow(i + n);
                        CopyRowDataExact(sourceRow, targetRow, 0, sourceRow.LastCellNum);
                        sheet.RemoveRow(sourceRow);
                    }
                    else
                    {
                        var targetRow = sheet.GetRow(i + n);
                        if (targetRow != null) sheet.RemoveRow(targetRow);
                    }
                }
            }
            
            foreach (var region in regionsToShift) sheet.AddMergedRegion(region);
        }

        private void ExpandPrintArea(IWorkbook workbook, ISheet sheet, int startRow, int n)
        {
            if (n <= 0) return;
            int sheetIndex = workbook.GetSheetIndex(sheet);
            string printArea = workbook.GetPrintArea(sheetIndex);
            if (!string.IsNullOrEmpty(printArea))
            {
                try
                {
                    var parts = printArea.Split('!');
                    string addr = parts[parts.Length - 1];
                    var range = NPOI.SS.Util.CellRangeAddress.ValueOf(addr);
                    if (startRow <= range.LastRow)
                    {
                        workbook.SetPrintArea(sheetIndex, range.FirstColumn, range.LastColumn, range.FirstRow, range.LastRow + n);
                    }
                }
                catch { } // Ignore if print area cannot be parsed
            }
        }

        private void ApplyFormatterDirect(ICell cell, object value, Dictionary<short, ICellStyle> dateStyleCache)
        {
            if (value == null)
            {
                cell.SetBlank();
                return;
            }

            if (value is DateTime dt)
            {
                cell.SetCellValue(dt);
                if (cell.CellStyle != null)
                {
                    short origIndex = cell.CellStyle.Index;
                    if (!dateStyleCache.TryGetValue(origIndex, out var newStyle))
                    {
                        newStyle = cell.Sheet.Workbook.CreateCellStyle();
                        newStyle.CloneStyleFrom(cell.CellStyle);
                        newStyle.DataFormat = cell.Sheet.Workbook.CreateDataFormat().GetFormat("dd/MM/yyyy");
                        dateStyleCache[origIndex] = newStyle;
                    }
                    cell.CellStyle = newStyle;
                }
            }
            else if (value is string str)
            {
                if (str.StartsWith("0") && str.Length > 1 && str.All(char.IsDigit)) cell.SetCellValue(str);
                else if (double.TryParse(str, out double numVal)) cell.SetCellValue(numVal);
                else cell.SetCellValue(str);
            }
            else if (value is int iVal) cell.SetCellValue(iVal);
            else if (value is double dVal) cell.SetCellValue(dVal);
            else if (value is decimal decVal) cell.SetCellValue(Convert.ToDouble(decVal));
            else cell.SetCellValue(value.ToString());
        }

        private string FormatForString(object val)
        {
            if (val == null) return "";
            if (val is DateTime dt) return dt.ToString("dd/MM/yyyy");
            if (val is decimal dec) return dec.ToString("N0");
            if (val is double d) return d.ToString("N0");
            if (val is int i) return i.ToString("N0");
            return val.ToString();
        }

        private void ValidateAndCleanWorkbook(IWorkbook workbook)
        {
            for (int s = 0; s < workbook.NumberOfSheets; s++)
            {
                var sheet = workbook.GetSheetAt(s);
                for (int rowIdx = sheet.FirstRowNum; rowIdx <= sheet.LastRowNum; rowIdx++)
                {
                    var row = sheet.GetRow(rowIdx);
                    if (row == null) continue;
                    
                    for (int colIdx = row.FirstCellNum; colIdx < row.LastCellNum; colIdx++)
                    {
                        if (colIdx < 0) continue;
                        var cell = row.GetCell(colIdx);
                        if (cell == null || cell.CellType != CellType.String) continue;

                        string text = cell.StringCellValue;
                        if (string.IsNullOrEmpty(text)) continue;

                        bool changed = false;
                        var regexVar = new Regex(@"%[a-zA-Z0-9_\.]+.*?%?");
                        if (regexVar.IsMatch(text))
                        {
                            text = regexVar.Replace(text, "");
                            changed = true;
                        }
                        
                        if (text.Contains("[TMP]"))
                        {
                            text = text.Replace("[TMP]", "");
                            changed = true;
                        }

                        if (changed) cell.SetCellValue(text);
                    }
                }
            }
        }
    }
}
