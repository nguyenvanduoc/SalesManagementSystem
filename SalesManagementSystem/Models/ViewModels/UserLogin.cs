using System;

namespace SalesManagementSystem.Models.ViewModels
{
    [Serializable]
    public class UserLogin
    {
        public int UserID { get; set; }
        public string UserName { get; set; }
        public string HoDem { get; set; }
        public string Ten { get; set; }
    }
}
