using System.Collections.Generic;

namespace SalesManagementSystem.Services.Interfaces
{
    public interface IExcelExportService
    {
        byte[] Export<T>(string maBieuMau, IEnumerable<T> data, out string fileExtension, Dictionary<string, object> variables = null);
    }
}
