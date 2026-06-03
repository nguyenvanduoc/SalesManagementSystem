using System;
using SalesManagementSystem.Models.Entities;

namespace SalesManagementSystem.Models.ViewModels
{
    public class AclLoginVM : AclLogin
    {
        public new string HoDem { get; set; }
        public new string Ten { get; set; }
        
        public string HoTenNhanVien 
        {
            get
            {
                return $"{HoDem} {Ten}".Trim();
            }
        }
    }
}
