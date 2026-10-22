using SalesManagementSystem.Models.ViewModels;

namespace SalesManagementSystem.Services.Interfaces
{
    public interface IChatbotService
    {
        /// <summary>
        /// Trả lời một câu hỏi tiếng Việt của người dùng.
        /// </summary>
        /// <param name="message">Câu hỏi người dùng gõ</param>
        /// <param name="previousIntent">Intent của câu trả lời trước (để hiểu câu hỏi nối tiếp như "còn khách B thì sao?")</param>
        ChatbotResponseViewModel Ask(string message, string previousIntent);
    }
}
