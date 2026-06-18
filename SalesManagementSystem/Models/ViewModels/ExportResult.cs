namespace SalesManagementSystem.Models.ViewModels
{
    /// <summary>
    /// Kết quả trả về sau khi export file Word/PDF
    /// </summary>
    public class ExportResult
    {
        public byte[] FileBytes { get; set; }
        public string FileName { get; set; }
        public string ContentType { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; }
    }
}
