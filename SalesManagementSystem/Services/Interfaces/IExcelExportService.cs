using System.Collections.Generic;
using System.Linq;

namespace SalesManagementSystem.Services.Interfaces
{
    public interface IExcelExportService
    {
        byte[] Export<T>(string maBieuMau, IEnumerable<T> data, out string fileExtension, Dictionary<string, object> variables = null);
        byte[] ExportGrouped<TKey, TItem>(string maBieuMau, IEnumerable<IGrouping<TKey, TItem>> groupedData, out string fileExtension, Dictionary<string, object> variables = null);
    }
}
