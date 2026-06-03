using System.ComponentModel.DataAnnotations;

namespace SalesManagementSystem.Models.ViewModels
{
    public class LoginModel
    {
        [Required(ErrorMessage = "Mời bạn nhập tên đăng nhập")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "Mời bạn nhập mật khẩu")]
        public string Password { get; set; }
    }
}
