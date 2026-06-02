using System;
using SalesManagementSystem.Models.Entities;

namespace SalesManagementSystem.Models.ViewModels
{
    public class AclLoginVM : AclLogin
    {
        public string HoDem { get; set; }
        public string Ten { get; set; }
        
        public string HoTenNhanVien 
        {
            get
            {
                return $"{HoDem} {Ten}".Trim();
            }
        }
    }
}
