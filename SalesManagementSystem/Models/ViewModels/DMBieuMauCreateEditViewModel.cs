using System.ComponentModel.DataAnnotations;
using System.Web;

namespace SalesManagementSystem.Models.ViewModels
{
    public class DMBieuMauCreateEditViewModel
    {
        public int ID { get; set; }

        [Required(ErrorMessage = "Mã biểu mẫu không được để trống")]
        [Display(Name = "Mã biểu mẫu")]
        public string MaBieuMau { get; set; }

        [Required(ErrorMessage = "Tên biểu mẫu không được để trống")]
        [Display(Name = "Tên biểu mẫu")]
        public string TenBieuMau { get; set; }

        // Used for display only (download)
        public string TenFile { get; set; }
        
        // This is where the uploaded file comes in
        public HttpPostedFileBase UploadedFile { get; set; }
    }
}
