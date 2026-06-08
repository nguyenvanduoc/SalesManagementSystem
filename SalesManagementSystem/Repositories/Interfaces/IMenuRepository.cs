using System.Collections.Generic;
using SalesManagementSystem.Models.ViewModels;

namespace SalesManagementSystem.Repositories.Interfaces
{
    public interface IMenuRepository
    {
        List<MenuGroupVM> GetSidebarGroups();
        List<MenuSearchResultVM> SearchMenu(string keyword);
    }
}
