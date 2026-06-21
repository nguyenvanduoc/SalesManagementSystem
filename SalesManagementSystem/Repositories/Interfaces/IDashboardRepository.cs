using System;
using SalesManagementSystem.Models.ViewModels;

namespace SalesManagementSystem.Repositories.Interfaces
{
    public interface IDashboardRepository
    {
        DashboardDataViewModel GetDashboardData(DateTime? tuNgay, DateTime? denNgay);
    }
}
