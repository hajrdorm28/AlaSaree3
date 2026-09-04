using AlaSaree3.ViewModels.Chatbot;

namespace AlaSaree3.Services.Interfaces
{
    public interface IChatbotService
    {
        Task<ChatSendResponse> SendMessageAsync(
            ChatSendRequest request,
            string? customerId,
            bool isAuthenticated);

        Task<List<ChatHistoryItemViewModel>> GetHistoryAsync(string sessionKey, string? customerId);
    }
}
