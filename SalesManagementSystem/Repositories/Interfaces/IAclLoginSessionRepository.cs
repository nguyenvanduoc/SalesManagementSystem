using System.Collections.Generic;
using SalesManagementSystem.Models.Entities;
using SalesManagementSystem.Models.ViewModels;

namespace SalesManagementSystem.Repositories.Interfaces
{
    public interface IAclLoginSessionRepository
    {
        int LogLogin(AclLoginSession session);
        void LogLogout(int loginId);
        IEnumerable<AclLoginSessionViewModel> GetPaged(int page, int pageSize, string keyword, out int totalRecords);
        void KickSession(int id);
        bool IsSessionActive(int id);
    }
}
