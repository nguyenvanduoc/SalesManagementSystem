using System;
using System.Web.Mvc;
using SalesManagementSystem.Helpers;
using SalesManagementSystem.Services;
using SalesManagementSystem.Services.Interfaces;

namespace SalesManagementSystem.Controllers
{
    /// <summary>
    /// Trợ lý ảo: nhận câu hỏi từ widget chat trên layout và trả lời dạng JSON.
    /// Chỉ người có quyền xem Dashboard mới dùng được; quyền dữ liệu chi tiết
    /// được kiểm tra theo từng loại câu hỏi trong ChatbotService.
    /// </summary>
    [CustomAuthorize(AuthorizeTypes.AuthorizedUsers)]
    public class ChatbotController : BaseController
    {
        private const int MAX_MESSAGE_LENGTH = 500;

        private readonly IChatbotService _chatbotService;

        public ChatbotController(IChatbotService chatbotService)
        {
            _chatbotService = chatbotService;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult Ask(string message, string intent = "")
        {
            if (!ChatbotService.CoQuyenSuDung())
                return Json(new { success = false, denied = true, message = "Bạn chưa được phân quyền sử dụng Trợ lý AI." });

            if (string.IsNullOrWhiteSpace(message))
                return Json(new { success = false, message = "Vui lòng nhập câu hỏi." });

            if (message.Length > MAX_MESSAGE_LENGTH)
                message = message.Substring(0, MAX_MESSAGE_LENGTH);

            try
            {
                var result = _chatbotService.Ask(message, intent);
                foreach (var link in result.Links)
                    link.Url = Url.Content(link.Url);

                return Json(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                LogHelper.WriteErrorLog(ex, System.Web.HttpContext.Current);
                return Json(new { success = false, message = "Trợ lý ảo đang gặp lỗi khi tra cứu dữ liệu. Vui lòng thử lại sau." });
            }
        }
    }
}
