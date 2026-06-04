using System;
using SalesManagementSystem.Models.Entities;

namespace SalesManagementSystem.Models.ViewModels
{
    public class AclLoginViewModel : AclLogin
    {
        public new string HoDem { get; set; }
        public new string Ten { get; set; }
        
        public string HoTen 
        {
            get
            {
                return $"{HoDem} {Ten}".Trim();
            }
        }
    }
}
